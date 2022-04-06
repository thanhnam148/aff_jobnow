using System;
using System.Collections.Generic;

namespace Aff.Models.Models
{
    public class UserCommon
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public string AffCode { get; set; }
        public string Address { get; set; }
        public string Company { get; set; }
        public LoginResult Status { get; set; }
        public string FullName { get; set; }
        public Nullable<int> RoleType { get; set; }
        public string Avatar { get; set; }
        public long TotalAmountEarning { get; set; }
        public long AvailableBalance { get; set; }
        public DateTime UpdatedDate { get; set; }
        public List<UserCommon> UserCommons { get; set; }
    }

}
