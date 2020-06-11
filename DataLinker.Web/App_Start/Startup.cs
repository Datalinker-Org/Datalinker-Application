using System;
using System.Configuration;
using System.Web.Helpers;
using DataLinker.Web.Security;
using Hangfire;
using IdentityServer3.Core;
using Owin;

namespace DataLinker.Web
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            log4net.Config.XmlConfigurator.Configure();
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(
                    ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["HangfireConnectionString"]]
                        .ConnectionString);

            app.UseHangfireDashboard();
            app.UseHangfireServer();
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = Constants.ClaimTypes.Email;
            var idsBaseUrl = ConfigurationManager.AppSettings["IdentityServerHost"];
            var idsUrl = new Uri(idsBaseUrl + "core");
            var dlHost = ConfigurationManager.AppSettings["DataLinkerHost"];
            var redirectUri = new Uri(dlHost + "Account/SignInCallback/");
            app.ConfigureAsIdsClient("datalinker.web", idsUrl, redirectUri);
        }
    }
}