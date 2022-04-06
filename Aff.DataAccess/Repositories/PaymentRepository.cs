using Aff.DataAccess.Common;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aff.DataAccess.Repositories
{
    public interface IPaymentRepository : IBaseRepository<tblPayment>
    {
        List<tblPayment> SearchPaymentByUser(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, List<int> status, out int totalPage);
        List<tblPayment> SearchPaymentByStatus(string textSearch, int currentPage, int pageSize, string sortField, string sortType, List<int> status, out int totalPage);
        List<tblPayment> GetPaymentByStatus(int userId, List<int> status);
    }
    public class PaymentRepository : BaseRepository<tblPayment>, IPaymentRepository
    {
        public PaymentRepository(TimaAffiliateEntities context)
            : base(context)
        {
        }

        public List<tblPayment> SearchPaymentByUser(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, List<int> status, out int totalPage)
        {
            var query = Dbset.AsQueryable().Where(t => t.UserId == userId && t.PaymentStatus.HasValue && status.Contains(t.PaymentStatus.Value));
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.PaymentId.ToString().Contains(textSearch) || t.Comment1.Contains(textSearch));
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            bool isAscending = false;
            var propertyInfo = typeof(tblPayment).GetProperty(sortField);
            if(propertyInfo == null)
            {
                var prop = typeof(tblPayment).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<tblPayment> SearchPaymentByStatus(string textSearch, int currentPage, int pageSize, string sortField, string sortType, List<int> status, out int totalPage)
        {
            var query = Dbset.Include("tblUser").AsQueryable().Where(t => t.PaymentStatus.HasValue && status.Contains(t.PaymentStatus.Value));
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.PaymentId.ToString().Contains(textSearch) || t.Comment1.Contains(textSearch) || t.tblUser.Email.Contains(textSearch));
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            bool isAscending = false;
            var propertyInfo = typeof(tblPayment).GetProperty(sortField);
            if (propertyInfo == null)
            {
                var prop = typeof(tblPayment).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<tblPayment> GetPaymentByStatus(int userId, List<int> status)
        {
            return Dbset.AsQueryable().Where(t => t.UserId == userId && t.PaymentStatus.HasValue && status.Contains(t.PaymentStatus.Value)).ToList();
        }
    }
}
