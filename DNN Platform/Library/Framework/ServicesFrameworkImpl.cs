// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;
    using System.Globalization;
    using System.Web.Helpers;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The default <see cref="IServicesFramework"/> implementation.</summary>
    internal class ServicesFrameworkImpl : IServicesFramework, IServiceFrameworkInternals
    {
        private const string AntiForgeryKey = "dnnAntiForgeryRequested";
        private const string ScriptKey = "dnnSFAjaxScriptRequested";
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="ServicesFrameworkImpl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo and IEventLogger. Scheduled removal in v12.0.0.")]
        public ServicesFrameworkImpl()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ServicesFrameworkImpl"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public ServicesFrameworkImpl(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
			eventLogger.LogError(">>> Entering ServicesFrameworkImpl(appStatus={AppStatus},eventLogger={EventLogger})", appStatus, eventLogger);
			eventLogger.LogDebug("ServicesFrameworkImpl(appStatus={AppStatus},eventLogger={EventLogger}): appStatus → {AppStatus1}", appStatus, eventLogger, appStatus);
			eventLogger.LogDebug("ServicesFrameworkImpl(appStatus={AppStatus},eventLogger={EventLogger}): eventLogger → {EventLogger1}", appStatus, eventLogger, eventLogger);
            var servicesProvider = Globals.GetCurrentServiceProvider();
			eventLogger.LogWarning("<<< Exiting ServicesFrameworkImpl(appStatus={AppStatus},eventLogger={EventLogger})", appStatus, eventLogger);
            this.appStatus = appStatus ?? servicesProvider.GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? servicesProvider.GetRequiredService<IEventLogger>();
        }

        /// <inheritdoc />
        public bool IsAjaxAntiForgerySupportRequired
        {
            get { return CheckKey(AntiForgeryKey); }
        }

        /// <inheritdoc />
        public bool IsAjaxScriptSupportRequired
        {
            get { return CheckKey(ScriptKey); }
        }

        /// <inheritdoc />
        public void RequestAjaxAntiForgerySupport()
        {
			eventLogger.LogError(">>> Entering RequestAjaxAntiForgerySupport()");
            this.RequestAjaxScriptSupport();
            SetKey(AntiForgeryKey);
			eventLogger.LogWarning("<<< Exiting RequestAjaxAntiForgerySupport()");
        }

        /// <inheritdoc />
        public void RegisterAjaxAntiForgery(Page page)
        {
			eventLogger.LogError(">>> Entering RegisterAjaxAntiForgery(page={Page})", page);
            var ctl = page.FindControl("ClientResourcesFormBottom");
			eventLogger.LogWarning("<<< Exiting RegisterAjaxAntiForgery(page={Page})", page);
			eventLogger.LogError("RegisterAjaxAntiForgery(page={Page}): ctl → {Ctl}", page, ctl);
            ctl?.Controls.Add(new LiteralControl(AntiForgery.GetHtml().ToHtmlString()));
        }

        /// <inheritdoc />
        public void RequestAjaxScriptSupport()
        {
			eventLogger.LogError(">>> Entering RequestAjaxScriptSupport()");
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.jQuery);
            SetKey(ScriptKey);
			eventLogger.LogWarning("<<< Exiting RequestAjaxScriptSupport()");
        }

        /// <inheritdoc />
        public void RegisterAjaxScript(Page page)
        {
			eventLogger.LogError(">>> Entering RegisterAjaxScript(page={Page})", page);
            var path = ServicesFramework.GetServiceFrameworkRoot();
			eventLogger.LogError("RegisterAjaxScript(page={Page}): path → {Path}", page, path);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            JavaScript.RegisterClientReference(page, ClientAPI.ClientNamespaceReferences.dnn);
            ClientAPI.RegisterClientVariable(page, "sf_siteRoot", path, /*overwrite*/ true);
            ClientAPI.RegisterClientVariable(page, "sf_tabId", PortalSettings.Current.ActiveTab.TabID.ToString(CultureInfo.InvariantCulture), /*overwrite*/ true);

            string scriptPath;
            if (HttpContextSource.Current.IsDebuggingEnabled)
            {
                scriptPath = "~/js/Debug/dnn.servicesframework.js";
				eventLogger.LogError("RegisterAjaxScript(page={Page}): scriptPath → {ScriptPath}", page, scriptPath);
            }
            else
            {
				eventLogger.LogDebug("RegisterAjaxScript(page={Page}): scriptPath → {ScriptPath}", page, scriptPath);
                scriptPath = "~/js/dnn.servicesframework.js";
            }

            GetClientResourcesController().RegisterScript(scriptPath);
			eventLogger.LogWarning("<<< Exiting RegisterAjaxScript(page={Page})", page);
        }

        private static void SetKey(string key)
        {
			eventLogger.LogError(">>> Entering SetKey(key={Key})", key);
            HttpContextSource.Current.Items[key] = true;
			eventLogger.LogWarning("<<< Exiting SetKey(key={Key})", key);
        }

        private static bool CheckKey(string antiForgeryKey)
        {
			eventLogger.LogError(">>> Entering CheckKey(antiForgeryKey={AntiForgeryKey})", antiForgeryKey);
            eventLogger.LogWarning("<<< Exiting CheckKey(antiForgeryKey={AntiForgeryKey})", antiForgeryKey);
            return HttpContextSource.Current.Items.Contains(antiForgeryKey);
        }

        private static IClientResourceController GetClientResourcesController()
        {
			eventLogger.LogError(">>> Entering GetClientResourcesController()");
            var serviceProvider = Globals.GetCurrentServiceProvider();
			eventLogger.LogError("GetClientResourcesController(): serviceProvider → {ServiceProvider}", serviceProvider);
			eventLogger.LogWarning("<<< Exiting GetClientResourcesController()");
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
