namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class UpdateLicenseAgreementsToUseConsumerProviderRegistrations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LicenseAgreements", "ConsumerProviderRegistrationId", c => c.Int(nullable: false));
            DropForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ConsumerLicenseID");
            DropColumn("dbo.LicenseAgreements", "ConsumerLicenseID");
        }

        public override void Down()
        {
            AddColumn("dbo.LicenseAgreements", "ConsumerLicenseID", c => c.Int(nullable: false));
            AddForeignKey("dbo.LicenseAgreements", "FK_LicenseAgreements_ConsumerLicenseID", "dbo.OrganizationLicenses");
            DropColumn("dbo.LicenseAgreements", "ConsumerProviderRegistrationId");
        }
    }
}
