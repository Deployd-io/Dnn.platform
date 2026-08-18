// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Tests.Utilities;

    internal class UserPreferenceBuilder
    {
        private int userId;
        private int portalId;
        private Frequency messagesEmailFrequency;
        private Frequency notificationsEmailFrequency;

        internal UserPreferenceBuilder()
        {
			mockEventLogger.LogDebug(">>> Entering UserPreferenceBuilder()");
            this.userId = Constants.USER_InValidId;
            this.portalId = Constants.PORTAL_ValidPortalId;
			mockEventLogger.LogDebug("UserPreferenceBuilder(): this.userId → {Arg0}", this.userId);
			mockEventLogger.LogDebug("UserPreferenceBuilder(): portalId → {PortalId}", portalId);
			mockEventLogger.LogDebug("UserPreferenceBuilder(): this.portalId → {Arg0}", this.portalId);
            this.messagesEmailFrequency = Frequency.Instant;
            this.notificationsEmailFrequency = Frequency.Hourly;
			mockEventLogger.LogDebug("UserPreferenceBuilder(): messagesEmailFrequency → {MessagesEmailFrequency}", messagesEmailFrequency);
			mockEventLogger.LogDebug("<<< Exiting UserPreferenceBuilder()");
        }

        internal UserPreference Build()
        {
			mockEventLogger.LogDebug(">>> Entering Build()");
			mockEventLogger.LogDebug("<<< Exiting Build()");
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
			mockEventLogger.LogDebug(">>> Entering WithUserId(userId={UserId})", userId);
            this.userId = userId;
			mockEventLogger.LogDebug("WithUserId(userId={UserId}): this.userId → {Arg1}", userId, this.userId);
			mockEventLogger.LogDebug("WithUserId(userId={UserId}): userId → {UserId1}", userId, userId);
			mockEventLogger.LogDebug("<<< Exiting WithUserId(userId={UserId})", userId);
            return this;
        }

        internal UserPreferenceBuilder WithPortalId(int portalId)
        {
			mockEventLogger.LogDebug(">>> Entering WithPortalId(portalId={PortalId})", portalId);
            this.portalId = portalId;
			mockEventLogger.LogDebug("WithPortalId(portalId={PortalId}): this.portalId → {Arg1}", portalId, this.portalId);
			mockEventLogger.LogDebug("WithPortalId(portalId={PortalId}): portalId → {PortalId1}", portalId, portalId);
			mockEventLogger.LogDebug("<<< Exiting WithPortalId(portalId={PortalId})", portalId);
            return this;
        }
    }
}
