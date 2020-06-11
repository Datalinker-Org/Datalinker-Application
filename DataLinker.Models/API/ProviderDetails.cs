using System.Collections.Generic;

namespace DataLinker.Models
{
    public class appDetails
    {
        public string ProviderName { get; set; }

        public string PublicID { get; set; }

        public string LogoUrl { get; set; }

        public string ProviderPhone { get; set; }

        public string ProviderAddress { get; set; }

        public oauthDetails OAuthInfo { get; set; }
        
        public IList<schemaEndpoint> Endpoints { get; set; }
    }

    public class oauthDetails
    {
        public string WellKnownUrl { get; set; }

        public string AuthorizationEndpoint { get; set; }

        public string TokenEndpoint { get; set; }

        public string UserInfoEndpoint { get; set; }

        public string EndSessionEndpoint { get; set; }

        public string RevocationEndpoint { get; set; }

        public string RegistrationEndpoint { get; set; }
    }

    public class schemaEndpoint
    {
        public SchemaDetails Schema { get; set; }
        
        public endpointDetails Endpoint { get; set; }
    }

    public class endpointDetails
    {
        public string ServiceUri { get; set; }

        public string Description { get; set; }

        public string Name { get; set; }

        public string Method { get; set; }
    }
}
