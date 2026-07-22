// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    using System.Web.Mvc;

    public class FakeOnExecutedRedirectAttribute : FakeRedirectAttribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
			Logger.LogDebug(">>> Entering OnActionExecuted(filterContext={FilterContext})", filterContext);
			Logger.LogDebug("OnActionExecuted(filterContext={FilterContext}): filterContext.Result → {Arg1}", filterContext, filterContext.Result);
            filterContext.Result = Result;
			Logger.LogDebug("<<< Exiting OnActionExecuted(filterContext={FilterContext})", filterContext);
        }
    }
}
