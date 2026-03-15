using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFoodOrderingAdmin.Data;
using RestaurantFoodOrderingAdmin.Models;
using System.Text;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class ReportController : Controller
    {
        private readonly AppDbContext db;

        public ReportController(AppDbContext context)
        {
            db = context;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminId") != null;
        }

        // Main Reports Dashboard
        public IActionResult Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Reports & Analytics";
            return View();
        }

        // Sales Reports
        public IActionResult Sales(string startDate = "", string endDate = "")
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Sales Reports";

            // Get date range
            DateTime start = string.IsNullOrEmpty(startDate) ? DateTime.Now.AddMonths(-1) : DateTime.Parse(startDate);
            DateTime end = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate);

            // Get sales data
            var orders = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .ToList();

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");
            ViewBag.TotalOrders = orders.Count;
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
            ViewBag.TotalDiscount = orders.Sum(o => o.DiscountAmount);
            ViewBag.NetRevenue = orders.Sum(o => o.TotalAmount - o.DiscountAmount);

            return View(orders);
        }

        // Get Sales Chart Data API
        [HttpGet]
        public IActionResult GetSalesChartData(string period = "monthly")
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { error = "Unauthorized" });
            }

            var labels = new List<string>();
            var values = new List<decimal>();

            DateTime startDate;
            var now = DateTime.Now;

            switch (period.ToLower())
            {
                case "daily":
                case "7days":
                    // Last 7 days
                    startDate = now.AddDays(-6);
                    for (int i = 0; i < 7; i++)
                    {
                        var date = startDate.AddDays(i);
                        labels.Add(date.ToString("dd MMM"));
                        var dailySales = db.Orders
                            .Where(o => o.OrderDate.Date == date.Date && o.Status == "Delivered")
                            .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                        values.Add(dailySales);
                    }
                    break;

                case "30days":
                    // Last 30 days
                    startDate = now.AddDays(-29);
                    for (int i = 0; i < 30; i++)
                    {
                        var date = startDate.AddDays(i);
                        labels.Add(date.ToString("dd MMM"));
                        var dailySales = db.Orders
                            .Where(o => o.OrderDate.Date == date.Date && o.Status == "Delivered")
                            .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                        values.Add(dailySales);
                    }
                    break;

                case "weekly":
                    // Last 8 weeks
                    startDate = now.AddDays(-56);
                    for (int i = 0; i < 8; i++)
                    {
                        var weekStart = startDate.AddDays(i * 7);
                        var weekEnd = weekStart.AddDays(6);
                        labels.Add($"Week {i + 1}");
                        var weeklySales = db.Orders
                            .Where(o => o.OrderDate.Date >= weekStart.Date && o.OrderDate.Date <= weekEnd.Date && o.Status == "Delivered")
                            .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                        values.Add(weeklySales);
                    }
                    break;

                case "monthly":
                    // Last 12 months
                    for (int i = 11; i >= 0; i--)
                    {
                        var monthDate = now.AddMonths(-i);
                        labels.Add(monthDate.ToString("MMM yyyy"));
                        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var monthlySales = db.Orders
                            .Where(o => o.OrderDate.Date >= monthStart.Date && o.OrderDate.Date <= monthEnd.Date && o.Status == "Delivered")
                            .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                        values.Add(monthlySales);
                    }
                    break;

                case "yearly":
                case "year":
                    // Last 5 years
                    for (int i = 4; i >= 0; i--)
                    {
                        var yearVal = now.Year - i;
                        labels.Add(yearVal.ToString());
                        var yearStart = new DateTime(yearVal, 1, 1);
                        var yearEnd = new DateTime(yearVal, 12, 31);
                        var yearlySales = db.Orders
                            .Where(o => o.OrderDate.Date >= yearStart.Date && o.OrderDate.Date <= yearEnd.Date && o.Status == "Delivered")
                            .Sum(o => (decimal?)o.TotalAmount) ?? 0;
                        values.Add(yearlySales);
                    }
                    break;

                case "all":
                    // All time by month (last 24 months)
                    for (int i = 23; i >= 0; i--)
                    {
                        var monthDate = now.AddMonths(-i);
                        labels.Add(monthDate.ToString("MMM yy"));
                        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var monthlySales = db.Orders
                            .Where(o => o.OrderDate.Date >= monthStart.Date && o.OrderDate.Date <= monthEnd.Date && o.Status == "Delivered")
                            .Sum(o => o.TotalAmount);
                        values.Add(monthlySales);
                    }
                    break;

                default:
                    return Json(new { error = "Invalid period" });
            }

            return Json(new { labels, values });
        }

        // Get Order Chart Data API
        [HttpGet]
        public IActionResult GetOrderChartData(string period = "monthly")
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { error = "Unauthorized" });
            }

            var labels = new List<string>();
            var values = new List<int>();

            var now = DateTime.Now;

            switch (period.ToLower())
            {
                case "daily":
                    // Last 7 days
                    for (int i = 6; i >= 0; i--)
                    {
                        var date = now.AddDays(-i);
                        labels.Add(date.ToString("dd MMM"));
                        var dailyOrders = db.Orders
                            .Count(o => o.OrderDate.Date == date.Date);
                        values.Add(dailyOrders);
                    }
                    break;

                case "weekly":
                    // Last 8 weeks
                    for (int i = 7; i >= 0; i--)
                    {
                        var weekStart = now.AddDays(-i * 7);
                        var weekEnd = weekStart.AddDays(6);
                        labels.Add($"Week {8 - i}");
                        var weeklyOrders = db.Orders
                            .Count(o => o.OrderDate.Date >= weekStart.Date && o.OrderDate.Date <= weekEnd.Date);
                        values.Add(weeklyOrders);
                    }
                    break;

                case "monthly":
                    // Last 12 months
                    for (int i = 11; i >= 0; i--)
                    {
                        var monthDate = now.AddMonths(-i);
                        labels.Add(monthDate.ToString("MMM yyyy"));
                        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var monthlyOrders = db.Orders
                            .Count(o => o.OrderDate.Date >= monthStart.Date && o.OrderDate.Date <= monthEnd.Date);
                        values.Add(monthlyOrders);
                    }
                    break;

                case "yearly":
                    // Last 5 years
                    for (int i = 4; i >= 0; i--)
                    {
                        var year = now.Year - i;
                        labels.Add(year.ToString());
                        var yearStart = new DateTime(year, 1, 1);
                        var yearEnd = new DateTime(year, 12, 31);
                        var yearlyOrders = db.Orders
                            .Count(o => o.OrderDate.Date >= yearStart.Date && o.OrderDate.Date <= yearEnd.Date);
                        values.Add(yearlyOrders);
                    }
                    break;

                case "all":
                    // All time by month (last 24 months)
                    for (int i = 23; i >= 0; i--)
                    {
                        var monthDate = now.AddMonths(-i);
                        labels.Add(monthDate.ToString("MMM yy"));
                        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var monthlyOrders = db.Orders
                            .Count(o => o.OrderDate.Date >= monthStart.Date && o.OrderDate.Date <= monthEnd.Date);
                        values.Add(monthlyOrders);
                    }
                    break;

                default:
                    return Json(new { error = "Invalid period" });
            }

            return Json(new { labels, values });
        }

        // Order Reports
        public IActionResult Orders(string status = "")
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Order Reports";

            var orders = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            // Execute orders query first
            var ordersList = orders.OrderByDescending(o => o.OrderDate).ToList();

            // Calculate summary statistics using Entity Framework
            // This uses the [Table("orders")] attribute configured in the Order model
            ViewBag.TotalOrders = db.Orders.Count();
            ViewBag.PendingOrders = db.Orders.Count(o => o.Status == "Pending");
            ViewBag.CompletedOrders = db.Orders.Count(o => o.Status == "Delivered");
            ViewBag.CancelledOrders = db.Orders.Count(o => o.Status == "Cancelled");
            
            // Average Order Value
            var allOrders = db.Orders.ToList();
            ViewBag.AverageOrderValue = allOrders.Any() 
                ? Math.Round(allOrders.Average(o => o.TotalAmount), 2) 
                : 0m;

            return View(ordersList);
        }

        // Customer Reports
        public IActionResult Customers()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Customer Reports";

            var customers = db.Customers.ToList();

            // Get all orders for counting
            var allOrders = db.Orders.ToList();
            var customerOrders = allOrders.GroupBy(o => o.CustomerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.CustomerOrders = (object)customerOrders;
            ViewBag.TotalCustomers = customers.Count;
            ViewBag.ActiveCustomers = customerOrders.Keys.Count;
            ViewBag.NewCustomersThisMonth = customers.Count(c => c.CreatedDate >= DateTime.Now.AddMonths(-1));
            
            // Customer Activity metrics
            ViewBag.InactiveCustomers = customers.Count - customerOrders.Keys.Count;
            ViewBag.RepeatCustomers = customerOrders.Count(co => co.Value.Count >= 2);
            ViewBag.NewCustomersThisWeek = customers.Count(c => c.CreatedDate >= DateTime.Now.AddDays(-7));
            
            // Average Spend Per Customer
            var totalRevenue = allOrders.Sum(o => o.TotalAmount);
            ViewBag.AvgSpendPerCustomer = customerOrders.Keys.Count > 0 
                ? Math.Round(totalRevenue / customerOrders.Keys.Count, 2) 
                : 0m;

            return View(customers);
        }

        // Get Customer Chart Data API
        [HttpGet]
        public IActionResult GetCustomerChartData(string period = "monthly")
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { error = "Unauthorized" });
            }

            var labels = new List<string>();
            var values = new List<int>();

            var now = DateTime.Now;
            var customers = db.Customers.ToList();

            switch (period.ToLower())
            {
                case "daily":
                    // Last 7 days
                    for (int i = 6; i >= 0; i--)
                    {
                        var date = now.AddDays(-i);
                        labels.Add(date.ToString("dd MMM"));
                        var count = customers.Count(c => c.CreatedDate.Date == date.Date);
                        values.Add(count);
                    }
                    break;

                case "weekly":
                    // Last 8 weeks
                    for (int i = 7; i >= 0; i--)
                    {
                        var weekStart = now.AddDays(-i * 7);
                        var weekEnd = weekStart.AddDays(6);
                        labels.Add($"Week {8 - i}");
                        var count = customers.Count(c => c.CreatedDate.Date >= weekStart.Date && c.CreatedDate.Date <= weekEnd.Date);
                        values.Add(count);
                    }
                    break;

                case "monthly":
                    // Last 12 months
                    for (int i = 11; i >= 0; i--)
                    {
                        var monthDate = now.AddMonths(-i);
                        labels.Add(monthDate.ToString("MMM yyyy"));
                        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var count = customers.Count(c => c.CreatedDate.Date >= monthStart.Date && c.CreatedDate.Date <= monthEnd.Date);
                        values.Add(count);
                    }
                    break;

                case "yearly":
                    // Last 5 years
                    for (int i = 4; i >= 0; i--)
                    {
                        var year = now.Year - i;
                        labels.Add(year.ToString());
                        var yearStart = new DateTime(year, 1, 1);
                        var yearEnd = new DateTime(year, 12, 31);
                        var count = customers.Count(c => c.CreatedDate.Date >= yearStart.Date && c.CreatedDate.Date <= yearEnd.Date);
                        values.Add(count);
                    }
                    break;

                case "all":
                    // All time by month (last 24 months)
                    for (int i = 23; i >= 0; i--)
                    {
                        var monthDate = now.AddMonths(-i);
                        labels.Add(monthDate.ToString("MMM yy"));
                        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var count = customers.Count(c => c.CreatedDate.Date >= monthStart.Date && c.CreatedDate.Date <= monthEnd.Date);
                        values.Add(count);
                    }
                    break;

                default:
                    return Json(new { error = "Invalid period" });
            }

            return Json(new { labels, values });
        }

        // Food Item Reports
        public IActionResult FoodItems()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Food Item Reports";

            var foodItems = db.FoodItems
                .Include(f => f.Category)
                .ToList();

            // Get all reviews for counting
            var allReviews = db.Reviews.ToList();
            var foodReviews = allReviews.GroupBy(r => r.FoodId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ViewBag.FoodReviews = (object)foodReviews;
            ViewBag.TotalItems = foodItems.Count;
            ViewBag.AvailableItems = foodItems.Count(f => f.IsAvailable);
            ViewBag.OutOfStockItems = foodItems.Count(f => !f.IsAvailable);
            ViewBag.VegItems = foodItems.Count(f => f.IsVeg);
            ViewBag.NonVegItems = foodItems.Count(f => !f.IsVeg);

            // Best Selling Items - Get order items and count by food id
            var orderItems = db.OrderItems.ToList();
            var salesByFood = orderItems
                .GroupBy(oi => oi.FoodId)
                .ToDictionary(g => g.Key, g => g.Sum(oi => oi.Quantity));

            var bestSellingItems = foodItems
                .Where(f => salesByFood.ContainsKey(f.FoodId))
                .OrderByDescending(f => salesByFood[f.FoodId])
                .Take(10)
                .Select(f => new
                {
                    f.FoodId,
                    f.FoodName,
                    f.ImageUrl,
                    CategoryName = f.Category?.CategoryName ?? "N/A",
                    f.Price,
                    f.IsVeg,
                    f.IsAvailable,
                    AvgRating = foodReviews.ContainsKey(f.FoodId) && foodReviews[f.FoodId].Any()
                        ? foodReviews[f.FoodId].Average(r => r.Rating)
                        : 0.0,
                    TotalSold = salesByFood[f.FoodId]
                })
                .ToList<dynamic>();

            ViewBag.BestSellingItems = bestSellingItems;

            // Category-wise Sales
            var categorySales = orderItems
                .Join(foodItems, oi => oi.FoodId, f => f.FoodId, (oi, f) => new { oi, f })
                .GroupBy(x => x.f.Category?.CategoryName ?? "Uncategorized")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalSold = g.Sum(x => x.oi.Quantity),
                    TotalRevenue = g.Sum(x => x.oi.Quantity * x.oi.Price)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ToList<dynamic>();

            ViewBag.CategorySales = categorySales;

            return View(foodItems);
        }

        // Revenue Reports
        public IActionResult Revenue(string period = "month")
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Title"] = "Revenue Reports";

            DateTime startDate = period switch
            {
                "today" => DateTime.Today,
                "week" => DateTime.Now.AddDays(-7),
                "month" => DateTime.Now.AddMonths(-1),
                "year" => DateTime.Now.AddYears(-1),
                "all" => DateTime.MinValue,
                _ => DateTime.Now.AddMonths(-1)
            };

            var orders = db.Orders
                .Where(o => o.OrderDate >= startDate && (o.Status == "Delivered" || o.Status == "Completed"))
                .ToList();

            var orderIds = orders.Select(o => o.OrderId).ToList();
            var categoryRevenue = db.OrderItems
                .Include(oi => oi.FoodItem)
                .ThenInclude(f => f.Category)
                .Where(oi => orderIds.Contains(oi.OrderId))
                .ToList()
                .GroupBy(oi => oi.FoodItem?.Category?.CategoryName ?? "Uncategorized")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(oi => oi.Quantity * oi.Price),
                    OrderItemCount = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList<dynamic>();

            ViewBag.Period = period;
            ViewBag.TotalOrders = orders.Count;
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);
            ViewBag.TotalDiscount = orders.Sum(o => o.DiscountAmount);
            ViewBag.NetRevenue = orders.Sum(o => o.TotalAmount - (o.DiscountAmount ?? 0));
            ViewBag.AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;
            ViewBag.CategoryRevenue = categoryRevenue;

            return View(orders);
        }

        // Export Sales Report to CSV
        public IActionResult ExportSalesCSV(string startDate = "", string endDate = "")
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            DateTime start = string.IsNullOrEmpty(startDate) ? DateTime.Now.AddMonths(-1) : DateTime.Parse(startDate);
            DateTime end = string.IsNullOrEmpty(endDate) ? DateTime.Now : DateTime.Parse(endDate);

            var orders = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Order ID,Customer Name,Order Date,Status,Items,Total Amount,Discount,Net Amount");

            foreach (var order in orders)
            {
                var itemCount = order.OrderItems?.Count ?? 0;
                var netAmount = order.TotalAmount - (order.DiscountAmount ?? 0);
                var customerName = order.Customer?.FullName ?? "N/A";
                csv.AppendLine($"{order.OrderId},{customerName},{order.OrderDate:yyyy-MM-dd},{order.Status},{itemCount},{order.TotalAmount},{order.DiscountAmount ?? 0},{netAmount}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"SalesReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // Export Orders Report to CSV
        public IActionResult ExportOrdersCSV()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            var csv = new StringBuilder();
            csv.AppendLine("Order ID,Customer Name,Phone,Address,Order Date,Status,Payment Method,Total Amount");

            foreach (var order in orders)
            {
                var customerName = order.Customer?.FullName ?? "N/A";
                csv.AppendLine($"{order.OrderId},{customerName},{order.ContactPhone},{order.DeliveryAddress},{order.OrderDate:yyyy-MM-dd},{order.Status},{order.PaymentMethod},{order.TotalAmount}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"OrdersReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // Export Customers Report to CSV
        public IActionResult ExportCustomersCSV()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var customers = db.Customers.ToList();
            var allOrders = db.Orders.ToList();
            var customerOrders = allOrders.GroupBy(o => o.CustomerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var csv = new StringBuilder();
            csv.AppendLine("Customer ID,Name,Email,Phone,Registration Date,Total Orders,Total Spent");

            foreach (var customer in customers)
            {
                var orders = customerOrders.ContainsKey(customer.CustomerId) ? customerOrders[customer.CustomerId] : new List<Order>();
                var totalOrders = orders.Count;
                var totalSpent = orders.Sum(o => o.TotalAmount);
                csv.AppendLine($"{customer.CustomerId},{customer.FullName},{customer.Email},{customer.Phone},{customer.CreatedDate:yyyy-MM-dd},{totalOrders},{totalSpent}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"CustomersReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // Export Food Items Report to CSV
        public IActionResult ExportFoodItemsCSV()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var foodItems = db.FoodItems
                .Include(f => f.Category)
                .ToList();

            var allReviews = db.Reviews.ToList();
            var foodReviews = allReviews.GroupBy(r => r.FoodId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var csv = new StringBuilder();
            csv.AppendLine("Food ID,Name,Category,Price,Type,Available,Reviews,Avg Rating");

            foreach (var item in foodItems)
            {
                var reviews = foodReviews.ContainsKey(item.FoodId) ? foodReviews[item.FoodId] : new List<Review>();
                var reviewCount = reviews.Count;
                var avgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                var type = item.IsVeg ? "Veg" : "Non-Veg";
                var available = item.IsAvailable ? "Yes" : "No";
                csv.AppendLine($"{item.FoodId},{item.FoodName},{item.Category?.CategoryName},{item.Price},{type},{available},{reviewCount},{avgRating:F1}");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"FoodItemsReport_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
