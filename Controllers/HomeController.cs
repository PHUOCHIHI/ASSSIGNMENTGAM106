using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;
using Minecraft.Models.ViewModels;
using System.Security.Cryptography;

namespace Minecraft.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                Players = await _context.Players.Include(p => p.GameMode).Include(p => p.Region).ToListAsync(),
                GameModes = await _context.GameModes.ToListAsync(),
                Items = await _context.Items.ToListAsync(),
                Vehicles = await _context.Vehicles.ToListAsync(),
                Quests = await _context.Quests.ToListAsync(),
                Monsters = await _context.Monsters.ToListAsync(),
                Purchases = await _context.Purchases
                    .Include(p => p.Player)
                    .Include(p => p.Item)
                    .Include(p => p.Vehicle)
                    .ToListAsync(),
                PlayerQuests = await _context.PlayerQuests
                    .Include(pq => pq.Player)
                    .Include(pq => pq.Quest)
                    .ToListAsync(),
                MonsterKills = await _context.MonsterKills
                    .Include(mk => mk.Player)
                    .Include(mk => mk.Monster)
                    .ToListAsync()
            };
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        // API Methods for Player Management
        [HttpGet]
        public async Task<IActionResult> GetPlayers()
        {
            var players = await _context.Players
                .Include(p => p.GameMode)
                .Include(p => p.Region)
                .ToListAsync();
            return Json(new ResponseAPI { Success = true, Data = players });
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var player = await _context.Players
                .Include(p => p.GameMode)
                .Include(p => p.Region)
                .FirstOrDefaultAsync(p => p.PlayerId == id);
            
            if (player == null)
                return Json(new ResponseAPI { Success = false, Message = "Player not found" });
            
            return Json(new ResponseAPI { Success = true, Data = player });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlayer([FromBody] Player player)
        {
            try
            {
                // Check if PlayerCode already exists
                if (await _context.Players.AnyAsync(p => p.PlayerCode == player.PlayerCode))
                    return Json(new ResponseAPI { Success = false, Message = "PlayerCode already exists" });

                // Check if Email already exists
                if (await _context.Players.AnyAsync(p => p.Email == player.Email))
                    return Json(new ResponseAPI { Success = false, Message = "Email already exists" });

                _context.Players.Add(player);
                await _context.SaveChangesAsync();
                return Json(new ResponseAPI { Success = true, Message = "Player created successfully", Data = player });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // API Methods for Items
        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var items = await _context.Items.ToListAsync();
            return Json(new ResponseAPI { Success = true, Data = items });
        }

        // API Methods for Vehicles
        [HttpGet]
        public async Task<IActionResult> GetVehicles()
        {
            var vehicles = await _context.Vehicles.ToListAsync();
            return Json(new ResponseAPI { Success = true, Data = vehicles });
        }

        // API Methods for Purchases
        [HttpPost]
        public async Task<IActionResult> PurchaseItem([FromBody] Purchase purchase)
        {
            try
            {
                var player = await _context.Players.FindAsync(purchase.PlayerId);
                if (player == null)
                    return Json(new ResponseAPI { Success = false, Message = "Player not found" });

                // Check if purchasing item or vehicle
                if (purchase.ItemId.HasValue)
                {
                    var item = await _context.Items.FindAsync(purchase.ItemId.Value);
                    if (item == null)
                        return Json(new ResponseAPI { Success = false, Message = "Item not found" });

                    if (player.ExperiencePoints < item.Value)
                        return Json(new ResponseAPI { Success = false, Message = "Insufficient experience points" });

                    player.ExperiencePoints -= item.Value;
                }
                else if (purchase.VehicleId.HasValue)
                {
                    var vehicle = await _context.Vehicles.FindAsync(purchase.VehicleId.Value);
                    if (vehicle == null)
                        return Json(new ResponseAPI { Success = false, Message = "Vehicle not found" });

                    if (player.ExperiencePoints < vehicle.Value)
                        return Json(new ResponseAPI { Success = false, Message = "Insufficient experience points" });

                    player.ExperiencePoints -= vehicle.Value;
                }
                else
                {
                    return Json(new ResponseAPI { Success = false, Message = "Either ItemId or VehicleId must be provided" });
                }

                purchase.PurchaseDate = DateTime.Now;
                _context.Purchases.Add(purchase);
                _context.Players.Update(player);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Purchase successful", Data = purchase });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // API Methods for Quests
        [HttpGet]
        public async Task<IActionResult> GetQuests()
        {
            var quests = await _context.Quests.ToListAsync();
            return Json(new ResponseAPI { Success = true, Data = quests });
        }

        [HttpPost]
        public async Task<IActionResult> CompleteQuest([FromBody] PlayerQuest playerQuest)
        {
            try
            {
                var player = await _context.Players.FindAsync(playerQuest.PlayerId);
                var quest = await _context.Quests.FindAsync(playerQuest.QuestId);

                if (player == null || quest == null)
                    return Json(new ResponseAPI { Success = false, Message = "Player or Quest not found" });

                var existingPlayerQuest = await _context.PlayerQuests
                    .FirstOrDefaultAsync(pq => pq.PlayerId == playerQuest.PlayerId && pq.QuestId == playerQuest.QuestId);

                if (existingPlayerQuest == null)
                {
                    playerQuest.Status = QuestStatus.Completed;
                    playerQuest.CompletedDate = DateTime.Now;
                    player.ExperiencePoints += quest.Reward;
                    _context.PlayerQuests.Add(playerQuest);
                }
                else if (existingPlayerQuest.Status != QuestStatus.Completed)
                {
                    existingPlayerQuest.Status = QuestStatus.Completed;
                    existingPlayerQuest.CompletedDate = DateTime.Now;
                    player.ExperiencePoints += quest.Reward;
                    _context.PlayerQuests.Update(existingPlayerQuest);
                }

                _context.Players.Update(player);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Quest completed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // API Methods for Monster Kills
        [HttpPost]
        public async Task<IActionResult> KillMonster([FromBody] MonsterKill monsterKill)
        {
            try
            {
                var player = await _context.Players.FindAsync(monsterKill.PlayerId);
                var monster = await _context.Monsters.FindAsync(monsterKill.MonsterId);

                if (player == null || monster == null)
                    return Json(new ResponseAPI { Success = false, Message = "Player or Monster not found" });

                monsterKill.KillDate = DateTime.Now;
                player.ExperiencePoints += monster.Reward;

                _context.MonsterKills.Add(monsterKill);
                _context.Players.Update(player);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Monster killed successfully", Data = monsterKill });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        // Y3.1: Lấy thông tin tất cả các loại tài nguyên trong game
        [HttpGet]
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
        [HttpGet]
        public async Task<IActionResult> GetPlayersByGameMode(string gameModeName)
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

        // Y3.3: Lấy tất cả các vũ khí có giá trị trên 100 điểm kinh nghiệm
        [HttpGet]
        public async Task<IActionResult> GetWeaponsAbove100Xp()
        {
            var weapons = await _context.Items
                .Where(i => i.Type == ItemType.Weapon && i.Value > 100)
                .ToListAsync();

            return Json(new ResponseAPI { Success = true, Data = weapons });
        }

        // Y3.4: Lấy thông tin các item mà người chơi có thể mua với số điểm kinh nghiệm hiện tại của họ
        [HttpGet]
        public async Task<IActionResult> GetAffordableItems(int playerId)
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
        [HttpGet]
        public async Task<IActionResult> GetDiamondItemsUnder500()
        {
            var keyword = "kim cương";
            var items = await _context.Items
                .Where(i => i.Name.ToLower().Contains(keyword) && i.Value < 500)
                .ToListAsync();

            return Json(new ResponseAPI { Success = true, Data = items });
        }

        // Y3.6: Lấy tất cả các giao dịch mua item và phương tiện của một người chơi cụ thể, sắp xếp theo thời gian
        [HttpGet]
        public async Task<IActionResult> GetPurchasesByPlayer(int playerId)
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

        // Y3.7: Thêm thông tin của một item mới
        [HttpPost]
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

        [HttpPost]
        public async Task<IActionResult> UpdatePlayerPassword([FromBody] UpdatePasswordRequest request)
        {
            if (request.PlayerId <= 0 || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return Json(new ResponseAPI { Success = false, Message = "PlayerId and NewPassword are required" });
            }

            var player = await _context.Players.FindAsync(request.PlayerId);
            if (player == null)
            {
                return Json(new ResponseAPI { Success = false, Message = "Player not found" });
            }

            try
            {
                player.Password = HashPassword(request.NewPassword);
                _context.Players.Update(player);
                await _context.SaveChangesAsync();

                return Json(new ResponseAPI { Success = true, Message = "Password updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new ResponseAPI { Success = false, Message = ex.Message });
            }
        }

        private static string HashPassword(string password)
        {
            const int iterations = 100000;
            var salt = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            return iterations + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        // Y3.9: Lấy danh sách các item được mua nhiều nhất
        [HttpGet]
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
        [HttpGet]
        public async Task<IActionResult> GetPlayersWithPurchaseCount()
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ViewModels.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

