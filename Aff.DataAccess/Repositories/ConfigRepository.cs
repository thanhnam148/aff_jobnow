using Aff.DataAccess.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.DataAccess.Repositories
{
    public interface IConfigRepository : IBaseRepository<tblConfig>
    {
        tblLoan RetrieveConfig(int configId);
    }
    public class ConfigRepository : BaseRepository<tblConfig>, IConfigRepository
    {
        public ConfigRepository(TimaAffiliateEntities context)
            : base(context)
        {
        }

        public tblLoan RetrieveConfig(int loanId)
        {
            throw new NotImplementedException();
        }
    }
}
