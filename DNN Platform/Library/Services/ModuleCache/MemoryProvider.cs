// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ModuleCache
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Web.Caching;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    public class MemoryProvider : ModuleCachingProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MemoryProvider));
        private const string CachePrefix = "ModuleCache:";

        /// <inheritdoc />
        public override string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy)
        {
			Logger.LogError(">>> Entering GenerateCacheKey(tabModuleId={TabModuleId},varyBy={VaryBy})", tabModuleId, varyBy);
            var cacheKey = new StringBuilder();
			Logger.LogError("GenerateCacheKey(tabModuleId={TabModuleId},varyBy={VaryBy}): cacheKey → {CacheKey}", tabModuleId, varyBy, cacheKey);
            if (varyBy != null)
            {
                foreach (KeyValuePair<string, string> kvp in varyBy)
                {
                    cacheKey.Append($"{kvp.Key.ToLowerInvariant()}={kvp.Value}|");
                }
            }

			Logger.LogWarning("<<< Exiting GenerateCacheKey(tabModuleId={TabModuleId},varyBy={VaryBy})", tabModuleId, varyBy);
            return $"{CachePrefix}|{tabModuleId.ToString(CultureInfo.InvariantCulture)}|{cacheKey}";
        }

        /// <inheritdoc />
        public override int GetItemCount(int tabModuleId)
        {
			Logger.LogError(">>> Entering GetItemCount(tabModuleId={TabModuleId})", tabModuleId);
            Logger.LogWarning("<<< Exiting GetItemCount(tabModuleId={TabModuleId})", tabModuleId);
            return GetCacheKeys(tabModuleId).Count;
        }

        /// <inheritdoc />
        public override byte[] GetModule(int tabModuleId, string cacheKey)
        {
			Logger.LogError(">>> Entering GetModule(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            Logger.LogWarning("<<< Exiting GetModule(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            return DataCache.GetCache<byte[]>(cacheKey);
        }

        /// <inheritdoc />
        public override void PurgeCache(int portalId)
        {
			Logger.LogError(">>> Entering PurgeCache(portalId={PortalId})", portalId);
            DataCache.ClearCache(CachePrefix);
			Logger.LogWarning("<<< Exiting PurgeCache(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public override void Remove(int tabModuleId)
        {
			Logger.LogError(">>> Entering Remove(tabModuleId={TabModuleId})", tabModuleId);
            DataCache.ClearCache($"{CachePrefix}|{tabModuleId.ToString(CultureInfo.InvariantCulture)}");
			Logger.LogWarning("<<< Exiting Remove(tabModuleId={TabModuleId})", tabModuleId);
        }

        /// <inheritdoc />
        public override void SetModule(int tabModuleId, string cacheKey, TimeSpan duration, byte[] moduleOutput)
        {
			Logger.LogError(">>> Entering SetModule(tabModuleId={TabModuleId},cacheKey={CacheKey},duration={Duration},moduleOutput={ModuleOutput})", tabModuleId, cacheKey, duration, moduleOutput);
            DNNCacheDependency dep = null;
            DataCache.SetCache(cacheKey, moduleOutput, dep, DateTime.UtcNow.Add(duration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
			Logger.LogWarning("<<< Exiting SetModule(tabModuleId={TabModuleId},cacheKey={CacheKey},duration={Duration},moduleOutput={ModuleOutput})", tabModuleId, cacheKey, duration, moduleOutput);
        }

        /// <inheritdoc />
        public override void PurgeExpiredItems(int portalId)
        {
			Logger.LogError(">>> Entering PurgeExpiredItems(portalId={PortalId})", portalId);
            // throw new NotSupportedException();
			Logger.LogWarning("<<< Exiting PurgeExpiredItems(portalId={PortalId})", portalId);
        }

        private static List<string> GetCacheKeys(int tabModuleId)
        {
			Logger.LogError(">>> Entering GetCacheKeys(tabModuleId={TabModuleId})", tabModuleId);
            var keys = new List<string>();
            IDictionaryEnumerator cacheEnum = CachingProvider.Instance().GetEnumerator();
			Logger.LogError("GetCacheKeys(tabModuleId={TabModuleId}): keys → {Keys}", tabModuleId, keys);
            while (cacheEnum.MoveNext())
            {
				Logger.LogError("GetCacheKeys(tabModuleId={TabModuleId}): cacheEnum → {CacheEnum}", tabModuleId, cacheEnum);
                if (cacheEnum.Key.ToString().StartsWith($"{CachePrefix}|{tabModuleId.ToString(CultureInfo.InvariantCulture)}|", StringComparison.Ordinal))
                {
                    keys.Add(cacheEnum.Key.ToString());
                }
            }

			Logger.LogWarning("<<< Exiting GetCacheKeys(tabModuleId={TabModuleId})", tabModuleId);
            return keys;
        }
    }
}
