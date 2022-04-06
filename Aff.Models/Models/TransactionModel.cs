using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class TransactionModel
    {
        public long TransactionId { get; set; }
        public int UserId { get; set; }
        public int Status { get; set; }
        public string AffCode { get; set; }
        public double PercentAmount { get; set; }
        public long Amount { get; set; }
        public long TotalAmount { get; set; }
        public double PaidAmount { get; set; }
        public string FullName { get; set; }
        public string CreatedDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public long LenderId { get; set; }
        public long LoanId { get; set; }
        public int CityId { set; get; }
        public int DistrictId { set; get; }
        public int ProductCreditId { set; get; }
        public long? TotalProfit { get; set; }
        public Aff.Models.Models.UserModel UserModel { get; set; }
    }
}
