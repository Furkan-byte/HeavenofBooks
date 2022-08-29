
using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using HeavenofBooks.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HeavenofBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnitofWork _context;

        public CompanyController(IUnitofWork db)
        {
            _context = db;
        }

        public IActionResult Index()
        {
            IEnumerable<AppCompany> dbContextVariables = _context.Company.GetAll();
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
            AppCompany company = new();
            if (id == null || id == 0)
            {
                //ViewBag.CategoryList = CategoryList;
                //ViewData["CoverTypeList"] = CoverTypeList;
                return View(company);
            }
            else
            {
                company = _context.Company.GetFirstOrDefault(u => u.Id == id);
                return View(company);
            }
            
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(AppCompany company)
        {
            
            if (ModelState.IsValid)
            {
               
                if (company.Id ==0)
                {
                    _context.Company.Add(company);
                    TempData["Success"] = "Company created successufly!";
                }
                else
                {
                    _context.Company.Update(company);
                    TempData["Success"] = "Company updated successufly!";
                }
                
                _context.Save();
                
                return RedirectToAction("Index");
            }
            return View(company);
        }
        
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var CompanyList = _context.Company.GetAll();
            return Json(new { data = CompanyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyItem = _context.Company.GetFirstOrDefault(u => u.Id == id);
         
            if (companyItem == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _context.Company.Remove(companyItem);
            _context.Save();

            return Json(new { success = true, message = "Deleted successfuly" });

        }
        #endregion
    }

}
