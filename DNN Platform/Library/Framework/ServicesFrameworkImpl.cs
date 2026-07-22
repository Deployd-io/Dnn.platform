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
		eventLogger.LogDebug(">>> Entering ServicesFrameworkImpl()");
        }
		eventLogger.LogDebug("<<< Exiting ServicesFrameworkImpl()");

        /// <summary>Initializes a new instance of the <see cref="ServicesFrameworkImpl"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public ServicesFrameworkImpl(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            var servicesProvider = Globals.GetCurrentServiceProvider();
            this.appStatus = appStatus ?? servicesProvider.GetRequiredService<IApplicationStatusInfo>();
			eventLogger.LogDebug("ServicesFrameworkImpl(appStatus={AppStatus},eventLogger={EventLogger}): servicesProvider → {ServicesProvider}", appStatus, eventLogger, servicesProvider);
			eventLogger.LogDebug("ServicesFrameworkImpl(appStatus={AppStatus},eventLogger={EventLogger}): appStatus → {AppStatus1}", appStatus, eventLogger, appStatus);
            this.eventLogger = eventLogger ?? servicesProvider.GetRequiredService<IEventLogger>();
			eventLogger.LogDebug("ServicesFrameworkImpl(appStatus={AppStatus},eventLogger={EventLogger}): eventLogger → {EventLogger1}", appStatus, eventLogger, eventLogger);
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
			eventLogger.LogDebug(">>> Entering RequestAjaxAntiForgerySupport()");
            this.RequestAjaxScriptSupport();
            SetKey(AntiForgeryKey);
			eventLogger.LogDebug("<<< Exiting RequestAjaxAntiForgerySupport()");
        }

        /// <inheritdoc />
        public void RegisterAjaxAntiForgery(Page page)
        {
			eventLogger.LogDebug(">>> Entering RegisterAjaxAntiForgery(page={Page})", page);
            var ctl = page.FindControl("ClientResourcesFormBottom");
            ctl?.Controls.Add(new LiteralControl(AntiForgery.GetHtml().ToHtmlString()));
			eventLogger.LogDebug("<<< Exiting RegisterAjaxAntiForgery(page={Page})", page);
			eventLogger.LogDebug("RegisterAjaxAntiForgery(page={Page}): ctl → {Ctl}", page, ctl);
        }

        /// <inheritdoc />
        public void RequestAjaxScriptSupport()
        {
			eventLogger.LogDebug(">>> Entering RequestAjaxScriptSupport()");
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.jQuery);
            SetKey(ScriptKey);
			eventLogger.LogDebug("<<< Exiting RequestAjaxScriptSupport()");
        }

        /// <inheritdoc />
        public void RegisterAjaxScript(Page page)
        {
			eventLogger.LogDebug(">>> Entering RegisterAjaxScript(page={Page})", page);
            var path = ServicesFramework.GetServiceFrameworkRoot();
            if (string.IsNullOrEmpty(path))
			eventLogger.LogDebug("RegisterAjaxScript(page={Page}): path → {Path}", page, path);
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
				eventLogger.LogDebug("RegisterAjaxScript(page={Page}): scriptPath → {ScriptPath}", page, scriptPath);
            }
            else
            {
                scriptPath = "~/js/dnn.servicesframework.js";
            }

            GetClientResourcesController().RegisterScript(scriptPath);
			eventLogger.LogDebug("<<< Exiting RegisterAjaxScript(page={Page})", page);
        }

        private static void SetKey(string key)
        {
			eventLogger.LogDebug(">>> Entering SetKey(key={Key})", key);
            HttpContextSource.Current.Items[key] = true;
			eventLogger.LogDebug("<<< Exiting SetKey(key={Key})", key);
        }

        private static bool CheckKey(string antiForgeryKey)
        {
			eventLogger.LogDebug(">>> Entering CheckKey(antiForgeryKey={AntiForgeryKey})", antiForgeryKey);
			eventLogger.LogDebug("<<< Exiting CheckKey(antiForgeryKey={AntiForgeryKey})", antiForgeryKey);
            return HttpContextSource.Current.Items.Contains(antiForgeryKey);
        }

        private static IClientResourceController GetClientResourcesController()
        {
			eventLogger.LogDebug(">>> Entering GetClientResourcesController()");
            var serviceProvider = Globals.GetCurrentServiceProvider();
			eventLogger.LogDebug("GetClientResourcesController(): serviceProvider → {ServiceProvider}", serviceProvider);
			eventLogger.LogDebug("<<< Exiting GetClientResourcesController()");
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
