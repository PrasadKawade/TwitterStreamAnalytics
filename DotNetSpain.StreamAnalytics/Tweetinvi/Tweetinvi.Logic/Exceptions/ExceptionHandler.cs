using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Tweetinvi.Core.Events;
using Tweetinvi.Core.Events.EventArguments;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Core.Injectinvi;
using Tweetinvi.Core.Interfaces.Exceptions;

namespace Tweetinvi.Logic.Exceptions
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly IFactory<ITwitterException> _twitterExceptionUnityFactory;

        private readonly object _lockExceptionInfos = new object();
        private readonly List<ITwitterException> _exceptionInfos;
        public event EventHandler<GenericEventArgs<ITwitterException>> WebExceptionReceived;
        public bool SwallowWebExceptions { get; set; }
        public bool LogExceptions { get; set; }

        public ExceptionHandler(IFactory<ITwitterException> twitterExceptionUnityFactory)
        {
            _twitterExceptionUnityFactory = twitterExceptionUnityFactory;
            _exceptionInfos = new List<ITwitterException>();
            SwallowWebExceptions = true;
            LogExceptions = true;
        }

        public IEnumerable<ITwitterException> ExceptionInfos
        {
            get { return _exceptionInfos; }
        }

        public ITwitterException LastExceptionInfos
        {
            get
            {
                lock (_lockExceptionInfos)
                {
                    return _exceptionInfos.LastOrDefault();
                }
            }
        }

        public void ClearLoggedExceptions()
        {
            lock (_lockExceptionInfos)
            {
                _exceptionInfos.Clear();
            }
        }

        public TwitterException AddWebException(WebException webException, string url)
        {
            var twitterException = GenerateTwitterException(webException, url);

            lock (_lockExceptionInfos)
            {
                _exceptionInfos.Add(twitterException);
            }

            this.Raise(WebExceptionReceived, twitterException);

            // Cannot throw from an interface :(
            return twitterException;
        }

        public TwitterException GenerateTwitterException(WebException webException, string url)
        {
            var webExceptionParameterOverride = _twitterExceptionUnityFactory.GenerateParameterOverrideWrapper("webException", webException);
            var urlParameterOverride = _twitterExceptionUnityFactory.GenerateParameterOverrideWrapper("url", url);
            var twitterException = _twitterExceptionUnityFactory.Create(webExceptionParameterOverride, urlParameterOverride);
            return twitterException as TwitterException;
        }
    }
}