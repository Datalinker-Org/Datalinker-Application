using DataLinker.Models;
using System.Collections.Generic;

namespace DataLinker.Services.FileProviders
{
    public interface ILicenseFileProvider
    {
        CustomFileDetails GetLicenseForDownload(int appId, int schemaId, int licenseId, LoggedInUserDetails user);

        CustomFileDetails GetTemplatedLicenseForPreview(List<SectionsWithClauses> model, int orgId, int schemaId, LoggedInUserDetails user);

        CustomFileDetails GetAgreement(int agreementId, LoggedInUserDetails user);

        CustomFileDetails GetLicense(int organizationLicenseId);

        CustomFileDetails GetLicense(int organizationLicenseId, int providerOrganizationId, int consumerOrganizationId);
    }
}