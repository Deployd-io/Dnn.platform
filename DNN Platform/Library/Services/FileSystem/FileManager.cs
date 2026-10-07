// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem.EventArgs;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>Exposes methods to manage files.</summary>
    public class FileManager(IFileSecurityController fileSecurityController, IFileLockingController fileLockingController, IFileVersionController fileVersionController, IFileDeletionController fileDeletionController, IFileContentTypeManager fileContentTypeManager, IFolderMappingController folderMappingController, IFolderPermissionController folderPermissionController, ISystemWorkflowManager systemWorkflowManager, IWorkflowManager workflowManager, IWorkflowEngine workflowEngine, IWorkflowSecurity workflowSecurity, IUserController userController, IContentController contentController, IContentTypeController contentTypeController, ITermController termController, IHostSettings hostSettings, ICBO cbo, IEventManager eventManager, IEventLogger eventLogger, IPortalController portalController, DataProvider dataProvider, IPathUtils pathUtils)
        : ComponentBase<IFileManager, FileManager>, IFileManager
    {
        private const int BufferSize = 4096;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileManager));

        private readonly IFolderManager folderManager = FolderManager.Instance;
        private readonly IFileSecurityController fileSecurityController = fileSecurityController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileSecurityController>();
        private readonly IFileLockingController fileLockingController = fileLockingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileLockingController>();
        private readonly IFileVersionController fileVersionController = fileVersionController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileVersionController>();
        private readonly IFileDeletionController fileDeletionController = fileDeletionController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileDeletionController>();
        private readonly IFileContentTypeManager fileContentTypeManager = fileContentTypeManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileContentTypeManager>();
        private readonly IFolderMappingController folderMappingController = folderMappingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderMappingController>();
        private readonly IFolderPermissionController folderPermissionController = folderPermissionController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderPermissionController>();
        private readonly ISystemWorkflowManager systemWorkflowManager = systemWorkflowManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<ISystemWorkflowManager>();
        private readonly IWorkflowManager workflowManager = workflowManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IWorkflowManager>();
        private readonly IWorkflowEngine workflowEngine = workflowEngine ?? Globals.GetCurrentServiceProvider().GetRequiredService<IWorkflowEngine>();
        private readonly IWorkflowSecurity workflowSecurity = workflowSecurity ?? Globals.GetCurrentServiceProvider().GetRequiredService<IWorkflowSecurity>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        private readonly IContentController contentController = contentController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IContentController>();
        private readonly IContentTypeController contentTypeController = contentTypeController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IContentTypeController>();
        private readonly ITermController termController = termController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITermController>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private readonly ICBO cbo = cbo ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICBO>();
        private readonly IEventManager eventManager = eventManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly DataProvider dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        private readonly IPathUtils pathUtils = pathUtils ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPathUtils>();

        /// <summary>Initializes a new instance of the <see cref="FileManager"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IFileSecurityController. Scheduled removal in v12.0.0.")]
        public FileManager()
            : this(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)
        {
		Logger.LogError(">>> Entering FileManager()");
        Logger.LogWarning("<<< Exiting FileManager()");
        }

        /// <inheritdoc cref="IFileContentTypeManager.ContentTypes"/>
        public virtual IDictionary<string, string> ContentTypes => this.fileContentTypeManager.ContentTypes;

        /// <inheritdoc />
        public FileExtensionWhitelist WhiteList
        {
            get
            {
                var user = this.userController.GetCurrentUserInfo();
                if (user != null)
                {
                    if (user.IsSuperUser)
                    {
                        return (FileExtensionWhitelist)this.hostSettings.AllowedExtensionAllowList;
                    }

                    if (!user.IsAdmin)
                    {
                        var settings = PortalSettings.Current;
                        if (settings != null)
                        {
                            return settings.AllowedExtensionsWhitelist;
                        }
                    }
                }

                return (FileExtensionWhitelist)this.hostSettings.AllowedExtensionAllowList;
            }
        }

        /// <summary>Adds a file to the specified folder.</summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent)
        {
			Logger.LogError(">>> Entering AddFile(folder={Folder},fileName={FileName},fileContent={FileContent})", folder, fileName, fileContent);
            Logger.LogWarning("<<< Exiting AddFile(folder={Folder},fileName={FileName},fileContent={FileContent})", folder, fileName, fileContent);
			Logger.LogWarning("<<< Exiting AddFile(folder={Folder},fileName={FileName},fileContent={FileContent},overwrite={Overwrite},checkPermissions={CheckPermissions},ignoreWhiteList={IgnoreWhiteList},contentType={ContentType},createdByUserID={CreatedByUserID})", folder, fileName, fileContent, overwrite, checkPermissions, ignoreWhiteList, contentType, createdByUserID);
            return this.AddFile(folder, fileName, fileContent, true, false, false, this.GetContentType(Path.GetExtension(fileName)), this.GetCurrentUserID());
        }

        /// <summary>Adds a file to the specified folder.</summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exits.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite)
        {
            return this.AddFile(folder, fileName, fileContent, overwrite, false, false, this.GetContentType(Path.GetExtension(fileName)), this.GetCurrentUserID());
        }

        /// <summary>Adds a file to the specified folder.</summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder, fileName or fileContent are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, string contentType)
        {
            return this.AddFile(folder, fileName, fileContent, overwrite, checkPermissions, false, contentType, this.GetCurrentUserID());
        }

        /// <summary>Adds a file to the specified folder.</summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="createdByUserID">ID of the user that creates the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder, fileName or fileContent are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, string contentType, int createdByUserID)
        {
            return this.AddFile(folder, fileName, fileContent, overwrite, checkPermissions, false, contentType, createdByUserID);
        }

        /// <summary>Adds a file to the specified folder.</summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="ignoreWhiteList">Indicates whether the whitelist should be ignored.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="createdByUserID">ID of the user that creates the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when folder, fileName or fileContent are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        public virtual IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, bool ignoreWhiteList, string contentType, int createdByUserID)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            this.CheckFileAddingRestrictions(folder, fileName, checkPermissions, ignoreWhiteList);

            // DNN-2949 If IgnoreWhiteList is set to true , then file should be copied and info logged into Event Viewer
            if (!this.IsAllowedExtension(fileName) && ignoreWhiteList)
            {
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT), };
                log.LogProperties.Add(new LogDetailInfo("Following file was imported/uploaded, but is not an authorized filetype: ", fileName));
                this.eventLogger.AddLog(log);
            }

            var folderMapping = this.folderMappingController.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);

            bool fileExists = this.FileExists(folder, fileName, true);
            bool needToWriteFile = fileContent != null && (overwrite || !folderProvider.FileExists(folder, fileName));
            bool usingSeekableStream = false;

            if (fileContent != null && !needToWriteFile && this.FileExists(folder, fileName))
            {
                return this.GetFile(folder, fileName);
            }

            var oldFile = fileExists ? this.GetFile(folder, fileName, true) : null;

            var now = DateTime.Now;
            var extension = Path.GetExtension(fileName);
            var file = new FileInfo
            {
                PortalId = folder.PortalID,
                FileName = fileName,
                Extension = (!string.IsNullOrEmpty(extension)) ? extension.Replace(".", string.Empty) : string.Empty,
                Width = Null.NullInteger,
                Height = Null.NullInteger,
                ContentType = contentType,
                Folder = folder.FolderPath,
                FolderId = folder.FolderID,
                LastModificationTime = now,
                StartDate = now,
                EndDate = Null.NullDate,
                EnablePublishPeriod = false,
                ContentItemID = oldFile?.ContentItemID ?? Null.NullInteger,
                Title = oldFile != null ? oldFile.Title : Null.NullString,
                SHA1Hash = oldFile != null ? oldFile.SHA1Hash : string.Empty,
            };

            try
            {
                Workflow folderWorkflow = null;
                var contentFileName = fileName;
                if (needToWriteFile)
                {
                    if (!fileContent.CanSeek)
                    {
                        fileContent = this.GetSeekableStream(fileContent);
                        usingSeekableStream = true;
                    }

                    this.CheckFileWritingRestrictions(folder, fileName, fileContent, oldFile, createdByUserID, ignoreWhiteList);

                    // Retrieve Metadata
                    this.SetInitialFileMetadata(ref fileContent, file, folderProvider);

                    // Workflow
                    folderWorkflow = this.workflowManager.GetWorkflow(folder.WorkflowID);
                    if (folderWorkflow != null)
                    {
                        this.SetContentItem(file);

                        file.FileId = oldFile?.FileId ?? Null.NullInteger;
                        if (folderWorkflow.WorkflowID == this.systemWorkflowManager.GetDirectPublishWorkflow(folderWorkflow.PortalID).WorkflowID)
                        {
                            if (file.FileId == Null.NullInteger)
                            {
                                this.AddFile(file, createdByUserID);
                                fileExists = true;
                            }
                            else
                            {
                                // File Events for updating will be not fired. Only events for adding must be fired
                                this.UpdateFile(file, true, false);
                            }

                            contentFileName = this.ProcessVersioning(folder, oldFile, file, createdByUserID);
                        }
                        else
                        {
                            contentFileName = this.UpdateWhileApproving(folder, createdByUserID, file, oldFile, fileContent);

                            // This case will be to overwrite an existing file or initial file workflow
                            this.ManageFileAdding(createdByUserID, folderWorkflow, fileExists, file);
                        }
                    }

                    // Versioning
                    else
                    {
                        contentFileName = this.ProcessVersioning(folder, oldFile, file, createdByUserID);
                    }
                }
                else
                {
                    file.Size = (int)folderProvider.GetFileSize(file);
                    file.SHA1Hash = folderProvider.GetHashCode(file);
                }

                var isDatabaseProvider = folderMapping.FolderProviderType == "DatabaseFolderProvider";

                try
                {
                    // add file into database first if folder provider is default providers
                    // add file into database after file saved into folder provider for remote folder providers to avoid multiple thread issue.
                    if (isDatabaseProvider)
                    {
                        if (folderWorkflow == null || !fileExists)
                        {
                            this.ManageFileAdding(createdByUserID, folderWorkflow, fileExists, file);
                        }

                        if (needToWriteFile)
                        {
                            folderProvider.AddFile(folder, contentFileName, fileContent);
                        }
                    }
                    else
                    {
                        if (needToWriteFile)
                        {
                            folderProvider.AddFile(folder, contentFileName, fileContent);
                        }

                        if (folderWorkflow == null || !fileExists)
                        {
                            this.ManageFileAdding(createdByUserID, folderWorkflow, fileExists, file);
                        }
                    }

                    var providerLastModificationTime = folderProvider.GetLastModificationTime(file);
                    if (file.LastModificationTime != providerLastModificationTime)
                    {
                        this.dataProvider.UpdateFileLastModificationTime(file.FileId, providerLastModificationTime);
                    }

                    var providerHash = folderProvider.GetHashCode(file);
                    if (file.SHA1Hash != providerHash)
                    {
                        this.dataProvider
                            .UpdateFileHashCode(file.FileId, providerHash);
                    }
                }
                catch (FileLockedException fle)
                {
                    Logger.Error(fle);
                    throw;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    if (!folderProvider.FileExists(folder, file.FileName))
                    {
                        this.fileDeletionController.DeleteFileData(file);
                    }

                    throw new FolderProviderException(
                        Localization.GetExceptionMessage(
                            "AddFileUnderlyingSystemError",
                            "The underlying system threw an exception. The file has not been added."),
                        ex);
                }

                DataCache.RemoveCache("GetFileById" + file.FileId);
                this.ClearFolderCache(folder.PortalID);
                var addedFile = this.GetFile(file.FileId, true); // The file could be pending to be approved, but it should be returned

                this.NotifyFileAddingEvents(folder, createdByUserID, fileExists, folderWorkflow, addedFile);

                return addedFile;
            }
            finally
            {
                if (usingSeekableStream)
                {
                    fileContent.Dispose();
                }
            }
        }

        /// <summary>Copies the specified file into the specified folder.</summary>
        /// <param name="file">The file to copy.</param>
        /// <param name="destinationFolder">The folder where to copy the file to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destinationFolder are null.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the copied file.</returns>
        public virtual IFileInfo CopyFile(IFileInfo file, IFolderInfo destinationFolder)
        {
			Logger.LogError(">>> Entering CopyFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            if (file.FolderMappingID == destinationFolder.FolderMappingID)
            {
                if (!this.folderPermissionController.CanAddFolder(destinationFolder))
                {
					Logger.LogError("Throwing PermissionsNotMetException from CopyFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
                    throw new PermissionsNotMetException(Localization.GetExceptionMessage("CopyFilePermissionsNotMet", "Permissions are not met. The file has not been copied."));
                }

                if (!this.portalController.HasSpaceAvailable(destinationFolder.PortalID, file.Size))
                {
					Logger.LogError("Throwing NoSpaceAvailableException from CopyFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
                    throw new NoSpaceAvailableException(Localization.GetExceptionMessage("CopyFileNoSpaceAvailable", "The portal has no space available to store the specified file. The file has not been copied."));
                }

                var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
				Logger.LogError("CopyFile(file={File},destinationFolder={DestinationFolder}): folderMapping → {FolderMapping}", file, destinationFolder, folderMapping);
                try
                {
                    // check for existing file
                    var existingFile = this.GetFile(destinationFolder, file.FileName, true);
					Logger.LogError("CopyFile(file={File},destinationFolder={DestinationFolder}): existingFile → {ExistingFile}", file, destinationFolder, existingFile);
                    if (existingFile != null)
                    {
                        this.DeleteFile(existingFile);
                    }

                    var folder = this.folderManager.GetFolder(file.FolderId);
					Logger.LogError("CopyFile(file={File},destinationFolder={DestinationFolder}): folder → {Folder}", file, destinationFolder, folder);
                    FolderProvider.Instance(folderMapping.FolderProviderType).CopyFile(folder.MappedPath, file.FileName, destinationFolder.MappedPath, folderMapping);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    throw new FolderProviderException(Localization.GetExceptionMessage("CopyFileUnderlyingSystemError", "The underlying system throw an exception. The file has not been copied."), ex);
                }

                // copy Content Item
                var contentItemId = this.CopyContentItem(file.ContentItemID);
				Logger.LogError("CopyFile(file={File},destinationFolder={DestinationFolder}): contentItemId → {ContentItemId}", file, destinationFolder, contentItemId);

                var fileId = this.dataProvider.AddFile(
                    file.PortalId,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    file.FileName,
                    file.Extension,
                    file.Size,
                    file.Width,
                    file.Height,
                    file.ContentType,
                    destinationFolder.FolderPath,
                    destinationFolder.FolderID,
                    this.GetCurrentUserID(),
                    file.SHA1Hash,
                    DateTime.Now,
                    file.Title,
                    file.Description,
                    file.StartDate,
                    file.EndDate,
                    file.EnablePublishPeriod,
                    contentItemId);

                var copiedFile = this.GetFile(fileId, true);
				Logger.LogError("CopyFile(file={File},destinationFolder={DestinationFolder}): copiedFile → {CopiedFile}", file, destinationFolder, copiedFile);

                // Notify added file event
                this.OnFileAdded(copiedFile, destinationFolder, this.GetCurrentUserID());

                return copiedFile;
            }

            using (var fileContent = this.GetFileContent(file))
            {
                // check for existing file
                var existingFile = this.GetFile(destinationFolder, file.FileName, true);
                if (existingFile != null)
                {
                    this.DeleteFile(existingFile);
                }

				Logger.LogError(">>> Entering GetFileContent(file={File})", file);
				Logger.LogWarning("<<< Exiting GetFileContent(file={File})", file);
			Logger.LogWarning("<<< Exiting CopyFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
                return this.AddFile(destinationFolder, file.FileName, fileContent, true, true, file.ContentType);
            }
        }

        /// <summary>Deletes the specified file.</summary>
        /// <param name="file">The file to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual void DeleteFile(IFileInfo file)
        {
			Logger.LogError(">>> Entering DeleteFile(file={File})", file);
            Requires.NotNull("file", file);
            this.fileDeletionController.DeleteFile(file);
            this.ClearFolderCache(file.PortalId);

            // Notify File Delete Event
            this.OnFileDeleted(file, this.GetCurrentUserID());
			Logger.LogWarning("<<< Exiting DeleteFile(file={File})", file);
        }

        /// <summary>Deletes the specified files.</summary>
        /// <param name="files">The files to delete.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when files is null.</exception>
        public virtual void DeleteFiles(IEnumerable<IFileInfo> files)
        {
			Logger.LogError(">>> Entering DeleteFiles(files={Files})", files);
            Requires.NotNull("files", files);

            foreach (var file in files)
            {
                this.DeleteFile(file);
            }
			Logger.LogWarning("<<< Exiting DeleteFiles(files={Files})", files);
        }

        /// <summary>Checks the existence of the specified file in the specified folder.</summary>
        /// <param name="folder">The folder where to check the existence of the file.</param>
        /// <param name="fileName">The file name to check the existence of.</param>
        /// <returns>A bool value indicating whether the file exists or not in the specified folder.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when fileName is null or empty.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual bool FileExists(IFolderInfo folder, string fileName)
        {
			Logger.LogError(">>> Entering FileExists(folder={Folder},fileName={FileName})", folder, fileName);
            Logger.LogWarning("<<< Exiting FileExists(folder={Folder},fileName={FileName})", folder, fileName);
			Logger.LogWarning("<<< Exiting FileExists(folder={Folder},fileName={FileName},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", folder, fileName, retrieveUnpublishedFiles);
            return this.FileExists(folder, fileName, false);
        }

        /// <summary>Checks the existence of the specified file in the specified folder.</summary>
        /// <param name="folder">The folder where to check the existence of the file.</param>
        /// <param name="fileName">The file name to check the existence of.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>A bool value indicating whether the file exists or not in the specified folder.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when folder is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when fileName is null or empty.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual bool FileExists(IFolderInfo folder, string fileName, bool retrieveUnpublishedFiles)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var file = this.GetFile(folder, fileName, retrieveUnpublishedFiles);
            var existsFile = file != null;
            var folderMapping = this.folderMappingController.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            try
            {
                existsFile = existsFile && FolderProvider.Instance(folderMapping.FolderProviderType).FileExists(folder, fileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }

            return existsFile;
        }

        /// <summary>Gets the Content Type for the specified file extension.</summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The Content Type for the specified extension.</returns>
        public virtual string GetContentType(string extension)
        {
			Logger.LogError(">>> Entering GetContentType(extension={Extension})", extension);
            Logger.LogWarning("<<< Exiting GetContentType(extension={Extension})", extension);
            return this.fileContentTypeManager.GetContentType(extension);
        }

        /// <summary>Gets the file metadata for the specified file.</summary>
        /// <param name="fileID">The file identifier.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int fileID)
        {
			Logger.LogError(">>> Entering GetFile(fileID={FileID})", fileID);
			Logger.LogWarning("<<< Exiting GetFile(fileID={FileID})", fileID);
			Logger.LogWarning("<<< Exiting GetFile(fileID={FileID},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", fileID, retrieveUnpublishedFiles);
			Logger.LogWarning("<<< Exiting GetFile(folder={Folder},fileName={FileName},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", folder, fileName, retrieveUnpublishedFiles);
			Logger.LogWarning("<<< Exiting GetFile(portalId={PortalId},relativePath={RelativePath},retrieveUnpublishedFiles={RetrieveUnpublishedFiles})", portalId, relativePath, retrieveUnpublishedFiles);
            return this.GetFile(fileID, false);
        }

        /// <summary>Gets the file metadata for the specified file.</summary>
        /// <param name="fileID">The file identifier.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int fileID, bool retrieveUnpublishedFiles)
        {
            if (fileID is 0 or -1)
            {
                return null;
            }

            var strCacheKey = "GetFileById" + fileID;
            var file = DataCache.GetCache(strCacheKey);
            if (file == null)
            {
                file = this.cbo.FillObject<FileInfo>(this.dataProvider.GetFileById(fileID, retrieveUnpublishedFiles));
                if (file != null)
                {
                    var intCacheTimeout = 20 * (int)this.GetPerformanceSetting();
                    DataCache.SetCache(strCacheKey, file, TimeSpan.FromMinutes(intCacheTimeout));
                }
            }

            return (IFileInfo)file;
        }

        /// <summary>Gets the file metadata for the specified file.</summary>
        /// <param name="folder">The folder where the file is stored.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(IFolderInfo folder, string fileName)
        {
            return this.GetFile(folder, fileName, false);
        }

        /// <summary>Gets the file metadata for the specified file.</summary>
        /// <param name="folder">The folder where the file is stored.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(IFolderInfo folder, string fileName, bool retrieveUnpublishedFiles)
        {
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("folder", folder);

            return this.cbo.FillObject<FileInfo>(this.dataProvider.GetFile(fileName, folder.FolderID, retrieveUnpublishedFiles));
        }

        /// <summary>Gets the file metadata for the specified file.</summary>
        /// <param name="portalId">The portal ID or Null.NullInteger for the Host.</param>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <remarks>Host and portal settings commonly return a relative path to a file.  This method uses that relative path to fetch file metadata.</remarks>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int portalId, string relativePath)
        {
            return this.GetFile(portalId, relativePath, false);
        }

        /// <summary>Gets the file metadata for the specified file.</summary>
        /// <param name="portalId">The portal ID or Null.NullInteger for the Host.</param>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <remarks>Host and portal settings commonly return a relative path to a file.  This method uses that relative path to fetch file metadata.</remarks>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        public virtual IFileInfo GetFile(int portalId, string relativePath, bool retrieveUnpublishedFiles)
        {
            Requires.NotNullOrEmpty("relativePath", relativePath);

            var folderPath = string.Empty;
            var separatorIndex = relativePath.LastIndexOf('/');
            if (separatorIndex > 0)
            {
                folderPath = relativePath.Substring(0, separatorIndex + 1);
            }

            var folderInfo = this.folderManager.GetFolder(portalId, folderPath);
            if (folderInfo == null)
            {
                return null;
            }

            var fileName = relativePath.Substring(folderPath.Length);
            return this.GetFile(folderInfo, fileName, retrieveUnpublishedFiles);
        }

        /// <summary>Gets the content of the specified file.</summary>
        /// <param name="file">The file to get the content from.</param>
        /// <returns>A stream with the content of the file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public virtual Stream GetFileContent(IFileInfo file)
        {
            Requires.NotNull("file", file);

            Stream stream = null;

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);

            if (folderMapping != null)
            {
                try
                {
                    stream = FolderProvider.Instance(folderMapping.FolderProviderType).GetFileStream(file);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception"), ex);
                }
            }

            return stream;
        }

        /// <summary>Gets a seekable Stream based on the specified non-seekable Stream.</summary>
        /// <param name="stream">A non-seekable Stream.</param>
        /// <returns>A seekable Stream.</returns>
        public virtual Stream GetSeekableStream(Stream stream)
        {
			Logger.LogError(">>> Entering GetSeekableStream(stream={Stream})", stream);
            Requires.NotNull("stream", stream);

            if (stream.CanSeek)
            {
                return stream;
            }

            var folderPath = this.GetHostMapPath();
			Logger.LogError("GetSeekableStream(stream={Stream}): folderPath → {FolderPath}", stream, folderPath);
            string filePath;

            do
            {
                filePath = Path.Combine(folderPath, Path.GetRandomFileName()) + ".resx";
				Logger.LogError("GetSeekableStream(stream={Stream}): filePath → {FilePath}", stream, filePath);
				Logger.LogDebug("GetSeekableStream(stream={Stream}): filePath → {FilePath}", stream, filePath);
            }
            while (File.Exists(filePath));

            var fileStream = this.GetAutoDeleteFileStream(filePath);
			Logger.LogError("GetSeekableStream(stream={Stream}): fileStream → {FileStream}", stream, fileStream);

            var array = new byte[BufferSize];

            int bytesRead;
            while ((bytesRead = stream.Read(array, 0, BufferSize)) > 0)
            {
                fileStream.Write(array, 0, bytesRead);
            }

            fileStream.Position = 0;
			Logger.LogDebug("GetSeekableStream(stream={Stream}): fileStream.Position → {Arg1}", stream, fileStream.Position);

			Logger.LogWarning("<<< Exiting GetSeekableStream(stream={Stream})", stream);
            return fileStream;
        }

        /// <summary>Gets the direct URL to the file.</summary>
        /// <param name="file">The file to get the Url.</param>
        /// <returns>The direct URL to the file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        public string GetUrl(IFileInfo file)
        {
			Logger.LogError(">>> Entering GetUrl(file={File})", file);
            Requires.NotNull("file", file);

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("GetUrl(file={File}): folderMapping → {FolderMapping}", file, folderMapping);

            try
            {
				Logger.LogWarning("<<< Exiting GetUrl(file={File})", file);
                return FolderProvider.Instance(folderMapping.FolderProviderType).GetFileUrl(file);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        /// <summary>Gets a flag that determines if the file is an Image.</summary>
        /// <param name="file">The file to test.</param>
        /// <returns>The flag as a boolean value.</returns>
        public virtual bool IsImageFile(IFileInfo file)
        {
			Logger.LogError(">>> Entering IsImageFile(file={File})", file);
            Logger.LogWarning("<<< Exiting IsImageFile(file={File})", file);
            return (Globals.ImageFileTypes + ",").Contains(file.Extension.Replace(".", string.Empty) + ",", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Moves the specified file into the specified folder.</summary>
        /// <param name="file">The file to move.</param>
        /// <param name="destinationFolder">The folder where to move the file to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destinationFolder are null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FolderProviderException">Thrown when the underlying system throw an exception.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidFileExtensionException">Thrown when the extension of the specified file is not allowed.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.NoSpaceAvailableException">Thrown when the portal has no space available to store the specified file.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        /// <returns>An <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the moved file.</returns>
        public virtual IFileInfo MoveFile(IFileInfo file, IFolderInfo destinationFolder)
        {
			Logger.LogError(">>> Entering MoveFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            // check whether the file is already in the dest folder.
            if (file.FolderId == destinationFolder.FolderID)
            {
                return file;
            }

            if (this.fileLockingController.IsFileLocked(file, out var lockReason))
            {
				Logger.LogError("Throwing FileLockedException from MoveFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            // check for existing file
            var existingFile = this.GetFile(destinationFolder, file.FileName, true);
			Logger.LogError("MoveFile(file={File},destinationFolder={DestinationFolder}): existingFile → {ExistingFile}", file, destinationFolder, existingFile);
            if (existingFile != null)
            {
                this.DeleteFile(existingFile);
            }

            var destinationFolderMapping = this.folderMappingController.GetFolderMapping(destinationFolder.PortalID, destinationFolder.FolderMappingID);
			Logger.LogError("MoveFile(file={File},destinationFolder={DestinationFolder}): destinationFolderMapping → {DestinationFolderMapping}", file, destinationFolder, destinationFolderMapping);
            var destinationFolderProvider = FolderProvider.Instance(destinationFolderMapping.FolderProviderType);
			Logger.LogError("MoveFile(file={File},destinationFolder={DestinationFolder}): destinationFolderProvider → {DestinationFolderProvider}", file, destinationFolder, destinationFolderProvider);

            var sourceFolderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("MoveFile(file={File},destinationFolder={DestinationFolder}): sourceFolderMapping → {SourceFolderMapping}", file, destinationFolder, sourceFolderMapping);
            var sourceFolderProvider = FolderProvider.Instance(sourceFolderMapping.FolderProviderType);
			Logger.LogError("MoveFile(file={File},destinationFolder={DestinationFolder}): sourceFolderProvider → {SourceFolderProvider}", file, destinationFolder, sourceFolderProvider);

            if (destinationFolderMapping.FolderMappingID == sourceFolderMapping.FolderMappingID && destinationFolderProvider.SupportsMoveFile)
            {
                // Implement Move
                destinationFolderProvider.MoveFile(file, destinationFolder);
            }
            else
            {
                // Implement Copy/Delete
                using (var fileContent = this.GetFileContent(file))
                {
                    if (destinationFolderMapping.MappingName == "Database")
                    {
                        destinationFolderProvider.UpdateFile(file, fileContent);
                    }
                    else
                    {
                        this.AddFileToFolderProvider(fileContent, file.FileName, destinationFolder, destinationFolderProvider);
                    }
                }

                DeleteFileFromFolderProvider(file, sourceFolderProvider);
            }

            if (file.FolderMappingID == destinationFolder.FolderMappingID)
            {
                this.MoveVersions(file, destinationFolder, sourceFolderProvider, destinationFolderProvider);
            }
            else
            {
                this.fileVersionController.DeleteAllUnpublishedVersions(file, true);
            }

            var oldFilePath = file.Folder;
            file.FolderId = destinationFolder.FolderID;
			Logger.LogDebug("MoveFile(file={File},destinationFolder={DestinationFolder}): file.FolderId → {Arg2}", file, destinationFolder, file.FolderId);
			Logger.LogDebug("MoveFile(file={File},destinationFolder={DestinationFolder}): file.FolderMappingID → {Arg2}", file, destinationFolder, file.FolderMappingID);
            file.Folder = destinationFolder.FolderPath;
			Logger.LogDebug("MoveFile(file={File},destinationFolder={DestinationFolder}): file.Folder → {Arg2}", file, destinationFolder, file.Folder);
            file.FolderMappingID = destinationFolder.FolderMappingID;
            file = this.UpdateFile(file);

            // Notify File Moved event
            this.OnFileMoved(file, oldFilePath, this.GetCurrentUserID());

			Logger.LogWarning("<<< Exiting MoveFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
            return file;
        }

        /// <summary>Renames the specified file.</summary>
        /// <param name="file">The file to rename.</param>
        /// <param name="newFileName">The new filename to assign to the file.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.FileAlreadyExistsException">Thrown when the folder already contains a file with the same name.</exception>
        /// <returns>An <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the renamed file.</returns>
        public virtual IFileInfo RenameFile(IFileInfo file, string newFileName)
        {
			Logger.LogError(">>> Entering RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
            Requires.NotNull("file", file);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            if (file.FileName == newFileName)
            {
                return file;
            }

            if (!this.IsAllowedExtension(newFileName))
            {
				Logger.LogError("Throwing InvalidFileExtensionException from RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
                throw new InvalidFileExtensionException(string.Format(CultureInfo.CurrentCulture, Localization.GetExceptionMessage("AddFileExtensionNotAllowed", "The extension '{0}' is not allowed. The file has not been added."), Path.GetExtension(newFileName)));
            }

            if (!this.IsValidFilename(newFileName))
            {
				Logger.LogError("Throwing InvalidFilenameException from RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
                throw new InvalidFilenameException(string.Format(CultureInfo.CurrentCulture, Localization.GetExceptionMessage("AddFilenameNotAllowed", "The file name '{0}' is not allowed. The file has not been added."), newFileName));
            }

            var folder = this.folderManager.GetFolder(file.FolderId);
			Logger.LogError("RenameFile(file={File},newFileName={NewFileName}): folder → {Folder}", file, newFileName, folder);

            if (this.FileExists(folder, newFileName))
            {
				Logger.LogError("Throwing FileAlreadyExistsException from RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
                throw new FileAlreadyExistsException(Localization.GetExceptionMessage("RenameFileAlreadyExists", "This folder already contains a file with the same name. The file has not been renamed."));
            }

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("RenameFile(file={File},newFileName={NewFileName}): folderMapping → {FolderMapping}", file, newFileName, folderMapping);

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).RenameFile(file, newFileName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                throw new FolderProviderException(Localization.GetExceptionMessage("RenameFileUnderlyingSystemError", "The underlying system threw an exception. The file has not been renamed."), ex);
            }

            var oldFileName = file.FileName;
            file.FileName = newFileName;
			Logger.LogDebug("RenameFile(file={File},newFileName={NewFileName}): file.FileName → {Arg2}", file, newFileName, file.FileName);
            if (Path.HasExtension(newFileName))
            {
                file.Extension = Path.GetExtension(newFileName).Replace(".", string.Empty);
            }

            var renamedFile = this.UpdateFile(file);
			Logger.LogError("RenameFile(file={File},newFileName={NewFileName}): renamedFile → {RenamedFile}", file, newFileName, renamedFile);

            // Notify File Renamed event
            this.OnFileRenamed(renamedFile, oldFileName, this.GetCurrentUserID());

			Logger.LogWarning("<<< Exiting RenameFile(file={File},newFileName={NewFileName})", file, newFileName);
            return renamedFile;
        }

        /// <summary>Sets the specified FileAttributes of the file.</summary>
        /// <param name="file">The file.</param>
        /// <param name="fileAttributes">The file attributes to add.</param>
        public void SetAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
			Logger.LogError(">>> Entering SetAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
            Requires.NotNull("file", file);

            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
			Logger.LogError("SetAttributes(file={File},fileAttributes={FileAttributes}): folderMapping → {FolderMapping}", file, fileAttributes, folderMapping);

            try
            {
                FolderProvider.Instance(folderMapping.FolderProviderType).SetFileAttributes(file, fileAttributes);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
			Logger.LogWarning("<<< Exiting SetAttributes(file={File},fileAttributes={FileAttributes})", file, fileAttributes);
                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        /// <summary>Extracts the files and folders contained in the specified zip file to the folder where the file belongs.</summary>
        /// <param name="file">The file to unzip.</param>
        /// <exception cref="System.ArgumentException">Thrown when file is not a zip compressed file.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destination folder are null.</exception>
        public virtual void UnzipFile(IFileInfo file)
        {
			Logger.LogError(">>> Entering UnzipFile(file={File})", file);
            Requires.NotNull("file", file);

            var destinationFolder = this.folderManager.GetFolder(file.FolderId);
			Logger.LogError("UnzipFile(file={File}): destinationFolder → {DestinationFolder}", file, destinationFolder);

            this.UnzipFile(file, destinationFolder);
			Logger.LogWarning("<<< Exiting UnzipFile(file={File})", file);
        }

        /// <summary>Extracts the files and folders contained in the specified zip file to the specified folder.</summary>
        /// <param name="file">The file to unzip.</param>
        /// <param name="destinationFolder">The folder to unzip to.</param>
        /// <exception cref="System.ArgumentException">Thrown when file is not a zip compressed file.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destination folder are null.</exception>
        public virtual void UnzipFile(IFileInfo file, IFolderInfo destinationFolder)
        {
			Logger.LogError(">>> Entering UnzipFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
            this.UnzipFile(file, destinationFolder, null);
			Logger.LogWarning("<<< Exiting UnzipFile(file={File},destinationFolder={DestinationFolder})", file, destinationFolder);
        }

        /// <summary>Extracts the files and folders contained in the specified zip file to the specified folder.</summary>
        /// <param name="file">The file to unzip.</param>
        /// <param name="destinationFolder">The folder to unzip to.</param>
        /// <param name="invalidFiles">Files which can't exact.</param>
        /// <returns>Total files count in the zip file.</returns>
        /// <exception cref="System.ArgumentException">Thrown when file is not a zip compressed file.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown when file or destination folder are null.</exception>
        public virtual int UnzipFile(IFileInfo file, IFolderInfo destinationFolder, IList<string> invalidFiles)
        {
			Logger.LogError(">>> Entering UnzipFile(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles})", file, destinationFolder, invalidFiles);
            Requires.NotNull("file", file);
            Requires.NotNull("destinationFolder", destinationFolder);

            if (file.Extension != "zip")
            {
				Logger.LogError("Throwing ArgumentException from UnzipFile(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles})", file, destinationFolder, invalidFiles);
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidZipFile", "The file specified is not a zip compressed file."));
            }

            // TODO: add parameter to interface to verify callers have considered permissions
            const bool alreadyCheckedPermissions = true;
			Logger.LogWarning("<<< Exiting UnzipFile(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles})", file, destinationFolder, invalidFiles);
            return this.ExtractFiles(file, destinationFolder, invalidFiles, alreadyCheckedPermissions);
        }

        /// <summary>Updates the metadata of the specified file.</summary>
        /// <param name="file">The file to update.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.InvalidMetadataValuesException">Thrown when the file metadata are not valid.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as the updated file.</returns>
        public virtual IFileInfo UpdateFile(IFileInfo file)
        {
			Logger.LogError(">>> Entering UpdateFile(file={File})", file);
            Requires.NotNull("file", file);
            if (!this.ValidMetadata(file, out var message))
            {
				Logger.LogError("Throwing InvalidMetadataValuesException from UpdateFile(file={File})", file);
                throw new InvalidMetadataValuesException(message);
            }

			Logger.LogWarning("<<< Exiting UpdateFile(file={File})", file);
			Logger.LogWarning("<<< Exiting UpdateFile(file={File},fileContent={FileContent})", file, fileContent);
			Logger.LogWarning("<<< Exiting UpdateFile(file={File},updateLazyload={UpdateLazyload})", file, updateLazyload);
			Logger.LogWarning("<<< Exiting UpdateFile(file={File},updateLazyload={UpdateLazyload},fireEvent={FireEvent})", file, updateLazyload, fireEvent);
            return this.UpdateFile(file, true);
        }

        /// <summary>Regenerates the hash and updates the metadata of the specified file.</summary>
        /// <param name="file">The file to update.</param>
        /// <param name="fileContent">Stream used to regenerate the hash.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as the updated file.</returns>
        public virtual IFileInfo UpdateFile(IFileInfo file, Stream fileContent)
        {
            Requires.NotNull("file", file);

            if (fileContent != null)
            {
                if (this.IsImageFile(file))
                {
                    Image image = null;

                    try
                    {
                        image = this.GetImageFromStream(fileContent);

                        file.Width = image.Width;
                        file.Height = image.Height;
                    }
                    catch
                    {
                        file.ContentType = "application/octet-stream";
                    }
                    finally
                    {
                        if (image != null)
                        {
                            image.Dispose();
                        }
                    }
                }

                file.SHA1Hash = FolderProvider.Instance(this.folderMappingController.GetFolderMapping(file.FolderMappingID).FolderProviderType).GetHashCode(file, fileContent);
            }

            // Get file size from folder provider.
            try
            {
                var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
                if (folderMapping != null)
                {
                    var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
                    file.Size = (int)folderProvider.GetFileSize(file);
                    file.LastModificationTime = folderProvider.GetLastModificationTime(file);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return this.UpdateFile(file);
        }

        /// <summary>Writes the content of the specified file into the specified stream.</summary>
        /// <param name="file">The file to write into the stream.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file or stream are null.</exception>
        public virtual void WriteFile(IFileInfo file, Stream stream)
        {
			Logger.LogError(">>> Entering WriteFile(file={File},stream={Stream})", file, stream);
            Requires.NotNull("file", file);
            Requires.NotNull("stream", stream);

            using var srcStream = this.GetFileContent(file);
			Logger.LogError("WriteFile(file={File},stream={Stream}): srcStream → {SrcStream}", file, stream, srcStream);
            const int BufferSize = 4096;
            var buffer = new byte[BufferSize];

            int bytesRead;
            while ((bytesRead = srcStream.Read(buffer, 0, BufferSize)) > 0)
            {
                stream.Write(buffer, 0, bytesRead);
            }
			Logger.LogWarning("<<< Exiting WriteFile(file={File},stream={Stream})", file, stream);
        }

        /// <summary>Downloads the specified file.</summary>
        /// <param name="file">The file to download.</param>
        /// <param name="contentDisposition">Indicates how to display the document once downloaded.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when file is null.</exception>
        /// <exception cref="DotNetNuke.Services.FileSystem.PermissionsNotMetException">Thrown when permissions are not met.</exception>
        public virtual void WriteFileToResponse(IFileInfo file, ContentDisposition contentDisposition)
        {
			Logger.LogError(">>> Entering WriteFileToResponse(file={File},contentDisposition={ContentDisposition})", file, contentDisposition);
            Requires.NotNull("file", file);

            var folder = this.folderManager.GetFolder(file.FolderId);
			Logger.LogError("WriteFileToResponse(file={File},contentDisposition={ContentDisposition}): folder → {Folder}", file, contentDisposition, folder);

            if (!this.folderPermissionController.CanViewFolder(folder))
            {
				Logger.LogError("Throwing PermissionsNotMetException from WriteFileToResponse(file={File},contentDisposition={ContentDisposition})", file, contentDisposition);
                throw new PermissionsNotMetException(Localization.GetExceptionMessage("WriteFileToResponsePermissionsNotMet", "Permissions are not met. The file cannot be downloaded."));
            }

            if (this.IsFileAutoSyncEnabled())
            {
                this.AutoSyncFile(file);
            }

            this.WriteFileToHttpContext(file, contentDisposition);
			Logger.LogWarning("<<< Exiting WriteFileToResponse(file={File},contentDisposition={ContentDisposition})", file, contentDisposition);
        }

        internal virtual int CopyContentItem(int contentItemId)
        {
			Logger.LogError(">>> Entering CopyContentItem(contentItemId={ContentItemId})", contentItemId);
            if (contentItemId == Null.NullInteger)
            {
                return Null.NullInteger;
            }

            var newContentItem = this.CreateFileContentItem();
			Logger.LogError("CopyContentItem(contentItemId={ContentItemId}): newContentItem → {NewContentItem}", contentItemId, newContentItem);

            // Clone terms
            foreach (var term in this.termController.GetTermsByContent(contentItemId))
            {
                this.termController.AddTermToContent(term, newContentItem);
            }

			Logger.LogWarning("<<< Exiting CopyContentItem(contentItemId={ContentItemId})", contentItemId);
            return newContentItem.ContentItemId;
        }

        internal virtual ContentItem CreateFileContentItem()
        {
			Logger.LogError(">>> Entering CreateFileContentItem()");
            var contentTypeFile = (from t in this.contentTypeController.GetContentTypes() where t.ContentType == "File" select t).SingleOrDefault();

			Logger.LogError("CreateFileContentItem(): contentTypeFile → {ContentTypeFile}", contentTypeFile);
            if (contentTypeFile == null)
            {
                contentTypeFile = new ContentType { ContentType = "File" };
                contentTypeFile.ContentTypeId = this.contentTypeController.AddContentType(contentTypeFile);
            }

            var objContent = new ContentItem
            {
                ContentTypeId = contentTypeFile.ContentTypeId,
                Indexed = false,
            };

            objContent.ContentItemId = this.contentController.AddContentItem(objContent);

			Logger.LogWarning("<<< Exiting CreateFileContentItem()");
            return objContent;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="file">The file.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="sourceFolderProvider">The folder provider of the source folder.</param>
        /// <param name="destinationFolderProvider">The folder provider of the destination folder.</param>
        internal virtual void MoveVersions(IFileInfo file, IFolderInfo destinationFolder, FolderProvider sourceFolderProvider, FolderProvider destinationFolderProvider)
        {
			Logger.LogError(">>> Entering MoveVersions(file={File},destinationFolder={DestinationFolder},sourceFolderProvider={SourceFolderProvider},destinationFolderProvider={DestinationFolderProvider})", file, destinationFolder, sourceFolderProvider, destinationFolderProvider);
            var versions = this.fileVersionController.GetFileVersions(file).ToArray();
			Logger.LogError("MoveVersions(file={File},destinationFolder={DestinationFolder},sourceFolderProvider={SourceFolderProvider},destinationFolderProvider={DestinationFolderProvider}): versions → {Versions}", file, destinationFolder, sourceFolderProvider, destinationFolderProvider, versions);
            if (versions.Length == 0)
            {
                return;
            }

            var folder = this.folderManager.GetFolder(file.FolderId);
			Logger.LogError("MoveVersions(file={File},destinationFolder={DestinationFolder},sourceFolderProvider={SourceFolderProvider},destinationFolderProvider={DestinationFolderProvider}): folder → {Folder}", file, destinationFolder, sourceFolderProvider, destinationFolderProvider, folder);

            foreach (var version in versions)
            {
                // Get Version File
                using (var fileContent = sourceFolderProvider.GetFileStream(folder, version.FileName))
                {
                    // This scenario is when the file is in the Database Folder Provider
                    if (fileContent == null)
                    {
                        continue;
                    }

                    this.AddFileToFolderProvider(fileContent, version.FileName, destinationFolder, destinationFolderProvider);
                }

                var fileVersion = new FileInfo
                {
                    FileName = version.FileName,
                    Folder = file.Folder,
                    FolderMappingID = file.FolderMappingID,
                    PortalId = folder.PortalID,
                };

                DeleteFileFromFolderProvider(fileVersion, sourceFolderProvider);
            }
			Logger.LogWarning("<<< Exiting MoveVersions(file={File},destinationFolder={DestinationFolder},sourceFolderProvider={SourceFolderProvider},destinationFolderProvider={DestinationFolderProvider})", file, destinationFolder, sourceFolderProvider, destinationFolderProvider);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="file">The file.</param>
        internal virtual void AutoSyncFile(IFileInfo file)
        {
			Logger.LogError(">>> Entering AutoSyncFile(file={File})", file);
            var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
			Logger.LogError("AutoSyncFile(file={File}): folderProvider → {FolderProvider}", file, folderProvider);
			Logger.LogError("AutoSyncFile(file={File}): folderMapping → {FolderMapping}", file, folderMapping);

            var folder = this.folderManager.GetFolder(file.FolderId);
			Logger.LogError("AutoSyncFile(file={File}): folder → {Folder}", file, folder);

            if (folderProvider.FileExists(folder, file.FileName))
            {
                var newFileSize = folderProvider.GetFileSize(file);
				Logger.LogError("AutoSyncFile(file={File}): newFileSize → {NewFileSize}", file, newFileSize);
                if (file.Size != newFileSize)
                {
                    using var fileContent = this.GetFileContent(file);
					Logger.LogError("AutoSyncFile(file={File}): fileContent → {FileContent}", file, fileContent);
                    this.UpdateFile(file, fileContent);
                }
            }
            else
            {
                this.DeleteFile(file);
            }
			Logger.LogWarning("<<< Exiting AutoSyncFile(file={File})", file);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="file">The zip file to extract.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        /// <param name="invalidFiles">A list into which the names of invalid files will be added.</param>
        /// <param name="alreadyCheckedPermissions">Whether the user's permissions to the folder have already been verified.</param>
        /// <returns>The number of files extracted.</returns>
        internal virtual int ExtractFiles(IFileInfo file, IFolderInfo destinationFolder, IList<string> invalidFiles, bool alreadyCheckedPermissions)
        {
			Logger.LogError(">>> Entering ExtractFiles(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles},alreadyCheckedPermissions={AlreadyCheckedPermissions})", file, destinationFolder, invalidFiles, alreadyCheckedPermissions);
            ZipArchive zipInputStream = null;

			Logger.LogError("ExtractFiles(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles},alreadyCheckedPermissions={AlreadyCheckedPermissions}): zipInputStream → {ZipInputStream}", file, destinationFolder, invalidFiles, alreadyCheckedPermissions, zipInputStream);
            invalidFiles ??= new List<string>();

            var exactFilesCount = 0;

            try
            {
                using var fileContent = this.GetFileContent(file);
				Logger.LogError("ExtractFiles(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles},alreadyCheckedPermissions={AlreadyCheckedPermissions}): fileContent → {FileContent}", file, destinationFolder, invalidFiles, alreadyCheckedPermissions, fileContent);
                zipInputStream = new ZipArchive(fileContent);

                foreach (var zipEntry in zipInputStream.FileEntries())
                {
                    exactFilesCount++;
                    var fileName = Path.GetFileName(zipEntry.FullName);
					Logger.LogError("ExtractFiles(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles},alreadyCheckedPermissions={AlreadyCheckedPermissions}): fileName → {FileName}", file, destinationFolder, invalidFiles, alreadyCheckedPermissions, fileName);

                    this.EnsureZipFolder(zipEntry.FullName, destinationFolder);

                    IFolderInfo parentFolder;
                    if (zipEntry.FullName.IndexOf('/') == -1)
                    {
                        parentFolder = destinationFolder;
						Logger.LogError("ExtractFiles(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles},alreadyCheckedPermissions={AlreadyCheckedPermissions}): parentFolder → {ParentFolder}", file, destinationFolder, invalidFiles, alreadyCheckedPermissions, parentFolder);
                    }
                    else
                    {
                        var folderPath = destinationFolder.FolderPath + zipEntry.FullName.Substring(0, zipEntry.FullName.LastIndexOf('/') + 1);
						Logger.LogError("ExtractFiles(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles},alreadyCheckedPermissions={AlreadyCheckedPermissions}): folderPath → {FolderPath}", file, destinationFolder, invalidFiles, alreadyCheckedPermissions, folderPath);
                        parentFolder = this.folderManager.GetFolder(file.PortalId, folderPath);
                    }

                    try
                    {
                        this.AddFile(parentFolder, fileName, zipEntry.Open(), true, !alreadyCheckedPermissions, this.GetContentType(Path.GetExtension(fileName)));
                    }
                    catch (PermissionsNotMetException exc)
                    {
                        Logger.Warn(exc);
                    }
                    catch (NoSpaceAvailableException exc)
                    {
                        Logger.Warn(exc);
                    }
                    catch (InvalidFileExtensionException exc)
                    {
                        invalidFiles.Add(zipEntry.FullName);
                        Logger.Warn(exc);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                    }
                }
            }
            finally
            {
                zipInputStream?.Dispose();
            }

			Logger.LogWarning("<<< Exiting ExtractFiles(file={File},destinationFolder={DestinationFolder},invalidFiles={InvalidFiles},alreadyCheckedPermissions={AlreadyCheckedPermissions})", file, destinationFolder, invalidFiles, alreadyCheckedPermissions);
            return exactFilesCount;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="fileName">The file path of the zip file.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal void EnsureZipFolder(string fileName, IFolderInfo destinationFolder)
        {
			Logger.LogError(">>> Entering EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder})", fileName, destinationFolder);
            var folderMapping = this.folderMappingController.GetFolderMapping(destinationFolder.PortalID, destinationFolder.FolderMappingID);

			Logger.LogError("EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder}): folderMapping → {FolderMapping}", fileName, destinationFolder, folderMapping);
            if (fileName.LastIndexOf('/') == -1)
            {
                return;
            }

            var zipFolder = fileName.Substring(0, fileName.LastIndexOf('/'));
			Logger.LogError("EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder}): zipFolder → {ZipFolder}", fileName, destinationFolder, zipFolder);

            var folderPath = this.pathUtils.RemoveTrailingSlash(zipFolder);
			Logger.LogError("EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder}): folderPath → {FolderPath}", fileName, destinationFolder, folderPath);

            if (!folderPath.Contains("/", StringComparison.Ordinal))
            {
                var newFolderPath = destinationFolder.FolderPath + this.pathUtils.FormatFolderPath(folderPath);
				Logger.LogError("EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder}): newFolderPath → {NewFolderPath}", fileName, destinationFolder, newFolderPath);
                if (!this.folderManager.FolderExists(destinationFolder.PortalID, newFolderPath))
                {
                    this.folderManager.AddFolder(folderMapping, newFolderPath);
                }
            }
            else
            {
                var zipFolders = folderPath.Split('/');
				Logger.LogError("EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder}): zipFolders → {ZipFolders}", fileName, destinationFolder, zipFolders);

                var parentFolder = destinationFolder;
				Logger.LogError("EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder}): parentFolder → {ParentFolder}", fileName, destinationFolder, parentFolder);

                for (var i = 0; i < zipFolders.Length; i++)
                {
                    var newFolderPath = parentFolder.FolderPath + this.pathUtils.FormatFolderPath(zipFolders[i]);
                    if (!this.folderManager.FolderExists(destinationFolder.PortalID, newFolderPath))
                    {
                        this.folderManager.AddFolder(this.folderMappingController.GetFolderMapping(parentFolder.PortalID, parentFolder.FolderMappingID), newFolderPath);
                    }

                    parentFolder = this.folderManager.GetFolder(destinationFolder.PortalID, newFolderPath);
                }
            }
			Logger.LogWarning("<<< Exiting EnsureZipFolder(fileName={FileName},destinationFolder={DestinationFolder})", fileName, destinationFolder);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>A <see cref="Stream"/> which will delete the file on close.</returns>
        internal virtual Stream GetAutoDeleteFileStream(string filePath)
        {
			Logger.LogError(">>> Entering GetAutoDeleteFileStream(filePath={FilePath})", filePath);
            Logger.LogWarning("<<< Exiting GetAutoDeleteFileStream(filePath={FilePath})", filePath);
            return new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, BufferSize, FileOptions.DeleteOnClose);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>The current user's ID.</returns>
        internal virtual int GetCurrentUserID()
        {
			Logger.LogError(">>> Entering GetCurrentUserID()");
            Logger.LogWarning("<<< Exiting GetCurrentUserID()");
            return this.userController.GetCurrentUserInfo().UserID;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="stream">The stream containing the file contents.</param>
        /// <returns>SHA1 hash of the file.</returns>
        internal virtual string GetHash(Stream stream)
        {
			Logger.LogError(">>> Entering GetHash(stream={Stream})", stream);
            Requires.NotNull("stream", stream);
            var hashText = new StringBuilder();
			Logger.LogError("GetHash(stream={Stream}): hashText → {HashText}", stream, hashText);
            using (var hasher = CryptographyUtils.CreateSHA256())
            {
                var hashData = hasher.ComputeHash(stream);
				Logger.LogError("GetHash(stream={Stream}): hashData → {HashData}", stream, hashData);
                foreach (var b in hashData)
                {
                    hashText.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                }
            }

			Logger.LogWarning("<<< Exiting GetHash(stream={Stream})", stream);
            return hashText.ToString();
        }

        /// <summary>Gets the hash of a file.</summary>
        /// <param name="fileInfo">The file info.</param>
        /// <returns>SHA1 hash of the file.</returns>
        internal virtual string GetHash(IFileInfo fileInfo)
        {
            return FolderProvider.Instance(this.folderMappingController.GetFolderMapping(fileInfo.FolderMappingID).FolderProviderType).GetHashCode(fileInfo);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>The host map path.</returns>
        internal virtual string GetHostMapPath()
        {
			Logger.LogError(">>> Entering GetHostMapPath()");
            Logger.LogWarning("<<< Exiting GetHostMapPath()");
            return TestableGlobals.Instance.HostMapPath;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="stream">A stream with the file contents.</param>
        /// <returns>An <see cref="Image"/> instance.</returns>
        internal virtual Image GetImageFromStream(Stream stream)
        {
			Logger.LogError(">>> Entering GetImageFromStream(stream={Stream})", stream);
            Logger.LogWarning("<<< Exiting GetImageFromStream(stream={Stream})", stream);
            return Image.FromStream(stream);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns>The <see cref="Globals.PerformanceSettings"/> value.</returns>
        internal virtual Globals.PerformanceSettings GetPerformanceSetting()
        {
			Logger.LogError(">>> Entering GetPerformanceSetting()");
            Logger.LogWarning("<<< Exiting GetPerformanceSetting()");
            return this.hostSettings.PerformanceSetting.ToOldEnum();
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="fileName">The file name.</param>
        /// <returns><see langword="true"/> if the <paramref name="fileName"/> has an allowed extension, otherwise <see langword="false"/>.</returns>
        internal virtual bool IsAllowedExtension(string fileName)
        {
			Logger.LogError(">>> Entering IsAllowedExtension(fileName={FileName})", fileName);
            var extension = Path.GetExtension(fileName);

			Logger.LogError("IsAllowedExtension(fileName={FileName}): extension → {Extension}", fileName, extension);
            // regex matches a dot followed by 1 or more chars followed by a semicolon
            // regex is meant to block files like "foo.asp;.png" which can take advantage
            // of a vulnerability in IIS6 which treats such files as .asp, not .png
			Logger.LogWarning("<<< Exiting IsAllowedExtension(fileName={FileName})", fileName);
            return !string.IsNullOrEmpty(extension)
                   && this.WhiteList.IsAllowedExtension(extension)
                   && !Globals.FileExtensionRegex.IsMatch(fileName);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="fileName">The file name.</param>
        /// <returns><see langword="true"/> if the file name is valid, otherwise <see langword="false"/>.</returns>
        internal virtual bool IsValidFilename(string fileName)
        {
			Logger.LogError(">>> Entering IsValidFilename(fileName={FileName})", fileName);
            // regex ensures the file is a valid filename and doesn't include illegal characters
			Logger.LogWarning("<<< Exiting IsValidFilename(fileName={FileName})", fileName);
            return Globals.FileValidNameRegex.IsMatch(fileName);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <returns><see langword="true"/> if the file system has auto-sync enabled, otherwise <see langword="false"/>.</returns>
        internal virtual bool IsFileAutoSyncEnabled()
        {
			Logger.LogError(">>> Entering IsFileAutoSyncEnabled()");
            Logger.LogWarning("<<< Exiting IsFileAutoSyncEnabled()");
            return this.hostSettings.EnableFileAutoSync;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="file">The file.</param>
        /// <param name="contentDisposition">The type of the content-disposition header.</param>
        internal virtual void WriteFileToHttpContext(IFileInfo file, ContentDisposition contentDisposition)
        {
			Logger.LogError(">>> Entering WriteFileToHttpContext(file={File},contentDisposition={ContentDisposition})", file, contentDisposition);
            var scriptTimeOut = HttpContext.Current.Server.ScriptTimeout;

            HttpContext.Current.Server.ScriptTimeout = int.MaxValue;
            var objResponse = HttpContext.Current.Response;

            objResponse.ClearContent();
            objResponse.ClearHeaders();

            switch (contentDisposition)
            {
                case ContentDisposition.Attachment:
                    objResponse.AppendHeader("content-disposition", "attachment; filename=\"" + file.FileName + "\"");
                    break;
                case ContentDisposition.Inline:
                    objResponse.AppendHeader("content-disposition", "inline; filename=\"" + file.FileName + "\"");
                    break;
                default:
					Logger.LogError("Throwing ArgumentOutOfRangeException from WriteFileToHttpContext(file={File},contentDisposition={ContentDisposition})", file, contentDisposition);
                    throw new ArgumentOutOfRangeException(nameof(contentDisposition));
            }

            // Do not send negative Content-Length (file.Size could be negative due to integer overflow for files > 2GB)
            if (file.Size >= 0)
            {
                objResponse.AppendHeader("Content-Length", file.Size.ToString(CultureInfo.InvariantCulture));
            }

            objResponse.ContentType = this.GetContentType(file.Extension.Replace(".", string.Empty));

            try
            {
                using var fileContent = this.GetFileContent(file);
				Logger.LogError("WriteFileToHttpContext(file={File},contentDisposition={ContentDisposition}): fileContent → {FileContent}", file, contentDisposition, fileContent);
                this.WriteStream(objResponse, fileContent);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                objResponse.Write("Error : " + ex.Message);
            }

            objResponse.Flush();
            objResponse.End();

            HttpContext.Current.Server.ScriptTimeout = scriptTimeOut;
			Logger.LogWarning("<<< Exiting WriteFileToHttpContext(file={File},contentDisposition={ContentDisposition})", file, contentDisposition);
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="objResponse">The HTTP response.</param>
        /// <param name="objStream">The stream to write to the response stream.</param>
        internal virtual void WriteStream(HttpResponse objResponse, Stream objStream)
        {
			Logger.LogError(">>> Entering WriteStream(objResponse={ObjResponse},objStream={ObjStream})", objResponse, objStream);
            var bytBuffer = new byte[10000];
            try
            {
                if (objResponse.IsClientConnected)
                {
                    var intLength = objStream.Read(bytBuffer, 0, 10000);
					Logger.LogError("WriteStream(objResponse={ObjResponse},objStream={ObjStream}): intLength → {IntLength}", objResponse, objStream, intLength);

                    while (objResponse.IsClientConnected && intLength > 0)
                    {
                        objResponse.OutputStream.Write(bytBuffer, 0, intLength);
                        objResponse.Flush();

                        intLength = objStream.Read(bytBuffer, 0, 10000);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                objResponse.Write("Error : " + ex.Message);
            }
            finally
            {
                if (objStream != null)
                {
                    objStream.Close();
                    objStream.Dispose();
                }
            }
			Logger.LogWarning("<<< Exiting WriteStream(objResponse={ObjResponse},objStream={ObjStream})", objResponse, objStream);
        }

        /// <summary>Update file info to database.</summary>
        /// <param name="file">File info.</param>
        /// <param name="updateLazyload">Whether to update the lazy load properties: Width, Height, Sha1Hash.</param>
        /// <returns>The file info.</returns>
        internal virtual IFileInfo UpdateFile(IFileInfo file, bool updateLazyload)
        {
            // By default, File Events will be fired
            return this.UpdateFile(file, updateLazyload, true);
        }

        /// <summary>Update file info to database.</summary>
        /// <param name="file">File info.</param>
        /// <param name="updateLazyload">Whether to update the lazy load properties: Width, Height, Sha1Hash.</param>
        /// <param name="fireEvent">Whether to fire File events or not.</param>
        /// <returns>The file info.</returns>
        internal virtual IFileInfo UpdateFile(IFileInfo file, bool updateLazyload, bool fireEvent)
        {
            Requires.NotNull("file", file);

            this.dataProvider.UpdateFile(
                file.FileId,
                file.VersionGuid,
                file.FileName,
                file.Extension,
                file.Size,
                updateLazyload ? file.Width : Null.NullInteger,
                updateLazyload ? file.Height : Null.NullInteger,
                file.ContentType,
                file.FolderId,
                this.GetCurrentUserID(),
                updateLazyload ? file.SHA1Hash : Null.NullString,
                file.LastModificationTime,
                file.Title,
                file.Description,
                file.StartDate,
                file.EndDate,
                file.EnablePublishPeriod,
                file.ContentItemID);

            DataCache.RemoveCache("GetFileById" + file.FileId);
            this.ClearFolderCache(file.PortalId);
            var updatedFile = this.GetFile(file.FileId);

            if (fireEvent)
            {
                this.OnFileMetadataChanged(updatedFile ?? this.GetFile(file.FileId, true), this.GetCurrentUserID());
            }

            return updatedFile;
        }

        /// <summary>This member is reserved for internal use and is not intended to be used directly from your code.</summary>
        /// <param name="portalId">The portal ID.</param>
        internal virtual void ClearFolderCache(int portalId)
        {
			Logger.LogError(">>> Entering ClearFolderCache(portalId={PortalId})", portalId);
            DataCache.ClearFolderCache(portalId);
			Logger.LogWarning("<<< Exiting ClearFolderCache(portalId={PortalId})", portalId);
        }

        private static ImageFormat GetImageFormat(Image img)
        {
			Logger.LogError(">>> Entering GetImageFormat(img={Img})", img);
            if (img.RawFormat.Equals(ImageFormat.Jpeg))
            {
                return ImageFormat.Jpeg;
            }

            if (img.RawFormat.Equals(ImageFormat.Bmp))
            {
                return ImageFormat.Bmp;
            }

            if (img.RawFormat.Equals(ImageFormat.Png))
            {
                return ImageFormat.Png;
            }

            if (img.RawFormat.Equals(ImageFormat.Emf))
            {
                return ImageFormat.Emf;
            }

            if (img.RawFormat.Equals(ImageFormat.Exif))
            {
                return ImageFormat.Exif;
            }

            if (img.RawFormat.Equals(ImageFormat.Gif))
            {
                return ImageFormat.Gif;
            }

            if (img.RawFormat.Equals(ImageFormat.Icon))
            {
                return ImageFormat.Icon;
            }

            if (img.RawFormat.Equals(ImageFormat.MemoryBmp))
            {
                return ImageFormat.Jpeg;
            }

            if (img.RawFormat.Equals(ImageFormat.Tiff))
            {
                return ImageFormat.Tiff;
            }
            else
            {
				Logger.LogWarning("<<< Exiting GetImageFormat(img={Img})", img);
                return ImageFormat.Wmf;
            }
        }

        private static MemoryStream ToStream(Image image, ImageFormat format)
        {
			Logger.LogError(">>> Entering ToStream(image={Image},format={Format})", image, format);
            var stream = new MemoryStream();
            image.Save(stream, format);
			Logger.LogError("ToStream(image={Image},format={Format}): stream → {Stream}", image, format, stream);
            stream.Position = 0;
			Logger.LogDebug("ToStream(image={Image},format={Format}): stream.Position → {Arg2}", image, format, stream.Position);
			Logger.LogWarning("<<< Exiting ToStream(image={Image},format={Format})", image, format);
            return stream;
        }

        private static RotateFlipType OrientationToFlipType(string orientation)
        {
			Logger.LogError(">>> Entering OrientationToFlipType(orientation={Orientation})", orientation);
            switch (int.Parse(orientation, CultureInfo.InvariantCulture))
            {
                case 1:
                    return RotateFlipType.RotateNoneFlipNone;
                case 2:
                    return RotateFlipType.RotateNoneFlipX;
                case 3:
                    return RotateFlipType.Rotate180FlipNone;
                case 4:
                    return RotateFlipType.Rotate180FlipX;
                case 5:
                    return RotateFlipType.Rotate90FlipX;
                case 6:
                    return RotateFlipType.Rotate90FlipNone;
                case 7:
                    return RotateFlipType.Rotate270FlipX;
                case 8:
                    return RotateFlipType.Rotate270FlipNone;
                default:
					Logger.LogWarning("<<< Exiting OrientationToFlipType(orientation={Orientation})", orientation);
                    return RotateFlipType.RotateNoneFlipNone;
            }
        }

        private static void DeleteFileFromFolderProvider(IFileInfo file, FolderProvider provider)
        {
			Logger.LogError(">>> Entering DeleteFileFromFolderProvider(file={File},provider={Provider})", file, provider);
            try
            {
                // We can't delete the file until the fileContent resource has been released
                provider.DeleteFile(file);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
			Logger.LogWarning("<<< Exiting DeleteFileFromFolderProvider(file={File},provider={Provider})", file, provider);
                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        private bool ValidMetadata(IFileInfo file, out string exceptionMessage)
        {
			Logger.LogError(">>> Entering ValidMetadata(file={File})", file);
            exceptionMessage = string.Empty;

			Logger.LogDebug("ValidMetadata(file={File}): exceptionMessage → {ExceptionMessage}", file, exceptionMessage);
			Logger.LogError("ValidMetadata(file={File}): exceptionMessage → {ExceptionMessage}", file, exceptionMessage);
            // TODO check dynamically all required fields from MetadataInfo

            // TODO check dynamically all max lengths from MetadataInfo
            // TODO Use the MaxLength from MetadataInfo
            if (!string.IsNullOrEmpty(file.Title) && file.Title.Length > 256)
            {
                exceptionMessage = Localization.GetExceptionMessage("MaxLengthExceeded", "The maximum length of the field {0} has been exceeded", DefaultMetadataNames.Title);
                return false;
            }

            if (file.StartDate == null || file.StartDate == Null.NullDate)
            {
                exceptionMessage = Localization.GetExceptionMessage("StartDateRequired", "The Start Date is required");
                return false;
            }

            var savedFile = this.GetFile(file.FileId);
			Logger.LogError("ValidMetadata(file={File}): savedFile → {SavedFile}", file, savedFile);
            if (file.StartDate < file.CreatedOnDate.Date && file.StartDate != savedFile.StartDate)
            {
                exceptionMessage = Localization.GetExceptionMessage("StartDateMustNotBeInThePast", "The Start Date must not be in the past");
                return false;
            }

            if (file.EndDate != Null.NullDate && file.StartDate > file.EndDate)
            {
                exceptionMessage = Localization.GetExceptionMessage("InvalidPublishPeriod", "The End Date must be after the Start Date");
                return false;
            }

			Logger.LogWarning("<<< Exiting ValidMetadata(file={File})", file);
            return true;
        }

        private void OnFileDeleted(IFileInfo fileInfo, int userId)
        {
			Logger.LogError(">>> Entering OnFileDeleted(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
            this.eventManager.OnFileDeleted(new FileDeletedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                IsCascadeDeleting = false,
            });
			Logger.LogWarning("<<< Exiting OnFileDeleted(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
        }

        private void OnFileRenamed(IFileInfo fileInfo, string oldFileName, int userId)
        {
			Logger.LogError(">>> Entering OnFileRenamed(fileInfo={FileInfo},oldFileName={OldFileName},userId={UserId})", fileInfo, oldFileName, userId);
            this.eventManager.OnFileRenamed(new FileRenamedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                OldFileName = oldFileName,
            });
			Logger.LogWarning("<<< Exiting OnFileRenamed(fileInfo={FileInfo},oldFileName={OldFileName},userId={UserId})", fileInfo, oldFileName, userId);
        }

        private void OnFileMoved(IFileInfo fileInfo, string oldFilePath, int userId)
        {
			Logger.LogError(">>> Entering OnFileMoved(fileInfo={FileInfo},oldFilePath={OldFilePath},userId={UserId})", fileInfo, oldFilePath, userId);
            this.eventManager.OnFileMoved(new FileMovedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                OldFilePath = oldFilePath,
            });
			Logger.LogWarning("<<< Exiting OnFileMoved(fileInfo={FileInfo},oldFilePath={OldFilePath},userId={UserId})", fileInfo, oldFilePath, userId);
        }

        private void OnFileOverwritten(IFileInfo fileInfo, int userId)
        {
			Logger.LogError(">>> Entering OnFileOverwritten(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
            this.eventManager.OnFileOverwritten(new FileChangedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
            });
			Logger.LogWarning("<<< Exiting OnFileOverwritten(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
        }

        private void OnFileMetadataChanged(IFileInfo fileInfo, int userId)
        {
			Logger.LogError(">>> Entering OnFileMetadataChanged(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
            this.eventManager.OnFileMetadataChanged(new FileChangedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
            });
			Logger.LogWarning("<<< Exiting OnFileMetadataChanged(fileInfo={FileInfo},userId={UserId})", fileInfo, userId);
        }

        private void OnFileAdded(IFileInfo fileInfo, IFolderInfo folderInfo, int userId)
        {
			Logger.LogError(">>> Entering OnFileAdded(fileInfo={FileInfo},folderInfo={FolderInfo},userId={UserId})", fileInfo, folderInfo, userId);
            this.eventManager.OnFileAdded(new FileAddedEventArgs
            {
                FileInfo = fileInfo,
                UserId = userId,
                FolderInfo = folderInfo,
            });
			Logger.LogWarning("<<< Exiting OnFileAdded(fileInfo={FileInfo},folderInfo={FolderInfo},userId={UserId})", fileInfo, folderInfo, userId);
        }

        private void NotifyFileAddingEvents(IFolderInfo folder, int createdByUserId, bool fileExists, Workflow folderWorkflow, IFileInfo file)
        {
			Logger.LogError(">>> Entering NotifyFileAddingEvents(folder={Folder},createdByUserId={CreatedByUserId},fileExists={FileExists},folderWorkflow={FolderWorkflow},file={File})", folder, createdByUserId, fileExists, folderWorkflow, file);
            // Notify file event
            if (fileExists &&
                (folderWorkflow == null || folderWorkflow.WorkflowID == this.systemWorkflowManager.GetDirectPublishWorkflow(folderWorkflow.PortalID).WorkflowID))
            {
                this.OnFileOverwritten(file, createdByUserId);
            }

            if (!fileExists)
            {
                this.OnFileAdded(file, folder, createdByUserId);
            }
			Logger.LogWarning("<<< Exiting NotifyFileAddingEvents(folder={Folder},createdByUserId={CreatedByUserId},fileExists={FileExists},folderWorkflow={FolderWorkflow},file={File})", folder, createdByUserId, fileExists, folderWorkflow, file);
        }

        private void AddFile(FileInfo file, int createdByUserId)
        {
            file.FileId = this.dataProvider.AddFile(
                file.PortalId,
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
                createdByUserId,
                file.SHA1Hash,
                file.LastModificationTime,
                file.Title,
                file.Description,
                file.StartDate,
                file.EndDate,
                file.EnablePublishPeriod,
                file.ContentItemID);
        }

        private string ProcessVersioning(IFolderInfo folder, IFileInfo oldFile, FileInfo file, int createdByUserId)
        {
			Logger.LogError(">>> Entering ProcessVersioning(folder={Folder},oldFile={OldFile},file={File},createdByUserId={CreatedByUserId})", folder, oldFile, file, createdByUserId);
            if (oldFile != null && this.fileVersionController.IsFolderVersioned(folder) && oldFile.SHA1Hash != file.SHA1Hash)
            {
                return this.fileVersionController.AddFileVersion(oldFile, createdByUserId);
            }

			Logger.LogWarning("<<< Exiting ProcessVersioning(folder={Folder},oldFile={OldFile},file={File},createdByUserId={CreatedByUserId})", folder, oldFile, file, createdByUserId);
            return file.FileName;
        }

        private bool CanUpdateWhenApproving(IFolderInfo folder, ContentItem item, int createdByUserId)
        {
			Logger.LogError(">>> Entering CanUpdateWhenApproving(folder={Folder},item={Item},createdByUserId={CreatedByUserId})", folder, item, createdByUserId);
            if (this.workflowEngine.IsWorkflowOnDraft(item.ContentItemId))
            {
                // We assume User can add content to folder
                return true;
            }

			Logger.LogWarning("<<< Exiting CanUpdateWhenApproving(folder={Folder},item={Item},createdByUserId={CreatedByUserId})", folder, item, createdByUserId);
            return this.workflowSecurity.HasStateReviewerPermission(folder.PortalID, createdByUserId, item.StateID);
        }

        private bool StartWorkflow(int createdByUserId, Workflow folderWorkflow, int contentItemId)
        {
			Logger.LogError(">>> Entering StartWorkflow(createdByUserId={CreatedByUserId},folderWorkflow={FolderWorkflow},contentItemId={ContentItemId})", createdByUserId, folderWorkflow, contentItemId);
            if (this.workflowEngine.IsWorkflowCompleted(contentItemId))
            {
                this.workflowEngine.StartWorkflow(folderWorkflow.WorkflowID, contentItemId, createdByUserId);
                return true;
            }

			Logger.LogWarning("<<< Exiting StartWorkflow(createdByUserId={CreatedByUserId},folderWorkflow={FolderWorkflow},contentItemId={ContentItemId})", createdByUserId, folderWorkflow, contentItemId);
            return false;
        }

        private string UpdateWhileApproving(IFolderInfo folder, int createdByUserId, FileInfo file, IFileInfo oldFile, Stream content)
        {
			Logger.LogError(">>> Entering UpdateWhileApproving(folder={Folder},createdByUserId={CreatedByUserId},file={File},oldFile={OldFile},content={Content})", folder, createdByUserId, file, oldFile, content);
            bool workflowCompleted = this.workflowEngine.IsWorkflowCompleted(file.ContentItemID);

            var isDatabaseMapping = this.folderMappingController.GetFolderMapping(folder.PortalID, folder.FolderMappingID).MappingName == "Database";
			Logger.LogError("UpdateWhileApproving(folder={Folder},createdByUserId={CreatedByUserId},file={File},oldFile={OldFile},content={Content}): isDatabaseMapping → {IsDatabaseMapping}", folder, createdByUserId, file, oldFile, content, isDatabaseMapping);

            // If the file does not exist, then the field would not have a value.
            // Currently, first upload has not version file
            if (oldFile is not { HasBeenPublished: true, })
            {
                return file.FileName;
            }

            if (workflowCompleted)
            {
                // We assume User can add content to folder
                return isDatabaseMapping ? this.fileVersionController.AddFileVersion(file, createdByUserId, false, false, content) : this.fileVersionController.AddFileVersion(file, createdByUserId, false);
            }

            if (this.CanUpdateWhenApproving(folder, this.contentController.GetContentItem(file.ContentItemID), createdByUserId))
            {
                // Update the Unpublished version
                var versions = this.fileVersionController.GetFileVersions(file).ToArray();
				Logger.LogError("UpdateWhileApproving(folder={Folder},createdByUserId={CreatedByUserId},file={File},oldFile={OldFile},content={Content}): versions → {Versions}", folder, createdByUserId, file, oldFile, content, versions);
                if (versions.Length != 0)
                {
                    this.fileVersionController.DeleteFileVersion(file, versions.OrderByDescending(f => f.Version).FirstOrDefault().Version);
                }

                return isDatabaseMapping ? this.fileVersionController.AddFileVersion(file, createdByUserId, false, false, content) : this.fileVersionController.AddFileVersion(file, createdByUserId, false);
            }

					Logger.LogError("Throwing FileLockedException from UpdateWhileApproving(folder={Folder},createdByUserId={CreatedByUserId},file={File},oldFile={OldFile},content={Content})", folder, createdByUserId, file, oldFile, content);
					Logger.LogWarning("<<< Exiting UpdateWhileApproving(folder={Folder},createdByUserId={CreatedByUserId},file={File},oldFile={OldFile},content={Content})", folder, createdByUserId, file, oldFile, content);
            throw new FileLockedException(
                Localization.GetExceptionMessage(
                    "FileLockedRunningWorkflowError",
                    "File locked. The file cannot be updated because it has a running workflow"));
        }

        private void AddFileToFolderProvider(Stream fileContent, string fileName, IFolderInfo destinationFolder, FolderProvider provider)
        {
			Logger.LogError(">>> Entering AddFileToFolderProvider(fileContent={FileContent},fileName={FileName},destinationFolder={DestinationFolder},provider={Provider})", fileContent, fileName, destinationFolder, provider);
            try
            {
                if (!fileContent.CanSeek)
                {
                    using var seekableStream = this.GetSeekableStream(fileContent);
					Logger.LogError("AddFileToFolderProvider(fileContent={FileContent},fileName={FileName},destinationFolder={DestinationFolder},provider={Provider}): seekableStream → {SeekableStream}", fileContent, fileName, destinationFolder, provider, seekableStream);
                    provider.AddFile(destinationFolder, fileName, seekableStream);
                }
                else
                {
                    provider.AddFile(destinationFolder, fileName, fileContent);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
			Logger.LogWarning("<<< Exiting AddFileToFolderProvider(fileContent={FileContent},fileName={FileName},destinationFolder={DestinationFolder},provider={Provider})", fileContent, fileName, destinationFolder, provider);
                throw new FolderProviderException(Localization.GetExceptionMessage("UnderlyingSystemError", "The underlying system threw an exception."), ex);
            }
        }

        /// <summary>Rotate/Flip the image as per the metadata and reset the metadata.</summary>
        /// <param name="content">The image contents.</param>
        private void RotateFlipImage(ref Stream content)
        {
			Logger.LogError(">>> Entering RotateFlipImage(content={Content})", content);
            try
            {
                using var image = this.GetImageFromStream(content);
				Logger.LogError("RotateFlipImage(content={Content}): image → {Image}", content, image);
                if (image.PropertyIdList.All(x => x != 274))
                {
                    return;
                }

                var orientation = image.GetPropertyItem(274); // Find rotation/flip meta property
				Logger.LogError("RotateFlipImage(content={Content}): orientation → {Orientation}", content, orientation);
                if (orientation == null)
                {
                    return;
                }

                var flip = OrientationToFlipType(orientation.Value[0].ToString(CultureInfo.InvariantCulture));
				Logger.LogError("RotateFlipImage(content={Content}): flip → {Flip}", content, flip);
                if (flip == RotateFlipType.RotateNoneFlipNone)
                {
                    return; // No rotation or flip required
                }

                image.RotateFlip(flip);
                var newOrientation = new byte[2];
                newOrientation[0] = 1; // little Endian
                newOrientation[1] = 0;
                orientation.Value = newOrientation;
				Logger.LogDebug("RotateFlipImage(content={Content}): orientation.Value → {Arg1}", content, orientation.Value);
                image.SetPropertyItem(orientation);
                content = ToStream(image, GetImageFormat(image));
				Logger.LogError("RotateFlipImage(content={Content}): content → {Content1}", content, content);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting RotateFlipImage(content={Content})", content);
        }

        private void CheckFileAddingRestrictions(IFolderInfo folder, string fileName, bool checkPermissions, bool ignoreWhiteList)
          {
			Logger.LogError(">>> Entering CheckFileAddingRestrictions(folder={Folder},fileName={FileName},checkPermissions={CheckPermissions},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, checkPermissions, ignoreWhiteList);
            if (checkPermissions && !this.folderPermissionController.CanAddFolder(folder))
            {
				Logger.LogError("Throwing PermissionsNotMetException from CheckFileAddingRestrictions(folder={Folder},fileName={FileName},checkPermissions={CheckPermissions},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, checkPermissions, ignoreWhiteList);
                throw new PermissionsNotMetException(Localization.GetExceptionMessage(
                    "AddFilePermissionsNotMet",
                    "Permissions are not met. The file has not been added."));
            }

            if (!this.IsAllowedExtension(fileName) && !(this.userController.GetCurrentUserInfo().IsSuperUser && ignoreWhiteList))
            {
				Logger.LogError("Throwing InvalidFileExtensionException from CheckFileAddingRestrictions(folder={Folder},fileName={FileName},checkPermissions={CheckPermissions},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, checkPermissions, ignoreWhiteList);
                throw new InvalidFileExtensionException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Localization.GetExceptionMessage(
                            "AddFileExtensionNotAllowed",
                            "The extension '{0}' is not allowed. The file has not been added."),
                        Path.GetExtension(fileName)));
            }

            if (!this.IsValidFilename(fileName))
            {
				Logger.LogError("Throwing InvalidFilenameException from CheckFileAddingRestrictions(folder={Folder},fileName={FileName},checkPermissions={CheckPermissions},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, checkPermissions, ignoreWhiteList);
                throw new InvalidFilenameException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Localization.GetExceptionMessage(
                            "AddFilenameNotAllowed",
                            "The file name '{0}' is not allowed. The file has not been added."),
                        fileName));
            }
			Logger.LogWarning("<<< Exiting CheckFileAddingRestrictions(folder={Folder},fileName={FileName},checkPermissions={CheckPermissions},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, checkPermissions, ignoreWhiteList);
        }

        private void SetContentItem(FileInfo file)
        {
			Logger.LogError(">>> Entering SetContentItem(file={File})", file);
            // Create Content Item if it does not exist
            if (file.ContentItemID == Null.NullInteger)
            {
                file.ContentItemID = this.CreateFileContentItem().ContentItemId;
            }
			Logger.LogWarning("<<< Exiting SetContentItem(file={File})", file);
        }

        private void SetInitialFileMetadata(ref Stream fileContent, FileInfo file, FolderProvider folderProvider)
        {
			Logger.LogError(">>> Entering SetInitialFileMetadata(fileContent={FileContent},file={File},folderProvider={FolderProvider})", fileContent, file, folderProvider);
            file.Size = (int)fileContent.Length;
            var fileHash = folderProvider.GetHashCode(file, fileContent);
			Logger.LogError("SetInitialFileMetadata(fileContent={FileContent},file={File},folderProvider={FolderProvider}): fileHash → {FileHash}", fileContent, file, folderProvider, fileHash);
			Logger.LogDebug("SetInitialFileMetadata(fileContent={FileContent},file={File},folderProvider={FolderProvider}): fileContent.Position → {Arg3}", fileContent, file, folderProvider, fileContent.Position);
            file.SHA1Hash = fileHash;
			Logger.LogDebug("SetInitialFileMetadata(fileContent={FileContent},file={File},folderProvider={FolderProvider}): file.SHA1Hash → {Arg3}", fileContent, file, folderProvider, file.SHA1Hash);
            fileContent.Position = 0;

            file.Width = 0;
			Logger.LogDebug("SetInitialFileMetadata(fileContent={FileContent},file={File},folderProvider={FolderProvider}): file.Width → {Arg3}", fileContent, file, folderProvider, file.Width);
            file.Height = 0;
			Logger.LogDebug("SetInitialFileMetadata(fileContent={FileContent},file={File},folderProvider={FolderProvider}): file.Height → {Arg3}", fileContent, file, folderProvider, file.Height);

            if (this.IsImageFile(file))
            {
                this.RotateFlipImage(ref fileContent);
                this.SetImageProperties(file, fileContent);
            }
			Logger.LogWarning("<<< Exiting SetInitialFileMetadata(fileContent={FileContent},file={File},folderProvider={FolderProvider})", fileContent, file, folderProvider);
        }

        private void SetImageProperties(FileInfo file, Stream fileContent)
        {
			Logger.LogError(">>> Entering SetImageProperties(file={File},fileContent={FileContent})", file, fileContent);
            try
            {
                using var image = this.GetImageFromStream(fileContent);
				Logger.LogError("SetImageProperties(file={File},fileContent={FileContent}): image → {Image}", file, fileContent, image);
				Logger.LogDebug("SetImageProperties(file={File},fileContent={FileContent}): file.Height → {Arg2}", file, fileContent, file.Height);
                file.Width = image.Width;
				Logger.LogDebug("SetImageProperties(file={File},fileContent={FileContent}): file.Width → {Arg2}", file, fileContent, file.Width);
                file.Height = image.Height;
            }
            catch
            {
				Logger.LogError("Exception in SetImageProperties(file={File},fileContent={FileContent}): {Arg2}", file, fileContent, ex.Message);
                file.ContentType = "application/octet-stream";
				Logger.LogDebug("SetImageProperties(file={File},fileContent={FileContent}): file.ContentType → {Arg2}", file, fileContent, file.ContentType);
            }
            finally
            {
                fileContent.Position = 0;
				Logger.LogDebug("SetImageProperties(file={File},fileContent={FileContent}): fileContent.Position → {Arg2}", file, fileContent, fileContent.Position);
            }
			Logger.LogWarning("<<< Exiting SetImageProperties(file={File},fileContent={FileContent})", file, fileContent);
        }

        private void CheckFileWritingRestrictions(IFolderInfo folder, string fileName, Stream fileContent, IFileInfo oldFile, int createdByUserId, bool ignoreWhiteList)
        {
			Logger.LogError(">>> Entering CheckFileWritingRestrictions(folder={Folder},fileName={FileName},fileContent={FileContent},oldFile={OldFile},createdByUserId={CreatedByUserId},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, fileContent, oldFile, createdByUserId, ignoreWhiteList);
            if (!this.portalController.HasSpaceAvailable(folder.PortalID, fileContent.Length))
            {
				Logger.LogError("Throwing NoSpaceAvailableException from CheckFileWritingRestrictions(folder={Folder},fileName={FileName},fileContent={FileContent},oldFile={OldFile},createdByUserId={CreatedByUserId},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, fileContent, oldFile, createdByUserId, ignoreWhiteList);
                throw new NoSpaceAvailableException(
                    Localization.GetExceptionMessage(
                        "AddFileNoSpaceAvailable",
                        "The portal has no space available to store the specified file. The file has not been added."));
            }

            // Publish Period
            if (oldFile != null && this.fileLockingController.IsFileOutOfPublishPeriod(oldFile, folder.PortalID, createdByUserId))
            {
				Logger.LogError("Throwing FileLockedException from CheckFileWritingRestrictions(folder={Folder},fileName={FileName},fileContent={FileContent},oldFile={OldFile},createdByUserId={CreatedByUserId},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, fileContent, oldFile, createdByUserId, ignoreWhiteList);
                throw new FileLockedException(
                    Localization.GetExceptionMessage(
                        "FileLockedOutOfPublishPeriodError",
                        "File locked. The file cannot be updated because it is out of Publish Period"));
            }

            if (!this.fileSecurityController.Validate(fileName, fileContent))
            {
                const string DefaultMessage = "The content of '{0}' is not valid. The file has not been added.";
                var errorMessage = Localization.GetExceptionMessage("AddFileInvalidContent", DefaultMessage);
				Logger.LogError("Throwing InvalidFileContentException from CheckFileWritingRestrictions(folder={Folder},fileName={FileName},fileContent={FileContent},oldFile={OldFile},createdByUserId={CreatedByUserId},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, fileContent, oldFile, createdByUserId, ignoreWhiteList);
				Logger.LogError("CheckFileWritingRestrictions(folder={Folder},fileName={FileName},fileContent={FileContent},oldFile={OldFile},createdByUserId={CreatedByUserId},ignoreWhiteList={IgnoreWhiteList}): errorMessage → {ErrorMessage}", folder, fileName, fileContent, oldFile, createdByUserId, ignoreWhiteList, errorMessage);
                throw new InvalidFileContentException(string.Format(CultureInfo.CurrentCulture, errorMessage, fileName));
            }

            var checkWhiteList = !(this.userController.GetCurrentUserInfo().IsSuperUser && ignoreWhiteList);
			Logger.LogError("CheckFileWritingRestrictions(folder={Folder},fileName={FileName},fileContent={FileContent},oldFile={OldFile},createdByUserId={CreatedByUserId},ignoreWhiteList={IgnoreWhiteList}): checkWhiteList → {CheckWhiteList}", folder, fileName, fileContent, oldFile, createdByUserId, ignoreWhiteList, checkWhiteList);
            if (checkWhiteList && !this.WhiteList.IsAllowedExtension(".exe") && !this.fileSecurityController.ValidateNotExectuable(fileContent))
            {
                const string DefaultMessage = "The content of '{0}' is not valid. The file has not been added.";
                var errorMessage = Localization.GetExceptionMessage("AddFileInvalidContent", DefaultMessage);
                throw new InvalidFileContentException(string.Format(CultureInfo.CurrentCulture, errorMessage, fileName));
            }
			Logger.LogWarning("<<< Exiting CheckFileWritingRestrictions(folder={Folder},fileName={FileName},fileContent={FileContent},oldFile={OldFile},createdByUserId={CreatedByUserId},ignoreWhiteList={IgnoreWhiteList})", folder, fileName, fileContent, oldFile, createdByUserId, ignoreWhiteList);
        }

        private void ManageFileAdding(int createdByUserId, Workflow folderWorkflow, bool fileExists, FileInfo file)
        {
			Logger.LogError(">>> Entering ManageFileAdding(createdByUserId={CreatedByUserId},folderWorkflow={FolderWorkflow},fileExists={FileExists},file={File})", createdByUserId, folderWorkflow, fileExists, file);
            if (folderWorkflow == null || !fileExists)
            {
                this.AddFile(file, createdByUserId);
            }
            else
            {
                // File Events for updating will not be fired. Only events for adding must be fired
                this.UpdateFile(file, true, false);
            }

            if (folderWorkflow != null && this.StartWorkflow(createdByUserId, folderWorkflow, file.ContentItemID))
            {
                // if file exists it could have been published. So We don't have to update the field
                if (!fileExists)
                {
                    // Maybe here we can set HasBeenPublished as 0
                    this.dataProvider.SetFileHasBeenPublished(file.FileId, false);
                }
            }
			Logger.LogWarning("<<< Exiting ManageFileAdding(createdByUserId={CreatedByUserId},folderWorkflow={FolderWorkflow},fileExists={FileExists},file={File})", createdByUserId, folderWorkflow, fileExists, file);
        }
    }
}
