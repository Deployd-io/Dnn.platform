// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Routing;

    public class DnnUrlHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ViewContext viewContext;

        private readonly IDnnController controller;

        public DnnUrlHelper(ViewContext viewContext)
            : this(viewContext, RouteTable.Routes)
        {
		Logger.LogDebug(">>> Entering DnnUrlHelper(viewContext={ViewContext})", viewContext);
        }
		Logger.LogDebug("<<< Exiting DnnUrlHelper(viewContext={ViewContext})", viewContext);

        public DnnUrlHelper(RequestContext requestContext, IDnnController controller)
        {
			Logger.LogDebug(">>> Entering DnnUrlHelper(requestContext={RequestContext},controller={Controller})", requestContext, controller);
            Requires.NotNull("requestContext", requestContext);
            Requires.NotNull("controller", controller);

            this.UrlHelper = new UrlHelper(requestContext);
            this.controller = controller;
			Logger.LogDebug("DnnUrlHelper(requestContext={RequestContext},controller={Controller}): controller → {Controller1}", requestContext, controller, controller);
			Logger.LogDebug("DnnUrlHelper(requestContext={RequestContext},controller={Controller}): this.controller → {Arg2}", requestContext, controller, this.controller);
            this.ModuleContext = this.controller.ModuleContext;
			Logger.LogDebug("<<< Exiting DnnUrlHelper(requestContext={RequestContext},controller={Controller})", requestContext, controller);
			Logger.LogDebug("DnnUrlHelper(requestContext={RequestContext},controller={Controller}): this.ModuleContext → {Arg2}", requestContext, controller, this.ModuleContext);
        }

        public DnnUrlHelper(ViewContext viewContext, RouteCollection routeCollection)
        {
            Requires.NotNull("viewContext", viewContext);

            this.UrlHelper = new UrlHelper(viewContext.RequestContext, routeCollection);

            this.viewContext = viewContext;
			Logger.LogDebug("DnnUrlHelper(viewContext={ViewContext},routeCollection={RouteCollection}): viewContext → {ViewContext1}", viewContext, routeCollection, viewContext);
			Logger.LogDebug("DnnUrlHelper(viewContext={ViewContext},routeCollection={RouteCollection}): this.viewContext → {Arg2}", viewContext, routeCollection, this.viewContext);
			Logger.LogDebug("DnnUrlHelper(viewContext={ViewContext},routeCollection={RouteCollection}): controller → {Controller}", viewContext, routeCollection, controller);
			Logger.LogDebug("DnnUrlHelper(viewContext={ViewContext},routeCollection={RouteCollection}): this.controller → {Arg2}", viewContext, routeCollection, this.controller);

            this.controller = viewContext.Controller as IDnnController;

            if (this.controller == null)
            {
                throw new InvalidOperationException("The DnnUrlHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            this.ModuleContext = this.controller.ModuleContext;
			Logger.LogDebug("DnnUrlHelper(viewContext={ViewContext},routeCollection={RouteCollection}): this.ModuleContext → {Arg2}", viewContext, routeCollection, this.ModuleContext);
        }

        public ModuleInstanceContext ModuleContext { get; set; }

        internal UrlHelper UrlHelper { get; set; }

        public virtual string Encode(string url)
        {
			Logger.LogDebug(">>> Entering Encode(url={Url})", url);
			Logger.LogDebug("<<< Exiting Encode(url={Url})", url);
            return HttpUtility.UrlEncode(url);
        }

        /// <summary>Converts a virtual (relative) path to an application absolute path.</summary>
        ///
        /// <returns>
        /// The application absolute path.
        /// </returns>
        /// <param name="contentPath">The virtual path of the content.</param>
        public virtual string Content(string contentPath)
        {
			Logger.LogDebug(">>> Entering Content(contentPath={ContentPath})", contentPath);
			Logger.LogDebug("<<< Exiting Content(contentPath={ContentPath})", contentPath);
            return this.UrlHelper.Content(contentPath);
        }

        /// <summary>Returns a value that indicates whether the URL is local.</summary>
        ///
        /// <returns>
        /// true if the URL is local; otherwise, false.
        /// </returns>
        /// <param name="url">The URL.</param>
        public virtual bool IsLocalUrl(string url)
        {
			Logger.LogDebug(">>> Entering IsLocalUrl(url={Url})", url);
			Logger.LogDebug("<<< Exiting IsLocalUrl(url={Url})", url);
            return this.UrlHelper.IsLocalUrl(url);
        }

        public virtual string Action()
        {
			Logger.LogDebug(">>> Entering Action()");
			Logger.LogDebug("<<< Exiting Action()");
            return this.UrlHelper.RequestContext.HttpContext.Request.RawUrl;
        }

        public virtual string Action(string actionName)
        {
			Logger.LogDebug("<<< Exiting Action(actionName={ActionName})", actionName);
            return this.GenerateUrl(actionName, null, new RouteValueDictionary());
        }

        public virtual string Action(string actionName, RouteValueDictionary routeValues)
        {
			Logger.LogDebug("<<< Exiting Action(actionName={ActionName},routeValues={RouteValues})", actionName, routeValues);
            return this.GenerateUrl(actionName, null, routeValues);
        }

        public virtual string Action(string actionName, object routeValues)
        {
            return this.GenerateUrl(actionName, null, TypeHelper.ObjectToDictionary(routeValues));
        }

        public virtual string Action(string actionName, string controllerName)
        {
			Logger.LogDebug("<<< Exiting Action(actionName={ActionName},controllerName={ControllerName})", actionName, controllerName);
            return this.GenerateUrl(actionName, controllerName, new RouteValueDictionary());
        }

        public virtual string Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
			Logger.LogDebug("<<< Exiting Action(actionName={ActionName},controllerName={ControllerName},routeValues={RouteValues})", actionName, controllerName, routeValues);
            return this.GenerateUrl(actionName, controllerName, routeValues);
        }

        public virtual string Action(string actionName, string controllerName, object routeValues)
        {
            return this.GenerateUrl(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues));
        }

        private string GenerateUrl(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
			Logger.LogDebug(">>> Entering GenerateUrl(actionName={ActionName},controllerName={ControllerName},routeValues={RouteValues})", actionName, controllerName, routeValues);
            routeValues["controller"] = controllerName ?? this.controller.ControllerContext?.RouteData.Values["controller"];
            routeValues["action"] = actionName;
			Logger.LogDebug("<<< Exiting GenerateUrl(actionName={ActionName},controllerName={ControllerName},routeValues={RouteValues})", actionName, controllerName, routeValues);
            return ModuleRoutingProvider.Instance().GenerateUrl(routeValues, this.ModuleContext);
        }
    }
}
