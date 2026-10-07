// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.Membership
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.HttpModules.Services;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Skins.EventListeners;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Information about membership.</summary>
    public partial class MembershipModule : IHttpModule
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MembershipModule));

        /// <summary>Initializes a new instance of the <see cref="MembershipModule"/> class.</summary>
        public MembershipModule()
        {
		Logger.LogError(">>> Entering MembershipModule()");
        Logger.LogWarning("<<< Exiting MembershipModule()");
        }

        /// <summary>Initializes a new instance of the <see cref="MembershipModule"/> class.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="ipAddressController">The IP address controller.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload without parameters. Scheduled removal in v12.0.0.")]
        public MembershipModule(IHostSettingsService hostSettingsService, IPortalController portalController, IUserRequestIPAddressController ipAddressController, IRoleController roleController, IUserController userController, IEventLogger eventLogger)
            : this()
        {
        }

        /// <summary>Gets the name of the module.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ModuleName => "DNNMembershipModule";

        /// <summary>Called when unverified user skin initialize.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SkinEventArgs"/> instance containing the event data.</param>
        [Obsolete("This method has been deprecated. No equivalent. Scheduled removal in v11.0.0.")]
        public static void OnUnverifiedUserSkinInit(object sender, SkinEventArgs e)
        {
			Logger.LogError(">>> Entering OnUnverifiedUserSkinInit(sender={Sender},e={E})", sender, e);
            var pageService = Globals.GetCurrentServiceProvider().GetRequiredService<IPageService>();
			Logger.LogWarning("<<< Exiting OnUnverifiedUserSkinInit(sender={Sender},e={E})", sender, e);
            var strMessage = Localization.GetString("UnverifiedUser", Localization.SharedResourceFile, Thread.CurrentThread.CurrentCulture.Name);
			Logger.LogError("OnUnverifiedUserSkinInit(sender={Sender},e={E}): strMessage → {StrMessage}", sender, e, strMessage);
			Logger.LogError("OnUnverifiedUserSkinInit(sender={Sender},e={E}): pageService → {PageService}", sender, e, pageService);
            pageService.AddMessage(new PageMessage(string.Empty, strMessage, PageMessageType.Warning, string.Empty, PagePriority.Site));
        }

        /// <summary>Authenticates the request.</summary>
        /// <param name="context">The context.</param>
        /// <param name="allowUnknownExtensions">if set to <see langword="true"/> to allow unknown extensions.</param>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettingsService")]
        public static partial void AuthenticateRequest(HttpContextBase context, bool allowUnknownExtensions)
            => AuthenticateRequest(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserRequestIPAddressController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>(),
                context,
                allowUnknownExtensions);

        /// <summary>Authenticates the request.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="ipAddressController">The user request IP address controller.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="context">The context.</param>
        /// <param name="allowUnknownExtensions">if set to <c>true</c> to allow unknown extensions.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void AuthenticateRequest(IHostSettingsService hostSettingsService, IPortalController portalController, IUserRequestIPAddressController ipAddressController, IRoleController roleController, HttpContextBase context, bool allowUnknownExtensions)
            => AuthenticateRequest(hostSettingsService, portalController, ipAddressController, roleController, Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), context, allowUnknownExtensions);

        /// <summary>Authenticates the request.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="ipAddressController">The user request IP address controller.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="context">The context.</param>
        /// <param name="allowUnknownExtensions">if set to <c>true</c> to allow unknown extensions.</param>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial void AuthenticateRequest(IHostSettingsService hostSettingsService, IPortalController portalController, IUserRequestIPAddressController ipAddressController, IRoleController roleController, IEventLogger eventLogger, HttpContextBase context, bool allowUnknownExtensions)
            => AuthenticateRequest(hostSettingsService, portalController, ipAddressController, roleController, eventLogger, Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), context, allowUnknownExtensions);

        /// <summary>Authenticates the request.</summary>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="ipAddressController">The user request IP address controller.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="context">The context.</param>
        /// <param name="allowUnknownExtensions">if set to <c>true</c> to allow unknown extensions.</param>
        public static void AuthenticateRequest(IHostSettingsService hostSettingsService, IPortalController portalController, IUserRequestIPAddressController ipAddressController, IRoleController roleController, IEventLogger eventLogger, IHostSettings hostSettings, HttpContextBase context, bool allowUnknownExtensions)
        {
			Logger.LogError(">>> Entering AuthenticateRequest(context={Context},allowUnknownExtensions={AllowUnknownExtensions})", context, allowUnknownExtensions);
            HttpRequestBase request = context.Request;
            HttpResponseBase response = context.Response;
			Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): response → {Response}", hostSettingsService, portalController, ipAddressController, roleController, context, allowUnknownExtensions, response);

            // First check if we are upgrading/installing
            if (!Initialize.ProcessHttpModule(context.ApplicationInstance.Request, allowUnknownExtensions, false))
            {
				Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},eventLogger={EventLogger},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): response → {Response}", hostSettingsService, portalController, ipAddressController, roleController, eventLogger, context, allowUnknownExtensions, response);
				Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},eventLogger={EventLogger},hostSettings={HostSettings},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): response → {Response}", hostSettingsService, portalController, ipAddressController, roleController, eventLogger, hostSettings, context, allowUnknownExtensions, response);
				Logger.LogError("AuthenticateRequest(context={Context},allowUnknownExtensions={AllowUnknownExtensions}): response → {Response}", context, allowUnknownExtensions, response);
                return;
            }

            // Obtain PortalSettings from Current Context
            var portalSettings = portalController.GetCurrentSettings();
			Logger.LogError("AuthenticateRequest(context={Context},allowUnknownExtensions={AllowUnknownExtensions}): portalSettings → {PortalSettings}", context, allowUnknownExtensions, portalSettings);
			Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): portalSettings → {PortalSettings}", hostSettingsService, portalController, ipAddressController, roleController, context, allowUnknownExtensions, portalSettings);

            if (request.IsAuthenticated && !IsActiveDirectoryAuthHeaderPresent() && portalSettings != null)
            {
				Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},eventLogger={EventLogger},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): portalSettings → {PortalSettings}", hostSettingsService, portalController, ipAddressController, roleController, eventLogger, context, allowUnknownExtensions, portalSettings);
				Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},eventLogger={EventLogger},hostSettings={HostSettings},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): portalSettings → {PortalSettings}", hostSettingsService, portalController, ipAddressController, roleController, eventLogger, hostSettings, context, allowUnknownExtensions, portalSettings);
                var user = UserController.GetCachedUser(hostSettings, portalSettings.PortalId, context.User.Identity.Name);

				Logger.LogError("AuthenticateRequest(context={Context},allowUnknownExtensions={AllowUnknownExtensions}): user → {User}", context, allowUnknownExtensions, user);
				Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): user → {User}", hostSettingsService, portalController, ipAddressController, roleController, context, allowUnknownExtensions, user);
                // if current login is from windows authentication, the ignore the process
                if (user == null && context.User is WindowsPrincipal)
                {
					Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},eventLogger={EventLogger},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): user → {User}", hostSettingsService, portalController, ipAddressController, roleController, eventLogger, context, allowUnknownExtensions, user);
					Logger.LogError("AuthenticateRequest(hostSettingsService={HostSettingsService},portalController={PortalController},ipAddressController={IpAddressController},roleController={RoleController},eventLogger={EventLogger},hostSettings={HostSettings},context={Context},allowUnknownExtensions={AllowUnknownExtensions}): user → {User}", hostSettingsService, portalController, ipAddressController, roleController, eventLogger, hostSettings, context, allowUnknownExtensions, user);
                    return;
                }

                // authenticate user and set last login ( this is necessary for users who have a permanent Auth cookie set )
                if (RequireLogout(hostSettingsService, context, user))
                {
                    var portalSecurity = PortalSecurity.Instance;
                    portalSecurity.SignOut();

                    // Remove user from cache
                    if (user != null)
                    {
                        DataCache.ClearUserCache(portalSettings.PortalId, context.User.Identity.Name);
                    }

                    // Redirect browser back to home page
                    response.Redirect(request.RawUrl, true);
                    return;
                }

			Logger.LogWarning("<<< Exiting AuthenticateRequest(context={Context},allowUnknownExtensions={AllowUnknownExtensions})", context, allowUnknownExtensions);
                // if users LastActivityDate is outside the UsersOnlineTimeWindow then record user activity
    #pragma warning disable CS0618 // Type or member is obsolete
                var usersOnlineTimeWindow = Host.UsersOnlineTimeWindow;
    #pragma warning restore CS0618 // Type or member is obsolete
                if (user != null && DateTime.Compare(user.Membership.LastActivityDate.AddMinutes(usersOnlineTimeWindow), DateTime.Now) < 0)
                {
                    // update LastActivityDate and IP Address for user
                    user.Membership.LastActivityDate = DateTime.Now;
                    user.LastIPAddress = ipAddressController.GetUserRequestIPAddress(request);
                    UserController.UpdateUser(eventLogger, portalSettings.PortalId, user, false, false);
                }

                // check for RSVP code
                if (user != null && request.QueryString["rsvp"] != null && !string.IsNullOrEmpty(request.QueryString["rsvp"]))
                {
                    foreach (var role in roleController.GetRoles(portalSettings.PortalId, r => (r.SecurityMode != SecurityMode.SocialGroup || r.IsPublic) && r.Status == RoleStatus.Approved))
                    {
                        if (role.RSVPCode == request.QueryString["rsvp"])
                        {
                            roleController.UpdateUserRole(portalSettings.PortalId, user.UserID, role.RoleID, RoleStatus.Approved, false, false);
                        }
                    }
                }

                // save userinfo object in context
                if (context.Items["UserInfo"] != null)
                {
                    context.Items["UserInfo"] = user;
                }
                else
                {
                    context.Items.Add("UserInfo", user);
                }

                // Localization.SetLanguage also updates the user profile, so this needs to go after the profile is loaded
                if (user != null && request.RawUrl != null && !ServicesModule.ServiceApi.IsMatch(request.RawUrl))
                {
                    Localization.SetLanguage(user.Profile.PreferredLocale);
                }
            }

            if (context.Items["UserInfo"] == null)
            {
                context.Items.Add("UserInfo", new UserInfo());
            }
        }

        /// <summary>Initializes the specified application.</summary>
        /// <param name="application">The application.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
			Logger.LogError(">>> Entering Init(application={Application})", application);
            application.AuthenticateRequest += OnAuthenticateRequest;
            application.PreRequestHandlerExecute += Context_PreRequestHandlerExecute;
            application.PreSendRequestHeaders += OnPreSendRequestHeaders;
			Logger.LogWarning("<<< Exiting Init(application={Application})", application);
        }

        /// <summary>Disposes of the resources (other than memory) used by the module that implements <see cref="System.Web.IHttpModule" />.</summary>
        public void Dispose()
        {
		Logger.LogError(">>> Entering Dispose()");
        Logger.LogWarning("<<< Exiting Dispose()");
        }

        private static bool IsActiveDirectoryAuthHeaderPresent()
        {
			Logger.LogError(">>> Entering IsActiveDirectoryAuthHeaderPresent()");
            var auth = HttpContext.Current.Request.Headers.Get("Authorization");
			Logger.LogError("IsActiveDirectoryAuthHeaderPresent(): auth → {Auth}", auth);
            if (!string.IsNullOrEmpty(auth))
            {
                if (auth.StartsWith("Negotiate", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

			Logger.LogWarning("<<< Exiting IsActiveDirectoryAuthHeaderPresent()");
            return false;
        }

        private static bool RequireLogout(IHostSettingsService hostSettingsService, HttpContextBase context, UserInfo user)
        {
			Logger.LogError(">>> Entering RequireLogout(hostSettingsService={HostSettingsService},context={Context},user={User})", hostSettingsService, context, user);
            try
            {
                if (user == null || user.IsDeleted || user.Membership.LockedOut
                    || (!user.Membership.Approved && !user.IsInRole("Unverified Users"))
                    || !user.Username.Equals(context.User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var forceLogout = hostSettingsService.GetBoolean("ForceLogoutAfterPasswordChanged");
				Logger.LogError("RequireLogout(hostSettingsService={HostSettingsService},context={Context},user={User}): forceLogout → {ForceLogout}", hostSettingsService, context, user, forceLogout);
                if (!forceLogout)
                {
                    return false;
                }

                // if user's password changed after the user cookie created, then force user to login again.
                DateTime? issueDate = null;
                if (context.User.Identity is FormsIdentity formsIdentity)
                {
                    issueDate = formsIdentity.Ticket.IssueDate;
					Logger.LogDebug("RequireLogout(hostSettingsService={HostSettingsService},context={Context},user={User}): issueDate → {IssueDate}", hostSettingsService, context, user, issueDate);
                }

                return !Null.IsNull(issueDate) && issueDate < user.Membership.LastPasswordChangeDate;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting RequireLogout(hostSettingsService={HostSettingsService},context={Context},user={User})", hostSettingsService, context, user);
                return true;
            }
        }

        private static void OnAuthenticateRequest(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering OnAuthenticateRequest(sender={Sender},e={E})", sender, e);
            var application = (HttpApplication)sender;
            var serviceProvider = application.Context.GetScope().ServiceProvider;
			Logger.LogError("OnAuthenticateRequest(sender={Sender},e={E}): serviceProvider → {ServiceProvider}", sender, e, serviceProvider);
			Logger.LogError("OnAuthenticateRequest(sender={Sender},e={E}): application → {Application}", sender, e, application);

            AuthenticateRequest(
                serviceProvider.GetRequiredService<IHostSettingsService>(),
                serviceProvider.GetRequiredService<IPortalController>(),
                serviceProvider.GetRequiredService<IUserRequestIPAddressController>(),
                serviceProvider.GetRequiredService<IRoleController>(),
                serviceProvider.GetRequiredService<IEventLogger>(),
                serviceProvider.GetRequiredService<IHostSettings>(),
                new HttpContextWrapper(application.Context),
                false);
				Logger.LogWarning("<<< Exiting OnAuthenticateRequest(sender={Sender},e={E})", sender, e);
        }

        // DNN-6973: if the authentication cookie set by cookie slide in membership,
        // then use SignIn method instead if current portal is in portal group.
        private static void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreSendRequestHeaders(sender={Sender},e={E})", sender, e);
            var application = (HttpApplication)sender;
            var serviceProvider = GetOrCreateServiceProvider(application.Context);
			Logger.LogError("OnPreSendRequestHeaders(sender={Sender},e={E}): serviceProvider → {ServiceProvider}", sender, e, serviceProvider);
            var portalController = serviceProvider.GetRequiredService<IPortalController>();
			Logger.LogError("OnPreSendRequestHeaders(sender={Sender},e={E}): portalController → {PortalController}", sender, e, portalController);
			Logger.LogError("OnPreSendRequestHeaders(sender={Sender},e={E}): application → {Application}", sender, e, application);

            var portalSettings = portalController.GetCurrentSettings();
			Logger.LogError("OnPreSendRequestHeaders(sender={Sender},e={E}): portalSettings → {PortalSettings}", sender, e, portalSettings);
            var hasAuthCookie = application.Response.Headers["Set-Cookie"] != null
                                    && application.Response.Headers["Set-Cookie"].Contains(FormsAuthentication.FormsCookieName);
            if (portalSettings != null && hasAuthCookie && !application.Context.Items.Contains("DNN_UserSignIn"))
            {
                var isInPortalGroup = PortalController.IsMemberOfPortalGroup(portalController, portalSettings.PortalId);
				Logger.LogError("OnPreSendRequestHeaders(sender={Sender},e={E}): isInPortalGroup → {IsInPortalGroup}", sender, e, isInPortalGroup);
                if (isInPortalGroup)
                {
                    var authCookie = application.Response.Cookies[FormsAuthentication.FormsCookieName];
                    if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value) && string.IsNullOrEmpty(authCookie.Domain))
                    {
                        application.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
                        PortalSecurity.Instance.SignIn(UserController.Instance.GetCurrentUserInfo(), false);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting OnPreSendRequestHeaders(sender={Sender},e={E})", sender, e);
        }

        private static void Context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering Context_PreRequestHandlerExecute(sender={Sender},e={E})", sender, e);
            var content = HttpContextSource.Current;
            var request = content.Request;
            var serviceProvider = content.GetScope().ServiceProvider;
			Logger.LogError("Context_PreRequestHandlerExecute(sender={Sender},e={E}): serviceProvider → {ServiceProvider}", sender, e, serviceProvider);
            var portalController = serviceProvider.GetRequiredService<IPortalController>();
			Logger.LogError("Context_PreRequestHandlerExecute(sender={Sender},e={E}): portalController → {PortalController}", sender, e, portalController);
            var userController = serviceProvider.GetRequiredService<IUserController>();
			Logger.LogError("Context_PreRequestHandlerExecute(sender={Sender},e={E}): userController → {UserController}", sender, e, userController);

            var portalSettings = portalController.GetCurrentSettings();
			Logger.LogError("Context_PreRequestHandlerExecute(sender={Sender},e={E}): portalSettings → {PortalSettings}", sender, e, portalSettings);
            var user = userController.GetCurrentUserInfo();
			Logger.LogError("Context_PreRequestHandlerExecute(sender={Sender},e={E}): user → {User}", sender, e, user);
            if (!request.IsAuthenticated || IsActiveDirectoryAuthHeaderPresent() || portalSettings == null)
            {
                return;
            }

            var contextItems = HttpContext.Current.Items;
            if (!user.IsSuperUser && user.IsInRole("Unverified Users") && !contextItems.Contains(Skin.OnInitMessage))
            {
                contextItems.Add(Skin.OnInitMessage, Localization.GetString("UnverifiedUser", Localization.SharedResourceFile, Thread.CurrentThread.CurrentCulture.Name));
            }

            if (!user.IsSuperUser && request.QueryString.AllKeys.Contains("VerificationSuccess") && !contextItems.Contains(Skin.OnInitMessage))
            {
                contextItems.Add(Skin.OnInitMessage, Localization.GetString("VerificationSuccess", Localization.SharedResourceFile, Thread.CurrentThread.CurrentCulture.Name));
                contextItems.Add(Skin.OnInitMessageType, ModuleMessage.ModuleMessageType.GreenSuccess);
            }
			Logger.LogWarning("<<< Exiting Context_PreRequestHandlerExecute(sender={Sender},e={E})", sender, e);
        }

        private static IServiceProvider GetOrCreateServiceProvider(HttpContext context)
        {
			Logger.LogError(">>> Entering GetOrCreateServiceProvider(context={Context})", context);
            // NOTE: Using HttpContextDependencyInjectionExtensions.GetScope can cause an infinite loop/stack overflow, presumably due to timing issues
            var scope = context.Items[typeof(IServiceScope)] as IServiceScope ?? Globals.DependencyProvider.CreateScope();
			Logger.LogError("GetOrCreateServiceProvider(context={Context}): scope → {Scope}", context, scope);
			Logger.LogWarning("<<< Exiting GetOrCreateServiceProvider(context={Context})", context);
            return scope.ServiceProvider;
        }
    }
}
