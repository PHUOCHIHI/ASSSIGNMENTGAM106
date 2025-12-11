using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Minecraft.Data;
using Minecraft.Models;

namespace Minecraft.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string AdminSessionKey = "IsAdmin";

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString(AdminSessionKey) == "true";
        }

        private IActionResult RequireAdmin()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return null!;
        }

        // Trang tổng quan admin
        public async Task<IActionResult> Index()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var stats = new
            {
                PlayerCount = await _context.Players.CountAsync(),
                GameModeCount = await _context.GameModes.CountAsync(),
                ItemCount = await _context.Items.CountAsync(),
                PurchaseCount = await _context.Purchases.CountAsync()
            };

            return View(stats);
        }

        // Quản lý người chơi
        public async Task<IActionResult> Players()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var players = await _context.Players
                .Include(p => p.GameMode)
                .ToListAsync();
            return View(players);
        }

        // Quản lý chế độ chơi
        public async Task<IActionResult> GameModes()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var modes = await _context.GameModes.ToListAsync();
            return View(modes);
        }

        // Quản lý vật phẩm
        public async Task<IActionResult> Items()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var items = await _context.Items.ToListAsync();
            return View(items);
        }

        // Quản lý phương tiện
        public async Task<IActionResult> Vehicles()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var vehicles = await _context.Vehicles.ToListAsync();
            return View(vehicles);
        }

        // Tạo vật phẩm mới
        [HttpGet]
        public IActionResult CreateItem()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItem(Item item)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            if (!ModelState.IsValid)
            {
                return View(item);
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Items));
        }

        [HttpGet]
        public IActionResult CreateVehicle()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Vehicles));
        }

        // Sửa vật phẩm
        [HttpGet]
        public async Task<IActionResult> EditItem(int id)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> EditVehicle(int id)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItem(Item item)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            if (!ModelState.IsValid)
            {
                return View(item);
            }

            _context.Items.Update(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Items));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(Vehicle vehicle)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            if (!ModelState.IsValid)
            {
                return View(vehicle);
            }

            _context.Vehicles.Update(vehicle);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Vehicles));
        }

        // Xoá vật phẩm
        [HttpGet]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("DeleteItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItemConfirmed(int id)
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Items));
        }

        // Quản lý giao dịch (mua vật phẩm / phương tiện)
        public async Task<IActionResult> Purchases()
        {
            var redirect = RequireAdmin();
            if (redirect != null && redirect is RedirectToActionResult)
            {
                return redirect;
            }

            var purchases = await _context.Purchases
                .Include(p => p.Player)
                .Include(p => p.Item)
                .Include(p => p.Vehicle)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
            return View(purchases);
        }
    }
}
