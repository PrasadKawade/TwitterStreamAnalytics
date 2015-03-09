namespace DotNetSpain.StreamAnalytics.TwitterSender
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Newtonsoft.Json;
    using Tweetinvi.Core.Interfaces;

    public class EventHubSender
    {
        private EventHubClient client;

        public EventHubSender(string eventHubName)
        {
            if (string.IsNullOrEmpty(eventHubName))
            {
                throw new ArgumentNullException("eventHubName");
            }

            client = EventHubClient.CreateFromConnectionString(
                RoleEnvironment.GetConfigurationSettingValue("Microsoft.ServiceBus.ConnectionString"),
                eventHubName);
        }

        public Task SendMessage(ITweet value)
        {
            byte[] serialized = SerializeObject(value);
            EventData data = new EventData(serialized);
            return client.SendAsync(data);
        }

        private byte[] SerializeObject(ITweet value)
        {
            byte[] result = null;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error = new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>(OnSerializationError);
            settings.NullValueHandling = NullValueHandling.Ignore;


            string json = JsonConvert.SerializeObject(value.TweetDTO, settings);
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    using (GZipStream gzip = new GZipStream(
            //        ms, CompressionMode.Compress))
            //    {
            //        result = Encoding.UTF8.GetBytes(json);
            //        gzip.Write(result, 0, result.Length);
            //        gzip.Flush();
            //        ms.Position = 0;
            //        result = ms.ToArray();
            //    }
            //}
            result = Encoding.UTF8.GetBytes(json);


            return result;
        }

        private void OnSerializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
        }
    }
}
