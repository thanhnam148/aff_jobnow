using Aff.DataAccess;
using Aff.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aff.Services
{
    public static class Mapper
    {
        #region Mapping User
        public static tblUser MapToEntity(this UserModel model)
        {
            DateTime dt = DateTime.Now;
            if (!string.IsNullOrEmpty(model.CreatedDate) && DateTime.TryParse(model.CreatedDate, out dt))
            {
                dt = DateTime.Now;
            }
            var user = new tblUser();
            user.UserId = model.UserId;
            user.PassWord = model.Password;
            user.UserName = model.UserName;
            user.CreatedDate = dt;
            user.Email = model.Email.Trim().ToLower();
            user.FullName = model.FullName;
            user.Phone = model.Phone;
            user.IsActive = model.IsActive;
            //user.AffCode = model.AffCode;
            user.Address = model.Address;
            user.Company = model.Company;
            user.RoleType = model.RoleType;
            user.AvailableBalance = model.AvailableBalance;
            user.UpdatedDate = model.UpdatedDate;
            user.TotalAmountEarning = model.TotalAmountEarning;
            user.EndMatchingDate = model.EndMatchingDate;
            user.EndPaymentDate = model.EndPaymentDate;
            return user;
        }

        public static PaymentModel MapToPaymentModel(this UserModel model)
        {
            string accInfo = string.Empty;
            if (!string.IsNullOrEmpty(model.BankAccount))
                accInfo = string.Format("Ngân hàng: {0} - Số TK: {1} - Tên TK: {2} - Chi nhánh: {3}.", model.BankName, model.BankAccount, model.BankOwnerName, model.BankAddress);
            PaymentModel pm = new PaymentModel
            {
                AccountInfo = accInfo,
                CreatedDate = DateTime.Now.ToString(),
                PaymentMethod = 1,
                PaymentStatus = 2,
                AmountEarning = model.AlreadyAmountTranfer,
                UpdatedDate = DateTime.Now.ToString(),
                UserId = model.UserId,
                VerifyStatus = 1,
                Email = model.Email
            };
            return pm;
        }

        public static UserModel MapToModel(this tblUser entity, DateTime? startDate = null, DateTime? endDate = null, string monthOfYear = null, bool encryptPhone = false)
        {
            var user = new UserModel();
            user.UserId = entity.UserId;
            user.Password = entity.PassWord;
            user.UserName = entity.UserName;
            user.CreatedDate = entity.CreatedDate.HasValue ? entity.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            user.Email = entity.Email;
            user.FullName = entity.FullName;
            user.Phone = entity.Phone;
            if(encryptPhone)
            {
                user.Phone = (!string.IsNullOrEmpty(entity.Phone) && entity.Phone.Length > 4) ? string.Format("{0}*****{1}", entity.Phone.Substring(0, 3), entity.Phone.Substring(entity.Phone.Length - 3, 3)) : "09*********";
            }
            user.IsActive = entity.IsActive;
            user.AffCode = entity.AffCode;
            user.Address = entity.Address;
            user.Company = entity.Company;
            user.RoleType = entity.RoleType;
            user.AvailableBalance = entity.AvailableBalance.HasValue ? entity.AvailableBalance.Value: 0;
            user.UpdatedDate = entity.UpdatedDate;
            user.TotalAmountEarning = entity.TotalAmountEarning.HasValue ? entity.TotalAmountEarning.Value : 0;
            user.EndMatchingDate = entity.EndMatchingDate;
            user.EndPaymentDate = entity.EndPaymentDate;
            if (entity.tblBankAccounts != null && entity.tblBankAccounts.Any())
            {
                var userInfor = entity.tblBankAccounts.FirstOrDefault();
                user.BankName = userInfor.BankName;
                user.BankOwnerName = userInfor.BankOwnerName;
                user.BankAccount = userInfor.BankAccount;
                user.BankAddress = userInfor.BankAddress;
            }
            
            user.StatusAmount = 0;
            long lastranferAmount = 0; 
            if (!string.IsNullOrEmpty(monthOfYear) && entity.tblUserReports != null && entity.tblUserReports.Any())
            {
                var lastUserReport = entity.tblUserReports.Where(r => r.DataOfMonth == monthOfYear.Trim() && r.TranferAmount.HasValue).OrderByDescending(o=>o.UserReportId).FirstOrDefault();
                if(lastUserReport != null)
                {
                    user.StatusAmount = 1;
                    lastranferAmount = lastUserReport.TranferAmount.Value;
                    user.AvailableBalance = lastUserReport.TotalAvailableAmount.Value - lastranferAmount;
                    user.DifferenceAmount = user.AlreadyAmountTranfer - lastranferAmount;
                    //user.AlreadyAmountTranfer = user.AlreadyAmountTranfer - totalReportPreview;
                    //user.TotalAvailableAmountTranfer = user.TotalAvailableAmountTranfer - totalReportPreview;
                }
            }
            if (startDate != null && startDate.HasValue && startDate.Value.Year > 1901 && endDate != null && endDate.HasValue)
            {
                var pendingAmountTranfer = entity.tblPayments != null ? entity.tblPayments.Where(p => p.PaymentStatus.HasValue && p.PaymentStatus == 2).Sum(t => t.AmountEarning) : 0;
                user.AlreadyAmountTranfer = entity.tblReports != null ? (entity.tblReports.Where(r => r.Amount.HasValue && r.CreatedDate > startDate.Value && r.CreatedDate < endDate.Value).Sum(t => t.Amount).Value) - pendingAmountTranfer : 0;
                if (user.AlreadyAmountTranfer > lastranferAmount) user.StatusAmount = 0;
                user.TotalAvailableAmountTranfer = user.AvailableBalance + user.AlreadyAmountTranfer;
            }
            return user;
        }
        public static List<tblUser> MapToEntities(this List<UserModel> models)
        {
            return models.Select(x => x.MapToEntity()).ToList();
        }

        public static List<UserModel> MapToModels(this List<tblUser> entities, DateTime? startDate = null, DateTime? endDate =  null, string monthOfYear = null, bool encryptPhone = false)
        {
            return entities.Select(x => x.MapToModel(startDate, endDate, monthOfYear, encryptPhone)).ToList();
        }
        #endregion

        #region Mapping Transaction
        public static tblTransaction MapToEntity(this TransactionModel model)
        {
            DateTime dt = DateTime.Now;
            if(!string.IsNullOrEmpty(model.CreatedDate) && DateTime.TryParse(model.CreatedDate, out dt))
            {
                dt = DateTime.Now;
            }
            var transaction = new tblTransaction
            {
                UserId = model.UserId,
                TransactionId = model.TransactionId,
                AffCode = model.AffCode,
                TotalAmount = model.TotalAmount,
                CreatedDate = dt,
                FullName = model.FullName,
                PercentAmount = model.PercentAmount,
                Status = model.Status,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                LenderId = model.LenderId,
                LoanId = model.LoanId
            };
            return transaction;
        }

        public static TransactionModel MapToModel(this tblTransaction entity)
        {
            var totalMoney = entity.PercentAmount * entity.TotalAmount;
            var trans = new TransactionModel()
            {
                UserId = entity.UserId.HasValue ? entity.UserId.Value : 0,
                TransactionId = entity.TransactionId,
                AffCode = entity.AffCode,
                TotalAmount = entity.TotalAmount.HasValue ? entity.TotalAmount.Value : 0,
                CreatedDate = entity.CreatedDate.HasValue ? entity.CreatedDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                FullName = entity.FullName,
                PercentAmount = entity.PercentAmount.HasValue ? entity.PercentAmount.Value : 0,
                Status = entity.Status.HasValue ? entity.Status.Value : 0,
                PhoneNumber = (!string.IsNullOrEmpty(entity.PhoneNumber) && entity.PhoneNumber.Length > 4) ? string.Format("{0}*****{1}", entity.PhoneNumber.Substring(0,3), entity.PhoneNumber.Substring(entity.PhoneNumber.Length - 3, 3)) : "090*****361",//entity.PhoneNumber,
                Address = entity.Address,
                TotalProfit = (long)totalMoney,
                UserModel = entity.tblUser != null? entity.tblUser.MapToModel():null,
                PaidAmount = entity.TotalAmount.HasValue ? (entity.TotalAmount.Value * (entity.PercentAmount.HasValue ? entity.PercentAmount.Value : 0))/100 : 0,
            };
            return trans;
        }
        public static List<tblTransaction> MapToEntities(this List<TransactionModel> models)
        {
            return models.Select(x => x.MapToEntity()).ToList();
        }

        public static List<TransactionModel> MapToModels(this List<tblTransaction> entities)
        {
            return entities.Select(x => x.MapToModel()).ToList();
        }
        #endregion

        #region Mapping Report
        public static tblReport MapToEntity(this ReportModel model)
        {
            var report = new tblReport
            {
                Amount = model.Amount,
                CreatedDate = model.CreatedDate,
                TransactionId = model.TransactionId,
                UserId = model.UserId,
                UserReferId = model.UserReferId
            };
            return report;
        }

        public static ReportModel MapToModel(this tblReport entity)
        {
            var trans = new ReportModel()
            {
                Amount = entity.Amount.HasValue ? entity.Amount.Value:0,
                CreatedDate = entity.CreatedDate.HasValue ? entity.CreatedDate.Value : DateTime.Now,
                TransactionId = entity.TransactionId,
                UserId = entity.UserId,
                TotalAmount = entity.TransactionAmount.HasValue? entity.TransactionAmount.Value:0,
                TransactionModel = entity.tblTransaction != null? entity.tblTransaction.MapToModel():null,
                UserRefer = entity.tblTransaction != null? entity.tblTransaction.tblUser!= null? entity.tblTransaction.tblUser.MapToModel() : null: null,
                UserReferId = entity.UserReferId.HasValue ? entity.UserReferId.Value : 0
            };
            return trans;
        }
        public static List<tblReport> MapToEntities(this List<ReportModel> models)
        {
            return models.Select(x => x.MapToEntity()).ToList();
        }

        public static List<ReportModel> MapToModels(this List<tblReport> entities)
        {
            return entities.Select(x => x.MapToModel()).ToList();
        }
        #endregion

        #region Mapping Loan
        public static tblLoan MapToEntity(this LoanModel model)
        {
            DateTime dt = DateTime.Now;
            if (!string.IsNullOrEmpty(model.CreatedDate) && DateTime.TryParse(model.CreatedDate, out dt))
            {
                dt = DateTime.Now;
            }
            var loan = new tblLoan
            {
                LoanId = model.LoanId,
                AffCode = model.AffCode,
                Amount = model.Amount,
                CreatedDate = dt,
                FullName = model.FullName,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                Status = model.Status
            };
            return loan;
        }

        public static LoanModel MapToModel(this tblLoan entity)
        {
            var loan = new LoanModel()
            {
                LoanId = entity.LoanId,
                AffCode = entity.AffCode,
                Amount = entity.Amount.HasValue ? entity.Amount.Value : 0,
                CreatedDate = entity.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss"),
                FullName = entity.FullName,
                Address = entity.Address,
                PhoneNumber = (!string.IsNullOrEmpty(entity.PhoneNumber) && entity.PhoneNumber.Length > 4) ? string.Format("{0}*****{1}", entity.PhoneNumber.Substring(0, 3), entity.PhoneNumber.Substring(entity.PhoneNumber.Length - 3, 3)) : "090*****361",
                Status = entity.Status.HasValue? entity.Status.Value: 0
            };
            return loan;
        }
        public static List<tblLoan> MapToEntities(this List<LoanModel> models)
        {
            return models.Select(x => x.MapToEntity()).ToList();
        }

        public static List<LoanModel> MapToModels(this List<tblLoan> entities)
        {
            return entities.Select(x => x.MapToModel()).ToList();
        }
        #endregion

        #region Mapping BankAccount
        public static tblBankAccount MapToEntity(this BankAccountModel model)
        {
            var bankAccount = new tblBankAccount
            {
                BankAccount = model.BankAccount,
                BankAddress = model.BankAddress,
                BankId = model.BankId,
                BankName = model.BankName,
                BankOwnerName = model.BankOwnerName,
                CreatedDate = model.CreatedDate,
                Note = model.Note,
                UpdatedDate = model.UpdatedDate,
                UserId = model.UserId,
            };
            return bankAccount;
        }

        public static BankAccountModel MapToModel(this tblBankAccount entity)
        {
            var bankAccount = new BankAccountModel()
            {
                BankAccount = entity.BankAccount,
                BankAddress = entity.BankAddress,
                BankId = entity.BankId,
                BankName = entity.BankName,
                BankOwnerName = entity.BankOwnerName,
                CreatedDate = entity.CreatedDate,
                Note = entity.Note,
                UpdatedDate = entity.UpdatedDate,
                UserId = entity.UserId.HasValue ? entity.UserId.Value: 0,
            };
            return bankAccount;
        }
        public static List<tblBankAccount> MapToEntities(this List<BankAccountModel> models)
        {
            return models.Select(x => x.MapToEntity()).ToList();
        }

        public static List<BankAccountModel> MapToModels(this List<tblBankAccount> entities)
        {
            return entities.Select(x => x.MapToModel()).ToList();
        }
        #endregion
        #region Mapping Payment
        public static tblPayment MapToEntity(this PaymentModel model)
        {
            DateTime dtCreate = DateTime.Now;
            DateTime dtUpdate = DateTime.Now;
            DateTime dtPaid = DateTime.Now;
            if (!string.IsNullOrEmpty(model.CreatedDate) && DateTime.TryParse(model.CreatedDate, out dtCreate))
            {
                dtCreate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(model.UpdatedDate) && DateTime.TryParse(model.UpdatedDate, out dtUpdate))
            {
                dtUpdate = DateTime.Now;
            }
            if (!string.IsNullOrEmpty(model.PaidDate) && DateTime.TryParse(model.PaidDate, out dtPaid))
            {
                dtPaid = DateTime.Now;
            }
            var payment = new tblPayment
            {
                AccountInfo = model.AccountInfo,
                Comment1 = model.Comment1,
                Comment2 = model.Comment2,
                Comment3 = model.Comment3,
                CreatedDate = dtCreate,
                PaidDate = dtPaid,
                PaymentId = model.PaymentId,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = model.PaymentStatus,
                AmountEarning = model.AmountEarning,
                UpdatedDate = dtUpdate,
                UserId = model.UserId,
                VerifyStatus = model.VerifyStatus
            };
            return payment;
        }

        public static PaymentModel MapToModel(this tblPayment entity)
        {
            var payment = new PaymentModel()
            {
                AccountInfo = entity.AccountInfo,
                Comment1 = entity.Comment1,
                Comment2 = entity.Comment2,
                Comment3 = entity.Comment3,
                CreatedDate = entity.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss"),
                PaidDate = entity.PaidDate.HasValue? entity.PaidDate.Value.ToString("dd/MM/yyyy HH:mm:ss") : "",
                PaymentId = entity.PaymentId,
                PaymentMethod = entity.PaymentMethod.HasValue? entity.PaymentMethod.Value: 0,
                PaymentStatus = entity.PaymentStatus.HasValue? entity.PaymentStatus.Value: 0,
                AmountEarning = entity.AmountEarning,
                UpdatedDate = entity.UpdatedDate.ToString("dd/MM/yyyy HH:mm:ss"),
                UserId = entity.UserId,
                VerifyStatus = entity.VerifyStatus.HasValue? entity.VerifyStatus.Value:0,
                Email = entity.tblUser != null ? entity.tblUser.Email : ""
            };
            return payment;
        }

        public static List<tblPayment> MapToEntities(this List<PaymentModel> models)
        {
            return models.Select(x => x.MapToEntity()).ToList();
        }

        public static List<PaymentModel> MapToModels(this List<tblPayment> entities)
        {
            return entities.Select(x => x.MapToModel()).ToList();
        }
        #endregion
    }
}