using Aff.Models.Models;
using Aff.Services.Services;
using Aff.WebApplication.Filters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aff.WebApplication.Controllers
{
    public class LoanController : BaseController
    {
        private readonly ILoanService _loanService;
        private readonly ITransactionService _transactionService;
        public LoanController(ILoanService loanService, ITransactionService transactionService)
        {
            _loanService = loanService;
            _transactionService = transactionService;
        }
        // GET: Loan
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult SearchLoans(LoanSearchModel loanModel)
        {
            int totalRecords = 0;
            loanModel.RequestParam(Request.Params);
            loanModel.PageSize = loanModel.PageSize > 100 ? loanModel.PageSize = 30 : loanModel.PageSize;
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!string.IsNullOrEmpty(Request.Params["datatable[query][Range]"]) && Request.Params["datatable[query][Range]"].IndexOf("/") > 0)
            {
                var dates = Request.Params["datatable[query][Range]"].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (dates.Length == 2)
                {
                    CultureInfo vn = new CultureInfo("vi-VN");

                    fromDate = Convert.ToDateTime(dates[0].Trim(), vn.DateTimeFormat);
                    toDate = Convert.ToDateTime(dates[1].Trim(), vn.DateTimeFormat);
                }
            }

            var loans = _loanService.SearchLoans(loanModel.TextSearch, loanModel.PageIndex, loanModel.PageSize, CurrentUser.AffCode, loanModel.Field, loanModel.Sort, out totalRecords, fromDate, toDate);
            return Json(new
            {
                meta = new MetaData
                {
                    field = loanModel.Field,
                    page = loanModel.PageIndex,
                    pages = loanModel.Pages,
                    perpage = loanModel.PageSize,
                    total = totalRecords,
                    sort = loanModel.Sort
                },
                data = loans
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }
        [CustomAuthorize]
        public ActionResult statistic(TransactionSearchModel transModel)
        {
            return View();
        }
        [CustomAuthorize]
        public ActionResult statisticadmin(TransactionSearchModel transModel)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.DateRangePicker = string.Format("{0} / {1}", DateTime.Now.ToString("dd-MM-yyyy"), DateTime.Now.ToString("dd-MM-yyyy"));
            //long totalPaymentAmount = 0;
            //var totalPurchaseAmount = _transactionService.GetSummary(DateTime.Now, DateTime.Now, out totalPaymentAmount);
            
            //ViewBag.TotalPurchaseAmount = totalPurchaseAmount;
            //ViewBag.TotalPaymentAmount = totalPaymentAmount;
            //ViewBag.TotalProfitAmount = (totalPurchaseAmount - totalPaymentAmount);
            return View();
        }

        [CustomAuthorize]
        public ActionResult SearchLoansAdmin(LoanSearchModel loanModel)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            int totalRecords = 0;
            loanModel.RequestParam(Request.Params);
            loanModel.PageSize = loanModel.PageSize > 100 ? loanModel.PageSize = 30 : loanModel.PageSize;
            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;
            if (!string.IsNullOrEmpty(Request.Params["datatable[query][Range]"]) && Request.Params["datatable[query][Range]"].IndexOf("/") > 0)
            {
                var dates = Request.Params["datatable[query][Range]"].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (dates.Length == 2)
                {
                    CultureInfo vn = new CultureInfo("vi-VN");

                    fromDate = Convert.ToDateTime(dates[0].Trim(), vn.DateTimeFormat);
                    toDate = Convert.ToDateTime(dates[1].Trim(), vn.DateTimeFormat);
                }
            }
            var loans = _loanService.SearchLoansAdmin(loanModel.TextSearch, loanModel.PageIndex, loanModel.PageSize, CurrentUser.AffCode, loanModel.Field, loanModel.Sort, out totalRecords, fromDate: fromDate, toDate: toDate);

            long totalPaymentAmount = 0;
            var totalPurchaseAmount = _transactionService.GetSummary(fromDate, toDate, out totalPaymentAmount);

            return Json(new
            {
                meta = new MetaData
                {
                    field = loanModel.Field,
                    page = loanModel.PageIndex,
                    pages = loanModel.Pages,
                    perpage = loanModel.PageSize,
                    total = totalRecords,
                    sort = loanModel.Sort
                },
                data = loans,
                totalPurchase = totalPurchaseAmount,
                totalPayment = totalPaymentAmount,
                totalProfit = (totalPurchaseAmount - totalPaymentAmount)
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }
    }
}