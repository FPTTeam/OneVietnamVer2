
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OneVietnam.BLL;
using OneVietnam.DTL;
using OneVietnam.Models;
using Facebook;
using System.Collections;
using System.Configuration;
using System.Security.Claims;
using OneVietnam.Common;
using Microsoft.AspNet.SignalR.Hubs;
namespace OneVietnam.Controllers
{
    [System.Web.Mvc.Authorize]
    public class NewsfeedController : Controller
    {
        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
        private static readonly CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(Microsoft.Azure.CloudConfigurationManager.GetSetting("StorageConnectionString"));
        private readonly CloudBlobClient _blobClient = StorageAccount.CreateCloudBlobClient();        
        public static PostViewModel PostView;        

        public NewsfeedController()
        {
        }

        public NewsfeedController(ApplicationUserManager userManager)
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
        public List<Icon> GenderIcon
        {
            get
            {
                var gender = IconManager.GetIconGender().Result;
                return gender;
            }
        }

        public async Task _CreatePost()
        {            
            var tagList = await TagManager.FindAllAsync(false);
            if (tagList != null)
            {
                ViewData["TagList"] = tagList;
            }                                               
            var iconList = await IconManager.GetIconPostAsync();
            if (iconList != null)
            {
                ViewData["PostTypes"] = iconList;
            }            
        }

        private HttpFileCollectionBase _illustrationList;

        public void GetIllustrations()
        {            
            _illustrationList = Request.Files;
            Session.Clear();
            Session.Add("Illustrations", _illustrationList);
            Console.WriteLine("Go Get OK");            
        }

        [HttpPost]
        [System.Web.Mvc.Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreatePost(CreatePostViewModel p)
        {            
            ViewData.Clear();            
            var post = new Post(p)
            {
                CreatedDate = System.DateTime.Now,
                UserId = User.Identity.GetUserId()
            };
            var tagList = await PostManager.AddAndGetAddedTags(Request, TagManager, "TagsInput");
            _illustrationList = (HttpFileCollectionBase)Session["Illustrations"];
            var illList = await PostManager.AzureUploadAsync(_illustrationList, post.Id);
            Session["Illustrations"] = null;
            _illustrationList = null;
            if (tagList != null)
            {
                post.Tags = tagList;
            }

            if (illList != null)
            {
                post.Illustrations = illList;
            }            
            await PostManager.CreateAsync(post);

            if (post.PostType == (int) PostTypeEnum.AdminPost)
            {
                var hubContext =  GlobalHost.ConnectionManager.GetHubContext<OneHub>();
                var avatar = ((ClaimsIdentity) User.Identity).FindFirst("Avatar").Value;                
                var description = Constants.AdminNotification + "\"" +
                                  post.Title + "\"";
                var notice = new Notification(Url.Action("ShowPostDetailPage", "Newsfeed", new { post.Id }), avatar, description);
                await UserManager.PushAdminNotificationToAllUsersAsync(notice);
                await hubContext.Clients.All.pushNotification();
            }
            PostView = new PostViewModel(post);
            return RedirectToAction("Index", "Newsfeed");
        }        

        public const int RecordsPerPage = 60;
        [AllowAnonymous]
        public async Task<ActionResult> _AdminPost()
        {
            List<PostViewModel> list = new List<PostViewModel>();
            var posts = await PostManager.FindAllActiveAdminPostAsync();
            foreach (var post in posts)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(post.UserId);
                if(user==null || user.DeletedFlag==true || user.LockedFlag==true) continue;
                list.Add(new PostViewModel(post, user.UserName, user.Avatar));

            }
            return PartialView("_AdminPost", list);
        }
        [AllowAnonymous]
        public async Task<ActionResult> Index(int? pageNum,int? filterVal)
        {            
            var tagList = await TagManager.FindAllAsync(false);
            if (tagList != null)
            {
                ViewData["TagList"] = tagList;
            }
            var iconList = await IconManager.GetIconPostAsync();
            if (iconList != null)
            {
                ViewData["PostTypes"] = iconList;
            }
            pageNum = pageNum ?? 1;
            ViewBag.IsEndOfRecords = false;

            BaseFilter filter;
            List<Post> posts;
            List<PostViewModel> list = new List<PostViewModel>();
            if (Request.IsAjaxRequest())
                {
                filter = new BaseFilter { CurrentPage = pageNum.Value };
                if (filterVal == -1||filterVal==null) {
                posts = await PostManager.FindAllDescenderAsync(filter);
                }
                else
                {
                    
                posts = await PostManager.FindPostsByTypeAsync(filter, filterVal);

                }
                if (posts.Count < filter.ItemsPerPage) ViewBag.IsEndOfRecords = true;
                    foreach (var post in posts)
                    {
                        ApplicationUser user = await UserManager.FindByIdAsync(post.UserId);
                        if(user==null || user.DeletedFlag==true|| user.LockedFlag==true) continue;                        
                    list.Add(new PostViewModel(post, user.UserName, user.Avatar));

                    }
                //ViewBag.IsEndOfRecords = (posts.Any()) && ((pageNum.Value * RecordsPerPage) >= posts.Last().Key);
                return PartialView("_PostRow", list);
                }
        
            filter = new BaseFilter { CurrentPage = pageNum.Value };
            posts = await PostManager.FindAllDescenderAsync(filter);
            if (posts.Count < filter.ItemsPerPage) ViewBag.IsEndOfRecords = true;
            foreach (var post in posts)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(post.UserId);
                //don't load post of deleted user
                if (user?.DeletedFlag == false && user?.LockedFlag==false)
                {
                    list.Add(new PostViewModel(post, user.UserName, user.Avatar));
                }
            }
            ViewBag.Posts = list;
            return View();
        }

        /// <summary>
        /// Get posts for pagenum
        /// </summary>
        /// <param name="pageNum"></param>
        /// <returns>List Post></returns>
        [AllowAnonymous]
        public async Task<ActionResult> _suggestedPost(string postId,int? pageNum)
        {
            Post post = await PostManager.FindByIdAsync(postId);
            List<Tag> tagsList = post.Tags;
            //BaseFilter baseFilter = new BaseFilter { CurrentPage = pageNum.Value };            
            var result = await PostManager.FindPostByTagsAsync(tagsList,postId);
            if (result.Count == 0) return null;
            var suggestedList = new List<SuggestedPost>();            
            foreach (var item in result)
            {
                int score = 0;
                foreach (var tag in tagsList)
                {
                    if (item.Tags.Contains(tag))
                    {
                        score++;
                    }
                }
                var s = new SuggestedPost
                {
                    post = item,
                    score = score
                };
                suggestedList.Add(s);
            }            
            suggestedList.Sort();
            var list = new List<PostViewModel>();            
            for (var i = suggestedList.Count-1; i >=0; i--)
            {
                //for infinite scrolling
                //if (i == suggestedList.Count - 1 - baseFilter.Skip - baseFilter.Limit) break;
                var avatarLink = await UserManager.GetAvatarByIdAsync(suggestedList[i].post.UserId);
                var userName = await UserManager.GetUserNameByIdAsync(suggestedList[i].post.UserId);
                var postView = new PostViewModel(suggestedList[i].post,userName,avatarLink);
                list.Add(postView);                
            }
            return PartialView(list);
        }
                
        [AllowAnonymous]
        public async Task<ActionResult> ShowPostDetailPage(string Id)
        {
            Post post = await PostManager.FindByIdAsync(Id);
            if (post != null)
            {
                ApplicationUser postUser = await UserManager.FindByIdAsync(post.UserId);
                if (postUser != null)
                {

                    PostViewModel showPost = new PostViewModel(post, postUser.UserName, postUser.Avatar);

                    return View(showPost);
                }
            }
            return View();
        }
        public async Task<List<Post>> GetRecordsForPage(int pageNum)
        {
            //Dictionary<int, Post> posts = (Session["Posts"] as Dictionary<int, Post>);

            //int from = (pageNum * RecordsPerPage);
            //int to = from + RecordsPerPage;

            //return posts
            //    .Where(x => x.Key > from && x.Key <= to)
            //    .OrderBy(x => x.Key)
            //    .ToDictionary(x => x.Key, x => x.Value);
            var filter = new BaseFilter { CurrentPage = pageNum };
            return await PostManager.FindAllAsync(filter);
        }
        [AllowAnonymous]
        public async Task<ActionResult> _ShowPostDetailModal(string postId)
        {
            //List<Post> list = await PostManager.FindByUserId(User.Identity.GetUserId());
            Post post = await PostManager.FindByIdAsync(postId);
            if (post != null)
            {
                ApplicationUser postUser = await UserManager.FindByIdAsync(post.UserId);
                if (postUser != null)
                {

                    PostViewModel showPost = new PostViewModel(post, postUser.UserName, postUser.Avatar);

                    return PartialView(showPost);
                }
            }
            return PartialView();
        }

        [AllowAnonymous]
        public JsonResult GetCommentInfo(string commentid)
        {     
            var fb = new FacebookClient(Constants.accessTokenFacebook);
            dynamic commentInfo = fb.Get(commentid);                                            
            string id = commentInfo["from"]["id"];
            dynamic userInfo = fb.Get(id+"?fields=picture");            
            var commentor = new CommentorViewModel
            {
                Avatar = userInfo["picture"]["data"]["url"],
                Username = commentInfo["from"]["name"]
            };

            return Json(commentor, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ShowPostDetail(PostViewModel pPostView)

        {            
            ViewData.Clear();
            string strPostId = "";
            if (Request.Form.Count > 0)
            {
                strPostId = Request.Form["PostId"];

            }                                    
            return RedirectToAction("DeletePost", "Newsfeed", new { postId = strPostId });
        }

        //[HttpPost]
        //public async Task ReportPost(string userId, string postId, string description)
        //{
        //    Report report = new Report(userId, postId, description);
        //    await ReportManager.CreateAsync(report);
        //    //TODO send notification to Mod
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReportPost(ReportViewModel model)
        {
            Report report = new Report(model) { ReporterId = User.Identity.GetUserId() };
            await ReportManager.CreateAsync(report);
            return PartialView("../Newsfeed/_Report", new ReportViewModel(model.Id, model.UserId));
        }

        [System.Web.Mvc.Authorize]        
        public async Task<ActionResult> EditPost(string postId)
        {
           
            CloudBlobContainer blobContainer = _blobClient.GetContainerReference(postId);
            await blobContainer.CreateIfNotExistsAsync();

            List<Uri> allBlobs = new List<Uri>();

            foreach (IListBlobItem blob in blobContainer.ListBlobs())
            {
                if (blob.GetType() == typeof(CloudBlockBlob))
                    allBlobs.Add(blob.Uri);
            }
            ViewData["Blobs"] = allBlobs;
            var tagList = await TagManager.FindAllAsync(false);
            if (tagList != null)
                    {
                ViewData["TagList"] = tagList;
                    }
            var iconList = await IconManager.GetIconPostAsync();
            if (iconList != null)
                    {
                ViewData["PostTypes"] = iconList;
                    }
            Post post = await PostManager.FindByIdAsync(postId);
                    PostViewModel showPost = new PostViewModel(post);
                    return View(showPost);
                }

        [HttpPost]
        [System.Web.Mvc.Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPost(PostViewModel pPostView)
        {                                                
            ViewData.Clear();
            var tagList = await PostManager.AddAndGetAddedTags(Request, TagManager, "TagsInput");
            if (tagList != null)
            {
                pPostView.Tags = tagList;
            }
            CloudBlobContainer blobContainer= _blobClient.GetContainerReference(pPostView.Id);
            await blobContainer.CreateIfNotExistsAsync();
            blobContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
            List<Illustration> illList=new List<Illustration>();
         
            _illustrationList = (HttpFileCollectionBase)Session["Illustrations"];
            if (_illustrationList?.Count > 0) { 
                for (int i = 0; i < _illustrationList.Count; i++)
            {

                CloudBlockBlob blob = blobContainer.GetBlockBlobReference(GetRandomBlobName(_illustrationList[i].FileName));
                await blob.UploadFromStreamAsync(_illustrationList[i].InputStream);

            }
                
            }
            var blobList = blobContainer.ListBlobs();
            
            foreach (var blob in blobList)
            {
                Illustration newIll = new Illustration(blob.Uri.ToString());
                illList.Add(newIll);
            }
            if (illList.Count>0)
            {
                pPostView.Illustrations = illList;
            }
            Post post = new Post(pPostView);           
            await PostManager.UpdateAsync(post);
            return RedirectToAction("ShowPostDetailPage", "Newsfeed", new { pPostView.Id });
        }
        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }

        [HttpPost]
        [System.Web.Mvc.Authorize(Roles = "Admin")]        
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAdminPost(AdminPostViewModel pPostView)
        {
            ViewData.Clear();            
            var illList = await PostManager.AzureUploadAsync(_illustrationList, pPostView.Id);
            if (illList != null)
            {
                pPostView.Illustrations = illList;
            }
            Post post = new Post(pPostView);
            await PostManager.UpdateAsync(post);
            return RedirectToAction("ShowPostDetail", "Newsfeed", new { postId = pPostView.Id });
        }

        [System.Web.Mvc.Authorize]        
        public async Task<ActionResult> DeletePost(string postId)
        {        
            //await PostManager.DeleteByIdAsync(postId);
            var post = await PostManager.FindByIdAsync(postId);
            post.DeletedFlag = true;
            await PostManager.UpdateAsync(post);
            //CloudBlobContainer blobContainer = _blobClient.GetContainerReference(postId);
            //await blobContainer.DeleteIfExistsAsync();
            return RedirectToAction("Index", "Newsfeed");
        }
        public async Task DeleteImages(string name, string id)
        {
           
           
            try
            {
                await PostManager.AzureDeleteAsync(name, id);


            }

            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
            }
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
        // TraNT: Auto create post
        #region AutoGeneratePosts
        //private readonly ArrayList _arrRawDesciptions = new ArrayList();

        private readonly String[,] _strRawDescriptions =
        {
            // 0 -3 - nhà ở
            {
                "cho thuê nhà giá 1.000 yên / tháng tại Hokkaido",
                "bán nhà ở Tokyo giá 100.000 yên"
            },
            // 1- 4 - việc làm
            {
                "cần tuyển nhân viên chạy bàn, lương 800 yên / giờ.",
                "khuân vác vật liệu xây dựng, lương 900 yên . giờ. Liên hệ: +1800 7508"
            },

            // 2- 5 - cho đồ
            {
                "cần cho bộ ghế sa-lông cũ",
                "có vài bộ quần áo trẻ con cũ cần cho đi gấp."
            },

            // 3- 6 - xách tay
            {
                "nhận xách tay son Mac thương hiệu Nhât Bản",
                "Nhờ xách tay hộ Iphone6 về Hà Nội có hậu tạ"
            },
            // 4- 7 - mua bán
            {
                "Cần bán 1 tivi + tủ lạnh ",
                "Cần mua một chiếc máy tính cũ giá khoảng 12.000 yên"
            },
            // 5- 8 - cần giúp đỡ
            {
                "Em vừa mới đến Tokyo có ai có công việc làm thêm nào giới thiệu cho em với",
                "Cháu bị lạc tàu điện ở Osaka, trong người không có tiền"
            },
            // 6- 9 - cảnh báo
            {
                "có một tên biến thái gần khu vực Okinawa",
                "Cảnh báo các bạn không nên ăn ở quán Takoyaki gần Nedan nhé"
            }

            };
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

        // photo links for issulation
        private readonly ArrayList _arrRawImages = new ArrayList();
        private readonly String[] _strRawImages =
        {
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001195331195_aac2d231-4d9f-4feb-a941-6f82c43a6878.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001196775973_8e638f0c-caa8-435b-bef0-446fb89e45a6.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001198078743_5395a850-8cd4-44df-9099-9732732a6075.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001198827229_8006d5fc-d265-4b32-b95d-4b521b11205b.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001200076322_d3b22408-c9c1-4fcd-957b-e91367c27114.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001201306026_b1f245d3-1f8c-4092-9fe2-8b43622e8b37.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001202005800_26b1fbdf-adfb-465e-ad60-2dbe117b1851.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001202815544_e980bf90-94b2-4179-a1b7-8e748e33ddbd.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001203597074_2a3802c3-138a-4afe-bfb8-e18fd14ddd33.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001204389200_5c0bde13-8cdc-4dc0-b425-32fdc5205ca2.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001205078415_78a6dc22-ba47-4f1c-b74c-12c06ec4dfdf.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001205926467_a2f21f80-1ebc-4021-8bb8-a058dd152a2d.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001206637090_d5448f1d-a26c-466c-9c70-298f2360401d.jpg",
            "https://onevietnam.blob.core.windows.net/57c43ba627d14325242184cc/1636081001207314058_eefe20b7-88ca-445a-ad97-0f0a200a81e8.jpg"
        };

        // for test only
        // change post title to name of type, eg: posttitle = "title", postype = 5 ( xach tay), => posttitle=xach tay
        private Dictionary<int, string> dictionary = new Dictionary<int, string>();
        private int[] keys = { 3, 4, 5, 6, 7, 8, 9 };
        private string[] values =
        {
             "Nhà ở", "Việc làm", "Cho đồ", "Xách tay", "Mua bán", "Cần giúp đỡ", "Cảnh báo"
        };
        public async Task<ActionResult> AutoGeneratePost()
        {
            // Define number of random posts
            var numberOfPost = 20000;

            var p = await PrepareData();
            var r = new Random();
            // Do the action for numberOfPost times
            for (var i = numberOfPost; i > 0; i--)
            {
                await CreateOnePost(p, r);
            }
            return RedirectToAction("Index", "Newsfeed");
        }

        private async Task CreateOnePost(CreatePostViewModel p, Random r)
        {
            var post = new Post(p)
            {
                CreatedDate = System.DateTime.Now,
                UserId = User.Identity.GetUserId()
            };

            // try to fake data for Post
            var rIndex = 0;

            post = new Post(p)
            {
                CreatedDate = System.DateTime.Now,
                UserId = User.Identity.GetUserId()
            };

            // fake location
            // create a random number betwwen 0 and  2147483647
            rIndex = r.Next(0, Int32.MaxValue);

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

            // assign the fake location to post
            post.PostLocation = postLocation;

            // random post type
            post.PostType = r.Next(3, 10);
            // for only test
            post.Title = dictionary[post.PostType].ToString();
            //r.Next(0, _arrRawDesciptions.Count);
            rIndex = post.PostType - 3; // mapping postype from 3 to 9  and the 2-dimentional array: strRawDescription 
            var strDescriptions = _strRawDescriptions[rIndex, r.Next(0, 2)]; // [x,y]: x_post type index, y: 0..number of data
            // fake description : random string
            post.Description = strDescriptions;

            // random images, each post has only 2 random images.
            var illList = new List<Illustration>();

            rIndex = r.Next(0, _arrRawImages.Count);
            var newIll = new Illustration(_arrRawImages[rIndex].ToString());
            illList.Add(newIll);

            rIndex = r.Next(0, _arrRawImages.Count);
            newIll = new Illustration(_arrRawImages[rIndex].ToString());
            illList.Add(newIll);

            post.Illustrations = illList;
            // insert one post into database
            await PostManager.CreateAsync(post);
        }

        private async Task<CreatePostViewModel> PrepareData()
        {
            // initial data
            // - create description list
            //foreach (var str in _strRawDescriptions)
            //{
            //    _arrRawDesciptions.Add(str);
            //}

            // - create location list
            foreach (var str in _strRawLocations)
            {
                _arrRawLocations.Add(str);
            }
            // -create photo list - illustration
            foreach (var str in _strRawImages)
            {
                _arrRawImages.Add(str);
            }
            // For only test
            // create dictionary
            for (var index = 0; index < 7; ++index)
            {
                dictionary.Add(keys[index], values[index]);
            }

            // Create ViewModel
            // all of the commands below are trying to simlate command in CreatePost();
            // need to map the code below with the lastest in master.
            var p = new CreatePostViewModel();

            ViewData.Clear();

            var tagList = await PostManager.AddAndGetAddedTags(Request, TagManager, "TagsInput");
            _illustrationList = (HttpFileCollectionBase)Session["Illustrations"];
            if (tagList != null)
            {
                p.Tags = tagList;
            }
            return p;
        }

        #endregion
        // End comment TraNT: AutoCreatePost
    }
}