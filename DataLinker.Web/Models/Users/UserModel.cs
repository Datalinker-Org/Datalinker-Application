using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using DataLinker.Models;

namespace DataLinker.Web.Models.Users
{
    public class UserModel : UserDetailsModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Remote("IsEmailNotExists", "Account", ErrorMessage = "Emails already in use", AdditionalFields = "InitialEmail")]
        [Display(Name = "E-Mail")]
        public new string Email { get; set; }
    }
}