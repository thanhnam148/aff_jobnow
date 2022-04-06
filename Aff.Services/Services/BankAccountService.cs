using Aff.DataAccess;
using Aff.DataAccess.Common;
using Aff.DataAccess.Repositories;
using Aff.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Services
{
    public interface IBankAccountService : IEntityService<tblBankAccount>
    {
        List<BankAccountModel> RetrieveBankAccountsByUser(int userId);
        List<BankAccountModel> SearchBankAccounts(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, out int totalPage);
        bool UpdateBankAccount(UserModel userModel, out string message);
    }

    public class BankAccountService : EntityService<tblBankAccount>, IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;
        public BankAccountService(IUnitOfWork unitOfWork, IBankAccountRepository bankAccountRepository)
            : base(unitOfWork, bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
        }

        public List<BankAccountModel> RetrieveBankAccountsByUser(int userId)
        {
            var bankAccounts = GetAll().Where(t => t.UserId == userId);
            if (bankAccounts != null && bankAccounts.Any())
                return bankAccounts.ToList().MapToModels();
            return new List<BankAccountModel>();
        }

        public List<BankAccountModel> SearchBankAccounts(string textSearch, int currentPage, int pageSize, int  userId, string sortField, string sortType, out int totalPage)
        {
            var transactionEntities = _bankAccountRepository.SearchBankAccountByUser(textSearch, currentPage, pageSize, userId, sortField, sortType, out totalPage);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModels();
            }
            return null;
        }

        public bool UpdateBankAccount(UserModel userModel, out string message)
        {
            var baEntity = _bankAccountRepository.Find(ba=>ba.UserId == userModel.UserId);
            if (baEntity != null)
            {
                baEntity.BankAccount = userModel.BankAccount;
                baEntity.BankAddress = userModel.BankAddress;
                baEntity.BankName = userModel.BankName;
                baEntity.BankOwnerName = userModel.BankOwnerName;
                baEntity.UpdatedDate = DateTime.Now;
                _bankAccountRepository.Update(baEntity);
                UnitOfWork.SaveChanges();
                message = "Cập nhật thành công";
                return true;
            }
            else
            {
                var bAccount = new tblBankAccount
                {
                    BankAccount = userModel.BankAccount,
                    BankAddress = userModel.BankAddress,
                    BankName = userModel.BankName,
                    BankOwnerName = userModel.BankOwnerName,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UserId = userModel.UserId
                };
                _bankAccountRepository.Insert(bAccount);
                UnitOfWork.SaveChanges();
                message = "Cập nhật thành công";
                return true;
            }
            message = "Cập nhật tài khoản thất bại.";
            return false;
        }
    }
}
