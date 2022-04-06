using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using Aff.DataAccess.Common;
using System;
using Aff.Models.Models;

namespace Aff.DataAccess.Repositories
{
    public interface IUserRepository : IBaseRepository<tblUser>
    {
        tblUser RetrieveUser(int userId);
        List<tblUser> SearchUserAvailableBalance(int status, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage);
        List<tblUser> SearchUser(string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null);
        List<tblUser> SearchUserAff(int userId, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage);
        List<tblUser> GetUserMatching(string textSearch, string sortField, string sortType, DateTime fromDate, DateTime toDate, out int totalPage);
        bool UpdateMatchingAmount(List<UserModel> userMatchings, string dataOfMonth, DateTime fromDate, DateTime toDate);
        tblUser GetUserByEmail(string email);
        List<tblUser> Search(int currentPage, int pageSize, string searchStr, out int totalPage);
        List<tblUser> SearchAll(string textSearch);
        List<tblUser> GetAlls(string searchTxt,DateTime? fromDate,DateTime? toDate);
        tblUser GetUserByUserName(string username);
    }
    public class UserRepository : BaseRepository<tblUser>, IUserRepository
    {
        private readonly TimaAffiliateEntities _dbContext;
        public UserRepository(TimaAffiliateEntities context)
            : base(context)
        {
            _dbContext = context;
        }
        public List<tblUser> Search(int currentPage, int pageSize,string searchStr, out int totalPage)
        {
            var query = Dbset.AsQueryable();
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;
            if (!string.IsNullOrEmpty(searchStr))
            {
                query = query.Where(c=>c.FullName.Contains(searchStr) || c.Email.Contains(searchStr) || c.Phone.Contains(searchStr) || c.Address.Contains(searchStr) || c.AffCode.Contains(searchStr));
            }
            totalPage = query.Count();
            return query.OrderBy(c => c.UserId).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }
        public tblUser RetrieveUser(int userId)
        {
            return Dbset.Include("tblBankAccounts").FirstOrDefault(x => x.UserId == userId);
        }

        public tblUser GetUserByUserName(string username)
        {
            return Dbset.FirstOrDefault(c => c.UserName.Equals(username) && (!c.IsActive));
        }

        public bool UpdateMatchingAmount(List<UserModel> userMatchings, string dataOfMonth, DateTime fromDate, DateTime toDate)
        {
            try
            {
                if (string.IsNullOrEmpty(dataOfMonth)) return false;
                dataOfMonth = dataOfMonth.Replace(" ", "");
                using (_dbContext)
                {
                    foreach (var item in userMatchings)
                    {
                        var cUser = _dbContext.tblUsers.Include("tblUserReports").FirstOrDefault(r => r.UserId == item.UserId);
                        if (cUser == null) continue;
                        //check user report
                        var userReportCheck = cUser.tblUserReports.Where(r => r.UserId == item.UserId && r.DataOfMonth == dataOfMonth.Trim() && r.TranferAmount.HasValue).OrderByDescending(o => o.UserReportId).FirstOrDefault();
                        if (userReportCheck == null)
                        {
                            CreateUserReport(_dbContext, item, dataOfMonth, fromDate, toDate);
                        }
                        else //Existed month of year
                        {
                            var lastAmountTranferBefor = userReportCheck.TranferAmount.Value;
                            //item.TotalAvailableAmountTranfer = item.TotalAvailableAmountTranfer - lastAmountTranferBefor;
                            if (item.AlreadyAmountTranfer - lastAmountTranferBefor <= 0) continue;
                            CreateUserReport(_dbContext, item, dataOfMonth, fromDate, toDate);
                        }
                        //update user
                        cUser.UpdatedDate = DateTime.Now;
                        cUser.EndMatchingDate = toDate;
                        cUser.AvailableBalance = item.TotalAvailableAmountTranfer;
                        _dbContext.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool CreateUserReport(TimaAffiliateEntities dbContext, UserModel userModel, string dataOfMonth, DateTime fromDate, DateTime toDate)
        {
            try
            {
                //var userReportCheck = _dbContext.tblUserReports.Where(r=>r.UserId == userModel.UserId )
                dataOfMonth = dataOfMonth.PadLeft(7, '0');
                var report = new tblUserReport
                {
                    TotalAvailableAmount = userModel.TotalAvailableAmountTranfer,
                    CreatedDate = DateTime.Now,
                    UserId = userModel.UserId,
                    TranferAmount = userModel.AlreadyAmountTranfer,
                    FromDate = fromDate,
                    ToDate = toDate,
                    DataOfMonth = dataOfMonth.Trim()
                };
                dbContext.tblUserReports.Add(report);
                dbContext.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<tblUser> SearchUserAvailableBalance(int status, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage)
        {
            var query = Dbset.AsQueryable().Include("tblBankAccounts").Where(t => t.RoleType.HasValue && t.RoleType.Value == 3 && t.AvailableBalance.HasValue);
            if(status == 1)
            {
                query = query.Where(t => t.AvailableBalance.Value >= 50000 && t.tblBankAccounts.Any());
            }
            else if(status == 2)
            {
                query = query.Where(t => t.AvailableBalance.Value < 50000 && t.AvailableBalance.Value > 0);
            }

            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.Email.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.Phone.Contains(textSearch) || t.Address.Contains(textSearch) || t.AffCode.Contains(textSearch));
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            bool isAscending = false;
            var propertyInfo = typeof(tblUser).GetProperty(sortField);
            if (propertyInfo == null)
            {
                var prop = typeof(tblUser).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<tblUser> SearchUser(string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = Dbset.AsQueryable().Where(t => t.RoleType.HasValue && t.RoleType.Value == 3);
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.Email.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.Phone.Contains(textSearch) || t.Address.Contains(textSearch) || t.AffCode.Contains(textSearch));
            }
            if(fromDate.HasValue)
            {
                query = query.Where(d => d.CreatedDate.Value > fromDate.Value);
            }
            if (toDate.HasValue)
            {
                query = query.Where(d => d.CreatedDate.Value < toDate.Value);
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            bool isAscending = false;
            var propertyInfo = typeof(tblUser).GetProperty(sortField);
            if (propertyInfo == null)
            {
                var prop = typeof(tblUser).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<tblUser> SearchUserAff(int userId, string textSearch, int currentPage, int pageSize, string sortField, string sortType, out int totalPage)
        {
            var query = Dbset.AsQueryable().Where(t => t.RoleType.HasValue && t.RoleType.Value == 3 && t.tblUserReferences.Any(r=>r.UserReferenceId == userId && r.Level.HasValue && r.Level.Value == 1));
            //var query = Dbset.AsQueryable().Where(t => t.RoleType.HasValue && t.RoleType.Value == 3 && t.tblUserReferences1.Any(r=>r.UserId == userId && r.Level.HasValue && r.Level.Value == 1));
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.Email.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.Phone.Contains(textSearch) || t.Address.Contains(textSearch) || t.AffCode.Contains(textSearch));
            }
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 10 : pageSize;

            //Sorting
            
            bool isAscending = false;
            if (sortField == null) sortField = "";
            var propertyInfo = typeof(tblUser).GetProperty(sortField);
            if (propertyInfo == null)
            {
                var prop = typeof(tblUser).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        public List<tblUser> GetUserMatching(string textSearch, string sortField, string sortType, DateTime fromDate, DateTime toDate, out int totalPage)
        {
            var query = Dbset.AsQueryable().Include("tblReports").Include("tblPayments").Include("tblUserReports").Include("tblBankAccounts").Where(t => t.RoleType.HasValue && t.RoleType.Value == 3 && t.tblReports.Any(r=> r.CreatedDate > fromDate && r.CreatedDate < toDate));
            if (!string.IsNullOrEmpty(textSearch))
            {
                query = query.Where(t => t.Email.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.Phone.Contains(textSearch) || t.Address.Contains(textSearch) || t.AffCode.Contains(textSearch));
            }

            //Sorting
            bool isAscending = false;
            var propertyInfo = !string.IsNullOrEmpty(sortField) ? typeof(tblUser).GetProperty(sortField): null;
            if (propertyInfo == null)
            {
                var prop = typeof(tblUser).GetProperties().FirstOrDefault();
                sortField = prop.Name;
            }

            if (!string.IsNullOrEmpty(sortType) && sortType.ToLower().Contains("asc"))
                isAscending = true;

            query = query.Order(sortField, isAscending);
            totalPage = query.Count();
            return query.ToList();
        }

        public tblUser GetUserByEmail(string email)
        {
            return Dbset.FirstOrDefault(c => c.Email.Equals(email));
        }

        public List<tblUser> GetAlls(string searchTxt,DateTime? fromDate, DateTime? toDate)
        {
            var query = Dbset.AsQueryable();
            if (!string.IsNullOrEmpty(searchTxt))
            {
                query = query.Where(t => t.Email.ToString().Contains(searchTxt) || t.FullName.Contains(searchTxt) || t.Phone.Contains(searchTxt) || t.Address.Contains(searchTxt) || t.AffCode.Contains(searchTxt));
            }
            if (fromDate != null)
            {
                query = query.Where(c=>c.CreatedDate >= fromDate);
            }
            if (toDate != null)
            {
                query = query.Where(c => c.CreatedDate <= toDate);
            }

            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }

        public List<tblUser> SearchAll(string textSearch)
        {
            var query = Dbset.AsQueryable().Include(c=>c.tblUserReferences).Where(t => t.Email.ToString().Contains(textSearch) || t.FullName.Contains(textSearch) || t.Phone.Contains(textSearch) || t.Address.Contains(textSearch) || t.AffCode.Contains(textSearch));
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }
    }
}
