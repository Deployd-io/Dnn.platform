// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    using System.Web.Mvc;
	using Microsoft.Extensions.Logging;

    public class FakeHandleExceptionRedirectAttribute : FakeRedirectAttribute, IExceptionFilter
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public static bool IsExceptionHandled { get; set; }

        public void OnException(ExceptionContext filterContext)
        {
			_logger.LogDebug(">>> Entering OnException(filterContext={FilterContext})", filterContext);
            filterContext.Result = Result;
            filterContext.ExceptionHandled = IsExceptionHandled;
			_logger.LogDebug("OnException(filterContext={FilterContext}): filterContext.ExceptionHandled → {Arg1}", filterContext, filterContext.ExceptionHandled);
			_logger.LogDebug("OnException(filterContext={FilterContext}): filterContext.Result → {Arg1}", filterContext, filterContext.Result);
			_logger.LogDebug("<<< Exiting OnException(filterContext={FilterContext})", filterContext);
        }
    }
}
