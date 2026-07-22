// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for moderation in the Groups module.</summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="roleProvider">The role provider.</param>
    /// <param name="roleController">The role controller.</param>
    /// <param name="eventManager">The event manager.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="userController">The user controller.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="hostSettings">The host settings.</param>
    [DnnAuthorize]
    public class ModerationServiceController(INavigationManager navigationManager, RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger, IHostSettings hostSettings)
        : DnnApiController
    {
		Logger.LogDebug(">>> Entering ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings})", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings);
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModerationServiceController));
        private readonly RoleProvider roleProvider = roleProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>();
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): Logger → {Logger}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, Logger);
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): roleProvider → {RoleProvider1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, roleProvider);
        private readonly IRoleController roleController = roleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
        private readonly IEventManager eventManager = eventManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>();
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): roleController → {RoleController1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, roleController);
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): eventManager → {EventManager1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, eventManager);
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): portalController → {PortalController1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, portalController);
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): userController → {UserController1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, userController);
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): eventLogger → {EventLogger1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, eventLogger);
		Logger.LogDebug("ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): hostSettings → {HostSettings1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, hostSettings);
        private int tabId;
        private int moduleId;
        private int roleId;
        private int memberId;
        private RoleInfo roleInfo;

        /// <summary>Initializes a new instance of the <see cref="ModerationServiceController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public ModerationServiceController(INavigationManager navigationManager)
            : this(navigationManager, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModerationServiceController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventManager">The event manager.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModerationServiceController(INavigationManager navigationManager, RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger)
            : this(navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, null)
        {
		Logger.LogDebug("<<< Exiting ModerationServiceController(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings})", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings);
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; } = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ApproveGroup(NotificationDTO postData)
        {
			Logger.LogDebug(">>> Entering ApproveGroup(postData={PostData})", postData);
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
				Logger.LogDebug("ApproveGroup(postData={PostData}): recipient → {Recipient}", postData, recipient);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
				Logger.LogDebug("ApproveGroup(postData={PostData}): notification → {Notification}", postData, notification);
                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate role");
                }

                if (!this.IsMod())
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized!");
                }

                this.roleInfo.Status = RoleStatus.Approved;
                RoleController.Instance.UpdateRole(this.roleInfo);
                var roleCreator = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);

				Logger.LogDebug("ApproveGroup(postData={PostData}): roleCreator → {RoleCreator}", postData, roleCreator);
                // Update the original creator's role
                RoleController.Instance.UpdateUserRole(this.PortalSettings.PortalId, roleCreator.UserID, this.roleInfo.RoleID, RoleStatus.Approved, true, false);
                GroupUtilities.CreateJournalEntry(this.roleInfo, roleCreator);

                var notifications = new Notifications(this.hostSettings);
                var siteAdmin = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.PortalSettings.AdministratorId);
				Logger.LogDebug("ApproveGroup(postData={PostData}): notifications → {Notifications}", postData, notifications);
				Logger.LogDebug("ApproveGroup(postData={PostData}): siteAdmin → {SiteAdmin}", postData, siteAdmin);
                notifications.AddGroupNotification(Constants.GroupApprovedNotification, this.tabId, this.moduleId, this.roleInfo, siteAdmin, new List<RoleInfo> { this.roleInfo });
                NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting ApproveGroup(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RejectGroup(NotificationDTO postData)
        {
			Logger.LogDebug(">>> Entering RejectGroup(postData={PostData})", postData);
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
				Logger.LogDebug("RejectGroup(postData={PostData}): recipient → {Recipient}", postData, recipient);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
				Logger.LogDebug("RejectGroup(postData={PostData}): notification → {Notification}", postData, notification);
                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate role");
                }

                if (!this.IsMod())
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized!");
                }

                var notifications = new Notifications(this.hostSettings);
                var roleCreator = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);
				Logger.LogDebug("RejectGroup(postData={PostData}): notifications → {Notifications}", postData, notifications);
				Logger.LogDebug("RejectGroup(postData={PostData}): roleCreator → {RoleCreator}", postData, roleCreator);
                var siteAdmin = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.PortalSettings.AdministratorId);
                notifications.AddGroupNotification(Constants.GroupRejectedNotification, this.tabId, this.moduleId, this.roleInfo, siteAdmin, new List<RoleInfo> { this.roleInfo }, roleCreator);
				Logger.LogDebug("RejectGroup(postData={PostData}): siteAdmin → {SiteAdmin}", postData, siteAdmin);

                var role = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == this.roleId);
                RoleController.Instance.DeleteRole(role);
				Logger.LogDebug("RejectGroup(postData={PostData}): role → {Role}", postData, role);
                NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting RejectGroup(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [SupportedModules("Social Groups")]
        public HttpResponseMessage JoinGroup(RoleDTO postData)
        {
			Logger.LogDebug(">>> Entering JoinGroup(postData={PostData})", postData);
            try
            {
                if (this.UserInfo.UserID >= 0 && postData.RoleId > -1)
                {
                    this.roleInfo = RoleController.Instance.GetRoleById(this.PortalSettings.PortalId, postData.RoleId);
					Logger.LogDebug("JoinGroup(postData={PostData}): roleInfo → {RoleInfo}", postData, roleInfo);
                    if (this.roleInfo != null)
                    {
                        var requireApproval = false;

						Logger.LogDebug("JoinGroup(postData={PostData}): requireApproval → {RequireApproval}", postData, requireApproval);
                        if (this.roleInfo.Settings.ContainsKey("ReviewMembers"))
                        {
                            requireApproval = Convert.ToBoolean(this.roleInfo.Settings["ReviewMembers"]);
                        }

                        if ((this.roleInfo.IsPublic || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName)) && !requireApproval)
                        {
                            RoleController.Instance.AddUserRole(this.PortalSettings.PortalId, this.UserInfo.UserID, this.roleInfo.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                            RoleController.Instance.UpdateRole(this.roleInfo);

                            var url = this.NavigationManager.NavigateURL(postData.GroupViewTabId, string.Empty, new[] { "groupid=" + this.roleInfo.RoleID });
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", URL = url });
                        }

                        if (this.roleInfo.IsPublic && requireApproval)
                        {
                            RoleController.Instance.AddUserRole(this.PortalSettings.PortalId, this.UserInfo.UserID, this.roleInfo.RoleID, RoleStatus.Pending, false, Null.NullDate, Null.NullDate);
                            var notifications = new Notifications(this.hostSettings);
                            notifications.AddGroupOwnerNotification(Constants.MemberPendingNotification, this.tabId, this.moduleId, this.roleInfo, this.UserInfo);
							Logger.LogDebug("JoinGroup(postData={PostData}): notifications → {Notifications}", postData, notifications);
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", URL = string.Empty });
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

			Logger.LogDebug("<<< Exiting JoinGroup(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [SupportedModules("Social Groups")]
        public HttpResponseMessage LeaveGroup(RoleDTO postData)
        {
			Logger.LogDebug(">>> Entering LeaveGroup(postData={PostData})", postData);
            var success = false;

			Logger.LogDebug("LeaveGroup(postData={PostData}): success → {Success}", postData, success);
            try
            {
                if (this.UserInfo.UserID >= 0 && postData.RoleId > 0)
                {
                    this.roleInfo = RoleController.Instance.GetRoleById(this.PortalSettings.PortalId, postData.RoleId);
					Logger.LogDebug("LeaveGroup(postData={PostData}): roleInfo → {RoleInfo}", postData, roleInfo);

                    if (this.roleInfo != null)
                    {
                        if (this.UserInfo.IsInRole(this.roleInfo.RoleName))
                        {
                            RoleController.DeleteUserRole(this.roleProvider, this.roleController, this.eventManager, this.portalController, this.userController, this.eventLogger, this.UserInfo, this.roleInfo, this.PortalSettings, false);
                        }

                        success = true;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            if (success)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }

			Logger.LogDebug("<<< Exiting LeaveGroup(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ApproveMember(NotificationDTO postData)
        {
			Logger.LogDebug(">>> Entering ApproveMember(postData={PostData})", postData);
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
				Logger.LogDebug("ApproveMember(postData={PostData}): recipient → {Recipient}", postData, recipient);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
				Logger.LogDebug("ApproveMember(postData={PostData}): notification → {Notification}", postData, notification);
                if (this.memberId <= 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Member");
                }

                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Role");
                }

                var member = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.memberId);

				Logger.LogDebug("ApproveMember(postData={PostData}): member → {Member}", postData, member);
                if (member != null)
                {
                    var memberRoleInfo = RoleController.Instance.GetUserRole(this.PortalSettings.PortalId, this.memberId, this.roleInfo.RoleID);
                    memberRoleInfo.Status = RoleStatus.Approved;
					Logger.LogDebug("ApproveMember(postData={PostData}): memberRoleInfo → {MemberRoleInfo}", postData, memberRoleInfo);
					Logger.LogDebug("ApproveMember(postData={PostData}): memberRoleInfo.Status → {Arg1}", postData, memberRoleInfo.Status);
                    RoleController.Instance.UpdateUserRole(this.PortalSettings.PortalId, this.memberId, this.roleInfo.RoleID, RoleStatus.Approved, false, false);

                    var notifications = new Notifications(this.hostSettings);
                    var groupOwner = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);
					Logger.LogDebug("ApproveMember(postData={PostData}): notifications → {Notifications}", postData, notifications);
					Logger.LogDebug("ApproveMember(postData={PostData}): groupOwner → {GroupOwner}", postData, groupOwner);
                    notifications.AddMemberNotification(Constants.MemberApprovedNotification, this.tabId, this.moduleId, this.roleInfo, groupOwner, member);
                    NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

			Logger.LogDebug("<<< Exiting ApproveMember(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RejectMember(NotificationDTO postData)
        {
			Logger.LogDebug(">>> Entering RejectMember(postData={PostData})", postData);
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
				Logger.LogDebug("RejectMember(postData={PostData}): recipient → {Recipient}", postData, recipient);
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
				Logger.LogDebug("RejectMember(postData={PostData}): notification → {Notification}", postData, notification);
                if (this.memberId <= 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Member");
                }

                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Role");
                }

                var member = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.memberId);

				Logger.LogDebug("RejectMember(postData={PostData}): member → {Member}", postData, member);
                if (member != null)
                {
                    RoleController.DeleteUserRole(this.roleProvider, this.roleController, this.eventManager, this.portalController, this.userController, this.eventLogger, member, this.roleInfo, this.PortalSettings, false);
                    var notifications = new Notifications(this.hostSettings);
                    var groupOwner = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);
					Logger.LogDebug("RejectMember(postData={PostData}): notifications → {Notifications}", postData, notifications);
					Logger.LogDebug("RejectMember(postData={PostData}): groupOwner → {GroupOwner}", postData, groupOwner);
                    notifications.AddMemberNotification(Constants.MemberRejectedNotification, this.tabId, this.moduleId, this.roleInfo, groupOwner, member);
                    NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

			Logger.LogDebug("<<< Exiting RejectMember(postData={PostData})", postData);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        private void ParseKey(string key)
        {
			Logger.LogDebug(">>> Entering ParseKey(key={Key})", key);
            this.tabId = -1;
            this.moduleId = -1;
			Logger.LogDebug("ParseKey(key={Key}): this.tabId → {Arg1}", key, this.tabId);
			Logger.LogDebug("ParseKey(key={Key}): moduleId → {ModuleId}", key, moduleId);
			Logger.LogDebug("ParseKey(key={Key}): this.moduleId → {Arg1}", key, this.moduleId);
			Logger.LogDebug("ParseKey(key={Key}): roleInfo → {RoleInfo}", key, roleInfo);
			Logger.LogDebug("ParseKey(key={Key}): this.roleInfo → {Arg1}", key, this.roleInfo);
			Logger.LogDebug("ParseKey(key={Key}): tabId → {TabId}", key, tabId);
            this.roleId = -1;
            this.memberId = -1;
            this.roleInfo = null;
			Logger.LogDebug("ParseKey(key={Key}): roleId → {RoleId}", key, roleId);
			Logger.LogDebug("ParseKey(key={Key}): this.roleId → {Arg1}", key, this.roleId);
			Logger.LogDebug("ParseKey(key={Key}): memberId → {MemberId}", key, memberId);
			Logger.LogDebug("ParseKey(key={Key}): this.memberId → {Arg1}", key, this.memberId);
            if (!string.IsNullOrEmpty(key))
            {
                string[] keys = key.Split(':');
                this.tabId = Convert.ToInt32(keys[0]);
                this.moduleId = Convert.ToInt32(keys[1]);
                this.roleId = Convert.ToInt32(keys[2]);
                if (keys.Length > 3)
                {
                    this.memberId = Convert.ToInt32(keys[3]);
                }
            }

            if (this.roleId > 0)
            {
                this.roleInfo = RoleController.Instance.GetRoleById(this.PortalSettings.PortalId, this.roleId);
            }
			Logger.LogDebug("<<< Exiting ParseKey(key={Key})", key);
        }

        private bool IsMod()
        {
			Logger.LogDebug(">>> Entering IsMod()");
            var objModulePermissions = new ModulePermissionCollection(CBO.FillCollection(DataProvider.Instance().GetModulePermissionsByModuleID(this.moduleId, -1), typeof(ModulePermissionInfo)));
			Logger.LogDebug("IsMod(): objModulePermissions → {ObjModulePermissions}", objModulePermissions);
			Logger.LogDebug("<<< Exiting IsMod()");
            return ModulePermissionController.HasModulePermission(objModulePermissions, "MODGROUP");
        }

        public class NotificationDTO
        {
            public int NotificationId { get; set; }
        }

        public class RoleDTO
        {
            public int RoleId { get; set; }

            public int GroupViewTabId { get; set; }
        }
    }
}
