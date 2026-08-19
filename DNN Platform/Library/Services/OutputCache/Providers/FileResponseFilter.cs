// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers
{
    using System;
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Common.Utilities;

    /// <summary>FileResponseFilter implements <see cref="OutputCacheResponseFilter"/> to capture the response into files.</summary>
    public class FileResponseFilter : OutputCacheResponseFilter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileResponseFilter));
        ////Private _content As StringBuilder
        private DateTime cacheExpiration;

        /// <summary>Initializes a new instance of the <see cref="FileResponseFilter"/> class.</summary>
        /// <param name="itemId">The tab ID.</param>
        /// <param name="maxVaryByCount">The maximum number of values by which the cached response can vary.</param>
        /// <param name="filterChain">The stream to write into the file.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheDuration">The duration for which the response should be cached.</param>
        internal FileResponseFilter(int itemId, int maxVaryByCount, Stream filterChain, string cacheKey, TimeSpan cacheDuration)
            : base(filterChain, cacheKey, cacheDuration, maxVaryByCount)
        {
			Logger.LogDebug(">>> Entering FileResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration})", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration);
            if (maxVaryByCount > -1 && Services.OutputCache.Providers.FileProvider.GetCachedItemCount(itemId) >= maxVaryByCount)
            {
                this.HasErrored = true;
				Logger.LogDebug("FileResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration}): this.HasErrored → {Arg5}", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration, this.HasErrored);
                return;
            }

            this.CachedOutputTempFileName = Services.OutputCache.Providers.FileProvider.GetTempFileName(itemId, cacheKey);
            this.CachedOutputFileName = Services.OutputCache.Providers.FileProvider.GetCachedOutputFileName(itemId, cacheKey);
            this.CachedOutputAttribFileName = Services.OutputCache.Providers.FileProvider.GetAttribFileName(itemId, cacheKey);
            if (File.Exists(this.CachedOutputTempFileName))
            {
                bool fileDeleted = FileSystemUtils.DeleteFileWithWait(this.CachedOutputTempFileName, 100, 200);
                if (fileDeleted == false)
                {
                    this.HasErrored = true;
                }
            }

            if (this.HasErrored == false)
            {
                try
                {
                    this.CaptureStream = new FileStream(this.CachedOutputTempFileName, FileMode.CreateNew, FileAccess.Write);
                }
                catch (Exception)
                {
					Logger.LogError("Exception in FileResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration}): {Arg5}", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration, Exception.Message);
                    this.HasErrored = true;
                    throw;
                }

                this.cacheExpiration = DateTime.UtcNow.Add(cacheDuration);
				Logger.LogDebug("FileResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration}): cacheExpiration → {CacheExpiration}", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration, cacheExpiration);
                this.HasErrored = false;
            }
			Logger.LogDebug("<<< Exiting FileResponseFilter(itemId={ItemId},maxVaryByCount={MaxVaryByCount},filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration})", itemId, maxVaryByCount, filterChain, cacheKey, cacheDuration);
        }

        public string CachedOutputTempFileName { get; set; }

        public string CachedOutputFileName { get; set; }

        public string CachedOutputAttribFileName { get; set; }

        public DateTime CacheExpiration
        {
            get
            {
                return this.cacheExpiration;
            }

            set
            {
                this.cacheExpiration = value;
            }
        }

        /// <inheritdoc />
        public override byte[] StopFiltering(int itemId, bool deleteData)
        {
			Logger.LogDebug(">>> Entering StopFiltering(itemId={ItemId},deleteData={DeleteData})", itemId, deleteData);
            if (this.HasErrored)
            {
                return null;
            }

            if (this.CaptureStream != null)
            {
                this.CaptureStream.Close();

                if (File.Exists(this.CachedOutputFileName))
                {
                    FileSystemUtils.DeleteFileWithWait(this.CachedOutputFileName, 100, 200);
                }

                File.Move(this.CachedOutputTempFileName, this.CachedOutputFileName);

                StreamWriter oWrite = File.CreateText(this.CachedOutputAttribFileName);
				Logger.LogDebug("StopFiltering(itemId={ItemId},deleteData={DeleteData}): oWrite → {OWrite}", itemId, deleteData, oWrite);
                oWrite.WriteLine(this.cacheExpiration.ToString(CultureInfo.InvariantCulture));
                oWrite.Close();
            }

            if (deleteData)
            {
                FileSystemUtils.DeleteFileWithWait(this.CachedOutputFileName, 100, 200);
                FileSystemUtils.DeleteFileWithWait(this.CachedOutputAttribFileName, 100, 200);
            }

			Logger.LogDebug("<<< Exiting StopFiltering(itemId={ItemId},deleteData={DeleteData})", itemId, deleteData);
            return null;
        }
    }
}
