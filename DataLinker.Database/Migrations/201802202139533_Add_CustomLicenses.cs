namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_CustomLicenses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomLicenses",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        Content = c.Binary(),
                        MimeType = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CustomLicenses");
        }
    }
}
