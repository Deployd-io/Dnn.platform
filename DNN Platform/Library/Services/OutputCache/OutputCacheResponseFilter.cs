// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.OutputCache
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    // helper class to capture the response into a file
    [SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Breaking change")]
    public abstract class OutputCacheResponseFilter : Stream
    {
        private Stream captureStream;

        /// <summary>Initializes a new instance of the <see cref="OutputCacheResponseFilter"/> class.</summary>
        /// <param name="filterChain">The stream to write into <see cref="CaptureStream"/>.</param>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="cacheDuration">The duration for which the response should be cached.</param>
        /// <param name="maxVaryByCount">The maximum number of values by which the cached response can vary.</param>
        public OutputCacheResponseFilter(Stream filterChain, string cacheKey, TimeSpan cacheDuration, int maxVaryByCount)
        {
			Logger.LogDebug(">>> Entering OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount})", filterChain, cacheKey, cacheDuration, maxVaryByCount);
            this.ChainedStream = filterChain;
            this.CacheKey = cacheKey;
			Logger.LogDebug("OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount}): this.ChainedStream → {Arg4}", filterChain, cacheKey, cacheDuration, maxVaryByCount, this.ChainedStream);
			Logger.LogDebug("OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount}): this.CacheKey → {Arg4}", filterChain, cacheKey, cacheDuration, maxVaryByCount, this.CacheKey);
            this.CacheDuration = cacheDuration;
            this.MaxVaryByCount = maxVaryByCount;
            this.captureStream = this.CaptureStream;
			Logger.LogDebug("<<< Exiting OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount})", filterChain, cacheKey, cacheDuration, maxVaryByCount);
			Logger.LogDebug("OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount}): this.CacheDuration → {Arg4}", filterChain, cacheKey, cacheDuration, maxVaryByCount, this.CacheDuration);
			Logger.LogDebug("OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount}): this.MaxVaryByCount → {Arg4}", filterChain, cacheKey, cacheDuration, maxVaryByCount, this.MaxVaryByCount);
			Logger.LogDebug("OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount}): captureStream → {CaptureStream}", filterChain, cacheKey, cacheDuration, maxVaryByCount, captureStream);
			Logger.LogDebug("OutputCacheResponseFilter(filterChain={FilterChain},cacheKey={CacheKey},cacheDuration={CacheDuration},maxVaryByCount={MaxVaryByCount}): this.captureStream → {Arg4}", filterChain, cacheKey, cacheDuration, maxVaryByCount, this.captureStream);
        }

        /// <inheritdoc />
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc />
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <inheritdoc />
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public TimeSpan CacheDuration { get; set; }

        public virtual string CacheKey { get; set; }

        public Stream CaptureStream
        {
            get
            {
                return this.captureStream;
            }

            set
            {
                this.captureStream = value;
            }
        }

        public Stream ChainedStream { get; set; }

        public bool HasErrored { get; set; }

        public int MaxVaryByCount { get; set; }

        /// <inheritdoc />
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public override void Flush()
        {
			Logger.LogDebug(">>> Entering Flush()");
            this.ChainedStream.Flush();
            if (this.HasErrored)
            {
                return;
            }

            if (this.captureStream != null)
            {
                this.captureStream.Flush();
            }
			Logger.LogDebug("<<< Exiting Flush()");
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
			Logger.LogDebug(">>> Entering Write(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
            this.ChainedStream.Write(buffer, offset, count);
            if (this.HasErrored)
            {
                return;
            }

            if (this.captureStream != null)
            {
                this.captureStream.Write(buffer, offset, count);
            }
			Logger.LogDebug("<<< Exiting Write(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
			Logger.LogDebug(">>> Entering Read(buffer={Buffer},offset={Offset},count={Count})", buffer, offset, count);
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
			Logger.LogDebug(">>> Entering Seek(offset={Offset},origin={Origin})", offset, origin);
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
			Logger.LogDebug(">>> Entering SetLength(value={Value})", value);
            throw new NotSupportedException();
        }

        public virtual byte[] StopFiltering(int itemId, bool deleteData)
        {
			Logger.LogDebug(">>> Entering StopFiltering(itemId={ItemId},deleteData={DeleteData})", itemId, deleteData);
            if (this.HasErrored)
            {
                return null;
            }

            if (this.CaptureStream != null)
            {
                this.CaptureStream.Position = 0;
                using (var reader = new StreamReader(this.CaptureStream, Encoding.Default))
                {
                    string output = reader.ReadToEnd();
                    this.AddItemToCache(itemId, output);
                }

                this.CaptureStream.Close();
                this.CaptureStream = null;
				Logger.LogDebug("StopFiltering(itemId={ItemId},deleteData={DeleteData}): this.CaptureStream → {Arg2}", itemId, deleteData, this.CaptureStream);
            }

            if (deleteData)
            {
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                this.RemoveItemFromCache(itemId);
            }

			Logger.LogDebug("<<< Exiting StopFiltering(itemId={ItemId},deleteData={DeleteData})", itemId, deleteData);
            return null;
        }

        protected virtual void AddItemToCache(int itemId, string output)
        {
		Logger.LogDebug(">>> Entering AddItemToCache(itemId={ItemId},output={Output})", itemId, output);
		Logger.LogDebug("<<< Exiting AddItemToCache(itemId={ItemId},output={Output})", itemId, output);
        }

        protected virtual void RemoveItemFromCache(int itemId)
        {
		Logger.LogDebug(">>> Entering RemoveItemFromCache(itemId={ItemId})", itemId);
		Logger.LogDebug("<<< Exiting RemoveItemFromCache(itemId={ItemId})", itemId);
        }
    }
}
