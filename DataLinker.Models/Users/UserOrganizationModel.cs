using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class UserOrganizationModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string OrganizationName { get; set; }

        [Required]
        [Display(Name = "Phone")]
        public string OrganizationPhone { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string OrganizationAddress { get; set; }

        [Required]
        [Display(Name = "Administrative Full Name")]
        public string AdministrativeContact { get; set; }

        [Required]
        [Display(Name = "Administrative Phone")]
        public string AdministrativePhone { get; set; }

        [Required]
        [Display(Name = "Terms of Service")]
        public string TermsOfService { get; set; }
        
        [Range(typeof(bool), "true", "true", ErrorMessage = "You should agree with terms!")]
        public bool IsAgreeWithTerms { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You should agree with terms!")]
        public bool IsHaveReadTerms { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You should agree with terms!")]
        public bool IsAuthorised { get; set; }
    }
}