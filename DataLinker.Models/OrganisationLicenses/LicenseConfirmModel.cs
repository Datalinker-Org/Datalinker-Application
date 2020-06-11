using DataLinker.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class LicenseConfirmModel
    {
        public LicenseConfirmModel() { }

        public int? ID { get; set; }
        
        [Display(Name = "License Text")]
        public string LicenseContent { get; set; }

        public string OrganizationName { get; set; }

        public bool IsProvider { get; set; }
        
        public OrganisationLicenseType Type { get; set; }

        public int? LicenseFileID { get; set; }
    }
}