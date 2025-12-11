using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GameApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Y3.1: Lấy thông tin tất cả các loại tài nguyên trong game
        [HttpGet("resources")]
        public async Task<IActionResult> GetAllResources()
        {
            var resources = new
            {
                GameModes = await _context.GameModes.ToListAsync(),
                Items = await _context.Items.ToListAsync(),
                Vehicles = await _context.Vehicles.ToListAsync(),
                Quests = await _context.Quests.ToListAsync(),
                Monsters = await _context.Monsters.ToListAsync()
            };

            return Json(new ResponseAPI { Success = true, Data = resources });
        }

        // Y3.2: Lấy thông tin tất cả người chơi theo từng chế độ chơi (truyền tên chế độ chơi)
        [HttpGet("players/by-gamemode")]
        public async Task<IActionResult> GetPlayersByGameMode([FromQuery] string gameModeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gameModeName))
                {
                    return Json(new ResponseAPI { Success = false, Message = "Game mode name is required" });
                }

                var players = await _context.Players
                    .Include(p => p.GameMode)
                    .Where(p => p.GameMode != null && p.GameMode.Name.ToLower() == gameModeName.ToLower())
                    .ToListAsync();

                return Json(new ResponseAPI { Success = true, Data = players });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // Y3.3: Lấy tất cả các vũ khí có giá trị trên 100 điểm kinh nghiệm
        [HttpGet("items/weapons-above-100xp")]
        public async Task<IActionResult> GetWeaponsAbove100Xp()
        {
            var weapons = await _context.Items
                .Where(i => i.Type == ItemType.Weapon && i.Value > 100)
                .ToListAsync();

            return Json(new ResponseAPI { Success = true, Data = weapons });
        }

        // Y3.4: Lấy thông tin các item mà người chơi có thể mua với số điểm kinh nghiệm hiện tại của họ
        [HttpGet("items/affordable")]
        public async Task<IActionResult> GetAffordableItems([FromQuery] int playerId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                return Json(new ResponseAPI { Success = false, Message = "Player not found" });
            }

            var items = await _context.Items
                .Where(i => i.Value <= player.ExperiencePoints)
                .ToListAsync();

            return Json(new ResponseAPI { Success = true, Data = items });
        }

        // Y3.5: Lấy thông tin các item có tên chứa từ 'kim cương' và có giá trị dưới 500 điểm kinh nghiệm
        [HttpGet("items/diamond-under-500xp")]
        public async Task<IActionResult> GetDiamondItemsUnder500()
        {
            var keyword = "kim cương";
            var items = await _context.Items
                .Where(i => i.Name.ToLower().Contains(keyword) && i.Value < 500)
                .ToListAsync();

            return Ok(new ResponseAPI { Success = true, Data = items });
        }

        // Y3.6: Lấy tất cả các giao dịch mua item và phương tiện của một người chơi cụ thể, sắp xếp theo thời gian
        [HttpGet("purchases/by-player")]
        public async Task<IActionResult> GetPurchasesByPlayer([FromQuery] int playerId)
        {
            try
            {
                var playerExists = await _context.Players.AnyAsync(p => p.PlayerId == playerId);
                if (!playerExists)
                {
                    return Json(new ResponseAPI { Success = false, Message = "Player not found" });
                }

                var purchases = await _context.Purchases
                    .Include(p => p.Item)
                    .Include(p => p.Vehicle)
                    .Where(p => p.PlayerId == playerId)
                    .OrderBy(p => p.PurchaseDate)
                    .ToListAsync();

                return Json(new ResponseAPI { Success = true, Data = purchases });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // Y3.7: Thêm thông tin của một item mới
        [HttpPost("items")]        
        public async Task<IActionResult> CreateItem([FromBody] Item item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new ResponseAPI { Success = false, Message = "Invalid item data" });
            }

            try
            {
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
                return Json(new ResponseAPI { Success = true, Message = "Item created successfully", Data = item });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // Y3.8: Cập nhật mật khẩu của người chơi
        public class UpdatePasswordRequest
        {
            public int PlayerId { get; set; }
            public string NewPassword { get; set; } = string.Empty;
        }

        [HttpPost("players/update-password")]
        public async Task<IActionResult> UpdatePlayerPassword([FromBody] UpdatePasswordRequest request)
        {
            if (request.PlayerId <= 0 || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Json(new ResponseAPI { Success = false, Message = "PlayerId and NewPassword are required" });
            }

            var player = await _context.Players.FindAsync(request.PlayerId);
            if (player == null)
            {
                return Ok(new ResponseAPI { Success = false, Message = "Player not found" });
            }

            try
            {
                player.Password = request.NewPassword;
                _context.Players.Update(player);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // Y3.9: Lấy danh sách các item được mua nhiều nhất
        [HttpGet("items/most-purchased")]
        public async Task<IActionResult> GetMostPurchasedItems()
        {
            var query = await _context.Purchases
                .Where(p => p.ItemId.HasValue)
                .GroupBy(p => p.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    PurchaseCount = g.Count()
                })
                .OrderByDescending(x => x.PurchaseCount)
                .ToListAsync();

            var itemIds = query.Select(x => x.ItemId).ToList();
            var items = await _context.Items
                .Where(i => itemIds.Contains(i.ItemId))
                .ToListAsync();

            var result = query
                .Join(items,
                    q => q.ItemId,
                    i => i.ItemId,
                    (q, i) => new
                    {
                        Item = i,
                        q.PurchaseCount
                    })
                .ToList();

            return Json(new ResponseAPI { Success = true, Data = result });
        }

        // Y3.10: Lấy danh sách tất cả người chơi và số lần họ đã mua hàng
        [HttpGet("players/purchase-count")]        
        public async Task<IActionResult> GetPlayersWithPurchaseCount()
        {
            try
            {
                var purchasesByPlayer = await _context.Purchases
                    .GroupBy(p => p.PlayerId)
                    .Select(g => new
                    {
                        PlayerId = g.Key,
                        PurchaseCount = g.Count()
                    })
                    .ToListAsync();

                var playerIds = purchasesByPlayer.Select(x => x.PlayerId).ToList();
                var players = await _context.Players
                    .Include(p => p.GameMode)
                    .Where(p => playerIds.Contains(p.PlayerId))
                    .ToListAsync();

                var result = players
                    .GroupJoin(
                        purchasesByPlayer,
                        p => p.PlayerId,
                        pb => pb.PlayerId,
                        (p, pb) => new
                        {
                            Player = p,
                            PurchaseCount = pb.FirstOrDefault() != null ? pb.First().PurchaseCount : 0
                        })
                    .ToList();

                return Json(new ResponseAPI { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }
    }
}
