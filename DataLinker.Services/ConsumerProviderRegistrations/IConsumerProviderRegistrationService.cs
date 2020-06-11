using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Applications;
using DataLinker.Models.ConsumerProviderRegistration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Services.ConsumerProviderRegistrations
{
    public interface IConsumerProviderRegistrationService
    {
        SchemaProviderVm GetProvidersBySchemaId(int appId, int schemaId);
        void RequestForAccess(int consumerAppId, int providerLicenseId, LoggedInUserDetails user);
        LegalApprovalModel GetLegalApprovalModel(string token, LoggedInUserDetails user);
        LegalApprovalModel GetLegalApprovalModel(int consumerProviderRegistrationId, LoggedInUserDetails user);
        ConsumerProviderRegistrationDetail ApproveByConsumerLegal(int consumerProviderRegistrationId, LoggedInUserDetails user);
        ConsumerProviderRegistrationDetail DeclineByConsumerLegal(int consumerProviderRegistrationId, string declineReason, LoggedInUserDetails user);
        ConsumerProviderRegistrationDetail ApproveByProviderLegal(int consumerProviderRegistrationId, LoggedInUserDetails user);
        ConsumerProviderRegistrationDetail DeclineByProviderLegal(int consumerProviderRegistrationId, string declineReason, LoggedInUserDetails user);
        ConsumerProviderRegistrationDetail GetConsumerProviderRegistrationDetail(int consumerProviderRegistrationId, LoggedInUserDetails user);
    }
}
