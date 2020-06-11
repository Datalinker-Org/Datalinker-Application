using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using Rezare.IdentityServer.Common;

namespace DataLinker.Web.Security
{
    internal static class IdentityExtensions
    {
        public const string IdentityServerClientName = "livestockone.web";

        /// <summary>
        /// Event to handle Token Validation from ID provider
        /// </summary>
        /// <param name="n">The context for this notification</param>
        /// <param name="authorizationService">The service to use for authorization</param>
        /// <returns></returns>
        private static async Task OnSecurityTokenValidated(
            SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> n)
        {
            // get access and id tokens
            var accessToken = n.ProtocolMessage.AccessToken;
            var idToken = n.ProtocolMessage.IdToken;

            // persist access token in cookie
            if (!String.IsNullOrEmpty(accessToken))
            {
                // add access token
                n.AuthenticationTicket.Identity.AddClaim(
                    new Claim("access_token", accessToken));

                // get user info from ID server
                var userClient = new UserInfoClient(KnownIdServer.UserInfoEndpoint, accessToken);
                var userInfo = await userClient.GetAsync();

                // get claims from ticket
                n.AuthenticationTicket.Identity.AddClaims(userInfo.Claims.Select(c => new Claim(c.Item1, c.Item2)));

                // get username
                var username =
                    n.AuthenticationTicket.Identity.Claims.FirstOrDefault(
                        c => c.Type == "preferred_username");
            }

            // if we have an id token
            if (!String.IsNullOrEmpty(idToken))
            {
                // add to authentication ticket
                n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", idToken));
            }
        }

        /// <summary>
        /// Event to handle redirect from ID provider
        /// </summary>
        /// <param name="n">The context for the event</param>
        /// <returns></returns>
        private static Task OnRedirectToIdentityProvider(
            RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> n)
        {
            // if this is a logout
            if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
            {
                // get the id token
                var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                // if we have one, add to response to ID server
                if (idTokenHint != null) n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Configures the web site to use Identity Server
        /// </summary>
        /// <param name="app">The app builder</param>
        /// <param name="userService">The user service</param>
        /// <param name="authorizationService">The authorization service</param>
        //public static void ConfigureIds(this IAppBuilder app, IService<User> userService, IAuthorizationService authorizationService)
        //{
        //    var baseUri = new Uri(Properties.Settings.Default.SiteBaseUrl);
        //    var returnUri = new Uri(baseUri, "Authentication/SignInCallback/");
        //    Uri identityUri = new Uri(Properties.Settings.Default.IdentityServerUrl);

        //    // configure app as IDS client
        //    app.ConfigureAsIdsClient(IdentityServerClientName, identityUri, returnUri, authorizationService);
        //}

        /// <summary>
        /// Configure LivestockOne to be an IDS client
        /// </summary>
        /// <param name="app">The app builder</param>
        /// <param name="clientName">The client name to use</param>
        /// <param name="serviceUri">The IDS url</param>
        /// <param name="redirectUri">The base URL for this application</param>
        /// <param name="authorizationService"></param>
        public static void ConfigureAsIdsClient(this IAppBuilder app, string clientName, Uri serviceUri, Uri redirectUri)
        {
            // create mapping for jwts
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            // make sure we use cookies for authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            KnownIdServer.Setup(serviceUri);

            // use openId for authentication
            app.UseCustomOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                // setup client details
                ClientId = clientName,
                RedirectUri = redirectUri.ToString(),

                // setup authority
                Authority = serviceUri.ToString(),

                // setup response
                ResponseType = "id_token token",
                Scope = "openid email profile read write",

                // set authentication type
                SignInAsAuthenticationType = "Cookies",

                // setup notifications
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // on security token validated
                    SecurityTokenValidated = n => OnSecurityTokenValidated(n),

                    // on redirect to ID server
                    RedirectToIdentityProvider = n => OnRedirectToIdentityProvider(n)
                }
            });
        }
        
        private static void UseCustomOpenIdConnectAuthentication(this IAppBuilder app, OpenIdConnectAuthenticationOptions options)
        {
            app.Use<CustomOpenIdConnectAuthenticationMiddleware>(app, options);
        }
    }
}
