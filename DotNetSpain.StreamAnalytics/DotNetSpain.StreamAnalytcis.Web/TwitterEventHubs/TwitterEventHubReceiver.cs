namespace DotNetSpain.StreamAnalytcis.Web.TwitterEventHubs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Configuration;
    using Microsoft.ServiceBus.Messaging;

    public class TwitterEventHubReceiver
    {
        private EventHubClient eventHubClient;
        private EventHubConsumerGroup defaultConsumerGroup;
        private EventProcessorHost eventProcessorHost;
        private Action<EventHubMessagesEventArg> action;
        private EventProcessorOptions eventProcessorOptions;
        private string path;

        public TwitterEventHubReceiver(Action<EventHubMessagesEventArg> value, string path)
        {
            this.action = value;
            this.path = path;
        }

        public async Task StartReveiver()
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(
               WebConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"],
               path);
            defaultConsumerGroup = eventHubClient.GetConsumerGroup("web");
            string blobConnectionString = WebConfigurationManager.AppSettings["AzureStorageConnectionString"]; // Required for checkpoint/state
            eventProcessorHost = new EventProcessorHost(
                "singleworker",
                eventHubClient.Path,
                defaultConsumerGroup.GroupName,
                WebConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"],
                blobConnectionString);

            eventProcessorOptions = EventProcessorOptions.DefaultOptions;
            eventProcessorOptions.InitialOffsetProvider = new Func<string, object>(SelectOffsetProvider);
            eventProcessorOptions.ExceptionReceived += OnEventProcessorOptionsExceptionReceived;

            await eventProcessorHost.RegisterEventProcessorFactoryAsync(
                new TwitterEventProcessorFactory(OnEventHubMessagesReveived),
                eventProcessorOptions);
        }

        private void OnEventProcessorOptionsExceptionReceived(object sender, ExceptionReceivedEventArgs e)
        {
            if (e.Action != null && e.Exception != null)
            {
                Trace.WriteLine(string.Format(
                    "OnEventProcessorOptionsExceptionReceived {0} | {1}",
                    e.Action, 
                    e.Exception));
            }
        }

        private string SelectOffsetProvider(object partitionId)
        {
            return DateTime.UtcNow.ToString();
        }

        public async Task Close()
        {
            await eventProcessorHost.UnregisterEventProcessorAsync();
            eventHubClient.Close();
        }

        private void OnEventHubMessagesReveived(EventHubMessagesEventArg value)
        {
            if (action != null)
            {
                action(value);
            }
        }
    }
}