namespace DataLinker.Database.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class MakeLicenseTemplateIdnotrequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrganizationLicenses", "LicenseTemplateID", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrganizationLicenses", "LicenseTemplateID", c => c.Int(nullable: false));
        }
    }
}
