// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Social.Subscriptions.Data;
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This controller is responsible to manage the subscription types.</summary>
    /// <param name="dataService">The data service.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class SubscriptionTypeController(IDataService dataService, IHostSettings hostSettings)
        : ServiceLocator<ISubscriptionTypeController, SubscriptionTypeController>, ISubscriptionTypeController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SubscriptionTypeController));
        private readonly IDataService dataService = dataService ?? DataService.Instance;
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="SubscriptionTypeController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public SubscriptionTypeController()
            : this(null, null)
        {
		Logger.LogError(">>> Entering SubscriptionTypeController()");
        Logger.LogWarning("<<< Exiting SubscriptionTypeController()");
        }

        /// <inheritdoc />
        public void AddSubscriptionType(SubscriptionType subscriptionType)
        {
			Logger.LogError(">>> Entering AddSubscriptionType(subscriptionType={SubscriptionType})", subscriptionType);
            Requires.NotNull("subscriptionType", subscriptionType);

            subscriptionType.SubscriptionTypeId = this.dataService.AddSubscriptionType(
                subscriptionType.SubscriptionName,
                subscriptionType.FriendlyName,
                subscriptionType.DesktopModuleId);

            CleanCache();
			Logger.LogWarning("<<< Exiting AddSubscriptionType(subscriptionType={SubscriptionType})", subscriptionType);
        }

        /// <inheritdoc />
        public SubscriptionType GetSubscriptionType(Func<SubscriptionType, bool> predicate)
        {
			Logger.LogError(">>> Entering GetSubscriptionType(predicate={Predicate})", predicate);
            Requires.NotNull("predicate", predicate);

			Logger.LogWarning("<<< Exiting GetSubscriptionType(predicate={Predicate})", predicate);
            return this.GetSubscriptionTypes().SingleOrDefault(predicate);
        }

        /// <inheritdoc />
        public IEnumerable<SubscriptionType> GetSubscriptionTypes()
        {
			Logger.LogError(">>> Entering GetSubscriptionTypes()");
            var cacheArgs = new CacheItemArgs(
                DataCache.SubscriptionTypesCacheKey,
                DataCache.SubscriptionTypesTimeOut,
                DataCache.SubscriptionTypesCachePriority);

			Logger.LogWarning("<<< Exiting GetSubscriptionTypes()");
            return CBO.GetCachedObject<IEnumerable<SubscriptionType>>(
                this.hostSettings,
                cacheArgs,
			Logger.LogWarning("<<< Exiting GetSubscriptionTypes(predicate={Predicate})", predicate);
                _ => CBO.FillCollection<SubscriptionType>(this.dataService.GetSubscriptionTypes()));
        }

        /// <inheritdoc />
        public IEnumerable<SubscriptionType> GetSubscriptionTypes(Func<SubscriptionType, bool> predicate)
        {
            Requires.NotNull("predicate", predicate);

            return this.GetSubscriptionTypes().Where(predicate);
        }

        /// <inheritdoc />
        public void DeleteSubscriptionType(SubscriptionType subscriptionType)
        {
			Logger.LogError(">>> Entering DeleteSubscriptionType(subscriptionType={SubscriptionType})", subscriptionType);
            Requires.NotNull("subscriptionType", subscriptionType);
            Requires.NotNegative("subscriptionType.SubscriptionTypeId", subscriptionType.SubscriptionTypeId);

            this.dataService.DeleteSubscriptionType(subscriptionType.SubscriptionTypeId);
            CleanCache();
			Logger.LogWarning("<<< Exiting DeleteSubscriptionType(subscriptionType={SubscriptionType})", subscriptionType);
        }

        /// <inheritdoc />
        protected override Func<ISubscriptionTypeController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<ISubscriptionTypeController>();
        }

        private static void CleanCache()
        {
			Logger.LogError(">>> Entering CleanCache()");
            DataCache.RemoveCache(DataCache.SubscriptionTypesCacheKey);
			Logger.LogWarning("<<< Exiting CleanCache()");
        }
    }
}
