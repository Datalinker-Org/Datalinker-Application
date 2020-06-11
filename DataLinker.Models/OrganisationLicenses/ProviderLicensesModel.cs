using System.Collections.Generic;

namespace DataLinker.Models
{
    public class ProviderLicensesModel
    {
        public List<ProviderLicenseModel> Items { get; set; }

        public bool AnyInVerificationProcess { get; set; }

        public bool AnyPublished { get; set; }

        public string SchemaName { get; set; }
        
        public bool IsProvider { get; set; }
    }
}
