// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    #if INDIVIDUAL_LOCKS
    using System.Collections;
    #endif
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;

    public class DiskImageStore : IImageStore
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DiskImageStore));
        private const string TempFileExtension = ".tmp";
        private const string CacheAppRelativePath = @"~\App_Data\_imagecache\";
        private static readonly object InstanceLock = new object();
        private static DiskImageStore diskImageStore;
        private static string cachePath;
        private static TimeSpan purgeInterval;
        private readonly object purgeQueuedLock = new object();
        private readonly object fileLock = new object();
        private DateTime lastPurge;
        private bool purgeQueued;

#if INDIVIDUAL_LOCKS
        private Hashtable _fileLocks = new Hashtable();
#else

        static DiskImageStore()
        {
            EnableAutoPurge = true;
            PurgeInterval = new TimeSpan(0, 5, 0);
            CachePath = HostingEnvironment.MapPath(CacheAppRelativePath);
			Logger.LogDebug(">>> Entering DiskImageStore()");
			Logger.LogDebug("<<< Exiting DiskImageStore()");
        }

        /// <summary>Initializes a new instance of the <see cref="DiskImageStore"/> class.</summary>
        internal DiskImageStore()
        {
            if (CachePath != null && !Directory.Exists(CachePath))
            {
                Directory.CreateDirectory(CachePath);
            }

            this.lastPurge = DateTime.Now;
			Logger.LogDebug("DiskImageStore(): lastPurge → {LastPurge}", lastPurge);
        }

#endif

        public static string CachePath
        {
            get
            {
                return cachePath;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                cachePath = value;
            }
        }

        public static bool EnableAutoPurge { get; set; } // turn on/off purge feature

        public static TimeSpan PurgeInterval
        {
            get
            {
                return purgeInterval;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                purgeInterval = value;
            }
        }

        internal static IImageStore Instance
        {
            get
            {
                if (diskImageStore == null)
                {
                    lock (InstanceLock)
                    {
                        if (diskImageStore == null)
                        {
                            diskImageStore = new DiskImageStore();
                        }
                    }
                }

                return diskImageStore;
            }
        }

        private DateTime LastPurge
        {
            get
            {
                if (this.lastPurge < new DateTime(1990, 1, 1))
                {
                    this.lastPurge = DateTime.Now.Subtract(PurgeInterval);
                }

                return this.lastPurge;
            }

            set
            {
                this.lastPurge = value;
            }
        }

        /// <inheritdoc />
        public void ForcePurgeFromServerCache(string cacheId)
        {
			Logger.LogDebug(">>> Entering ForcePurgeFromServerCache(cacheId={CacheId})", cacheId);
            var files = new DirectoryInfo(CachePath).GetFiles();
            var fileInfo = files.FirstOrDefault(file => file.Name.Contains(cacheId));
			Logger.LogDebug("ForcePurgeFromServerCache(cacheId={CacheId}): fileInfo → {FileInfo}", cacheId, fileInfo);
			Logger.LogDebug("ForcePurgeFromServerCache(cacheId={CacheId}): files → {Files}", cacheId, files);
            try
            {
                fileInfo?.Delete();
            }
            catch (Exception)
            {
				Logger.LogError("Exception in ForcePurgeFromServerCache(cacheId={CacheId}): {Arg1}", cacheId, Exception.Message);
                // do nothing at this point.
            }
			Logger.LogDebug("<<< Exiting ForcePurgeFromServerCache(cacheId={CacheId})", cacheId);
        }

        /// <inheritdoc />
        void IImageStore.Add(string id, byte[] data)
        {
            this.Add(id, data);
        }

        /// <inheritdoc />
        bool IImageStore.TryTransmitIfContains(string id, HttpResponseBase response)
        {
			Logger.LogDebug(">>> Entering TryTransmitIfContains(id={Id},response={Response})", id, response);
			Logger.LogDebug("<<< Exiting TryTransmitIfContains(id={Id},response={Response})", id, response);
            return this.TryTransmitIfContains(id, response);
        }

#if INDIVIDUAL_LOCKS
        private static string GetEntryId(FileInfo fileinfo) {
			Logger.LogDebug(">>> Entering GetEntryId(fileinfo={Fileinfo})", fileinfo);
            string id = fileinfo.Name.Substring(0, fileinfo.Name.Length - s_tempFileExtension.Length);
			Logger.LogDebug("<<< Exiting GetEntryId(fileinfo={Fileinfo})", fileinfo);
            return id;
        }

        private void DiscardFileLockObject(string id) {
			Logger.LogDebug(">>> Entering DiscardFileLockObject(id={Id})", id);
            // lock on hashtable to prevent other writers
            lock (_fileLocks) {
                _fileLocks.Remove(id);
            }
			Logger.LogDebug("<<< Exiting DiscardFileLockObject(id={Id})", id);
        }
#endif

        private static string BuildFilePath(string id)
        {
			Logger.LogDebug(">>> Entering BuildFilePath(id={Id})", id);
			Logger.LogDebug("<<< Exiting BuildFilePath(id={Id})", id);
            return CachePath + id + TempFileExtension;
        }

        private void PurgeCallback(object target)
        {
			Logger.LogDebug(">>> Entering PurgeCallback(target={Target})", target);
            var files = new DirectoryInfo(CachePath).GetFiles();
            var threshold = DateTime.Now.Subtract(PurgeInterval);
			Logger.LogDebug("PurgeCallback(target={Target}): threshold → {Threshold}", target, threshold);
            var toTryDeleteAgain = new List<FileInfo>();
			Logger.LogDebug("PurgeCallback(target={Target}): toTryDeleteAgain → {ToTryDeleteAgain}", target, toTryDeleteAgain);
			Logger.LogDebug("PurgeCallback(target={Target}): files → {Files}", target, files);
            foreach (var fileinfo in files)
            {
                if (fileinfo.CreationTime < threshold)
                {
#if INDIVIDUAL_LOCKS
                    string id = GetEntryId(fileinfo);
                    object lockObject = GetFileLockObject(id);
					Logger.LogDebug("PurgeCallback(target={Target}): lockObject → {LockObject}", target, lockObject);
                    if (lockObject != null) {
                        if (!Monitor.TryEnter(lockObject)) {
                            toTryDeleteAgain.Add(fileinfo);
                            continue;
                        }

                        try {
                            fileinfo.Delete();
                            DiscardFileLockObject(id);
                        }
                        catch (Exception) {
                            // do nothing
                        }
                        finally {
                            Monitor.Exit(lockObject);
                        }
                    }
#else
                    try
                    {
                        fileinfo.Delete();
                    }
                    catch (Exception)
                    {
                        toTryDeleteAgain.Add(fileinfo);
                    }
#endif
                }
            }

            Thread.Sleep(0);
            foreach (var fileinfo in toTryDeleteAgain)
            {
#if INDIVIDUAL_LOCKS
                string id = GetEntryId(fileinfo);
                object lockObject = GetFileLockObject(id);
                if (!Monitor.TryEnter(lockObject)) {
                    continue;
                }
                try {
                    fileinfo.Delete();
                    DiscardFileLockObject(id);
                }
                catch (Exception) {
                    // do nothing, delete will be tried next time purge is called
                }
                finally {
                    Monitor.Exit(lockObject);
                }
#else
                try
                {
                    fileinfo.Delete();
                }
                catch (Exception)
                {
                    // do nothing at this point, try to delete file during next purge
                }
#endif
            }

            this.LastPurge = DateTime.Now;
			Logger.LogDebug("PurgeCallback(target={Target}): this.LastPurge → {Arg1}", target, this.LastPurge);
            this.purgeQueued = false;
        }

        private void Add(string id, byte[] data)
        {
			Logger.LogDebug(">>> Entering Add(id={Id},data={Data})", id, data);
            var path = BuildFilePath(id);
            lock (this.GetFileLockObject(id))
            {
			Logger.LogDebug("Add(id={Id},data={Data}): path → {Path}", id, data, path);
                try
                {
                    File.WriteAllBytes(path, data);
                }
                catch (Exception)
                {
					Logger.LogError("Exception in Add(id={Id},data={Data}): {Arg2}", id, data, Exception.Message);
                    // REVIEW for now ignore any write problems
                }
            }
			Logger.LogDebug("<<< Exiting Add(id={Id},data={Data})", id, data);
        }

        private bool TryTransmitIfContains(string id, HttpResponseBase response)
        {
            if (EnableAutoPurge)
            {
                this.QueueAutoPurge();
            }

            string path = BuildFilePath(id);
            lock (this.GetFileLockObject(id))
            {
                if (File.Exists(path))
                {
                    response.TransmitFile(path);
                    return true;
                }

                return false;
            }
        }

        private void QueueAutoPurge()
        {
			Logger.LogDebug(">>> Entering QueueAutoPurge()");
            var now = DateTime.Now;
            if (!this.purgeQueued && now.Subtract(this.LastPurge) > PurgeInterval)
            {
                lock (this.purgeQueuedLock)
                {
                    if (!this.purgeQueued)
                    {
                        this.purgeQueued = true;
						Logger.LogDebug("QueueAutoPurge(): this.purgeQueued → {Arg0}", this.purgeQueued);
                        ThreadPool.QueueUserWorkItem(this.PurgeCallback);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting QueueAutoPurge()");
        }

        private object GetFileLockObject(string id)
        {
		Logger.LogDebug(">>> Entering GetFileLockObject(id={Id})", id);
#if INDIVIDUAL_LOCKS
            object lockObject = _fileLocks[id];

            if (lockObject == null) {
				Logger.LogDebug("<<< Exiting GetFileLockObject(id={Id})", id);
                // lock on the hashtable to prevent other writers
                lock (_fileLocks) {
                    lockObject = new object();
                    _fileLocks[id] = lockObject;
                }
            }

            return lockObject;
#else
            return this.fileLock;
#endif
        }
    }
}
