using DataLinker.Models;

namespace DataLinker.Services.LicenseTemplates
{
    public interface ILicenseClauseTemplateService
    {
        void CreateClausesForSection(int sectionId, byte[] file, LoggedInUserDetails user);

        void CreateClauseTemplate(int sectionId, LicenseClauseTemplateModel model, LoggedInUserDetails user);

        void EditClauseTemplate(int id, LicenseClauseTemplateModel model, LoggedInUserDetails user);

        LicenseClauseTemplateModel GetClauseForEdit(int id, LoggedInUserDetails user);

        LicenseClauseTemplateModel GetClauseModel(int sectionId, LoggedInUserDetails user);

        SectionsAndClausesModel GetSectionsWithClausesModel(LoggedInUserDetails user);

        void PublishTemplate(int id, LoggedInUserDetails user);

        void RetractTemplate(int id, LoggedInUserDetails user);
    }
}