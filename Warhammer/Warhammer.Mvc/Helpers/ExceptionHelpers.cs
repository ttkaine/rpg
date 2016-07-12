using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Warhammer.Core.Abstract;

namespace Warhammer.Mvc.Helpers
{
    public static class ExceptionHelpers
    {
        public static Exception LogExceptionSequence(this Exception exception, DateTime timestamp, HttpRequestBase request)
        {
            try
            {
                if (exception == null) return null;

                var e = exception;
                var identifier = e.GetHashCode().ToString();
                e = new Exception(identifier, e);

                var sequence = 1;
                LogException(e, identifier, sequence, timestamp);
                var innerException = e.InnerException;

                while (innerException != null)
                {
                    sequence++;
                    LogException(innerException, identifier, sequence, timestamp);
                    innerException = innerException.InnerException;
                }

                if (request != null)
                {
                    sequence++;

                    var usefulRequestInfo =
                        String.Format("BROWSER: {0}; VERSION: {1}; HOST ADDRESS: {2}; LANGUAGE: {3}; REFERRER URL: {4};",
                            request.Browser.Browser, request.Browser.Version, request.UserHostAddress,
                            request.UserLanguages != null && request.UserLanguages.Any() ? request.UserLanguages[0] : "NULL",
                            request.UrlReferrer != null ? request.UrlReferrer.AbsoluteUri : "NULL");

                    LogException(new Exception(usefulRequestInfo), identifier, sequence, timestamp);
                }

                return e;
            }
            catch (Exception)
            {
                // Do Nothing - If we're throwing exceptions trying to log an exception we're just making things worse.
                return null;
            }
        }

        private static void LogException(this Exception exception, string identifier, int sequence, DateTime timestamp)
        {
            var logger = DependencyResolver.Current.GetService<IExceptionLogHandler>();
            if (logger != null) logger.LogException(exception, identifier, sequence, timestamp);
        }
    }
}