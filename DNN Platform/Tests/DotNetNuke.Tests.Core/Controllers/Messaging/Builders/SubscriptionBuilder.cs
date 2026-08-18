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
			mockEventLogger.LogDebug(">>> Entering SubscriptionBuilder()");
            this.subscriptionId = 1;
            this.userId = Constants.USER_InValidId;
			mockEventLogger.LogDebug("SubscriptionBuilder(): this.subscriptionId → {Arg0}", this.subscriptionId);
			mockEventLogger.LogDebug("SubscriptionBuilder(): userId → {UserId}", userId);
			mockEventLogger.LogDebug("SubscriptionBuilder(): this.userId → {Arg0}", this.userId);
            this.subscriptionTypeId = 1;
            this.portalId = Constants.PORTAL_ValidPortalId;
            this.moduleId = Null.NullInteger;
			mockEventLogger.LogDebug("SubscriptionBuilder(): subscriptionTypeId → {SubscriptionTypeId}", subscriptionTypeId);
			mockEventLogger.LogDebug("SubscriptionBuilder(): this.subscriptionTypeId → {Arg0}", this.subscriptionTypeId);
			mockEventLogger.LogDebug("SubscriptionBuilder(): portalId → {PortalId}", portalId);
			mockEventLogger.LogDebug("SubscriptionBuilder(): this.portalId → {Arg0}", this.portalId);
			mockEventLogger.LogDebug("SubscriptionBuilder(): this.moduleId → {Arg0}", this.moduleId);
			mockEventLogger.LogDebug("SubscriptionBuilder(): moduleId → {ModuleId}", moduleId);
            this.tabId = Null.NullInteger;
            this.objectKey = "content";
            this.description = "my content description";
            this.objectData = string.Empty;
			mockEventLogger.LogDebug("<<< Exiting SubscriptionBuilder()");
        }

        internal SubscriptionBuilder WithSubscriptionId(int subscriptionId)
        {
			mockEventLogger.LogDebug(">>> Entering WithSubscriptionId(subscriptionId={SubscriptionId})", subscriptionId);
            this.subscriptionId = subscriptionId;
			mockEventLogger.LogDebug("<<< Exiting WithSubscriptionId(subscriptionId={SubscriptionId})", subscriptionId);
            return this;
        }

        internal SubscriptionBuilder WithUserId(int userId)
        {
			mockEventLogger.LogDebug(">>> Entering WithUserId(userId={UserId})", userId);
            this.userId = userId;
			mockEventLogger.LogDebug("WithUserId(userId={UserId}): this.userId → {Arg1}", userId, this.userId);
			mockEventLogger.LogDebug("WithUserId(userId={UserId}): userId → {UserId1}", userId, userId);
			mockEventLogger.LogDebug("<<< Exiting WithUserId(userId={UserId})", userId);
            return this;
        }

        internal SubscriptionBuilder WithPortalId(int portalId)
        {
			mockEventLogger.LogDebug(">>> Entering WithPortalId(portalId={PortalId})", portalId);
            this.portalId = portalId;
			mockEventLogger.LogDebug("WithPortalId(portalId={PortalId}): this.portalId → {Arg1}", portalId, this.portalId);
			mockEventLogger.LogDebug("WithPortalId(portalId={PortalId}): portalId → {PortalId1}", portalId, portalId);
			mockEventLogger.LogDebug("<<< Exiting WithPortalId(portalId={PortalId})", portalId);
            return this;
        }

        internal SubscriptionBuilder WithSubscriptionTypeId(int subscriptionTypeId)
        {
			mockEventLogger.LogDebug(">>> Entering WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            this.subscriptionTypeId = subscriptionTypeId;
			mockEventLogger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): this.subscriptionTypeId → {Arg1}", subscriptionTypeId, this.subscriptionTypeId);
			mockEventLogger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): subscriptionTypeId → {SubscriptionTypeId1}", subscriptionTypeId, subscriptionTypeId);
			mockEventLogger.LogDebug("<<< Exiting WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            return this;
        }

        internal SubscriptionBuilder WithObjectKey(string objectKey)
        {
			mockEventLogger.LogDebug(">>> Entering WithObjectKey(objectKey={ObjectKey})", objectKey);
            this.objectKey = objectKey;
			mockEventLogger.LogDebug("WithObjectKey(objectKey={ObjectKey}): this.objectKey → {Arg1}", objectKey, this.objectKey);
			mockEventLogger.LogDebug("<<< Exiting WithObjectKey(objectKey={ObjectKey})", objectKey);
            return this;
        }

        internal SubscriptionBuilder WithDescription(string description)
        {
			mockEventLogger.LogDebug(">>> Entering WithDescription(description={Description})", description);
            this.description = description;
			mockEventLogger.LogDebug("WithDescription(description={Description}): this.description → {Arg1}", description, this.description);
			mockEventLogger.LogDebug("<<< Exiting WithDescription(description={Description})", description);
            return this;
        }

        internal SubscriptionBuilder WithModuleId(int moduleId)
        {
			mockEventLogger.LogDebug(">>> Entering WithModuleId(moduleId={ModuleId})", moduleId);
            this.moduleId = moduleId;
			mockEventLogger.LogDebug("WithModuleId(moduleId={ModuleId}): moduleId → {ModuleId1}", moduleId, moduleId);
			mockEventLogger.LogDebug("WithModuleId(moduleId={ModuleId}): this.moduleId → {Arg1}", moduleId, this.moduleId);
			mockEventLogger.LogDebug("<<< Exiting WithModuleId(moduleId={ModuleId})", moduleId);
            return this;
        }

        internal SubscriptionBuilder WithTabId(int tabId)
        {
			mockEventLogger.LogDebug(">>> Entering WithTabId(tabId={TabId})", tabId);
            this.tabId = tabId;
			mockEventLogger.LogDebug("WithTabId(tabId={TabId}): this.tabId → {Arg1}", tabId, this.tabId);
			mockEventLogger.LogDebug("WithTabId(tabId={TabId}): tabId → {TabId1}", tabId, tabId);
			mockEventLogger.LogDebug("<<< Exiting WithTabId(tabId={TabId})", tabId);
            return this;
        }

        internal Subscription Build()
        {
			mockEventLogger.LogDebug(">>> Entering Build()");
			mockEventLogger.LogDebug("<<< Exiting Build()");
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
