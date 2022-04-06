using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Models.Models
{
    public class PaymentModel
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
        [Required(ErrorMessage = "Bạn cần phải nhập số tiền cần rút")]
        public long AmountEarning { get; set; }
        public int PaymentStatus { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedDate { get; set; }
        public int VerifyStatus { get; set; }
        public string Comment1 { get; set; }
        public string Comment2 { get; set; }
        public string Comment3 { get; set; }
        public int PaymentMethod { get; set; }
        public string PaidDate { get; set; }
        public string AccountInfo { get; set; }
        public Nullable<long> AvailableBalance { get; set; }
    }
}
