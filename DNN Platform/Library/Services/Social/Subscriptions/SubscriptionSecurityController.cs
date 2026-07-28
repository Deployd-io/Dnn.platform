// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    /// <summary>This controller provides permission info about the User Subscription.</summary>
    public class SubscriptionSecurityController : ServiceLocator<ISubscriptionSecurityController, SubscriptionSecurityController>, ISubscriptionSecurityController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public bool HasPermission(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering HasPermission(subscription={Subscription})", subscription);
            var userInfo = GetUserFromSubscription(subscription);

			Logger.LogDebug("HasPermission(subscription={Subscription}): userInfo → {UserInfo}", subscription, userInfo);
            var moduleInfo = GetModuleFromSubscription(subscription);
            if (moduleInfo != null && !moduleInfo.InheritViewPermissions)
			Logger.LogDebug("HasPermission(subscription={Subscription}): moduleInfo → {ModuleInfo}", subscription, moduleInfo);
            {
                return HasUserModuleViewPermission(userInfo, moduleInfo);
            }

            var tabInfo = GetTabFromSubscription(subscription);
            if (tabInfo != null)
			Logger.LogDebug("HasPermission(subscription={Subscription}): tabInfo → {TabInfo}", subscription, tabInfo);
            {
                return HasUserTabViewPermission(userInfo, tabInfo);
            }

			Logger.LogDebug("<<< Exiting HasPermission(subscription={Subscription})", subscription);
            return true;
        }

        /// <inheritdoc />
        protected override Func<ISubscriptionSecurityController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new SubscriptionSecurityController();
        }

        private static bool HasUserModuleViewPermission(UserInfo userInfo, ModuleInfo moduleInfo)
        {
            var portalSettings = new PortalSettings(moduleInfo.PortalID);
            return PortalSecurity.IsInRoles(userInfo, portalSettings, moduleInfo.ModulePermissions.ToString("VIEW"));
        }

        private static bool HasUserTabViewPermission(UserInfo userInfo, TabInfo tabInfo)
        {
            var portalSettings = new PortalSettings(tabInfo.PortalID);
            return PortalSecurity.IsInRoles(userInfo, portalSettings, tabInfo.TabPermissions.ToString("VIEW"));
        }

        private static TabInfo GetTabFromSubscription(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering GetTabFromSubscription(subscription={Subscription})", subscription);
			Logger.LogDebug("<<< Exiting GetTabFromSubscription(subscription={Subscription})", subscription);
            return TabController.Instance.GetTab(subscription.TabId, subscription.PortalId, false);
        }

        private static ModuleInfo GetModuleFromSubscription(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering GetModuleFromSubscription(subscription={Subscription})", subscription);
			Logger.LogDebug("<<< Exiting GetModuleFromSubscription(subscription={Subscription})", subscription);
            return ModuleController.Instance.GetModule(subscription.ModuleId, Null.NullInteger, true);
        }

        private static UserInfo GetUserFromSubscription(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering GetUserFromSubscription(subscription={Subscription})", subscription);
			Logger.LogDebug("<<< Exiting GetUserFromSubscription(subscription={Subscription})", subscription);
            return UserController.Instance.GetUser(subscription.PortalId, subscription.UserId);
        }
    }
}
