namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_OrganisationLicenses_To_Support_CustomLicenses : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrganizationLicenses", "CustomLicenseID", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrganizationLicenses", "CustomLicenseID");
        }
    }
}
