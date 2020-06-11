using System;
using System.ComponentModel.DataAnnotations;
using DataLinker.Models.Enums;

namespace DataLinker.Models
{
    public class LicenseTemplateDetails
    {
        public int? ID { get; set; }

        public int LicenseId { get; set; }

        public int Version { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public TemplateStatus Status { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        [Display(Name = "HTML Template")]
        public string LicenseText { get; set; }

        public bool IsActive { get; set; }

        public bool IsDraft { get; set; }

        public bool IsRetracted { get; set; }
    }
}