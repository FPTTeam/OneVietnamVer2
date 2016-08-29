using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;
using OneVietnam.DTL;

namespace OneVietnam.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Địa chỉ email là trường bắt buộc")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
        [Required(ErrorMessage = "Không lấy được vị trí của trình duyệt")]
        [Display(Name = "Vị trí")]
        public string LocationExternal { get; set; }        
        public double XCoordinateExternal { get; set; }        
        public double YCoordinateExternal { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = "Provider là trường bắt buộc")]
        public string Provider { get; set; }

        [Required(ErrorMessage = "Mã xác nhận là trường bắt buộc")]
        [Display(Name = "Mã xác nhận")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Ghi nhớ trình duyệt?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(ErrorMessage = "Địa chỉ email là trường bắt buộc")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Địa chỉ email là trường bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; }        
        [Required(ErrorMessage = "Mật khẩu là trường bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Duy trì đăng nhập")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ và tên là trường bắt buộc")]        
        [StringLength(50,ErrorMessage = "Họ và tên phải có ít nhất {2} kí tự, nhiều nhất {1} kí tự ",MinimumLength = 2)]
        [Display(Name = "Họ và tên")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Địa chỉ email là trường bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Giới tính là trường bắt buộc")]
        [Display(Name = "Giới tính")]
        public int Gender { get; set; }
        [Required(ErrorMessage = "Mật khẩu là trường bắt buộc")]
        [StringLength(25, ErrorMessage = "{0} phải có ít nhất {2} kí tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Yêu cầu xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật Khẩu Xác Nhận Không Trùng Khớp")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "Không lấy được vị trí của trình duyệt")]
        [Display(Name = "Vị trí")]
        public string Address { get; set; }        
        public double XCoordinate { get; set; }        
        public double YCoordinate { get; set; }

    }

    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Địa chỉ email là trường bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là trường bắt buộc")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} kí tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không trùng khớp.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Địa chỉ Email là trường bắt buộc")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
    public class UserViewModel
    {
        public string UserName { get; set; }
        
        public string Email { get; set; }        

        public string PhoneNumber { get; set; }

        public Location Location { get; set; }

        public int Gender { get; set; }

        public string UserId { get; set; }
        public string Avatar { get; set; }
        public string Cover { get; set; }
        public string JoinedDate { get; set; }

        public UserViewModel()
        {

        }

        public UserViewModel(ApplicationUser appUser)
        {
            UserName = appUser.UserName;
            Email = appUser.Email;
            Location = appUser.Location;
            Gender = appUser.Gender;
            PhoneNumber = appUser.PhoneNumber;
            UserId = appUser.Id;
            Avatar = appUser.Avatar;
            JoinedDate = Utilities.GetTimeInterval(appUser.CreatedDate); 
            Cover = appUser.Cover;
        }
    }    
}
