namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialRelease : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TwitterItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TwittId = c.String(),
                        Text = c.String(),
                        Source = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UserId = c.String(),
                        ProfileImageUrl = c.String(),
                        UserName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TwitterItems");
        }
    }
}
