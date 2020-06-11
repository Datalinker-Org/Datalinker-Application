using System.Collections.Generic;
using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.Applications
{
    public interface IApplicationsService
    {
        void AddEndpoint(int id, ProviderEndpointModel model, LoggedInUserDetails user);

        void AddHost(int id, string host, LoggedInUserDetails user);

        void ApproveIndustryGoodRequest(int id, LoggedInUserDetails user);

        Application Create(string url, NewApplicationDetails model, LoggedInUserDetails user);

        void DeclineIndustryGoodRequest(int id, LoggedInUserDetails user);

        Application EditApplication(int id, string url, ApplicationDetails appDetails, LoggedInUserDetails user);

        void EditAuthentication(int id, ApplicationAuthenticationDetails model, LoggedInUserDetails user);

        void ExpireAppToken(int id, int tokenId, LoggedInUserDetails user);

        ApplicationAuthenticationDetails GetApplicationAuthModel(int id, LoggedInUserDetails user);

        ApplicationDetails GetApplicationDetailsModel(int id, LoggedInUserDetails user);

        List<ApplicationDetails> GetApplications(LoggedInUserDetails user);

        List<ApplicationTokenDetails> GetHosts(int id, LoggedInUserDetails user);

        ApplicationToken CreateNewToken(int id, int tokenId, LoggedInUserDetails user);

        bool IsApplicationExistsForThisOrganization(string name, string InitialName, LoggedInUserDetails user);

        ProviderEndpoint EditEndpoint(int appId, int endpointId, ProviderEndpointModel model, LoggedInUserDetails user);

        ProviderEndpointModel SetupAddProviderEndpointModel(int id, LoggedInUserDetails user);

        ApplicationAuthenticationDetails SetupEditAppAuthModel(int id, LoggedInUserDetails user);

        ProviderEndpointModel SetupProviderEndpointModel(int appId, int endpointId, LoggedInUserDetails user);

        ApplicationDetails GetDetailsModelForEdit(int id, LoggedInUserDetails user);

        void UpdateStatus(int id, bool value, LoggedInUserDetails user);

        List<appDetails> GetProviders(string[] schemas, LoggedInApplication LoggedInApp);

        LoggedInApplication GetLoggedInApp(string referer, string token);
    }
}