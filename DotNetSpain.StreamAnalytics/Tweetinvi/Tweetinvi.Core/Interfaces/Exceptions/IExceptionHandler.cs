using System;
using System.Collections.Generic;
using System.Net;
using Tweetinvi.Core.Events.EventArguments;
using Tweetinvi.Core.Exceptions;
using Tweetinvi.Logic.Exceptions;

namespace Tweetinvi.Core.Interfaces.Exceptions
{
    public interface IExceptionHandler
    {
        event EventHandler<GenericEventArgs<ITwitterException>> WebExceptionReceived;

        bool SwallowWebExceptions { get; set; }
        bool LogExceptions { get; set; }

        IEnumerable<ITwitterException> ExceptionInfos { get; }
        ITwitterException LastExceptionInfos { get; }

        void ClearLoggedExceptions();

        TwitterException AddWebException(WebException webException, string url);
        TwitterException GenerateTwitterException(WebException webException, string url);
    }
}