using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DataLinker.Web.Models.Users
{
    public class RegisterModel: DataLinker.Models.UserOrganizationModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Remote("IsEmailNotExists", "Account", ErrorMessage = "Email already in use")]
        public new string Email { get; set; }

        [Required]
        [Display(Name = "Name")]
        [Remote("IsOrganizationExists", "Organization", ErrorMessage = "Organization name already in use")]
        public new string OrganizationName { get; set; }
    }
}