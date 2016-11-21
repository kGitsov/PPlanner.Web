using PPlanner.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Security.AccessControl;
using System.Web.Security;

namespace PPlanner.Models
{

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Това поле е задължително за попълване.")]
        [DataType(DataType.Password)]
        [Display(Name = "Текуща парола")]
        public string OldPassword { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Това поле е задължително за попълване.")]
        [StringLength(100, ErrorMessage = "Полето {0} е необходимо да бъде поне {2} символа дълга", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Нова парола")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Потвърди парола")]
        [Compare("NewPassword", ErrorMessage = "Паролите не съвпадат")]
        public string ConfirmPassword { get; set; }


    }

    public class UserDetails
    {
        [Display(Name = "User name")]
        public string UserName { get; set; }
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
    public class UserRoles
    {
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "Моля въведете стойност.")]
        [Display(Name = "UserName")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Моля въведете стойност.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterModel
    {
        [Required(ErrorMessage = "Моля въведете потребителско име.")]
        [StringLength(30, ErrorMessage = "Моля въведете парола с дължина минимум {1} символа", MinimumLength = 3)]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Моля въведете парола.")]
        [StringLength(100, ErrorMessage = "Моля въведете парола с дължина минимум {1} символа", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Въведените пароли не съответстват.")]
        public string ConfirmPassword { get; set; }
               
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }
}
