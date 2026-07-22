// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>WebForms control for hosting an MVC module control.</summary>
    public class MvcHostControl : ModuleControlBase, IActionable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private ModuleRequestResult result;
        private string controlKey;

        /// <summary>Initializes a new instance of the <see cref="MvcHostControl"/> class.</summary>
        public MvcHostControl()
        {
			Logger.LogDebug(">>> Entering MvcHostControl()");
            this.controlKey = string.Empty;
			Logger.LogDebug("<<< Exiting MvcHostControl()");
			Logger.LogDebug("MvcHostControl(): this.controlKey → {Arg0}", this.controlKey);
        }

        /// <summary>Initializes a new instance of the <see cref="MvcHostControl"/> class.</summary>
        /// <param name="controlKey">The module control key.</param>
        public MvcHostControl(string controlKey)
        {
			Logger.LogDebug(">>> Entering MvcHostControl(controlKey={ControlKey})", controlKey);
            this.controlKey = controlKey;
			Logger.LogDebug("<<< Exiting MvcHostControl(controlKey={ControlKey})", controlKey);
			Logger.LogDebug("MvcHostControl(controlKey={ControlKey}): this.controlKey → {Arg1}", controlKey, this.controlKey);
        }

        /// <inheritdoc />
        public ModuleActionCollection ModuleActions { get; private set; }

        /// <summary>Gets or sets a value indicating whether the module controller should execute immediately (i.e. during <see cref="Control.OnInit"/> rather than <see cref="ISettingsControl.LoadSettings"/>).</summary>
        protected bool ExecuteModuleImmediately { get; set; } = true;

        /// <summary>Runs and renders the MVC action.</summary>
        protected void ExecuteModule()
        {
			Logger.LogDebug(">>> Entering ExecuteModule()");
            try
            {
                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

				Logger.LogDebug("ExecuteModule(): httpContext → {HttpContext}", httpContext);
                var moduleExecutionEngine = GetModuleExecutionEngine();

				Logger.LogDebug("ExecuteModule(): moduleExecutionEngine → {ModuleExecutionEngine}", moduleExecutionEngine);
                this.result = moduleExecutionEngine.ExecuteModule(this.GetModuleRequestContext(httpContext));
				Logger.LogDebug("ExecuteModule(): result → {Result}", result);

                this.ModuleActions = this.LoadActions(this.result);

                httpContext.SetModuleRequestResult(this.result);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in ExecuteModule(): {Arg0}", ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting ExecuteModule()");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (this.ExecuteModuleImmediately)
            {
                this.ExecuteModule();
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            try
            {
                if (this.result == null)
                {
                    return;
                }

                var mvcString = RenderModule(this.result);
                if (!string.IsNullOrEmpty(Convert.ToString(mvcString, CultureInfo.InvariantCulture)))
				Logger.LogDebug("OnPreRender(e={E}): mvcString → {MvcString}", e, mvcString);
                {
                    this.Controls.Add(new LiteralControl(Convert.ToString(mvcString, CultureInfo.InvariantCulture)));
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnPreRender(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private static ModuleApplication GetModuleApplication(
            IBusinessControllerProvider businessControllerProvider,
            DesktopModuleInfo desktopModule,
            RouteData defaultRouteData)
        {
            // Check if the MVC Module overrides the base ModuleApplication class.
            var moduleApplication = businessControllerProvider.GetInstance<ModuleApplication>(desktopModule);
            if (moduleApplication != null)
            {
                defaultRouteData.Values["controller"] = moduleApplication.DefaultControllerName;
                defaultRouteData.Values["action"] = moduleApplication.DefaultActionName;
                defaultRouteData.DataTokens["namespaces"] = moduleApplication.DefaultNamespaces;
                return moduleApplication;
            }

            var defaultControllerName = (string)defaultRouteData.Values["controller"];
            var defaultActionName = (string)defaultRouteData.Values["action"];
            var defaultNamespaces = (string[])defaultRouteData.DataTokens["namespaces"];

            return new ModuleApplication
            {
                DefaultActionName = defaultControllerName,
                DefaultControllerName = defaultActionName,
                DefaultNamespaces = defaultNamespaces,
                ModuleName = desktopModule.ModuleName,
                FolderPath = desktopModule.FolderName,
            };
        }

        private static IModuleExecutionEngine GetModuleExecutionEngine()
        {
			Logger.LogDebug(">>> Entering GetModuleExecutionEngine()");
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

			Logger.LogDebug("GetModuleExecutionEngine(): moduleExecutionEngine → {ModuleExecutionEngine}", moduleExecutionEngine);
            if (moduleExecutionEngine == null)
            {
                moduleExecutionEngine = new ModuleExecutionEngine();
                ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(moduleExecutionEngine);
            }

			Logger.LogDebug("<<< Exiting GetModuleExecutionEngine()");
            return moduleExecutionEngine;
        }

        private static MvcHtmlString RenderModule(ModuleRequestResult moduleResult)
        {
			Logger.LogDebug(">>> Entering RenderModule(moduleResult={ModuleResult})", moduleResult);
            using var writer = new StringWriter(CultureInfo.CurrentCulture);
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();
			Logger.LogDebug("RenderModule(moduleResult={ModuleResult}): writer → {Writer}", moduleResult, writer);
			Logger.LogDebug("RenderModule(moduleResult={ModuleResult}): moduleExecutionEngine → {ModuleExecutionEngine}", moduleResult, moduleExecutionEngine);

            moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);

			Logger.LogDebug("<<< Exiting RenderModule(moduleResult={ModuleResult})", moduleResult);
            return MvcHtmlString.Create(writer.ToString());
        }

        private ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext)
        {
            var module = this.ModuleContext.Configuration;

            // TODO DesktopModuleControllerAdapter usage is temporary in order to make method testable
            var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(module.DesktopModuleID, module.PortalID);
            var defaultControl = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(string.Empty, module.ModuleDefID);

            var defaultRouteData = ModuleRoutingProvider.Instance().GetRouteData(null, defaultControl);

            var moduleApplication = GetModuleApplication(
                httpContext.GetScope().ServiceProvider.GetRequiredService<IBusinessControllerProvider>(),
                desktopModule,
                defaultRouteData);

            RouteData routeData;

            var queryString = httpContext.Request.QueryString;

            if (string.IsNullOrEmpty(this.controlKey))
            {
                this.controlKey = queryString.GetValueOrDefault("ctl", string.Empty);
            }

            var moduleId = Null.NullInteger;
            if (queryString["moduleid"] != null)
            {
                if (!int.TryParse(queryString["moduleid"], out moduleId))
                {
                    moduleId = Null.NullInteger;
                }
            }

            if (moduleId != this.ModuleContext.ModuleId && string.IsNullOrEmpty(this.controlKey))
            {
                // Set default routeData for module that is not the "selected" module
                routeData = defaultRouteData;
            }
            else
            {
                var control = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(this.controlKey, module.ModuleDefID);
                routeData = ModuleRoutingProvider.Instance().GetRouteData(httpContext, control);
            }

            var moduleRequestContext = new ModuleRequestContext
            {
                DnnPage = this.Page,
                HttpContext = httpContext,
                ModuleContext = this.ModuleContext,
                ModuleApplication = moduleApplication,
                RouteData = routeData,
            };

            return moduleRequestContext;
        }

        private ModuleActionCollection LoadActions(ModuleRequestResult requestResult)
        {
			Logger.LogDebug(">>> Entering LoadActions(requestResult={RequestResult})", requestResult);
            var actions = new ModuleActionCollection();

			Logger.LogDebug("LoadActions(requestResult={RequestResult}): actions → {Actions}", requestResult, actions);
            if (requestResult.ModuleActions != null)
            {
                foreach (ModuleAction action in requestResult.ModuleActions)
                {
                    action.ID = this.ModuleContext.GetNextActionID();
                    actions.Add(action);
                }
            }

			Logger.LogDebug("<<< Exiting LoadActions(requestResult={RequestResult})", requestResult);
            return actions;
        }
    }
}
