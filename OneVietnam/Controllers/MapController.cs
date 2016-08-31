using Microsoft.AspNet.Identity.Owin;
using OneVietnam.BLL;
using OneVietnam.DTL;
using OneVietnam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using MongoDB.Driver;

namespace OneVietnam.Controllers
{
    [Authorize]
    public class MapController : Controller
    {

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

        [AllowAnonymous]
        public async Task<ActionResult> ShowMap(double? XCoordinate, double? YCoordinate, int? postType, string postId = "")
        {
            ViewBag.XCoordinate = XCoordinate;
            ViewBag.YCoordinate = YCoordinate;
            ViewBag.PostType = postType;
            ViewBag.PostId = postId;

            var userslist = await UserManager.AllUsersAsync().ConfigureAwait(false);

            MapViewModel mapModal;
            List<MapViewModel> list = new List<MapViewModel>();
            
            foreach (ApplicationUser user in userslist)
            {
                mapModal = new MapViewModel();

                if (user.LockedFlag == false && user.DeletedFlag == false)
                {
                    mapModal.X = user.Location.XCoordinate;
                    mapModal.Y = user.Location.YCoordinate;
                    mapModal.UserId = user.Id;
                    mapModal.Gender = user.Gender;
                    list.Add(mapModal);
                }
            }
            //var list = userslist.Select(user => new MapViewModel
            //{

            //    X = user.Location.XCoordinate,
            //    Y = user.Location.YCoordinate,
            //    UserId = user.Id,
            //    Gender = user.Gender
            //}).ToList();
          
            ViewBag.topPostModel = await GetTopPostInfo().ConfigureAwait(false);
            return View(list);
        }

        [AllowAnonymous]
        public async Task<List<PostInfoWindowModel>> GetTopPostInfo()
        {
            var baseFilter = new BaseFilter { Limit = Constants.LimitedNumberOfPost };
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("DeletedFlag", false) & builder.Eq("LockedFlag", false) & builder.Eq("Status", true);
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            var topPostList = await PostManager.FindAllAsync(baseFilter, filter, sort).ConfigureAwait(false);

            var result = new PostInfoWindowModel();

            return topPostList.Select(p => new PostInfoWindowModel
            {
                Address = p.PostLocation.Address,
                postId = p.Id,
                CreatedDate = (DateTimeOffset)p.CreatedDate,
                Title = p.Title,
                Description = p.Description,
                PostLocation = p.PostLocation,
                PostType = p.PostType,
                Illustrations = p.Illustrations,
                TimeInterval = Utilities.GetTimeInterval(p.CreatedDate)

        }).ToList();
        }

        //[HttpPost] // can be HttpGet
        [AllowAnonymous]
        public async Task<JsonResult> GetUserInfo(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId).ConfigureAwait(false);
            return Json(user, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        public async Task<JsonResult> GetUserLocation(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId);
            return Json(user.Location ?? null, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetPostPartialView(string postId)
        {
            var post = await PostManager.FindByIdAsync(postId).ConfigureAwait(false);
            if (post != null)
            {
                ApplicationUser postUser = await UserManager.FindByIdAsync(post.UserId);
                if (postUser != null)
                {

                    PostViewModel showPost = new PostViewModel(post, postUser.UserName, postUser.Avatar);
                    return PartialView("../Newsfeed/_ShowPostDetailModal", showPost);

                }
            }
            var result = new PostViewModel(post);
            return PartialView("../Newsfeed/_ShowPostDetailModal", result);
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetUserPartialView(string userId)
        {
            var user = await UserManager.FindByIdAsync(userId).ConfigureAwait(false);
            var result = new UserViewModel(user);
            return PartialView("_UserInfo", result);
        }

        [AllowAnonymous]
        public async Task<ActionResult> GetMorePost(int? pageNum)
        {
            var baseFilter = new BaseFilter { Limit = Constants.LimitedNumberOfPost, ItemsPerPage = 5, CurrentPage = pageNum.Value };
            var builder = Builders<Post>.Filter;
            var filter = builder.Eq("DeletedFlag", false) & builder.Eq("LockedFlag", false) & builder.Eq("Status", true);
            var sort = Builders<Post>.Sort.Descending("CreatedDate");
            var topPostList = await PostManager.FindAllAsync(baseFilter, filter, sort).ConfigureAwait(false);

            List<PostInfoWindowModel> topListModel = new List<PostInfoWindowModel>();
            PostInfoWindowModel result;

            foreach (Post post in topPostList)
            {
                result = new PostInfoWindowModel();
                result.Address = post.PostLocation.Address;
                result.CreatedDate = post.CreatedDate;
                result.postId = post.Id;
                result.Title = post.Title;
                result.UserId = post.UserId;
                result.PostLocation = post.PostLocation;
                result.PostType = post.PostType;
                result.Illustrations = post.Illustrations;
                result.Description = post.Description;
                result.TimeInterval = Utilities.GetTimeInterval(post.CreatedDate);

                topListModel.Add(result);
            }

            return PartialView("_SidenavPost", topListModel);
        }

        [AllowAnonymous]
        public async Task<JsonResult> GetListOfAPostType(int postType)
        {
            var baseFilter = new BaseFilter { IsNeedPaging = false };
            var postlist = await PostManager.FindPostsByTypeAsync(baseFilter,postType).ConfigureAwait(false);

            var list = postlist.Select(p => new MapViewModel
            {
                //UserId = p.UserId,
                X = p.PostLocation.XCoordinate,
                Y = p.PostLocation.YCoordinate,
                PostId = p.Id
                //PostType = p.PostType
            }).ToList();

            var jsonResult = Json(list, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;

            return jsonResult;
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
    }
}