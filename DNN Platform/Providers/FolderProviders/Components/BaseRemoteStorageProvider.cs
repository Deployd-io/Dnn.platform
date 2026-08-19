// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.FolderProviders.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public abstract class BaseRemoteStorageProvider : FolderProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BaseRemoteStorageProvider));
        /// <summary>Initializes a new instance of the <see cref="BaseRemoteStorageProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        protected BaseRemoteStorageProvider()
            : this(null)
        {
		Logger.LogDebug(">>> Entering BaseRemoteStorageProvider()");
		Logger.LogDebug("<<< Exiting BaseRemoteStorageProvider()");
		Logger.LogDebug(">>> Entering BaseRemoteStorageProvider(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="BaseRemoteStorageProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        protected BaseRemoteStorageProvider(IHostSettings hostSettings)
        {
            this.HostSettings = hostSettings ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();
			Logger.LogDebug("<<< Exiting BaseRemoteStorageProvider(hostSettings={HostSettings})", hostSettings);
        }

        /// <inheritdoc />
        public override bool SupportsMappedPaths => true;

        /// <inheritdoc />
        public override bool SupportsMoveFile => false;

        /// <inheritdoc />
        public override bool SupportsMoveFolder => true;

        /// <summary>Gets the host settings.</summary>
        protected IHostSettings HostSettings { get; }

        protected virtual string FileNotFoundMessage => string.Empty;

        protected virtual string ObjectCacheKey => string.Empty;

        protected virtual int ObjectCacheTimeout => 150;

        protected virtual string ListObjectsCacheKey => string.Empty;

        protected virtual int ListObjectsCacheTimeout => 300;

        /// <inheritdoc />
        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering AddFolder(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            this.AddFolder(folderPath, folderMapping, folderPath);
			Logger.LogDebug("<<< Exiting AddFolder(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
        }

        /// <inheritdoc />
        public override void AddFile(IFolderInfo folder, string fileName, Stream content)
        {
			Logger.LogDebug(">>> Entering AddFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            this.UpdateFile(folder, fileName, content);
			Logger.LogDebug("<<< Exiting AddFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
        }

        public virtual void ClearCache(int folderMappingId)
        {
			Logger.LogDebug(">>> Entering ClearCache(folderMappingId=<redacted>)");
            var cacheKey = string.Format(this.ListObjectsCacheKey, folderMappingId);
            DataCache.RemoveCache(cacheKey);
			Logger.LogDebug("ClearCache(folderMappingId=<redacted>): cacheKey → {CacheKey}", cacheKey);

            // Clear cached objects
            DataCache.ClearCache(string.Format(this.ObjectCacheKey, folderMappingId, string.Empty));
			Logger.LogDebug("<<< Exiting ClearCache(folderMappingId=<redacted>)");
        }

        /// <inheritdoc />
        public override void CopyFile(string folderPath, string fileName, string newFolderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, fileName, newFolderPath);
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            if (folderPath == newFolderPath)
            {
                return;
            }

            this.CopyFileInternal(folderMapping, folderPath + fileName, newFolderPath + fileName);
			Logger.LogDebug("<<< Exiting CopyFile(folderPath={FolderPath},fileName={FileName},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, fileName, newFolderPath);
        }

        /// <inheritdoc />
        public override void DeleteFile(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering DeleteFile(file={File})", file);
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogDebug("DeleteFile(file={File}): folderMapping → {FolderMapping}", file, folderMapping);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogDebug("DeleteFile(file={File}): folder → {Folder}", file, folder);

            this.DeleteFileInternal(folderMapping, folder.MappedPath + file.FileName);
			Logger.LogDebug("<<< Exiting DeleteFile(file={File})", file);
        }

        /// <remarks>Azure Storage doesn't support folders, but we need to delete the file added while creating the folder.</remarks>
        /// <inheritdoc />
        public override void DeleteFolder(IFolderInfo folder)
        {
			Logger.LogDebug(">>> Entering DeleteFolder(folder={Folder})", folder);
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogDebug("DeleteFolder(folder={Folder}): folderMapping → {FolderMapping}", folder, folderMapping);

            this.DeleteFolderInternal(folderMapping, folder);
			Logger.LogDebug("<<< Exiting DeleteFolder(folder={Folder})", folder);
        }

        /// <inheritdoc />
        public override bool FileExists(IFolderInfo folder, string fileName)
        {
			Logger.LogDebug(">>> Entering FileExists(folder={Folder},fileName={FileName})", folder, fileName);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogDebug("FileExists(folder={Folder},fileName={FileName}): folderMapping → {FolderMapping}", folder, fileName, folderMapping);
            var item = this.GetStorageItem(folderMapping, folder.MappedPath + fileName);
			Logger.LogDebug("FileExists(folder={Folder},fileName={FileName}): item → {Item}", folder, fileName, item);
			Logger.LogDebug("<<< Exiting FileExists(folder={Folder},fileName={FileName})", folder, fileName);
            return item != null;
        }

        /// <inheritdoc />
        public override bool FolderExists(string folderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering FolderExists(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            // the root folder should always exist.
            if (folderPath == string.Empty)
            {
                return true;
            }

            var list = this.GetObjectList(folderMapping, folderPath);
			Logger.LogDebug("FolderExists(folderPath={FolderPath},folderMapping=<redacted>): list → {List}", folderPath, list);

			Logger.LogDebug("<<< Exiting FolderExists(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            return list.Any(o => o.Key.StartsWith(folderPath, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <remarks>Amazon doesn't support file attributes.</remarks>
        /// <inheritdoc />
        public override FileAttributes? GetFileAttributes(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileAttributes(file={File})", file);
			Logger.LogDebug("<<< Exiting GetFileAttributes(file={File})", file);
            return null;
        }

        /// <inheritdoc />
        public override string[] GetFiles(IFolderInfo folder)
        {
			Logger.LogDebug(">>> Entering GetFiles(folder={Folder})", folder);
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogDebug("GetFiles(folder={Folder}): folderMapping → {FolderMapping}", folder, folderMapping);
            var list = this.GetObjectList(folderMapping, folder.MappedPath);
			Logger.LogDebug("GetFiles(folder={Folder}): list → {List}", folder, list);
            var mappedPath = folder.MappedPath;

            ////return (from i in list
            ////       let f = i.Key
            ////       let r = (!string.IsNullOrEmpty(mappedPath) ? f.Replace(mappedPath, "") : f)
            ////       where f.StartsWith(mappedPath, true, CultureInfo.InvariantCulture) && f.Length > mappedPath.Length && r.IndexOf("/", StringComparison.Ordinal) == -1
            ////       select Path.GetFileName(f)).ToArray();
            var pattern = "^" + mappedPath;
			Logger.LogDebug("GetFiles(folder={Folder}): pattern → {Pattern}", folder, pattern);
			Logger.LogDebug("<<< Exiting GetFiles(folder={Folder})", folder);
            return (from i in list
                    let f = i.Key
                    let r = !string.IsNullOrEmpty(mappedPath) ? Regex.Replace(f, pattern, string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2)) : f
                    where f.StartsWith(mappedPath, true, CultureInfo.InvariantCulture) && f.Length > mappedPath.Length && r.IndexOf("/", StringComparison.Ordinal) == -1
                    select Path.GetFileName(f)).ToArray();
        }

        /// <inheritdoc />
        public override long GetFileSize(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileSize(file={File})", file);
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogDebug("GetFileSize(file={File}): folderMapping → {FolderMapping}", file, folderMapping);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogDebug("GetFileSize(file={File}): folder → {Folder}", file, folder);

            var item = this.GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
			Logger.LogDebug("GetFileSize(file={File}): item → {Item}", file, item);
            if (item == null)
            {
				Logger.LogWarning("Throwing FileNotFoundException from GetFileSize(file={File})", file);
			Logger.LogDebug("<<< Exiting GetFileSize(file={File})", file);
                throw new FileNotFoundException(this.FileNotFoundMessage, file.RelativePath);
            }

            return item.Size;
        }

        /// <inheritdoc />
        public override Stream GetFileStream(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetFileStream(file={File})", file);
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogDebug("GetFileStream(file={File}): folderMapping → {FolderMapping}", file, folderMapping);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogDebug("GetFileStream(file={File}): folder → {Folder}", file, folder);

			Logger.LogDebug("<<< Exiting GetFileStream(file={File})", file);
			Logger.LogDebug("<<< Exiting GetFileStream(folder={Folder},fileName={FileName})", folder, fileName);
            return this.GetFileStreamInternal(folderMapping, folder.MappedPath + file.FileName);
        }

        /// <inheritdoc />
        public override Stream GetFileStream(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            return this.GetFileStreamInternal(folderMapping, folder.MappedPath + fileName);
        }

        /// <inheritdoc />
        public override DateTime GetLastModificationTime(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetLastModificationTime(file={File})", file);
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogDebug("GetLastModificationTime(file={File}): folderMapping → {FolderMapping}", file, folderMapping);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogDebug("GetLastModificationTime(file={File}): folder → {Folder}", file, folder);

            var item = this.GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
			Logger.LogDebug("GetLastModificationTime(file={File}): item → {Item}", file, item);
            if (item == null)
            {
				Logger.LogWarning("Throwing FileNotFoundException from GetLastModificationTime(file={File})", file);
			Logger.LogDebug("<<< Exiting GetLastModificationTime(file={File})", file);
                throw new FileNotFoundException(this.FileNotFoundMessage, file.RelativePath);
            }

            return item.LastModified;
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            var list = this.GetObjectList(folderMapping, folderPath);
			Logger.LogDebug("GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>): list → {List}", folderPath, list);

            var pattern = "^" + Regex.Escape(folderPath);
			Logger.LogDebug("GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>): pattern → {Pattern}", folderPath, pattern);

			Logger.LogDebug("<<< Exiting GetSubFolders(folderPath={FolderPath},folderMapping=<redacted>)", folderPath);
            return (from o in list
                    let f = o.Key
                    let r =
                        !string.IsNullOrEmpty(folderPath)
                            ? Regex.Replace(f, pattern, string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2))
                            : f
                    where f.StartsWith(folderPath, StringComparison.InvariantCultureIgnoreCase)
                                       && f.Length > folderPath.Length
                                       && r.IndexOf("/", StringComparison.Ordinal) > -1
                    select folderPath + r.Substring(0, r.IndexOf("/", StringComparison.Ordinal)) + "/").Distinct().ToList();

            ////var mylist =  (from o in list
            ////       let f = o.Key
            ////       let r = (!string.IsNullOrEmpty(folderPath) ? RegexUtils.GetCachedRegex(Regex.Escape(folderPath)).Replace(f, string.Empty, 1) : f)
            ////       where f.StartsWith(folderPath)
            ////           && f.Length > folderPath.Length
            ////           && r.IndexOf("/", StringComparison.Ordinal) > -1
            ////       select folderPath + r.Substring(0, r.IndexOf("/", StringComparison.Ordinal)) + "/").Distinct().ToList();
            ////
            ////return mylist;
        }

        /// <remarks>For now, it returns false always until we find a better way to check if the file is synchronized.</remarks>
        /// <inheritdoc />
        public override bool IsInSync(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering IsInSync(file={File})", file);
			Logger.LogDebug("<<< Exiting IsInSync(file={File})", file);
            return Convert.ToInt32((file.LastModificationTime - this.GetLastModificationTime(file)).TotalSeconds) == 0;
        }

        /// <inheritdoc />
        public override void MoveFolder(string folderPath, string newFolderPath, FolderMappingInfo folderMapping)
        {
			Logger.LogDebug(">>> Entering MoveFolder(folderPath={FolderPath},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, newFolderPath);
            Requires.NotNullOrEmpty("folderPath", folderPath);
            Requires.NotNullOrEmpty("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            this.MoveFolderInternal(folderMapping, folderPath, newFolderPath);
			Logger.LogDebug("<<< Exiting MoveFolder(folderPath={FolderPath},newFolderPath={NewFolderPath},folderMapping=<redacted>)", folderPath, newFolderPath);
        }

        /// <inheritdoc />
        public override void RenameFile(IFileInfo file, string newFileName)
        {
			Logger.LogDebug(">>> Entering RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
            Requires.NotNull("file", file);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogDebug("RenameFile(file={File},newFileName={NewFileName}): folderMapping → {FolderMapping}", file, newFileName, folderMapping);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogDebug("RenameFile(file={File},newFileName={NewFileName}): folder → {Folder}", file, newFileName, folder);

            this.MoveFileInternal(folderMapping, folder.MappedPath + file.FileName, folder.MappedPath + newFileName);
			Logger.LogDebug("<<< Exiting RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
        }

        /// <inheritdoc />
        public override void RenameFolder(IFolderInfo folder, string newFolderName)
        {
			Logger.LogDebug(">>> Entering RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogDebug("RenameFolder(folder={Folder},newFolderName={NewFolderName}): folderMapping → {FolderMapping}", folder, newFolderName, folderMapping);
            var mappedPath = folder.MappedPath;
            if (mappedPath == folder.FolderName + "/" || mappedPath.EndsWith("/" + folder.FolderName + "/", StringComparison.Ordinal))
            {
                var newMappedPath =
                    PathUtils.Instance.FormatFolderPath(
                        mappedPath.Substring(0, mappedPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal)) +
                        newFolderName);

                this.MoveFolderInternal(folderMapping, mappedPath, newMappedPath);
            }
			Logger.LogDebug("<<< Exiting RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
        }

        /// <remarks>No implementation needed because this provider doesn't support <see cref="FileAttributes"/>.</remarks>
        /// <inheritdoc />
        public override void SetFileAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
		Logger.LogDebug(">>> Entering SetFileAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
		Logger.LogDebug("<<< Exiting SetFileAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
        }

        /// <remarks>Amazon doesn't support file attributes.</remarks>
        /// <inheritdoc />
        public override bool SupportsFileAttributes()
        {
			Logger.LogDebug(">>> Entering SupportsFileAttributes()");
			Logger.LogDebug("<<< Exiting SupportsFileAttributes()");
            return false;
        }

        /// <inheritdoc />
        public override void UpdateFile(IFileInfo file, Stream content)
        {
			Logger.LogDebug(">>> Entering UpdateFile(file={File},content={Content})", file, content);
            Requires.NotNull("file", file);
            Requires.NotNull("content", content);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogDebug("UpdateFile(file={File},content={Content}): folderMapping → {FolderMapping}", file, content, folderMapping);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogDebug("UpdateFile(file={File},content={Content}): folder → {Folder}", file, content, folder);

            this.UpdateFileInternal(content, folderMapping, folder.MappedPath + file.FileName);
			Logger.LogDebug("<<< Exiting UpdateFile(file={File},content={Content})", file, content);
        }

        /// <inheritdoc />
        public override void UpdateFile(IFolderInfo folder, string fileName, Stream content)
        {
			Logger.LogDebug(">>> Entering UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogDebug("UpdateFile(folder={Folder},fileName={FileName},content={Content}): folderMapping → {FolderMapping}", folder, fileName, content, folderMapping);

            this.UpdateFileInternal(content, folderMapping, folder.MappedPath + fileName);
			Logger.LogDebug("<<< Exiting UpdateFile(folder={Folder},fileName={FileName},content={Content})", folder, fileName, content);
        }

        /// <inheritdoc />
        public override string GetHashCode(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering GetHashCode(file={File})", file);
            Requires.NotNull("file", file);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogDebug("GetHashCode(file={File}): folder → {Folder}", file, folder);
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogDebug("GetHashCode(file={File}): folderMapping → {FolderMapping}", file, folderMapping);

            var item = this.GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
			Logger.LogDebug("GetHashCode(file={File}): item → {Item}", file, item);
            if (item == null)
            {
				Logger.LogWarning("Throwing FileNotFoundException from GetHashCode(file={File})", file);
			Logger.LogDebug("<<< Exiting GetHashCode(file={File})", file);
                throw new FileNotFoundException(this.FileNotFoundMessage, file.RelativePath);
            }

			Logger.LogDebug("<<< Exiting GetHashCode(file={File},fileContent={FileContent})", file, fileContent);
            return (item.Size == file.Size) ? item.HashCode : string.Empty;
        }

        /// <inheritdoc />
        public override string GetHashCode(IFileInfo file, Stream fileContent)
        {
            if (this.FileExists(FolderManager.Instance.GetFolder(file.FolderId), file.FileName))
            {
                return this.GetHashCode(file);
            }

            return string.Empty;
        }

        protected static bool GetBooleanSetting(FolderMappingInfo folderMapping, string settingName)
        {
			Logger.LogDebug(">>> Entering GetBooleanSetting(folderMapping=<redacted>,settingName={SettingName})", settingName);
			Logger.LogDebug("<<< Exiting GetBooleanSetting(folderMapping=<redacted>,settingName={SettingName})", settingName);
            return bool.Parse(folderMapping.FolderMappingSettings[settingName].ToString());
        }

        protected static int GetIntegerSetting(FolderMappingInfo folderMapping, string settingName, int defaultValue)
        {
			Logger.LogDebug(">>> Entering GetIntegerSetting(folderMapping=<redacted>,settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
            if (int.TryParse(GetSetting(folderMapping, settingName), out var value))
            {
			Logger.LogDebug("<<< Exiting GetIntegerSetting(folderMapping=<redacted>,settingName={SettingName},defaultValue={DefaultValue})", settingName, defaultValue);
                return value;
            }

            return defaultValue;
        }

        protected static string GetSetting(FolderMappingInfo folderMapping, string settingName)
        {
			Logger.LogDebug(">>> Entering GetSetting(folderMapping=<redacted>,settingName={SettingName})", settingName);
            Requires.NotNull(nameof(folderMapping), folderMapping);
            Requires.NotNullOrEmpty(nameof(settingName), settingName);

			Logger.LogDebug("<<< Exiting GetSetting(folderMapping=<redacted>,settingName={SettingName})", settingName);
            return folderMapping.FolderMappingSettings[settingName]?.ToString();
        }

        protected abstract void CopyFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void DeleteFileInternal(FolderMappingInfo folderMapping, string uri);

        protected abstract void DeleteFolderInternal(FolderMappingInfo folderMapping, IFolderInfo folder);

        protected abstract Stream GetFileStreamInternal(FolderMappingInfo folderMapping, string uri);

        protected abstract IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping);

        protected virtual IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping, string path)
        {
			Logger.LogDebug(">>> Entering GetObjectList(folderMapping=<redacted>,path={Path})", path);
			Logger.LogDebug("<<< Exiting GetObjectList(folderMapping=<redacted>,path={Path})", path);
            return this.GetObjectList(folderMapping);
        }

        protected abstract void MoveFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void MoveFolderInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void UpdateFileInternal(Stream stream, FolderMappingInfo folderMapping, string uri);

        protected virtual IRemoteStorageItem GetStorageItem(FolderMappingInfo folderMapping, string key)
        {
			Logger.LogDebug(">>> Entering GetStorageItem(folderMapping=<redacted>,key={Key})", key);
			Logger.LogDebug("<<< Exiting GetStorageItem(folderMapping=<redacted>,key={Key})", key);
            return this.GetStorageItemInternal(folderMapping, key);
        }

        private IRemoteStorageItem GetStorageItemInternal(FolderMappingInfo folderMapping, string key)
        {
			Logger.LogDebug(">>> Entering GetStorageItemInternal(folderMapping=<redacted>,key={Key})", key);
            var cacheKey = string.Format(this.ObjectCacheKey, folderMapping.FolderMappingID, key);

			Logger.LogDebug("GetStorageItemInternal(folderMapping=<redacted>,key={Key}): cacheKey → {CacheKey}", key, cacheKey);
            return CBO.GetCachedObject<IRemoteStorageItem>(
                this.HostSettings,
                new CacheItemArgs(
                cacheKey,
                this.ObjectCacheTimeout,
                CacheItemPriority.Default,
                folderMapping.FolderMappingID),
                c =>
                {
                    var list = this.GetObjectList(folderMapping, key);

                    ////return list.FirstOrDefault(i => i.Key == key);
				Logger.LogDebug("<<< Exiting GetStorageItemInternal(folderMapping=<redacted>,key={Key})", key);
                    return list.FirstOrDefault(i => i.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                });
        }
    }
}
