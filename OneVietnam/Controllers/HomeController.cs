using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MongoDB.Driver;
using OneVietnam.BLL;
using OneVietnam.DTL;
using OneVietnam.Models;

namespace OneVietnam.Controllers
{
    [Authorize]
    public class HomeController : Controller
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
        [AllowAnonymous]
        public ActionResult Index()
        {
            return RedirectToAction("ShowMap", "Map");
        }
        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        [AllowAnonymous]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Chat()
        {
            return View();
        }

        public async Task<JsonResult> GetConversationById(string friendId)
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var messagesList = new List<Message>();                        
            if (!user.Conversations.ContainsKey(friendId))
            {
                return Json(messagesList, JsonRequestBehavior.AllowGet);
            }
            var friendConversation = user.Conversations[friendId];
            if (!friendConversation.Seen)
            {
                friendConversation.Seen = true;
                await UserManager.UpdateAsync(user);
            }
            messagesList = friendConversation.MessageList;
            return Json(messagesList, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetConversations()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var conversationList = new List<ConversationViewModel>();
            if(user.Conversations==null) return Json(conversationList, JsonRequestBehavior.AllowGet);
            var conversations = user.Conversations.OrderByDescending(c => c.Value).ToList();            

            for (var i = 0; i < conversations.Count(); i++)
            {
                var friend = await UserManager.FindByIdAsync(conversations[i].Key);
                if(friend==null) continue;                
                var con = new ConversationViewModel
                {
                    Id = conversations[i].Key,
                    FriendName = friend.UserName,
                    Avatar = friend.Avatar,
                    UpdatedDate = Utilities.GetTimeInterval(conversations[i].Value.UpdatedDate),
                    LastestMessage = conversations[i].Value.LastestMessage.Truncate(Constants.MessagePreviewMaxLength),
                    LastestType = conversations[i].Value.LastestType,
                    Seen = conversations[i].Value.Seen
                };


                conversationList.Add(con);
            }

            return Json(conversationList, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetNotifications()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            List<NotificationModel> list = new List<NotificationModel>();
            var notifications = user.Notifications;
            if (notifications == null) return Json(list, JsonRequestBehavior.AllowGet);
            int updatedCount = 0;            
            for (int i = 0; i < notifications.Count; i++)
            {
                var not = new NotificationModel
                {
                    Id=notifications.Values[i].Id,
                    Url = notifications.Values[i].Url,
                    Avatar = notifications.Values[i].Avatar,
                    Description = notifications.Values[i].Description,
                    Seen = notifications.Values[i].Seen
                };
                if (notifications.Values[i].Seen == false)
                {
                    notifications.Values[i].Seen = true;
                    updatedCount++;
                }                
                not.CreatedDate = Utilities.GetTimeInterval(notifications.Values[i].CreatedDate);
                list.Add(not);
            }
            if (updatedCount > 0)
            {
                await UserManager.UpdateAsync(user);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public async Task<bool> RemoveNotificationById(string id)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var result = user.Notifications.Remove(id);
            await UserManager.UpdateAsync(user);
            return result;
        }

        public async Task<int> GetNotificationsNo()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            return user.CountUnReadNotifications();
        }
        public async Task<JsonResult> GetMessageNo(string id)
        {
            var user = await UserManager.FindByIdAsync(id);
            int count = user.CountUnReadConversations();
            return Json(count, JsonRequestBehavior.AllowGet);
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

        public async Task<bool> RemoveConversationById(string id)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            var result = user.Conversations.Remove(id);
            await UserManager.UpdateAsync(user);
            return result;
        }
    }
}