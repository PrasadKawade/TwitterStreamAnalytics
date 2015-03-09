namespace DotNetSpain.StreamAnalytics.TwitterSender.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Twitter
    {
        public string Text { get; set; }

        public List<string> Hashtags { get; set; }

        public bool IsRetweet { get; set; }
    }
}
