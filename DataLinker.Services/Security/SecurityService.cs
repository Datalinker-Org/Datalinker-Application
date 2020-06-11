using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Services.Exceptions;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Services.Security
{
    internal class SecurityService: ISecurityService
    {
        private readonly IService<Application, int> _applications;

        public SecurityService(IService<Application, int> applications)
        {
            _applications = applications;
        }

        public void CheckBasicAccess(LoggedInUserDetails user)
        {
            if (!user.IsActive)
            {
                throw new BaseException("User is not active.");
            }

            if (!user.HasOrganization)
            {
                throw new BaseException("User does not have associated organisation.");
            }
        }

        public Application CheckAccessToApplication(LoggedInUserDetails user, int applicationId)
        {
            // Check basic access
            CheckBasicAccess(user);

            // Get application
            var application = _applications.FirstOrDefault(i=>i.ID == applicationId);
            if(application == null)
            {
                throw new BaseException("Not found");
            }

            // Check whether application belongs to users organisation
            if(application.OrganizationID != user.Organization.ID)
            {
                throw new BaseException($"Access to application {applicationId} denied.");
            }

            return application;
        }
    }
}
