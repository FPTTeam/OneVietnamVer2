
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using MongoDB.Driver;
using OneVietnam.BLL;
using OneVietnam.Common;
using OneVietnam.DTL;
using OneVietnam.Models;
using Icon = System.Drawing.Icon;
using Tag = OneVietnam.DTL.Tag;

namespace OneVietnam.Controllers
{
    [Authorize]
    public class TimelineController : Controller
    {
        static CloudBlobClient blobClient;
        const string blobContainerName = "avatar";
        static CloudBlobContainer blobContainer;
        public TimelineController()
        {
        }

        public TimelineController(ApplicationUserManager userManager)
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

        private PostManager _postManager;
        public PostManager PostManager
        {
            get
            {
                return _postManager ?? HttpContext.GetOwinContext().Get<PostManager>();
            }
            private set { _postManager = value; }
        }

        private TagManager _tagManager;
        public TagManager TagManager
        {
            get
            {
                return _tagManager ?? HttpContext.GetOwinContext().Get<TagManager>();
            }
            private set { _tagManager = value; }
        }

        private IconManager _iconManager;
        public IconManager IconManager
        {
            get
            {
                return _iconManager ?? HttpContext.GetOwinContext().Get<IconManager>();
            }
            private set { _iconManager = value; }
        }


        private ReportManager _reportManager;
        public ReportManager ReportManager
        {
            get
            {
                return _reportManager ?? HttpContext.GetOwinContext().Get<ReportManager>();
            }
            private set { _reportManager = value; }
        }

        public List<Tag> TagList
        {
            get
            {
                var tags = TagManager.FindAllAsync();
                return tags?.Result;
            }
        }
        //public List<Icon> IconList
        //{
        //    get
        //    {
        //        var icons = IconManager.GetIconPostAsync().Result;
        //        return icons;
        //    }
        //}

        //ThamDTH 
        [AllowAnonymous]

        public async Task<ActionResult> Index(string id, int? pageNum, int? filterVal)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length != 24)
            {
                return View();
            }
             CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create a blob client for interacting with the blob service.
            blobClient = storageAccount.CreateCloudBlobClient();
            pageNum = pageNum ?? 1;
            ViewBag.IsEndOfRecords = false;
            BaseFilter filter;
            List<Post> posts;
            List<PostViewModel> list = new List<PostViewModel>();
            ApplicationUser user = await UserManager.FindByIdAsync(id);
            if (Request.IsAjaxRequest())
            {
                filter = new BaseFilter { CurrentPage = pageNum.Value };
                if (filterVal == -1 || filterVal == null)
                {
                    posts = await PostManager.FindAllDescenderByIdAsync(filter, id);
                }
                else
                {
                    posts = await PostManager.FindPostByTypeAndUserIdAsync(filter, id, filterVal);
                }
                if (posts.Count < filter.ItemsPerPage) ViewBag.IsEndOfRecords = true;
                foreach (var post in posts)
                {
                    list.Add(new PostViewModel(post, user.UserName, user.Avatar));

                }
                //ViewBag.IsEndOfRecords = (posts.Any()) && ((pageNum.Value * RecordsPerPage) >= posts.Last().Key);
                return PartialView("_PostRow", list);
            }
            filter = new BaseFilter { CurrentPage = pageNum.Value };
            posts = await PostManager.FindAllDescenderByIdAsync(filter, id);
            var postCount = await PostManager.FindNumberOfPost(id);
            var postTypeList = await IconManager.GetIconPostAsync();
            var timelinePostTypeList = new List<DTL.Icon>();
            foreach (var postType in postTypeList)
            {
                if (!await UserManager.IsInRoleAsync(id, "Admin") && postType.IconValue == (int) PostTypeEnum.AdminPost)
                {
                    continue;
                }
                timelinePostTypeList.Add(postType);
            }            
            ViewData["PostTypes"] = timelinePostTypeList;
            
            ViewData["PostCount"] = postCount;
            var genderList = await IconManager.GetIconGender();
            if (genderList != null)
            {
                ViewData["GenderTypes"] = genderList;
            }
            TimelineViewModel timeLine = new TimelineViewModel(user, posts);
            return View(timeLine);
        }

        [HttpGet]
        public async Task<PartialViewResult> EditProfile()
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            UserProfileViewModel profile = new UserProfileViewModel(user);
            var genderList = await IconManager.GetIconGender();
            if (genderList != null)
            {
                ViewData["GenderTypes"] = genderList;
            }
            return PartialView("_EditProfile", profile);
        }
        public async Task<string> AddPhoneNumber(string phoneNumber)
        {

            // Send result of: UserManager.GetPhoneNumberCodeAsync(User.Identity.GetUserId(), phoneNumber);
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = phoneNumber,
                    Body = "Mã bảo mật của bạn là: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return "";
        }
        //
        // GET: /Account/RemovePhoneNumber
        public async Task<int> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return (int)VerifyStatus.Failure; ;
            }
            
          
            return (int)VerifyStatus.Success;
        }

        [HttpPost]
        [System.Web.Mvc.Authorize]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> EditProfile(UserProfileViewModel profile)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_EditProfile", profile);
            }
            //await UserManager.UpdateProfileUserAsync(profile);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {

                user.UserName = profile.UserName;
                user.Gender = profile.Gender;
                user.Email = profile.Email;
                user.Location = profile.Location;
                user.DateOfBirth = profile.DateOfBirth;
                var result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                AddErrors(result);
            }
            var genderList = await IconManager.GetIconGender();
            if (genderList != null)
            {
                ViewData["GenderTypes"] = genderList;
            }
            return PartialView("_EditProfile", profile);
        }
        [Authorize]
        public async Task<int> VerifyPhoneNumber(string phoneNumber, string code)
        {
            if (!ModelState.IsValid)
            {
                return (int)VerifyStatus.Failure;
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), phoneNumber, code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    user.PhoneNumber = phoneNumber;
                }
                return (int)VerifyStatus.Success;
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Lỗi khi xác nhận số điện thoại");
            return (int)VerifyStatus.Failure;
        }
        [HttpPost]
        [System.Web.Mvc.Authorize]
        public async Task ChangeTwoFactorAuthentication(string value)
        {
            if (value == "Bật")
            {
                await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            }
            else
            {
                await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);

            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
                TwoFacterViewModel setting = new TwoFacterViewModel(user);
            }
        }

        [HttpGet]
        [System.Web.Mvc.Authorize]
        public ActionResult ChangePassword()
        {
            return PartialView("_ChangePassword", new ChangePasswordViewModel());
        }

        [HttpPost]
        [System.Web.Mvc.Authorize]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_ChangePassword", model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }
                return null;
            }            
             AddErrors(result);
            return PartialView("_ChangePassword", model);
        }

        [HttpGet]
        [System.Web.Mvc.Authorize]
        public ActionResult SetPassword()
        {
            return PartialView("_SetPassword", new SetPasswordViewModel());
        }

        [HttpPost]
        [System.Web.Mvc.Authorize]
        [ValidateAntiForgeryToken]
        public async Task<PartialViewResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {                        
                        await SignInAsync(user, isPersistent: false);                                                    
                    }
                    return PartialView("_ChangePassword", new ChangePasswordViewModel());
                }
                AddErrors(result);
            }
            return PartialView("_SetPassword", model);
        }

        [HttpPost]
        [System.Web.Mvc.Authorize]
        public async Task<ActionResult> ChangeAvatar()
        {
            blobContainer = blobClient.GetContainerReference("avatar");
            await blobContainer.CreateIfNotExistsAsync();

            // To view the uploaded blob in a browser, you have two options. The first option is to use a Shared Access Signature (SAS) token to delegate  
            // access to the resource. See the documentation links at the top for more information on SAS. The second approach is to set permissions  
            // to allow public access to blobs in this container. Comment the line below to not use this approach and to use SAS. Then you can view the image  
            // using: https://[InsertYourStorageAccountNameHere].blob.core.windows.net/webappstoragedotnet-imagecontainer/FileName 
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            HttpFileCollectionBase file = Request.Files;

            if (file != null && file.Count > 0)
            {
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference(User.Identity.GetUserId() + Path.GetExtension(file[0].FileName));
                await blob.DeleteIfExistsAsync();
                await blob.UploadFromStreamAsync(file[0].InputStream);
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    user.Avatar = blob.Uri.ToString();
                    var result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    AddErrors(result);
                }
            }
            return RedirectToAction("Index", "Timeline", new { Id = User.Identity.GetUserId() });

        }

        [HttpPost]
        [System.Web.Mvc.Authorize]
        public async Task<ActionResult> ChangeCover()
        {
            blobContainer = blobClient.GetContainerReference("cover");
            await blobContainer.CreateIfNotExistsAsync();

            // To view the uploaded blob in a browser, you have two options. The first option is to use a Shared Access Signature (SAS) token to delegate  
            // access to the resource. See the documentation links at the top for more information on SAS. The second approach is to set permissions  
            // to allow public access to blobs in this container. Comment the line below to not use this approach and to use SAS. Then you can view the image  
            // using: https://[InsertYourStorageAccountNameHere].blob.core.windows.net/webappstoragedotnet-imagecontainer/FileName 
            await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            HttpFileCollectionBase file = Request.Files;

            if (file != null && file.Count > 0)
            {
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference(User.Identity.GetUserId() + Path.GetExtension(file[0].FileName));
                await blob.DeleteIfExistsAsync();
                await blob.UploadFromStreamAsync(file[0].InputStream);
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    user.Cover = blob.Uri.ToString();
                    var result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    AddErrors(result);
                }
            }
            return RedirectToAction("Index", "Timeline", new { Id = User.Identity.GetUserId() });

        }

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.Contains("Incorrect password"))
                {
                    ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác.");
                    continue;
                }
                ModelState.AddModelError("", error);
            }
        }

        public ActionResult ReportUser(string userId)
        {            
            return PartialView("../Timeline/_ReportUser", new ReportViewModel(userId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReportUser(ReportViewModel model)
        {
            Report report = new Report(model) { ReporterId = User.Identity.GetUserId() };
            await ReportManager.CreateAsync(report);
            return PartialView("../Timeline/_ReportUser", new ReportViewModel(model.UserId));
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }            
            if (disposing && _iconManager != null)
            {
                _iconManager.Dispose();
                _iconManager = null;
            }
            if (disposing && _postManager != null)
            {
                _postManager.Dispose();
                _postManager = null;
            }
            if (disposing && _reportManager != null)
            {
                _reportManager.Dispose();
                _reportManager = null;
            }
            base.Dispose(disposing);
        }
    }
}