using DataLinker.Database.Models;

namespace DataLinker.Models
{
    public class LicenseCheckResult
    {
        public OrganizationLicense License { get; set; }

        public LicenseApprovalRequest VerificationRequest { get; set; }

        public User LegalOfficer { get; set; }
    }
}
