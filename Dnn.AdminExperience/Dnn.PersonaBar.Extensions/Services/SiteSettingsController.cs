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

        private readonly Components.SiteSettingsController controller = new Components.SiteSettingsController();
        private readonly INavigationManager navigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        private readonly IApplicationStatusInfo appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        private readonly PersonalizationController personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IPortalGroupController portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
        private readonly ICryptographyProvider cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        private readonly IHostSettingsService hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IPortalAliasService portalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
        private readonly RoleProvider roleProvider = roleProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>();
        private readonly ITabController tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();

        /// <summary>Initializes a new instance of the <see cref="SiteSettingsController"/> class.</summary>
        /// <param name="navigationManager">A manager to provide navigation services.</param>
        /// <param name="applicationStatusInfo">The application status info.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public SiteSettingsController(INavigationManager navigationManager, IApplicationStatusInfo applicationStatusInfo)
            : this(navigationManager, applicationStatusInfo, null, null, null, null, null, null, null, null, null, null, null)
        {
		Logger.LogError(">>> Entering SiteSettingsController(navigationManager={NavigationManager},applicationStatusInfo={ApplicationStatusInfo})", navigationManager, applicationStatusInfo);
        Logger.LogWarning("<<< Exiting SiteSettingsController(navigationManager={NavigationManager},applicationStatusInfo={ApplicationStatusInfo})", navigationManager, applicationStatusInfo);
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
			Logger.LogError(">>> Entering GetPortalSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
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
				Logger.LogError("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): language → {Language}", portalId, cultureCode, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portal = PortalController.Instance.GetPortal(pid, cultureCode);
				Logger.LogError("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): portalSettings → {PortalSettings}", portalId, cultureCode, portalSettings);
                var logoFile = string.IsNullOrEmpty(portal.LogoFile) ? null : FileManager.Instance.GetFile(pid, portal.LogoFile);
				Logger.LogError("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): logoFile → {LogoFile}", portalId, cultureCode, logoFile);
                var faviconPath = new FavIcon(((IPortalInfo)portal).PortalId).GetSettingPath();
				Logger.LogError("GetPortalSettings(portalId={PortalId},cultureCode={CultureCode}): faviconPath → {FaviconPath}", portalId, cultureCode, faviconPath);
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
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Settings = settings,
                    TimeZones = TimeZoneInfo.GetSystemTimeZones().Select(z => new
                    {
                        z.Id,
                        z.DisplayName,
                    }),
                    IconSets = IconController.GetIconSets(this.appStatus),
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
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
			Logger.LogError(">>> Entering GetCultureList(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("GetCultureList(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetCultureList(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

                string viewType = this.GetLanguageDisplayMode(pid);

                var locals = LocaleController.Instance.GetLocales(pid).Values;
				Logger.LogError("GetCultureList(portalId={PortalId}): locals → {Locals}", portalId, locals);
                var cultureCodeList = locals.Select(local => new
                {
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
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetCultureList(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering UpdatePortalSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var cultureCode = string.IsNullOrEmpty(request.CultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : request.CultureCode;
				Logger.LogError("UpdatePortalSettings(request={Request}): cultureCode → {CultureCode}", request, cultureCode);

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
				Logger.LogError("UpdatePortalSettings(request={Request}): language → {Language}", request, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portalInfo = PortalController.Instance.GetPortal(pid, cultureCode);
				Logger.LogError("UpdatePortalSettings(request={Request}): portalInfo → {PortalInfo}", request, portalInfo);
                portalInfo.PortalName = request.PortalName;
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
				Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo.KeyWords → {Arg1}", request, portalInfo.KeyWords);
                portalInfo.Description = request.Description;
				Logger.LogDebug("UpdatePortalSettings(request={Request}): portalInfo.Description → {Arg1}", request, portalInfo.Description);
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
				Logger.LogWarning("<<< Exiting UpdatePortalSettings(request={Request})", request);
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
			Logger.LogError(">>> Entering GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
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
				Logger.LogError("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): language → {Language}", portalId, cultureCode, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portal = PortalController.Instance.GetPortal(pid, cultureCode);
				Logger.LogError("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): portal → {Portal}", portalId, cultureCode, portal);
                var localizedPortalSettings = PortalController.Instance.GetPortalSettings(pid, cultureCode);
				Logger.LogError("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): localizedPortalSettings → {LocalizedPortalSettings}", portalId, cultureCode, localizedPortalSettings);

                int redirectAfterLoginTabId = GetLocalizedTabIdSetting(localizedPortalSettings, "Redirect_AfterLogin");
				Logger.LogError("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): redirectAfterLoginTabId → {RedirectAfterLoginTabId}", portalId, cultureCode, redirectAfterLoginTabId);
                int redirectAfterLogoutTabId = GetLocalizedTabIdSetting(localizedPortalSettings, "Redirect_AfterLogout");
				Logger.LogError("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): redirectAfterLogoutTabId → {RedirectAfterLogoutTabId}", portalId, cultureCode, redirectAfterLogoutTabId);
                int redirectAfterRegistrationTabId = GetLocalizedTabIdSetting(localizedPortalSettings, "Redirect_AfterRegistration");
				Logger.LogError("GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode}): redirectAfterRegistrationTabId → {RedirectAfterRegistrationTabId}", portalId, cultureCode, redirectAfterRegistrationTabId);

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
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetDefaultPagesSettings(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
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
			Logger.LogError(">>> Entering UpdateDefaultPagesSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var cultureCode = string.IsNullOrEmpty(request.CultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : request.CultureCode;
				Logger.LogError("UpdateDefaultPagesSettings(request={Request}): cultureCode → {CultureCode}", request, cultureCode);
                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
				Logger.LogError("UpdateDefaultPagesSettings(request={Request}): language → {Language}", request, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var portalInfo = PortalController.Instance.GetPortal(pid, cultureCode);
				Logger.LogError("UpdateDefaultPagesSettings(request={Request}): portalInfo → {PortalInfo}", request, portalInfo);
                portalInfo.SplashTabId = this.ValidateTabId(request.SplashTabId, pid);
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
				Logger.LogWarning("<<< Exiting UpdateDefaultPagesSettings(request={Request})", request);
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
			Logger.LogError(">>> Entering GetMessagingSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("GetMessagingSettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetMessagingSettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

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
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetMessagingSettings(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering UpdateMessagingSettings(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting UpdateMessagingSettings(request={Request})", request);
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
			Logger.LogError(">>> Entering GetProfileSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var urlSettings = new FriendlyUrlSettings(this.portalController, this.hostSettings, this.hostSettingsService, pid);
				Logger.LogError("GetProfileSettings(portalId={PortalId}): urlSettings → {UrlSettings}", portalId, urlSettings);
                var userSettings = UserController.GetUserSettings(pid);
				Logger.LogError("GetProfileSettings(portalId={PortalId}): userSettings → {UserSettings}", portalId, userSettings);

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
                        {
							Logger.LogDebug("GetProfileSettings(portalId={PortalId}): UserVisibilityOptions → {UserVisibilityOptions}", portalId, UserVisibilityOptions);
                            label = v.ToString(),
                            value = (int)v,
                        }).ToList(),
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetProfileSettings(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering UpdateProfileSettings(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting UpdateProfileSettings(request={Request})", request);
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
			Logger.LogError(">>> Entering GetProfileProperties(portalId={PortalId})", portalId);
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

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    PortalId = pid,
                    Properties = profileProperties,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetProfileProperties(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering GetProfileProperty(propertyId={PropertyId},portalId={PortalId})", propertyId, portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var profileProperty = ProfileController.GetPropertyDefinition(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, propertyId ?? -1, pid);
				Logger.LogError("GetProfileProperty(propertyId={PropertyId},portalId={PortalId}): profileProperty → {ProfileProperty}", propertyId, portalId, profileProperty);

                var cultureList = Localization.LoadCultureInListItems(this.GetCultureDropDownType(pid), Thread.CurrentThread.CurrentUICulture.Name, string.Empty, false);
				Logger.LogError("GetProfileProperty(propertyId={PropertyId},portalId={PortalId}): cultureList → {CultureList}", propertyId, portalId, cultureList);

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
							Logger.LogDebug("GetProfileProperty(propertyId={PropertyId},portalId={PortalId}): UserVisibilityOptions → {UserVisibilityOptions}", propertyId, portalId, UserVisibilityOptions);
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
				Logger.LogDebug("GetProfileProperty(propertyId={PropertyId},portalId={PortalId}): Icon → {Icon}", propertyId, portalId, Icon);
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetProfileProperty(propertyId={PropertyId},portalId={PortalId})", propertyId, portalId);
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
			Logger.LogError(">>> Entering GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory})", portalId, cultureCode, propertyName, propertyCategory);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                cultureCode = string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode;
				Logger.LogError("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): cultureCode → {CultureCode1}", portalId, cultureCode, propertyName, propertyCategory, cultureCode);

                var language = LocaleController.Instance.GetLocale(pid, cultureCode);
				Logger.LogError("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): language → {Language}", portalId, cultureCode, propertyName, propertyCategory, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var resourceFile = "~/DesktopModules/Admin/Security/App_LocalResources/Profile.ascx";
				Logger.LogError("GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory}): resourceFile → {ResourceFile}", portalId, cultureCode, propertyName, propertyCategory, resourceFile);
                var response = new
                {
                    Success = true,
                    PropertyLocalization = new
                    {
                        Language = cultureCode,
                        PropertyName = Localization.GetString("ProfileProperties_" + propertyName, resourceFile, cultureCode) ?? string.Empty,
                        PropertyHelp = Localization.GetString("ProfileProperties_" + propertyName + ".Help", resourceFile, cultureCode) ?? string.Empty,
                        PropertyRequired = Localization.GetString("ProfileProperties_" + propertyName + ".Required", resourceFile, cultureCode) ?? string.Empty,
                        PropertyValidation = Localization.GetString("ProfileProperties_" + propertyName + ".Validation", resourceFile, cultureCode) ?? string.Empty,
                        CategoryName = Localization.GetString("ProfileProperties_" + propertyCategory + ".Header", resourceFile, cultureCode) ?? string.Empty,
                    },
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetProfilePropertyLocalization(portalId={PortalId},cultureCode={CultureCode},propertyName={PropertyName},propertyCategory={PropertyCategory})", portalId, cultureCode, propertyName, propertyCategory);
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
			Logger.LogError(">>> Entering UpdateProfilePropertyLocalization(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocale(pid, request.Language);
				Logger.LogError("UpdateProfilePropertyLocalization(request={Request}): language → {Language}", request, language);
                if (language == null)
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
				Logger.LogWarning("<<< Exiting UpdateProfilePropertyLocalization(request={Request})", request);
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
			Logger.LogError(">>> Entering AddProfileProperty(request={Request})", request);
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
					Logger.LogError("AddProfileProperty(request={Request}): propertyId → {PropertyId}", request, propertyId);
                    if (propertyId < Null.NullInteger)
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
			Logger.LogError(">>> Entering UpdateProfileProperty(request={Request})", request);
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
			Logger.LogError(">>> Entering UpdateProfilePropertyOrders(request={Request})", request);
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
					Logger.LogError("UpdateProfilePropertyOrders(request={Request}): profileProperty → {ProfileProperty}", request, profileProperty);
                    if (profileProperty.ViewOrder != i)
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
				Logger.LogWarning("<<< Exiting UpdateProfilePropertyOrders(request={Request})", request);
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
			Logger.LogError(">>> Entering DeleteProfileProperty(propertyId={PropertyId},portalId={PortalId})", propertyId, portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var propertyDefinition = ProfileController.GetPropertyDefinition(this.hostSettings, this.portalController, this.appStatus, this.portalGroupController, propertyId, pid);
				Logger.LogError("DeleteProfileProperty(propertyId={PropertyId},portalId={PortalId}): propertyDefinition → {PropertyDefinition}", propertyId, portalId, propertyDefinition);

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
				Logger.LogWarning("<<< Exiting DeleteProfileProperty(propertyId={PropertyId},portalId={PortalId})", propertyId, portalId);
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
			Logger.LogError(">>> Entering GetUrlMappingSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                Dictionary<string, string> settings = PortalController.Instance.GetPortalSettings(pid);
				Logger.LogError("GetUrlMappingSettings(portalId={PortalId}): settings → {Settings}", portalId, settings);
                string portalAliasMapping;
                if (settings.TryGetValue("PortalAliasMapping", out portalAliasMapping))
                {
                    if (string.IsNullOrEmpty(portalAliasMapping))
                    {
                        portalAliasMapping = "CANONICALURL";
						Logger.LogError("GetUrlMappingSettings(portalId={PortalId}): portalAliasMapping → {PortalAliasMapping}", portalId, portalAliasMapping);
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
					Logger.LogDebug("GetUrlMappingSettings(portalId={PortalId}): Settings → {Settings}", portalId, Settings);
                    Success = true,
                    Settings = new
                    {
                        PortalId = pid,
                        PortalAliasMapping = portalAliasMapping,
                        AutoAddPortalAliasEnabled = !(PortalController.Instance.GetPortals().Count > 1),
                        AutoAddPortalAlias = PortalController.Instance.GetPortals().Count <= 1 && this.hostSettingsService.GetBoolean("AutoAddPortalAlias"),
                    },
                    PortalAliasMappingModes = portalAliasMappingModes,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetUrlMappingSettings(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering UpdateUrlMappingSettings(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting UpdateUrlMappingSettings(request={Request})", request);
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
			Logger.LogError(">>> Entering GetSiteAliases(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("GetSiteAliases(portalId={PortalId}): portal → {Portal}", portalId, portal);

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

                var response = new
                {
                    PortalId = pid,
                    PortalAliases = aliases,
                    BrowserTypes = Enum.GetNames(typeof(DotNetNuke.Abstractions.Urls.BrowserTypes)),
                    Languages = LocaleController.Instance.GetLocales(pid).Select(l => new
                    {
                        l.Key,
                        Value = l.Key,
                    }),
                    Skins = SkinController.GetSkins(portal, SkinController.RootSkin, SkinScope.All),
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetSiteAliases(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering GetSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
            try
            {
                var alias = this.portalAliasService.GetPortalAliasByPortalAliasId(portalAliasId);
				Logger.LogError("GetSiteAlias(portalAliasId={PortalAliasId}): alias → {Alias}", portalAliasId, alias);
                if (!this.UserInfo.IsSuperUser && alias.PortalId != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var response = new
                {
					Logger.LogDebug("GetSiteAlias(portalAliasId={PortalAliasId}): PortalAlias → {PortalAlias}", portalAliasId, PortalAlias);
                    Success = true,
                    PortalAlias = new
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
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
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
			Logger.LogError(">>> Entering AddSiteAlias(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string strAlias = request.HTTPAlias;
				Logger.LogError("AddSiteAlias(request={Request}): strAlias → {StrAlias}", request, strAlias);
                if (!string.IsNullOrEmpty(strAlias))
                {
                    strAlias = strAlias.Trim();
                }

                if (this.IsHttpAliasValid(strAlias))
                {
                    var aliases = this.portalAliasService.GetPortalAliases();
					Logger.LogError("AddSiteAlias(request={Request}): aliases → {Aliases}", request, aliases);
                    if (aliases.ContainsKey(strAlias))
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
					Logger.LogDebug("AddSiteAlias(request={Request}): portalAlias.BrowserType → {Arg1}", request, portalAlias.BrowserType);
                    portalAlias.HttpAlias = strAlias;
					Logger.LogDebug("AddSiteAlias(request={Request}): portalAlias.HttpAlias → {Arg1}", request, portalAlias.HttpAlias);
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
				Logger.LogWarning("<<< Exiting AddSiteAlias(request={Request})", request);
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
			Logger.LogError(">>> Entering UpdateSiteAlias(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                string strAlias = request.HTTPAlias;
				Logger.LogError("UpdateSiteAlias(request={Request}): strAlias → {StrAlias}", request, strAlias);
                if (!string.IsNullOrEmpty(strAlias))
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
						Logger.LogDebug("UpdateSiteAlias(request={Request}): portalAlias.HttpAlias → {Arg1}", request, portalAlias.HttpAlias);
						Logger.LogDebug("UpdateSiteAlias(request={Request}): portalAlias.BrowserType → {Arg1}", request, portalAlias.BrowserType);
                        portalAlias.PortalAliasId = request.PortalAliasID.Value;
						Logger.LogDebug("UpdateSiteAlias(request={Request}): portalAlias.PortalAliasId → {Arg1}", request, portalAlias.PortalAliasId);
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
				Logger.LogWarning("<<< Exiting UpdateSiteAlias(request={Request})", request);
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
			Logger.LogError(">>> Entering DeleteSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
            try
            {
                var portalAlias = this.portalAliasService.GetPortalAliasByPortalAliasId(portalAliasId);
				Logger.LogError("DeleteSiteAlias(portalAliasId={PortalAliasId}): portalAlias → {PortalAlias}", portalAliasId, portalAlias);
                if (!this.UserInfo.IsSuperUser && portalAlias.PortalId != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                this.portalAliasService.DeletePortalAlias(portalAlias);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting DeleteSiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
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
			Logger.LogError(">>> Entering SetPrimarySiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
            try
            {
                var alias = this.portalAliasService.GetPortalAliasByPortalAliasId(portalAliasId);
				Logger.LogError("SetPrimarySiteAlias(portalAliasId={PortalAliasId}): alias → {Alias}", portalAliasId, alias);
                if (!this.UserInfo.IsSuperUser && alias.PortalId != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                IPortalAliasInfo portalAlias = new PortalAliasInfo { IsPrimary = true, };
                portalAlias.PortalId = alias.PortalId;
				Logger.LogDebug("SetPrimarySiteAlias(portalAliasId={PortalAliasId}): portalAlias.PortalId → {Arg1}", portalAliasId, portalAlias.PortalId);
                portalAlias.PortalAliasId = portalAliasId;
				Logger.LogDebug("SetPrimarySiteAlias(portalAliasId={PortalAliasId}): portalAlias.PortalAliasId → {Arg1}", portalAliasId, portalAlias.PortalAliasId);

                this.portalAliasService.UpdatePortalAlias(portalAlias);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting SetPrimarySiteAlias(portalAliasId={PortalAliasId})", portalAliasId);
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
			Logger.LogError(">>> Entering GetListInfo(listName={ListName},portalId={PortalId})", listName, portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var entries = this.listController.GetListEntryInfoItems(listName, string.Empty, pid);
				Logger.LogError("GetListInfo(listName={ListName},portalId={PortalId}): entries → {Entries}", listName, portalId, entries);
                var response = new
                {
                    Success = true,
                    this.listController.GetListInfo(listName, string.Empty, pid)?.EnableSortOrder,
                    Entries = entries.Select(t => new
                    {
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
				Logger.LogWarning("<<< Exiting GetListInfo(listName={ListName},portalId={PortalId})", listName, portalId);
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
			Logger.LogError(">>> Entering UpdateListEntry(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting UpdateListEntry(request={Request})", request);
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
			Logger.LogError(">>> Entering DeleteListEntry(entryId={EntryId},portalId={PortalId})", entryId, portalId);
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
				Logger.LogWarning("<<< Exiting DeleteListEntry(entryId={EntryId},portalId={PortalId})", entryId, portalId);
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
			Logger.LogError(">>> Entering UpdateListEntryOrders(request={Request})", request);
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
						Logger.LogError("UpdateListEntryOrders(request={Request}): entry → {Entry}", request, entry);
                        if (entry.SortOrder == i + 1)
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
				Logger.LogWarning("<<< Exiting UpdateListEntryOrders(request={Request})", request);
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
			Logger.LogError(">>> Entering GetPrivacySettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("GetPrivacySettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetPrivacySettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

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
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetPrivacySettings(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering UpdatePrivacySettings(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting UpdatePrivacySettings(request={Request})", request);
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
			Logger.LogError(">>> Entering ResetTermsAgreement(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting ResetTermsAgreement(request={Request})", request);
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
			Logger.LogError(">>> Entering GetBasicSearchSettings()");
            try
            {
                dynamic settings = new ExpandoObject();
				Logger.LogError("GetBasicSearchSettings(): settings → {Settings}", settings);
                settings.MinWordLength = this.hostSettingsService.GetInteger("Search_MinKeyWordLength", 3);
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
				Logger.LogError("GetBasicSearchSettings(): searchStatistics → {SearchStatistics}", searchStatistics);
                if (searchStatistics != null)
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
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetBasicSearchSettings()");
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
			Logger.LogError(">>> Entering UpdateBasicSearchSettings(request={Request})", request);
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
				Logger.LogError("UpdateBasicSearchSettings(request={Request}): oldMinLength → {OldMinLength}", request, oldMinLength);
                if (request.MinWordLength != oldMinLength)
                {
                    this.hostSettingsService.Update("Search_MinKeyWordLength", request.MinWordLength.ToString(CultureInfo.InvariantCulture));
                }

                var oldMaxResultPerPage = this.hostSettingsService.GetInteger("Search_MaxResultPerPage", 100);
				Logger.LogError("UpdateBasicSearchSettings(request={Request}): oldMaxResultPerPage → {OldMaxResultPerPage}", request, oldMaxResultPerPage);
                if (request.MaxResultPerPage != oldMaxResultPerPage)
                {
                    this.hostSettingsService.Update("Search_MaxResultPerPage", request.MaxResultPerPage.ToString(CultureInfo.InvariantCulture));
                }

                var oldMaxLength = this.hostSettingsService.GetInteger("Search_MaxKeyWordLength", 255);
				Logger.LogError("UpdateBasicSearchSettings(request={Request}): oldMaxLength → {OldMaxLength}", request, oldMaxLength);
                if (request.MaxWordLength != oldMaxLength)
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
				Logger.LogError("UpdateBasicSearchSettings(request={Request}): oldAnalyzer → {OldAnalyzer}", request, oldAnalyzer);
                var newAnalyzer = request.SearchCustomAnalyzer.Trim();
				Logger.LogError("UpdateBasicSearchSettings(request={Request}): newAnalyzer → {NewAnalyzer}", request, newAnalyzer);
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
				Logger.LogWarning("<<< Exiting UpdateBasicSearchSettings(request={Request})", request);
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
			Logger.LogError(">>> Entering CompactSearchIndex()");
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting CompactSearchIndex()");
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
			Logger.LogError(">>> Entering HostSearchReindex()");
            try
            {
                SearchHelper.Instance.SetSearchReindexRequestTime(-1);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting HostSearchReindex()");
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
			Logger.LogError(">>> Entering PortalSearchReindex(portalId={PortalId})", portalId);
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
				Logger.LogWarning("<<< Exiting PortalSearchReindex(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering GetPortals()");
            try
            {
                var portals = PortalController.Instance.GetPortals().OfType<IPortalInfo>();
				Logger.LogError("GetPortals(): portals → {Portals}", portals);
                var availablePortals = portals.Select(v => new
                {
                    PortalID = v.PortalId,
                    v.PortalName,
                    IsCurrentPortal = this.PortalId == v.PortalId,
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = availablePortals,
                    TotalResults = availablePortals.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetPortals()");
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
			Logger.LogError(">>> Entering GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocaleOrCurrent(pid, cultureCode);
				Logger.LogError("GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode}): language → {Language}", portalId, cultureCode, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var groups = SearchHelper.Instance.GetSynonymsGroups(pid, string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode);
				Logger.LogError("GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode}): groups → {Groups}", portalId, cultureCode, groups);

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
				Logger.LogWarning("<<< Exiting GetSynonymsGroups(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
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
			Logger.LogError(">>> Entering AddSynonymsGroup(request={Request})", request);
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
				Logger.LogError("AddSynonymsGroup(request={Request}): language → {Language}", request, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var synonymsGroupId = SearchHelper.Instance.AddSynonymsGroup(request.SynonymsTags, pid, cultureCode, out var duplicateWord);
				Logger.LogError("AddSynonymsGroup(request={Request}): synonymsGroupId → {SynonymsGroupId}", request, synonymsGroupId);
                if (synonymsGroupId > 0)
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
				Logger.LogWarning("<<< Exiting AddSynonymsGroup(request={Request})", request);
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
			Logger.LogError(">>> Entering UpdateSynonymsGroup(request={Request})", request);
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
				Logger.LogError("UpdateSynonymsGroup(request={Request}): language → {Language}", request, language);
                if (language == null)
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
				Logger.LogWarning("<<< Exiting UpdateSynonymsGroup(request={Request})", request);
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
			Logger.LogError(">>> Entering DeleteSynonymsGroup(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting DeleteSynonymsGroup(request={Request})", request);
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
			Logger.LogError(">>> Entering GetIgnoreWords(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocaleOrCurrent(pid, cultureCode);
				Logger.LogError("GetIgnoreWords(portalId={PortalId},cultureCode={CultureCode}): language → {Language}", portalId, cultureCode, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var words = SearchHelper.Instance.GetSearchStopWords(pid, string.IsNullOrEmpty(cultureCode) ? LocaleController.Instance.GetCurrentLocale(pid).Code : cultureCode);
				Logger.LogError("GetIgnoreWords(portalId={PortalId},cultureCode={CultureCode}): words → {Words}", portalId, cultureCode, words);

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
				Logger.LogWarning("<<< Exiting GetIgnoreWords(portalId={PortalId},cultureCode={CultureCode})", portalId, cultureCode);
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
			Logger.LogError(">>> Entering AddIgnoreWords(request={Request})", request);
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
				Logger.LogError("AddIgnoreWords(request={Request}): language → {Language}", request, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                var stopWordsId = SearchHelper.Instance.AddSearchStopWords(request.StopWords, pid, cultureCode);
				Logger.LogError("AddIgnoreWords(request={Request}): stopWordsId → {StopWordsId}", request, stopWordsId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Id = stopWordsId });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting AddIgnoreWords(request={Request})", request);
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
			Logger.LogError(">>> Entering UpdateIgnoreWords(request={Request})", request);
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
				Logger.LogError("UpdateIgnoreWords(request={Request}): language → {Language}", request, language);
                if (language == null)
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
				Logger.LogWarning("<<< Exiting UpdateIgnoreWords(request={Request})", request);
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
			Logger.LogError(">>> Entering DeleteIgnoreWords(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting DeleteIgnoreWords(request={Request})", request);
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
			Logger.LogError(">>> Entering GetLanguageSettings(portalId={PortalId})", portalId);
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
				Logger.LogError("GetLanguageSettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetLanguageSettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

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
                        l.NativeName,
                        l.EnglishName,
                        l.Name,
                        Icon = Globals.ResolveUrl(string.IsNullOrEmpty(l.Name) ? "~/images/Flags/none.gif" : $"~/images/Flags/{l.Name}.gif"),
                    }),
                    LanguageDisplayModes = languageDisplayModes,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetLanguageSettings(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering UpdateLanguageSettings(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("UpdateLanguageSettings(request={Request}): portal → {Portal}", request, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("UpdateLanguageSettings(request={Request}): portalSettings → {PortalSettings}", request, portalSettings);

                PortalController.UpdatePortalSetting(this.portalController, pid, "EnableBrowserLanguage", request.EnableBrowserLanguage.ToString());
                PortalController.UpdatePortalSetting(this.portalController, pid, "AllowUserUICulture", request.AllowUserUICulture.ToString());

                if (!portalSettings.ContentLocalizationEnabled)
                {
                    // first check whether portal default language has changed
                    string newDefaultLanguage = request.SiteDefaultLanguage;
                    if (newDefaultLanguage != portalSettings.DefaultLanguage)
                    {
                        var needToRemoveOldDefaultLanguage = LocaleController.Instance.GetLocales(pid).Count == 1;
						Logger.LogError("UpdateLanguageSettings(request={Request}): needToRemoveOldDefaultLanguage → {NeedToRemoveOldDefaultLanguage}", request, needToRemoveOldDefaultLanguage);
                        var oldDefaultLanguage = LocaleController.Instance.GetLocale(pid, portalSettings.DefaultLanguage);
						Logger.LogError("UpdateLanguageSettings(request={Request}): oldDefaultLanguage → {OldDefaultLanguage}", request, oldDefaultLanguage);
                        if (!IsLanguageEnabled(pid, newDefaultLanguage))
                        {
                            var language = LocaleController.Instance.GetLocale(pid, newDefaultLanguage);
							Logger.LogError("UpdateLanguageSettings(request={Request}): language → {Language}", request, language);
                            Localization.AddLanguageToPortal(this.eventLogger, pid, language.LanguageId, true);
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
				Logger.LogError("UpdateLanguageSettings(request={Request}): oldLanguageDisplayMode → {OldLanguageDisplayMode}", request, oldLanguageDisplayMode);
                if (request.LanguageDisplayMode != oldLanguageDisplayMode)
                {
                    var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, pid);
					Logger.LogError("UpdateLanguageSettings(request={Request}): personalization → {Personalization}", request, personalization);
                    Personalization.SetProfile(personalization, "LanguageDisplayMode", "ViewType" + pid, request.LanguageDisplayMode);
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
				Logger.LogWarning("<<< Exiting UpdateLanguageSettings(request={Request})", request);
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
			Logger.LogError(">>> Entering GetLanguages(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("GetLanguages(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetLanguages(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

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
					Logger.LogDebug("GetLanguages(portalId={PortalId}): Icon → {Icon}", portalId, Icon);
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
				Logger.LogWarning("<<< Exiting GetLanguages(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering GetLanguage(portalId={PortalId},languageId={LanguageId})", portalId, languageId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var lid = languageId ?? Null.NullInteger;
                var portalSettings = new PortalSettings(pid);
				Logger.LogError("GetLanguage(portalId={PortalId},languageId={LanguageId}): portalSettings → {PortalSettings}", portalId, languageId, portalSettings);
                var language = lid != Null.NullInteger ? LocaleController.Instance.GetLocale(lid) : null;
				Logger.LogError("GetLanguage(portalId={PortalId},languageId={LanguageId}): language → {Language}", portalId, languageId, language);

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
                                : $"~/images/Flags/{l.Name}.gif"),
                    }).ToList() : LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger))
                    .Select(l => new
                    {
						Logger.LogDebug("GetLanguage(portalId={PortalId},languageId={LanguageId}): Icon → {Icon}", portalId, languageId, Icon);
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
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetLanguage(portalId={PortalId},languageId={LanguageId})", portalId, languageId);
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
			Logger.LogError(">>> Entering GetAllLanguages()");
            try
            {
                var supportedLanguages = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger));
				Logger.LogError("GetAllLanguages(): supportedLanguages → {SupportedLanguages}", supportedLanguages);
                var cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
				Logger.LogError("GetAllLanguages(): cultures → {Cultures}", cultures);

                foreach (CultureInfo info in supportedLanguages)
                {
                    string cultureCode = info.Name;
                    CultureInfo culture = cultures.SingleOrDefault(c => c.Name == cultureCode);
					Logger.LogError("GetAllLanguages(): culture → {Culture}", culture);
                    if (culture != null)
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
                                : $"~/images/Flags/{c.Name}.gif"),
                    }),
                });
				Logger.LogDebug("GetAllLanguages(): Icon → {Icon}", Icon);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetAllLanguages()");
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
			Logger.LogError(">>> Entering AddLanguage(request={Request})", request);
            try
            {
                var pid = request.PortalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocale(request.Code) ?? new Locale { Code = request.Code };
				Logger.LogError("AddLanguage(request={Request}): language → {Language}", request, language);
				Logger.LogDebug("AddLanguage(request={Request}): language.Fallback → {Arg1}", request, language.Fallback);
                language.Code = request.Code;
				Logger.LogDebug("AddLanguage(request={Request}): language.Code → {Arg1}", request, language.Code);
                language.Fallback = request.Fallback;
                language.Text = CultureInfo.GetCultureInfo(request.Code).NativeName;
                Localization.SaveLanguage(this.eventLogger, language);

                if (!IsLanguageEnabled(pid, language.Code))
                {
                    Localization.AddLanguageToPortal(this.eventLogger, pid, language.LanguageId, true);
                }

                string roles = $"Administrators;Translator ({language.Code})";
				Logger.LogError("AddLanguage(request={Request}): roles → {Roles}", request, roles);
                PortalController.UpdatePortalSetting(this.portalController, pid, $"DefaultTranslatorRoles-{language.Code}", roles);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting AddLanguage(request={Request})", request);
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
			Logger.LogError(">>> Entering UpdateLanguageRoles(request={Request})", request);
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
				Logger.LogWarning("<<< Exiting UpdateLanguageRoles(request={Request})", request);
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
			Logger.LogError(">>> Entering UpdateLanguage(request={Request})", request);
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
					Logger.LogError("UpdateLanguage(request={Request}): enabledLanguages → {EnabledLanguages}", request, enabledLanguages);
                    var localizedTabs = this.PortalSettings.ContentLocalizationEnabled
                        ? this.tabController.GetTabsByPortal(pid).WithCulture(request.Code, false).AsList()
                        : new List<TabInfo>();
                    Locale defaultLocale = LocaleController.Instance.GetDefaultLocale(pid);
					Logger.LogError("UpdateLanguage(request={Request}): defaultLocale → {DefaultLocale}", request, defaultLocale);
                    string redirectUrl = string.Empty;
					Logger.LogError("UpdateLanguage(request={Request}): redirectUrl → {RedirectUrl}", request, redirectUrl);

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
				Logger.LogWarning("<<< Exiting UpdateLanguage(request={Request})", request);
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
			Logger.LogError(">>> Entering VerifyLanguageResourceFiles()");
            try
            {
                var files = new SortedList();
				Logger.LogError("VerifyLanguageResourceFiles(): files → {Files}", files);
                Dictionary<string, Locale> locales = LocaleController.Instance.GetLocales(Null.NullInteger);
				Logger.LogError("VerifyLanguageResourceFiles(): locales → {Locales}", locales);
                GetResourceFiles(files, HttpContext.Current.Server.MapPath("~\\"));

                var tables = new List<object>();
				Logger.LogError("VerifyLanguageResourceFiles(): tables → {Tables}", tables);

                foreach (var locale in locales.Values)
                {
                    var tableMissing = new List<string>();
					Logger.LogError("VerifyLanguageResourceFiles(): tableMissing → {TableMissing}", tableMissing);
                    var tableEntries = new List<string>();
					Logger.LogError("VerifyLanguageResourceFiles(): tableEntries → {TableEntries}", tableEntries);
                    var tableObsolete = new List<string>();
					Logger.LogError("VerifyLanguageResourceFiles(): tableObsolete → {TableObsolete}", tableObsolete);
                    var tableOld = new List<string>();
					Logger.LogError("VerifyLanguageResourceFiles(): tableOld → {TableOld}", tableOld);
                    var tableDuplicate = new List<string>();
					Logger.LogError("VerifyLanguageResourceFiles(): tableDuplicate → {TableDuplicate}", tableDuplicate);
                    var tableError = new List<string>();
					Logger.LogError("VerifyLanguageResourceFiles(): tableError → {TableError}", tableError);

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
							Logger.LogError("VerifyLanguageResourceFiles(): dsDef → {DsDef}", dsDef);
                            var dsRes = new DataSet();
							Logger.LogError("VerifyLanguageResourceFiles(): dsRes → {DsRes}", dsRes);

                            try
                            {
                                using var xmlReader = XmlReader.Create(file.Key.ToString());
								Logger.LogError("VerifyLanguageResourceFiles(): xmlReader → {XmlReader}", xmlReader);
                                dsDef.ReadXml(xmlReader);
                            }
                            catch
                            {
								Logger.LogError("Exception in VerifyLanguageResourceFiles(): {Arg0}", exc.Message);
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
								Logger.LogError("VerifyLanguageResourceFiles(): dtDef → {DtDef}", dtDef);
                                dtDef.TableName = "default";
								Logger.LogDebug("VerifyLanguageResourceFiles(): dtDef.TableName → {Arg0}", dtDef.TableName);
                                var dtRes = dsRes.Tables["data"].Copy();
								Logger.LogError("VerifyLanguageResourceFiles(): dtRes → {DtRes}", dtRes);
                                dtRes.TableName = "localized";
								Logger.LogDebug("VerifyLanguageResourceFiles(): dtRes.TableName → {Arg0}", dtRes.TableName);
                                dsDef.Tables.Add(dtRes);

                                // Check for duplicate entries in localized file
                                try
                                {
                                    // if this fails-> file contains duplicates
                                    var c = new UniqueConstraint("uniqueness", dtRes.Columns["name"]);
									Logger.LogError("VerifyLanguageResourceFiles(): c → {C}", c);
                                    dtRes.Constraints.Add(c);
                                    dtRes.Constraints.Remove("uniqueness");
                                }
                                catch
                                {
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
								Logger.LogError("VerifyLanguageResourceFiles(): resFile → {ResFile}", resFile);
                                if (((FileInfo)file.Value).LastWriteTime > resFile.LastWriteTime)
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
                            : $"~/images/Flags/{locale.Code}.gif"),
                        MissingFiles = tableMissing,
                        FilesWithDuplicateEntries = tableDuplicate,
                        FilesWithMissingEntries = tableEntries,
                        FilesWithObsoleteEntries = tableObsolete,
                        OldFiles = tableOld,
                        MalformedFiles = tableError,
                    });
					Logger.LogDebug("VerifyLanguageResourceFiles(): Icon → {Icon}", Icon);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Results = tables,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting VerifyLanguageResourceFiles()");
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
			Logger.LogError(">>> Entering GetModuleList(type={Type})", type);
            try
            {
                List<object> modules = new List<object>();
				Logger.LogError("GetModuleList(type={Type}): modules → {Modules}", type, modules);
                switch (type)
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
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetModuleList(type={Type})", type);
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
			Logger.LogError(">>> Entering CreateLanguagePack(request={Request})", request);
            try
            {
                bool created = false;
				Logger.LogError("CreateLanguagePack(request={Request}): created → {Created}", request, created);
                switch (request.PackType)
                {
                    case "Core":
                        created = this.controller.CreateCorePackage(request.CultureCode, request.FileName, true);
                        break;
                    case "Module":
                        foreach (int moduleId in request.ModuleIds)
                        {
                            DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModule(this.hostSettings, moduleId, Null.NullInteger);
							Logger.LogError("CreateLanguagePack(request={Request}): desktopModule → {DesktopModule}", request, desktopModule);
                            created = this.controller.CreateModulePackage(request.CultureCode, desktopModule, true);
                        }

                        break;
                    case "Provider":
                        foreach (int moduleId in request.ModuleIds)
                        {
                            PackageInfo provider = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == moduleId);
							Logger.LogError("CreateLanguagePack(request={Request}): provider → {Provider}", request, provider);
                            created = this.controller.CreateProviderPackage(request.CultureCode, provider, true);
                        }

                        break;
                    case "AuthSystem":
                        foreach (int moduleId in request.ModuleIds)
                        {
                            PackageInfo authSystem = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == moduleId);
							Logger.LogError("CreateLanguagePack(request={Request}): authSystem → {AuthSystem}", request, authSystem);
                            created = this.controller.CreateAuthSystemPackage(request.CultureCode, authSystem, true);
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
				Logger.LogWarning("<<< Exiting CreateLanguagePack(request={Request})", request);
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
			Logger.LogError(">>> Entering GetTranslatorRoles(portalId={PortalId},groupId={GroupId},cultureCode={CultureCode})", portalId, groupId, cultureCode);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = LocaleController.Instance.GetLocale(cultureCode);
				Logger.LogError("GetTranslatorRoles(portalId={PortalId},groupId={GroupId},cultureCode={CultureCode}): language → {Language}", portalId, groupId, cultureCode, language);
                if (language == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest,
                        string.Format(CultureInfo.CurrentCulture, Localization.GetString("InvalidLocale.ErrorMessage", Components.Constants.Constants.LocalResourcesFile), cultureCode));
                }

                string defaultRoles = PortalController.GetPortalSetting(this.portalController, $"DefaultTranslatorRoles-{cultureCode}", pid, "Administrators");
                var selectedRoleNames = new ArrayList(defaultRoles.Split(';'));
				Logger.LogError("GetTranslatorRoles(portalId={PortalId},groupId={GroupId},cultureCode={CultureCode}): selectedRoleNames → {SelectedRoleNames}", portalId, groupId, cultureCode, selectedRoleNames);

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
				Logger.LogWarning("<<< Exiting GetTranslatorRoles(portalId={PortalId},groupId={GroupId},cultureCode={CultureCode})", portalId, groupId, cultureCode);
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
			Logger.LogError(">>> Entering GetTranslatorRoleGroups(portalId={PortalId})", portalId);
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
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetTranslatorRoleGroups(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering GetOtherSettings(portalId={PortalId})", portalId);
            try
            {
                var pid = portalId ?? this.PortalId;
                if (!this.UserInfo.IsSuperUser && this.PortalId != pid)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
				Logger.LogError("GetOtherSettings(portalId={PortalId}): portal → {Portal}", portalId, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetOtherSettings(portalId={PortalId}): portalSettings → {PortalSettings}", portalId, portalSettings);

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
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetOtherSettings(portalId={PortalId})", portalId);
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
			Logger.LogError(">>> Entering UpdateOtherSettings(request={Request})", request);
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
					Logger.LogError("UpdateOtherSettings(request={Request}): allowList → {AllowList}", request, allowList);
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
				Logger.LogWarning("<<< Exiting UpdateOtherSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static void GetResourceFiles(SortedList fileList, string path)
        {
			Logger.LogError(">>> Entering GetResourceFiles(fileList={FileList},path={Path})", fileList, path);
            var folders = Directory.GetDirectories(path);

			Logger.LogError("GetResourceFiles(fileList={FileList},path={Path}): folders → {Folders}", fileList, path, folders);
            foreach (var folder in folders)
            {
                var objFolder = new DirectoryInfo(folder);
				Logger.LogError("GetResourceFiles(fileList={FileList},path={Path}): objFolder → {ObjFolder}", fileList, path, objFolder);

                bool resxFilesDirectory = objFolder.Name.Equals(Localization.LocalResourceDirectory, StringComparison.OrdinalIgnoreCase) ||
                                          objFolder.Name.Equals(Localization.ApplicationResourceDirectory.Replace("~/", string.Empty), StringComparison.OrdinalIgnoreCase) ||
                                          folder.EndsWith("\\portals\\_default", StringComparison.OrdinalIgnoreCase);

                if (resxFilesDirectory)
                {
                    var sysLocale = Localization.SystemLocale.ToLowerInvariant();
					Logger.LogError("GetResourceFiles(fileList={FileList},path={Path}): sysLocale → {SysLocale}", fileList, path, sysLocale);
                    foreach (var file in Directory.GetFiles(objFolder.FullName, "*.resx"))
                    {
                        var fileInfo = new FileInfo(file);
						Logger.LogError("GetResourceFiles(fileList={FileList},path={Path}): fileInfo → {FileInfo}", fileList, path, fileInfo);
                        var match = LanguagesController.FileInfoRegex.Match(fileInfo.Name);
						Logger.LogError("GetResourceFiles(fileList={FileList},path={Path}): match → {Match}", fileList, path, match);

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
			Logger.LogWarning("<<< Exiting GetResourceFiles(fileList={FileList},path={Path})", fileList, path);
        }

        private static bool IsLanguagePublished(int portalId, string code)
        {
			Logger.LogError(">>> Entering IsLanguagePublished(portalId={PortalId},code={Code})", portalId, code);
            bool isPublished = Null.NullBoolean;
			Logger.LogError("IsLanguagePublished(portalId={PortalId},code={Code}): isPublished → {IsPublished}", portalId, code, isPublished);
            if (LocaleController.Instance.GetLocales(portalId).TryGetValue(code, out var enabledLanguage))
            {
                isPublished = enabledLanguage.IsPublished;
            }

			Logger.LogWarning("<<< Exiting IsLanguagePublished(portalId={PortalId},code={Code})", portalId, code);
            return isPublished;
        }

        private static bool IsDefaultLanguage(PortalSettings portalSettings, string code)
        {
			Logger.LogError(">>> Entering IsDefaultLanguage(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            Logger.LogWarning("<<< Exiting IsDefaultLanguage(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            return code == portalSettings.DefaultLanguage;
        }

        private static bool CanDeleteProperty(ProfilePropertyDefinition definition)
        {
			Logger.LogError(">>> Entering CanDeleteProperty(definition={Definition})", definition);
            switch (definition.PropertyName.ToLowerInvariant())
            {
                case "lastname":
                case "firstname":
                case "preferredtimezone":
                case "preferredlocale":
                    return false;
                default:
					Logger.LogWarning("<<< Exiting CanDeleteProperty(definition={Definition})", definition);
                    return true;
            }
        }

        private static string DisplayDataType(ListController listController, int dataType)
        {
			Logger.LogError(">>> Entering DisplayDataType(listController={ListController},dataType={DataType})", listController, dataType);
            var retValue = Null.NullString;
            var definitionEntry = listController.GetListEntryInfo("DataType", dataType);
			Logger.LogError("DisplayDataType(listController={ListController},dataType={DataType}): retValue → {RetValue}", listController, dataType, retValue);
            if (definitionEntry != null)
            {
				Logger.LogError("DisplayDataType(listController={ListController},dataType={DataType}): definitionEntry → {DefinitionEntry}", listController, dataType, definitionEntry);
                retValue = definitionEntry.Value;
            }

			Logger.LogWarning("<<< Exiting DisplayDataType(listController={ListController},dataType={DataType})", listController, dataType);
            return retValue;
        }

        private static bool IsLanguageEnabled(int portalId, string code)
        {
			Logger.LogError(">>> Entering IsLanguageEnabled(portalId={PortalId},code={Code})", portalId, code);
            Logger.LogWarning("<<< Exiting IsLanguageEnabled(portalId={PortalId},code={Code})", portalId, code);
            return LocaleController.Instance.GetLocales(portalId).TryGetValue(code, out _);
        }

        private static bool CanEnableDisable(PortalSettings portalSettings, string code)
        {
			Logger.LogError(">>> Entering CanEnableDisable(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            bool canEnable;
            if (IsLanguageEnabled(portalSettings.PortalId, code))
            {
                canEnable = !IsDefaultLanguage(portalSettings, code) && !IsLanguagePublished(portalSettings.PortalId, code);
				Logger.LogError("CanEnableDisable(portalSettings={PortalSettings},code={Code}): canEnable → {CanEnable}", portalSettings, code, canEnable);
            }
            else
            {
                canEnable = !IsDefaultLanguage(portalSettings, code);
				Logger.LogDebug("CanEnableDisable(portalSettings={PortalSettings},code={Code}): canEnable → {CanEnable}", portalSettings, code, canEnable);
            }

			Logger.LogWarning("<<< Exiting CanEnableDisable(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            return canEnable;
        }

        private static SearchStatistics GetSearchStatistics()
        {
			Logger.LogError(">>> Entering GetSearchStatistics()");
            try
            {
                return InternalSearchController.Instance.GetSearchStatistics();
            }
            catch (SearchIndexEmptyException)
            {
				Logger.LogError("Exception in GetSearchStatistics(): {Arg0}", SearchIndexEmptyException.Message);
				Logger.LogWarning("<<< Exiting GetSearchStatistics()");
                return null;
            }
        }

        private static void ClearEntriesCache(string listName, int portalId)
        {
			Logger.LogError(">>> Entering ClearEntriesCache(listName={ListName},portalId={PortalId})", listName, portalId);
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.ListEntriesCacheKey, portalId, listName);
			Logger.LogWarning("<<< Exiting ClearEntriesCache(listName={ListName},portalId={PortalId})", listName, portalId);
            DataCache.RemoveCache(cacheKey);
        }

        private static int GetLocalizedTabIdSetting(Dictionary<string, string> localizedPortalSettings, string settingKey)
        {
			Logger.LogError(">>> Entering GetLocalizedTabIdSetting(localizedPortalSettings={LocalizedPortalSettings},settingKey={SettingKey})", localizedPortalSettings, settingKey);
            if (localizedPortalSettings.TryGetValue(settingKey, out var settingValue))
            {
                int tabId = int.TryParse(settingValue, out tabId) ? tabId : -1;
				Logger.LogError("GetLocalizedTabIdSetting(localizedPortalSettings={LocalizedPortalSettings},settingKey={SettingKey}): tabId → {TabId}", localizedPortalSettings, settingKey, tabId);
                return tabId;
            }

			Logger.LogWarning("<<< Exiting GetLocalizedTabIdSetting(localizedPortalSettings={LocalizedPortalSettings},settingKey={SettingKey})", localizedPortalSettings, settingKey);
            return -1;
        }

        private string GetTranslatedPages(PortalSettings portalSettings, string code)
        {
			Logger.LogError(">>> Entering GetTranslatedPages(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            string status = string.Empty;
			Logger.LogError("GetTranslatedPages(portalSettings={PortalSettings},code={Code}): status → {Status}", portalSettings, code, status);
            if (!IsDefaultLanguage(portalSettings, code) && this.IsLocalized(portalSettings, code))
            {
                int translatedCount = (from t in this.tabController.GetTabsByPortal(portalSettings.PortalId).WithCulture(code, false).Values where t.IsTranslated && !t.IsDeleted select t).Count();
				Logger.LogError("GetTranslatedPages(portalSettings={PortalSettings},code={Code}): translatedCount → {TranslatedCount}", portalSettings, code, translatedCount);
                status = translatedCount.ToString(CultureInfo.InvariantCulture);
            }

			Logger.LogWarning("<<< Exiting GetTranslatedPages(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            return status;
        }

        private string GetLocalizedStatus(PortalSettings portalSettings, string code)
        {
			Logger.LogError(">>> Entering GetLocalizedStatus(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            string status = string.Empty;
			Logger.LogError("GetLocalizedStatus(portalSettings={PortalSettings},code={Code}): status → {Status}", portalSettings, code, status);
            if (!IsDefaultLanguage(portalSettings, code) && this.IsLocalized(portalSettings, code))
            {
                int defaultPageCount = this.GetLocalizedPages(portalSettings.PortalId, portalSettings.DefaultLanguage, false).Count;
				Logger.LogError("GetLocalizedStatus(portalSettings={PortalSettings},code={Code}): defaultPageCount → {DefaultPageCount}", portalSettings, code, defaultPageCount);
                int currentPageCount = this.GetLocalizedPages(portalSettings.PortalId, code, false).Count;
				Logger.LogError("GetLocalizedStatus(portalSettings={PortalSettings},code={Code}): currentPageCount → {CurrentPageCount}", portalSettings, code, currentPageCount);
                status = $"{currentPageCount / (float)defaultPageCount:#0%}";
            }

			Logger.LogWarning("<<< Exiting GetLocalizedStatus(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            return status;
        }

        private string GetLocalizablePages(int portalId, string code)
        {
			Logger.LogError(">>> Entering GetLocalizablePages(portalId={PortalId},code={Code})", portalId, code);
            int count = this.GetLocalizedPages(portalId, code, false).Count(t => !t.Value.IsDeleted);
			Logger.LogError("GetLocalizablePages(portalId={PortalId},code={Code}): count → {Count}", portalId, code, count);
			Logger.LogWarning("<<< Exiting GetLocalizablePages(portalId={PortalId},code={Code})", portalId, code);
            return count.ToString(CultureInfo.CurrentCulture);
        }

        private TabCollection GetLocalizedPages(int portalId, string code, bool includeNeutral)
        {
			Logger.LogError(">>> Entering GetLocalizedPages(portalId={PortalId},code={Code},includeNeutral={IncludeNeutral})", portalId, code, includeNeutral);
            Logger.LogWarning("<<< Exiting GetLocalizedPages(portalId={PortalId},code={Code},includeNeutral={IncludeNeutral})", portalId, code, includeNeutral);
            return this.tabController.GetTabsByPortal(portalId).WithCulture(code, includeNeutral);
        }

        private int GetPublishedLocalizedPages(int portalId, string code)
        {
			Logger.LogError(">>> Entering GetPublishedLocalizedPages(portalId={PortalId},code={Code})", portalId, code);
            var localizedTabs = this.tabController.GetTabsByPortal(portalId).WithCulture(code, false);
			Logger.LogError("GetPublishedLocalizedPages(portalId={PortalId},code={Code}): localizedTabs → {LocalizedTabs}", portalId, code, localizedTabs);
			Logger.LogWarning("<<< Exiting GetPublishedLocalizedPages(portalId={PortalId},code={Code})", portalId, code);
            return localizedTabs.Count(t => this.tabController.IsTabPublished(t.Value));
        }

        private string GetTranslatedStatus(PortalSettings portalSettings, string code)
        {
			Logger.LogError(">>> Entering GetTranslatedStatus(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            string status = string.Empty;
			Logger.LogError("GetTranslatedStatus(portalSettings={PortalSettings},code={Code}): status → {Status}", portalSettings, code, status);
            if (!IsDefaultLanguage(portalSettings, code) && this.IsLocalized(portalSettings, code))
            {
                int localizedCount = this.GetLocalizedPages(portalSettings.PortalId, code, false).Count;
				Logger.LogError("GetTranslatedStatus(portalSettings={PortalSettings},code={Code}): localizedCount → {LocalizedCount}", portalSettings, code, localizedCount);
                int translatedCount = (from t in this.tabController.GetTabsByPortal(portalSettings.PortalId).WithCulture(code, false).Values where t.IsTranslated select t).Count();
				Logger.LogError("GetTranslatedStatus(portalSettings={PortalSettings},code={Code}): translatedCount → {TranslatedCount}", portalSettings, code, translatedCount);
                status = $"{translatedCount / (float)localizedCount:#0%}";
            }

			Logger.LogWarning("<<< Exiting GetTranslatedStatus(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            return status;
        }

        private bool IsLocalized(PortalSettings portalSettings, string code)
        {
			Logger.LogError(">>> Entering IsLocalized(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            Logger.LogWarning("<<< Exiting IsLocalized(portalSettings={PortalSettings},code={Code})", portalSettings, code);
            return code != portalSettings.DefaultLanguage && this.GetLocalizedPages(portalSettings.PortalId, code, false).Count > 0;
        }

        private TabInfo TabSanitizer(int tabId, int portalId)
        {
			Logger.LogError(">>> Entering TabSanitizer(tabId={TabId},portalId={PortalId})", tabId, portalId);
            var tab = this.tabController.GetTab(tabId, portalId);
			Logger.LogError("TabSanitizer(tabId={TabId},portalId={PortalId}): tab → {Tab}", tabId, portalId, tab);
            if (tab is { IsDeleted: false, })
            {
                return tab;
            }
            else
            {
				Logger.LogWarning("<<< Exiting TabSanitizer(tabId={TabId},portalId={PortalId})", tabId, portalId);
                return null;
            }
        }

        private int ValidateTabId(int tabId, int portalId)
        {
			Logger.LogError(">>> Entering ValidateTabId(tabId={TabId},portalId={PortalId})", tabId, portalId);
            var tab = this.tabController.GetTab(tabId, portalId);
			Logger.LogError("ValidateTabId(tabId={TabId},portalId={PortalId}): tab → {Tab}", tabId, portalId, tab);
			Logger.LogWarning("<<< Exiting ValidateTabId(tabId={TabId},portalId={PortalId})", tabId, portalId);
            return tab is { IsDeleted: false, } ? tab.TabID : Null.NullInteger;
        }

        private bool IsHttpAliasValid(string strAlias)
        {
			Logger.LogError(">>> Entering IsHttpAliasValid(strAlias={StrAlias})", strAlias);
            bool isValid = true;
			Logger.LogError("IsHttpAliasValid(strAlias={StrAlias}): isValid → {IsValid}", strAlias, isValid);
            if (string.IsNullOrEmpty(strAlias))
            {
                isValid = false;
            }
            else
            {
                if (strAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("://", StringComparison.Ordinal) + 3);
					Logger.LogError("IsHttpAliasValid(strAlias={StrAlias}): strAlias → {StrAlias1}", strAlias, strAlias);
                }

                if (strAlias.IndexOf("\\\\", StringComparison.Ordinal) != -1)
                {
                    strAlias = strAlias.Remove(0, strAlias.IndexOf("\\\\", StringComparison.Ordinal) + 2);
                }

                // Validate Alias, this needs to be done with lowercase, downstream we only check with lowercase variables
                if (!this.portalAliasService.ValidateAlias(strAlias.ToLowerInvariant(), false))
                {
                    isValid = false;
                }
            }

			Logger.LogWarning("<<< Exiting IsHttpAliasValid(strAlias={StrAlias})", strAlias);
            return isValid;
        }

        private string ResourceFile(string filename, string language)
        {
			Logger.LogError(">>> Entering ResourceFile(filename={Filename},language={Language})", filename, language);
            Logger.LogWarning("<<< Exiting ResourceFile(filename={Filename},language={Language})", filename, language);
            return Localization.GetResourceFileName(filename, language, string.Empty, Globals.GetPortalSettings(this.hostSettings).PortalId);
        }

        private string GetAbsoluteServerPath()
        {
			Logger.LogError(">>> Entering GetAbsoluteServerPath()");
            if (this.Request.Properties["MS_HttpContext"] is HttpContextWrapper httpContext)
            {
                var strServerPath = httpContext.Request.MapPath(httpContext.Request.ApplicationPath);
				Logger.LogError("GetAbsoluteServerPath(): strServerPath → {StrServerPath}", strServerPath);
                if (!strServerPath.EndsWith(@"\", StringComparison.Ordinal))
                {
                    strServerPath += @"\";
                }

                return strServerPath;
            }
            else
            {
				Logger.LogWarning("<<< Exiting GetAbsoluteServerPath()");
                return string.Empty;
            }
        }

        private bool ValidateProperty(ProfilePropertyDefinition definition, out HttpResponseMessage httpPropertyValidationError)
        {
			Logger.LogError(">>> Entering ValidateProperty(definition={Definition})", definition);
            bool isValid = true;
            httpPropertyValidationError = null;
			Logger.LogDebug("ValidateProperty(definition={Definition}): httpPropertyValidationError → {HttpPropertyValidationError}", definition, httpPropertyValidationError);
            string strDataType = this.listController.GetListEntryInfo("DataType", definition.DataType).Value;
			Logger.LogError("ValidateProperty(definition={Definition}): isValid → {IsValid}", definition, isValid);
            Regex propertyNameRegex = new Regex("^[a-zA-Z0-9]+$");
            if (!propertyNameRegex.IsMatch(definition.PropertyName))
            {
				Logger.LogError("ValidateProperty(definition={Definition}): propertyNameRegex → {PropertyNameRegex}", definition, propertyNameRegex);
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

			Logger.LogWarning("<<< Exiting ValidateProperty(definition={Definition})", definition);
            return isValid;
        }

        private CultureDropDownTypes GetCultureDropDownType(int portalId)
        {
			Logger.LogError(">>> Entering GetCultureDropDownType(portalId={PortalId})", portalId);
            CultureDropDownTypes displayType;
            string viewType = this.GetLanguageDisplayMode(portalId);
            switch (viewType)
            {
                case "NATIVE":
                    displayType = CultureDropDownTypes.NativeName;
					Logger.LogError("GetCultureDropDownType(portalId={PortalId}): displayType → {DisplayType}", portalId, displayType);
					Logger.LogDebug("GetCultureDropDownType(portalId={PortalId}): displayType → {DisplayType}", portalId, displayType);
                    break;
                case "ENGLISH":
                    displayType = CultureDropDownTypes.EnglishName;
                    break;
                default:
                    displayType = CultureDropDownTypes.DisplayName;
                    break;
            }

			Logger.LogWarning("<<< Exiting GetCultureDropDownType(portalId={PortalId})", portalId);
            return displayType;
        }

        private string GetLanguageDisplayMode(int portalId)
        {
			Logger.LogError(">>> Entering GetLanguageDisplayMode(portalId={PortalId})", portalId);
            string viewTypePersonalizationKey = "LanguageDisplayMode:ViewType" + portalId;
            var personalization = this.personalizationController.LoadProfile(this.UserInfo.UserID, portalId);
			Logger.LogError("GetLanguageDisplayMode(portalId={PortalId}): personalization → {Personalization}", portalId, personalization);

            string viewType = Convert.ToString(personalization.Profile[viewTypePersonalizationKey], CultureInfo.InvariantCulture);
			Logger.LogWarning("<<< Exiting GetLanguageDisplayMode(portalId={PortalId})", portalId);
            return string.IsNullOrEmpty(viewType) ? "NATIVE" : viewType;
        }
    }
}
