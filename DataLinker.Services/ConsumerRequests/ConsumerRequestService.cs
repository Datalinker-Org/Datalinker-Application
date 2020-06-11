using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Emails;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Security;
using Rezare.CommandBuilder.Services;
using System.Collections.Generic;
using System.Linq;

namespace DataLinker.Services.ConsumerRequests
{
    public class ConsumerRequestService : IConsumerRequestService
    {
        private ISecurityService _security;
        private IService<ConsumerRequest, int> _requests;
        private IService<Application, int> _applications;
        private IService<Organization, int> _organisations;
        private IService<DataSchema, int> _schemas;
        private INotificationService _notifications;

        public ConsumerRequestService(ISecurityService security,
            IService<ConsumerRequest, int> requests,
            IService<Application, int> applications,
            IService<Organization, int> organisations,
            IService<DataSchema, int> schemas,
            INotificationService notifications)
        {
            _security = security;
            _requests = requests;
            _applications = applications;
            _organisations = organisations;
            _schemas = schemas;
            _notifications = notifications;
        }

        public List<ConsumerRequestModel> GetConsumerRequestModels(int applicationId, LoggedInUserDetails user)
        {
            // Check whether user has access
            _security.CheckAccessToApplication(user, applicationId);
            if (!user.IsLegalRep)
            {
                throw new BaseException("Access denied.");
            }

            // Get requests for application
            var requests = _requests.Where(i => i.ProviderID == applicationId).Where(i => i.Status == (int)RequestStatus.NotProcessed);

            // Setup result
            var result = new List<ConsumerRequestModel>();
            foreach (var consumerRequest in requests)
            {
                // Get organisation id
                var organizationId = _applications.FirstOrDefault(i => i.ID == consumerRequest.ConsumerID).OrganizationID;

                // Get organisation
                var organization = _organisations.FirstOrDefault(i => i.ID == organizationId);

                // Get schema
                var schema = _schemas.FirstOrDefault(i => i.ID == consumerRequest.DataSchemaID);

                // Setup model
                var model = new ConsumerRequestModel
                {
                    ConsumerName = organization.Name,
                    SchemaName = schema.Name,
                    CreatedAt = consumerRequest.CreatedAt,
                    Id = consumerRequest.ID
                };

                // Add to result
                result.Add(model);
            }

            return result;
        }

        public void ApproveRequest(int applicationId, int id, LoggedInUserDetails user)
        {
            // Check whether user has access to application
            _security.CheckAccessToApplication(user, applicationId);
            if (!user.IsLegalRep)
            {
                throw new BaseException("Access denied.");
            }

            // Get request
            var request = _requests.FirstOrDefault(i => i.ID == id);

            // Setup confirmation details
            request.Status = (int)RequestStatus.Approved;

            // Save changes
            _requests.Update(request);

            // Notify consumer about approval
            _notifications.LegalOfficer.ApprovedConsumerRequestInBackground(request.ProviderID, request.DataSchemaID);
        }

        public void DeclineRequest(int applicationId, int id, LoggedInUserDetails user)
        {
            // Check whether user has access
            _security.CheckAccessToApplication(user, applicationId);
            if (!user.IsLegalRep)
            {
                throw new BaseException("Access denied.");
            }

            // Get request details
            var request = _requests.FirstOrDefault(i => i.ID == id);

            // Setup decline details
            request.Status = (int)RequestStatus.Declined;

            // Save changes
            _requests.Update(request);

            // Notify consumer details
            _notifications.LegalOfficer.RejectedConsumerRequestInBackground(request.ProviderID, request.DataSchemaID);
        }

        public int GetNotProcessedRequests(int applicationId, LoggedInUserDetails user)
        {
            // Check whether user has access
            _security.CheckAccessToApplication(user, applicationId);
            if (!user.IsLegalRep)
            {
                throw new BaseException("Access denied");
            }

            // Get not processed requests
            var requests = _requests.Where(i => i.ProviderID == applicationId).Where(i => i.Status == (int)RequestStatus.NotProcessed);

            // Get count
            var result = requests.Count();
            return result;
        }
    }
}
