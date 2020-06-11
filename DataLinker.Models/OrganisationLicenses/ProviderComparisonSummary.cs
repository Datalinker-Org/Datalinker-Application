using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class ProviderComparisonSummary
    {
        public List<ProviderMatch> Endpoints { get; set; }

        // This field used to display name of property
        [Display(Name = "Organization")]
        public string OrganizationName { get; set; }

        public string SchemaName { get; set; }

        public List<string> SectionNames { get; set; }
    }

    public class ProviderMatch
    {
        public int EndpointId { get; set; }

        public int ProviderLicenseId { get; set; }
        
        public bool IsMatch { get; set; }

        public string OrganizationName { get; set; }

        public string EndpointName { get; set; }
        
        public List<ClauseMatch> Clauses { get; set; }
    }

    public class ClauseMatch
    {
        public string SectionName { get; set; }

        public bool IsMatched { get; set; }
        
        public int ClauseId { get; set; }

        public string Message { get; set; }

        public string Value { get; set; }
    }
}