using Aff.Models.Models;
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
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IBankAccountService _bankAccountService;
        public PaymentController(IPaymentService paymentService, IBankAccountService bankAccountService)
        {
            _paymentService = paymentService;
            _bankAccountService = bankAccountService;
        }
        // GET: Payment
        public ActionResult Index()
        {
            return View();
        }
        [CustomAuthorize]
        public ActionResult SearchPayments(PaymentSearchModel paymentModel)
        {
            int totalRecords = 0;
            paymentModel.RequestParam(Request.Params);
            //paymentModel.PageSize = paymentModel.PageSize > 100 ? paymentModel.PageSize = 30 : paymentModel.PageSize;
            var pStatus = new List<int> { 1,2 };
            var payments = _paymentService.SearchPayments(paymentModel.TextSearch, paymentModel.PageIndex, paymentModel.PageSize, CurrentUser.UserId, paymentModel.Field, paymentModel.Sort, pStatus, out totalRecords);
            return Json(new
            {
                meta = new MetaData
                {
                    field = paymentModel.Field,
                    page = paymentModel.PageIndex,
                    pages = paymentModel.Pages,
                    perpage = paymentModel.PageSize,
                    total = totalRecords,
                    sort = paymentModel.Sort
                },
                data = payments
            }, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize]
        public ActionResult SearchInvoices(PaymentSearchModel paymentModel)
        {
            int totalRecords = 0;
            paymentModel.RequestParam(Request.Params);
            paymentModel.PageSize = paymentModel.PageSize > 100 ? paymentModel.PageSize = 30 : paymentModel.PageSize;
            var pStatus = new List<int> { 3 };
            var payments = _paymentService.SearchPayments(paymentModel.TextSearch, paymentModel.PageIndex, paymentModel.PageSize, CurrentUser.UserId, paymentModel.Field, paymentModel.Sort, pStatus, out totalRecords);
            return Json(new
            {
                meta = new MetaData
                {
                    field = paymentModel.Field,
                    page = paymentModel.PageIndex,
                    pages = paymentModel.Pages,
                    perpage = paymentModel.PageSize,
                    total = totalRecords,
                    sort = paymentModel.Sort
                },
                data = payments
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }
        [CustomAuthorize]
        public ActionResult Invoice(TransactionSearchModel transModel)
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult AdminSearchPayments(PaymentSearchModel paymentModel)
        {
            int totalRecords = 0;
            paymentModel.RequestParam(Request.Params);
            //paymentModel.PageSize = paymentModel.PageSize > 100 ? paymentModel.PageSize = 30 : paymentModel.PageSize;
            var pStatus = new List<int> { 1,2 };
            var payments = _paymentService.SearchPaymentByStatus(paymentModel.TextSearch, paymentModel.PageIndex, paymentModel.PageSize, paymentModel.Field, paymentModel.Sort, pStatus, out totalRecords);
            return Json(new
            {
                meta = new MetaData
                {
                    field = paymentModel.Field,
                    page = paymentModel.PageIndex,
                    pages = paymentModel.Pages,
                    perpage = paymentModel.PageSize,
                    total = totalRecords,
                    sort = paymentModel.Sort
                },
                data = payments
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }
        [CustomAuthorize]
        public ActionResult AdminPayment(TransactionSearchModel transModel)
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult PrintView(TransactionSearchModel transModel)
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult ValidePayment()
        {
            string message = "";
            var bankAccount = _bankAccountService.RetrieveBankAccountsByUser(CurrentUser.UserId);
            if(bankAccount == null || !bankAccount.Any())
            {
                message = "Bạn chưa cập nhật thông tin tài khoản. Xin vui lòng <a href=\"/account/UpdateUser\">cập nhật thông tin tài khoản</a> của bạn";
                return Json(new { IsError = true, Message = message });
            }
            var currentAvailableBalance = CurrentUser.AvailableBalance;
            var pendingAmount = _paymentService.GetAmountWithDrawStatus(CurrentUser.UserId, new List<int> { 1, 2 });
            if (pendingAmount != null && pendingAmount.Any())
            {
                currentAvailableBalance -= pendingAmount.Sum(p => p.AmountEarning);
            }
            if (currentAvailableBalance < 50000)
            {
                message = "Số dư khả dụng trong tài khoản của bạn phải lớn hơn 50,000.";
                return Json(new { IsError = true, Message = message });
            }

            return Json(new { IsError = false, Message = message });
        }
        [CustomAuthorize]
        public ActionResult CreatePayment(PaymentModel paymentModel)
        {
            var bankAccount = _bankAccountService.RetrieveBankAccountsByUser(CurrentUser.UserId).FirstOrDefault();
            //var balanceModel = _reportService.RetrieveBalanceByUser(CurrentUser.UserId);
            var pmodel = new PaymentModel
            {
                PaymentMethod = 1,
                AvailableBalance = CurrentUser.AvailableBalance,
                UserId = CurrentUser.UserId,
                AccountInfo = string.Format("Ngân hàng: {0} - Số TK: {1} - Tên TK: {2} - Chi nhánh: {3}.", bankAccount.BankName, bankAccount.BankAccount, bankAccount.BankOwnerName, bankAccount.BankAddress)
            };
            var pendingAmount = _paymentService.GetAmountWithDrawStatus(CurrentUser.UserId, new List<int> { 1, 2 });
            if (pendingAmount != null && pendingAmount.Any())
            {
                pmodel.AmountEarning = pendingAmount.Sum(p => p.AmountEarning);
            }
            return View(pmodel);
        }

        [CustomAuthorize]
        public ActionResult CreateNewPayment(PaymentModel paymentModel)
        {
            //paymentModel.UserId = CurrentUser.UserId;
            //_paymentService.CreatePayment(paymentModel);
            return RedirectToAction("withdraws", "Transaction");

        }
    }
}