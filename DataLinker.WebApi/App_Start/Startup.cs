using System.Configuration;
using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DataLinker.WebApi.App_Start.Startup))]

namespace DataLinker.WebApi.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            log4net.Config.XmlConfigurator.Configure();
            
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(
                    ConfigurationManager.ConnectionStrings[ConfigurationManager.AppSettings["HangfireConnectionString"]]
                        .ConnectionString);

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
