
using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using HeavenofBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeavenofBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitofWork _db;

        public CategoryController(IUnitofWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> dbContextVariables = _db.Category.GetAll();
            return View(dbContextVariables);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name and DisplayOrder values are matching!!");
            }
            if (ModelState.IsValid)
            {
                _db.Category.Add(category);
                _db.Save();
                TempData["Success"] = "Category created successufly!";
                return RedirectToAction("Index");
            }
            return View(category);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Category.GetFirstOrDefault(u => u.Id == id);
            return View(categoryFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "Name and DisplayOrder values are matching!!");
            }
            if (ModelState.IsValid)
            {
                _db.Category.Update(category);
                _db.Save();
                TempData["Success"] = "Category updated successufly!";
                return RedirectToAction("Index");
            }
            return View(category);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Category.GetFirstOrDefault(u => u.Id == id);
            return View(categoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategoryPost(int? id)
        {
            var categoryItem = _db.Category.GetFirstOrDefault(u => u.Id == id);
            if (categoryItem == null)
            {
                return NotFound();
            }
            _db.Category.Remove(categoryItem);
            _db.Save();
            TempData["Success"] = "Category deleted successufly!";
            return RedirectToAction("Index");

        }
    }
}
