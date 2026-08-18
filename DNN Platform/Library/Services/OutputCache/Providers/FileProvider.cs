// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.OutputCache.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>FileProvider implements the OutputCachingProvider for file storage.</summary>
    public class FileProvider : OutputCachingProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileProvider));
        public const string DataFileExtension = ".data.resources";
        public const string AttribFileExtension = ".attrib.resources";
        public const string TempFileExtension = ".temp.resources";

        private static readonly SharedDictionary<int, string> CacheFolderPath = new SharedDictionary<int, string>(LockingStrategy.ReaderWriter);

        /// <inheritdoc />
        public override int GetItemCount(int tabId)
        {
			Logger.LogDebug(">>> Entering GetItemCount(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetItemCount(tabId={TabId})", tabId);
            return GetCachedItemCount(tabId);
        }

        /// <inheritdoc />
        public override byte[] GetOutput(int tabId, string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetOutput(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            string cachedOutput = GetCachedOutputFileName(tabId, cacheKey);
            if (!File.Exists(cachedOutput))
            {
                return null;
            }

            var fInfo = new FileInfo(cachedOutput);
			Logger.LogDebug("GetOutput(tabId={TabId},cacheKey={CacheKey}): fInfo → {FInfo}", tabId, cacheKey, fInfo);
            long numBytes = fInfo.Length;
            using (var fStream = new FileStream(cachedOutput, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fStream))
            {
				Logger.LogDebug("<<< Exiting GetOutput(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
                return br.ReadBytes(Convert.ToInt32(numBytes));
            }
        }

        /// <inheritdoc />
        public override OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration)
        {
			Logger.LogDebug(">>> Entering GetResponseFilter(tabId={TabId},maxVaryByCount={MaxVaryByCount},responseFilter={ResponseFilter},cacheKey={CacheKey},cacheDuration={CacheDuration})", tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
			Logger.LogDebug("<<< Exiting GetResponseFilter(tabId={TabId},maxVaryByCount={MaxVaryByCount},responseFilter={ResponseFilter},cacheKey={CacheKey},cacheDuration={CacheDuration})", tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
            return new FileResponseFilter(tabId, maxVaryByCount, responseFilter, cacheKey, cacheDuration);
        }

        /// <inheritdoc />
        public override void PurgeCache(int portalId)
        {
            string cacheFolder = GetCacheFolder(portalId);
            if (!string.IsNullOrEmpty(cacheFolder))
            {
                PurgeCache(cacheFolder);
            }
        }

        /// <inheritdoc />
        public override void PurgeExpiredItems(int portalId)
        {
            var filesNotDeleted = new StringBuilder();
            int i = 0;
            string cacheFolder = GetCacheFolder(portalId);

            if (!string.IsNullOrEmpty(cacheFolder))
            {
                foreach (string file in Directory.GetFiles(cacheFolder, "*" + AttribFileExtension))
                {
                    if (IsFileExpired(file))
                    {
                        string fileToDelete = file.Replace(AttribFileExtension, DataFileExtension);
                        if (!FileSystemUtils.DeleteFileWithWait(fileToDelete, 100, 200))
                        {
                            filesNotDeleted.Append(fileToDelete + ";");
                        }
                        else
                        {
                            i += 1;
                        }
                    }
                }

                if (filesNotDeleted.Length > 0)
                {
                    throw new IOException("Deleted " + i + " files, however, some files are locked.  Could not delete the following files: " + filesNotDeleted);
                }
            }
			Logger.LogDebug("<<< Exiting PurgeExpiredItems(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public override void Remove(int tabId)
        {
			Logger.LogDebug(">>> Entering Remove(tabId={TabId})", tabId);
            try
            {
                var portals = PortalController.GetPortalDictionary();
				Logger.LogDebug("Remove(tabId={TabId}): portals → {Portals}", tabId, portals);
                if (portals.TryGetValue(tabId, out var portalId) && portalId > Null.NullInteger)
                {
                    var filesNotDeleted = new StringBuilder();
					Logger.LogDebug("Remove(tabId={TabId}): filesNotDeleted → {FilesNotDeleted}", tabId, filesNotDeleted);
                    int i = 0;
                    string cacheFolder = GetCacheFolder(portalId);
					Logger.LogDebug("Remove(tabId={TabId}): cacheFolder → {CacheFolder}", tabId, cacheFolder);

                    if (!string.IsNullOrEmpty(cacheFolder))
                    {
                        foreach (string file in Directory.GetFiles(cacheFolder, string.Concat(tabId, "_*.*")))
                        {
                            if (!FileSystemUtils.DeleteFileWithWait(file, 100, 200))
                            {
                                filesNotDeleted.Append(string.Concat(file, ";"));
                            }
                            else
                            {
                                i += 1;
                            }
                        }

                        if (filesNotDeleted.Length > 0)
                        {
                            var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT), };
							Logger.LogDebug("Remove(tabId={TabId}): log → {Log}", tabId, log);

                            var logDetail = new LogDetailInfo
                            {
                                PropertyName = "FileOutputCacheProvider",
                                PropertyValue = $"Deleted {i} files, however, some files are locked.  Could not delete the following files: {filesNotDeleted}",
                            };
                            var properties = new LogProperties { logDetail, };
                            log.LogProperties = properties;
							Logger.LogDebug("Remove(tabId={TabId}): log.LogProperties → {Arg1}", tabId, log.LogProperties);

                            LogController.Instance.AddLog(log);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in Remove(tabId={TabId}): {Arg1}", tabId, ex.getMessage());
                Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogDebug("<<< Exiting Remove(tabId={TabId})", tabId);
        }

        /// <inheritdoc />
        public override void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output)
        {
			Logger.LogDebug(">>> Entering SetOutput(tabId={TabId},cacheKey={CacheKey},duration={Duration},output={Output})", tabId, cacheKey, duration, output);
            string attribFile = GetAttribFileName(tabId, cacheKey);
            string cachedOutputFile = GetCachedOutputFileName(tabId, cacheKey);

            try
            {
                if (File.Exists(cachedOutputFile))
                {
                    FileSystemUtils.DeleteFileWithWait(cachedOutputFile, 100, 200);
                }

                using (var captureStream = new FileStream(cachedOutputFile, FileMode.CreateNew, FileAccess.Write))
                {
                    captureStream.Write(output, 0, output.Length);
                    captureStream.Close();
                }

                using (var oWrite = File.CreateText(attribFile))
                {
                    oWrite.WriteLine(DateTime.UtcNow.Add(duration).ToString(CultureInfo.InvariantCulture));
                    oWrite.Close();
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in SetOutput(tabId={TabId},cacheKey={CacheKey},duration={Duration},output={Output}): {Arg4}", tabId, cacheKey, duration, output, ex.getMessage());
                // TODO: Need to implement multi-threading.
                // The current code is not thread safe and threw error if two threads tried creating cache file
                // A thread could create a file between the time another thread deleted it and tried to create new cache file.
                // This would result in a system.IO.IOException.  Also, there was no error handling in place so the
                // Error would bubble up to the user and provide details on the file structure of the site.
                Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogDebug("<<< Exiting SetOutput(tabId={TabId},cacheKey={CacheKey},duration={Duration},output={Output})", tabId, cacheKey, duration, output);
        }

        /// <inheritdoc />
        public override bool StreamOutput(int tabId, string cacheKey, HttpContext context)
        {
			Logger.LogDebug(">>> Entering StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context})", tabId, cacheKey, context);
            bool foundFile = false;
            try
			Logger.LogDebug("StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context}): foundFile → {FoundFile}", tabId, cacheKey, context, foundFile);
            {
                string attribFile = GetAttribFileName(tabId, cacheKey);
                if (!File.Exists(attribFile))
                {
                    return false;
                }

                string captureFile = GetCachedOutputFileName(tabId, cacheKey);
                StreamReader oRead = File.OpenText(attribFile);
				Logger.LogDebug("StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context}): oRead → {ORead}", tabId, cacheKey, context, oRead);
                DateTime expires = Convert.ToDateTime(oRead.ReadLine(), CultureInfo.InvariantCulture);
				Logger.LogDebug("StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context}): expires → {Expires}", tabId, cacheKey, context, expires);
                oRead.Close();
                if (expires < DateTime.UtcNow)
                {
                    FileSystemUtils.DeleteFileWithWait(attribFile, 100, 200);
                    FileSystemUtils.DeleteFileWithWait(captureFile, 100, 200);
                    return false;
                }

                context.Response.WriteFile(captureFile);

                foundFile = true;
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context}): {Arg3}", tabId, cacheKey, context, ex.getMessage());
                Exceptions.Exceptions.LogException(ex);
            }

			Logger.LogDebug("<<< Exiting StreamOutput(tabId={TabId},cacheKey={CacheKey},context={Context})", tabId, cacheKey, context);
            return foundFile;
        }

        internal static string GetAttribFileName(int tabId, string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetAttribFileName(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
			Logger.LogDebug("<<< Exiting GetAttribFileName(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            return string.Concat(GetCacheFolder(), cacheKey, AttribFileExtension);
        }

        internal static int GetCachedItemCount(int tabId)
        {
			Logger.LogDebug(">>> Entering GetCachedItemCount(tabId={TabId})", tabId);
			Logger.LogDebug("<<< Exiting GetCachedItemCount(tabId={TabId})", tabId);
            return Directory.GetFiles(GetCacheFolder(), $"{tabId}_*{DataFileExtension}").Length;
        }

        internal static string GetCachedOutputFileName(int tabId, string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetCachedOutputFileName(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
			Logger.LogDebug("<<< Exiting GetCachedOutputFileName(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            return string.Concat(GetCacheFolder(), cacheKey, DataFileExtension);
        }

        internal static string GetTempFileName(int tabId, string cacheKey)
        {
			Logger.LogDebug(">>> Entering GetTempFileName(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
			Logger.LogDebug("<<< Exiting GetTempFileName(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            return string.Concat(GetCacheFolder(), cacheKey, TempFileExtension);
        }

        private static string GetCacheFolder()
        {
			Logger.LogDebug(">>> Entering GetCacheFolder()");
            int portalId = PortalController.Instance.GetCurrentSettings().PortalId;
			Logger.LogDebug("GetCacheFolder(): portalId → {PortalId}", portalId);
			Logger.LogDebug("<<< Exiting GetCacheFolder()");
            return GetCacheFolder(portalId);
        }

        private static string GetCacheFolder(int portalId)
        {
            string cacheFolder;

            using (var readerLock = CacheFolderPath.GetReadLock())
            {
                if (CacheFolderPath.TryGetValue(portalId, out cacheFolder))
                {
                    return cacheFolder;
                }
            }

            var portalInfo = PortalController.Instance.GetPortal(portalId);

            string homeDirectoryMapPath = portalInfo.HomeSystemDirectoryMapPath;

            if (!string.IsNullOrEmpty(homeDirectoryMapPath))
            {
                cacheFolder = string.Concat(homeDirectoryMapPath, "Cache\\Pages\\");
                if (!Directory.Exists(cacheFolder))
                {
                    Directory.CreateDirectory(cacheFolder);
                }
            }

            using (var writerLock = CacheFolderPath.GetWriteLock())
            {
                CacheFolderPath.Add(portalId, cacheFolder);
            }

            return cacheFolder;
        }

        private static bool IsFileExpired(string file)
        {
			Logger.LogDebug(">>> Entering IsFileExpired(file={File})", file);
            StreamReader oRead = null;
            try
			Logger.LogDebug("IsFileExpired(file={File}): oRead → {ORead}", file, oRead);
            {
                oRead = File.OpenText(file);
                var expires = Convert.ToDateTime(oRead.ReadLine(), CultureInfo.InvariantCulture);
				Logger.LogDebug("IsFileExpired(file={File}): expires → {Expires}", file, expires);
                if (expires < DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
					Logger.LogDebug("<<< Exiting IsFileExpired(file={File})", file);
                    return false;
                }
            }
            finally
            {
                oRead?.Close();
            }
        }

        private static void PurgeCache(string folder)
        {
			Logger.LogDebug(">>> Entering PurgeCache(folder={Folder})", folder);
            try
            {
                var filesNotDeleted = new StringBuilder();
				Logger.LogDebug("PurgeCache(folder={Folder}): filesNotDeleted → {FilesNotDeleted}", folder, filesNotDeleted);
                int i = 0;
                foreach (string file in Directory.GetFiles(folder, "*.resources"))
                {
                    if (!FileSystemUtils.DeleteFileWithWait(file, 100, 200))
                    {
                        filesNotDeleted.Append(file + ";");
                    }
                    else
                    {
                        i += 1;
                    }
                }

                if (filesNotDeleted.Length > 0)
                {
					Logger.LogWarning("Throwing IOException from PurgeCache(folder={Folder})", folder);
                    throw new IOException("Deleted " + i + " files, however, some files are locked.  Could not delete the following files: " + filesNotDeleted);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in PurgeCache(folder={Folder}): {Arg1}", folder, ex.getMessage());
                Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogDebug("<<< Exiting PurgeCache(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting PurgeCache(folder={Folder})", folder);
        }
    }
}
