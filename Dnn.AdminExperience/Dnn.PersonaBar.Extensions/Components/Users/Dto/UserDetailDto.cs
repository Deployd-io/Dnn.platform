// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using Microsoft.Extensions.DependencyInjection;

    [DataContract]
    public class UserDetailDto : UserBasicDto
    {
        public UserDetailDto()
        {
		eventLogger.LogDebug(">>> Entering UserDetailDto()");
		eventLogger.LogDebug("<<< Exiting UserDetailDto()");
        }

        public UserDetailDto(UserInfo user)
            : base(user)
        {
            this.LastLogin = user.Membership.LastLoginDate;
            this.LastActivity = user.Membership.LastActivityDate;
			eventLogger.LogDebug("UserDetailDto(user={User}): this.LastLogin → {Arg1}", user, this.LastLogin);
			eventLogger.LogDebug("UserDetailDto(user={User}): this.LastActivity → {Arg1}", user, this.LastActivity);
            this.LastPasswordChange = user.Membership.LastPasswordChangeDate;
            this.LastLockout = user.Membership.LastLockoutDate;
#pragma warning disable CS0618 // Type or member is obsolete
			eventLogger.LogDebug("UserDetailDto(user={User}): this.LastPasswordChange → {Arg1}", user, this.LastPasswordChange);
			eventLogger.LogDebug("UserDetailDto(user={User}): this.LastLockout → {Arg1}", user, this.LastLockout);
            this.IsOnline = user.Membership.IsOnLine;
#pragma warning restore CS0618 // Type or member is obsolete
            this.IsLocked = user.Membership.LockedOut;
            this.NeedUpdatePassword = user.Membership.UpdatePassword;
			eventLogger.LogDebug("UserDetailDto(user={User}): this.IsOnline → {Arg1}", user, this.IsOnline);
			eventLogger.LogDebug("UserDetailDto(user={User}): this.IsLocked → {Arg1}", user, this.IsLocked);
			eventLogger.LogDebug("UserDetailDto(user={User}): this.NeedUpdatePassword → {Arg1}", user, this.NeedUpdatePassword);
            this.PortalId = user.PortalID;
            this.UserFolder = FolderManager.Instance.GetUserFolder(user).FolderPath.Substring(6);
            var userFolder = FolderManager.Instance.GetUserFolder(user);
			eventLogger.LogDebug("UserDetailDto(user={User}): userFolder → {UserFolder}", user, userFolder);
            if (userFolder != null)
            {
                this.UserFolderId = userFolder.FolderID;

				eventLogger.LogDebug("UserDetailDto(user={User}): this.PortalId → {Arg1}", user, this.PortalId);
				eventLogger.LogDebug("UserDetailDto(user={User}): this.UserFolderId → {Arg1}", user, this.UserFolderId);
                // check whether user had upload files
                var files = FolderManager.Instance.GetFiles(userFolder, true);
				eventLogger.LogDebug("UserDetailDto(user={User}): files → {Files}", user, files);
                this.HasUserFiles = files.Any();
            }

            this.HasAgreedToTermsOn = user.HasAgreedToTermsOn;
			eventLogger.LogDebug("UserDetailDto(user={User}): this.HasAgreedToTermsOn → {Arg1}", user, this.HasAgreedToTermsOn);
            this.ProfileUrl = this.UserId > 0 ? Globals.UserProfileURL(this.UserId) : null;
            this.EditProfileUrl = this.UserId > 0 ? GetSettingUrl(this.PortalId, this.UserId) : null;
        }

        [DataMember(Name = "profileUrl")]
        public string ProfileUrl { get; set; }

        [DataMember(Name = "editProfileUrl")]
        public string EditProfileUrl { get; set; }

        [DataMember(Name = "lastLogin")]
        public DateTime LastLogin { get; set; }

        [DataMember(Name = "lastActivity")]
        public DateTime LastActivity { get; set; }

        [DataMember(Name = "lastPasswordChange")]
        public DateTime LastPasswordChange { get; set; }

        [DataMember(Name = "lastLockout")]
        public DateTime LastLockout { get; set; }

        [DataMember(Name = "isOnline")]
        public bool IsOnline { get; set; }

        [DataMember(Name = "isLocked")]
        public bool IsLocked { get; set; }

        [DataMember(Name = "needUpdatePassword")]
        public bool NeedUpdatePassword { get; set; }

        [IgnoreDataMember]
        public int PortalId { get; set; }

        [DataMember(Name = "userFolder")]
        public string UserFolder { get; set; }

        [DataMember(Name = "userFolderId")]
        public int UserFolderId { get; set; }

        [DataMember(Name = "hasUserFiles")]
        public bool HasUserFiles { get; set; }

        [DataMember(Name = "hasAgreedToTermsOn")]
        public DateTime HasAgreedToTermsOn { get; set; }

        private static INavigationManager NavigationManager => Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        private static string GetSettingUrl(int portalId, int userId)
        {
			eventLogger.LogDebug(">>> Entering GetSettingUrl(portalId={PortalId},userId={UserId})", portalId, userId);
            var module = ModuleController.Instance.GetModulesByDefinition(UserController.Instance.GetUserById(portalId, userId).IsSuperUser ? -1 : portalId, "User Accounts")
                .Cast<ModuleInfo>().FirstOrDefault();
            if (module == null)
            {
                return string.Empty;
            }

            var tabId = TabController.Instance.GetTabsByModuleID(module.ModuleID).Keys.FirstOrDefault();
			eventLogger.LogDebug("GetSettingUrl(portalId={PortalId},userId={UserId}): tabId → {TabId}", portalId, userId, tabId);
            if (tabId <= 0)
            {
                return string.Empty;
            }

            var extraParams = new Dictionary<string, string>();
			eventLogger.LogDebug("GetSettingUrl(portalId={PortalId},userId={UserId}): extraParams → {ExtraParams}", portalId, userId, extraParams);
            var skinSrc = Globals.HostPath + "Skins/_default/popUpSkin";
			eventLogger.LogDebug("GetSettingUrl(portalId={PortalId},userId={UserId}): skinSrc → {SkinSrc}", portalId, userId, skinSrc);
            var containerSrc = Globals.HostPath + "Containers/_default/popUpContainer";
			eventLogger.LogDebug("GetSettingUrl(portalId={PortalId},userId={UserId}): containerSrc → {ContainerSrc}", portalId, userId, containerSrc);
            extraParams.Add("SkinSrc", skinSrc);
            extraParams.Add("ContainerSrc", containerSrc);
            extraParams.Add("mid", module.ModuleID.ToString(CultureInfo.InvariantCulture));
            extraParams.Add("popUp", "true");
            extraParams.Add("UserId", userId.ToString(CultureInfo.InvariantCulture));
            extraParams.Add("editprofile", "true");

            // ctl/Edit/mid/345/packageid/52
			eventLogger.LogDebug("<<< Exiting GetSettingUrl(portalId={PortalId},userId={UserId})", portalId, userId);
            return NavigationManager.NavigateURL(
                tabId,
                PortalSettings.Current,
                "Edit",
                extraParams.Select(p => $"{p.Key}={p.Value}").ToArray());
        }
    }
}
