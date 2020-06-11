using System;
using System.ComponentModel.DataAnnotations;
using DataLinker.Models.Enums;

namespace DataLinker.Models
{
    public class SchemaModel
    {
        public int DataSchemaID { get; set; }
        
        public string Name { get; set; }
        
        public string PublicId { get; set; }
        
        public string Description { get; set; }

        public int Version { get; set; }

        public TemplateStatus Status { get; set; }

        public PublishStatus LicenseStatus { get; set; }

        [Display(Name = "Date Published")]
        public DateTime? PublishedAt { get; set; }

        public int SchemaFileId { get; set; }

        [Display(Name = "Is Industry Good?")]
        public bool IsIndustryGood { get; set; }

        [Display(Name = "Is Aggregate?")]
        public bool IsAggregate { get; set; }
    }
}