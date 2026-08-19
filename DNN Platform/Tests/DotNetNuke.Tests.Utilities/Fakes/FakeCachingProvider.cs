// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Utilities.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Services.Cache;

    using Moq;
	using Microsoft.Extensions.Logging;

    public class FakeCachingProvider : CachingProvider
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Dictionary<string, object> _dictionary;

        public FakeCachingProvider(Dictionary<string, object> dictionary)
            : base(Mock.Of<IHostSettings>())
        {
			_logger.LogDebug(">>> Entering FakeCachingProvider(dictionary={Dictionary})", dictionary);
            this._dictionary = dictionary;
			_logger.LogDebug("FakeCachingProvider(dictionary={Dictionary}): this._dictionary → {Arg1}", dictionary, this._dictionary);
			_logger.LogDebug("<<< Exiting FakeCachingProvider(dictionary={Dictionary})", dictionary);
        }

        public override void Insert(string cacheKey, object itemToCache, DNNCacheDependency dependency, DateTime absoluteExpiration,
            TimeSpan slidingExpiration, CacheItemPriority priority, CacheItemRemovedCallback onRemoveCallback)
        {
			_logger.LogDebug(">>> Entering Insert(cacheKey={CacheKey},itemToCache={ItemToCache},dependency={Dependency},absoluteExpiration={AbsoluteExpiration},slidingExpiration={SlidingExpiration},priority={Priority},onRemoveCallback={OnRemoveCallback})", cacheKey, itemToCache, dependency, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
            this._dictionary[cacheKey] = itemToCache;
			_logger.LogDebug("<<< Exiting Insert(cacheKey={CacheKey},itemToCache={ItemToCache},dependency={Dependency},absoluteExpiration={AbsoluteExpiration},slidingExpiration={SlidingExpiration},priority={Priority},onRemoveCallback={OnRemoveCallback})", cacheKey, itemToCache, dependency, absoluteExpiration, slidingExpiration, priority, onRemoveCallback);
        }

        public override object GetItem(string cacheKey)
        {
			_logger.LogDebug(">>> Entering GetItem(cacheKey={CacheKey})", cacheKey);
			_logger.LogDebug("<<< Exiting GetItem(cacheKey={CacheKey})", cacheKey);
            return this._dictionary.ContainsKey(cacheKey) ? this._dictionary[cacheKey] : null;
        }
    }
}
