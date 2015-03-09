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


    public class EventHubPersistenceSource
    {
        private EventHubClient eventHubClient;
        private EventHubConsumerGroup defaultConsumerGroup;
        private EventProcessorHost eventProcessorHost;

        private CloudTableClient client;
        private CloudTable table;

        public EventHubPersistenceSource(string path)
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
                try
                {
                    JObject json = JObject.Parse(value);
                    SqlTwitterItem twitterItem = new SqlTwitterItem()
                    {
                        TwittId = json["id"].Value<string>(),
                        CreatedAt = json["created_at"].Value<DateTime>(),
                        Path = eventHubClient.Path,
                        ProfileImageUrl = json["user"]["profile_image_url"].Value<string>(),
                        Source = json["source"].Value<string>(),
                        Text = json["text"].Value<string>(),
                        UserId = json["user"]["Id"].Value<string>(),
                        UserName = json["user"]["name"].Value<string>()
                    };

                    sqlContext.TwitterItems.Add(twitterItem);
                    int rows = sqlContext.SaveChanges();
                    Trace.WriteLine(string.Format("SQL Azure rows inserted {0}", rows), "Information");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.ToString());
                    Trace.WriteLine(value);
                }
            }
        }


        private void HandleAzureStorageInsert(string value)
        {
            try
            {
                JObject json = JObject.Parse(value);

                StorageTwitterItem item = new StorageTwitterItem();
                item.Path = eventHubClient.Path;
                item.RowKey = string.Concat(Guid.NewGuid().ToString(), "-", json["id"].Value<string>());
                item.Id = json["id"].Value<string>();
                item.PartitionKey = json["id"].Value<string>();
                item.Text = json["text"].Value<string>();
                item.CreatedAt = json["created_at"].Value<DateTime>();
                item.UserId = json["user"]["Id"].Value<string>();
                item.ProfileImageUrl = json["user"]["profile_image_url"].Value<string>();
                item.UserName = json["user"]["name"].Value<string>();
                item.Source = json["source"].Value<string>();
                TableOperation insertOperation = TableOperation.Insert(item);
                table.Execute(insertOperation);


                Trace.WriteLine("Azure Storage rows inserted 1", "Information");
            }
            catch (Exception eX)
            {
                Trace.WriteLine(eX.ToString());
                Trace.WriteLine(value);
            }
        }
    }
}

