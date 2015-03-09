namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole.Sql
{
    using System.Data.Entity;
    using DotNetSpain.StreamAnalytics.PersistenceStorageRole.Sql.Model;

    public class EventHubDbContext : DbContext
    {
        public EventHubDbContext()
            : base("EventHubDbContext")
        {

        }

        public DbSet<TwitterItem> TwitterItems { get; set; }
    }
}
