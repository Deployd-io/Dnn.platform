// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Caching;

    /// <summary>MemoryResponseFilter implements the OutputCachingProvider for memory storage.</summary>
    public class MemoryProvider : OutputCachingProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:ConstFieldNamesMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected const string cachePrefix = "DNN_OUTPUT:";

        private static System.Web.Caching.Cache runtimeCache;

        internal static System.Web.Caching.Cache Cache
        {
            get
            {
                // create singleton of the cache object
                if (runtimeCache == null)
                {
                    runtimeCache = HttpRuntime.Cache;
                }

                return runtimeCache;
            }
        }

        /// <inheritdoc />
        public override string GenerateCacheKey(int tabId, System.Collections.Specialized.StringCollection includeVaryByKeys, System.Collections.Specialized.StringCollection excludeVaryByKeys, SortedDictionary<string, string> varyBy)
        {
            return GetCacheKey(base.GenerateCacheKey(tabId, includeVaryByKeys, excludeVaryByKeys, varyBy));
        }

        /// <inheritdoc />
        public override int GetItemCount(int tabId)
        {
			Logger.LogDebug(">>> Entering GetItemCount(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetItemCount(tabId={TabId})", tabId);
            return GetCacheKeys(tabId).Count;
        }

        /// <inheritdoc />
        public override byte[] GetOutput(int tabId, string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetOutput(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            object output = Cache[cacheKey];
			Logger.LogDebug("<<< Exiting GetOutput(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            return (byte[])output;
        }

        /// <inheritdoc />
        public override OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration)
        {
			Logger.LogDebug(">>> Entering GetResponseFilter(tabId={TabId},maxVaryByCount={MaxVaryByCount},responseFilter={ResponseFilter},cacheKey={CacheKey},cacheDuration={CacheDuration})", tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
			Logger.LogDebug("<<< Exiting GetResponseFilter(tabId={TabId},maxVaryByCount={MaxVaryByCount},responseFilter={ResponseFilter},cacheKey={CacheKey},cacheDuration={CacheDuration})", tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
            return new MemoryResponseFilter(tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
        }

        /// <inheritdoc />
        public override void PurgeCache(int portalId)
        {
			Logger.LogDebug(">>> Entering PurgeCache(portalId={PortalId})", portalId);
            foreach (string key in GetCacheKeys())
            {
                Cache.Remove(key);
            }
        }

        /// <inheritdoc />
        public override void PurgeExpiredItems(int portalId)
        {
			Logger.LogDebug(">>> Entering PurgeExpiredItems(portalId={PortalId})", portalId);
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Remove(int tabId)
        {
			Logger.LogDebug(">>> Entering Remove(tabId={TabId})", tabId);
            foreach (string key in GetCacheKeys(tabId))
            {
                Cache.Remove(key);
				Logger.LogDebug(">>> Entering GetCacheKeys()");
            }
        }

        /// <inheritdoc />
        public override void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output)
        {
            Cache.Insert(cacheKey, output, null, DateTime.UtcNow.Add(duration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
        }

        /// <inheritdoc />
        public override bool StreamOutput(int tabId, string cacheKey, HttpContext context)
        {
			Logger.LogDebug(">>> Entering StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context})", tabId, cacheKey, context);
            if (Cache[cacheKey] == null)
            {
                return false;
            }

            context.Response.BinaryWrite(Encoding.Default.GetBytes(Cache[cacheKey].ToString()));
			Logger.LogDebug("<<< Exiting StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context})", tabId, cacheKey, context);
            return true;
        }

        internal static List<string> GetCacheKeys()
        {
			Logger.LogDebug(">>> Entering GetCacheKeys()");
            var keys = new List<string>();
            IDictionaryEnumerator cacheEnum = Cache.GetEnumerator();
			Logger.LogDebug("GetCacheKeys(): keys → {Keys}", keys);
			Logger.LogDebug("GetCacheKeys(): cacheEnum → {CacheEnum}", cacheEnum);
            while (cacheEnum.MoveNext())
            {
                if (cacheEnum.Key.ToString().StartsWith(cachePrefix, StringComparison.Ordinal))
                {
                    keys.Add(cacheEnum.Key.ToString());
                }
            }

            return keys;
        }

        internal static List<string> GetCacheKeys(int tabId)
        {
			Logger.LogDebug(">>> Entering GetCacheKeys(tabId={TabId})", tabId);
            var keys = new List<string>();
			Logger.LogDebug("<<< Exiting GetCacheKeys()");
			Logger.LogDebug("GetCacheKeys(tabId={TabId}): keys → {Keys}", tabId, keys);
            IDictionaryEnumerator cacheEnum = Cache.GetEnumerator();
            while (cacheEnum.MoveNext())
			Logger.LogDebug("GetCacheKeys(tabId={TabId}): cacheEnum → {CacheEnum}", tabId, cacheEnum);
            {
                if (cacheEnum.Key.ToString().StartsWith($"{cachePrefix}{tabId.ToString(CultureInfo.InvariantCulture)}_", StringComparison.Ordinal))
                {
                    keys.Add(cacheEnum.Key.ToString());
                }
            }

            return keys;
        }

        private static string GetCacheKey(string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetCacheKey(cacheKey={CacheKey})", cacheKey);
            if (string.IsNullOrEmpty(cacheKey))
            {
                throw new ArgumentException("Argument cannot be null or an empty string", nameof(cacheKey));
            }

			Logger.LogDebug("<<< Exiting GetCacheKey(cacheKey={CacheKey})", cacheKey);
            return string.Concat(cachePrefix, cacheKey);
        }
    }
}
