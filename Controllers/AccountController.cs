using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Minecraft.Controllers
{
    public class AccountController : Controller
    {
        private const string AdminSessionKey = "IsAdmin";

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString(AdminSessionKey) == "true")
            {
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                HttpContext.Session.SetString(AdminSessionKey, "true");
                return RedirectToAction("Index", "Admin");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return RedirectToAction("Login");
        }
    }
}
