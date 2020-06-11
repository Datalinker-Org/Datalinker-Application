using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.Mappings
{
    public static class OrganisationMappings
    {
        public static LoggedInOrganization ToLoggedInOrg(this Organization org)
        {
            var result = new LoggedInOrganization
            {
                IsActive = org.IsActive,
                Name = org.Name,
                ID = org.ID,
            };

            return result;
        }

        public static DashboardModel ToDashboardOrg(this Organization organization)
        {
            var result = new DashboardModel();
            result.OrganizationID = organization.ID;
            result.OrganizationName = organization.Name;
            result.IsActive = organization.IsActive;

            return result;
        }

        public static OrganizationModel ToModel(this Organization organization)
        {
            var result = new OrganizationModel();
            result.ID = organization.ID;
            result.Name = organization.Name;
            result.IsActive = organization.IsActive;
            result.Phone = organization.Phone;
            result.Address = organization.Address;
            result.AdministrativeContact = organization.AdministrativeContact;
            result.AdministrativePhone = organization.AdministrativePhone;
            result.TermsOfService = organization.TermsOfService;

            return result;
        }
    }
}