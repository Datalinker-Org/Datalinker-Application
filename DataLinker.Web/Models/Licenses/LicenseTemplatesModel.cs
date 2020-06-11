using DataLinker.Models;
using PagedList;

namespace DataLinker.Web.Models.Licenses
{
    public class LicenseTemplatesModel
    {
        public bool IsActivePresent { get; set; }

        public bool IncludeRetracted { get; set; }

        public IPagedList<LicenseTemplateDetails> Templates { get; set; }
    }
}