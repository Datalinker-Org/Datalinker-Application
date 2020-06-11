using System;
using System.Collections.Generic;

namespace DataLinker.Models
{
    public class SectionModel
    {
        public IReadOnlyList<LicenseClauseTemplateModel> ClauseTemplates;
        
        public int ID { get; set; }

        public string Title { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public int CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }
    }
}