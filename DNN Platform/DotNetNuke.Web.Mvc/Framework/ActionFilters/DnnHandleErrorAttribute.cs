// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class DnnHandleErrorAttribute : HandleErrorAttribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnHandleErrorAttribute));
        /// <inheritdoc />
        public override void OnException(ExceptionContext filterContext)
        {
			Logger.LogDebug(">>> Entering OnException(filterContext={FilterContext})", filterContext);
            var controller = filterContext.Controller as IDnnController;

            if (controller == null)
            {
				Logger.LogWarning("Throwing InvalidOperationException from OnException(filterContext={FilterContext})", filterContext);
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            this.LogException(filterContext.Exception);
			Logger.LogDebug("<<< Exiting OnException(filterContext={FilterContext})", filterContext);
        }

        protected virtual void LogException(Exception exception)
        {
			Logger.LogDebug(">>> Entering LogException(exception={Exception})", exception);
            Exceptions.LogException(exception);
			Logger.LogDebug("<<< Exiting LogException(exception={Exception})", exception);
        }
    }
}
