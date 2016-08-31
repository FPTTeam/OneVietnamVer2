using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace OneVietnam
{
    public static class Constants
    {
        public const int ResultMaximumNumber = 7;
        public const int DescriptionMaxLength = 200;
        public const int MessagePreviewMaxLength = 35;
        public const int TitleMaxLength = 100;
        public const int LimitedNumberDisplayUsers = 7;
        public const int LimitedNumberOfPost = 5;
        public const string IconTypeGender = "gender";
        public const string IconTypePost = "postType";
        public const string DefaultAvatarLink = "/Content/Images/Avatar_Default.jpg";
        public const string DefaultCoverLink = "/Content/Images/Cover_default.jpg";
        public const string CommentDescription = " đã bình luận bài : ";
        public const string AdminNotification = "OneVietnam Đã đăng bài : ";
        public const string accessTokenFacebook = "EAAW9j1nWUtoBACXdsFz953I7xEzTS3TtYfWKC00MVF0alKlEqYcz8uhJDVWZBn4eRMoe7fZAlmUglRMZAI72b0QvUzKJm3ynrNFf4tzexK3orOTeOJNZCO9bLz12Ch7otiNjZCFwvzlci4PSwmYfPKuTNpJECYXYl0BIDAembLwZDZD";

    }
    public static class CustomRoles
    {
        public const string Admin = "Admin";
        public const string Mod = "Mod";        
    }
}