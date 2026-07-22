// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ResultCapturingActionInvoker : ControllerActionInvoker
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public ActionResult ResultOfLastInvoke { get; set; }

        /// <inheritdoc />
        protected override ActionExecutedContext InvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            var context = base.InvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters);
            this.ResultOfLastInvoke = context.Result;
            return context;
        }

        /// <inheritdoc />
        protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
        {
			Logger.LogDebug(">>> Entering InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception})", controllerContext, filters, exception);
            var context = base.InvokeExceptionFilters(controllerContext, filters, exception);
            this.ResultOfLastInvoke = context.Result;
			Logger.LogDebug("InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception}): context → {Context}", controllerContext, filters, exception, context);
			Logger.LogDebug("InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception}): this.ResultOfLastInvoke → {Arg3}", controllerContext, filters, exception, this.ResultOfLastInvoke);
			Logger.LogDebug("<<< Exiting InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception})", controllerContext, filters, exception);
            return context;
        }

        /// <inheritdoc />
        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
			Logger.LogDebug(">>> Entering InvokeActionResult(controllerContext={ControllerContext},actionResult={ActionResult})", controllerContext, actionResult);
            // Do not invoke the action.  Instead, store it for later retrieval
            if (this.ResultOfLastInvoke == null)
            {
                this.ResultOfLastInvoke = actionResult;
				Logger.LogDebug("InvokeActionResult(controllerContext={ControllerContext},actionResult={ActionResult}): this.ResultOfLastInvoke → {Arg2}", controllerContext, actionResult, this.ResultOfLastInvoke);
            }
			Logger.LogDebug("<<< Exiting InvokeActionResult(controllerContext={ControllerContext},actionResult={ActionResult})", controllerContext, actionResult);
        }
    }
}
