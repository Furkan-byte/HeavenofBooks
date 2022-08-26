
using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using HeavenofBooks.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HeavenofBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitofWork _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitofWork db, IWebHostEnvironment hostEnvironment)
        {
            _context = db;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> dbContextVariables = _context.Product.GetAll();
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
                _context.Product.Add(product);
                _context.Save();
                TempData["Success"] = "Product created successufly!";
                return RedirectToAction("Index");
            }
            return View(product);
        }
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                product = new(),
                CategoryList = _context.Category.GetAll().Select(
                    u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    }),
                CoverTypeList = _context.CoverType.GetAll().Select(
                     u => new SelectListItem
                     {
                         Text = u.Name,
                         Value = u.Id.ToString()
                     }),

            };
            

            if (id == null || id == 0)
            {
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(productVM);
            }
            else
            {
                productVM.product = _context.Product.GetFirstOrDefault(u => u.Id == id);
                return View(productVM);
            }
            
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM, IFormFile file)
        {
            
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);
                    if (productVM.product.ImageUrl !=null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension),FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    productVM.product.ImageUrl = @"\images\products\" + fileName + extension;
                }
                if (productVM.product.Id ==0)
                {
                    _context.Product.Add(productVM.product);
                    TempData["Success"] = "Product created successufly!";
                }
                else
                {
                    _context.Product.Update(productVM.product);
                    TempData["Success"] = "Product updated successufly!";
                }
                
                _context.Save();
                
                return RedirectToAction("Index");
            }
            return View(productVM);
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _context.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = productList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productItem = _context.Product.GetFirstOrDefault(u => u.Id == id);

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, productItem.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            if (productItem == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _context.Product.Remove(productItem);
            _context.Save();

            return Json(new { success = true, message = "Deleted successfuly" });

        }
        #endregion
    }

}
