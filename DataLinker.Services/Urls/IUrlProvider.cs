
namespace DataLinker.Services.Urls
{
    public interface IUrlProvider
    {
        string ToDownloadSchema(int schemaFileId);

        string ToLicenseVerification(int appId, int schemaId, string token);

        string ToDownloadLicense(int appId, int schemaId, int id);

        string ToDownloadLicenseView(int appId, int schemaId, int id);

        string ToOrganisationUsers(int organisationId);

        string ToOrganisationDetails(int organisationId);

        string ToSetupUserCredentials(string token);

        string ToConfirmEmailAddress(string token);
    }
}