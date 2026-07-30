// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Social.Subscriptions.Data;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    /// <summary>This controller is responsible to manage the user subscriptions.</summary>
    public class SubscriptionController : ServiceLocator<ISubscriptionController, SubscriptionController>, ISubscriptionController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IDataService dataService;
        private readonly ISubscriptionSecurityController subscriptionSecurityController;

        /// <summary>Initializes a new instance of the <see cref="SubscriptionController"/> class.</summary>
        public SubscriptionController()
        {
			Logger.LogDebug(">>> Entering SubscriptionController()");
            this.dataService = DataService.Instance;
            this.subscriptionSecurityController = SubscriptionSecurityController.Instance;
			Logger.LogDebug("<<< Exiting SubscriptionController()");
			Logger.LogDebug("SubscriptionController(): this.dataService → {Arg0}", this.dataService);
			Logger.LogDebug("SubscriptionController(): subscriptionSecurityController → {SubscriptionSecurityController}", subscriptionSecurityController);
			Logger.LogDebug("SubscriptionController(): this.subscriptionSecurityController → {Arg0}", this.subscriptionSecurityController);
			Logger.LogDebug("SubscriptionController(): dataService → {DataService}", dataService);
        }

        /// <inheritdoc />
        public IEnumerable<Subscription> GetUserSubscriptions(UserInfo user, int portalId, int subscriptionTypeId = -1)
        {
			Logger.LogDebug(">>> Entering GetUserSubscriptions(user={User},portalId={PortalId},subscriptionTypeId={SubscriptionTypeId})", user, portalId, subscriptionTypeId);
            var subscriptions = CBO.FillCollection<Subscription>(this.dataService.GetSubscriptionsByUser(
                portalId,
                user.UserID,
                subscriptionTypeId));

			Logger.LogDebug("<<< Exiting GetUserSubscriptions(user={User},portalId={PortalId},subscriptionTypeId={SubscriptionTypeId})", user, portalId, subscriptionTypeId);
            return subscriptions.Where(s => this.subscriptionSecurityController.HasPermission(s));
        }

        /// <inheritdoc />
        public IEnumerable<Subscription> GetContentSubscriptions(int portalId, int subscriptionTypeId, string objectKey)
        {
			Logger.LogDebug(">>> Entering GetContentSubscriptions(portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey})", portalId, subscriptionTypeId, objectKey);
            var subscriptions = CBO.FillCollection<Subscription>(this.dataService.GetSubscriptionsByContent(
                portalId,
                subscriptionTypeId,
                objectKey));

			Logger.LogDebug("<<< Exiting GetContentSubscriptions(portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey})", portalId, subscriptionTypeId, objectKey);
            return subscriptions.Where(s => this.subscriptionSecurityController.HasPermission(s));
        }

        /// <inheritdoc />
        public bool IsSubscribed(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering IsSubscribed(subscription={Subscription})", subscription);
            var fetchedSubscription = CBO.FillObject<Subscription>(this.dataService.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId));

			Logger.LogDebug("<<< Exiting IsSubscribed(subscription={Subscription})", subscription);
            return fetchedSubscription != null && this.subscriptionSecurityController.HasPermission(fetchedSubscription);
        }

        /// <inheritdoc />
        public void AddSubscription(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering AddSubscription(subscription={Subscription})", subscription);
            Requires.NotNull("subscription", subscription);
            Requires.NotNegative("subscription.UserId", subscription.UserId);
            Requires.NotNegative("subscription.SubscriptionTypeId", subscription.SubscriptionTypeId);
            Requires.PropertyNotNull("subscription.ObjectKey", subscription.ObjectKey);

            subscription.SubscriptionId = this.dataService.AddSubscription(
                subscription.UserId,
                subscription.PortalId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.Description,
                subscription.ModuleId,
                subscription.TabId,
                subscription.ObjectData);
				Logger.LogDebug("<<< Exiting AddSubscription(subscription={Subscription})", subscription);
        }

        /// <inheritdoc />
        public void DeleteSubscription(Subscription subscription)
        {
			Logger.LogDebug(">>> Entering DeleteSubscription(subscription={Subscription})", subscription);
            Requires.NotNull("subscription", subscription);

            var subscriptionToDelete = CBO.FillObject<Subscription>(this.dataService.IsSubscribed(
                subscription.PortalId,
                subscription.UserId,
                subscription.SubscriptionTypeId,
                subscription.ObjectKey,
                subscription.ModuleId,
                subscription.TabId));

            if (subscriptionToDelete == null)
            {
                return;
            }

            this.dataService.DeleteSubscription(subscriptionToDelete.SubscriptionId);
			Logger.LogDebug("<<< Exiting DeleteSubscription(subscription={Subscription})", subscription);
        }

        /// <inheritdoc />
        public int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription)
        {
			Logger.LogDebug(">>> Entering UpdateSubscriptionDescription(objectKey={ObjectKey},portalId={PortalId},newDescription={NewDescription})", objectKey, portalId, newDescription);
            Requires.PropertyNotNull("objectKey", objectKey);
            Requires.NotNull("portalId", portalId);
            Requires.PropertyNotNull("newDescription", newDescription);
			Logger.LogDebug("<<< Exiting UpdateSubscriptionDescription(objectKey={ObjectKey},portalId={PortalId},newDescription={NewDescription})", objectKey, portalId, newDescription);
            return this.dataService.UpdateSubscriptionDescription(objectKey, portalId, newDescription);
        }

        /// <inheritdoc />
        public void DeleteSubscriptionsByObjectKey(int portalId, string objectKey)
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionsByObjectKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
            this.dataService.DeleteSubscriptionsByObjectKey(portalId, objectKey);
			Logger.LogDebug("<<< Exiting DeleteSubscriptionsByObjectKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
        }

        /// <inheritdoc />
        protected override Func<ISubscriptionController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new SubscriptionController();
        }
    }
}
