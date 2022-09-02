using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using HeavenofBooks.Models.ViewModels;
using HeavenofBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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

            AppUser appUser = _contextUoW.AppUser.GetFirstOrDefault(u => u.Id == claim.Value);
            if (appUser.Company.GetValueOrDefault() ==0)
            {
                shoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            }
            else
            {
                shoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusApproved;
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
            if (appUser.Company.GetValueOrDefault() == 0)
            {

            
            //stripe settings
            var domain = "https://localhost:44335/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
              
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain+$"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain+$"customer/cart/index",
            };

            foreach (var item in shoppingCartVM.ListCart)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title,
                        },

                    },
                    Quantity = item.Counter,
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _contextUoW.OrderHeader.UpdateStripePaymentId(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _contextUoW.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
            }
            else
            {
                return RedirectToAction("OrderConfirmation", "Cart", new { id = shoppingCartVM.OrderHeader.Id });
            }
            //_contextUoW.ShoppingCart.RemoveRange(shoppingCartVM.ListCart);
            //_contextUoW.Save();

            //return RedirectToAction("Index", "Home");
        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _contextUoW.OrderHeader.GetFirstOrDefault(u => u.Id == id);
            if (orderHeader.PaymentStatus != StaticDetails.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _contextUoW.OrderHeader.UpdateStatus(id, StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
                    _contextUoW.Save();
                }
            }
            
            List<ShoppingCart> shoppingCarts = _contextUoW.ShoppingCart.GetAll(u => u.AppUserId == orderHeader.AppUserId).ToList();
            _contextUoW.ShoppingCart.RemoveRange(shoppingCarts);
            _contextUoW.Save();

            return View(id);
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
