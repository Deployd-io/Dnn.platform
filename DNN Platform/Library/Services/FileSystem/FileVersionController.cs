// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using Microsoft.Extensions.DependencyInjection;

    public class FileVersionController(IFolderMappingController folderMappingController, DataProvider dataProvider, IEventManager eventManager, IUserController userController, IPortalController portalController) : ComponentBase<IFileVersionController, FileVersionController>, IFileVersionController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileVersionController));
        private readonly IFolderMappingController folderMappingController = folderMappingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderMappingController>();
        private readonly DataProvider dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        private readonly IEventManager eventManager = eventManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();

        /// <summary>Initializes a new instance of the <see cref="FileVersionController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with DataProvider. Scheduled removal in v12.0.0.")]
        public FileVersionController()
        : this(null, null, null, null, null)
        {
		Logger.LogError(">>> Entering FileVersionController()");
        Logger.LogWarning("<<< Exiting FileVersionController()");
        }

        /// <inheritdoc />
        public string AddFileVersion(IFileInfo file, int userId, bool published, bool removeOldestVersions, Stream content = null)
        {
			Logger.LogError(">>> Entering AddFileVersion(file={File},userId={UserId},published={Published},removeOldestVersions={RemoveOldestVersions},content={Content})", file, userId, published, removeOldestVersions, content);
            Requires.NotNull("file", file);

            byte[] fileContent = null;

            if (content != null)
            {
                var buffer = new byte[16 * 1024];
                using var ms = new MemoryStream();
				Logger.LogError("AddFileVersion(file={File},userId={UserId},published={Published},removeOldestVersions={RemoveOldestVersions},content={Content}): ms → {Ms}", file, userId, published, removeOldestVersions, content, ms);
                int read;
                while ((read = content.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                fileContent = ms.ToArray();
				Logger.LogError("AddFileVersion(file={File},userId={UserId},published={Published},removeOldestVersions={RemoveOldestVersions},content={Content}): fileContent → {FileContent}", file, userId, published, removeOldestVersions, content, fileContent);
				Logger.LogDebug("AddFileVersion(file={File},userId={UserId},published={Published},removeOldestVersions={RemoveOldestVersions},content={Content}): fileContent → {FileContent}", file, userId, published, removeOldestVersions, content, fileContent);
            }

            var newVersion = this.dataProvider
                                             .AddFileVersion(
                                                 file.FileId,
                                                 file.UniqueId,
                                                 file.VersionGuid,
                                                 file.FileName,
                                                 file.Extension,
                                                 file.Size,
                                                 file.Width,
                                                 file.Height,
                                                 file.ContentType,
                                                 file.Folder,
                                                 file.FolderId,
                                                 userId,
                                                 file.SHA1Hash,
                                                 file.LastModificationTime,
                                                 file.Title,
                                                 file.EnablePublishPeriod,
                                                 file.StartDate,
                                                 file.EndDate,
                                                 file.ContentItemID,
                                                 published,
                                                 fileContent);

            DataCache.RemoveCache("GetFileById" + file.FileId);

            if (removeOldestVersions)
            {
                this.RemoveOldestVersions(file);
            }

            if (published)
            {
                this.RenameFile(file, GetVersionedFilename(file, file.PublishedVersion));
                return file.FileName;
            }

			Logger.LogWarning("<<< Exiting AddFileVersion(file={File},userId={UserId},published={Published},removeOldestVersions={RemoveOldestVersions},content={Content})", file, userId, published, removeOldestVersions, content);
            return GetVersionedFilename(file, newVersion);
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SetPublishedVersion(IFileInfo file, int newPublishedVersion)
        {
			Logger.LogError(">>> Entering SetPublishedVersion(file={File},newPublishedVersion={NewPublishedVersion})", file, newPublishedVersion);
            this.dataProvider.SetPublishedVersion(file.FileId, newPublishedVersion);
            DataCache.RemoveCache("GetFileById" + file.FileId);

            // Rename the original file to the versioned name
            // Rename the new versioned name to the original file name
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("SetPublishedVersion(file={File},newPublishedVersion={NewPublishedVersion}): folderMapping → {FolderMapping}", file, newPublishedVersion, folderMapping);
            if (folderMapping == null)
            {
                return;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("SetPublishedVersion(file={File},newPublishedVersion={NewPublishedVersion}): folderProvider → {FolderProvider}", file, newPublishedVersion, folderProvider);

            folderProvider.RenameFile(file, GetVersionedFilename(file, file.PublishedVersion));
            folderProvider.RenameFile(
                    new FileInfo { FileName = GetVersionedFilename(file, newPublishedVersion), Folder = file.Folder, FolderId = file.FolderId, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID },
                    file.FileName);

            // Notify File Changed
            this.OnFileChanged(file, this.userController.GetCurrentUserInfo().UserID);
			Logger.LogWarning("<<< Exiting SetPublishedVersion(file={File},newPublishedVersion={NewPublishedVersion})", file, newPublishedVersion);
        }

        /// <inheritdoc />
        public int DeleteFileVersion(IFileInfo file, int version)
        {
			Logger.LogError(">>> Entering DeleteFileVersion(file={File},version={Version})", file, version);
            Requires.NotNull("file", file);

            int newVersion;

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("DeleteFileVersion(file={File},version={Version}): folderMapping → {FolderMapping}", file, version, folderMapping);
            if (folderMapping == null)
            {
                return Null.NullInteger;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("DeleteFileVersion(file={File},version={Version}): folderProvider → {FolderProvider}", file, version, folderProvider);

            if (file.PublishedVersion == version)
            {
                folderProvider.DeleteFile(new FileInfo { FileId = file.FileId, FileName = file.FileName, Folder = file.Folder, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID, FolderId = file.FolderId });
                newVersion = this.dataProvider.DeleteFileVersion(file.FileId, version);
				Logger.LogError("DeleteFileVersion(file={File},version={Version}): newVersion → {NewVersion}", file, version, newVersion);
				Logger.LogDebug("DeleteFileVersion(file={File},version={Version}): newVersion → {NewVersion}", file, version, newVersion);

                folderProvider.RenameFile(
                    new FileInfo { FileId = file.FileId, FileName = GetVersionedFilename(file, newVersion), Folder = file.Folder, FolderId = file.FolderId, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID },
                    file.FileName);

                // Update the Last Modification Time
                var providerLastModificationTime = folderProvider.GetLastModificationTime(file);
				Logger.LogError("DeleteFileVersion(file={File},version={Version}): providerLastModificationTime → {ProviderLastModificationTime}", file, version, providerLastModificationTime);
                if (file.LastModificationTime != providerLastModificationTime)
                {
                    this.dataProvider.UpdateFileLastModificationTime(file.FileId, providerLastModificationTime);
                }

                // Notify File Changed
                this.OnFileChanged(file, this.userController.GetCurrentUserInfo().UserID);
            }
            else
            {
                newVersion = this.dataProvider.DeleteFileVersion(file.FileId, version);
                folderProvider.DeleteFile(new FileInfo { FileName = GetVersionedFilename(file, version), Folder = file.Folder, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID, FolderId = file.FolderId });
            }

            DataCache.RemoveCache("GetFileById" + file.FileId);

			Logger.LogWarning("<<< Exiting DeleteFileVersion(file={File},version={Version})", file, version);
            return newVersion;
        }

        /// <inheritdoc />
        public FileVersionInfo GetFileVersion(IFileInfo file, int version)
        {
			Logger.LogError(">>> Entering GetFileVersion(file={File},version={Version})", file, version);
            Requires.NotNull("file", file);
			Logger.LogWarning("<<< Exiting GetFileVersion(file={File},version={Version})", file, version);
            return CBO.FillObject<FileVersionInfo>(this.dataProvider.GetFileVersion(file.FileId, version));
        }

        /// <inheritdoc />
        public void DeleteAllUnpublishedVersions(IFileInfo file, bool resetPublishedVersionNumber)
        {
			Logger.LogError(">>> Entering DeleteAllUnpublishedVersions(file={File},resetPublishedVersionNumber={ResetPublishedVersionNumber})", file, resetPublishedVersionNumber);
            Requires.NotNull("file", file);

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("DeleteAllUnpublishedVersions(file={File},resetPublishedVersionNumber={ResetPublishedVersionNumber}): folderMapping → {FolderMapping}", file, resetPublishedVersionNumber, folderMapping);
            if (folderMapping == null)
            {
                return;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("DeleteAllUnpublishedVersions(file={File},resetPublishedVersionNumber={ResetPublishedVersionNumber}): folderProvider → {FolderProvider}", file, resetPublishedVersionNumber, folderProvider);

            foreach (var version in this.GetFileVersions(file))
            {
                folderProvider.DeleteFile(new FileInfo { FileName = version.FileName, Folder = file.Folder, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID, FolderId = file.FolderId });
                this.dataProvider.DeleteFileVersion(version.FileId, version.Version);
            }

            if (resetPublishedVersionNumber)
            {
                file.PublishedVersion = 1;
				Logger.LogDebug("DeleteAllUnpublishedVersions(file={File},resetPublishedVersionNumber={ResetPublishedVersionNumber}): file.PublishedVersion → {Arg2}", file, resetPublishedVersionNumber, file.PublishedVersion);
                this.dataProvider.ResetFilePublishedVersion(file.FileId);
                DataCache.RemoveCache("GetFileById" + file.FileId);
            }
			Logger.LogWarning("<<< Exiting DeleteAllUnpublishedVersions(file={File},resetPublishedVersionNumber={ResetPublishedVersionNumber})", file, resetPublishedVersionNumber);
        }

        /// <inheritdoc />
        public IEnumerable<FileVersionInfo> GetFileVersions(IFileInfo file)
        {
			Logger.LogError(">>> Entering GetFileVersions(file={File})", file);
            Requires.NotNull("file", file);
			Logger.LogWarning("<<< Exiting GetFileVersions(file={File})", file);
            return CBO.FillCollection<FileVersionInfo>(this.dataProvider.GetFileVersions(file.FileId));
        }

        /// <inheritdoc />
        public bool IsFolderVersioned(int folderId)
        {
			Logger.LogError(">>> Entering IsFolderVersioned(folderId={FolderId})", folderId);
            Logger.LogWarning("<<< Exiting IsFolderVersioned(folderId={FolderId})", folderId);
            return this.IsFolderVersioned(FolderManager.Instance.GetFolder(folderId));
        }

        /// <inheritdoc />
        public bool IsFolderVersioned(IFolderInfo folder)
        {
            return this.IsFileVersionEnabled(folder.PortalID) && folder.IsVersioned;
        }

        /// <inheritdoc />
        public bool IsFileVersionEnabled(int portalId)
        {
			Logger.LogError(">>> Entering IsFileVersionEnabled(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting IsFileVersionEnabled(portalId={PortalId})", portalId);
            return PortalController.GetPortalSettingAsBoolean(this.portalController, "FileVersionEnabled", portalId, true);
        }

        /// <inheritdoc />
        public int MaxFileVersions(int portalId)
        {
			Logger.LogError(">>> Entering MaxFileVersions(portalId={PortalId})", portalId);
            Logger.LogWarning("<<< Exiting MaxFileVersions(portalId={PortalId})", portalId);
            return PortalController.GetPortalSettingAsInteger(this.portalController, "MaxFileVersions", portalId, 5);
        }

        /// <inheritdoc />
        public IEnumerable<FileVersionInfo> GetFileVersionsInFolder(int folderId)
        {
			Logger.LogError(">>> Entering GetFileVersionsInFolder(folderId={FolderId})", folderId);
            Requires.NotNegative("folderId", folderId);

			Logger.LogWarning("<<< Exiting GetFileVersionsInFolder(folderId={FolderId})", folderId);
            return CBO.FillCollection<FileVersionInfo>(this.dataProvider.GetFileVersionsInFolder(folderId));
        }

        /// <inheritdoc />
        public Stream GetVersionContent(IFileInfo file, int version)
        {
			Logger.LogError(">>> Entering GetVersionContent(file={File},version={Version})", file, version);
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("GetVersionContent(file={File},version={Version}): folderMapping → {FolderMapping}", file, version, folderMapping);
            if (folderMapping == null)
            {
                return null;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("GetVersionContent(file={File},version={Version}): folderProvider → {FolderProvider}", file, version, folderProvider);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogError("GetVersionContent(file={File},version={Version}): folder → {Folder}", file, version, folder);
			Logger.LogWarning("<<< Exiting GetVersionContent(file={File},version={Version})", file, version);
            return GetVersionContent(folderProvider, folder, file, version);
        }

        /// <inheritdoc />
        public void RollbackFileVersion(IFileInfo file, int version, int userId)
        {
			Logger.LogError(">>> Entering RollbackFileVersion(file={File},version={Version},userId={UserId})", file, version, userId);
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("RollbackFileVersion(file={File},version={Version},userId={UserId}): folderMapping → {FolderMapping}", file, version, userId, folderMapping);
            if (folderMapping == null)
            {
                return;
            }

            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("RollbackFileVersion(file={File},version={Version},userId={UserId}): folderProvider → {FolderProvider}", file, version, userId, folderProvider);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
			Logger.LogError("RollbackFileVersion(file={File},version={Version},userId={UserId}): folder → {Folder}", file, version, userId, folder);
            using (var content = GetVersionContent(folderProvider, folder, file, version))
            {
                FileManager.Instance.AddFile(folder, file.FileName, content, true, true, file.ContentType, userId);
            }

            // We need to refresh the file object
            file = FileManager.Instance.GetFile(file.FileId);
			Logger.LogError("RollbackFileVersion(file={File},version={Version},userId={UserId}): file → {File1}", file, version, userId, file);
			Logger.LogDebug("RollbackFileVersion(file={File},version={Version},userId={UserId}): file.Extension → {Arg3}", file, version, userId, file.Extension);
			Logger.LogDebug("RollbackFileVersion(file={File},version={Version},userId={UserId}): file.Size → {Arg3}", file, version, userId, file.Size);
			Logger.LogDebug("RollbackFileVersion(file={File},version={Version},userId={UserId}): file.SHA1Hash → {Arg3}", file, version, userId, file.SHA1Hash);
			Logger.LogDebug("RollbackFileVersion(file={File},version={Version},userId={UserId}): file.Width → {Arg3}", file, version, userId, file.Width);
			Logger.LogDebug("RollbackFileVersion(file={File},version={Version},userId={UserId}): file.Height → {Arg3}", file, version, userId, file.Height);
            var fileVersion = this.GetFileVersion(file, version);
			Logger.LogError("RollbackFileVersion(file={File},version={Version},userId={UserId}): fileVersion → {FileVersion}", file, version, userId, fileVersion);
            file.Extension = fileVersion.Extension;
            file.Size = fileVersion.Size;
            file.SHA1Hash = fileVersion.SHA1Hash;
            file.Width = fileVersion.Width;
            file.Height = fileVersion.Height;
            FileManager.Instance.UpdateFile(file);

            this.RemoveOldestVersions(file);
			Logger.LogWarning("<<< Exiting RollbackFileVersion(file={File},version={Version},userId={UserId})", file, version, userId);
        }

        internal static string GetVersionedFilename(IFileInfo file, int version)
        {
			Logger.LogError(">>> Entering GetVersionedFilename(file={File},version={Version})", file, version);
            Logger.LogWarning("<<< Exiting GetVersionedFilename(file={File},version={Version})", file, version);
            return $"{file.FileId}_{version}.v.resources";
        }

        private static Stream GetVersionContent(FolderProvider provider, IFolderInfo folder, IFileInfo file, int version)
        {
            return provider.GetFileStream(folder, file, version);
        }

        private void RenameFile(IFileInfo file, string newFileName)
        {
			Logger.LogError(">>> Entering RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("RenameFile(file={File},newFileName={NewFileName}): folderMapping → {FolderMapping}", file, newFileName, folderMapping);
            if (folderMapping != null)
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
				Logger.LogError("RenameFile(file={File},newFileName={NewFileName}): folderProvider → {FolderProvider}", file, newFileName, folderProvider);
                folderProvider.RenameFile(file, newFileName);
            }

            DataCache.RemoveCache("GetFileById" + file.FileId);
			Logger.LogWarning("<<< Exiting RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
        }

        private void OnFileChanged(IFileInfo fileInfo, int userId)
        {
			Logger.LogError(">>> Entering OnFileChanged(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
            this.eventManager.OnFileChanged(new FileChangedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
            });
			Logger.LogWarning("<<< Exiting OnFileChanged(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
        }

        private void RemoveOldestVersions(IFileInfo file)
        {
			Logger.LogError(">>> Entering RemoveOldestVersions(file={File})", file);
            var portalId = FolderManager.Instance.GetFolder(file.FolderId).PortalID;
            var versions = this.GetFileVersions(file);
			Logger.LogError("RemoveOldestVersions(file={File}): versions → {Versions}", file, versions);
			Logger.LogError("RemoveOldestVersions(file={File}): portalId → {PortalId}", file, portalId);
            var maxVersions = this.MaxFileVersions(portalId) - 1; // The published version is not in the FileVersions collection
            if (versions.Count() > maxVersions)
            {
				Logger.LogError("RemoveOldestVersions(file={File}): maxVersions → {MaxVersions}", file, maxVersions);
                foreach (var v in versions.OrderBy(v => v.Version).Take(versions.Count() - maxVersions))
                {
                    this.DeleteFileVersion(file, v.Version);
                }
            }
			Logger.LogWarning("<<< Exiting RemoveOldestVersions(file={File})", file);
        }
    }
}
