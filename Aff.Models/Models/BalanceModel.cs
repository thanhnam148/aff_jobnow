using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class BalanceModel
    {
        public int UserId { get; set; }
        public string AffCode { get; set; }
        public long AvailableBalance { get; set; }
        public long CurrentBalance { get; set; }
        public long TotalBalanceDirect { get; set; }
        public long TotalBalanceBonus { get; set; }
        public long TotalAmountEarning { get; set; }
        public long PendingAmountEarning { get; set; }
        //public long PendingReferral { get; set; }
    }
}
