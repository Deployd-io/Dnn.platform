// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Services.Search.Entities;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>Search Result Controller for Tab Indexer.</summary>
    [Serializable]
    public class UserResultController : BaseResultController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserResultController));
        private const string LocalizedResxFile = "~/DesktopModules/Admin/SearchResults/App_LocalResources/SearchableModules.resx";

        private static readonly Regex SearchResultMatchRegex = new Regex(@"^(\d+)_", RegexOptions.Compiled);
        private static readonly char[] RoleSeparator = [',',];

        /// <inheritdoc />
        public override string LocalizedSearchTypeName => Localization.GetString("Crawler_user", LocalizedResxFile);

        private static IPortalSettings PortalSettings => PortalController.Instance.GetCurrentSettings();

        /// <inheritdoc />
        public override bool HasViewPermission(SearchResult searchResult)
        {
			Logger.LogDebug(">>> Entering HasViewPermission(searchResult={SearchResult})", searchResult);
            var userId = GetUserId(searchResult);
            if (userId == Null.NullInteger)
			Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): userId → {UserId}", searchResult, userId);
            {
                return false;
            }

            var userInSearchResult = UserController.GetUserById(PortalSettings.PortalId, userId);
			Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): userInSearchResult → {UserInSearchResult}", searchResult, userInSearchResult);
            if (userInSearchResult == null || userInSearchResult.IsDeleted)
            {
                return false;
            }

            if (searchResult.UniqueKey.Contains("adminonly", StringComparison.OrdinalIgnoreCase))
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
				Logger.LogDebug("HasViewPermission(searchResult={SearchResult}): currentUser → {CurrentUser}", searchResult, currentUser);
                return currentUser.IsSuperUser
                        || currentUser.IsInRole("Administrators")
                        || currentUser.UserID == userId;
            }

            if (searchResult.UniqueKey.Contains("friendsandgroups", StringComparison.OrdinalIgnoreCase))
            {
                var extendedVisibility =
                    searchResult.UniqueKey.IndexOf("_", StringComparison.Ordinal) != searchResult.UniqueKey.LastIndexOf("_", StringComparison.Ordinal)
                        ? searchResult.UniqueKey.Split('_')[2]
                        : string.Empty;
                return HasSocialRelationship(userInSearchResult, UserController.Instance.GetCurrentUserInfo(), extendedVisibility);
            }

            if (searchResult.UniqueKey.Contains("membersonly", StringComparison.OrdinalIgnoreCase))
            {
                return UserController.Instance.GetCurrentUserInfo().UserID != Null.NullInteger;
            }

            if (searchResult.UniqueKey.Contains("allusers", StringComparison.OrdinalIgnoreCase))
            {
                var scopeForRoles =
                    PortalController.GetPortalSetting("SearchResult_ScopeForRoles", searchResult.PortalId, string.Empty)
                        .Split(RoleSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (scopeForRoles.Count > 0)
                {
                    if (userInSearchResult.IsSuperUser)
                    {
                        return scopeForRoles.Contains("Superusers");
                    }

                    return scopeForRoles.Any(i => userInSearchResult.IsInRole(i));
                }

			Logger.LogDebug("<<< Exiting HasViewPermission(searchResult={SearchResult})", searchResult);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override string GetDocUrl(SearchResult searchResult)
        {
			Logger.LogDebug(">>> Entering GetDocUrl(searchResult={SearchResult})", searchResult);
            var url = TestableGlobals.Instance.NavigateURL(PortalSettings.UserTabId, string.Empty, "userid=" + GetUserId(searchResult));
			Logger.LogDebug("GetDocUrl(searchResult={SearchResult}): url → {Url}", searchResult, url);
			Logger.LogDebug("<<< Exiting GetDocUrl(searchResult={SearchResult})", searchResult);
            return url;
        }

        private static int GetUserId(SearchDocumentToDelete searchResult)
        {
			Logger.LogDebug(">>> Entering GetUserId(searchResult={SearchResult})", searchResult);
            var match = SearchResultMatchRegex.Match(searchResult.UniqueKey);
			Logger.LogDebug("GetUserId(searchResult={SearchResult}): match → {Match}", searchResult, match);
			Logger.LogDebug("<<< Exiting GetUserId(searchResult={SearchResult})", searchResult);
            return match.Success ? Convert.ToInt32(match.Groups[1].Value, CultureInfo.InvariantCulture) : Null.NullInteger;
        }

        private static bool HasSocialRelationship(UserInfo targetUser, UserInfo accessingUser, string extendedVisibility)
        {
			Logger.LogDebug(">>> Entering HasSocialRelationship(targetUser={TargetUser},accessingUser={AccessingUser},extendedVisibility={ExtendedVisibility})", targetUser, accessingUser, extendedVisibility);
            if (string.IsNullOrEmpty(extendedVisibility))
            {
                return false;
            }

            var profileVisibility = new ProfileVisibility(PortalSettings.PortalId, extendedVisibility);
			Logger.LogDebug("HasSocialRelationship(targetUser={TargetUser},accessingUser={AccessingUser},extendedVisibility={ExtendedVisibility}): profileVisibility → {ProfileVisibility}", targetUser, accessingUser, extendedVisibility, profileVisibility);

            var isVisible = accessingUser.UserID == targetUser.UserID;
			Logger.LogDebug("HasSocialRelationship(targetUser={TargetUser},accessingUser={AccessingUser},extendedVisibility={ExtendedVisibility}): isVisible → {IsVisible}", targetUser, accessingUser, extendedVisibility, isVisible);
            if (!isVisible)
            {
                // Relationships
                foreach (var relationship in profileVisibility.RelationshipVisibilities)
                {
                    switch (relationship.RelationshipTypeId)
                    {
                        case (int)DefaultRelationshipTypes.Followers:
                            isVisible = targetUser.Social.Following != null && targetUser.Social.Following.Status == RelationshipStatus.Accepted;
                            break;
                        case (int)DefaultRelationshipTypes.Friends:
                            isVisible = targetUser.Social.Friend != null && targetUser.Social.Friend.Status == RelationshipStatus.Accepted;
                            break;
                        default:
                            isVisible = targetUser.Social.UserRelationships.Any(userRelationship =>
                                                                          (userRelationship.RelationshipId == relationship.RelationshipId
                                                                              && accessingUser.UserID == userRelationship.RelatedUserId
                                                                              && userRelationship.Status == RelationshipStatus.Accepted));
                            break;
                    }

                    if (isVisible)
                    {
                        break;
                    }
                }

                // Groups/Roles
                if (profileVisibility.RoleVisibilities.Any(role => accessingUser.IsInRole(role.RoleName)))
                {
                    isVisible = true;
                }
            }

			Logger.LogDebug("<<< Exiting HasSocialRelationship(targetUser={TargetUser},accessingUser={AccessingUser},extendedVisibility={ExtendedVisibility})", targetUser, accessingUser, extendedVisibility);
            return isVisible;
        }
    }
}
