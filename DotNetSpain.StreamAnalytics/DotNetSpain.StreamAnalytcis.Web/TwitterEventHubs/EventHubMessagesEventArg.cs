namespace DotNetSpain.StreamAnalytcis.Web.TwitterEventHubs
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    public class EventHubMessagesEventArg : EventArgs
    {
        public EventHubMessagesEventArg(List<Stream> items, string name)
        {
            Items = new ReadOnlyCollection<Stream>(items);
            EventHubName = name;
        }

        public IReadOnlyCollection<Stream> Items { get; private set; }

        public string EventHubName { get; private set; }

    }
}