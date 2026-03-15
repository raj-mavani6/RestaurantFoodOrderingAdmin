using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext db;

        public HomeController(AppDbContext context)
        {
            db = context;
        }

        // GET: Home/Index
        public IActionResult Index()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Card Statistics
            ViewBag.TotalCustomers = db.Customers.Count();
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.TotalRevenue = db.Orders.Where(o => o.Status == "Delivered").Sum(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.TotalFoodItems = db.FoodItems.Count();
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalCombos = db.Combos.Count();
            ViewBag.TotalReviews = db.Reviews.Count();
            ViewBag.TotalCoupons = db.Coupons.Count();
            ViewBag.TotalPaymentTransactions = db.Orders.Where(o => o.Status == "Delivered").Count();

            // Order Status Counts
            ViewBag.PendingOrders = db.Orders.Count(o => o.Status == "Pending");
            ViewBag.ConfirmedOrders = db.Orders.Count(o => o.Status == "Confirmed");
            ViewBag.PreparingOrders = db.Orders.Count(o => o.Status == "Preparing");
            ViewBag.OutForDeliveryOrders = db.Orders.Count(o => o.Status == "Out for Delivery");
            ViewBag.DeliveredOrders = db.Orders.Count(o => o.Status == "Delivered");
            ViewBag.CancelledOrders = db.Orders.Count(o => o.Status == "Cancelled");

            // Average Rating
            ViewBag.AverageRating = db.Reviews.Any() ? db.Reviews.Average(r => (double?)r.Rating) ?? 0 : 0;

            // Today's Orders
            var today = DateTime.Today;
            var todayOrders = db.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate.Date == today)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Top Selling Items
            var topSellingItems = db.OrderItems
                .Include(oi => oi.FoodItem)
                .GroupBy(oi => new { oi.FoodId, oi.FoodItem!.FoodName, oi.FoodItem.ImageUrl })
                .Select(g => new
                {
                    FoodId = g.Key.FoodId,
                    FoodName = g.Key.FoodName,
                    ImageUrl = g.Key.ImageUrl,
                    TotalOrders = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(x => x.TotalOrders)
                .Take(10)
                .ToList();

            ViewBag.TopSellingItems = topSellingItems;

            // Sales Data for Chart (Last 7 days)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => today.AddDays(-i))
                .Reverse()
                .ToList();

            var salesData = last7Days.Select(date => new
            {
                Date = date.ToString("dd MMM"),
                Amount = db.Orders
                    .Where(o => o.OrderDate.Date == date && o.Status == "Delivered")
                    .Sum(o => (decimal?)o.TotalAmount) ?? 0
            }).ToList();

            ViewBag.SalesChartDates = System.Text.Json.JsonSerializer.Serialize(salesData.Select(s => s.Date));
            ViewBag.SalesChartAmounts = System.Text.Json.JsonSerializer.Serialize(salesData.Select(s => s.Amount));

            // Orders by Status (Pending, Confirmed, Preparing, Out for Delivery, Cancelled)
            var ordersByStatus = db.Orders
                .Include(o => o.Customer)
                .Where(o => o.Status == "Pending" || o.Status == "Confirmed" || 
                           o.Status == "Preparing" || o.Status == "Out for Delivery" || 
                           o.Status == "Cancelled")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.OrdersByStatus = ordersByStatus;

            // Pending and Failed Payments (All with Scroll)
            var pendingFailedPayments = db.Orders
                .Include(o => o.Customer)
                .Where(o => o.PaymentStatus == "Pending" || o.PaymentStatus == "Failed")
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.PendingFailedPayments = pendingFailedPayments;

            return View(todayOrders);
        }
    }
}
