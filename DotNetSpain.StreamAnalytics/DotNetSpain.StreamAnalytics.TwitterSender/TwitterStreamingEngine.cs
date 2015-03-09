namespace DotNetSpain.StreamAnalytics.TwitterSender
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Tweetinvi;
    using Tweetinvi.Core.Events.EventArguments;
    using Tweetinvi.Core.Interfaces.Streaminvi;

    public class TwitterStreamingEngine
    {
        private IFilteredStream stream;
        private EventHubSender twitterSender;
        private EventHubSender twitterLike;
        private EventHubSender twitterDislike;
        private ProtectedTimer rateLimit;

        public TwitterStreamingEngine()
        {
            stream = Stream.CreateFilteredStream();
            twitterSender = new EventHubSender(RoleEnvironment.GetConfigurationSettingValue("EventHubName"));
            twitterLike = new EventHubSender(RoleEnvironment.GetConfigurationSettingValue("EventHubTwitterLike"));
            twitterDislike = new EventHubSender(RoleEnvironment.GetConfigurationSettingValue("EventHubTwitterDislike"));
        }

        public event EventHandler<StreamExceptionEventArgs> Error;

        public IFilteredStream TwitterStream
        {
            get
            {
                return stream;
            }
        }        

        public void CreateStream()
        {
            TwitterCredentials.SetCredentials(
               RoleEnvironment.GetConfigurationSettingValue("UserAccessToken"),
               RoleEnvironment.GetConfigurationSettingValue("UserAccessSecret"),
               RoleEnvironment.GetConfigurationSettingValue("ConsumerKey"),
               RoleEnvironment.GetConfigurationSettingValue("ConsumerSecret"));

            //rateLimit = new ProtectedTimer(TimeSpan.FromSeconds(60), new Action(OnCheckRateRequestLimit));
            //rateLimit.Start();

            string words = RoleEnvironment.GetConfigurationSettingValue("TwitterSearchTerms");
            string[] items = words.Split(';');
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item))
                {
                    stream.AddTrack(item);
                    Trace.Write(string.Concat("Track item -> ", item), typeof(TwitterStreamingEngine).FullName);
                }
            }

            stream.MatchingTweetReceived += OnMatchingTweetReceived;
            stream.DisconnectMessageReceived += OnStreamDisconnectMessageReveived;
            stream.WarningFallingBehindDetected += OnWarningFallingBehindDetected;
            stream.LimitReached += OnLimitReached;
            stream.StreamStarted += OnStreamStarted;
            stream.StreamStopped += OnStreamStopped;
            stream.StartStreamMatchingAllConditions();
        }

        private void OnWarningFallingBehindDetected(object sender, WarningFallingBehindEventArgs e)
        {
            Trace.WriteLine(string.Format(
                "Disconnect Message Code:{0} Message:{1} PercentFull:{2}",
                e.WarningMessage.Code,
                e.WarningMessage.Message,
                e.WarningMessage.PercentFull));
        }

        private void OnStreamDisconnectMessageReveived(object sender, DisconnectMessageEventArgs e)
        {
            Trace.WriteLine(string.Format(
                "Disconnect Message Code:{0} Reason:{1} StreamName:{2}", 
                e.DisconnectMessage.Code, 
                e.DisconnectMessage.Reason, 
                e.DisconnectMessage.StreamName));
        }

        public void Stop()
        {
            if (stream != null)
            {
                stream.StopStream();
            }
        }

        private void OnCheckRateRequestLimit()
        {
            var limits = Tweetinvi.RateLimit.GetCurrentCredentialsRateLimits();
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] pInfo = limits.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var item in pInfo)
            {
                if (item.CanRead)
                {
                    try
                    {
                        object value = item.GetValue(limits);
                        if (value != null)
                        {
                            sb.AppendLine(string.Format("{0} | {1}", item.Name, value.ToString()));
                        }
                    }
                    catch { }
                }
            }

            Trace.WriteLine(string.Concat("Limits -> ", sb.ToString()), "Information");
        }

        private void OnStreamStopped(object sender, StreamExceptionEventArgs e)
        {
            Trace.WriteLine("StreamStopped", "Information");
            if (e.Exception != null)
            {
                Trace.WriteLine(e.Exception.ToString(), "Error");

                var handler = Error;
                if (handler != null)
                {
                    handler(this, e);
                }
            }
        }

        private void OnStreamStarted(object sender, EventArgs e)
        {
            Trace.WriteLine("StreamStarted", "Information");
        }

        private void OnLimitReached(object sender, LimitReachedEventArgs e)
        {
            Trace.WriteLine(string.Format("LimitReached NumberOfTweetsNotReceived {0}", e.NumberOfTweetsNotReceived), "Information");
            RoleEnvironment.RequestRecycle();
        }

        private void OnMatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs e)
        {
            Trace.WriteLine(string.Concat("Twitter | ", e.Tweet.Text));
            twitterSender.SendMessage(e.Tweet);
            twitterLike.SendMessage(e.Tweet);
            twitterDislike.SendMessage(e.Tweet);
        }
    }
}
