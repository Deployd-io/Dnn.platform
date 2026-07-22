// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Caching;

    /// <summary>FileResponseFilter implements <see cref="OutputCacheResponseFilter"/> to capture the response into memory.</summary>
    public class MemoryResponseFilter : OutputCacheResponseFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        ////Private _content As StringBuilder
        private static System.Web.Caching.Cache runtimeCache;

        /// <summary>Initializes a new instance of the <see cref="MemoryResponseFilter"/> class.</summary>
        /// <param name="itemId">The tab ID.</param>
        /// <param name="maxVaryByCount">The maximum number of values by which the cached response can vary.</param>
        /// <param name="filterChain">The stream to write into the cache.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheDuration">The duration for which the response should be cached.</param>
        internal MemoryResponseFilter(int itemId, int maxVaryByCount, Stream filterChain, string cacheKey, TimeSpan cacheDuration)
            : base(filterChain, cacheKey, cacheDuration, maxVaryByCount)
        {
			Logger.LogDebug(">>> Entering MemoryResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration})", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration);
            if (maxVaryByCount > -1 && Services.OutputCache.Providers.MemoryProvider.GetCacheKeys(itemId).Count >= maxVaryByCount)
            {
                this.HasErrored = true;
				Logger.LogDebug("MemoryResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration}): this.HasErrored → {Arg5}", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration, this.HasErrored);
                return;
            }

            this.CaptureStream = new MemoryStream();
			Logger.LogDebug("<<< Exiting MemoryResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration})", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration);
        }

        protected static System.Web.Caching.Cache Cache
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
        protected override void AddItemToCache(int itemId, string output)
        {
			Logger.LogDebug(">>> Entering AddItemToCache(itemId={ItemId},output={Output})", itemId, output);
            Cache.Insert(this.CacheKey, output, null, DateTime.UtcNow.Add(this.CacheDuration), System.Web.Caching.Cache.NoSlidingExpiration, CacheItemPriority.Default, null);
			Logger.LogDebug("<<< Exiting AddItemToCache(itemId={ItemId},output={Output})", itemId, output);
        }

        /// <inheritdoc />
        protected override void RemoveItemFromCache(int itemId)
        {
			Logger.LogDebug(">>> Entering RemoveItemFromCache(itemId={ItemId})", itemId);
            Cache.Remove(this.CacheKey);
			Logger.LogDebug("<<< Exiting RemoveItemFromCache(itemId={ItemId})", itemId);
        }
    }
}
