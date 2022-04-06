using Aff.DataAccess.Common;
using Aff.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Aff.DataAccess.Common;
using System.Data.Entity;

namespace Aff.DataAccess.Repositories
{
    public interface ITransactionRepository : IBaseRepository<tblTransaction>
    {
        tblTransaction RetrieveTransaction(long transactionId);
        List<tblTransaction> SearchTransactionByUser(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, out int totalPage);
        bool CreateTransaction(TransactionModel transactionModel, out string message);
        long GetSummary(DateTime fromDate, DateTime toDate, out long totalPaymentAmount);
        List<tblTransaction> GetAllTranSactionPreviousMonth(DateTime dtCurrentMonth);
        List<tblTransaction> GetAllTranSactions();
        List<tblTransaction> GetAllTranSaction(int userId);
    }
    public class TransactionRepository : BaseRepository<tblTransaction>, ITransactionRepository
    {
        private readonly TimaAffiliateEntities _dbContext;
        public TransactionRepository(TimaAffiliateEntities context)
            : base(context)
        {
            _dbContext = context;
        }


        public tblTransaction RetrieveTransaction(long transactionId)
        {
            return Dbset.FirstOrDefault(x => x.TransactionId == transactionId);
        }

        public List<tblTransaction> SearchTransactionByUser(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, out int totalPage)
        {
            using (_dbContext)
            {
                var query = _dbContext.tblTransactions.Where(t => t.UserId == userId);
                if (!string.IsNullOrEmpty(textSearch))
                {
                    query = query.Where(t => t.TransactionId.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.PhoneNumber.Contains(textSearch) || t.Address.Contains(textSearch));
                }
                currentPage = (currentPage <= 0) ? 1 : currentPage;
                pageSize = (pageSize <= 0) ? 10 : pageSize;

                //Sorting
                bool isAscending = false;
                var propertyInfo = typeof(tblTransaction).GetProperty(sortField);
                if (propertyInfo == null)
                {
                    var prop = typeof(tblTransaction).GetProperties().FirstOrDefault();
                    sortField = prop.Name;
                }

                if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                    isAscending = true;

                query = query.Order(sortField, isAscending);
                totalPage = query.Count();
                return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }
        }

        public bool CreateTransaction(TransactionModel transactionModel, out string message)
        {
            message = "";
            //Create transaction
            try
            {
                if (transactionModel == null || string.IsNullOrEmpty(transactionModel.AffCode))
                {
                    message = "Dữ liệu không chính xác";
                    return false;
                }

                using (_dbContext)
                {
                    var user = _dbContext.tblUsers.Include("tblUserReferences").Include("tblUserReferences1").Where(u => u.AffCode == transactionModel.AffCode.Trim()).FirstOrDefault();
                    if (user == null)
                    {
                        message = "Mã Code không chính xác";
                        return false;
                    }
                    long loanId = transactionModel.TransactionId;
                    if(loanId <= 0)
                    {
                        message = "Dữ liệu không chính xác";
                        return false;
                    }

                    var loan = _dbContext.tblLoans.Include("tblTransactions").Where(u => u.AffCode == transactionModel.AffCode.Trim() && u.LoanId == loanId).FirstOrDefault();
                    if (loan == null)
                    {
                        message = "Mã Code không chính xác";
                        return false;
                    }
                    if (loan.tblTransactions.Any(t => t.TotalAmount.HasValue && t.TotalAmount.Value > 0))
                    {
                        message = "Transaction đã tồn tại";
                        return false;
                    }
                    if(transactionModel.LoanId == 0)
                    {
                        transactionModel.LoanId = loan.LoanId;
                    }

                    // Add table transaction
                    double percentAmount = 0;
                    double percentRefer = 0;
                    var config = _dbContext.tblConfigs.FirstOrDefault(c => c.ConfigId > 0);
                    if(config != null)
                    {
                        percentAmount = config.PercentAmount;
                        percentRefer = config.PercentRefer;
                    }
                    if(user.PercentAmount.HasValue && user.PercentAmount.Value > 0)
                    {
                        percentAmount = user.PercentAmount.Value;
                    }
                    if (user.PercentRefer.HasValue && user.PercentRefer.Value > 0)
                    {
                        percentRefer = user.PercentRefer.Value;
                    }
                    transactionModel.CreatedDate = DateTime.Now.ToString("dd/MM/yyyy");
                    transactionModel.PercentAmount = percentAmount;
                    transactionModel.UserId = user.UserId;
                    if(transactionModel.TotalAmount == 0 && loan.Amount.HasValue)
                        transactionModel.TotalAmount = loan.Amount.Value;

                    var transaction = new tblTransaction
                    {
                        UserId = transactionModel.UserId,
                        LoanId = loan.LoanId,
                        LenderId = transactionModel.LenderId,
                        AffCode = transactionModel.AffCode,
                        TotalAmount = transactionModel.TotalAmount,
                        CreatedDate = DateTime.Now,
                        FullName = loan.FullName,
                        PercentAmount = percentAmount,
                        Status = transactionModel.Status,
                        Address = loan.Address,
                        PhoneNumber = loan.PhoneNumber,
                        CityId = transactionModel.CityId,
                        DistrictId = transactionModel.DistrictId,
                        ProductCreditId = transactionModel.ProductCreditId
                    };
                    var tranEntity = _dbContext.tblTransactions.Add(transaction);
                    _dbContext.SaveChanges();
                    // Return
                    message = "Thêm tài khoản thành công";
                    CreateReports(transactionModel, tranEntity.TransactionId, user, percentAmount, percentRefer);
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = "Có lỗi xảy ra khi xử lý dữ liệu. Message: " + ex.Message;
                return false;
            }
        }

        public TransactionApiModel GetSummaryTransaction(DateTime fromDate, DateTime toDate)
        {
            using (_dbContext)
            {
                long totalPurchaseAmount = 0;
                int totalInvoice = 0;
                long totalPaymentAmount = 0;
                var trans = _dbContext.tblTransactions.Include("tblReports").Where(t => t.CreatedDate.HasValue && t.CreatedDate.Value > fromDate && t.CreatedDate.Value < toDate && t.tblReports.Any());
                if (trans != null && trans.Any())
                {
                    totalInvoice = trans.Count();
                    totalPurchaseAmount = trans.Where(o => o.TotalAmount.HasValue).Sum(t => t.TotalAmount).Value;
                    totalPaymentAmount = trans.Where(o => o.TotalAmount.HasValue).Sum(t => t.tblReports.Where(s => s.Amount.HasValue).Sum(r => r.Amount)).Value;
                }
                return new TransactionApiModel
                {
                    TotalInvoice = totalInvoice,
                    TotalPaymentAmount = totalPaymentAmount,
                    TotalPurchaseAmount = totalPurchaseAmount
                };
            }
                
        }

        public long GetSummary(DateTime fromDate, DateTime toDate, out long totalPaymentAmount)
        {
            using (_dbContext)
            {
                long totalPurchaseAmount = 0;
                totalPaymentAmount = 0;
                var trans = _dbContext.tblTransactions.Include("tblReports").Where(t => t.CreatedDate.HasValue && t.CreatedDate.Value > fromDate && t.CreatedDate.Value < toDate && t.tblReports.Any());
                if (trans != null && trans.Any())
                {
                    totalPurchaseAmount = trans.Where(o => o.TotalAmount.HasValue).Sum(t => t.TotalAmount).Value;
                    totalPaymentAmount = trans.Where(o => o.TotalAmount.HasValue).Sum(t => t.tblReports.Where(s => s.Amount.HasValue).Sum(r => r.Amount)).Value;
                }
                return totalPurchaseAmount;
            }

        }

        public SummaryDashboardApiModel GetSummaryDashBoard()
        {
            using (_dbContext)
            {
                var allTransReport = _dbContext.tblTransactions.Include("tblReports").Where(o => o.TotalAmount.HasValue && o.TotalAmount.Value > 0);
                SummaryDashboardApiModel sdApi = new SummaryDashboardApiModel();
                if (allTransReport != null && allTransReport.Any())
                {
                    sdApi.AllTimeSummary.TotalRevenue = allTransReport.Sum(t => t.TotalAmount.Value);
                    sdApi.AllTimeSummary.TotalPaymentMMO = allTransReport.Sum(t => t.tblReports.Where(s => s.Amount.HasValue).Sum(r => r.Amount.Value));

                    //Today
                    var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 1, 1);
                    var lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                    var currentDateTrans = allTransReport.Where(t => t.CreatedDate.HasValue && t.CreatedDate.Value >= firstDay && t.CreatedDate.Value <= lastDay);
                    if (currentDateTrans != null && currentDateTrans.Any())
                    {
                        sdApi.TodaySummary.TotalRevenue = currentDateTrans.Sum(t => t.TotalAmount.Value);
                        sdApi.TodaySummary.TotalPaymentMMO = currentDateTrans.Sum(t => t.tblReports.Where(s => s.Amount.HasValue).Sum(r => r.Amount.Value));
                    }

                    //current month
                    var fromDate = GetFirstDayOfMonth(DateTime.Now);
                    var toDate = GetLastDayOfMonth(DateTime.Now);
                    var currentTransMonth = allTransReport.Where(t => t.CreatedDate.HasValue && t.CreatedDate.Value >= fromDate && t.CreatedDate.Value <= toDate);
                    if(currentTransMonth != null && currentTransMonth.Any())
                    {
                        sdApi. CurrentMonthSummary.TotalRevenue = currentTransMonth.Sum(t => t.TotalAmount.Value);
                        sdApi.CurrentMonthSummary.TotalPaymentMMO = currentTransMonth.Sum(t => t.tblReports.Where(s => s.Amount.HasValue).Sum(r => r.Amount.Value));
                    }

                    //previous month
                    fromDate = GetFirstDayOfMonth(DateTime.Now.AddMonths(-1));
                    toDate = GetLastDayOfMonth(DateTime.Now.AddMonths(-1));
                    var previousTransMonth = allTransReport.Where(t => t.CreatedDate.HasValue && t.CreatedDate.Value >= fromDate && t.CreatedDate.Value <= toDate);
                    if (previousTransMonth != null && previousTransMonth.Any())
                    {
                        sdApi.PreviousMonthSummary.TotalRevenue = previousTransMonth.Sum(t => t.TotalAmount.Value);
                        sdApi.PreviousMonthSummary.TotalPaymentMMO = previousTransMonth.Sum(t => t.tblReports.Where(s => s.Amount.HasValue).Sum(r => r.Amount.Value));
                    }
                }
                return sdApi;
            }

        }

        public List<tblTransaction> GetAllTranSactionPreviousMonth(DateTime dtCurrentMonth)
        {
            //previous month
            var fromDate = GetFirstDayOfMonth(dtCurrentMonth.AddMonths(-1));
            var toDate = GetLastDayOfMonth(dtCurrentMonth.AddMonths(-1));
            var query = Dbset.Where(t => t.TotalAmount.HasValue && t.TotalAmount.Value > 0 && t.CreatedDate.Value > fromDate && t.CreatedDate.Value < toDate);
            return query.ToList();
        }

        public bool CreateReports(TransactionModel transaction, long transactionId, tblUser user, double percentAmount, double percentRefer)
        {
            using (_dbContext)
            {
                var bonusAmount = transaction.TotalAmount * (percentAmount / 100);
                var report = new tblReport
                {
                    TransactionId = transactionId,
                    LoanId = transaction.LoanId,
                    LenderId = transaction.LenderId,
                    UserId = user.UserId,
                    CreatedDate = DateTime.Now,
                    PercentAmount = percentAmount,
                    Amount = (long)bonusAmount,
                    TransactionAmount = transaction.TotalAmount
                };
                _dbContext.tblReports.Add(report);
                _dbContext.SaveChanges();

                foreach (var uf in user.tblUserReferences)
                {
                    var referBonusAmount = bonusAmount * Math.Pow((percentRefer / 100), uf.Level.Value);
                    var report2 = new tblReport
                    {
                        TransactionId = transactionId,
                        LoanId = transaction.LoanId,
                        LenderId = transaction.LenderId,
                        UserId = uf.UserReferenceId,
                        CreatedDate = DateTime.Now,
                        UserReferId = user.UserId,
                        Level = uf.Level,
                        PercentAmount = percentRefer,
                        Amount = Convert.ToInt64(referBonusAmount),
                        TransactionAmount = transaction.TotalAmount
                    };
                    _dbContext.tblReports.Add(report2);
                    _dbContext.SaveChanges();
                }
                return true;
            }
        }

        private DateTime GetLastDayOfMonth(DateTime dtCurrent)
        {
            return new DateTime(dtCurrent.Year, dtCurrent.Month, DateTime.DaysInMonth(dtCurrent.Year, dtCurrent.Month), 23, 59, 59);
        }
        private DateTime GetFirstDayOfMonth(DateTime dtCurrent)
        {
            return new DateTime(dtCurrent.Year, dtCurrent.Month, 1, 1, 1, 1);
        }

        public List<tblTransaction> GetAllTranSaction(int userId)
        {
            var query = Dbset.AsQueryable().Include(c => c.tblUser).Where(c => c.UserId == userId);
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }

        public List<tblTransaction> GetAllTranSactions()
        {
            var query = Dbset.AsQueryable().Include(c => c.tblUser);
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return query.ToList();
        }
    }
}
