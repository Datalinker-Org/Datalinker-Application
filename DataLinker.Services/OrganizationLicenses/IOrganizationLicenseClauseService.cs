using DataLinker.Models;

namespace DataLinker.Services.OrganizationLicenses
{
    public interface IOrganizationLicenseClauseService
    {
        bool IsClauseSelected(SectionsWithClauses section);

        string GetClauseData(ClauseModel selectedClause);
    }
}