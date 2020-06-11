using System;
using System.ComponentModel.DataAnnotations;
using DataLinker.Models.Enums;

namespace DataLinker.Models
{
    public class LicenseClauseTemplateModel
    {
        private const string LegalRegEx = @"(((\w+)*(\s+)*([_+.,!@#$%^&*();\\/|<>""'.-])*)*)({})?(((\w+)*(\s+)*([_+.,!@#$%^&*();\\/|<>""'.-])*)*)";
        
        public int ID { get; set; }

        public int LicenseSectionId { get; set; }

        public int LicenseClauseId { get; set; }

        [Required]
        [Display(Name = "Short Text")]
        public string ShortText { get; set; }
        
        public string Description { get; set; }

        public string SectionName { get; set; }

        [Required]
        [RegularExpression(LegalRegEx, ErrorMessage = "The field Legal Text can contain only one block {{}}. This block would be replaced by input on 'Build data agreement' screen.")]
        [Display(Name = "Legal Text")]
        public string LegalText { get; set; }

        public int Version { get; set; }

        public TemplateStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsDraft => Status == TemplateStatus.Draft;
        
        public bool IsActive => Status == TemplateStatus.Active;
        
        public bool IsRetracted => Status == TemplateStatus.Retracted;
    }
}