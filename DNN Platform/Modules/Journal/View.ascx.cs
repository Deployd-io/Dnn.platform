// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Journal
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Journal.Components;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The ViewJournal class displays the content.</summary>
    public partial class View : JournalModuleBase
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int PageSize = 20;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool AllowPhotos = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool AllowFiles = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int MaxMessageLength = 250;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool CanRender = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool ShowEditor = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool CanComment = true;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool IsGroup;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string BaseUrl;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public string ProfilePage;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int Gid = -1;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public int Pid = -1;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public long MaxUploadSize;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        public bool IsPublicGroup;

        private readonly INavigationManager navigationManager;
        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IServicesFramework servicesFramework;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public View()
            : this(null, null, null, null, null, null)
        {
		eventLogger.LogDebug(">>> Entering View()");
        }
		eventLogger.LogDebug("<<< Exiting View()");

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public View(INavigationManager navigationManager, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IServicesFramework servicesFramework)
            : this(navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, null)
        {
		eventLogger.LogDebug(">>> Entering View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework})", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework);
		eventLogger.LogDebug("<<< Exiting View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework})", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework);
		eventLogger.LogDebug("View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework},hostSettings={HostSettings}): clientResourceController → {ClientResourceController1}", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, hostSettings, clientResourceController);
		eventLogger.LogDebug("View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework},hostSettings={HostSettings}): appStatus → {AppStatus1}", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, hostSettings, appStatus);
		eventLogger.LogDebug("View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework},hostSettings={HostSettings}): eventLogger → {EventLogger1}", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, hostSettings, eventLogger);
		eventLogger.LogDebug("View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework},hostSettings={HostSettings}): servicesFramework → {ServicesFramework1}", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, hostSettings, servicesFramework);
		eventLogger.LogDebug("View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework},hostSettings={HostSettings}): hostSettings → {HostSettings1}", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, hostSettings, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="View"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        /// <param name="hostSettings">The host settings.</param>
        public View(INavigationManager navigationManager, IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, IServicesFramework servicesFramework, IHostSettings hostSettings)
        {
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.clientResourceController = clientResourceController ?? this.DependencyProvider.GetRequiredService<IClientResourceController>();
			eventLogger.LogDebug("View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework},hostSettings={HostSettings}): navigationManager → {NavigationManager1}", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, hostSettings, navigationManager);
            this.appStatus = appStatus ?? this.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.servicesFramework = servicesFramework ?? this.DependencyProvider.GetRequiredService<IServicesFramework>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
            this.MaxUploadSize = Config.GetMaxUploadSize(this.appStatus);
			eventLogger.LogDebug("View(navigationManager={NavigationManager},clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger},servicesFramework={ServicesFramework},hostSettings={HostSettings}): MaxUploadSize → {MaxUploadSize}", navigationManager, clientResourceController, appStatus, eventLogger, servicesFramework, hostSettings, MaxUploadSize);
        }

        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.jQueryFileUpload);
            this.servicesFramework.RequestAjaxAntiForgerySupport();
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.Knockout);

            this.clientResourceController.RegisterScript("~/DesktopModules/Journal/Scripts/journal.js");
            this.clientResourceController.RegisterScript("~/DesktopModules/Journal/Scripts/journalcomments.js");
            this.clientResourceController.RegisterScript("~/DesktopModules/Journal/Scripts/mentionsInput.js");

            var isAdmin = this.UserInfo.IsInRole(RoleController.Instance.GetRoleById(this.PortalId, this.PortalSettings.AdministratorRoleId).RoleName);
            if (!this.Request.IsAuthenticated || (!this.UserInfo.IsSuperUser && !isAdmin && this.UserInfo.IsInRole("Unverified Users")))
			eventLogger.LogDebug("OnInit(e={E}): isAdmin → {IsAdmin}", e, isAdmin);
            {
                this.ShowEditor = false;
				eventLogger.LogDebug("OnInit(e={E}): this.ShowEditor → {Arg1}", e, this.ShowEditor);
            }
            else
            {
                this.ShowEditor = this.EditorEnabled;
            }

            if (this.Settings.ContainsKey(Constants.DefaultPageSize))
            {
                this.PageSize = Convert.ToInt16(this.Settings[Constants.DefaultPageSize]);
				eventLogger.LogDebug("OnInit(e={E}): PageSize → {PageSize}", e, PageSize);
            }

            if (this.Settings.ContainsKey(Constants.MaxCharacters))
            {
                this.MaxMessageLength = Convert.ToInt16(this.Settings[Constants.MaxCharacters]);
				eventLogger.LogDebug("OnInit(e={E}): MaxMessageLength → {MaxMessageLength}", e, MaxMessageLength);
            }

            if (this.Settings.ContainsKey(Constants.AllowPhotos))
            {
                this.AllowPhotos = Convert.ToBoolean(this.Settings[Constants.AllowPhotos]);
            }

            if (this.Settings.ContainsKey(Constants.AllowFiles))
            {
                this.AllowFiles = Convert.ToBoolean(this.Settings[Constants.AllowFiles]);
            }

            this.ctlJournalList.Enabled = true;
            this.ctlJournalList.ProfileId = -1;
            this.ctlJournalList.PageSize = this.PageSize;
            this.ctlJournalList.ModuleId = this.ModuleId;

            ModuleInfo moduleInfo = this.ModuleContext.Configuration;

            foreach (var module in ModuleController.Instance.GetTabModules(this.TabId).Values)
            {
                if (module.ModuleDefinition.FriendlyName == "Social Groups")
                {
                    if (this.GroupId == -1 && this.FilterMode == JournalMode.Auto)
                    {
                        this.ShowEditor = false;
                        this.ctlJournalList.Enabled = false;
                    }

                    if (this.GroupId > 0)
                    {
                        RoleInfo roleInfo = RoleController.Instance.GetRoleById(moduleInfo.OwnerPortalID, this.GroupId);
                        if (roleInfo != null)
						eventLogger.LogDebug("OnInit(e={E}): roleInfo → {RoleInfo}", e, roleInfo);
                        {
                            if (this.UserInfo.IsInRole(roleInfo.RoleName))
                            {
                                this.ShowEditor = true;
                                this.CanComment = true;
                                this.IsGroup = true;
								eventLogger.LogDebug("OnInit(e={E}): this.CanComment → {Arg1}", e, this.CanComment);
								eventLogger.LogDebug("OnInit(e={E}): this.IsGroup → {Arg1}", e, this.IsGroup);
                            }
                            else
                            {
                                this.ShowEditor = false;
                                this.CanComment = false;
                            }

                            if (!roleInfo.IsPublic && !this.ShowEditor)
                            {
                                this.ctlJournalList.Enabled = false;
                            }

                            if (roleInfo.IsPublic && !this.ShowEditor)
                            {
                                this.ctlJournalList.Enabled = true;
                            }

                            if (roleInfo.IsPublic && this.ShowEditor)
                            {
                                this.ctlJournalList.Enabled = true;
                            }

                            if (roleInfo.IsPublic)
                            {
                                this.IsPublicGroup = true;
								eventLogger.LogDebug("OnInit(e={E}): this.IsPublicGroup → {Arg1}", e, this.IsPublicGroup);
                            }
                        }
                        else
                        {
                            this.ShowEditor = false;
                            this.ctlJournalList.Enabled = false;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.Request.QueryString["userId"]))
            {
                this.ctlJournalList.ProfileId = Convert.ToInt32(this.Request.QueryString["userId"]);
                if (!this.UserInfo.IsSuperUser && !isAdmin && this.ctlJournalList.ProfileId != this.UserId)
                {
                    this.ShowEditor = this.ShowEditor && Utilities.AreFriends(UserController.GetUserById(this.hostSettings, this.PortalId, this.ctlJournalList.ProfileId), this.UserInfo);
                }
            }
            else if (this.GroupId > 0)
            {
                this.ctlJournalList.SocialGroupId = Convert.ToInt32(this.Request.QueryString["groupId"]);
            }

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
                this.BaseUrl = Globals.ApplicationPath;
                this.BaseUrl = this.BaseUrl.EndsWith("/", StringComparison.Ordinal) ? this.BaseUrl : this.BaseUrl + "/";
				eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): this.BaseUrl → {Arg2}", sender, e, this.BaseUrl);
                this.BaseUrl += "DesktopModules/Journal/";

                this.ProfilePage = this.navigationManager.NavigateURL(this.PortalSettings.UserTabId, string.Empty, new[] { "userId=xxx" });

                if (!string.IsNullOrEmpty(this.Request.QueryString["userId"]))
                {
                    this.Pid = Convert.ToInt32(this.Request.QueryString["userId"]);
					eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): Pid → {Pid}", sender, e, Pid);
                    this.ctlJournalList.ProfileId = this.Pid;
                }
                else if (this.GroupId > 0)
                {
                    this.Gid = this.GroupId;
					eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): Gid → {Gid}", sender, e, Gid);
					eventLogger.LogDebug("Page_Load(sender={Sender},e={E}): this.Gid → {Arg2}", sender, e, this.Gid);
                    this.ctlJournalList.SocialGroupId = this.GroupId;
                }

                this.ctlJournalList.PageSize = this.PageSize;
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
