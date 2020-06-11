namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_UserInputs_Table : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserInputs",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        LicenseSelection = c.String(),
                        DataSchemaId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserInputs");
        }
    }
}
