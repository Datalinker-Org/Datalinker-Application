namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateLicenseSectionmodel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LicenseSections", "UpdatedAt", c => c.DateTime());
            AlterColumn("dbo.LicenseSections", "UpdatedBy", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LicenseSections", "UpdatedBy", c => c.DateTime(nullable: false));
            AlterColumn("dbo.LicenseSections", "UpdatedAt", c => c.DateTime(nullable: false));
        }
    }
}
