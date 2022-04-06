using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class LoanModel
    {
        public long LoanId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string CreatedDate { get; set; }
        public string AffCode { get; set; }
        public long Amount { get; set; }
        public int Status { get; set; }
        public string UtmSource { get; set; }
    }
}
