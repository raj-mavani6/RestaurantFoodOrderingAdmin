using Microsoft.AspNetCore.Mvc;
using RestaurantFoodOrderingAdmin.Data;
using RestaurantFoodOrderingAdmin.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class FoodItemController : Controller
    {
        private readonly AppDbContext db;

        public FoodItemController(AppDbContext context)
        {
            db = context;
        }

        // GET: FoodItem/Index
        public IActionResult Index()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var foodItems = db.FoodItems.Include(f => f.Category).OrderByDescending(f => f.CreatedDate).ToList();
            return View(foodItems);
        }

        // GET: FoodItem/Create
        public IActionResult Create()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Categories = db.Categories.Where(c => c.IsActive).ToList();
            return View();
        }

        // POST: FoodItem/Create
        [HttpPost]
        public IActionResult Create(FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                foodItem.CreatedDate = DateTime.Now;
                db.FoodItems.Add(foodItem);
                db.SaveChanges();
                TempData["Success"] = "Food item created successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.Categories = db.Categories.Where(c => c.IsActive).ToList();
            return View(foodItem);
        }

        // GET: FoodItem/Edit/5
        public IActionResult Edit(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var foodItem = db.FoodItems.Find(id);
            if (foodItem == null)
            {
                return NotFound();
            }
            ViewBag.Categories = db.Categories.Where(c => c.IsActive).ToList();
            return View(foodItem);
        }

        // POST: FoodItem/Edit/5
        [HttpPost]
        public IActionResult Edit(FoodItem foodItem)
        {
            if (ModelState.IsValid)
            {
                db.FoodItems.Update(foodItem);
                db.SaveChanges();
                TempData["Success"] = "Food item updated successfully!";
                return RedirectToAction("Index");
            }
            ViewBag.Categories = db.Categories.Where(c => c.IsActive).ToList();
            return View(foodItem);
        }

        // GET: FoodItem/Delete/5
        public IActionResult Delete(int id)
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var foodItem = db.FoodItems.Include(f => f.Category).FirstOrDefault(f => f.FoodId == id);
            if (foodItem == null)
            {
                return NotFound();
            }
            return View(foodItem);
        }

        // POST: FoodItem/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var foodItem = db.FoodItems.Find(id);
            if (foodItem != null)
            {
                db.FoodItems.Remove(foodItem);
                db.SaveChanges();
                TempData["Success"] = "Food item deleted successfully!";
            }
            return RedirectToAction("Index");
        }
    }
}
