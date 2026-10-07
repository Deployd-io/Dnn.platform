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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SubscriptionSecurityController));
        /// <inheritdoc />
        public bool HasPermission(Subscription subscription)
        {
			Logger.LogError(">>> Entering HasPermission(subscription={Subscription})", subscription);
            var userInfo = GetUserFromSubscription(subscription);

			Logger.LogError("HasPermission(subscription={Subscription}): userInfo → {UserInfo}", subscription, userInfo);
            var moduleInfo = GetModuleFromSubscription(subscription);
			Logger.LogError("HasPermission(subscription={Subscription}): moduleInfo → {ModuleInfo}", subscription, moduleInfo);
            if (moduleInfo != null && !moduleInfo.InheritViewPermissions)
            {
                return HasUserModuleViewPermission(userInfo, moduleInfo);
            }

            var tabInfo = GetTabFromSubscription(subscription);
			Logger.LogError("HasPermission(subscription={Subscription}): tabInfo → {TabInfo}", subscription, tabInfo);
            if (tabInfo != null)
            {
                return HasUserTabViewPermission(userInfo, tabInfo);
            }

			Logger.LogWarning("<<< Exiting HasPermission(subscription={Subscription})", subscription);
            return true;
        }

        /// <inheritdoc />
        protected override Func<ISubscriptionSecurityController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new SubscriptionSecurityController();
        }

        private static bool HasUserModuleViewPermission(UserInfo userInfo, ModuleInfo moduleInfo)
        {
			Logger.LogError(">>> Entering HasUserModuleViewPermission(userInfo={UserInfo},moduleInfo={ModuleInfo})", userInfo, moduleInfo);
            var portalSettings = new PortalSettings(moduleInfo.PortalID);
			Logger.LogError("HasUserModuleViewPermission(userInfo={UserInfo},moduleInfo={ModuleInfo}): portalSettings → {PortalSettings}", userInfo, moduleInfo, portalSettings);
			Logger.LogWarning("<<< Exiting HasUserModuleViewPermission(userInfo={UserInfo},moduleInfo={ModuleInfo})", userInfo, moduleInfo);
            return PortalSecurity.IsInRoles(userInfo, portalSettings, moduleInfo.ModulePermissions.ToString("VIEW"));
        }

        private static bool HasUserTabViewPermission(UserInfo userInfo, TabInfo tabInfo)
        {
			Logger.LogError(">>> Entering HasUserTabViewPermission(userInfo={UserInfo},tabInfo={TabInfo})", userInfo, tabInfo);
            var portalSettings = new PortalSettings(tabInfo.PortalID);
			Logger.LogError("HasUserTabViewPermission(userInfo={UserInfo},tabInfo={TabInfo}): portalSettings → {PortalSettings}", userInfo, tabInfo, portalSettings);
			Logger.LogWarning("<<< Exiting HasUserTabViewPermission(userInfo={UserInfo},tabInfo={TabInfo})", userInfo, tabInfo);
            return PortalSecurity.IsInRoles(userInfo, portalSettings, tabInfo.TabPermissions.ToString("VIEW"));
        }

        private static TabInfo GetTabFromSubscription(Subscription subscription)
        {
			Logger.LogError(">>> Entering GetTabFromSubscription(subscription={Subscription})", subscription);
            Logger.LogWarning("<<< Exiting GetTabFromSubscription(subscription={Subscription})", subscription);
            return TabController.Instance.GetTab(subscription.TabId, subscription.PortalId, false);
        }

        private static ModuleInfo GetModuleFromSubscription(Subscription subscription)
        {
			Logger.LogError(">>> Entering GetModuleFromSubscription(subscription={Subscription})", subscription);
            Logger.LogWarning("<<< Exiting GetModuleFromSubscription(subscription={Subscription})", subscription);
            return ModuleController.Instance.GetModule(subscription.ModuleId, Null.NullInteger, true);
        }

        private static UserInfo GetUserFromSubscription(Subscription subscription)
        {
			Logger.LogError(">>> Entering GetUserFromSubscription(subscription={Subscription})", subscription);
            Logger.LogWarning("<<< Exiting GetUserFromSubscription(subscription={Subscription})", subscription);
            return UserController.Instance.GetUser(subscription.PortalId, subscription.UserId);
        }
    }
}
