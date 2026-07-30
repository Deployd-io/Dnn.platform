// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for the Member Directory module.</summary>
    /// <param name="listController">The list controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    [SupportedModules("DotNetNuke.Modules.MemberDirectory")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class MemberDirectoryController(ListController listController, IHostSettings hostSettings)
        : DnnApiController
    {
		Logger.LogDebug(">>> Entering MemberDirectoryController(listController={ListController},hostSettings={HostSettings})", listController, hostSettings);
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MemberDirectoryController));
        private readonly ListController listController = listController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ListController>();
		Logger.LogDebug("MemberDirectoryController(listController={ListController},hostSettings={HostSettings}): Logger → {Logger}", listController, hostSettings, Logger);
		Logger.LogDebug("MemberDirectoryController(listController={ListController},hostSettings={HostSettings}): listController → {ListController1}", listController, hostSettings, listController);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

		Logger.LogDebug("MemberDirectoryController(listController={ListController},hostSettings={HostSettings}): hostSettings → {HostSettings1}", listController, hostSettings, hostSettings);
        /// <summary>Initializes a new instance of the <see cref="MemberDirectoryController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with ListController. Scheduled removal in v12.0.0.")]
        public MemberDirectoryController()
            : this(null, null)
        {
        }

        [HttpGet]
        public HttpResponseMessage AdvancedSearch(int userId, int groupId, int pageIndex, int pageSize, string searchTerm1, string searchTerm2, string searchTerm3, string searchTerm4)
        {
			Logger.LogDebug(">>> Entering AdvancedSearch(userId={UserId},groupId={GroupId},pageIndex={PageIndex},pageSize={PageSize},searchTerm1={SearchTerm1},searchTerm2={SearchTerm2},searchTerm3={SearchTerm3},searchTerm4={SearchTerm4})", userId, groupId, pageIndex, pageSize, searchTerm1, searchTerm2, searchTerm3, searchTerm4);
            try
            {
                if (userId < 0)
                {
                    userId = this.PortalSettings.UserId;
                }
				Logger.LogDebug("AdvancedSearch(userId={UserId},groupId={GroupId},pageIndex={PageIndex},pageSize={PageSize},searchTerm1={SearchTerm1},searchTerm2={SearchTerm2},searchTerm3={SearchTerm3},searchTerm4={SearchTerm4}): userId → {UserId1}", userId, groupId, pageIndex, pageSize, searchTerm1, searchTerm2, searchTerm3, searchTerm4, userId);

                var searchField1 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField1", "DisplayName");
                var searchField2 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField2", "Email");
				Logger.LogDebug("AdvancedSearch(userId={UserId},groupId={GroupId},pageIndex={PageIndex},pageSize={PageSize},searchTerm1={SearchTerm1},searchTerm2={SearchTerm2},searchTerm3={SearchTerm3},searchTerm4={SearchTerm4}): searchField1 → {SearchField1}", userId, groupId, pageIndex, pageSize, searchTerm1, searchTerm2, searchTerm3, searchTerm4, searchField1);
				Logger.LogDebug("AdvancedSearch(userId={UserId},groupId={GroupId},pageIndex={PageIndex},pageSize={PageSize},searchTerm1={SearchTerm1},searchTerm2={SearchTerm2},searchTerm3={SearchTerm3},searchTerm4={SearchTerm4}): searchField2 → {SearchField2}", userId, groupId, pageIndex, pageSize, searchTerm1, searchTerm2, searchTerm3, searchTerm4, searchField2);
                var searchField3 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField3", "City");
                var searchField4 = GetSetting(this.ActiveModule.TabModuleSettings, "SearchField4", "Country");
				Logger.LogDebug("AdvancedSearch(userId={UserId},groupId={GroupId},pageIndex={PageIndex},pageSize={PageSize},searchTerm1={SearchTerm1},searchTerm2={SearchTerm2},searchTerm3={SearchTerm3},searchTerm4={SearchTerm4}): searchField3 → {SearchField3}", userId, groupId, pageIndex, pageSize, searchTerm1, searchTerm2, searchTerm3, searchTerm4, searchField3);
				Logger.LogDebug("AdvancedSearch(userId={UserId},groupId={GroupId},pageIndex={PageIndex},pageSize={PageSize},searchTerm1={SearchTerm1},searchTerm2={SearchTerm2},searchTerm3={SearchTerm3},searchTerm4={SearchTerm4}): searchField4 → {SearchField4}", userId, groupId, pageIndex, pageSize, searchTerm1, searchTerm2, searchTerm3, searchTerm4, searchField4);

                var propertyNames = string.Empty;
                var propertyValues = string.Empty;
                AddSearchTerm(this.listController, ref propertyNames, ref propertyValues, searchField1, searchTerm1);
                AddSearchTerm(this.listController, ref propertyNames, ref propertyValues, searchField2, searchTerm2);
                AddSearchTerm(this.listController, ref propertyNames, ref propertyValues, searchField3, searchTerm3);
                AddSearchTerm(this.listController, ref propertyNames, ref propertyValues, searchField4, searchTerm4);

                var members = this.GetUsers(userId, groupId, searchTerm1, pageIndex, pageSize, propertyNames, propertyValues);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetMembers(members));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AdvancedSearch(userId={UserId},groupId={GroupId},pageIndex={PageIndex},pageSize={PageSize},searchTerm1={SearchTerm1},searchTerm2={SearchTerm2},searchTerm3={SearchTerm3},searchTerm4={SearchTerm4})", userId, groupId, pageIndex, pageSize, searchTerm1, searchTerm2, searchTerm3, searchTerm4);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage BasicSearch(int groupId, string searchTerm, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering BasicSearch(groupId={GroupId},searchTerm={SearchTerm},pageIndex={PageIndex},pageSize={PageSize})", groupId, searchTerm, pageIndex, pageSize);
            try
            {
                var users = this.GetUsers(-1, groupId, string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.Trim(), pageIndex, pageSize, string.Empty, string.Empty);
                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetMembers(users));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting BasicSearch(groupId={GroupId},searchTerm={SearchTerm},pageIndex={PageIndex},pageSize={PageSize})", groupId, searchTerm, pageIndex, pageSize);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetMember(int userId)
        {
			Logger.LogDebug(">>> Entering GetMember(userId={UserId})", userId);
            try
            {
                var users = new List<UserInfo>();
                var user = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, userId);
				Logger.LogDebug("GetMember(userId={UserId}): users → {Users}", userId, users);
				Logger.LogDebug("GetMember(userId={UserId}): user → {User}", userId, user);
                users.Add(user);

                return this.Request.CreateResponse(HttpStatusCode.OK, this.GetMembers(users));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetMember(userId={UserId})", userId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetSuggestions(int groupId, string displayName)
        {
			Logger.LogDebug(">>> Entering GetSuggestions(groupId={GroupId},displayName={DisplayName})", groupId, displayName);
            try
            {
                var names = (from UserInfo user in this.GetUsers(-1, groupId, displayName.Trim(), 0, 10, string.Empty, string.Empty)
                             select new { label = user.DisplayName, value = user.DisplayName, userId = user.UserID })
                                .ToList();

                return this.Request.CreateResponse(HttpStatusCode.OK, names);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting GetSuggestions(groupId={GroupId},displayName={DisplayName})", groupId, displayName);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AcceptFriend(FriendDTO postData)
        {
			Logger.LogDebug(">>> Entering AcceptFriend(postData={PostData})", postData);
            try
            {
                var friend = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.AcceptFriend(friend);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AcceptFriend(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddFriend(FriendDTO postData)
        {
			Logger.LogDebug(">>> Entering AddFriend(postData={PostData})", postData);
            try
            {
                var friend = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.AddFriend(friend);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting AddFriend(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Follow(FollowDTO postData)
        {
			Logger.LogDebug(">>> Entering Follow(postData={PostData})", postData);
            try
            {
                var follow = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, postData.FollowId);
                FollowersController.Instance.FollowUser(follow);
				Logger.LogDebug("Follow(postData={PostData}): follow → {Follow}", postData, follow);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting Follow(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveFriend(FriendDTO postData)
        {
			Logger.LogDebug(">>> Entering RemoveFriend(postData={PostData})", postData);
            try
            {
                var friend = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, postData.FriendId);
                FriendsController.Instance.DeleteFriend(friend);
				Logger.LogDebug("RemoveFriend(postData={PostData}): friend → {Friend}", postData, friend);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting RemoveFriend(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UnFollow(FollowDTO postData)
        {
			Logger.LogDebug(">>> Entering UnFollow(postData={PostData})", postData);
            try
            {
                var follow = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, postData.FollowId);
                FollowersController.Instance.UnFollowUser(follow);
				Logger.LogDebug("UnFollow(postData={PostData}): follow → {Follow}", postData, follow);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting UnFollow(postData={PostData})", postData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static void AddSearchTerm(ListController listController, ref string propertyNames, ref string propertyValues, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                propertyNames += name + ",";

                if (name.Equals("Country", StringComparison.InvariantCultureIgnoreCase) ||
                    name.Equals("Region", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = GetMatchedListEntryIds(listController, name, value);
                }

                propertyValues += value + ",";
            }
        }

        private static string GetMatchedListEntryIds(ListController listController, string name, string value)
        {
			Logger.LogDebug(">>> Entering GetMatchedListEntryIds(listController={ListController},name={Name},value={Value})", listController, name, value);
            var listEntries = listController.GetListEntryInfoItems(name)
                .Where(i => i.Text.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)
                            || i.TextNonLocalized.StartsWith(value, StringComparison.InvariantCultureIgnoreCase)
                            || i.Value.StartsWith(value, StringComparison.InvariantCultureIgnoreCase))
                .Select(i => i.EntryID);

			Logger.LogDebug("<<< Exiting GetMatchedListEntryIds(listController={ListController},name={Name},value={Value})", listController, name, value);
            return $"${string.Join("$", listEntries)}$";
        }

        private static string GetSetting(IDictionary settings, string key, string defaultValue)
        {
			Logger.LogDebug(">>> Entering GetSetting(settings={Settings},key={Key},defaultValue={DefaultValue})", settings, key, defaultValue);
            var setting = defaultValue;
            if (settings[key] != null)
			Logger.LogDebug("GetSetting(settings={Settings},key={Key},defaultValue={DefaultValue}): setting → {Setting}", settings, key, defaultValue, setting);
            {
                setting = Convert.ToString(settings[key]);
            }

			Logger.LogDebug("<<< Exiting GetSetting(settings={Settings},key={Key},defaultValue={DefaultValue})", settings, key, defaultValue);
            return setting;
        }

        private bool CanViewGroupMembers(int portalId, int groupId)
        {
            var group = RoleController.Instance.GetRole(portalId, r => r.RoleID == groupId);
            if (group == null)
            {
                return false;
            }

            var canView = (group.SecurityMode == SecurityMode.SecurityRole)
                               ? this.PortalSettings.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName)
                               : this.PortalSettings.UserInfo.IsInRole(group.RoleName);

            // if current user can view the group page and group is public, then should be able to view members.
            if (!canView)
            {
                canView = ModulePermissionController.CanViewModule(this.ActiveModule) && group.IsPublic;
            }

            return canView;
        }

        private IList<Member> GetMembers(IEnumerable<UserInfo> users)
        {
            return users.Select(user => new Member(user, this.PortalSettings)).ToList();
        }

        private IEnumerable<UserInfo> GetUsers(int userId, int groupId, string searchTerm, int pageIndex, int pageSize, string propertyNames, string propertyValues)
        {
            var portalId = this.PortalSettings.PortalId;
            var isAdmin = this.PortalSettings.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);

            var filterBy = GetSetting(this.ActiveModule.ModuleSettings, "FilterBy", string.Empty);
            var filterValue = GetSetting(this.ActiveModule.ModuleSettings, "FilterValue", string.Empty);

            if (filterBy == "Group" && filterValue == "-1" && groupId > 0)
            {
                filterValue = groupId.ToString();
            }

            var sortField = GetSetting(this.ActiveModule.TabModuleSettings, "SortField", "DisplayName");

            // QuickFix DNN-6096. See: https://dnntracker.atlassian.net/browse/DNN-6096
            // Instead of changing the available SortFields, we'll use "UserId" as SortField if the TabModuleSetting SortField was CreatedOnDate.
            // This is because the GetUsersBasicSearch and GetUsersAdvancedSearch do not allow sorting on CreatedOnDate. Sorting on UserId however
            // has the same effect as sorting on CreatedOnDate.
            if (sortField.Equals("CreatedOnDate", StringComparison.InvariantCultureIgnoreCase))
            {
                sortField = "UserId";
            }

            var sortOrder = GetSetting(this.ActiveModule.TabModuleSettings, "SortOrder", "ASC");

            var excludeHostUsers = bool.Parse(GetSetting(this.ActiveModule.TabModuleSettings, "ExcludeHostUsers", "false"));
            var isBasicSearch = false;
            if (string.IsNullOrEmpty(propertyNames))
            {
                isBasicSearch = true;
                AddSearchTerm(this.listController, ref propertyNames, ref propertyValues, "DisplayName", searchTerm);
            }

            IList<UserInfo> users;
            switch (filterBy)
            {
                case "User":
                    users = new List<UserInfo> { UserController.GetUserById(this.hostSettings, portalId, userId) };
                    break;
                case "Group":
                    if (groupId == -1)
                    {
                        groupId = int.Parse(filterValue);
                    }

                    if (this.CanViewGroupMembers(portalId, groupId))
                    {
                        users = UserController.Instance.GetUsersAdvancedSearch(
                            portalId,
                            this.PortalSettings.UserId,
                            userId,
                            groupId,
                            -1,
                            isAdmin,
                            pageIndex,
                            pageSize,
                            sortField,
                            sortOrder == "ASC",
                            propertyNames,
                            propertyValues);
                    }
                    else
                    {
                        users = new List<UserInfo>();
                    }

                    break;
                case "Relationship":
                    users = UserController.Instance.GetUsersAdvancedSearch(
                        portalId,
                        this.PortalSettings.UserId,
                        userId,
                        -1,
                        int.Parse(filterValue),
                        isAdmin,
                        pageIndex,
                        pageSize,
                        sortField,
                        sortOrder == "ASC",
                        propertyNames,
                        propertyValues);
                    break;
                case "ProfileProperty":
                    var propertyValue = GetSetting(this.ActiveModule.ModuleSettings, "FilterPropertyValue", string.Empty);
                    AddSearchTerm(this.listController, ref propertyNames, ref propertyValues, filterValue, propertyValue);

                    users = UserController.Instance.GetUsersAdvancedSearch(
                        portalId,
                        this.PortalSettings.UserId,
                        userId,
                        -1,
                        -1,
                        isAdmin,
                        pageIndex,
                        pageSize,
                        sortField,
                        sortOrder == "ASC",
                        propertyNames,
                        propertyValues);
                    break;
                default:
                    users = isBasicSearch
                        ? UserController.Instance.GetUsersBasicSearch(
                            this.PortalSettings.PortalId,
                            pageIndex,
                            pageSize,
                            sortField,
                            sortOrder == "ASC",
                            "DisplayName",
                            searchTerm)
                        : UserController.Instance.GetUsersAdvancedSearch(
                            portalId,
                            this.PortalSettings.UserId,
                            userId,
                            -1,
                            -1,
                            isAdmin,
                            pageIndex,
                            pageSize,
                            sortField,
                            sortOrder == "ASC",
                            propertyNames,
                            propertyValues);
                    break;
            }

            if (excludeHostUsers)
            {
                return this.FilterExcludedUsers(users);
            }

            return users;
        }

        private IEnumerable<UserInfo> FilterExcludedUsers(IEnumerable<UserInfo> users)
        {
            return users.Where(u => !u.IsSuperUser).Select(u => u).ToList();
        }

        public class FollowDTO
        {
            public int FollowId { get; set; }
        }

        public class FriendDTO
        {
            public int FriendId { get; set; }
        }
    }
}
