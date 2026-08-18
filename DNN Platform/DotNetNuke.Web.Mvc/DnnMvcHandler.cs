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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.HttpModules.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;

    using Microsoft.Extensions.DependencyInjection;

    public class DnnMvcHandler : IHttpHandler, IRequiresSessionState
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnMvcHandler));
        public static readonly string MvcVersionHeaderName = "X-AspNetMvc-Version";

        private ControllerBuilder controllerBuilder;

        public DnnMvcHandler(RequestContext requestContext)
        {
			Logger.LogDebug(">>> Entering DnnMvcHandler(requestContext={RequestContext})", requestContext);
            this.RequestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
			Logger.LogDebug("<<< Exiting DnnMvcHandler(requestContext={RequestContext})", requestContext);
        }

        public static bool DisableMvcResponseHeader { get; set; }

        public RequestContext RequestContext { get; private set; }

        /// <inheritdoc />
        bool IHttpHandler.IsReusable => this.IsReusable;

        internal ControllerBuilder ControllerBuilder
        {
            get => this.controllerBuilder ??= ControllerBuilder.Current;
            set => this.controllerBuilder = value;
        }

        protected virtual bool IsReusable => false;

        /// <inheritdoc />
        void IHttpHandler.ProcessRequest(HttpContext httpContext)
        {
            SetThreadCulture();
            MembershipModule.AuthenticateRequest(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>(),
                PortalController.Instance,
                UserRequestIPAddressController.Instance,
                RoleController.Instance,
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                this.RequestContext.HttpContext,
                allowUnknownExtensions: true);
            this.ProcessRequest(httpContext);
        }

        protected internal virtual void ProcessRequest(HttpContextBase httpContext)
        {
			Logger.LogDebug(">>> Entering ProcessRequest(httpContext={HttpContext})", httpContext);
            try
            {
                var moduleExecutionEngine = GetModuleExecutionEngine();
				Logger.LogDebug("ProcessRequest(httpContext={HttpContext}): moduleExecutionEngine → {ModuleExecutionEngine}", httpContext, moduleExecutionEngine);

                // Check if the controller supports IDnnController
                var moduleResult =
                    moduleExecutionEngine.ExecuteModule(this.GetModuleRequestContext(httpContext));
                httpContext.SetModuleRequestResult(moduleResult);
                this.RenderModule(moduleResult);
            }
            finally
            {
            }
			Logger.LogDebug("<<< Exiting ProcessRequest(httpContext={HttpContext})", httpContext);
        }

        protected virtual void ProcessRequest(HttpContext httpContext)
        {
            HttpContextBase httpContextBase = new HttpContextWrapper(httpContext);
            this.ProcessRequest(httpContextBase);
        }

        private static void SetThreadCulture()
        {
			Logger.LogDebug(">>> Entering SetThreadCulture()");
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            if (portalSettings is null)
			Logger.LogDebug("SetThreadCulture(): portalSettings → {PortalSettings}", portalSettings);
            {
                return;
            }

            var pageLocale = Localization.GetPageLocale(portalSettings);
			Logger.LogDebug("SetThreadCulture(): pageLocale → {PageLocale}", pageLocale);
            if (pageLocale is null)
            {
                return;
            }

            Localization.SetThreadCultures(pageLocale, portalSettings);
			Logger.LogDebug("<<< Exiting SetThreadCulture()");
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

        private ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext)
        {
			Logger.LogDebug(">>> Entering GetModuleRequestContext(httpContext={HttpContext})", httpContext);
            var moduleInfo = httpContext.Request.FindModuleInfo();
            var moduleContext = new ModuleInstanceContext() { Configuration = moduleInfo };
			Logger.LogDebug("GetModuleRequestContext(httpContext={HttpContext}): moduleContext → {ModuleContext}", httpContext, moduleContext);
            var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(moduleInfo.DesktopModuleID, moduleInfo.PortalID);
			Logger.LogDebug("GetModuleRequestContext(httpContext={HttpContext}): desktopModule → {DesktopModule}", httpContext, desktopModule);
			Logger.LogDebug("GetModuleRequestContext(httpContext={HttpContext}): moduleInfo → {ModuleInfo}", httpContext, moduleInfo);
            var moduleRequestContext = new ModuleRequestContext
            {
                HttpContext = httpContext,
                ModuleContext = moduleContext,
                ModuleApplication = new ModuleApplication(this.RequestContext, DisableMvcResponseHeader)
                {
                    ModuleName = desktopModule.ModuleName,
                    FolderPath = desktopModule.FolderName,
                },
            };

			Logger.LogDebug("<<< Exiting GetModuleRequestContext(httpContext={HttpContext})", httpContext);
            return moduleRequestContext;
        }

        private void RenderModule(ModuleRequestResult moduleResult)
        {
			Logger.LogDebug(">>> Entering RenderModule(moduleResult={ModuleResult})", moduleResult);
            var writer = this.RequestContext.HttpContext.Response.Output;

            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();
			Logger.LogDebug("RenderModule(moduleResult={ModuleResult}): moduleExecutionEngine → {ModuleExecutionEngine}", moduleResult, moduleExecutionEngine);

            moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);
			Logger.LogDebug("<<< Exiting RenderModule(moduleResult={ModuleResult})", moduleResult);
        }
    }
}
