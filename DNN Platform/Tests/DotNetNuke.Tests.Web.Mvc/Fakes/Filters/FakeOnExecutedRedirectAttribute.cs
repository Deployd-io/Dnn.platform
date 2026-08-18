// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    using System.Web.Mvc;
	using Microsoft.Extensions.Logging;

    public class FakeOnExecutedRedirectAttribute : FakeRedirectAttribute
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
			_logger.LogDebug(">>> Entering OnActionExecuted(filterContext={FilterContext})", filterContext);
			_logger.LogDebug("OnActionExecuted(filterContext={FilterContext}): filterContext.Result → {Arg1}", filterContext, filterContext.Result);
            filterContext.Result = Result;
			_logger.LogDebug("<<< Exiting OnActionExecuted(filterContext={FilterContext})", filterContext);
        }
    }
}
