// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;

    /// <summary>A View Engine that will delegate to whatever ViewEngine(s) the module application defines.</summary>
    public class ModuleDelegatingViewEngine : IViewEngine
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleDelegatingViewEngine));
        private readonly IHostSettings hostSettings;
        private readonly Dictionary<IView, IViewEngine> viewEngineMappings = new Dictionary<IView, IViewEngine>();

        /// <summary>Initializes a new instance of the <see cref="ModuleDelegatingViewEngine"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModuleDelegatingViewEngine()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleDelegatingViewEngine"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public ModuleDelegatingViewEngine(IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering ModuleDelegatingViewEngine(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings;
			Logger.LogDebug("ModuleDelegatingViewEngine(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        Logger.LogWarning("<<< Exiting ModuleDelegatingViewEngine(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Finds the specified partial view by using the specified controller context.</summary>
        /// <returns>The partial view.</returns>
        /// <param name="controllerContext">The controller context.</param><param name="partialViewName">The name of the partial view.</param><param name="useCache">true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false.</param>
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
			Logger.LogError(">>> Entering FindPartialView(controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache})", controllerContext, partialViewName, useCache);
			Logger.LogWarning("<<< Exiting FindPartialView(controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache})", controllerContext, partialViewName, useCache);
            return this.RunAgainstModuleViewEngines(controllerContext, e => e.FindPartialView(this.hostSettings, controllerContext, partialViewName, useCache));
        }

        /// <summary>Finds the specified view by using the specified controller context.</summary>
        /// <returns>The page view.</returns>
        /// <param name="controllerContext">The controller context.</param><param name="viewName">The name of the view.</param><param name="masterName">The name of the master.</param><param name="useCache">true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false.</param>
        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
			Logger.LogError(">>> Entering FindView(controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache})", controllerContext, viewName, masterName, useCache);
			Logger.LogWarning("<<< Exiting FindView(controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache})", controllerContext, viewName, masterName, useCache);
            return this.RunAgainstModuleViewEngines(controllerContext, e => e.FindView(this.hostSettings, controllerContext, viewName, masterName, useCache));
        }

        /// <summary>Releases the specified view by using the specified controller context.</summary>
        /// <param name="controllerContext">The controller context.</param><param name="view">The view.</param>
        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
			Logger.LogError(">>> Entering ReleaseView(controllerContext={ControllerContext},view={View})", controllerContext, view);
            if (this.viewEngineMappings.TryGetValue(view, out var viewEngine))
            {
                viewEngine.ReleaseView(controllerContext, view);
            }
			Logger.LogWarning("<<< Exiting ReleaseView(controllerContext={ControllerContext},view={View})", controllerContext, view);
        }

        private static ModuleRequestResult GetCurrentModuleRequestResult(ControllerContext controllerContext)
        {
			Logger.LogError(">>> Entering GetCurrentModuleRequestResult(controllerContext={ControllerContext})", controllerContext);
            if (controllerContext.HttpContext.HasModuleRequestResult())
            {
                return controllerContext.HttpContext.GetModuleRequestResult();
            }

			Logger.LogWarning("<<< Exiting GetCurrentModuleRequestResult(controllerContext={ControllerContext})", controllerContext);
            return null;
        }

        private ViewEngineResult RunAgainstModuleViewEngines(ControllerContext controllerContext, Func<ViewEngineCollection, ViewEngineResult> engineRequest)
        {
			Logger.LogError(">>> Entering RunAgainstModuleViewEngines(controllerContext={ControllerContext},engineRequest={EngineRequest})", controllerContext, engineRequest);
            var controller = controllerContext.Controller as IDnnController;
            if (controller?.ViewEngineCollectionEx == null)
            {
                return new ViewEngineResult([]);
            }

            var result = engineRequest(controller.ViewEngineCollectionEx);
			Logger.LogError("RunAgainstModuleViewEngines(controllerContext={ControllerContext},engineRequest={EngineRequest}): result → {Result}", controllerContext, engineRequest, result);

            // If there is a view, store the view<->viewengine mapping so release works correctly
            if (result.View != null)
            {
                this.viewEngineMappings[result.View] = result.ViewEngine;
            }

			Logger.LogWarning("<<< Exiting RunAgainstModuleViewEngines(controllerContext={ControllerContext},engineRequest={EngineRequest})", controllerContext, engineRequest);
            return result;
        }
    }
}
