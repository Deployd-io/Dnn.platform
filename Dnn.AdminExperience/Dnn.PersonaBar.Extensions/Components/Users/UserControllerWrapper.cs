// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    public class UserControllerWrapper : IUserControllerWrapper
    {
        public UserInfo GetUserById(int portalId, int userId)
        {
			eventLogger.LogDebug(">>> Entering GetUserById(portalId={PortalId},userId={UserId})", portalId, userId);
			eventLogger.LogDebug("<<< Exiting GetUserById(portalId={PortalId},userId={UserId})", portalId, userId);
            return UserController.GetUserById(portalId, userId);
        }

        public UserInfo GetUser(int userId, PortalSettings portalSettings, UserInfo currentUserInfo, out KeyValuePair<HttpStatusCode, string> response)
        {
			eventLogger.LogDebug(">>> Entering GetUser(userId={UserId},portalSettings={PortalSettings},currentUserInfo={CurrentUserInfo})", userId, portalSettings, currentUserInfo);
			eventLogger.LogDebug("<<< Exiting GetUser(userId={UserId},portalSettings={PortalSettings},currentUserInfo={CurrentUserInfo})", userId, portalSettings, currentUserInfo);
            return UsersController.GetUser(userId, portalSettings, currentUserInfo, out response);
        }

        public int? GetUsersByEmail(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly)
        {
			eventLogger.LogDebug(">>> Entering GetUsersByEmail(portalId={PortalId},searchTerm={SearchTerm},pageIndex={PageIndex},pageSize={PageSize},recCount={RecCount},includeDeleted={IncludeDeleted},isSuperUserOnly={IsSuperUserOnly})", portalId, searchTerm, pageIndex, pageSize, recCount, includeDeleted, isSuperUserOnly);
			eventLogger.LogDebug("<<< Exiting GetUsersByEmail(portalId={PortalId},searchTerm={SearchTerm},pageIndex={PageIndex},pageSize={PageSize},recCount={RecCount},includeDeleted={IncludeDeleted},isSuperUserOnly={IsSuperUserOnly})", portalId, searchTerm, pageIndex, pageSize, recCount, includeDeleted, isSuperUserOnly);
            return (UserController.GetUsersByEmail(portalId, searchTerm, pageIndex, pageSize, ref recCount, includeDeleted, isSuperUserOnly).ToArray().FirstOrDefault() as UserInfo)?.UserID;
        }

        public int? GetUsersByUserName(int portalId, string searchTerm, int pageIndex, int pageSize, ref int recCount, bool includeDeleted, bool isSuperUserOnly)
        {
			eventLogger.LogDebug(">>> Entering GetUsersByUserName(portalId={PortalId},searchTerm={SearchTerm},pageIndex={PageIndex},pageSize={PageSize},recCount={RecCount},includeDeleted={IncludeDeleted},isSuperUserOnly={IsSuperUserOnly})", portalId, searchTerm, pageIndex, pageSize, recCount, includeDeleted, isSuperUserOnly);
			eventLogger.LogDebug("<<< Exiting GetUsersByUserName(portalId={PortalId},searchTerm={SearchTerm},pageIndex={PageIndex},pageSize={PageSize},recCount={RecCount},includeDeleted={IncludeDeleted},isSuperUserOnly={IsSuperUserOnly})", portalId, searchTerm, pageIndex, pageSize, recCount, includeDeleted, isSuperUserOnly);
            return (UserController.GetUsersByUserName(portalId, searchTerm, pageIndex, pageSize, ref recCount, includeDeleted, isSuperUserOnly).ToArray().FirstOrDefault() as UserInfo)?.UserID;
        }

        public bool DeleteUserAndClearCache(ref UserInfo userInfo, bool notify, bool deleteAdmin)
        {
			eventLogger.LogDebug(">>> Entering DeleteUserAndClearCache(userInfo={UserInfo},notify={Notify},deleteAdmin={DeleteAdmin})", userInfo, notify, deleteAdmin);
            bool isDeleted = UserController.DeleteUser(ref userInfo, notify, deleteAdmin);

            if (isDeleted)
            {
                // We must clear User cache or else, when the user is 'removed' (so it can't be restored), you
                // will not be able to create a new user with the same username -- even though no user with that username
                // exists.
                DataCache.ClearUserCache(userInfo.PortalID, userInfo.Username);
            }

			eventLogger.LogDebug("<<< Exiting DeleteUserAndClearCache(userInfo={UserInfo},notify={Notify},deleteAdmin={DeleteAdmin})", userInfo, notify, deleteAdmin);
            return isDeleted;
        }
    }
}
