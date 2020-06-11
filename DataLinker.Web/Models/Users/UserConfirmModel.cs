using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DataLinker.Web.Models.Users
{
    public class UserConfirmModel
    {
        public string Token { get; set; }

        [Required]
        [StringLength(18, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d.=]{8,}$", ErrorMessage = "Your password must be a Minimum of 8 characters and must include: 1 Uppercase Alpha, 1 Lowercase Alpha, 1 Number")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [StringLength(18, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d.=]{8,}$", ErrorMessage = "Your password must be a Minimum of 8 characters and must include: 1 Uppercase Alpha, 1 Lowercase Alpha, 1 Number")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and Confirm password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}