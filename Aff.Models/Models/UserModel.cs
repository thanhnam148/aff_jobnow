using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aff.Models.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Trường Email bắt buộc phải nhập")]
        //[RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Xin vui lòng nhập đúng định dạng email")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Trường mật khẩu bắt buộc phải nhập")]
        [MaxLength(30,ErrorMessage = "Password cannot be Greater than 30 Charaters")]
        [StringLength(31, MinimumLength = 4 , ErrorMessage ="Password Must be Minimum 4 Charaters")]
        public string Password { get; set; }
        public string LevelString { get; set; }
        public long TransactionPercentage { get; set; }

        //(Model.AvailableBalance - Model.PendingAmountEarning)
        public long TotalSurplus { get; set; }
        public double BalanceAfterFee { get; set; }
        //(Model.AvailableBalance - Model.PendingAmountEarning) * 0.9)
        public long TotalBalance { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public string LevelName { get; set; }
        public string Phone { get; set; }
        public Nullable<int> RoleType { get; set; }
        public string CreatedDate { get; set; }
        public bool IsActive { get; set; }
        //[Required(ErrorMessage = "Trường Mã giới thiệu bắt buộc phải nhập")]
        public string AffCode { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }
        public long TotalAmountEarning { get; set; }
        public long AvailableBalance { get; set; }
        public long AlreadyAmountTranfer { get; set; }
        public long TotalAvailableAmountTranfer { get; set; }
        public long DifferenceAmount { get; set; }
        public long CurrentBalance { get; set; }
        public long TotalBalanceDirect { get; set; }
        public long TotalBalanceBonus { get; set; }
        public long PendingAmountEarning { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int BankId { get; set; }
        public List<TransactionModel> UserTransactions { get; set; }
        public double ProfitPercentage { get; set; }
        public string BankName { get; set; }
        public string BankAddress { get; set; }
        public string BankOwnerName { get; set; }
        public string BankAccount { get; set; }
        public string Note { get; set; }
        public int StatusAmount { get; set; }
        public int Level { get; set; }
        public int UserReferId { get; set; }
        public bool IsChild { get; set; }
        public long TotalMoney { get; set; }
        public DateTime? EndMatchingDate { get; set; }
        public DateTime? EndPaymentDate { get; set; }
        public List<UserModel> UserModels { get; set; }
        public List<GroupUser> GroupUser { get; set; }
    }

    public class UserChangePasswordModel
    {
        public string Email { get; set; }
        [Required(ErrorMessage = "Trường mật khẩu bắt buộc phải nhập")]
        [MaxLength(30, ErrorMessage = "Password cannot be Greater than 30 Charaters")]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "Trường mật khẩu mới bắt buộc phải nhập")]
        [MaxLength(30, ErrorMessage = "Password cannot be Greater than 30 Charaters")]
        [StringLength(31, MinimumLength = 4, ErrorMessage = "Password Must be Minimum 4 Charaters")]
        public string NewPassword { get; set; }
    }

    public class UserDetail
    {
        public int Level { get; set; }
        public List<UserModel> UserModel { get; set; }
        public List<GroupUser> GroupUsers { get; set; }
    }

    public class GroupUser
    {
        public int Level { get; set; }
        public List<UserModel> UserItems { get; set; }
    }
}