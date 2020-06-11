namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateTableConsumerProviderRegistration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConsumerProviderRegistrations",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ConsumerApplicationID = c.Int(nullable: false),
                        OrganizationLicenseID = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Remarks = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ConsumerProviderRegistrations");
        }
    }
}
