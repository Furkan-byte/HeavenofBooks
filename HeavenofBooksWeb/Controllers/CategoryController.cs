
using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeavenofBooksWeb.Controllers
{
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
        public IActionResult CreateCategory()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Category category)
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
        public IActionResult EditCategory(int? id)
        {
            if (id ==null || id==0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Category.GetFirstOrDefault(u =>u.Id==id);
            return View(categoryFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(Category category)
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
        public IActionResult DeleteCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var categoryFromDb = _db.Category.GetFirstOrDefault(u =>u.Id==id);
            return View(categoryFromDb);
        }
        [HttpPost,ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategoryPost(int? id)
        {
            var categoryItem = _db.Category.GetFirstOrDefault(u => u.Id == id);
            if (categoryItem==null)
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
