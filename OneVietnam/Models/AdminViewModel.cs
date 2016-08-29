using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace OneVietnam.Models
{


    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(AllowEmptyStrings = false,ErrorMessage = "Địa chỉ email là trường bắt buộc")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        public string Email { get; set; }

        public IEnumerable<SelectListItem> RolesList { get; set; }
    }
}