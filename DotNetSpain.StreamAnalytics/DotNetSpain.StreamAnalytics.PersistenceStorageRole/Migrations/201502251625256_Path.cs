namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Path : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TwitterItems", "Path", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TwitterItems", "Path");
        }
    }
}
