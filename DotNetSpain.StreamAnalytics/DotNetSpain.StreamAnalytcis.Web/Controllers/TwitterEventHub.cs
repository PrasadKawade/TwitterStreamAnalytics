namespace DotNetSpain.StreamAnalytcis.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using DotNetSpain.StreamAnalytcis.Web.TwitterEventHubs;
    using Microsoft.AspNet.SignalR;
    using Newtonsoft.Json.Linq;
    using Tweetinvi.Core.Interfaces.DTO;
    using Tweetinvi.Logic.DTO;

    public class TwitterEventHub : Hub
    {
        private static Dictionary<string, TwitterEventHubReceiver> items = new Dictionary<string, TwitterEventHubReceiver>();
        private TwitterEventHubReceiver receiver;

        public TwitterEventHub()
        {

        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            string id = Context.ConnectionId;
            ReceiverQueue.Instance.RemoveClient(id);
            await base.OnDisconnected(stopCalled);
        }

        private void OnEventHubMessagesReveived(EventHubMessagesEventArg value)
        {
            try
            {
                foreach (var stream in value.Items)
                {
                    byte[] buff = new byte[stream.Length];
                    stream.Read(buff, 0, buff.Length);
                    stream.Position = 0;
                    string json = Encoding.UTF8.GetString(buff);

                    if (json.StartsWith("{"))
                    {
                        Clients.Caller.sendTwitter(json);
                    }
                    else
                    {
                        if (json.StartsWith(@"[{""id"""))
                        {
                            JArray items = JArray.Parse(json);
                            foreach (var arrayItem in items)
                            {
                                string twittJson = arrayItem.ToString();
                                Clients.Caller.sendTwitter(twittJson);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

        public void StartEventHub(string path)
        {
            string id = Context.ConnectionId;
            ReceiverQueue.Instance.AddClientId(id, path, OnEventHubMessagesReveived);
        }
    }
}