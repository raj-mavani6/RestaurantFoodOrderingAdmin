using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFoodOrderingAdmin.Data;
using RestaurantFoodOrderingAdmin.Models;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class PaymentTransactionController(AppDbContext context) : Controller
    {
        private readonly AppDbContext db = context;

        // GET: PaymentTransaction/Index
        public IActionResult Index(string searchTerm, string paymentMethod, string paymentStatus, DateTime? fromDate, DateTime? toDate)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var transactions = db.Orders
                .Include(o => o.Customer)
                .Where(o => !string.IsNullOrEmpty(o.TransactionId) || o.PaymentMethod != null)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                transactions = transactions.Where(o =>
                    o.OrderId.ToString().Contains(searchTerm) ||
                    o.TransactionId!.Contains(searchTerm) ||
                    o.Customer!.FullName.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            // Payment Method filter
            if (!string.IsNullOrEmpty(paymentMethod))
            {
                transactions = transactions.Where(o => o.PaymentMethod == paymentMethod);
                ViewBag.PaymentMethod = paymentMethod;
            }

            // Payment Status filter
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                transactions = transactions.Where(o => o.PaymentStatus == paymentStatus);
                ViewBag.PaymentStatus = paymentStatus;
            }

            // Date range filter
            if (fromDate.HasValue)
            {
                transactions = transactions.Where(o => o.OrderDate >= fromDate.Value);
                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                transactions = transactions.Where(o => o.OrderDate < endDate);
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
            }

            var transactionList = transactions.OrderByDescending(o => o.OrderDate).ToList();

            // Calculate statistics
            ViewBag.TotalTransactions = transactionList.Count;
            ViewBag.TotalAmount = transactionList.Sum(o => o.FinalAmount ?? o.TotalAmount);
            ViewBag.PaidCount = transactionList.Count(o => o.PaymentStatus == "Paid");
            ViewBag.PendingCount = transactionList.Count(o => o.PaymentStatus == "Pending");
            ViewBag.FailedCount = transactionList.Count(o => o.PaymentStatus == "Failed");

            return View(transactionList);
        }

        // GET: PaymentTransaction/Details/5
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
                .Include(o => o.Coupon)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: PaymentTransaction/UpdateStatus
        [HttpPost]
        public IActionResult UpdateStatus(int orderId, string paymentStatus)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                order.PaymentStatus = paymentStatus;
                db.SaveChanges();
                TempData["Success"] = "Payment status updated successfully!";
            }

            return RedirectToAction("Details", new { id = orderId });
        }
    }
}
