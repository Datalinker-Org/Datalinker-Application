
using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.Mappings
{
    public static class SectionModelMappings
    {
        public static LicenseSectionModel ToModel(this LicenseSection section)
        {
            var result = new LicenseSectionModel();
            result.ID = section.ID;
            result.Title = section.Title;
            return result;
        }
    }
}