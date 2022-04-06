using System.Collections.Generic;
using System.Web;
using System;
using System.Web.Caching;
using System.Linq;
using Aff.DataAccess;
using Aff.Models.Models;
using Aff.DataAccess.Common;
using Aff.Models;
using Aff.DataAccess.Repositories;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using OfficeOpenXml.Table;

namespace Aff.Services
{
    public interface IUserService : IEntityService<tblUser>
    {
        UserModel RetrieveUser(int userId);
        List<UserModel> SearchUserAvaiableBalance(int status, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage);
        List<UserModel> SearchUser(string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null);
        List<UserModel> SearchUserAff(int userId, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage);
        bool UpdateMatchingAmount(List<UserModel> userMatchings, string dataOfMonth);
        bool DeleteUser(int id, out string message);
        UserCommon ValidateLogon(string email, string password, out string msgError, bool isRemoteAdmin = false);
        bool IsUserExist(string userName);
        bool IsUserEmailExit(string email);
        UserCommon GetUserByEmail(string email);
        bool ChangePassword(int userId, string passwordOld, string passwordNew, out string message);
        bool UpdateUser(UserModel userModel, out string message);
        bool CreateUser(UserModel userModel, out string message, bool isRequireAffCode = false);
        List<UserModel> GetUserMatching(string textSearch, string sortField, string sortType, string dataOfMonth, out int totalPage);
        string ExportOrderHistoryToExcel(string template, string exportDir, List<UserModel> userAvailable, out string fileName);
        string ExportToExcel_BangKeDoiSoat(string template, string exportDir, List<UserModel> userAvailable, out string fileName);
        bool ResetPassword(string email, string newPassord, out string message);
        List<UserModel> GetAllReference(int pageSize, int currentPage, string txtSearch, out int totalRecords);
        List<UserModel> GetByAffCode(int userId, int level);
        UserDetail GetDetail(int userId);
        List<UserModel> GetAllUserFirst(string searchTxt,DateTime? fromDate = null, DateTime? toDate = null);
        Stream CreateExcelUserTierFile(DateTime? fromDate, DateTime? toDate, string searchTxt, Stream stream = null);
        Stream CreateExcelFile(Stream stream = null);
        Stream CreateExcelUserFile(string textSearch,Stream stream = null);
    }

    public class UserService : EntityService<tblUser>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserReferenceRepository _userReferenceRepository;
        private readonly IReportRepository _reportRepository;
        private readonly ITransactionRepository _transactionRepository;

        private const int CacheTimeoutInHours = 2;

        public UserService(IUnitOfWork unitOfWork, IUserRepository userRepository, IUserReferenceRepository userReferenceRepository, IReportRepository reportRepository, ITransactionRepository transactionRepository)
            : base(unitOfWork, userRepository)
        {
            _userRepository = userRepository;
            _userReferenceRepository = userReferenceRepository;
            _transactionRepository = transactionRepository;
            _reportRepository = reportRepository;
        }

        public UserModel RetrieveUser(int userId)
        {
            var userEntities = _userRepository.RetrieveUser(userId);
            if (userEntities != null)
            {
                return userEntities.MapToModel();
            }
            return null;
        }

        /// <summary>
        /// Search list user
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <param name="textSearch"></param>
        /// <param name="totalPage"></param>
        /// <returns></returns>
        public List<UserModel> SearchUserAvaiableBalance(int status, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage)
        {
            var userEntities = _userRepository.SearchUserAvailableBalance(status, textSearch, currentPage, pageSize, sortField, sortType, out totalPage);
            if (userEntities != null)
            {
                return userEntities.MapToModels();
            }
            return null;
        }


        public string ExportOrderHistoryToExcel(string template, string exportDir, List<UserModel> userAvailable, out string fileName)
        {
            fileName = "";
            var filePath = "";
            //var userEntities = _userRepository.SearchUserAvailableBalance(status, textSearch, 1, 1000000, sortField, sortType, out totalPage);
            if (userAvailable == null) return filePath;
            fileName =
                $"{"BangKe"}_{"Aff"}_{DateTime.Now.ToString("dd-MM-yyyy")}.xlsx";
            var pathString = Path.Combine(exportDir, DateTime.Now.ToString("yyyyMMddmmss"));
            Directory.CreateDirectory(pathString);
            filePath = $@"{pathString}\{fileName}";
            var newFile = new FileInfo(filePath);
            var templateFile = new FileInfo(template);
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(filePath);
            }

            //using (var excelPkg = new ExcelPackage()
            using (var excelPkg = new ExcelPackage(newFile, templateFile))
            {
                var ws = excelPkg.Workbook.Worksheets[1];
                var currentRow = 1;
                var shopItem = 0;
                foreach (var item in userAvailable)
                {
                    shopItem++;
                    currentRow++;
                    ws.Cells["A" + currentRow].Value = item.UserId;
                    ws.Cells["B" + currentRow].Value = item.FullName;
                    ws.Cells["C" + currentRow].Value = item.Email;
                    ws.Cells["D" + currentRow].Value = item.Phone;
                    ws.Cells["E" + currentRow].Value = item.Address;
                    ws.Cells["F" + currentRow].Value = item.Company;
                    ws.Cells["K" + currentRow].Style.Numberformat.Format = "#,##0;(#,##0)";
                    ws.Cells["K" + currentRow].Value = item.AvailableBalance;

                    if (!string.IsNullOrEmpty(item.BankAccount))
                    {
                        ws.Cells["G" + currentRow].Value = item.BankAccount;//"Ngân hàng: " + item.BankName + " - Số TK: " + item.BankAccount + " - Tên TK: " + item.BankOwnerName + " - Chi nhánh: " + item.BankAddress + ".";
                        ws.Cells["H" + currentRow].Value = item.BankOwnerName;
                        ws.Cells["I" + currentRow].Value = item.BankName;
                        ws.Cells["J" + currentRow].Value = item.BankAddress;
                        ws.Cells["L" + currentRow].Value = string.Format("EDUMORE TT LUONG CTV T{0}/{1}", DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0'), DateTime.Now.Year.ToString());
                    }
                }

                excelPkg.Save();
            }
            return filePath;
        }

        public string ExportToExcel_BangKeDoiSoat(string template, string exportDir, List<UserModel> userAvailable, out string fileName)
        {
            fileName = "";
            var filePath = "";
            //var userEntities = _userRepository.SearchUserAvailableBalance(status, textSearch, 1, 1000000, sortField, sortType, out totalPage);
            if (userAvailable == null) return filePath;
            fileName =
                $"{"BangKeDoiSoat"}_{"Aff"}_{DateTime.Now.ToString("dd-MM-yyyy")}.xlsx";
            var pathString = Path.Combine(exportDir, DateTime.Now.ToString("yyyyMMddmmss"));
            Directory.CreateDirectory(pathString);
            filePath = $@"{pathString}\{fileName}";
            var newFile = new FileInfo(filePath);
            var templateFile = new FileInfo(template);
            if (newFile.Exists)
            {
                newFile.Delete();  // ensures we create a new workbook
                newFile = new FileInfo(filePath);
            }

            //using (var excelPkg = new ExcelPackage()
            using (var excelPkg = new ExcelPackage(newFile, templateFile))
            {
                var ws = excelPkg.Workbook.Worksheets[1];
                var currentRow = 6;
                var allTransactionInPreviewoustMonth = _reportRepository.GetAllTransactionPreviousMonth(DateTime.Now);
                foreach (var item in userAvailable)
                {
                    currentRow++;
                    int gdSuccess = allTransactionInPreviewoustMonth.Where(t => t.UserId == item.UserId).Count();
                    if (gdSuccess == 0) continue;
                    long availableBalance = item.AvailableBalance;
                    ws.Cells["A" + currentRow].Value = item.FullName;
                    ws.Cells["B" + currentRow].Value = item.Address;
                    ws.Cells["C" + currentRow].Value = gdSuccess;//DG thanh cong

                    ws.Cells["D" + currentRow].Style.Numberformat.Format = "#,##0;(#,##0)";
                    ws.Cells["D" + currentRow].Value = (long)availableBalance / gdSuccess;//item.Phone;

                    ws.Cells["E" + currentRow].Style.Numberformat.Format = "#,##0;(#,##0)";
                    ws.Cells["E" + currentRow].Value = availableBalance;
                    ws.Cells["F" + currentRow].Style.Numberformat.Format = "#,##0;(#,##0)";
                    ws.Cells["F" + currentRow].Value = availableBalance * 10 / 100;

                    ws.Cells["G" + currentRow].Style.Numberformat.Format = "#,##0;(#,##0)";
                    ws.Cells["G" + currentRow].Value = item.AvailableBalance * 90 / 100;

                    //if (!string.IsNullOrEmpty(item.BankAccount))
                    //{
                    //    ws.Cells["H" + currentRow].Value = "Ngân hàng: " + item.BankName + " - Số TK: " + item.BankAccount + " - Tên TK: " + item.BankOwnerName + " - Chi nhánh: " + item.BankAddress + ".";
                    //}
                }

                excelPkg.Save();
            }
            return filePath;
        }

        public List<UserModel> SearchUser(string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (fromDate.HasValue)
            {
                fromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 1, 1, 1);
            }
            if (toDate.HasValue)
            {
                toDate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 23, 59, 59);
            }
            var userEntities = _userRepository.SearchUser(textSearch, currentPage, pageSize, sortField, sortType, out totalPage, fromDate, toDate);
            if (userEntities != null)
            {
                return userEntities.MapToModels();
            }
            return null;
        }

        public List<UserModel> SearchUserAff(int userId, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage)
        {
            var userEntities = _userRepository.SearchUserAff(userId, textSearch, currentPage, pageSize, sortField, sortType, out totalPage);
            if (userEntities != null)
            {
                return userEntities.MapToModels(encryptPhone: true);
            }
            return null;
        }

        public List<UserModel> GetUserMatching(string textSearch, string sortField, string sortType, string dataOfMonth, out int totalPage)
        {
            var fromDate = GetFirstDayOfMonth(dataOfMonth);
            var toDate = GetLastDayOfMonth(dataOfMonth);
            var userEntities = _userRepository.GetUserMatching(textSearch, sortField, sortType, fromDate, toDate, out totalPage);
            if (userEntities != null)
            {
                return userEntities.MapToModels(fromDate, toDate, dataOfMonth);
            }
            return null;
        }

        public bool UpdateMatchingAmount(List<UserModel> userMatchings, string dataOfMonth)
        {
            var fromDate = GetFirstDayOfMonth(dataOfMonth);
            var toDate = GetLastDayOfMonth(dataOfMonth);
            return _userRepository.UpdateMatchingAmount(userMatchings, dataOfMonth, fromDate, toDate);
        }

        public bool DeleteUser(int id, out string message)
        {
            var user = _userRepository.GetById(id);
            if (user != null)
            {
                // Delete table User
                _userRepository.Delete(id);
                UnitOfWork.SaveChanges();

                message = "Xóa tài khoản thành công";
                return true;
            }

            message = "Xóa tài khoản thất bại";
            return false;
        }

        public UserCommon ValidateLogon(string email, string password, out string msgError, bool isRemoteAdmin = false)
        {
            msgError = string.Empty;
            var result = new UserCommon();
            if (!isRemoteAdmin)
                password = Security.SecurityUtil.EncryptText(password);
            var user = _userRepository.GetAll().Where(c => c.Email.Equals(email.Trim().ToLower()) && c.PassWord == password).FirstOrDefault();
            if (user == null)
            {
                msgError = "Thông tin Email hoặc mật khẩu không hợp lệ.";
                result.Status = LoginResult.InvalidEmail;
                return result;
            }

            if (user != null)
            {
                if (!user.IsActive)
                {
                    msgError = "Tài khoản của bạn chưa được kích hoạt, xin vui lòng liên hệ với người quản trị.";
                    result.Status = LoginResult.IsLockedOut;
                    return result;
                }

                result.Status = LoginResult.Success;
                result.Email = user.Email;
                result.AffCode = user.AffCode;
                result.UserId = user.UserId;
                result.UserName = user.UserName;
                result.RoleType = user.RoleType;
                result.Phone = result.Phone;
                result.FullName = user.FullName;
                result.Avatar = user.Avatar;
                result.Company = user.Company;

                result.Address = user.Address;
                result.AvailableBalance = user.AvailableBalance.HasValue ? user.AvailableBalance.Value : 0;
                result.TotalAmountEarning = user.TotalAmountEarning.HasValue ? user.TotalAmountEarning.Value : 0;

                //Store in cache,NoSlidingExpiration : timeout
                HttpRuntime.Cache.Insert(user.UserId.ToString(), user.RoleType, null, DateTime.Now.AddHours(CacheTimeoutInHours),
                                         Cache.NoSlidingExpiration);
                return result;
            }

            msgError = "Thông tin tài khoản không hợp lệ.";
            result.Status = LoginResult.Unknown;
            return result;
        }

        public bool IsUserExist(string userName)
        {
            var user = _userRepository.Query(x => x.UserName == userName).Any();
            return user;
        }

        public bool IsUserEmailExit(string email)
        {
            var user = _userRepository.Query(x => x.Email == email.Trim().ToLower()).Any();
            return user;
        }

        public tblUser CheckUserAff(string affCode)
        {
            if (string.IsNullOrEmpty(affCode)) return null;
            var user = _userRepository.Query(x => x.AffCode == affCode).FirstOrDefault();
            return user;
        }

        public UserCommon GetUserByEmail(string email)
        {
            var result = new UserCommon();
            var user = GetAll().AsQueryable()
                .FirstOrDefault(c => c.Email.ToLower().Equals(email.ToLower()) && (c.IsActive));

            if (user != null)
            {
                result.IsActive = user.IsActive;
                result.RoleType = user.RoleType;
                result.UserId = user.UserId;
                result.Avatar = user.Avatar;
                result.Email = user.Email;
                result.AffCode = user.AffCode;
                result.UserName = user.UserName;
                result.FullName = user.FullName;
                result.Address = user.Address;
                result.Company = user.Company;
                result.Phone = user.Phone;
                result.AvailableBalance = user.AvailableBalance.HasValue ? user.AvailableBalance.Value : 0;
                result.TotalAmountEarning = user.TotalAmountEarning.HasValue ? user.TotalAmountEarning.Value : 0;
            }

            return result;
        }

        public bool ChangePassword(int userId, string passwordOld, string passwordNew, out string message)
        {
            passwordOld = Security.SecurityUtil.EncryptText(passwordOld);
            passwordNew = Security.SecurityUtil.EncryptText(passwordNew);
            var user = _userRepository.Find(c => c.UserId == userId && c.PassWord == passwordOld);
            if (user == null)
            {
                message = "Mật khẩu cũ không chính xác.";
                return false;
            }
            user.PassWord = passwordNew;
            _userRepository.Update(user);

            UnitOfWork.SaveChanges();

            message = "Thay đổi mật khẩu thành công.";
            return true;
        }

        public bool ResetPassword(string email, string newPassord, out string message)
        {
            var user = _userRepository.Find(c => c.Email == email);
            if (user == null)
            {
                message = "Tài khoản không tồn tại, xin vui lòng nhập lại.";
                return false;
            }
            var encryptedNewPassord = Security.SecurityUtil.EncryptText(newPassord);
            user.PassWord = encryptedNewPassord;
            _userRepository.Update(user);

            UnitOfWork.SaveChanges();

            message = "Reset mật khẩu thành công. Xin vui lòng kiểm tra email để cập nhật.";
            return true;
        }

        public bool UpdateUser(UserModel userModel, out string message)
        {
            var userEntity = _userRepository.GetById(userModel.UserId);
            if (userEntity != null)
            {
                //if (!string.IsNullOrEmpty(userModel.Email))
                //{
                //    userEntity.Email = userModel.Email;
                //}
                if (!string.IsNullOrEmpty(userModel.UserName))
                {
                    userEntity.UserName = userModel.UserName;
                }
                //if (!string.IsNullOrEmpty(userModel.Password) && isUpdatePassword)
                //{
                //    userEntity.PassWord = Security.SecurityUtil.EncryptText(userModel.Password);
                //}
                if (!string.IsNullOrEmpty(userModel.FullName))
                {
                    userEntity.FullName = userModel.FullName;
                }
                if (!string.IsNullOrEmpty(userModel.Address))
                {
                    userEntity.Address = userModel.Address;
                }
                if (!string.IsNullOrEmpty(userModel.Phone))
                {
                    userEntity.Phone = userModel.Phone;
                }
                if (!string.IsNullOrEmpty(userModel.Company))
                {
                    userEntity.Company = userModel.Company;
                }
                if (userModel.AvailableBalance >= 0)
                {
                    userEntity.AvailableBalance = userModel.AvailableBalance;
                }
                if (userModel.TotalAmountEarning >= 0)
                {
                    userEntity.TotalAmountEarning = userModel.TotalAmountEarning;
                }
                if (userModel.EndPaymentDate.HasValue)
                {
                    userEntity.EndPaymentDate = userModel.EndPaymentDate.Value;
                }
                if (userModel.EndMatchingDate.HasValue)
                {
                    userEntity.EndMatchingDate = userModel.EndMatchingDate.Value;
                }
                _userRepository.Update(userEntity);
                UnitOfWork.SaveChanges();

                message = "Cập nhật thành công";
                return true;
            }
            message = "Cập nhật tài khoản thất bại.";
            return false;
        }

        public bool CreateUser(UserModel userModel, out string message, bool isRequireAffCode = false)
        {

            if (!IsUserEmailExit(userModel.Email.Trim().ToLower()))
            {
                var userReffer = CheckUserAff(userModel.AffCode);
                if (userReffer == null && isRequireAffCode)
                {
                    message = "Mã giới thiệu không chính xác.";
                    return false;
                }
                // Add table User
                userModel.Password = Security.SecurityUtil.EncryptText(userModel.Password);
                userModel.CreatedDate = DateTime.Now.ToString();
                userModel.IsActive = true;
                userModel.RoleType = 3;
                if (string.IsNullOrEmpty(userModel.UserName)) userModel.UserName = userModel.Email.Substring(0, userModel.Email.IndexOf('@'));
                var userEntity = _userRepository.Insert(userModel.MapToEntity());
                UnitOfWork.SaveChanges();
                userEntity.AffCode = AffCodeIdentifier.GenerateIdentifier(userEntity.UserId);
                UnitOfWork.SaveChanges();
                // Return
                message = "Thêm tài khoản thành công";

                //create user refference
                if (userReffer != null)
                {
                    userModel.UserId = userEntity.UserId;
                    userModel.AffCode = userEntity.AffCode;
                    CreateUserReffers(userModel, userReffer);
                }

                return true;
            }
            message = "Email đã tồn tại";
            return false;
        }

        private bool CreateUserReffers(UserModel userModel, tblUser userReffer)
        {
            try
            {
                //Update owner reffer code
                var userR1 = new tblUserReference
                {
                    UserId = userModel.UserId,
                    UserReferenceId = userReffer.UserId,
                    AffReferenceCode = userReffer.AffCode,
                    UserCode = userModel.AffCode,
                    Level = 1
                };
                _userReferenceRepository.Insert(userR1);
                UnitOfWork.SaveChanges();
                var userRefParents = _userReferenceRepository.FindAll(u => u.UserId == userReffer.UserId).OrderBy(o => o.Level.Value).Take(3);
                foreach (var uf in userRefParents)
                {
                    var userR2 = new tblUserReference
                    {
                        UserId = userModel.UserId,
                        UserReferenceId = uf.UserReferenceId,
                        AffReferenceCode = uf.AffReferenceCode,
                        UserCode = userModel.AffCode,
                        Level = uf.Level.Value + 1
                    };
                    _userReferenceRepository.Insert(userR2);
                    UnitOfWork.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Check user is updated
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        private bool IsUpdatedUser(UserModel userModel)
        {
            var userEntity = _userRepository.GetById(userModel.UserId);
            if (userEntity.Email != userModel.Email.Trim().ToLower())
                return true;
            if (userEntity.UserName != userModel.UserName)
                return true;
            if (userEntity.PassWord != userModel.Password)
                return true;
            return false;
        }

        private DateTime GetLastDayOfMonth(string dataOfMonth)
        {
            try
            {
                int year, month;
                char splitter = ' ';
                if (dataOfMonth.IndexOf('-') > 0) splitter = '-';
                if (dataOfMonth.IndexOf('/') > 0) splitter = '/';
                var values = dataOfMonth.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                int.TryParse(values[0], out month);
                int.TryParse(values[1], out year);

                return new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            }
            catch
            {
                return new DateTime(1900, 1, 1);
            }
        }
        private DateTime GetFirstDayOfMonth(string dataOfMonth)
        {
            try
            {
                int year, month;
                char splitter = ' ';
                if (dataOfMonth.IndexOf('-') > 0) splitter = '-';
                if (dataOfMonth.IndexOf('/') > 0) splitter = '/';
                var values = dataOfMonth.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries);
                int.TryParse(values[0].Trim(), out month);
                int.TryParse(values[1].Trim(), out year);

                return new DateTime(year, month, 1, 1, 1, 1);
            }
            catch
            {
                return new DateTime(1900, 1, 1);
            }

        }

        public List<UserModel> GetAllReference(int pageSize, int currentPage, string txtSearch, out int totalRecords)
        {
            try
            {
                totalRecords = 0;
                var entitiesUser = _userRepository.Search(currentPage, pageSize, txtSearch, out totalRecords);
                if (entitiesUser != null)
                {
                    var usersReturn = new List<UserModel>();
                    var userReferences = _userReferenceRepository.GetByListUserId(entitiesUser.Select(c => c.UserId).ToList());
                    if (userReferences != null)
                    {
                        foreach (var item in entitiesUser)
                        {
                            var reportEntities = _reportRepository.GetByUserId(item.UserId);
                            if (!userReferences.Any(c => c.UserId == item.UserId))
                            {
                                var itemUser = new UserModel();
                                itemUser = item.MapToModel();
                                if (reportEntities != null)
                                {
                                    var totalBalanceDirect = reportEntities.Where(t => !t.UserReferId.HasValue && t.Amount.HasValue);
                                    var totalBalanceBonus = reportEntities.Where(t => t.UserReferId.HasValue && t.Amount.HasValue);

                                    itemUser.TotalBalanceDirect = totalBalanceDirect != null && totalBalanceDirect.Any() ? totalBalanceDirect.Sum(r => r.Amount.Value) : 0;
                                    itemUser.TotalBalanceBonus = totalBalanceBonus != null && totalBalanceBonus.Any() ? totalBalanceBonus.Sum(r => r.Amount.Value) : 0;
                                    itemUser.TotalAmountEarning = item.TotalAmountEarning.HasValue ? item.TotalAmountEarning.Value : 0;
                                    itemUser.AvailableBalance = item.AvailableBalance.HasValue ? item.AvailableBalance.Value : 0;

                                    if (item.tblTransactions != null)
                                    {
                                        itemUser.UserTransactions = item.tblTransactions.ToList().MapToModels();
                                    }
                                }


                                itemUser.IsChild = false;
                                if (userReferences.Any(c => c.UserReferenceId == item.UserId))
                                {
                                    itemUser.IsChild = true;
                                }
                                var transactionEntities = _transactionRepository.GetAllTranSaction(item.UserId);
                                itemUser.TotalMoney = 0;
                                if (transactionEntities != null)
                                {
                                    itemUser.TotalMoney = (long)transactionEntities.Sum(c => c.TotalAmount);
                                }
                                usersReturn.Add(itemUser);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in entitiesUser)
                        {
                            usersReturn.Add(item.MapToModel());
                        }
                    }
                    return usersReturn;
                }
            }
            catch (Exception ex)
            {
                totalRecords = 0;
                return null;
            }
            return null;
        }

        public List<UserModel> GetByAffCode(int userId, int level)
        {
            try
            {
                var entities = _userReferenceRepository.GetByAffCode(userId, level);
                if (entities != null)
                {
                    var listReturn = new List<UserModel>();
                    foreach (var item in entities)
                    {
                        var itemModel = new UserModel();
                        itemModel = item.tblUser.MapToModel();
                        var reportEntities = _reportRepository.GetByUserForLoanId(item.UserId);
                        if (reportEntities != null)
                        {
                            var totalBalanceDirect = reportEntities.Where(t => !t.UserReferId.HasValue && t.Amount.HasValue);
                            var totalBalanceBonus = reportEntities.Where(t => t.UserReferId.HasValue && t.Amount.HasValue);

                            itemModel.TotalBalanceDirect = totalBalanceDirect != null && totalBalanceDirect.Any() ? totalBalanceDirect.Sum(r => r.Amount.Value) : 0;
                            itemModel.TotalBalanceBonus = totalBalanceBonus != null && totalBalanceBonus.Any() ? totalBalanceBonus.Sum(r => r.Amount.Value) : 0;
                            itemModel.TotalAmountEarning = item.tblUser.TotalAmountEarning.HasValue ? item.tblUser.TotalAmountEarning.Value : 0;
                            itemModel.AvailableBalance = item.tblUser.AvailableBalance.HasValue ? item.tblUser.AvailableBalance.Value : 0;
                        }

                        itemModel.IsChild = false;
                        itemModel.TotalMoney = 0;
                        var transactionEntities = _transactionRepository.GetAllTranSaction(item.UserId);
                        if (transactionEntities != null)
                        {
                            itemModel.TotalMoney = (long)transactionEntities.Sum(c => c.TotalAmount);
                        }
                        var userReference = _userReferenceRepository.GetByUserId(item.UserId);
                        var levelNext = level + 1;
                        if (userReference != null)
                        {
                            itemModel.IsChild = true;
                        }
                        if (item.Level.HasValue)
                        {
                            itemModel.Level = item.Level.Value;
                        }
                        listReturn.Add(itemModel);
                    }
                    return listReturn;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }


        #region Export Exel User

        private List<UserModel> GetUserForExcels(string textSearch)
        {
            var timeSheetDailys = _userRepository.SearchAll(textSearch);

            List<UserModel> userModelList = new List<UserModel>();

            foreach (var itemGroup in timeSheetDailys.Where(c=>!c.tblUserReferences.Any()))
            {
                var transactions = new UserModel();
                transactions.UserId = itemGroup.UserId;
                transactions.FullName = itemGroup.FullName;
                transactions.Phone = itemGroup.Phone;
                transactions.CreatedDate = itemGroup.CreatedDate.Value.ToString("dd/MM/yyyy");
                transactions.AffCode = itemGroup.AffCode;
                transactions.Address = itemGroup.Address;
                transactions.LevelName = "F0";
                userModelList.Add(transactions);
            }

            foreach (var itemGroup in timeSheetDailys.Where(c=>c.tblUserReferences.Any()).OrderBy(c=>c.tblUserReferences.FirstOrDefault().Level))
            {
                var transactions = new UserModel();
                transactions.UserId = itemGroup.UserId;
                transactions.FullName = itemGroup.FullName;
                transactions.Phone = itemGroup.Phone;
                transactions.CreatedDate = itemGroup.CreatedDate.Value.ToString("dd/MM/yyyy");
                transactions.AffCode = itemGroup.AffCode;
                transactions.Address = itemGroup.Address;
                transactions.LevelName = itemGroup.tblUserReferences.Count() > 0? "F" + itemGroup.tblUserReferences.FirstOrDefault().Level:"F0";
                userModelList.Add(transactions);
            }
            return userModelList;
        }

        public Stream CreateExcelUserFile(string textSearch,Stream stream = null)
        {
            var timeSheetDailys = GetUserForExcels(textSearch);
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
                BindingFormatUserForExcel(workSheet, timeSheetDailys);
                excelPackage.Save();
                return excelPackage.Stream;
            }
        }
        private void BindingFormatUserForExcel(ExcelWorksheet worksheet, List<UserModel> listItems)
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
            worksheet.Cells[2, 1].Value = "BÁO CÁO TÀI KHOẢN";
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.Font.Size = 20;
            worksheet.Row(2).Height = 25;
            worksheet.Cells[2, 5].Value = " ";


            // Tạo header
            worksheet.Cells[4, 1].Value = "STT";
            worksheet.Cells[4, 2].Value = "ID";
            worksheet.Cells[4, 3].Value = "Họ tên";
            worksheet.Cells[4, 4].Value = "Mã Aff";
            worksheet.Cells[4, 5].Value = "Ngày tạo";
            worksheet.Cells[4, 6].Value = "Địa chỉ";
            worksheet.Cells[4, 7].Value = "Level";
            worksheet.Cells[4, 8].Value = "Điện thoại";


            // Lấy range vào tạo format cho range đó ở đây là từ A1 tới H1
            using (var range = worksheet.Cells["A4:G4"])
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
            int row = 5;
            int i = 0;
            foreach (var item in listItems)
            {
                i += 1;
                worksheet.Cells[string.Format("A{0}", row)].Value = i;
                worksheet.Cells[string.Format("B{0}", row)].Value = item.UserId;
                worksheet.Cells[string.Format("C{0}", row)].Value = item.FullName;
                worksheet.Cells[string.Format("D{0}", row)].Value = item.AffCode;
                worksheet.Cells[string.Format("E{0}", row)].Value = item.CreatedDate;
                worksheet.Cells[string.Format("F{0}", row)].Value = item.Address;
                worksheet.Cells[string.Format("G{0}", row)].Value = item.LevelName;
                worksheet.Cells[string.Format("H{0}", row)].Value = item.Phone;
                row++;
            }
        }

        #endregion


        #region Export Exel Transaction

        private List<TransactionModel> GetTimeSheetDailyForExcels()
        {
            var timeSheetDailys = _transactionRepository.GetAllTranSactions();
            var groupUser = timeSheetDailys.GroupBy(c => c.UserId);

            List<TransactionModel> userModelList = new List<TransactionModel>();
            foreach (var itemGroup in groupUser)
            {
                foreach (var itemClass in itemGroup)
                {
                    var transactions = new TransactionModel();
                    transactions.UserId = itemClass.UserId != null ? itemClass.UserId.Value : 0;
                    transactions.FullName = itemClass.tblUser != null ? itemClass.tblUser.FullName : "";
                    transactions.CreatedDate = itemClass.CreatedDate.Value.ToString("dd/MM/yyyy");
                    transactions.Address = itemClass.Address;
                    transactions.AffCode = itemClass.AffCode;
                    transactions.PaidAmount = (long)(itemClass.TotalAmount * itemClass.PercentAmount);
                    transactions.TotalAmount = itemClass.TotalAmount.HasValue ? itemClass.TotalAmount.Value : 0;
                    transactions.PercentAmount = itemClass.PercentAmount.HasValue ? itemClass.PercentAmount.Value : 0;
                    userModelList.Add(transactions);
                }
            }
            return userModelList;
        }

        public Stream CreateExcelFile(Stream stream = null)
        {
            var timeSheetDailys = GetTimeSheetDailyForExcels();
            ExcelPackage.LicenseContext = LicenseContext.Commercial;
            using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
            {

                // Tạo author cho file Excel
                excelPackage.Workbook.Properties.Author = "LMS";
                // Tạo title cho file Excel

                excelPackage.Workbook.Properties.Title = "Báo cáo điểm danh";
                // Tạo comment cho file Exel 
                excelPackage.Workbook.Properties.Comments = "Xuất file exel";
                // Add Sheet vào file Excel
                excelPackage.Workbook.Worksheets.Add("First Sheet");
                // Lấy Sheet bạn vừa mới tạo ra để thao tác 
                var workSheet = excelPackage.Workbook.Worksheets[0];
                // Đổ data vào Excel file
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
            string tenCty = "Bảng cáo báo";

            //Tên công ty
            worksheet.Cells["A1:H1"].Merge = true;
            worksheet.Cells[1, 1].Value = tenCty;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells["A1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 5].Value = " ";

            //Tiêu đề
            worksheet.Cells["A2:H2"].Merge = true;
            worksheet.Cells[2, 1].Value = "BÁO CÁO ĐIỂM DANH";
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.Font.Size = 20;
            worksheet.Row(2).Height = 25;
            worksheet.Cells[2, 5].Value = " ";


            // Tạo header
            worksheet.Cells[4, 1].Value = "STT";
            worksheet.Cells[4, 2].Value = "Họ tên";
            worksheet.Cells[4, 3].Value = "Mã Aff";
            worksheet.Cells[4, 4].Value = "Ngày giao dịch";
            worksheet.Cells[4, 5].Value = "Phần trăm";
            worksheet.Cells[4, 6].Value = "Địa chỉ";
            worksheet.Cells[4, 7].Value = "Lợi nhuận";
            worksheet.Cells[4, 8].Value = "Tiền thực nhận";


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
                range.Style.Font.SetFromFont(new Font("Arial", 10));
                // Set color cho text
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.Font.Bold = true;
            }
            int row = 5;
            int i = 0;
            foreach (var item in listItems)
            {
                i += 1;
                worksheet.Cells[string.Format("A{0}", row)].Value = i;
                worksheet.Cells[string.Format("B{0}", row)].Value = item.FullName;
                worksheet.Cells[string.Format("C{0}", row)].Value = item.AffCode;
                worksheet.Cells[string.Format("D{0}", row)].Value = item.CreatedDate;
                worksheet.Cells[string.Format("E{0}", row)].Value = item.PercentAmount;
                worksheet.Cells[string.Format("F{0}", row)].Value = item.Address;
                worksheet.Cells[string.Format("G{0}", row)].Value = item.PaidAmount;
                worksheet.Cells[string.Format("H{0}", row)].Value = item.TotalAmount;
                row++;
            }
        }

        public List<UserModel> GetAllUserFirst(string searchTxt,DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var entities = _userRepository.GetAlls(searchTxt,fromDate, toDate);
                if (entities != null)
                {
                    var listReturn = new List<UserModel>();
                    foreach (var item in entities)
                    {
                        var itemModel = new UserModel();
                        itemModel.LevelString = "F0";
                        itemModel = item.MapToModel();

                        var userReference = _userReferenceRepository.GetByUser(item.UserId);
                        if (userReference != null && userReference.FirstOrDefault() != null)
                        {
                            itemModel.LevelString = "F" + userReference.FirstOrDefault().Level;
                        }

                        var reportEntities = _reportRepository.GetByUserId(item.UserId);
                        if (reportEntities != null)
                        {
                            var totalBalanceDirect = reportEntities.Where(t => !t.UserReferId.HasValue && t.Amount.HasValue);
                            var totalBalanceBonus = reportEntities.Where(t => t.UserReferId.HasValue && t.Amount.HasValue);

                            itemModel.TotalBalanceDirect = totalBalanceDirect != null && totalBalanceDirect.Any() ? totalBalanceDirect.Sum(r => r.Amount.Value) : 0;
                            itemModel.TotalBalanceBonus = totalBalanceBonus != null && totalBalanceBonus.Any() ? totalBalanceBonus.Sum(r => r.Amount.Value) : 0;
                            itemModel.TotalAmountEarning = item.TotalAmountEarning.HasValue ? item.TotalAmountEarning.Value : 0;
                            itemModel.AvailableBalance = itemModel.TotalBalanceDirect + itemModel.TotalBalanceBonus - itemModel.PendingAmountEarning;
                            itemModel.BalanceAfterFee = Math.Round((itemModel.AvailableBalance - itemModel.PendingAmountEarning) * 0.9, 3);
                            listReturn.Add(itemModel);
                        }
                    }
                    return listReturn;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }



        public UserDetail GetDetail(int userId)
        {
            try
            {
                //get level
                var entity = _userReferenceRepository.GetByUserFirst(userId);
                var listReturn = new UserDetail();
                var levelCurrent = 0;
                var userReferenceId = userId;
                if (entity != null)
                {
                    levelCurrent = entity.Level.Value;
                    userReferenceId = entity.UserReferenceId;
                }
                var entityReport = _reportRepository.GetByUserId(userReferenceId);
                listReturn.Level = levelCurrent;
                if (entityReport != null)
                {
                    //get data buyer index
                    var listUserModelFirst = new List<UserModel>();
                    foreach (var item in entityReport.Where(c => !c.Level.HasValue))
                    {
                        var itemUser = new UserModel();
                        itemUser.Address = item.tblTransaction.Address;
                        itemUser.FullName = item.tblTransaction.FullName;
                        itemUser.AffCode = item.tblTransaction.AffCode;
                        itemUser.TotalBalanceBonus = item.Amount.HasValue ? item.Amount.Value : 0;
                        itemUser.TotalMoney = item.TransactionAmount.HasValue ? item.TransactionAmount.Value : 0;
                        itemUser.ProfitPercentage = item.PercentAmount.HasValue ? item.PercentAmount.Value : 0;
                        itemUser.Phone = item.tblTransaction.PhoneNumber;
                        itemUser.CreatedDate = item.CreatedDate.Value.ToString("dd/MM/yyyy");
                        listUserModelFirst.Add(itemUser);
                    }
                    listReturn.UserModel = listUserModelFirst;

                    //get data another index
                    var groupUserList = new List<GroupUser>();
                    foreach (var item in entityReport.Where(c => c.Level.HasValue).GroupBy(g => g.Level))
                    {
                        var groupItem = new GroupUser();
                        groupItem.Level = item.Key.Value;

                        //get data
                        var listUserModelSecound = new List<UserModel>();
                        foreach (var itemDetail in item)
                        {
                            var itemUserSecound = new UserModel();
                            itemUserSecound.Address = itemDetail.tblTransaction.Address;
                            itemUserSecound.FullName = itemDetail.tblTransaction.FullName;
                            itemUserSecound.AffCode = itemDetail.tblTransaction.AffCode;
                            itemUserSecound.TotalBalanceBonus = itemDetail.Amount.HasValue ? itemDetail.Amount.Value : 0;
                            itemUserSecound.TotalMoney = itemDetail.TransactionAmount.HasValue ? itemDetail.TransactionAmount.Value : 0;
                            itemUserSecound.ProfitPercentage = itemDetail.PercentAmount.HasValue ? itemDetail.PercentAmount.Value : 0;
                            itemUserSecound.Phone = itemDetail.tblTransaction.PhoneNumber;
                            itemUserSecound.CreatedDate = itemDetail.CreatedDate.Value.ToString("dd/MM/yyyy");
                            listUserModelSecound.Add(itemUserSecound);
                        }
                        groupItem.UserItems = listUserModelSecound;
                        groupUserList.Add(groupItem);
                    }
                    listReturn.GroupUsers = groupUserList;
                }
                return listReturn;
            }
            catch (Exception ex)
            {
                return null;
            }
            return null;
        }

        #endregion

        #region Export Exel UserTier

        private List<UserModel> GetUserTierForExcels(DateTime? fromDate, DateTime? toDate,string searchTxt)
        {
            return GetAllUserFirst(searchTxt,fromDate: fromDate,toDate: toDate);
        }

        public Stream CreateExcelUserTierFile(DateTime? fromDate,DateTime? toDate,string searchTxt,Stream stream = null)
        {
            var timeSheetDailys = GetUserTierForExcels(fromDate, toDate, searchTxt);
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
                BindingFormatUserTierForExcel(workSheet, timeSheetDailys);
                excelPackage.Save();
                return excelPackage.Stream;
            }
        }
        private void BindingFormatUserTierForExcel(ExcelWorksheet worksheet, List<UserModel> listItems)
        {
            // Set default width cho tất cả column
            worksheet.DefaultColWidth = 20;
            // Set width cho cột 
            worksheet.Column(1).Width = 15;
            worksheet.Column(9).Width = 30;
            worksheet.Column(10).Width = 30;
            // Set height
            worksheet.Row(1).Height = 24;
            worksheet.DefaultRowHeight = 20;
            Format(worksheet);
            string tenCty = "Bảng cáo báo";

            //Tên công ty
            worksheet.Cells["A1:L1"].Merge = true;
            worksheet.Cells[1, 1].Value = tenCty;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells["A1:L1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[1, 5].Value = " ";

            //Tiêu đề
            worksheet.Cells["A2:L2"].Merge = true;
            worksheet.Cells[2, 1].Value = "BÁO CÁO TÀI KHOẢN DOANH THU";
            worksheet.Cells[2, 1].Style.Font.Bold = true;
            worksheet.Cells[2, 1].Style.Font.Size = 20;
            worksheet.Row(2).Height = 25;
            worksheet.Cells[2, 5].Value = " ";


            // Tạo header
            worksheet.Cells[4, 1].Value = "STT";
            worksheet.Cells[4, 2].Value = "ID";
            worksheet.Cells[4, 3].Value = "Email";
            worksheet.Cells[4, 4].Value = "Tên thành viên";
            worksheet.Cells[4, 5].Value = "Số ĐT";
            worksheet.Cells[4, 6].Value = "Địa chỉ";
            worksheet.Cells[4, 7].Value = "Cấp độ";
            worksheet.Cells[4, 8].Value = "Ngày tạo";
            worksheet.Cells[4, 9].Value = "Thu nhập từ liên kết trực tiếp";
            worksheet.Cells[4, 10].Value = "Thu nhập từ mã giới thiệu";
            worksheet.Cells[4, 11].Value = "Số dư hiện có";
            worksheet.Cells[4, 12].Value = "Số dư sau ví";


            // Lấy range vào tạo format cho range đó ở đây là từ A1 tới H1
            using (var range = worksheet.Cells["A4:L4"])
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
            int row = 5;
            int i = 0;
            foreach (var item in listItems)
            {
                i += 1;
                worksheet.Cells[string.Format("A{0}", row)].Value = i;
                worksheet.Cells[string.Format("B{0}", row)].Value = item.UserId;
                worksheet.Cells[string.Format("C{0}", row)].Value = item.Email;
                worksheet.Cells[string.Format("D{0}", row)].Value = item.FullName;
                worksheet.Cells[string.Format("E{0}", row)].Value = item.Phone;
                worksheet.Cells[string.Format("F{0}", row)].Value = item.Address;
                worksheet.Cells[string.Format("G{0}", row)].Value = item.LevelString;
                worksheet.Cells[string.Format("H{0}", row)].Value = item.CreatedDate;
                worksheet.Cells[string.Format("I{0}", row)].Value = item.TotalBalanceDirect;
                worksheet.Cells[string.Format("J{0}", row)].Value = item.TotalBalanceBonus;
                worksheet.Cells[string.Format("K{0}", row)].Value = item.AvailableBalance;
                worksheet.Cells[string.Format("L{0}", row)].Value = item.BalanceAfterFee;
                row++;
            }

        }

        #endregion

    

    }
}
