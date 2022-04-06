using Aff.API.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace Aff.API.Principal
{
    public class ApplicationPrincipalProvider
    {
        public bool SetPrincipal(IUserAccount account)
        {
            var principal = CreatePrincipal(account);
            if (principal == null) return false;
            Thread.CurrentPrincipal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
            return true;
        }

        private IPrincipal CreatePrincipal(IUserAccount account)
        {
            if (account == null) return null;

            var identity = new GenericIdentity(account.UserName);
            var principal = new ApplicationPrincipal(account.UserId, identity, new string [0]);
            return principal;
        }
    }
}