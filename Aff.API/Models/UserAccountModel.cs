using Aff.API.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Aff.API.Models
{
    public class UserAccountModel : IUserAccount
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime LastAccess { get; set; }
        public string Token { get; set; }
    }
}