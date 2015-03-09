namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole.TableStorage.Model
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;

    public class TwitterItem : TableEntity
    {
        public string Id { get; set; }
        
        public string Text { get; set; }

        public string Source { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }

        public string ProfileImageUrl { get; set; }

        public string UserName { get; set; }

        public string Path { get; set; }
    }
}
