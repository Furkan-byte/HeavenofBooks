
using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using HeavenofBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeavenofBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin + "," +StaticDetails.Role_User_Company)]

    public class CoverTypeController : Controller
    {
        private readonly IUnitofWork _db;

        public CoverTypeController(IUnitofWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> dbContextVariables = _db.CoverType.GetAll();
            return View(dbContextVariables);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType coverType)
        {
          
            if (ModelState.IsValid)
            {
                _db.CoverType.Add(coverType);
                _db.Save();
                TempData["Success"] = "Cover Type created successufly!";
                return RedirectToAction("Index");
            }
            return View(coverType);
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverTypeFromDb = _db.CoverType.GetFirstOrDefault(u => u.Id == id);
            return View(coverTypeFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType coverType)
        {
            
            if (ModelState.IsValid)
            {
                _db.CoverType.Update(coverType);
                _db.Save();
                TempData["Success"] = "Cover Type updated successufly!";
                return RedirectToAction("Index");
            }
            return View(coverType);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var coverTypeFromDb = _db.CoverType.GetFirstOrDefault(u => u.Id == id);
            return View(coverTypeFromDb);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategoryPost(int? id)
        {
            var coverTypeItem = _db.CoverType.GetFirstOrDefault(u => u.Id == id);
            if (coverTypeItem == null)
            {
                return NotFound();
            }
            _db.CoverType.Remove(coverTypeItem);
            _db.Save();
            TempData["Success"] = "Cover Type deleted successufly!";
            return RedirectToAction("Index");

        }
    }
}
