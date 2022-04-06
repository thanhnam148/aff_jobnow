using Aff.DataAccess.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.DataAccess.Repositories
{
    public interface IViewCountRepository : IBaseRepository<tblViewCount>
    {
        tblViewCount RetrieveViewCount(int viewCountId);
        bool CreateViewCount(string affCode, string ipAddress);
        long TotalAccessLinkRef(string affCode, out int accessInDay);
    }
    public class ViewCountRepository : BaseRepository<tblViewCount>, IViewCountRepository
    {
        private readonly TimaAffiliateEntities _dbContext;
        public ViewCountRepository(TimaAffiliateEntities context)
            : base(context)
        {
            _dbContext = context;
        }

        public tblViewCount RetrieveViewCount(int viewCountId)
        {
            return Dbset.FirstOrDefault(v => v.Id == viewCountId);
        }

        public bool CreateViewCount(string affCode, string ipAddress)
        {
            using (_dbContext)
            {
                var checkUser = _dbContext.tblUsers.Any(u => u.AffCode == affCode);
                if (!checkUser) return false;
                int count = 1;
                var currentIp = _dbContext.tblViewCounts.FirstOrDefault(i => i.AffCode == affCode && i.IpAddress == ipAddress);
                if (currentIp != null && currentIp.Count.HasValue)
                {
                    if (currentIp.CreatedDate.Value.AddMinutes(10) > DateTime.Now) return false;
                    count = currentIp.Count.Value + 1;
                }
                tblViewCount viewCount = new tblViewCount
                {
                    AffCode = affCode,
                    IpAddress = ipAddress,
                    CreatedDate = DateTime.Now,
                    Count = count
                };
                _dbContext.tblViewCounts.Add(viewCount);
                _dbContext.SaveChanges();

                return true;
            }
                
        }

        public long TotalAccessLinkRef(string affCode, out int accessInDay)
        {
            using (_dbContext)
            {
                accessInDay = 0;
                if (string.IsNullOrEmpty(affCode)) return 0;
                var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 1, 1);
                var lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                var query = _dbContext.tblViewCounts.Where(v => v.AffCode == affCode);
                accessInDay = query.Where(t => t.CreatedDate >= firstDay && t.CreatedDate < lastDay).Count();
                return query.LongCount();
            }
                
        }
    }
}
