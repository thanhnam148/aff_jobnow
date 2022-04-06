using Aff.Services;
using Aff.WebApplication.Filters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Security;

namespace Aff.WebApplication.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IContextService _contextService;
        public HomeController(IUserService userService, IContextService contextService)
        {
            _userService = userService;
            _contextService = contextService;
        }
        [CustomAuthorize]
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult Header()
        {
            return PartialView("~/Views/Shared/Partial/_HeaderPartial.cshtml", CurrentUser);
        }

        [CustomAuthorize]
        public ActionResult LeftMenu()
        {
            return PartialView("~/Views/Shared/Partial/_LeftMenuPartial.cshtml", CurrentUser);
        }

        [AllowAnonymous]
        public ActionResult About(string email, string p, int flag, string token)
        {
            ///--->/home/about?email=abc@gmail.comp=123456&flag=1&token=70196730-0bb6-40ee-816e-165efb2c5ea3
            if (flag != 1 || token != "70196730-0bb6-40ee-816e-165efb2c5ea3")
            {
                return RedirectToAction("Login", "Account");
            }    
            string errorMsg = "";
            var user = _userService.ValidateLogon(email, p, out errorMsg);
            if (user.Status == Aff.Models.LoginResult.Success)
            {
                var userData = JsonConvert.SerializeObject(user);
                var authTicket = new FormsAuthenticationTicket(1,
                    user.Email,
                    DateTime.Now,
                    DateTime.Now.AddMinutes(60),
                    false,
                    userData);
                var encTicket = FormsAuthentication.Encrypt(authTicket);
                var cookieKey = FormsAuthentication.FormsCookieName;
                _contextService.SaveInCookie(cookieKey, encTicket);
                FormsAuthentication.RedirectFromLoginPage(email, false);

                if (encTicket != null)
                    HttpRuntime.Cache.Insert(cookieKey, userData, null, DateTime.Now.AddHours(30),
                        Cache.NoSlidingExpiration);

                return RedirectToAction("Index", "Admin");
            }
            ViewBag.Message = "Your application description page.";

            return RedirectToAction("Login", "Account");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}