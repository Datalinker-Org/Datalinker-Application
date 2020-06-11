using DataLinker.Models;
using System.Collections.Generic;

namespace DataLinker.Services.Organizations
{
    public interface IOrganisationsService
    {
        List<OrganizationModel> GetOrganisationsModel(LoggedInUserDetails user);

        bool IsOrganisationNameUsed(string name);

        void UpdateStatus(int id, bool value, LoggedInUserDetails user);

        DashboardModel SetupDashboardModel(int id, LoggedInUserDetails user);
    }
}