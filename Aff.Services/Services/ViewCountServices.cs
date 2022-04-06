using Aff.DataAccess;
using Aff.DataAccess.Common;
using Aff.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Services
{
    public interface IViewCountServices : IEntityService<tblViewCount>
    {
        bool CreateViewCount(string affCode, string ipAddress);
        long TotalAccessLinkRef(string affCode, out int accessInDay);
    }
    public class ViewCountServices : EntityService<tblViewCount>, IViewCountServices
    {
        private readonly IViewCountRepository _viewCountRepository;
        public ViewCountServices(IUnitOfWork unitOfWork, IViewCountRepository viewCountRepository)
            : base(unitOfWork, viewCountRepository)
        {
            _viewCountRepository = viewCountRepository;
        }

        public bool CreateViewCount(string affCode, string ipAddress)
        {
            int count = 1;
            var currentIp = GetAll().FirstOrDefault(i => i.AffCode == affCode && i.IpAddress == ipAddress);
            if (currentIp != null && currentIp.Count.HasValue)
            {
                count = currentIp.Count.Value + 1;
            }
            tblViewCount viewCount = new tblViewCount
            {
                AffCode = affCode,
                IpAddress = ipAddress,
                CreatedDate = DateTime.Now,
                Count = count
            };
            _viewCountRepository.Insert(viewCount);
            UnitOfWork.SaveChanges();

            return true;
        }

        public long TotalAccessLinkRef(string affCode, out int accessInDay)
        {
            accessInDay = 0;
            if (string.IsNullOrEmpty(affCode)) return 0;
            var firstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 1, 1);
            var lastDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            var query = GetAll().Where(v => v.AffCode == affCode);
            accessInDay = query.Where(t => t.CreatedDate >= firstDay && t.CreatedDate < lastDay).Count();
            return query.LongCount();
        }
    }


}
