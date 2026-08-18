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
			Logger.LogDebug(">>> Entering DNNCacheDependency(systemCacheDependency={SystemCacheDependency})", systemCacheDependency);
            this.systemCacheDependency = systemCacheDependency;
			Logger.LogDebug("DNNCacheDependency(systemCacheDependency={SystemCacheDependency}): systemCacheDependency → {SystemCacheDependency1}", systemCacheDependency, systemCacheDependency);
			Logger.LogDebug("DNNCacheDependency(systemCacheDependency={SystemCacheDependency}): this.systemCacheDependency → {Arg1}", systemCacheDependency, this.systemCacheDependency);
			Logger.LogDebug("<<< Exiting DNNCacheDependency(systemCacheDependency={SystemCacheDependency})", systemCacheDependency);
        }

        /// <summary>Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.</summary>
        /// <param name="filename">The path to a file which the cache depends on.</param>
        public DNNCacheDependency(string filename)
        {
            this.fileNames = new[] { filename };
			Logger.LogDebug(">>> Entering DNNCacheDependency(filename={Filename})", filename);
			Logger.LogDebug("<<< Exiting DNNCacheDependency(filename={Filename})", filename);
        }

		Logger.LogDebug("<<< Exiting DNNCacheDependency(systemCacheDependency={SystemCacheDependency})", systemCacheDependency);
		Logger.LogDebug(">>> Entering DNNCacheDependency(filename={Filename})", filename);
        /// <summary>Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories) for changes.</summary>
		Logger.LogDebug("<<< Exiting DNNCacheDependency(filenames={Filenames},start={Start})", filenames, start);
        /// <param name="filenames">set the cache depend on multiple files.</param>
        public DNNCacheDependency(string[] filenames)
        {
            this.fileNames = filenames;
			Logger.LogDebug(">>> Entering DNNCacheDependency(filenames={Filenames})", filenames);
			Logger.LogDebug("<<< Exiting DNNCacheDependency(filenames={Filenames})", filenames);
        }

		Logger.LogDebug("<<< Exiting DNNCacheDependency(filenames={Filenames})", filenames);
        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths
        ///  (to files or directories) for changes and specifies a time when change monitoring begins.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string[] filenames, DateTime start)
        {
			Logger.LogDebug(">>> Entering DNNCacheDependency(filenames={Filenames},start={Start})", filenames, start);
            this.utcStart = start.ToUniversalTime();
            this.fileNames = filenames;
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},start={Start}): utcStart → {UtcStart}", filenames, start, utcStart);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors an array of paths (to files or directories),
        /// an array of cache keys, or both for changes.
        /// </summary>
        /// <param name="filenames">The filenames.</param>
        /// <param name="cachekeys">The cachekeys.</param>
        public DNNCacheDependency(string[] filenames, string[] cachekeys)
        {
			Logger.LogDebug(">>> Entering DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys})", filenames, cachekeys);
            this.fileNames = filenames;
            this.cacheKeys = cachekeys;
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys}): this.fileNames → {Arg2}", filenames, cachekeys, this.fileNames);
			Logger.LogDebug("<<< Exiting DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys})", filenames, cachekeys);
        }

        /// <summary>Initializes a new instance of the <see cref="DNNCacheDependency"/> class that monitors a file or directory for changes.</summary>
        /// <param name="filename">The filename.</param>
        /// <param name="start">The start.</param>
        public DNNCacheDependency(string filename, DateTime start)
        {
			Logger.LogDebug(">>> Entering DNNCacheDependency(filename={Filename},start={Start})", filename, start);
            this.utcStart = start.ToUniversalTime();
            if (filename != null)
			Logger.LogDebug("DNNCacheDependency(filename={Filename},start={Start}): utcStart → {UtcStart}", filename, start, utcStart);
			Logger.LogDebug("DNNCacheDependency(filename={Filename},start={Start}): this.fileNames → {Arg2}", filename, start, this.fileNames);
            {
                this.fileNames = new[] { filename };
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
            this.fileNames = filenames;
            this.cacheKeys = cachekeys;
			Logger.LogDebug("DNNCacheDependency(filenames={Filenames},cachekeys={Cachekeys},dependency={Dependency}): this.fileNames → {Arg3}", filenames, cachekeys, dependency, this.fileNames);
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
			Logger.LogDebug(">>> Entering Dispose()");
            this.Dispose(true);
            GC.SuppressFinalize(this);
			Logger.LogDebug("Dispose(disposing={Disposing}): this.cacheDependency → {Arg1}", disposing, this.cacheDependency);
			Logger.LogDebug("<<< Exiting Dispose()");
        }

        // Method that does the actual disposal of resources
        protected virtual void Dispose(bool disposing)
        {
			Logger.LogDebug(">>> Entering Dispose(disposing={Disposing})", disposing);
            if (disposing)
            {
                if (this.cacheDependency != null)
                {
                    this.cacheDependency.Dispose(disposing);
                }

                if (this.systemCacheDependency != null)
                {
                    this.systemCacheDependency.Dispose();
                }

                this.fileNames = null;
				Logger.LogDebug("Dispose(disposing={Disposing}): this.fileNames → {Arg1}", disposing, this.fileNames);
				Logger.LogDebug("Dispose(disposing={Disposing}): cacheDependency → {CacheDependency}", disposing, cacheDependency);
                this.cacheKeys = null;
                this.cacheDependency = null;
				Logger.LogDebug("Dispose(disposing={Disposing}): this.cacheKeys → {Arg1}", disposing, this.cacheKeys);
                this.systemCacheDependency = null;
            }
			Logger.LogDebug("<<< Exiting Dispose(disposing={Disposing})", disposing);
        }
    }
}
