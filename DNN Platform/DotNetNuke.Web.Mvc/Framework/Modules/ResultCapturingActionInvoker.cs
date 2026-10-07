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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResultCapturingActionInvoker));
        public ActionResult ResultOfLastInvoke { get; set; }

        /// <inheritdoc />
        protected override ActionExecutedContext InvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
			Logger.LogError(">>> Entering InvokeActionMethodWithFilters(controllerContext={ControllerContext},filters={Filters},actionDescriptor={ActionDescriptor},parameters={Parameters})", controllerContext, filters, actionDescriptor, parameters);
            var context = base.InvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters);
			Logger.LogError("InvokeActionMethodWithFilters(controllerContext={ControllerContext},filters={Filters},actionDescriptor={ActionDescriptor},parameters={Parameters}): context → {Context}", controllerContext, filters, actionDescriptor, parameters, context);
            this.ResultOfLastInvoke = context.Result;
			Logger.LogDebug("InvokeActionMethodWithFilters(controllerContext={ControllerContext},filters={Filters},actionDescriptor={ActionDescriptor},parameters={Parameters}): this.ResultOfLastInvoke → {Arg4}", controllerContext, filters, actionDescriptor, parameters, this.ResultOfLastInvoke);
        Logger.LogWarning("<<< Exiting InvokeActionMethodWithFilters(controllerContext={ControllerContext},filters={Filters},actionDescriptor={ActionDescriptor},parameters={Parameters})", controllerContext, filters, actionDescriptor, parameters);
            return context;
        }

        /// <inheritdoc />
        protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
        {
			Logger.LogError(">>> Entering InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception})", controllerContext, filters, exception);
			Logger.LogDebug("InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception}): this.ResultOfLastInvoke → {Arg3}", controllerContext, filters, exception, this.ResultOfLastInvoke);
        Logger.LogWarning("<<< Exiting InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception})", controllerContext, filters, exception);
            var context = base.InvokeExceptionFilters(controllerContext, filters, exception);
			Logger.LogError("InvokeExceptionFilters(controllerContext={ControllerContext},filters={Filters},exception={Exception}): context → {Context}", controllerContext, filters, exception, context);
            this.ResultOfLastInvoke = context.Result;
            return context;
        }

        /// <inheritdoc />
        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
			Logger.LogError(">>> Entering InvokeActionResult(controllerContext={ControllerContext},actionResult={ActionResult})", controllerContext, actionResult);
            // Do not invoke the action.  Instead, store it for later retrieval
            if (this.ResultOfLastInvoke == null)
            {
				Logger.LogWarning("<<< Exiting InvokeActionResult(controllerContext={ControllerContext},actionResult={ActionResult})", controllerContext, actionResult);
                this.ResultOfLastInvoke = actionResult;
				Logger.LogDebug("InvokeActionResult(controllerContext={ControllerContext},actionResult={ActionResult}): this.ResultOfLastInvoke → {Arg2}", controllerContext, actionResult, this.ResultOfLastInvoke);
            }
        }
    }
}
