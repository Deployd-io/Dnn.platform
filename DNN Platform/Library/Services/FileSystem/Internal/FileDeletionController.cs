// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class FileDeletionController(IFileLockingController fileLockingController, IFileVersionController fileVersionController, IFolderMappingController folderMappingController, IContentController contentController, DataProvider dataProvider)
        : ServiceLocator<IFileDeletionController, FileDeletionController>, IFileDeletionController
    {
		Logger.LogDebug(">>> Entering FileDeletionController(fileLockingController={FileLockingController},fileVersionController={FileVersionController},folderMappingController={FolderMappingController},contentController={ContentController},dataProvider={DataProvider})", fileLockingController, fileVersionController, folderMappingController, contentController, dataProvider);
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileDeletionController));
        private readonly IFileLockingController fileLockingController = fileLockingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileLockingController>();
		Logger.LogDebug("FileDeletionController(fileLockingController={FileLockingController},fileVersionController={FileVersionController},folderMappingController={FolderMappingController},contentController={ContentController},dataProvider={DataProvider}): Logger → {Logger}", fileLockingController, fileVersionController, folderMappingController, contentController, dataProvider, Logger);
		Logger.LogDebug("FileDeletionController(fileLockingController={FileLockingController},fileVersionController={FileVersionController},folderMappingController={FolderMappingController},contentController={ContentController},dataProvider={DataProvider}): fileLockingController → {FileLockingController1}", fileLockingController, fileVersionController, folderMappingController, contentController, dataProvider, fileLockingController);
        private readonly IFileVersionController fileVersionController = fileVersionController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileVersionController>();
        private readonly IFolderMappingController folderMappingController = folderMappingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderMappingController>();
		Logger.LogDebug("FileDeletionController(fileLockingController={FileLockingController},fileVersionController={FileVersionController},folderMappingController={FolderMappingController},contentController={ContentController},dataProvider={DataProvider}): fileVersionController → {FileVersionController1}", fileLockingController, fileVersionController, folderMappingController, contentController, dataProvider, fileVersionController);
		Logger.LogDebug("FileDeletionController(fileLockingController={FileLockingController},fileVersionController={FileVersionController},folderMappingController={FolderMappingController},contentController={ContentController},dataProvider={DataProvider}): folderMappingController → {FolderMappingController1}", fileLockingController, fileVersionController, folderMappingController, contentController, dataProvider, folderMappingController);
        private readonly IContentController contentController = contentController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IContentController>();
        private readonly DataProvider dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
		Logger.LogDebug("FileDeletionController(fileLockingController={FileLockingController},fileVersionController={FileVersionController},folderMappingController={FolderMappingController},contentController={ContentController},dataProvider={DataProvider}): contentController → {ContentController1}", fileLockingController, fileVersionController, folderMappingController, contentController, dataProvider, contentController);
		Logger.LogDebug("FileDeletionController(fileLockingController={FileLockingController},fileVersionController={FileVersionController},folderMappingController={FolderMappingController},contentController={ContentController},dataProvider={DataProvider}): dataProvider → {DataProvider1}", fileLockingController, fileVersionController, folderMappingController, contentController, dataProvider, dataProvider);

        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IFileLockingController. Scheduled removal in v12.0.0.")]
        public FileDeletionController()
            : this(null, null, null, null, null)
        {
        }

        /// <inheritdoc />
        public void DeleteFile(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering DeleteFile(file={File})", file);
            if (this.fileLockingController.IsFileLocked(file, out var lockReason))
            {
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            this.fileVersionController.DeleteAllUnpublishedVersions(file, false);
            try
            {
                var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
                FolderProvider.Instance(folderMapping.FolderProviderType).DeleteFile(file);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new FolderProviderException(Localization.GetExceptionMessage("DeleteFileUnderlyingSystemError", "The underlying system threw an exception. The file has not been deleted."), ex);
            }

            this.DeleteFileData(file);

            DataCache.RemoveCache("GetFileById" + file.FileId);
			Logger.LogDebug("<<< Exiting DeleteFile(file={File})", file);
        }

        /// <inheritdoc />
        public void UnlinkFile(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering UnlinkFile(file={File})", file);
            if (this.fileLockingController.IsFileLocked(file, out var lockReason))
            {
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            this.fileVersionController.DeleteAllUnpublishedVersions(file, false);

            this.DeleteFileData(file);
			Logger.LogDebug("<<< Exiting UnlinkFile(file={File})", file);
        }

        /// <inheritdoc />
        public void DeleteFileData(IFileInfo file)
        {
			Logger.LogDebug(">>> Entering DeleteFileData(file={File})", file);
            this.dataProvider.DeleteFile(file.PortalId, file.FileName, file.FolderId);
            this.DeleteContentItem(file.ContentItemID);
			Logger.LogDebug("<<< Exiting DeleteFileData(file={File})", file);
        }

        /// <inheritdoc />
        protected override Func<IFileDeletionController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IFileDeletionController>();
        }

        private void DeleteContentItem(int contentItemId)
        {
			Logger.LogDebug(">>> Entering DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
            if (contentItemId == Null.NullInteger)
            {
                return;
            }

            this.contentController.DeleteContentItem(contentItemId);
			Logger.LogDebug("<<< Exiting DeleteContentItem(contentItemId={ContentItemId})", contentItemId);
        }
    }
}
