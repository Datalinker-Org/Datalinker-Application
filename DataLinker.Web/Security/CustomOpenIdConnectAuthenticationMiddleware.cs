using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OpenIdConnect;

namespace DataLinker.Web.Security
{
    public class CustomOpenIdConnectAuthenticationMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        private readonly ILogger _logger;
        public CustomOpenIdConnectAuthenticationMiddleware(OwinMiddleware next, Owin.IAppBuilder app,
            OpenIdConnectAuthenticationOptions options)
            : base(next, app, options)
        {
            _logger = app.CreateLogger<CustomOpenIdConnectAuthenticationMiddleware>();
        }

        protected override AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new SawtoothOpenIdConnectAuthenticationHandler(_logger);
        }
    }
}