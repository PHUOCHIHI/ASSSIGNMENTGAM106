using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;
using System.Security.Cryptography;

namespace Minecraft.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.Players
                    .Include(p => p.GameMode)
                    .Include(p => p.Region)
                    .Select(p => new
                    {
                        p.PlayerId,
                        p.PlayerCode,
                        p.Email,
                        p.Health,
                        p.Food,
                        p.ExperiencePoints,
                        p.GameModeId,
                        GameMode = p.GameMode == null ? null : new { p.GameMode.GameModeId, p.GameMode.Name, p.GameMode.Description },
                        p.RegionId,
                        Region = p.Region == null ? null : new { p.Region.RegionId, p.Region.Name, p.Region.Description }
                    })
                    .ToListAsync();

                return Json(new ResponseAPI { Success = true, Data = users });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        public class RegisterRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public int RegionId { get; set; }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                request.RegionId <= 0)
            {
                return Json(new ResponseAPI { Success = false, Message = "Name, Email, Password and RegionId are required" });
            }

            try
            {
                var email = request.Email.Trim();
                var playerCode = request.Name.Trim();

                if (await _context.Players.AnyAsync(p => p.PlayerCode == playerCode))
                    return Json(new ResponseAPI { Success = false, Message = "PlayerCode already exists" });

                if (await _context.Players.AnyAsync(p => p.Email == email))
                    return Json(new ResponseAPI { Success = false, Message = "Email already exists" });

                var defaultGameModeId = await _context.GameModes
                    .Where(g => g.Name == "Survival")
                    .Select(g => g.GameModeId)
                    .FirstOrDefaultAsync();

                if (defaultGameModeId == 0)
                {
                    defaultGameModeId = await _context.GameModes
                        .Select(g => g.GameModeId)
                        .FirstOrDefaultAsync();
                }

                if (defaultGameModeId == 0)
                    return Json(new ResponseAPI { Success = false, Message = "No GameMode available" });

                var regionExists = await _context.Regions.AnyAsync(r => r.RegionId == request.RegionId);
                if (!regionExists)
                    return Json(new ResponseAPI { Success = false, Message = "Region not found" });

                var player = new Player
                {
                    PlayerCode = playerCode,
                    Email = email,
                    Password = HashPassword(request.Password),
                    GameModeId = defaultGameModeId,
                    RegionId = request.RegionId,
                    Health = 100,
                    Food = 100,
                    ExperiencePoints = 0
                };

                _context.Players.Add(player);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI
                {
                    Success = true,
                    Message = "Registered successfully",
                    Data = new { player.PlayerId, player.PlayerCode, player.Email, player.GameModeId, player.RegionId }
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Json(new ResponseAPI { Success = false, Message = "Email and Password are required" });
            }

            try
            {
                var email = request.Email.Trim();
                var player = await _context.Players.FirstOrDefaultAsync(p => p.Email == email);

                if (player == null)
                    return Json(new ResponseAPI { Success = false, Message = "Invalid email or password" });

                if (!VerifyPassword(player.Password, request.Password))
                    return Json(new ResponseAPI { Success = false, Message = "Invalid email or password" });

                if (!IsHashedFormat(player.Password))
                {
                    player.Password = HashPassword(request.Password);
                    _context.Players.Update(player);
                    await _context.SaveChangesAsync();
                }

                var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

                return Json(new ResponseAPI
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new
                    {
                        Token = token,
                        Player = new { player.PlayerId, player.PlayerCode, player.Email, player.GameModeId }
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        private static bool IsHashedFormat(string stored)
        {
            if (string.IsNullOrWhiteSpace(stored)) return false;
            return stored.Count(c => c == '.') == 2;
        }

        private static string HashPassword(string password)
        {
            const int iterations = 100000;
            var salt = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            return iterations + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        private static bool VerifyPassword(string stored, string password)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            if (!IsHashedFormat(stored))
            {
                return stored == password;
            }

            var parts = stored.Split('.');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out var iterations)) return false;

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch
            {
                return false;
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var actualHash = pbkdf2.GetBytes(expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
    }
}
