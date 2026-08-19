// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;

    using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

    /// <summary>The UserOnlineController class provides Business Layer methods for Users Online.</summary>
    [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
    public partial class UserOnlineController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserOnlineController));
        private static readonly MembershipProvider MemberProvider = MembershipProvider.Instance();
        private static readonly object Locker = new object();
        private static readonly string CacheKey = "OnlineUserList";

        /// <summary>Clears the cached Users Online Information.</summary>
        public void ClearUserList()
        {
			Logger.LogDebug(">>> Entering ClearUserList()");
            string key = "OnlineUserList";
            DataCache.RemoveCache(key);
			Logger.LogDebug("<<< Exiting ClearUserList()");
        }

        /// <summary>Gets the Online time window.</summary>
        /// <returns>The window to use in minutes when determining if the user is online.</returns>
        public int GetOnlineTimeWindow()
        {
			Logger.LogDebug(">>> Entering GetOnlineTimeWindow()");
			Logger.LogDebug("<<< Exiting GetOnlineTimeWindow()");
            return Host.Host.UsersOnlineTimeWindow;
        }

        /// <summary>Gets the cached Users Online Information.</summary>
        /// <returns>A <see cref="Hashtable"/> with <see cref="string"/> keys for the user ID (a GUID for anonymous users) and <see cref="BaseUserInfo"/> instances for the values.</returns>
        public Hashtable GetUserList()
        {
			Logger.LogDebug(">>> Entering GetUserList()");
            var userList = (Hashtable)DataCache.GetCache(CacheKey);
            if (userList == null)
            {
                lock (Locker)
                {
                    userList = (Hashtable)DataCache.GetCache(CacheKey);
                    if (userList == null)
                    {
                        userList = Hashtable.Synchronized(new Hashtable());
                        DataCache.SetCache(CacheKey, userList);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetUserList()");
            return userList;
        }

        /// <summary>Gets whether the Users Online functionality is enabled.</summary>
        /// <returns><see langword="true"/> if the Users Online functionality is enabled, otherwise <see langword="false"/>.</returns>
        public bool IsEnabled()
        {
			Logger.LogDebug(">>> Entering IsEnabled()");
			Logger.LogDebug("<<< Exiting IsEnabled()");
            return Host.Host.EnableUsersOnline;
        }

        /// <summary>Determines whether a User is online.</summary>
        /// <param name="user">The user.</param>
        /// <returns><see langword="true"/> if the user is online, otherwise <see langword="false"/>.</returns>
        public bool IsUserOnline(UserInfo user)
        {
			Logger.LogDebug(">>> Entering IsUserOnline(user={User})", user);
            bool isOnline = false;
            if (this.IsEnabled())
            {
                isOnline = MemberProvider.IsUserOnline(user);
            }

			Logger.LogDebug("<<< Exiting IsUserOnline(user={User})", user);
            return isOnline;
        }

        /// <summary>Sets the cached Users Online Information.</summary>
        /// <param name="userList">A <see cref="Hashtable"/> with <see cref="string"/> keys for the user ID (a GUID for anonymous users) and <see cref="BaseUserInfo"/> instances for the values.</param>
        public void SetUserList(Hashtable userList)
        {
			Logger.LogDebug(">>> Entering SetUserList(userList={UserList})", userList);
            DataCache.SetCache(CacheKey, userList);
			Logger.LogDebug("<<< Exiting SetUserList(userList={UserList})", userList);
        }

        /// <summary>Tracks an online User.</summary>
        public void TrackUsers()
        {
			Logger.LogDebug(">>> Entering TrackUsers()");
            HttpContext context = HttpContext.Current;

            // Have we already done the work for this request?
            if (context.Items["CheckedUsersOnlineCookie"] != null)
            {
                return;
            }
            else
            {
                context.Items["CheckedUsersOnlineCookie"] = "true";
            }

            if (context.Request.IsAuthenticated)
            {
                this.TrackAuthenticatedUser(context);
            }
            else if (context.Request.Browser.Cookies)
            {
                this.TrackAnonymousUser(context);
            }
			Logger.LogDebug("<<< Exiting TrackUsers()");
        }

        /// <summary>Update the Users Online information.</summary>
        public void UpdateUsersOnline()
        {
			Logger.LogDebug(">>> Entering UpdateUsersOnline()");
            // Get a Current User List
            Hashtable userList = this.GetUserList();
			Logger.LogDebug("UpdateUsersOnline(): userList → {UserList}", userList);

            // Create a shallow copy of the list to Process
            var listToProcess = (Hashtable)userList.Clone();
			Logger.LogDebug("UpdateUsersOnline(): listToProcess → {ListToProcess}", listToProcess);

            // Clear the list
            this.ClearUserList();

            // Persist the current User List
            try
            {
                MemberProvider.UpdateUsersOnline(listToProcess);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            // Remove users that have expired
            MemberProvider.DeleteUsersOnline(this.GetOnlineTimeWindow());
			Logger.LogDebug("<<< Exiting UpdateUsersOnline()");
        }

        /// <summary>Tracks an Anonymous User.</summary>
        /// <param name="context">An HttpContext Object.</param>
        private void TrackAnonymousUser(HttpContext context)
        {
			Logger.LogDebug(">>> Entering TrackAnonymousUser(context={Context})", context);
            string cookieName = "DotNetNukeAnonymous";
            var portalSettings = (PortalSettings)context.Items["PortalSettings"];
			Logger.LogDebug("TrackAnonymousUser(context={Context}): portalSettings → {PortalSettings}", context, portalSettings);
            if (portalSettings == null)
            {
                return;
            }

            AnonymousUserInfo user;
            Hashtable userList = this.GetUserList();
			Logger.LogDebug("TrackAnonymousUser(context={Context}): userList → {UserList}", context, userList);
            string userID;

            // Check if the Tracking cookie exists
            HttpCookie cookie = context.Request.Cookies[cookieName];

            // Track Anonymous User
            if (cookie == null)
            {
                // Create a temporary userId
                userID = Guid.NewGuid().ToString();

				Logger.LogDebug("TrackAnonymousUser(context={Context}): userID → {UserID}", context, userID);
                // Create a new cookie
                cookie = new HttpCookie(cookieName, userID)
                {
                    Expires = DateTime.Now.AddMinutes(20),
                    Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                };
                context.Response.Cookies.Add(cookie);

                // Create a user
                user = new AnonymousUserInfo
				Logger.LogDebug("TrackAnonymousUser(context={Context}): user → {User}", context, user);
                {
                    UserID = userID,
                    PortalID = portalSettings.PortalId,
                    TabID = portalSettings.ActiveTab.TabID,
                    CreationDate = DateTime.Now,
                    LastActiveDate = DateTime.Now,
                };

                // Add the user
                if (!userList.Contains(userID))
                {
                    userList[userID] = user;
                }
            }
            else
            {
                if (cookie.Value == null)
                {
                    // Expire the cookie, there is something wrong with it
                    context.Response.Cookies[cookieName].Expires = new DateTime(1999, 10, 12);

                    // No need to do anything else
                    return;
                }

                // Get userID out of cookie
                userID = cookie.Value;

                // Find the cookie in the user list
                if (userList[userID] == null)
                {
                    userList[userID] = new AnonymousUserInfo();
                    ((AnonymousUserInfo)userList[userID]).CreationDate = DateTime.Now;
                }

                user = (AnonymousUserInfo)userList[userID];
                user.UserID = userID;
                user.PortalID = portalSettings.PortalId;
				Logger.LogDebug("TrackAnonymousUser(context={Context}): user.UserID → {Arg1}", context, user.UserID);
				Logger.LogDebug("TrackAnonymousUser(context={Context}): user.PortalID → {Arg1}", context, user.PortalID);
				Logger.LogDebug("TrackAnonymousUser(context={Context}): user.TabID → {Arg1}", context, user.TabID);
				Logger.LogDebug("TrackAnonymousUser(context={Context}): user.LastActiveDate → {Arg1}", context, user.LastActiveDate);
                user.TabID = portalSettings.ActiveTab.TabID;
                user.LastActiveDate = DateTime.Now;

                // Reset the expiration on the cookie
                cookie = new HttpCookie(cookieName, userID)
                {
                    Expires = DateTime.Now.AddMinutes(20),
                    Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
                };
                context.Response.Cookies.Add(cookie);
            }
			Logger.LogDebug("<<< Exiting TrackAnonymousUser(context={Context})", context);
        }

        /// <summary>Tracks an Authenticated User.</summary>
        /// <param name="context">An HttpContext Object.</param>
        private void TrackAuthenticatedUser(HttpContext context)
        {
			Logger.LogDebug(">>> Entering TrackAuthenticatedUser(context={Context})", context);
            // Retrieve Portal Settings
            var portalSettings = (PortalSettings)context.Items["PortalSettings"];
			Logger.LogDebug("TrackAuthenticatedUser(context={Context}): portalSettings → {PortalSettings}", context, portalSettings);

            if (portalSettings == null)
            {
                return;
            }

            // Get the logged in User ID
            UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("TrackAuthenticatedUser(context={Context}): objUserInfo → {ObjUserInfo}", context, objUserInfo);

            // Get user list
            Hashtable userList = this.GetUserList();
			Logger.LogDebug("TrackAuthenticatedUser(context={Context}): userList → {UserList}", context, userList);

            var user = new OnlineUserInfo();
			Logger.LogDebug("TrackAuthenticatedUser(context={Context}): user → {User}", context, user);
            if (objUserInfo.UserID > 0)
            {
                user.UserID = objUserInfo.UserID;
				Logger.LogDebug("TrackAuthenticatedUser(context={Context}): user.UserID → {Arg1}", context, user.UserID);
            }

            user.PortalID = portalSettings.PortalId;
			Logger.LogDebug("TrackAuthenticatedUser(context={Context}): user.PortalID → {Arg1}", context, user.PortalID);
			Logger.LogDebug("TrackAuthenticatedUser(context={Context}): user.TabID → {Arg1}", context, user.TabID);
			Logger.LogDebug("TrackAuthenticatedUser(context={Context}): user.LastActiveDate → {Arg1}", context, user.LastActiveDate);
            user.TabID = portalSettings.ActiveTab.TabID;
            user.LastActiveDate = DateTime.Now;
            if (userList[objUserInfo.UserID.ToString(CultureInfo.InvariantCulture)] == null)
            {
                user.CreationDate = user.LastActiveDate;
				Logger.LogDebug("TrackAuthenticatedUser(context={Context}): user.CreationDate → {Arg1}", context, user.CreationDate);
            }

            userList[objUserInfo.UserID.ToString(CultureInfo.InvariantCulture)] = user;
            this.SetUserList(userList);
			Logger.LogDebug("<<< Exiting TrackAuthenticatedUser(context={Context})", context);
        }
    }
}
