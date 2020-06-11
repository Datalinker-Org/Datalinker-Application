using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DataLinker.Models.Enums;

namespace DataLinker.Models
{
    public class ProviderEndpointModel
    {     
        public int ID { get; set; }
        
        public string Name { get; set; }

        [Required]
        [Url]
        [Display(Name = "URL")]
        public string Uri { get; set; }
        
        public string Description { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? LastUpdatedAt { get; set; }

        [Display(Name = "Schema")]
        public Dictionary<string, string> Schemas { get; set; }

        public int SelectedSchemaID { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Schema ID")]
        public string SchemaPublicID { get; set; }

        public SchemaModel Schema { get; set; }

        public PublishStatus LicenseStatus { get; set; }
    }
}