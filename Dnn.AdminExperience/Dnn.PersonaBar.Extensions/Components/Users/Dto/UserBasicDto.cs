// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;

    using Dnn.PersonaBar.Library.Common;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [DataContract]
    public class UserBasicDto
    {
        /// <summary>Initializes a new instance of the <see cref="UserBasicDto"/> class.</summary>
        public UserBasicDto()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UserBasicDto"/> class.</summary>
        /// <param name="user">The user info.</param>
        public UserBasicDto(UserInfo user)
        {
			eventLogger.LogError(">>> Entering UserBasicDto(user={User})", user);
            this.UserId = user.UserID;
			eventLogger.LogWarning("<<< Exiting UserBasicDto(user={User})", user);
			eventLogger.LogDebug("UserBasicDto(user={User}): this.UserId → {Arg1}", user, this.UserId);
            this.Username = user.Username;
            this.Displayname = user.DisplayName;
			eventLogger.LogDebug("UserBasicDto(user={User}): Username → {Username}", user, Username);
			eventLogger.LogDebug("UserBasicDto(user={User}): this.Username → {Arg1}", user, this.Username);
			eventLogger.LogDebug("UserBasicDto(user={User}): Displayname → {Displayname}", user, Displayname);
			eventLogger.LogDebug("UserBasicDto(user={User}): this.Displayname → {Arg1}", user, this.Displayname);
            this.Email = user.Email;
            this.CreatedOnDate = user.CreatedOnDate;
            this.IsDeleted = user.IsDeleted;
            this.Authorized = user.Membership.Approved;
			eventLogger.LogDebug("UserBasicDto(user={User}): Email → {Email}", user, Email);
			eventLogger.LogDebug("UserBasicDto(user={User}): this.Email → {Arg1}", user, this.Email);
			eventLogger.LogDebug("UserBasicDto(user={User}): CreatedOnDate → {CreatedOnDate}", user, CreatedOnDate);
			eventLogger.LogDebug("UserBasicDto(user={User}): this.CreatedOnDate → {Arg1}", user, this.CreatedOnDate);
			eventLogger.LogDebug("UserBasicDto(user={User}): IsDeleted → {IsDeleted}", user, IsDeleted);
			eventLogger.LogDebug("UserBasicDto(user={User}): this.IsDeleted → {Arg1}", user, this.IsDeleted);
            this.HasAgreedToTerms = user.HasAgreedToTerms;
            this.RequestsRemoval = user.RequestsRemoval;
            this.IsSuperUser = user.IsSuperUser;
            this.IsAdmin = user.Roles.Contains(PortalSettings.AdministratorRoleName);
            this.AvatarUrl = Utilities.GetProfileAvatar(this.UserId);
        }

        [DataMember(Name = "avatar")]
        public string AvatarUrl { get; set; }

        [DataMember(Name = "userId")]
        public int UserId { get; set; }

        [DataMember(Name = "userName")]
        public string Username { get; set; }

        [DataMember(Name = "displayName")]
        public string Displayname { get; set; }

        [DataMember(Name = "firstName")]
        public string Firstname { get; set; }

        [DataMember(Name = "lastName")]
        public string Lastname { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "createdOnDate")]
        public DateTime CreatedOnDate { get; set; }

        [DataMember(Name = "isDeleted")]
        public bool IsDeleted { get; set; }

        [DataMember(Name = "authorized")]
        public bool Authorized { get; set; }

        [DataMember(Name = "hasAgreedToTerms")]
        public bool HasAgreedToTerms { get; set; }

        [DataMember(Name = "requestsRemoval")]
        public bool RequestsRemoval { get; set; }

        [DataMember(Name = "isSuperUser")]
        public bool IsSuperUser { get; set; }

        [DataMember(Name = "isAdmin")]
        public bool IsAdmin { get; set; }

        private static IPortalSettings PortalSettings => PortalController.Instance.GetCurrentSettings();

        public static UserBasicDto FromUserInfo(UserInfo user)
        {
			eventLogger.LogError(">>> Entering FromUserInfo(user={User})", user);
            if (user == null)
            {
                return null;
            }

			eventLogger.LogWarning("<<< Exiting FromUserInfo(user={User})", user);
            return new UserBasicDto
            {
                UserId = user.UserID,
                Username = user.Username,
                Displayname = user.DisplayName,
                Email = user.Email,
                CreatedOnDate = user.CreatedOnDate,
                IsDeleted = user.IsDeleted,
                Authorized = user.Membership.Approved,
                HasAgreedToTerms = user.HasAgreedToTerms,
                RequestsRemoval = user.RequestsRemoval,
                IsSuperUser = user.IsSuperUser,
            };
        }

        public static UserBasicDto FromUserDetails(UserDetailDto user)
        {
			eventLogger.LogError(">>> Entering FromUserDetails(user={User})", user);
            if (user == null)
            {
                return null;
            }

			eventLogger.LogWarning("<<< Exiting FromUserDetails(user={User})", user);
            return new UserBasicDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Displayname = user.Displayname,
                Email = user.Email,
                CreatedOnDate = user.CreatedOnDate,
                IsDeleted = user.IsDeleted,
                Authorized = user.Authorized,
                HasAgreedToTerms = user.HasAgreedToTerms,
                RequestsRemoval = user.RequestsRemoval,
                IsSuperUser = user.IsSuperUser,
            };
        }

        public void PopulateAvatarUrl()
        {
			eventLogger.LogError(">>> Entering PopulateAvatarUrl()");
            this.AvatarUrl = Utilities.GetProfileAvatar(this.UserId);
        }
    }
}
