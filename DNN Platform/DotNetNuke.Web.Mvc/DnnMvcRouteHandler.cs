// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.SessionState;

    public class DnnMvcRouteHandler : IRouteHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnMvcRouteHandler));
        private readonly IControllerFactory controllerFactory;

        /// <summary>Initializes a new instance of the <see cref="DnnMvcRouteHandler"/> class.</summary>
        public DnnMvcRouteHandler()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnMvcRouteHandler"/> class.</summary>
        /// <param name="controllerFactory">The controller factory.</param>
        public DnnMvcRouteHandler(IControllerFactory controllerFactory)
        {
			Logger.LogError(">>> Entering DnnMvcRouteHandler(controllerFactory={ControllerFactory})", controllerFactory);
            this.controllerFactory = controllerFactory;
			Logger.LogDebug("DnnMvcRouteHandler(controllerFactory={ControllerFactory}): controllerFactory → {ControllerFactory1}", controllerFactory, controllerFactory);
        Logger.LogWarning("<<< Exiting DnnMvcRouteHandler(controllerFactory={ControllerFactory})", controllerFactory);
        }

        /// <inheritdoc />
        IHttpHandler IRouteHandler.GetHttpHandler(RequestContext requestContext)
        {
			Logger.LogError(">>> Entering GetHttpHandler(requestContext={RequestContext})", requestContext);
			Logger.LogWarning("<<< Exiting GetHttpHandler(requestContext={RequestContext})", requestContext);
            return this.GetHttpHandler(requestContext);
        }

        protected virtual IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            requestContext.HttpContext.SetSessionStateBehavior(this.GetSessionStateBehavior(requestContext));
            return new DnnMvcHandler(requestContext);
        }

        protected virtual SessionStateBehavior GetSessionStateBehavior(RequestContext requestContext)
        {
			Logger.LogError(">>> Entering GetSessionStateBehavior(requestContext={RequestContext})", requestContext);
            string controllerName = (string)requestContext.RouteData.Values["controller"];
            if (string.IsNullOrWhiteSpace(controllerName))
            {
				Logger.LogError("Throwing InvalidOperationException from GetSessionStateBehavior(requestContext={RequestContext})", requestContext);
                throw new InvalidOperationException("No Controller");
            }

            IControllerFactory controllerFactory = this.controllerFactory ?? ControllerBuilder.Current.GetControllerFactory();
			Logger.LogError("GetSessionStateBehavior(requestContext={RequestContext}): controllerFactory → {ControllerFactory}", requestContext, controllerFactory);
			Logger.LogWarning("<<< Exiting GetSessionStateBehavior(requestContext={RequestContext})", requestContext);
            return controllerFactory.GetControllerSessionBehavior(requestContext, controllerName);
        }
    }
}
