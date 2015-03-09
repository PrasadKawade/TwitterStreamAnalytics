namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    public class TwitterEventProcessorFactory : IEventProcessorFactory
    {
        private Action<EventHubMessagesEventArg> action;

        public TwitterEventProcessorFactory(Action<EventHubMessagesEventArg> value)
        {
            this.action = value;
        }

        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            TwitterEventProcessor processor = new TwitterEventProcessor();
            processor.EventHubMessageReceived += OnEventHubMessageReceived;
            return processor;
        }

        private void OnEventHubMessageReceived(object sender, EventHubMessagesEventArg e)
        {
            if (action != null)
            {
                action(e);
            }
        }
    }
}
