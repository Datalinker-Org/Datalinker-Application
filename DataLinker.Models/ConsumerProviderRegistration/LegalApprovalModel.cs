using DataLinker.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Models.ConsumerProviderRegistration
{
    public class LegalApprovalModel
    {
        public int? ID { get; set; }

        public int? ConsumerProviderRegistrationID { get; set; }

        public string LicenseContent { get; set; }

        public string OrganizationName { get; set; }

        public bool IsProvider { get; set; }

        public OrganisationLicenseType Type { get; set; }

        public int? LicenseFileID { get; set; }
    }
}
