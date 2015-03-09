namespace DotNetSpain.StreamAnalytcis.Web.TwitterEventHubs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    public class ReceiverQueue
    {
        private static ReceiverQueue instance = new ReceiverQueue();

        private Dictionary<string, Tuple<List<Tuple<string, Action<EventHubMessagesEventArg>>>, TwitterEventHubReceiver>> items =
            new Dictionary<string, Tuple<List<Tuple<string, Action<EventHubMessagesEventArg>>>, TwitterEventHubReceiver>>();
       // private ReaderWriterLockSlim rwls = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);


        private ReceiverQueue()
        {

        }

        public static ReceiverQueue Instance
        {
            get
            {
                return instance;
            }
        }
        
        public void RemoveClient(string clientId)
        {
            lock(typeof(ReceiverQueue))
            {
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new ArgumentNullException("clientId");
                }

                //    rwls.EnterUpgradeableReadLock();
                string keyToRemove = null;
                try
                {
                    foreach (var item in items)
                    {
                        var found = (from p in item.Value.Item1
                                     where p.Item1 == clientId
                                     select p).FirstOrDefault();
                        if (found != null)
                        {
                            item.Value.Item1.Remove(found);
                            if (item.Value.Item1.Count == 0)
                            {
                                item.Value.Item2.Close().Wait();
                                keyToRemove = item.Key;
                            }
                        }
                    }
                }
                finally
                {
                    //rwls.ExitUpgradeableReadLock();
                }

                if (!string.IsNullOrEmpty(keyToRemove))
                {
                    //rwls.EnterWriteLock();
                    try
                    {
                        items.Remove(keyToRemove);
                    }
                    finally
                    {
                        //rwls.ExitWriteLock();
                    }
                }
            }
        }
        
        public void AddClientId(string clientId, string eventHubName, Action<EventHubMessagesEventArg> value)
        {
            lock(typeof(ReceiverQueue))
            {
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new ArgumentNullException("clientId");
                }

                if (string.IsNullOrEmpty(eventHubName))
                {
                    throw new ArgumentNullException("eventHubName");
                }

                //rwls.EnterUpgradeableReadLock();
                try
                {
                    if (!items.ContainsKey(eventHubName))
                    {
                        TwitterEventHubReceiver receiver = new TwitterEventHubReceiver(OnMessageReceived, eventHubName);
                        //rwls.EnterWriteLock();
                        try
                        {
                            items.Add(eventHubName, new Tuple<List<Tuple<string, Action<EventHubMessagesEventArg>>>, TwitterEventHubReceiver>(
                                new List<Tuple<string, Action<EventHubMessagesEventArg>>>() { new Tuple<string, Action<EventHubMessagesEventArg>>(clientId, value) }, receiver));
                            receiver.StartReveiver();
                        }
                        finally
                        {
                            //rwls.ExitWriteLock();
                        }
                    }
                    else
                    {
                        items[eventHubName].Item1.Add(new Tuple<string, Action<EventHubMessagesEventArg>>(clientId, value));
                    }
                }
                finally
                {
                    //rwls.ExitUpgradeableReadLock();
                }
            }
        }
        
        private void OnMessageReceived(EventHubMessagesEventArg value)
        {
            lock(typeof(ReceiverQueue))
            {
                List<Tuple<string, Action<EventHubMessagesEventArg>>> list = null;
                //rwls.EnterReadLock();
                try
                {
                    if (items.ContainsKey(value.EventHubName))
                    {
                        list = items[value.EventHubName].Item1;
                    }
                }
                finally
                {
                    //rwls.ExitReadLock();
                }

                if (list != null)
                {
                    foreach (var item in list)
                    {
                        item.Item2(value);
                    }
                }
            }
        }
    }
}