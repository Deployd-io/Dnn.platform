// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    internal class SubscriptionTypeBuilder
    {
        private int subscriptionTypeId;
        private string subscriptionName;
        private string friendlyName;
        private int desktopModuleId;

        internal SubscriptionTypeBuilder()
        {
			mockEventLogger.LogDebug(">>> Entering SubscriptionTypeBuilder()");
            this.subscriptionTypeId = 1;
            this.subscriptionName = "MySubscriptionType";
			mockEventLogger.LogDebug("SubscriptionTypeBuilder(): this.subscriptionTypeId → {Arg0}", this.subscriptionTypeId);
			mockEventLogger.LogDebug("SubscriptionTypeBuilder(): this.subscriptionName → {Arg0}", this.subscriptionName);
            this.friendlyName = "My Subscription Type";
            this.desktopModuleId = Null.NullInteger;
			mockEventLogger.LogDebug("<<< Exiting SubscriptionTypeBuilder()");
        }

        internal SubscriptionTypeBuilder WithSubscriptionTypeId(int subscriptionTypeId)
        {
			mockEventLogger.LogDebug(">>> Entering WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            this.subscriptionTypeId = subscriptionTypeId;
			mockEventLogger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): subscriptionTypeId → {SubscriptionTypeId1}", subscriptionTypeId, subscriptionTypeId);
			mockEventLogger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): this.subscriptionTypeId → {Arg1}", subscriptionTypeId, this.subscriptionTypeId);
			mockEventLogger.LogDebug("<<< Exiting WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            return this;
        }

        internal SubscriptionTypeBuilder WithSubscriptionName(string subscriptionName)
        {
			mockEventLogger.LogDebug(">>> Entering WithSubscriptionName(subscriptionName={SubscriptionName})", subscriptionName);
            this.subscriptionName = subscriptionName;
			mockEventLogger.LogDebug("WithSubscriptionName(subscriptionName={SubscriptionName}): this.subscriptionName → {Arg1}", subscriptionName, this.subscriptionName);
			mockEventLogger.LogDebug("<<< Exiting WithSubscriptionName(subscriptionName={SubscriptionName})", subscriptionName);
            return this;
        }

        internal SubscriptionType Build()
        {
			mockEventLogger.LogDebug(">>> Entering Build()");
			mockEventLogger.LogDebug("<<< Exiting Build()");
            return new SubscriptionType
            {
                SubscriptionTypeId = this.subscriptionTypeId,
                SubscriptionName = this.subscriptionName,
                DesktopModuleId = this.desktopModuleId,
                FriendlyName = this.friendlyName,
            };
        }
    }
}
