using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantFoodOrderingAdmin.Data;
using RestaurantFoodOrderingAdmin.Models;

namespace RestaurantFoodOrderingAdmin.Controllers
{
    public class ComboController : Controller
    {
        private readonly AppDbContext db;

        public ComboController(AppDbContext context)
        {
            db = context;
        }

        // Check if admin is logged in
        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32("AdminId") != null;
        }

        // GET: Combo/Index
        public IActionResult Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var combos = db.Combos
                .Include(c => c.ComboItems)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();
            return View(combos);
        }

        // GET: Combo/Details/5
        public IActionResult Details(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var combo = db.Combos
                .Include(c => c.ComboItems!)
                    .ThenInclude(ci => ci.FoodItem)
                        .ThenInclude(f => f!.Category)
                .FirstOrDefault(c => c.ComboId == id);

            if (combo == null)
            {
                TempData["Error"] = "Combo not found";
                return RedirectToAction("Index");
            }

            return View(combo);
        }

        // GET: Combo/Create
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.FoodItems = new SelectList(db.FoodItems.Where(f => f.IsAvailable), "FoodId", "FoodName");
            return View();
        }

        // POST: Combo/Create
        [HttpPost]
        public IActionResult Create(Combo combo, List<int>? selectedFoodIds, List<int>? quantities)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Calculate discount percentage
                if (combo.OriginalPrice > 0)
                {
                    combo.Discount = ((combo.OriginalPrice - combo.ComboPrice) / combo.OriginalPrice) * 100;
                }

                combo.CreatedDate = DateTime.Now;
                db.Combos.Add(combo);
                db.SaveChanges();

                // Add combo items
                if (selectedFoodIds != null && quantities != null && selectedFoodIds.Count > 0)
                {
                    for (int i = 0; i < selectedFoodIds.Count; i++)
                    {
                        if (selectedFoodIds[i] > 0 && quantities[i] > 0)
                        {
                            var comboItem = new ComboItem
                            {
                                ComboId = combo.ComboId,
                                FoodId = selectedFoodIds[i],
                                Quantity = quantities[i]
                            };
                            db.ComboItems.Add(comboItem);
                        }
                    }
                    db.SaveChanges();
                }

                TempData["Success"] = "Combo created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.FoodItems = new SelectList(db.FoodItems.Where(f => f.IsAvailable), "FoodId", "FoodName");
            return View(combo);
        }

        // GET: Combo/Edit/5
        public IActionResult Edit(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var combo = db.Combos
                .Include(c => c.ComboItems)
                .FirstOrDefault(c => c.ComboId == id);

            if (combo == null)
            {
                TempData["Error"] = "Combo not found";
                return RedirectToAction("Index");
            }

            ViewBag.FoodItems = new SelectList(db.FoodItems.Where(f => f.IsAvailable), "FoodId", "FoodName");
            return View(combo);
        }

        // POST: Combo/Edit/5
        [HttpPost]
        public IActionResult Edit(Combo combo, List<int>? selectedFoodIds, List<int>? quantities)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Calculate discount percentage
                if (combo.OriginalPrice > 0)
                {
                    combo.Discount = ((combo.OriginalPrice - combo.ComboPrice) / combo.OriginalPrice) * 100;
                }

                db.Combos.Update(combo);

                // Remove existing combo items
                var existingItems = db.ComboItems.Where(ci => ci.ComboId == combo.ComboId);
                db.ComboItems.RemoveRange(existingItems);

                // Add updated combo items
                if (selectedFoodIds != null && quantities != null && selectedFoodIds.Count > 0)
                {
                    for (int i = 0; i < selectedFoodIds.Count; i++)
                    {
                        if (selectedFoodIds[i] > 0 && quantities[i] > 0)
                        {
                            var comboItem = new ComboItem
                            {
                                ComboId = combo.ComboId,
                                FoodId = selectedFoodIds[i],
                                Quantity = quantities[i]
                            };
                            db.ComboItems.Add(comboItem);
                        }
                    }
                }

                db.SaveChanges();
                TempData["Success"] = "Combo updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.FoodItems = new SelectList(db.FoodItems.Where(f => f.IsAvailable), "FoodId", "FoodName");
            return View(combo);
        }

        // GET: Combo/Delete/5
        public IActionResult Delete(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var combo = db.Combos
                .Include(c => c.ComboItems!)
                    .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefault(c => c.ComboId == id);

            if (combo == null)
            {
                TempData["Error"] = "Combo not found";
                return RedirectToAction("Index");
            }

            return View(combo);
        }

        // POST: Combo/Delete/5
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var combo = db.Combos.Find(id);
            if (combo != null)
            {
                db.Combos.Remove(combo);
                db.SaveChanges();
                TempData["Success"] = "Combo deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Combo not found";
            }

            return RedirectToAction("Index");
        }

        // POST: Combo/ToggleAvailability/5
        [HttpPost]
        public IActionResult ToggleAvailability(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var combo = db.Combos.Find(id);
            if (combo != null)
            {
                combo.IsAvailable = !combo.IsAvailable;
                db.SaveChanges();
                return Json(new { success = true, isAvailable = combo.IsAvailable });
            }

            return Json(new { success = false, message = "Combo not found" });
        }
    }
}
