using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Aff.WebApplication.Models;
using System.Web.Caching;
using Newtonsoft.Json;
using System.Web.Security;
using Aff.Services;
using Aff.Models.Models;
using Aff.WebApplication.Filters;
using Aff.Services.Services;
using Aff.WebApplication.Configurations;
using System.Collections.Generic;
using System.IO;

namespace Aff.WebApplication.Controllers
{

    [Authorize]
    public class AccountController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IUserService _userService;
        private readonly IContextService _contextService;
        private readonly IBankAccountService _bankAccountService;
        private readonly IPaymentService _paymentService;
        private readonly ITransactionService _transactionService;
        private readonly IMailService _mailService;
        private readonly IReportService _reportService;
        public AccountController(IUserService userService, IContextService contextService, IBankAccountService bankAccountService, IPaymentService paymentService, IMailService mailService, ITransactionService transactionService, IReportService reportService)
        {
            _userService = userService;
            _contextService = contextService;
            _bankAccountService = bankAccountService;
            _paymentService = paymentService;
            _transactionService = transactionService;
            _mailService = mailService;
            _reportService = reportService;
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserModel model, string returnUrl)
        {
            var msgError = string.Empty;
            if (ModelState.IsValid)
            {
                var user = _userService.ValidateLogon(model.Email, model.Password, out msgError);
                if (user.Status == Aff.Models.LoginResult.Success)
                {
                    var userData = JsonConvert.SerializeObject(user);
                    var authTicket = new FormsAuthenticationTicket(1,
                        user.Email,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(60),
                        false,
                        userData);
                    var encTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookieKey = FormsAuthentication.FormsCookieName;

                    _contextService.SaveInCookie(cookieKey, encTicket);
                    FormsAuthentication.RedirectFromLoginPage(model.Email, false);

                    if (encTicket != null)
                        HttpRuntime.Cache.Insert(cookieKey, userData, null, DateTime.Now.AddHours(30),
                            Cache.NoSlidingExpiration);

                    //if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Length > 1)
                    //    Response.Redirect(returnUrl);

                    return RedirectToAction("Index", "Admin");
                }
            }
            return Json(new
            {
                IsError = true,
                RedirectUrl = "",
                Message = msgError
            }, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [CustomAuthorize]
        public ActionResult LinkRefer()
        {

            var ReferralsDomain = Configurations.SystemConfiguration.GetStringKey("ReferralsDomain");
            var AffiliateDomain = Configurations.SystemConfiguration.GetStringKey("AffiliateDomain"); ;
            var BannerDomain = Configurations.SystemConfiguration.GetStringKey("BannerDomain"); ;
            UserModel _UserModel = new UserModel();

            //var userId = SignInManager.GetVerifiedUserId();
            ViewBag.BannerDomain = BannerDomain;
            ViewBag.ReferralsDomain = ReferralsDomain;
            ViewBag.AffiliateDomain = AffiliateDomain;
            ViewBag.AffCode = CurrentUser.AffCode;
            return View();
        }

        public ActionResult IndexUser()
        {
            int totalRecords;
            var model = new UserSearchIndexModel
            {
                Users = _userService.GetAllReference(SystemConfiguration.PageSizeDefault, 1, "", out totalRecords),
                PageIndex = 1,
                PageSize = SystemConfiguration.PageSizeDefault,
                TotalRecords = totalRecords,
            };
            return View(model);
        }
        public ActionResult UserTier()
        {
            int totalRecords;
            var model = new UserSearchIndexModel
            {
                Users = _userService.GetAllReference(SystemConfiguration.PageSizeDefault, 1, "", out totalRecords),
                PageIndex = 1,
                PageSize = SystemConfiguration.PageSizeDefault,
                TotalRecords = totalRecords,
            };
            ViewBag.TotalAmount = _transactionService.GetAll().Sum(c => c.TotalAmount);
            return View(model);
        }



        public ActionResult SearchAccount(int currentPage, string textSearch, string stringIndex)
        {
            int totalRecords;
            var model = new UserSearchIndexModel
            {
                Users = _userService.GetAllReference(SystemConfiguration.PageSizeDefault, currentPage, textSearch, out totalRecords),
                PageIndex = currentPage,
                PageSize = SystemConfiguration.PageSizeDefault,
                TotalRecords = totalRecords,
            };
            var html = "";
            if (stringIndex == "UserTier")
            {
                html = RenderPartialViewToString("~/Views/Account/_tableUserTierPartial.cshtml", model);
            }
            else
            {
                html = RenderPartialViewToString("~/Views/Account/_tableUserPartial.cshtml", model);

            }

            return Json(new
            {
                IsError = false,
                HTML = html
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetByAffCode(int userId, int level)
        {

            var model = _userService.GetByAffCode(userId, level);
            if (model != null)
            {
                return Json(new
                {
                    IsError = false,
                    data = model
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new
            {
                IsError = false,

            }, JsonRequestBehavior.AllowGet);

        }

        //
        // GET: /Account/UpdateUser
        [CustomAuthorize]
        public ActionResult UpdateUser()
        {
            var baccount = _bankAccountService.RetrieveBankAccountsByUser(CurrentUser.UserId).FirstOrDefault();
            var userModel = new UserModel
            {
                UserId = CurrentUser.UserId,
                Address = CurrentUser.Address,
                Phone = CurrentUser.Phone,
                FullName = CurrentUser.FullName,
                Company = CurrentUser.Company,
                BankAccount = baccount != null ? baccount.BankAccount : "",
                BankAddress = baccount != null ? baccount.BankAddress : "",
                BankName = baccount != null ? baccount.BankName : "",
                BankOwnerName = baccount != null ? baccount.BankOwnerName : ""
            };
            ViewBag.IsSuccess = true;
            ViewBag.Message = "";
            return View(userModel);

        }
        // POST: /Account/UpdateUser
        [HttpPost]
        [CustomAuthorize]
        public ActionResult UpdateUser(UserModel userModel)
        {
            string message = "";
            bool isSuccess = false;
            userModel.UserId = CurrentUser.UserId;
            isSuccess = _userService.UpdateUser(userModel, out message);
            _bankAccountService.UpdateBankAccount(userModel, out message);
            ViewBag.IsSuccess = isSuccess;
            ViewBag.Message = message;
            return View(userModel);
            //return RedirectToAction("Index", "Admin");
        }
        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserModel viewModel)
        {
            if (!ModelState.IsValid) return View(viewModel);
            string errorMsg;
            string upassword = viewModel.Password;
            var isRequireAffCode = SystemConfiguration.GetStringKey("RequireAffCode") == "1";
            var isSucess = _userService.CreateUser(viewModel, out errorMsg, isRequireAffCode);
            ViewBag.IsSuccess = isSucess;
            TempData["Email"] = viewModel.Email;

            if (isSucess)
            {
                var user = _userService.ValidateLogon(viewModel.Email, upassword, out errorMsg);
                if (user.Status == Aff.Models.LoginResult.Success)
                {
                    var userData = JsonConvert.SerializeObject(user);
                    var authTicket = new FormsAuthenticationTicket(1,
                        user.Email,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(60),
                        false,
                        userData);
                    var encTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookieKey = FormsAuthentication.FormsCookieName;
                    _contextService.SaveInCookie(cookieKey, encTicket);
                    FormsAuthentication.RedirectFromLoginPage(viewModel.Email, false);

                    if (encTicket != null)
                        HttpRuntime.Cache.Insert(cookieKey, userData, null, DateTime.Now.AddHours(30),
                            Cache.NoSlidingExpiration);

                    return RedirectToAction("Index", "Admin");
                }
            }
            return Json(new
            {
                IsError = true,
                RedirectUrl = "",
                Message = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize]
        public ActionResult RemoteLoginAdmin(int userid)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Login", "Account");
            }
            string errorMsg = "";
            var eUser = _userService.GetById(userid);

            if (eUser != null)
            {
                ViewBag.IsSuccess = true;
                TempData["Email"] = eUser.Email;
                var user = _userService.ValidateLogon(eUser.Email, eUser.PassWord, out errorMsg, true);
                if (user.Status == Aff.Models.LoginResult.Success)
                {
                    var userData = JsonConvert.SerializeObject(user);
                    var authTicket = new FormsAuthenticationTicket(1,
                        user.Email,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(60),
                        false,
                        userData);
                    var encTicket = FormsAuthentication.Encrypt(authTicket);
                    var cookieKey = FormsAuthentication.FormsCookieName;
                    _contextService.SaveInCookie(cookieKey, encTicket);
                    //FormsAuthentication.RedirectFromLoginPage(eUser.Email, false);

                    if (encTicket != null)
                        HttpRuntime.Cache.Insert(cookieKey, userData, null, DateTime.Now.AddHours(30),
                            Cache.NoSlidingExpiration);

                    return Json(new
                    {
                        IsError = false,
                        RedirectUrl = "",
                        Message = ""
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new
            {
                IsError = true,
                RedirectUrl = "",
                Message = errorMsg
            }, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize]
        public ActionResult SearchUserRegister(UserSearchModel userModel)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            int totalRecords = 0;
            userModel.RequestParam(Request.Params);
            userModel.PageSize = userModel.PageSize > 100 ? userModel.PageSize = 30 : userModel.PageSize;
            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;
            if (!string.IsNullOrEmpty(Request.Params["datatable[query][Range]"]) && Request.Params["datatable[query][Range]"].IndexOf("/") > 0)
            {
                var dates = Request.Params["datatable[query][Range]"].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (dates.Length == 2)
                {
                    //DateTime.TryParse(dates[0].Trim(), out fromDate);
                    //DateTime.TryParse(dates[1].Trim(), out toDate);
                    CultureInfo vn = new CultureInfo("vi-VN");

                    fromDate = Convert.ToDateTime(dates[0].Trim(), vn.DateTimeFormat);
                    toDate = Convert.ToDateTime(dates[1].Trim(), vn.DateTimeFormat);
                }
            }

            var users = _userService.SearchUser(userModel.TextSearch, userModel.PageIndex, userModel.PageSize, userModel.Field, userModel.Sort, out totalRecords, fromDate, toDate);
            return Json(new
            {
                meta = new MetaData
                {
                    field = userModel.Field,
                    page = userModel.PageIndex,
                    pages = userModel.Pages,
                    perpage = userModel.PageSize,
                    total = totalRecords,
                    sort = userModel.Sort
                },
                data = users
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }
        public ActionResult GetAllUserFisrt()
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!string.IsNullOrEmpty(Request.Params["datatable[query][Range]"]) && Request.Params["datatable[query][Range]"].IndexOf("/") > 0)
            {
                var dates = Request.Params["datatable[query][Range]"].Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (dates.Length == 2)
                {
                    CultureInfo vn = new CultureInfo("vi-VN");
                    fromDate = Convert.ToDateTime(dates[0].Trim(), vn.DateTimeFormat);
                    toDate = Convert.ToDateTime(dates[1].Trim(), vn.DateTimeFormat);
                }
            }
            var model = _userService.GetAllUserFirst("",fromDate: fromDate, toDate: toDate);
            return Json(new
            {
                meta = new MetaData
                {
                    field = "",
                    page = 1,
                    pages = 1,
                    perpage = model.Count(),
                    total = model.Count(),
                    sort = ""
                },
                data = model
            }, JsonRequestBehavior.AllowGet);

        }
        [CustomAuthorize]
        public ActionResult SearchUserRefferenceAvailable(UserSearchModel userModel)
        {
            int totalRecords = 0;
            userModel.RequestParam(Request.Params);
            userModel.PageSize = userModel.PageSize > 100 ? userModel.PageSize = 30 : userModel.PageSize;
            var users = _userService.SearchUserAff(CurrentUser.UserId, userModel.TextSearch, userModel.PageIndex, userModel.PageSize, userModel.Field, userModel.Sort, out totalRecords);
            return Json(new
            {
                meta = new MetaData
                {
                    field = userModel.Field,
                    page = userModel.PageIndex,
                    pages = userModel.Pages,
                    perpage = userModel.PageSize,
                    total = totalRecords,
                    sort = userModel.Sort
                },
                data = users
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }
        [CustomAuthorize]
        public ActionResult SearchUserReff(UserCommon userModel)
        {
            return View();
        }

        [CustomAuthorize]
        public ActionResult SearchUserAvailableBalance(UserSearchModel userModel)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            int totalRecords = 0;
            userModel.RequestParam(Request.Params);
            int userStatus = 0;
            if (!int.TryParse(Request.Params["datatable[query][Type]"], out userStatus))
            {
                userStatus = 1;
            }
            var users = _userService.SearchUserAvaiableBalance(userStatus, userModel.TextSearch, 1, 10000000, userModel.Field, userModel.Sort, out totalRecords);
            Session["UserAvailableBalance"] = users;
            return Json(new
            {
                meta = new MetaData
                {
                    field = userModel.Field,
                    page = userModel.PageIndex,
                    pages = userModel.Pages,
                    perpage = userModel.PageSize,
                    total = totalRecords,
                    sort = userModel.Sort
                },
                data = users
            }, JsonRequestBehavior.AllowGet);
            //return View(searchModel);
        }

        public ActionResult ExportUserToExcel()
        {
            try
            {
                var template = Server.MapPath("/Upload/ck-tpl.xlsx");
                var exportDir = Server.MapPath("/App_Data");
                string fileName = string.Empty;
                string filePath = string.Empty;
                var userAvailableBalance = this.Session["UserAvailableBalance"] as List<UserModel>;
                if (CurrentUser != null && userAvailableBalance != null)
                {
                    filePath = _userService.ExportOrderHistoryToExcel(template, exportDir, userAvailableBalance, out fileName);
                }

                return File(filePath, "application/vnd.ms-excel", fileName);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public ActionResult ExportBangKeDoiSoatToExcel()
        {
            try
            {
                var template = Server.MapPath("/Upload/bangke_doisoat-tpl.xlsx");
                var exportDir = Server.MapPath("/App_Data");
                string fileName = string.Empty;
                string filePath = string.Empty;
                var userAvailableBalance = this.Session["UserAvailableBalance"] as List<UserModel>;
                if (CurrentUser != null && userAvailableBalance != null)
                {
                    filePath = _userService.ExportToExcel_BangKeDoiSoat(template, exportDir, userAvailableBalance, out fileName);
                }

                return File(filePath, "application/vnd.ms-excel", fileName);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [CustomAuthorize]
        public ActionResult SearchUser(UserCommon userModel)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            ViewBag.DateRangePicker = string.Format("{0} / {1}", DateTime.Now.ToString("dd-MM-yyyy"), DateTime.Now.ToString("dd-MM-yyyy"));
            return View();
        }

        [CustomAuthorize]
        public ActionResult ProcessMatchingUser(UserMatchinhchModel userModel, string monthOfYear, string generalSearch)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2) || string.IsNullOrEmpty(monthOfYear))
            {
                return RedirectToAction("Index", "Admin");
            }
            try
            {
                int totalRecords = 0;
                //UserMatchinhchModel userModel = new UserMatchinhchModel();
                userModel.RequestParam(Request.Params);
                //userModel.TextSearch = generalSearch;
                monthOfYear = monthOfYear.Replace(" ", "");
                var users = _userService.GetUserMatching(userModel.TextSearch, userModel.Field, userModel.Sort, monthOfYear, out totalRecords);
                _userService.UpdateMatchingAmount(users, monthOfYear);
                return Json(new
                {
                    IsError = false,
                    RedirectUrl = "",
                    Message = ""
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsError = true,
                    RedirectUrl = "",
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [CustomAuthorize]
        public ActionResult CreateNewPayment(int currentUserId, string generalSearch = "")
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            try
            {
                if (currentUserId > 0)
                {
                    var user = _userService.RetrieveUser(currentUserId);
                    CreateNewPayment(user, user.AvailableBalance);
                }
                //Update all
                if (currentUserId == 0)
                {
                    int totalPage = 0;
                    var userSearchs = _userService.SearchUserAvaiableBalance(1, generalSearch, 1, 10000000, "UserId", "asc", out totalPage);
                    foreach (var uItem in userSearchs)
                    {
                        CreateNewPayment(uItem, uItem.AvailableBalance);
                    }
                }

                return Json(new
                {
                    IsError = false,
                    RedirectUrl = "",
                    Message = ""
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    IsError = true,
                    RedirectUrl = "",
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }

        }

        private bool CreateNewPayment(UserModel user, long amountEarning)
        {
            if (user == null || amountEarning < 50000) return false;
            long userAmountEarned = (amountEarning * 9 / 10);
            string message = "";
            bool isSuccess = false;
            try
            {
                PaymentModel pm = new PaymentModel
                {
                    AmountEarning = userAmountEarned,
                    PaymentStatus = 3,
                    PaymentMethod = 1,
                    UserId = user.UserId,
                    VerifyStatus = 1,
                    CreatedDate = DateTime.Now.ToString(),
                    UpdatedDate = DateTime.Now.ToString(),
                    PaidDate = DateTime.Now.ToString(),
                    AccountInfo = string.Format("Ngân hàng: {0} - Số TK: {1} - Tên TK: {2} - Chi nhánh: {3}.", user.BankName, user.BankAccount, user.BankOwnerName, user.BankAddress)
                };

                isSuccess = _paymentService.CreatePayment(pm);
                if (isSuccess)
                {
                    //Update user
                    user.TotalAmountEarning = user.TotalAmountEarning + userAmountEarned;
                    user.AvailableBalance = user.AvailableBalance - amountEarning;
                    user.EndPaymentDate = DateTime.Now;
                    isSuccess = _userService.UpdateUser(user, out message);
                    if (isSuccess)
                    {
                        //send email
                        string emailTemplate = @"<div style='font-family:Arial'>
                                                <div style='margin:0px;padding:10px;background:#cccccc;font-family:Arial;font-size:12px'>
                                                    <div style='border-radius:6px;width:598px;margin:0px auto;padding:15px 0px;background:#ffffff;color:#4d4d4d'>
                                                        <div style='width:546px;margin:0px auto;border:4px solid #007cc2'>
                                                            <div style='padding:10px 20px'>
                                                                <p style='padding:4px 0px 10px 0px;margin:0px;font-family:Arial'>Kính chào <b>{0},</b></p>
                                                                <div style='margin-bottom:0px'>
                                                                    <p style='font-family:Arial'>JobNow đã thanh toán tiền hoa hồng tháng {6}: {1} vnđ vào tài khoản của bạn.</p>
                                                                </div>
                                                                <div style='font-weight:bold;margin-bottom:2px;font-size:11px'>
                                                                    
                                                                    <p style='font-family:Arial'>Thông tin chuyển khoản:</p>
                                                                    <ul style='font-family:Arial'>
                                                                        <li>Số tiền đã chuyển: {1}</li>
                                                                        <li>Tài khoản hưởng: {2}</li>
                                                                        <li>Tên tài khoản: {3}</li>
                                                                        <li>Ngân hàng: {4}</li>
							                                            <li>Chi nhánh: {5}</li>
                                                                    </ul>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div style='color:#555555;width:554px;margin:5px auto;padding:5px 0px'>
                                                            <div style='padding-top:15px;font-family:Arial' align='center'><b>Cảm ơn bạn đã tham gia vào hệ thống của chúng tôi.</b></div>
                                                        </div>
                                                    </div>
        
	                                            </div>
                                            </div>";
                        var title = "Thông tin thanh toán trên website aff.jobnow.com.vn";
                        string strAmountEarned = string.Format("{0:0,0}", userAmountEarned);
                        var bodyContent = string.Format(emailTemplate, user.Email, strAmountEarned.Replace(",", "."), user.BankAccount, user.BankOwnerName, user.BankName, user.BankAddress, DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0'));
                        _mailService.SendMail(user.Email, title, bodyContent);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return isSuccess;
        }

        [CustomAuthorize]
        public ActionResult MatchingUserRegister(UserMatchinhchModel userModel, string monthOfYear, string generalSearch)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            int totalRecords = 0;
            userModel.RequestParam(Request.Params);
            //if(string.IsNullOrEmpty(userModel.TextSearch))
            //    userModel.TextSearch = generalSearch;
            if (!string.IsNullOrEmpty(monthOfYear)) userModel.DataOfMonth = monthOfYear.Replace(" ", "");
            var users = _userService.GetUserMatching(userModel.TextSearch, userModel.Field, userModel.Sort, userModel.DataOfMonth, out totalRecords);
            return Json(new
            {
                meta = new MetaData
                {
                    field = userModel.Field,
                    page = 1,
                    pages = 0,
                    perpage = 1000000,
                    total = totalRecords,
                    sort = userModel.Sort
                },
                data = users
            }, JsonRequestBehavior.AllowGet);
        }
        [CustomAuthorize]
        public ActionResult MatchingUser(UserMatchinhchModel userModel)
        {
            if (!CurrentUser.RoleType.HasValue || (CurrentUser.RoleType.HasValue && CurrentUser.RoleType.Value > 2))
            {
                return RedirectToAction("Index", "Admin");
            }
            userModel.RequestParam(Request.Params);
            if (string.IsNullOrEmpty(userModel.DataOfMonth))
            {
                if (DateTime.Now.Day < 12)
                {
                    userModel.DataOfMonth = string.Format("{0}-{1}", (DateTime.Now.Month - 1).ToString().PadLeft(2, '0'), DateTime.Now.Year);
                }
                if (DateTime.Now.Day >= 12)
                {
                    userModel.DataOfMonth = string.Format("{0}-{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year);
                }
            }
            ViewBag.IsSuccess = true;
            ViewBag.Message = "";
            return View(userModel);
        }

        public ActionResult ExportMatchingUser(int userId)
        {
            var modelUser = _userService.GetById(userId).MapToModel();
            var modelReport = _reportService.GetRevenue(userId);
            if (modelReport != null)
            {
                modelUser.AvailableBalance = modelReport.IndirectRevenue.Sum(c => c.Amount) + modelReport.DirectRevenue.Sum(c => c.Amount);
            };
            var model = new ReportModelExport
            {
                ReportModels = modelReport,
                UserModel = modelUser
            };

            if (model != null)
            {
                return View(model);
            }
            return View(new TransactionModel());
        }


        public ActionResult ParadigmUser()
        {
            
            return View();
        }
        
        public ActionResult UserDetail(int userId)
        {
            //get level
            var model = _userService.GetDetail(userId);

            if (model != null)
            {
                return View(model);
            }
            return View(new UserDetail());
        }
        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPassword
        //[CustomAuthorize]
        [AllowAnonymous]
        public ActionResult ChangePassword()
        {
            ViewBag.IsSuccess = true;
            ViewBag.Message = "";
            return View(new UserChangePasswordModel());
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(UserChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                string message = "";
                bool isSuccess = false;
                if (model.CurrentPassword == model.NewPassword)
                {
                    message = "Mật khẩu mới không được trùng với mật khẩu cũ";
                }
                else
                    isSuccess = _userService.ChangePassword(CurrentUser.UserId, model.CurrentPassword, model.NewPassword, out message);
                ViewBag.IsSuccess = isSuccess;
                ViewBag.Message = message;
            }
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        ////
        //// GET: /Account/ResetPassword
        //[AllowAnonymous]
        //public ActionResult ResetPassword(string code)
        //{
        //    return code == null ? View("Error") : View();
        //}

        //

        [AllowAnonymous]
        public ActionResult ResetPassword(string email)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    IsError = true,
                    RedirectUrl = "",
                    Message = ""
                }, JsonRequestBehavior.AllowGet);
            }
            string message = "";
            string newPass = Guid.NewGuid().ToString("n").Replace("-", "").Substring(0, 6);
            var response = _userService.ResetPassword(email, newPass, out message);
            if (response)
            {
                string emailTemplate = @"<div style='font-family:Arial'>
	                                    <div style='margin:0px;padding:10px;background:#cccccc;font-family:Arial;font-size:12px'>
		                                    <div style='border-radius:6px;width:598px;margin:0px auto;padding:15px 0px;background:#ffffff;color:#4d4d4d'>
			                                    <div style='width:546px;margin:0px auto;border:4px solid #007cc2'>
				                                    <div style='padding:10px 20px'>
					                                    <p style='padding:4px 0px 10px 0px;margin:0px;font-family:Arial'>Kính chào <b>{0},</b></p>
					                                    <div style='margin-bottom:0px'>
						                                    <p style='font-family:Arial'>JobNow đã reset lại mật khẩu của bạn trên website <a href='https://aff.jobnow.com.vn' target='_blank'>aff.jobnow.com.vn</a></p>
						                                    <p style='font-family:Arial'>Mật khẩu mới của bạn là: <b>{1}</b></p>
						                                    <p style='font-family:Arial'>Xin vui lòng đăng nhập vào hệ thống bằng mật khẩu này và cập nhật lại mật khẩu mới của bạn!</p>
					                                    </div>
					                                    <div style='font-weight:bold;margin-bottom:2px;font-size:11px'>
						                                    
					                                    </div>
				                                    </div>
			                                    </div>
			                                    <div style='color:#555555;width:554px;margin:5px auto;padding:5px 0px'>
				                                    <div style='padding-top:15px;font-family:Arial' align='center'><b>Cảm ơn bạn đã tham gia vào hệ thống của chúng tôi.</b></div>
			                                    </div>
		                                    </div>
	                                    </div>
                                    </div>";
                var bodyContent = string.Format(emailTemplate, email, newPass);
                response = _mailService.SendMail(email, "[aff.jobnow.com.vn] Reset mật khẩu ", bodyContent);
            }

            return Json(new
            {
                IsError = response,
                RedirectUrl = "",
                Message = message
            }, JsonRequestBehavior.AllowGet);
        }
        //

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion


        #region Export Exel
        [HttpGet]
        public void ExportTimeSheetDaily()
        {
            // Gọi lại hàm để tạo file excel                      
            Stream stream = _userService.CreateExcelFile();

            // Tạo buffer memory stream để hứng file excel
            var buffer = stream as MemoryStream;
            Response.Clear();
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AddHeader("content-disposition", "attachment; filename = BaoCaoDiemDanh" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
            Response.ContentType = "application/text";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Response.BinaryWrite(buffer.ToArray());
            Response.End();
        }

        [HttpGet]
        public void ExportUser(string textSearch)
        {
            // Gọi lại hàm để tạo file excel                      
            Stream stream = _userService.CreateExcelUserFile(textSearch);

            // Tạo buffer memory stream để hứng file excel
            var buffer = stream as MemoryStream;
            Response.Clear();
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AddHeader("content-disposition", "attachment; filename = BaoCaoTaiKhoan" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
            Response.ContentType = "application/text";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Response.BinaryWrite(buffer.ToArray());
            Response.End();
        }

        [HttpGet]
        public void ExportUserTier(string stringSearch, string dateTime)
        {
            DateTime? fromDate = null;
            DateTime? toDate = null;
            if (!string.IsNullOrEmpty(dateTime) && dateTime.IndexOf("/") > 0)
            {
                var dates = dateTime.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (dates.Length == 2)
                {
                    CultureInfo vn = new CultureInfo("vi-VN");
                    fromDate = Convert.ToDateTime(dates[0].Trim(), vn.DateTimeFormat);
                    toDate = Convert.ToDateTime(dates[1].Trim(), vn.DateTimeFormat);
                }
            }
            // Gọi lại hàm để tạo file excel                      
            Stream stream = _userService.CreateExcelUserTierFile(fromDate,toDate, stringSearch);


            // Tạo buffer memory stream để hứng file excel
            var buffer = stream as MemoryStream;
            Response.Clear();
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AddHeader("content-disposition", "attachment; filename = BaoCaoTaiKhoanDoanhThu" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
            Response.ContentType = "application/text";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Response.BinaryWrite(buffer.ToArray());
            Response.End();
        }

        [HttpGet]
        public void ExportMatchingUserExel(int userId)
        {
            // Gọi lại hàm để tạo file excel                      
            Stream stream = _reportService.CreateExcelUserMattchingFile(userId);

            // Tạo buffer memory stream để hứng file excel
            var buffer = stream as MemoryStream;
            Response.Clear();
            Response.Charset = System.Text.UTF8Encoding.UTF8.WebName;
            Response.ContentEncoding = System.Text.UTF8Encoding.UTF8;
            Response.AddHeader("content-disposition", "attachment; filename = BaoCaoDoiSoat" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx");
            Response.ContentType = "application/text";
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("utf-8");
            Response.BinaryWrite(buffer.ToArray());
            Response.End();
        }

        #endregion

    }
}