// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    internal class SubscriptionTypeBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int subscriptionTypeId;
        private string subscriptionName;
        private string friendlyName;
        private int desktopModuleId;

        internal SubscriptionTypeBuilder()
        {
			Logger.LogDebug(">>> Entering SubscriptionTypeBuilder()");
            this.subscriptionTypeId = 1;
            this.subscriptionName = "MySubscriptionType";
			Logger.LogDebug("SubscriptionTypeBuilder(): this.subscriptionTypeId → {Arg0}", this.subscriptionTypeId);
			Logger.LogDebug("SubscriptionTypeBuilder(): this.subscriptionName → {Arg0}", this.subscriptionName);
            this.friendlyName = "My Subscription Type";
            this.desktopModuleId = Null.NullInteger;
			Logger.LogDebug("<<< Exiting SubscriptionTypeBuilder()");
			Logger.LogDebug("SubscriptionTypeBuilder(): this.friendlyName → {Arg0}", this.friendlyName);
			Logger.LogDebug("SubscriptionTypeBuilder(): desktopModuleId → {DesktopModuleId}", desktopModuleId);
			Logger.LogDebug("SubscriptionTypeBuilder(): subscriptionTypeId → {SubscriptionTypeId}", subscriptionTypeId);
			Logger.LogDebug("SubscriptionTypeBuilder(): this.desktopModuleId → {Arg0}", this.desktopModuleId);
        }

        internal SubscriptionTypeBuilder WithSubscriptionTypeId(int subscriptionTypeId)
        {
			Logger.LogDebug(">>> Entering WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            this.subscriptionTypeId = subscriptionTypeId;
			Logger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): this.subscriptionTypeId → {Arg1}", subscriptionTypeId, this.subscriptionTypeId);
			Logger.LogDebug("WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId}): subscriptionTypeId → {SubscriptionTypeId1}", subscriptionTypeId, subscriptionTypeId);
			Logger.LogDebug("<<< Exiting WithSubscriptionTypeId(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            return this;
        }

        internal SubscriptionTypeBuilder WithSubscriptionName(string subscriptionName)
        {
            this.subscriptionName = subscriptionName;
			Logger.LogDebug("WithSubscriptionName(subscriptionName={SubscriptionName}): this.subscriptionName → {Arg1}", subscriptionName, this.subscriptionName);
			Logger.LogDebug("<<< Exiting WithSubscriptionName(subscriptionName={SubscriptionName})", subscriptionName);
            return this;
        }

        internal SubscriptionType Build()
        {
			Logger.LogDebug(">>> Entering Build()");
			Logger.LogDebug("<<< Exiting Build()");
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
