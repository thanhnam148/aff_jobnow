using Aff.WebApplication.Configurations;
using Aff.WebApplication.Security;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace Aff.WebApplication.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            var redirectControllerName = SystemConfiguration.GetStringKey("AuthenticationControllerName", "Account");
            var redirectActionName = SystemConfiguration.GetStringKey("AuthenticationActionName", "Login");
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                if (!filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var routeUrl = string.Format("~/{0}/{1}?returnUrl={2}", redirectControllerName, redirectActionName, System.Web.HttpContext.Current.Request.RawUrl);
                    filterContext.Result = new RedirectResult(routeUrl);
                    return;
                }
                //var routeData = new RouteData();
                //routeData.Values["returnUrl"] = System.Web.HttpContext.Current.Request.RawUrl;
                //routeData.Values.Add("controller", redirectControllerName);
                //routeData.Values.Add("action", redirectActionName);
                var urlHelper = new UrlHelper(filterContext.RequestContext);
                filterContext.Result = new JavaScriptResult
                {
                    Script = string.Format("window.location = '/{0}/{1}';", redirectControllerName, redirectActionName)
                };
                return;
            }
            else
            {
                var customPrincipal = filterContext.HttpContext.User.Identity as ClientSystemIdentity;
                if (customPrincipal == null || (customPrincipal != null && customPrincipal.UserCommon == null))
                {
                    var routeUrl = string.Format("~/{0}/{1}?returnUrl={2}", redirectControllerName, redirectActionName, System.Web.HttpContext.Current.Request.RawUrl);
                    filterContext.Result = new RedirectResult(routeUrl);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                var roles = Roles.Split(',');
                if (!roles.Any(x => filterContext.HttpContext.User.IsInRole(x.ToLower())))
                {
                    if (!filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.Result = new RedirectResult("~/PageInfo/Denied");
                    }
                    else
                    {
                        var urlHelper = new UrlHelper(filterContext.RequestContext);
                        var routeData = new RouteData();
                        routeData.Values.Add("controller", "PageInfo");
                        routeData.Values.Add("action", "Denied");
                        filterContext.Result = new JavaScriptResult
                        {
                            Script = "window.location = '" + urlHelper.RouteUrl(routeData) + "';"
                        };
                    }
                }
            }

        }

    }
}