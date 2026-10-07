// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>Exposes methods to manage folders.</summary>
    public class FolderManager : ComponentBase<IFolderManager, FolderManager>, IFolderManager
    {
        private const string DefaultUsersFoldersPath = "Users";
        private const string DefaultMappedPathSetting = "DefaultMappedPath";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderManager));
        private static readonly ConcurrentDictionary<int, SyncFolderData> SyncFoldersData = new ConcurrentDictionary<int, SyncFolderData>();
        private static readonly object ThreadLocker = new object();
        private readonly IEventLogger eventLogger;
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private readonly IHostSettings hostSettings;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="FolderManager"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public FolderManager()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FolderManager"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        public FolderManager(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings, IPortalController portalController)
        {
			Logger.LogError(">>> Entering FolderManager(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},portalController={PortalController})", eventLogger, permissionDefinitionService, hostSettings, portalController);
			Logger.LogDebug("FolderManager(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},portalController={PortalController}): permissionDefinitionService → {PermissionDefinitionService1}", eventLogger, permissionDefinitionService, hostSettings, portalController, permissionDefinitionService);
			Logger.LogDebug("FolderManager(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},portalController={PortalController}): hostSettings → {HostSettings1}", eventLogger, permissionDefinitionService, hostSettings, portalController, hostSettings);
			Logger.LogDebug("FolderManager(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},portalController={PortalController}): portalController → {PortalController1}", eventLogger, permissionDefinitionService, hostSettings, portalController, portalController);
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			Logger.LogWarning("<<< Exiting FolderManager(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},portalController={PortalController})", eventLogger, permissionDefinitionService, hostSettings, portalController);
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        }

        /// <summary>Gets the localization key for MyFolderName.</summary>
        public virtual string MyFolderName => Localization.GetString("MyFolderName");

        /// <summary>Creates a new folder using the provided folder path.</summary>
        /// <param name="folderMapping">The folder mapping to use.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folderPath or folderMapping are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <returns>The added folder.</returns>
        public virtual IFolderInfo AddFolder(FolderMappingInfo folderMapping, string folderPath)
        {
			Logger.LogError(">>> Entering AddFolder(folderMapping=<redacted>,folderPath={FolderPath})", folderPath);
            Logger.LogWarning("<<< Exiting AddFolder(folderMapping=<redacted>,folderPath={FolderPath})", folderPath);
			Logger.LogWarning("<<< Exiting AddFolder(folderMapping=<redacted>,folderPath={FolderPath},mappedPath={MappedPath})", folderPath, mappedPath);
			Logger.LogWarning("<<< Exiting AddFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            return this.AddFolder(folderMapping, folderPath, folderPath);
        }

        /// <summary>Creates a new folder using the provided folder path and mapping.</summary>
        /// <param name="folderMapping">The folder mapping to use.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <param name="mappedPath">The mapped path of the new folder.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folderPath or folderMapping are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <returns>The added folder.</returns>
        public virtual IFolderInfo AddFolder(FolderMappingInfo folderMapping, string folderPath, string mappedPath)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            folderPath = folderPath.Trim();

            if (this.FolderExists(folderMapping.PortalID, folderPath))
            {
                throw new FolderAlreadyExistsException(Localization.GetExceptionMessage("AddFolderAlreadyExists", "The provided folder path already exists. The folder has not been added."));
            }

            if (!this.IsValidFolderPath(folderPath))
            {
                throw new InvalidFolderPathException(Localization.GetExceptionMessage("AddFolderNotAllowed", "The folder path '{0}' is not allowed. The folder has not been added.", folderPath));
            }

            var parentFolder = this.GetParentFolder(folderMapping.PortalID, folderPath);
            if (parentFolder != null)
            {
                var parentFolderMapping = FolderMappingController.Instance.GetFolderMapping(
                    parentFolder.PortalID,
                    parentFolder.FolderMappingID);
                if (FolderProvider.Instance(parentFolderMapping.FolderProviderType).SupportsMappedPaths)
                {
                    folderMapping = parentFolderMapping;
                    mappedPath = string.IsNullOrEmpty(parentFolder.FolderPath) ? PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath + folderPath)
                                                                            : PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath + folderPath.Replace(parentFolder.FolderPath, string.Empty));
                }
                else if (!FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
                {
                    mappedPath = folderPath;
                }
                else
                {
                    // Parent foldermapping DOESN'T support mapped path
                    // abd current foldermapping YES support mapped path
                    mappedPath = PathUtils.Instance.FormatFolderPath(GetDefaultMappedPath(folderMapping) + mappedPath);
                }
            }
            else if (FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
            {
                mappedPath = PathUtils.Instance.FormatFolderPath(GetDefaultMappedPath(folderMapping) + mappedPath);
            }

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).AddFolder(folderPath, folderMapping, mappedPath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("AddFolderUnderlyingSystemError", "The underlying system threw an exception. The folder has not been added."), ex);
            }

            this.CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(folderMapping.PortalID, folderPath));
            var folderId = this.CreateFolderInDatabase(folderMapping.PortalID, folderPath, folderMapping.FolderMappingID, mappedPath);

            var folder = this.GetFolder(folderId);

            // Notify add folder event
            OnFolderAdded(folder, this.GetCurrentUserId());

            return folder;
        }

        /// <summary>
        /// Creates a new folder in the given portal using the provided folder path.
        /// The same mapping as the parent folder will be used to create this folder. So this method have to be used only to create subfolders.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="folderPath">The path of the new folder.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folderPath is null or empty.</exception>
        /// <returns>The added folder.</returns>
        public virtual IFolderInfo AddFolder(int portalId, string folderPath)
        {
            Requires.NotNullOrEmpty("folderPath", folderPath);

            folderPath = PathUtils.Instance.FormatFolderPath(folderPath);

            var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
            var parentFolder = this.GetFolder(portalId, parentFolderPath) ?? this.AddFolder(portalId, parentFolderPath);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, parentFolder.FolderMappingID);

            return this.AddFolder(folderMapping, folderPath);
        }

        /// <summary>Deletes the specified folder.</summary>
        /// <param name="folder">The folder to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual void DeleteFolder(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering DeleteFolder(folder={Folder})", folder);
            this.DeleteFolderInternal(folder, false);
			Logger.LogWarning("<<< Exiting DeleteFolder(folder={Folder})", folder);
        }

        /// <summary>Removes the database reference to a folder on disk.</summary>
        /// <param name="folder">The folder to unlink.</param>
        public virtual void UnlinkFolder(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering UnlinkFolder(folder={Folder})", folder);
            this.DeleteFolderRecursive(folder, new Collection<IFolderInfo>(), true, true);
			Logger.LogWarning("<<< Exiting UnlinkFolder(folder={Folder})", folder);
        }

        /// <summary>Deletes the specified folder.</summary>
        /// <param name="folderId">The folder identifier.</param>
        public virtual void DeleteFolder(int folderId)
        {
			Logger.LogError(">>> Entering DeleteFolder(folderId={FolderId})", folderId);
            var folder = this.GetFolder(folderId);

			Logger.LogError("DeleteFolder(folderId={FolderId}): folder → {Folder}", folderId, folder);
            this.DeleteFolder(folder);
			Logger.LogWarning("<<< Exiting DeleteFolder(folderId={FolderId})", folderId);
        }

        /// <summary>Deletes the specified folder and all its content.</summary>
        /// <param name="folder">The folder to delete.</param>
        /// <param name="notDeletedSubfolders">A collection with all not deleted subfolders after processing the action.</param>
        public void DeleteFolder(IFolderInfo folder, ICollection<IFolderInfo> notDeletedSubfolders)
        {
			Logger.LogError(">>> Entering DeleteFolder(folder={Folder},notDeletedSubfolders={NotDeletedSubfolders})", folder, notDeletedSubfolders);
            this.DeleteFolderRecursive(folder, notDeletedSubfolders, true, this.GetOnlyUnmap(folder));
			Logger.LogWarning("<<< Exiting DeleteFolder(folder={Folder},notDeletedSubfolders={NotDeletedSubfolders})", folder, notDeletedSubfolders);
        }

        /// <summary>Checks the existence of the specified folder in the specified portal.</summary>
        /// <param name="portalId">The portal where to check the existence of the folder.</param>
        /// <param name="folderPath">The path of folder to check the existence of.</param>
        /// <returns>A bool value indicating whether the folder exists or not in the specified portal.</returns>
        public virtual bool FolderExists(int portalId, string folderPath)
        {
			Logger.LogError(">>> Entering FolderExists(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            Requires.PropertyNotNull("folderPath", folderPath);

			Logger.LogWarning("<<< Exiting FolderExists(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            return this.GetFolder(portalId, folderPath) != null;
        }

        /// <summary>Gets the files contained in the specified folder.</summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering GetFiles(folder={Folder})", folder);
            Logger.LogWarning("<<< Exiting GetFiles(folder={Folder})", folder);
			Logger.LogWarning("<<< Exiting GetFiles(folder={Folder},recursive={Recursive},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", folder, recursive, retrieveUnpublishedFiles);
            return this.GetFiles(folder, false);
        }

        /// <summary>Gets the files contained in the specified folder.</summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether to include all the subfolders.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive)
        {
            return this.GetFiles(folder, recursive, false);
        }

        /// <summary>Gets the files contained in the specified folder.</summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="recursive">Whether to include all the subfolders.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> GetFiles(IFolderInfo folder, bool recursive, bool retrieveUnpublishedFiles)
        {
            Requires.NotNull("folder", folder);

            return CBO.Instance.FillCollection<FileInfo>(DataProvider.Instance().GetFiles(folder.FolderID, retrieveUnpublishedFiles, recursive));
        }

        /// <summary>Gets the list of Standard folders the specified user has the provided permissions.</summary>
        /// <param name="user">The user info.</param>
        /// <param name="permissions">The permissions the folders have to meet.</param>
        /// <returns>The list of Standard folders the specified user has the provided permissions.</returns>
        /// <remarks>This method is used to support legacy behaviours and situations where we know the file/folder is in the file system.</remarks>
        public virtual IEnumerable<IFolderInfo> GetFileSystemFolders(UserInfo user, string permissions)
        {
			Logger.LogError(">>> Entering GetFileSystemFolders(user={User},permissions={Permissions})", user, permissions);
            var userFolders = new List<IFolderInfo>();

			Logger.LogError("GetFileSystemFolders(user={User},permissions={Permissions}): userFolders → {UserFolders}", user, permissions, userFolders);
            var portalId = user.PortalID;

            var userFolder = this.GetUserFolder(user);
			Logger.LogError("GetFileSystemFolders(user={User},permissions={Permissions}): userFolder → {UserFolder}", user, permissions, userFolder);

            var defaultFolderMaping = FolderMappingController.Instance.GetDefaultFolderMapping(portalId);
			Logger.LogError("GetFileSystemFolders(user={User},permissions={Permissions}): defaultFolderMaping → {DefaultFolderMaping}", user, permissions, defaultFolderMaping);

            var folders = this.GetFolders(portalId, permissions, user.UserID).Where(f => f.FolderPath != null && f.FolderMappingID == defaultFolderMaping.FolderMappingID);
			Logger.LogError("GetFileSystemFolders(user={User},permissions={Permissions}): folders → {Folders}", user, permissions, folders);

            foreach (var folder in folders)
            {
                if (folder.FolderPath.StartsWith(DefaultUsersFoldersPath + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (folder.FolderID == userFolder.FolderID)
                    {
                        folder.DisplayPath = this.MyFolderName + "/";
                        folder.DisplayName = this.MyFolderName;
                    }
                    else if (!folder.FolderPath.StartsWith(userFolder.FolderPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Allow UserFolder children
                        continue;
                    }
                }

                userFolders.Add(folder);
            }

			Logger.LogWarning("<<< Exiting GetFileSystemFolders(user={User},permissions={Permissions})", user, permissions);
            return userFolders;
        }

        /// <summary>Gets a folder entity by providing a folder identifier.</summary>
        /// <param name="folderId">The identifier of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(int folderId)
        {
			Logger.LogError(">>> Entering GetFolder(folderId={FolderId})", folderId);
            // Try and get the folder from the portal cache
            IFolderInfo folder = null;
			Logger.LogError("GetFolder(folderId={FolderId}): folder → {Folder}", folderId, folder);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			Logger.LogError("GetFolder(folderId={FolderId}): portalSettings → {PortalSettings}", folderId, portalSettings);
            if (portalSettings != null)
            {
                var folders = this.GetFolders(portalSettings.PortalId);
				Logger.LogError("GetFolder(folderId={FolderId}): folders → {Folders}", folderId, folders);
                folder = folders.SingleOrDefault(f => f.FolderID == folderId) ?? CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(folderId));
            }

			Logger.LogWarning("<<< Exiting GetFolder(folderId={FolderId})", folderId);
			Logger.LogWarning("<<< Exiting GetFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            return folder ?? CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(folderId));
        }

        /// <summary>Gets a folder entity by providing a portal identifier and folder path.</summary>
        /// <param name="portalId">The portal where the folder exists.</param>
        /// <param name="folderPath">The path of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(int portalId, string folderPath)
        {
            Requires.PropertyNotNull("folderPath", folderPath);

            folderPath = PathUtils.Instance.FormatFolderPath(folderPath);

            var folders = this.GetFolders(portalId);
            return folders.SingleOrDefault(f => f.FolderPath == folderPath) ?? CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(portalId, folderPath));
        }

        /// <summary>Gets a folder entity by providing its unique id.</summary>
        /// <param name="uniqueId">The unique id of the folder.</param>
        /// <returns>The folder entity or null if the folder cannot be located.</returns>
        public virtual IFolderInfo GetFolder(Guid uniqueId)
        {
            return CBO.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolderByUniqueID(uniqueId));
        }

        /// <summary>Gets the list of subfolders for the specified folder.</summary>
        /// <param name="parentFolder">The folder to get the list of subfolders.</param>
        /// <returns>The list of subfolders for the specified folder.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when parentFolder is null.</exception>
        public virtual IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder)
        {
			Logger.LogError(">>> Entering GetFolders(parentFolder={ParentFolder})", parentFolder);
			Logger.LogWarning("<<< Exiting GetFolders(parentFolder={ParentFolder})", parentFolder);
			Logger.LogWarning("<<< Exiting GetFolders(portalId={PortalId},useCache={UseCache})", portalId, useCache);
			Logger.LogWarning("<<< Exiting GetFolders(portalId={PortalId})", portalId);
			Logger.LogWarning("<<< Exiting GetFolders(portalId={PortalId},permissions={Permissions},userId={UserId})", portalId, permissions, userId);
			Logger.LogWarning("<<< Exiting GetFolders(user={User},permissions={Permissions})", user, permissions);
            return this.GetFolders(parentFolder, false);
        }

        /// <summary>Gets the sorted list of folders of the provided portal.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="useCache">True = Read from Cache, False = Read from DB. </param>
        /// <returns>The sorted list of folders of the provided portal.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(int portalId, bool useCache)
        {
            if (!useCache)
            {
                this.ClearFolderCache(portalId);
            }

            return this.GetFolders(portalId);
        }

        /// <summary>Gets the sorted list of folders of the provided portal.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The sorted list of folders of the provided portal.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(int portalId)
        {
            var folders = new List<IFolderInfo>();

            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderCacheKey, portalId);
            CBO.Instance.GetCachedObject<List<FolderInfo>>(new CacheItemArgs(cacheKey, DataCache.FolderCacheTimeOut, DataCache.FolderCachePriority, portalId), this.GetFoldersSortedCallBack, false).ForEach(folders.Add);

            return folders;
        }

        /// <summary>Gets the sorted list of folders that match the provided permissions in the specified portal.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="permissions">The permissions to match.</param>
        /// <param name="userId">The user identifier to be used to check permissions.</param>
        /// <returns>The list of folders that match the provided permissions in the specified portal.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(int portalId, string permissions, int userId)
        {
            var folders = new List<IFolderInfo>();

            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderUserCacheKey, portalId, permissions, userId);
            var cacheItemArgs = new CacheItemArgs(cacheKey, DataCache.FolderUserCacheTimeOut, DataCache.FolderUserCachePriority, portalId, permissions, userId);
            CBO.Instance.GetCachedObject<List<FolderInfo>>(cacheItemArgs, this.GetFoldersByPermissionSortedCallBack, false).ForEach(folders.Add);

            return folders;
        }

        /// <summary>Gets the list of folders the specified user has read permissions.</summary>
        /// <param name="user">The user info.</param>
        /// <returns>The list of folders the specified user has read permissions.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(UserInfo user)
        {
            return this.GetFolders(user, "READ");
        }

        /// <summary>Gets the list of folders the specified user has the provided permissions.</summary>
        /// <param name="user">The user info.</param>
        /// <param name="permissions">The permissions the folders have to met.</param>
        /// <returns>The list of folders the specified user has the provided permissions.</returns>
        public virtual IEnumerable<IFolderInfo> GetFolders(UserInfo user, string permissions)
        {
            var userFolders = new List<IFolderInfo>();

            var portalId = user.PortalID;

            var userFolder = this.GetUserFolder(user);

            foreach (var folder in this.GetFolders(portalId, permissions, user.UserID).Where(folder => folder.FolderPath != null))
            {
                if (folder.FolderPath.StartsWith(DefaultUsersFoldersPath + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (folder.FolderID == userFolder.FolderID)
                    {
                        folder.DisplayPath = Localization.GetString("MyFolderName") + "/";
                        folder.DisplayName = Localization.GetString("MyFolderName");
                    }
                    else if (!folder.FolderPath.StartsWith(userFolder.FolderPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Allow UserFolder children
                        continue;
                    }
                }

                userFolders.Add(folder);
            }

            return userFolders;
        }

        /// <summary>Gets the folder that belongs to a specific user.</summary>
        /// <param name="userInfo">The user to get the folder for.</param>
        /// <returns>The information about the user folder, <see cref="IFolderInfo"></see>.</returns>
        public virtual IFolderInfo GetUserFolder(UserInfo userInfo)
        {
			Logger.LogError(">>> Entering GetUserFolder(userInfo={UserInfo})", userInfo);
            // always use _default portal for a superuser
            int portalId = userInfo.IsSuperUser ? -1 : userInfo.PortalID;

            string userFolderPath = PathUtils.GetUserFolderPathInternal(userInfo);
			Logger.LogWarning("<<< Exiting GetUserFolder(userInfo={UserInfo})", userInfo);
            return this.GetFolder(portalId, userFolderPath) ?? this.AddUserFolder(userInfo);
        }

        /// <summary>Moves a folder to a new location.</summary>
        /// <param name="folder">The folder to move.</param>
        /// <param name="destinationFolder">Where to move the new folder.</param>
        /// <returns>The information about the moved folder, <see cref="IFolderInfo"></see>.</returns>
        public virtual IFolderInfo MoveFolder(IFolderInfo folder, IFolderInfo destinationFolder)
        {
			Logger.LogError(">>> Entering MoveFolder(folder={Folder},destinationFolder={DestinationFolder})", folder, destinationFolder);
            Requires.NotNull("folder", folder);
            Requires.NotNull("destinationFolder", destinationFolder);

            var newFolderPath = PathUtils.Instance.FormatFolderPath(destinationFolder.FolderPath + folder.FolderName + "/");
			Logger.LogError("MoveFolder(folder={Folder},destinationFolder={DestinationFolder}): newFolderPath → {NewFolderPath}", folder, destinationFolder, newFolderPath);

            if (folder.FolderPath == destinationFolder.FolderPath)
            {
                return folder;
            }

            if (this.FolderExists(folder.PortalID, newFolderPath))
            {
				Logger.LogError("Throwing InvalidOperationException from MoveFolder(folder={Folder},destinationFolder={DestinationFolder})", folder, destinationFolder);
                throw new InvalidOperationException(Localization.GetExceptionMessage(
                    "CannotMoveFolderAlreadyExists",
                    "The folder with name '{0}' cannot be moved. A folder with that name already exists under the folder '{1}'.",
                    folder.FolderName,
                    destinationFolder.FolderName));
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogError("MoveFolder(folder={Folder},destinationFolder={DestinationFolder}): folderMapping → {FolderMapping}", folder, destinationFolder, folderMapping);
            var destinationFolderMapping = FolderMappingController.Instance.GetFolderMapping(destinationFolder.PortalID, destinationFolder.FolderMappingID);
			Logger.LogError("MoveFolder(folder={Folder},destinationFolder={DestinationFolder}): destinationFolderMapping → {DestinationFolderMapping}", folder, destinationFolder, destinationFolderMapping);

            if (!this.CanMoveBetweenFolderMappings(folderMapping, destinationFolderMapping))
            {
                throw new InvalidOperationException(Localization.GetExceptionMessage(
                    "CannotMoveFolderBetweenFolderType",
                    "The folder with name '{0}' cannot be moved. Move Folder operation between this two folder types is not allowed",
                    folder.FolderName));
            }

            if (!this.IsMoveOperationValid(folder, destinationFolder, newFolderPath))
            {
                throw new InvalidOperationException(Localization.GetExceptionMessage("MoveFolderCannotComplete", "The operation cannot be completed."));
            }

            var currentFolderPath = folder.FolderPath;

            if ((folder.FolderMappingID == destinationFolder.FolderMappingID && FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMoveFolder) ||
                (IsStandardFolderProviderType(folderMapping) && IsStandardFolderProviderType(destinationFolderMapping)))
            {
                this.MoveFolderWithinProvider(folder, destinationFolder);
            }
            else
            {
                this.MoveFolderBetweenProviders(folder, newFolderPath);
            }

            // log the folder moved event.
            var log = new LogInfo();
			Logger.LogError("MoveFolder(folder={Folder},destinationFolder={DestinationFolder}): log → {Log}", folder, destinationFolder, log);
            log.AddProperty("Old Folder Path", currentFolderPath);
            log.AddProperty("New Folder Path", newFolderPath);
            log.AddProperty("Home Directory", folder.PortalID == Null.NullInteger ? Globals.HostPath : PortalSettings.Current.HomeDirectory);
            log.LogTypeKey = nameof(Abstractions.Logging.EventLogType.FOLDER_MOVED);
            LogController.Instance.AddLog(log);

            // Files in cache are obsolete because their physical path is not correct after moving
            this.DeleteFilesFromCache(folder.PortalID, newFolderPath);
            var movedFolder = this.GetFolder(folder.FolderID);
			Logger.LogError("MoveFolder(folder={Folder},destinationFolder={DestinationFolder}): movedFolder → {MovedFolder}", folder, destinationFolder, movedFolder);

            // Notify folder moved event
            OnFolderMoved(folder, this.GetCurrentUserId(), currentFolderPath);

			Logger.LogWarning("<<< Exiting MoveFolder(folder={Folder},destinationFolder={DestinationFolder})", folder, destinationFolder);
            return movedFolder;
        }

        /// <summary>Renames the specified folder by setting the new provided folder name.</summary>
        /// <param name="folder">The folder to rename.</param>
        /// <param name="newFolderName">The new name to apply to the folder.</param>
        /// <exception cref="System.ArgumentException">Thrown when newFolderName is null or empty.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual void RenameFolder(IFolderInfo folder, string newFolderName)
        {
			Logger.LogError(">>> Entering RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            if (folder.FolderName.Equals(newFolderName, StringComparison.Ordinal))
            {
                return;
            }

            var currentFolderName = folder.FolderName;

            var newFolderPath = folder.FolderPath.Substring(0, folder.FolderPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal)) + PathUtils.Instance.FormatFolderPath(newFolderName);
			Logger.LogError("RenameFolder(folder={Folder},newFolderName={NewFolderName}): newFolderPath → {NewFolderPath}", folder, newFolderName, newFolderPath);

            if (this.FolderExists(folder.PortalID, newFolderPath))
            {
				Logger.LogError("Throwing FolderAlreadyExistsException from RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
                throw new FolderAlreadyExistsException(Localization.GetExceptionMessage("RenameFolderAlreadyExists", "The destination folder already exists. The folder has not been renamed."));
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogError("RenameFolder(folder={Folder},newFolderName={NewFolderName}): folderMapping → {FolderMapping}", folder, newFolderName, folderMapping);
            var provider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("RenameFolder(folder={Folder},newFolderName={NewFolderName}): provider → {Provider}", folder, newFolderName, provider);

            this.RenameFolderInFileSystem(folder, newFolderPath);

            // Update Provider
            provider.RenameFolder(folder, newFolderName);

            // Update database
            this.UpdateChildFolders(folder, newFolderPath);

            // Files in cache are obsolete because their physical path is not correct after rename
            this.DeleteFilesFromCache(folder.PortalID, newFolderPath);

            // Notify folder renamed event
            OnFolderRenamed(folder, this.GetCurrentUserId(), currentFolderName);
			Logger.LogWarning("<<< Exiting RenameFolder(folder={Folder},newFolderName={NewFolderName})", folder, newFolderName);
        }

        /// <summary>Search the files contained in the specified folder, for a matching pattern.</summary>
        /// <param name="folder">The folder from which to retrieve the files.</param>
        /// <param name="pattern">The patter to search for.</param>
        /// <param name="recursive">Should the search be recursive.</param>
        /// <returns>The list of files contained in the specified folder.</returns>
        public virtual IEnumerable<IFileInfo> SearchFiles(IFolderInfo folder, string pattern, bool recursive)
        {
			Logger.LogError(">>> Entering SearchFiles(folder={Folder},pattern={Pattern},recursive={Recursive})", folder, pattern, recursive);
            Requires.NotNull("folder", folder);

            if (!FolderPermissionController.Instance.CanViewFolder(folder))
            {
				Logger.LogError("Throwing FolderProviderException from SearchFiles(folder={Folder},pattern={Pattern},recursive={Recursive})", folder, pattern, recursive);
                throw new FolderProviderException("No permission to view the folder");
            }

			Logger.LogWarning("<<< Exiting SearchFiles(folder={Folder},pattern={Pattern},recursive={Recursive})", folder, pattern, recursive);
			Logger.LogWarning("<<< Exiting SearchFiles(folder={Folder},regex={Regex},recursive={Recursive})", folder, regex, recursive);
            return this.SearchFiles(folder, WildcardToRegex(pattern), recursive);
        }

        /// <summary>Synchronizes the entire folder tree for the specified portal.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The number of folder collisions.</returns>
        public virtual int Synchronize(int portalId)
        {
			Logger.LogError(">>> Entering Synchronize(portalId={PortalId})", portalId);
            var folderCollisions = this.Synchronize(portalId, string.Empty, true, true);

			Logger.LogError("Synchronize(portalId={PortalId}): folderCollisions → {FolderCollisions}", portalId, folderCollisions);
            DataCache.ClearFolderCache(portalId);

			Logger.LogWarning("<<< Exiting Synchronize(portalId={PortalId})", portalId);
			Logger.LogWarning("<<< Exiting Synchronize(portalId={PortalId},relativePath={RelativePath},isRecursive={IsRecursive},syncFiles={SyncFiles})", portalId, relativePath, isRecursive, syncFiles);
            return folderCollisions;
        }

        /// <summary>Synchronizes the specified folder, its files and its subfolders.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="relativePath">The relative path of the folder.</param>
        /// <returns>The number of folder collisions.</returns>
        public virtual int Synchronize(int portalId, string relativePath)
        {
            return this.Synchronize(portalId, relativePath, true, true);
        }

        /// <summary>Synchronizes the specified folder, its files and, optionally, its subfolders.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="relativePath">The relative path of the folder.</param>
        /// <param name="isRecursive">Indicates if the synchronization has to be recursive.</param>
        /// <param name="syncFiles">Indicates if files need to be synchronized.</param>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoNetworkAvailableException">Thrown when there are folder mappings requiring network connection but there is no network available.</exception>
        /// <returns>The number of folder collisions.</returns>
        public virtual int Synchronize(int portalId, string relativePath, bool isRecursive, bool syncFiles)
        {
            Requires.PropertyNotNull("relativePath", relativePath);

            if (this.AreThereFolderMappingsRequiringNetworkConnectivity(portalId, relativePath, isRecursive) && !this.IsNetworkAvailable())
            {
                throw new NoNetworkAvailableException(Localization.GetExceptionMessage("NoNetworkAvailableError", "Network connectivity is needed but there is no network available."));
            }

            int? scriptTimeOut = null;

            Monitor.Enter(ThreadLocker);
            try
            {
                if (HttpContext.Current != null)
                {
                    scriptTimeOut = this.GetCurrentScriptTimeout();

                    // Synchronization could be a time-consuming process. To not get a time-out, we need to modify the request time-out value
                    this.SetScriptTimeout(int.MaxValue);
                }

                var mergedTree = this.GetMergedTree(portalId, relativePath, isRecursive);

                // Step 1: Add Folders
                InitialiseSyncFoldersData(portalId, relativePath);
                for (var i = 0; i < mergedTree.Count; i++)
                {
                    var item = mergedTree.Values[i];
                    this.ProcessMergedTreeItemInAddMode(item, portalId);
                }

                RemoveSyncFoldersData();

                // Step 2: Delete Files and Folders
                for (var i = mergedTree.Count - 1; i >= 0; i--)
                {
                    var item = mergedTree.Values[i];

                    if (syncFiles)
                    {
                        this.SynchronizeFiles(item, portalId);
                    }

                    this.ProcessMergedTreeItemInDeleteMode(item, portalId);
                }
            }
            finally
            {
                Monitor.Exit(ThreadLocker);

                // Restore original time-out
                if (HttpContext.Current != null && scriptTimeOut != null)
                {
                    this.SetScriptTimeout(scriptTimeOut.Value);
                }
            }

            return 0;
        }

        /// <summary>Updates metadata of the specified folder.</summary>
        /// <param name="folder">The folder to update.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <returns>The information about the updated folder, <see cref="IFolderInfo"></see>.</returns>
        public virtual IFolderInfo UpdateFolder(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering UpdateFolder(folder={Folder})", folder);
            var updatedFolder = this.UpdateFolderInternal(folder, true);

			Logger.LogError("UpdateFolder(folder={Folder}): updatedFolder → {UpdatedFolder}", folder, updatedFolder);
            this.AddLogEntry(updatedFolder, EventLogType.FOLDER_UPDATED);

            this.SaveFolderPermissions(updatedFolder);

			Logger.LogWarning("<<< Exiting UpdateFolder(folder={Folder})", folder);
            return updatedFolder;
        }

        /// <summary>Adds read permissions for all users to the specified folder.</summary>
        /// <param name="folder">The folder to add the permission to.</param>
        /// <param name="permission">Used as base class for FolderPermissionInfo when there is no read permission already defined.</param>
        public virtual void AddAllUserReadPermission(IFolderInfo folder, PermissionInfo permission)
            => this.AddAllUserReadPermission(folder, (IPermissionDefinitionInfo)permission);

        /// <summary>Adds read permissions for all users to the specified folder.</summary>
        /// <param name="folder">The folder to add the permission to.</param>
        /// <param name="permission">Used as base class for FolderPermissionInfo when there is no read permission already defined.</param>
        public virtual void AddAllUserReadPermission(IFolderInfo folder, IPermissionDefinitionInfo permission)
        {
			Logger.LogError(">>> Entering AddAllUserReadPermission(folder={Folder},permission={Permission})", folder, permission);
            var roleId = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);

			Logger.LogError("AddAllUserReadPermission(folder={Folder},permission={Permission}): roleId → {RoleId}", folder, permission, roleId);
            var folderPermission =
                (from IFolderPermissionInfo p in folder.FolderPermissions
                 where p.PermissionKey == "READ" && p.FolderId == folder.FolderID && p.RoleId == roleId && p.UserId == Null.NullInteger
                 select p).SingleOrDefault();

            if (folderPermission != null)
            {
                folderPermission.AllowAccess = true;
            }
            else
            {
                folderPermission = new FolderPermissionInfo(permission);
				Logger.LogError("AddAllUserReadPermission(folder={Folder},permission={Permission}): folderPermission → {FolderPermission}", folder, permission, folderPermission);
                folderPermission.FolderId = folder.FolderID;
                folderPermission.UserId = Null.NullInteger;
                folderPermission.RoleId = roleId;
                folderPermission.AllowAccess = true;

                folder.FolderPermissions.Add((FolderPermissionInfo)folderPermission);
            }
        }

        /// <summary>Sets folder permissions to the given folder by copying parent folder permissions.</summary>
        /// <param name="folder">The folder to copy permissions to.</param>
        public virtual void CopyParentFolderPermissions(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering CopyParentFolderPermissions(folder={Folder})", folder);
            Requires.NotNull("folder", folder);

            if (string.IsNullOrEmpty(folder.FolderPath))
            {
                return;
            }

            var parentFolderPath = folder.FolderPath.Substring(0, folder.FolderPath.Substring(0, folder.FolderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
			Logger.LogError("CopyParentFolderPermissions(folder={Folder}): parentFolderPath → {ParentFolderPath}", folder, parentFolderPath);

            foreach (IFolderPermissionInfo objPermission in GetFolderPermissionsFromSyncData(folder.PortalID, parentFolderPath))
            {
                var folderPermission = new FolderPermissionInfo(objPermission);
				Logger.LogError("CopyParentFolderPermissions(folder={Folder}): folderPermission → {FolderPermission}", folder, folderPermission);
                IFolderPermissionInfo iFolderPermission = folderPermission;
                iFolderPermission.FolderId = folder.FolderID;
				Logger.LogDebug("CopyParentFolderPermissions(folder={Folder}): iFolderPermission.FolderId → {Arg1}", folder, iFolderPermission.FolderId);
				Logger.LogDebug("CopyParentFolderPermissions(folder={Folder}): iFolderPermission.UserId → {Arg1}", folder, iFolderPermission.UserId);
				Logger.LogDebug("CopyParentFolderPermissions(folder={Folder}): iFolderPermission.AllowAccess → {Arg1}", folder, iFolderPermission.AllowAccess);
                iFolderPermission.RoleId = objPermission.RoleId;
				Logger.LogDebug("CopyParentFolderPermissions(folder={Folder}): iFolderPermission.RoleId → {Arg1}", folder, iFolderPermission.RoleId);
                iFolderPermission.UserId = objPermission.UserId;
                iFolderPermission.AllowAccess = objPermission.AllowAccess;
                folder.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
			Logger.LogWarning("<<< Exiting CopyParentFolderPermissions(folder={Folder})", folder);
        }

        /// <summary>Sets specific folder permissions for the given role to the given folder.</summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="permissionId">The id of the permission to assign.</param>
        /// <param name="roleId">The role to assign the permission to.</param>
        public virtual void SetFolderPermission(IFolderInfo folder, int permissionId, int roleId)
        {
			Logger.LogError(">>> Entering SetFolderPermission(folder={Folder},permissionId={PermissionId},roleId={RoleId})", folder, permissionId, roleId);
            this.SetFolderPermission(folder, permissionId, roleId, Null.NullInteger);
			Logger.LogWarning("<<< Exiting SetFolderPermission(folder={Folder},permissionId={PermissionId},roleId={RoleId})", folder, permissionId, roleId);
        }

        /// <summary>Sets specific folder permissions for the given role/user to the given folder.</summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="permissionId">The id of the permission to assign.</param>
        /// <param name="roleId">The role to assign the permission to.</param>
        /// <param name="userId">The user to assign the permission to.</param>
        public virtual void SetFolderPermission(IFolderInfo folder, int permissionId, int roleId, int userId)
        {
			Logger.LogError(">>> Entering SetFolderPermission(folder={Folder},permissionId={PermissionId},roleId={RoleId},userId={UserId})", folder, permissionId, roleId, userId);
            if (folder.FolderPermissions.Cast<IFolderPermissionInfo>()
                .Any(fpi => fpi.FolderId == folder.FolderID && fpi.PermissionId == permissionId && fpi.RoleId == roleId && fpi.UserId == userId && fpi.AllowAccess))
            {
				Logger.LogWarning("<<< Exiting SetFolderPermission(folder={Folder},permissionId={PermissionId},roleId={RoleId})", folder, permissionId, roleId);
                return;
            }

            var folderPermission = new FolderPermissionInfo();
			Logger.LogError("SetFolderPermission(folder={Folder},permissionId={PermissionId},roleId={RoleId},userId={UserId}): folderPermission → {FolderPermission}", folder, permissionId, roleId, userId, folderPermission);
            ((IFolderPermissionInfo)folderPermission).FolderId = folder.FolderID;
            ((IFolderPermissionInfo)folderPermission).PermissionId = permissionId;
            ((IFolderPermissionInfo)folderPermission).RoleId = roleId;
            ((IFolderPermissionInfo)folderPermission).UserId = userId;
            folderPermission.AllowAccess = true;
			Logger.LogDebug("SetFolderPermission(folder={Folder},permissionId={PermissionId},roleId={RoleId},userId={UserId}): folderPermission.AllowAccess → {Arg4}", folder, permissionId, roleId, userId, folderPermission.AllowAccess);

            folder.FolderPermissions.Add(folderPermission, true);
            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        /// <summary>Sets folder permissions for administrator role to the given folder.</summary>
        /// <param name="folder">The folder to set permission to.</param>
        /// <param name="administratorRoleId">The administrator role id to assign the permission to.</param>
        public virtual void SetFolderPermissions(IFolderInfo folder, int administratorRoleId)
        {
			Logger.LogError(">>> Entering SetFolderPermissions(folder={Folder},administratorRoleId={AdministratorRoleId})", folder, administratorRoleId);
            Requires.NotNull("folder", folder);

            foreach (var objPermission in this.permissionDefinitionService.GetDefinitionsByFolder())
            {
                var folderPermission = new FolderPermissionInfo(objPermission);
				Logger.LogError("SetFolderPermissions(folder={Folder},administratorRoleId={AdministratorRoleId}): folderPermission → {FolderPermission}", folder, administratorRoleId, folderPermission);
                ((IFolderPermissionInfo)folderPermission).FolderId = folder.FolderID;
                ((IFolderPermissionInfo)folderPermission).RoleId = administratorRoleId;

                folder.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
			Logger.LogWarning("<<< Exiting SetFolderPermissions(folder={Folder},administratorRoleId={AdministratorRoleId})", folder, administratorRoleId);
        }

        /// <summary>Checks if a given folder path is valid.</summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>A value indicating whether the folder path is valid.</returns>
        internal virtual bool IsValidFolderPath(string folderPath)
        {
			Logger.LogError(">>> Entering IsValidFolderPath(folderPath={FolderPath})", folderPath);
            var illegalInFolderPath = new Regex($"[{Regex.Escape(new string(Path.GetInvalidPathChars()))}]", RegexOptions.Compiled);
			Logger.LogError("IsValidFolderPath(folderPath={FolderPath}): illegalInFolderPath → {IllegalInFolderPath}", folderPath, illegalInFolderPath);
			Logger.LogWarning("<<< Exiting IsValidFolderPath(folderPath={FolderPath})", folderPath);
            return !illegalInFolderPath.IsMatch(folderPath) && !folderPath.TrimEnd('/', '\\').EndsWith(".", StringComparison.Ordinal);
        }

        /// <summary>Adds a log entry.</summary>
        /// <param name="folder">The folder to log about.</param>
        /// <param name="eventLogType">The type of the log entry.</param>
        internal virtual void AddLogEntry(IFolderInfo folder, EventLogType eventLogType)
        {
			Logger.LogError(">>> Entering AddLogEntry(folder={Folder},eventLogType={EventLogType})", folder, eventLogType);
            this.eventLogger.AddLog(folder, PortalController.Instance.GetCurrentSettings(), this.GetCurrentUserId(), string.Empty, eventLogType);
			Logger.LogWarning("<<< Exiting AddLogEntry(folder={Folder},eventLogType={EventLogType})", folder, eventLogType);
        }

        /// <summary>Adds a log entry.</summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="eventLogType">The type of log entry.</param>
        internal virtual void AddLogEntry(string propertyName, string propertyValue, EventLogType eventLogType)
        {
			Logger.LogError(">>> Entering AddLogEntry(propertyName={PropertyName},propertyValue={PropertyValue},eventLogType={EventLogType})", propertyName, propertyValue, eventLogType);
            this.eventLogger.AddLog(propertyName, propertyValue, PortalController.Instance.GetCurrentSettings(), this.GetCurrentUserId(), eventLogType);
			Logger.LogWarning("<<< Exiting AddLogEntry(propertyName={PropertyName},propertyValue={PropertyValue},eventLogType={EventLogType})", propertyName, propertyValue, eventLogType);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        internal void DeleteFilesFromCache(int portalId, string newFolderPath)
        {
			Logger.LogError(">>> Entering DeleteFilesFromCache(portalId={PortalId},newFolderPath={NewFolderPath})", portalId, newFolderPath);
            var folders = this.GetFolders(portalId).Where(f => f.FolderPath.StartsWith(newFolderPath, StringComparison.OrdinalIgnoreCase));
			Logger.LogError("DeleteFilesFromCache(portalId={PortalId},newFolderPath={NewFolderPath}): folders → {Folders}", portalId, newFolderPath, folders);
            foreach (var folderInfo in folders)
            {
                var fileIds = this.GetFiles(folderInfo).Select(f => f.FileId);
				Logger.LogError("DeleteFilesFromCache(portalId={PortalId},newFolderPath={NewFolderPath}): fileIds → {FileIds}", portalId, newFolderPath, fileIds);
                foreach (var fileId in fileIds)
                {
                    DataCache.RemoveCache("GetFileById" + fileId);
                }
            }
			Logger.LogWarning("<<< Exiting DeleteFilesFromCache(portalId={PortalId},newFolderPath={NewFolderPath})", portalId, newFolderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>The added folder information, <see cref="IFolderInfo" />.</returns>
        /// <param name="user">the user to add a folder for.</param>
        internal virtual IFolderInfo AddUserFolder(UserInfo user)
        {
			Logger.LogError(">>> Entering AddUserFolder(user={User})", user);
            // user _default portal for all super users
            var portalId = user.IsSuperUser ? Null.NullInteger : user.PortalID;

            var folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(portalId, DefaultUsersFoldersPath) ?? FolderMappingController.Instance.GetDefaultFolderMapping(portalId);
			Logger.LogError("AddUserFolder(user={User}): folderMapping → {FolderMapping}", user, folderMapping);

            if (!this.FolderExists(portalId, DefaultUsersFoldersPath))
            {
                this.AddFolder(folderMapping, DefaultUsersFoldersPath);
            }

            var rootFolder = PathUtils.GetUserFolderPathElement(user.UserID, PathUtils.UserFolderElement.Root);
			Logger.LogError("AddUserFolder(user={User}): rootFolder → {RootFolder}", user, rootFolder);
            var folderPath = PathUtils.Instance.FormatFolderPath($"{DefaultUsersFoldersPath}/{rootFolder}");
			Logger.LogError("AddUserFolder(user={User}): folderPath → {FolderPath}", user, folderPath);

            if (!this.FolderExists(portalId, folderPath))
            {
                this.AddFolder(folderMapping, folderPath);
            }

            folderPath = PathUtils.Instance.FormatFolderPath(string.Concat(folderPath, PathUtils.GetUserFolderPathElement(user.UserID, PathUtils.UserFolderElement.SubFolder)));
            if (!this.FolderExists(portalId, folderPath))
            {
                this.AddFolder(folderMapping, folderPath);
            }

            folderPath = PathUtils.Instance.FormatFolderPath(string.Concat(folderPath, user.UserID.ToString(CultureInfo.InvariantCulture)));

            if (this.FolderExists(portalId, folderPath))
            {
                return this.GetFolder(portalId, folderPath);
            }

            this.AddFolder(folderMapping, folderPath);

            var folder = this.GetFolder(portalId, folderPath);
			Logger.LogError("AddUserFolder(user={User}): folder → {Folder}", user, folder);

            foreach (var permission in this.permissionDefinitionService.GetDefinitionsByFolder())
            {
                if (!permission.PermissionKey.Equals("READ", StringComparison.OrdinalIgnoreCase) &&
                    !permission.PermissionKey.Equals("WRITE", StringComparison.OrdinalIgnoreCase) &&
                    !permission.PermissionKey.Equals("BROWSE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var folderPermission = new FolderPermissionInfo(permission);
				Logger.LogError("AddUserFolder(user={User}): folderPermission → {FolderPermission}", user, folderPermission);
                ((IFolderPermissionInfo)folderPermission).FolderId = folder.FolderID;
                ((IFolderPermissionInfo)folderPermission).UserId = user.UserID;
                ((IFolderPermissionInfo)folderPermission).RoleId = int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture);
                folderPermission.AllowAccess = true;
				Logger.LogDebug("AddUserFolder(user={User}): folderPermission.AllowAccess → {Arg1}", user, folderPermission.AllowAccess);

                folder.FolderPermissions.Add(folderPermission);

                if (permission.PermissionKey.Equals("READ", StringComparison.OrdinalIgnoreCase))
                {
                    this.AddAllUserReadPermission(folder, permission);
                }
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);

			Logger.LogWarning("<<< Exiting AddUserFolder(user={User})", user);
            return this.GetFolder(portalId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>A value indicating whether there are any folder mappings that required network connectivity.</returns>
        /// <param name="portalId">The site (Portal) ID.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isRecursive">A value indicating whether the check should be recursive or not.</param>
        internal virtual bool AreThereFolderMappingsRequiringNetworkConnectivity(int portalId, string relativePath, bool isRecursive)
        {
			Logger.LogError(">>> Entering AreThereFolderMappingsRequiringNetworkConnectivity(portalId={PortalId},relativePath={RelativePath},isRecursive={IsRecursive})", portalId, relativePath, isRecursive);
            var folder = this.GetFolder(portalId, relativePath);

			Logger.LogError("AreThereFolderMappingsRequiringNetworkConnectivity(portalId={PortalId},relativePath={RelativePath},isRecursive={IsRecursive}): folder → {Folder}", portalId, relativePath, isRecursive, folder);
            if (folder != null)
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
				Logger.LogError("AreThereFolderMappingsRequiringNetworkConnectivity(portalId={PortalId},relativePath={RelativePath},isRecursive={IsRecursive}): folderMapping → {FolderMapping}", portalId, relativePath, isRecursive, folderMapping);
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
				Logger.LogError("AreThereFolderMappingsRequiringNetworkConnectivity(portalId={PortalId},relativePath={RelativePath},isRecursive={IsRecursive}): folderProvider → {FolderProvider}", portalId, relativePath, isRecursive, folderProvider);

                if (folderProvider.RequiresNetworkConnectivity)
                {
                    return true;
                }
            }

            if (!isRecursive)
            {
                return false;
            }

            var folderMappingsRequiringNetworkConnectivity =
                from fm in FolderMappingController.Instance.GetFolderMappings(portalId)
                where
                    fm.IsEditable &&
                    FolderProvider.Instance(fm.FolderProviderType).RequiresNetworkConnectivity
                select fm;

			Logger.LogWarning("<<< Exiting AreThereFolderMappingsRequiringNetworkConnectivity(portalId={PortalId},relativePath={RelativePath},isRecursive={IsRecursive})", portalId, relativePath, isRecursive);
            return folderMappingsRequiringNetworkConnectivity.Any();
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void ClearFolderProviderCachedLists(int portalId)
        {
			Logger.LogError(">>> Entering ClearFolderProviderCachedLists(portalId={PortalId})", portalId);
            foreach (var folderMapping in FolderMappingController.Instance.GetFolderMappings(portalId))
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
				Logger.LogError("ClearFolderProviderCachedLists(portalId={PortalId}): folderProvider → {FolderProvider}", portalId, folderProvider);

                if (folderMapping.MappingName == "Standard" ||
                    folderMapping.MappingName == "Secure" ||
                    folderMapping.MappingName == "Database" ||
                    !this.hostSettings.EnableFileAutoSync)
                {
                    continue;
                }

                var type = folderProvider.GetType();
				Logger.LogError("ClearFolderProviderCachedLists(portalId={PortalId}): type → {Type}", portalId, type);
                MethodInfo method = type.GetMethod("ClearCache");
				Logger.LogError("ClearFolderProviderCachedLists(portalId={PortalId}): method → {Method}", portalId, method);
                if (method != null)
                {
                    method.Invoke(folderProvider, [folderMapping.FolderMappingID]);
                }
            }
			Logger.LogWarning("<<< Exiting ClearFolderProviderCachedLists(portalId={PortalId})", portalId);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        internal virtual void ClearFolderCache(int portalId)
        {
			Logger.LogError(">>> Entering ClearFolderCache(portalId={PortalId})", portalId);
            DataCache.ClearFolderCache(portalId);
			Logger.LogWarning("<<< Exiting ClearFolderCache(portalId={PortalId})", portalId);
        }

        /// <summary>Creates a folder in the database.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="folderPath">The folder path to create.</param>
        /// <param name="folderMappingId">The folder mapping id, <see cref="FolderMappings"/>.</param>
        /// <returns>The created folder ID.</returns>
        internal virtual int CreateFolderInDatabase(int portalId, string folderPath, int folderMappingId)
        {
			Logger.LogError(">>> Entering CreateFolderInDatabase(portalId={PortalId},folderPath={FolderPath},folderMappingId=<redacted>)", portalId, folderPath);
			Logger.LogWarning("<<< Exiting CreateFolderInDatabase(portalId={PortalId},folderPath={FolderPath},folderMappingId=<redacted>)", portalId, folderPath);
            return this.CreateFolderInDatabase(portalId, folderPath, folderMappingId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="folderPath">The folder path to create.</param>
        /// <param name="folderMappingId">The id for the folder mapping.</param>
        /// <param name="mappedPath">The mapped path.</param>
        /// <returns>The created folder ID.</returns>
        internal virtual int CreateFolderInDatabase(int portalId, string folderPath, int folderMappingId, string mappedPath)
        {
            var isProtected = PathUtils.Instance.IsDefaultProtectedPath(folderPath);
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folderMappingId);
            var storageLocation = (int)FolderController.StorageLocationTypes.DatabaseSecure;
            if (!folderMapping.IsEditable)
            {
                storageLocation = folderMapping.MappingName switch
                {
                    "Standard" => (int)FolderController.StorageLocationTypes.InsecureFileSystem,
                    "Secure" => (int)FolderController.StorageLocationTypes.SecureFileSystem,
                    _ => (int)FolderController.StorageLocationTypes.DatabaseSecure,
                };
            }

            var folder = new FolderInfo(true)
            {
                PortalID = portalId,
                FolderPath = folderPath,
                MappedPath = mappedPath,
                StorageLocation = storageLocation,
                IsProtected = isProtected,
                IsCached = false,
                FolderMappingID = folderMappingId,
                LastUpdated = Null.NullDate,
            };

            folder.FolderID = this.AddFolderInternal(folder);

            if (portalId != Null.NullInteger)
            {
                // Set Folder Permissions to inherit from parent
                this.CopyParentFolderPermissions(folder);
            }

            return folder.FolderID;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="physicalPath">The physical path to create the folder in.</param>
        internal virtual void CreateFolderInFileSystem(string physicalPath)
        {
			Logger.LogError(">>> Entering CreateFolderInFileSystem(physicalPath={PhysicalPath})", physicalPath);
            var di = new DirectoryInfo(physicalPath);
			Logger.LogError("CreateFolderInFileSystem(physicalPath={PhysicalPath}): di → {Di}", physicalPath, di);
            if (!di.Exists)
            {
                di.Create();
            }
			Logger.LogWarning("<<< Exiting CreateFolderInFileSystem(physicalPath={PhysicalPath})", physicalPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="folderPath">The folder path.</param>
        internal virtual void DeleteFolder(int portalId, string folderPath)
        {
			Logger.LogError(">>> Entering DeleteFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            DataProvider.Instance().DeleteFolder(portalId, PathUtils.Instance.FormatFolderPath(folderPath));
            this.AddLogEntry("FolderPath", folderPath, EventLogType.FOLDER_DELETED);
            this.UpdateParentFolder(portalId, folderPath);
            DataCache.ClearFolderCache(portalId);
			Logger.LogWarning("<<< Exiting DeleteFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folderMappings">The folder mapping affected.</param>
        /// <param name="foldersToDelete">A list of folders to delete.</param>
        internal virtual void DeleteFoldersFromExternalStorageLocations(Dictionary<int, FolderMappingInfo> folderMappings, IEnumerable<IFolderInfo> foldersToDelete)
        {
			Logger.LogError(">>> Entering DeleteFoldersFromExternalStorageLocations(folderMappings=<redacted>,foldersToDelete={FoldersToDelete})", foldersToDelete);
            foreach (var folderToDelete in foldersToDelete)
            {
                // Delete source folder from its storage location
                var folderMapping = this.GetFolderMapping(folderMappings, folderToDelete.FolderMappingID);
				Logger.LogError("DeleteFoldersFromExternalStorageLocations(folderMappings=<redacted>,foldersToDelete={FoldersToDelete}): folderMapping → {FolderMapping}", foldersToDelete, folderMapping);

                try
                {
                    var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
					Logger.LogError("DeleteFoldersFromExternalStorageLocations(folderMappings=<redacted>,foldersToDelete={FoldersToDelete}): folderProvider → {FolderProvider}", foldersToDelete, folderProvider);

                    // IMPORTANT: We cannot delete the folder from its storage location when it contains other subfolders
                    if (!folderProvider.GetSubFolders(folderToDelete.MappedPath, folderMapping).Any())
                    {
                        folderProvider.DeleteFolder(folderToDelete);
                    }
                }
                catch (Exception ex)
                {
                    // The folders that cannot be deleted from its storage location will be handled during the next sync
                    Logger.Error(ex);
                }
            }
			Logger.LogWarning("<<< Exiting DeleteFoldersFromExternalStorageLocations(folderMappings=<redacted>,foldersToDelete={FoldersToDelete})", foldersToDelete);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>A value representing the current script timeout.</returns>
        internal virtual int GetCurrentScriptTimeout()
        {
			Logger.LogError(">>> Entering GetCurrentScriptTimeout()");
            Logger.LogWarning("<<< Exiting GetCurrentScriptTimeout()");
            return HttpContext.Current.Server.ScriptTimeout;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>The id of the current user.</returns>
        internal virtual int GetCurrentUserId()
        {
			Logger.LogError(">>> Entering GetCurrentUserId()");
            Logger.LogWarning("<<< Exiting GetCurrentUserId()");
            return UserController.Instance.GetCurrentUserInfo().UserID;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isRecursive">A value indicating whether to return folders recursively.</param>
        /// <returns>A sorted list of folders.</returns>
        internal virtual SortedList<string, MergedTreeItem> GetDatabaseFolders(int portalId, string relativePath, bool isRecursive)
        {
            var databaseFolders = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());

            var folder = this.GetFolder(portalId, relativePath);
            if (folder is null)
            {
                return databaseFolders;
            }

            if (!isRecursive)
            {
                var item = new MergedTreeItem
                {
                    FolderID = folder.FolderID,
                    FolderMappingID = folder.FolderMappingID,
                    FolderPath = folder.FolderPath,
                    ExistsInDatabase = true,
                    MappedPath = folder.MappedPath,
                };

                databaseFolders.Add(relativePath, item);
            }
            else
            {
                databaseFolders = this.GetDatabaseFoldersRecursive(folder);
            }

            return databaseFolders;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folder">The folder to get.</param>
        /// <returns>A sorted list of folders.</returns>
        internal virtual SortedList<string, MergedTreeItem> GetDatabaseFoldersRecursive(IFolderInfo folder)
        {
            var result = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());
            var stack = new Stack<IFolderInfo>();

            stack.Push(folder);

            while (stack.Count > 0)
            {
                var folderInfo = stack.Pop();

                var item = new MergedTreeItem
                {
                    FolderID = folderInfo.FolderID,
                    FolderMappingID = folderInfo.FolderMappingID,
                    FolderPath = folderInfo.FolderPath,
                    ExistsInDatabase = true,
                    MappedPath = folderInfo.MappedPath,
                };

                if (!result.ContainsKey(item.FolderPath))
                {
                    result.Add(item.FolderPath, item);
                }

                foreach (var subfolder in this.GetFolders(folderInfo))
                {
                    stack.Push(subfolder);
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isRecursive">A value indicating whether to return the folders recursively.</param>
        /// <returns>A sorted list of folders.</returns>
        internal virtual SortedList<string, MergedTreeItem> GetFileSystemFolders(int portalId, string relativePath, bool isRecursive)
        {
            var fileSystemFolders = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());

            var physicalPath = PathUtils.Instance.GetPhysicalPath(portalId, relativePath);
            var hideFoldersEnabled = PortalController.GetPortalSettingAsBoolean(this.portalController, "HideFoldersEnabled", portalId, true);

            if (!DirectoryWrapper.Instance.Exists(physicalPath))
            {
                return fileSystemFolders;
            }

            var directoryIsHidden = (FileWrapper.Instance.GetAttributes(physicalPath) & FileAttributes.Hidden) == FileAttributes.Hidden;
            if ((directoryIsHidden || physicalPath.StartsWith("_", StringComparison.Ordinal)) && hideFoldersEnabled)
            {
                return fileSystemFolders;
            }

            if (!isRecursive)
            {
                var item = new MergedTreeItem
                {
                    FolderID = -1,
                    FolderMappingID = -1,
                    FolderPath = relativePath,
                    ExistsInFileSystem = true,
                    MappedPath = string.Empty,
                };

                fileSystemFolders.Add(relativePath, item);
            }
            else
            {
                fileSystemFolders = this.GetFileSystemFoldersRecursive(portalId, physicalPath);
            }

            return fileSystemFolders;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="physicalPath">The physical path of the folder.</param>
        /// <returns>A sorted list of folders.</returns>
        internal virtual SortedList<string, MergedTreeItem> GetFileSystemFoldersRecursive(int portalId, string physicalPath)
        {
            var result = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());
            var stack = new Stack<string>();

            stack.Push(physicalPath);

            var hideFoldersEnabled = PortalController.GetPortalSettingAsBoolean(this.portalController, "HideFoldersEnabled", portalId, true);

            while (stack.Count > 0)
            {
                var dir = stack.Pop();

                try
                {
                    var item = new MergedTreeItem
                    {
                        FolderID = -1,
                        FolderMappingID = -1,
                        FolderPath = PathUtils.Instance.GetRelativePath(portalId, dir),
                        ExistsInFileSystem = true,
                        MappedPath = string.Empty,
                    };

                    result.Add(item.FolderPath, item);

                    foreach (var dn in DirectoryWrapper.Instance.GetDirectories(dir))
                    {
                        if (((FileWrapper.Instance.GetAttributes(dn) & FileAttributes.Hidden) == FileAttributes.Hidden || dn.StartsWith("_", StringComparison.Ordinal)) && hideFoldersEnabled)
                        {
                            continue;
                        }

                        stack.Push(dn);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folderMappings">The folder mapping to search in.</param>
        /// <param name="folderMappingId">The folder mapping id to return.</param>
        /// <returns>A single folder mapping, <see cref="FolderMappingInfo"/>.</returns>
        internal virtual FolderMappingInfo GetFolderMapping(Dictionary<int, FolderMappingInfo> folderMappings, int folderMappingId)
        {
			Logger.LogError(">>> Entering GetFolderMapping(folderMappings=<redacted>,folderMappingId=<redacted>)");
            if (!folderMappings.TryGetValue(folderMappingId, out var folderMapping))
            {
                folderMapping = FolderMappingController.Instance.GetFolderMapping(folderMappingId);
				Logger.LogError("GetFolderMapping(folderMappings=<redacted>,folderMappingId=<redacted>): folderMapping → {FolderMapping}", folderMapping);
				Logger.LogDebug("GetFolderMapping(folderMappings=<redacted>,folderMappingId=<redacted>): folderMapping → {FolderMapping}", folderMapping);
                folderMappings.Add(folderMappingId, folderMapping);
            }

			Logger.LogWarning("<<< Exiting GetFolderMapping(folderMappings=<redacted>,folderMappingId=<redacted>)");
            return folderMapping;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folderMapping">The folder mapping to use.</param>
        /// <param name="folder">The folder base path.</param>
        /// <returns>A sorted list of folder mappings.</returns>
        internal virtual SortedList<string, MergedTreeItem> GetFolderMappingFoldersRecursive(FolderMappingInfo folderMapping, IFolderInfo folder)
        {
            var result = new SortedList<string, MergedTreeItem>(new IgnoreCaseStringComparer());
            var stack = new Stack<string>();
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            var baseMappedPath = folder.MappedPath;
            var baseFolderPath = folder.FolderPath;

            stack.Push(baseMappedPath);

            while (stack.Count > 0)
            {
                var mappedPath = stack.Pop();
                var relativePath = string.IsNullOrEmpty(mappedPath)
                                        ? string.Empty
                                        : string.IsNullOrEmpty(baseMappedPath)
                                            ? mappedPath
                                            : RegexUtils.GetCachedRegex(Regex.Escape(baseMappedPath)).Replace(mappedPath, string.Empty, 1);

                var folderPath = baseFolderPath + relativePath;

                if (!folderProvider.FolderExists(mappedPath, folderMapping))
                {
                    continue;
                }

                var item = new MergedTreeItem
                {
                    FolderID = -1,
                    FolderMappingID = folderMapping.FolderMappingID,
                    FolderPath = folderPath,
                    ExistsInFolderMapping = true,
                    MappedPath = mappedPath,
                };

                if (!result.ContainsKey(item.FolderPath))
                {
                    result.Add(item.FolderPath, item);
                }

                foreach (var subfolderPath in folderProvider.GetSubFolders(mappedPath, folderMapping))
                {
                    if (folderMapping.SyncAllSubFolders || folderProvider.FolderExists(subfolderPath, folderMapping))
                    {
                        stack.Push(subfolderPath);
                    }
                }
            }

            return result;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="cacheItemArgs">The cached items arguments, <see cref="CacheItemArgs"/>.</param>
        /// <returns>A list of folders.</returns>
        internal virtual object GetFoldersByPermissionSortedCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetFoldersByPermissionSortedCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalId = (int)cacheItemArgs.ParamList[0];
            var permissions = (string)cacheItemArgs.ParamList[1];
			Logger.LogError("GetFoldersByPermissionSortedCallBack(cacheItemArgs={CacheItemArgs}): permissions → {Permissions}", cacheItemArgs, permissions);
            var userId = (int)cacheItemArgs.ParamList[2];
			Logger.LogError("GetFoldersByPermissionSortedCallBack(cacheItemArgs={CacheItemArgs}): userId → {UserId}", cacheItemArgs, userId);
			Logger.LogError("GetFoldersByPermissionSortedCallBack(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);
			Logger.LogWarning("<<< Exiting GetFoldersByPermissionSortedCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return CBO.Instance.FillCollection<FolderInfo>(DataProvider.Instance().GetFoldersByPortalAndPermissions(portalId, permissions, userId));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="cacheItemArgs">The cache item arguments, <see cref="CacheItemArgs"/>.</param>
        /// <returns>A list of folders.</returns>
        internal virtual object GetFoldersSortedCallBack(CacheItemArgs cacheItemArgs)
        {
			Logger.LogError(">>> Entering GetFoldersSortedCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            var portalId = (int)cacheItemArgs.ParamList[0];
			Logger.LogError("GetFoldersSortedCallBack(cacheItemArgs={CacheItemArgs}): portalId → {PortalId}", cacheItemArgs, portalId);
			Logger.LogWarning("<<< Exiting GetFoldersSortedCallBack(cacheItemArgs={CacheItemArgs})", cacheItemArgs);
            return CBO.Instance.FillCollection<FolderInfo>(DataProvider.Instance().GetFoldersByPortal(portalId));
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="isRecursive">A value indicating whether the search should be recursive.</param>
        /// <returns>A sorted list of <see cref="MergedTreeItem"/>.</returns>
        internal virtual SortedList<string, MergedTreeItem> GetMergedTree(int portalId, string relativePath, bool isRecursive)
        {
            var fileSystemFolders = this.GetFileSystemFolders(portalId, relativePath, isRecursive);
            var databaseFolders = this.GetDatabaseFolders(portalId, relativePath, isRecursive);

            var mergedTree = this.MergeFolderLists(fileSystemFolders, databaseFolders);
            var mappedFolders = new SortedList<string, MergedTreeItem>();

            // Some providers cache the list of objects for performance
            this.ClearFolderProviderCachedLists(portalId);

            foreach (var mergedItem in mergedTree.Values)
            {
                if (mergedItem.FolderMappingID == Null.NullInteger)
                {
                    continue;
                }

                var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, mergedItem.FolderMappingID);

                // Add any folders from non-core providers
                if (folderMapping.MappingName != "Standard" && folderMapping.MappingName != "Secure" && folderMapping.MappingName != "Database")
                {
                    if (!isRecursive)
                    {
                        mergedItem.ExistsInFolderMapping = true;
                    }
                    else
                    {
                        var folder = this.GetFolder(portalId, mergedItem.FolderPath);
                        mappedFolders = this.MergeFolderLists(mappedFolders, this.GetFolderMappingFoldersRecursive(folderMapping, folder));
                    }
                }
                else
                {
                    mergedItem.ExistsInFolderMapping = folderMapping.MappingName == "Database" ? mergedItem.ExistsInDatabase : mergedItem.ExistsInFileSystem;
                }
            }

            mergedTree = this.MergeFolderLists(mergedTree, mappedFolders);

            // Update ExistsInFolderMapping if the Parent Does Not ExistsInFolderMapping
            var mergedTreeItems = mergedTree.Values;
            foreach (var mergedItem in mergedTreeItems.Where(m => m.ExistsInFolderMapping
                                       && mergedTreeItems.Any(mt2 => !mt2.ExistsInFolderMapping && m.FolderPath.StartsWith(mt2.FolderPath, StringComparison.OrdinalIgnoreCase))))
            {
                mergedItem.ExistsInFolderMapping = false;
            }

            return mergedTree;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folderMapping">The folder mapping.</param>
        /// <returns>A value indicating whether the folder mapping is editable.</returns>
        internal virtual bool IsFolderMappingEditable(FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering IsFolderMappingEditable(folderMapping=<redacted>)");
            Logger.LogWarning("<<< Exiting IsFolderMappingEditable(folderMapping=<redacted>)");
            return folderMapping.IsEditable;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folderToMove">The folder to move.</param>
        /// <param name="destinationFolder">The destination folder to move the folder to.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        /// <returns>A value indicating whether the move operation would be valid.</returns>
        internal virtual bool IsMoveOperationValid(IFolderInfo folderToMove, IFolderInfo destinationFolder, string newFolderPath)
        {
			Logger.LogError(">>> Entering IsMoveOperationValid(folderToMove={FolderToMove},destinationFolder={DestinationFolder},newFolderPath={NewFolderPath})", folderToMove, destinationFolder, newFolderPath);
            // FolderMapping cases
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folderToMove.PortalID, folderToMove.FolderMappingID);
			Logger.LogError("IsMoveOperationValid(folderToMove={FolderToMove},destinationFolder={DestinationFolder},newFolderPath={NewFolderPath}): folderMapping → {FolderMapping}", folderToMove, destinationFolder, newFolderPath, folderMapping);
            if (folderToMove.FolderMappingID != destinationFolder.FolderMappingID ||
                !FolderProvider.Instance(folderMapping.FolderProviderType).SupportsMappedPaths)
            {
                return this.IsMoveOperationValid(folderToMove, newFolderPath);
            }

            // Root mapped folder cannot be moved, when folder mappings are equal
            if (folderToMove.MappedPath == string.Empty)
            {
                return false;
            }

            // Destination folder cannot be a child mapped folder from the folder to move
			Logger.LogWarning("<<< Exiting IsMoveOperationValid(folderToMove={FolderToMove},destinationFolder={DestinationFolder},newFolderPath={NewFolderPath})", folderToMove, destinationFolder, newFolderPath);
			Logger.LogWarning("<<< Exiting IsMoveOperationValid(folderToMove={FolderToMove},newFolderPath={NewFolderPath})", folderToMove, newFolderPath);
            return !destinationFolder.MappedPath.StartsWith(folderToMove.MappedPath, StringComparison.OrdinalIgnoreCase) && this.IsMoveOperationValid(folderToMove, newFolderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folderToMove">The folder to move.</param>
        /// <param name="newFolderPath">The destination folder to move the folder into.</param>
        /// <returns>A value indicating if the move operation would be valid.</returns>
        internal virtual bool IsMoveOperationValid(IFolderInfo folderToMove, string newFolderPath)
        {
            // Root folder cannot be moved
            if (folderToMove.FolderPath == string.Empty)
            {
                return false;
            }

            // newParentFolder cannot be a child of folderToMove
            return !newFolderPath.StartsWith(folderToMove.FolderPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>A value indicating whether the network is available.</returns>
        internal virtual bool IsNetworkAvailable()
        {
			Logger.LogError(">>> Entering IsNetworkAvailable()");
            Logger.LogWarning("<<< Exiting IsNetworkAvailable()");
            return NetworkInterface.GetIsNetworkAvailable();
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="list1">The first list for the merge.</param>
        /// <param name="list2">The second list for the merge.</param>
        /// <returns>A merged sorted list of MergedTreeItems, <see cref="MergedTreeItem" />.</returns>
        internal virtual SortedList<string, MergedTreeItem> MergeFolderLists(SortedList<string, MergedTreeItem> list1, SortedList<string, MergedTreeItem> list2)
        {
            foreach (var item in list2.Values)
            {
                if (list1.TryGetValue(item.FolderPath, out var existingItem))
                {
                    if (existingItem.FolderID < 0)
                    {
                        existingItem.FolderID = item.FolderID;
                    }

                    if (existingItem.FolderMappingID < 0)
                    {
                        existingItem.FolderMappingID = item.FolderMappingID;
                    }

                    if (string.IsNullOrEmpty(existingItem.MappedPath))
                    {
                        existingItem.MappedPath = item.MappedPath;
                    }

                    existingItem.ExistsInFileSystem = existingItem.ExistsInFileSystem || item.ExistsInFileSystem;
                    existingItem.ExistsInDatabase = existingItem.ExistsInDatabase || item.ExistsInDatabase;
                    existingItem.ExistsInFolderMapping = existingItem.ExistsInFolderMapping || item.ExistsInFolderMapping;
                }
                else
                {
                    list1.Add(item.FolderPath, item);
                }
            }

            return list1;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="source">The source directory.</param>
        /// <param name="target">The target directory.</param>
        internal virtual void MoveDirectory(string source, string target)
        {
			Logger.LogError(">>> Entering MoveDirectory(source={Source},target={Target})", source, target);
            var stack = new Stack<MoveFoldersInfo>();
            stack.Push(new MoveFoldersInfo(source, target));
			Logger.LogError("MoveDirectory(source={Source},target={Target}): stack → {Stack}", source, target, stack);

            // ReSharper disable AssignNullToNotNullAttribute
            while (stack.Count > 0)
            {
                var folders = stack.Pop();
				Logger.LogError("MoveDirectory(source={Source},target={Target}): folders → {Folders}", source, target, folders);
                Directory.CreateDirectory(folders.Target);
                foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
                {
                    var targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
					Logger.LogError("MoveDirectory(source={Source},target={Target}): targetFile → {TargetFile}", source, target, targetFile);
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }

                    File.Move(file, targetFile);
                }

                foreach (var folder in Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new MoveFoldersInfo(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
                }
            }

            // ReSharper restore AssignNullToNotNullAttribute
            Directory.Delete(source, true);
			Logger.LogWarning("<<< Exiting MoveDirectory(source={Source},target={Target})", source, target);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folder">The folder to move.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        internal virtual void MoveFolderWithinProvider(IFolderInfo folder, IFolderInfo destinationFolder)
        {
			Logger.LogError(">>> Entering MoveFolderWithinProvider(folder={Folder},destinationFolder={DestinationFolder})", folder, destinationFolder);
            var newFolderPath = destinationFolder.FolderPath + folder.FolderName + "/";
            this.RenameFolderInFileSystem(folder, newFolderPath);
			Logger.LogError("MoveFolderWithinProvider(folder={Folder},destinationFolder={DestinationFolder}): newFolderPath → {NewFolderPath}", folder, destinationFolder, newFolderPath);

            // Update provider
            var newMappedPath = destinationFolder.MappedPath + folder.FolderName + "/";
			Logger.LogError("MoveFolderWithinProvider(folder={Folder},destinationFolder={DestinationFolder}): newMappedPath → {NewMappedPath}", folder, destinationFolder, newMappedPath);
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogError("MoveFolderWithinProvider(folder={Folder},destinationFolder={DestinationFolder}): folderMapping → {FolderMapping}", folder, destinationFolder, folderMapping);
            var provider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("MoveFolderWithinProvider(folder={Folder},destinationFolder={DestinationFolder}): provider → {Provider}", folder, destinationFolder, provider);
            provider.MoveFolder(folder.MappedPath, newMappedPath, folderMapping);

            // Update database
            this.UpdateChildFolders(folder, Path.Combine(destinationFolder.FolderPath, folder.FolderName));
			Logger.LogWarning("<<< Exiting MoveFolderWithinProvider(folder={Folder},destinationFolder={DestinationFolder})", folder, destinationFolder);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folder">The folder to move.</param>
        /// <param name="newFolderPath">The target folder for the move.</param>
        internal virtual void MoveFolderBetweenProviders(IFolderInfo folder, string newFolderPath)
        {
			Logger.LogError(">>> Entering MoveFolderBetweenProviders(folder={Folder},newFolderPath={NewFolderPath})", folder, newFolderPath);
            this.RenameFolderInFileSystem(folder, newFolderPath);

            var folderInfos = this.GetFolders(folder.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(folder.FolderPath, StringComparison.OrdinalIgnoreCase)).ToArray();
			Logger.LogError("MoveFolderBetweenProviders(folder={Folder},newFolderPath={NewFolderPath}): folderInfos → {FolderInfos}", folder, newFolderPath, folderInfos);
            var tmpFolderPath = folder.FolderPath;

            foreach (var folderInfo in folderInfos)
            {
                var folderPath = newFolderPath + folderInfo.FolderPath.Substring(tmpFolderPath.Length);
				Logger.LogError("MoveFolderBetweenProviders(folder={Folder},newFolderPath={NewFolderPath}): folderPath → {FolderPath}", folder, newFolderPath, folderPath);

                var parentFolder = this.GetParentFolder(folder.PortalID, folderPath);
				Logger.LogError("MoveFolderBetweenProviders(folder={Folder},newFolderPath={NewFolderPath}): parentFolder → {ParentFolder}", folder, newFolderPath, parentFolder);
                folderInfo.ParentID = parentFolder.FolderID;
                folderInfo.FolderPath = folderPath;
                this.UpdateFolderInternal(folderInfo, true);
            }
			Logger.LogWarning("<<< Exiting MoveFolderBetweenProviders(folder={Folder},newFolderPath={NewFolderPath})", folder, newFolderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="sourceFolder">The source folder.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="folderMappings">The folder mappings.</param>
        /// <param name="foldersToDelete">The folders to delete.</param>
        internal virtual void OverwriteFolder(IFolderInfo sourceFolder, IFolderInfo destinationFolder, Dictionary<int, FolderMappingInfo> folderMappings, SortedList<string, IFolderInfo> foldersToDelete)
        {
			Logger.LogError(">>> Entering OverwriteFolder(sourceFolder={SourceFolder},destinationFolder={DestinationFolder},folderMappings=<redacted>,foldersToDelete={FoldersToDelete})", sourceFolder, destinationFolder, foldersToDelete);
            var fileManager = FileManager.Instance;
            var files = this.GetFiles(sourceFolder, true);
			Logger.LogError("OverwriteFolder(sourceFolder={SourceFolder},destinationFolder={DestinationFolder},folderMappings=<redacted>,foldersToDelete={FoldersToDelete}): files → {Files}", sourceFolder, destinationFolder, foldersToDelete, files);

            foreach (var file in files)
            {
                fileManager.MoveFile(file, destinationFolder);
            }

            // Delete source folder in database
            this.DeleteFolder(sourceFolder.PortalID, sourceFolder.FolderPath);

            var folderMapping = this.GetFolderMapping(folderMappings, sourceFolder.FolderMappingID);
			Logger.LogError("OverwriteFolder(sourceFolder={SourceFolder},destinationFolder={DestinationFolder},folderMappings=<redacted>,foldersToDelete={FoldersToDelete}): folderMapping → {FolderMapping}", sourceFolder, destinationFolder, foldersToDelete, folderMapping);

            if (this.IsFolderMappingEditable(folderMapping))
            {
                foldersToDelete.Add(sourceFolder.FolderPath, sourceFolder);
            }
			Logger.LogWarning("<<< Exiting OverwriteFolder(sourceFolder={SourceFolder},destinationFolder={DestinationFolder},folderMappings=<redacted>,foldersToDelete={FoldersToDelete})", sourceFolder, destinationFolder, foldersToDelete);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="item">The item to process.</param>
        /// <param name="portalId">The site (portal) ID.</param>
        internal virtual void ProcessMergedTreeItemInAddMode(MergedTreeItem item, int portalId)
        {
			Logger.LogError(">>> Entering ProcessMergedTreeItemInAddMode(item={Item},portalId={PortalId})", item, portalId);
            try
            {
                if (item.ExistsInFileSystem)
                {
                    if (!item.ExistsInDatabase)
                    {
                        var folderMappingId = this.FindFolderMappingId(item, portalId);
                        this.CreateFolderInDatabase(portalId, item.FolderPath, folderMappingId);
                    }
                }
                else
                {
                    if (item.ExistsInDatabase)
                    {
                        if (item.ExistsInFolderMapping)
                        {
                            this.CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath));
                        }
                    }
                    else
                    {
                        // by exclusion, it exists in the Folder Mapping
                        this.CreateFolderInFileSystem(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath));
                        this.CreateFolderInDatabase(portalId, item.FolderPath, item.FolderMappingID, item.MappedPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Could not create folder {item.FolderPath}. EXCEPTION: {ex.Message}", ex);
            }
			Logger.LogWarning("<<< Exiting ProcessMergedTreeItemInAddMode(item={Item},portalId={PortalId})", item, portalId);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="item"><see cref="MergedTreeItem"></see>.</param>
        /// <param name="portalId">The site (portal) ID.</param>
        internal virtual void ProcessMergedTreeItemInDeleteMode(MergedTreeItem item, int portalId)
        {
			Logger.LogError(">>> Entering ProcessMergedTreeItemInDeleteMode(item={Item},portalId={PortalId})", item, portalId);
            if (item.ExistsInFileSystem)
            {
                if (!item.ExistsInDatabase)
                {
                    return;
                }

                if (item.FolderPath == string.Empty)
                {
                    return; // Do not process root folder
                }

                if (item.ExistsInFolderMapping)
                {
                    return;
                }

                var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, item.FolderMappingID);
				Logger.LogError("ProcessMergedTreeItemInDeleteMode(item={Item},portalId={PortalId}): folderMapping → {FolderMapping}", item, portalId, folderMapping);
                if (!folderMapping.IsEditable)
                {
                    return;
                }

                DirectoryWrapper.Instance.Delete(PathUtils.Instance.GetPhysicalPath(portalId, item.FolderPath), false);
                this.DeleteFolder(portalId, item.FolderPath);
            }
            else
            {
                if (item.ExistsInDatabase && !item.ExistsInFolderMapping)
                {
                    this.DeleteFolder(portalId, item.FolderPath);
                }
            }
			Logger.LogWarning("<<< Exiting ProcessMergedTreeItemInDeleteMode(item={Item},portalId={PortalId})", item, portalId);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folder">The folder to clean up.</param>
        internal virtual void RemoveOrphanedFiles(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering RemoveOrphanedFiles(folder={Folder})", folder);
            var files = this.GetFiles(folder, false, true);

			Logger.LogError("RemoveOrphanedFiles(folder={Folder}): files → {Files}", folder, files);
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogError("RemoveOrphanedFiles(folder={Folder}): folderMapping → {FolderMapping}", folder, folderMapping);

            if (folderMapping != null)
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
				Logger.LogError("RemoveOrphanedFiles(folder={Folder}): folderProvider → {FolderProvider}", folder, folderProvider);

                foreach (var file in files)
                {
                    try
                    {
                        if (!folderProvider.FileExists(folder, file.FileName))
                        {
                            FileDeletionController.Instance.DeleteFileData(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting RemoveOrphanedFiles(folder={Folder})", folder);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folder">The folder to rename.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        internal virtual void RenameFolderInFileSystem(IFolderInfo folder, string newFolderPath)
        {
			Logger.LogError(">>> Entering RenameFolderInFileSystem(folder={Folder},newFolderPath={NewFolderPath})", folder, newFolderPath);
            var source = folder.PhysicalPath;

            var di = new DirectoryInfo(source);
			Logger.LogError("RenameFolderInFileSystem(folder={Folder},newFolderPath={NewFolderPath}): di → {Di}", folder, newFolderPath, di);
            if (!di.Exists)
            {
                return;
            }

            var target = PathUtils.Instance.GetPhysicalPath(folder.PortalID, newFolderPath);
			Logger.LogError("RenameFolderInFileSystem(folder={Folder},newFolderPath={NewFolderPath}): target → {Target}", folder, newFolderPath, target);
            this.MoveDirectory(source, target);
			Logger.LogWarning("<<< Exiting RenameFolderInFileSystem(folder={Folder},newFolderPath={NewFolderPath})", folder, newFolderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folder">The folder to save the permissions for.</param>
        internal virtual void SaveFolderPermissions(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering SaveFolderPermissions(folder={Folder})", folder);
            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
			Logger.LogWarning("<<< Exiting SaveFolderPermissions(folder={Folder})", folder);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="timeout">The timeout in seconds.</param>
        internal virtual void SetScriptTimeout(int timeout)
        {
			Logger.LogError(">>> Entering SetScriptTimeout(timeout={Timeout})", timeout);
            HttpContext.Current.Server.ScriptTimeout = timeout;
			Logger.LogWarning("<<< Exiting SetScriptTimeout(timeout={Timeout})", timeout);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="item">The item to synchronize.</param>
        /// <param name="portalId">The site (portal) ID.</param>
        internal virtual void SynchronizeFiles(MergedTreeItem item, int portalId)
        {
			Logger.LogError(">>> Entering SynchronizeFiles(item={Item},portalId={PortalId})", item, portalId);
            var folder = this.GetFolder(portalId, item.FolderPath);
			Logger.LogError("SynchronizeFiles(item={Item},portalId={PortalId}): folder → {Folder}", item, portalId, folder);
            if (folder is null)
            {
                return;
            }

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folder.FolderMappingID);
			Logger.LogError("SynchronizeFiles(item={Item},portalId={PortalId}): folderMapping → {FolderMapping}", item, portalId, folderMapping);
            if (folderMapping is null)
            {
                return;
            }

            try
            {
                var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
				Logger.LogError("SynchronizeFiles(item={Item},portalId={PortalId}): folderProvider → {FolderProvider}", item, portalId, folderProvider);
                var fileManager = FileManager.Instance;

                if (folderProvider.FolderExists(folder.MappedPath, folderMapping))
                {
                    var files = folderProvider.GetFiles(folder);

                    files = files.Except(FileVersionController.Instance.GetFileVersionsInFolder(folder.FolderID).Select(f => f.FileName)).ToArray();
					Logger.LogError("SynchronizeFiles(item={Item},portalId={PortalId}): files → {Files}", item, portalId, files);

                    foreach (var fileName in files)
                    {
                        try
                        {
                            var file = fileManager.GetFile(folder, fileName, true);
							Logger.LogError("SynchronizeFiles(item={Item},portalId={PortalId}): file → {File}", item, portalId, file);
                            if (file is null)
                            {
                                const bool operationDoesNotRequirePermissionsCheck = true;
                                fileManager.AddFile(folder, fileName, null, false, !operationDoesNotRequirePermissionsCheck, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)));
                            }
                            else if (!folderProvider.IsInSync(file))
                            {
                                fileManager.UpdateFile(file, null);
                            }
                        }
                        catch (InvalidFileExtensionException ex)
                        {
                            Logger.Info(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }

                this.RemoveOrphanedFiles(folder);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting SynchronizeFiles(item={Item},portalId={PortalId})", item, portalId);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The site (portal) ID.</param>
        /// <param name="folderPath">The folder path to update.</param>
        internal virtual void UpdateParentFolder(int portalId, string folderPath)
        {
			Logger.LogError(">>> Entering UpdateParentFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
			Logger.LogError("UpdateParentFolder(portalId={PortalId},folderPath={FolderPath}): parentFolderPath → {ParentFolderPath}", portalId, folderPath, parentFolderPath);
            var objFolder = this.GetFolder(portalId, parentFolderPath);
			Logger.LogError("UpdateParentFolder(portalId={PortalId},folderPath={FolderPath}): objFolder → {ObjFolder}", portalId, folderPath, objFolder);
            if (objFolder != null)
            {
                // UpdateFolder(objFolder);
                this.UpdateFolderInternal(objFolder, false);
            }
			Logger.LogWarning("<<< Exiting UpdateParentFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="folder">The folder to update.</param>
        /// <param name="newFolderPath">The new folder path.</param>
        internal virtual void UpdateChildFolders(IFolderInfo folder, string newFolderPath)
        {
			Logger.LogError(">>> Entering UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath})", folder, newFolderPath);
            var originalFolderPath = folder.FolderPath;

            var folderInfos = this.GetFolders(folder.PortalID).Where(f => f.FolderPath != string.Empty && f.FolderPath.StartsWith(originalFolderPath, StringComparison.OrdinalIgnoreCase)).ToArray();
			Logger.LogError("UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath}): folderInfos → {FolderInfos}", folder, newFolderPath, folderInfos);

            foreach (var folderInfo in folderInfos)
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folderInfo.FolderMappingID);
				Logger.LogError("UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath}): folderMapping → {FolderMapping}", folder, newFolderPath, folderMapping);
                var provider = FolderProvider.Instance(folderMapping.FolderProviderType);
				Logger.LogError("UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath}): provider → {Provider}", folder, newFolderPath, provider);

                var folderPath = newFolderPath + (newFolderPath.EndsWith("/", StringComparison.Ordinal) ? string.Empty : "/") + folderInfo.FolderPath.Substring(originalFolderPath.Length);
				Logger.LogError("UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath}): folderPath → {FolderPath}", folder, newFolderPath, folderPath);

                var parentFolder = this.GetParentFolder(folder.PortalID, folderPath);
				Logger.LogError("UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath}): parentFolder → {ParentFolder}", folder, newFolderPath, parentFolder);
                folderInfo.ParentID = parentFolder.FolderID;
                folderInfo.FolderPath = folderPath;

                var parentProvider = FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(parentFolder.PortalID, parentFolder.FolderMappingID).FolderProviderType);
				Logger.LogError("UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath}): parentProvider → {ParentProvider}", folder, newFolderPath, parentProvider);
                if (parentProvider.SupportsMappedPaths || !provider.SupportsMappedPaths)
                {
                    if (provider.SupportsMappedPaths)
                    {
                        var mappedPath = parentFolder.FolderPath == string.Empty ? string.Empty : folderPath.Replace(parentFolder.FolderPath, string.Empty);
						Logger.LogError("UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath}): mappedPath → {MappedPath}", folder, newFolderPath, mappedPath);
                        folderInfo.MappedPath = PathUtils.Instance.FormatFolderPath(parentFolder.MappedPath + mappedPath);
                    }
                    else
                    {
                        folderInfo.MappedPath = folderPath;
                    }
                }
                else if (provider.SupportsMappedPaths)
                {
                    if (originalFolderPath == folderInfo.MappedPath)
                    {
                        folderInfo.MappedPath = folderPath;
                    }
                    else if (folderInfo.MappedPath.EndsWith("/" + originalFolderPath, StringComparison.Ordinal))
                    {
                        var newMappedPath = PathUtils.Instance.FormatFolderPath(
                        folderInfo.MappedPath.Substring(0, folderInfo.MappedPath.LastIndexOf("/" + originalFolderPath, StringComparison.Ordinal)) + "/" + folderPath);
                        folderInfo.MappedPath = newMappedPath;
                    }
                }

                this.UpdateFolderInternal(folderInfo, false);
            }

            this.ClearFolderCache(folder.PortalID);
			Logger.LogWarning("<<< Exiting UpdateChildFolders(folder={Folder},newFolderPath={NewFolderPath})", folder, newFolderPath);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="sourceFolderMapping">The source folder mapping.</param>
        /// <param name="destinationFolderMapping">The destination folder mapping.</param>
        /// <returns>A value indicating whether a folder mapping can be moved to another one.</returns>
        internal virtual bool CanMoveBetweenFolderMappings(FolderMappingInfo sourceFolderMapping, FolderMappingInfo destinationFolderMapping)
        {
			Logger.LogError(">>> Entering CanMoveBetweenFolderMappings(sourceFolderMapping=<redacted>,destinationFolderMapping=<redacted>)");
            // If Folder Mappings are exactly the same
            if (sourceFolderMapping.FolderMappingID == destinationFolderMapping.FolderMappingID)
            {
                return true;
            }

			Logger.LogWarning("<<< Exiting CanMoveBetweenFolderMappings(sourceFolderMapping=<redacted>,destinationFolderMapping=<redacted>)");
            return IsStandardFolderProviderType(sourceFolderMapping) && IsStandardFolderProviderType(destinationFolderMapping);
        }

        private static Regex WildcardToRegex(string pattern)
        {
			Logger.LogError(">>> Entering WildcardToRegex(pattern={Pattern})", pattern);
            pattern = Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".");
			Logger.LogError("WildcardToRegex(pattern={Pattern}): pattern → {Pattern1}", pattern, pattern);
			Logger.LogWarning("<<< Exiting WildcardToRegex(pattern={Pattern})", pattern);
            return RegexUtils.GetCachedRegex(pattern, RegexOptions.IgnoreCase);
        }

        private static bool IsStandardFolderProviderType(FolderMappingInfo folderMappingInfo)
        {
			Logger.LogError(">>> Entering IsStandardFolderProviderType(folderMappingInfo=<redacted>)");
            var compatibleTypes = new[] { "StandardFolderProvider", "SecureFolderProvider", "DatabaseFolderProvider" };
			Logger.LogWarning("<<< Exiting IsStandardFolderProviderType(folderMappingInfo=<redacted>)");
			Logger.LogError("IsStandardFolderProviderType(folderMappingInfo=<redacted>): compatibleTypes → {CompatibleTypes}", compatibleTypes);
            return compatibleTypes.Contains(folderMappingInfo.FolderProviderType);
        }

        private static string GetDefaultMappedPath(FolderMappingInfo folderMapping)
        {
			Logger.LogError(">>> Entering GetDefaultMappedPath(folderMapping=<redacted>)");
            var defaultMappedPath = folderMapping.FolderMappingSettings[DefaultMappedPathSetting];
            if (defaultMappedPath is null)
            {
                return string.Empty;
            }

			Logger.LogWarning("<<< Exiting GetDefaultMappedPath(folderMapping=<redacted>)");
            return defaultMappedPath.ToString();
        }

        private static void OnFolderMoved(IFolderInfo folderInfo, int userId, string oldFolderPath)
        {
			Logger.LogError(">>> Entering OnFolderMoved(folderInfo={FolderInfo},userId={UserId},oldFolderPath={OldFolderPath})", folderInfo, userId, oldFolderPath);
            EventManager.Instance.OnFolderMoved(new FolderMovedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
                OldFolderPath = oldFolderPath,
            });
			Logger.LogWarning("<<< Exiting OnFolderMoved(folderInfo={FolderInfo},userId={UserId},oldFolderPath={OldFolderPath})", folderInfo, userId, oldFolderPath);
        }

        private static void OnFolderRenamed(IFolderInfo folderInfo, int userId, string oldFolderName)
        {
			Logger.LogError(">>> Entering OnFolderRenamed(folderInfo={FolderInfo},userId={UserId},oldFolderName={OldFolderName})", folderInfo, userId, oldFolderName);
            EventManager.Instance.OnFolderRenamed(new FolderRenamedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
                OldFolderName = oldFolderName,
            });
			Logger.LogWarning("<<< Exiting OnFolderRenamed(folderInfo={FolderInfo},userId={UserId},oldFolderName={OldFolderName})", folderInfo, userId, oldFolderName);
        }

        private static void OnFolderDeleted(IFolderInfo folderInfo, int userId, bool isCascadeDeleting)
        {
			Logger.LogError(">>> Entering OnFolderDeleted(folderInfo={FolderInfo},userId={UserId},isCascadeDeleting={IsCascadeDeleting})", folderInfo, userId, isCascadeDeleting);
            EventManager.Instance.OnFolderDeleted(new FolderDeletedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
                IsCascadeDeletng = isCascadeDeleting,
            });
			Logger.LogWarning("<<< Exiting OnFolderDeleted(folderInfo={FolderInfo},userId={UserId},isCascadeDeleting={IsCascadeDeleting})", folderInfo, userId, isCascadeDeleting);
        }

        private static void OnFolderAdded(IFolderInfo folderInfo, int userId)
        {
			Logger.LogError(">>> Entering OnFolderAdded(folderInfo={FolderInfo},userId={UserId})", folderInfo, userId);
            EventManager.Instance.OnFolderAdded(new FolderChangedEventArgs
            {
                FolderInfo = folderInfo,
                UserId = userId,
            });
			Logger.LogWarning("<<< Exiting OnFolderAdded(folderInfo={FolderInfo},userId={UserId})", folderInfo, userId);
        }

        private static void OnFileDeleted(IFileInfo fileInfo, int userId, bool isCascadeDeleting)
        {
			Logger.LogError(">>> Entering OnFileDeleted(fileInfo={FileInfo},userId={UserId},isCascadeDeleting={IsCascadeDeleting})", fileInfo, userId, isCascadeDeleting);
            EventManager.Instance.OnFileDeleted(new FileDeletedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                IsCascadeDeleting = isCascadeDeleting,
            });
			Logger.LogWarning("<<< Exiting OnFileDeleted(fileInfo={FileInfo},userId={UserId},isCascadeDeleting={IsCascadeDeleting})", fileInfo, userId, isCascadeDeleting);
        }

        private static FolderPermissionCollection GetFolderPermissionsFromSyncData(int portalId, string relativePath)
        {
			Logger.LogError(">>> Entering GetFolderPermissionsFromSyncData(portalId={PortalId},relativePath={RelativePath})", portalId, relativePath);
            var threadId = Environment.CurrentManagedThreadId;
            FolderPermissionCollection permissions = null;
			Logger.LogError("GetFolderPermissionsFromSyncData(portalId={PortalId},relativePath={RelativePath}): permissions → {Permissions}", portalId, relativePath, permissions);
            if (SyncFoldersData.ContainsKey(threadId))
            {
                if (SyncFoldersData[threadId].FolderPath == relativePath && SyncFoldersData[threadId].PortalId == portalId)
                {
                    return SyncFoldersData[threadId].Permissions;
                }

                permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalId, relativePath);
                SyncFoldersData[threadId] = new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions };
                return permissions;
            }

            permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalId, relativePath);
            SyncFoldersData.TryAdd(threadId, new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions });

			Logger.LogWarning("<<< Exiting GetFolderPermissionsFromSyncData(portalId={PortalId},relativePath={RelativePath})", portalId, relativePath);
            return permissions;
        }

        private static void InitialiseSyncFoldersData(int portalId, string relativePath)
        {
			Logger.LogError(">>> Entering InitialiseSyncFoldersData(portalId={PortalId},relativePath={RelativePath})", portalId, relativePath);
            var threadId = Environment.CurrentManagedThreadId;
            var permissions = FolderPermissionController.GetFolderPermissionsCollectionByFolder(portalId, relativePath);
			Logger.LogError("InitialiseSyncFoldersData(portalId={PortalId},relativePath={RelativePath}): permissions → {Permissions}", portalId, relativePath, permissions);
            if (SyncFoldersData.ContainsKey(threadId))
            {
                if (SyncFoldersData[threadId].FolderPath == relativePath && SyncFoldersData[threadId].PortalId == portalId)
                {
                    SyncFoldersData[threadId].Permissions = permissions;
                }
                else
                {
                    SyncFoldersData[threadId] = new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions };
                }
            }
            else
            {
                SyncFoldersData.TryAdd(threadId, new SyncFolderData { PortalId = portalId, FolderPath = relativePath, Permissions = permissions });
            }
			Logger.LogWarning("<<< Exiting InitialiseSyncFoldersData(portalId={PortalId},relativePath={RelativePath})", portalId, relativePath);
        }

        private static void RemoveSyncFoldersData()
        {
			Logger.LogError(">>> Entering RemoveSyncFoldersData()");
            var threadId = Environment.CurrentManagedThreadId;
            if (SyncFoldersData.ContainsKey(threadId))
            {
                SyncFoldersData.TryRemove(threadId, out _);
            }
			Logger.LogWarning("<<< Exiting RemoveSyncFoldersData()");
        }

        private int AddFolderInternal(FolderInfo folder)
        {
			Logger.LogError(">>> Entering AddFolderInternal(folder={Folder})", folder);
            // Check this is not a duplicate
            var existingFolder = this.GetFolder(folder.PortalID, folder.FolderPath);
			Logger.LogError("AddFolderInternal(folder={Folder}): existingFolder → {ExistingFolder}", folder, existingFolder);
            if (existingFolder is not null && folder.FolderID == Null.NullInteger)
            {
                folder.FolderID = existingFolder.FolderID;
				Logger.LogDebug("AddFolderInternal(folder={Folder}): folder.FolderID → {Arg1}", folder, folder.FolderID);
            }

            if (folder.FolderID == Null.NullInteger)
            {
                var isVersioned = folder.IsVersioned;
				Logger.LogError("AddFolderInternal(folder={Folder}): isVersioned → {IsVersioned}", folder, isVersioned);
                var workflowId = folder.WorkflowID;
				Logger.LogError("AddFolderInternal(folder={Folder}): workflowId → {WorkflowId}", folder, workflowId);

                // Inherit some configuration from its Parent Folder
                var parentFolder = this.GetParentFolder(folder.PortalID, folder.FolderPath);
				Logger.LogError("AddFolderInternal(folder={Folder}): parentFolder → {ParentFolder}", folder, parentFolder);
                var parentId = Null.NullInteger;
				Logger.LogError("AddFolderInternal(folder={Folder}): parentId → {ParentId}", folder, parentId);
                if (parentFolder != null)
                {
                    isVersioned = parentFolder.IsVersioned;
                    workflowId = parentFolder.WorkflowID;
                    parentId = parentFolder.FolderID;
                }

                folder.FolderPath = PathUtils.Instance.FormatFolderPath(folder.FolderPath);
                folder.FolderID = DataProvider.Instance().AddFolder(
                    folder.PortalID,
                    folder.UniqueId,
                    folder.VersionGuid,
                    folder.FolderPath,
                    folder.MappedPath,
                    folder.StorageLocation,
                    folder.IsProtected,
                    folder.IsCached,
                    folder.LastUpdated,
                    this.GetCurrentUserId(),
                    folder.FolderMappingID,
                    isVersioned,
                    workflowId,
                    parentId);

                // Refetch folder for logging
                this.AddLogEntry(this.GetFolder(folder.PortalID, folder.FolderPath), EventLogType.FOLDER_CREATED);

                if (parentFolder != null)
                {
                    this.UpdateFolderInternal(parentFolder, false);
                }
                else
                {
                    this.UpdateParentFolder(folder.PortalID, folder.FolderPath);
                }
            }
            else
            {
                var parentFolder = this.GetParentFolder(folder.PortalID, folder.FolderPath);
                if (parentFolder != null)
                {
                    // Ensure that Parent ID is repaired
                    folder.ParentID = parentFolder.FolderID;
					Logger.LogDebug("AddFolderInternal(folder={Folder}): folder.ParentID → {Arg1}", folder, folder.ParentID);
                }

                this.UpdateFolderInternal(folder, false);
            }

            // Invalidate Cache
            this.ClearFolderCache(folder.PortalID);

			Logger.LogWarning("<<< Exiting AddFolderInternal(folder={Folder})", folder);
            return folder.FolderID;
        }

        private bool GetOnlyUnmap(IFolderInfo folder)
        {
			Logger.LogError(">>> Entering GetOnlyUnmap(folder={Folder})", folder);
            if (folder == null || folder.ParentID == Null.NullInteger)
            {
                return true;
            }

			Logger.LogWarning("<<< Exiting GetOnlyUnmap(folder={Folder})", folder);
            return FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(folder.FolderMappingID).FolderProviderType).SupportsMappedPaths &&
                this.GetFolder(folder.ParentID).FolderMappingID != folder.FolderMappingID;
        }

        private void UnmapFolderInternal(IFolderInfo folder, bool isCascadeDeleting)
        {
			Logger.LogError(">>> Entering UnmapFolderInternal(folder={Folder},isCascadeDeleting={IsCascadeDeleting})", folder, isCascadeDeleting);
            Requires.NotNull("folder", folder);

            if (DirectoryWrapper.Instance.Exists(folder.PhysicalPath))
            {
                DirectoryWrapper.Instance.Delete(folder.PhysicalPath, true);
            }

            this.DeleteFolder(folder.PortalID, folder.FolderPath);

            // Notify folder deleted event
            OnFolderDeleted(folder, this.GetCurrentUserId(), isCascadeDeleting);
			Logger.LogWarning("<<< Exiting UnmapFolderInternal(folder={Folder},isCascadeDeleting={IsCascadeDeleting})", folder, isCascadeDeleting);
        }

        private void DeleteFolderInternal(IFolderInfo folder, bool isCascadeDeleting)
        {
			Logger.LogError(">>> Entering DeleteFolderInternal(folder={Folder},isCascadeDeleting={IsCascadeDeleting})", folder, isCascadeDeleting);
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
			Logger.LogError("DeleteFolderInternal(folder={Folder},isCascadeDeleting={IsCascadeDeleting}): folderMapping → {FolderMapping}", folder, isCascadeDeleting, folderMapping);

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).DeleteFolder(folder);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(
                    Localization.GetExceptionMessage(
                        "DeleteFolderUnderlyingSystemError",
                        "The underlying system threw an exception. The folder has not been deleted."),
                    ex);
            }

            if (DirectoryWrapper.Instance.Exists(folder.PhysicalPath))
            {
                DirectoryWrapper.Instance.Delete(folder.PhysicalPath, true);
            }

            this.DeleteFolder(folder.PortalID, folder.FolderPath);

            // Notify folder deleted event
            OnFolderDeleted(folder, this.GetCurrentUserId(), isCascadeDeleting);
			Logger.LogWarning("<<< Exiting DeleteFolderInternal(folder={Folder},isCascadeDeleting={IsCascadeDeleting})", folder, isCascadeDeleting);
        }

        private IFolderInfo GetParentFolder(int portalId, string folderPath)
        {
			Logger.LogError(">>> Entering GetParentFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            if (string.IsNullOrEmpty(folderPath))
            {
                return null;
            }

            var parentFolderPath = folderPath.Substring(0, folderPath.Substring(0, folderPath.Length - 1).LastIndexOf("/", StringComparison.Ordinal) + 1);
			Logger.LogError("GetParentFolder(portalId={PortalId},folderPath={FolderPath}): parentFolderPath → {ParentFolderPath}", portalId, folderPath, parentFolderPath);
			Logger.LogWarning("<<< Exiting GetParentFolder(portalId={PortalId},folderPath={FolderPath})", portalId, folderPath);
            return this.GetFolder(portalId, parentFolderPath);
        }

        private IEnumerable<IFileInfo> SearchFiles(IFolderInfo folder, Regex regex, bool recursive)
        {
            // Get all files in one DB call
            var fileCollection = CBO.Instance.FillCollection<FileInfo>(
                DataProvider.Instance().GetFiles(folder.FolderID, false, recursive));

            if (!recursive)
            {
                return fileCollection.Where(f => regex.IsMatch(f.FileName));
            }

            // Pre-compute allowed folders once
            var allowedFolderPaths = this.GetFolders(folder.PortalID)
                .Where(f => f.FolderPath.StartsWith(folder.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                           FolderPermissionController.Instance.CanViewFolder(f))
                .Select(f => f.FolderPath)
                .ToHashSet();

            // Simple filter with folder permission lookup
            return fileCollection
                .Where(f => allowedFolderPaths.Contains(f.Folder) && regex.IsMatch(f.FileName));
        }

        private IFolderInfo UpdateFolderInternal(IFolderInfo folder, bool clearCache)
        {
			Logger.LogError(">>> Entering UpdateFolderInternal(folder={Folder},clearCache={ClearCache})", folder, clearCache);
            Requires.NotNull("folder", folder);

            DataProvider.Instance().UpdateFolder(
                folder.PortalID,
                folder.VersionGuid,
                folder.FolderID,
                PathUtils.Instance.FormatFolderPath(folder.FolderPath),
                folder.StorageLocation,
                folder.MappedPath,
                folder.IsProtected,
                folder.IsCached,
                folder.LastUpdated,
                this.GetCurrentUserId(),
                folder.FolderMappingID,
                folder.IsVersioned,
                folder.WorkflowID,
                folder.ParentID);

            if (clearCache)
            {
                this.ClearFolderCache(folder.PortalID);
            }

			Logger.LogWarning("<<< Exiting UpdateFolderInternal(folder={Folder},clearCache={ClearCache})", folder, clearCache);
            return folder;
        }

        private int FindFolderMappingId(MergedTreeItem item, int portalId)
        {
			Logger.LogError(">>> Entering FindFolderMappingId(item={Item},portalId={PortalId})", item, portalId);
            if (item.ExistsInFolderMapping)
            {
                return item.FolderMappingID;
            }

            if (item.FolderPath.IndexOf('/') != item.FolderPath.LastIndexOf('/'))
            {
                var parentPath = item.FolderPath.Substring(0, item.FolderPath.TrimEnd('/').LastIndexOf('/') + 1);
				Logger.LogError("FindFolderMappingId(item={Item},portalId={PortalId}): parentPath → {ParentPath}", item, portalId, parentPath);
                var folder = this.GetFolder(portalId, parentPath);
				Logger.LogError("FindFolderMappingId(item={Item},portalId={PortalId}): folder → {Folder}", item, portalId, folder);
                if (folder != null)
                {
                    return folder.FolderMappingID;
                }
            }

			Logger.LogWarning("<<< Exiting FindFolderMappingId(item={Item},portalId={PortalId})", item, portalId);
            return FolderMappingController.Instance.GetDefaultFolderMapping(portalId).FolderMappingID;
        }

        private bool DeleteFolderRecursive(IFolderInfo folder, ICollection<IFolderInfo> notDeletedSubfolders, bool isRecursiveDeletionFolder, bool unmap)
        {
			Logger.LogError(">>> Entering DeleteFolderRecursive(folder={Folder},notDeletedSubfolders={NotDeletedSubfolders},isRecursiveDeletionFolder={IsRecursiveDeletionFolder},unmap={Unmap})", folder, notDeletedSubfolders, isRecursiveDeletionFolder, unmap);
            Requires.NotNull("folder", folder);

            if (UserSecurityController.Instance.HasFolderPermission(folder, "DELETE"))
            {
                var subfolders = this.GetFolders(folder);
				Logger.LogError("DeleteFolderRecursive(folder={Folder},notDeletedSubfolders={NotDeletedSubfolders},isRecursiveDeletionFolder={IsRecursiveDeletionFolder},unmap={Unmap}): subfolders → {Subfolders}", folder, notDeletedSubfolders, isRecursiveDeletionFolder, unmap, subfolders);

                var allSubFoldersHasBeenDeleted = true;
				Logger.LogError("DeleteFolderRecursive(folder={Folder},notDeletedSubfolders={NotDeletedSubfolders},isRecursiveDeletionFolder={IsRecursiveDeletionFolder},unmap={Unmap}): allSubFoldersHasBeenDeleted → {AllSubFoldersHasBeenDeleted}", folder, notDeletedSubfolders, isRecursiveDeletionFolder, unmap, allSubFoldersHasBeenDeleted);

                foreach (var subfolder in subfolders)
                {
                    if (!this.DeleteFolderRecursive(subfolder, notDeletedSubfolders, false, unmap || this.GetOnlyUnmap(subfolder)))
                    {
                        allSubFoldersHasBeenDeleted = false;
                    }
                }

                var files = this.GetFiles(folder, false, true);
				Logger.LogError("DeleteFolderRecursive(folder={Folder},notDeletedSubfolders={NotDeletedSubfolders},isRecursiveDeletionFolder={IsRecursiveDeletionFolder},unmap={Unmap}): files → {Files}", folder, notDeletedSubfolders, isRecursiveDeletionFolder, unmap, files);
                foreach (var file in files)
                {
                    if (unmap)
                    {
                        FileDeletionController.Instance.UnlinkFile(file);
                    }
                    else
                    {
                        FileDeletionController.Instance.DeleteFile(file);
                    }

                    OnFileDeleted(file, this.GetCurrentUserId(), true);
                }

                if (allSubFoldersHasBeenDeleted)
                {
                    if (unmap)
                    {
                        this.UnmapFolderInternal(folder, !isRecursiveDeletionFolder);
                    }
                    else
                    {
                        this.DeleteFolderInternal(folder, !isRecursiveDeletionFolder);
                    }

                    return true;
                }
            }

            notDeletedSubfolders.Add(folder);
			Logger.LogWarning("<<< Exiting DeleteFolderRecursive(folder={Folder},notDeletedSubfolders={NotDeletedSubfolders},isRecursiveDeletionFolder={IsRecursiveDeletionFolder},unmap={Unmap})", folder, notDeletedSubfolders, isRecursiveDeletionFolder, unmap);
            return false;
        }

        private IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder, bool allSubFolders)
        {
            Requires.NotNull("parentFolder", parentFolder);

            var folders = this.GetFolders(parentFolder.PortalID);
            if (allSubFolders)
            {
                folders =
                    folders
                        .Where(f =>
                            f.FolderPath.StartsWith(
                                parentFolder.FolderPath,
                                StringComparison.InvariantCultureIgnoreCase));
            }

            return folders.Where(f => f.ParentID == parentFolder.FolderID);
        }

        /// <summary>This class and its members are reserved for internal use and are not intended to be used in your code.</summary>
        internal class MergedTreeItem
        {
            /// <summary>Gets or sets a value indicating whether the item exists in the file system.</summary>
            public bool ExistsInFileSystem { get; set; }

            /// <summary>Gets or sets a value indicating whether the item exists in the database.</summary>
            public bool ExistsInDatabase { get; set; }

            /// <summary>Gets or sets a value indicating whether the item exists in the folder mappings.</summary>
            public bool ExistsInFolderMapping { get; set; }

            /// <summary>Gets or sets the folder id.</summary>
            public int FolderID { get; set; }

            /// <summary>Gets or sets the folder path.</summary>
            public int FolderMappingID { get; set; }

            /// <summary>Gets or sets the folder path.</summary>
            public string FolderPath { get; set; }

            /// <summary>Gets or sets the mapped path.</summary>
            public string MappedPath { get; set; }
        }

        /// <summary>This class and its members are reserved for internal use and are not intended to be used in your code.</summary>
        private class IgnoreCaseStringComparer : IComparer<string>
        {
            /// <inheritdoc />
            public int Compare(string x, string y)
            {
				Logger.LogError(">>> Entering Compare(x={X},y={Y})", x, y);
                Logger.LogWarning("<<< Exiting Compare(x={X},y={Y})", x, y);
                return string.Compare(x.ToLowerInvariant(), y.ToLowerInvariant(), StringComparison.Ordinal);
            }
        }

        /// <summary>This class and its members are reserved for internal use and are not intended to be used in your code.</summary>
        private class MoveFoldersInfo
        {
            /// <summary>Initializes a new instance of the <see cref="MoveFoldersInfo"/> class.</summary>
            /// <param name="source">The source folder.</param>
            /// <param name="target">The destination folder.</param>
            public MoveFoldersInfo(string source, string target)
            {
				Logger.LogError(">>> Entering MoveFoldersInfo(source={Source},target={Target})", source, target);
                this.Source = source;
				Logger.LogDebug("MoveFoldersInfo(source={Source},target={Target}): this.Source → {Arg2}", source, target, this.Source);
            Logger.LogWarning("<<< Exiting MoveFoldersInfo(source={Source},target={Target})", source, target);
                this.Target = target;
            }

            /// <summary>Gets the Source folder.</summary>
            public string Source { get; private set; }

            /// <summary>Gets the target folder.</summary>
            public string Target { get; private set; }
        }
    }
}
