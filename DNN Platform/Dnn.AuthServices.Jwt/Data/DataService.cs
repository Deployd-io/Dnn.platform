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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly DataProvider dataProvider = DataProvider.Instance();
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="DataService"/> class.</summary>
        public DataService()
            : this(null)
        {
		Logger.LogDebug(">>> Entering DataService()");
		Logger.LogDebug(">>> Entering DataService(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("DataService(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
		Logger.LogDebug("<<< Exiting DataService()");
		Logger.LogDebug("<<< Exiting DataService(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="DataService"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public DataService(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ??
                                HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>() ??
								Logger.LogDebug("DataService(hostSettings={HostSettings}): this.hostSettings → {Arg1}", hostSettings, this.hostSettings);
                                new HostSettings(
                                    new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                        new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                        new Lazy<IPortalController>(() => PortalController.Instance)));
        }

        /// <inheritdoc />
        public virtual PersistedToken GetTokenById(string tokenId)
        {
			Logger.LogDebug(">>> Entering GetTokenById(tokenId={TokenId})", tokenId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                return CBO.GetCachedObject<PersistedToken>(
                    this.hostSettings,
                    new CacheItemArgs(GetCacheKey(tokenId), 60, CacheItemPriority.Default),
                    _ => CBO.FillObject<PersistedToken>(this.dataProvider.ExecuteReader("JsonWebTokens_GetById", tokenId)));
					Logger.LogInformation("GetTokenById(tokenId)={TokenId}: database query executed in {Arg1} ms", tokenId, __logStart.ElapsedMilliseconds);
            }
            catch (InvalidCastException)
            {
				Logger.LogError("Exception in GetTokenById(tokenId={TokenId}): {Arg1}", tokenId, ex.Message);
                // occurs when no record found in th DB
				Logger.LogDebug("<<< Exiting GetTokenById(tokenId={TokenId})", tokenId);
                return null;
            }
        }

        /// <inheritdoc />
        public virtual IList<PersistedToken> GetUserTokens(int userId)
        {
			Logger.LogDebug(">>> Entering GetUserTokens(userId={UserId})", userId);
			Logger.LogDebug("<<< Exiting GetUserTokens(userId={UserId})", userId);
            return CBO.FillCollection<PersistedToken>(this.dataProvider.ExecuteReader("JsonWebTokens_GetByUserId", userId));
        }

        /// <inheritdoc />
        public virtual void AddToken(PersistedToken token)
        {
			Logger.LogDebug(">>> Entering AddToken(token={Token})", token);
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
			Logger.LogInformation("AddToken(token)={Token}: database query executed in {Arg1} ms", token, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting AddToken(token={Token})", token);
        }

        /// <inheritdoc />
        public virtual void UpdateToken(PersistedToken token)
        {
			Logger.LogDebug(">>> Entering UpdateToken(token={Token})", token);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_Update", token.TokenId, token.TokenExpiry, token.TokenHash);
            token.RenewCount += 1;
            DataCache.SetCache(GetCacheKey(token.TokenId), token, token.TokenExpiry.ToLocalTime());
			Logger.LogInformation("UpdateToken(token)={Token}: database query executed in {Arg1} ms", token, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting UpdateToken(token={Token})", token);
        }

        /// <inheritdoc />
        public virtual void DeleteToken(string tokenId)
        {
			Logger.LogDebug(">>> Entering DeleteToken(tokenId={TokenId})", tokenId);
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteById", tokenId);
            DataCache.RemoveCache(GetCacheKey(tokenId));
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteToken(tokenId)={TokenId}: database query executed in {Arg1} ms", tokenId, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting DeleteToken(tokenId={TokenId})", tokenId);
        }

        /// <inheritdoc />
        public virtual void DeleteUserTokens(int userId)
        {
			Logger.LogDebug(">>> Entering DeleteUserTokens(userId={UserId})", userId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteByUser", userId);
            foreach (var token in this.GetUserTokens(userId))
            {
			Logger.LogInformation("DeleteUserTokens(userId)={UserId}: database query executed in {Arg1} ms", userId, __logStart.ElapsedMilliseconds);
                DataCache.RemoveCache(GetCacheKey(token.TokenId));
            }
			Logger.LogDebug("<<< Exiting DeleteUserTokens(userId={UserId})", userId);
        }

        /// <inheritdoc />
        public virtual void DeleteExpiredTokens()
        {
			Logger.LogDebug(">>> Entering DeleteExpiredTokens()");
            // don't worry about caching; these will already be invalidated by cache manager
            this.dataProvider.ExecuteNonQuery("JsonWebTokens_DeleteExpired");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("DeleteExpiredTokens(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting DeleteExpiredTokens()");
        }

        private static string GetCacheKey(string tokenId)
        {
			Logger.LogDebug(">>> Entering GetCacheKey(tokenId={TokenId})", tokenId);
			Logger.LogDebug("<<< Exiting GetCacheKey(tokenId={TokenId})", tokenId);
            return string.Join(":", "JsonWebTokens", tokenId);
        }
    }
}
