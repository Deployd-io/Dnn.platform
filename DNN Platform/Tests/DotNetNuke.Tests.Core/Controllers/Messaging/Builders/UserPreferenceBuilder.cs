// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Tests.Utilities;

    internal class UserPreferenceBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int userId;
        private int portalId;
        private Frequency messagesEmailFrequency;
        private Frequency notificationsEmailFrequency;

        internal UserPreferenceBuilder()
        {
			Logger.LogDebug(">>> Entering UserPreferenceBuilder()");
            this.userId = Constants.USER_InValidId;
            this.portalId = Constants.PORTAL_ValidPortalId;
			Logger.LogDebug("UserPreferenceBuilder(): this.userId → {Arg0}", this.userId);
			Logger.LogDebug("UserPreferenceBuilder(): portalId → {PortalId}", portalId);
			Logger.LogDebug("UserPreferenceBuilder(): this.portalId → {Arg0}", this.portalId);
            this.messagesEmailFrequency = Frequency.Instant;
            this.notificationsEmailFrequency = Frequency.Hourly;
			Logger.LogDebug("<<< Exiting UserPreferenceBuilder()");
			Logger.LogDebug("UserPreferenceBuilder(): messagesEmailFrequency → {MessagesEmailFrequency}", messagesEmailFrequency);
			Logger.LogDebug("UserPreferenceBuilder(): this.messagesEmailFrequency → {Arg0}", this.messagesEmailFrequency);
			Logger.LogDebug("UserPreferenceBuilder(): notificationsEmailFrequency → {NotificationsEmailFrequency}", notificationsEmailFrequency);
			Logger.LogDebug("UserPreferenceBuilder(): this.notificationsEmailFrequency → {Arg0}", this.notificationsEmailFrequency);
			Logger.LogDebug("UserPreferenceBuilder(): userId → {UserId}", userId);
        }

        internal UserPreference Build()
        {
			Logger.LogDebug(">>> Entering Build()");
			Logger.LogDebug("<<< Exiting Build()");
            return new UserPreference
            {
                UserId = this.userId,
                PortalId = this.portalId,
                MessagesEmailFrequency = this.messagesEmailFrequency,
                NotificationsEmailFrequency = this.notificationsEmailFrequency,
            };
        }

        internal UserPreferenceBuilder WithUserId(int userId)
        {
			Logger.LogDebug(">>> Entering WithUserId(userId={UserId})", userId);
            this.userId = userId;
			Logger.LogDebug("WithUserId(userId={UserId}): this.userId → {Arg1}", userId, this.userId);
			Logger.LogDebug("WithUserId(userId={UserId}): userId → {UserId1}", userId, userId);
			Logger.LogDebug("<<< Exiting WithUserId(userId={UserId})", userId);
            return this;
        }

        internal UserPreferenceBuilder WithPortalId(int portalId)
        {
			Logger.LogDebug(">>> Entering WithPortalId(portalId={PortalId})", portalId);
            this.portalId = portalId;
			Logger.LogDebug("WithPortalId(portalId={PortalId}): this.portalId → {Arg1}", portalId, this.portalId);
			Logger.LogDebug("WithPortalId(portalId={PortalId}): portalId → {PortalId1}", portalId, portalId);
			Logger.LogDebug("<<< Exiting WithPortalId(portalId={PortalId})", portalId);
            return this;
        }
    }
}
