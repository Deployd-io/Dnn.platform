// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    using System.Web.Mvc;

    public class FakeHandleExceptionRedirectAttribute : FakeRedirectAttribute, IExceptionFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static bool IsExceptionHandled { get; set; }

        public void OnException(ExceptionContext filterContext)
        {
			Logger.LogDebug(">>> Entering OnException(filterContext={FilterContext})", filterContext);
            filterContext.Result = Result;
            filterContext.ExceptionHandled = IsExceptionHandled;
			Logger.LogDebug("<<< Exiting OnException(filterContext={FilterContext})", filterContext);
			Logger.LogDebug("OnException(filterContext={FilterContext}): filterContext.ExceptionHandled → {Arg1}", filterContext, filterContext.ExceptionHandled);
			Logger.LogDebug("OnException(filterContext={FilterContext}): filterContext.Result → {Arg1}", filterContext, filterContext.Result);
        }
    }
}
