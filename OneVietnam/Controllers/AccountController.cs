using System.Globalization;
using OneVietnam.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Facebook;
using Microsoft.AspNet.SignalR;
using MongoDB.Bson.IO;
using System.Web.Script.Serialization;
using OneVietnam.BLL;
using OneVietnam.DTL;
using System.IO;
using System.Drawing;
using OneVietnam.Common;
using SignInStatus = OneVietnam.Common.SignInStatus;
using System.Collections;

namespace OneVietnam.Controllers
{
    [System.Web.Mvc.Authorize]
    public class AccountController : Controller
    {
        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        private ApplicationUserManager _userManager;
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
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        private SignInHelper _helper;

        private SignInHelper SignInHelper
        {
            get
            {
                if (_helper == null)
                {
                    _helper = new SignInHelper(UserManager, AuthenticationManager);
                }
                return _helper;
            }
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }         
            // This doen't count login failures towards lockout only two factor authentication
            // To enable password failures to trigger lockout, change to shouldLockout: true
            var result = await SignInHelper.PasswordSignIn(model.Email.ToLower(), model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:                    
                    return RedirectToLocal(returnUrl);
                case SignInStatus.Locked:
                    ViewBag.errorMessage = "Chúng tôi xin lỗi, Vì một số lý do chúng tôi phải khóa tài khoản của bạn. " +
                                           "Để khắc phục vấn đề mời bạn gửi email đến ban quan trị theo địa chỉ onevietnamteam@gmail.com";
                    return View("Error");
                case SignInStatus.LockedOut:
                    ViewBag.LockedDuration =
                        ConfigurationManager.AppSettings["DefaultAccountLockoutTimeSpan"].ToString();
                    return View("Lockout");
                case SignInStatus.RequiresConfirmingEmail:
                    ViewBag.errorMessage = "Bản Phải Xác Nhận Mail Trước Khi Đăng Nhập";
                    return View("Error");
                case SignInStatus.RequiresTwoFactorAuthentication:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Thông Tin Đăng Nhập Không Hợp Lệ");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> _VerifyCode(string provider)
        {
            if (!await SignInHelper.SendTwoFactorCode(provider))
            {
                return View("Error");
            }
            // Require that the user has already logged in via username/password or external login
            if (!await SignInHelper.HasBeenVerified())
            {
                return View("Error");
            }
            var user = await UserManager.FindByIdAsync(await SignInHelper.GetVerifiedUserIdAsync());
            if (user != null)
            {
            }
            return PartialView();
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInHelper.TwoFactorSignIn(model.Provider, model.Code, isPersistent: false, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Mã xác nhận không đúng");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var location = new Location
                {
                    XCoordinate = model.XCoordinate,
                    YCoordinate = model.YCoordinate,
                    Address = model.Address
                };

                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email.ToLower(),
                    Gender = model.Gender,Location = location, CreatedDate = DateTimeOffset.UtcNow,
                    Avatar = Constants.DefaultAvatarLink,Cover = Constants.DefaultCoverLink };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                       new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id,
                       "Chào mừng bạn đến với mạng xã hội OneVietnam.", "Mời bạn xác nhận tài khoản bằng cách click vào <a href=\""
                       + callbackUrl + "\">đây</a>");
                    ViewBag.Message = "Click vào đường link xác nhận tài khoản qua Email được gửi vào hòm thư của bạn để tiếp tục";
                    return View("Info");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
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
            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            AddErrors(result);
            return View("Error");
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
                var user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Mời bạn click vào: <a href=\"" + callbackUrl + "\">link</a> để đặt lại mật khẩu");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        private async Task<string> SendEmailConfirmationTokenAsync(string userID, string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account",
               new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(userID, subject,
               "Hãy xác nhận tài khoản của bạn bằng cách click vào<a href=\"" + callbackUrl + "\">đây</a>");

            return callbackUrl;
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
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
        public ActionResult ExternalLogin(string provider, string returnUrl, string locationExternal, double xCoordinateExternal, double yCoordinateExternal)
        {                           
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", 
                new { ReturnUrl = returnUrl, locationExternal= locationExternal, xCoordinateExternal= xCoordinateExternal,
                    yCoordinateExternal = yCoordinateExternal }));
        }

        //
        // GET: /Account/SendCode

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl)
        {
            var userId = await SignInHelper.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl });
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl, string locationExternal, double xCoordinateExternal, double yCoordinateExternal)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInHelper.ExternalSignIn(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:                    
                    return RedirectToLocal(returnUrl);
                case SignInStatus.Locked:
                    ViewBag.errorMessage = "Chúng tôi xin lỗi, Vì một số lý do chúng tôi phải khóa tài khoản của bạn. " +
                                           "Để khắc phục vấn đề mời bạn gửi email đến ban quan trị theo địa chỉ onevietnamteam@gmail.com";
                    return View("Error");
                case SignInStatus.LockedOut:
                    ViewBag.LockedDuration =
                        ConfigurationManager.AppSettings["DefaultAccountLockoutTimeSpan"].ToString();
                    return View("Lockout");
                case SignInStatus.RequiresTwoFactorAuthentication:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    //Get ExternalCookie
                    var claimsIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);

                    if (User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    // if there are no errores
                    if (ModelState.IsValid)
                    {
                        // Get the information about the user from the external login provider
                        var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                        if (info == null)
                        {
                            return View("ExternalLoginFailure");
                        }
                        var access_token = claimsIdentity.FindAll("FacebookAccessToken").First().Value;
                        ViewBag.Accesstoken = access_token;
                        var fb = new FacebookClient(access_token);                        
                        dynamic userInfo = fb.Get("me?fields=name,email,gender,picture.width(800)");                        
                        string name = userInfo["name"];
                        string email = userInfo["email"];
                        string fbGender = userInfo["gender"];
                        string avatar = userInfo["picture"]["data"]["url"];
                        var gender=(int)Gender.Other;
                        Gender choice;
                        if (Enum.TryParse(fbGender, out choice))
                        {
                            gender = (int) choice;
                        }                        
                        var location = new Location(xCoordinateExternal, yCoordinateExternal, locationExternal);
                        var user = new ApplicationUser { UserName = name, Email = email,Location = location,Gender = gender,Avatar = avatar,
                            CreatedDate = DateTimeOffset.UtcNow,Cover =Constants.DefaultCoverLink };
                        //Add to database
                        var result2 = await UserManager.CreateAsync(user);
                        if (result2.Succeeded)
                        {
                            result2 = await UserManager.SetEmailConfirmed(user.Id);
                            if (result2.Succeeded)
                            {
                                result2 = await UserManager.AddLoginAsync(user.Id, info.Login);
                                if (result2.Succeeded)
                                {
                                    await StoreFacebookAuthToken(user);
                                    await SignInHelper.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                                    return RedirectToLocal(returnUrl);
                                }
                            }                                                        
                        }
                        AddErrors(result2);
                    }

                    ViewBag.ReturnUrl = returnUrl;

                    return RedirectToAction("Index", "Home");
            }
        }
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {            
            //IHubContext hub = GlobalHost.ConnectionManager.GetHubContext<OneHub>();
            //hub.Clients.User(User.Identity.GetUserId()).logOff();
            AuthenticationManager.SignOut();                               
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        private async Task StoreFacebookAuthToken(ApplicationUser user)
        {
            //Get the claims from the cookie
            var claimsIdentity = await AuthenticationManager.GetExternalIdentityAsync(DefaultAuthenticationTypes.ExternalCookie);
            if (claimsIdentity != null)
            {
                // Retrieve the existing claims for the user and add the FacebookAccessTokenClaim
                var currentClaims = await UserManager.GetClaimsAsync(user.Id);
                var facebookTokenFromDb = currentClaims.FirstOrDefault(o => o.Type == "FacebookAccessToken");

                //Search the claims (from the cookie) for the facebook access token
                var facebookAccessToken = claimsIdentity.FindAll("FacebookAccessToken").First();

                //If its not in the db, store it.
                if (facebookTokenFromDb != null)
                {
                    //It is in the db, so see if the stored token matches the new token.
                    //If the user has for ex removed the facebook app (ie they logged in to facebook and
                    //removed this app from their settings), we need to save a new token since 
                    //they have reauthenticated.
                    if (facebookTokenFromDb.Value != facebookAccessToken.Value)
                    {
                        await UserManager.RemoveClaimAsync(user.Id, facebookTokenFromDb);
                        await UserManager.AddClaimAsync(user.Id, facebookAccessToken);
                    }
                }
                else
                {
                    //There is no access token stored in the db, add it.
                    await UserManager.AddClaimAsync(user.Id, facebookAccessToken);
                }

            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

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

        #region AutoGenerateUsers
        // Define location
        private int MinLat = 30;
        private int MaxLat = 45;
        private int MinLong = 130;
        private int MaxLong = 149;
        private readonly ArrayList _arrRawLocations = new ArrayList();
        private readonly String[] _strRawLocations =
        {
           "Takakuko, Nasu, Nasu District, Tochigi Prefecture 325-0001, Nhật Bản",
            "Shinjuku, Tokyo, Nhật Bản",
            "Hanshin Expressway Route 1 Loop Route, Nhật Bản",
            "975-1 Oshibedanicho Komi, Nishi Ward, Kobe, Hyogo Prefecture 651-2223, Nhật Bản",
            "5 Chome-3 Ichinotanichō, Suma-ku, Kōbe-shi, Hyōgo-ken 654-0076, Nhật Bản"
        };
        public async Task<string> AutoGenerateUsers()
        {
            // initial data
            InitialData();

            // define the number of user
            var numberOfUsers = 5000;

            // Create ViewModel
            var model = new RegisterViewModel();

            var r = new Random();
            while (numberOfUsers != 0)
            {
                PrepareModel(model, numberOfUsers, r);
                await CreateOneUser(model);
                numberOfUsers--;
            }
            // If we got this far, something failed, redisplay form
            return "create done";
        }

        private void PrepareModel(RegisterViewModel model, int index, Random r)
        {
            #region fake location

            // create a random number betwwen 0 and  2147483647
            var rIndex = r.Next(0, int.MaxValue);

            // reduce the number in ranage 0 and the count of _arrRawLocations
            rIndex = rIndex % _arrRawLocations.Count;
            var strAddress = _arrRawLocations[rIndex].ToString();

            // Declare new location
            var postLocation = new Location();

            // get the range of Lat
            var range = MaxLat - MinLat;
            // create a random double in range
            var rDouble = r.NextDouble() * range;
            // the random XCoordinate is created by MinLat plus the random number above
            postLocation.XCoordinate = MinLat * 1.00 + rDouble;

            // get the range of Long
            range = MaxLong - MinLong;
            // create a random double in range
            rDouble = r.NextDouble() * range;
            // the random YCoordinate is created by MinLong plus the random number above
            postLocation.YCoordinate = MinLong * 1.00 + rDouble;
            // the random address is made above.
            postLocation.Address = strAddress;
            #endregion
            // Please change properties Location from String to Location Class
            model.Address = postLocation.Address;
            model.YCoordinate = postLocation.YCoordinate;
            model.XCoordinate = postLocation.XCoordinate;

            #region fake ApplicationUser
            model.UserName = "usr" + index;
            model.Email = "email" + index + "@gmail.com";
            model.Password = "121212";
            model.Gender = r.Next(0, 3); // 0: female, 1: male, 2: other
            #endregion
        }
        private void InitialData()
        {
            // - create location list
            foreach (var str in _strRawLocations)
            {
                _arrRawLocations.Add(str);
            }
        }
        private async Task<bool> CreateOneUser(RegisterViewModel model)
        {
            var location = new Location
            {
                XCoordinate = model.XCoordinate,
                YCoordinate = model.YCoordinate,
                Address = model.Address
            };

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                Gender = model.Gender,
                Location = location,
                CreatedDate = DateTimeOffset.UtcNow,
                Avatar = Constants.DefaultAvatarLink,
                Cover = Constants.DefaultCoverLink,
                EmailConfirmed = true
            };
            await UserManager.CreateAsync(user, model.Password);
            return true;
        }
        #endregion
    }
}