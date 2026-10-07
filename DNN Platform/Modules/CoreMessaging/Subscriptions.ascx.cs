// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements the logic of the Subscription view.</summary>
    /// <param name="clientResourceController">The client resources controller.</param>
    /// <param name="servicesFramework">The web API service framework.</param>
    /// <param name="hostSettings">The host settings.</param>
    public partial class Subscriptions(IClientResourceController clientResourceController, IServicesFramework servicesFramework, IHostSettings hostSettings)
        : UserControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Subscriptions.ascx));
        private const string SharedResources = "~/DesktopModules/CoreMessaging/App_LocalResources/SharedResources.resx";
        private readonly IClientResourceController clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
        private readonly IServicesFramework servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="Subscriptions"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.0. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public Subscriptions()
            : this(null, null, null)
        {
		Logger.LogError(">>> Entering Subscriptions()");
        Logger.LogWarning("<<< Exiting Subscriptions()");
        }

        /// <summary>Initializes a new instance of the <see cref="Subscriptions"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public Subscriptions(IClientResourceController clientResourceController)
            : this(clientResourceController, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Subscriptions"/> class.</summary>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Subscriptions(IClientResourceController clientResourceController, IServicesFramework servicesFramework)
            : this(clientResourceController, servicesFramework, null)
        {
        }

        /// <summary>Gets or sets the module context.</summary>
        public ModuleInstanceContext ModuleContext { get; set; }

        /// <summary>Gets or sets the module information.</summary>
        public ModuleInfo ModuleConfiguration
        {
            get => this.ModuleContext?.Configuration;
            set => this.ModuleContext.Configuration = value;
        }

        /// <summary>Gets or sets the localization resource file.</summary>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets the settings formatted as a JSON string.</summary>
        /// <returns>The settings formatted as a JSON string.</returns>
        public string GetSettingsAsJson()
        {
			Logger.LogError(">>> Entering GetSettingsAsJson()");
            var settings = GetModuleSettings(PortalSettings.Current, this.ModuleConfiguration, Null.NullInteger);
			Logger.LogError("GetSettingsAsJson(): settings → {Settings}", settings);
            foreach (DictionaryEntry entry in this.GetViewSettings())
            {
                if (settings.ContainsKey(entry.Key))
                {
                    settings[entry.Key] = entry.Value;
                }
                else
                {
                    settings.Add(entry.Key, entry.Value);
                }
            }

			Logger.LogWarning("<<< Exiting GetSettingsAsJson()");
            return settings.ToJson();
        }

        /// <summary>Localizes a string.</summary>
        /// <param name="key">The localization key to get the localized value.</param>
        /// <returns>The localized value for the provided key.</returns>
        protected string LocalizeString(string key)
        {
			Logger.LogError(">>> Entering LocalizeString(key={Key})", key);
            Logger.LogWarning("<<< Exiting LocalizeString(key={Key})", key);
            return Localization.GetString(key, this.LocalResourceFile);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.servicesFramework.RequestAjaxAntiForgerySupport();

            if (this.Request.IsAuthenticated)
            {
                this.clientResourceController.RegisterScript("~/DesktopModules/CoreMessaging/Scripts/LocalizationController.js");
                this.clientResourceController.RegisterScript("~/DesktopModules/CoreMessaging/Scripts/SubscriptionsViewModel.js");
                this.clientResourceController.RegisterScript("~/DesktopModules/CoreMessaging/Scripts/Subscription.js");
                this.clientResourceController.RegisterStylesheet("~/DesktopModules/CoreMessaging/subscriptions.css");
            }
            else
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), false);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        private static Hashtable GetModuleSettings(PortalSettings portalSettings, ModuleInfo moduleInfo, int uniqueId)
        {
			Logger.LogError(">>> Entering GetModuleSettings(portalSettings={PortalSettings},moduleInfo={ModuleInfo},uniqueId={UniqueId})", portalSettings, moduleInfo, uniqueId);
            var usePopup =
                portalSettings.EnablePopUps &&
                portalSettings.LoginTabId == Null.NullInteger &&
                !AuthenticationController.HasSocialAuthenticationEnabled();

            var navigationKey =
                moduleInfo is { DesktopModule: not null, }
                    ? GetHistoryNavigationKey(moduleInfo.DesktopModule.FriendlyName)
                    : null;

            var moduleRoot =
                moduleInfo is { DesktopModule: not null, }
                    ? moduleInfo.DesktopModule.FolderName
                    : null;

            var moduleTitle = moduleInfo?.ModuleTitle;

            var moduleId = moduleInfo?.ModuleID ?? Null.NullInteger;

            var moduleSettings = moduleInfo != null ? moduleInfo.ModuleSettings : new Hashtable();
			Logger.LogError("GetModuleSettings(portalSettings={PortalSettings},moduleInfo={ModuleInfo},uniqueId={UniqueId}): moduleSettings → {ModuleSettings}", portalSettings, moduleInfo, uniqueId, moduleSettings);

            var debug = false;
			Logger.LogWarning("<<< Exiting GetModuleSettings(portalSettings={PortalSettings},moduleInfo={ModuleInfo},uniqueId={UniqueId})", portalSettings, moduleInfo, uniqueId);
			Logger.LogError("GetModuleSettings(portalSettings={PortalSettings},moduleInfo={ModuleInfo},uniqueId={UniqueId}): debug → {Debug}", portalSettings, moduleInfo, uniqueId, debug);

#if DEBUG
            debug = true;
#else
            if (HttpContext.Current != null)
            {
                debug = HttpContext.Current.IsDebuggingEnabled;
            }
#endif

            return new Hashtable
                   {
                       { "anonymous", PortalSettings.Current.UserId < 0 },
                       { "currentUserId", PortalSettings.Current.UserId },
                       { "debug", debug },
                       { "culture", CultureInfo.CurrentUICulture.Name },
                       { "showMissingKeys", Localization.ShowMissingKeys },
                       { "portalId", portalSettings.PortalId },
                       { "moduleId", moduleId },
                       { "moduleSettings", moduleSettings },
                       { "moduleTitle", moduleTitle },
                       { "moduleRoot", moduleRoot },
                       { "navigationKey", navigationKey },
                       { "sessionTimeout", Convert.ToInt32(GetSessionTimeout().TotalMinutes) },
                       { "sharedResources", GetSharedResources() },
                       { "authorizationUrl", GetLoginUrl(portalSettings) },
                       { "usePopup", usePopup },
                       { "returnUrl", HttpContext.Current.Request.UrlReferrer },
                       { "uniqueId", uniqueId },
                   };
        }

        private static string GetHistoryNavigationKey(string moduleName)
        {
			Logger.LogError(">>> Entering GetHistoryNavigationKey(moduleName={ModuleName})", moduleName);
            Logger.LogWarning("<<< Exiting GetHistoryNavigationKey(moduleName={ModuleName})", moduleName);
            return HttpContext.Current.Server.HtmlEncode(moduleName.ToLowerInvariant().Replace(" ", string.Empty));
        }

        private static TimeSpan GetSessionTimeout()
        {
			Logger.LogError(">>> Entering GetSessionTimeout()");
            try
            {
                if (WebConfigurationManager.GetSection("system.web/sessionState") is SessionStateSection sessionSection)
                {
                    return sessionSection.Timeout;
                }
            }
            catch
            {
				Logger.LogError("Exception in GetSessionTimeout(): {Arg0}", ex.Message);
                // FIXME(cbond): The default configuration doesn't actually let us see this data
                // FIXME(cbond): It's too annoying seeing this fill the Event Log, we need to add the permission to web.config
                ////Exceptions.LogException(ex);
            }

			Logger.LogWarning("<<< Exiting GetSessionTimeout()");
            return TimeSpan.FromMinutes(25);
        }

        private static IDictionary<string, string> GetSharedResources()
        {
            return new Dictionary<string, string>
                {
                    { "ExceptionTitle", Localization.GetString("ExceptionTitle", SharedResources) },
                    { "ExceptionMessage", Localization.GetString("ExceptionMessage", SharedResources) },
                };
        }

        private static string GetLoginUrl(PortalSettings portalSettings)
        {
			Logger.LogError(">>> Entering GetLoginUrl(portalSettings={PortalSettings})", portalSettings);
            var returnUrl = HttpContext.Current.Request.RawUrl;

			Logger.LogError("GetLoginUrl(portalSettings={PortalSettings}): returnUrl → {ReturnUrl}", portalSettings, returnUrl);
            if (portalSettings.UserId < 1)
            {
                var indexOf = returnUrl.IndexOf("?returnurl=", StringComparison.InvariantCultureIgnoreCase);
				Logger.LogError("GetLoginUrl(portalSettings={PortalSettings}): indexOf → {IndexOf}", portalSettings, indexOf);
                if (indexOf >= 0)
                {
                    returnUrl = returnUrl.Substring(0, indexOf);
                }

                returnUrl = Common.Globals.LoginURL(HttpUtility.UrlEncode(returnUrl), true);
            }

			Logger.LogWarning("<<< Exiting GetLoginUrl(portalSettings={PortalSettings})", portalSettings);
            return returnUrl;
        }

        /// <summary>
        /// These values are passed in as the 'settings' parameter of the JavaScript initialization function, together with
        /// values that are automatically retrieved by Social Library such as portalId and moduleId.
        /// </summary>
        private Hashtable GetViewSettings()
        {
			Logger.LogError(">>> Entering GetViewSettings()");
            var portalSettings = PortalSettings.Current;
            var userPreferenceController = UserPreferencesController.Instance;
            var user = UserController.GetUserById(this.hostSettings, portalSettings.PortalId, portalSettings.UserId);
			Logger.LogError("GetViewSettings(): user → {User}", user);
            UserPreference userPreference = null;
			Logger.LogError("GetViewSettings(): userPreference → {UserPreference}", userPreference);
            if (user != null)
            {
                userPreference = userPreferenceController.GetUserPreference(user);
            }

            const int notifyFrequency = 2;
            const int messageFrequency = 0;

			Logger.LogWarning("<<< Exiting GetViewSettings()");
            return new Hashtable
                   {
                       { "moduleScope", $"#{this.ScopeWrapper.ClientID}" },
                       { "pageSize", 25 },
                       { "notifyFrequency", userPreference != null ? (int)userPreference.NotificationsEmailFrequency : notifyFrequency },
                       { "msgFrequency", userPreference != null ? (int)userPreference.MessagesEmailFrequency : messageFrequency },
                   };
        }
    }
}
