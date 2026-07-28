// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Mvc.Fakes.Filters
{
    using System.Web.Mvc;

    public class FakeOnExecutingRedirectAttribute : FakeRedirectAttribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
			Logger.LogDebug(">>> Entering OnActionExecuting(filterContext={FilterContext})", filterContext);
			Logger.LogDebug("OnActionExecuting(filterContext={FilterContext}): filterContext.Result → {Arg1}", filterContext, filterContext.Result);
            filterContext.Result = Result;
			Logger.LogDebug("<<< Exiting OnActionExecuting(filterContext={FilterContext})", filterContext);
        }
    }
}
