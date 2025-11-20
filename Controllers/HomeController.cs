using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;
using Minecraft.Models.ViewModels;

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

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                Players = await _context.Players.Include(p => p.GameMode).ToListAsync(),
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
                .ToListAsync();
            return Json(new ResponseAPI { Success = true, Data = players });
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayer(int id)
        {
            var player = await _context.Players
                .Include(p => p.GameMode)
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Models.ViewModels.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

