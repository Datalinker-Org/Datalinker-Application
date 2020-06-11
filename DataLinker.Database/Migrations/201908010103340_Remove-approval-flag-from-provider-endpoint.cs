namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removeapprovalflagfromproviderendpoint : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ProviderEndpoints", "NeedPersonalApproval");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProviderEndpoints", "NeedPersonalApproval", c => c.Boolean(nullable: false));
        }
    }
}
