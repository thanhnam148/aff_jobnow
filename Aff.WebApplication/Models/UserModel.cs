using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aff.WebApplication.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Trường Email bắt buộc phải nhập")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Xin vui lòng nhập đúng định dạng email")]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Trường mật khẩu bắt buộc phải nhập")]
        [MaxLength(30,ErrorMessage = "Password cannot be Greater than 30 Charaters")]
        [StringLength(31, MinimumLength = 6 , ErrorMessage ="Password Must be Minimum 6 Charaters")]
        public string Password { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public Nullable<int> RoleType { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public string AffCode { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }
    }
}