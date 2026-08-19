// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Themes.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Xml;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Themes.Components;
    using Dnn.PersonaBar.Themes.Components.DTO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class ThemesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));
        private readonly IThemesController controller;
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly ILocaleController localeController;

        /// <summary>Initializes a new instance of the <see cref="ThemesController"/> class.</summary>
        public ThemesController()
            : this(null, null, null, null)
        {
		Logger.LogDebug(">>> Entering ThemesController()");
		Logger.LogDebug("<<< Exiting ThemesController()");
		Logger.LogDebug(">>> Entering ThemesController(controller={Controller},hostSettings={HostSettings},appStatus={AppStatus},localeController={LocaleController})", controller, hostSettings, appStatus, localeController);
		Logger.LogDebug("ThemesController(controller={Controller},hostSettings={HostSettings},appStatus={AppStatus},localeController={LocaleController}): hostSettings → {HostSettings1}", controller, hostSettings, appStatus, localeController, hostSettings);
		Logger.LogDebug("ThemesController(controller={Controller},hostSettings={HostSettings},appStatus={AppStatus},localeController={LocaleController}): appStatus → {AppStatus1}", controller, hostSettings, appStatus, localeController, appStatus);
		Logger.LogDebug("ThemesController(controller={Controller},hostSettings={HostSettings},appStatus={AppStatus},localeController={LocaleController}): localeController → {LocaleController1}", controller, hostSettings, appStatus, localeController, localeController);
        }

        /// <summary>Initializes a new instance of the <see cref="ThemesController"/> class.</summary>
        /// <param name="controller">The themes controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="localeController">The locale controller.</param>
        public ThemesController(IThemesController controller, IHostSettings hostSettings, IApplicationStatusInfo appStatus, ILocaleController localeController)
        {
            this.controller = controller ?? Globals.GetCurrentServiceProvider().GetRequiredService<IThemesController>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.localeController = localeController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaleController>();
			Logger.LogDebug("<<< Exiting ThemesController(controller={Controller},hostSettings={HostSettings},appStatus={AppStatus},localeController={LocaleController})", controller, hostSettings, appStatus, localeController);
        }

        [HttpGet]
        public HttpResponseMessage GetCurrentTheme(string language)
        {
			Logger.LogDebug(">>> Entering GetCurrentTheme(language={Language})", language);
            try
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetCurrentTheme(language={Language})", language);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetThemes(ThemeLevel level)
        {
			Logger.LogDebug(">>> Entering GetThemes(level={Level})", level);
            try
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Layouts = this.controller.GetLayouts(this.PortalSettings, level),
                    Containers = this.controller.GetContainers(this.PortalSettings, level),
                });
				Logger.LogDebug("GetThemes(level={Level}): Layouts → {Layouts}", level, Layouts);
				Logger.LogDebug("GetThemes(level={Level}): Containers → {Containers}", level, Containers);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetThemes(level={Level})", level);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetThemeFiles(string themeName, ThemeType type, ThemeLevel level)
        {
			Logger.LogDebug(">>> Entering GetThemeFiles(themeName={ThemeName},type={Type},level={Level})", themeName, type, level);
            try
            {
                var theme = (type == ThemeType.Skin ? this.controller.GetLayouts(this.PortalSettings, level)
                                                    : this.controller.GetContainers(this.PortalSettings, level))
                            .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase));

                if (theme == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "ThemeNotFound");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, this.controller.GetThemeFiles(this.PortalSettings, theme));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetThemeFiles(themeName={ThemeName},type={Type},level={Level})", themeName, type, level);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage ApplyTheme(ApplyThemeInfo applyTheme, string language)
        {
			Logger.LogDebug(">>> Entering ApplyTheme(applyTheme={ApplyTheme},language={Language})", applyTheme, language);
            try
            {
                this.controller.ApplyTheme(this.PortalId, applyTheme.ThemeFile, applyTheme.Scope);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting ApplyTheme(applyTheme={ApplyTheme},language={Language})", applyTheme, language);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage ApplyDefaultTheme(ApplyDefaultThemeInfo defaultTheme, string language)
        {
			Logger.LogDebug(">>> Entering ApplyDefaultTheme(defaultTheme={DefaultTheme},language={Language})", defaultTheme, language);
            try
            {
                var themeInfo = this.controller.GetLayouts(this.PortalSettings, ThemeLevel.All)
                                    .FirstOrDefault(
                                        t => t.PackageName.Equals(defaultTheme.ThemeName, StringComparison.OrdinalIgnoreCase)
                                                && t.Level == defaultTheme.Level);

                if (themeInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "ThemeNotFound");
                }

                var themeFiles = this.controller.GetThemeFiles(this.PortalSettings, themeInfo);
				Logger.LogDebug("ApplyDefaultTheme(defaultTheme={DefaultTheme},language={Language}): themeFiles → {ThemeFiles}", defaultTheme, language, themeFiles);
                if (themeFiles.Count == 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "NoThemeFile");
                }

                this.controller.ApplyDefaultTheme(this.PortalSettings, defaultTheme.ThemeName, defaultTheme.Level);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting ApplyDefaultTheme(defaultTheme={DefaultTheme},language={Language})", defaultTheme, language);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage DeleteThemePackage(ThemeInfo theme)
        {
			Logger.LogDebug(">>> Entering DeleteThemePackage(theme={Theme})", theme);
            try
            {
                if (theme.Level == ThemeLevel.Global && !this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "NoPermission");
                }

                this.controller.DeleteThemePackage(this.PortalSettings, theme);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (InvalidOperationException ex)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting DeleteThemePackage(theme={Theme})", theme);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetEditableTokens()
        {
			Logger.LogDebug(">>> Entering GetEditableTokens()");
            try
            {
                var tokens = SkinControlController.GetSkinControls().Values
                    .Where(c => !string.IsNullOrEmpty(c.ControlKey) && !string.IsNullOrEmpty(c.ControlSrc))
                    .Select(c => new ListItemInfo { Text = c.ControlKey, Value = c.ControlSrc });

                return this.Request.CreateResponse(HttpStatusCode.OK, tokens);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting GetEditableTokens()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEditableSettings(string token)
        {
			Logger.LogDebug(">>> Entering GetEditableSettings(token=<redacted>)");
            try
            {
                if (!SkinControlController.GetSkinControls().Values.Select(sc => sc.ControlSrc).ToHashSet(StringComparer.OrdinalIgnoreCase).Contains(token))
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                var strFile = this.appStatus.ApplicationMapPath + @"\" + token.ToLowerInvariant().Replace("/", @"\").Replace(".ascx", ".xml");
				Logger.LogDebug("GetEditableSettings(token=<redacted>): strFile → {StrFile}", strFile);
                var settings = new List<ListItemInfo>();
				Logger.LogDebug("GetEditableSettings(token=<redacted>): settings → {Settings}", settings);
                if (File.Exists(strFile))
                {
                    var xmlDoc = new XmlDocument { XmlResolver = null };
                    using (var xmlReader = XmlReader.Create(strFile, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        xmlDoc.Load(xmlReader);
                    }

                    foreach (XmlNode xmlSetting in xmlDoc.SelectNodes("//Settings/Setting"))
                    {
                        settings.Add(new ListItemInfo(xmlSetting.SelectSingleNode("Name").InnerText, xmlSetting.SelectSingleNode("Name").InnerText));
                    }
                }

				Logger.LogDebug("<<< Exiting GetEditableSettings(token=<redacted>)");
                return this.Request.CreateResponse(HttpStatusCode.OK, settings);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetEditableValues(string token, string setting)
        {
			Logger.LogDebug(">>> Entering GetEditableValues(token=<redacted>,setting={Setting})", setting);
            try
            {
                if (!SkinControlController.GetSkinControls().Values.Select(sc => sc.ControlSrc).ToHashSet(StringComparer.OrdinalIgnoreCase).Contains(token))
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                var strFile = this.appStatus.ApplicationMapPath + @"\" + token.ToLowerInvariant().Replace("/", @"\").Replace(".ascx", ".xml");
				Logger.LogDebug("GetEditableValues(token=<redacted>,setting={Setting}): strFile → {StrFile}", setting, strFile);
                var value = string.Empty;
				Logger.LogDebug("GetEditableValues(token=<redacted>,setting={Setting}): value → {Value}", setting, value);
                if (File.Exists(strFile))
                {
                    var xmlDoc = new XmlDocument { XmlResolver = null };
                    using (var xmlReader = XmlReader.Create(strFile, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        xmlDoc.Load(xmlReader);
                    }

                    foreach (XmlNode xmlSetting in xmlDoc.SelectNodes("//Settings/Setting"))
                    {
                        if (xmlSetting.SelectSingleNode("Name").InnerText == setting)
                        {
                            string strValue = xmlSetting.SelectSingleNode("Value").InnerText;
                            switch (strValue)
                            {
                                case "":
                                    break;
                                case "[TABID]":
                                    value = "Pages";
                                    break;
                                default:
                                    value = strValue;
                                    break;
                            }
                        }
                    }
                }

				Logger.LogDebug("<<< Exiting GetEditableValues(token=<redacted>,setting={Setting})", setting);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Value = value });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage UpdateTheme(UpdateThemeInfo updateTheme)
        {
			Logger.LogDebug(">>> Entering UpdateTheme(updateTheme={UpdateTheme})", updateTheme);
            try
            {
                var token = SkinControlController.GetSkinControls().Values.FirstOrDefault(t => t.ControlSrc == updateTheme.Token);
				Logger.LogWarning("UpdateTheme(updateTheme={UpdateTheme}): token → {Token}", updateTheme, token);
                if (token == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidParameter");
                }

                var themeFilePath = updateTheme.Path;
                if ((!themeFilePath.StartsWith("[g]", StringComparison.OrdinalIgnoreCase) && !themeFilePath.StartsWith("[l]", StringComparison.OrdinalIgnoreCase) && !themeFilePath.StartsWith("[s]", StringComparison.OrdinalIgnoreCase))
                    || (themeFilePath.StartsWith("[g]", StringComparison.OrdinalIgnoreCase) && !this.UserInfo.IsSuperUser))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPermission");
                }

                updateTheme.Token = token.ControlKey;
				Logger.LogDebug("UpdateTheme(updateTheme={UpdateTheme}): updateTheme.Token → {Arg1}", updateTheme, updateTheme.Token);
                this.controller.UpdateTheme(this.PortalSettings, updateTheme);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting UpdateTheme(updateTheme={UpdateTheme})", updateTheme);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage ParseTheme(ParseThemeInfo parseTheme)
        {
			Logger.LogDebug(">>> Entering ParseTheme(parseTheme={ParseTheme})", parseTheme);
            try
            {
                var themeName = parseTheme.ThemeName;

                var layout = this.controller.GetLayouts(this.PortalSettings, ThemeLevel.All)
                                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == parseTheme.Level);

                if (layout != null)
                {
                    this.controller.ParseTheme(this.PortalSettings, layout, parseTheme.ParseType);
                }

                var container = this.controller.GetContainers(this.PortalSettings, ThemeLevel.All)
                                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == parseTheme.Level);

                if (container != null)
                {
                    this.controller.ParseTheme(this.PortalSettings, container, parseTheme.ParseType);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting ParseTheme(parseTheme={ParseTheme})", parseTheme);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.Edit)]
        public HttpResponseMessage RestoreTheme(string language)
        {
			Logger.LogDebug(">>> Entering RestoreTheme(language={Language})", language);
            try
            {
                SkinController.SetSkin(SkinController.RootSkin, this.PortalId, SkinType.Portal, string.Empty);
                SkinController.SetSkin(SkinController.RootContainer, this.PortalId, SkinType.Portal, string.Empty);
                SkinController.SetSkin(SkinController.RootSkin, this.PortalId, SkinType.Admin, string.Empty);
                SkinController.SetSkin(SkinController.RootContainer, this.PortalId, SkinType.Admin, string.Empty);
                DataCache.ClearPortalCache(this.PortalId, true);

                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetCurrentThemeObject());
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogDebug("<<< Exiting RestoreTheme(language={Language})", language);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private object GetCurrentThemeObject()
        {
			Logger.LogDebug(">>> Entering GetCurrentThemeObject()");
            var cultureCode = this.localeController.GetCurrentLocale(this.PortalId).Code;
            var siteLayout = PortalController.GetPortalSetting("DefaultPortalSkin", this.PortalId, this.hostSettings.DefaultPortalSkin, cultureCode);
			Logger.LogDebug("GetCurrentThemeObject(): siteLayout → {SiteLayout}", siteLayout);
            var siteContainer = PortalController.GetPortalSetting("DefaultPortalContainer", this.PortalId, this.hostSettings.DefaultPortalContainer, cultureCode);
			Logger.LogDebug("GetCurrentThemeObject(): siteContainer → {SiteContainer}", siteContainer);
            var editLayout = PortalController.GetPortalSetting("DefaultAdminSkin", this.PortalId, this.hostSettings.DefaultAdminSkin, cultureCode);
			Logger.LogDebug("GetCurrentThemeObject(): editLayout → {EditLayout}", editLayout);
            var editContainer = PortalController.GetPortalSetting("DefaultAdminContainer", this.PortalId, this.hostSettings.DefaultAdminContainer, cultureCode);
			Logger.LogDebug("GetCurrentThemeObject(): editContainer → {EditContainer}", editContainer);
			Logger.LogDebug("GetCurrentThemeObject(): cultureCode → {CultureCode}", cultureCode);

			Logger.LogDebug("<<< Exiting GetCurrentThemeObject()");
            return new
            {
                SiteLayout = this.controller.GetThemeFile(this.PortalSettings, siteLayout, ThemeType.Skin),
                SiteContainer = this.controller.GetThemeFile(this.PortalSettings, siteContainer, ThemeType.Container),
                EditLayout = this.controller.GetThemeFile(this.PortalSettings, editLayout, ThemeType.Skin),
                EditContainer = this.controller.GetThemeFile(this.PortalSettings, editContainer, ThemeType.Container),
                Logger.LogDebug("GetCurrentThemeObject(): SiteLayout → {SiteLayout}", SiteLayout);
            };
        }
    }
}
