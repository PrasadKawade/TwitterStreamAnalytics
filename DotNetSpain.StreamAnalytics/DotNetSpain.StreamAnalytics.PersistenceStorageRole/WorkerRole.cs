using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;

namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private EventHubPersistence twitterOutputLike;
        private EventHubPersistence twitterOutputDislike;
        private EventHubPersistenceSource twitter;

        public override void Run()
        {
            Trace.TraceInformation("DotNetSpain.StreamAnalytics.PersistenceStorageRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("DotNetSpain.StreamAnalytics.PersistenceStorageRole has been started");
            twitterOutputLike = new EventHubPersistence(RoleEnvironment.GetConfigurationSettingValue("EventHubNameLike"));
            twitterOutputDislike = new EventHubPersistence(RoleEnvironment.GetConfigurationSettingValue("EventHubNameDislike"));
            twitter = new EventHubPersistenceSource(RoleEnvironment.GetConfigurationSettingValue("EventHubName"));
            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("DotNetSpain.StreamAnalytics.PersistenceStorageRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("DotNetSpain.StreamAnalytics.PersistenceStorageRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            await twitterOutputDislike.Initialize();
            await twitterOutputLike.Initialize();
            await twitter.Initialize();

            //CloudStorageAccount account = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("AzureStorage"));
            //var table = account.CreateCloudTableClient();
            //var t = table.GetTableReference("TwitterItem");
            //t.DeleteIfExists();
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                //Trace.TraceInformation("Working");
                await Task.Delay(1000);
            }
        }
    }
}
