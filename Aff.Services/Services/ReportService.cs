using Aff.DataAccess;
using Aff.DataAccess.Common;
using Aff.DataAccess.Repositories;
using Aff.Models.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Services.Services
{
    public interface IReportService : IEntityService<tblReport>
    {
        BalanceModel RetrieveBalanceByUser(int userId);
        ReportModel RetrieveReport(int reportId);
        List<ReportModel> SearchReport(int currentPage, int pageSize, int userId, out int totalPage);
        List<ReportModel> GetByUserId(int userId);
        bool CreateReport(ReportModel reportModel, out string message);
        RevenueModel GetRevenue(int userId);
        Stream CreateExcelUserMattchingFile(int userId, Stream stream = null);
    }

    public class ReportService : EntityService<tblReport>, IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        public ReportService(IUnitOfWork unitOfWork, IReportRepository reportRepository, IUserRepository userRepository)
            : base(unitOfWork, reportRepository)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
        }

        public bool CreateReport(ReportModel reportModel, out string message)
        {
            throw new NotImplementedException();
        }

        public List<ReportModel> GetByUserId(int userId)
        {
            try
            {
                var entities = _reportRepository.GetByUserId(userId);
                if (entities != null)
                {
                    return entities.MapToModels();
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        //lấy doanh thu trực tiếp và gián tiếp
        public RevenueModel GetRevenue(int userId)
        {
            try
            {
                var entities = _reportRepository.GetByUserId(userId);

                if (entities != null)
                {
                    var revenueModelReturn = new RevenueModel();


                    var totalBalanceDirect = entities.Where(t => !t.UserReferId.HasValue && t.Amount.HasValue);
                    var totalBalanceBonus = entities.Where(t => t.UserReferId.HasValue && t.Amount.HasValue);

                    if (totalBalanceDirect != null)
                        revenueModelReturn.DirectRevenue = totalBalanceDirect.ToList().MapToModels();

                    if (totalBalanceBonus != null)
                        revenueModelReturn.IndirectRevenue = totalBalanceBonus.ToList().MapToModels();

                    return revenueModelReturn;

                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        public BalanceModel RetrieveBalanceByUser(int userId)
        {
            var userEntity = _userRepository.RetrieveUser(userId);
            if (userEntity != null)
            {
                var reportEntities = GetAll().AsQueryable().Where(r => r.UserId == userId);
                if (reportEntities != null)
                {

                    var totalBalanceDirect = reportEntities.Where(t => !t.UserReferId.HasValue && t.Amount.HasValue);
                    var totalBalanceBonus = reportEntities.Where(t => t.UserReferId.HasValue && t.Amount.HasValue);
                    if (userEntity.EndMatchingDate.HasValue)
                    {
                        userEntity.EndMatchingDate = userEntity.EndMatchingDate.Value.AddMinutes(1);
                        totalBalanceDirect = totalBalanceDirect.Where(r => r.CreatedDate > userEntity.EndMatchingDate.Value);
                        totalBalanceBonus = totalBalanceBonus.Where(r => r.CreatedDate > userEntity.EndMatchingDate.Value);
                    }

                    return new BalanceModel
                    {
                        UserId = userId,
                        TotalBalanceDirect = totalBalanceDirect != null && totalBalanceDirect.Any() ? totalBalanceDirect.Sum(r => r.Amount.Value) : 0,
                        TotalBalanceBonus = totalBalanceBonus != null && totalBalanceBonus.Any() ? totalBalanceBonus.Sum(r => r.Amount.Value) : 0,
                        TotalAmountEarning = userEntity.TotalAmountEarning.HasValue ? userEntity.TotalAmountEarning.Value : 0,
                        AvailableBalance = userEntity.AvailableBalance.HasValue ? userEntity.AvailableBalance.Value : 0
                    };
                }
            }

            return new BalanceModel();
        }

        public ReportModel RetrieveReport(int reportId)
        {
            throw new NotImplementedException();
        }

        public List<ReportModel> SearchReport(int currentPage, int pageSize, int userId, out int totalPage)
        {
            throw new NotImplementedException();
        }

        #region Export Exel UserTier

        private ReportModelExport GetUserMattchingForExcels(int userId)
        {
            try
            {
                var entityUser = _userRepository.GetById(userId).MapToModel();
                var entityReport = GetRevenue(userId);
                if (entityReport != null)
                {
                    entityUser.AvailableBalance = entityReport.IndirectRevenue.Sum(c => c.Amount) + entityReport.DirectRevenue.Sum(c => c.Amount);
                };
                var entityReturn = new ReportModelExport
                {
                    ReportModels = entityReport,
                    UserModel = entityUser
                };
                return entityReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
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
        public Stream CreateExcelUserMattchingFile(int userId, Stream stream = null)
        {
            var timeSheetDailys = GetUserMattchingForExcels(userId);
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {

                // Tạo author cho file Excel
                excelPackage.Workbook.Properties.Author = "LMS";
                // Tạo title cho file Excel

                excelPackage.Workbook.Properties.Title = "Báo cáo tài khoản";
                // Tạo comment cho file Exel 
                excelPackage.Workbook.Properties.Comments = "Xuất file exel";
                // Add Sheet vào file Excel
                excelPackage.Workbook.Worksheets.Add("First Sheet");
                // Lấy Sheet bạn vừa mới tạo ra để thao tác 
                var workSheet = excelPackage.Workbook.Worksheets[0];
                // Đổ data vào Excel file
                BindingFormatUserMattchingForExcel(workSheet, timeSheetDailys);
                excelPackage.Save();
                return excelPackage.Stream;
            }
        }
        private void BindingFormatUserMattchingForExcel(ExcelWorksheet worksheet, ReportModelExport listItems)
        {
            // Set default width cho tất cả column
            worksheet.DefaultColWidth = 20;
            // Set width cho cột 
            worksheet.Column(1).Width = 10;
            // Set height
            worksheet.Row(1).Height = 24;
            worksheet.DefaultRowHeight = 20;
            Format(worksheet);
            string tenCty = "Bảng cáo báo";

            //Tên công ty
            worksheet.Cells["A1:H1"].Merge = true;
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

            //khởi tạo bộ đếm dòng
            int row = 5;
            int i = 0;
            int indexRow = 5;

            if (listItems != null)
            {
                //lặp user
                if (listItems.UserModel != null)
                {
                    worksheet.Cells["A3:G3"].Merge = true;
                    worksheet.Cells[3, 1].Value = "Đối soát số dư";
                    worksheet.Cells["A3:G3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[3, 1].Style.Font.Bold = true;
                    worksheet.Cells[3, 1].Style.Font.Size = 10;

                    // Tạo header cho user
                    worksheet.Cells[4, 1].Value = "STT";
                    worksheet.Cells[4, 2].Value = "Mã liên kết";
                    worksheet.Cells[4, 3].Value = "Người giao dịch";
                    worksheet.Cells[4, 4].Value = "Đầu kì";
                    worksheet.Cells[4, 5].Value = "Ngày giao dịch";
                    worksheet.Cells[4, 6].Value = "Lợi nhuận";


                    // Lấy range vào tạo format cho range đó ở đây là từ A1 tới H1
                    using (var range = worksheet.Cells["A4:F4"])
                    {
                        // Set PatternType
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        // Set Màu cho Background
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkCyan);
                        // Căn giữa cho các text
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        // Set Font cho text  trong Range hiện tại
                        range.Style.Font.SetFromFont(new Font("Arial", 10));
                        // Set color cho text
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Font.Bold = true;
                    }

                    //lặp user
                    i += 1;
                    indexRow++;
                    worksheet.Cells[string.Format("A{0}", row)].Value = i;
                    worksheet.Cells[string.Format("B{0}", row)].Value = listItems.UserModel.FullName;
                    worksheet.Cells[string.Format("C{0}", row)].Value = listItems.UserModel.AffCode;
                    worksheet.Cells[string.Format("D{0}", row)].Value = listItems.UserModel.FullName;
                    worksheet.Cells[string.Format("E{0}", row)].Value = listItems.UserModel.CreatedDate;
                    worksheet.Cells[string.Format("F{0}", row)].Value = listItems.UserModel.AvailableBalance;
                }

                //lặp doanh thu trực tiếp
                if (listItems.ReportModels.DirectRevenue != null)
                {
                    row++;
                    var stringFormat = 4 + i;
                    var stringIndex = "A" + indexRow + ":H" + indexRow;

                    worksheet.Cells[stringIndex].Merge = true;
                    worksheet.Cells[indexRow, 1].Value = "Doanh thu trực tiếp";
                    worksheet.Cells[stringIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[indexRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[indexRow, 1].Style.Font.Size = 10;
                    indexRow++;
                    // Tạo header cho user
                    worksheet.Cells[indexRow, 1].Value = "STT";
                    worksheet.Cells[indexRow, 2].Value = "Tên tài khoản giới thiệu";
                    worksheet.Cells[indexRow, 3].Value = "Người giao dịch";
                    worksheet.Cells[indexRow, 4].Value = "Địa chỉ";
                    worksheet.Cells[indexRow, 5].Value = "Số điện thoại";
                    worksheet.Cells[indexRow, 6].Value = "Ngày giao dịch";
                    worksheet.Cells[indexRow, 7].Value = "Lợi nhuận tổng";
                    worksheet.Cells[indexRow, 8].Value = "Lợi nhuận thực thu";


                    // Lấy range vào tạo format cho range đó ở đây là từ A1 tới H1
                    stringIndex = "A" + indexRow + ":H" + indexRow;
                    using (var range = worksheet.Cells[stringIndex])
                    {
                        // Set PatternType
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        // Set Màu cho Background
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkCyan);
                        // Căn giữa cho các text
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        // Set Font cho text  trong Range hiện tại
                        range.Style.Font.SetFromFont(new Font("Arial", 10));
                        // Set color cho text
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Font.Bold = true;
                    }
                    indexRow++;
                    var a = 0;
                    //lặp user
                    foreach (var itemDirectRevenue in listItems.ReportModels.DirectRevenue)
                    {
                        a += 1;
                        worksheet.Cells[string.Format("A{0}", indexRow)].Value = a;
                        worksheet.Cells[string.Format("B{0}", indexRow)].Value = itemDirectRevenue.UserRefer.Email;
                        worksheet.Cells[string.Format("C{0}", indexRow)].Value = itemDirectRevenue.TransactionModel.FullName;
                        worksheet.Cells[string.Format("D{0}", indexRow)].Value = itemDirectRevenue.TransactionModel.Address;
                        worksheet.Cells[string.Format("E{0}", indexRow)].Value = itemDirectRevenue.TransactionModel.PhoneNumber;
                        worksheet.Cells[string.Format("F{0}", indexRow)].Value = itemDirectRevenue.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                        worksheet.Cells[string.Format("G{0}", indexRow)].Value = itemDirectRevenue.TotalAmount;
                        worksheet.Cells[string.Format("H{0}", indexRow)].Value = itemDirectRevenue.Amount;
                        row++;
                        indexRow++;
                    }

                }

                if (listItems.ReportModels.IndirectRevenue != null)
                {
                    row++;
                    var stringFormat = 4 + i;
                    var stringIndex = "A" + indexRow + ":H" + indexRow;

                    worksheet.Cells[stringIndex].Merge = true;
                    worksheet.Cells[indexRow, 1].Value = "Doanh thu gián tiếp";
                    worksheet.Cells[stringIndex].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[indexRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[indexRow, 1].Style.Font.Size = 10;
                    indexRow++;
                    // Tạo header cho user
                    worksheet.Cells[indexRow, 1].Value = "STT";
                    worksheet.Cells[indexRow, 2].Value = "Tên tài khoản giới thiệu";
                    worksheet.Cells[indexRow, 3].Value = "Người giao dịch";
                    worksheet.Cells[indexRow, 4].Value = "Địa chỉ";
                    worksheet.Cells[indexRow, 5].Value = "Số điện thoại";
                    worksheet.Cells[indexRow, 6].Value = "Ngày giao dịch";
                    worksheet.Cells[indexRow, 7].Value = "Lợi nhuận tổng";
                    worksheet.Cells[indexRow, 8].Value = "Lợi nhuận thực thu";


                    // Lấy range vào tạo format cho range đó ở đây là từ A1 tới H1
                    stringIndex = "A" + indexRow + ":H" + indexRow;
                    using (var range = worksheet.Cells[stringIndex])
                    {
                        // Set PatternType
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        // Set Màu cho Background
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkCyan);
                        // Căn giữa cho các text
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        // Set Font cho text  trong Range hiện tại
                        range.Style.Font.SetFromFont(new Font("Arial", 10));
                        // Set color cho text
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Font.Bold = true;
                    }
                    indexRow++;
                    var c = 0;
                    //lặp user
                    foreach (var itemDirectRevenue in listItems.ReportModels.IndirectRevenue)
                    {
                        c += 1;
                        worksheet.Cells[string.Format("A{0}", indexRow)].Value = c;
                        worksheet.Cells[string.Format("B{0}", indexRow)].Value = itemDirectRevenue.UserRefer.Email;
                        worksheet.Cells[string.Format("C{0}", indexRow)].Value = itemDirectRevenue.TransactionModel.FullName;
                        worksheet.Cells[string.Format("D{0}", indexRow)].Value = itemDirectRevenue.TransactionModel.Address;
                        worksheet.Cells[string.Format("E{0}", indexRow)].Value = itemDirectRevenue.TransactionModel.PhoneNumber;
                        worksheet.Cells[string.Format("F{0}", indexRow)].Value = itemDirectRevenue.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss");
                        worksheet.Cells[string.Format("G{0}", indexRow)].Value = itemDirectRevenue.TotalAmount;
                        worksheet.Cells[string.Format("H{0}", indexRow)].Value = itemDirectRevenue.Amount;
                        row++;
                        indexRow++;
                    }
                }


            }

            #endregion
        }
    }
}
