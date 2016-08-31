using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AspNet.Identity.MongoDB;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MongoDB.Driver;
using OneVietnam.BLL;
using OneVietnam.Common;
using OneVietnam.DTL;
using OneVietnam.Models;

namespace OneVietnam.Controllers
{
    [Authorize(Roles = CustomRoles.Admin + "," + CustomRoles.Mod)]
    public class AdministrationController : Controller
    {
        public AdministrationController() { }

        public AdministrationController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
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

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
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
        private PostManager _postManager;
        public PostManager PostManager
        {
            get
            {
                return _postManager ?? HttpContext.GetOwinContext().Get<PostManager>();
            }
            private set { _postManager = value; }
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


        public async Task<ActionResult> Index()
        {

            var users = await UserManager.TextSearchMultipleQuery("", DateTimeOffset.Now.Date.AddDays(-7).ToUniversalTime(), DateTimeOffset.Now.Date.AddDays(1).ToUniversalTime(), "", null);
            var roles = await RoleManager.AllRolesAsync();
            var posts = await PostManager.SearchPostMultipleQuery("", DateTimeOffset.Now.Date.AddDays(-7).ToUniversalTime(), DateTimeOffset.Now.Date.AddDays(1).ToUniversalTime(), null);
            var reports = await ReportManager.FindAllAsync();
            List<ReportViewModel> reportViewList = new List<ReportViewModel>();
            foreach (var report in reports)
            {
                ReportViewModel reportView = new ReportViewModel(report);
                if (!string.IsNullOrWhiteSpace(report.HandlerId))
                {
                    var handlerUser = await UserManager.FindByIdAsync(report.HandlerId);
                    if (handlerUser != null)
                    {
                        reportView.HandlerName = handlerUser.UserName;
                    }
                }
                if (!string.IsNullOrWhiteSpace(report.PostId))
                {
                    var reportedPost = await PostManager.FindByIdAsync(report.PostId);
                    if (reportedPost != null)
                    {
                        reportView.PostTile = reportedPost.Title;
                    }
                }
                var reportedUser = await UserManager.FindByIdAsync(report.UserId);
                if (!string.IsNullOrWhiteSpace(reportedUser.UserName))
                {
                    reportView.UserName = reportedUser.UserName;
                }

                reportViewList.Add(reportView);

            }

            AdministrationViewModel administrationView = new AdministrationViewModel(users, roles, posts, reportViewList);
            List<AdminPostViewModel> postViews = new List<AdminPostViewModel>();
            if (posts != null && posts.Count > 0)
            {
                foreach (var item in posts)
                {
                    AdminPostViewModel model = new AdminPostViewModel(item);
                    var user = await UserManager.FindByIdAsync(item.UserId);
                    if(user==null || user.DeletedFlag==true || user.LockedFlag==true) continue;
                    model.UserName = user.UserName;
                    postViews.Add(model);
                }

            }
            administrationView.Posts = postViews;
            return View(administrationView);
        }

        [HttpPost]
        [Authorize(Roles = CustomRoles.Admin)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddRole(RoleViewModel roleViewModel)
        {

            if (ModelState.IsValid)
            {
                var role = new IdentityRole(roleViewModel.Name);
                var roleresult = await RoleManager.CreateAsync(role);
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError("",
                        roleresult.Errors.First().Contains("is already taken")
                            ? "Quyền này đã tồn tại trong cơ sở dữ liệu."
                            : roleresult.Errors.First());
                }

            }
            var roleList = await RoleManager.AllRolesAsync();

            List<RoleViewModel> roles = new List<RoleViewModel>();
            if (roleList != null && roleList.Count > 0)
            {
                foreach (var oneRole in roleList)
                {
                    RoleViewModel roleView = new RoleViewModel(oneRole);
                    roles.Add(roleView);
                }
            }
            return PartialView("_RoleManagementPanel", roles);
        }

        [HttpPost]
        [Authorize(Roles = CustomRoles.Admin)]
        public async Task<ActionResult> RemoveRole(string roleId)
        {
            //Remove role in roles document
            var role = await RoleManager.FindByIdAsync(roleId);
            var roleresult = await RoleManager.DeleteAsync(role);
            if (!roleresult.Succeeded)
            {
                ModelState.AddModelError("", roleresult.Errors.First());
            }

            //Remove role if user has this role
            var users = await UserManager.FindUsersByRoleAsync(role);
            foreach (var user in users)
            {
                user.Roles.Remove(role.Name);
                var userResult = await UserManager.UpdateAsync(user);
                if (!userResult.Succeeded)
                {
                    ModelState.AddModelError("", userResult.Errors.First());
                }
            }

            //Return new role list after remove this role
            var roleList = await RoleManager.AllRolesAsync();
            List<RoleViewModel> roles = new List<RoleViewModel>();
            if (roleList != null && roleList.Count > 0)
            {
                foreach (var oneRole in roleList)
                {
                    RoleViewModel roleView = new RoleViewModel(oneRole);
                    roles.Add(roleView);
                }
            }
            return PartialView("_RoleManagementPanel", roles);
        }

        [HttpGet]
        [Authorize(Roles = CustomRoles.Admin)]
        public async Task<ActionResult> GetOtherRoles(string userId)
        {
            var roleList = await RoleManager.AllRolesAsync();
            var user = await UserManager.FindByIdAsync(userId);

            List<IdentityRole> roles = new List<IdentityRole>();
            if (roleList != null && roleList.Count > 0)
            {
                foreach (var role in roleList)
                {
                    if (user?.Roles != null && user.Roles.Count > 0)
                    {
                        if (!user.Roles.Contains(role.Name))
                        {
                            roles.Add(role);
                        }
                    }
                    else
                    {
                        roles.Add(role);
                    }
                }
            }
            ViewData["OtherRole"] = roles;
            UserManagementViewModel userView = new UserManagementViewModel(user);
            return PartialView("_AddUserRole", userView);
        }

        [HttpPost]
        [Authorize(Roles = CustomRoles.Admin)]
        public async Task<ActionResult> RemoveUserRole(string userId, string role)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (ModelState.IsValid)
            {
                if (user?.Roles != null && user.Roles.Count > 0)
                {
                    user.Roles.Remove(role);
                }
                var result = await UserManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                }
            }
            UserManagementViewModel userView = new UserManagementViewModel(user);
            return PartialView("_UserRoles", userView);
        }

        [HttpPost]
        [Authorize(Roles = CustomRoles.Admin)]
        public async Task<ActionResult> AddUserRole(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    if (user.Roles == null)
                    {
                        user.Roles = new List<string>();
                    }
                    if (Request.Form.Count > 0)
                    {
                        string roleAdded = Request.Form["txtUserRole"];
                        if (string.IsNullOrEmpty(roleAdded))
                        {
                            ModelState.AddModelError("", "Chưa chọn quyền.");
                        }
                        else
                        {
                            user.Roles.Add(roleAdded);
                            var result = await UserManager.UpdateAsync(user);
                            if (!result.Succeeded)
                            {
                                ModelState.AddModelError("", result.Errors.First());
                            }
                        }
                    }
                }

            }
            UserManagementViewModel userView = new UserManagementViewModel();
            if (user != null)
            {
                userView = new UserManagementViewModel(user);
            }

            return PartialView("_UserRoles", userView);
        }

        [HttpPost]
        [Authorize(Roles = CustomRoles.Admin)]
        public async Task<ActionResult> ChangeLockedStatus(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (ModelState.IsValid)
            {
                if (user != null)
                {
                    user.LockedFlag = !user.LockedFlag;
                    var result = await UserManager.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", result.Errors.First());
                    }
                }

            }
            UserManagementViewModel userView = new UserManagementViewModel();
            if (user != null)
            {
                userView = new UserManagementViewModel(user);
            }

            return PartialView("_UserLockedStatus", userView);
        }

        [HttpPost]
        public void GetUploadImage()
        {
            Session["IllustrationList"] = Request.Files;
        }

        [HttpPost]
        public async Task<ActionResult> ChangeReportStatus(string reportAction, string reportId)
        {
            var report = await ReportManager.FindByIdAsync(reportId);
            try
            {
                report.Status = reportAction;
                report.HandlerId = User.Identity.GetUserId();
                if (string.Equals(reportAction, ReportStatus.Closed.ToString()) || string.Equals(reportAction, ReportStatus.Canceled.ToString()))
                {
                    report.CloseDate = DateTimeOffset.UtcNow;
                }
                await ReportManager.UpdateAsync(report);
                if (string.Equals(reportAction, ReportStatus.Closed.ToString()))
                {
                    if (!string.IsNullOrWhiteSpace(report.PostId))
                    {
                        var post = await PostManager.FindByIdAsync(report.PostId);
                        post.LockedFlag = true;
                        await PostManager.UpdateAsync(post);
                    }
                    else
                    {
                        var user = await UserManager.FindByIdAsync(report.UserId);
                        user.LockedFlag = true;
                        await UserManager.UpdateAsync(user);
                    }
                }

                ReportViewModel viewModal = new ReportViewModel(report);
                return PartialView("../Administration/_ShowReportStatus", viewModal);
            }
            catch
            {
                ReportViewModel viewModal = new ReportViewModel(report);
                return PartialView("../Administration/_ShowReportStatus", viewModal);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateIcon(CreateIconViewModel model)
        {
            var icon = new Icon(model);
            await IconManager.CreateAsync(icon);
            return RedirectToAction("Index", "Administration");
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SearchUserMultipleQuery()
        {
            string userName = "";
            DateTimeOffset? createdDateFrom = null;
            DateTimeOffset? createdDateTo = null;
            string role = "";
            bool? isConnection = null;

            if (Request.Form.Count > 0)
            {
                userName = Request.Form["txtSearchUserName"];
                string dateFrom = Request.Form["dtCreatedDateFrom"];
                if (!string.IsNullOrWhiteSpace(dateFrom))
                {
                    createdDateFrom = Convert.ToDateTime(dateFrom).ToUniversalTime();
                }
                string dateTo = Request.Form["dtCreatedDateTo"];
                if (!string.IsNullOrWhiteSpace(dateTo))
                {
                    createdDateTo = Convert.ToDateTime(dateTo).AddHours(24).ToUniversalTime();
                }
                role = Request.Form["txtSearchUserRole"];
                var connection = Request.Form["chkIsOnline"];
                if (!string.IsNullOrWhiteSpace(connection) && string.Equals(connection, "on"))
                {
                    isConnection = true;
                }
            }
            var users = await UserManager.TextSearchMultipleQuery(userName, createdDateFrom, createdDateTo, role, isConnection);
            List<UserManagementViewModel> userViews = new List<UserManagementViewModel>();
            if (users != null && users.Count > 0)
            {
                foreach (var item in users)
                {
                    UserManagementViewModel model = new UserManagementViewModel(item);
                    userViews.Add(model);
                }

            }
            return PartialView("../Administration/_UsersManagementPanel", userViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SearchPostMultipleQuery()
        {
            string postTitle = "";
            DateTimeOffset? createdDateFrom = null;
            DateTimeOffset? createdDateTo = null;
            bool? postStatus = null;
            if (Request.Form.Count > 0)
            {
                postTitle = Request.Form["txtSearchPostTitle"];
                string dateFrom = Request.Form["dtPostCreatedDateFrom"];
                if (!string.IsNullOrWhiteSpace(dateFrom))
                {
                    createdDateFrom = Convert.ToDateTime(dateFrom).ToUniversalTime();
                }
                string dateTo = Request.Form["dtPostCreatedDateTo"];
                if (!string.IsNullOrWhiteSpace(dateTo))
                {
                    createdDateTo = Convert.ToDateTime(dateTo).AddHours(24).ToUniversalTime();
                }
                var status = Request.Form["rdStatus"];
                if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all"))
                {
                    postStatus = Convert.ToBoolean(status);
                }
            }
            var posts = await PostManager.SearchPostMultipleQuery(postTitle, createdDateFrom, createdDateTo, postStatus);
            List<AdminPostViewModel> postViews = new List<AdminPostViewModel>();
            if (posts != null && posts.Count > 0)
            {
                foreach (var item in posts)
                {
                    if (item.PostType == (int)PostTypeEnum.AdminPost)
                    {
                        AdminPostViewModel model = new AdminPostViewModel(item);
                        var user = await UserManager.FindByIdAsync(item.UserId);
                        model.UserName = user.UserName;
                        postViews.Add(model);
                    }                    
                }

            }
            return PartialView("../Administration/_PostsManagementPanel", postViews);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SearchReportMultipleQuery()
        {
            DateTimeOffset? createdDateFrom = null;
            DateTimeOffset? createdDateTo = null;
            string repostStatus = "";
            if (Request.Form.Count > 0)
            {
                string dateFrom = Request.Form["dtReportCreatedDateFrom"];
                if (!string.IsNullOrWhiteSpace(dateFrom))
                {
                    createdDateFrom = Convert.ToDateTime(dateFrom).ToUniversalTime();
                }
                string dateTo = Request.Form["dtReportCreatedDateTo"];
                if (!string.IsNullOrWhiteSpace(dateTo))
                {
                    createdDateTo = Convert.ToDateTime(dateTo).AddHours(24).ToUniversalTime();
                }
                var status = Request.Form["rdReportStatus"];
                if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "all"))
                {
                    repostStatus = status;
                }
            }
            var reports = await ReportManager.SearchRepostMultipleQuery(createdDateFrom, createdDateTo, repostStatus);
            List<ReportViewModel> reportViewList = new List<ReportViewModel>();
            foreach (var report in reports)
            {
                ReportViewModel reportView = new ReportViewModel(report);
                if (!string.IsNullOrWhiteSpace(report.HandlerId))
                {
                    var handlerUser = await UserManager.FindByIdAsync(report.HandlerId);
                    if (handlerUser != null)
                    {
                        reportView.HandlerName = handlerUser.UserName;
                    }
                }
                if (!string.IsNullOrWhiteSpace(report.PostId))
                {
                    var reportedPost = await PostManager.FindByIdAsync(report.PostId);
                    if (reportedPost != null)
                    {
                        reportView.PostTile = reportedPost.Title;
                    }
                }
                var reportedUser = await UserManager.FindByIdAsync(report.UserId);
                if (!string.IsNullOrWhiteSpace(reportedUser.UserName))
                {
                    reportView.UserName = reportedUser.UserName;
                }

                reportViewList.Add(reportView);

            }
            return PartialView("../Administration/_ReportsManagementPanel", reportViewList);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }
            if (disposing && _roleManager != null)
            {
                _roleManager.Dispose();
                _roleManager = null;
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