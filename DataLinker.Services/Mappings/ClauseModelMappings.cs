using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;

namespace DataLinker.Services.Mappings
{
    public static class ClauseModelMappings
    {
        public static ClauseModel ToModel(this LicenseClauseTemplate template, int sectionId)
        {
            var result = new ClauseModel();
            result.ClauseId = template.LicenseClauseID;
            result.ClauseTemplateId = template.ID;
            result.LegalText = template.LegalText;
            result.SectionId = sectionId;
            result.Type = template.ClauseType;

            if((ClauseType)result.Type == ClauseType.InputAndDropDown)
            {
                result.SetupDropDownItems();
            }

            return result;
        }

        public static LicenseClauseTemplateModel ToModel(this LicenseClauseTemplate model)
        {
            // Setup result
            var result = new LicenseClauseTemplateModel();
            result.ID = model.ID;
            result.ShortText = model.ShortText;
            result.Description = model.Description;
            result.LegalText = model.LegalText;
            result.Version = model.Version;
            result.Status = (TemplateStatus)model.Status;
            result.CreatedAt = model.CreatedAt;
            result.UpdatedAt = model.UpdatedAt;

            // Return result
            return result;
        }
    }
}