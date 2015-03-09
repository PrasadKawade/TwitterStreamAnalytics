namespace DotNetSpain.StreamAnalytcis.Web.TwitterEventHubs
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public class TwitterEventProcessor : IEventProcessor
    {
        private PartitionContext partitionContext;
        public event EventHandler<EventHubMessagesEventArg> EventHubMessageReceived;

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.WriteLine(string.Format("Processor Shuting Down.  Partition '{0}', Reason: '{1}'.", this.partitionContext.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
               // await context.CheckpointAsync();
            }
        }

        public async Task OpenAsync(PartitionContext context)
        {
            this.partitionContext = context;
            Trace.WriteLine(string.Format("TwitterEventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            var hanlder = EventHubMessageReceived;
            if (hanlder != null)
            {
                List<Stream> list = new List<Stream>();
                foreach (var item in messages)
                {
                    list.Add(item.GetBodyStream());
                }
                hanlder(this, new EventHubMessagesEventArg(list, context.EventHubPath));
            }

           // await context.CheckpointAsync();
        }
    }
}