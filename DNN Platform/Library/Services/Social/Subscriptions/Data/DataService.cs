// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions.Data
{
    using System;
    using System.Data;

    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class DataService : ServiceLocator<IDataService, DataService>, IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        private readonly DataProvider provider;

        /// <summary>Initializes a new instance of the <see cref="DataService"/> class.</summary>
        public DataService()
        {
			Logger.LogError(">>> Entering DataService()");
            this.provider = DataProvider.Instance();
			Logger.LogDebug("DataService(): provider → {Provider}", provider);
        Logger.LogWarning("<<< Exiting DataService()");
        }

        /// <inheritdoc />
        public int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId)
        {
			Logger.LogError(">>> Entering AddSubscriptionType(subscriptionName={SubscriptionName},friendlyName={FriendlyName},desktopModuleId={DesktopModuleId})", subscriptionName, friendlyName, desktopModuleId);
            Logger.LogWarning("<<< Exiting AddSubscriptionType(subscriptionName={SubscriptionName},friendlyName={FriendlyName},desktopModuleId={DesktopModuleId})", subscriptionName, friendlyName, desktopModuleId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_AddSubscriptionType", subscriptionName, friendlyName, desktopModuleId);
        }

        /// <inheritdoc />
        public IDataReader GetSubscriptionTypes()
        {
			Logger.LogError(">>> Entering GetSubscriptionTypes()");
            Logger.LogWarning("<<< Exiting GetSubscriptionTypes()");
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionTypes");
        }

        /// <inheritdoc />
        public bool DeleteSubscriptionType(int subscriptionTypeId)
        {
			Logger.LogError(">>> Entering DeleteSubscriptionType(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            Logger.LogWarning("<<< Exiting DeleteSubscriptionType(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscriptionType", subscriptionTypeId) == 0;
        }

        /// <inheritdoc />
        public int AddSubscription(int userId, int portalId, int subscriptionTypeId, string objectKey, string description, int moduleId, int tabId, string objectData)
        {
			Logger.LogError(">>> Entering AddSubscription(userId={UserId},portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey},description={Description},moduleId={ModuleId},tabId={TabId},objectData={ObjectData})", userId, portalId, subscriptionTypeId, objectKey, description, moduleId, tabId, objectData);
            Logger.LogWarning("<<< Exiting AddSubscription(userId={UserId},portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey},description={Description},moduleId={ModuleId},tabId={TabId},objectData={ObjectData})", userId, portalId, subscriptionTypeId, objectKey, description, moduleId, tabId, objectData);
            return this.provider.ExecuteScalar<int>(
                "CoreMessaging_AddSubscription",
                userId,
                this.provider.GetNull(portalId),
                subscriptionTypeId,
                objectKey,
                description,
                this.provider.GetNull(moduleId),
                this.provider.GetNull(tabId),
                objectData);
        }

        /// <inheritdoc />
        public IDataReader GetSubscriptionsByUser(int portalId, int userId, int subscriptionTypeId)
        {
			Logger.LogError(">>> Entering GetSubscriptionsByUser(portalId={PortalId},userId={UserId},subscriptionTypeId={SubscriptionTypeId})", portalId, userId, subscriptionTypeId);
            Logger.LogWarning("<<< Exiting GetSubscriptionsByUser(portalId={PortalId},userId={UserId},subscriptionTypeId={SubscriptionTypeId})", portalId, userId, subscriptionTypeId);
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionsByUser", this.provider.GetNull(portalId), userId, this.provider.GetNull(subscriptionTypeId));
        }

        /// <inheritdoc />
        public IDataReader GetSubscriptionsByContent(int portalId, int subscriptionTypeId, string objectKey)
        {
			Logger.LogError(">>> Entering GetSubscriptionsByContent(portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey})", portalId, subscriptionTypeId, objectKey);
            Logger.LogWarning("<<< Exiting GetSubscriptionsByContent(portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey})", portalId, subscriptionTypeId, objectKey);
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionsByContent", this.provider.GetNull(portalId), subscriptionTypeId, objectKey);
        }

        /// <inheritdoc />
        public IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, string objectKey, int moduleId, int tabId)
        {
			Logger.LogError(">>> Entering IsSubscribed(portalId={PortalId},userId={UserId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey},moduleId={ModuleId},tabId={TabId})", portalId, userId, subscriptionTypeId, objectKey, moduleId, tabId);
            return this.provider.ExecuteReader(
                "CoreMessaging_IsSubscribed",
                this.provider.GetNull(portalId),
                userId,
                subscriptionTypeId,
                objectKey,
                this.provider.GetNull(moduleId),
                this.provider.GetNull(tabId));
        }

        /// <inheritdoc />
        public bool DeleteSubscription(int subscriptionId)
        {
			Logger.LogError(">>> Entering DeleteSubscription(subscriptionId={SubscriptionId})", subscriptionId);
            Logger.LogWarning("<<< Exiting DeleteSubscription(subscriptionId={SubscriptionId})", subscriptionId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscription", subscriptionId) == 0;
        }

        /// <inheritdoc />
        public int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription)
        {
			Logger.LogError(">>> Entering UpdateSubscriptionDescription(objectKey={ObjectKey},portalId={PortalId},newDescription={NewDescription})", objectKey, portalId, newDescription);
            Logger.LogWarning("<<< Exiting UpdateSubscriptionDescription(objectKey={ObjectKey},portalId={PortalId},newDescription={NewDescription})", objectKey, portalId, newDescription);
            return this.provider.ExecuteScalar<int>(
                "CoreMessaging_UpdateSubscriptionDescription",
                objectKey,
                portalId,
                newDescription);
        }

        /// <inheritdoc />
        public void DeleteSubscriptionsByObjectKey(int portalId, string objectKey)
        {
			Logger.LogError(">>> Entering DeleteSubscriptionsByObjectKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteSubscriptionsByObjectKey", portalId, objectKey);
			Logger.LogWarning("<<< Exiting DeleteSubscriptionsByObjectKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
        }

        /// <inheritdoc />
        protected override Func<IDataService> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new DataService();
        }
    }
}
