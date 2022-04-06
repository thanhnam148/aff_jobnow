using Aff.DataAccess.Common;
using Aff.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.DataAccess.Repositories
{
    public interface ILoanRepository : IBaseRepository<tblLoan>
    {
        tblLoan RetrieveLoan(int loanId);
        List<tblLoan> SearchLoansByUser(string textSearch, int currentPage, int pageSize, string affCode, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null);
        List<tblLoan> SearchLoansByAdmin(string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage, int? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
    public class LoanRepository : BaseRepository<tblLoan>, ILoanRepository
    {
        private readonly TimaAffiliateEntities _dbContext;
        public LoanRepository(TimaAffiliateEntities context)
            : base(context)
        {
            _dbContext = context;
        }

        public tblLoan RetrieveLoan(int loanId)
        {
            throw new NotImplementedException();
        }

        public List<tblLoan> SearchLoansByUser(string textSearch, int currentPage, int pageSize, string affCode, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = Dbset.AsQueryable().Where(t => t.AffCode == affCode);
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.LoanId.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.PhoneNumber.Contains(textSearch) || t.Address.Contains(textSearch));
            }
            if (fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(t => t.CreatedDate <= toDate.Value);
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            bool isAscending = false;
            var propertyInfo = typeof(tblLoan).GetProperty(sortField);
            if (propertyInfo == null)
            {
                var prop = typeof(tblLoan).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<tblLoan> SearchLoansByAdmin(string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage, int? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = Dbset.AsQueryable();
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.LoanId.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.PhoneNumber.Contains(textSearch) || t.Address.Contains(textSearch));
            }
            if(fromDate.HasValue)
            {
                query = query.Where(t => t.CreatedDate >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(t => t.CreatedDate <= toDate.Value);
            }
            if(status.HasValue)
            {
                query = query.Where(t => t.Status.HasValue && t.Status.Value == status);
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            bool isAscending = false;
            var propertyInfo = typeof(tblLoan).GetProperty(sortField);
            if (propertyInfo == null)
            {
                var prop = typeof(tblLoan).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public bool CreateLoanCredit(TransactionModel loanCreditModel, out string message)
        {
            message = "";
            //Create transaction
            try
            {
                if (loanCreditModel == null || string.IsNullOrEmpty(loanCreditModel.AffCode))
                {
                    message = "Dữ liệu không chính xác";
                    return false;
                }

                using (_dbContext)
                {
                    long loanId = loanCreditModel.TransactionId;
                    //Check don huy status = 0
                    if(loanCreditModel.Status == 0)
                    {
                        var existedLoan = _dbContext.tblLoans.FirstOrDefault(l => l.LoanId == loanId);
                        if(existedLoan != null)
                        {
                            existedLoan.Status = 0;
                            _dbContext.SaveChanges();
                            message = "Cập nhật trạng thái thành công.";
                            return true;
                        }
                    }


                    var user = _dbContext.tblUsers.Include("tblUserReferences").Include("tblUserReferences1").Where(u => u.AffCode == loanCreditModel.AffCode.Trim()).FirstOrDefault();
                    if (user == null)
                    {
                        message = "Mã Code không chính xác";
                        return false;
                    }

                    // Add table tblLoan
                    var config = _dbContext.tblConfigs.FirstOrDefault(c => c.ConfigId > 0);
                    loanCreditModel.CreatedDate = DateTime.Now.ToString("dd/MM/yyyy");
                    loanCreditModel.PercentAmount = config.PercentAmount;
                    loanCreditModel.UserId = user.UserId;
                    
                    if(loanId > 0)
                    {
                        var loan = new tblLoan
                        {
                            UserId = loanCreditModel.UserId,
                            AffCode = loanCreditModel.AffCode,
                            CreatedDate = DateTime.Now,
                            FullName = loanCreditModel.FullName,
                            Address = loanCreditModel.Address,
                            Amount = loanCreditModel.TotalAmount,
                            LoanId = loanId,
                            PhoneNumber = loanCreditModel.PhoneNumber,
                            CityId = loanCreditModel.CityId,
                            DistrictId = loanCreditModel.DistrictId,
                            ProductCreditId = loanCreditModel.ProductCreditId,
                            Status = loanCreditModel.Status
                        };
                        var tranEntity = _dbContext.tblLoans.Add(loan);
                        _dbContext.SaveChanges();
                        // Return
                        message = "Thêm mới thành công";
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = "Có lỗi xảy ra khi xử lý dữ liệu. Message: " + ex.Message;
                return false;
            }
        }
    }
}
