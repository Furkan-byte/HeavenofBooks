using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace HeavenofBooksWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _contextUoW;

        public HomeController(ILogger<HomeController> logger,IUnitofWork contextUiO)
        {
            _logger = logger;
            _contextUoW = contextUiO;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _contextUoW.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart shoppingCart = new()
            {
                Counter = 1,
                ProductId = productId,
                Product = _contextUoW.Product.GetFirstOrDefault(u => u.Id == productId, includeProperties: "Category,CoverType"),
            };
                   return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.AppUserId = claim.Value;

            ShoppingCart cartFromDb = _contextUoW.ShoppingCart.GetFirstOrDefault(u => u.AppUserId == claim.Value && u.ProductId == shoppingCart.ProductId);
            if (cartFromDb ==null)
            {
                _contextUoW.ShoppingCart.Add(shoppingCart);
                TempData["Success"] = "Product added into cart successfully.";
            }
            else
            {
                _contextUoW.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Counter);
            }
            
            _contextUoW.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}