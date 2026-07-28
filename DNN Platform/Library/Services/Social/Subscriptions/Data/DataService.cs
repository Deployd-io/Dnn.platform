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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly DataProvider provider;

        /// <summary>Initializes a new instance of the <see cref="DataService"/> class.</summary>
        public DataService()
        {
			Logger.LogDebug(">>> Entering DataService()");
            this.provider = DataProvider.Instance();
			Logger.LogDebug("<<< Exiting DataService()");
			Logger.LogDebug("DataService(): provider → {Provider}", provider);
        }

        /// <inheritdoc />
        public int AddSubscriptionType(string subscriptionName, string friendlyName, int desktopModuleId)
        {
			Logger.LogDebug(">>> Entering AddSubscriptionType(subscriptionName={SubscriptionName},friendlyName={FriendlyName},desktopModuleId={DesktopModuleId})", subscriptionName, friendlyName, desktopModuleId);
			Logger.LogDebug("<<< Exiting AddSubscriptionType(subscriptionName={SubscriptionName},friendlyName={FriendlyName},desktopModuleId={DesktopModuleId})", subscriptionName, friendlyName, desktopModuleId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_AddSubscriptionType", subscriptionName, friendlyName, desktopModuleId);
        }

        /// <inheritdoc />
        public IDataReader GetSubscriptionTypes()
        {
			Logger.LogDebug(">>> Entering GetSubscriptionTypes()");
			Logger.LogDebug("<<< Exiting GetSubscriptionTypes()");
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionTypes");
        }

        /// <inheritdoc />
        public bool DeleteSubscriptionType(int subscriptionTypeId)
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionType(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
			Logger.LogDebug("<<< Exiting DeleteSubscriptionType(subscriptionTypeId={SubscriptionTypeId})", subscriptionTypeId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscriptionType", subscriptionTypeId) == 0;
        }

        /// <inheritdoc />
        public int AddSubscription(int userId, int portalId, int subscriptionTypeId, string objectKey, string description, int moduleId, int tabId, string objectData)
        {
			Logger.LogDebug(">>> Entering AddSubscription(userId={UserId},portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey},description={Description},moduleId={ModuleId},tabId={TabId},objectData={ObjectData})", userId, portalId, subscriptionTypeId, objectKey, description, moduleId, tabId, objectData);
			Logger.LogDebug("<<< Exiting AddSubscription(userId={UserId},portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey},description={Description},moduleId={ModuleId},tabId={TabId},objectData={ObjectData})", userId, portalId, subscriptionTypeId, objectKey, description, moduleId, tabId, objectData);
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
			Logger.LogDebug(">>> Entering GetSubscriptionsByUser(portalId={PortalId},userId={UserId},subscriptionTypeId={SubscriptionTypeId})", portalId, userId, subscriptionTypeId);
			Logger.LogDebug("<<< Exiting GetSubscriptionsByUser(portalId={PortalId},userId={UserId},subscriptionTypeId={SubscriptionTypeId})", portalId, userId, subscriptionTypeId);
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionsByUser", this.provider.GetNull(portalId), userId, this.provider.GetNull(subscriptionTypeId));
        }

        /// <inheritdoc />
        public IDataReader GetSubscriptionsByContent(int portalId, int subscriptionTypeId, string objectKey)
        {
			Logger.LogDebug(">>> Entering GetSubscriptionsByContent(portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey})", portalId, subscriptionTypeId, objectKey);
			Logger.LogDebug("<<< Exiting GetSubscriptionsByContent(portalId={PortalId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey})", portalId, subscriptionTypeId, objectKey);
            return this.provider.ExecuteReader("CoreMessaging_GetSubscriptionsByContent", this.provider.GetNull(portalId), subscriptionTypeId, objectKey);
        }

        /// <inheritdoc />
        public IDataReader IsSubscribed(int portalId, int userId, int subscriptionTypeId, string objectKey, int moduleId, int tabId)
        {
			Logger.LogDebug(">>> Entering IsSubscribed(portalId={PortalId},userId={UserId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey},moduleId={ModuleId},tabId={TabId})", portalId, userId, subscriptionTypeId, objectKey, moduleId, tabId);
			Logger.LogDebug("<<< Exiting IsSubscribed(portalId={PortalId},userId={UserId},subscriptionTypeId={SubscriptionTypeId},objectKey={ObjectKey},moduleId={ModuleId},tabId={TabId})", portalId, userId, subscriptionTypeId, objectKey, moduleId, tabId);
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
			Logger.LogDebug(">>> Entering DeleteSubscription(subscriptionId={SubscriptionId})", subscriptionId);
			Logger.LogDebug("<<< Exiting DeleteSubscription(subscriptionId={SubscriptionId})", subscriptionId);
            return this.provider.ExecuteScalar<int>("CoreMessaging_DeleteSubscription", subscriptionId) == 0;
        }

        /// <inheritdoc />
        public int UpdateSubscriptionDescription(string objectKey, int portalId, string newDescription)
        {
			Logger.LogDebug(">>> Entering UpdateSubscriptionDescription(objectKey={ObjectKey},portalId={PortalId},newDescription={NewDescription})", objectKey, portalId, newDescription);
			Logger.LogDebug("<<< Exiting UpdateSubscriptionDescription(objectKey={ObjectKey},portalId={PortalId},newDescription={NewDescription})", objectKey, portalId, newDescription);
            return this.provider.ExecuteScalar<int>(
                "CoreMessaging_UpdateSubscriptionDescription",
                objectKey,
                portalId,
                newDescription);
        }

        /// <inheritdoc />
        public void DeleteSubscriptionsByObjectKey(int portalId, string objectKey)
        {
			Logger.LogDebug(">>> Entering DeleteSubscriptionsByObjectKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery("CoreMessaging_DeleteSubscriptionsByObjectKey", portalId, objectKey);
			Logger.LogDebug("<<< Exiting DeleteSubscriptionsByObjectKey(portalId={PortalId},objectKey={ObjectKey})", portalId, objectKey);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteSubscriptionsByObjectKey(portalId,objectKey)={PortalId},{ObjectKey}: database query executed in {Arg2} ms", portalId, objectKey, __logStart.ElapsedMilliseconds);
        }

		Logger.LogInformation("DeleteSubscriptionsByObjectKey(portalId,objectKey)={PortalId},{ObjectKey}: database query executed in {Arg2} ms", portalId, objectKey, __logStart.ElapsedMilliseconds);
        /// <inheritdoc />
        protected override Func<IDataService> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new DataService();
        }
    }
}
