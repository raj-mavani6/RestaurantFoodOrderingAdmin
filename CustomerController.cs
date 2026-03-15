using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingAdmin.Data;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AppDbContext db;

        public CustomerController(AppDbContext context)
        {
            db = context;
        }

        // GET: Customer/Index
        public IActionResult Index()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customers = db.Customers.OrderByDescending(c => c.CreatedDate).ToList();
            return View(customers);
        }

        // GET: Customer/Details/5
        public IActionResult Details(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }

            ViewBag.OrderCount = db.Orders.Count(o => o.CustomerId == id);
            ViewBag.TotalSpent = db.Orders.Where(o => o.CustomerId == id && o.Status == "Delivered").Sum(o => (decimal?)o.TotalAmount) ?? 0;

            return View(customer);
        }

        // POST: Customer/ToggleStatus
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer != null)
            {
                customer.IsActive = !customer.IsActive;
                db.SaveChanges();
                TempData["Success"] = $"Customer {(customer.IsActive ? "activated" : "deactivated")} successfully!";
            }
            return RedirectToAction("Index");
        }

        // GET: Customer/Delete/5
        public IActionResult Delete(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.Find(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer != null)
            {
                db.Customers.Remove(customer);
                db.SaveChanges();
                TempData["Success"] = "Customer deleted successfully!";
            }
            return RedirectToAction("Index");
        }
    }
}
