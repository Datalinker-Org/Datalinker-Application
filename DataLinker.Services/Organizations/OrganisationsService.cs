using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Emails;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Mappings;
using Rezare.CommandBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLinker.Services.Organizations
{
    public class OrganisationsService : IOrganisationsService
    {
        private IService<Organization, int> _organisations;
        private INotificationService _notifications;
        private IService<User, int> _users;
        private IService<OrganizationLicense, int> _licenses;
        private IService<Application, int> _applications;
        private IService<ConsumerProviderRegistration, int> _consumerProviderRegistraions;

        public OrganisationsService(IService<Organization, int> organisations,
            IService<User, int> users,
            IService<OrganizationLicense, int> licenses,
            IService<Application, int> apps,
            INotificationService notifications,
            IService<ConsumerProviderRegistration, int> consumerProviderRegistrations
            )
        {
            _organisations = organisations;
            _notifications = notifications;
            _users = users;
            _applications = apps;
            _licenses = licenses;
            _consumerProviderRegistraions = consumerProviderRegistrations;
        }

        public List<OrganizationModel> GetOrganisationsModel(LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Define result model
            var result = new List<OrganizationModel>();

            // Get organisations
            var orgs = _organisations.All();

            // Order data by created date
            orgs = orgs.OrderByDescending(i => i.CreatedAt);

            // Setup result model
            foreach (var organization in orgs)
            {
                var model = organization.ToModel();
                result.Add(model);
            }

            // Return result
            return result;
        }

        public void UpdateStatus(int id, bool value, LoggedInUserDetails user)
        {
            // Return error if user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get organisation
            var organization = _organisations.FirstOrDefault(i=>i.ID == id);

            // Return error if organization does not have legal officer
            var legalOfficers = _users.Where(i => i.OrganizationID == organization.ID).Where(i => i.IsActive == true && i.IsIntroducedAsLegalOfficer == true && i.IsVerifiedAsLegalOfficer == true);
            if (!legalOfficers.Any())
            {
                const string message = "Organization must have at least one confirmed legal officer for activation. Check organization members list, if there is a requests for legal confirmation - verify new legal officer and confirm.";
                throw new BaseException(message);
            }
            // Update activation flag for organization
            organization.IsActive = value;

            // Save changes
            _organisations.Update(organization);

            // Notify organization members about organization activation
            _notifications.User.OrganizationIsActiveInBackground(organization.ID, value ? ActivationState.Active : ActivationState.NotActive);
        }

        public bool IsOrganisationNameUsed(string name)
        {
            var organisations = _organisations.All();
            var result = organisations.Any(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            return result;
        }

        public DashboardModel SetupDashboardModel(int id, LoggedInUserDetails user)
        {
            // Check whether use does not has access
            if (user.Organization.ID != id && !user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get organization
            var organization = _organisations.FirstOrDefault(i=>i.ID == id);

            // Get organisation members
            var organizationMembers = _users.Where(i => i.OrganizationID == organization.ID).Where(i => i.IsActive == true).ToList();

            var legalRegistrationStatus = GetRegistrationStatus(organizationMembers);
            // Deactivate organization if no legal officers
            if (organization.IsActive && legalRegistrationStatus == LegalRegistrationStatus.NotFound)
            {
                organization.IsActive = false;
                _organisations.Update(organization);
            }
            // Get model for dashboard
            var activeMembers = organizationMembers.Where(i => i.IsActive == true).ToList();

            // Setup dashboard model
            var model = GetDashboardModel(organization, activeMembers, user);
            model.LegalRegistration = legalRegistrationStatus;
            return model;
        }

        private DashboardModel GetDashboardModel(Organization organization, IEnumerable<User> users, LoggedInUserDetails user)
        {
            // Setup result model
            var model = organization.ToDashboardOrg();

            model.Members = new List<UserDetailsModel>();
            model.PendingLegalApproval = new List<ApplicationModel>();
            model.OtherApplications = new List<ApplicationModel>();

            // Add organization members to model
            foreach (var item in users)
            {
                var userModel = item.ToUserModel();
                model.Members.Add(userModel);
            }
            // Get all applications for organization
            var apps = _applications.Where(i => i.OrganizationID == user.Organization.ID);
            // Add each application to result model
            foreach (var app in apps)
            {
                var regModels = GetRegistrationPendingConsumerApproval(app);
                if (regModels.Count > 0)
                {
                    model.ConsumerLegelPendingApproval.AddRange(regModels);
                }

                // Check if application is pending approval
                var isPendingApproval = IsAppPendingApproval(app);

                // Setup application model
                var appModel = app.ToAppModel();

                // Setup pending approval application
                if (isPendingApproval)
                {
                    model.PendingLegalApproval.Add(appModel);
                    continue;
                }
                // Setup not pending approval applications
                model.OtherApplications.Add(appModel);
            }
            return model;
        }

        private bool IsAppPendingApproval(Application app)
        {
            // Get licenses for application
            var licenses = _licenses.Where(i => i.ApplicationID == app.ID);
            if (licenses.Any(i => i.Status == (int)PublishStatus.PendingApproval))
            {
                // Set flag if license is in pending approval state
                return true;
            }
            return false;
        }

        private List<RegistrationModel> GetRegistrationPendingConsumerApproval(Application app)
        {
            var models = new List<RegistrationModel>();
            var consumerProviderRegistrations = _consumerProviderRegistraions.Where(p => p.ConsumerApplicationID == app.ID && p.Status == (int)ConsumerProviderRegistrationStatus.PendingConsumerApproval);
            foreach(var consumerProviderRegistration in consumerProviderRegistrations)
            {
                var model = new RegistrationModel();
                model.ConsumerApplicationName = app.Name;
                models.Add(model);
            }
            return models;
        }

        private LegalRegistrationStatus GetRegistrationStatus(List<User> members)
        {
            if (members.Any(i => i.IsLegalOfficer == true && i.IsActive == true))
            {
                return LegalRegistrationStatus.Completed;
            }
            if (members.Any(i => i.IsIntroducedAsLegalOfficer == true && i.IsActive == true))
            {
                return LegalRegistrationStatus.NotVerified;
            }
            // Introduced as legal officer and not registered in IdentityServer
            if (members.Any(i => i.IsIntroducedAsLegalOfficer == true && i.IsActive == false && string.IsNullOrEmpty(i.UserID)))
            {
                return LegalRegistrationStatus.NotCompleted;
            }

            return LegalRegistrationStatus.NotFound;
        }
    }
}
