using DataLinker.Services.Admin;
using DataLinker.Services.Applications;
using DataLinker.Services.Authorisation;
using DataLinker.Services.ConsumerProviderRegistrations;
using DataLinker.Services.ConsumerRequests;
using DataLinker.Services.CustomLicenses;
using DataLinker.Services.FileProviders;
using DataLinker.Services.Identities;
using DataLinker.Services.LicenseContent;
using DataLinker.Services.LicenseTemplates;
using DataLinker.Services.LicenseVerification;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Services.Organizations;
using DataLinker.Services.Schemas;
using DataLinker.Services.Security;
using DataLinker.Services.SoftwareStatements;
using DataLinker.Services.Tokens;
using DataLinker.Services.Urls;
using DataLinker.Services.Users;
using Ninject;
using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using Rezare.CommandBuilder.SqlServer;
using System.Configuration;

namespace DataLinker.Services
{
    /// <summary>
    ///     Register all necessary services bindings.
    /// </summary>
    /// <param name="kernel">The NInject kernel to bind onto</param>
    public static class ServicesSetup
    {
        public static void RegisterNInjectBindings(IKernel kernel)
        {
            // bind transaction factory and connection info
            kernel.Bind<IConnectionInfo>().ToMethod((ctx) =>
            {
                var connectionStringKey = ConfigurationManager.AppSettings["ConnectionString"];
                var connectionString = ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString;
                return new SqlServerConnectionInfo(connectionString);
            });

            // bind transaction factory
            kernel.Bind<ITransactionFactory>().ToMethod((ctx) =>
            {
                var connectionInfo = ctx.Kernel.Get<IConnectionInfo>();

                var factory = new TransactionFactory();
                factory.SetDefaultConnection(connectionInfo);

                return factory;
            });

            // General Services
            kernel.Bind(typeof (IService<>)).To(typeof (Service<>));
            kernel.Bind(typeof (IService<,>)).To(typeof (Service<,>));

            kernel.Bind<IUserService>().To<UserService>();
            kernel.Bind<IAuthorisationService>().To<AuthorisationService>();
            kernel.Bind<IApplicationsService>().To<ApplicationsService>();
            kernel.Bind<ISoftwareStatementService>().To<SoftwareSoftwareStatementService>();
            kernel.Bind<IDataSchemaService>().To<DataSchemaService>();
            kernel.Bind<IOrganizationLicenseService>().To<OrganizationLicenseService>();
            kernel.Bind<IOrganisationsService>().To<OrganisationsService>();
            kernel.Bind<IOrganizationLicenseClauseService>().To<OrganizationLicenseClauseService>();
            kernel.Bind<ILicenseContentBuilder>().To<LicenseContentBuilder>();
            kernel.Bind<ILicenseTemplatesService>().To<LicenseTemplatesService>();
            kernel.Bind<ILicenseClauseTemplateService>().To<LicenseClauseTemplateService>();
            kernel.Bind<ILicenseMatchesService>().To<LicenseMatchesService>();
            kernel.Bind<ILicenseComparerService>().To<LicenseComparerService>();
            kernel.Bind<ITokenService>().To<TokenService>();
            kernel.Bind<ISecurityService>().To<SecurityService>();
            kernel.Bind<ICustomLicenseService>().To<CustomLicenseService>();
            kernel.Bind<ILicenseFileProvider>().To<LicenseFileProvider>();
            kernel.Bind<IUrlProvider>().To<UrlProvider>();
            kernel.Bind<ILicenseVerificationService>().To<LicenseVerificationService>();
            kernel.Bind<IAdminService>().To<AdminService>();
            kernel.Bind<IConsumerRequestService>().To<ConsumerRequestService>();
            kernel.Bind<IConsumerProviderRegistrationService>().To<ConsumerProviderRegistrationService>();
        }
    }
}