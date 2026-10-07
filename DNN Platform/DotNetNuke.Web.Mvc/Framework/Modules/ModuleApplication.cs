// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Framework.Controllers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Web.Infrastructure.DynamicValidationHelper;

    public class ModuleApplication
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleApplication));
        internal static readonly string MvcVersion = GetMvcVersionString();
        protected const string ControllerMasterFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        protected const string SharedMasterFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        protected const string ControllerViewFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        protected const string SharedViewFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        protected const string ControllerPartialFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        protected const string SharedPartialFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        private const string MvcVersionHeaderName = "X-AspNetMvc-Version";
        private readonly object @lock = new object();

        private bool initialized;

        public ModuleApplication()
            : this(null, false)
        {
		Logger.LogError(">>> Entering ModuleApplication()");
        Logger.LogWarning("<<< Exiting ModuleApplication()");
        }

        public ModuleApplication(bool disableMvcResponseHeader)
            : this(null, disableMvcResponseHeader)
        {
		Logger.LogError(">>> Entering ModuleApplication(disableMvcResponseHeader={DisableMvcResponseHeader})", disableMvcResponseHeader);
        }

        public ModuleApplication(RequestContext requestContext, bool disableMvcResponseHeader)
        {
            this.RequestContext = requestContext;

			Logger.LogDebug("ModuleApplication(requestContext={RequestContext},disableMvcResponseHeader={DisableMvcResponseHeader}): this.RequestContext → {Arg2}", requestContext, disableMvcResponseHeader, this.RequestContext);
            DisableMvcResponseHeader = disableMvcResponseHeader;
			Logger.LogDebug("ModuleApplication(requestContext={RequestContext},disableMvcResponseHeader={DisableMvcResponseHeader}): DisableMvcResponseHeader → {DisableMvcResponseHeader1}", requestContext, disableMvcResponseHeader, DisableMvcResponseHeader);

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
			Logger.LogWarning("<<< Exiting ModuleApplication(disableMvcResponseHeader={DisableMvcResponseHeader})", disableMvcResponseHeader);
            this.ControllerFactory = Globals.GetCurrentServiceProvider().GetRequiredService<IControllerFactory>();
            this.ViewEngines = new ViewEngineCollection();

            ////ViewEngines.Add(new ModuleDelegatingViewEngine());
        }

        public RequestContext RequestContext { get; private set; }

        public virtual IControllerFactory ControllerFactory { get; set; }

        public string DefaultActionName { get; set; }

        public string DefaultControllerName { get; set; }

        public string[] DefaultNamespaces { get; set; }

        public string FolderPath { get; set; }

        public string ModuleName { get; set; }

        public ViewEngineCollection ViewEngines { get; set; }

        private static bool DisableMvcResponseHeader { get; set; }

        public virtual ModuleRequestResult ExecuteRequest(ModuleRequestContext context)
        {
			Logger.LogError(">>> Entering ExecuteRequest(context={Context})", context);
            this.EnsureInitialized();
            this.RequestContext = this.RequestContext ?? new RequestContext(context.HttpContext, context.RouteData);
            var currentContext = HttpContext.Current;
            if (currentContext != null)
            {
                var isRequestValidationEnabled = ValidationUtility.IsValidationEnabled(currentContext);
				Logger.LogError("ExecuteRequest(context={Context}): isRequestValidationEnabled → {IsRequestValidationEnabled}", context, isRequestValidationEnabled);
                if (isRequestValidationEnabled == true)
                {
                    ValidationUtility.EnableDynamicValidation(currentContext);
                }
            }

            this.AddVersionHeader(this.RequestContext.HttpContext);
            this.RemoveOptionalRoutingParameters();

            var controllerName = this.RequestContext.RouteData.GetRequiredString("controller");
			Logger.LogError("ExecuteRequest(context={Context}): controllerName → {ControllerName}", context, controllerName);

            // Construct the controller using the ControllerFactory
            var controller = this.ControllerFactory.CreateController(this.RequestContext, controllerName);
			Logger.LogError("ExecuteRequest(context={Context}): controller → {Controller}", context, controller);
            try
            {
                // Check if the controller supports IDnnController
                var moduleController = controller as IDnnController;

                // If we couldn't adapt it, we fail.  We can't support IController implementations directly :(
                // Because we need to retrieve the ActionResult without executing it, IController won't cut it
                if (moduleController == null)
                {
					Logger.LogError("Throwing InvalidOperationException from ExecuteRequest(context={Context})", context);
                    throw new InvalidOperationException("Could Not Construct Controller");
                }

                moduleController.ValidateRequest = false;
				Logger.LogDebug("ExecuteRequest(context={Context}): moduleController.ValidateRequest → {Arg1}", context, moduleController.ValidateRequest);

                moduleController.DnnPage = context.DnnPage;
				Logger.LogDebug("ExecuteRequest(context={Context}): moduleController.DnnPage → {Arg1}", context, moduleController.DnnPage);

                moduleController.ModuleContext = context.ModuleContext;
				Logger.LogDebug("ExecuteRequest(context={Context}): moduleController.ModuleContext → {Arg1}", context, moduleController.ModuleContext);

                moduleController.LocalResourceFile =
                    $"~/DesktopModules/MVC/{context.ModuleContext.Configuration.DesktopModule.FolderName}/{Localization.LocalResourceDirectory}/{controllerName}.resx";

                moduleController.ViewEngineCollectionEx = this.ViewEngines;
				Logger.LogDebug("ExecuteRequest(context={Context}): moduleController.ViewEngineCollectionEx → {Arg1}", context, moduleController.ViewEngineCollectionEx);

                // Execute the controller and capture the result
                // if our ActionFilter is executed after the ActionResult has triggered an Exception the filter
                // MUST explicitly flip the ExceptionHandled bit otherwise the view will not render
                moduleController.Execute(this.RequestContext);
                var result = moduleController.ResultOfLastExecute;
				Logger.LogError("ExecuteRequest(context={Context}): result → {Result}", context, result);

                // Return the final result
				Logger.LogWarning("<<< Exiting ExecuteRequest(context={Context})", context);
                return new ModuleRequestResult
                {
                    ActionResult = result,
                    ControllerContext = moduleController.ControllerContext,
                    ModuleActions = moduleController.ModuleActions,
                    ModuleContext = context.ModuleContext,
                    ModuleApplication = this,
                };
            }
            finally
            {
                this.ControllerFactory.ReleaseController(controller);
            }
        }

        protected internal virtual void Init()
        {
			Logger.LogError(">>> Entering Init()");
            var prefix = NormalizeFolderPath(this.FolderPath);
			Logger.LogError("Init(): prefix → {Prefix}", prefix);
            string[] masterFormats =
            {
                string.Format(CultureInfo.InvariantCulture, ControllerMasterFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, SharedMasterFormat, prefix),
            };
            string[] viewFormats =
            {
                string.Format(CultureInfo.InvariantCulture, ControllerViewFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, SharedViewFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, ControllerPartialFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, SharedPartialFormat, prefix),
            };

            this.ViewEngines.Add(new RazorViewEngine
            {
                MasterLocationFormats = masterFormats,
                ViewLocationFormats = viewFormats,
                PartialViewLocationFormats = viewFormats,
            });
			Logger.LogWarning("<<< Exiting Init()");
        }

        protected internal virtual void AddVersionHeader(HttpContextBase httpContext)
        {
			Logger.LogError(">>> Entering AddVersionHeader(httpContext={HttpContext})", httpContext);
            if (!DisableMvcResponseHeader)
            {
                httpContext.Response.AppendHeader(MvcVersionHeaderName, MvcVersion);
            }
			Logger.LogWarning("<<< Exiting AddVersionHeader(httpContext={HttpContext})", httpContext);
        }

        protected static string NormalizeFolderPath(string path)
        {
			Logger.LogError(">>> Entering NormalizeFolderPath(path={Path})", path);
            // Remove leading and trailing slashes
			Logger.LogWarning("<<< Exiting NormalizeFolderPath(path={Path})", path);
            return !string.IsNullOrEmpty(path) ? path.Trim('/') : path;
        }

        protected void EnsureInitialized()
        {
			Logger.LogError(">>> Entering EnsureInitialized()");
            // Double-check lock to wait for initialization
            // TODO: Is there a better (preferably using events and waits) way to do this?
            if (this.initialized)
            {
                return;
            }

            lock (this.@lock)
            {
                if (this.initialized)
                {
                    return;
                }

                this.Init();
                this.initialized = true;
				Logger.LogDebug("EnsureInitialized(): this.initialized → {Arg0}", this.initialized);
            }
			Logger.LogWarning("<<< Exiting EnsureInitialized()");
        }

        private static string GetMvcVersionString()
        {
			Logger.LogError(">>> Entering GetMvcVersionString()");
            // DevDiv 216459:
            // This code originally used Assembly.GetName(), but that requires FileIOPermission, which isn't granted in
            // medium trust. However, Assembly.FullName *is* accessible in medium trust.
			Logger.LogWarning("<<< Exiting GetMvcVersionString()");
            return new AssemblyName(typeof(MvcHandler).Assembly.FullName).Version.ToString(2);
        }

        private void RemoveOptionalRoutingParameters()
        {
			Logger.LogError(">>> Entering RemoveOptionalRoutingParameters()");
            var rvd = this.RequestContext.RouteData.Values;

            // Ensure delegate is stateless
            rvd.RemoveFromDictionary((entry) => entry.Value == UrlParameter.Optional);
			Logger.LogWarning("<<< Exiting RemoveOptionalRoutingParameters()");
        }
    }
}
