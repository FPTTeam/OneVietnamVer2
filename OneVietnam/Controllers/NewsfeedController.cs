
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

        // TraNT: Auto create post
        #region AutoGeneratePosts
        private readonly ArrayList _arrRawDesciptions = new ArrayList();
        private readonly String[] _strRawDescriptions =
        {
            "Chiều ngày 17/7/2016, Hội đồng bầu cử quốc gia"
            ," đã họp đột xuất Phiên thứ tám tại Nhà Quốc hội"
            ,"Thủ đô Hà Nội dưới sự chủ trì của Chủ tịch Quốc hội,"
            ," Chủ tịch Hội đồng bầu cử quốc gia Nguyễn Thị Kim Ngân."
            ,"Tham dự Phiên họp có các thành viên Hội đồng bầu cử quốc gia"
            ,"; đại diện thường trực Ủy ban trung ương Mặt trận tổ quốc Việt Nam"
            ,"Đại diện lãnh đạo Ban tổ chức trung ương, Văn phòng Trung ương Đảng, "
            ,"Văn phòng Quốc hội, Ban công tác đại biểu, đại diện các "
            ,"Tiểu ban giúp việc của Hội đồng bầu cử quốc gia."
            ,"Với tinh thần nghiêm túc, trách nhiệm, Hội đồng bầu cử quốc"
            ," gia đã thận trọng xem xét, xác nhận tư cách đại biểu Quốc hội khóa XIV."
            ,"Tại phiên họp này, Hội đồng bầu cử quốc gia đã tiến hành"
            ," biểu quyết bằng hình thức bỏ phiếu kín để xác nhận tư cách"
            ," người trúng cử đại biểu Quốc hội khóa XIV đối 494 đại biểu."
            ,"Bên cạnh đó, Hội đồng bầu quốc gia cũng tiến hành bỏ phiếu kín "
            ,"không xác nhận tư cách đại biểu Quốc hội khóa XIV đối với "
            ,"Bà Nguyễn Thị Nguyệt Hường, đại biểu Quốc hội khóa XIII."
            ,"Kết quả kiểm phiếu, có 100% các thành viên trong Hội đồng "
            ,"bầu cử quốc gia có mặt tại phiên họp nhất trí xác nhận"
            ," tư cách đại biểu Quốc hội khóa XIV đối với 494 đại biểu."
            ,"100% các thành viên Hội đồng bầu cử quốc gia bỏ phiếu không xác "
            ,"nhận tư cách đại biểu Quốc hội khóa XIV đối với bà Nguyễn Thị Nguyệt Hường "
            ,"vì không đủ tiêu chuẩn đại biểu Quốc hội khóa XIV."
            ,"Theo Văn phòng Hội đồng bầu cử quốc gia cũng cho biết, cá nhân bà "
            ,"Hường có đơn xin rút không tham gia đại biểu Quốc hội khóa XIV."
            ,"Bà Nguyệt Hường ứng cử tại đơn vị bầu cử số 5 TP.Hà Nội "
            ,"(gồm các huyện Bắc Từ Liêm, Nam Từ Liêm, Đan Phượng, Hoài Đức) và trúng cử với tỷ lệ 78,51%."
            ,"Sinh năm 1970 tại Nam Định, bà Hường là một đại diện của "
            ,"giới doanh nhân tại diễn đàn Quốc hội, là đại biểu Quốc hội khóa XII, XIII."
            ,"Kỳ này, nữ Chủ tịch HĐQT Cty cổ phần đầu tư TNG Holdings "
            ,"Việt Nam tiếp tục được giới thiệu tái cử khóa XIV, là một đại biểu trong "
            ,"khối của UB TƯ Mặt trận tổ quốc Việt Nam."
            ,"Như vậy, cùng với ông Trịnh Xuân Thanh – Tỉnh ủy viên tỉnh Hậu Giang,"
            ," nguyên Phó Chủ tịch UBND tỉnh này không được công nhận tư cách đại biểu Quốc hội khóa XIV"
            ,"Nữ doanh nhân Nguyễn Thị Nguyệt Hường là trường hợp thứ 2 cũng"
            ," không qua được vòng thẩm tra tư cách đại biểu dù đã trúng cử. "
            ,"Quốc hội khóa XIV, đến thời điểm này, có 494 đại biểu."
            ,"Tài năng nhí nổi bật được khán giả cả nước công nhận sẽ khuấy động"
            ," đêm chung kết và khiến bầu không khí trở nên “sục sôi” hơn bao giờ hết."
            ,"Top 4 gồm 4 gương mặt thí sinh nhí xuất sắc nhất và"
            ," được khán giả yêu mến gồm Jayden, Gia Khiêm"
            ,"Hồ Văn Cường và Bảo Trân đang tập luyện nghiêm túc"
            ," để ghi dấu bản thân trong đêm thi cuối cùng."
            ,"Trong đêm Gala Chung kết, mỗi thí sinh sẽ biểu diễn hai ca khúc."
            ,"Trong đó, một ca khúc đã từng được các bé diễn ở các đêm "
            ,"liveshow trước và một ca khúc lần đầu tiên xuất hiện "
            ,"trên sân khấu Idol Kids. Ở đêm thi quan trọng này"
            ,"Các bé sẽ chọn những bài hát mình yêu thích và dòng nhạc sở "
            ,"trường của mình để nhận được những phiếu bầu chọn cuối cùng của khán giả."
            ,"Ngoài các tiết mục biểu diễn cá nhân, top 4 sẽ có "
            ,"một tiết mục chung mở màn cùng các bạn nhỏ trong top 13 gồm:"
            ,"Bảo Khương, Diệp Nhi, Khánh Linh, Thuỳ Anh."
            ,"Top 4 đã có rất nhiều những kỉ niệm, khi ca sĩ Tóc "
            ,"Tiên yêu cầu vui “Kể tật xấu của các bạn trong nhà chung” thì"
            ,"Gia Khiêm “méc” là Bảo Trân nóng tính. Jayden kể Gia Khiêm rất "
            ,"nghịch nhưng vì là “trẻ em” nên dù nghịch vẫn “chấp nhận được”. "
            ,"“Cặp đôi anh em thân thiết” là Hồ Văn Cường và Jayden thì"
            ,"khẳng định “đối phương” không hề có tật xấu nào cả khiến “đôi bên” đều hài lòng"
            ,"PGS-TS Đỗ Văn Dũng, Hiệu trưởng Trường ĐH Sư phạm "
            ,"kỹ thuật TPHCM, đơn vị chủ trì cụm thi tỉnh Bình Thuận, "
            ,"cho biết nhìn chung điểm năm nay đẹp hơn năm trước vì độ phân"
            ," hóa cao hơn, điểm thấp hơn. Năm ngoái mức điểm từ 7-8 nhiều"
            ," quá nên các trường khó chọn nhưng năm nay phổ điểm là "
            ,"5-6 điểm, mức 7-8 điểm ít hơn nên các trường sẽ dễ tuyển hơn. Năm nay điểm cao ít,"
            ," như ở môn Toán và Văn điểm cao nhất chỉ 9,25. "
            ,"Ngược lại điểm liệt của các môn cũng ít hơn. "
            ,"Môn Sử và Địa, mức điểm phổ biến chủ yếu từ 4-6 điểm."
            ,"TS Lê Chí Thông, Trưởng phòng đào tạo trường ĐH Bách khoa "
            ,"(ĐH Quốc gia TPHCM) cũng cho biết nhìn chung điểm "
            ,"thi không cao lắm. Đối với Văn, phổ điểm ở mức từ 4-6, "
            ,"không có bài thi nào đạt điểm 9. Trong khi đó, điểm môn Toán "
            ,"nhỉnh hơn một chút với mức điểm phổ biến từ 5-7 điểm và không có bài thi nào đạt 10 điểm."
            ,"Qua thống kê sơ bộ của cụm thi do trường ĐH Nông lâm "
            ,"TPHCM chủ trì, “Môn Toán mức điểm phổ biến từ 4 đến cận "
            ,"7 điểm chiếm đến gần 50%. Đến hôm qua, bài điểm cao nhất "
            ,"môn toán ở cụm này là 9,25. Môn Văn đã có điểm 9. "
            ,"Trong khi đó, phổ điểm môn Sử thống kê sơ bộ có phần thấp hơn năm ngoái."
            ," Môn Địa xuất hiện bài thi đạt cao nhất là 9,75 điểm, "
            ,"phổ điểm có phần cao hơn môn Sử nhưng cũng không bằng năm ngoái”,"
            ," TS Trần Đình Lý - Trưởng phòng đào tạo trường cho biết."
            ,"Theo Ths Phạm Thái Sơn, Phó trưởng phòng Đào tạo trường ĐH "
            ,"Công nghiệp Thực phẩm TPHCM, đơn vị chủ trì cụm thi tỉnh Tây Ninh,"
            ," Phổ điểm các môn cũng tương đối thấp như môn Toán: 4- 6 điểm, môn Văn: 4- 5,5 điểm; "
            ,"Sử: 3,5- 5 điểm ; Địa: 4- 5,5; Hóa: 4- 5,5; Lý: 4,5- 6; Anh Văn: 3,5- 5. "
            ,"Tuy nhiên, ông Sơn cũng cho biết điểm liệt các môn năm nay rất ít, "
            ,"và phổ điểm phân bố khá chuẩn chứng tỏ đề thi có sự phân loại cao."
            ,"Từ kết quả điểm thi THPT quốc gia 2016, nhiều chuyên gia đã"
            ," nhận định điểm chuẩn vào nhiều ngành, nhiều trường sẽ giảm mạnh so với năm 2015."
            ," Dự báo về điểm chuẩn của Trường ĐH Sư phạm Kỹ thuật TPHCM,"
            ," PGS-TS Đỗ Văn Dũng cho rằng: “Qua nắm tình hình chấm thi của các tỉnh, "
            ,"thành phố cùng với tình hình thí sinh đăng ký xét tuyển ĐH, "
            ,"CĐ giảm, tôi cho rằng điểm chuẩn năm nay của trường sẽ thấp hơn từ 1-2 điểm"
            ,"Tương tự, ông Phạm Thái Sơn cho biết riêng với cụm thi Tây Ninh,"
            ," mức điểm từ 7,5 trở lên không nhiều mà phổ điểm tập trung từ 4-6 điểm ở các môn tự luận. "
            ,"Riêng môn Sử phổ điểm thấp hơn chủ yếu ở 3,5-5 điểm. "
            ,"Ngược lại số điểm liệt rất ít chỉ khoảng 10 bài/môn. "
            ,"Đối với các môn trắc nghiệm, môn Lý và Hóa có nhích hơn một chút so với năm trước, "
            ,"phổ điểm dao động từ 4,5-6 điểm. Với phổ điểm này, đối với các tổ hợp môn truyền thống gồm A, A1, D "
            ,"thì ngưỡng điểm sẽ an toàn mức tương đương như năm ngoái. "
            ,"Nếu điểm chuẩn có giảm thì chủ yếu do lượng thí sinh năm nay ít, "
            ,"thì ngưỡng điểm khả năng cũng sẽ giảm nhưng không quá nhiều."
            ,"Dự đoán, điểm trúng tuyển của các trường tốp trên và tốp dưới"
            ," sẽ không thay đổi nhiều so với năm ngoái. Riêng nhóm trường "
            ,"giữa vốn có điểm chuẩn dao động từ 19-20 điểm "
            ,"thì khả năng điểm chuẩn sẽ có biến động, nhất là các ngành "
            ,"không phải nhóm tổ hợp môn truyền thống khả năng điểm sẽ giảm."
            ,"Điều đáng mừng là đề thi có tính phân hóa tốt nên khi xét tuyển"
            ," các trường sẽ không gặp khó đến mức đưa ra nhiều tiêu chí phụ như năm trước. "
            ,"Ông Sơn cũng lưu ý thí sinh năm nay:"
            ," “Khi xét tuyển nên chú ý ngưỡng điểm xét tuyển của các trường, "
            ,"bên cạnh đó chú ý theo dõi thông tin phân tích của các chuyên gia"
            ," đồng thời đừng nên vội vàng quá khi nộp hồ sơ đăng ký xét tuyển. "
            ,"Năm nay có nhiều hình thức nộp xét tuyển nhưng tốt nhất nên nộp qua đường bưu điện, "
            ,"vừa đỡ mất thời gian, tiền bạc truy cập online. Ở các bưu điện "
            ,"cũng có dịch vụ thu phí đăng ký xét tuyển nên thí sinh sẽ không phải vất vả nhiều"
            ,"Dự đoán ngưỡng điểm chuẩn vào trường ĐH Công nghiệp thực phẩm TPHCM năm "
            ,"nay sẽ giảm hơn năm ngoái từ 1-2 điểm tùy theo ngành."
            ,"Trong khi đó, TS Trần Đình Lý - Trưởng phòng đào tạo trường ĐH Nông lâm TPHCM, "
            ,"chủ trì cụm thi tỉnh Gia Lai cho rằng với phổ điểm năm nay, khả năng điểm chuẩn "
            ,"của trường sẽ bằng hoặc cao hơn năm ngoái một ít. "
            ,"“Trong 3 năm liên tiếp gần đây, điểm chuẩn của trường luôn năm sau cao hơn năm trước."
            ," Một số ngành cao hơn năm trước khoảng 1 điểm”, TS Lý cho hay."
        };
        // Define location
        private int MinLat = 30;
        private int MaxLat = 45;
        private int MinLong = 130;
        private int MaxLong = 149;
        private readonly ArrayList _arrRawLocations = new ArrayList();
        private readonly String[] _strRawLocations =
        {
            "1. Vùng núi Zhangye Danxia, tỉnh Cam Túc, Trung Quốc:"
            ,"2. Nơi ‘tận cùng thế giới’ ở Banos, Ecuador:"
            ,"3. Hố xanh vỹ đại ở Belize:"
            ,"4. Cánh đồng hoa tulip Hà Lan:"
            ,"5. Hang Sơn Đoòng, Quảng Bình, Việt Nam:"
            ,"6. Thiên đường hoa ở công viên Hitachi Seaside:"
            ,"7. Hang động băng Mendenhall, Alaska, Mỹ:"
            ,"8. Ngọn núi Roraima nằm giữa Venezuela, Brazil và Guyana:"
            ,"9. Khu vực Cappadocia, Thổ Nhĩ Kỳ:"
            ,"10. Bãi biển sao trên đảo Vaadhoo, Mandives:"
            ,"11. Thác nước Victoria:"
            ,"12. Trolltunga ở Hordaland, Na Uy:"
            ,"13. Bãi biển Whitehaven, Australia:"
            ,"14. Vườn quốc gia Grand Canyon, bang Arizona, Mỹ:"
            ,"15. Hang động cẩm thạch Marble Cathedral, Chile:"
            ,"16. Đường hầm tình yêu ‘Tunnel of love':"
            ,"17. Cánh đồng muối tuyệt đẹp Salar de Uyuni ở Bolivia:"
            ,"18. Enchanted Well – Chapada Diamantina National Park (Brazil):"
            ,"19. Hẻm núi Antelope, Mỹ:"
            ,"20. Hang động Fingal, Scotland:"
            ,"21. Hồ bơi khổng lồ Tosua Ocean Trench, Samoa:"
            ,"22. Rừng tre Sagano ở Nhật Bản:"
            ,"23. Hang động đom đóm ở New Zealand:"
            ,"24. Cầu thang Haiku, ở Oahu, Hawaii:"
            ,"25. Núi lửa ở bán đảo Kamchatka, Nga:"
            ,"26. Hố sụt ở bán đảo Yucatan, Mexico:"
            ,"27. Hồ đổi màu trên đỉnh núi Kelimutu ở Indonesia:"
        };

        public async Task<ActionResult> AutoGeneratePost()
        {
            // Define number of random posts
            Int32 numberOfPost = 100000;

            // initial data
            // - create description list
            foreach (var str in _strRawDescriptions)
            {
                _arrRawDesciptions.Add(str);
            }

            // - create location list
            foreach (var str in _strRawLocations)
            {
                _arrRawLocations.Add(str);
            }

            // Create ViewModel
            // all of the commands below are trying to simlate command in CreatePost();
            // need to map the code below with the lastest in master.
            CreatePostViewModel p = new CreatePostViewModel();
            p.Title = "0";
            p.Description = "Description";

            ViewData.Clear();
            var post = new Post(p)
            {
                CreatedDate = System.DateTime.Now,
                UserId = User.Identity.GetUserId()
            };
            //p.UserName = User.Identity.Name;
            var tagList = await PostManager.AddAndGetAddedTags(Request, TagManager, "TagsInput");
            _illustrationList = (HttpFileCollectionBase)Session["Illustrations"];
           // var illList = await PostManager.GetIllustration(_illustrationList, post.Id);
            if (tagList != null)
            {
                p.Tags = tagList;
            }

            //if (illList != null)
            //{
            //    p.Illustrations = illList;
            //}

            Random r = new Random();
            // Do the action for numberOfPost times
            for (int i = numberOfPost; i > 0; i--)
            {
                // try to fake data for Post
                //Int32 rIndex = r.Next(0, Int32.MaxValue);
                //rIndex = rIndex % _arrRawDesciptions.Count;

                Int32 rIndex = r.Next(0, _arrRawDesciptions.Count);
                String strDescriptions = _arrRawDesciptions[rIndex].ToString();

                // fake title: index
                p.Title = i.ToString();
                // fake description : random string
                p.Description = strDescriptions;
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
                String strAddress = _arrRawLocations[rIndex].ToString();

                // Declare new location
                Location postLocation = new Location();

                // get the range of Lat
                int range = MaxLat - MinLat;
                // create a random double in range
                double rDouble = r.NextDouble() * range;
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
                //post.PostType = 5;
                post.PostType = r.Next(3, 10);

                // insert one post into database
                await PostManager.CreateAsync(post);
            }

            //CreatedPost = true;
            //PostView = new PostViewModel(post);
            //return RedirectToAction("ShowPostDetail", "Post", new { postId = post.Id });
            return RedirectToAction("Index", "Newsfeed");
        }
        #endregion
        // End comment TraNT: AutoCreatePost
    }
}