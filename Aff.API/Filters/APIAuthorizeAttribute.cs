
using Aff.API.Security;
using Aff.DataAccess;
using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Aff.API.Filters
{
    public class APIAuthorize : AuthorizeAttribute
    {
        private TimaAffiliateEntities _dbContext = new TimaAffiliateEntities();
        public override void OnAuthorization(HttpActionContext filterContext)
        {
            if (Authorize(filterContext))
            {
                return;
            }
            HandleUnauthorizedRequest(filterContext);
        }
        protected override void HandleUnauthorizedRequest(HttpActionContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);
        }

        private bool Authorize(HttpActionContext actionContext)
        {
            try
            {
                var encodedString = actionContext.Request.Headers.GetValues("Token").FirstOrDefault();
                var timeExpiry = actionContext.Request.Headers.GetValues("TokenExpiry").FirstOrDefault(); 
                if (!string.IsNullOrEmpty(encodedString))
                {
                    var key = Security.SecurityUtil.DecryptText(encodedString);

                    string[] parts = key.Split(new char[] { ':' });
                    if (parts == null || parts.Length < 2) return false;
                    var UserID = Convert.ToInt32(parts[0]);       // UserID
                    var tokenKey = parts[1];                     // Random Key
                    var lastToken = SecurityProvider.GetAccountByToken(tokenKey);
                    if (lastToken != null && lastToken.UserId == UserID && lastToken.LastAccess.AddMinutes(30) > DateTime.Now)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}