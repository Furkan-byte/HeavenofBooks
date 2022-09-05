using HeavenofBooks.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using HeavenofBooks.Utility;

namespace HeavenofBooksWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitofWork _contextUoW;
        public ShoppingCartViewComponent(IUnitofWork contextUoW)
        {
            _contextUoW = contextUoW;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim!=null)
            {
                if (HttpContext.Session.GetInt32(StaticDetails.SessionCart)!=null)
                {
                    return View(HttpContext.Session.GetInt32(StaticDetails.SessionCart));
                }
                else
                {
                    HttpContext.Session.SetInt32(StaticDetails.SessionCart,
                        _contextUoW.ShoppingCart.GetAll(u => u.AppUserId == claim.Value).ToList().Count);
                    return View(HttpContext.Session.GetInt32(StaticDetails.SessionCart));
                }
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
