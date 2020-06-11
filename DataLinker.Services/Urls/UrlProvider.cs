using DataLinker.Services.Configuration;

namespace DataLinker.Services.Urls
{
    internal class UrlProvider :IUrlProvider
    {
        private readonly IConfigurationService _config;

        public UrlProvider(IConfigurationService config)
        {
            _config = config;
        }

        public string ToDownloadSchema(int schemaFileId)
        {
            return $"{_config.DataLinkerHost}/data-schemas/{schemaFileId}/download";
        }

        public string ToLicenseVerification(int appId, int schemaId, string token)
        {
            var url = $"{_config.DataLinkerHost}/applications/{appId}/schemas/{schemaId}/license-verification?token={token}";
            return url;
        }

        public string ToDownloadLicense(int appId, int schemaId, int id)
        {
            var result = $"{_config.DataLinkerHost}/applications/{appId}/schemas/{schemaId}/provider-licenses/{id}/download";
            return result;
        }
        public string ToDownloadLicenseView(int appId, int schemaId, int id)
        {
            var result = $"{_config.DataLinkerHost}/applications/{appId}/schemas/{schemaId}/provider-licenses/{id}/downloadview";
            return result;
        }

        public string ToOrganisationUsers(int organisationId)
        {
            var result = $"{_config.DataLinkerHost}/organisations/{organisationId}/users";
            return result;
        }

        public string ToOrganisationDetails(int organisationId)
        {
            var result = $"{_config.DataLinkerHost}/organisations/{organisationId}/dashboard";
            return result;
        }
        public string ToSetupUserCredentials(string token)
        {
            var result = $"{_config.DataLinkerHost}users/confirm-email-address?token={token}";
            return result;
        }

        public string ToConfirmEmailAddress(string token)
        {
            var result = $"{_config.DataLinkerHost}/users/change-email-address?token={token}";
            return result;
        }
    }
}