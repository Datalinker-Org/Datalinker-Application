using System.Collections.Generic;

namespace DataLinker.Models
{
    public class AdminDashboardModel
    {
        public string AdminOrgName { get; set; }

        public int ActiveOrganisations { get; set; }

        public int PublishedDataSchemas { get; set; }

        public int FinalizedConsumers { get; set; }

        public int PublishedProviders { get; set; }

        public int ActiveUsers { get; set; }

        public List<DashboardItem> Last7RegisteredOrgs { get; set; }

        public List<DashboardItem> Last7RegisteredSchemas { get; set; }
    }
}
