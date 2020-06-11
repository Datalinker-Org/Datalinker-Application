using DataLinker.Database.Models;
using DataLinker.Models;
using System.IO;

namespace DataLinker.Services.CustomLicenses
{
    public interface ICustomLicenseService
    {
        CustomLicense Add(int applicationId, int schemaId, MemoryStream stream, string mimeType, string fileName, LoggedInUserDetails user);

        CustomLicense Get(int applicationId, int schemaId, int organisationLicenseId, LoggedInUserDetails user);
    }
}
