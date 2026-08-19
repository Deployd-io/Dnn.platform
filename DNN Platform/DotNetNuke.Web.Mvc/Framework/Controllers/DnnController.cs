// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.ActionResults;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Helpers;

    public abstract class DnnController : Controller, IDnnController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnController));
        /// <summary>Initializes a new instance of the <see cref="DnnController"/> class.</summary>
        protected DnnController()
        {
			Logger.LogDebug(">>> Entering DnnController()");
            this.ActionInvoker = new ResultCapturingActionInvoker();
			Logger.LogDebug("<<< Exiting DnnController()");
        }

        public ModuleInfo ActiveModule
        {
            get { return (this.ModuleContext == null) ? null : this.ModuleContext.Configuration; }
        }

        public TabInfo ActivePage
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.ActiveTab; }
        }

        public PortalSettings PortalSettings
        {
            get { return (this.ModuleContext == null) ? null : this.ModuleContext.PortalSettings; }
        }

        /// <inheritdoc />
        public ActionResult ResultOfLastExecute
        {
            get
            {
                var actionInvoker = this.ActionInvoker as ResultCapturingActionInvoker;
                return (actionInvoker != null) ? actionInvoker.ResultOfLastInvoke : null;
            }
        }

        public new UserInfo User
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.UserInfo; }
        }

        /// <inheritdoc />
        public Page DnnPage { get; set; }

        /// <inheritdoc />
        public new DnnUrlHelper Url { get; set; }

        /// <inheritdoc />
        public string LocalResourceFile { get; set; }

        /// <inheritdoc />
        public ModuleActionCollection ModuleActions { get; set; }

        /// <inheritdoc />
        public ModuleInstanceContext ModuleContext { get; set; }

        /// <inheritdoc />
        public ViewEngineCollection ViewEngineCollectionEx { get; set; }

        /// <inheritdoc />
        public string LocalizeString(string key)
        {
			Logger.LogDebug(">>> Entering LocalizeString(key={Key})", key);
			Logger.LogDebug("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, this.LocalResourceFile);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected internal RedirectToRouteResult RedirectToDefaultRoute()
        {
			Logger.LogDebug(">>> Entering RedirectToDefaultRoute()");
			Logger.LogDebug("<<< Exiting RedirectToDefaultRoute()");
            return new DnnRedirecttoRouteResult(string.Empty, string.Empty, string.Empty, null, false);
        }

        /// <inheritdoc />
        protected override RedirectToRouteResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
			Logger.LogDebug(">>> Entering RedirectToAction(actionName={ActionName},controllerName={ControllerName},routeValues={RouteValues})", actionName, controllerName, routeValues);
			Logger.LogDebug("<<< Exiting RedirectToAction(actionName={ActionName},controllerName={ControllerName},routeValues={RouteValues})", actionName, controllerName, routeValues);
            return new DnnRedirecttoRouteResult(actionName, controllerName, string.Empty, routeValues, false, this.Url);
        }

        /// <inheritdoc />
        protected override ViewResult View(IView view, object model)
        {
			Logger.LogDebug(">>> Entering View(view={View},model={Model})", view, model);
            if (model != null)
            {
                this.ViewData.Model = model;
            }

			Logger.LogDebug("<<< Exiting View(view={View},model={Model})", view, model);
            return new DnnViewResult
            {
                View = view,
                ViewData = this.ViewData,
                TempData = this.TempData,
			Logger.LogDebug("<<< Exiting View(viewName={ViewName},masterName={MasterName},model={Model})", viewName, masterName, model);
            };
        }

        /// <inheritdoc />
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            if (model != null)
            {
                this.ViewData.Model = model;
            }

            return new DnnViewResult
            {
                ViewName = viewName,
                MasterName = masterName,
                ViewData = this.ViewData,
                TempData = this.TempData,
                ViewEngineCollection = this.ViewEngineCollection,
            };
        }

        /// <inheritdoc />
        protected override PartialViewResult PartialView(string viewName, object model)
        {
			Logger.LogDebug(">>> Entering PartialView(viewName={ViewName},model={Model})", viewName, model);
            if (model != null)
            {
                this.ViewData.Model = model;
            }

			Logger.LogDebug("<<< Exiting PartialView(viewName={ViewName},model={Model})", viewName, model);
            return new DnnPartialViewResult
            {
                ViewName = viewName,
                ViewData = this.ViewData,
                TempData = this.TempData,
                ViewEngineCollection = this.ViewEngineCollection,
            };
        }

        /// <inheritdoc />
        protected override void Initialize(RequestContext requestContext)
        {
			Logger.LogDebug(">>> Entering Initialize(requestContext={RequestContext})", requestContext);
            base.Initialize(requestContext);
            this.Url = new DnnUrlHelper(requestContext, this);
			Logger.LogDebug("<<< Exiting Initialize(requestContext={RequestContext})", requestContext);
        }
    }
}
