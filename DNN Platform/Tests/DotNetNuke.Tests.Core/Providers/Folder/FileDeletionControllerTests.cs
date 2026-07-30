// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Core.Providers.Builders;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    public class FileDeletionControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Mock<IFileLockingController> mockFileLockingController;
        private Mock<DataProvider> mockData;
        private Mock<IFileVersionController> fileVersionController;
        private Mock<IFolderMappingController> folderMappingController;
        private Mock<FolderProvider> mockFolderProvider;
        private Mock<IContentController> mockContentController;
        private FileDeletionController fileDeletionController;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this.mockFileLockingController = new Mock<IFileLockingController>();
            this.mockData = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("Setup(): mockData → {MockData}", mockData);
			Logger.LogDebug("Setup(): mockFileLockingController → {MockFileLockingController}", mockFileLockingController);
            this.fileVersionController = new Mock<IFileVersionController>();
			Logger.LogDebug("Setup(): fileVersionController → {FileVersionController}", fileVersionController);
            this.folderMappingController = new Mock<IFolderMappingController>();
			Logger.LogDebug("Setup(): folderMappingController → {FolderMappingController}", folderMappingController);
            this.mockFolderProvider = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
			Logger.LogDebug("Setup(): mockFolderProvider → {MockFolderProvider}", mockFolderProvider);
            this.mockContentController = new Mock<IContentController>();
			Logger.LogDebug("Setup(): mockContentController → {MockContentController}", mockContentController);
            MockComponentProvider.CreateDataCacheProvider();
            FileLockingController.SetTestableInstance(this.mockFileLockingController.Object);
            FileVersionController.RegisterInstance(this.fileVersionController.Object);
            FolderMappingController.RegisterInstance(this.folderMappingController.Object);

            this.fileDeletionController = new FileDeletionController(this.mockFileLockingController.Object, this.fileVersionController.Object, this.folderMappingController.Object, this.mockContentController.Object, this.mockData.Object);
			Logger.LogDebug("<<< Exiting Setup()");
			Logger.LogDebug("Setup(): fileDeletionController → {FileDeletionController}", fileDeletionController);
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            FileLockingController.ClearInstance();
            MockComponentProvider.ResetContainer();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void DeleteFile_Calls_FolderProviderDeleteFile()
        {
			Logger.LogDebug(">>> Entering DeleteFile_Calls_FolderProviderDeleteFile()");
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();
            this.fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));
			Logger.LogDebug("DeleteFile_Calls_FolderProviderDeleteFile(): fileInfo → {FileInfo}", fileInfo);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockData.Setup(md => md.DeleteFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));

            this.mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo)).Verifiable();

            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            // Act
            this.fileDeletionController.DeleteFile(fileInfo);

            // Assert
            this.mockFolderProvider.Verify();
			Logger.LogDebug("<<< Exiting DeleteFile_Calls_FolderProviderDeleteFile()");
        }

        [Test]
        public void DeleteFile_Throws_WhenFileIsLocked()
        {
			Logger.LogDebug(">>> Entering DeleteFile_Throws_WhenFileIsLocked()");
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();

			Logger.LogDebug("DeleteFile_Throws_WhenFileIsLocked(): fileInfo → {FileInfo}", fileInfo);
            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(true);

            // Act
            Assert.Throws<FileLockedException>(() => this.fileDeletionController.DeleteFile(fileInfo));
			Logger.LogDebug("<<< Exiting DeleteFile_Throws_WhenFileIsLocked()");
        }

        [Test]
        public void DeleteFile_Throws_WhenFolderProviderThrows()
        {
			Logger.LogDebug(">>> Entering DeleteFile_Throws_WhenFolderProviderThrows()");
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();

			Logger.LogDebug("DeleteFile_Throws_WhenFolderProviderThrows(): fileInfo → {FileInfo}", fileInfo);
            this.fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            this.mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo)).Throws<Exception>();

            Assert.Throws<FolderProviderException>(() => this.fileDeletionController.DeleteFile(fileInfo));
			Logger.LogDebug("<<< Exiting DeleteFile_Throws_WhenFolderProviderThrows()");
        }

        [Test]
        public void DeleteFileData_Calls_DataProviderDeleteFile()
        {
			Logger.LogDebug(">>> Entering DeleteFileData_Calls_DataProviderDeleteFile()");
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();

			Logger.LogDebug("DeleteFileData_Calls_DataProviderDeleteFile(): fileInfo → {FileInfo}", fileInfo);
            this.fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockData.Setup(md => md.DeleteFile(Constants.CONTENT_ValidPortalId, It.IsAny<string>(), Constants.FOLDER_ValidFolderId)).Verifiable();

            this.mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo));

            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            // Act
            this.fileDeletionController.DeleteFileData(fileInfo);

            // Assert
            this.mockData.Verify();
			Logger.LogDebug("<<< Exiting DeleteFileData_Calls_DataProviderDeleteFile()");
        }

        [Test]
        public void DeleteFileData_Calls_ContentControllerDeleteContentItem()
        {
			Logger.LogDebug(">>> Entering DeleteFileData_Calls_ContentControllerDeleteContentItem()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithContentItemId(Constants.CONTENT_ValidContentItemId)
                .Build();

            this.fileVersionController.Setup(fv => fv.DeleteAllUnpublishedVersions(fileInfo, false));

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockData.Setup(md => md.DeleteFile(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));

            this.mockFolderProvider.Setup(mf => mf.DeleteFile(fileInfo));

            this.mockContentController.Setup(mcc => mcc.DeleteContentItem(Constants.CONTENT_ValidContentItemId)).Verifiable();

            string someString;
            this.mockFileLockingController.Setup(mflc => mflc.IsFileLocked(fileInfo, out someString)).Returns(false);

            // Act
            this.fileDeletionController.DeleteFileData(fileInfo);

            // Assert
            this.mockContentController.Verify();
			Logger.LogDebug("<<< Exiting DeleteFileData_Calls_ContentControllerDeleteContentItem()");
        }
    }
}
