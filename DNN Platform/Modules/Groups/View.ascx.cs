// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ViewSocialGroups class displays the content.</summary>
    public partial class View : GroupsModuleBase
    {
        private readonly INavigationManager navigationManager;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public View()
            : this(null, null, null)
        {
		eventLogger.LogDebug(">>> Entering View()");
        }
		eventLogger.LogDebug("<<< Exiting View()");

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public View(INavigationManager navigationManager, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.appStatus = appStatus ?? this.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
			eventLogger.LogDebug("View(navigationManager={NavigationManager},appStatus={AppStatus},eventLogger={EventLogger}): appStatus → {AppStatus1}", navigationManager, appStatus, eventLogger, appStatus);
			eventLogger.LogDebug("View(navigationManager={NavigationManager},appStatus={AppStatus},eventLogger={EventLogger}): navigationManager → {NavigationManager1}", navigationManager, appStatus, eventLogger, navigationManager);
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
			eventLogger.LogDebug("View(navigationManager={NavigationManager},appStatus={AppStatus},eventLogger={EventLogger}): eventLogger → {EventLogger1}", navigationManager, appStatus, eventLogger, eventLogger);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            this.InitializeComponent();
            base.OnInit(e);
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void InitializeComponent()
        {
			eventLogger.LogDebug(">>> Entering InitializeComponent()");
            this.Load += this.Page_Load;
			eventLogger.LogDebug("<<< Exiting InitializeComponent()");
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        private void Page_Load(object sender, EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering Page_Load(sender={Sender},e={E})", sender, e);
            try
            {
                JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);
                if (this.GroupId < 0)
                {
                    if (this.TabId != this.GroupListTabId && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                    {
                        this.Response.Redirect(this.navigationManager.NavigateURL(this.GroupListTabId));
                    }
                }

                GroupsModuleBase ctl = (GroupsModuleBase)this.LoadControl(this.ControlPath);
                ctl.ModuleConfiguration = this.ModuleConfiguration;
				eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): ctl → {Ctl}", sender, e, ctl);
				eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): ctl.ModuleConfiguration → {Arg2}", sender, e, ctl.ModuleConfiguration);
                this.plhContent.Controls.Clear();
                this.plhContent.Controls.Add(ctl);
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in Page_Load(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogDebug("<<< Exiting Page_Load(sender={Sender},e={E})", sender, e);
        }
    }
}
