using System.ComponentModel.DataAnnotations;

namespace WebsiteCreatorMVC.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }

    public class MoneyViewModel
    {
        [Display(Name = "AsMoney account")]
        public string AsMoneyAccount { get; set; }

        [Display(Name = "Bitcoin address")]
        public string BitcoinAddress { get; set; }

        [Display(Name = "Litecoin address")]
        public string LitecoinAddress { get; set; }

        [Display(Name = "PerfectMoney account")]
        public string PerfectMoney { get; set; }

    } // MoneyViewModel

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Email")]        
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "AsMoney account")]
        public string AsMoneyAccount { get; set; }

        [Display(Name = "Bitcoin address")]
        public string BitcoinAddress { get; set; }
        
        [Display(Name = "Litecoin address")]
        public string LitecoinAddress { get; set; }

        [Display(Name = "PerfectMoney account")]
        public string PerfectMoney { get; set; }
    }
}
