using Aff.Models.Models;
using Aff.WebApplication.Models;
using System;
using System.Web.Security;

namespace Aff.WebApplication.Security
{
    public class ClientSystemMembershipUser : MembershipUser
    {
        #region Properties

        public UserCommon UserCommon { get; set; }
        
        #endregion

        public ClientSystemMembershipUser(UserCommon user)
            : base("ClientMemberShipProvider", user.UserName, user.FullName, user.Email, string.Empty, string.Empty, true, false, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow)
        {
            UserCommon = user;
        }
    }
}