// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services
{
    using System;
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
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Xml;

    using Dnn.PersonaBar.Extensions.Components.Security.Ssl;
    using Dnn.PersonaBar.Extensions.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Security.Helper;
    using Dnn.PersonaBar.Security.Services.Dto;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Common.Utils;
    using DotNetNuke.ContentSecurityPolicy;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    using Microsoft.Extensions.DependencyInjection;

    using Constants = Dnn.PersonaBar.Library.Constants;
    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>Provides REST APIs to manage security settings.</summary>
    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class SecurityController : PersonaBarApiController
    {
        private const string BULLETINXMLNODEPATH = "//channel/item";
        private const string UserRequestIPHeaderSettingName = "UserRequestIPHeader";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityController));

        private readonly Components.SecurityController controller;
        private readonly IPagesController pagesController;
        private readonly IPortalAliasService portalAliasService;
        private readonly IApiTokenController apiTokenController;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="SecurityController"/> class.</summary>
        /// <param name="pagesController">The pages controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="apiTokenController">The API token controller.</param>
        /// <param name="hostSettingsService">Provides services to manage host settings.</param>
        /// <param name="applicationStatusInfo">Provides information about the application status.</param>
        public SecurityController(
            IPagesController pagesController,
            IPortalAliasService portalAliasService,
            IApiTokenController apiTokenController,
            IHostSettingsService hostSettingsService,
            IApplicationStatusInfo applicationStatusInfo)
            : this(
                new Components.SecurityController(),
                pagesController,
                portalAliasService,
                apiTokenController,
                hostSettingsService,
                applicationStatusInfo,
                null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SecurityController"/> class.</summary>
        /// <param name="controller">The security controller.</param>
        /// <param name="pagesController">The pages controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="apiTokenController">The API token controller.</param>
        /// <param name="hostSettingsService">Provides services to manage host settings.</param>
        /// <param name="applicationStatusInfo">Provides information about the application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        internal SecurityController(
            Components.SecurityController controller,
            IPagesController pagesController,
            IPortalAliasService portalAliasService,
            IApiTokenController apiTokenController,
            IHostSettingsService hostSettingsService,
            IApplicationStatusInfo applicationStatusInfo,
            IHostSettings hostSettings)
        {
            this.pagesController = pagesController;
            this.controller = controller;
            this.portalAliasService = portalAliasService;
            this.apiTokenController = apiTokenController;
            this.hostSettingsService = hostSettingsService;
            this.applicationStatusInfo = applicationStatusInfo;
            this.applicationStatusInfo = applicationStatusInfo;
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("SecurityController(controller={Controller},pagesController={PagesController},portalAliasService={PortalAliasService},apiTokenController=<redacted>,hostSettingsService={HostSettingsService},applicationStatusInfo={ApplicationStatusInfo},hostSettings={HostSettings}): this.apiTokenController → {Arg7}", controller, pagesController, portalAliasService, hostSettingsService, applicationStatusInfo, hostSettings, this.apiTokenController);
			Logger.LogWarning("<<< Exiting SecurityController(controller={Controller},pagesController={PagesController},portalAliasService={PortalAliasService},apiTokenController=<redacted>,hostSettingsService={HostSettingsService},applicationStatusInfo={ApplicationStatusInfo},hostSettings={HostSettings})", controller, pagesController, portalAliasService, hostSettingsService, applicationStatusInfo, hostSettings);
        }

        /// GET: api/Security/GetBasicLoginSettings
        /// <summary>Gets portal's basic login settings.</summary>
        /// <param name="cultureCode">The culture code or <see langword="null"/> or empty to use the current locale.</param>
        /// <returns>Portal's basic login settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.BasicLoginSettingsView)]
        public HttpResponseMessage GetBasicLoginSettings(string cultureCode)
        {
			Logger.LogError(">>> Entering GetBasicLoginSettings(cultureCode={CultureCode})", cultureCode);
            try
            {
                cultureCode = string.IsNullOrEmpty(cultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(this.PortalId).Code
                    : cultureCode;

                var portal = PortalController.Instance.GetPortal(this.PortalId, cultureCode);
				Logger.LogError("GetBasicLoginSettings(cultureCode={CultureCode}): portal → {Portal}", cultureCode, portal);
                var portalSettings = new PortalSettings(portal);
				Logger.LogError("GetBasicLoginSettings(cultureCode={CultureCode}): portalSettings → {PortalSettings}", cultureCode, portalSettings);

                dynamic settings = new ExpandoObject();
				Logger.LogError("GetBasicLoginSettings(cultureCode={CultureCode}): settings → {Settings}", cultureCode, settings);
                settings.DefaultAuthProvider = PortalController.GetPortalSetting("DefaultAuthProvider", this.PortalId, "DNN");
                settings.PrimaryAdministratorId = PortalSettings.Current.AdministratorId;
				Logger.LogDebug("GetBasicLoginSettings(cultureCode={CultureCode}): settings.PrimaryAdministratorId → {Arg1}", cultureCode, settings.PrimaryAdministratorId);
                settings.RequireValidProfileAtLogin = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfileAtLogin", this.PortalId, true);
                settings.CaptchaLogin = PortalController.GetPortalSettingAsBoolean("Security_CaptchaLogin", this.PortalId, false);
                settings.CaptchaRetrievePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRetrivePassword", this.PortalId, false);
                settings.CaptchaChangePassword = PortalController.GetPortalSettingAsBoolean("Security_CaptchaChangePassword", this.PortalId, false);
                settings.HideLoginControl = this.PortalSettings.HideLoginControl;
				Logger.LogDebug("GetBasicLoginSettings(cultureCode={CultureCode}): settings.HideLoginControl → {Arg1}", cultureCode, settings.HideLoginControl);
                settings.cultureCode = cultureCode;
				Logger.LogDebug("GetBasicLoginSettings(cultureCode={CultureCode}): settings.cultureCode → {Arg1}", cultureCode, settings.cultureCode);
                settings.userRequestIPHeader = this.hostSettingsService.GetString(UserRequestIPHeaderSettingName, string.Empty);

                var authProviders = this.controller.GetAuthenticationProviders().Select(v => new
                {
                    Name = v,
                    Value = v,
                }).ToList();

                var adminUsers = this.controller.GetAdminUsers(this.PortalId).Select(v => new
                {
                    v.UserID,
                    v.FullName,
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Settings = settings,
                        AuthProviders = authProviders,
                        Administrators = adminUsers,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetBasicLoginSettings(cultureCode={CultureCode})", cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateBasicLoginSettings
        /// <summary>Updates an existing log settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.BasicLoginSettingsView + "&" + Components.Constants.BasicLoginSettingsEdit)]
        public HttpResponseMessage UpdateBasicLoginSettings(UpdateBasicLoginSettingsRequest request)
        {
			Logger.LogError(">>> Entering UpdateBasicLoginSettings(request={Request})", request);
            if (!this.ModelState.IsValid)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, this.ModelState);
            }

            try
            {
                var cultureCode = string.IsNullOrEmpty(request.CultureCode)
                    ? LocaleController.Instance.GetCurrentLocale(this.PortalId).Code
                    : request.CultureCode;

                var portalInfo = PortalController.Instance.GetPortal(this.PortalId);
				Logger.LogError("UpdateBasicLoginSettings(request={Request}): portalInfo → {PortalInfo}", request, portalInfo);
                portalInfo.AdministratorId = Convert.ToInt32(request.PrimaryAdministratorId);
                PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(this.PortalId, "DefaultAuthProvider", request.DefaultAuthProvider);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_RequireValidProfile", request.RequireValidProfileAtLogin.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaLogin", request.CaptchaLogin.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaRetrivePassword", request.CaptchaRetrievePassword.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaChangePassword", request.CaptchaChangePassword.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "HideLoginControl", request.HideLoginControl.ToString(), false);

                var originalUserRequestIPHeader = this.hostSettingsService.GetString(UserRequestIPHeaderSettingName, string.Empty);
				Logger.LogError("UpdateBasicLoginSettings(request={Request}): originalUserRequestIPHeader → {OriginalUserRequestIPHeader}", request, originalUserRequestIPHeader);
                if (request.UserRequestIPHeader != originalUserRequestIPHeader)
                {
                    this.hostSettingsService.Update(UserRequestIPHeaderSettingName, request.UserRequestIPHeader, true);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting UpdateBasicLoginSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetIpFilters
        /// <summary>Gets list of IP filters.</summary>
        /// <returns>List of IP filters.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetIpFilters()
        {
			Logger.LogError(">>> Entering GetIpFilters()");
            try
            {
                var filters = IPFilterController.Instance.GetIPFilters().Select(v => new
                {
                    v.IPFilterID,
                    IPFilter = NetworkUtils.FormatAsCidr(v.IPAddress, v.SubnetMask),
                    v.RuleType,
                    v.Notes,
                }).ToList();
                var response = new
                {
					Logger.LogDebug("GetIpFilters(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        Filters = filters,
                        this.hostSettings.EnableIPChecking,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetIpFilters()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetIpFilter
        /// <summary>Gets an IP filter.</summary>
        /// <param name="filterId">The ID of the IP filter to get.</param>
        /// <returns>IP filter.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetIpFilter(int filterId)
        {
			Logger.LogError(">>> Entering GetIpFilter(filterId={FilterId})", filterId);
            try
            {
                IPFilterInfo filter = IPFilterController.Instance.GetIPFilter(filterId);
				Logger.LogError("GetIpFilter(filterId={FilterId}): filter → {Filter}", filterId, filter);
                var response = new
                {
					Logger.LogDebug("GetIpFilter(filterId={FilterId}): Results → {Results}", filterId, Results);
                    Success = true,
                    Results = new
                    {
                        filter.IPAddress,
                        filter.IPFilterID,
                        filter.RuleType,
                        filter.SubnetMask,
                        filter.Notes,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetIpFilter(filterId={FilterId})", filterId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateIpFilter
        /// <summary>Updates an IP filter.</summary>
        /// <param name="request"><see cref="UpdateIpFilterRequest"/>.</param>
        /// <returns>Ok or BadRequest or InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage UpdateIpFilter(UpdateIpFilterRequest request)
        {
			Logger.LogError(">>> Entering UpdateIpFilter(request={Request})", request);
            try
            {
                var ipf = new IPFilterInfo();
				Logger.LogError("UpdateIpFilter(request={Request}): ipf → {Ipf}", request, ipf);
				Logger.LogDebug("UpdateIpFilter(request={Request}): ipf.SubnetMask → {Arg1}", request, ipf.SubnetMask);
				Logger.LogDebug("UpdateIpFilter(request={Request}): ipf.RuleType → {Arg1}", request, ipf.RuleType);
				Logger.LogDebug("UpdateIpFilter(request={Request}): ipf.Notes → {Arg1}", request, ipf.Notes);
                ipf.IPAddress = request.IPAddress;
				Logger.LogDebug("UpdateIpFilter(request={Request}): ipf.IPAddress → {Arg1}", request, ipf.IPAddress);
                ipf.SubnetMask = request.SubnetMask;
                ipf.RuleType = request.RuleType;
                ipf.Notes = request.Notes;

                if ((ipf.IPAddress == "127.0.0.1" || ipf.IPAddress == "localhost" || ipf.IPAddress == "::1" || ipf.IPAddress == "*") && ipf.RuleType == 2)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("CannotDeleteLocalhost.Text", Components.Constants.LocalResourcesFile));
                }

                if (IPFilterController.Instance.IsAllowableDeny(HttpContext.Current.Request.UserHostAddress, ipf) == false)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("CannotDeleteIPInUse.Text", Components.Constants.LocalResourcesFile));
                }

                if (request.IPFilterID > 0)
                {
                    ipf.IPFilterID = request.IPFilterID;
					Logger.LogDebug("UpdateIpFilter(request={Request}): ipf.IPFilterID → {Arg1}", request, ipf.IPFilterID);
                    IPFilterController.Instance.UpdateIPFilter(ipf);
                }
                else
                {
                    IPFilterController.Instance.AddIPFilter(ipf);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (ArgumentException exc)
            {
                Logger.Info(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting UpdateIpFilter(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/DeleteIpFilter
        /// <summary>Deletes an IP filter.</summary>
        /// <param name="filterId">The filter ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DeleteIpFilter(int filterId)
        {
			Logger.LogError(">>> Entering DeleteIpFilter(filterId={FilterId})", filterId);
            try
            {
                IList<IPFilterInfo> currentRules = IPFilterController.Instance.GetIPFilters();
				Logger.LogError("DeleteIpFilter(filterId={FilterId}): currentRules → {CurrentRules}", filterId, currentRules);
                List<IPFilterInfo> currentWithDeleteRemoved = (from p in currentRules where p.IPFilterID != filterId select p).ToList();
				Logger.LogError("DeleteIpFilter(filterId={FilterId}): currentWithDeleteRemoved → {CurrentWithDeleteRemoved}", filterId, currentWithDeleteRemoved);

                if (IPFilterController.Instance.CanIPStillAccess(HttpContext.Current.Request.UserHostAddress, currentWithDeleteRemoved) == false)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("CannotDelete.Text", Components.Constants.LocalResourcesFile));
                }
                else
                {
                    var ipf = new IPFilterInfo();
					Logger.LogError("DeleteIpFilter(filterId={FilterId}): ipf → {Ipf}", filterId, ipf);
                    ipf.IPFilterID = filterId;
					Logger.LogDebug("DeleteIpFilter(filterId={FilterId}): ipf.IPFilterID → {Arg1}", filterId, ipf.IPFilterID);
                    IPFilterController.Instance.DeleteIPFilter(ipf);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting DeleteIpFilter(filterId={FilterId})", filterId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetMemberSettings
        /// <summary>Gets portal's member settings.</summary>
        /// <returns>Portal's member settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetMemberSettings()
        {
			Logger.LogError(">>> Entering GetMemberSettings()");
            try
            {
                var response = new
                {
					Logger.LogDebug("GetMemberSettings(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        Settings = new
						Logger.LogDebug("GetMemberSettings(): Settings → {Settings}", Settings);
                        {
                            MembershipResetLinkValidity = this.hostSettings.MembershipResetLinkValidity.TotalMinutes,
                            AdminMembershipResetLinkValidity = this.hostSettings.AdminMembershipResetLinkValidity.TotalMinutes,
                            this.hostSettings.EnablePasswordHistory,
                            this.hostSettings.MembershipNumberPasswords,
                            this.hostSettings.MembershipDaysBeforePasswordReuse,
                            this.hostSettings.EnableBannedList,
                            this.hostSettings.EnableStrengthMeter,
                            this.hostSettings.EnableIPChecking,
                            PasswordExpiry = this.hostSettings.PasswordExpiry.TotalDays,
                            PasswordExpiryReminder = this.hostSettings.PasswordExpiryReminder.TotalDays,
                            ForceLogoutAfterPasswordChanged = this.hostSettingsService.GetBoolean("ForceLogoutAfterPasswordChanged"),
                        },
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetMemberSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateMemberSettings
        /// <summary>Updates member settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage UpdateMemberSettings(UpdateMemberSettingsRequest request)
        {
			Logger.LogError(">>> Entering UpdateMemberSettings(request={Request})", request);
            try
            {
                this.hostSettingsService.Update("EnableBannedList", request.EnableBannedList ? "Y" : "N", false);
                this.hostSettingsService.Update("EnableStrengthMeter", request.EnableStrengthMeter ? "Y" : "N", false);
                this.hostSettingsService.Update("EnableIPChecking", request.EnableIPChecking ? "Y" : "N", false);
                this.hostSettingsService.Update("EnablePasswordHistory", request.EnablePasswordHistory ? "Y" : "N", false);
                this.hostSettingsService.Update("MembershipResetLinkValidity", request.MembershipResetLinkValidity.ToString(CultureInfo.InvariantCulture), false);
                this.hostSettingsService.Update("AdminMembershipResetLinkValidity", request.AdminMembershipResetLinkValidity.ToString(CultureInfo.InvariantCulture), false);
                this.hostSettingsService.Update("MembershipNumberPasswords", request.MembershipNumberPasswords.ToString(CultureInfo.InvariantCulture), false);
                this.hostSettingsService.Update("MembershipDaysBeforePasswordReuse", request.MembershipDaysBeforePasswordReuse.ToString(CultureInfo.InvariantCulture), false);
                this.hostSettingsService.Update("PasswordExpiry", request.PasswordExpiry.ToString(CultureInfo.InvariantCulture));
                this.hostSettingsService.Update("PasswordExpiryReminder", request.PasswordExpiryReminder.ToString(CultureInfo.InvariantCulture));
                this.hostSettingsService.Update("ForceLogoutAfterPasswordChanged", request.ForceLogoutAfterPasswordChanged ? "Y" : "N", false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting UpdateMemberSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetRegistrationSettings
        /// <summary>Gets portal's registration settings.</summary>
        /// <returns>Portal's registration settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RegistrationSettingsView)]
        public HttpResponseMessage GetRegistrationSettings()
        {
			Logger.LogError(">>> Entering GetRegistrationSettings()");
            try
            {
                List<KeyValuePair<string, int>> userRegistrationOptions = RegistrationSettingsHelper.GetUserRegistrationOptions();
                List<KeyValuePair<string, int>> registrationFormTypeOptions = RegistrationSettingsHelper.GetRegistrationFormOptions();

                var activeLanguage = LocaleController.Instance.GetDefaultLocale(this.PortalId).Code;
				Logger.LogError("GetRegistrationSettings(): activeLanguage → {ActiveLanguage}", activeLanguage);
                var portal = PortalController.Instance.GetPortal(this.PortalId, activeLanguage);
				Logger.LogError("GetRegistrationSettings(): portal → {Portal}", portal);

                var response = new
                {
					Logger.LogDebug("GetRegistrationSettings(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        Settings = new
						Logger.LogDebug("GetRegistrationSettings(): Settings → {Settings}", Settings);
                        {
                            portal.UserRegistration,
                            EnableRegisterNotification = PortalController.GetPortalSettingAsBoolean("EnableRegisterNotification", this.PortalId, true),
                            EnableUnapprovedPasswordReminderNotification = PortalController.GetPortalSettingAsBoolean("EnableUnapprovedPasswordReminderNotification", this.PortalId, true),
                            UseAuthenticationProviders = PortalController.GetPortalSettingAsBoolean("Registration_UseAuthProviders", this.PortalId, false),
                            ExcludedTerms = PortalController.GetPortalSetting("Registration_ExcludeTerms", this.PortalId, string.Empty),
                            UseProfanityFilter = PortalController.GetPortalSettingAsBoolean("Registration_UseProfanityFilter", this.PortalId, false),
                            this.PortalSettings.Registration.RegistrationFormType,
                            this.PortalSettings.Registration.RegistrationFields,
                            UseEmailAsUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", this.PortalId, false),
                            RequireUniqueDisplayName = PortalController.GetPortalSettingAsBoolean("Registration_RequireUniqueDisplayName", this.PortalId, false),
                            DisplayNameFormat = PortalController.GetPortalSetting("Security_DisplayNameFormat", this.PortalId, string.Empty),
                            UserNameMinLength = PortalController.GetPortalSettingAsInteger("Security_UserNameMinLength", this.PortalId, Globals.glbUserNameMinLength),
                            UserNameValidation = PortalController.GetPortalSetting("Security_UserNameValidation", this.PortalId, Globals.glbUserNameRegEx),
                            EmailAddressValidation = PortalController.GetPortalSetting("Security_EmailValidation", this.PortalId, Globals.glbEmailRegEx),
                            UseRandomPassword = PortalController.GetPortalSettingAsBoolean("Registration_RandomPassword", this.PortalId, false),
                            RequirePasswordConfirmation = PortalController.GetPortalSettingAsBoolean("Registration_RequireConfirmPassword", this.PortalId, true),
                            RequireValidProfile = PortalController.GetPortalSettingAsBoolean("Security_RequireValidProfile", this.PortalId, false),
                            UseCaptchaRegister = PortalController.GetPortalSettingAsBoolean("Security_CaptchaRegister", this.PortalId, false),
                            RequiresUniqueEmail = MembershipProviderConfig.RequiresUniqueEmail.ToString(CultureInfo.InvariantCulture),
                            PasswordFormat = MembershipProviderConfig.PasswordFormat.ToString(),
                            PasswordRetrievalEnabled = MembershipProviderConfig.PasswordRetrievalEnabled.ToString(CultureInfo.InvariantCulture),
                            PasswordResetEnabled = MembershipProviderConfig.PasswordResetEnabled.ToString(CultureInfo.InvariantCulture),
                            MinPasswordLength = MembershipProviderConfig.MinPasswordLength.ToString(CultureInfo.InvariantCulture),
                            MinNonAlphanumericCharacters = MembershipProviderConfig.MinNonAlphanumericCharacters.ToString(CultureInfo.InvariantCulture),
                            RequiresQuestionAndAnswer = MembershipProviderConfig.RequiresQuestionAndAnswer.ToString(CultureInfo.InvariantCulture),
                            MembershipProviderConfig.PasswordStrengthRegularExpression,
                            MaxInvalidPasswordAttempts = MembershipProviderConfig.MaxInvalidPasswordAttempts.ToString(CultureInfo.InvariantCulture),
                            PasswordAttemptWindow = MembershipProviderConfig.PasswordAttemptWindow.ToString(CultureInfo.InvariantCulture),
                        },
                        UserRegistrationOptions = userRegistrationOptions,
                        RegistrationFormTypeOptions = registrationFormTypeOptions,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetRegistrationSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetSslSettings
        /// <summary>Gets portal's SSL settings.</summary>
        /// <returns>Portal's SSL settings.</returns>
        [HttpGet]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage GetSslSettings()
        {
			Logger.LogError(">>> Entering GetSslSettings()");
            try
            {
                dynamic settings = new ExpandoObject();
				Logger.LogError("GetSslSettings(): settings → {Settings}", settings);
                settings.SSLSetup = PortalController.GetPortalSettingAsInteger("SSLSetup", this.PortalId, 0);
                settings.SSLEnforced = PortalController.GetPortalSettingAsBoolean("SSLEnforced", this.PortalId, false);
                settings.SSLURL = PortalController.GetPortalSetting("SSLURL", this.PortalId, Null.NullString);
                settings.STDURL = PortalController.GetPortalSetting("STDURL", this.PortalId, Null.NullString);

                var portalStats = SslController.Instance.GetPortalStats(this.PortalId);
				Logger.LogError("GetSslSettings(): portalStats → {PortalStats}", portalStats);
				Logger.LogDebug("GetSslSettings(): settings.NumberOfNonSecureTabs → {Arg0}", settings.NumberOfNonSecureTabs);
                settings.NumberOfSecureTabs = portalStats.NumberOfSecureTabs;
				Logger.LogDebug("GetSslSettings(): settings.NumberOfSecureTabs → {Arg0}", settings.NumberOfSecureTabs);
                settings.NumberOfNonSecureTabs = portalStats.NumberOfNonSecureTabs;

                if (this.UserInfo.IsSuperUser)
                {
                    settings.SSLOffloadHeader = this.hostSettingsService.GetString("SSLOffloadHeader", string.Empty);
                }

                var response = new
                {
					Logger.LogDebug("GetSslSettings(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        Settings = settings,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetSslSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateRegistrationSettings
        /// <summary>Updates registration settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.RegistrationSettingsView + "&" + Components.Constants.RegistrationSettingsEdit)]
        public HttpResponseMessage UpdateRegistrationSettings(UpdateRegistrationSettingsRequest request)
        {
			Logger.LogError(">>> Entering UpdateRegistrationSettings(request={Request})", request);
            if (!this.ModelState.IsValid)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, this.ModelState);
            }

            try
            {
                var setting = request.RegistrationFields;
				Logger.LogError("UpdateRegistrationSettings(request={Request}): setting → {Setting}", request, setting);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RegistrationFields", setting);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RegistrationFormType", request.RegistrationFormType.ToString(CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_UseEmailAsUserName", request.UseEmailAsUsername.ToString(), false);

                var portalInfo = PortalController.Instance.GetPortal(this.PortalId);
				Logger.LogError("UpdateRegistrationSettings(request={Request}): portalInfo → {PortalInfo}", request, portalInfo);
                portalInfo.UserRegistration = Convert.ToInt32(request.UserRegistration, CultureInfo.InvariantCulture);
                PortalController.Instance.UpdatePortalInfo(portalInfo);

                PortalController.UpdatePortalSetting(this.PortalId, "EnableRegisterNotification", request.EnableRegisterNotification.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "EnableUnapprovedPasswordReminderNotification", request.EnableUnapprovedPasswordReminderNotification.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_UseAuthProviders", request.UseAuthenticationProviders.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_ExcludeTerms", request.ExcludedTerms, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_UseProfanityFilter", request.UseProfanityFilter.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RequireUniqueDisplayName", request.RequireUniqueDisplayName.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_DisplayNameFormat", request.DisplayNameFormat, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_UserNameMinLength", request.UserNameMinLength, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_UserNameValidation", request.UserNameValidation, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_EmailValidation", request.EmailAddressValidation, false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RandomPassword", request.UseRandomPassword.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Registration_RequireConfirmPassword", request.RequirePasswordConfirmation.ToString(), true);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_RequireValidProfile", request.RequireValidProfile.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "Security_CaptchaRegister", request.UseCaptchaRegister.ToString(), false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting UpdateRegistrationSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateSslSettings
        /// <summary>Updates SSL settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage UpdateSslSettings(UpdateSslSettingsRequest request)
        {
			Logger.LogError(">>> Entering UpdateSslSettings(request={Request})", request);
            try
            {
                switch (request.SSLSetup)
                {
                    case 0:
                        request.SSLEnforced = false;
						Logger.LogDebug("UpdateSslSettings(request={Request}): request.SSLEnforced → {Arg1}", request, request.SSLEnforced);
						Logger.LogDebug("UpdateSslSettings(request={Request}): request.STDURL → {Arg1}", request, request.STDURL);
                        request.SSLURL = string.Empty;
						Logger.LogDebug("UpdateSslSettings(request={Request}): request.SSLURL → {Arg1}", request, request.SSLURL);
                        request.STDURL = string.Empty;
                        break;
                    case 1:
                        request.SSLEnforced = false;
                        request.SSLURL = string.Empty;
                        request.STDURL = string.Empty;
                        request.SSLOffloadHeader = string.Empty;
						Logger.LogDebug("UpdateSslSettings(request={Request}): request.SSLOffloadHeader → {Arg1}", request, request.SSLOffloadHeader);
                        break;
                }

                PortalController.UpdatePortalSetting(this.PortalId, "SSLSetup", request.SSLSetup.ToString(CultureInfo.InvariantCulture), false);
                PortalController.UpdatePortalSetting(this.PortalId, "SSLEnforced", request.SSLEnforced.ToString(), false);
                PortalController.UpdatePortalSetting(this.PortalId, "SSLURL", this.AddPortalAlias(request.SSLURL, this.PortalId), false);
                PortalController.UpdatePortalSetting(this.PortalId, "STDURL", this.AddPortalAlias(request.STDURL, this.PortalId), false);

                if (this.UserInfo.IsSuperUser)
                {
                    this.hostSettingsService.Update("SSLOffloadHeader", request.SSLOffloadHeader);
                }

                DataCache.ClearPortalCache(this.PortalId, false);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting UpdateSslSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/SetAllPagesSecure
        /// <summary>Sets all pages in the portal to be secure.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = Constants.AdminsRoleName)]
        public HttpResponseMessage SetAllPagesSecure()
        {
			Logger.LogError(">>> Entering SetAllPagesSecure()");
            try
            {
                DotNetNuke.Data.DataProvider.Instance().SetAllPortalTabsSecure(this.PortalId, true);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting SetAllPagesSecure()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetSecurityBulletins
        /// <summary>Gets security bulletins.</summary>
        /// <returns>Security bulletins.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSecurityBulletins()
        {
			Logger.LogError(">>> Entering GetSecurityBulletins()");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var platformVersion = System.Reflection.Assembly.LoadFrom(this.applicationStatusInfo.ApplicationMapPath + @"\bin\DotNetNuke.dll").GetName().Version;
				Logger.LogError("GetSecurityBulletins(): platformVersion → {PlatformVersion}", platformVersion);
                string sRequest = $"https://dnnplatform.io/security.aspx?type={DotNetNukeContext.Current.Application.Type}&name=DNNCORP.CE&version={Globals.FormatVersion(platformVersion, "00", 3, string.Empty)}";

                // format for display with "." delimiter
                string sVersion = Globals.FormatVersion(platformVersion, "00", 3, ".");

                // make remote request
                Stream oStream = null;
				Logger.LogError("GetSecurityBulletins(): oStream → {OStream}", oStream);
                try
                {
                    HttpWebRequest oRequest = Globals.GetExternalRequest(this.hostSettings, sRequest);
					Logger.LogError("GetSecurityBulletins(): oRequest → {ORequest}", oRequest);
                    oRequest.Timeout = 10000; // 10 seconds
					Logger.LogDebug("GetSecurityBulletins(): oRequest.Timeout → {Arg0}", oRequest.Timeout);
                    WebResponse oResponse = oRequest.GetResponse();
					Logger.LogError("GetSecurityBulletins(): oResponse → {OResponse}", oResponse);
                    oStream = oResponse.GetResponseStream();
                }
                catch (Exception oExc)
                {
					Logger.LogError("Exception in GetSecurityBulletins(): {Arg0}", oExc.Message);
                    // connectivity issues
                    if (PortalSecurity.IsInRoles(this.PortalSettings.AdministratorRoleId.ToString(CultureInfo.InvariantCulture)))
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Format(CultureInfo.CurrentCulture, Localization.GetString("RequestFailed_Admin.Text", Components.Constants.LocalResourcesFile), sRequest));
                    }
                    else
                    {
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("RequestFailed_User.Text", Components.Constants.LocalResourcesFile) + oExc.Message);
                    }
                }

                // load XML document
                StreamReader oReader = new StreamReader(oStream);
				Logger.LogError("GetSecurityBulletins(): oReader → {OReader}", oReader);
                XmlDocument oDoc = new XmlDocument { XmlResolver = null, };
                using (var xmlReader = XmlReader.Create(new StringReader(oReader.ReadToEnd()), new XmlReaderSettings { XmlResolver = null, }))
                {
                    oDoc.Load(xmlReader);
                }

                List<object> items = [];
                foreach (XmlNode selectNode in oDoc.SelectNodes(BULLETINXMLNODEPATH))
                {
                    items.Add(new
                    {
                        Title = selectNode.SelectSingleNode("title") != null ? selectNode.SelectSingleNode("title").InnerText : string.Empty,
                        Link = selectNode.SelectSingleNode("link") != null ? selectNode.SelectSingleNode("link").InnerText : string.Empty,
                        Description = selectNode.SelectSingleNode("description") != null ? selectNode.SelectSingleNode("description").InnerText : string.Empty,
                        Author = selectNode.SelectSingleNode("author") != null ? selectNode.SelectSingleNode("author").InnerText : string.Empty,
                        PubDate = selectNode.SelectSingleNode("pubDate") != null ? selectNode.SelectSingleNode("pubDate").InnerText.Split(' ')[0] : string.Empty,
                    });
                }

                var response = new
                {
					Logger.LogDebug("GetSecurityBulletins(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        PlatformVersion = sVersion,
                        SecurityBulletins = items,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogError("GetSecurityBulletins(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
				Logger.LogWarning("<<< Exiting GetSecurityBulletins()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetOtherSettings
        /// <summary>Gets host other settings.</summary>
        /// <returns>Portal's other settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetOtherSettings()
        {
			Logger.LogError(">>> Entering GetOtherSettings()");
            try
            {
                var response = new
                {
					Logger.LogDebug("GetOtherSettings(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        Settings = new
						Logger.LogDebug("GetOtherSettings(): Settings → {Settings}", Settings);
                        {
                            this.hostSettings.ShowCriticalErrors,
                            this.hostSettings.DebugMode,
                            this.hostSettings.RememberCheckbox,
                            this.hostSettings.AllowOverrideThemeViaQueryString,
                            this.hostSettings.AllowRichTextModuleTitle,
                            AutoAccountUnlockDuration = this.hostSettings.AutoAccountUnlockDuration.TotalMinutes,
                            AsyncTimeout = this.hostSettings.AsyncTimeout.TotalMinutes,
                            MaxUploadSize = Config.GetMaxUploadSize(this.applicationStatusInfo) / 1024 / 1024,
                            RangeUploadSize = 4294967295 / 1024 / 1024, // 4GB (max allowedContentLength supported in IIS7)
                            AllowedExtensionWhitelist = this.hostSettings.AllowedExtensionAllowList.ToStorageString(),
                            DefaultEndUserExtensionWhitelist = this.hostSettings.DefaultEndUserExtensionAllowList.ToStorageString(),
                        },
                    },
                };
							Logger.LogDebug("GetOtherSettings(): RangeUploadSize → {RangeUploadSize}", RangeUploadSize);

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetOtherSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateOtherSettings
        /// <summary>Updates other settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateOtherSettings(UpdateOtherSettingsRequest request)
        {
			Logger.LogError(">>> Entering UpdateOtherSettings(request={Request})", request);
            try
            {
                this.hostSettingsService.Update("ShowCriticalErrors", request.ShowCriticalErrors ? "Y" : "N", false);
                this.hostSettingsService.Update("DebugMode", request.DebugMode ? "True" : "False", false);
                this.hostSettingsService.Update("RememberCheckbox", request.RememberCheckbox ? "Y" : "N", false);
                this.hostSettingsService.Update("AllowOverrideThemeViaQueryString", request.AllowOverrideThemeViaQueryString ? "Y" : "N", false);
                this.hostSettingsService.Update("AllowRichTextModuleTitle", request.AllowRichTextModuleTitle ? "Y" : "N", false);
                this.hostSettingsService.Update("AutoAccountUnlockDuration", request.AutoAccountUnlockDuration.ToString(CultureInfo.InvariantCulture), false);
                this.hostSettingsService.Update("AsyncTimeout", request.AsyncTimeout.ToString(CultureInfo.InvariantCulture), false);
                var oldExtensionList = this.hostSettings.AllowedExtensionAllowList.ToStorageString();
				Logger.LogError("UpdateOtherSettings(request={Request}): oldExtensionList → {OldExtensionList}", request, oldExtensionList);
                var fileExtensions = new FileExtensionWhitelist(request.AllowedExtensionWhitelist);
				Logger.LogError("UpdateOtherSettings(request={Request}): fileExtensions → {FileExtensions}", request, fileExtensions);
                var newExtensionList = fileExtensions.ToStorageString();
				Logger.LogError("UpdateOtherSettings(request={Request}): newExtensionList → {NewExtensionList}", request, newExtensionList);
                this.hostSettingsService.Update("FileExtensions", newExtensionList, false);
                if (oldExtensionList != newExtensionList)
                {
                    PortalSecurity.Instance.CheckAllPortalFileExtensionWhitelists(newExtensionList);
                }

                var defaultEndUserExtensionWhitelist = new FileExtensionWhitelist(request.DefaultEndUserExtensionWhitelist);
                defaultEndUserExtensionWhitelist = defaultEndUserExtensionWhitelist.RestrictBy(fileExtensions);
				Logger.LogError("UpdateOtherSettings(request={Request}): defaultEndUserExtensionWhitelist → {DefaultEndUserExtensionWhitelist}", request, defaultEndUserExtensionWhitelist);
                this.hostSettingsService.Update("DefaultEndUserExtensionWhitelist", defaultEndUserExtensionWhitelist.ToStorageString(), false);

                var maxCurrentRequest = Config.GetMaxUploadSize(this.applicationStatusInfo);
				Logger.LogError("UpdateOtherSettings(request={Request}): maxCurrentRequest → {MaxCurrentRequest}", request, maxCurrentRequest);
                var maxUploadByMb = request.MaxUploadSize * 1024 * 1024;
                if (maxCurrentRequest != maxUploadByMb)
                {
                    Config.SetMaxUploadSize(this.applicationStatusInfo, maxUploadByMb);
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

        /// GET: api/Security/GetAuditCheckResults
        /// <summary>Gets audit check results.</summary>
        /// <param name="checkAll">Whether to run all checks, or only the checks not marked to be lazy loaded.</param>
        /// <returns>audit check results.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAuditCheckResults([FromUri] bool checkAll = false)
        {
			Logger.LogError(">>> Entering GetAuditCheckResults(checkAll={CheckAll})", checkAll);
            try
            {
                var audit = new Components.AuditChecks(this.pagesController);
				Logger.LogError("GetAuditCheckResults(checkAll={CheckAll}): audit → {Audit}", checkAll, audit);
                var results = audit.DoChecks(checkAll);
				Logger.LogError("GetAuditCheckResults(checkAll={CheckAll}): results → {Results}", checkAll, results);
                var response = new
                {
                    Success = true,
                    Results = results,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetAuditCheckResults(checkAll={CheckAll})", checkAll);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetAuditCheckResult?id={id}
        /// <summary> Gets audit check result for a specific checker.</summary>
        /// <param name="id"> The ID of the audit check to perform.</param>
        /// <returns>audit check result.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAuditCheckResult([FromUri] string id)
        {
			Logger.LogError(">>> Entering GetAuditCheckResult(id={Id})", id);
            try
            {
                var audit = new Components.AuditChecks(this.pagesController);
				Logger.LogError("GetAuditCheckResult(id={Id}): audit → {Audit}", id, audit);
                var result = audit.DoCheck(id);
				Logger.LogError("GetAuditCheckResult(id={Id}): result → {Result}", id, result);
                var response = new
                {
                    Success = true,
                    Result = result,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetAuditCheckResult(id={Id})", id);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetSuperuserActivities
        /// <summary>Gets super user activities.</summary>
        /// <returns>super user activities.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSuperuserActivities()
        {
			Logger.LogError(">>> Entering GetSuperuserActivities()");
            try
            {
                var users = UserController.GetUsers(true, true, -1).Cast<UserInfo>().Select(u => new
                {
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.DisplayName,
                    u.Email,
                    CreatedDate = DisplayDate(u.Membership.CreatedDate),
                    LastLoginDate = DisplayDate(u.Membership.LastLoginDate),
                    LastActivityDate = DisplayDate(u.Membership.LastActivityDate),
                }).ToList();

                var response = new
                {
					Logger.LogDebug("GetSuperuserActivities(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        Activities = users,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetSuperuserActivities()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/SearchFileSystemAndDatabase
        /// <summary>Searches file system and database.</summary>
        /// <param name="term">The term to check for.</param>
        /// <returns>The search results from files and database.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage SearchFileSystemAndDatabase(string term)
        {
			Logger.LogError(">>> Entering SearchFileSystemAndDatabase(term={Term})", term);
            try
            {
                // run these in parallel
                var task1 = Task.Factory.StartNew(() => Components.Utility.SearchFiles(term));
				Logger.LogError("SearchFileSystemAndDatabase(term={Term}): task1 → {Task1}", term, task1);
                var task2 = Task.Factory.StartNew(() => Components.Utility.SearchDatabase(term));
				Logger.LogError("SearchFileSystemAndDatabase(term={Term}): task2 → {Task2}", term, task2);
                Task.WhenAll(task1, task2).Wait();

                var foundinfiles = task1.Result;
                var foundindb = task2.Result;
                var response = new
                {
					Logger.LogDebug("SearchFileSystemAndDatabase(term={Term}): Results → {Results}", term, Results);
                    Success = true,
                    Results = new
                    {
                        FoundInFiles = foundinfiles,
                        FoundInDatabase = foundindb,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting SearchFileSystemAndDatabase(term={Term})", term);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetLastModifiedFiles
        /// <summary>Gets recently modified files.</summary>
        /// <returns>last modified files.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetLastModifiedFiles()
        {
			Logger.LogError(">>> Entering GetLastModifiedFiles()");
            try
            {
                var highRiskFiles = Components.Utility.GetLastModifiedExecutableFiles().Select(f => new
                {
                    FilePath = this.GetFilePath(f.FullName),
                    LastWriteTime = DisplayDate(f.LastWriteTime),
                });
                var lowRiskFiles = Components.Utility.GetLastModifiedFiles().Select(f => new
                {
                    FilePath = this.GetFilePath(f.FullName),
                    LastWriteTime = DisplayDate(f.LastWriteTime),
                });
                var response = new
                {
					Logger.LogDebug("GetLastModifiedFiles(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        HighRiskFiles = highRiskFiles,
                        LowRiskFiles = lowRiskFiles,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetLastModifiedFiles()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetLastModifiedSettings
        /// <summary>Gets last modified settings.</summary>
        /// <returns>last modified settings.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetLastModifiedSettings()
        {
			Logger.LogError(">>> Entering GetLastModifiedSettings()");
            try
            {
                var settings = this.controller.GetModifiedSettings();
				Logger.LogError("GetLastModifiedSettings(): settings → {Settings}", settings);
                var portalSettings = (from DataRow dr in settings[0].Rows
                                      select new SettingsDto
                                      {
                                          PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger, CultureInfo.InvariantCulture),
                                          SettingName = Convert.ToString(dr["SettingName"], CultureInfo.InvariantCulture),
                                          SettingValue = Convert.ToString(dr["SettingValue"], CultureInfo.InvariantCulture),
                                          LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"], CultureInfo.InvariantCulture),
                                          LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"], CultureInfo.InvariantCulture)),
                                      }).ToList();

                var hostSettings = (from DataRow dr in settings[1].Rows
                                    select new SettingsDto
                                    {
                                        SettingName = Convert.ToString(dr["SettingName"], CultureInfo.InvariantCulture),
                                        SettingValue = Convert.ToString(dr["SettingValue"], CultureInfo.InvariantCulture),
                                        LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"], CultureInfo.InvariantCulture),
                                        LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"], CultureInfo.InvariantCulture)),
                                    }).ToList();

                var tabSettings = (from DataRow dr in settings[2].Rows
                                   select new SettingsDto
                                   {
                                       TabId = Convert.ToInt32(dr["TabID"], CultureInfo.InvariantCulture),
                                       PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger, CultureInfo.InvariantCulture),
                                       SettingName = Convert.ToString(dr["SettingName"], CultureInfo.InvariantCulture),
                                       SettingValue = Convert.ToString(dr["SettingValue"], CultureInfo.InvariantCulture),
                                       LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"], CultureInfo.InvariantCulture),
                                       LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"], CultureInfo.InvariantCulture)),
                                   }).ToList();

                var moduleSettings = (from DataRow dr in settings[3].Rows
                                      select new SettingsDto
                                      {
                                          ModuleId = Convert.ToInt32(dr["ModuleID"], CultureInfo.InvariantCulture),
                                          PortalId = Convert.ToInt32(dr["PortalID"] != DBNull.Value ? dr["PortalID"] : Null.NullInteger, CultureInfo.InvariantCulture),
                                          Type = Convert.ToString(dr["Type"], CultureInfo.InvariantCulture),
                                          SettingName = Convert.ToString(dr["SettingName"], CultureInfo.InvariantCulture),
                                          SettingValue = Convert.ToString(dr["SettingValue"], CultureInfo.InvariantCulture),
                                          LastModifiedByUserId = Convert.ToInt32(dr["LastModifiedByUserID"], CultureInfo.InvariantCulture),
                                          LastModifiedOnDate = DisplayDate(Convert.ToDateTime(dr["LastModifiedOnDate"], CultureInfo.InvariantCulture)),
                                      }).ToList();

                var response = new
                {
					Logger.LogDebug("GetLastModifiedSettings(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        PortalSettings = portalSettings,
                        HostSettings = hostSettings,
                        TabSettings = tabSettings,
                        ModuleSettings = moduleSettings,
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetLastModifiedSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/Security/GetApiTokenSettings
        /// <summary>Gets settings for API tokens.</summary>
        /// <returns>API token settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]
        public HttpResponseMessage GetApiTokenSettings()
        {
			Logger.LogError(">>> Entering GetApiTokenSettings()");
            try
            {
                var response = new
                {
					Logger.LogDebug("GetApiTokenSettings(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        ApiTokenSettings = ApiTokenSettings.GetSettings(this.PortalId),
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetApiTokenSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateApiTokenSettings
        /// <summary>Updates API Token settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [RequireAdmin]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateApiTokenSettings(UpdateApiTokenSettingsRequest request)
        {
			Logger.LogError(">>> Entering UpdateApiTokenSettings(request={Request})", request);
            try
            {
                var settings = ApiTokenSettings.GetSettings(this.PortalId);
				Logger.LogError("UpdateApiTokenSettings(request={Request}): settings → {Settings}", request, settings);
                var oldMax = (int)settings.MaximumSiteTimespan;
				Logger.LogError("UpdateApiTokenSettings(request={Request}): oldMax → {OldMax}", request, oldMax);
                if (this.UserInfo.IsSuperUser)
                {
                    settings.MaximumSiteTimespan = (ApiTokenTimespan)request.MaximumSiteTimespan;
                }

                var newMax = (int)settings.MaximumSiteTimespan;
				Logger.LogError("UpdateApiTokenSettings(request={Request}): newMax → {NewMax}", request, newMax);
                var maxHasBeenReduced = newMax < oldMax;

                settings.UserTokenTimespan = (ApiTokenTimespan)request.UserTokenTimespan;
                settings.AllowApiTokens = request.AllowApiTokens;
				Logger.LogDebug("UpdateApiTokenSettings(request={Request}): settings.AllowApiTokens → {Arg1}", request, settings.AllowApiTokens);
                settings.SaveSettings(this.PortalId);

                if (maxHasBeenReduced)
                {
                    foreach (IPortalInfo p in PortalController.Instance.GetPortalList(Null.NullString))
                    {
                        var tokenSettings = ApiTokenSettings.GetSettings(p.PortalId);
						Logger.LogError("UpdateApiTokenSettings(request={Request}): tokenSettings → {TokenSettings}", request, tokenSettings);
                        var oldValue = (int)tokenSettings.UserTokenTimespan;
						Logger.LogError("UpdateApiTokenSettings(request={Request}): oldValue → {OldValue}", request, oldValue);
                        if (oldValue > newMax)
                        {
                            tokenSettings.UserTokenTimespan = (ApiTokenTimespan)newMax;
                            tokenSettings.SaveSettings(p.PortalId);
                        }
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting UpdateApiTokenSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Retrieves a paged list of API tokens for the specified portal and page, with the specified page size.
        /// </summary>
        /// <param name="portalId">The ID of the portal for which to retrieve API tokens. Use -2 for all portals.</param>
        /// <param name="filter">Value indicating which tokens to return based on status.</param>
        /// <param name="apiKey">API key to filter the results by.</param>
        /// <param name="scope">Filter the results by scope or use -2 for no scope.</param>
        /// <param name="pageIndex">The page index (starting from 0) of the API token list to retrieve.</param>
        /// <param name="pageSize">The number of API tokens per page to retrieve.</param>
        /// <returns>A paged list of `ApiToken` objects for the specified portal and page.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]
        public HttpResponseMessage GetApiTokens(int portalId, int filter, string apiKey, int scope, int pageIndex, int pageSize)
        {
			Logger.LogError(">>> Entering GetApiTokens(portalId={PortalId},filter={Filter},apiKey=<redacted>,scope={Scope},pageIndex={PageIndex},pageSize={PageSize})", portalId, filter, scope, pageIndex, pageSize);
            if (portalId < 0)
            {
                portalId = Null.NullInteger;
				Logger.LogError("GetApiTokens(portalId={PortalId},filter={Filter},apiKey=<redacted>,scope={Scope},pageIndex={PageIndex},pageSize={PageSize}): portalId → {PortalId1}", portalId, filter, scope, pageIndex, pageSize, portalId);
            }

            var noScopeDefined = scope == -2;
            var requestedScope = ApiTokenScope.User;
			Logger.LogError("GetApiTokens(portalId={PortalId},filter={Filter},apiKey=<redacted>,scope={Scope},pageIndex={PageIndex},pageSize={PageSize}): requestedScope → {RequestedScope}", portalId, filter, scope, pageIndex, pageSize, requestedScope);
            var user = this.UserInfo;
            var requestingUser = user.UserID;
			Logger.LogError("GetApiTokens(portalId={PortalId},filter={Filter},apiKey=<redacted>,scope={Scope},pageIndex={PageIndex},pageSize={PageSize}): requestingUser → {RequestingUser}", portalId, filter, scope, pageIndex, pageSize, requestingUser);

            if (user.IsSuperUser)
            {
                requestingUser = Null.NullInteger;
                if (noScopeDefined)
                {
                    requestedScope = ApiTokenScope.Host;
                }
                else
                {
                    requestedScope = (ApiTokenScope)scope;
                }
            }
            else if (user.IsAdmin)
            {
                requestingUser = Null.NullInteger;
                portalId = PortalSettings.Current.PortalId;
                if (noScopeDefined)
                {
                    requestedScope = ApiTokenScope.Portal;
                }
                else if (scope > 1)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid scope");
                }
                else
                {
                    requestedScope = (ApiTokenScope)scope;
                }
            }
            else
            {
                portalId = PortalSettings.Current.PortalId;
                if (scope > 1)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid scope");
                }
            }

            var response = this.apiTokenController.GetApiTokens(requestedScope, noScopeDefined, portalId, requestingUser, (ApiTokenFilter)filter, apiKey, pageIndex, pageSize);
			Logger.LogError("GetApiTokens(portalId={PortalId},filter={Filter},apiKey=<redacted>,scope={Scope},pageIndex={PageIndex},pageSize={PageSize}): response → {Response}", portalId, filter, scope, pageIndex, pageSize, response);
			Logger.LogWarning("<<< Exiting GetApiTokens(portalId={PortalId},filter={Filter},apiKey=<redacted>,scope={Scope},pageIndex={PageIndex},pageSize={PageSize})", portalId, filter, scope, pageIndex, pageSize);
            return this.Request.CreateResponse(HttpStatusCode.OK, response.Serialize());
        }

        /// <summary>
        /// Retrieves a dictionary with the API token keyword and its corresponding attribute based on the user scope.
        /// </summary>
        /// <returns>A dictionary with the API token keyword and its corresponding attribute.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]
        public HttpResponseMessage GetApiTokenKeys()
        {
			Logger.LogError(">>> Entering GetApiTokenKeys()");
            // The response.
            var response = new SortedDictionary<string, ApiTokenAttribute>();
			Logger.LogError("GetApiTokenKeys(): response → {Response}", response);

            // Checks if the user is authorized.
            var user = this.UserInfo;

            if (user.IsSuperUser)
            {
                // If the user is a superuser, sets the API token scope to host.
                response = this.apiTokenController.ApiTokenKeyList(ApiTokenScope.Host, Thread.CurrentThread.CurrentUICulture.Name);
            }
            else if (user.IsAdmin)
            {
                // If the user is an admin, sets the API token scope to portal.
                response = this.apiTokenController.ApiTokenKeyList(ApiTokenScope.Portal, Thread.CurrentThread.CurrentUICulture.Name);
            }
            else
            {
                // If the user is regular, set the API token scope to user.
                response = this.apiTokenController.ApiTokenKeyList(ApiTokenScope.User, Thread.CurrentThread.CurrentUICulture.Name);
            }

            // Returns the response.
			Logger.LogWarning("<<< Exiting GetApiTokenKeys()");
            return this.Request.CreateResponse(HttpStatusCode.OK, response.Values);
        }

        /// <summary>
        /// Creates a new <see cref="ApiToken"/> object with the specified parameters and returns it.
        /// </summary>
        /// <param name="data">The parameters for the creation of this token.</param>
        /// <returns>A new <see cref="ApiToken"/> object.</returns>
        [HttpPost]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]
        public HttpResponseMessage CreateApiToken(CreateApiTokenRequest data)
        {
			Logger.LogError(">>> Entering CreateApiToken(data={Data})", data);
            var settings = ApiTokenSettings.GetSettings(this.PortalId);
			Logger.LogError("CreateApiToken(data={Data}): settings → {Settings}", data, settings);
            if (!settings.ApiTokensEnabled || (!settings.AllowApiTokens && data.Scope != (int)ApiTokenScope.Host))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "API tokens are disabled");
            }

            var requestedScope = ApiTokenScope.User;
			Logger.LogError("CreateApiToken(data={Data}): requestedScope → {RequestedScope}", data, requestedScope);
            var requestedTimespan = data.TokenTimespan;
			Logger.LogError("CreateApiToken(data={Data}): requestedTimespan → {RequestedTimespan}", data, requestedTimespan);

            // Checks if the user is authorized.
            var user = this.UserInfo;
            if (user.IsSuperUser)
            {
                requestedScope = (ApiTokenScope)data.Scope;
            }
            else if (user.IsAdmin)
            {
                if (data.Scope > 1)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid scope");
                }
                else
                {
                    requestedScope = (ApiTokenScope)data.Scope;
                }

                requestedTimespan = Math.Min(requestedTimespan, (int)settings.MaximumSiteTimespan);
            }
            else
            {
                if (data.Scope > 1)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid scope");
                }

                requestedTimespan = (int)settings.UserTokenTimespan;
            }

            // Check the expiration time
            var expirationTime = DateTime.Now;
			Logger.LogError("CreateApiToken(data={Data}): expirationTime → {ExpirationTime}", data, expirationTime);
            var setTimespan = (ApiTokenTimespan)requestedTimespan;
			Logger.LogError("CreateApiToken(data={Data}): setTimespan → {SetTimespan}", data, setTimespan);
            switch (setTimespan)
            {
                case ApiTokenTimespan.Days30:
                    expirationTime = expirationTime.AddDays(30);
                    break;
                case ApiTokenTimespan.Days60:
                    expirationTime = expirationTime.AddDays(60);
                    break;
                case ApiTokenTimespan.Days90:
                    expirationTime = expirationTime.AddDays(90);
                    break;
                case ApiTokenTimespan.Days180:
                    expirationTime = expirationTime.AddDays(180);
                    break;
                case ApiTokenTimespan.Years1:
                    expirationTime = expirationTime.AddYears(1);
                    break;
                case ApiTokenTimespan.Years2:
                    expirationTime = expirationTime.AddYears(2);
                    break;
            }

            var token = this.apiTokenController.CreateApiToken(this.PortalId, data.TokenName.Trim(), requestedScope, expirationTime, data.ApiKeys, this.UserInfo.UserID);
			Logger.LogError("CreateApiToken(data={Data}): token → {Token}", data, token);
			Logger.LogWarning("<<< Exiting CreateApiToken(data={Data})", data);
            return this.Request.CreateResponse(HttpStatusCode.OK, token);
        }

        /// <summary>
        /// Revokes or deletes the specified API token of the user.
        /// </summary>
        /// <param name="data">The `RevokeDeleteApiTokenRequest` object which contains the ID of the API token to revoke or delete.</param>
        /// <returns>An HTTP response message with a boolean value indicating whether the token was successfully revoked or deleted.</returns>
        [HttpPost]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]
        public HttpResponseMessage RevokeOrDeleteApiToken(RevokeDeleteApiTokenRequest data)
        {
			Logger.LogError(">>> Entering RevokeOrDeleteApiToken(data={Data})", data);
            var token = this.apiTokenController.GetApiToken(data.ApiTokenId);
			Logger.LogError("RevokeOrDeleteApiToken(data={Data}): token → {Token}", data, token);
            if (token == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid token");
            }

            // Checks if the user is authorized.
            var user = this.UserInfo;
            var canManage = false;
			Logger.LogError("RevokeOrDeleteApiToken(data={Data}): canManage → {CanManage}", data, canManage);
            if (user.IsSuperUser)
            {
                canManage = true;
            }
            else if (user.IsAdmin)
            {
                if (token.PortalId != PortalSettings.Current.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "You have no access to this token.");
                }
                else
                {
                    canManage = true;
                }
            }
            else
            {
                if (token.PortalId != PortalSettings.Current.PortalId || token.CreatedByUserId != user.UserID)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "You have no access to this token.");
                }
                else
                {
                    canManage = true;
                }
            }

            if (canManage)
            {
                this.apiTokenController.RevokeOrDeleteApiToken(token, data.Delete, user.UserID);
            }

			Logger.LogWarning("<<< Exiting RevokeOrDeleteApiToken(data={Data})", data);
            return this.Request.CreateResponse(HttpStatusCode.OK, true);
        }

        /// <summary>
        /// Deletes expired API tokens.
        /// </summary>
        /// <remarks>
        /// If the user is a SuperUser, all expired API tokens across all portals will be deleted.
        /// If the user is an Admin, only the expired API tokens for the current portal will be deleted.
        /// For all other users, only their own expired API tokens will be deleted.
        /// </remarks>
        /// <returns>An HTTP response message with a boolean value indicating whether expired API tokens were deleted.</returns>
        [HttpPost]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.ManageApiTokens)]
        public HttpResponseMessage DeleteExpiredTokens()
        {
			Logger.LogError(">>> Entering DeleteExpiredTokens()");
            var portalId = this.PortalId;
            var user = this.UserInfo;
			Logger.LogError("DeleteExpiredTokens(): portalId → {PortalId}", portalId);
            var userId = user.UserID;
			Logger.LogError("DeleteExpiredTokens(): userId → {UserId}", userId);

            if (user.IsSuperUser)
            {
                portalId = -1;
                userId = -1;
            }
            else if (user.IsAdmin)
            {
                userId = -1;
            }

            this.apiTokenController.DeleteExpiredAndRevokedApiTokens(portalId, userId);

			Logger.LogWarning("<<< Exiting DeleteExpiredTokens()");
            return this.Request.CreateResponse(HttpStatusCode.OK, true);
        }

        /// GET: api/Security/GetCspSettings
        /// <summary>Gets CSP settings.</summary>
        /// <returns>CSP settings.</returns>
        [HttpGet]
        [RequireAdmin]
        public HttpResponseMessage GetCspSettings()
        {
			Logger.LogError(">>> Entering GetCspSettings()");
            try
            {
                _ = bool.TryParse(Config.GetSetting("DisableCsp"), out bool disableCsp);
				Logger.LogError("GetCspSettings(): _ → {_}", _);

                var response = new
                {
					Logger.LogDebug("GetCspSettings(): Results → {Results}", Results);
                    Success = true,
                    Results = new
                    {
                        Settings = new
						Logger.LogDebug("GetCspSettings(): Settings → {Settings}", Settings);
                        {
                            this.PortalSettings.CspHeaderMode,
                            this.PortalSettings.CspHeaderFixed,
                            this.PortalSettings.CspHeader,
                            this.PortalSettings.CspReportingHeader,
                            CspDisabled = disableCsp,
                        },
                    },
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetCspSettings()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/Security/UpdateCspSettings
        /// <summary>Updates CSP settings.</summary>
        /// <param name="request">The CSP settings.</param>
        /// <returns>CSP settings.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireAdmin]
        public HttpResponseMessage UpdateCspSettings(UpdateCspSettingsRequest request)
        {
			Logger.LogError(">>> Entering UpdateCspSettings(request={Request})", request);
            try
            {
                var policy = new ContentSecurityPolicy(true);
				Logger.LogError("UpdateCspSettings(request={Request}): policy → {Policy}", request, policy);
                var parser = new ContentSecurityPolicyParser(policy);
				Logger.LogError("UpdateCspSettings(request={Request}): parser → {Parser}", request, parser);
                try
                {
                    parser.Parse(request.CspHeader);
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in UpdateCspSettings(request={Request}): {Arg1}", request, ex.Message);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        Success = false,
                        Message = "Bad CspHeader - " + ex.Message,
                        Error = new
                        {
                            CspHeader = true,
                            CspHeaderErrors = new[] { ex.Message },
                            CspReportingHeader = false,
                            CspReportingHeaderErrors = Array.Empty<string>(),
                        },
                    });
                }

                if (!string.IsNullOrEmpty(request.CspReportingHeader))
                {
                    try
                    {
                        policy.AddReportEndpointHeader(request.CspReportingHeader);
                    }
                    catch (Exception ex)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, new
                        {
                            Success = false,
                            Message = "Bad CspReportingHeader - " + ex.Message,
                            Error = new
                            {
                                CspHeader = false,
                                CspHeaderErrors = Array.Empty<string>(),
                                CspReportingHeader = true,
                                CspReportingHeaderErrors = new[] { ex.Message },
                            },
                        });
                    }
                }

                PortalController.UpdatePortalSetting(this.PortalId, "CspHeaderMode", request.CspHeaderMode.ToString().ToUpperInvariant());
                PortalController.UpdatePortalSetting(this.PortalId, "CspHeaderFixed", request.CspHeaderFixed.ToString().ToUpperInvariant());
                PortalController.UpdatePortalSetting(this.PortalId, "CspHeader", request.CspHeader);
                PortalController.UpdatePortalSetting(this.PortalId, "CspReportingHeader", request.CspReportingHeader);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting UpdateCspSettings(request={Request})", request);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// <summary>
        /// Adds a portal alias.
        /// </summary>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The portalAlias.</returns>
        internal string AddPortalAlias(string portalAlias, int portalId)
        {
			Logger.LogError(">>> Entering AddPortalAlias(portalAlias={PortalAlias},portalId={PortalId})", portalAlias, portalId);
            if (!string.IsNullOrEmpty(portalAlias))
            {
                portalAlias = portalAlias.ToLowerInvariant().Trim('/');
				Logger.LogError("AddPortalAlias(portalAlias={PortalAlias},portalId={PortalId}): portalAlias → {PortalAlias1}", portalAlias, portalId, portalAlias);
                if (portalAlias.IndexOf("://", StringComparison.Ordinal) != -1)
                {
                    portalAlias = portalAlias.Remove(0, portalAlias.IndexOf("://", StringComparison.Ordinal) + 3);
                }

                var alias = this.portalAliasService.GetPortalAlias(portalAlias, portalId);
				Logger.LogError("AddPortalAlias(portalAlias={PortalAlias},portalId={PortalId}): alias → {Alias}", portalAlias, portalId, alias);
                if (alias == null)
                {
                    alias = new PortalAliasInfo();
                    alias.PortalId = portalId;
					Logger.LogDebug("AddPortalAlias(portalAlias={PortalAlias},portalId={PortalId}): alias.PortalId → {Arg2}", portalAlias, portalId, alias.PortalId);
                    alias.HttpAlias = portalAlias;
					Logger.LogDebug("AddPortalAlias(portalAlias={PortalAlias},portalId={PortalId}): alias.HttpAlias → {Arg2}", portalAlias, portalId, alias.HttpAlias);
                    this.portalAliasService.AddPortalAlias(alias);
                }
            }

			Logger.LogWarning("<<< Exiting AddPortalAlias(portalAlias={PortalAlias},portalId={PortalId})", portalAlias, portalId);
            return portalAlias;
        }

        private static string DisplayDate(DateTime userDate)
        {
			Logger.LogError(">>> Entering DisplayDate(userDate={UserDate})", userDate);
            var date = Null.NullString;
            date = !Null.IsNull(userDate) ? userDate.ToString(CultureInfo.InvariantCulture) : string.Empty;
			Logger.LogError("DisplayDate(userDate={UserDate}): date → {Date}", userDate, date);
			Logger.LogWarning("<<< Exiting DisplayDate(userDate={UserDate})", userDate);
            return date;
        }

        private int ValidateTabId(int tabId)
        {
			Logger.LogError(">>> Entering ValidateTabId(tabId={TabId})", tabId);
            var tab = TabController.Instance.GetTab(tabId, this.PortalId);
			Logger.LogError("ValidateTabId(tabId={TabId}): tab → {Tab}", tabId, tab);
			Logger.LogWarning("<<< Exiting ValidateTabId(tabId={TabId})", tabId);
            return tab?.TabID ?? Null.NullInteger;
        }

        private string GetTabName(int tabId)
        {
			Logger.LogError(">>> Entering GetTabName(tabId={TabId})", tabId);
            if (tabId == Null.NullInteger)
            {
                return string.Empty;
            }
            else
            {
                var tab = TabController.Instance.GetTab(tabId, this.PortalId);
				Logger.LogError("GetTabName(tabId={TabId}): tab → {Tab}", tabId, tab);
				Logger.LogWarning("<<< Exiting GetTabName(tabId={TabId})", tabId);
                return tab != null ? tab.TabName : string.Empty;
            }
        }

        private string GetTabPath(int tabId)
        {
			Logger.LogError(">>> Entering GetTabPath(tabId={TabId})", tabId);
            if (tabId == Null.NullInteger)
            {
                return string.Empty;
            }
            else
            {
                var tab = TabController.Instance.GetTab(tabId, this.PortalId);
				Logger.LogError("GetTabPath(tabId={TabId}): tab → {Tab}", tabId, tab);
				Logger.LogWarning("<<< Exiting GetTabPath(tabId={TabId})", tabId);
                return tab != null ? tab.TabPath : string.Empty;
            }
        }

        private string GetFilePath(string filePath)
        {
			Logger.LogError(">>> Entering GetFilePath(filePath={FilePath})", filePath);
            var path = Regex.Replace(filePath, Regex.Escape(this.applicationStatusInfo.ApplicationMapPath), string.Empty, RegexOptions.IgnoreCase);
			Logger.LogError("GetFilePath(filePath={FilePath}): path → {Path}", filePath, path);
			Logger.LogWarning("<<< Exiting GetFilePath(filePath={FilePath})", filePath);
            return path.TrimStart('\\');
        }
    }
}
