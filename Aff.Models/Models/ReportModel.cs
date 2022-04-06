using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class ReportModel
    {
        public long TransactionId { get; set; }
        public int UserId { get; set; }
        public long TotalAmount { get; set; }
        public long Amount { get; set; }
        public UserModel UserRefer { get; set; }
        public TransactionModel TransactionModel { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserReferId { get; set; }
    }

    public class ReportModelExport
    {
        public RevenueModel ReportModels { get; set; }
        public UserModel UserModel { get; set; }
    }

    public class RevenueModel { 
        public List<ReportModel> DirectRevenue { get; set; }
        public List<ReportModel> IndirectRevenue { get; set; }
    }
}
