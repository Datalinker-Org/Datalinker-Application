namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateLicenseAgreements : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ProviderLicenseID");
            DropColumn("dbo.LicenseAgreements", "ProviderLicenseID");
            DropForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ConsumerOrganizationID");
            DropColumn("dbo.LicenseAgreements", "ConsumerOrganizationID");
            DropForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ProviderOrganizationID");
            DropColumn("dbo.LicenseAgreements", "ProviderOrganizationID");
            DropForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_DataSchemaID");
            DropColumn("dbo.LicenseAgreements", "DataSchemaID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.LicenseAgreements", "DataSchemaID", c => c.Int(nullable: false));
            AddForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ProviderLicenseID", "dbo.DataSchemas");
            AddColumn("dbo.LicenseAgreements", "ProviderOrganizationID", c => c.Int(nullable: false));
            AddForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ProviderOrganizationID","dbo.Organizations");
            AddColumn("dbo.LicenseAgreements", "ConsumerOrganizationID", c => c.Int(nullable: false));
            AddForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ConsumerOrganizationID", "dbo.Organizations");
            AddColumn("dbo.LicenseAgreements", "ProviderLicenseID", c => c.Int(nullable: false));
            AddForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ProviderLicenseID","dbo.OrganizationLicenses");
        }
    }
}
