// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.AuthServices.Jwt.Data
{
    using System;
    using System.Collections.Generic;
    using System.Web.Caching;

    using Dnn.AuthServices.Jwt.Components.Entity;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This class provides the Data Access Layer for the JWT Authentication library.</summary>
    public class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        private readonly DataProvider dataProvider = DataProvider.Instance();
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DataService"/> class.</summary>
        public DataService()
            : this(null)
        {
		Logger.LogError(">>> Entering DataService()");
		Logger.LogWarning("<<< Exiting DataService()");
        }

        /// <summary>Initializes a new instance of the <see cref="DataService"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DataService(IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering DataService(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings ??
								Logger.LogWarning("<<< Exiting DataService()");
								Logger.LogDebug("DataService(hostSettings={HostSettings}): this.hostSettings → {Arg1}", hostSettings, this.hostSettings);
                                HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ??
                                new HostSettings(
                                    new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                        new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                        new Lazy<IPortalController>(() => PortalController.Instance)));
										Logger.LogDebug("DataService(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        Logger.LogWarning("<<< Exiting DataService(hostSettings={HostSettings})", hostSettings);
        }

        /// <inheritdoc />
        public virtual PersistedToken GetTokenById(string tokenId)
        {
			Logger.LogError(">>> Entering GetTokenById(tokenId=<redacted>)");
            try
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
					Logger.LogError("GetTokenById(tokenId)=<redacted>: database query executed in {Arg1} ms", __logStart.ElapsedMilliseconds);
                return CBO.GetCachedObject<PersistedToken>(
                    this.hostSettings,
                    new CacheItemArgs(GetCacheKey(tokenId), 60, CacheItemPriority.Default),
                    _ => CBO.FillObject<PersistedToken>(this.dataProvider.ExecuteReader("JsonWebTokens_GetById", tokenId)));
            }
            catch (InvalidCastException)
            {
				Logger.LogError("Exception in GetTokenById(tokenId=<redacted>): {Arg1}", InvalidCastException.Message);
                // occurs when no record found in th DB
				Logger.LogWarning("<<< Exiting GetTokenById(tokenId=<redacted>)");
                return null;
            }
        }

        /// <inheritdoc />
        public virtual IList<PersistedToken> GetUserTokens(int userId)
        {
			Logger.LogError(">>> Entering GetUserTokens(userId={UserId})", userId);
            Logger.LogWarning("<<< Exiting GetUserTokens(userId={UserId})", userId);
            return CBO.FillCollection<PersistedToken>(this.dataProvider.ExecuteReader("JsonWebTokens_GetByUserId", userId));
        }

        /// <inheritdoc />
        public virtual void AddToken(PersistedToken token)
        {
			Logger.LogError(">>> Entering AddToken(token=<redacted>)");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.dataProvider.ExecuteNonQuery(
                "JsonWebTokens_Add",
                token.TokenId,
                token.UserId,
                token.TokenExpiry,
                token.RenewalExpiry,
                token.TokenHash,
                token.RenewalHash);
            DataCache.SetCache(GetCacheKey(token.TokenId), token, token.TokenExpiry.ToLocalTime());
			Logger.LogError("AddToken(token)=<redacted>: database query executed in {Arg1} ms", __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting AddToken(token=<redacted>)");
        }

        /// <inheritdoc />
        public virtual void UpdateToken(PersistedToken token)
        {
			Logger.LogError(">>> Entering UpdateToken(token=<redacted>)");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_Update", token.TokenId, token.TokenExpiry, token.TokenHash);
            token.RenewCount += 1;
            DataCache.SetCache(GetCacheKey(token.TokenId), token, token.TokenExpiry.ToLocalTime());
			Logger.LogError("UpdateToken(token)=<redacted>: database query executed in {Arg1} ms", __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting UpdateToken(token=<redacted>)");
        }

        /// <inheritdoc />
        public virtual void DeleteToken(string tokenId)
        {
			Logger.LogError(">>> Entering DeleteToken(tokenId=<redacted>)");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteById", tokenId);
            DataCache.RemoveCache(GetCacheKey(tokenId));
			Logger.LogError("DeleteToken(tokenId)=<redacted>: database query executed in {Arg1} ms", __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting DeleteToken(tokenId=<redacted>)");
        }

        /// <inheritdoc />
        public virtual void DeleteUserTokens(int userId)
        {
			Logger.LogError(">>> Entering DeleteUserTokens(userId={UserId})", userId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogError("DeleteUserTokens(userId)={UserId}: database query executed in {Arg1} ms", userId, __logStart.ElapsedMilliseconds);
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteByUser", userId);
            foreach (var token in this.GetUserTokens(userId))
            {
                DataCache.RemoveCache(GetCacheKey(token.TokenId));
            }
			Logger.LogWarning("<<< Exiting DeleteUserTokens(userId={UserId})", userId);
        }

        /// <inheritdoc />
        public virtual void DeleteExpiredTokens()
        {
			Logger.LogError(">>> Entering DeleteExpiredTokens()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // don't worry about caching; these will already be invalidated by cache manager
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteExpired");
			Logger.LogError("DeleteExpiredTokens(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        Logger.LogWarning("<<< Exiting DeleteExpiredTokens()");
        }

        private static string GetCacheKey(string tokenId)
        {
			Logger.LogError(">>> Entering GetCacheKey(tokenId=<redacted>)");
            Logger.LogWarning("<<< Exiting GetCacheKey(tokenId=<redacted>)");
            return string.Join(":", "JsonWebTokens", tokenId);
        }
    }
}
