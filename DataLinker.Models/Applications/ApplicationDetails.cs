using DataLinker.Models.ConsumerProviderRegistration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class ApplicationDetails
    {        
        public int ID { get; set; }

        public int OrganizationID { get; set; }
        
        public string Name { get; set; }

        [Display(Name = "Organisation Name")]
        public string OrganizationName { get; set; }
        
        public string Description { get; set; }

        public Guid PublicID { get; set; }

        [Display(Name = "Type")]
        public bool IsProvider { get; set; }

        [Display(Name = "Is Industry Good")]
        public bool IsIntroducedAsIndustryGood { get; set; }

        public bool IsVerifiedAsIndustryGood { get; set; }
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Updated At")]
        public DateTime LastUpdatedAt { get; set; }
        
        [Required]
        [Display(Name = "Host")]
        public string Host { get; set; }

        public bool IsIndustryGood => IsIntroducedAsIndustryGood && IsVerifiedAsIndustryGood;

        public List<ProviderEndpointModel> Endpoints { get; set; }

        public List<ApplicationTokenDetails> Hosts { get; set; }

        public ApplicationAuthenticationDetails AuthDetails { get; set; }

        public List<SchemaModel> Schemas { get; set; }

        public bool AreSchemasPresent { get; set; }

        public bool IsLicenseTemplatePresent { get; set; }
        public List<ConsumerProviderRegistrationDetail> RegistrationDetails { get; set; }
    }
}