using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantFoodOrderingAdmin.Data;
using RestaurantFoodOrderingAdmin.Models;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class CouponController : Controller
    {
        private readonly AppDbContext db;

        public CouponController(AppDbContext context)
        {
            db = context;
        }

        // GET: Coupon/Index
        public IActionResult Index(string searchTerm = "", string filterStatus = "all")
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var coupons = db.Coupons.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                coupons = coupons.Where(c => c.CouponCode.Contains(searchTerm) ||
                                            c.Description.Contains(searchTerm));
            }

            // Status filter
            if (filterStatus == "active")
            {
                coupons = coupons.Where(c => c.IsActive && c.ExpiryDate >= DateTime.Now);
            }
            else if (filterStatus == "expired")
            {
                coupons = coupons.Where(c => c.ExpiryDate < DateTime.Now);
            }
            else if (filterStatus == "inactive")
            {
                coupons = coupons.Where(c => !c.IsActive);
            }

            var couponList = coupons.OrderByDescending(c => c.CreatedDate).ToList();

            // Calculate statistics
            ViewBag.TotalCoupons = db.Coupons.Count();
            ViewBag.ActiveCoupons = db.Coupons.Count(c => c.IsActive && c.ExpiryDate >= DateTime.Now);
            ViewBag.ExpiredCoupons = db.Coupons.Count(c => c.ExpiryDate < DateTime.Now);
            ViewBag.TotalAssignments = db.CustomerCoupons.Count();
            ViewBag.UsedCoupons = db.CustomerCoupons.Count(cc => cc.IsUsed);

            ViewBag.SearchTerm = searchTerm;
            ViewBag.FilterStatus = filterStatus;

            return View(couponList);
        }

        // GET: Coupon/Create
        public IActionResult Create()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Coupon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                // Check if coupon code already exists
                if (db.Coupons.Any(c => c.CouponCode == coupon.CouponCode))
                {
                    TempData["Error"] = "Coupon code already exists!";
                    return View(coupon);
                }

                coupon.CreatedDate = DateTime.Now;
                db.Coupons.Add(coupon);
                db.SaveChanges();

                TempData["Success"] = "Coupon created successfully!";
                return RedirectToAction("Index");
            }

            return View(coupon);
        }

        // GET: Coupon/Edit/5
        public IActionResult Edit(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var coupon = db.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }

        // POST: Coupon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                // Check if coupon code exists for other coupons
                if (db.Coupons.Any(c => c.CouponCode == coupon.CouponCode && c.CouponId != coupon.CouponId))
                {
                    TempData["Error"] = "Coupon code already exists!";
                    return View(coupon);
                }

                db.Coupons.Update(coupon);
                db.SaveChanges();

                TempData["Success"] = "Coupon updated successfully!";
                return RedirectToAction("Index");
            }

            return View(coupon);
        }

        // GET: Coupon/Delete/5
        public IActionResult Delete(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var coupon = db.Coupons
                .Include(c => c.CustomerCoupons)
                .FirstOrDefault(c => c.CouponId == id);

            if (coupon == null)
            {
                return NotFound();
            }

            ViewBag.AssignmentCount = coupon.CustomerCoupons?.Count ?? 0;
            return View(coupon);
        }

        // POST: Coupon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var coupon = db.Coupons.Find(id);
            if (coupon != null)
            {
                db.Coupons.Remove(coupon);
                db.SaveChanges();
                TempData["Success"] = "Coupon deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        // GET: Coupon/Assign
        public IActionResult Assign()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Customers = db.Customers.OrderBy(c => c.FullName).ToList();
            ViewBag.Coupons = db.Coupons
                .Where(c => c.IsActive && c.ExpiryDate >= DateTime.Now)
                .OrderBy(c => c.CouponCode)
                .ToList();

            return View();
        }

        // POST: Coupon/AssignToCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignToCustomer(int customerId, int couponId, int remainingUsage = 1)
        {
            // Check if already assigned
            var existing = db.CustomerCoupons
                .FirstOrDefault(cc => cc.CustomerId == customerId && cc.CouponId == couponId && !cc.IsUsed);

            if (existing != null)
            {
                TempData["Error"] = "This coupon is already assigned to this customer!";
                return RedirectToAction("Assign");
            }

            var customerCoupon = new CustomerCoupon
            {
                CustomerId = customerId,
                CouponId = couponId,
                AssignedDate = DateTime.Now,
                RemainingUsage = remainingUsage,
                IsUsed = false
            };

            db.CustomerCoupons.Add(customerCoupon);
            db.SaveChanges();

            TempData["Success"] = "Coupon assigned successfully!";
            return RedirectToAction("Assign");
        }

        // POST: Coupon/AssignToAll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignToAll(int couponId, int remainingUsage = 1)
        {
            var customers = db.Customers.ToList();
            int assignedCount = 0;

            foreach (var customer in customers)
            {
                // Check if not already assigned
                var existing = db.CustomerCoupons
                    .FirstOrDefault(cc => cc.CustomerId == customer.CustomerId &&
                                        cc.CouponId == couponId && !cc.IsUsed);

                if (existing == null)
                {
                    var customerCoupon = new CustomerCoupon
                    {
                        CustomerId = customer.CustomerId,
                        CouponId = couponId,
                        AssignedDate = DateTime.Now,
                        RemainingUsage = remainingUsage,
                        IsUsed = false
                    };

                    db.CustomerCoupons.Add(customerCoupon);
                    assignedCount++;
                }
            }

            db.SaveChanges();

            TempData["Success"] = $"Coupon assigned to {assignedCount} customers successfully!";
            return RedirectToAction("Assign");
        }

        // GET: Coupon/Assignments/5
        public IActionResult Assignments(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var coupon = db.Coupons.Find(id);
            if (coupon == null)
            {
                return NotFound();
            }

            var assignments = db.CustomerCoupons
                .Include(cc => cc.Customer)
                .Include(cc => cc.Order)
                .Where(cc => cc.CouponId == id)
                .OrderByDescending(cc => cc.AssignedDate)
                .ToList();

            ViewBag.Coupon = coupon;
            return View(assignments);
        }

        // POST: Coupon/ToggleStatus/5
        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var coupon = db.Coupons.Find(id);
            if (coupon != null)
            {
                coupon.IsActive = !coupon.IsActive;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    isActive = coupon.IsActive,
                    message = coupon.IsActive ? "Coupon activated" : "Coupon deactivated"
                });
            }

            return Json(new { success = false, message = "Coupon not found" });
        }

        // POST: Coupon/RevokeAssignment/5
        [HttpPost]
        public IActionResult RevokeAssignment(int id)
        {
            var assignment = db.CustomerCoupons.Find(id);
            if (assignment != null && !assignment.IsUsed)
            {
                db.CustomerCoupons.Remove(assignment);
                db.SaveChanges();

                return Json(new { success = true, message = "Assignment revoked successfully" });
            }

            return Json(new { success = false, message = "Cannot revoke used coupon" });
        }

        // GET: Coupon/Statistics
        public IActionResult Statistics()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var coupons = db.Coupons
                .Include(c => c.CustomerCoupons!)
                .ThenInclude(cc => cc.Order)
                .ToList();

            var stats = coupons.Select(c => new
            {
                Coupon = c,
                TotalAssigned = c.CustomerCoupons?.Count ?? 0,
                TotalUsed = c.CustomerCoupons?.Count(cc => cc.IsUsed) ?? 0,
                TotalDiscount = c.CustomerCoupons?
                    .Where(cc => cc.IsUsed && cc.Order != null)
                    .Sum(cc => cc.Order?.DiscountAmount ?? 0) ?? 0
            }).ToList();

            return View(stats);
        }
    }
}
