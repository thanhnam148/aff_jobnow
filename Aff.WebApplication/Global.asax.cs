using Aff.WebApplication.Dependency;
using Aff.WebApplication.Security;
using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Aff.WebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            System.Web.Helpers.AntiForgeryConfig.SuppressXFrameOptionsHeader = true;
            RegisterAutoFac();
        }

        private static void RegisterAutoFac()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new RepositoryModule());
            builder.RegisterModule(new ServiceModule());
            builder.RegisterModule(new EFModule());

            // Register your MVC controllers.
            var callingAssembly = Assembly.GetExecutingAssembly();
            builder.RegisterControllers(callingAssembly);

            // Set the dependency resolver to be Autofac.
            var container = builder.Build();
            var resolver = new AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(resolver);
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                var customPrincipal = HttpContext.Current.User.Identity as ClientSystemIdentity;
                if(customPrincipal == null)
                {
                    var identity = new ClientSystemIdentity(HttpContext.Current.User.Identity);
                    var principal = new ClientSystemPrincipal(identity);
                    HttpContext.Current.User = principal;
                }
            }
        }

        void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (Context.User == null)
            {
                var oldTicket = ExtractTicketFromCookie(Context, FormsAuthentication.FormsCookieName);
                if (oldTicket != null && !oldTicket.Expired)
                {
                    var ticket = oldTicket;
                    if (FormsAuthentication.SlidingExpiration)
                    {
                        ticket = FormsAuthentication.RenewTicketIfOld(oldTicket);
                        if (ticket == null)
                            return;
                    }

                    Context.User = new GenericPrincipal(new FormsIdentity(ticket), new string[0]);
                    if (ticket == oldTicket)
                        return;

                    // update the cookie since we've refreshed the ticket
                    var cookieValue = FormsAuthentication.Encrypt(ticket);
                    var cookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName] ??
                                 new HttpCookie(FormsAuthentication.FormsCookieName, cookieValue) { Path = ticket.CookiePath };

                    if (ticket.IsPersistent)
                        cookie.Expires = ticket.Expiration;
                    cookie.Value = cookieValue;
                    cookie.Secure = FormsAuthentication.RequireSSL;
                    cookie.HttpOnly = true;
                    if (FormsAuthentication.CookieDomain != null)
                        cookie.Domain = FormsAuthentication.CookieDomain;
                    Context.Response.Cookies.Remove(cookie.Name);
                    Context.Response.Cookies.Add(cookie);
                }
            }
        }

        private static FormsAuthenticationTicket ExtractTicketFromCookie(HttpContext context, string name)
        {
            FormsAuthenticationTicket ticket = null;
            string encryptedTicket = null;

            var cookie = context.Request.Cookies[name];
            if (cookie != null)
            {
                encryptedTicket = cookie.Value;
            }

            if (string.IsNullOrEmpty(encryptedTicket))
                return null;

            try
            {
                ticket = FormsAuthentication.Decrypt(encryptedTicket);
            }
            catch
            {
                context.Request.Cookies.Remove(name);
            }

            if (ticket != null && !ticket.Expired)
            {
                return ticket;
            }

            // if the ticket is expired then remove it
            context.Request.Cookies.Remove(name);
            return null;
        }
    }
}
