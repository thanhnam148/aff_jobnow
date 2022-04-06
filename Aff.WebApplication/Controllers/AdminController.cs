using Aff.DataAccess;
using Aff.DataAccess.Repositories;
using Aff.Services;
using Aff.Services.Services;
using Aff.WebApplication.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aff.WebApplication.Controllers
{
    public class AdminController : BaseController
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IViewCountServices _viewCountServices;
        
        public AdminController(IBankAccountService bankAccountService, IPaymentService paymentService)
        {
            _bankAccountService = bankAccountService;
        }
        [CustomAuthorize]
        public ActionResult Index()
        {
            var viewCountInDay = 0;
            var bankAccount = _bankAccountService.RetrieveBankAccountsByUser(CurrentUser.UserId);
            ViewBag.HasBankAccount = (bankAccount != null && bankAccount.Any() || CurrentUser.RoleType < 2) ? 1 : 0;

            TimaAffiliateEntities dbContext = new TimaAffiliateEntities();
            var repo = new ViewCountRepository(dbContext);
            ViewBag.TotalCountAccess = repo.TotalAccessLinkRef(CurrentUser.AffCode, out viewCountInDay);
            ViewBag.ViewCountInDay = viewCountInDay;
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

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}