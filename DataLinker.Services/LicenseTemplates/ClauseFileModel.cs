using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace DataLinker.Services.LicenseTemplates
{
    public class ClauseFileModel
    {
        public string Title { get; set; }

        public List<Clause> clauses { get; set; }
    }

    public class Clause
    {
        [YamlMember(Alias = "short_text", ApplyNamingConventions = false)]
        public string ShortText { get; set; }

        public string Description { get; set; }

        [YamlMember(Alias = "legal_text", ApplyNamingConventions = false)]
        public string LegalText { get; set; }

        [YamlMember(Alias = "order_number", ApplyNamingConventions = false)]
        public int OrderNumber { get; set; }
    }
}