// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Social.Subscriptions.Entities;
    using DotNetNuke.Tests.Utilities;

    public class SubscriptionBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int subscriptionId;
        private int userId;
        private int portalId;
        private int subscriptionTypeId;
        private string objectKey;
        private string description;
        private int moduleId;
        private int tabId;
        private string objectData;

        internal SubscriptionBuilder()
        {
			Logger.LogDebug(">>> Entering SubscriptionBuilder()");
            this.subscriptionId = 1;
            this.userId = Constants.USER_InValidId;
			Logger.LogDebug("SubscriptionBuilder(): this.subscriptionId → {Arg0}", this.subscriptionId);
			Logger.LogDebug("SubscriptionBuilder(): userId → {UserId}", userId);
			Logger.LogDebug("SubscriptionBuilder(): this.userId → {Arg0}", this.userId);
            this.subscriptionTypeId = 1;
            this.portalId = Constants.PORTAL_ValidPortalId;
            this.moduleId = Null.NullInteger;
			Logger.LogDebug("SubscriptionBuilder(): subscriptionTypeId → {SubscriptionTypeId}", subscriptionTypeId);
			Logger.LogDebug("SubscriptionBuilder(): this.subscriptionTypeId → {Arg0}", this.subscriptionTypeId);
			Logger.LogDebug("SubscriptionBuilder(): portalId → {PortalId}", portalId);
			Logger.LogDebug("SubscriptionBuilder(): this.portalId → {Arg0}", this.portalId);
			Logger.LogDebug("SubscriptionBuilder(): moduleId → {ModuleId}", moduleId);
			Logger.LogDebug("SubscriptionBuilder(): this.moduleId → {Arg0}", this.moduleId);
            this.tabId = Null.NullInteger;
            this.objectKey = "content";
            this.description = "my content description";
            this.objectData = string.Empty;
			Logger.LogDebug("<<< Exiting SubscriptionBuilder()");
			Logger.LogDebug("SubscriptionBuilder(): tabId → {TabId}", tabId);
			Logger.LogDebug("SubscriptionBuilder(): this.tabId → {Arg0}", this.tabId);
			Logger.LogDebug("SubscriptionBuilder(): this.objectKey → {Arg0}", this.objectKey);
			Logger.LogDebug("SubscriptionBuilder(): this.description → {Arg0}", this.description);
			Logger.LogDebug("SubscriptionBuilder(): this.objectData → {Arg0}", this.objectData);
			Logger.LogDebug("SubscriptionBuilder(): subscriptionId → {SubscriptionId}", subscriptionId);
        }

        internal SubscriptionBuilder WithSubscriptionId(int subscriptionId)
        {
            this.subscriptionId = subscriptionId;
			Logger.LogDebug("<<< Exiting WithSubscriptionId(subscriptionId={SubscriptionId})", subscriptionId);
            return this;
        }

        internal SubscriptionBuilder WithUserId(int userId)
        {
			Logger.LogDebug(">>> Entering WithUserId(userId={UserId})", userId);
            this.userId = userId;
			Logger.LogDebug("WithUserId(userId={UserId}): this.userId → {Arg1}", userId, this.userId);
			Logger.LogDebug("WithUserId(userId={UserId}): userId → {UserId1}", userId, userId);
			Logger.LogDebug("<<< Exiting WithUserId(userId={UserId})", userId);
            return this;
        }

        internal SubscriptionBuilder WithPortalId(int portalId)
        {
            this.portalId = portalId;
			Logger.LogDebug("WithPortalId(portalId={PortalId}): this.portalId → {Arg1}", portalId, this.portalId);
			Logger.LogDebug("WithPortalId(portalId={PortalId}): portalId → {PortalId1}", portalId, portalId);
			Logger.LogDebug("<<< Exiting WithPortalId(portalId={PortalId})", portalId);
            return this;
        }

        internal SubscriptionBuilder WithSubscriptionTypeId(int subscriptionTypeId)
        {
			Logger.LogDebug(">>> Entering WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            this.subscriptionTypeId = subscriptionTypeId;
			Logger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): this.subscriptionTypeId → {Arg1}", subscriptionTypeId, this.subscriptionTypeId);
			Logger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): subscriptionTypeId → {SubscriptionTypeId1}", subscriptionTypeId, subscriptionTypeId);
			Logger.LogDebug("<<< Exiting WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            return this;
        }

        internal SubscriptionBuilder WithObjectKey(string objectKey)
        {
			Logger.LogDebug(">>> Entering WithObjectKey(objectKey={ObjectKey})", objectKey);
            this.objectKey = objectKey;
			Logger.LogDebug("WithObjectKey(objectKey={ObjectKey}): this.objectKey → {Arg1}", objectKey, this.objectKey);
			Logger.LogDebug("<<< Exiting WithObjectKey(objectKey={ObjectKey})", objectKey);
            return this;
        }

        internal SubscriptionBuilder WithDescription(string description)
        {
			Logger.LogDebug(">>> Entering WithDescription(description={Description})", description);
            this.description = description;
			Logger.LogDebug("WithDescription(description={Description}): this.description → {Arg1}", description, this.description);
			Logger.LogDebug("<<< Exiting WithDescription(description={Description})", description);
            return this;
        }

        internal SubscriptionBuilder WithModuleId(int moduleId)
        {
			Logger.LogDebug(">>> Entering WithModuleId(moduleId={ModuleId})", moduleId);
            this.moduleId = moduleId;
			Logger.LogDebug("WithModuleId(moduleId={ModuleId}): this.moduleId → {Arg1}", moduleId, this.moduleId);
			Logger.LogDebug("WithModuleId(moduleId={ModuleId}): moduleId → {ModuleId1}", moduleId, moduleId);
			Logger.LogDebug("<<< Exiting WithModuleId(moduleId={ModuleId})", moduleId);
            return this;
        }

        internal SubscriptionBuilder WithTabId(int tabId)
        {
			Logger.LogDebug(">>> Entering WithTabId(tabId={TabId})", tabId);
            this.tabId = tabId;
			Logger.LogDebug("WithTabId(tabId={TabId}): this.tabId → {Arg1}", tabId, this.tabId);
			Logger.LogDebug("WithTabId(tabId={TabId}): tabId → {TabId1}", tabId, tabId);
			Logger.LogDebug("<<< Exiting WithTabId(tabId={TabId})", tabId);
            return this;
        }

        internal Subscription Build()
        {
			Logger.LogDebug(">>> Entering Build()");
			Logger.LogDebug("<<< Exiting Build()");
            return new Subscription
            {
                SubscriptionTypeId = this.subscriptionTypeId,
                SubscriptionId = this.subscriptionId,
                CreatedOnDate = DateTime.UtcNow,
                ModuleId = this.moduleId,
                ObjectKey = this.objectKey,
                Description = this.description,
                PortalId = this.portalId,
                TabId = this.tabId,
                UserId = this.userId,
                ObjectData = this.objectData,
            };
        }
    }
}
