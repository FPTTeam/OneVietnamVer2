using System;
using System.Collections.Generic;
using OneVietnam.DTL;
using System.ComponentModel.DataAnnotations;
using OneVietnam.Common;

namespace OneVietnam.Models
{
    public class CreatePostViewModel
    {

        [Required(ErrorMessage = "{0} đang để trống.")]
        [DataType(DataType.Text)]
        [StringLength(320, ErrorMessage = "Nhập tiêu đề trong khoảng 3 đến 320 ký tự.", MinimumLength = 3)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0} đang để trống.")]        
        [Display(Name = "Người đăng")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "{0} chưa được chọn.")]        
        [Display(Name = "Loại bài đăng")]
        public int PostType { get; set; }
        [Required(ErrorMessage = "Không lấy được vị trí của trình duyệt")]
        [Display(Name = "Địa chỉ liên hệ")]        
        public Location PostLocation { get; set; }

        [Required(ErrorMessage = "{0} đang để trống.")]
        [DataType(DataType.Text)]
        [StringLength(5000, ErrorMessage = "Nhập mô tả trong khoảng 3 đến 5000 ký tự.", MinimumLength = 3)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        public List<Illustration> Illustrations { get; set; }

        public List<Tag> Tags { get; set; }    

    }
    public class PostViewModel
    {
        public string Id { get; set; }     

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.Text)]
        [Display(Name = "Người đăng")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.Text)]
        [Display(Name = "Người đăng")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "{0} đang để trống.")]
        [DataType(DataType.Text)]
        [StringLength(320, ErrorMessage = "Nhập tiêu đề trong khoảng 3 đến 320 ký tự.", MinimumLength = 3)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        public string AvartarLink { get; set; }

        [Required(ErrorMessage = "{0} đang để trống.")]
        [DataType(DataType.Text)]
        [StringLength(5000, ErrorMessage = "Nhập mô tả trong khoảng 3 đến 5000 ký tự.", MinimumLength = 3)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.DateTime, ErrorMessage = "{0} không đúng định dạng thời gian")]
        [Display(Name = "Ngày tạo")]
        public DateTimeOffset CreatedDate { get; set; }

        [Required(ErrorMessage = "{0} chưa được chọn.")]        
        [Display(Name = "Loại bài đăng")]
        public int PostType { get; set; }

        [Required(ErrorMessage = "{0} chưa được chỉ định.")]        
        [Display(Name = "Cờ xác nhận xóa bài")]
        public bool DeletedFlag { get; set; }

        [Required(ErrorMessage = "{0} chưa được chỉ định.")]
        [Display(Name = "Tình trạng bài đăng")]
        public bool Status { get; set; }

        public string TimeInterval { get; set; }

        [Required(ErrorMessage = "{0} chưa được chỉ định.")]
        [Display(Name = "Cờ xác nhận khóa bài")]
        public bool LockedFlag { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [Display(Name = "Địa chỉ bài đăng")]
        public Location PostLocation { get; set; }
        public List<Illustration> Illustrations { get; set; }
        public List<Tag> Tags { get; set; }

        public PostViewModel()
        {
        }
        public PostViewModel(Post post)
        {
            Id = post.Id;
            UserId = post.UserId;     
            Title = post.Title;
            Description = post.Description;
            CreatedDate = post.CreatedDate;
            TimeInterval = Utilities.GetTimeInterval(post.CreatedDate);
            PostType = post.PostType;
            DeletedFlag = post.DeletedFlag;
            Status = post.Status;
            LockedFlag = post.LockedFlag;           
            PostLocation = post.PostLocation;
            Tags = post.Tags;
            Illustrations = post.Illustrations;
        }
        public PostViewModel(Post post, string pUserName,string postUserAvatar)
        {
            AvartarLink = postUserAvatar;
            Id = post.Id;
            UserId = post.UserId;
            UserName = pUserName;
            Title = post.Title;
            Description = post.Description;
            CreatedDate = post.CreatedDate;
            TimeInterval = Utilities.GetTimeInterval(post.CreatedDate);
            PostType = post.PostType;
            DeletedFlag = post.DeletedFlag;
            Status = post.Status;
            LockedFlag = post.LockedFlag;
            PostLocation = post.PostLocation;
            Tags = post.Tags;
            Illustrations = post.Illustrations;
        }
    }

    public class CreateAdminPostViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.Text)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.Text)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        public List<Illustration> Illustrations { get; set; }

        public CreateAdminPostViewModel() { }

        public CreateAdminPostViewModel(Post post)
        {
            Id = post.Id;
        }
    }

    public class AdminPostViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]        
        [Display(Name = "Người đăng")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.Text)]
        [Display(Name = "Người đăng")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.Text)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.Text)]
        [Display(Name = "Nội dung")]
        public string Description { get; set; }

        [Required(ErrorMessage = "{0} chưa được điền.")]
        [DataType(DataType.DateTime, ErrorMessage = "{0} không đúng định dạng thời gian")]
        [Display(Name = "Ngày tạo")]
        public DateTimeOffset CreatedDate { get; set; }

        [Required(ErrorMessage = "{0} chưa được chọn.")]        
        [Display(Name = "Loại bài đăng")]
        public int PostType { get; set; }

        [Required(ErrorMessage = "{0} chưa được chỉ định.")]
        [Display(Name = "Cờ xác nhận xóa bài")]
        public bool DeletedFlag { get; set; }

        [Required(ErrorMessage = "{0} chưa được chỉ định.")]
        [Display(Name = "Tình trạng bài đăng")]
        public bool Status { get; set; }
       
        public List<Illustration> Illustrations { get; set; }

        public AdminPostViewModel()
        {

        }
        public AdminPostViewModel(Post pPost)
        {
            Id = pPost.Id;
            UserId = pPost.UserId;
            Title = pPost.Title;
            PostType = pPost.PostType;
            Description = pPost.Description;
            CreatedDate = pPost.CreatedDate;
            Status = pPost.Status;
            DeletedFlag = pPost.DeletedFlag;
            if(pPost.Illustrations != null && pPost.Illustrations.Count > 0)
            {
                Illustrations = pPost.Illustrations;
            }
            
        }

        public AdminPostViewModel(PostViewModel pPost)
        {
            Id = pPost.Id;
            UserId = pPost.UserId;
            Title = pPost.Title;
            PostType = pPost.PostType;
            Description = pPost.Description;
            CreatedDate = pPost.CreatedDate;
            Status = pPost.Status;
            DeletedFlag = pPost.DeletedFlag;
            if (pPost.Illustrations != null && pPost.Illustrations.Count > 0)
            {
                Illustrations = pPost.Illustrations;
            }
        }
    }
}