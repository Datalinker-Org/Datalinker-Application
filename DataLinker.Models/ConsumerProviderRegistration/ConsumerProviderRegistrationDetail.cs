using DataLinker.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Models.ConsumerProviderRegistration
{
    public class LicenseClauseVm
    {
        public string Title;
        public string LegalText;
        public string ShortText;
        public string OrgText;
    }

    public class ProviderVm
    {
        public int ApplicationId;
        public string ApplicationName;
        public int LicenseId;
        public ConsumerProviderRegistrationStatus Status;
        public string Remarks;
        public List<LicenseClauseVm> LicenseClauses;
    }

    public class SchemaProviderVm 
    {
        public int SchemaId { get; set; }
        public string SchemaName { get; set; }
        public List<ProviderVm> Providers { get; set; }
    }

    public class ConsumerProviderRegistrationDetail
    {
        public int? ID { get; set; }

        public int? ConsumerApplicationID { get; set; }

        public string ConsumerApplicationName { get; set; }
        
        public int? ProviderApplicationID { get; set; }

        public string ProviderApplicationName { get; set; }

        public int? OrganizationLicenseID { get; set; }

        public int? Status { get; set; }

        public int? SchemaID { get; set; }

        public string SchemaName { get; set; }
    }
}
