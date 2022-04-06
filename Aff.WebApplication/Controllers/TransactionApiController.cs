using Aff.DataAccess;
using Aff.DataAccess.Repositories;
using Aff.Models.Models;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Aff.WebApplication.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/Transaction")]
    public class TransactionApiController : ApiController
    {
        [HttpGet]
        [Route("TestInfo")]
        public string Test()
        {
            return "Test";
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Summary")]
        public IHttpActionResult GetSummary(TransactionRequestModel request)
        {
            if (request == null || (string.IsNullOrEmpty(request.FromDate) && string.IsNullOrEmpty(request.ToDate)))
            {
                return BadRequest("BadRequest.");
            }
            TimaAffiliateEntities dbContext = new TimaAffiliateEntities();
            var repo = new TransactionRepository(dbContext);
            CultureInfo vn = new CultureInfo("vi-VN");

            DateTime dtFrom = Convert.ToDateTime(request.FromDate, vn.DateTimeFormat);
            DateTime dtTo = Convert.ToDateTime(request.ToDate, vn.DateTimeFormat);
            var transaciton = repo.GetSummaryTransaction(dtFrom, dtTo);
            return Ok(transaciton);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SummaryDashBoard")]
        public IHttpActionResult GetDashBoard()
        {
            TimaAffiliateEntities dbContext = new TimaAffiliateEntities();
            var repo = new TransactionRepository(dbContext);
            var getDashBoard = repo.GetSummaryDashBoard();
            return Ok(getDashBoard);
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("CreateOrder")]
        public IHttpActionResult CreateOrder(TransactionModel model)
        {
            if (model == null)
            {
                return BadRequest("BadRequest.");
            }
            string message = "";
            try
            {
                using (TimaAffiliateEntities dbContext = new TimaAffiliateEntities())
                {
                    var repo = new LoanRepository(dbContext);
                    if (repo.CreateLoanCredit(model, out message))
                    {
                        return Ok();
                    }
                }

                return BadRequest(message);
            }
            catch (Exception ex)
            {
                return BadRequest("Cõ lỗi xảy ra, xin vui lòng liên hệ với ban quản trị.");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ApproveOrder")]
        public IHttpActionResult ApproveOrder(TransactionModel model)
        {
            if (model == null)
            {
                return BadRequest("BadRequest.");
            }
            string message = "";
            try
            {
                using (TimaAffiliateEntities dbContext = new TimaAffiliateEntities())
                {
                    var repo = new TransactionRepository(dbContext);
                    if (repo.CreateTransaction(model, out message))
                    {
                        return Ok();
                    }
                }

                return BadRequest(message);
            }
            catch (Exception ex)
            {
                return BadRequest("Cõ lỗi xảy ra, xin vui lòng liên hệ với ban quản trị.");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CreateUser")]
        public IHttpActionResult CreateUser(UserModel model)
        {
            if (model == null)
            {
                return BadRequest("BadRequest.");
            }
            string message = "";
            try
            {
                using (TimaAffiliateEntities dbContext = new TimaAffiliateEntities())
                {
                    // Add table User
                    model.Email = model.Email.Trim().ToLower();
                    var checkUser = dbContext.tblUsers.FirstOrDefault(c => c.Email == model.Email);
                    if (checkUser != null)
                        return Ok();

                    model.Password = Services.Security.SecurityUtil.EncryptText(model.Password);
                    model.CreatedDate = DateTime.Now.ToString();
                    model.IsActive = true;
                    model.RoleType = 3;
                    if (string.IsNullOrEmpty(model.UserName)) model.UserName = model.Email.Substring(0, model.Email.IndexOf('@'));

                    var user = new tblUser();
                    user.PassWord = model.Password;
                    user.UserName = model.UserName;
                    user.CreatedDate = DateTime.Now;
                    user.Email = model.Email;
                    user.FullName = model.FullName;
                    user.Phone = model.Phone;
                    user.IsActive = model.IsActive;
                    user.Address = model.Address;
                    user.Company = model.Company;
                    user.RoleType = model.RoleType;
                    user.UpdatedDate = model.UpdatedDate;

                    var userEntity = dbContext.tblUsers.Add(user);
                    dbContext.SaveChanges();
                    userEntity.AffCode = DataAccess.Common.AffCodeIdentifier.GenerateIdentifier(userEntity.UserId);
                    dbContext.SaveChanges();

                    if(!string.IsNullOrEmpty(model.AffCode))
                    {
                        var userReffer = dbContext.tblUsers.FirstOrDefault(c => c.AffCode == model.AffCode);
                        if(userReffer != null)
                        {
                            //Update owner reffer code
                            var userR1 = new tblUserReference
                            {
                                UserId = userEntity.UserId,
                                UserReferenceId = userReffer.UserId,
                                AffReferenceCode = userReffer.AffCode,
                                UserCode = userEntity.AffCode,
                                Level = 1
                            };
                            dbContext.tblUserReferences.Add(userR1);
                            dbContext.SaveChanges();
                            var userRefParents = dbContext.tblUserReferences.Where(u => u.UserId == userReffer.UserId).OrderBy(o => o.Level.Value).Take(3);
                            if(userRefParents != null && userRefParents.Any())
                            {
                                foreach (var uf in userRefParents)
                                {
                                    var userR2 = new tblUserReference
                                    {
                                        UserId = userEntity.UserId,
                                        UserReferenceId = uf.UserReferenceId,
                                        AffReferenceCode = uf.AffReferenceCode,
                                        UserCode = userEntity.AffCode,
                                        Level = uf.Level.Value + 1
                                    };
                                    dbContext.tblUserReferences.Add(userR1);
                                    dbContext.SaveChanges();
                                }
                            }    
                            
                        }    
                    }    
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Cõ lỗi xảy ra, xin vui lòng liên hệ với ban quản trị.");
            }
        }

        private void ErrorLog(string sPathName, string sErrMsg)
        {
            string sLogFormat = DateTime.Now.ToShortDateString().ToString() + " " + DateTime.Now.ToLongTimeString().ToString() + " ==> ";

            //for example filename : ErrorLogYYYYMMDD
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            string sErrorTime = sYear + sMonth + sDay;

            StreamWriter sw = new StreamWriter(sPathName + sErrorTime, true);
            sw.WriteLine(sLogFormat + sErrMsg);
            sw.Flush();
            sw.Close();
        }
    }
}
