using PagedList;

namespace DataLinker.Web.Models.Licenses
{
    public class ProviderLicensesModel : DataLinker.Models.ProviderLicensesModel
    {
        public new IPagedList<DataLinker.Models.ProviderLicenseModel> Items { get; set; }
    }
}