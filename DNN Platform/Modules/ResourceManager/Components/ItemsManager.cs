// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Dnn.Modules.ResourceManager.Components.Common;
    using Dnn.Modules.ResourceManager.Exceptions;
    using Dnn.Modules.ResourceManager.Helpers;
    using Dnn.Modules.ResourceManager.Services.Dto;

    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Assets;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using DotNetNuke.Services.Localization;

    /// <summary>Provides services to manage items.</summary>
    public class ItemsManager : ServiceLocator<IItemsManager, ItemsManager>, IItemsManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int MaxDescriptionLength = 500;
        private readonly IRoleController roleController;
        private readonly IFileManager fileManager;
        private readonly IAssetManager assetManager;
        private readonly IPermissionsManager permissionsManager;

        /// <summary>Initializes a new instance of the <see cref="ItemsManager"/> class.</summary>
        public ItemsManager()
        {
			Logger.LogDebug(">>> Entering ItemsManager()");
            this.roleController = RoleController.Instance;
            this.fileManager = FileManager.Instance;
			Logger.LogDebug("ItemsManager(): this.roleController → {Arg0}", this.roleController);
			Logger.LogDebug("ItemsManager(): fileManager → {FileManager}", fileManager);
			Logger.LogDebug("ItemsManager(): this.fileManager → {Arg0}", this.fileManager);
            this.assetManager = AssetManager.Instance;
            this.permissionsManager = PermissionsManager.Instance;
			Logger.LogDebug("<<< Exiting ItemsManager()");
			Logger.LogDebug("ItemsManager(): assetManager → {AssetManager}", assetManager);
			Logger.LogDebug("ItemsManager(): this.assetManager → {Arg0}", this.assetManager);
			Logger.LogDebug("ItemsManager(): permissionsManager → {PermissionsManager}", permissionsManager);
			Logger.LogDebug("ItemsManager(): this.permissionsManager → {Arg0}", this.permissionsManager);
			Logger.LogDebug("ItemsManager(): roleController → {RoleController}", roleController);
        }

        /// <inheritdoc />
        public ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sorting, int moduleMode)
        {
			Logger.LogDebug(">>> Entering GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sorting={Sorting},moduleMode={ModuleMode})", folderId, startIndex, numItems, sorting, moduleMode);
			Logger.LogDebug("<<< Exiting GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sorting={Sorting},moduleMode={ModuleMode})", folderId, startIndex, numItems, sorting, moduleMode);
            return this.GetFolderContent(folderId, startIndex, numItems, sorting, "Ascending", moduleMode);
        }

        /// <inheritdoc />
        public ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sorting, string sortingOrder, int moduleMode)
        {
            var noPermissionMessage = Localization.GetExceptionMessage(
                "UserHasNoPermissionToBrowseFolder",
                Constants.UserHasNoPermissionToBrowseFolderDefaultMessage);

            if (!this.permissionsManager.HasFolderContentPermission(folderId, moduleMode))
            {
                throw new FolderPermissionNotMetException(noPermissionMessage);
            }

            string sortingOrderShort = sortingOrder == "Ascending" ? "ASC" : "DESC";

            try
            {
				Logger.LogDebug("<<< Exiting GetFolderContent(folderId={FolderId},startIndex={StartIndex},numItems={NumItems},sorting={Sorting},sortingOrder={SortingOrder},moduleMode={ModuleMode})", folderId, startIndex, numItems, sorting, sortingOrder, moduleMode);
                return AssetManager.Instance.GetFolderContent(folderId, startIndex, numItems, sorting + " " + sortingOrderShort);
            }
            catch (AssetManagerException)
            {
                throw new FolderPermissionNotMetException(noPermissionMessage);
            }
        }

        /// <inheritdoc />
        public Stream GetFileContent(int fileId, out string fileName, out string contentType)
        {
			Logger.LogDebug(">>> Entering GetFileContent(fileId={FileId})", fileId);
            var file = this.fileManager.GetFile(fileId, true);

			Logger.LogDebug("GetFileContent(fileId={FileId}): file → {File}", fileId, file);
            if (!this.permissionsManager.HasGetFileContentPermission(file.FolderId))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString(Constants.UserHasNoPermissionToDownloadKey));
            }

            var content = this.fileManager.GetFileContent(file);
            fileName = file.FileName;
			Logger.LogDebug("GetFileContent(fileId={FileId}): content → {Content}", fileId, content);
			Logger.LogDebug("GetFileContent(fileId={FileId}): fileName → {FileName}", fileId, fileName);
            contentType = file.ContentType;

			Logger.LogDebug("GetFileContent(fileId={FileId}): contentType → {ContentType}", fileId, contentType);
            EventManager.Instance.OnFileDownloaded(new FileDownloadedEventArgs
            {
                FileInfo = file,
                UserId = UserController.Instance.GetCurrentUserInfo().UserID,
            });
			Logger.LogDebug("<<< Exiting GetFileContent(fileId={FileId})", fileId);
            return content;
        }

        /// <inheritdoc />
        public IFolderInfo CreateNewFolder(string folderName, int parentFolderId, int folderMappingId, string mappedName, int moduleMode)
        {
			Logger.LogDebug(">>> Entering CreateNewFolder(folderName={FolderName},parentFolderId={ParentFolderId},folderMappingId={FolderMappingId},mappedName={MappedName},moduleMode={ModuleMode})", folderName, parentFolderId, folderMappingId, mappedName, moduleMode);
            if (!this.permissionsManager.HasAddFoldersPermission(moduleMode, parentFolderId))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString(Constants.UserHasNoPermissionToAddFoldersKey));
            }

			Logger.LogDebug("<<< Exiting CreateNewFolder(folderName={FolderName},parentFolderId={ParentFolderId},folderMappingId={FolderMappingId},mappedName={MappedName},moduleMode={ModuleMode})", folderName, parentFolderId, folderMappingId, mappedName, moduleMode);
            return AssetManager.Instance.CreateFolder(folderName, parentFolderId, folderMappingId, mappedName);
        }

        /// <inheritdoc />
        public void SaveFileDetails(IFileInfo file, FileDetailsRequest fileDetails)
        {
            var propertyChanged = false;

            this.assetManager.RenameFile(file.FileId, fileDetails.FileName);
            if (file.Title != fileDetails.Title)
            {
                file.Title = fileDetails.Title;
                propertyChanged = true;
            }

            if (file.Description != fileDetails.Description)
            {
                file.Description = fileDetails.Description;
                if (!string.IsNullOrEmpty(file.Description) && file.Description.Length > MaxDescriptionLength)
                {
                    file.Description = file.Description.Substring(0, MaxDescriptionLength);
                }

                propertyChanged = true;
            }

            if (propertyChanged)
            {
                FileManager.Instance.UpdateFile(file);
            }
        }

        /// <inheritdoc />
        public void SaveFolderDetails(IFolderInfo folder, FolderDetailsRequest folderDetails)
        {
            if (!string.IsNullOrWhiteSpace(folderDetails.FolderName))
            {
                this.assetManager.RenameFolder(folderDetails.FolderId, folderDetails.FolderName);
            }

            folder.FolderPermissions.Clear();
            folder.FolderPermissions.AddRange(folderDetails.Permissions.RolePermissions.AsFolderPermissions(folderDetails.FolderId));
            folder.FolderPermissions.AddRange(folderDetails.Permissions.UserPermissions.AsFolderPermissions(folderDetails.FolderId));
            FolderManager.Instance.UpdateFolder(folder);
        }

        /// <inheritdoc />
        public void DeleteFile(int fileId, int moduleMode, int groupId)
        {
            var file = FileManager.Instance.GetFile(fileId);
            if (file == null)
            {
                return;
            }

            if (moduleMode == (int)Constants.ModuleModes.Group && this.IsGroupIcon(file))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString(Constants.GroupIconCantBeDeletedKey));
            }

            if (!this.permissionsManager.HasDeletePermission(moduleMode, file.FolderId))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFile.Error"));
            }

            AssetManager.Instance.DeleteFile(fileId);
			Logger.LogDebug("<<< Exiting DeleteFile(fileId={FileId},moduleMode={ModuleMode},groupId={GroupId})", fileId, moduleMode, groupId);
        }

        /// <inheritdoc />
        public void DeleteFolder(int folderId, bool unlinkAllowedStatus, int moduleMode)
        {
			Logger.LogDebug(">>> Entering DeleteFolder(folderId={FolderId},unlinkAllowedStatus={UnlinkAllowedStatus},moduleMode={ModuleMode})", folderId, unlinkAllowedStatus, moduleMode);
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
			Logger.LogDebug("DeleteFolder(folderId={FolderId},unlinkAllowedStatus={UnlinkAllowedStatus},moduleMode={ModuleMode}): folder → {Folder}", folderId, unlinkAllowedStatus, moduleMode, folder);
            {
                return;
            }

            if (!this.permissionsManager.HasDeletePermission(moduleMode, folderId))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFolder.Error"));
            }

            var nonDeletedSubfolders = new List<IFolderInfo>();
            AssetManager.Instance.DeleteFolder(folderId, unlinkAllowedStatus, nonDeletedSubfolders);
			Logger.LogDebug("<<< Exiting DeleteFolder(folderId={FolderId},unlinkAllowedStatus={UnlinkAllowedStatus},moduleMode={ModuleMode})", folderId, unlinkAllowedStatus, moduleMode);
        }

        /// <inheritdoc />
        public void MoveFile(int sourceFileId, int destinationFolderId, int moduleMode, int groupId)
        {
            var file = FileManager.Instance.GetFile(sourceFileId);
            var destinationFolder = FolderManager.Instance.GetFolder(destinationFolderId);

            if (file == null || destinationFolder == null)
            {
                return;
            }

            if (!this.permissionsManager.HasDeletePermission(moduleMode, file.FolderId))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFolder.Error"));
            }

            if (!this.permissionsManager.HasAddFilesPermission(moduleMode, destinationFolderId))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error"));
            }

            FileManager.Instance.MoveFile(file, destinationFolder);
			Logger.LogDebug("<<< Exiting MoveFile(sourceFileId={SourceFileId},destinationFolderId={DestinationFolderId},moduleMode={ModuleMode},groupId={GroupId})", sourceFileId, destinationFolderId, moduleMode, groupId);
        }

        /// <inheritdoc />
        public void MoveFolder(int sourceFolderId, int destinationFolderId, int moduleMode, int groupId)
        {
            var sourceFolder = FolderManager.Instance.GetFolder(sourceFolderId);
            var sourceFolderParent = FolderManager.Instance.GetFolder(sourceFolder.ParentID);
            var destinationFolder = FolderManager.Instance.GetFolder(destinationFolderId);

            if (sourceFolder == null || destinationFolder == null || sourceFolderParent == null)
            {
                return;
            }

            if (
                !this.permissionsManager.HasDeletePermission(moduleMode, sourceFolderId) ||
                !this.permissionsManager.HasDeletePermission(moduleMode, sourceFolderParent.FolderID))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToDeleteFolder.Error"));
            }

            if (
                !this.permissionsManager.HasAddFilesPermission(moduleMode, destinationFolderId) ||
                !this.permissionsManager.HasAddFoldersPermission(moduleMode, destinationFolderId))
            {
                throw new FolderPermissionNotMetException(LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error"));
            }

            FolderManager.Instance.MoveFolder(sourceFolder, destinationFolder);
			Logger.LogDebug("<<< Exiting MoveFolder(sourceFolderId={SourceFolderId},destinationFolderId={DestinationFolderId},moduleMode={ModuleMode},groupId={GroupId})", sourceFolderId, destinationFolderId, moduleMode, groupId);
        }

        /// <inheritdoc />
        protected override Func<IItemsManager> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new ItemsManager();
        }

        private bool IsGroupIcon(IFileInfo file)
        {
            var groupId = Utils.GetFolderGroupId(file.FolderId);
            if (groupId < 0)
            {
                return false;
            }

            var portalId = PortalSettings.Current.PortalId;
            var role = this.roleController.GetRoleById(portalId, groupId);
            return role?.IconFile?.Substring(7) == file.FileId.ToString();
        }
    }
}
