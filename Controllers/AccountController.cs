using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingAdmin.Data;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext db;
        private readonly IConfiguration config;

        public AccountController(AppDbContext context, IConfiguration configuration)
        {
            db = context;
            config = configuration;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter username and password";
                return View();
            }

            var admin = db.Admins.FirstOrDefault(a => a.Username == username && a.Password == password && a.IsActive);

            if (admin != null)
            {
                HttpContext.Session.SetInt32("AdminId", admin.AdminId);
                HttpContext.Session.SetString("AdminName", admin.FullName ?? admin.Username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
