using System;
using System.Configuration;
using System.Diagnostics;
using Owin;
using Rezare.IdentityServer;
using Rezare.IdentityServer.Api;
using Rezare.IdentityServer.Common;
using Rezare.IdentityServer.Configuration;
using Rezare.Lib.Email;

namespace DataLinker.IdentityServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureIdentityServer(app, ConfigurationManager.AppSettings["IdentityServerHost"], ConfigurationManager.AppSettings["IdentityServerName"], ConfigurationManager.AppSettings["IdentityServerLogLocation"]);
        }

        public void ConfigureIdentityServer(IAppBuilder app,string baseUri, string serverName, string logLocation)
        {
            // setup defaults
            var identityProvider = new Rezare.IdentityServer.AspNetIdentity.IdentityProviderHelper();
            try
            {
                var assemblyName = typeof(Scopes).Assembly.FullName;
                var clients = ClientConfigurationHelper.Get();

                // setup identity server options
                var options = new IdentityServerSetupOptions()
                {
                    IdentityServerName = serverName,
                    BaseUri = baseUri.ToString(),
                    IdentityProvider = identityProvider,
                    Scopes = Scopes.Get(),
                    Clients = clients,
                    HostRedirectFinalPage = true,
                    UpdateDatabaseFromConfig = true,
                    ConnectionStringName = ConfigurationManager.AppSettings["ConnectionStringName"],
                    EmailSettings = new ServerSettings()
                    {
                        SendAs = ConfigurationManager.AppSettings["SmtpSendAs"],
                        Password = ConfigurationManager.AppSettings["SmtpPassword"],
                        Port = Convert.ToInt32(ConfigurationManager.AppSettings["SmtpPort"]),
                        ServerName = ConfigurationManager.AppSettings["SmtpServer"],
                        UserName = ConfigurationManager.AppSettings["SmtpUser"],
                        UseSSL = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpUseSsl"])
                    }
                };

                options.EmbeddedViewOptions.ContentAssembly = assemblyName;
                options.InternalClientOptions.Secret = ConfigurationManager.AppSettings["InternalClientSecret"];
                options.AccountOptions.ForgottenPasswordOptions= new ForgottenPasswordOptions
                {
                    Provide = true,
                    FindUserBy = FindUserBy.Username,
                    Assembly = assemblyName
                };

                options.PasswordOptions.RequireNonLetterOrDigit = false;
                options.PasswordOptions.MinimumLength = 8;
                options.AccountOptions.ForgottenPasswordOptions.FindUserBy = FindUserBy.Username;
                options.ProcessUris();

                // setup logging
                app.ConfigureLogging(logLocation);

                // use identity server api
                app.UseIdentityServerApi(new IdentityServerApiSetupOptions()
                {
                    Authority = options.FullUri,
                    IdentityProvider = identityProvider,
                    PasswordOptions = options.PasswordOptions,
                    ConnectionStringName = ConfigurationManager.AppSettings["ConnectionStringName"]
                });

                // use identity server
                app.UseIdentityServer(options);
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
            }
        }
    }
}
