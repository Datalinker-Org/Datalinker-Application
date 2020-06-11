using System.Collections.Generic;

namespace DataLinker.Models
{
    public class BuildLicenseModel
    {
        public string SchemaName { get; set; }

        public bool IsProvider { get; set; }

        public bool IsPublishedProviderLicensePresent { get; set; }

        public List<SectionsWithClauses> Sections { get; set; }

        public List<DataProvider> Providers { get; set; }
    }
}