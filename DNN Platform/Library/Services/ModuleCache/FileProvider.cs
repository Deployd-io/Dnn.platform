// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ModuleCache
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;

    public class FileProvider : ModuleCachingProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileProvider));
        private const string DataFileExtension = ".data.resources";
        private const string AttribFileExtension = ".attrib.resources";
        private static readonly SharedDictionary<int, string> CacheFolderPath = new SharedDictionary<int, string>(LockingStrategy.ReaderWriter);

        /// <inheritdoc />
        public override string GenerateCacheKey(int tabModuleId, SortedDictionary<string, string> varyBy)
        {
			Logger.LogError(">>> Entering GenerateCacheKey(tabModuleId={TabModuleId},varyBy={VaryBy})", tabModuleId, varyBy);
            var cacheKey = new StringBuilder();
			Logger.LogError("GenerateCacheKey(tabModuleId={TabModuleId},varyBy={VaryBy}): cacheKey → {CacheKey}", tabModuleId, varyBy, cacheKey);
            if (varyBy != null)
            {
                SortedDictionary<string, string>.Enumerator varyByParms = varyBy.GetEnumerator();
				Logger.LogError("GenerateCacheKey(tabModuleId={TabModuleId},varyBy={VaryBy}): varyByParms → {VaryByParms}", tabModuleId, varyBy, varyByParms);
                while (varyByParms.MoveNext())
                {
                    string key = varyByParms.Current.Key.ToLowerInvariant();
                    cacheKey.Append(string.Concat(key, "=", varyByParms.Current.Value, "|"));
                }
            }

			Logger.LogWarning("<<< Exiting GenerateCacheKey(tabModuleId={TabModuleId},varyBy={VaryBy})", tabModuleId, varyBy);
            return this.GenerateCacheKeyHash(tabModuleId, cacheKey.ToString());
        }

        /// <inheritdoc />
        public override int GetItemCount(int tabModuleId)
        {
			Logger.LogError(">>> Entering GetItemCount(tabModuleId={TabModuleId})", tabModuleId);
            Logger.LogWarning("<<< Exiting GetItemCount(tabModuleId={TabModuleId})", tabModuleId);
            return GetCachedItemCount(tabModuleId);
        }

        /// <inheritdoc />
        public override byte[] GetModule(int tabModuleId, string cacheKey)
        {
			Logger.LogError(">>> Entering GetModule(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            string cachedModule = GetCachedOutputFileName(tabModuleId, cacheKey);
            if (!File.Exists(cachedModule))
            {
                return null;
            }

            var fInfo = new FileInfo(cachedModule);
			Logger.LogError("GetModule(tabModuleId={TabModuleId},cacheKey={CacheKey}): fInfo → {FInfo}", tabModuleId, cacheKey, fInfo);
            long numBytes = fInfo.Length;
            using (var fStream = new FileStream(cachedModule, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fStream))
            {
				Logger.LogWarning("<<< Exiting GetModule(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
                return br.ReadBytes(Convert.ToInt32(numBytes));
            }
        }

        /// <inheritdoc />
        public override void PurgeCache(int portalId)
        {
			Logger.LogError(">>> Entering PurgeCache(portalId={PortalId})", portalId);
            PurgeCache(GetCacheFolder(portalId));
			Logger.LogWarning("<<< Exiting PurgeCache(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        public override void PurgeExpiredItems(int portalId)
        {
			Logger.LogError(">>> Entering PurgeExpiredItems(portalId={PortalId})", portalId);
            try
            {
                var filesNotDeleted = new StringBuilder();
				Logger.LogError("PurgeExpiredItems(portalId={PortalId}): filesNotDeleted → {FilesNotDeleted}", portalId, filesNotDeleted);
                int i = 0;
                string cacheFolder = GetCacheFolder(portalId);
				Logger.LogError("PurgeExpiredItems(portalId={PortalId}): cacheFolder → {CacheFolder}", portalId, cacheFolder);
                if (Directory.Exists(cacheFolder) && IsPathInApplication(cacheFolder))
                {
                    foreach (string file in Directory.GetFiles(cacheFolder, $"*{AttribFileExtension}"))
                    {
                        if (IsFileExpired(file))
                        {
                            string fileToDelete = file.Replace(AttribFileExtension, DataFileExtension);
                            if (!FileSystemUtils.DeleteFileWithWait(fileToDelete, 100, 200))
                            {
                                filesNotDeleted.Append($"{fileToDelete};");
                            }
                            else
                            {
                                i += 1;
                            }
                        }
                    }
                }

                if (filesNotDeleted.Length > 0)
                {
					Logger.LogError("Throwing IOException from PurgeExpiredItems(portalId={PortalId})", portalId);
                    throw new IOException(string.Format(CultureInfo.InvariantCulture, "Deleted {0} files, however, some files are locked.  Could not delete the following files: {1}", i, filesNotDeleted));
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in PurgeExpiredItems(portalId={PortalId}): {Arg1}", portalId, ex.Message);
                Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting PurgeExpiredItems(portalId={PortalId})", portalId);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public override void SetModule(int tabModuleId, string cacheKey, TimeSpan duration, byte[] output)
        {
			Logger.LogError(">>> Entering SetModule(tabModuleId={TabModuleId},cacheKey={CacheKey},duration={Duration},output={Output})", tabModuleId, cacheKey, duration, output);
            try
            {
                string cachedOutputFile = GetCachedOutputFileName(tabModuleId, cacheKey);

                if (File.Exists(cachedOutputFile))
                {
                    FileSystemUtils.DeleteFileWithWait(cachedOutputFile, 100, 200);
                }

                string attribFile = GetAttribFileName(tabModuleId, cacheKey);

                File.WriteAllBytes(cachedOutputFile, output);
                File.WriteAllLines(attribFile, new[] { DateTime.UtcNow.Add(duration).ToString(CultureInfo.InvariantCulture) });
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in SetModule(tabModuleId={TabModuleId},cacheKey={CacheKey},duration={Duration},output={Output}): {Arg4}", tabModuleId, cacheKey, duration, output, ex.Message);
                Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting SetModule(tabModuleId={TabModuleId},cacheKey={CacheKey},duration={Duration},output={Output})", tabModuleId, cacheKey, duration, output);
        }

        /// <inheritdoc />
        public override void Remove(int tabModuleId)
        {
			Logger.LogError(">>> Entering Remove(tabModuleId={TabModuleId})", tabModuleId);
            try
            {
                ModuleInfo tabModule = ModuleController.Instance.GetTabModule(tabModuleId);
				Logger.LogError("Remove(tabModuleId={TabModuleId}): tabModule → {TabModule}", tabModuleId, tabModule);

                int portalId = tabModule.PortalID;
				Logger.LogError("Remove(tabModuleId={TabModuleId}): portalId → {PortalId}", tabModuleId, portalId);
                if (portalId == Null.NullInteger)
                {
                    portalId = PortalSettings.Current.PortalId;
                }

                string cacheFolder = GetCacheFolder(portalId);
				Logger.LogError("Remove(tabModuleId={TabModuleId}): cacheFolder → {CacheFolder}", tabModuleId, cacheFolder);
                var filesNotDeleted = new StringBuilder();
				Logger.LogError("Remove(tabModuleId={TabModuleId}): filesNotDeleted → {FilesNotDeleted}", tabModuleId, filesNotDeleted);
                int i = 0;
                foreach (string file in Directory.GetFiles(cacheFolder, tabModuleId + "_*.*"))
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
					Logger.LogError("Throwing IOException from Remove(tabModuleId={TabModuleId})", tabModuleId);
                    throw new IOException("Deleted " + i + " files, however, some files are locked.  Could not delete the following files: " + filesNotDeleted);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in Remove(tabModuleId={TabModuleId}): {Arg1}", tabModuleId, ex.Message);
                Exceptions.Exceptions.LogException(ex);
            }
			Logger.LogWarning("<<< Exiting Remove(tabModuleId={TabModuleId})", tabModuleId);
        }

        private static string GetAttribFileName(int tabModuleId, string cacheKey)
        {
			Logger.LogError(">>> Entering GetAttribFileName(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            Logger.LogWarning("<<< Exiting GetAttribFileName(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            return string.Concat(GetCacheFolder(), cacheKey, AttribFileExtension);
        }

        private static int GetCachedItemCount(int tabModuleId)
        {
			Logger.LogError(">>> Entering GetCachedItemCount(tabModuleId={TabModuleId})", tabModuleId);
            Logger.LogWarning("<<< Exiting GetCachedItemCount(tabModuleId={TabModuleId})", tabModuleId);
            return Directory.GetFiles(GetCacheFolder(), $"*{DataFileExtension}").Length;
        }

        private static string GetCachedOutputFileName(int tabModuleId, string cacheKey)
        {
			Logger.LogError(">>> Entering GetCachedOutputFileName(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            Logger.LogWarning("<<< Exiting GetCachedOutputFileName(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            return string.Concat(GetCacheFolder(), cacheKey, DataFileExtension);
        }

        private static string GetCacheFolder(int portalId)
        {
			Logger.LogError(">>> Entering GetCacheFolder(portalId={PortalId})", portalId);
            string cacheFolder;

            using (var readerLock = CacheFolderPath.GetReadLock())
            {
                if (CacheFolderPath.TryGetValue(portalId, out cacheFolder))
                {
                    return cacheFolder;
                }
            }

            var portalInfo = PortalController.Instance.GetPortal(portalId);
			Logger.LogError("GetCacheFolder(portalId={PortalId}): portalInfo → {PortalInfo}", portalId, portalInfo);

            string homeDirectoryMapPath = portalInfo.HomeSystemDirectoryMapPath;

            if (!string.IsNullOrEmpty(homeDirectoryMapPath))
            {
                cacheFolder = string.Concat(homeDirectoryMapPath, "Cache\\Modules\\");
                if (!Directory.Exists(cacheFolder))
                {
                    Directory.CreateDirectory(cacheFolder);
                }
            }

            using (var writerLock = CacheFolderPath.GetWriteLock())
            {
                if (!CacheFolderPath.ContainsKey(portalId))
                {
                    CacheFolderPath.Add(portalId, cacheFolder);
                }
            }

			Logger.LogError("GetCacheFolder(): portalId → {PortalId}", portalId);
			Logger.LogWarning("<<< Exiting GetCacheFolder(portalId={PortalId})", portalId);
            return cacheFolder;
        }

        private static string GetCacheFolder()
        {
            int portalId = PortalController.Instance.GetCurrentSettings().PortalId;
            return GetCacheFolder(portalId);
        }

        private static bool IsPathInApplication(string cacheFolder)
        {
			Logger.LogError(">>> Entering IsPathInApplication(cacheFolder={CacheFolder})", cacheFolder);
            Logger.LogWarning("<<< Exiting IsPathInApplication(cacheFolder={CacheFolder})", cacheFolder);
            return cacheFolder.Contains(Globals.ApplicationMapPath);
        }

        private static bool IsFileExpired(string file)
        {
			Logger.LogError(">>> Entering IsFileExpired(file={File})", file);
            StreamReader oRead = null;
			Logger.LogError("IsFileExpired(file={File}): oRead → {ORead}", file, oRead);
            try
            {
                oRead = File.OpenText(file);
                DateTime expires = DateTime.Parse(oRead.ReadLine(), CultureInfo.InvariantCulture);
				Logger.LogError("IsFileExpired(file={File}): expires → {Expires}", file, expires);
                if (expires < DateTime.UtcNow)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
				Logger.LogError("Exception in IsFileExpired(file={File}): {Arg1}", file, ex.Message);
                // if check expire time failed, then force to expire the cache.
				Logger.LogWarning("<<< Exiting IsFileExpired(file={File})", file);
                return true;
            }
            finally
            {
                oRead?.Close();
            }
        }

        private static void PurgeCache(string folder)
        {
			Logger.LogError(">>> Entering PurgeCache(folder={Folder})", folder);
            var filesNotDeleted = new StringBuilder();
            int i = 0;
			Logger.LogError("PurgeCache(folder={Folder}): filesNotDeleted → {FilesNotDeleted}", folder, filesNotDeleted);
            foreach (string file in Directory.GetFiles(folder, "*.resources"))
            {
                if (!FileSystemUtils.DeleteFileWithWait(file, 100, 200))
                {
                    filesNotDeleted.Append($"{file};");
                }
                else
                {
                    i += 1;
                }
            }

            if (filesNotDeleted.Length > 0)
            {
				Logger.LogError("Throwing IOException from PurgeCache(folder={Folder})", folder);
                throw new IOException($"Deleted {i} files, however, some files are locked.  Could not delete the following files: {filesNotDeleted}");
            }
			Logger.LogWarning("<<< Exiting PurgeCache(folder={Folder})", folder);
        }

        private string GenerateCacheKeyHash(int tabModuleId, string cacheKey)
        {
			Logger.LogError(">>> Entering GenerateCacheKeyHash(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
            byte[] hash = Encoding.ASCII.GetBytes(cacheKey);
            using (var sha256 = new SHA256CryptoServiceProvider())
            {
                hash = sha256.ComputeHash(hash);
				Logger.LogWarning("<<< Exiting GenerateCacheKeyHash(tabModuleId={TabModuleId},cacheKey={CacheKey})", tabModuleId, cacheKey);
                return tabModuleId + "_" + this.ByteArrayToString(hash);
            }
        }
    }
}
