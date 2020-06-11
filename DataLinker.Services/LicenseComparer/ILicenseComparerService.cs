using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.OrganizationLicenses
{
    public interface ILicenseComparerService
    {
        ComparisonResult CompareClauses(OrganizationLicenseClause providerClause, OrganizationLicenseClause consumerClause);

        ProviderComparisonSummary GetSchemaProviders(int schemaId, LoggedInUserDetails user);
    }
}