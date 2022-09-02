using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using HeavenofBooks.Models.ViewModels;
using HeavenofBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HeavenofBooksWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitofWork _contextUoW;
        public ShoppingCartVM shoppingCartVM { get; set; }
        public int OrderTotal { get; set; }
        public CartController(IUnitofWork contextUoW)
        {
            _contextUoW = contextUoW;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _contextUoW.ShoppingCart.GetAll(u => u.AppUserId == claim.Value, includeProperties: "Product"),
                OrderHeader = new()
            };
            foreach (var cart in shoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Counter, cart.Product.Price, 
                    cart.Product.Price50, cart.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Counter);
            }
            if (shoppingCartVM.OrderHeader.OrderTotal >= 2500)
            {
                shoppingCartVM.OrderHeader.OrderTotal -= shoppingCartVM.OrderHeader.OrderTotal / 50;
            }
            return View(shoppingCartVM);
        }

        public IActionResult Summary()
        {
              var claimsIdentity = (ClaimsIdentity)User.Identity;
              var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
              shoppingCartVM = new ShoppingCartVM()
              {
                  ListCart = _contextUoW.ShoppingCart.GetAll(u => u.AppUserId == claim.Value, includeProperties: "Product"),
                  OrderHeader = new()
              };
            shoppingCartVM.OrderHeader.appUser = _contextUoW.AppUser.GetFirstOrDefault(u => u.Id == claim.Value);
            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.appUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.appUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.appUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.appUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.appUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.appUser.PostalCode;

            
            foreach (var cart in shoppingCartVM.ListCart)
              {
                  cart.Price = GetPriceBasedOnQuantity(cart.Counter, cart.Product.Price,
                      cart.Product.Price50, cart.Product.Price100);
                  shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Counter);
              }
              if (shoppingCartVM.OrderHeader.OrderTotal >= 2500)
              {
                  shoppingCartVM.OrderHeader.OrderTotal -= shoppingCartVM.OrderHeader.OrderTotal / 50;
              }
              return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        [ValidateAntiForgeryToken]
        public IActionResult SummaryPOST(ShoppingCartVM shoppingCartVM)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            shoppingCartVM.ListCart = _contextUoW.ShoppingCart.GetAll(
                u => u.AppUserId == claim.Value, includeProperties: "Product");

            shoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
            shoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            shoppingCartVM.OrderHeader.AppUserId = claim.Value;

            foreach (var cart in shoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQuantity(cart.Counter, cart.Product.Price,
                    cart.Product.Price50, cart.Product.Price100);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Counter);
            }
            if (shoppingCartVM.OrderHeader.OrderTotal >= 2500)
            {
                shoppingCartVM.OrderHeader.OrderTotal -= shoppingCartVM.OrderHeader.OrderTotal / 50;
            }

            _contextUoW.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _contextUoW.Save();

            foreach (var cart in shoppingCartVM.ListCart)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Counter
                };
                _contextUoW.OrderDetail.Add(orderDetail);
                _contextUoW.Save();
            }

            _contextUoW.ShoppingCart.RemoveRange(shoppingCartVM.ListCart);
            _contextUoW.Save();

            return RedirectToAction("Index" , "Home");
        }

        public IActionResult Plus(int cartId)
        {
            var cart = _contextUoW.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            _contextUoW.ShoppingCart.IncrementCount(cart, 1);
            _contextUoW.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cart = _contextUoW.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);
            if (cart.Counter<1)
            {
                _contextUoW.ShoppingCart.Remove(cart);
            }
            else 
            {
                _contextUoW.ShoppingCart.DecrementCount(cart, 1);
            }           
            _contextUoW.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Delete(int cartId)
        {
            var cart = _contextUoW.ShoppingCart.GetFirstOrDefault(u => u.Id == cartId);          
            _contextUoW.ShoppingCart.Remove(cart);
            _contextUoW.Save();
            return RedirectToAction(nameof(Index));
        }
        private double GetPriceBasedOnQuantity(double quantity, double price,double price50,double price100)
        {
            if (quantity<=50)
            {
                return price;
            }
            else
            {
                if (quantity<=100)
                {
                    return price50;
                }
                return price100;
            }
        }
    }
}
