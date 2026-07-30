// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ScopeTypeController provides the business layer of ScopeType.</summary>
    /// <seealso cref="TermController"/>
    public class ScopeTypeController(IDataService dataService, IHostSettings hostSettings) : IScopeTypeController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int CacheTimeOut = 20;
		Logger.LogDebug(">>> Entering ScopeTypeController(dataService={DataService},hostSettings={HostSettings})", dataService, hostSettings);
        private readonly IDataService dataService = dataService ?? Util.GetDataService();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
		Logger.LogDebug("ScopeTypeController(dataService={DataService},hostSettings={HostSettings}): dataService → {DataService1}", dataService, hostSettings, dataService);
		Logger.LogDebug("ScopeTypeController(dataService={DataService},hostSettings={HostSettings}): hostSettings → {HostSettings1}", dataService, hostSettings, hostSettings);

        /// <summary>Initializes a new instance of the <see cref="ScopeTypeController"/> class.</summary>
		Logger.LogDebug("<<< Exiting ScopeTypeController(dataService={DataService},hostSettings={HostSettings})", dataService, hostSettings);
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ScopeTypeController()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ScopeTypeController"/> class.</summary>
        /// <param name="dataService">The data service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ScopeTypeController(IDataService dataService)
            : this(dataService, null)
        {
        }

        /// <inheritdoc />
        public int AddScopeType(ScopeType scopeType)
        {
			Logger.LogDebug(">>> Entering AddScopeType(scopeType={ScopeType})", scopeType);
            // Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

            scopeType.ScopeTypeId = this.dataService.AddScopeType(scopeType);

            // Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);

			Logger.LogDebug("<<< Exiting AddScopeType(scopeType={ScopeType})", scopeType);
            return scopeType.ScopeTypeId;
        }

        /// <inheritdoc />
        public void ClearScopeTypeCache()
        {
			Logger.LogDebug(">>> Entering ClearScopeTypeCache()");
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
			Logger.LogDebug("<<< Exiting ClearScopeTypeCache()");
        }

        /// <inheritdoc />
        public void DeleteScopeType(ScopeType scopeType)
        {
			Logger.LogDebug(">>> Entering DeleteScopeType(scopeType={ScopeType})", scopeType);
            // Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);

            this.dataService.DeleteScopeType(scopeType);

            // Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
			Logger.LogDebug("<<< Exiting DeleteScopeType(scopeType={ScopeType})", scopeType);
        }

        /// <inheritdoc />
        public IQueryable<ScopeType> GetScopeTypes()
        {
			Logger.LogDebug(">>> Entering GetScopeTypes()");
            var scopeTypes = CBO.GetCachedObject<List<ScopeType>>(
                this.hostSettings,
                new CacheItemArgs(DataCache.ScopeTypesCacheKey, CacheTimeOut),
                this.GetScopeTypesCallBack);
			Logger.LogDebug("<<< Exiting GetScopeTypes()");
            return scopeTypes.AsQueryable();
        }

        /// <inheritdoc />
        public void UpdateScopeType(ScopeType scopeType)
        {
			Logger.LogDebug(">>> Entering UpdateScopeType(scopeType={ScopeType})", scopeType);
            // Argument Contract
            Requires.NotNull("scopeType", scopeType);
            Requires.PropertyNotNegative("scopeType", "ScopeTypeId", scopeType.ScopeTypeId);
            Requires.PropertyNotNullOrEmpty("scopeType", "ScopeType", scopeType.ScopeType);

            this.dataService.UpdateScopeType(scopeType);

            // Refresh cached collection of types
            DataCache.RemoveCache(DataCache.ScopeTypesCacheKey);
			Logger.LogDebug("<<< Exiting UpdateScopeType(scopeType={ScopeType})", scopeType);
        }

        private object GetScopeTypesCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogDebug(">>> Entering GetScopeTypesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
			Logger.LogDebug("<<< Exiting GetScopeTypesCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return CBO.FillQueryable<ScopeType>(this.dataService.GetScopeTypes()).ToList();
        }
    }
}
