namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;
    using DotNetSpain.StreamAnalytics.PersistenceStorageRole.Sql;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json.Linq;
    using SqlTwitterItem = DotNetSpain.StreamAnalytics.PersistenceStorageRole.Sql.Model.TwitterItem;
    using StorageTwitterItem = DotNetSpain.StreamAnalytics.PersistenceStorageRole.TableStorage.Model.TwitterItem;

    public class EventHubPersistence
    {
        private EventHubClient eventHubClient;
        private EventHubConsumerGroup defaultConsumerGroup;
        private EventProcessorHost eventProcessorHost;

        private CloudTableClient client;
        private CloudTable table;

        public EventHubPersistence(string path)
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(
               RoleEnvironment.GetConfigurationSettingValue("Microsoft.ServiceBus.ConnectionString"),
               path);
        }

        public async Task Initialize()
        {
            defaultConsumerGroup = eventHubClient.GetConsumerGroup("workerrole");
            string blobConnectionString = RoleEnvironment.GetConfigurationSettingValue("AzureStorage");
            eventProcessorHost = new EventProcessorHost(
                "singleworker",
                eventHubClient.Path,
                defaultConsumerGroup.GroupName,
                RoleEnvironment.GetConfigurationSettingValue("Microsoft.ServiceBus.ConnectionString"),
                blobConnectionString);
            client = CloudStorageAccount.Parse(blobConnectionString).CreateCloudTableClient();
            table = client.GetTableReference("TwitterItem");
            await table.CreateIfNotExistsAsync();
            await eventProcessorHost.RegisterEventProcessorFactoryAsync(new TwitterEventProcessorFactory(OnEventHubMessagesReveived));
        }

        private void OnEventHubMessagesReveived(EventHubMessagesEventArg value)
        {
            foreach (var item in value.Items)
            {
                using (item)
                {
                    byte[] buff = new byte[item.Length];
                    item.Read(buff, 0, buff.Length);
                    string json = Encoding.UTF8.GetString(buff);
                    HandleSqlInsert(json);
                    HandleAzureStorageInsert(json);
                }
            }
        }

        private void HandleSqlInsert(string value)
        {
            using (EventHubDbContext sqlContext = new EventHubDbContext())
            {
                JArray items = JArray.Parse(value);
                foreach (var item in items.Children())
                {
                    try
                    {
                        var twitterItem = item.ToObject<SqlTwitterItem>();
                        twitterItem.Path = eventHubClient.Path;
                        sqlContext.TwitterItems.Add(twitterItem);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                        Trace.WriteLine(item.ToString());
                    }
                }
                try
                {
                    int rows = sqlContext.SaveChanges();
                    Trace.WriteLine(string.Format("SQL Azure rows inserted {0}", rows), "Information");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                }
            }
        }

        private void HandleAzureStorageInsert(string value)
        {
            try
            {
                JArray items = JArray.Parse(value);
                int rows = 0;
                foreach (var json in items.Children())
                {
                    try
                    {
                        StorageTwitterItem item = new StorageTwitterItem();
                        item.Path = eventHubClient.Path;
                        item.RowKey = string.Concat(Guid.NewGuid().ToString(), "-", json["id"].Value<string>());
                        item.Id = json["id"].Value<string>();
                        item.PartitionKey = json["id"].Value<string>();
                        item.Text = json["text"].Value<string>();
                        item.CreatedAt = json["created_at"].Value<DateTime>();
                        item.UserId = json["userid"].Value<string>();
                        item.ProfileImageUrl = json["profile_image_url"].Value<string>();
                        item.UserName = json["name"].Value<string>();
                        item.Source = json["source"].Value<string>();
                        TableOperation insertOperation = TableOperation.Insert(item);
                        table.Execute(insertOperation);
                        rows++;
                    }
                    catch (Exception jsonException)
                    {
                        Trace.WriteLine(jsonException.ToString());
                        Trace.WriteLine(json);
                    }
                }

                Trace.WriteLine(string.Format("Azure Storage rows inserted {0}", rows), "Information");
            }
            catch (Exception eX)
            {
                Trace.WriteLine(eX.ToString());
                Trace.WriteLine(value);
            }
        }
    }
}
