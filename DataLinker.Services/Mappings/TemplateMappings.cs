using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;

namespace DataLinker.Services.Mappings
{
    public static class TemplateMappings
    {
        public static LicenseTemplateDetails ToModel(this LicenseTemplate licenseTemplate) {

            var result = new LicenseTemplateDetails();

            result.ID = licenseTemplate.ID;
            result.LicenseId = licenseTemplate.LicenseID;
            result.Name = licenseTemplate.Name;
            result.Description = licenseTemplate.Description;
            result.Status = (TemplateStatus)licenseTemplate.Status;
            result.CreatedAt = licenseTemplate.CreatedAt;
            result.Version = licenseTemplate.Version;
            result.IsActive = licenseTemplate.Status == (int)TemplateStatus.Active;
            result.IsDraft = licenseTemplate.Status == (int)TemplateStatus.Draft;
            result.IsRetracted = licenseTemplate.Status == (int)TemplateStatus.Retracted;

            return result;
        }
    }
}