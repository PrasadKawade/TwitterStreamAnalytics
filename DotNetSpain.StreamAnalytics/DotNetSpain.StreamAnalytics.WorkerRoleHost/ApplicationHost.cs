using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetSpain.StreamAnalytics.TwitterSender;
using Microsoft.WindowsAzure.ServiceRuntime;
using Tweetinvi.Core.Enum;

namespace DotNetSpain.StreamAnalytics.WorkerRoleHost
{
    public class ApplicationHost
    {
        private TwitterStreamingEngine engine;

        public ApplicationHost()
        {
            RoleEnvironment.Changed += OnRoleEnvironmentChanged;
        }

        private void OnRoleEnvironmentChanged(object sender, RoleEnvironmentChangedEventArgs e)
        {
            var changes = e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>();
            if (changes.Count() > 0)
            {
                StopApp();
                ActivateApp();
            }
        }

        public void ActivateApp()
        {
            if (engine != null)
            {
                if (engine.TwitterStream.StreamState == StreamState.Stop)
                {
                    engine = new TwitterStreamingEngine();
                    engine.CreateStream();
                    engine.Error += OnError;

                    Trace.WriteLine("Starting Twitter Streaming Service");
                }
            }
            else
            {
                engine = new TwitterStreamingEngine();
                engine.CreateStream();
                engine.Error += OnError;

                Trace.WriteLine("Starting Twitter Streaming Service");
            }
        }

        private void OnError(object sender, Tweetinvi.Core.Events.EventArguments.StreamExceptionEventArgs e)
        {
            engine.Error -= OnError;
            StopApp();
            ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(StartTwitterAsync), null);
            Trace.WriteLine("ThreadPool work item sent");
        }

        private void StartTwitterAsync(object target)
        {
            Trace.WriteLine("ThreadPoolWorkItem waiting...");
            Thread.Sleep(1000 * 30);
            Trace.WriteLine("Starting app");
            
            ActivateApp();
        }

        public void StopApp()
        {
            if (engine != null)
            {
                Trace.WriteLine("Stopping Twitter Streaming Service");
                engine.Stop();
            }
        }
    }
}
