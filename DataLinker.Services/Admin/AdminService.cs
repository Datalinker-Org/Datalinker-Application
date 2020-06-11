using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using Rezare.CommandBuilder.Services;
using System.Collections.Generic;
using System.Linq;

namespace DataLinker.Services.Admin
{
    internal class AdminService : IAdminService
    {
        private IService<Organization, int> _organisations;
        private IService<User, int> _users;
        private IService<DataSchema, int> _dataSchemas;
        private IService<OrganizationLicense, int> _licenses;

        public AdminService(IService<Organization, int> orgs,
            IService<DataSchema, int> dataSchemas,
            IService<OrganizationLicense, int> licenses,
            IService<User, int> users)
        {
            _organisations = orgs;
            _users = users;
            _licenses = licenses;
            _dataSchemas = dataSchemas;
        }

        public AdminDashboardModel GetModelForAdmin(LoggedInUserDetails user)
        {
            // Check whether user has access
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Setup result
            var result = new AdminDashboardModel();
            result.AdminOrgName = user.Organization.Name;
            // Setup active organisations
            result.ActiveOrganisations = GetActiveOrganisations();

            // Setup active users
            result.ActiveUsers = GetActiveUsers();

            // Setup published data schemas
            result.PublishedDataSchemas = GetPublishedDataSchemas();

            // Setup last 7 registered organisations
            result.Last7RegisteredOrgs = GetLast7RegisteredOrgs();

            // Setup last 7 published data schemas
            result.Last7RegisteredSchemas = GetLast7PublishedSchemas();

            // Setup published provders
            result.PublishedProviders = GetPublishedProviders();

            // Setup published consumers
            result.FinalizedConsumers = GetFinalizedConsumers();

            // Return result
            return result;
        }

        private int GetActiveOrganisations()
        {
            var result = _organisations.Where(i => i.IsActive == true);
            return result.Count();
        }

        private int GetActiveUsers()
        {
            var result = _users.Where(i => i.IsActive == true);
            return result.Count();
        }

        private int GetPublishedDataSchemas()
        {
            var result = _dataSchemas.Where(i => i.Status == (int)TemplateStatus.Active);
            return result.Count();
        }

        private List<DashboardItem> GetLast7RegisteredOrgs()
        {
            var result = new List<DashboardItem>();
            // Get last 7 registered organisations
            var last7Orgs = _organisations.All().OrderByDescending(i => i.CreatedAt).AsEnumerable();
            if (last7Orgs.Count() > 7)
            {
                last7Orgs = last7Orgs.Take(7);
            }

            // Setup result model
            foreach (var org in last7Orgs)
            {
                var item = new DashboardItem
                {
                    Name = org.Name,
                    Date = org.CreatedAt
                };

                result.Add(item);
            }

            // Return result
            return result;
        }

        private List<DashboardItem> GetLast7PublishedSchemas()
        {
            var result = new List<DashboardItem>();

            // Get last 7 published schemas
            var last7Schemas = _dataSchemas.Where(i => i.Status == (int)TemplateStatus.Active).OrderByDescending(i => i.PublishedAt).AsEnumerable();
            if (last7Schemas.Count() > 7)
            {
                last7Schemas = last7Schemas.Take(7);
            }

            // Setup result model
            foreach (var org in last7Schemas)
            {
                var item = new DashboardItem
                {
                    Name = org.Name,
                    Date = org.CreatedAt
                };

                result.Add(item);
            }

            // Return result
            return result;
        }

        private int GetPublishedProviders()
        {
            var publishedLicenses = _licenses.Where(i => i.Status == (int)PublishStatus.Published);
            var providerLicenses = publishedLicenses.Where(i => i.ProviderEndpointID != 0);
            return providerLicenses.Count();
        }

        private int GetFinalizedConsumers()
        {
            var publishedLicenses = _licenses.Where(i => i.Status == (int)PublishStatus.Published);
            var consumerLicenses = publishedLicenses.Where(i => i.ProviderEndpointID == 0);
            // Count consumer apps as consumer can have multiple custom licenses with one templated license
            var consumerApps = consumerLicenses.Select(i => i.ApplicationID).Distinct();
            return consumerApps.Count();
        }
    }
}
