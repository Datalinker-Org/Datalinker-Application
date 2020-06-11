//-------------------------------------------------------------------------------
// <copyright file="NinjectWebCommon.cs" company="Ninject Project Contributors">
//   Copyright (c) 2012 Ninject Project Contributors
//   Authors: Remo Gloor (remo.gloor@gmail.com)
//           
//   Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
//   you may not use this file except in compliance with one of the Licenses.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//   or
//       http://www.microsoft.com/opensource/licenses.mspx
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

using System.Web.ModelBinding;
using DataLinker.API.App_Start;
using DataLinker.Services.Emails.Models;
using Microsoft.Owin.Security;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace DataLinker.API.App_Start
{
    using System;
    using System.Reflection;
    using System.Web;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using Services.Configuration;
    using System.Configuration;
    using Services;
    using Hangfire;
    using DataLinker.Services.Identities;
    using DataLinker.IdsService;
    using Ninject.Web.Common.WebHost;

    /// <summary>
    /// Bootstrapper for the application.
    /// </summary>
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Settings.AllowNullInjection = true;
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);

            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            var connectionStringKey = ConfigurationManager.AppSettings["ConnectionString"];
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;
            var expireInAfterHours = ConfigurationManager.AppSettings["ConfirmNewEmailExpires"];
            var approvalLinkExpireIn = ConfigurationManager.AppSettings["ApprovalLinkExpiresIn"];
            var pathToTempFiles = ConfigurationManager.AppSettings["PathToTempFiles"];
            var dataLinkerHost = ConfigurationManager.AppSettings["DataLinkerHost"];

            // Page size
            var manageUsersPageSize = ConfigurationManager.AppSettings["ManageUsersPageSize"];
            var manageSchemasPageSize = ConfigurationManager.AppSettings["ManageSchemasPageSize"];
            var manageLicensesPageSize = ConfigurationManager.AppSettings["ManageLicensesPageSize"];
            var manageApplicationsPageSize = ConfigurationManager.AppSettings["ManageApplicationsPageSize"];
            var manageOrganizationsPageSize = ConfigurationManager.AppSettings["ManageOrganizationsPageSize"];
            var manageLicenseTemplatesPageSize = ConfigurationManager.AppSettings["ManageLicenseTemplatesPageSize"];

            kernel.Bind<IConfigurationService>()
                .To<ConfigurationService>()
                .WithPropertyValue("ConnectionString", connectionString)
                .WithPropertyValue("ConfirmNewEmailExpires", expireInAfterHours)
                .WithPropertyValue("ApprovalLinkExpiresIn", approvalLinkExpireIn)
                .WithPropertyValue("PathToTempFiles", pathToTempFiles)
                .WithPropertyValue("DataLinkerHost", dataLinkerHost)
                // Page size for list views
                .WithPropertyValue("ManageUsersPageSize", int.Parse(manageUsersPageSize))
                .WithPropertyValue("ManageSchemasPageSize", int.Parse(manageSchemasPageSize))
                .WithPropertyValue("ManageLicensesPageSize", int.Parse(manageLicensesPageSize))
                .WithPropertyValue("ManageApplicationsPageSize", int.Parse(manageApplicationsPageSize))
                .WithPropertyValue("ManageOrganizationsPageSize", int.Parse(manageOrganizationsPageSize))
                .WithPropertyValue("ManageLicenseTemplatesPageSize", int.Parse(manageLicenseTemplatesPageSize));
            
        kernel.Bind<IEmailSettings>()
                .To<EmailSettings>()
                .WithPropertyValue("SmtpServer", ConfigurationManager.AppSettings["SmtpServer"])
                .WithPropertyValue("SmtpUser", ConfigurationManager.AppSettings["SmtpUser"])
                .WithPropertyValue("SmtpPort", ConfigurationManager.AppSettings["SmtpPort"])
                .WithPropertyValue("SmtpUseDefault", Boolean.Parse(ConfigurationManager.AppSettings["SmtpUseDefault"] ?? "false"))
                .WithPropertyValue("SmtpUseSsl", Boolean.Parse(ConfigurationManager.AppSettings["SmtpUseSsl"] ?? "false"))
                .WithPropertyValue("SmtpPassword", ConfigurationManager.AppSettings["SmtpPassword"])
                .WithPropertyValue("SmtpFromName", ConfigurationManager.AppSettings["SmtpFromName"])
                .WithPropertyValue("SmtpFromEmail", ConfigurationManager.AppSettings["SmtpFromEmail"])
                .WithPropertyValue("SendEmail", Boolean.Parse(ConfigurationManager.AppSettings["SendEmail"] ?? "false"))
                .WithPropertyValue("PathToMail", ConfigurationManager.AppSettings["PathToMail"]);

            kernel.Bind<IIdsApiService>().To<IdsApiService>();
            ServicesSetup.RegisterNInjectBindings(kernel);
            SetupEmailServices.RegisterNInjectBindings(kernel);

            GlobalConfiguration.Configuration.UseNinjectActivator(kernel);
            JobActivator.Current = new NinjectJobActivator(kernel);
        }        
    }
}
