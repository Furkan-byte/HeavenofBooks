using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HeavenofBooksWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitofWork _contextUiO;

        public HomeController(ILogger<HomeController> logger,IUnitofWork contextUiO)
        {
            _logger = logger;
            _contextUiO = contextUiO;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _contextUiO.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }
        public IActionResult Details(int id)
        {
            ShoppingCart shoppingCart = new()
            {
                Counter = 1,
                Product = _contextUiO.Product.GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType"),
            };
                   return View(shoppingCart);
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