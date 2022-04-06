using Aff.Models.Models;
using Aff.Services.Services;
using Aff.WebApplication.Filters;
using Autofac.Integration.Mvc;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Aff.Services;
using System;
using System.Globalization;
using System.IO;

namespace Aff.WebApplication.Controllers
{
    public class TransactionController : BaseController
    {
        private readonly ITransactionService _transactionService;
        private readonly IPaymentService _paymentService;
        private readonly IReportService _reportService;
        private readonly IBankAccountService _bankAccountService;
        public TransactionController(ITransactionService transactionService, IReportService reportService, IPaymentService paymentService, IBankAccountService bankAccountService)
        {
            _transactionService = transactionService;
            _reportService = reportService;
            _paymentService = paymentService;
            _bankAccountService = bankAccountService;
        }
        // GET: Transaction
        public ActionResult Index()
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult CreateTransaction(TransactionModel transModel)
        {
            return View();
        }
        [CustomAuthorize]
        public ActionResult Balance()
        {
            var balanceModel = _reportService.RetrieveBalanceByUser(CurrentUser.UserId);
            //balanceModel.TotalAmountEarning = CurrentUser.TotalAmountEarning;
            //balanceModel.AvailableBalance = CurrentUser.AvailableBalance;
            var pendingAmount = _paymentService.GetAmountWithDrawStatus(CurrentUser.UserId, new List<int> {1,2});
            if(pendingAmount != null && pendingAmount.Any())
            {
                balanceModel.PendingAmountEarning = pendingAmount.Sum(p => p.AmountEarning);
            }
            return PartialView("~/Views/Transaction/Partial/_Balance.cshtml", balanceModel);
        }
        [CustomAuthorize]
        public ActionResult withdraws(TransactionModel transModel)
        {
            var bankAccount = _bankAccountService.RetrieveBankAccountsByUser(CurrentUser.UserId);
            ViewBag.HasBankAccount = (bankAccount != null && bankAccount.Any()) ? 1 : 0;
            return View();
        }
        [CustomAuthorize]
        public ActionResult SearchTransaction(TransactionSearchModel transModel)
        {
            int totalRecords = 0;
            transModel.RequestParam(Request.Params);
            transModel.PageSize = transModel.PageSize > 100 ? transModel.PageSize = 30 : transModel.PageSize;
            var transactions = _transactionService.SearchTransaction(transModel.TextSearch, transModel.PageIndex, transModel.PageSize, CurrentUser.UserId, transModel.Field, transModel.Sort, out totalRecords);
            return Json(new
            {
                meta = new MetaData
                {
                    field = transModel.Field,
                    page = transModel.PageIndex,
                    pages = transModel.Pages,
                    perpage = transModel.PageSize,
                    total = totalRecords,
                    sort = transModel.Sort
                },
                data = transactions
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }
        [CustomAuthorize]
        public ActionResult statistic(TransactionSearchModel transModel)
        {
            return View();
        }

        #region Export Exel
        [HttpGet]
        public void ExportTransaction(int userId)
        {
            DateTime date = DateTime.Now;
            DateTime? fromDate = null;
            DateTime? toDate = null;
            // Gọi lại hàm để tạo file excel                      
            Stream stream = _transactionService.CreateExcelFile(userId);

            // Tạo buffer memory stream để hứng file excel
            var buffer = stream as MemoryStream;
            Response.Clear();
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AddHeader("content-disposition", "attachment; filename = BaoCaoDoiSoat" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
            Response.ContentType = "application/text";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Response.BinaryWrite(buffer.ToArray());
            Response.End();
        }

        #endregion

    }
}