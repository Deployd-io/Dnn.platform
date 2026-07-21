// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Dynamic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Web.Http;
    using System.Xml;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.SiteSettings.Services.Dto;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.UI.Internals;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    using Constants = Dnn.PersonaBar.Library.Constants;
    using FileInfo = System.IO.FileInfo;

    /// <summary>Provides Web API methods for the Site Settings module to use.</summary>
    /// <param name="navigationManager">A manager to provide navigation services.</param>
    /// <param name="appStatus">The application status info.</param>
    /// <param name="personalizationController">The personalization controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="listController">The list controller.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="portalAliasService">The portal alias service.</param>
    /// <param name="roleProvider">The role provider.</param>
    /// <param name="tabController">The tab controller.</param>
    [MenuPermission(MenuName = Components.Constants.Constants.MenuName)]
    public class SiteSettingsController(INavigationManager navigationManager, IApplicationStatusInfo appStatus, PersonalizationController personalizationController, IHostSettings hostSettings, ListController listController, IPortalController portalController, IPortalGroupController portalGroupController, ICryptographyProvider cryptographyProvider, IHostSettingsService hostSettingsService, IEventLogger eventLogger, IPortalAliasService portalAliasService, RoleProvider roleProvider, ITabController tabController)
        : PersonaBarApiController
    {
		Logger.LogDebug(">>> Entering SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController})", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController);
        // Field Boost Settings - they are scaled down by 10.
        private const int DefaultSearchTitleBoost = 50;
        private const string AuthFailureMessage = "Authorization has been denied for this request.";
        private const int DefaultSearchTagBoost = 40;
        private const int DefaultSearchContentBoost = 35;
        private const int DefaultSearchDescriptionBoost = 20;
        private const int DefaultSearchAuthorBoost = 15;

        // Field Boost Setting Names
        private const string SearchTitleBoostSetting = "Search_Title_Boost";
        private const string SearchTagBoostSetting = "Search_Tag_Boost";
        private const string SearchContentBoostSetting = "Search_Content_Boost";
        private const string SearchDescriptionBoostSetting = "Search_Description_Boost";
        private const string SearchAuthorBoostSetting = "Search_Author_Boost";

        private const double DefaultMessagingThrottlingInterval = 0.5; // set default MessagingThrottlingInterval value to 30 seconds.
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SiteSettingsController));

		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): Logger → {Logger}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, Logger);
        private readonly Components.SiteSettingsController controller = new Components.SiteSettingsController();
        private readonly INavigationManager navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): controller → {Controller}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, controller);
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): navigationManager → {NavigationManager1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, navigationManager);
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly PersonalizationController personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): appStatus → {AppStatus1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, appStatus);
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): personalizationController → {PersonalizationController1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, personalizationController);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): hostSettings → {HostSettings1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, hostSettings);
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): listController → {ListController1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, listController);
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): portalController → {PortalController1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, portalController);
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): portalGroupController → {PortalGroupController1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, portalGroupController);
        private readonly ICryptographyProvider cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        private readonly IHostSettingsService hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): cryptographyProvider → {CryptographyProvider1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, cryptographyProvider);
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): hostSettingsService → {HostSettingsService1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, hostSettingsService);
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IPortalAliasService portalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): eventLogger → {EventLogger1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, eventLogger);
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): portalAliasService → {PortalAliasService1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, portalAliasService);
        private readonly RoleProvider roleProvider = roleProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>();
        private readonly ITabController tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): roleProvider → {RoleProvider1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, roleProvider);
		Logger.LogDebug("SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController}): tabController → {TabController1}", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController, tabController);

        /// <summary>Initializes a new instance of the <see cref="SiteSettingsController"/> class.</summary>
        /// <param name="navigationManager">A manager to provide navigation services.</param>
        /// <param name="applicationStatusInfo">The application status info.</param>
		Logger.LogDebug("<<< Exiting SiteSettingsController(navigationManager={NavigationManager},appStatus={AppStatus},personalizationController={PersonalizationController},hostSettings={HostSettings},listController={ListController},portalController={PortalController},portalGroupController={PortalGroupController},cryptographyProvider={CryptographyProvider},hostSettingsService={HostSettingsService},eventLogger={EventLogger},portalAliasService={PortalAliasService},roleProvider={RoleProvider},tabController={TabController})", navigationManager, appStatus, personalizationController, hostSettings, listController, portalController, portalGroupController, cryptographyProvider, hostSettingsService, eventLogger, portalAliasService, roleProvider, tabController);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public SiteSettingsController(INavigationManager navigationManager, IApplicationStatusInfo applicationStatusInfo)
            : this(navigationManager, applicationStatusInfo, null, null, null, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SiteSettingsController"/> class.</summary>
        /// <param name="navigationManager">A manager to provide navigation services.</param>
        /// <param name="applicationStatusInfo">The application status info.</param>
        /// <param name="personalizationController">The personalization controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public SiteSettingsController(INavigationManager navigationManager, IApplicationStatusInfo applicationStatusInfo, PersonalizationController personalizationController, IHostSettings hostSettings)
            : this(navigationManager, applicationStatusInfo, personalizationController, hostSettings, null, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Gets provides navigation services.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.10.2. Please use DI to register your own navigation manager. Scheduled for removal in v11.0.0.")]
        protected INavigationManager NavigationManager => this.navigationManager;

        /// GET: api/SiteSettings/GetPortalSettings
        /// <summary>Gets site settings.</summary>
        /// <param name="portalId">The ID of the portal to get the settings for.</param>
        /// <param name="cultureCode">The culture in which to get the settings in.</param>
        /// <returns>Localized site settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.Constants.MenuName, Permission = Components.Constants.Constants.SiteInfoView)]
        public HttpResponseMessage GetPortalSettings(int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                cultureCode = string.IsNullOrEmpty(cultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : cultureCode;

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): language → {Language}", portalId, cultureCode, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portal = PortalController.Instance.GetPortal(pid, cultureCode);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
				Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): portalSettings → {PortalSettings}", portalId, cultureCode, portalSettings);
                var logoFile = string.IsNullOrEmpty(portal.LogoFile) ? null : FileManager.Instance.GetFile(pid, portal.LogoFile);
                var faviconPath = new FavIcon(((IPortalInfo)portal).PortalId).GetSettingPath();
				Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): logoFile → {LogoFile}", portalId, cultureCode, logoFile);
				Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): faviconPath → {FaviconPath}", portalId, cultureCode, faviconPath);
                var favIcon = string.IsNullOrEmpty(faviconPath)
                    ? null
                    : FileManager.Instance.GetFile(pid, faviconPath);

                var settings = new
                {
                    ((IPortalInfo)portal).PortalId,
                    portal.CultureCode,
                    portal.PortalName,
                    portal.Description,
                    portal.KeyWords,
                    GUID = portal.GUID.ToString().ToUpperInvariant(),
                    portal.FooterText,
                    TimeZone = portalSettings.TimeZone.Id,
                    portal.HomeDirectory,
                    LogoFile = logoFile != null ? new FileDto()
                    {
					Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): GUID → {GUID}", portalId, cultureCode, GUID);
                        fileName = logoFile.FileName,
                        folderPath = logoFile.Folder,
                        fileId = logoFile.FileId,
                        folderId = logoFile.FolderId,
                    }
                    : null,
                    FavIcon = favIcon != null ? new FileDto()
                    {
                        fileName = favIcon.FileName,
                        folderPath = favIcon.Folder,
                        fileId = favIcon.FileId,
                        folderId = favIcon.FolderId,
                    }
                    : null,
                    new DnnFileUploadOptions(this.hostSettings, this.appStatus, this.cryptographyProvider).ValidationCode,
                    IconSet = PortalController.GetPortalSetting(this.portalController, "DefaultIconLocation", pid, "Sigma", cultureCode).Replace("icons/", string.Empty),
                };
					Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): IconSet → {IconSet}", portalId, cultureCode, IconSet);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = settings,
                    TimeZones = TimeZoneInfo.GetSystemTimeZones().Select(z => new
                    {
					Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): Settings → {Settings}", portalId, cultureCode, Settings);
                        z.Id,
                        z.DisplayName,
                    }),
                    IconSets = IconController.GetIconSets(this.appStatus),
                });
					Logger.LogDebug("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): IconSets → {IconSets}", portalId, cultureCode, IconSets);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetPortalSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetCultureList
        /// <summary>Gets culture list.</summary>
        /// <param name="portalId">The ID of the portal for which to get the list of cultures.</param>
        /// <returns>Culture List.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.Constants.MenuName, Permission = Components.Constants.Constants.SiteInfoView)]
        public HttpResponseMessage GetCultureList(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetCultureList(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("GetCultureList(portalId={PortalId}): portal → {Portal}", portalId, portal);
				Logger.LogDebug("GetCultureList(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

                string viewType = this.GetLanguageDisplayMode(pid);

                var locals = LocaleController.Instance.GetLocales(pid).Values;
                var cultureCodeList = locals.Select(local => new
                {
				Logger.LogDebug("GetCultureList(portalId={PortalId}): locals → {Locals}", portalId, locals);
                    Name = viewType == "NATIVE" ? local.NativeName : local.EnglishName,
                    local.Code,
                    Icon = Globals.ResolveUrl(string.IsNullOrEmpty(local.Code) ? "~/images/Flags/none.gif" :
                        $"~/images/Flags/{local.Code}.gif"),
                    IsDefault = local.Code == portalSettings.DefaultLanguage,
                }).ToList();
						Logger.LogDebug("GetCultureList(portalId={PortalId}): Icon → {Icon}", portalId, Icon);

                var response = new
                {
                    Success = true,
                    Cultures = cultureCodeList,
                };
					Logger.LogDebug("GetCultureList(portalId={PortalId}): Success → {Success}", portalId, Success);
					Logger.LogDebug("GetCultureList(portalId={PortalId}): Cultures → {Cultures}", portalId, Cultures);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetCultureList(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdatePortalSettings
        /// <summary>Updates site settings.</summary>
        /// <param name="request"><see cref="UpdateSiteSettingsRequest"/>.</param>
        /// <returns>A success or error message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.Constants.MenuName, Permission = Components.Constants.Constants.SiteInfoView + "&" + Components.Constants.Constants.SiteInfoEdit)]
        public HttpResponseMessage UpdatePortalSettings(UpdateSiteSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdatePortalSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var cultureCode = string.IsNullOrEmpty(request.CultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : request.CultureCode;

				Logger.LogDebug("UpdatePortalSettings(request={Request}): cultureCode → {CultureCode}", request, cultureCode);
                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("UpdatePortalSettings(request={Request}): language → {Language}", request, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portalInfo = PortalController.Instance.GetPortal(pid, cultureCode);
                portalInfo.PortalName = request.PortalName;
				Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo → {PortalInfo}", request, portalInfo);
				Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo.PortalName → {Arg1}", request, portalInfo.PortalName);

                if (request.LogoFile != null && request.LogoFile.fileId != Null.NullInteger && !string.IsNullOrEmpty(request.LogoFile.fileName))
                {
                    portalInfo.LogoFile = FileManager.Instance.GetFile(request.LogoFile.fileId).RelativePath;
                }
                else
                {
                    portalInfo.LogoFile = string.Empty;
					Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo.LogoFile → {Arg1}", request, portalInfo.LogoFile);
                }

                portalInfo.FooterText = request.FooterText;
				Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo.FooterText → {Arg1}", request, portalInfo.FooterText);
				Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo.Description → {Arg1}", request, portalInfo.Description);
				Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo.KeyWords → {Arg1}", request, portalInfo.KeyWords);
                portalInfo.Description = request.Description;
                portalInfo.KeyWords = request.KeyWords;

                PortalController.Instance.UpdatePortalInfo(portalInfo);
                PortalController.UpdatePortalSetting(this.portalController, pid, "TimeZone", request.TimeZone, false);

                if (request.FavIcon != null && request.FavIcon.fileId != Null.NullInteger)
                {
                    new FavIcon(pid).Update(request.FavIcon.fileId);
                }

                PortalController.UpdatePortalSetting(this.portalController, pid, "DefaultIconLocation", "icons/" + request.IconSet, false, cultureCode);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdatePortalSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetDefaultPagesSettings
        /// <summary>Gets default pages settings.</summary>
        /// <param name="portalId">The ID of the portal for which to getting the settings for.</param>
        /// <param name="cultureCode">The culture code for which to get the page settings in.</param>
        /// <returns>default pages settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetDefaultPagesSettings(int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                cultureCode = string.IsNullOrEmpty(cultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : cultureCode;

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): language → {Language}", portalId, cultureCode, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portal = PortalController.Instance.GetPortal(pid, cultureCode);
                var localizedPortalSettings = PortalController.Instance.GetPortalSettings(pid, cultureCode);
				Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
				Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): localizedPortalSettings → {LocalizedPortalSettings}", portalId, cultureCode, localizedPortalSettings);

                int redirectAfterLoginTabId = GetLocalizedTabIdSetting(localizedPortalSettings, "Redirect_AfterLogin");
                int redirectAfterLogoutTabId = GetLocalizedTabIdSetting(localizedPortalSettings, "Redirect_AfterLogout");
				Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): redirectAfterLoginTabId → {RedirectAfterLoginTabId}", portalId, cultureCode, redirectAfterLoginTabId);
				Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): redirectAfterLogoutTabId → {RedirectAfterLogoutTabId}", portalId, cultureCode, redirectAfterLogoutTabId);
                int redirectAfterRegistrationTabId = GetLocalizedTabIdSetting(localizedPortalSettings, "Redirect_AfterRegistration");

				Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): redirectAfterRegistrationTabId → {RedirectAfterRegistrationTabId}", portalId, cultureCode, redirectAfterRegistrationTabId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
					Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): Settings → {Settings}", portalId, cultureCode, Settings);
                    {
                        ((IPortalInfo)portal).PortalId,
                        portal.CultureCode,
                        SplashTabId = this.TabSanitizer(portal.SplashTabId, pid)?.TabID,
                        SplashTabName = this.TabSanitizer(portal.SplashTabId, pid)?.TabName,
                        HomeTabId = this.TabSanitizer(portal.HomeTabId, pid)?.TabID,
                        HomeTabName = this.TabSanitizer(portal.HomeTabId, pid)?.TabName,
                        LoginTabId = this.TabSanitizer(portal.LoginTabId, pid)?.TabID,
                        LoginTabName = this.TabSanitizer(portal.LoginTabId, pid)?.TabName,
                        RegisterTabId = this.TabSanitizer(portal.RegisterTabId, pid)?.TabID,
                        RegisterTabName = this.TabSanitizer(portal.RegisterTabId, pid)?.TabName,
                        UserTabId = this.TabSanitizer(portal.UserTabId, pid)?.TabID,
                        UserTabName = this.TabSanitizer(portal.UserTabId, pid)?.TabName,
                        SearchTabId = this.TabSanitizer(portal.SearchTabId, pid)?.TabID,
                        SearchTabName = this.TabSanitizer(portal.SearchTabId, pid)?.TabName,
                        Custom404TabId = this.TabSanitizer(portal.Custom404TabId, pid)?.TabID,
                        Custom404TabName = this.TabSanitizer(portal.Custom404TabId, pid)?.TabName,
                        Custom500TabId = this.TabSanitizer(portal.Custom500TabId, pid)?.TabID,
                        Custom500TabName = this.TabSanitizer(portal.Custom500TabId, pid)?.TabName,
                        TermsTabId = this.TabSanitizer(portal.TermsTabId, pid)?.TabID,
                        TermsTabName = this.TabSanitizer(portal.TermsTabId, pid)?.TabName,
                        PrivacyTabId = this.TabSanitizer(portal.PrivacyTabId, pid)?.TabID,
                        PrivacyTabName = this.TabSanitizer(portal.PrivacyTabId, pid)?.TabName,
                        RedirectAfterLoginTabId = this.TabSanitizer(redirectAfterLoginTabId, pid)?.TabID,
                        RedirectAfterLoginTabName = this.TabSanitizer(redirectAfterLoginTabId, pid)?.TabName,
                        RedirectAfterLogoutTabId = this.TabSanitizer(redirectAfterLogoutTabId, pid)?.TabID,
                        RedirectAfterLogoutTabName = this.TabSanitizer(redirectAfterLogoutTabId, pid)?.TabName,
                        RedirectAfterRegistrationTabId = this.TabSanitizer(redirectAfterRegistrationTabId, pid)?.TabID,
                        RedirectAfterRegistrationTabName = this.TabSanitizer(redirectAfterRegistrationTabId, pid)?.TabName,
                        PageHeadText = localizedPortalSettings["PageHeadText"],
                    },
                });
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): SplashTabId → {SplashTabId}", portalId, cultureCode, SplashTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): SplashTabName → {SplashTabName}", portalId, cultureCode, SplashTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): HomeTabId → {HomeTabId}", portalId, cultureCode, HomeTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): HomeTabName → {HomeTabName}", portalId, cultureCode, HomeTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): LoginTabId → {LoginTabId}", portalId, cultureCode, LoginTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): LoginTabName → {LoginTabName}", portalId, cultureCode, LoginTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RegisterTabId → {RegisterTabId}", portalId, cultureCode, RegisterTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RegisterTabName → {RegisterTabName}", portalId, cultureCode, RegisterTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): UserTabId → {UserTabId}", portalId, cultureCode, UserTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): UserTabName → {UserTabName}", portalId, cultureCode, UserTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): SearchTabId → {SearchTabId}", portalId, cultureCode, SearchTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): SearchTabName → {SearchTabName}", portalId, cultureCode, SearchTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): Custom404TabId → {Custom404TabId}", portalId, cultureCode, Custom404TabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): Custom404TabName → {Custom404TabName}", portalId, cultureCode, Custom404TabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): Custom500TabId → {Custom500TabId}", portalId, cultureCode, Custom500TabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): Custom500TabName → {Custom500TabName}", portalId, cultureCode, Custom500TabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): TermsTabId → {TermsTabId}", portalId, cultureCode, TermsTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): TermsTabName → {TermsTabName}", portalId, cultureCode, TermsTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): PrivacyTabId → {PrivacyTabId}", portalId, cultureCode, PrivacyTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): PrivacyTabName → {PrivacyTabName}", portalId, cultureCode, PrivacyTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RedirectAfterLoginTabId → {RedirectAfterLoginTabId}", portalId, cultureCode, RedirectAfterLoginTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RedirectAfterLoginTabName → {RedirectAfterLoginTabName}", portalId, cultureCode, RedirectAfterLoginTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RedirectAfterLogoutTabId → {RedirectAfterLogoutTabId}", portalId, cultureCode, RedirectAfterLogoutTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RedirectAfterLogoutTabName → {RedirectAfterLogoutTabName}", portalId, cultureCode, RedirectAfterLogoutTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RedirectAfterRegistrationTabId → {RedirectAfterRegistrationTabId}", portalId, cultureCode, RedirectAfterRegistrationTabId);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): RedirectAfterRegistrationTabName → {RedirectAfterRegistrationTabName}", portalId, cultureCode, RedirectAfterRegistrationTabName);
						Logger.LogDebug("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): PageHeadText → {PageHeadText}", portalId, cultureCode, PageHeadText);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateDefaultPagesSettings
        /// <summary>Updates default pages settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateDefaultPagesSettings(UpdateDefaultPagesSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateDefaultPagesSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var cultureCode = string.IsNullOrEmpty(request.CultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : request.CultureCode;
                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
				Logger.LogDebug("UpdateDefaultPagesSettings(request={Request}): cultureCode → {CultureCode}", request, cultureCode);
                if (language == null)
				Logger.LogDebug("UpdateDefaultPagesSettings(request={Request}): language → {Language}", request, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portalInfo = PortalController.Instance.GetPortal(pid, cultureCode);
                portalInfo.SplashTabId = this.ValidateTabId(request.SplashTabId, pid);
				Logger.LogDebug("UpdateDefaultPagesSettings(request={Request}): portalInfo → {PortalInfo}", request, portalInfo);
                portalInfo.HomeTabId = this.ValidateTabId(request.HomeTabId, pid);
                portalInfo.LoginTabId = this.ValidateTabId(request.LoginTabId, pid);
                portalInfo.RegisterTabId = this.ValidateTabId(request.RegisterTabId, pid);
                portalInfo.UserTabId = this.ValidateTabId(request.UserTabId, pid);
                portalInfo.SearchTabId = this.ValidateTabId(request.SearchTabId, pid);
                portalInfo.Custom404TabId = this.ValidateTabId(request.Custom404TabId, pid);
                portalInfo.Custom500TabId = this.ValidateTabId(request.Custom500TabId, pid);
                portalInfo.TermsTabId = this.ValidateTabId(request.TermsTabId, pid);
                portalInfo.PrivacyTabId = this.ValidateTabId(request.PrivacyTabId, pid);
                PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(this.portalController, pid, "Redirect_AfterLogin", this.ValidateTabId(request.RedirectAfterLoginTabId, pid).ToString(CultureInfo.InvariantCulture), false, cultureCode);
                PortalController.UpdatePortalSetting(this.portalController, pid, "Redirect_AfterLogout", this.ValidateTabId(request.RedirectAfterLogoutTabId, pid).ToString(CultureInfo.InvariantCulture), false, cultureCode);
                PortalController.UpdatePortalSetting(this.portalController, pid, "Redirect_AfterRegistration", this.ValidateTabId(request.RedirectAfterRegistrationTabId, pid).ToString(CultureInfo.InvariantCulture), false, cultureCode);
                PortalController.UpdatePortalSetting(this.portalController, pid, "PageHeadText", string.IsNullOrEmpty(request.PageHeadText) ? "false" : request.PageHeadText);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateDefaultPagesSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetMessagingSettings
        /// <summary>Gets messaging settings.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>messaging settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetMessagingSettings(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetMessagingSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
				Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
					Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): Settings → {Settings}", portalId, Settings);
                    {
                        ((IPortalInfo)portal).PortalId,
                        portal.CultureCode,
                        portalSettings.DisablePrivateMessage,
                        ThrottlingInterval = PortalController.GetPortalSettingAsDouble(this.portalController, "MessagingThrottlingInterval", pid, DefaultMessagingThrottlingInterval),
                        RecipientLimit = PortalController.GetPortalSettingAsInteger(this.portalController, "MessagingRecipientLimit", pid, 5),
                        AllowAttachments = PortalController.GetPortalSettingAsBoolean(this.portalController, "MessagingAllowAttachments", pid, false),
                        ProfanityFilters = PortalController.GetPortalSettingAsBoolean(this.portalController, "MessagingProfanityFilters", pid, false),
                        IncludeAttachments = PortalController.GetPortalSettingAsBoolean(this.portalController, "MessagingIncludeAttachments", pid, false),
                        SendEmail = PortalController.GetPortalSetting(this.portalController, "MessagingSendEmail", pid, "YES") == "YES",
                    },
                });
						Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): ThrottlingInterval → {ThrottlingInterval}", portalId, ThrottlingInterval);
						Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): RecipientLimit → {RecipientLimit}", portalId, RecipientLimit);
						Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): AllowAttachments → {AllowAttachments}", portalId, AllowAttachments);
						Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): ProfanityFilters → {ProfanityFilters}", portalId, ProfanityFilters);
						Logger.LogDebug("GetMessagingSettings(portalId={PortalId}): IncludeAttachments → {IncludeAttachments}", portalId, IncludeAttachments);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetMessagingSettings(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateMessagingSettings
        /// <summary>Updates messaging settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateMessagingSettings(UpdateMessagingSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateMessagingSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                PortalController.UpdatePortalSetting(this.portalController, pid, "MessagingThrottlingInterval", request.ThrottlingInterval.ToString("F1", CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "MessagingRecipientLimit", request.RecipientLimit.ToString(CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "MessagingAllowAttachments", request.AllowAttachments ? "YES" : "NO", false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "MessagingIncludeAttachments", request.IncludeAttachments ? "YES" : "NO", false);

                PortalController.UpdatePortalSetting(this.portalController, pid, "MessagingProfanityFilters", request.ProfanityFilters ? "YES" : "NO", false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "MessagingSendEmail", request.SendEmail ? "YES" : "NO", false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "DisablePrivateMessage", request.DisablePrivateMessage ? "Y" : "N", false);

                DataCache.ClearPortalCache(pid, false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateMessagingSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetProfileSettings
        /// <summary>Gets profile settings.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>profile settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetProfileSettings(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetProfileSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var urlSettings = new FriendlyUrlSettings(this.portalController, this.hostSettings, this.hostSettingsService, pid);
                var userSettings = UserController.GetUserSettings(pid);
				Logger.LogDebug("GetProfileSettings(portalId={PortalId}): urlSettings → {UrlSettings}", portalId, urlSettings);
				Logger.LogDebug("GetProfileSettings(portalId={PortalId}): userSettings → {UserSettings}", portalId, userSettings);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
					Logger.LogDebug("GetProfileSettings(portalId={PortalId}): Settings → {Settings}", portalId, Settings);
                    {
                        PortalId = pid,
                        RedirectOldProfileUrl = Config.GetFriendlyUrlProvider() == "advanced" && urlSettings.RedirectOldProfileUrl,
                        urlSettings.VanityUrlPrefix,
                        ProfileDefaultVisibility = userSettings["Profile_DefaultVisibility"] == null ? (int)UserVisibilityMode.AdminOnly : Convert.ToInt32(userSettings["Profile_DefaultVisibility"], CultureInfo.InvariantCulture),
                        ProfileDisplayVisibility = PortalController.GetPortalSettingAsBoolean(this.portalController, "Profile_DisplayVisibility", pid, true),
                    },
                    UserVisibilityOptions = Enum.GetValues(typeof(UserVisibilityMode)).Cast<UserVisibilityMode>().Select(
                        v => new
						Logger.LogDebug("GetProfileSettings(portalId={PortalId}): PortalId → {PortalId1}", portalId, PortalId);
                        {
                            label = v.ToString(),
                            value = (int)v,
                        }).ToList(),
                });
							Logger.LogDebug("GetProfileSettings(portalId={PortalId}): label → {Label}", portalId, label);
							Logger.LogDebug("GetProfileSettings(portalId={PortalId}): value → {Value}", portalId, value);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetProfileSettings(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateProfileSettings
        /// <summary>Updates profile settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateProfileSettings(UpdateProfileSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateProfileSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (Config.GetFriendlyUrlProvider() == "advanced")
                {
                    PortalController.UpdatePortalSetting(this.portalController, pid, FriendlyUrlSettings.RedirectOldProfileUrlSetting, request.RedirectOldProfileUrl ? "Y" : "N", false);
                }

                PortalController.UpdatePortalSetting(this.portalController, pid, FriendlyUrlSettings.VanityUrlPrefixSetting, request.VanityUrlPrefix, false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "Profile_DefaultVisibility", request.ProfileDefaultVisibility.ToString(CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "Profile_DisplayVisibility", request.ProfileDisplayVisibility.ToString(), true);

                DataCache.ClearPortalCache(pid, false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateProfileSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetProfileProperties
        /// <summary>Gets profile properties.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>profile properties.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetProfileProperties(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetProfileProperties(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var profileProperties =
                    from v in ProfileController.GetPropertyDefinitionsByPortal(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, pid, false, false)
                    orderby v.ViewOrder
                    select new
                    {
                        v.PropertyDefinitionId,
                        v.PropertyName,
                        DataType = DisplayDataType(this.listController, v.DataType),
                        DefaultVisibility = v.DefaultVisibility.ToString(),
                        v.Required,
                        v.Visible,
                        v.ViewOrder,
                        CanDelete = CanDeleteProperty(v),
                    };
						Logger.LogDebug("GetProfileProperties(portalId={PortalId}): DataType → {DataType}", portalId, DataType);
						Logger.LogDebug("GetProfileProperties(portalId={PortalId}): DefaultVisibility → {DefaultVisibility}", portalId, DefaultVisibility);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    PortalId = pid,
                    Properties = profileProperties,
                });
					Logger.LogDebug("GetProfileProperties(portalId={PortalId}): PortalId → {PortalId1}", portalId, PortalId);
					Logger.LogDebug("GetProfileProperties(portalId={PortalId}): Properties → {Properties}", portalId, Properties);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetProfileProperties(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetProfileProperty
        /// <summary>Gets profile property by ID.</summary>
        /// <param name="propertyId">The property ID.</param>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>profile property.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetProfileProperty(int? propertyId, int? portalId)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var profileProperty = ProfileController.GetPropertyDefinition(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, propertyId ?? -1, pid);

                var cultureList = Localization.LoadCultureInListItems(this.GetCultureDropDownType(pid), Thread.CurrentThread.CurrentUICulture.Name, string.Empty, false);

                var response = new
                {
                    Success = true,
                    ProfileProperty = profileProperty != null ? new
                    {
                        profileProperty.PortalId,
                        profileProperty.PropertyDefinitionId,
                        profileProperty.PropertyName,
                        profileProperty.DataType,
                        profileProperty.PropertyCategory,
                        profileProperty.Length,
                        profileProperty.DefaultValue,
                        profileProperty.ValidationExpression,
                        profileProperty.Required,
                        profileProperty.ReadOnly,
                        profileProperty.Visible,
                        profileProperty.ViewOrder,
                        DefaultVisibility = (int)profileProperty.DefaultVisibility,
                    }
                    : null,
                    UserVisibilityOptions = Enum.GetValues(typeof(UserVisibilityMode)).Cast<UserVisibilityMode>().Select(
                        v => new
                        {
                            label = v.ToString(),
                            value = (int)v,
                        }).ToList(),
                    DataTypeOptions = this.listController.GetListEntryInfoItems("DataType").Select(t => new
                    {
                        t.EntryID,
                        t.Value,
                    }),
                    LanguageOptions = cultureList.Select(c => new
                    {
                        c.Text,
                        c.Value,
                        Icon = Globals.ResolveUrl(
                            string.IsNullOrEmpty(c.Value)
                                ? "~/images/Flags/none.gif"
                                : $"~/images/Flags/{c.Value}.gif"),
                    }),
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetProfilePropertyLocalization
        /// <summary>Gets profile property localization.</summary>
        /// <param name="portalId">The ID of the portal for which to get the profile properties for.</param>
        /// <param name="cultureCode">The culture code in which to get the category in.</param>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="propertyCategory">The category of the property to get.</param>
        /// <returns>Profile property.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetProfilePropertyLocalization(int? portalId, string cultureCode, string propertyName, string propertyCategory)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                cultureCode = string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode;

				Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): cultureCode → {CultureCode1}", portalId, cultureCode, propertyName, propertyCategory, cultureCode);
                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): language → {Language}", portalId, cultureCode, propertyName, propertyCategory, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var resourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
                var response = new
				Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): resourceFile → {ResourceFile}", portalId, cultureCode, propertyName, propertyCategory, resourceFile);
                {
                    Success = true,
                    PropertyLocalization = new
					Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): Success → {Success}", portalId, cultureCode, propertyName, propertyCategory, Success);
					Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): PropertyLocalization → {PropertyLocalization}", portalId, cultureCode, propertyName, propertyCategory, PropertyLocalization);
                    {
                        Language = cultureCode,
                        PropertyName = Localization.GetString("ProfileProperties_" + propertyName, resourceFile, cultureCode) ?? string.Empty,
                        PropertyHelp = Localization.GetString("ProfileProperties_" + propertyName + ".Help", resourceFile, cultureCode) ?? string.Empty,
                        PropertyRequired = Localization.GetString("ProfileProperties_" + propertyName + ".Required", resourceFile, cultureCode) ?? string.Empty,
                        PropertyValidation = Localization.GetString("ProfileProperties_" + propertyName + ".Validation", resourceFile, cultureCode) ?? string.Empty,
                        CategoryName = Localization.GetString("ProfileProperties_" + propertyCategory + ".Header", resourceFile, cultureCode) ?? string.Empty,
                    },
                };
						Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): Language → {Language}", portalId, cultureCode, propertyName, propertyCategory, Language);
						Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): PropertyName → {PropertyName1}", portalId, cultureCode, propertyName, propertyCategory, PropertyName);
						Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): PropertyHelp → {PropertyHelp}", portalId, cultureCode, propertyName, propertyCategory, PropertyHelp);
						Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): PropertyRequired → {PropertyRequired}", portalId, cultureCode, propertyName, propertyCategory, PropertyRequired);
						Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): PropertyValidation → {PropertyValidation}", portalId, cultureCode, propertyName, propertyCategory, PropertyValidation);
						Logger.LogDebug("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): CategoryName → {CategoryName}", portalId, cultureCode, propertyName, propertyCategory, CategoryName);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory})", portalId, cultureCode, propertyName, propertyCategory);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateProfilePropertyLocalization
        /// <summary>Updates profile property localization.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateProfilePropertyLocalization(UpdateProfilePropertyLocalizationRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateProfilePropertyLocalization(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocale(pid, request.Language);
                if (language == null)
				Logger.LogDebug("UpdateProfilePropertyLocalization(request={Request}): language → {Language}", request, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), request.Language));
                }

                this.controller.SaveLocalizedKeys(
                    pid,
                    request.PropertyName,
                    request.PropertyCategory,
                    request.Language,
                    request.PropertyNameString,
                    request.PropertyHelpString,
                    request.PropertyRequiredString,
                    request.PropertyValidationString,
                    request.CategoryNameString);
                DataCache.ClearCache();

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateProfilePropertyLocalization(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddProfileProperty
        /// <summary>Creates profile property.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage AddProfileProperty(UpdateProfilePropertyRequest request)
        {
			Logger.LogDebug(">>> Entering AddProfileProperty(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var property = new ProfilePropertyDefinition(pid)
                {
                    DataType = request.DataType,
                    DefaultValue = request.DefaultValue,
                    PropertyCategory = request.PropertyCategory,
                    PropertyName = request.PropertyName,
                    ReadOnly = request.ReadOnly,
                    Required = !Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID) && request.Required,
                    ValidationExpression = request.ValidationExpression,
                    ViewOrder = request.ViewOrder,
                    Visible = request.Visible,
                    Length = request.Length,
                    DefaultVisibility = (UserVisibilityMode)request.DefaultVisibility,
                };

                if (this.ValidateProperty(property, out var httpPropertyValidationError))
                {
                    var propertyId = ProfileController.AddPropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, property);
                    if (propertyId < Null.NullInteger)
					Logger.LogDebug("AddProfileProperty(request={Request}): propertyId → {PropertyId}", request, propertyId);
                    {
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Localization.GetString("DuplicateName", Components.Constants.Constants.LocalResourcesFile));
                    }
                    else
                    {
                        DataCache.ClearDefinitionsCache(pid);
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                    }
                }
                else
                {
                    return httpPropertyValidationError;
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AddProfileProperty(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateProfileProperty
        /// <summary>Updates profile property.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateProfileProperty(UpdateProfilePropertyRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateProfileProperty(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var definitionId = request.PropertyDefinitionId ?? Null.NullInteger;

                if (definitionId != Null.NullInteger)
                {
                    var property = new ProfilePropertyDefinition(pid)
                    {
                        PropertyDefinitionId = definitionId,
                        DataType = request.DataType,
                        DefaultValue = request.DefaultValue,
                        PropertyCategory = request.PropertyCategory,
                        PropertyName = request.PropertyName,
                        ReadOnly = request.ReadOnly,
                        Required = request.Required,
                        ValidationExpression = request.ValidationExpression,
                        ViewOrder = request.ViewOrder,
                        Visible = request.Visible,
                        Length = request.Length,
                        DefaultVisibility = (UserVisibilityMode)request.DefaultVisibility,
                    };

                    HttpResponseMessage httpPropertyValidationError;

                    if (this.ValidateProperty(property, out httpPropertyValidationError))
                    {
                        ProfileController.UpdatePropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, property);
                        DataCache.ClearDefinitionsCache(pid);
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                    }
                    else
                    {
                        return httpPropertyValidationError;
                    }
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateProfileProperty(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/SwapProfilePropertyOrders
        /// <summary>Moves profile property.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateProfilePropertyOrders(UpdateProfilePropertyOrdersRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateProfilePropertyOrders(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                for (int i = 0; i <= request.Properties.Length - 1; i++)
                {
                    var profileProperty = ProfileController.GetPropertyDefinition(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, request.Properties[i].PropertyDefinitionId.Value, pid);
                    if (profileProperty.ViewOrder != i)
					Logger.LogDebug("UpdateProfilePropertyOrders(request={Request}): profileProperty → {ProfileProperty}", request, profileProperty);
                    {
                        profileProperty.ViewOrder = i;
						Logger.LogDebug("UpdateProfilePropertyOrders(request={Request}): profileProperty.ViewOrder → {Arg1}", request, profileProperty.ViewOrder);
                        ProfileController.UpdatePropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, profileProperty);
                    }
                }

                DataCache.ClearDefinitionsCache(pid);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateProfilePropertyOrders(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteProfileProperty
        /// <summary>Deletes profile property.</summary>
        /// <param name="propertyId">The property ID.</param>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage DeleteProfileProperty(int propertyId, int? portalId)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var propertyDefinition = ProfileController.GetPropertyDefinition(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, propertyId, pid);

                if (!CanDeleteProperty(propertyDefinition))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ForbiddenDelete");
                }

                ProfileController.DeletePropertyDefinition(this.eventLogger, this.portalController, this.appStatus, this.portalGroupController, propertyDefinition);
                DataCache.ClearDefinitionsCache(pid);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetUrlMappingSettings
        /// <summary>Gets URL mapping settings.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>Url mapping settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetUrlMappingSettings(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetUrlMappingSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(pid);
                string portalAliasMapping;
				Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): settings → {Settings}", portalId, settings);
                if (settings.TryGetValue("PortalAliasMapping", out portalAliasMapping))
                {
                    if (string.IsNullOrEmpty(portalAliasMapping))
                    {
                        portalAliasMapping = "CANONICALURL";
						Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): portalAliasMapping → {PortalAliasMapping}", portalId, portalAliasMapping);
                    }
                }
                else
                {
                    portalAliasMapping = "CANONICALURL";
                }

                var portalAliasMappingModes = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(
                        Localization.GetString("Canonical", Components.Constants.Constants.LocalResourcesFile),
                        "CANONICALURL"),
                    new KeyValuePair<string, string>(Localization.GetString("Redirect", Components.Constants.Constants.LocalResourcesFile), "REDIRECT"),
                    new KeyValuePair<string, string>(Localization.GetString("None", Components.Constants.Constants.LocalResourcesFile), "NONE"),
                };

                var response = new
                {
                    Success = true,
                    Settings = new
					Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): Success → {Success}", portalId, Success);
					Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): Settings → {Settings}", portalId, Settings);
                    {
                        PortalId = pid,
                        PortalAliasMapping = portalAliasMapping,
                        AutoAddPortalAliasEnabled = !(PortalController.Instance.GetPortals().Count > 1),
                        AutoAddPortalAlias = PortalController.Instance.GetPortals().Count <= 1 && this.hostSettingsService.GetBoolean("AutoAddPortalAlias"),
                    },
                    PortalAliasMappingModes = portalAliasMappingModes,
                };
						Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): PortalId → {PortalId1}", portalId, PortalId);
						Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): PortalAliasMapping → {PortalAliasMapping}", portalId, PortalAliasMapping);
						Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): AutoAddPortalAliasEnabled → {AutoAddPortalAliasEnabled}", portalId, AutoAddPortalAliasEnabled);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetUrlMappingSettings(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateUrlMappingSettings
        /// <summary>Updates URL mapping settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateUrlMappingSettings(UpdateUrlMappingSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateUrlMappingSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                PortalController.UpdatePortalSetting(this.portalController, pid, "PortalAliasMapping", request.PortalAliasMapping, false);
                this.hostSettingsService.Update("AutoAddPortalAlias", request.AutoAddPortalAlias ? "Y" : "N", true);

                DataCache.ClearPortalCache(pid, false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateUrlMappingSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/getSiteAliases
        /// <summary>Gets site aliases.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>site aliases.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSiteAliases(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetSiteAliases(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);

				Logger.LogDebug("GetSiteAliases(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var aliases = this.portalAliasService.GetPortalAliasesByPortalId(pid).Select(a => new
                {
                    PortalAliasID = a.PortalAliasId,
                    HTTPAlias = a.HttpAlias,
                    BrowserType = a.BrowserType.ToString(),
                    a.Skin,
                    a.IsPrimary,
                    a.CultureCode,
                    Deletable = a.PortalAliasId != ((IPortalAliasInfo)this.PortalSettings.PortalAlias).PortalAliasId && !a.IsPrimary,
                    Editable = a.PortalAliasId != ((IPortalAliasInfo)this.PortalSettings.PortalAlias).PortalAliasId,
                });
					Logger.LogDebug("GetSiteAliases(portalId={PortalId}): PortalAliasID → {PortalAliasID}", portalId, PortalAliasID);
					Logger.LogDebug("GetSiteAliases(portalId={PortalId}): HTTPAlias → {HTTPAlias}", portalId, HTTPAlias);
					Logger.LogDebug("GetSiteAliases(portalId={PortalId}): BrowserType → {BrowserType}", portalId, BrowserType);

                var response = new
                {
                    PortalId = pid,
                    PortalAliases = aliases,
                    BrowserTypes = Enum.GetNames(typeof(DotNetNuke.Abstractions.Urls.BrowserTypes)),
                    Languages = LocaleController.Instance.GetLocales(pid).Select(l => new
                    {
					Logger.LogDebug("GetSiteAliases(portalId={PortalId}): PortalId → {PortalId1}", portalId, PortalId);
					Logger.LogDebug("GetSiteAliases(portalId={PortalId}): PortalAliases → {PortalAliases}", portalId, PortalAliases);
					Logger.LogDebug("GetSiteAliases(portalId={PortalId}): BrowserTypes → {BrowserTypes}", portalId, BrowserTypes);
                        l.Key,
                        Value = l.Key,
                    }),
                    Skins = SkinController.GetSkins(portal, SkinController.RootSkin, SkinScope.All),
                };
						Logger.LogDebug("GetSiteAliases(portalId={PortalId}): Value → {Value}", portalId, Value);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetSiteAliases(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetSiteAlias
        /// <summary>Gets site alias by id.</summary>
        /// <param name="portalAliasId">The portal alias ID.</param>
        /// <returns>site alias.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSiteAlias(int portalAliasId)
        {
			Logger.LogDebug(">>> Entering GetSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
            try
            {
                var alias = this.portalAliasService.GetPortalAliasByPortalAliasId(portalAliasId);
                if (!this.UserInfo.IsSuperUser && alias.PortalId != this.PortalId)
				Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): alias → {Alias}", portalAliasId, alias);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var response = new
                {
                    Success = true,
                    PortalAlias = new
					Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): Success → {Success}", portalAliasId, Success);
					Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): PortalAlias → {PortalAlias}", portalAliasId, PortalAlias);
                    {
                        PortalID = alias.PortalId,
                        PortalAliasID = alias.PortalAliasId,
                        HTTPAlias = alias.HttpAlias,
                        BrowserType = alias.BrowserType.ToString(),
                        alias.Skin,
                        alias.IsPrimary,
                        alias.CultureCode,
                    },
                };
						Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): PortalID → {PortalID}", portalAliasId, PortalID);
						Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): PortalAliasID → {PortalAliasID}", portalAliasId, PortalAliasID);
						Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): HTTPAlias → {HTTPAlias}", portalAliasId, HTTPAlias);
						Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): BrowserType → {BrowserType}", portalAliasId, BrowserType);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddSiteAlias
        /// <summary>Adds site alias.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage AddSiteAlias(UpdateSiteAliasRequest request)
        {
			Logger.LogDebug(">>> Entering AddSiteAlias(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string strAlias = request.HTTPAlias;
                if (!string.IsNullOrEmpty(strAlias))
				Logger.LogDebug("AddSiteAlias(request={Request}): strAlias → {StrAlias}", request, strAlias);
                {
                    strAlias = strAlias.Trim();
                }

                if (this.IsHttpAliasValid(strAlias))
                {
                    var aliases = this.portalAliasService.GetPortalAliases();
                    if (aliases.ContainsKey(strAlias))
					Logger.LogDebug("AddSiteAlias(request={Request}): aliases → {Aliases}", request, aliases);
                    {
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Localization.GetString("DuplicateAlias", Components.Constants.Constants.LocalResourcesFile));
                    }

                    if (!Enum.TryParse(request.BrowserType, out DotNetNuke.Abstractions.Urls.BrowserTypes browser))
                    {
                        browser = DotNetNuke.Abstractions.Urls.BrowserTypes.Normal;
						Logger.LogDebug("AddSiteAlias(request={Request}): browser → {Browser}", request, browser);
                    }

                    IPortalAliasInfo portalAlias = new PortalAliasInfo
                    {
                        Skin = request.Skin,
                        CultureCode = request.CultureCode,
                        IsPrimary = request.IsPrimary,
                    };
                    portalAlias.PortalId = pid;
					Logger.LogDebug("AddSiteAlias(request={Request}): portalAlias.PortalId → {Arg1}", request, portalAlias.PortalId);
					Logger.LogDebug("AddSiteAlias(request={Request}): portalAlias.HttpAlias → {Arg1}", request, portalAlias.HttpAlias);
					Logger.LogDebug("AddSiteAlias(request={Request}): portalAlias.BrowserType → {Arg1}", request, portalAlias.BrowserType);
                    portalAlias.HttpAlias = strAlias;
                    portalAlias.BrowserType = browser;

                    this.portalAliasService.AddPortalAlias(portalAlias);
                }
                else
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("InvalidAlias", Components.Constants.Constants.LocalResourcesFile));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AddSiteAlias(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateSiteAlias
        /// <summary>Updates site alias.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSiteAlias(UpdateSiteAliasRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateSiteAlias(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string strAlias = request.HTTPAlias;
                if (!string.IsNullOrEmpty(strAlias))
				Logger.LogDebug("UpdateSiteAlias(request={Request}): strAlias → {StrAlias}", request, strAlias);
                {
                    strAlias = strAlias.Trim();
                }

                if (this.IsHttpAliasValid(strAlias))
                {
                    if (!Enum.TryParse(request.BrowserType, out DotNetNuke.Abstractions.Urls.BrowserTypes browser))
                    {
                        browser = DotNetNuke.Abstractions.Urls.BrowserTypes.Normal;
						Logger.LogDebug("UpdateSiteAlias(request={Request}): browser → {Browser}", request, browser);
                    }

                    if (request.PortalAliasID != null)
                    {
                        IPortalAliasInfo portalAlias = new PortalAliasInfo
                        {
                            Skin = request.Skin,
                            CultureCode = request.CultureCode,
                            IsPrimary = request.IsPrimary,
                        };
                        portalAlias.PortalId = pid;
						Logger.LogDebug("UpdateSiteAlias(request={Request}): portalAlias.PortalId → {Arg1}", request, portalAlias.PortalId);
						Logger.LogDebug("UpdateSiteAlias(request={Request}): portalAlias.PortalAliasId → {Arg1}", request, portalAlias.PortalAliasId);
						Logger.LogDebug("UpdateSiteAlias(request={Request}): portalAlias.HttpAlias → {Arg1}", request, portalAlias.HttpAlias);
						Logger.LogDebug("UpdateSiteAlias(request={Request}): portalAlias.BrowserType → {Arg1}", request, portalAlias.BrowserType);
                        portalAlias.PortalAliasId = request.PortalAliasID.Value;
                        portalAlias.HttpAlias = strAlias;
                        portalAlias.BrowserType = browser;

                        this.portalAliasService.UpdatePortalAlias(portalAlias);
                    }
                    else
                    {
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Localization.GetString("InvalidAlias", Components.Constants.Constants.LocalResourcesFile));
                    }
                }
                else
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("InvalidAlias", Components.Constants.Constants.LocalResourcesFile));
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateSiteAlias(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteSiteAlias
        /// <summary>Deletes site alias.</summary>
        /// <param name="portalAliasId">The portal alias ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteSiteAlias(int portalAliasId)
        {
			Logger.LogDebug(">>> Entering DeleteSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
            try
            {
                var portalAlias = this.portalAliasService.GetPortalAliasByPortalAliasId(portalAliasId);
                if (!this.UserInfo.IsSuperUser && portalAlias.PortalId != this.PortalId)
				Logger.LogDebug("DeleteSiteAlias(portalAliasId={PortalAliasId}): portalAlias → {PortalAlias}", portalAliasId, portalAlias);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                this.portalAliasService.DeletePortalAlias(portalAlias);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting DeleteSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/SetPrimarySiteAlias
        /// <summary>Sets primary site alias.</summary>
        /// <param name="portalAliasId">The portal alias ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SetPrimarySiteAlias(int portalAliasId)
        {
			Logger.LogDebug(">>> Entering SetPrimarySiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
            try
            {
                var alias = this.portalAliasService.GetPortalAliasByPortalAliasId(portalAliasId);
                if (!this.UserInfo.IsSuperUser && alias.PortalId != this.PortalId)
				Logger.LogDebug("SetPrimarySiteAlias(portalAliasId={PortalAliasId}): alias → {Alias}", portalAliasId, alias);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                IPortalAliasInfo portalAlias = new PortalAliasInfo { IsPrimary = true, };
                portalAlias.PortalId = alias.PortalId;
				Logger.LogDebug("SetPrimarySiteAlias(portalAliasId={PortalAliasId}): portalAlias.PortalId → {Arg1}", portalAliasId, portalAlias.PortalId);
				Logger.LogDebug("SetPrimarySiteAlias(portalAliasId={PortalAliasId}): portalAlias.PortalAliasId → {Arg1}", portalAliasId, portalAlias.PortalAliasId);
                portalAlias.PortalAliasId = portalAliasId;

                this.portalAliasService.UpdatePortalAlias(portalAlias);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting SetPrimarySiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetListInfo
        /// <summary>Gets list info.</summary>
        /// <param name="listName">The list name.</param>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>list entries.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetListInfo(string listName, int? portalId)
        {
			Logger.LogDebug(">>> Entering GetListInfo(listName={ListName},portalId={PortalId})", listName, portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var entries = this.listController.GetListEntryInfoItems(listName, string.Empty, pid);
                var response = new
				Logger.LogDebug("GetListInfo(listName={ListName},portalId={PortalId}): entries → {Entries}", listName, portalId, entries);
                {
                    Success = true,
                    this.listController.GetListInfo(listName, string.Empty, pid)?.EnableSortOrder,
                    Entries = entries.Select(t => new
                    {
					Logger.LogDebug("GetListInfo(listName={ListName},portalId={PortalId}): Success → {Success}", listName, portalId, Success);
                        t.EntryID,
                        t.Text,
                        t.Value,
                        t.SortOrder,
                    }),
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetListInfo(listName={ListName},portalId={PortalId})", listName, portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateListEntry
        /// <summary>Adds/Updates list entry.</summary>
        /// <param name="request">The request body.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateListEntry(UpdateListEntryRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateListEntry(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var entry = new ListEntryInfo
                {
                    DefinitionID = Null.NullInteger,
                    PortalID = pid,
                    ListName = request.ListName,
                    Value = request.Value,
                    Text = request.Text,
                    SortOrder = request.EnableSortOrder ? 1 : 0,
                };

                if (request.EntryId.HasValue)
                {
                    entry.EntryID = request.EntryId.Value;
					Logger.LogDebug("UpdateListEntry(request={Request}): entry.EntryID → {Arg1}", request, entry.EntryID);
                    this.listController.UpdateListEntry(entry);
                }
                else
                {
                    this.listController.AddListEntry(entry);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateListEntry(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteListEntry
        /// <summary>Deletes list entry.</summary>
        /// <param name="entryId">The entry ID.</param>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage DeleteListEntry([FromUri] int entryId, [FromUri] int? portalId)
        {
			Logger.LogDebug(">>> Entering DeleteListEntry(entryId={EntryId},portalId={PortalId})", entryId, portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                this.listController.DeleteListEntryByID(entryId, true);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting DeleteListEntry(entryId={EntryId},portalId={PortalId})", entryId, portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateListEntryOrders
        /// <summary>Updates list entry sort order.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateListEntryOrders(UpdateListEntryOrdersRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateListEntryOrders(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                for (var i = 0; i <= request.Entries.Length - 1; i++)
                {
                    if (request.Entries[i].EntryId.HasValue)
                    {
                        var entry = this.listController.GetListEntryInfo(request.Entries[i].EntryId.Value);
                        if (entry.SortOrder == i + 1)
						Logger.LogDebug("UpdateListEntryOrders(request={Request}): entry → {Entry}", request, entry);
                        {
                            continue;
                        }

                        if (entry.SortOrder > i + 1)
                        {
                            var j = entry.SortOrder - i - 1;
                            while (j > 0)
                            {
                                this.listController.UpdateListSortOrder(entry.EntryID, true);
                                j--;
                            }
                        }
                        else
                        {
                            var j = i + 1 - entry.SortOrder;
                            while (j > 0)
                            {
                                this.listController.UpdateListSortOrder(entry.EntryID, false);
                                j--;
                            }
                        }
                    }
                }

                DataCache.ClearListsCache(pid);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateListEntryOrders(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetPrivacySettings
        /// <summary>Gets messaging settings.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>privacy settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetPrivacySettings(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetPrivacySettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("GetPrivacySettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
				Logger.LogDebug("GetPrivacySettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
					Logger.LogDebug("GetPrivacySettings(portalId={PortalId}): Settings → {Settings}", portalId, Settings);
                    {
                        ((IPortalInfo)portal).PortalId,
                        portal.CultureCode,
                        portalSettings.ShowCookieConsent,
                        portalSettings.CookieMoreLink,
                        CheckUpgrade = this.hostSettingsService.GetBoolean("CheckUpgrade", true),
                        portalSettings.DataConsentActive,
                        DataConsentResetTerms = false,
                        DataConsentConsentRedirect = this.TabSanitizer(portalSettings.DataConsentConsentRedirect, pid)?.TabID,
                        DataConsentConsentRedirectName = this.TabSanitizer(portalSettings.DataConsentConsentRedirect, pid)?.TabName,
                        DataConsentUserDeleteAction = (int)portalSettings.DataConsentUserDeleteAction,
                        this.PortalSettings.DataConsentDelay,
                        this.PortalSettings.DataConsentDelayMeasurement,
                    },
                });
						Logger.LogDebug("GetPrivacySettings(portalId={PortalId}): CheckUpgrade → {CheckUpgrade}", portalId, CheckUpgrade);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetPrivacySettings(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdatePrivacySettings
        /// <summary>Updates privacy settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdatePrivacySettings(UpdatePrivacySettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdatePrivacySettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                PortalController.UpdatePortalSetting(this.portalController, pid, "ShowCookieConsent", request.ShowCookieConsent.ToString(), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "CookieMoreLink", request.CookieMoreLink, false, request.CultureCode);
                if (this.UserInfo.IsSuperUser)
                {
                    this.hostSettingsService.Update("CheckUpgrade", request.CheckUpgrade ? "Y" : "N", false);
                }

                PortalController.UpdatePortalSetting(this.portalController, pid, "DataConsentActive", request.DataConsentActive.ToString(), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "DataConsentConsentRedirect", this.ValidateTabId(request.DataConsentConsentRedirect, pid).ToString(CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "DataConsentUserDeleteAction", request.DataConsentUserDeleteAction.ToString(CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "DataConsentDelay", request.DataConsentDelay.ToString(CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.portalController, pid, "DataConsentDelayMeasurement", request.DataConsentDelayMeasurement, false);
                DataCache.ClearCache();

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdatePrivacySettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/ResetTermsAgreement
        /// <summary>Resets terms and conditions agreements.</summary>
        /// <param name="request">The reset request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage ResetTermsAgreement(ResetTermsAgreementRequest request)
        {
			Logger.LogDebug(">>> Entering ResetTermsAgreement(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                UserController.ResetTermsAgreement(pid);
                PortalController.UpdatePortalSetting(this.portalController, pid, "DataConsentTermsLastChange", DateTime.Now.ToString("O", CultureInfo.InvariantCulture), true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting ResetTermsAgreement(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetBasicSearchSettings
        /// <summary>Gets basic search settings.</summary>
        /// <returns>basic search settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetBasicSearchSettings()
        {
			Logger.LogDebug(">>> Entering GetBasicSearchSettings()");
            try
            {
                dynamic settings = new ExpandoObject();
                settings.MinWordLength = this.hostSettingsService.GetInteger("Search_MinKeyWordLength", 3);
				Logger.LogDebug("GetBasicSearchSettings(): settings → {Settings}", settings);
                settings.MaxWordLength = this.hostSettingsService.GetInteger("Search_MaxKeyWordLength", 255);
                settings.AllowLeadingWildcard = this.hostSettingsService.GetString("Search_AllowLeadingWildcard", "N") == "Y";
                settings.SearchCustomAnalyzer = this.hostSettingsService.GetString("Search_CustomAnalyzer", string.Empty);
                settings.TitleBoost = this.hostSettingsService.GetInteger(SearchTitleBoostSetting, DefaultSearchTitleBoost);
                settings.TagBoost = this.hostSettingsService.GetInteger(SearchTagBoostSetting, DefaultSearchTagBoost);
                settings.ContentBoost = this.hostSettingsService.GetInteger(SearchContentBoostSetting, DefaultSearchContentBoost);
                settings.DescriptionBoost = this.hostSettingsService.GetInteger(SearchDescriptionBoostSetting, DefaultSearchDescriptionBoost);
                settings.AuthorBoost = this.hostSettingsService.GetInteger(SearchAuthorBoostSetting, DefaultSearchAuthorBoost);
                settings.SearchIndexPath = Path.Combine(this.appStatus.ApplicationMapPath, this.hostSettingsService.GetString("SearchFolder", @"App_Data\Search"));
                settings.MaxResultPerPage = this.hostSettingsService.GetInteger("Search_MaxResultPerPage", 100);

                SearchStatistics searchStatistics = GetSearchStatistics();
                if (searchStatistics != null)
				Logger.LogDebug("GetBasicSearchSettings(): searchStatistics → {SearchStatistics}", searchStatistics);
                {
                    settings.SearchIndexDbSize = (searchStatistics.IndexDbSize / 1024f / 1024f).ToString("N", CultureInfo.InvariantCulture) + " MB";
                    settings.SearchIndexLastModifedOn = DateUtils.CalculateDateForDisplay(searchStatistics.LastModifiedOn);
                    settings.SearchIndexTotalActiveDocuments = searchStatistics.TotalActiveDocuments.ToString(CultureInfo.InvariantCulture);
                    settings.SearchIndexTotalDeletedDocuments = searchStatistics.TotalDeletedDocuments.ToString(CultureInfo.InvariantCulture);
                }

                var response = new
                {
                    Success = true,
                    Settings = settings,
                    SearchCustomAnalyzers = this.controller.GetAvailableAnalyzers(),
                };
					Logger.LogDebug("GetBasicSearchSettings(): Success → {Success}", Success);
					Logger.LogDebug("GetBasicSearchSettings(): Settings → {Settings}", Settings);
					Logger.LogDebug("GetBasicSearchSettings(): SearchCustomAnalyzers → {SearchCustomAnalyzers}", SearchCustomAnalyzers);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetBasicSearchSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateBasicSearchSettings
        /// <summary>Updates basic search settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateBasicSearchSettings(UpdateBasicSearchSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateBasicSearchSettings(request={Request})", request);
            try
            {
                if (request.MinWordLength == Null.NullInteger || request.MinWordLength == 0)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("valIndexWordMinLengthRequired.Error", Components.Constants.Constants.LocalResourcesFile));
                }
                else if (request.MaxWordLength == Null.NullInteger || request.MaxWordLength == 0)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("valIndexWordMaxLengthRequired.Error", Components.Constants.Constants.LocalResourcesFile));
                }
                else if (request.MinWordLength >= request.MaxWordLength)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("valIndexWordMaxLengthRequired.Error", Components.Constants.Constants.LocalResourcesFile));
                }

                if (request.MaxResultPerPage == Null.NullInteger || request.MaxResultPerPage == 0)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        Localization.GetString("valMaxResultPerPageRequired.Error", Components.Constants.Constants.LocalResourcesFile));
                }

                var oldMinLength = this.hostSettingsService.GetInteger("Search_MinKeyWordLength", 3);
                if (request.MinWordLength != oldMinLength)
				Logger.LogDebug("UpdateBasicSearchSettings(request={Request}): oldMinLength → {OldMinLength}", request, oldMinLength);
                {
                    this.hostSettingsService.Update("Search_MinKeyWordLength", request.MinWordLength.ToString(CultureInfo.InvariantCulture));
                }

                var oldMaxResultPerPage = this.hostSettingsService.GetInteger("Search_MaxResultPerPage", 100);
                if (request.MaxResultPerPage != oldMaxResultPerPage)
				Logger.LogDebug("UpdateBasicSearchSettings(request={Request}): oldMaxResultPerPage → {OldMaxResultPerPage}", request, oldMaxResultPerPage);
                {
                    this.hostSettingsService.Update("Search_MaxResultPerPage", request.MaxResultPerPage.ToString(CultureInfo.InvariantCulture));
                }

                var oldMaxLength = this.hostSettingsService.GetInteger("Search_MaxKeyWordLength", 255);
                if (request.MaxWordLength != oldMaxLength)
				Logger.LogDebug("UpdateBasicSearchSettings(request={Request}): oldMaxLength → {OldMaxLength}", request, oldMaxLength);
                {
                    this.hostSettingsService.Update("Search_MaxKeyWordLength", request.MaxWordLength.ToString(CultureInfo.InvariantCulture));
                }

                this.hostSettingsService.Update("Search_AllowLeadingWildcard", request.AllowLeadingWildcard ? "Y" : "N");
                this.hostSettingsService.Update(SearchTitleBoostSetting, (request.TitleBoost == Null.NullInteger) ? DefaultSearchTitleBoost.ToString(CultureInfo.InvariantCulture) : request.TitleBoost.ToString(CultureInfo.InvariantCulture));
                this.hostSettingsService.Update(SearchTagBoostSetting, (request.TagBoost == Null.NullInteger) ? DefaultSearchTagBoost.ToString(CultureInfo.InvariantCulture) : request.TagBoost.ToString(CultureInfo.InvariantCulture));
                this.hostSettingsService.Update(SearchContentBoostSetting, (request.ContentBoost == Null.NullInteger) ? DefaultSearchContentBoost.ToString(CultureInfo.InvariantCulture) : request.ContentBoost.ToString(CultureInfo.InvariantCulture));
                this.hostSettingsService.Update(SearchDescriptionBoostSetting, (request.DescriptionBoost == Null.NullInteger) ? DefaultSearchDescriptionBoost.ToString(CultureInfo.InvariantCulture) : request.DescriptionBoost.ToString(CultureInfo.InvariantCulture));
                this.hostSettingsService.Update(SearchAuthorBoostSetting, (request.AuthorBoost == Null.NullInteger) ? DefaultSearchAuthorBoost.ToString(CultureInfo.InvariantCulture) : request.AuthorBoost.ToString(CultureInfo.InvariantCulture));

                var oldAnalyzer = this.hostSettingsService.GetString("Search_CustomAnalyzer", string.Empty);
                var newAnalyzer = request.SearchCustomAnalyzer.Trim();
				Logger.LogDebug("UpdateBasicSearchSettings(request={Request}): oldAnalyzer → {OldAnalyzer}", request, oldAnalyzer);
				Logger.LogDebug("UpdateBasicSearchSettings(request={Request}): newAnalyzer → {NewAnalyzer}", request, newAnalyzer);
                if (!oldAnalyzer.Equals(newAnalyzer, StringComparison.Ordinal))
                {
                    this.hostSettingsService.Update("Search_CustomAnalyzer", newAnalyzer);

                    // force the app restart to use new analyzer.
                    Config.Touch(this.appStatus);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateBasicSearchSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/CompactSearchIndex
        /// <summary>Compacts search index.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CompactSearchIndex()
        {
			Logger.LogDebug(">>> Entering CompactSearchIndex()");
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting CompactSearchIndex()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/HostSearchReindex
        /// <summary>Re-index host search.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage HostSearchReindex()
        {
			Logger.LogDebug(">>> Entering HostSearchReindex()");
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(-1);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting HostSearchReindex()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/PortalSearchReindex
        /// <summary>Re-index portal search.</summary>
        /// <param name="portalId">The portal ID (default: the current portal ID).</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage PortalSearchReindex(int? portalId)
        {
			Logger.LogDebug(">>> Entering PortalSearchReindex(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                SearchHelper.Instance.SetSearchReindexRequestTime(pid);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting PortalSearchReindex(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetPortals
        /// <summary>Gets portals.</summary>
        /// <returns>List of portals.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetPortals()
        {
			Logger.LogDebug(">>> Entering GetPortals()");
            try
            {
                var portals = PortalController.Instance.GetPortals().OfType<IPortalInfo>();
                var availablePortals = portals.Select(v => new
                {
				Logger.LogDebug("GetPortals(): portals → {Portals}", portals);
                    PortalID = v.PortalId,
                    v.PortalName,
                    IsCurrentPortal = this.PortalId == v.PortalId,
                }).ToList();
					Logger.LogDebug("GetPortals(): PortalID → {PortalID}", PortalID);

                var response = new
                {
                    Success = true,
                    Results = availablePortals,
                    TotalResults = availablePortals.Count,
                };
					Logger.LogDebug("GetPortals(): Success → {Success}", Success);
					Logger.LogDebug("GetPortals(): Results → {Results}", Results);
					Logger.LogDebug("GetPortals(): TotalResults → {TotalResults}", TotalResults);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetPortals()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetSynonymsGroups
        /// <summary>Gets Synonyms Groups.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>Synonyms Groups.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetSynonymsGroups(int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocaleOrCurrent(pid, cultureCode);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var groups = SearchHelper.Instance.GetSynonymsGroups(pid, string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode);

                var response = new
                {
                    PortalId = pid,
                    CultureCode = language.Code,
                    SynonymsGroups = groups.Select(g => new
                    {
                        g.SynonymsGroupId,
                        g.SynonymsTags,
                    }),
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddSynonymsGroup
        /// <summary>Adds Synonyms Group.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success and the synonyms group ID.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage AddSynonymsGroup(UpdateSynonymsGroupRequest request)
        {
			Logger.LogDebug(">>> Entering AddSynonymsGroup(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("AddSynonymsGroup(request={Request}): language → {Language}", request, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var synonymsGroupId = SearchHelper.Instance.AddSynonymsGroup(request.SynonymsTags, pid, cultureCode, out var duplicateWord);
                if (synonymsGroupId > 0)
				Logger.LogDebug("AddSynonymsGroup(request={Request}): synonymsGroupId → {SynonymsGroupId}", request, synonymsGroupId);
                {
                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        new { Success = true, Id = synonymsGroupId });
                }
                else
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        $"[{duplicateWord}] {Localization.GetString("SynonymsTagDuplicated", Components.Constants.Constants.LocalResourcesFile)}");
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AddSynonymsGroup(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateSynonymsGroup
        /// <summary>Updates Synonyms Group.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateSynonymsGroup(UpdateSynonymsGroupRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateSynonymsGroup(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("UpdateSynonymsGroup(request={Request}): language → {Language}", request, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                if (request.SynonymsGroupID != null)
                {
                    var synonymsGroupId = SearchHelper.Instance.UpdateSynonymsGroup(
                        request.SynonymsGroupID.Value,
                        request.SynonymsTags,
                        pid,
                        cultureCode,
                        out var duplicateWord);
                    if (synonymsGroupId > 0)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                    }
                    else
                    {
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            $"[{duplicateWord}] {Localization.GetString("SynonymsTagDuplicated", Components.Constants.Constants.LocalResourcesFile)}");
                    }
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateSynonymsGroup(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteSynonymsGroup
        /// <summary>Deletes Synonyms Group.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage DeleteSynonymsGroup(UpdateSynonymsGroupRequest request)
        {
			Logger.LogDebug(">>> Entering DeleteSynonymsGroup(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (request.SynonymsGroupID != null)
                {
                    SearchHelper.Instance.DeleteSynonymsGroup(request.SynonymsGroupID.Value, pid, request.CultureCode);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting DeleteSynonymsGroup(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetIgnoreWords
        /// <summary>Gets ignore words.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>ignore words.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetIgnoreWords(int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocaleOrCurrent(pid, cultureCode);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var words = SearchHelper.Instance.GetSearchStopWords(pid, string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode);

                var response = new
                {
                    PortalId = pid,
                    CultureCode = language.Code,
                    StopWordsId = words?.StopWordsId ?? Null.NullInteger,
                    words?.StopWords,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddIgnoreWords
        /// <summary>Adds ignore words.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success and the stop words ID.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage AddIgnoreWords(UpdateIgnoreWordsRequest request)
        {
			Logger.LogDebug(">>> Entering AddIgnoreWords(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("AddIgnoreWords(request={Request}): language → {Language}", request, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var stopWordsId = SearchHelper.Instance.AddSearchStopWords(request.StopWords, pid, cultureCode);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Id = stopWordsId });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AddIgnoreWords(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateIgnoreWords
        /// <summary>Updates ignore words.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateIgnoreWords(UpdateIgnoreWordsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateIgnoreWords(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
                if (language == null)
				Logger.LogDebug("UpdateIgnoreWords(request={Request}): language → {Language}", request, language);
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                SearchHelper.Instance.UpdateSearchStopWords(request.StopWordsId, request.StopWords, pid, cultureCode);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateIgnoreWords(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/DeleteSynonymsGroup
        /// <summary>Deletes Synonyms Group.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage DeleteIgnoreWords(UpdateIgnoreWordsRequest request)
        {
			Logger.LogDebug(">>> Entering DeleteIgnoreWords(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(pid).Code
                    : request.CultureCode;
                SearchHelper.Instance.DeleteSearchStopWords(request.StopWordsId, pid, cultureCode);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting DeleteIgnoreWords(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetLanguageSettings
        /// <summary>Gets language settings.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>language settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetLanguageSettings(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetLanguageSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.Unauthorized,
                        "Authorization has been denied for this request.");
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("GetLanguageSettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
				Logger.LogDebug("GetLanguageSettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

                var languageDisplayModes = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(Localization.GetString("NativeName", Components.Constants.Constants.LocalResourcesFile), "NATIVE"),
                    new KeyValuePair<string, string>(
                        Localization.GetString("EnglishName", Components.Constants.Constants.LocalResourcesFile),
                        "ENGLISH"),
                };

                var settings = new
                {
                    ((IPortalInfo)portal).PortalId,
                    portalSettings.ContentLocalizationEnabled,
                    SystemDefaultLanguage = string.IsNullOrEmpty(Localization.SystemLocale)
                    ? Localization.GetString("NeutralCulture", Localization.GlobalResourceFile)
                    : Localization.GetLocaleName(Localization.SystemLocale, this.GetCultureDropDownType(pid)),
                    SystemDefaultLanguageIcon = Globals.ResolveUrl(string.IsNullOrEmpty(Localization.SystemLocale) ? "~/images/Flags/none.gif" : $"~/images/Flags/{Localization.SystemLocale}.gif"),
                    SiteDefaultLanguage = portalSettings.DefaultLanguage,
                    LanguageDisplayMode = this.GetLanguageDisplayMode(pid),
                    portalSettings.EnableUrlLanguage,
                    portalSettings.EnableBrowserLanguage,
                    portalSettings.AllowUserUICulture,
                    portal.CultureCode,
                    AllowContentLocalization = this.hostSettings.EnableContentLocalization,
                };
					Logger.LogDebug("GetLanguageSettings(portalId={PortalId}): SystemDefaultLanguage → {SystemDefaultLanguage}", portalId, SystemDefaultLanguage);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = settings,
                    Languages = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger)).Select(l => new
                    {
					Logger.LogDebug("GetLanguageSettings(portalId={PortalId}): Settings → {Settings}", portalId, Settings);
                        l.NativeName,
                        l.EnglishName,
                        l.Name,
                        Icon = Globals.ResolveUrl(string.IsNullOrEmpty(l.Name) ? "~/images/Flags/none.gif" : $"~/images/Flags/{l.Name}.gif"),
                    }),
                    LanguageDisplayModes = languageDisplayModes,
                });
						Logger.LogDebug("GetLanguageSettings(portalId={PortalId}): Icon → {Icon}", portalId, Icon);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLanguageSettings(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateLanguageSettings
        /// <summary>Updates language settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateLanguageSettings(UpdateLanguageSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateLanguageSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("UpdateLanguageSettings(request={Request}): portal → {Portal}", request, portal);
				Logger.LogDebug("UpdateLanguageSettings(request={Request}): portalSettings → {PortalSettings}", request, portalSettings);

                PortalController.UpdatePortalSetting(this.portalController, pid, "EnableBrowserLanguage", request.EnableBrowserLanguage.ToString());
                PortalController.UpdatePortalSetting(this.portalController, pid, "AllowUserUICulture", request.AllowUserUICulture.ToString());

                if (!portalSettings.ContentLocalizationEnabled)
                {
                    // first check whether portal default language has changed
                    string newDefaultLanguage = request.SiteDefaultLanguage;
                    if (newDefaultLanguage != portalSettings.DefaultLanguage)
                    {
                        var needToRemoveOldDefaultLanguage = LocaleController.Instance.GetLocales(pid).Count == 1;
                        var oldDefaultLanguage = LocaleController.Instance.GetLocale(pid, portalSettings.DefaultLanguage);
						Logger.LogDebug("UpdateLanguageSettings(request={Request}): needToRemoveOldDefaultLanguage → {NeedToRemoveOldDefaultLanguage}", request, needToRemoveOldDefaultLanguage);
						Logger.LogDebug("UpdateLanguageSettings(request={Request}): oldDefaultLanguage → {OldDefaultLanguage}", request, oldDefaultLanguage);
                        if (!IsLanguageEnabled(pid, newDefaultLanguage))
                        {
                            var language = LocaleController.Instance.GetLocale(pid, newDefaultLanguage);
                            Localization.AddLanguageToPortal(this.eventLogger, pid, language.LanguageId, true);
							Logger.LogDebug("UpdateLanguageSettings(request={Request}): language → {Language}", request, language);
                        }

                        // update portal default language
                        portal.DefaultLanguage = newDefaultLanguage;
						Logger.LogDebug("UpdateLanguageSettings(request={Request}): portal.DefaultLanguage → {Arg1}", request, portal.DefaultLanguage);
                        PortalController.Instance.UpdatePortalInfo(portal);

                        if (needToRemoveOldDefaultLanguage)
                        {
                            Localization.RemoveLanguageFromPortal(this.eventLogger, pid, oldDefaultLanguage.LanguageId);
                        }
                    }

                    PortalController.UpdatePortalSetting(this.portalController, pid, "EnableUrlLanguage", request.EnableUrlLanguage.ToString());
                }

                var oldLanguageDisplayMode = this.GetLanguageDisplayMode(pid);
                if (request.LanguageDisplayMode != oldLanguageDisplayMode)
				Logger.LogDebug("UpdateLanguageSettings(request={Request}): oldLanguageDisplayMode → {OldLanguageDisplayMode}", request, oldLanguageDisplayMode);
                {
                    var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, pid);
                    Personalization.SetProfile(personalization, "LanguageDisplayMode", "ViewType" + pid, request.LanguageDisplayMode);
					Logger.LogDebug("UpdateLanguageSettings(request={Request}): personalization → {Personalization}", request, personalization);
                    this.personalizationController.SaveProfile(personalization);
                }

                if (this.UserInfo.IsSuperUser && this.hostSettings.EnableContentLocalization != request.AllowContentLocalization)
                {
                    this.hostSettingsService.Update("EnableContentLocalization", request.AllowContentLocalization ? "Y" : "N", false);
                    DataCache.ClearCache();
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateLanguageSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetLanguages
        /// <summary>Gets languages.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <returns>languages.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetLanguages(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetLanguages(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("GetLanguages(portalId={PortalId}): portal → {Portal}", portalId, portal);
				Logger.LogDebug("GetLanguages(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

                if (portalSettings.ContentLocalizationEnabled)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Languages = LocaleController.Instance.GetLocales(Null.NullInteger).Values.Select(l => new
                        {
                            l.LanguageId,
                            Icon = Globals.ResolveUrl(
                                string.IsNullOrEmpty(l.Code)
                                    ? "~/images/Flags/none.gif"
								Logger.LogDebug("GetLanguages(portalId={PortalId}): Icon → {Icon}", portalId, Icon);
                                    : $"~/images/Flags/{l.Code}.gif"),
                            l.Code,
                            l.NativeName,
                            l.EnglishName,
                            Enabled = IsLanguageEnabled(pid, l.Code),
                            IsDefault = l.Code == portalSettings.DefaultLanguage,
                            LocalizablePages = this.GetLocalizablePages(pid, l.Code),
                            LocalizedStatus = this.GetLocalizedStatus(portalSettings, l.Code),
                            TranslatedPages = this.GetTranslatedPages(portalSettings, l.Code),
                            TranslatedStatus = this.GetTranslatedStatus(portalSettings, l.Code),
                            Active = IsLanguagePublished(pid, l.Code),
                            IsLocalized = this.IsLocalized(portalSettings, l.Code),
                            PublishedPages = this.GetPublishedLocalizedPages(pid, l.Code),
                        }),
                    });
							Logger.LogInformation("GetLanguages(portalId={PortalId}): Enabled → {Enabled}", portalId, Enabled);
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Languages = LocaleController.Instance.GetLocales(Null.NullInteger).Values.Select(l => new
                        {
                            l.LanguageId,
                            Icon = Globals.ResolveUrl(
                                string.IsNullOrEmpty(l.Code)
                                    ? "~/images/Flags/none.gif"
                                    : $"~/images/Flags/{l.Code}.gif"),
                            l.Code,
                            l.NativeName,
                            l.EnglishName,
                            Enabled = IsLanguageEnabled(pid, l.Code),
                            IsDefault = l.Code == portalSettings.DefaultLanguage,
                        }),
                    });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLanguages(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetLanguage
        /// <summary>Gets language.</summary>
        /// <param name="portalId">The portal ID (default: current portal ID).</param>
        /// <param name="languageId">The language ID or <see langword="null" />.</param>
        /// <returns>language.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetLanguage(int? portalId, int? languageId)
        {
			Logger.LogDebug(">>> Entering GetLanguage(portalId={PortalId},languageId={LanguageId})", portalId, languageId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var lid = languageId ?? Null.NullInteger;
                var portalSettings = new PortalSettings(pid);
                var language = lid != Null.NullInteger ? LocaleController.Instance.GetLocale(lid) : null;
				Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): portalSettings → {PortalSettings}", portalId, languageId, portalSettings);
				Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): language → {Language}", portalId, languageId, language);

                var fallbacks = language != null ? LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger))
                    .Where(l => l.Name != language.Code)
                    .Select(l => new
                    {
                        l.NativeName,
                        l.EnglishName,
                        l.Name,
                        Icon = Globals.ResolveUrl(
                            string.IsNullOrEmpty(l.Name)
                                ? "~/images/Flags/none.gif"
							Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): Icon → {Icon}", portalId, languageId, Icon);
                                : $"~/images/Flags/{l.Name}.gif"),
                    }).ToList() : LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger))
                    .Select(l => new
                    {
                        l.NativeName,
                        l.EnglishName,
                        l.Name,
                        Icon = Globals.ResolveUrl(
                            string.IsNullOrEmpty(l.Name)
                                ? "~/images/Flags/none.gif"
                                : $"~/images/Flags/{l.Name}.gif"),
                    }).ToList();

                fallbacks.Insert(0, new
                {
                    NativeName = Localization.GetString("System_Default", Components.Constants.Constants.LocalResourcesFile),
                    EnglishName = Localization.GetString("System_Default", Components.Constants.Constants.LocalResourcesFile),
                    Name = string.Empty,
                    Icon = Globals.ResolveUrl("~/images/Flags/none.gif"),
                });
					Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): NativeName → {NativeName}", portalId, languageId, NativeName);
					Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): EnglishName → {EnglishName}", portalId, languageId, EnglishName);
					Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): Name → {Name}", portalId, languageId, Name);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Language = language != null ? new
                    {
                        PortalId = pid,
                        language.LanguageId,
                        language.NativeName,
                        language.EnglishName,
                        language.Code,
                        language.Fallback,
                        Enabled = IsLanguageEnabled(pid, language.Code),
                        CanEnableDisable = CanEnableDisable(portalSettings, language.Code),
                        IsDefault = language.Code == portalSettings.DefaultLanguage,
                        Roles = PortalController.GetPortalSetting(this.portalController, $"DefaultTranslatorRoles-{language.Code}", pid, "Administrators"),
                    }
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): PortalId → {PortalId1}", portalId, languageId, PortalId);
                    : new
                    {
                        PortalId = pid,
                        LanguageId = Null.NullInteger,
                        NativeName = string.Empty,
                        EnglishName = string.Empty,
                        Code = string.Empty,
                        Fallback = string.Empty,
                        Enabled = false,
                        CanEnableDisable = false,
                        IsDefault = false,
                        Roles = string.Empty,
                    },
                    SupportedFallbacks = fallbacks,
                });
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): LanguageId → {LanguageId1}", portalId, languageId, LanguageId);
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): Code → {Code}", portalId, languageId, Code);
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): Fallback → {Fallback}", portalId, languageId, Fallback);
						Logger.LogInformation("GetLanguage(portalId={PortalId},languageId={LanguageId}): Enabled → {Enabled}", portalId, languageId, Enabled);
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): CanEnableDisable → {CanEnableDisable}", portalId, languageId, CanEnableDisable);
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): IsDefault → {IsDefault}", portalId, languageId, IsDefault);
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): Roles → {Roles}", portalId, languageId, Roles);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetLanguage(portalId={PortalId},languageId={LanguageId})", portalId, languageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetAllLanguages
        /// <summary>Gets language.</summary>
        /// <returns>all languages.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetAllLanguages()
        {
			Logger.LogDebug(">>> Entering GetAllLanguages()");
            try
            {
                var supportedLanguages = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger));
                var cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
				Logger.LogDebug("GetAllLanguages(): supportedLanguages → {SupportedLanguages}", supportedLanguages);
				Logger.LogDebug("GetAllLanguages(): cultures → {Cultures}", cultures);

                foreach (CultureInfo info in supportedLanguages)
                {
                    string cultureCode = info.Name;
                    CultureInfo culture = cultures.SingleOrDefault(c => c.Name == cultureCode);
                    if (culture != null)
					Logger.LogDebug("GetAllLanguages(): culture → {Culture}", culture);
                    {
                        cultures.Remove(culture);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    FullLanguageList = cultures.Select(c => new
                    {
                        c.NativeName,
                        c.EnglishName,
                        c.Name,
                        Icon = Globals.ResolveUrl(
                            string.IsNullOrEmpty(c.Name)
                                ? "~/images/Flags/none.gif"
							Logger.LogDebug("GetAllLanguages(): Icon → {Icon}", Icon);
                                : $"~/images/Flags/{c.Name}.gif"),
                    }),
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetAllLanguages()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/AddLanguage
        /// <summary>Adds language.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage AddLanguage(UpdateLanguageRequest request)
        {
			Logger.LogDebug(">>> Entering AddLanguage(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocale(request.Code) ?? new Locale { Code = request.Code };
                language.Code = request.Code;
				Logger.LogDebug("AddLanguage(request={Request}): language → {Language}", request, language);
				Logger.LogDebug("AddLanguage(request={Request}): language.Code → {Arg1}", request, language.Code);
				Logger.LogDebug("AddLanguage(request={Request}): language.Fallback → {Arg1}", request, language.Fallback);
                language.Fallback = request.Fallback;
                language.Text = CultureInfo.GetCultureInfo(request.Code).NativeName;
                Localization.SaveLanguage(this.eventLogger, language);

                if (!IsLanguageEnabled(pid, language.Code))
                {
                    Localization.AddLanguageToPortal(this.eventLogger, pid, language.LanguageId, true);
                }

                string roles = $"Administrators;Translator ({language.Code})";
                PortalController.UpdatePortalSetting(this.portalController, pid, $"DefaultTranslatorRoles-{language.Code}", roles);
				Logger.LogDebug("AddLanguage(request={Request}): roles → {Roles}", request, roles);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AddLanguage(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateLanguageRoles
        /// <summary>Updates language security.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateLanguageRoles(UpdateLanguageRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateLanguageRoles(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                PortalController.UpdatePortalSetting(this.portalController, pid, $"DefaultTranslatorRoles-{request.Code}", request.Roles);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateLanguageRoles(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateLanguage
        /// <summary>Updates language.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success with an optional redirect URL.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateLanguage(UpdateLanguageRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateLanguage(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (request.LanguageId != null)
                {
                    var language = LocaleController.Instance.GetLocale(request.LanguageId.Value) ??
                                       LocaleController.Instance.GetLocale(request.Code) ??
                                        new Locale { Code = request.Code };
                    if (this.UserInfo.IsSuperUser)
                    {
                        language.Fallback = request.Fallback;
						Logger.LogDebug("UpdateLanguage(request={Request}): language.Fallback → {Arg1}", request, language.Fallback);
                        language.Text = CultureInfo.GetCultureInfo(language.Code).NativeName;
                        Localization.SaveLanguage(this.eventLogger, language);
                    }

                    Dictionary<string, Locale> enabledLanguages = LocaleController.Instance.GetLocales(pid);
                    var localizedTabs = this.PortalSettings.ContentLocalizationEnabled
						Logger.LogDebug("UpdateLanguage(request={Request}): enabledLanguages → {EnabledLanguages}", request, enabledLanguages);
                        ? this.tabController.GetTabsByPortal(pid).WithCulture(request.Code, false).AsList()
                        : new List<TabInfo>();
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(pid);
                    string redirectUrl = string.Empty;
					Logger.LogDebug("UpdateLanguage(request={Request}): defaultLocale → {DefaultLocale}", request, defaultLocale);
					Logger.LogDebug("UpdateLanguage(request={Request}): redirectUrl → {RedirectUrl}", request, redirectUrl);

                    if (request.Enabled)
                    {
                        if (!enabledLanguages.ContainsKey(request.Code))
                        {
                            // Add language to portal
                            Localization.AddLanguageToPortal(this.eventLogger, pid, language.LanguageId, true);
                        }

                        // restore the tabs and modules
                        foreach (var tab in localizedTabs)
                        {
                            this.tabController.RestoreTab(tab, this.PortalSettings);
                            ModuleController.Instance.GetTabModules(tab.TabID)
                                .Values.ToList()
                                .ForEach(ModuleController.Instance.RestoreModule);
                        }

                        if (LocaleController.Instance.GetLocales(pid).Count == 2)
                        {
                            redirectUrl = this.navigationManager.NavigateURL();
                        }
                    }
                    else
                    {
                        // remove language from portal
                        Localization.RemoveLanguageFromPortal(this.eventLogger, pid, language.LanguageId);

                        // if the disable language is current language, should redirect to default language.
                        if (
                            request.Code.Equals(
                                Thread.CurrentThread.CurrentUICulture.ToString(),
                                StringComparison.OrdinalIgnoreCase) ||
                            LocaleController.Instance.GetLocales(pid).Count == 1)
                        {
                            redirectUrl = this.navigationManager.NavigateURL(
                                this.PortalSettings.ActiveTab.TabID,
                                this.PortalSettings.ActiveTab.IsSuperTab,
                                this.PortalSettings,
                                string.Empty,
                                defaultLocale.Code);
                        }

                        // delete the tabs in this language
                        foreach (var tab in localizedTabs)
                        {
                            tab.DefaultLanguageGuid = Guid.Empty;
                            this.tabController.SoftDeleteTab(tab.TabID, this.PortalSettings);
                        }
                    }

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, RedirectUrl = pid == this.PortalId ? redirectUrl : string.Empty });
                }
                else
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateLanguage(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/VerifyLanguageResourceFiles
        /// <summary>Verifies language resource files.</summary>
        /// <returns>verification results.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage VerifyLanguageResourceFiles()
        {
			Logger.LogDebug(">>> Entering VerifyLanguageResourceFiles()");
            try
            {
                var files = new SortedList();
                Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(Null.NullInteger);
				Logger.LogDebug("VerifyLanguageResourceFiles(): files → {Files}", files);
				Logger.LogDebug("VerifyLanguageResourceFiles(): locales → {Locales}", locales);
                GetResourceFiles(files, HttpContext.Current.Server.MapPath("~\\"));

                var tables = new List<object>();

				Logger.LogDebug("VerifyLanguageResourceFiles(): tables → {Tables}", tables);
                foreach (var locale in locales.Values)
                {
                    var tableMissing = new List<string>();
                    var tableEntries = new List<string>();
					Logger.LogDebug("VerifyLanguageResourceFiles(): tableMissing → {TableMissing}", tableMissing);
					Logger.LogDebug("VerifyLanguageResourceFiles(): tableEntries → {TableEntries}", tableEntries);
                    var tableObsolete = new List<string>();
                    var tableOld = new List<string>();
					Logger.LogDebug("VerifyLanguageResourceFiles(): tableObsolete → {TableObsolete}", tableObsolete);
					Logger.LogDebug("VerifyLanguageResourceFiles(): tableOld → {TableOld}", tableOld);
                    var tableDuplicate = new List<string>();
                    var tableError = new List<string>();
					Logger.LogDebug("VerifyLanguageResourceFiles(): tableDuplicate → {TableDuplicate}", tableDuplicate);
					Logger.LogDebug("VerifyLanguageResourceFiles(): tableError → {TableError}", tableError);

                    foreach (DictionaryEntry file in files)
                    {
                        if (!File.Exists(this.ResourceFile(file.Key.ToString(), locale.Code)))
                        {
                            tableMissing.Add(
                                this.ResourceFile(file.Key.ToString(), locale.Code)
                                    .Replace(HttpContext.Current.Server.MapPath("~"), string.Empty));
                        }
                        else
                        {
                            var dsDef = new DataSet();
                            var dsRes = new DataSet();
							Logger.LogDebug("VerifyLanguageResourceFiles(): dsDef → {DsDef}", dsDef);
							Logger.LogDebug("VerifyLanguageResourceFiles(): dsRes → {DsRes}", dsRes);

                            try
                            {
                                using var xmlReader = XmlReader.Create(file.Key.ToString());
                                dsDef.ReadXml(xmlReader);
								Logger.LogDebug("VerifyLanguageResourceFiles(): xmlReader → {XmlReader}", xmlReader);
                            }
                            catch
                            {
								Logger.LogError("Exception in VerifyLanguageResourceFiles(): {Arg0}", ex.Message);
                                tableError.Add(file.Key.ToString().Replace(HttpContext.Current.Server.MapPath("~"), string.Empty));
                                dsDef = null;
                            }

                            try
                            {
                                using var xmlReader = XmlReader.Create(this.ResourceFile(file.Key.ToString(), locale.Code));
                                dsRes.ReadXml(xmlReader);
                            }
                            catch
                            {
                                if (locale.Text != Localization.SystemLocale)
                                {
                                    tableError.Add(this.ResourceFile(file.Key.ToString(), locale.Code).Replace(HttpContext.Current.Server.MapPath("~"), string.Empty));
                                    dsRes = null;
                                }
                            }

                            if (dsRes != null && dsDef != null && dsRes.Tables["data"] != null && dsDef.Tables["data"] != null)
                            {
                                var dtDef = dsDef.Tables["data"];
                                dtDef.TableName = "default";
								Logger.LogDebug("VerifyLanguageResourceFiles(): dtDef → {DtDef}", dtDef);
								Logger.LogDebug("VerifyLanguageResourceFiles(): dtDef.TableName → {Arg0}", dtDef.TableName);
                                var dtRes = dsRes.Tables["data"].Copy();
                                dtRes.TableName = "localized";
								Logger.LogDebug("VerifyLanguageResourceFiles(): dtRes → {DtRes}", dtRes);
                                dsDef.Tables.Add(dtRes);

                                // Check for duplicate entries in localized file
                                try
                                {
                                    // if this fails-> file contains duplicates
                                    var c = new UniqueConstraint("uniqueness", dtRes.Columns["name"]);
                                    dtRes.Constraints.Add(c);
									Logger.LogDebug("VerifyLanguageResourceFiles(): c → {C}", c);
                                    dtRes.Constraints.Remove("uniqueness");
                                }
                                catch
                                {
									Logger.LogError("Exception in VerifyLanguageResourceFiles(): {Arg0}", ex.Message);
                                    tableDuplicate.Add(this.ResourceFile(file.Key.ToString(), locale.Code).Replace(HttpContext.Current.Server.MapPath("~"), string.Empty));
                                }

                                // Check for missing entries in localized file
                                try
                                {
                                    // if this fails-> some entries in System default file are not found in Resource file
                                    dsDef.Relations.Add("missing", dtRes.Columns["name"], dtDef.Columns["name"]);
                                }
                                catch
                                {
                                    tableEntries.Add(this.ResourceFile(file.Key.ToString(), locale.Code).Replace(HttpContext.Current.Server.MapPath("~"), string.Empty));
                                }
                                finally
                                {
                                    dsDef.Relations.Remove("missing");
                                }

                                // Check for obsolete entries in localized file
                                try
                                {
                                    // if this fails-> some entries in Resource File are not found in System default
                                    dsDef.Relations.Add("obsolete", dtDef.Columns["name"], dtRes.Columns["name"]);
                                }
                                catch
                                {
                                    tableObsolete.Add(this.ResourceFile(file.Key.ToString(), locale.Code).Replace(HttpContext.Current.Server.MapPath("~"), string.Empty));
                                }
                                finally
                                {
                                    dsDef.Relations.Remove("obsolete");
                                }

                                // Check older files
                                var resFile = new FileInfo(this.ResourceFile(file.Key.ToString(), locale.Code));
                                if (((FileInfo)file.Value).LastWriteTime > resFile.LastWriteTime)
								Logger.LogDebug("VerifyLanguageResourceFiles(): resFile → {ResFile}", resFile);
                                {
                                    tableOld.Add(this.ResourceFile(file.Key.ToString(), locale.Code).Replace(HttpContext.Current.Server.MapPath("~"), string.Empty));
                                }
                            }
                        }
                    }

                    tables.Add(new
                    {
                        Language = Localization.GetLocaleName(locale.Code, this.GetCultureDropDownType(this.PortalId)),
                        IsSystemDefault = Localization.SystemLocale == locale.Code,
                        Icon = Globals.ResolveUrl(string.IsNullOrEmpty(locale.Code)
                            ? "~/images/Flags/none.gif"
						Logger.LogDebug("VerifyLanguageResourceFiles(): Language → {Language}", Language);
                            : $"~/images/Flags/{locale.Code}.gif"),
                        MissingFiles = tableMissing,
                        FilesWithDuplicateEntries = tableDuplicate,
                        FilesWithMissingEntries = tableEntries,
                        FilesWithObsoleteEntries = tableObsolete,
                        OldFiles = tableOld,
                        MalformedFiles = tableError,
                    });
						Logger.LogDebug("VerifyLanguageResourceFiles(): MissingFiles → {MissingFiles}", MissingFiles);
						Logger.LogDebug("VerifyLanguageResourceFiles(): FilesWithDuplicateEntries → {FilesWithDuplicateEntries}", FilesWithDuplicateEntries);
						Logger.LogDebug("VerifyLanguageResourceFiles(): FilesWithMissingEntries → {FilesWithMissingEntries}", FilesWithMissingEntries);
						Logger.LogDebug("VerifyLanguageResourceFiles(): FilesWithObsoleteEntries → {FilesWithObsoleteEntries}", FilesWithObsoleteEntries);
						Logger.LogDebug("VerifyLanguageResourceFiles(): OldFiles → {OldFiles}", OldFiles);
						Logger.LogDebug("VerifyLanguageResourceFiles(): MalformedFiles → {MalformedFiles}", MalformedFiles);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Results = tables,
                });
					Logger.LogDebug("VerifyLanguageResourceFiles(): Results → {Results}", Results);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting VerifyLanguageResourceFiles()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetModuleList
        /// <summary>Gets module list by type.</summary>
        /// <param name="type">The type of extension to get such as Module, Provider or AuthSystem.</param>
        /// <returns>list of modules.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleList(string type)
        {
			Logger.LogDebug(">>> Entering GetModuleList(type={Type})", type);
            try
            {
                List<object> modules = new List<object>();
                switch (type)
				Logger.LogDebug("GetModuleList(type={Type}): modules → {Modules}", type, modules);
                {
                    case "Module":
                        foreach (var objDm in DesktopModuleController.GetDesktopModules(this.hostSettings, Null.NullInteger).Values)
                        {
                            if (!objDm.FolderName.StartsWith("Admin/", StringComparison.OrdinalIgnoreCase))
                            {
                                if (Null.IsNull(objDm.Version))
                                {
                                    modules.Add(new KeyValuePair<string, int>(objDm.FriendlyName, objDm.DesktopModuleID));
                                }
                                else
                                {
                                    modules.Add(
                                        new KeyValuePair<string, int>(
                                            $"{objDm.FriendlyName} [{objDm.Version}]",
                                            objDm.DesktopModuleID));
                                }
                            }
                        }

                        break;
                    case "Provider":
                        modules.AddRange(PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Provider").Select(objPackage => Null.IsNull(objPackage.Version) ? new KeyValuePair<string, int>(objPackage.FriendlyName, objPackage.PackageID) : new KeyValuePair<string, int>(objPackage.FriendlyName + " [" + Globals.FormatVersion(objPackage.Version) + "]", objPackage.PackageID)).Cast<object>());
                        break;
                    case "AuthSystem":
                        modules.AddRange(PackageController.Instance.GetExtensionPackages(Null.NullInteger, p => p.PackageType == "Auth_System").Select(objPackage => Null.IsNull(objPackage.Version) ? new KeyValuePair<string, int>(objPackage.FriendlyName, objPackage.PackageID) : new KeyValuePair<string, int>(objPackage.FriendlyName + " [" + Globals.FormatVersion(objPackage.Version) + "]", objPackage.PackageID)).Cast<object>());
                        break;
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Modules = modules,
                });
					Logger.LogDebug("GetModuleList(type={Type}): Modules → {Modules}", type, Modules);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetModuleList(type={Type})", type);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/CreateLanguagePack
        /// <summary>Creates language.</summary>
        /// <param name="request">The creation request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateLanguagePack(CreateLanguagePackRequest request)
        {
			Logger.LogDebug(">>> Entering CreateLanguagePack(request={Request})", request);
            try
            {
                bool created = false;
                switch (request.PackType)
				Logger.LogDebug("CreateLanguagePack(request={Request}): created → {Created}", request, created);
                {
                    case "Core":
                        created = this.controller.CreateCorePackage(request.CultureCode, request.FileName, true);
                        break;
                    case "Module":
                        foreach (int moduleId in request.ModuleIds)
                        {
                            DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModule(this.hostSettings, moduleId, Null.NullInteger);
                            created = this.controller.CreateModulePackage(request.CultureCode, desktopModule, true);
							Logger.LogDebug("CreateLanguagePack(request={Request}): desktopModule → {DesktopModule}", request, desktopModule);
                        }

                        break;
                    case "Provider":
                        foreach (int moduleId in request.ModuleIds)
                        {
                            PackageInfo provider = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == moduleId);
                            created = this.controller.CreateProviderPackage(request.CultureCode, provider, true);
							Logger.LogDebug("CreateLanguagePack(request={Request}): provider → {Provider}", request, provider);
                        }

                        break;
                    case "AuthSystem":
                        foreach (int moduleId in request.ModuleIds)
                        {
                            PackageInfo authSystem = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == moduleId);
                            created = this.controller.CreateAuthSystemPackage(request.CultureCode, authSystem, true);
							Logger.LogDebug("CreateLanguagePack(request={Request}): authSystem → {AuthSystem}", request, authSystem);
                        }

                        break;
                    case "Full":
                        this.controller.CreateFullPackage(request.CultureCode, request.FileName);
                        created = true;
                        break;
                }

                if (created)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = true,
                        Message = string.Format(
                            CultureInfo.CurrentCulture,
                            Localization.GetString("LanguagePackCreateSuccess", Components.Constants.Constants.LocalResourcesFile),
                            ((IPortalAliasInfo)this.PortalSettings.PortalAlias).HttpAlias),
                    });
						Logger.LogDebug("CreateLanguagePack(request={Request}): Success → {Success}", request, Success);
						Logger.LogDebug("CreateLanguagePack(request={Request}): Message → {Message}", request, Message);
                }
                else
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("LanguagePackCreateFailure", Components.Constants.Constants.LocalResourcesFile));
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting CreateLanguagePack(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetTranslatorRoles
        /// <summary>Gets roles.</summary>
        /// <param name="portalId">The ID of the portal for which to get the translator roles for.</param>
        /// <param name="groupId">The ID of the role group to filter the results.</param>
        /// <param name="cultureCode">The culture code for which to get the translators for.</param>
        /// <returns>list of translator roles.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetTranslatorRoles(int? portalId, int groupId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocale(cultureCode);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                string defaultRoles = PortalController.GetPortalSetting(this.portalController, $"DefaultTranslatorRoles-{cultureCode}", pid, "Administrators");
                var selectedRoleNames = new ArrayList(defaultRoles.Split(';'));

                var roles = (groupId < Null.NullInteger
                                    ? RoleController.Instance.GetRoles(pid, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved)
                                    : RoleController.Instance.GetRoles(pid, r => r.RoleGroupID == groupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved))
                                    .Select(r => new
                                    {
                                        r.RoleID,
                                        r.RoleName,
                                        Selected = selectedRoleNames.Contains(r.RoleName),
                                    });

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Roles = roles,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetTranslatorRoleGroups
        /// <summary>Gets role groups.</summary>
        /// <param name="portalId">The ID of the portal for which to get the translator role groups for.</param>
        /// <returns>list of translator role groups.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetTranslatorRoleGroups(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetTranslatorRoleGroups(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var groups = RoleController.GetRoleGroups(this.roleProvider, pid)
                                .Cast<RoleGroupInfo>()
                                .Select(g => new
                                {
                                    g.RoleGroupID,
                                    g.RoleGroupName,
                                });

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Groups = groups,
                });
					Logger.LogDebug("GetTranslatorRoleGroups(portalId={PortalId}): Groups → {Groups}", portalId, Groups);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetTranslatorRoleGroups(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/SiteSettings/GetOtherSettings
        /// <summary>Gets other settings.</summary>
        /// <param name="portalId">The ID of the portal for which to get the other settings for.</param>
        /// <returns>other settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetOtherSettings(int? portalId)
        {
			Logger.LogDebug(">>> Entering GetOtherSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
				Logger.LogDebug("GetOtherSettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
				Logger.LogDebug("GetOtherSettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = new
					Logger.LogDebug("GetOtherSettings(portalId={PortalId}): Settings → {Settings}", portalId, Settings);
                    {
                        AllowedExtensionsWhitelist = portalSettings.AllowedExtensionsWhitelist.ToStorageString(),
                        HostAllowedExtensionsWhitelists = this.hostSettings.DefaultEndUserExtensionAllowList.ToStorageString(),
                        ImageExtensionsList = Globals.ImageFileTypes,
                        EnablePopups = portalSettings.EnablePopUps,
                        portalSettings.InjectModuleHyperLink,
                        portalSettings.InlineEditorEnabled,
                        portalSettings.AllowJsInModuleHeaders,
                        portalSettings.AllowJsInModuleFooters,
                        portalSettings.ShowQuickModuleAddMenu,
                        EnabledVersioning = TabVersionSettings.Instance.IsVersioningEnabled(pid),
                        MaxNumberOfVersions = TabVersionSettings.Instance.GetMaxNumberOfVersions(pid),
                        WorkflowEnabled = TabWorkflowSettings.Instance.IsWorkflowEnabled(pid),
                        DefaultTabWorkflowId = TabWorkflowSettings.Instance.GetDefaultTabWorkflowId(pid),
                    },
                    Workflows = WorkflowManager.Instance.GetWorkflows(pid).Select(w => new { label = w.WorkflowName, value = w.WorkflowID }).ToList(),
                });
						Logger.LogDebug("GetOtherSettings(portalId={PortalId}): AllowedExtensionsWhitelist → {AllowedExtensionsWhitelist}", portalId, AllowedExtensionsWhitelist);
						Logger.LogDebug("GetOtherSettings(portalId={PortalId}): HostAllowedExtensionsWhitelists → {HostAllowedExtensionsWhitelists}", portalId, HostAllowedExtensionsWhitelists);
						Logger.LogDebug("GetOtherSettings(portalId={PortalId}): ImageExtensionsList → {ImageExtensionsList}", portalId, ImageExtensionsList);
						Logger.LogDebug("GetOtherSettings(portalId={PortalId}): EnablePopups → {EnablePopups}", portalId, EnablePopups);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetOtherSettings(portalId={PortalId})", portalId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/SiteSettings/UpdateOtherSettings
        /// <summary>Updates other settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateOtherSettings(UpdateOtherSettingsRequest request)
        {
			Logger.LogDebug(">>> Entering UpdateOtherSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                PortalController.Instance.UpdatePortalSetting(pid, "EnablePopups", request.EnablePopups.ToString(), false, null, false);
                PortalController.Instance.UpdatePortalSetting(pid, "InjectModuleHyperLink", request.InjectModuleHyperLink.ToString(), false, null, false);
                PortalController.Instance.UpdatePortalSetting(pid, "InlineEditorEnabled", request.InlineEditorEnabled.ToString(), false, null, false);
                PortalController.Instance.UpdatePortalSetting(pid, "AllowJsInModuleHeaders", request.AllowJsInModuleHeaders.ToString(), false, null, false);
                PortalController.Instance.UpdatePortalSetting(pid, "AllowJsInModuleFooters", request.AllowJsInModuleFooters.ToString(), false, null, false);
                PortalController.Instance.UpdatePortalSetting(pid, "ShowQuickModuleAddMenu", request.ShowQuickModuleAddMenu.ToString(), false, null, false);
                if (request.AllowedExtensionsWhitelist == this.hostSettings.DefaultEndUserExtensionAllowList.ToStorageString())
                {
                    PortalController.Instance.UpdatePortalSetting(pid, "AllowedExtensionsWhitelist", null, false, null, false);
                }
                else
                {
                    IFileExtensionAllowList allowList = new FileExtensionWhitelist(request.AllowedExtensionsWhitelist);
                    allowList = allowList.RestrictBy(this.hostSettings.AllowedExtensionAllowList);
					Logger.LogDebug("UpdateOtherSettings(request={Request}): allowList → {AllowList}", request, allowList);
                    PortalController.Instance.UpdatePortalSetting(pid, "AllowedExtensionsWhitelist", allowList.ToStorageString(), false, null, false);
                }

                if (request.EnabledVersioning.HasValue)
                {
                    TabVersionSettings.Instance.SetEnabledVersioningForPortal(pid, request.EnabledVersioning.Value);
                }

                if (request.MaxNumberOfVersions.HasValue)
                {
                    TabVersionSettings.Instance.SetMaxNumberOfVersions(pid, request.MaxNumberOfVersions.Value);
                }

                if (request.WorkflowEnabled.HasValue)
                {
                    TabWorkflowSettings.Instance.SetWorkflowEnabled(pid, request.WorkflowEnabled.Value);
                }

                if (request.DefaultTabWorkflowId.HasValue)
                {
                    TabWorkflowSettings.Instance.SetDefaultTabWorkflowId(pid, request.DefaultTabWorkflowId.Value);
                }

                DataCache.ClearCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UpdateOtherSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static void GetResourceFiles(SortedList fileList, string path)
        {
			Logger.LogDebug(">>> Entering GetResourceFiles(fileList={FileList},path={Path})", fileList, path);
            var folders = Directory.GetDirectories(path);

			Logger.LogDebug("GetResourceFiles(fileList={FileList},path={Path}): folders → {Folders}", fileList, path, folders);
            foreach (var folder in folders)
            {
                var objFolder = new DirectoryInfo(folder);

				Logger.LogDebug("GetResourceFiles(fileList={FileList},path={Path}): objFolder → {ObjFolder}", fileList, path, objFolder);
                bool resxFilesDirectory = objFolder.Name.Equals(Localization.LocalResourceDirectory, StringComparison.OrdinalIgnoreCase) ||
                                          objFolder.Name.Equals(Localization.ApplicationResourceDirectory.Replace("~/", string.Empty), StringComparison.OrdinalIgnoreCase) ||
                                          folder.EndsWith("\\portals\\_default", StringComparison.OrdinalIgnoreCase);

                if (resxFilesDirectory)
                {
                    var sysLocale = Localization.SystemLocale.ToLowerInvariant();
                    foreach (var file in Directory.GetFiles(objFolder.FullName, "*.resx"))
                    {
					Logger.LogDebug("GetResourceFiles(fileList={FileList},path={Path}): sysLocale → {SysLocale}", fileList, path, sysLocale);
                        var fileInfo = new FileInfo(file);
                        var match = LanguagesController.FileInfoRegex.Match(fileInfo.Name);
						Logger.LogDebug("GetResourceFiles(fileList={FileList},path={Path}): fileInfo → {FileInfo}", fileList, path, fileInfo);
						Logger.LogDebug("GetResourceFiles(fileList={FileList},path={Path}): match → {Match}", fileList, path, match);

                        if (match.Success && !match.Groups[1].Value.Equals(sysLocale, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        fileList.Add(fileInfo.FullName, fileInfo);
                    }
                }
                else
                {
                    GetResourceFiles(fileList, folder);
                }
            }
			Logger.LogDebug("<<< Exiting GetResourceFiles(fileList={FileList},path={Path})", fileList, path);
        }

        private static bool IsLanguagePublished(int portalId, string code)
        {
			Logger.LogDebug(">>> Entering IsLanguagePublished(portalId={PortalId},code={Code})", portalId, code);
            bool isPublished = Null.NullBoolean;
            if (LocaleController.Instance.GetLocales(portalId).TryGetValue(code, out var enabledLanguage))
			Logger.LogDebug("IsLanguagePublished(portalId={PortalId},code={Code}): isPublished → {IsPublished}", portalId, code, isPublished);
            {
                isPublished = enabledLanguage.IsPublished;
            }

			Logger.LogDebug("<<< Exiting IsLanguagePublished(portalId={PortalId},code={Code})", portalId, code);
            return isPublished;
        }

        private static bool IsDefaultLanguage(PortalSettings portalSettings, string code)
        {
            return code == portalSettings.DefaultLanguage;
        }

        private static bool CanDeleteProperty(ProfilePropertyDefinition definition)
        {
			Logger.LogDebug(">>> Entering CanDeleteProperty(definition={Definition})", definition);
            switch (definition.PropertyName.ToLowerInvariant())
            {
                case "lastname":
                case "firstname":
                case "preferredtimezone":
                case "preferredlocale":
                    return false;
                default:
					Logger.LogDebug("<<< Exiting CanDeleteProperty(definition={Definition})", definition);
                    return true;
            }
        }

        private static string DisplayDataType(ListController listController, int dataType)
        {
			Logger.LogDebug(">>> Entering DisplayDataType(listController={ListController},dataType={DataType})", listController, dataType);
            var retValue = Null.NullString;
            var definitionEntry = listController.GetListEntryInfo("DataType", dataType);
			Logger.LogDebug("DisplayDataType(listController={ListController},dataType={DataType}): retValue → {RetValue}", listController, dataType, retValue);
			Logger.LogDebug("DisplayDataType(listController={ListController},dataType={DataType}): definitionEntry → {DefinitionEntry}", listController, dataType, definitionEntry);
            if (definitionEntry != null)
            {
                retValue = definitionEntry.Value;
            }

			Logger.LogDebug("<<< Exiting DisplayDataType(listController={ListController},dataType={DataType})", listController, dataType);
            return retValue;
        }

        private static bool IsLanguageEnabled(int portalId, string code)
        {
			Logger.LogDebug(">>> Entering IsLanguageEnabled(portalId={PortalId},code={Code})", portalId, code);
			Logger.LogDebug("<<< Exiting IsLanguageEnabled(portalId={PortalId},code={Code})", portalId, code);
            return LocaleController.Instance.GetLocales(portalId).TryGetValue(code, out _);
        }

        private static bool CanEnableDisable(PortalSettings portalSettings, string code)
        {
            bool canEnable;
            if (IsLanguageEnabled(portalSettings.PortalId, code))
            {
                canEnable = !IsDefaultLanguage(portalSettings, code) && !IsLanguagePublished(portalSettings.PortalId, code);
            }
            else
            {
                canEnable = !IsDefaultLanguage(portalSettings, code);
            }

            return canEnable;
        }

        private static SearchStatistics GetSearchStatistics()
        {
			Logger.LogDebug(">>> Entering GetSearchStatistics()");
            try
            {
                return InternalSearchController.Instance.GetSearchStatistics();
            }
            catch (SearchIndexEmptyException)
            {
				Logger.LogError("Exception in GetSearchStatistics(): {Arg0}", ex.Message);
				Logger.LogDebug("<<< Exiting GetSearchStatistics()");
                return null;
            }
        }

        private static void ClearEntriesCache(string listName, int portalId)
        {
			Logger.LogDebug(">>> Entering ClearEntriesCache(listName={ListName},portalId={PortalId})", listName, portalId);
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.ListEntriesCacheKey, portalId, listName);
            DataCache.RemoveCache(cacheKey);
			Logger.LogDebug("<<< Exiting ClearEntriesCache(listName={ListName},portalId={PortalId})", listName, portalId);
        }

        private static int GetLocalizedTabIdSetting(Dictionary<string, string> localizedPortalSettings, string settingKey)
        {
            if (localizedPortalSettings.TryGetValue(settingKey, out var settingValue))
            {
                int tabId = int.TryParse(settingValue, out tabId) ? tabId : -1;
                return tabId;
            }

            return -1;
        }

        private string GetTranslatedPages(PortalSettings portalSettings, string code)
        {
            string status = string.Empty;
            if (!IsDefaultLanguage(portalSettings, code) && this.IsLocalized(portalSettings, code))
            {
                int translatedCount = (from t in this.tabController.GetTabsByPortal(portalSettings.PortalId).WithCulture(code, false).Values where t.IsTranslated && !t.IsDeleted select t).Count();
                status = translatedCount.ToString(CultureInfo.InvariantCulture);
            }

            return status;
        }

        private string GetLocalizedStatus(PortalSettings portalSettings, string code)
        {
            string status = string.Empty;
            if (!IsDefaultLanguage(portalSettings, code) && this.IsLocalized(portalSettings, code))
            {
                int defaultPageCount = this.GetLocalizedPages(portalSettings.PortalId, portalSettings.DefaultLanguage, false).Count;
                int currentPageCount = this.GetLocalizedPages(portalSettings.PortalId, code, false).Count;
                status = $"{currentPageCount / (float)defaultPageCount:#0%}";
            }

            return status;
        }

        private string GetLocalizablePages(int portalId, string code)
        {
			Logger.LogDebug(">>> Entering GetLocalizablePages(portalId={PortalId},code={Code})", portalId, code);
            int count = this.GetLocalizedPages(portalId, code, false).Count(t => !t.Value.IsDeleted);
			Logger.LogDebug("GetLocalizablePages(portalId={PortalId},code={Code}): count → {Count}", portalId, code, count);
			Logger.LogDebug("<<< Exiting GetLocalizablePages(portalId={PortalId},code={Code})", portalId, code);
            return count.ToString(CultureInfo.CurrentCulture);
        }

        private TabCollection GetLocalizedPages(int portalId, string code, bool includeNeutral)
        {
			Logger.LogDebug(">>> Entering GetLocalizedPages(portalId={PortalId},code={Code},includeNeutral={IncludeNeutral})", portalId, code, includeNeutral);
			Logger.LogDebug("<<< Exiting GetLocalizedPages(portalId={PortalId},code={Code},includeNeutral={IncludeNeutral})", portalId, code, includeNeutral);
            return this.tabController.GetTabsByPortal(portalId).WithCulture(code, includeNeutral);
        }

        private int GetPublishedLocalizedPages(int portalId, string code)
        {
			Logger.LogDebug(">>> Entering GetPublishedLocalizedPages(portalId={PortalId},code={Code})", portalId, code);
            var localizedTabs = this.tabController.GetTabsByPortal(portalId).WithCulture(code, false);
			Logger.LogDebug("GetPublishedLocalizedPages(portalId={PortalId},code={Code}): localizedTabs → {LocalizedTabs}", portalId, code, localizedTabs);
			Logger.LogDebug("<<< Exiting GetPublishedLocalizedPages(portalId={PortalId},code={Code})", portalId, code);
            return localizedTabs.Count(t => this.tabController.IsTabPublished(t.Value));
        }

        private string GetTranslatedStatus(PortalSettings portalSettings, string code)
        {
            string status = string.Empty;
            if (!IsDefaultLanguage(portalSettings, code) && this.IsLocalized(portalSettings, code))
            {
                int localizedCount = this.GetLocalizedPages(portalSettings.PortalId, code, false).Count;
                int translatedCount = (from t in this.tabController.GetTabsByPortal(portalSettings.PortalId).WithCulture(code, false).Values where t.IsTranslated select t).Count();
                status = $"{translatedCount / (float)localizedCount:#0%}";
            }

            return status;
        }

        private bool IsLocalized(PortalSettings portalSettings, string code)
        {
            return code != portalSettings.DefaultLanguage && this.GetLocalizedPages(portalSettings.PortalId, code, false).Count > 0;
        }

        private TabInfo TabSanitizer(int tabId, int portalId)
        {
            var tab = this.tabController.GetTab(tabId, portalId);
            if (tab is { IsDeleted: false, })
            {
                return tab;
            }
            else
            {
                return null;
            }
        }

        private int ValidateTabId(int tabId, int portalId)
        {
            var tab = this.tabController.GetTab(tabId, portalId);
            return tab is { IsDeleted: false, } ? tab.TabID : Null.NullInteger;
        }

        private bool IsHttpAliasValid(string strAlias)
        {
            bool isValid = true;
            if (string.IsNullOrEmpty(strAlias))
            {
                isValid = false;
            }
            else
            {
                if (strAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }
				Logger.LogDebug("IsHttpAliasValid(strAlias={StrAlias}): strAlias → {StrAlias1}", strAlias, strAlias);

                if (strAlias.IndexOf("\\\\", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("\\\\", StringComparison.Ordinal) + 2);
                }

                // Validate Alias, this needs to be done with lowercase, downstream we only check with lowercase variables
                if (!this.portalAliasService.ValidateAlias(strAlias.ToLowerInvariant(), false))
                {
                    isValid = false;
                }
				Logger.LogDebug("IsHttpAliasValid(strAlias={StrAlias}): isValid → {IsValid}", strAlias, isValid);
            }

			Logger.LogDebug("<<< Exiting IsHttpAliasValid(strAlias={StrAlias})", strAlias);
            return isValid;
        }

        private string ResourceFile(string filename, string language)
        {
            return Localization.GetResourceFileName(filename, language, string.Empty, Globals.GetPortalSettings(this.hostSettings).PortalId);
        }

        private string GetAbsoluteServerPath()
        {
			Logger.LogDebug(">>> Entering GetAbsoluteServerPath()");
            if (this.Request.Properties["MS_HttpContext"] is HttpContextWrapper httpContext)
            {
                var strServerPath = httpContext.Request.MapPath(httpContext.Request.ApplicationPath);
                if (!strServerPath.EndsWith(@"\", StringComparison.Ordinal))
				Logger.LogDebug("GetAbsoluteServerPath(): strServerPath → {StrServerPath}", strServerPath);
                {
                    strServerPath += @"\";
                }

                return strServerPath;
            }
            else
            {
				Logger.LogDebug("<<< Exiting GetAbsoluteServerPath()");
                return string.Empty;
            }
        }

        private bool ValidateProperty(ProfilePropertyDefinition definition, out HttpResponseMessage httpPropertyValidationError)
        {
			Logger.LogDebug(">>> Entering ValidateProperty(definition={Definition})", definition);
            bool isValid = true;
            httpPropertyValidationError = null;
			Logger.LogDebug("ValidateProperty(definition={Definition}): isValid → {IsValid}", definition, isValid);
			Logger.LogDebug("ValidateProperty(definition={Definition}): httpPropertyValidationError → {HttpPropertyValidationError}", definition, httpPropertyValidationError);
            string strDataType = this.listController.GetListEntryInfo("DataType", definition.DataType).Value;
            Regex propertyNameRegex = new Regex("^[a-zA-Z0-9]+$");
			Logger.LogDebug("ValidateProperty(definition={Definition}): propertyNameRegex → {PropertyNameRegex}", definition, propertyNameRegex);
            if (!propertyNameRegex.IsMatch(definition.PropertyName))
            {
                isValid = false;
                httpPropertyValidationError = this.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    Localization.GetString("NoSpecialCharacterName.Text", Components.Constants.Constants.LocalResourcesFile));
            }

            switch (strDataType)
            {
                case "Text":
                    if (definition.Required && definition.Length == 0)
                    {
                        isValid = Null.NullBoolean;
                    }

                    break;
            }

            if (!isValid)
            {
                httpPropertyValidationError = this.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    Localization.GetString("RequiredTextBox", Components.Constants.Constants.LocalResourcesFile));
            }

			Logger.LogDebug("<<< Exiting ValidateProperty(definition={Definition})", definition);
            return isValid;
        }

        private CultureDropDownTypes GetCultureDropDownType(int portalId)
        {
			Logger.LogDebug(">>> Entering GetCultureDropDownType(portalId={PortalId})", portalId);
            CultureDropDownTypes displayType;
            string viewType = this.GetLanguageDisplayMode(portalId);
            switch (viewType)
            {
                case "NATIVE":
                    displayType = CultureDropDownTypes.NativeName;
					Logger.LogDebug("GetCultureDropDownType(portalId={PortalId}): displayType → {DisplayType}", portalId, displayType);
                    break;
                case "ENGLISH":
                    displayType = CultureDropDownTypes.EnglishName;
                    break;
                default:
                    displayType = CultureDropDownTypes.DisplayName;
                    break;
            }

			Logger.LogDebug("<<< Exiting GetCultureDropDownType(portalId={PortalId})", portalId);
            return displayType;
        }

        private string GetLanguageDisplayMode(int portalId)
        {
			Logger.LogDebug(">>> Entering GetLanguageDisplayMode(portalId={PortalId})", portalId);
            string viewTypePersonalizationKey = "LanguageDisplayMode:ViewType" + portalId;
            var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, portalId);
			Logger.LogDebug("GetLanguageDisplayMode(portalId={PortalId}): personalization → {Personalization}", portalId, personalization);

            string viewType = Convert.ToString(personalization.Profile[viewTypePersonalizationKey], CultureInfo.InvariantCulture);
			Logger.LogDebug("<<< Exiting GetLanguageDisplayMode(portalId={PortalId})", portalId);
            return string.IsNullOrEmpty(viewType) ? "NATIVE" : viewType;
        }
    }
}
