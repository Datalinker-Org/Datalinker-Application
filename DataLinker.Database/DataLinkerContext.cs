using DataLinker.Database.Models;
using System.Data.Entity;

namespace DataLinker.Database
{
    public class DataLinkerContext: DbContext
    {
        public DataLinkerContext() : base("name=DataLinker.Database")
        {
        }

        public DbSet<Application> Applications { get; set; }

        public DbSet<ApplicationAuthentication> ApplicationAuthentication { get; set; }

        public DbSet<ApplicationToken> ApplicationTokens { get; set; }

        public DbSet<ConsumerRequest> ConsumerRequests { get; set; }

        public DbSet<LicenseAgreement> LicenseAgreements { get; set; }

        public DbSet<License> Licenses { get; set; }

        public DbSet<LicenseClause> LicenseCluases { get; set; }

        public DbSet<LicenseClauseTemplate> LicenseCluaseTemplates { get; set; }

        public DbSet<LicenseSection> LicenseSections { get; set; }

        public DbSet<LicenseTemplate> LicenseTemplates { get; set; }

        public DbSet<LicenseApprovalRequest> LicenseApprovalRequests { get; set; }

        public DbSet<LicenseMatch> LicenseMatches { get; set; }

        public DbSet<OrganizationLicense> OrganisationLicenses { get; set; }

        public DbSet<OrganizationLicenseClause> OrganisationLicenseClauses { get; set; }

        public DbSet<Organization> Organisations { get; set; }

        public DbSet<DataSchema> DataSchemas { get; set; }

        public DbSet<ProviderEndpoint> ProviderEndpoints { get; set; }
        
        public DbSet<SchemaFile> SchemaFiles { get; set; }

        public DbSet<SoftwareStatement> SoftwareStatements { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserInput> UserInputs { get; set; }

        public DbSet<CustomLicense> CustomLicenses { get; set; }

        public DbSet<ConsumerProviderRegistration> ConsumerProviderRegistrations { get; set; }
    }
}
