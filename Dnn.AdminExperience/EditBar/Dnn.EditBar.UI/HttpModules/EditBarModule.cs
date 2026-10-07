// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.HttpModules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    using Dnn.EditBar.UI.Controllers;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Skins.EventListeners;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An HTTP module which injects the <see cref="ContentEditorManager"/> control on requests that may need to contain the Edit Bar.</summary>
    public class EditBarModule : IHttpModule
    {
        private static readonly object LockAppStarted = new object();
        private static bool hasAppStarted;

        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="EditBarModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public EditBarModule()
            : this(null)
        {
		eventLogger.LogError(">>> Entering EditBarModule()");
        }

        /// <summary>Initializes a new instance of the <see cref="EditBarModule"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public EditBarModule(IHostSettings hostSettings)
        {
            if (hostSettings is not null)
            {
				eventLogger.LogWarning("<<< Exiting EditBarModule()");
                this.hostSettings = hostSettings;
				eventLogger.LogDebug("EditBarModule(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
            }
            else
            {
				eventLogger.LogDebug("EditBarModule(hostSettings={HostSettings}): this.hostSettings → {Arg1}", hostSettings, this.hostSettings);
                var scope = HttpContextSource.Current?.GetScope();
				eventLogger.LogError("EditBarModule(hostSettings={HostSettings}): scope → {Scope}", hostSettings, scope);
                if (scope is not null)
                {
                    this.hostSettings = scope.ServiceProvider.GetRequiredService<IHostSettings>();
                }
                else
                {
                    this.hostSettings = new HostSettings(
                        new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                            new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                            new Lazy<IPortalController>(() => PortalController.Instance)));
                }
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
			eventLogger.LogError(">>> Entering Init(application={Application})", application);
            if (hasAppStarted)
            {
                return;
            }

            lock (LockAppStarted)
            {
                if (hasAppStarted)
                {
                    return;
                }

                this.ApplicationStart();
                hasAppStarted = true;
				eventLogger.LogDebug("Init(application={Application}): hasAppStarted → {HasAppStarted}", application, hasAppStarted);
            }
			eventLogger.LogWarning("<<< Exiting Init(application={Application})", application);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        private void ApplicationStart()
        {
			eventLogger.LogError(">>> Entering ApplicationStart()");
            DotNetNukeContext.Current.SkinEventListeners.Add(new SkinEventListener(SkinEventType.OnSkinInit, this.OnSkinInit));
			eventLogger.LogWarning("<<< Exiting ApplicationStart()");
        }

        private void OnSkinInit(object sender, SkinEventArgs e)
        {
			eventLogger.LogError(">>> Entering OnSkinInit(sender={Sender},e={E})", sender, e);
            if (this.hostSettings.DisableEditBar)
            {
                return;
            }

            var request = e.Skin.Page.Request;
            var isSpecialPageMode = request.QueryString["dnnprintmode"] == "true" || request.QueryString["popUp"] == "true";
			eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): isSpecialPageMode → {IsSpecialPageMode}", sender, e, isSpecialPageMode);
            if (isSpecialPageMode || Globals.IsAdminControl())
            {
                return;
            }

            if (ContentEditorManager.GetCurrent(e.Skin.Page) == null && !Globals.IsAdminControl())
            {
                if (PortalSettings.Current.UserId > 0)
                {
                    var scope = HttpContextSource.Current.GetScope();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): scope → {Scope}", sender, e, scope);
                    var clientResourceController = scope.ServiceProvider.GetRequiredService<IClientResourceController>();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): clientResourceController → {ClientResourceController}", sender, e, clientResourceController);
                    var appStatus = scope.ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): appStatus → {AppStatus}", sender, e, appStatus);
                    var eventLogger = scope.ServiceProvider.GetRequiredService<IEventLogger>();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): eventLogger → {EventLogger}", sender, e, eventLogger);
                    var portalController = scope.ServiceProvider.GetRequiredService<IPortalController>();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): portalController → {PortalController}", sender, e, portalController);
                    var userController = scope.ServiceProvider.GetRequiredService<IUserController>();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): userController → {UserController}", sender, e, userController);
                    var hostSettingsService = scope.ServiceProvider.GetRequiredService<IHostSettingsService>();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): hostSettingsService → {HostSettingsService}", sender, e, hostSettingsService);
                    var servicesFramework = scope.ServiceProvider.GetRequiredService<IServicesFramework>();
					eventLogger.LogError("OnSkinInit(sender={Sender},e={E}): servicesFramework → {ServicesFramework}", sender, e, servicesFramework);

                    e.Skin.Page.Form.Controls.Add(new ContentEditorManager(clientResourceController, appStatus, eventLogger, portalController, this.hostSettings, userController, hostSettingsService, servicesFramework) { Skin = e.Skin, });
                }
            }
			eventLogger.LogWarning("<<< Exiting OnSkinInit(sender={Sender},e={E})", sender, e);
        }
    }
}
