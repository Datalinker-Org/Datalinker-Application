using System.Collections.Generic;

namespace DataLinker.Models
{
    public class SectionsAndClausesModel
    {
        public IList<SectionModel> Sections { get; set; }

        public LicenseTemplateDetails GlobalLicense { get; set; }
    }
}