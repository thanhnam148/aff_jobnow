using Aff.DataAccess.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.DataAccess.Repositories
{
    public interface IReportRepository : IBaseRepository<tblReport>
    {
        List<tblReport> SearchReportByUser(int currentPage, int pageSize, int userId, out int totalPage);
        List<tblReport> GetAllTransactionPreviousMonth(DateTime dtCurrentMonth);
        List<tblReport> GetByUserId(int userId);
        List<tblReport> GetAlls();
        List<tblReport> GetByUserForLoanId(int userId);
    }
    public class ReportRepository : BaseRepository<tblReport>, IReportRepository
    {
        public ReportRepository(TimaAffiliateEntities context)
            : base(context)
        {
        }

        public List<tblReport> SearchReportByUser(int currentPage, int pageSize, int userId, out int totalPage)
        {
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;
            var query = Dbset.Where(c => c.UserId == userId);
            totalPage = query.Count();
            query = query.OrderBy(c => c.TransactionId);

            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<tblReport> GetAllTransactionPreviousMonth(DateTime dtCurrentMonth)
        {
            //previous month
            var fromDate = GetFirstDayOfMonth(dtCurrentMonth.AddMonths(-1));
            var toDate = GetLastDayOfMonth(dtCurrentMonth.AddMonths(-1));
            var query = Dbset.Where(t => t.Amount.HasValue && t.Amount.Value > 0 && t.CreatedDate.Value > fromDate && t.CreatedDate.Value < toDate && (!t.Level.HasValue || t.Level.Value == 1));
            return query.ToList();
        }

        private DateTime GetLastDayOfMonth(DateTime dtCurrent)
        {
            return new DateTime(dtCurrent.Year, dtCurrent.Month, DateTime.DaysInMonth(dtCurrent.Year, dtCurrent.Month), 23, 59, 59);
        }
        private DateTime GetFirstDayOfMonth(DateTime dtCurrent)
        {
            return new DateTime(dtCurrent.Year, dtCurrent.Month, 1, 1, 1, 1);
        }

        public List<tblReport> GetByUserId(int userId)
        {
            var query = Dbset.AsQueryable().Include(c=>c.tblTransaction).Include(c=>c.tblTransaction.tblUser).Where(c => c.UserId == userId);
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }

        public List<tblReport> GetByUserForLoanId(int userId)
        {
            var query = Dbset.AsQueryable().Where(c => c.UserReferId == userId);
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return query.ToList();
        }

        public List<tblReport> GetAlls()
        {
            var query = Dbset.AsQueryable().Include(c => c.tblTransaction).Include(c=>c.tblUser);
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }
    }
}
