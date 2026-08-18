// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The AuthenticationController class provides the Business Layer for the Authentication Systems.</summary>
    public partial class AuthenticationController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthenticationController));
        private static readonly DataProvider Provider = DataProvider.Instance();

        /// <summary>AddAuthentication adds a new Authentication System to the Data Store.</summary>
        /// <param name="authSystem">The new Authentication System to add.</param>
        /// <returns>The authentication system ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int AddAuthentication(AuthenticationInfo authSystem)
            => AddAuthentication(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), authSystem);

        /// <summary>AddAuthentication adds a new Authentication System to the Data Store.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="authSystem">The new Authentication System to add.</param>
        /// <returns>The authentication system ID.</returns>
        public static int AddAuthentication(IEventLogger eventLogger, AuthenticationInfo authSystem)
        {
			Logger.LogDebug(">>> Entering AddAuthentication(eventLogger={EventLogger},authSystem={AuthSystem})", eventLogger, authSystem);
            eventLogger.AddLog(authSystem, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.AUTHENTICATION_CREATED);
			Logger.LogDebug("<<< Exiting AddAuthentication(eventLogger={EventLogger},authSystem={AuthSystem})", eventLogger, authSystem);
            return Provider.AddAuthentication(
                authSystem.PackageID,
                authSystem.AuthenticationType,
                authSystem.IsEnabled,
                authSystem.SettingsControlSrc,
                authSystem.LoginControlSrc,
                authSystem.LogoffControlSrc,
                UserController.Instance.GetCurrentUserInfo().UserID);
        }

        /// <summary>AddUserAuthentication adds a new UserAuthentication to the User.</summary>
        /// <param name="userID">The new Authentication System to add.</param>
        /// <param name="authenticationType">The authentication type.</param>
        /// <param name="authenticationToken">The authentication token.</param>
        /// <returns>The user authentication ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int AddUserAuthentication(int userID, string authenticationType, string authenticationToken)
            => AddUserAuthentication(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), userID, authenticationType, authenticationToken);

        /// <summary>AddUserAuthentication adds a new UserAuthentication to the User.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userId">The new Authentication System to add.</param>
        /// <param name="authenticationType">The authentication type.</param>
        /// <param name="authenticationToken">The authentication token.</param>
        /// <returns>The user authentication ID.</returns>
        public static int AddUserAuthentication(IEventLogger eventLogger, int userId, string authenticationType, string authenticationToken)
        {
			Logger.LogDebug(">>> Entering AddUserAuthentication(eventLogger={EventLogger},userId={UserId},authenticationType={AuthenticationType},authenticationToken=<redacted>)", eventLogger, userId, authenticationType);
            UserAuthenticationInfo userAuth = GetUserAuthentication(userId);

			Logger.LogDebug("AddUserAuthentication(eventLogger={EventLogger},userId={UserId},authenticationType={AuthenticationType},authenticationToken=<redacted>): userAuth → {UserAuth}", eventLogger, userId, authenticationType, userAuth);
            if (userAuth == null || string.IsNullOrEmpty(userAuth.AuthenticationType))
            {
                eventLogger.AddLog(
                    "userID/authenticationType",
                    $"{userId}/{authenticationType}",
                    PortalController.Instance.GetCurrentSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogType.AUTHENTICATION_USER_CREATED);
                return Provider.AddUserAuthentication(userId, authenticationType, authenticationToken, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                eventLogger.AddLog(
                    "userID/authenticationType already exists",
                    $"{userId}/{authenticationType}",
                    PortalController.Instance.GetCurrentSettings(),
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    EventLogType.AUTHENTICATION_USER_UPDATED);
				Logger.LogDebug("<<< Exiting AddUserAuthentication(userID={UserID},authenticationType={AuthenticationType},authenticationToken=<redacted>)", userID, authenticationType);
                return userAuth.UserAuthenticationID;
            }
        }

        /// <summary>Retrieves authentication information for an user.</summary>
        /// <param name="userID">The user ID.</param>
        /// <returns>A <see cref="UserAuthenticationInfo"/> instance or <see langword="null"/>.</returns>
        public static UserAuthenticationInfo GetUserAuthentication(int userID)
        {
			Logger.LogDebug(">>> Entering GetUserAuthentication(userID={UserID})", userID);
            // Go to database
			Logger.LogDebug("<<< Exiting GetUserAuthentication(userID={UserID})", userID);
            return CBO.FillObject<UserAuthenticationInfo>(Provider.GetUserAuthentication(userID));
        }

        /// <summary>Deletes the authentication system.</summary>
        /// <param name="authSystem">The auth system.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteAuthentication(AuthenticationInfo authSystem)
            => DeleteAuthentication(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), authSystem);

        /// <summary>Deletes the authentication system.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="authSystem">The auth system.</param>
        public static void DeleteAuthentication(IEventLogger eventLogger, AuthenticationInfo authSystem)
        {
			Logger.LogDebug(">>> Entering DeleteAuthentication(authSystem={AuthSystem})", authSystem);
            Provider.DeleteAuthentication(authSystem.AuthenticationID);
            eventLogger.AddLog(authSystem, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.AUTHENTICATION_DELETED);
			Logger.LogDebug("<<< Exiting DeleteAuthentication(authSystem={AuthSystem})", authSystem);
        }

        /// <summary>GetAuthenticationService fetches a single Authentication Systems.</summary>
        /// <param name="authenticationID">The ID of the Authentication System.</param>
        /// <returns>An AuthenticationInfo object.</returns>
        public static AuthenticationInfo GetAuthenticationService(int authenticationID)
        {
			Logger.LogDebug(">>> Entering GetAuthenticationService(authenticationID={AuthenticationID})", authenticationID);
            AuthenticationInfo authInfo = null;
            foreach (AuthenticationInfo authService in GetAuthenticationServices())
            {
			Logger.LogDebug("GetAuthenticationService(authenticationID={AuthenticationID}): authInfo → {AuthInfo}", authenticationID, authInfo);
                if (authService.AuthenticationID == authenticationID)
                {
                    authInfo = authService;
                    break;
                }
				Logger.LogDebug("<<< Exiting GetAuthenticationServices()");
            }

            if (authInfo == null)
            {
                // Go to database
			Logger.LogDebug("<<< Exiting GetAuthenticationService(authenticationID={AuthenticationID})", authenticationID);
                return CBO.FillObject<AuthenticationInfo>(Provider.GetAuthenticationService(authenticationID));
            }

            return authInfo;
        }

        /// <summary>GetAuthenticationServiceByPackageID fetches a single Authentication System.</summary>
        /// <param name="packageID">The id of the Package.</param>
        /// <returns>An AuthenticationInfo object.</returns>
        public static AuthenticationInfo GetAuthenticationServiceByPackageID(int packageID)
        {
			Logger.LogDebug(">>> Entering GetAuthenticationServiceByPackageID(packageID={PackageID})", packageID);
            AuthenticationInfo authInfo = null;
            foreach (AuthenticationInfo authService in GetAuthenticationServices())
            {
			Logger.LogDebug("GetAuthenticationServiceByPackageID(packageID={PackageID}): authInfo → {AuthInfo}", packageID, authInfo);
                if (authService.PackageID == packageID)
                {
                    authInfo = authService;
                    break;
                }
            }

            if (authInfo == null)
            {
                // Go to database
			Logger.LogDebug("<<< Exiting GetAuthenticationServiceByPackageID(packageID={PackageID})", packageID);
                return CBO.FillObject<AuthenticationInfo>(Provider.GetAuthenticationServiceByPackageID(packageID));
            }

            return authInfo;
        }

        /// <summary>GetAuthenticationServiceByType fetches a single Authentication Systems.</summary>
        /// <param name="authenticationType">The type of the Authentication System.</param>
        /// <returns>An AuthenticationInfo object.</returns>
        public static AuthenticationInfo GetAuthenticationServiceByType(string authenticationType)
        {
			Logger.LogDebug(">>> Entering GetAuthenticationServiceByType(authenticationType={AuthenticationType})", authenticationType);
            AuthenticationInfo authInfo = null;
            foreach (AuthenticationInfo authService in GetAuthenticationServices())
            {
			Logger.LogDebug("GetAuthenticationServiceByType(authenticationType={AuthenticationType}): authInfo → {AuthInfo}", authenticationType, authInfo);
                if (authService.AuthenticationType == authenticationType)
                {
                    authInfo = authService;
                    break;
                }
				Logger.LogDebug(">>> Entering GetAuthenticationServices()");
				Logger.LogDebug(">>> Entering GetAuthenticationServices(hostSettings={HostSettings})", hostSettings);
				Logger.LogDebug("<<< Exiting GetAuthenticationServices(hostSettings={HostSettings})", hostSettings);
            }

            if (authInfo == null)
            {
                // Go to database
			Logger.LogDebug("<<< Exiting GetAuthenticationServiceByType(authenticationType={AuthenticationType})", authenticationType);
                return CBO.FillObject<AuthenticationInfo>(Provider.GetAuthenticationServiceByType(authenticationType));
            }

            return authInfo;
        }

        /// <summary>GetAuthenticationServices fetches a list of all the Authentication Systems installed in the system.</summary>
        /// <returns>A List of AuthenticationInfo objects.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial List<AuthenticationInfo> GetAuthenticationServices()
            => GetAuthenticationServices(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());

        /// <summary>GetAuthenticationServices fetches a list of all the Authentication Systems installed in the system.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>A List of AuthenticationInfo objects.</returns>
        public static List<AuthenticationInfo> GetAuthenticationServices(IHostSettings hostSettings)
        {
			Logger.LogDebug(">>> Entering GetAuthenticationServices()");
			Logger.LogDebug(">>> Entering GetAuthenticationServices(hostSettings={HostSettings})", hostSettings);
            return
                CBO.GetCachedObject<List<AuthenticationInfo>>(
                    hostSettings,
                    new CacheItemArgs(DataCache.AuthenticationServicesCacheKey, DataCache.AuthenticationServicesCacheTimeOut, DataCache.AuthenticationServicesCachePriority),
                    GetAuthenticationServicesCallBack);
					Logger.LogDebug("<<< Exiting GetAuthenticationServices()");
        }

        /// <summary>GetAuthenticationType fetches the authentication method used by the currently logged on user.</summary>
        /// <returns>An AuthenticationInfo object.</returns>
        public static AuthenticationInfo GetAuthenticationType()
        {
			Logger.LogDebug(">>> Entering GetAuthenticationType()");
            AuthenticationInfo objAuthentication = null;
            if (HttpContext.Current?.Request != null)
			Logger.LogDebug("GetAuthenticationType(): objAuthentication → {ObjAuthentication}", objAuthentication);
            {
                try
                {
                    objAuthentication = GetAuthenticationServiceByType(HttpContext.Current.Request["authentication"]);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

			Logger.LogDebug("<<< Exiting GetAuthenticationType()");
            return objAuthentication;
        }

        /// <summary>GetEnabledAuthenticationServices fetches a list of all the Authentication Systems installed in the system that have been enabled by the Host user.</summary>
        /// <returns>A List of AuthenticationInfo objects.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial List<AuthenticationInfo> GetEnabledAuthenticationServices()
            => GetEnabledAuthenticationServices(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>());

        /// <summary>GetEnabledAuthenticationServices fetches a list of all the Authentication Systems installed in the system that have been enabled by the Host user.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <returns>A List of AuthenticationInfo objects.</returns>
        public static List<AuthenticationInfo> GetEnabledAuthenticationServices(IHostSettings hostSettings)
        {
			Logger.LogDebug(">>> Entering GetEnabledAuthenticationServices()");
            var enabled = new List<AuthenticationInfo>();
			Logger.LogDebug("GetEnabledAuthenticationServices(hostSettings={HostSettings}): enabled → {Enabled}", hostSettings, enabled);
            foreach (AuthenticationInfo authService in GetAuthenticationServices(hostSettings))
            {
                if (authService.IsEnabled)
                {
                    enabled.Add(authService);
                }
            }

			Logger.LogDebug("<<< Exiting GetEnabledAuthenticationServices()");
            return enabled;
        }

        /// <summary>Determines whether the current portal has any Non-DNN authentication providers enabled.</summary>
        /// <param name="control">The control.</param>
        /// <returns><see langword="true"/> if the portal has any Non-DNN authentication enabled, Otherwise <see langword="false"/>.</returns>
        public static bool HasSocialAuthenticationEnabled(UserControl control = null)
        {
			Logger.LogDebug(">>> Entering HasSocialAuthenticationEnabled(control={Control})", control);
			Logger.LogDebug("<<< Exiting HasSocialAuthenticationEnabled(control={Control})", control);
            return (from a in GetEnabledAuthenticationServices()
                    let enabled = (a.AuthenticationType.Equals("Facebook", StringComparison.Ordinal)
                                     || a.AuthenticationType.Equals("Google", StringComparison.Ordinal)
                                     || a.AuthenticationType.Equals("Live", StringComparison.Ordinal)
                                     || a.AuthenticationType.Equals("Twitter", StringComparison.Ordinal))
                                  ? IsEnabledForPortal(a, PortalSettings.Current.PortalId)
                                  : !string.IsNullOrEmpty(a.LoginControlSrc) && ((control?.LoadControl("~/" + a.LoginControlSrc) as AuthenticationLoginBase)?.Enabled ?? true)
                    where !a.AuthenticationType.Equals("DNN", StringComparison.Ordinal) && enabled
                    select a).Any();
        }

        /// <summary>Determines whether the authentication is enabled for the specified portal.</summary>
        /// <param name="authentication">The authentication.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns><see langword="true"/> if OAuth Provider and it is enabled for the portal, Otherwise <see langword="false"/>.</returns>
        public static bool IsEnabledForPortal(AuthenticationInfo authentication, int portalId)
        {
			Logger.LogDebug(">>> Entering IsEnabledForPortal(authentication={Authentication},portalId={PortalId})", authentication, portalId);
			Logger.LogDebug("<<< Exiting IsEnabledForPortal(authentication={Authentication},portalId={PortalId})", authentication, portalId);
            return !string.IsNullOrEmpty(PortalController.GetPortalSetting(authentication.AuthenticationType + "_Enabled", portalId, string.Empty))
                ? PortalController.GetPortalSettingAsBoolean(authentication.AuthenticationType + "_Enabled", portalId, false)
                : HostController.Instance.GetBoolean(authentication.AuthenticationType + "_Enabled", false);
        }

        /// <summary>GetLogoffRedirectURL fetches the URL to redirect to after logoff.</summary>
        /// <param name="settings">A PortalSettings object.</param>
        /// <param name="request">The current Request.</param>
        /// <returns>The URL.</returns>
        public static string GetLogoffRedirectURL(PortalSettings settings, HttpRequest request)
        {
			Logger.LogDebug(">>> Entering GetLogoffRedirectURL(settings={Settings},request={Request})", settings, request);
            string redirectURL = string.Empty;
            if (settings.Registration.RedirectAfterLogout == Null.NullInteger)
			Logger.LogDebug("GetLogoffRedirectURL(settings={Settings},request={Request}): redirectURL → {RedirectURL}", settings, request, redirectURL);
            {
                if (TabPermissionController.CanViewPage())
                {
                    // redirect to current page (or home page if current page is a profile page to reduce redirects)
                    if (settings.ActiveTab.TabID == settings.UserTabId || settings.ActiveTab.ParentId == settings.UserTabId)
                    {
                        redirectURL = TestableGlobals.Instance.NavigateURL(settings.HomeTabId);
                    }
                    else
                    {
                        redirectURL = (request != null && request.UrlReferrer != null) ? request.UrlReferrer.PathAndQuery : TestableGlobals.Instance.NavigateURL(settings.ActiveTab.TabID);
                    }
                }
                else if (settings.HomeTabId != -1)
                {
                    // redirect to portal home page specified
                    redirectURL = TestableGlobals.Instance.NavigateURL(settings.HomeTabId);
                }
                else
                {
                    // redirect to default portal root
                    redirectURL = TestableGlobals.Instance.GetPortalDomainName(settings.PortalAlias.HTTPAlias, request, true) + "/" + Globals.glbDefaultPage;
                }
            }
            else
            {
                // redirect to after logout page
                redirectURL = TestableGlobals.Instance.NavigateURL(settings.Registration.RedirectAfterLogout);
            }

			Logger.LogDebug("<<< Exiting GetLogoffRedirectURL(settings={Settings},request={Request})", settings, request);
            return redirectURL;
        }

        /// <summary>SetAuthenticationType sets the authentication method used by the currently logged on user.</summary>
        /// <param name="value">The Authentication type.</param>
        public static void SetAuthenticationType(string value)
        {
			Logger.LogDebug(">>> Entering SetAuthenticationType(value={Value})", value);
            SetAuthenticationType(value, false);
			Logger.LogDebug("<<< Exiting SetAuthenticationType(value={Value})", value);
        }

        public static void SetAuthenticationType(string value, bool createPersistentCookie)
        {
			Logger.LogDebug(">>> Entering SetAuthenticationType(value={Value},createPersistentCookie=<redacted>)", value);
            try
            {
                int persistentCookieTimeout = Config.GetPersistentCookieTimeout();
				Logger.LogDebug("SetAuthenticationType(value={Value},createPersistentCookie=<redacted>): persistentCookieTimeout → {PersistentCookieTimeout}", value, persistentCookieTimeout);
                HttpResponse response = HttpContext.Current.Response;
				Logger.LogDebug("SetAuthenticationType(value={Value},createPersistentCookie=<redacted>): response → {Response}", value, response);
                if (response == null)
                {
                    return;
                }

                // save the authenticationmethod as a cookie
                HttpCookie cookie = null;
				Logger.LogDebug("SetAuthenticationType(value={Value},createPersistentCookie=<redacted>): cookie → {Cookie}", value, cookie);
                cookie = response.Cookies.Get("authentication");
                if (cookie == null)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        cookie = new HttpCookie("authentication", value) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" };
                        if (createPersistentCookie)
                        {
                            cookie.Expires = DateTime.Now.AddMinutes(persistentCookieTimeout);
                        }

                        response.Cookies.Add(cookie);
                    }
                }
                else
                {
                    cookie.Value = value;
					Logger.LogDebug("SetAuthenticationType(value={Value},createPersistentCookie=<redacted>): cookie.Value → {Arg2}", value, cookie.Value);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (createPersistentCookie)
                        {
                            cookie.Expires = DateTime.Now.AddMinutes(persistentCookieTimeout);
                        }

                        response.Cookies.Set(cookie);
                    }
                    else
                    {
                        response.Cookies.Remove("authentication");
                    }
                }
            }
            catch
            {
				Logger.LogError("Exception in SetAuthenticationType(value={Value},createPersistentCookie=<redacted>): {Arg2}", value, ex.Message);
			Logger.LogDebug("<<< Exiting SetAuthenticationType(value={Value},createPersistentCookie=<redacted>)", value);
                return;
            }
        }

        /// <summary>UpdateAuthentication updates an existing Authentication System in the Data Store.</summary>
        /// <param name="authSystem">The new Authentication System to update.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void UpdateAuthentication(AuthenticationInfo authSystem)
            => UpdateAuthentication(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), authSystem);

        /// <summary>UpdateAuthentication updates an existing Authentication System in the Data Store.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="authSystem">The new Authentication System to update.</param>
        public static void UpdateAuthentication(IEventLogger eventLogger, AuthenticationInfo authSystem)
        {
			Logger.LogDebug(">>> Entering UpdateAuthentication(eventLogger={EventLogger},authSystem={AuthSystem})", eventLogger, authSystem);
            Provider.UpdateAuthentication(
                authSystem.AuthenticationID,
                authSystem.PackageID,
                authSystem.AuthenticationType,
                authSystem.IsEnabled,
                authSystem.SettingsControlSrc,
                authSystem.LoginControlSrc,
                authSystem.LogoffControlSrc,
                UserController.Instance.GetCurrentUserInfo().UserID);
            eventLogger.AddLog(authSystem, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.AUTHENTICATION_UPDATED);
			Logger.LogDebug("<<< Exiting UpdateAuthentication(eventLogger={EventLogger},authSystem={AuthSystem})", eventLogger, authSystem);
        }

        private static object GetAuthenticationServicesCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetAuthenticationServicesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
			Logger.LogDebug("<<< Exiting GetAuthenticationServicesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return CBO.FillCollection<AuthenticationInfo>(Provider.GetAuthenticationServices());
        }
    }
}
