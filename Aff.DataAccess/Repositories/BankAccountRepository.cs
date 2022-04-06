using Aff.DataAccess.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.DataAccess.Repositories
{
    public interface IBankAccountRepository : IBaseRepository<tblBankAccount>
    {
        List<tblBankAccount> SearchBankAccountByUser(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, out int totalPage);
    }
    public class BankAccountRepository : BaseRepository<tblBankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(TimaAffiliateEntities context)
            : base(context)
        {
        }

        public List<tblBankAccount> SearchBankAccountByUser(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, out int totalPage)
        {
            var query = Dbset.AsQueryable().Where(t => t.UserId == userId);
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.BankId.ToString().Contains(textSearch) || t.BankName.Contains(textSearch));
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            bool isAscending = false;
            var propertyInfo = typeof(tblBankAccount).GetProperty(sortField);
            if (propertyInfo == null)
            {
                var prop = typeof(tblBankAccount).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
