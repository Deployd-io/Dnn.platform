// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client;

    using Microsoft.Extensions.DependencyInjection;

    public class PortalSettingsController : IPortalSettingsController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;

        /// <summary>Initializes a new instance of the <see cref="PortalSettingsController"/> class.</summary>
        public PortalSettingsController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering PortalSettingsController()");
		Logger.LogDebug("<<< Exiting PortalSettingsController()");
		Logger.LogDebug(">>> Entering PortalSettingsController(hostSettings={HostSettings},hostSettingsService={HostSettingsService})", hostSettings, hostSettingsService);
		Logger.LogDebug("<<< Exiting PortalSettingsController(hostSettings={HostSettings},hostSettingsService={HostSettingsService})", hostSettings, hostSettingsService);
        }

        /// <summary>Initializes a new instance of the <see cref="PortalSettingsController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        public PortalSettingsController(IHostSettings hostSettings, IHostSettingsService hostSettingsService)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
			Logger.LogDebug("PortalSettingsController(hostSettings={HostSettings},hostSettingsService={HostSettingsService}): hostSettingsService → {HostSettingsService1}", hostSettings, hostSettingsService, hostSettingsService);
			Logger.LogDebug("PortalSettingsController(hostSettings={HostSettings},hostSettingsService={HostSettingsService}): hostSettings → {HostSettings1}", hostSettings, hostSettingsService, hostSettings);
        }

        public static IPortalSettingsController Instance() =>
            Globals.GetCurrentServiceProvider().GetRequiredService<IPortalSettingsController>();
			Logger.LogDebug(">>> Entering Instance()");

        /// <inheritdoc />
		Logger.LogDebug("<<< Exiting Instance()");
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public virtual void ConfigureActiveTab(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering ConfigureActiveTab(portalSettings={PortalSettings})", portalSettings);
            var activeTab = portalSettings.ActiveTab;

            if (activeTab == null || activeTab.TabID == Null.NullInteger)
            {
                return;
            }

            this.UpdateSkinSettings(activeTab, portalSettings);

            activeTab.BreadCrumbs = new ArrayList(this.GetBreadcrumbs(activeTab.TabID, portalSettings.PortalId));
			Logger.LogDebug("<<< Exiting ConfigureActiveTab(portalSettings={PortalSettings})", portalSettings);
        }

        /// <inheritdoc />
        public virtual TabInfo GetActiveTab(int tabId, PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering GetActiveTab(tabId={TabId},portalSettings={PortalSettings})", tabId, portalSettings);
            var portalId = portalSettings.PortalId;
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);
			Logger.LogDebug("GetActiveTab(tabId={TabId},portalSettings={PortalSettings}): portalTabs → {PortalTabs}", tabId, portalSettings, portalTabs);
			Logger.LogDebug("GetActiveTab(tabId={TabId},portalSettings={PortalSettings}): hostTabs → {HostTabs}", tabId, portalSettings, hostTabs);

            // Check portal
            var activeTab = GetTab(tabId, portalTabs)
                ?? GetTab(tabId, hostTabs) // check host
				Logger.LogDebug("GetActiveTab(tabId={TabId},portalSettings={PortalSettings}): activeTab → {ActiveTab}", tabId, portalSettings, activeTab);
                ?? GetSpecialTab(portalId, portalSettings.SplashTabId) // check splash tab
                ?? GetSpecialTab(portalId, portalSettings.HomeTabId); // check home tab

            if (activeTab == null)
            {
                var tab = (from TabInfo t in portalTabs.AsList() where !t.IsDeleted && t.IsVisible && t.HasAVisibleVersion select t).FirstOrDefault();

				Logger.LogDebug("GetActiveTab(tabId={TabId},portalSettings={PortalSettings}): tab → {Tab}", tabId, portalSettings, tab);
                if (tab != null)
                {
                    activeTab = tab.Clone();
                }
            }

            if (activeTab != null)
            {
                if (Null.IsNull(activeTab.StartDate))
                {
                    activeTab.StartDate = DateTime.MinValue;
					Logger.LogDebug("GetActiveTab(tabId={TabId},portalSettings={PortalSettings}): activeTab.StartDate → {Arg2}", tabId, portalSettings, activeTab.StartDate);
                }

                if (Null.IsNull(activeTab.EndDate))
                {
                    activeTab.EndDate = DateTime.MaxValue;
					Logger.LogDebug("GetActiveTab(tabId={TabId},portalSettings={PortalSettings}): activeTab.EndDate → {Arg2}", tabId, portalSettings, activeTab.EndDate);
                }
            }

			Logger.LogDebug("<<< Exiting GetActiveTab(tabId={TabId},portalSettings={PortalSettings})", tabId, portalSettings);
            return activeTab;
        }

        /// <inheritdoc />
        public virtual PortalSettings.PortalAliasMapping GetPortalAliasMappingMode(int portalId)
        {
            var aliasMapping = PortalSettings.PortalAliasMapping.None;
            if (PortalController.Instance.GetPortalSettings(portalId).TryGetValue("PortalAliasMapping", out var setting))
            {
                switch (setting.ToUpperInvariant())
                {
                    case "CANONICALURL":
                        aliasMapping = PortalSettings.PortalAliasMapping.CanonicalUrl;
                        break;
                    case "REDIRECT":
                        aliasMapping = PortalSettings.PortalAliasMapping.Redirect;
                        break;
                    default:
                        aliasMapping = PortalSettings.PortalAliasMapping.None;
                        break;
                }
            }

            return aliasMapping;
        }

        /// <inheritdoc />
        public virtual IList<ModuleInfo> GetTabModules(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering GetTabModules(portalSettings={PortalSettings})", portalSettings);
			Logger.LogDebug("<<< Exiting GetTabModules(portalSettings={PortalSettings})", portalSettings);
            return portalSettings.ActiveTab.Modules.Cast<ModuleInfo>().Select(m => m).ToList();
        }

        /// <inheritdoc />
        public virtual void LoadPortal(PortalInfo portal, PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering LoadPortal(portal={Portal},portalSettings={PortalSettings})", portal, portalSettings);
            portalSettings.PortalName = portal.PortalName;
            portalSettings.LogoFile = portal.LogoFile;
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.LogoFile → {Arg2}", portal, portalSettings, portalSettings.LogoFile);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.FooterText → {Arg2}", portal, portalSettings, portalSettings.FooterText);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.ExpiryDate → {Arg2}", portal, portalSettings, portalSettings.ExpiryDate);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.UserRegistration → {Arg2}", portal, portalSettings, portalSettings.UserRegistration);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.BannerAdvertising → {Arg2}", portal, portalSettings, portalSettings.BannerAdvertising);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.Currency → {Arg2}", portal, portalSettings, portalSettings.Currency);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.AdministratorId → {Arg2}", portal, portalSettings, portalSettings.AdministratorId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.Email → {Arg2}", portal, portalSettings, portalSettings.Email);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.HostFee → {Arg2}", portal, portalSettings, portalSettings.HostFee);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.PortalName → {Arg2}", portal, portalSettings, portalSettings.PortalName);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.PageQuota → {Arg2}", portal, portalSettings, portalSettings.PageQuota);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.UserQuota → {Arg2}", portal, portalSettings, portalSettings.UserQuota);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.AdministratorRoleId → {Arg2}", portal, portalSettings, portalSettings.AdministratorRoleId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.AdministratorRoleName → {Arg2}", portal, portalSettings, portalSettings.AdministratorRoleName);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.RegisteredRoleId → {Arg2}", portal, portalSettings, portalSettings.RegisteredRoleId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.RegisteredRoleName → {Arg2}", portal, portalSettings, portalSettings.RegisteredRoleName);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.Description → {Arg2}", portal, portalSettings, portalSettings.Description);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.KeyWords → {Arg2}", portal, portalSettings, portalSettings.KeyWords);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.BackgroundFile → {Arg2}", portal, portalSettings, portalSettings.BackgroundFile);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.GUID → {Arg2}", portal, portalSettings, portalSettings.GUID);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.AdminTabId → {Arg2}", portal, portalSettings, portalSettings.AdminTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.SuperTabId → {Arg2}", portal, portalSettings, portalSettings.SuperTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.SplashTabId → {Arg2}", portal, portalSettings, portalSettings.SplashTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.HomeTabId → {Arg2}", portal, portalSettings, portalSettings.HomeTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.LoginTabId → {Arg2}", portal, portalSettings, portalSettings.LoginTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.RegisterTabId → {Arg2}", portal, portalSettings, portalSettings.RegisterTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.UserTabId → {Arg2}", portal, portalSettings, portalSettings.UserTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.SearchTabId → {Arg2}", portal, portalSettings, portalSettings.SearchTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.ErrorPage404 → {Arg2}", portal, portalSettings, portalSettings.ErrorPage404);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.ErrorPage500 → {Arg2}", portal, portalSettings, portalSettings.ErrorPage500);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.TermsTabId → {Arg2}", portal, portalSettings, portalSettings.TermsTabId);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.PrivacyTabId → {Arg2}", portal, portalSettings, portalSettings.PrivacyTabId);
            portalSettings.FooterText = portal.FooterText;
            portalSettings.ExpiryDate = portal.ExpiryDate;
            portalSettings.UserRegistration = portal.UserRegistration;
            portalSettings.BannerAdvertising = portal.BannerAdvertising;
            portalSettings.Currency = portal.Currency;
            portalSettings.AdministratorId = portal.AdministratorId;
            portalSettings.Email = portal.Email;
            portalSettings.HostFee = portal.HostFee;
            portalSettings.HostSpace = Null.IsNull(portal.HostSpace) ? 0 : portal.HostSpace;
            portalSettings.PageQuota = portal.PageQuota;
            portalSettings.UserQuota = portal.UserQuota;
            portalSettings.AdministratorRoleId = portal.AdministratorRoleId;
            portalSettings.AdministratorRoleName = portal.AdministratorRoleName;
            portalSettings.RegisteredRoleId = portal.RegisteredRoleId;
            portalSettings.RegisteredRoleName = portal.RegisteredRoleName;
            portalSettings.Description = portal.Description;
            portalSettings.KeyWords = portal.KeyWords;
            portalSettings.BackgroundFile = portal.BackgroundFile;
            portalSettings.GUID = portal.GUID;
            portalSettings.AdminTabId = portal.AdminTabId;
            portalSettings.SuperTabId = portal.SuperTabId;
            portalSettings.SplashTabId = portal.SplashTabId;
            portalSettings.HomeTabId = portal.HomeTabId;
            portalSettings.LoginTabId = portal.LoginTabId;
            portalSettings.RegisterTabId = portal.RegisterTabId;
            portalSettings.UserTabId = portal.UserTabId;
            portalSettings.SearchTabId = portal.SearchTabId;
            portalSettings.ErrorPage404 = portal.Custom404TabId;
            portalSettings.ErrorPage500 = portal.Custom500TabId;
            portalSettings.TermsTabId = portal.TermsTabId;
            portalSettings.PrivacyTabId = portal.PrivacyTabId;
            portalSettings.DefaultLanguage = Null.IsNull(portal.DefaultLanguage) ? Localization.SystemLocale : portal.DefaultLanguage;
            portalSettings.HomeDirectory = Globals.ApplicationPath + "/" + portal.HomeDirectory + "/";
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.HomeDirectory → {Arg2}", portal, portalSettings, portalSettings.HomeDirectory);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.HomeDirectoryMapPath → {Arg2}", portal, portalSettings, portalSettings.HomeDirectoryMapPath);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.HomeSystemDirectory → {Arg2}", portal, portalSettings, portalSettings.HomeSystemDirectory);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.HomeSystemDirectoryMapPath → {Arg2}", portal, portalSettings, portalSettings.HomeSystemDirectoryMapPath);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.Pages → {Arg2}", portal, portalSettings, portalSettings.Pages);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.Users → {Arg2}", portal, portalSettings, portalSettings.Users);
			Logger.LogDebug("LoadPortal(portal={Portal},portalSettings={PortalSettings}): portalSettings.CultureCode → {Arg2}", portal, portalSettings, portalSettings.CultureCode);
            portalSettings.HomeDirectoryMapPath = portal.HomeDirectoryMapPath;
            portalSettings.HomeSystemDirectory = Globals.ApplicationPath + "/" + portal.HomeSystemDirectory + "/";
            portalSettings.HomeSystemDirectoryMapPath = portal.HomeSystemDirectoryMapPath;
            portalSettings.Pages = portal.Pages;
            portalSettings.Users = portal.Users;
            portalSettings.CultureCode = portal.CultureCode;
			Logger.LogDebug("<<< Exiting LoadPortal(portal={Portal},portalSettings={PortalSettings})", portal, portalSettings);
        }

        /// <inheritdoc />
        public virtual void LoadPortalSettings(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering LoadPortalSettings(portalSettings={PortalSettings})", portalSettings);
            var settings = PortalController.Instance.GetPortalSettings(portalSettings.PortalId);
            portalSettings.Registration = new RegistrationSettings(settings);
			Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): settings → {Settings}", portalSettings, settings);

            var clientResourcesSettings = new ClientResourceSettings();
            bool overridingDefaultSettings = clientResourcesSettings.IsOverridingDefaultSettingsEnabled(portalSettings.PortalId);
			Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): clientResourcesSettings → {ClientResourcesSettings}", portalSettings, clientResourcesSettings);

            int crmVersion;
            if (overridingDefaultSettings)
            {
                int? globalVersion = new ClientResourceSettings().GetVersion(portalSettings.PortalId);
                crmVersion = globalVersion ?? default(int);
				Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): crmVersion → {CrmVersion}", portalSettings, crmVersion);
            }
            else
            {
                crmVersion = settings.GetValueOrDefault("CrmVersion", this.hostSettingsService.GetInteger("CrmVersion"));
            }

            portalSettings.AllowUserUICulture = settings.GetValueOrDefault("AllowUserUICulture", false);
            portalSettings.CdfVersion = crmVersion;
			Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): portalSettings.CdfVersion → {Arg1}", portalSettings, portalSettings.CdfVersion);
            portalSettings.ContentLocalizationEnabled = settings.GetValueOrDefault("ContentLocalizationEnabled", false);
            portalSettings.DefaultAdminContainer = settings.GetValueOrDefault("DefaultAdminContainer", this.hostSettings.DefaultAdminContainer);
            portalSettings.DefaultAdminSkin = settings.GetValueOrDefault("DefaultAdminSkin", this.hostSettings.DefaultAdminSkin);
            portalSettings.DefaultIconLocation = settings.GetValueOrDefault("DefaultIconLocation", "icons/sigma");
            portalSettings.DefaultModuleId = settings.GetValueOrDefault("defaultmoduleid", Null.NullInteger);
            portalSettings.DefaultModuleActionMenu = settings.GetValueOrDefault("DefaultModuleActionMenu", "~/admin/Menus/ModuleActions/ModuleActions.ascx");
            portalSettings.DefaultPortalContainer = settings.GetValueOrDefault("DefaultPortalContainer", this.hostSettings.DefaultPortalContainer);
            portalSettings.DefaultPortalSkin = settings.GetValueOrDefault("DefaultPortalSkin", this.hostSettings.DefaultPortalSkin);
            portalSettings.DefaultTabId = settings.GetValueOrDefault("defaulttabid", Null.NullInteger);
            portalSettings.EnableBrowserLanguage = settings.GetValueOrDefault("EnableBrowserLanguage", this.hostSettings.EnableBrowserLanguage);
            portalSettings.EnableCompositeFiles = settings.GetValueOrDefault("EnableCompositeFiles", false);
            portalSettings.EnablePopUps = settings.GetValueOrDefault("EnablePopUps", true);
            portalSettings.HideLoginControl = settings.GetValueOrDefault("HideLoginControl", false);
            portalSettings.EnableSkinWidgets = settings.GetValueOrDefault("EnableSkinWidgets", true);
            portalSettings.ShowCookieConsent = settings.GetValueOrDefault("ShowCookieConsent", false);
            portalSettings.CookieMoreLink = settings.GetValueOrDefault("CookieMoreLink", Null.NullString);
            portalSettings.EnableUrlLanguage = settings.GetValueOrDefault("EnableUrlLanguage", this.hostSettings.EnableUrlLanguage);
            portalSettings.HideFoldersEnabled = settings.GetValueOrDefault("HideFoldersEnabled", true);
            portalSettings.InlineEditorEnabled = settings.GetValueOrDefault("InlineEditorEnabled", true);
            portalSettings.AllowJsInModuleHeaders = settings.GetValueOrDefault("AllowJsInModuleHeaders", true);
            portalSettings.AllowJsInModuleFooters = settings.GetValueOrDefault("AllowJsInModuleFooters", true);
            portalSettings.SearchIncludeCommon = settings.GetValueOrDefault("SearchIncludeCommon", this.hostSettings.SearchIncludeCommon);
            portalSettings.SearchIncludeNumeric = settings.GetValueOrDefault("SearchIncludeNumeric", this.hostSettings.SearchIncludeNumeric);
            portalSettings.SearchIncludedTagInfoFilter = settings.GetValueOrDefault("SearchIncludedTagInfoFilter", this.hostSettings.SearchIncludedTagInfoFilter);
            portalSettings.SearchMaxWordlLength = settings.GetValueOrDefault("MaxSearchWordLength", this.hostSettings.SearchMaxWordLength);
            portalSettings.SearchMinWordlLength = settings.GetValueOrDefault("MinSearchWordLength", this.hostSettings.SearchMinWordLength);
            portalSettings.SSLSetup = (Abstractions.Security.SiteSslSetup)settings.GetValueOrDefault("SSLSetup", (int)Abstractions.Security.SiteSslSetup.Off);
            portalSettings.SSLEnforced = settings.GetValueOrDefault("SSLEnforced", false);
            portalSettings.SSLURL = settings.GetValueOrDefault("SSLURL", Null.NullString);
            portalSettings.STDURL = settings.GetValueOrDefault("STDURL", Null.NullString);
            portalSettings.EnableRegisterNotification = settings.GetValueOrDefault("EnableRegisterNotification", true);
            portalSettings.EnableUnapprovedPasswordReminderNotification = settings.GetValueOrDefault("EnableUnapprovedPasswordReminderNotification", true);
            portalSettings.DefaultAuthProvider = settings.GetValueOrDefault("DefaultAuthProvider", "DNN");
            portalSettings.SMTPConnectionLimit = settings.GetValueOrDefault("SMTPConnectionLimit", 2);
            portalSettings.SMTPMaxIdleTime = settings.GetValueOrDefault("SMTPMaxIdleTime", 100000);

            portalSettings.ControlPanelSecurity = PortalSettings.ControlPanelPermission.ModuleEditor;
			Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): portalSettings.ControlPanelSecurity → {Arg1}", portalSettings, portalSettings.ControlPanelSecurity);
            var setting = settings.GetValueOrDefault("ControlPanelSecurity", string.Empty);
            if (setting.Equals("TAB", StringComparison.OrdinalIgnoreCase))
			Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): setting → {Setting}", portalSettings, setting);
            {
                portalSettings.ControlPanelSecurity = PortalSettings.ControlPanelPermission.TabEditor;
            }

            portalSettings.DefaultControlPanelMode = PortalSettings.Mode.View;
			Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): portalSettings.DefaultControlPanelMode → {Arg1}", portalSettings, portalSettings.DefaultControlPanelMode);
            setting = settings.GetValueOrDefault("ControlPanelMode", string.Empty);
            if (setting.Equals("EDIT", StringComparison.OrdinalIgnoreCase))
            {
                portalSettings.DefaultControlPanelMode = PortalSettings.Mode.Edit;
            }

            setting = settings.GetValueOrDefault("ControlPanelVisibility", string.Empty);
            portalSettings.DefaultControlPanelVisibility = !setting.Equals("MIN", StringComparison.OrdinalIgnoreCase);

            setting = settings.GetValueOrDefault("TimeZone", string.Empty);
            if (!string.IsNullOrEmpty(setting))
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(setting);
                if (timeZone != null)
				Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): timeZone → {TimeZone}", portalSettings, timeZone);
                {
                    portalSettings.TimeZone = timeZone;
					Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): portalSettings.TimeZone → {Arg1}", portalSettings, portalSettings.TimeZone);
                }
            }

            setting = settings.GetValueOrDefault("DataConsentActive", "False");
            portalSettings.DataConsentActive = bool.Parse(setting);
            setting = settings.GetValueOrDefault("DataConsentTermsLastChange", string.Empty);
            if (!string.IsNullOrEmpty(setting))
            {
                portalSettings.DataConsentTermsLastChange = DateTime.Parse(setting, CultureInfo.InvariantCulture);
            }

            setting = settings.GetValueOrDefault("DataConsentConsentRedirect", "-1");
            portalSettings.DataConsentConsentRedirect = int.Parse(setting, CultureInfo.InvariantCulture);
            setting = settings.GetValueOrDefault("DataConsentUserDeleteAction", "0");
            portalSettings.DataConsentUserDeleteAction = (PortalSettings.UserDeleteAction)int.Parse(setting, CultureInfo.InvariantCulture);
            setting = settings.GetValueOrDefault("DataConsentDelay", "1");
            portalSettings.DataConsentDelay = int.Parse(setting, CultureInfo.InvariantCulture);
            setting = settings.GetValueOrDefault("DataConsentDelayMeasurement", "d");
            portalSettings.DataConsentDelayMeasurement = setting;
			Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): portalSettings.DataConsentDelayMeasurement → {Arg1}", portalSettings, portalSettings.DataConsentDelayMeasurement);
            setting = settings.GetValueOrDefault("AllowedExtensionsWhitelist", this.hostSettingsService.GetString("DefaultEndUserExtensionWhitelist"));
            portalSettings.AllowedExtensionsWhitelist = new FileExtensionWhitelist(setting);

            setting = settings.GetValueOrDefault("CspHeaderMode", "OFF");
            switch (setting.ToUpperInvariant())
            {
                case "ON":
                    portalSettings.CspHeaderMode = PortalSettings.CspMode.On;
					Logger.LogDebug("LoadPortalSettings(portalSettings={PortalSettings}): portalSettings.CspHeaderMode → {Arg1}", portalSettings, portalSettings.CspHeaderMode);
                    break;
                case "REPORTONLY":
                    portalSettings.CspHeaderMode = PortalSettings.CspMode.ReportOnly;
                    break;
                default:
                    portalSettings.CspHeaderMode = PortalSettings.CspMode.Off;
                    break;
            }

            portalSettings.CspHeaderFixed = settings.GetValueOrDefault("CspHeaderFixed", false);
            portalSettings.CspHeader = settings.GetValueOrDefault("CspHeader", "style-src 'unsafe-inline'; object-src 'none'; frame-ancestors 'none';");
            portalSettings.CspReportingHeader = settings.GetValueOrDefault("CspReportingHeader", string.Empty);
			Logger.LogDebug("<<< Exiting LoadPortalSettings(portalSettings={PortalSettings})", portalSettings);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected List<TabInfo> GetBreadcrumbs(int tabId, int portalId)
        {
            var breadCrumbs = new List<TabInfo>();
            GetBreadCrumbs(breadCrumbs, tabId, portalId);
            return breadCrumbs;
        }

        protected virtual void UpdateSkinSettings(TabInfo activeTab, PortalSettings portalSettings)
        {
            if (Globals.IsAdminSkin())
            {
                // DNN-6170 ensure skin value is culture specific
                activeTab.SkinSrc = string.IsNullOrEmpty(
                    PortalController.GetPortalSetting(
                        "DefaultAdminSkin",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultAdminSkin,
                        portalSettings.CultureCode))
                    ? portalSettings.DefaultAdminSkin
                    : PortalController.GetPortalSetting(
                        "DefaultAdminSkin",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultAdminSkin,
                        portalSettings.CultureCode);
            }
            else if (string.IsNullOrEmpty(activeTab.SkinSrc))
            {
                // DNN-6170 ensure skin value is culture specific
                activeTab.SkinSrc = string.IsNullOrEmpty(
                    PortalController.GetPortalSetting(
                        "DefaultPortalSkin",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultPortalSkin,
                        portalSettings.CultureCode))
                    ? portalSettings.DefaultPortalSkin
                    : PortalController.GetPortalSetting(
                        "DefaultPortalSkin",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultPortalSkin,
                        portalSettings.CultureCode);
            }

            activeTab.SkinSrc = SkinController.FormatSkinSrc(activeTab.SkinSrc, portalSettings);
            activeTab.SkinPath = SkinController.FormatSkinPath(activeTab.SkinSrc);

            if (Globals.IsAdminSkin())
            {
                activeTab.ContainerSrc = string.IsNullOrEmpty(
                    PortalController.GetPortalSetting(
                        "DefaultAdminContainer",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultAdminContainer,
                        portalSettings.CultureCode))
                    ? portalSettings.DefaultAdminContainer
                    : PortalController.GetPortalSetting(
                        "DefaultAdminContainer",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultAdminContainer,
                        portalSettings.CultureCode);
            }
            else if (string.IsNullOrEmpty(activeTab.ContainerSrc))
            {
                activeTab.ContainerSrc = string.IsNullOrEmpty(
                    PortalController.GetPortalSetting(
                        "DefaultPortalContainer",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultPortalContainer,
                        portalSettings.CultureCode))
                    ? portalSettings.DefaultPortalContainer
                    : PortalController.GetPortalSetting(
                        "DefaultPortalContainer",
                        portalSettings.PortalId,
                        this.hostSettings.DefaultPortalContainer,
                        portalSettings.CultureCode);
            }

            activeTab.ContainerSrc = SkinController.FormatSkinSrc(activeTab.ContainerSrc, portalSettings);
            activeTab.ContainerPath = SkinController.FormatSkinPath(activeTab.ContainerSrc);
        }

        private static void GetBreadCrumbs(List<TabInfo> breadCrumbs, int tabId, int portalId)
        {
            var portalTabs = TabController.Instance.GetTabsByPortal(portalId);
            var hostTabs = TabController.Instance.GetTabsByPortal(Null.NullInteger);
            while (true)
            {
                TabInfo tab;
                if (portalTabs.TryGetValue(tabId, out tab) || hostTabs.TryGetValue(tabId, out tab))
                {
                    // add tab to breadcrumb collection
                    breadCrumbs.Insert(0, tab.Clone());

                    // get the tab parent
                    if (!Null.IsNull(tab.ParentId) && tabId != tab.ParentId)
                    {
                        tabId = tab.ParentId;
                        continue;
                    }
                }

                break;
            }
        }

        private static TabInfo GetSpecialTab(int portalId, int tabId)
        {
            TabInfo activeTab = null;

            if (tabId > 0)
            {
                var tab = TabController.Instance.GetTab(tabId, portalId, false);
                if (tab != null)
                {
                    activeTab = tab.Clone();
                }
            }

            return activeTab;
        }

        // This method is called few times with each request; it would be
        // better to have it cache the "activeTab" for a short period.
        private static TabInfo GetTab(int tabId, TabCollection tabs)
        {
			Logger.LogDebug(">>> Entering GetTab(tabId={TabId},tabs={Tabs})", tabId, tabs);
			Logger.LogDebug("<<< Exiting GetTab(tabId={TabId},tabs={Tabs})", tabId, tabs);
            return tabId != Null.NullInteger && tabs.TryGetValue(tabId, out var tab) && !tab.IsDeleted
                ? tab.Clone()
                : null;
        }
    }
}
