// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Text;
    using System.Web.Caching;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    /// <summary>CachePageStatePersister provides a cache based page state persistence mechanism.</summary>
    public class CachePageStatePersister : PageStatePersister
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string ViewStateCacheKey = "__VIEWSTATE_CACHEKEY";

        /// <summary>Initializes a new instance of the <see cref="CachePageStatePersister"/> class.</summary>
        /// <param name="page">The <see cref="Page"/> that the view state persistence mechanism is created for.</param>
        public CachePageStatePersister(Page page)
            : base(page)
        {
		Logger.LogDebug(">>> Entering CachePageStatePersister(page={Page})", page);
		Logger.LogDebug("<<< Exiting CachePageStatePersister(page={Page})", page);
        }

        /// <summary>Loads the Page State from the Cache.</summary>
        public override void Load()
        {
			Logger.LogDebug(">>> Entering Load()");
            // Get the cache key from the web form data
            string key = this.Page.Request.Params[ViewStateCacheKey];

			Logger.LogWarning("Load(): key → {Key}", key);
            // Abort if cache key is not available or valid
            if (string.IsNullOrEmpty(key) || !key.StartsWith("VS_", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidViewStateCacheKeyException("Missing valid " + ViewStateCacheKey);
            }

            var state = DataCache.GetCache<Pair>(key);
            if (state != null)
			Logger.LogDebug("Load(): state → {State}", state);
            {
                // Set view state and control state
                this.ViewState = state.First;
				Logger.LogDebug("Load(): this.ViewState → {Arg0}", this.ViewState);
                this.ControlState = state.Second;
            }
			Logger.LogDebug("Load(): this.ControlState → {Arg0}", this.ControlState);

            // Remove this ViewState from the cache as it has served its purpose
            if (!this.Page.IsCallback)
            {
                DataCache.RemoveCache(key);
            }
			Logger.LogDebug("<<< Exiting Load()");
        }

        /// <summary>Saves the Page State to the Cache.</summary>
        public override void Save()
        {
			Logger.LogDebug(">>> Entering Save()");
            // No processing needed if no states available
            if (this.ViewState == null && this.ControlState == null)
            {
                return;
            }

            // Generate a unique cache key
            var key = new StringBuilder();
            {
				Logger.LogWarning("Save(): key → {Key}", key);
                key.Append("VS_");
                key.Append(this.Page.Session == null ? Guid.NewGuid().ToString() : this.Page.Session.SessionID);
                key.Append('_');
                key.Append(DateTime.Now.Ticks);
            }

            // Save view state and control state separately
            var state = new Pair(this.ViewState, this.ControlState);

			Logger.LogDebug("Save(): state → {State}", state);
            // Add view state and control state to cache
            DNNCacheDependency objDependency = null;
            DataCache.SetCache(key.ToString(), state, objDependency, DateTime.Now.AddMinutes(this.Page.Session.Timeout), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);

            // Register hidden field to store cache key in
            this.Page.ClientScript.RegisterHiddenField(ViewStateCacheKey, key.ToString());
			Logger.LogDebug("<<< Exiting Save()");
        }
    }
}
