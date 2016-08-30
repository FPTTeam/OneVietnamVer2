using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using OneVietnam.BLL;
using OneVietnam.Common;
using OneVietnam.DAL;
using OneVietnam.DTL;
using OneVietnam.Models;

namespace OneVietnam
{
    [Authorize]
    public class OneHub : Hub
    {
        public ApplicationUserManager UserManager { get; private set; }

        public OneHub(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }
        public async Task SendChatMessage(string friendId, string message)
        {
            var name = Context.User.Identity.Name;
            var id = Context.User.Identity.GetUserId();
            var avatar = ((ClaimsIdentity)Context.User.Identity).FindFirst("Avatar").Value;
            var friend = await UserManager.FindByIdAsync(friendId);
            // Store Message To Database       
            await UserManager.AddMessage(Context.User.Identity.GetUserId(), friendId, message);
            var connection = friend.Connections;
            if (connection != null)
            {
                foreach (var conn in connection.Where(conn => conn.Connected == true))
                {
                    // call client's javascript function 
                    Clients.Client(conn.ConnectionId).addChatMessage(id, name, message, avatar);
                }
            }                 
        }

        public async Task PushNotification(CommentViewModel comment)
        {
            var friend = await UserManager.FindByIdAsync(comment.UserId);
            var connection = friend.Connections;
            if (connection != null)
            {
                foreach (var conn in connection)
                {
                    // call client's javascript function 
                    Clients.Client(conn.ConnectionId).pushNotification();
                }
            }
            var description = comment.Commentor + Constants.CommentDescription + "\"" + comment.Title + "\"";
            var notice = new Notification(comment.Url, comment.Avatar, description);
            if (friend.Notifications == null)
            {
                friend.Notifications = new SortedList<string, Notification>();
            }
            friend.Notifications.Add(notice.Id, notice);
            await UserManager.UpdateAsync(friend);

        }        
        public override async Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId();
            var conn = new Connection
            {
                ConnectionId = Context.ConnectionId,
                Connected = true,
                UserAgent = Context.Request.Headers["User-Agent"]
            };                        
            await UserManager.AddConnection(userId, conn);
            await base.OnConnected();
        }
        public override async Task OnDisconnected(bool stopCalled)
        {
            var userId = Context.User.Identity.GetUserId();
            var connectionId = Context.ConnectionId;
            await UserManager.DisConnection(userId, connectionId);
            await base.OnDisconnected(stopCalled);
        }
    }
}