namespace DotNetSpain.StreamAnalytics.TwitterSender
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public class ProtectedTimer
    {
        private Action action;
        private TimeSpan time;
        private Timer timer;
        private volatile int isWorking;

        public ProtectedTimer(TimeSpan time, Action action)
        {
            this.action = action;
            this.time = time;
            Start();
        }

        public void Stop()
        {
            timer.Dispose();
        }

        public void Start()
        {
            timer = new Timer(new TimerCallback(OnTimerTick), null, 0, (int)time.TotalMilliseconds);
        }

        private void OnTimerTick(object arg)
        {
            Interlocked.Increment(ref isWorking);
            if (isWorking == 1)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("ProtectedTimer: " + ex.ToString());
                }

                isWorking = 0;
            }
        }
    }
}
