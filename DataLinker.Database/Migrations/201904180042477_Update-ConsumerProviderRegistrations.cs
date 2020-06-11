namespace DataLinker.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateConsumerProviderRegistrations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConsumerProviderRegistrations", "ApprovedAt", c => c.DateTime());
            AddColumn("dbo.ConsumerProviderRegistrations", "ApprovedBy", c => c.Int());
            AddColumn("dbo.ConsumerProviderRegistrations", "ProviderApprovedAt", c => c.DateTime());
            AddColumn("dbo.ConsumerProviderRegistrations", "ProviderApprovedBy", c => c.Int());
            AddColumn("dbo.ConsumerProviderRegistrations", "DeclinedAt", c => c.DateTime());
            AddColumn("dbo.ConsumerProviderRegistrations", "DeclinedBy", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConsumerProviderRegistrations", "DeclinedBy");
            DropColumn("dbo.ConsumerProviderRegistrations", "DeclinedAt");
            DropColumn("dbo.ConsumerProviderRegistrations", "ProviderApprovedBy");
            DropColumn("dbo.ConsumerProviderRegistrations", "ProviderApprovedAt");
            DropColumn("dbo.ConsumerProviderRegistrations", "ApprovedBy");
            DropColumn("dbo.ConsumerProviderRegistrations", "ApprovedAt");
        }
    }
}
