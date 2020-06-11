using DataLinker.Models;

namespace DataLinker.Services.LicenseVerification
{
    public interface ILicenseVerificationService
    {
        LicenseConfirmModel GetConfirmModel(string token, LoggedInUserDetails user);

        void Approve(int licenseId, string urlToLicenses, LoggedInUserDetails user);

        void Decline(int licenseId, string urlToLicenses, LoggedInUserDetails user);

        LicenseConfirmModel GetLicenseForProviderAndSchema(int providerId, int schemaId, LoggedInUserDetails user);

        void ConsumerApprove(int licenseId, string urlToLicenses, LoggedInUserDetails user);

        void ConsumerDecline(int licenseId, string urlToLicenses, LoggedInUserDetails user);
    }
}
