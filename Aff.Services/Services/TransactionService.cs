using Aff.DataAccess;
using Aff.DataAccess.Common;
using Aff.DataAccess.Repositories;
using Aff.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Web.UI.WebControls;
using Microsoft.Office.Interop.Word;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Style;
using System.Drawing;
using System.ComponentModel;

namespace Aff.Services.Services
{
    public interface ITransactionService : IEntityService<tblTransaction>
    {
        List<TransactionModel> RetrieveTransactionByUser(int userId);
        TransactionModel RetrieveTransaction(long transactionId);
        List<TransactionModel> SearchTransaction(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, out int totalPage);
        bool CreateTransaction(TransactionModel transactionModel, out string message);
        TransactionApiModel GetSummaryTransaction(DateTime fromDate, DateTime toDate);
        long GetSummary(DateTime fromDate, DateTime toDate, out long totalPaymentAmount);
        Stream CreateExcelFile(int userId, Stream stream = null);
    }

    public class TransactionService : EntityService<tblTransaction>, ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IConfigRepository _configRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        public TransactionService(IUnitOfWork unitOfWork, ITransactionRepository transactionRepository, IConfigRepository configRepository, IReportRepository reportRepository, IUserRepository userRepository)
            : base(unitOfWork, transactionRepository)
        {
            _transactionRepository = transactionRepository;
            _configRepository = configRepository;
            _reportRepository = reportRepository;
            _userRepository = userRepository;
        }

        public bool CreateTransaction(TransactionModel transactionModel, out string message)
        {
            message = "";
            //Create transaction
            _transactionRepository.CreateTransaction(transactionModel, out message);
            return true;
        }

        public TransactionModel RetrieveTransaction(long transactionId)
        {
            var transactionEntities = _transactionRepository.RetrieveTransaction(transactionId);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModel();
            }
            return null;
        }

        public List<TransactionModel> SearchTransaction(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, out int totalPage)
        {
            var transactionEntities = _transactionRepository.SearchTransactionByUser(textSearch, currentPage, pageSize, userId, sortField, sortType, out totalPage);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModels();
            }
            return null;
        }

        public List<TransactionModel> RetrieveTransactionByUser(int userId)
        {
            var trans = _transactionRepository.GetAllTranSaction(userId);
            
            if (trans != null && trans.Any())
            {
                var returnList = trans.ToList().MapToModels();
                return returnList;
            }
            return null;
        }

        public TransactionApiModel GetSummaryTransaction(DateTime fromDate, DateTime toDate)
        {
            long totalPurchaseAmount = 0;
            int totalInvoice = 0;
            long totalPaymentAmount = 0;
            var trans = GetAll().AsQueryable().Include(i => i.tblReports).Where(t => t.CreatedDate.HasValue && t.CreatedDate.Value > fromDate && t.CreatedDate.Value < toDate && t.tblReports.Any());
            if (trans != null && trans.Any())
            {
                totalInvoice = trans.Count();
                totalPurchaseAmount = trans.Where(o=>o.TotalAmount.HasValue).Sum(t => t.TotalAmount).Value;
                totalPaymentAmount = trans.Where(o => o.TotalAmount.HasValue).Sum(t => t.tblReports.Where(s=>s.Amount.HasValue).Sum(r => r.Amount)).Value;
            }
            return new TransactionApiModel
            {
                TotalInvoice = totalInvoice,
                TotalPaymentAmount = totalPaymentAmount,
                TotalPurchaseAmount = totalPurchaseAmount
            };
        }

        public long GetSummary(DateTime fromDate, DateTime toDate, out long totalPaymentAmount)
        {
            fromDate = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 1, 1, 1);
            toDate = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
            return _transactionRepository.GetSummary(fromDate, toDate, out totalPaymentAmount);
        }

        #region Export Exel

        private List<TransactionModel> GetTransactionForExcels(int userId)
        {
            var transactionEntities = _transactionRepository.GetAllTranSaction(userId);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModels();
            }
            return null;
        }

        public Stream CreateExcelFile(int userId, Stream stream = null)
        {
            var timeSheetDailys = GetTransactionForExcels(userId);
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {

                // Tạo author cho file Excel
                excelPackage.Workbook.Properties.Author = "Báo cáo";
                // Tạo title cho file Excel

                excelPackage.Workbook.Properties.Title = "Báo cáo đối soát";
                // Tạo comment cho file Exel 
                excelPackage.Workbook.Properties.Comments = "Xuất file exel";
                // Add Sheet vào file Excel
                excelPackage.Workbook.Worksheets.Add("First Sheet");
                // Lấy Sheet bạn vừa mới tạo ra để thao tác 
                var workSheet = excelPackage.Workbook.Worksheets[0];
             
                BindingFormatForExcel(workSheet, timeSheetDailys);
                excelPackage.Save();
                return excelPackage.Stream;
            }
        }
        private void Format(ExcelWorksheet worksheet)
        {
            // Tự động xuống hàng khi text quá dài
            worksheet.Cells.Style.WrapText = true;
            // Style cho border
            worksheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            worksheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            worksheet.Cells.Style.Border.Left.Color.SetColor(Color.Gray);
            worksheet.Cells.Style.Border.Right.Color.SetColor(Color.Gray);
            worksheet.Cells.Style.Border.Top.Color.SetColor(Color.Gray);
            worksheet.Cells.Style.Border.Bottom.Color.SetColor(Color.Gray);
            // Căn giữa cho text
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            // Set background cho Cells
            worksheet.Cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells.Style.Fill.BackgroundColor.SetColor(Color.White);

        }
        private void BindingFormatForExcel(ExcelWorksheet worksheet, List<TransactionModel> listItems)
        {
            // Set default width cho tất cả column
            worksheet.DefaultColWidth = 20;
            // Set width cho cột 
            worksheet.Column(1).Width = 10;
            // Set height
            worksheet.Row(1).Height = 24;
            worksheet.DefaultRowHeight = 20;
            Format(worksheet);
            string tenCty = "JOBNOW";

            //Tên công ty
            //worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells[1, 1].Value = tenCty;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells["A1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 5].Value = " ";

            //Tiêu đề
            worksheet.Cells["A2:H2"].Merge = true;
            worksheet.Cells[2, 1].Value = "BÁO CÁO ĐỐI SOÁT";
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.Font.Size = 20;
            worksheet.Row(2).Height = 25;
            worksheet.Cells[2, 5].Value = " ";


            // Tạo header
            worksheet.Cells[4, 1].Value = "STT";
            worksheet.Cells[4, 2].Value = "Tên giao dịch";
            worksheet.Cells[4, 3].Value = "Người giao dịch";
            worksheet.Cells[4, 4].Value = "Đầu kì";
            worksheet.Cells[4, 5].Value = "Ngày giao dịch";
            worksheet.Cells[4, 6].Value = "Phần trăm";
            worksheet.Cells[4, 7].Value = "Lợi nhuận";
            worksheet.Cells[4, 8].Value = "Tổng tiền thực";


            // Lấy range vào tạo format cho range đó ở đây là từ A1 tới H1
            using (var range = worksheet.Cells["A4:H4"])
            {
                // Set PatternType
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                // Set Màu cho Background
                range.Style.Fill.BackgroundColor.SetColor(Color.DarkCyan);
                // Căn giữa cho các text
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                // Set Font cho text  trong Range hiện tại
                range.Style.Font.SetFromFont(new System.Drawing.Font("Arial", 10));
                // Set color cho text
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.Font.Bold = true;
            }
            int row = 5;
            int i = 0;
            double totalMoney = 0;
            foreach (var item in listItems)
            {
                i += 1;
                worksheet.Cells[string.Format("A{0}", row)].Value = i;
                worksheet.Cells[string.Format("B{0}", row)].Value = item.FullName;
                worksheet.Cells[string.Format("C{0}", row)].Value = item.FullName;
                worksheet.Cells[string.Format("D{0}", row)].Value = item.FullName;
                worksheet.Cells[string.Format("E{0}", row)].Value = item.CreatedDate;
                worksheet.Cells[string.Format("F{0}", row)].Value = item.PercentAmount;
                worksheet.Cells[string.Format("G{0}", row)].Value = item.TotalProfit;
                worksheet.Cells[string.Format("H{0}", row)].Value = item.TotalAmount;
                totalMoney += item.TotalAmount;
                row++;
            }
            worksheet.Cells[string.Format("A{0}:G{0}", row)].Style.Font.Bold = true;
            worksheet.Cells[string.Format("A{0}:G{0}", row)].Merge = true;
            worksheet.Cells[string.Format("A{0}:G{0}", row)].Value = "Tổng";
            worksheet.Cells[string.Format("G{0}:H{0}", row)].Value = totalMoney;
            //worksheet.Cells[string.Format("D{0}", row)].Value = item.TongCong.DauKy;
            //worksheet.Cells[string.Format("E{0}", row)].Value = item.TongCong.SoLuongNhap;
            //worksheet.Cells[string.Format("F{0}", row)].Value = item.TongCong.SoLuongXuat;
            //worksheet.Cells[string.Format("G{0}", row)].Value = item.TongCong.TonKho;
            //worksheet.Cells[string.Format("H{0}", row)].Value = item.TongCong.DinhMucSuDung;
            row++;
        }

        #endregion

    }
}
