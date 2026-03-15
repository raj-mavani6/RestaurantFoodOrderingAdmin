using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext db;

        public OrderController(AppDbContext context)
        {
            db = context;
        }

        // GET: Order/Index
        public IActionResult Index()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Order/Details/5
        public IActionResult Details(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems!)
                .ThenInclude(oi => oi.FoodItem)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Order/UpdateStatus
        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                if (status == "Delivered")
                {
                    order.DeliveryDate = DateTime.Now;
                }
                db.SaveChanges();
                TempData["Success"] = "Order status updated successfully!";
            }
            return RedirectToAction("Details", new { id = orderId });
        }

        // GET: Order/Delete/5
        public IActionResult Delete(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null)
            {
                db.Orders.Remove(order);
                db.SaveChanges();
                TempData["Success"] = "Order deleted successfully!";
            }
            return RedirectToAction("Index");
        }
    }
}
