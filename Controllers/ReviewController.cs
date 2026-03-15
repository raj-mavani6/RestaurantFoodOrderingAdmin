using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext db;

        public ReviewController(AppDbContext context)
        {
            db = context;
        }

        // GET: Review/Index
        public IActionResult Index(string searchTerm, int? rating, string status, string sortBy)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = db.Reviews
                .Include(r => r.Customer)
                .Include(r => r.FoodItem)
                .ThenInclude(f => f!.Category)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                reviews = reviews.Where(r =>
                    r.FoodItem!.FoodName.Contains(searchTerm) ||
                    r.Customer!.FullName.Contains(searchTerm) ||
                    r.Comment!.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            // Rating filter
            if (rating.HasValue && rating.Value > 0)
            {
                reviews = reviews.Where(r => r.Rating == rating.Value);
                ViewBag.SelectedRating = rating.Value;
            }

            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "approved")
                {
                    reviews = reviews.Where(r => r.IsApproved == true);
                }
                else if (status == "hidden")
                {
                    reviews = reviews.Where(r => r.IsApproved == false);
                }
                ViewBag.SelectedStatus = status;
            }

            // Sorting
            reviews = sortBy switch
            {
                "date_asc" => reviews.OrderBy(r => r.ReviewDate),
                "date_desc" => reviews.OrderByDescending(r => r.ReviewDate),
                "rating_asc" => reviews.OrderBy(r => r.Rating),
                "rating_desc" => reviews.OrderByDescending(r => r.Rating),
                "customer" => reviews.OrderBy(r => r.Customer!.FullName),
                "food" => reviews.OrderBy(r => r.FoodItem!.FoodName),
                _ => reviews.OrderByDescending(r => r.ReviewDate)
            };
            ViewBag.SortBy = sortBy ?? "date_desc";

            // Statistics
            var allReviews = db.Reviews.ToList();
            ViewBag.TotalReviews = allReviews.Count;
            ViewBag.AverageRating = allReviews.Any() ? allReviews.Average(r => r.Rating) : 0;
            ViewBag.FiveStarCount = allReviews.Count(r => r.Rating == 5);
            ViewBag.FourStarCount = allReviews.Count(r => r.Rating == 4);
            ViewBag.ThreeStarCount = allReviews.Count(r => r.Rating == 3);
            ViewBag.TwoStarCount = allReviews.Count(r => r.Rating == 2);
            ViewBag.OneStarCount = allReviews.Count(r => r.Rating == 1);

            return View(reviews.ToList());
        }

        // GET: Review/Details/5
        public IActionResult Details(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var review = db.Reviews
                .Include(r => r.Customer)
                .Include(r => r.FoodItem)
                .ThenInclude(f => f!.Category)
                .FirstOrDefault(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Review/Delete/5
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var review = db.Reviews.Find(id);
            if (review != null)
            {
                db.Reviews.Remove(review);
                db.SaveChanges();
                return Json(new { success = true, message = "Review deleted successfully!" });
            }

            return Json(new { success = false, message = "Review not found" });
        }

        // POST: Review/ToggleApproval/5
        [HttpPost]
        public IActionResult ToggleApproval(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var review = db.Reviews.Find(id);
            if (review != null)
            {
                review.IsApproved = !review.IsApproved;
                db.SaveChanges();

                string status = review.IsApproved ? "approved" : "hidden";
                return Json(new { success = true, message = $"Review {status} successfully!", isApproved = review.IsApproved });
            }

            return Json(new { success = false, message = "Review not found" });
        }

        // POST: Review/DeleteMultiple
        [HttpPost]
        public IActionResult DeleteMultiple(string ids)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            if (string.IsNullOrEmpty(ids))
            {
                return Json(new { success = false, message = "No reviews selected" });
            }

            var reviewIds = ids.Split(',').Select(int.Parse).ToList();
            var reviews = db.Reviews.Where(r => reviewIds.Contains(r.ReviewId)).ToList();

            if (reviews.Any())
            {
                db.Reviews.RemoveRange(reviews);
                db.SaveChanges();
                return Json(new { success = true, message = $"{reviews.Count} reviews deleted successfully!" });
            }

            return Json(new { success = false, message = "No reviews found" });
        }
    }
}
