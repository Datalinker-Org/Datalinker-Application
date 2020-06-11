using System.Collections.Generic;
using DataLinker.Models;

namespace DataLinker.Services.LicenseTemplates
{
    public interface ILicenseTemplatesService
    {
        LicenseTemplateDetails GetEditModel(int id, LoggedInUserDetails user);

        CustomFileDetails GetFileDetails(int fileId, LoggedInUserDetails user);

        LicenseTemplateDetails GetLicenseModel(int id, LoggedInUserDetails user);

        List<LicenseTemplateDetails> GetLicenseTemplates(bool includeRetracted, LoggedInUserDetails user);

        CustomFileDetails GetReportDetails(LoggedInUserDetails user);

        void PublishTemplate(int id, LoggedInUserDetails user);

        void RetractLicenseTemplate(int id, LoggedInUserDetails user);

        void SaveLicenseTemplate(LicenseTemplateDetails model, byte[] file, LoggedInUserDetails user);
    }
}