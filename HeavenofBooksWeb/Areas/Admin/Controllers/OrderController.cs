using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using HeavenofBooks.Models.ViewModels;
using HeavenofBooks.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace HeavenofBooksWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitofWork _contextUoW;
        [BindProperty]
        public OrderVM orderVM { get; set; }
        public OrderController(IUnitofWork contextUoW)
        {
            _contextUoW = contextUoW;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            orderVM = new OrderVM()
            {
                orderHeader = _contextUoW.OrderHeader.GetFirstOrDefault(u => u.Id == orderId, includeProperties: "appUser"),
                orderDetail = _contextUoW.OrderDetail.GetAll(u => u.OrderId == orderId, includeProperties: "product"),
            };
            return View(orderVM);
        }
        [ActionName("Details")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DetailsPAY_NOW()
        {
            orderVM.orderHeader = _contextUoW.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeader.Id, includeProperties: "appUser");
            orderVM.orderDetail = _contextUoW.OrderDetail.GetAll(u => u.OrderId == orderVM.orderHeader.Id, includeProperties: "product");

            var domain = "https://localhost:44335/";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },

                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderid={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details=orderId={orderVM.orderHeader.Id}",
            };

            foreach (var item in orderVM.orderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "try",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.product.Title,
                        },

                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            Session session = service.Create(options);
            _contextUoW.OrderHeader.UpdateSessionId(orderVM.orderHeader.Id, session.Id);
            _contextUoW.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        public IActionResult PaymentConfirmation(int orderHeaderid)
        {
            OrderHeader orderHeader = _contextUoW.OrderHeader.GetFirstOrDefault(u => u.Id == orderHeaderid);
            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _contextUoW.OrderHeader.UpdateStatus(orderHeaderid, orderHeader.OrderStatus, StaticDetails.PaymentStatusApproved);
                    _contextUoW.OrderHeader.UpdatePaymentIntentId(orderHeaderid, session.PaymentIntentId);
                    _contextUoW.Save();
                }
            }
            return View(orderHeaderid);
        }
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + ","+ StaticDetails.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderfromDB = _contextUoW.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeader.Id,tracked:false);
            orderHeaderfromDB.Name = orderVM.orderHeader.Name;
            orderHeaderfromDB.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderHeaderfromDB.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderHeaderfromDB.City = orderVM.orderHeader.City;
            orderHeaderfromDB.State = orderVM.orderHeader.State;
            orderHeaderfromDB.PostalCode = orderVM.orderHeader.PostalCode;
			if (orderVM.orderHeader.Carrier !=null)
			{
                orderHeaderfromDB.Carrier = orderVM.orderHeader.Carrier;
            }
            if (orderVM.orderHeader.TrackingNumber != null)
            {
                orderHeaderfromDB.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            }
            _contextUoW.OrderHeader.Update(orderHeaderfromDB);
            _contextUoW.Save();
            TempData["Success"] = "Order Details updated successfuly.";
            return RedirectToAction("Details", "Order", new {orderId = orderHeaderfromDB.Id});
        }
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult StartProcessing()
        {
            _contextUoW.OrderHeader.UpdateStatus(orderVM.orderHeader.Id, StaticDetails.StatusInProcess);
            _contextUoW.Save();
            TempData["Success"] = "Order Status updated successfuly.";
            return RedirectToAction("Details", "Order", new { orderId = orderVM.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult ShipOrder()
        {
            var orderHeaderfromDB = _contextUoW.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeader.Id, tracked: false);
            orderHeaderfromDB.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeaderfromDB.Carrier = orderVM.orderHeader.Carrier;
            orderHeaderfromDB.OrderStatus = StaticDetails.StatusShipped;
            orderHeaderfromDB.ShippingDate = DateTime.Now;
            if (orderHeaderfromDB.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
            {
                orderHeaderfromDB.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            _contextUoW.OrderHeader.Update(orderHeaderfromDB);
            _contextUoW.Save();
            TempData["Success"] = "Order Shipped successfuly.";
            return RedirectToAction("Details", "Order", new { orderId = orderVM.orderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            var orderHeaderfromDB = _contextUoW.OrderHeader.GetFirstOrDefault(u => u.Id == orderVM.orderHeader.Id, tracked: false);
            if (orderHeaderfromDB.PaymentStatus == StaticDetails.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderfromDB.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _contextUoW.OrderHeader.UpdateStatus(orderHeaderfromDB.Id ,
                    StaticDetails.StatusCancelled,StaticDetails.StatusRefunded);
            }
            else
            {
                _contextUoW.OrderHeader.UpdateStatus(orderHeaderfromDB.Id,
                    StaticDetails.StatusCancelled, StaticDetails.StatusCancelled);
            }
           
            _contextUoW.Save();
            TempData["Success"] = "Order Cancelled successfuly.";
            return RedirectToAction("Details", "Order", new { orderId = orderVM.orderHeader.Id });
        }
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
            if (User.IsInRole(StaticDetails.Role_Admin) || User.IsInRole(StaticDetails.Role_Employee))
            {
                orderHeaders = _contextUoW.OrderHeader.GetAll(includeProperties: "appUser");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                orderHeaders = _contextUoW.OrderHeader.GetAll(u => u.AppUserId == claim.Value, includeProperties: "appUser");
            }
            

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == StaticDetails.PaymentStatusPending);
                    break;
                case "paymentdelayed":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticDetails.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticDetails.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == StaticDetails.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = orderHeaders });
        }
        #endregion
    }
}
