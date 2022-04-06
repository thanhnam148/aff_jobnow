using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using Aff.DataAccess.Common;

namespace Aff.DataAccess.Repositories
{
    public interface IUserReferenceRepository : IBaseRepository<tblUserReference>
    {
        List<tblUserReference> GetAllReferences(int pageSize, int currentPage, string txtSearch, out int totalRecords);
        List<tblUserReference> GetAlls();
        List<tblUserReference> GetByAffCode(int userId, int level);
        List<tblUserReference> GetByUserId(int userId);
        List<tblUserReference> GetByUser(int userId);
        tblUserReference GetByUserFirst(int userId);
        List<tblUserReference> GetByListUserId(List<int> userId);
    }
    public class UserReferenceRepository : BaseRepository<tblUserReference>, IUserReferenceRepository
    {
        public UserReferenceRepository(TimaAffiliateEntities context)
            : base(context)
        {
        }

        public List<tblUserReference> GetAllReferences(int pageSize,int currentPage, string txtSearch, out int totalRecords)
        {
            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (pageSize <= 0) ? 50 : pageSize;
            var query = Dbset.AsQueryable().Include(c => c.tblUser).Include(c => c.tblUser1);
            totalRecords = query.Count();
            if (!string.IsNullOrEmpty(txtSearch))
            {
                query = query.Where(c=>c.tblUser.FullName.Contains(txtSearch) || c.tblUser.Phone.Contains(txtSearch) || c.tblUser.Email.Contains(txtSearch) || c.tblUser.Address.Contains(txtSearch));
            }
            if (query.Count() > 0)
            {
                return query.OrderBy(c=>c.UserId).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
            }
            return query.ToList();
        }

        public List<tblUserReference> GetAlls()
        {
            var query = Dbset.AsQueryable().Include(c => c.tblUser).Include(c => c.tblUser1);
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }

        public List<tblUserReference> GetByAffCode(int userId,int level)
        {
            var query = Dbset.AsQueryable().Include(c => c.tblUser1).Include(c => c.tblUser).Where(c => c.UserReferenceId == userId && c.Level == (level+1));
            if (query.Count() >0)
            {
                return query.ToList();
            }
            return null;
        }

        public List<tblUserReference> GetByListUserId(List<int> userId)
        {
            var query = Dbset.AsQueryable().Where(c=> userId.Contains(c.UserReferenceId));
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }

        public List<tblUserReference> GetByUser(int userId)
        {
            var query = Dbset.AsQueryable().Where(c=>c.UserId == userId);
            if (query != null)
            {
                return query.ToList();
            }
            return null;
        }

        public tblUserReference GetByUserFirst(int userId)
        {
            var query = Dbset.AsQueryable().Where(c => c.UserReferenceId == userId);
            if (query.Count() > 0)
            {
                return query.FirstOrDefault();
            }
            return null;
        }

        public List<tblUserReference> GetByUserId(int userId)
        {
            var query = Dbset.AsQueryable().Where(c => c.UserReferenceId == userId);
            if (query.Count() > 0)
            {
                return query.ToList();
            }
            return null;
        }
    }
}
