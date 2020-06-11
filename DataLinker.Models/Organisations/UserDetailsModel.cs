using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class UserDetailsModel
    {
        public int ID { get; set; }

        public int OrganizationId { get; set; }

        [Display(Name = "E-Mail")]
        public string Email { get; set; }

        [Display(Name = "Organization name")]
        public string OrganizationName { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Phone]
        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Display(Name = "Is legal officer?")]
        public bool IsIntroducedAsLegalOfficer { get; set; }

        [Display(Name = "Is active?")]
        public bool IsActive { get; set; }

        public bool IsLegalOfficer { get; set; }

        public bool IsSingleLegalOfficer { get; set; }
    }
}