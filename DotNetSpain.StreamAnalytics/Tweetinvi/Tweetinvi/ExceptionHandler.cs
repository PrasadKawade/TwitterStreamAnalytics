using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Tweetinvi.Core.Events.EventArguments;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Core.Interfaces.Exceptions;
using Tweetinvi.Logic.Exceptions;

namespace Tweetinvi
{
    public static class ExceptionHandler
    {
        [ThreadStatic]
        private static IExceptionHandler _exceptionHandler;
        private static IExceptionHandler TheExceptionHandler
        {
            get
            {
                if (_exceptionHandler == null)
                {
                    Initialise();
                }

                return _exceptionHandler;
            }
        }

        public static event EventHandler<GenericEventArgs<ITwitterException>> WebExceptionReceived
        {
            add { TheExceptionHandler.WebExceptionReceived += value; }
            remove { TheExceptionHandler.WebExceptionReceived -= value; }
        }

        static ExceptionHandler()
        {
            Initialise();
        }

        private static void Initialise()
        {
            _exceptionHandler = TweetinviContainer.Resolve<IExceptionHandler>();
        }

        public static bool SwallowWebExceptions
        {
            get { return TheExceptionHandler.SwallowWebExceptions; } 
            set { TheExceptionHandler.SwallowWebExceptions = value; }
        }

        public static bool LogExceptions
        {
            get { return TheExceptionHandler.LogExceptions; }
            set { TheExceptionHandler.LogExceptions = value; }
        }

        public static IEnumerable<ITwitterException> GetExceptions()
        {
            return TheExceptionHandler.ExceptionInfos;
        }

        public static ITwitterException GetLastException()
        {
            return TheExceptionHandler.ExceptionInfos.LastOrDefault();
        }

        public static TwitterException AddWebException(WebException webException, string url)
        {
            return TheExceptionHandler.AddWebException(webException, url);
        }

        public static void ClearLoggedExceptions()
        {
            TheExceptionHandler.ClearLoggedExceptions();
        }

        public static ITwitterException GenerateTwitterException(WebException webException, string url)
        {
            return TheExceptionHandler.GenerateTwitterException(webException, url);
        }

        public static string GetLifetimeExceptionDetails()
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (var twitterException in _exceptionHandler.ExceptionInfos)
            {
                strBuilder.Append(twitterException);
                strBuilder.Append("---");
            }
            return strBuilder.ToString();
        }
    }
}