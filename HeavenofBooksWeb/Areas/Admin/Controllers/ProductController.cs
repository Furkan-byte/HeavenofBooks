
using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HeavenofBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitofWork _db;

        public ProductController(IUnitofWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> dbContextVariables = _db.Product.GetAll();
            return View(dbContextVariables);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
          
            if (ModelState.IsValid)
            {
                _db.Product.Add(product);
                _db.Save();
                TempData["Success"] = "Product created successufly!";
                return RedirectToAction("Index");
            }
            return View(product);
        }
        public IActionResult Upsert(int? id)
        {
            Product product = new();
            IEnumerable<SelectListItem> CategoryList = _db.Category.GetAll().Select
                (u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }) ;
            IEnumerable<SelectListItem> CoverTypeList = _db.CoverType.GetAll().Select
                (u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            if (id == null || id == 0)
            {
                ViewBag.CategoryList = CategoryList;
                return View(product);
            }
            
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Product product)
        {
            
            if (ModelState.IsValid)
            {
                _db.Product.Update(product);
                _db.Save();
                TempData["Success"] = "Product updated successufly!";
                return RedirectToAction("Index");
            }
            return View(product);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var productFromDb = _db.Product.GetFirstOrDefault(u => u.Id == id);
            return View(productFromDb);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategoryPost(int? id)
        {
            var productItem = _db.Product.GetFirstOrDefault(u => u.Id == id);
            if (productItem == null)
            {
                return NotFound();
            }
            _db.Product.Remove(productItem);
            _db.Save();
            TempData["Success"] = "Product deleted successufly!";
            return RedirectToAction("Index");

        }
    }
}
