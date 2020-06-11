using DataLinker.Database.Models;
using DataLinker.Models;
using System.Collections.Generic;

namespace DataLinker.Services.Mappings
{
    public static class ApplicationMappings
    {
        public static ApplicationDetails ToModel(this Application app)
        {
            var result = new ApplicationDetails();

            result.ID = app.ID;
            result.OrganizationID = app.OrganizationID;
            result.Name = app.Name;
            result.Description = app.Description;
            result.PublicID = app.PublicID;
            result.IsProvider = app.IsProvider;
            result.IsIntroducedAsIndustryGood = app.IsIntroducedAsIndustryGood;
            result.IsVerifiedAsIndustryGood = app.IsVerifiedAsIndustryGood;
            result.IsActive = app.IsActive;
            result.LastUpdatedAt = app.UpdatedAt.HasValue ? app.UpdatedAt.Value : app.CreatedAt;
            result.Hosts = new List<ApplicationTokenDetails>();
            result.AuthDetails = new ApplicationAuthenticationDetails();

            return result;
        }

        public static ApplicationModel ToAppModel(this Application application)
        {
            var result = new ApplicationModel();
            result.ID = application.ID;
            result.Name = application.Name;
            result.IsProvider = application.IsProvider;

            return result;
        }

        public static ApplicationTokenDetails ToModel(this ApplicationToken token)
        {
            var result = new ApplicationTokenDetails();

            result.Token = token.Token;
            result.ID = token.ID;
            result.ApplicationID = token.ApplicationID;
            result.OriginHost = token.OriginHost;
            result.IsExpired = token.ExpiredAt.HasValue;

            return result;
        }

        public static ApplicationAuthenticationDetails ToModel(this ApplicationAuthentication appAuth)
        {
            var result = new ApplicationAuthenticationDetails();
            
            result.ID = appAuth.ID;
            result.ApplicationID = appAuth.ApplicationID;
            result.WellKnownUrl = appAuth.WellKnownUrl;
            result.Issuer = appAuth.Issuer;
            result.JwksUri = appAuth.JwksUri;
            result.AuthorizationEndpoint = appAuth.AuthorizationEndpoint;
            result.TokenEndpoint = appAuth.TokenEndpoint;
            result.RegistrationEndpoint = appAuth.RegistrationEndpoint;
            result.RevocationEndpoint = appAuth.RevocationEndpoint;

            return result;
        }

        public static ProviderEndpointModel ToModel(this ProviderEndpoint endpoint)
        {
            var result = new ProviderEndpointModel();

            result.Name = endpoint.Name;
            result.Uri = endpoint.ServiceUri;
            result.Description = endpoint.Description;
            result.ID = endpoint.ID;
            result.LastUpdatedAt = endpoint.UpdatedAt;
            result.IsActive = endpoint.IsActive;
            return result;
        }

        public static oauthDetails ToApiModel(this ApplicationAuthentication authentication)
        {
            var result = new oauthDetails
            {
                WellKnownUrl = authentication.WellKnownUrl,
                AuthorizationEndpoint = authentication.AuthorizationEndpoint,
                TokenEndpoint = authentication.TokenEndpoint,
                UserInfoEndpoint = authentication.UserInfoEndpoint,
                EndSessionEndpoint = authentication.EndSessionEndpoint,
                RevocationEndpoint = authentication.RevocationEndpoint,
                RegistrationEndpoint = authentication.RegistrationEndpoint
            };

            return result;
        }
    }
}