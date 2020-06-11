using System;
using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class NewApplicationDetails: ApplicationAuthenticationDetails
    {
        // New Application
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        
        [Display(Name = "Description")]
        public string Description { get; set; }

        public string AppType { get; set; }

        public bool IsProvider { get; set; }

        public string AppTypeDescription => IsProvider ? "service" : "application";

        [Display(Name = "Register this application as industry good.")]
        public bool IsIntroducedAsIndustryGood { get; set; }

        // App Token
        [Required]
        [Display(Name = "Host")]
        [RegularExpression(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)",ErrorMessage = "The Host field is not a valid fully-qualified http, https")]
        public string OriginHosts { get; set; }
    }
}