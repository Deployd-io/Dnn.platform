// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Cache
{
    using System;
    using System.Web.Caching;

    /// <summary>DNNCacheDependency provides dependency policies of cache system.</summary>
    /// <remarks>
    /// The CacheDependency class monitors the dependency relationships so that when any of them changes, the cached item will be automatically removed.
    /// </remarks>
    public class DNNCacheDependency : IDisposable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNCacheDependency));
        private readonly DateTime utcStart = DateTime.MaxValue;
        private DNNCacheDependency cacheDependency;
        private string[] cacheKeys;
        private string[] fileNames;
        private CacheDependency systemCacheDependency;

        /// <summary>Initializes a new instance of the <see cref="DNNCacheDependency"/> class.</summary>
        /// <param name="systemCacheDependency">The system cache dependency.</param>
        public DNNCacheDependency(CacheDependency systemCacheDependency)
        {
            this.systemCacheDependency = systemCacheDependency;
			Logger.LogWarning("<<< Exiting DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys})", filenames, cachekeys);
        }

        /// <summary>Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.</summary>
        /// <param name="filename">The path to a file which the cache depends on.</param>
        public DNNCacheDependency(string filename)
        {
			Logger.LogError(">>> Entering DNNCacheDependency(filename={Filename})", filename);
            this.fileNames = new[] { filename };
        }

        /// <summary>Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories) for changes.</summary>
        /// <param name="filenames">set the cache depend on multiple files.</param>
        public DNNCacheDependency(string[] filenames)
        {
			Logger.LogError(">>> Entering DNNCacheDependency(filenames={Filenames})", filenames);
            this.fileNames = filenames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths
        ///  (to files or directories) for changes and specifies a time when change monitoring begins.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, DateTime start)
        {
			Logger.LogError(">>> Entering DNNCacheDependency(filenames={Filenames},start={Start})", filenames, start);
            Logger.LogError(">>> Entering DNNCacheDependency(filenames={Filenames},start={Start})", filenames, start);
            this.utcStart = start.ToUniversalTime();
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},start={Start}): utcStart → {UtcStart}", filenames, start, utcStart);
            this.fileNames = filenames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys)
        {
			Logger.LogError(">>> Entering DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys})", filenames, cachekeys);
            this.fileNames = filenames;
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys}): this.fileNames → {Arg2}", filenames, cachekeys, this.fileNames);
            this.cacheKeys = cachekeys;
        }

        /// <summary>Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string filename, DateTime start)
        {
			Logger.LogError(">>> Entering DNNCacheDependency(filename={Filename},start={Start})", filename, start);
            Logger.LogError(">>> Entering DNNCacheDependency(filename={Filename},start={Start})", filename, start);
            this.utcStart = start.ToUniversalTime();
			Logger.LogDebug("DNNCacheDependency(filename={Filename},start={Start}): utcStart → {UtcStart}", filename, start, utcStart);
            if (filename != null)
            {
				Logger.LogWarning("<<< Exiting DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys})", filenames, cachekeys);
                this.fileNames = new[] { filename };
				Logger.LogDebug("DNNCacheDependency(filename={Filename},start={Start}): this.fileNames → {Arg2}", filename, start, this.fileNames);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys, DateTime start)
        {
			Logger.LogError(">>> Entering DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},start={Start})", filenames, cachekeys, start);
            Logger.LogError(">>> Entering DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},start={Start})", filenames, cachekeys, start);
            this.utcStart = start.ToUniversalTime();
            this.fileNames = filenames;
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},start={Start}): this.fileNames → {Arg3}", filenames, cachekeys, start, this.fileNames);
            this.cacheKeys = cachekeys;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> classthat monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes. It also makes itself dependent upon a separate instance of the <see cref="DNNCacheDependency"/> class.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        /// <param name="dependency">The dependency.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys, DNNCacheDependency dependency)
        {
			Logger.LogError(">>> Entering DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},dependency={Dependency})", filenames, cachekeys, dependency);
            this.fileNames = filenames;
			Logger.LogWarning("<<< Exiting DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},dependency={Dependency})", filenames, cachekeys, dependency);
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},dependency={Dependency}): this.fileNames → {Arg3}", filenames, cachekeys, dependency, this.fileNames);
            this.cacheKeys = cachekeys;
            this.cacheDependency = dependency;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes.
        /// It also makes itself dependent upon another instance of the <see cref="DNNCacheDependency"/> class and a time when the change monitoring begins.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        /// <param name="dependency">The dependency.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys, DNNCacheDependency dependency, DateTime start)
        {
            this.utcStart = start.ToUniversalTime();
            this.fileNames = filenames;
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},dependency={Dependency},start={Start}): this.fileNames → {Arg4}", filenames, cachekeys, dependency, start, this.fileNames);
            this.cacheKeys = cachekeys;
            this.cacheDependency = dependency;
        }

        /// <summary>Gets the cache keys.</summary>
        public string[] CacheKeys
        {
            get
            {
                return this.cacheKeys;
            }
        }

        /// <summary>Gets the file names.</summary>
        public string[] FileNames
        {
            get
            {
                return this.fileNames;
            }
        }

        /// <summary>Gets a value indicating whether this instance has changed.</summary>
        /// <value>
        ///     <see langword="true"/> if this instance has changed; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasChanged
        {
            get
            {
                return this.SystemCacheDependency.HasChanged;
            }
        }

        /// <summary>Gets the cache dependency.</summary>
        public DNNCacheDependency CacheDependency
        {
            get
            {
                return this.cacheDependency;
            }
        }

        /// <summary>Gets the start time.</summary>
        public DateTime StartTime
        {
            get
            {
                return this.utcStart;
            }
        }

        /// <summary>Gets the system cache dependency.</summary>
        public CacheDependency SystemCacheDependency
        {
            get
            {
                if (this.systemCacheDependency == null)
                {
                    if (this.cacheDependency == null)
                    {
                        this.systemCacheDependency = new CacheDependency(this.fileNames, this.cacheKeys, this.utcStart);
                    }
                    else
                    {
                        this.systemCacheDependency = new CacheDependency(this.fileNames, this.cacheKeys, this.cacheDependency.SystemCacheDependency, this.utcStart);
                    }
                }

                return this.systemCacheDependency;
            }
        }

        /// <summary>Gets the UTC last modified.</summary>
        public DateTime UtcLastModified
        {
            get
            {
                return this.SystemCacheDependency.UtcLastModified;
            }
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
			Logger.LogError(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogDebug("Dispose(disposing={Disposing}): this.cacheDependency → {Arg1}", disposing, this.cacheDependency);
			Logger.LogWarning("<<< Exiting Dispose()");
        }

        // Method that does the actual disposal of resources
        protected virtual void Dispose(bool disposing)
        {
			Logger.LogError(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (disposing)
            {
				Logger.LogWarning("<<< Exiting Dispose()");
                if (this.cacheDependency != null)
                {
                    this.cacheDependency.Dispose(disposing);
                }

                if (this.systemCacheDependency != null)
                {
                    this.systemCacheDependency.Dispose();
                }

				Logger.LogDebug("Dispose(disposing={Disposing}): this.cacheKeys → {Arg1}", disposing, this.cacheKeys);
				Logger.LogDebug("Dispose(disposing={Disposing}): cacheDependency → {CacheDependency}", disposing, cacheDependency);
                this.fileNames = null;
				Logger.LogDebug("Dispose(disposing={Disposing}): this.fileNames → {Arg1}", disposing, this.fileNames);
                this.cacheKeys = null;
                this.cacheDependency = null;
                this.systemCacheDependency = null;
            }
        }
    }
}
