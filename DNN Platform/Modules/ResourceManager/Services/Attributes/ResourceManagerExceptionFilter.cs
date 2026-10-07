// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Attributes
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http.Filters;

    using Dnn.Modules.ResourceManager.Exceptions;

    /// <summary>A custom <see cref="ExceptionFilterAttribute"/> for the resource manager.</summary>
    public class ResourceManagerExceptionFilter : ExceptionFilterAttribute
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ResourceManagerExceptionFilter));
        /// <inheritdoc />
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
			Logger.LogError(">>> Entering OnException(actionExecutedContext={ActionExecutedContext})", actionExecutedContext);
            if (actionExecutedContext.Exception == null)
            {
                return;
            }

            var exception = actionExecutedContext.Exception;

            if (exception is FolderPermissionNotMetException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.Forbidden,
                    new { message = exception.Message });
                return;
            }

            if (exception is NotFoundException)
            {
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(
                    HttpStatusCode.NotFound,
                    new { message = exception.Message });
			Logger.LogError("Throwing exception from OnException(actionExecutedContext={ActionExecutedContext})", actionExecutedContext);
                return;
            }

			Logger.LogWarning("<<< Exiting OnException(actionExecutedContext={ActionExecutedContext})", actionExecutedContext);
            throw exception;
        }
    }
}
