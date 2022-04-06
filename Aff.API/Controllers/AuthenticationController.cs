using Aff.API.Repository;
using Aff.DataAccess;
using Aff.API.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Aff.API.Models;
using System.Web.Http.Cors;
using System.Net.Http;
using System.Configuration;

namespace Aff.API.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AuthenticationController : ApiController
    {
        private readonly IAuthentication _authentication;
        public AuthenticationController()
        {
            _authentication = new Authentication();
        }

        [HttpGet]
        public string Test()
        {
            return "Test";
        }

        [HttpPost]
        [AllowAnonymous]
        public HttpResponseMessage Login(UserModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.PassWord))
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                message.Content = new StringContent("Not Valid Request");
                return message;
            }
            return DoLogin(login);
        }

        private HttpResponseMessage DoLogin(UserModel userLogin)
        {
            var user = _authentication.ValidateRegisteredUser(userLogin);
            
            if (user != null)
            {
                var accountContext = new UserAccountModel() { UserId = user.UserId, UserName = user.UserName, Email = user.Email, FullName = user.FullName };
                SecurityProvider.StoreAccount(accountContext);
                HttpResponseMessage response = new HttpResponseMessage();
                response = Request.CreateResponse(HttpStatusCode.OK, "Authorized");
                response.Headers.Add("Token", accountContext.Token);
                response.Headers.Add("TokenExpiry", ConfigurationManager.AppSettings["TokenExpiry"]);
                response.Headers.Add("Access-Control-Expose-Headers", "Token,TokenExpiry");
                return response;
            }
            else
            {
                var message = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
                message.Content = new StringContent("Error in Creating Token");
                return message;
            }
        }
    }
}
