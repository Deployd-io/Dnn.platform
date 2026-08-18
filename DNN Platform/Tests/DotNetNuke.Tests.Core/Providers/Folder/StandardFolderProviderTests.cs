// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;

    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using ICryptographyProvider = DotNetNuke.Abstractions.Security.ICryptographyProvider;

    [TestFixture]
    public class StandardFolderProviderTests
    {
        private StandardFolderProvider sfp;
        private Mock<IFolderInfo> folderInfo;
        private Mock<IFileInfo> fileInfo;
        private Mock<IFile> fileWrapper;
        private Mock<IDirectory> directoryWrapper;
        private Mock<IFolderManager> folderManager;
        private Mock<IFileManager> fileManager;
        private Mock<IPathUtils> pathUtils;
        private Mock<IPortalController> portalControllerMock;
        private Mock<ILocaleController> localeControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            this.sfp = new StandardFolderProvider();
            this.folderInfo = new Mock<IFolderInfo>();
			mockEventLogger.LogDebug("Setup(): folderInfo → {FolderInfo}", folderInfo);
            this.fileInfo = new Mock<IFileInfo>();
			mockEventLogger.LogDebug("Setup(): fileInfo → {FileInfo}", fileInfo);
            this.fileWrapper = new Mock<IFile>();
			mockEventLogger.LogDebug("Setup(): fileWrapper → {FileWrapper}", fileWrapper);
            this.directoryWrapper = new Mock<IDirectory>();
			mockEventLogger.LogDebug("Setup(): directoryWrapper → {DirectoryWrapper}", directoryWrapper);
            this.folderManager = new Mock<IFolderManager>();
			mockEventLogger.LogDebug("Setup(): folderManager → {FolderManager}", folderManager);
            this.fileManager = new Mock<IFileManager>();
			mockEventLogger.LogDebug("Setup(): fileManager → {FileManager}", fileManager);
            this.pathUtils = new Mock<IPathUtils>();
			mockEventLogger.LogDebug("Setup(): pathUtils → {PathUtils}", pathUtils);
            this.portalControllerMock = new Mock<IPortalController>();
			mockEventLogger.LogDebug("Setup(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
            this.portalControllerMock.Setup(p => p.GetPortalSettings(Constants.CONTENT_ValidPortalId))
                .Returns(this.GetPortalSettingsDictionaryMock());
            this.portalControllerMock.Setup(p => p.GetCurrentPortalSettings()).Returns(this.GetPortalSettingsMock());
            var cryptographyProviderMock = new Mock<ICryptographyProvider>();
			mockEventLogger.LogDebug("Setup(): cryptographyProviderMock → {CryptographyProviderMock}", cryptographyProviderMock);
            cryptographyProviderMock.Setup(c => c.EncryptParameter(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((Guid.NewGuid().ToString("N"), nameof(Aes)));
            this.localeControllerMock = new Mock<ILocaleController>();
			mockEventLogger.LogDebug("Setup(): localeControllerMock → {LocaleControllerMock}", localeControllerMock);
            this.localeControllerMock.Setup(l => l.GetLocales(Constants.CONTENT_ValidPortalId)).Returns(new Dictionary<string, Locale>
            {
                { "en-us", new Locale() },
            });

            FileWrapper.RegisterInstance(this.fileWrapper.Object);
            DirectoryWrapper.RegisterInstance(this.directoryWrapper.Object);
            FolderManager.RegisterInstance(this.folderManager.Object);
            FileManager.RegisterInstance(this.fileManager.Object);
            PathUtils.RegisterInstance(this.pathUtils.Object);
            PortalController.SetTestableInstance(this.portalControllerMock.Object);
            LocaleController.RegisterInstance(this.localeControllerMock.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.folderManager.Object);
                    services.AddSingleton(this.fileManager.Object);
                    services.AddSingleton(this.pathUtils.Object);
                    services.AddSingleton(this.portalControllerMock.Object);
                    services.AddSingleton(cryptographyProviderMock.Object);
                    services.AddSingleton(this.localeControllerMock.Object);
                });
				mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            MockComponentProvider.ResetContainer();
            TestableGlobals.ClearInstance();
            PortalController.ClearInstance();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void AddFile_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering AddFile_Throws_On_Null_Folder()");
            var stream = new Mock<Stream>();

			mockEventLogger.LogDebug("AddFile_Throws_On_Null_Folder(): stream → {Stream}", stream);
            Assert.Throws<ArgumentNullException>(() => this.sfp.AddFile(null, Constants.FOLDER_ValidFileName, stream.Object));
			mockEventLogger.LogDebug("<<< Exiting AddFile_Throws_On_Null_Folder()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
			mockEventLogger.LogDebug(">>> Entering AddFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
            var stream = new Mock<Stream>();

			mockEventLogger.LogDebug("AddFile_Throws_On_NullOrEmpty_FileName(fileName={FileName}): stream → {Stream}", fileName, stream);
            Assert.Throws<ArgumentException>(() => this.sfp.AddFile(this.folderInfo.Object, fileName, stream.Object));
			mockEventLogger.LogDebug("<<< Exiting AddFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void AddFile_Throws_On_Null_Content()
        {
			mockEventLogger.LogDebug(">>> Entering AddFile_Throws_On_Null_Content()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, null));
			mockEventLogger.LogDebug("<<< Exiting AddFile_Throws_On_Null_Content()");
        }

        [Test]
        public void DeleteFile_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering DeleteFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.DeleteFile(null));
			mockEventLogger.LogDebug("<<< Exiting DeleteFile_Throws_On_Null_File()");
        }

        // [Test]
        // public void DeleteFile_Calls_FileWrapper_Delete_When_File_Exists()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(true);

        // sfp.DeleteFile(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Once());
        // }

        // [Test]
        // public void DeleteFile_Does_Not_Call_FileWrapper_Delete_When_File_Does_Not_Exist()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

        // sfp.DeleteFile(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Never());
        // }
        [Test]
        public void ExistsFile_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.FileExists(null, Constants.FOLDER_ValidFileName));
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_Folder()");
        }

        [Test]
        public void ExistsFile_Throws_On_Null_FileName()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Throws_On_Null_FileName()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.FileExists(this.folderInfo.Object, null));
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_FileName()");
        }

        [Test]
        public void ExistsFile_Calls_FileWrapper_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Calls_FileWrapper_Exists()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.fileWrapper.Verify(fw => fw.Exists(Constants.FOLDER_ValidFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Calls_FileWrapper_Exists()");
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Returns_True_When_File_Exists()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(true);

            var result = this.sfp.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);
			mockEventLogger.LogDebug("ExistsFile_Returns_True_When_File_Exists(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Returns_True_When_File_Exists()");
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Returns_False_When_File_Does_Not_Exist()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

            var result = this.sfp.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);
			mockEventLogger.LogDebug("ExistsFile_Returns_False_When_File_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Returns_False_When_File_Does_Not_Exist()");
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderMapping()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Throws_On_Null_FolderMapping()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.FolderExists(Constants.FOLDER_ValidFolderPath, null));
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Throws_On_Null_FolderMapping()");
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Throws_On_Null_FolderPath()");
            var folderMapping = new FolderMappingInfo();

			mockEventLogger.LogDebug("ExistsFolder_Throws_On_Null_FolderPath(): folderMapping → {FolderMapping}", folderMapping);
            Assert.Throws<ArgumentNullException>(() => this.sfp.FolderExists(null, folderMapping));
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Throws_On_Null_FolderPath()");
        }

        [Test]
        public void ExistsFolder_Calls_DirectoryWrapper_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Calls_DirectoryWrapper_Exists()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            this.directoryWrapper.Verify(dw => dw.Exists(Constants.FOLDER_ValidFolderPath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Calls_DirectoryWrapper_Exists()");
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Returns_True_When_Folder_Exists()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            var result = this.sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);
			mockEventLogger.LogDebug("ExistsFolder_Returns_True_When_Folder_Exists(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Returns_True_When_Folder_Exists()");
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(false);

            var result = this.sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);
			mockEventLogger.LogDebug("ExistsFolder_Returns_False_When_Folder_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()");
        }

        [Test]
        public void GetFileAttributes_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileAttributes_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetFileAttributes(null));
			mockEventLogger.LogDebug("<<< Exiting GetFileAttributes_Throws_On_Null_File()");
        }

        // [Test]
        // public void GetFileAttributes_Calls_FileWrapper_GetAttributes()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // sfp.GetFileAttributes(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.GetAttributes(Constants.FOLDER_ValidFilePath), Times.Once());
        // }

        // [Test]
        // public void GetFileAttributes_Returns_File_Attributes_When_File_Exists()
        // {
        //    var expectedFileAttributes = FileAttributes.Normal;

        // fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidFilePath)).Returns(expectedFileAttributes);

        // var result = sfp.GetFileAttributes(fileInfo.Object);

        // Assert.AreEqual(expectedFileAttributes, result);
        // }
        [Test]
        public void GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()");
            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this.fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidFilePath)).Throws<FileNotFoundException>();

            var result = this.sfp.GetFileAttributes(this.fileInfo.Object);
			mockEventLogger.LogDebug("GetFileAttributes_Returns_Null_When_File_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()");
        }

        [Test]
        public void GetFiles_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetFiles(null));
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Throws_On_Null_Folder()");
        }

        [Test]
        public void GetFiles_Calls_DirectoryWrapper_GetFiles()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Calls_DirectoryWrapper_GetFiles()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.GetFiles(this.folderInfo.Object);

            this.directoryWrapper.Verify(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath));
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Calls_DirectoryWrapper_GetFiles()");
        }

        [Test]
        public void GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new[] { string.Empty, string.Empty, string.Empty };

            this.directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this.sfp.GetFiles(this.folderInfo.Object);
			mockEventLogger.LogDebug("GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count(): files → {Files}", files);

            Assert.That(files, Has.Length.EqualTo(filesReturned.Length));
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()");
        }

        [Test]
        public void GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new[] { "C:\\folder\\file1.txt", "C:\\folder\\file2.txt", "C:\\folder\\file3.txt" };
			mockEventLogger.LogDebug("GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files(): filesReturned → {FilesReturned}", filesReturned);
            var expectedValues = new[] { "file1.txt", "file2.txt", "file3.txt" };
			mockEventLogger.LogDebug("GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files(): expectedValues → {ExpectedValues}", expectedValues);

            this.directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this.sfp.GetFiles(this.folderInfo.Object);
			mockEventLogger.LogDebug("GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files(): files → {Files}", files);

            Assert.That(files, Is.EqualTo(expectedValues).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()");
        }

        [Test]
        public void GetFileStream_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetFileStream(null, Constants.FOLDER_ValidFileName));
			mockEventLogger.LogDebug("<<< Exiting GetFileStream_Throws_On_Null_Folder()");
        }

        [Test]
        public void GetFileStream_Throws_On_Null_FileName()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Throws_On_Null_FileName()");
            Assert.Throws<ArgumentException>(() => this.sfp.GetFileStream(this.folderInfo.Object, null));
			mockEventLogger.LogDebug("<<< Exiting GetFileStream_Throws_On_Null_FileName()");
        }

        [Test]
        public void GetFileStream_Calls_FileWrapper_OpenRead()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Calls_FileWrapper_OpenRead()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.GetFileStream(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.fileWrapper.Verify(fw => fw.OpenRead(Constants.FOLDER_ValidFilePath), Times.Once());
			mockEventLogger.LogInformation("GetFileStream_Calls_FileWrapper_OpenRead(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			mockEventLogger.LogDebug("<<< Exiting GetFileStream_Calls_FileWrapper_OpenRead()");
        }

        [Test]
        public void GetFileStream_Returns_Valid_Stream_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Returns_Valid_Stream_When_File_Exists()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var validFileBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var memoryStream = new MemoryStream(validFileBytes);
			mockEventLogger.LogDebug("GetFileStream_Returns_Valid_Stream_When_File_Exists(): memoryStream → {MemoryStream}", memoryStream);

            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidFilePath)).Returns(memoryStream);

            var result = this.sfp.GetFileStream(this.folderInfo.Object, Constants.FOLDER_ValidFileName);
			mockEventLogger.LogDebug("GetFileStream_Returns_Valid_Stream_When_File_Exists(): result → {Result}", result);

            byte[] resultBytes;
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = result.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                resultBytes = ms.ToArray();
            }

            Assert.That(resultBytes, Is.EqualTo(validFileBytes));
			mockEventLogger.LogInformation("GetFileStream_Returns_Valid_Stream_When_File_Exists(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			mockEventLogger.LogDebug("<<< Exiting GetFileStream_Returns_Valid_Stream_When_File_Exists()");
        }

        [Test]
        public void GetFileStream_Returns_Null_When_File_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Returns_Null_When_File_Does_Not_Exist()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidFilePath)).Throws<FileNotFoundException>();

            var result = this.sfp.GetFileStream(this.folderInfo.Object, Constants.FOLDER_ValidFileName);
			mockEventLogger.LogDebug("GetFileStream_Returns_Null_When_File_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.Null);
			mockEventLogger.LogInformation("GetFileStream_Returns_Null_When_File_Does_Not_Exist(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
			mockEventLogger.LogDebug("<<< Exiting GetFileStream_Returns_Null_When_File_Does_Not_Exist()");
        }

        [Test]
        public void GetImageUrl_Calls_IconControllerWrapper_IconURL()
        {
			mockEventLogger.LogDebug(">>> Entering GetImageUrl_Calls_IconControllerWrapper_IconURL()");
            var iconControllerWrapper = new Mock<IIconController>();
            IconControllerWrapper.RegisterInstance(iconControllerWrapper.Object);
			mockEventLogger.LogDebug("GetImageUrl_Calls_IconControllerWrapper_IconURL(): iconControllerWrapper → {IconControllerWrapper}", iconControllerWrapper);

            this.sfp.GetFolderProviderIconPath();

            iconControllerWrapper.Verify(icw => icw.IconURL("FolderStandard", "32x32"), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting GetImageUrl_Calls_IconControllerWrapper_IconURL()");
        }

        [Test]
        public void GetLastModificationTime_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering GetLastModificationTime_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetLastModificationTime(null));
			mockEventLogger.LogDebug("<<< Exiting GetLastModificationTime_Throws_On_Null_File()");
        }

        // [Test]
        // public void GetLastModificationTime_Calls_FileWrapper_GetLastWriteTime()
        // {
        //    fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // sfp.GetLastModificationTime(fileInfo.Object);

        // fileWrapper.Verify(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidFilePath), Times.Once());
        // }

        // [Test]
        // public void GetLastModificationTime_Returns_Valid_Date_When_File_Exists()
        // {
        //    var expectedDate = DateTime.Now;

        // fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidFilePath)).Returns(expectedDate);

        // var result = sfp.GetLastModificationTime(fileInfo.Object);

        // Assert.AreEqual(expectedDate, result);
        // }
        [Test]
        public void GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()");
            var expectedDate = Null.NullDate;

            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this.fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidFilePath)).Throws<FileNotFoundException>();

            var result = this.sfp.GetLastModificationTime(this.fileInfo.Object);
			mockEventLogger.LogDebug("GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.EqualTo(expectedDate));
			mockEventLogger.LogDebug("<<< Exiting GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()");
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderMapping()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Throws_On_Null_FolderMapping()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderPath, null).ToList());
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Throws_On_Null_FolderMapping()");
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderPath()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Throws_On_Null_FolderPath()");
            var folderMapping = new FolderMappingInfo();

			mockEventLogger.LogDebug("GetSubFolders_Throws_On_Null_FolderPath(): folderMapping → {FolderMapping}", folderMapping);
            Assert.Throws<ArgumentNullException>(() => this.sfp.GetSubFolders(null, folderMapping).ToList());
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Throws_On_Null_FolderPath()");
        }

        [Test]
        public void GetSubFolders_Calls_DirectoryWrapper_GetDirectories()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Calls_DirectoryWrapper_GetDirectories()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            this.directoryWrapper.Verify(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Calls_DirectoryWrapper_GetDirectories()");
        }

        [Test]
        public void GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this.directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();
			mockEventLogger.LogDebug("GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count(): result → {Result}", result);

            Assert.That(result, Has.Count.EqualTo(subFolders.Length));
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count()");
        }

        [Test]
        public void GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()");
            var expectedSubFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderRelativePath,
                Constants.FOLDER_OtherValidSubFolderRelativePath,
            };

            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this.pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this.pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this.directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this.sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();
			mockEventLogger.LogDebug("GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty(): result → {Result}", result);

            Assert.That(result, Is.EqualTo(expectedSubFolders).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()");
        }

        [Test]
        public void GetFileUrl_WhenCurrentPortalSettingsReturnsNull_DontThrow()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileUrl_WhenCurrentPortalSettingsReturnsNull_DontThrow()");
            // arrange
            var sfp = new Mock<StandardFolderProvider>
            {
                CallBase = true,
            };

            sfp.Setup(x => x.GetPortalSettings(Constants.CONTENT_ValidPortalId))
                .Returns(this.GetPortalSettingsMock());

            this.fileInfo.Setup(x => x.FileName)
                .Returns(Constants.FOLDER_ValidFileName);

            this.fileInfo.Setup(x => x.PortalId)
                .Returns(Constants.CONTENT_ValidPortalId);

            this.portalControllerMock.Setup(x => x.GetCurrentPortalSettings())
                .Returns<PortalSettings>(null);

            string fileUrl = null;
            Assert.DoesNotThrow((TestDelegate)(() => fileUrl = sfp.Object.GetFileUrl(this.fileInfo.Object)));
            Assert.That(fileUrl, Is.Not.Null);
			mockEventLogger.LogDebug("<<< Exiting GetFileUrl_WhenCurrentPortalSettingsReturnsNull_DontThrow()");
        }

        [Test]
        [TestCase("(")]
        [TestCase(")")]
        [TestCase("")]
        public void GetFileUrl_ReturnsStandardUrl_WhenFileUrlDoesNotContainInvalidCharactes(string fileNameChar)
        {
			mockEventLogger.LogDebug(">>> Entering GetFileUrl_ReturnsStandardUrl_WhenFileUrlDoesNotContainInvalidCharactes(fileNameChar={FileNameChar})", fileNameChar);
            // Arrange
            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            var portalSettingsMock = this.GetPortalSettingsMock();
			mockEventLogger.LogDebug("GetFileUrl_ReturnsStandardUrl_WhenFileUrlDoesNotContainInvalidCharactes(fileNameChar={FileNameChar}): portalSettingsMock → {PortalSettingsMock}", fileNameChar, portalSettingsMock);
            sfp.Setup(fp => fp.GetPortalSettings(Constants.CONTENT_ValidPortalId)).Returns(portalSettingsMock);
            this.fileInfo.Setup(f => f.FileName).Returns($"MyFileName {fileNameChar} Copy");
            this.fileInfo.Setup(f => f.PortalId).Returns(Constants.CONTENT_ValidPortalId);

            // Act
            var fileUrl = sfp.Object.GetFileUrl(this.fileInfo.Object);
			mockEventLogger.LogDebug("GetFileUrl_ReturnsStandardUrl_WhenFileUrlDoesNotContainInvalidCharactes(fileNameChar={FileNameChar}): fileUrl → {FileUrl}", fileNameChar, fileUrl);

            // Assert
            Assert.That(fileUrl.ToLowerInvariant().Contains("linkclick"), Is.False);
			mockEventLogger.LogDebug("<<< Exiting GetFileUrl_ReturnsStandardUrl_WhenFileUrlDoesNotContainInvalidCharactes(fileNameChar={FileNameChar})", fileNameChar);
        }

        [Test]
        [TestCase("?")]
        [TestCase("&")]
        [TestCase("+")]
        [TestCase(";")]
        [TestCase(":")]
        [TestCase("@")]
        [TestCase("=")]
        [TestCase("$")]
        [TestCase(",")]
        [TestCase("%")]
        public void GetFileUrl_ReturnsLinkclickUrl_WhenFileUrlContainsInvalidCharactes(string fileNameChar)
        {
			mockEventLogger.LogDebug(">>> Entering GetFileUrl_ReturnsLinkclickUrl_WhenFileUrlContainsInvalidCharactes(fileNameChar={FileNameChar})", fileNameChar);
            // Arrange
            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            var portalSettingsMock = this.GetPortalSettingsMock();
			mockEventLogger.LogDebug("GetFileUrl_ReturnsLinkclickUrl_WhenFileUrlContainsInvalidCharactes(fileNameChar={FileNameChar}): portalSettingsMock → {PortalSettingsMock}", fileNameChar, portalSettingsMock);
            sfp.Setup(fp => fp.GetPortalSettings(Constants.CONTENT_ValidPortalId)).Returns(portalSettingsMock);
            this.fileInfo.Setup(f => f.FileName).Returns($"MyFileName {fileNameChar} Copy");
            this.fileInfo.Setup(f => f.PortalId).Returns(Constants.CONTENT_ValidPortalId);

            // Act
            var fileUrl = sfp.Object.GetFileUrl(this.fileInfo.Object);
			mockEventLogger.LogDebug("GetFileUrl_ReturnsLinkclickUrl_WhenFileUrlContainsInvalidCharactes(fileNameChar={FileNameChar}): fileUrl → {FileUrl}", fileNameChar, fileUrl);

            // Assert
            Assert.That(fileUrl, Does.Contain("linkclick").IgnoreCase);
			mockEventLogger.LogDebug("<<< Exiting GetFileUrl_ReturnsLinkclickUrl_WhenFileUrlContainsInvalidCharactes(fileNameChar={FileNameChar})", fileNameChar);
        }

        [Test]
        public void IsInSync_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering IsInSync_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.IsInSync(null));
			mockEventLogger.LogDebug("<<< Exiting IsInSync_Throws_On_Null_File()");
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_In_Sync()
        {
			mockEventLogger.LogDebug(">>> Entering IsInSync_Returns_True_When_File_Is_In_Sync()");
            this.fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this.fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var result = sfp.Object.IsInSync(this.fileInfo.Object);
			mockEventLogger.LogDebug("IsInSync_Returns_True_When_File_Is_In_Sync(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting IsInSync_Returns_True_When_File_Is_In_Sync()");
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_Not_In_Sync()
        {
			mockEventLogger.LogDebug(">>> Entering IsInSync_Returns_True_When_File_Is_Not_In_Sync()");
            this.fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<StandardFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this.fileInfo.Object)).Returns(Constants.FOLDER_ModifiedFileHash);

            var result = sfp.Object.IsInSync(this.fileInfo.Object);
			mockEventLogger.LogDebug("IsInSync_Returns_True_When_File_Is_Not_In_Sync(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting IsInSync_Returns_True_When_File_Is_Not_In_Sync()");
        }

        [Test]
        public void RenameFile_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.RenameFile(null, Constants.FOLDER_ValidFileName));
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Throws_On_Null_File()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void RenameFile_Throws_On_NullOrEmpty_NewFileName(string newFileName)
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Throws_On_NullOrEmpty_NewFileName(newFileName={NewFileName})", newFileName);
            Assert.Throws<ArgumentException>(() => this.sfp.RenameFile(this.fileInfo.Object, newFileName));
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Throws_On_NullOrEmpty_NewFileName(newFileName={NewFileName})", newFileName);
        }

        [Test]
        public void RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()");
            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.sfp.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            this.fileWrapper.Verify(fw => fw.Move(Constants.FOLDER_ValidFilePath, Constants.FOLDER_OtherValidFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()");
        }

        [Test]
        public void RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()");
            this.fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            this.sfp.RenameFile(this.fileInfo.Object, Constants.FOLDER_ValidFileName);

            this.fileWrapper.Verify(fw => fw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()");
        }

        [Test]
        public void SetFileAttributes_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering SetFileAttributes_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.SetFileAttributes(null, FileAttributes.Archive));
			mockEventLogger.LogDebug("<<< Exiting SetFileAttributes_Throws_On_Null_File()");
        }

        // [Test]
        // public void SetFileAttributes_Calls_FileWrapper_SetAttributes()
        // {
        //    const FileAttributes validFileAttributes = FileAttributes.Archive;

        // fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

        // sfp.SetFileAttributes(fileInfo.Object, validFileAttributes);

        // fileWrapper.Verify(fw => fw.SetAttributes(Constants.FOLDER_ValidFilePath, validFileAttributes), Times.Once());
        // }
        [Test]
        public void SupportsFileAttributes_Returns_True()
        {
			mockEventLogger.LogDebug(">>> Entering SupportsFileAttributes_Returns_True()");
            var result = this.sfp.SupportsFileAttributes();

			mockEventLogger.LogDebug("SupportsFileAttributes_Returns_True(): result → {Result}", result);
            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting SupportsFileAttributes_Returns_True()");
        }

        [Test]
        public void UpdateFile_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Throws_On_Null_Folder()");
            var stream = new Mock<Stream>();

			mockEventLogger.LogDebug("UpdateFile_Throws_On_Null_Folder(): stream → {Stream}", stream);
            Assert.Throws<ArgumentNullException>(() => this.sfp.UpdateFile(null, Constants.FOLDER_ValidFileName, stream.Object));
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Throws_On_Null_Folder()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void UpdateFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
            var stream = new Mock<Stream>();

			mockEventLogger.LogDebug("UpdateFile_Throws_On_NullOrEmpty_FileName(fileName={FileName}): stream → {Stream}", fileName, stream);
            Assert.Throws<ArgumentException>(() => this.sfp.UpdateFile(this.folderInfo.Object, fileName, stream.Object));
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void UpdateFile_Throws_On_Null_Content()
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Throws_On_Null_Content()");
            Assert.Throws<ArgumentNullException>(() => this.sfp.UpdateFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, null));
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Throws_On_Null_Content()");
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(true);

            var stream = new Mock<Stream>();
			mockEventLogger.LogDebug("UpdateFile_Calls_FileWrapper_Delete_When_File_Exists(): stream → {Stream}", stream);

            this.sfp.UpdateFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this.fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()");
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Create()
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Calls_FileWrapper_Create()");
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this.fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

            var stream = new Mock<Stream>();
			mockEventLogger.LogDebug("UpdateFile_Calls_FileWrapper_Create(): stream → {Stream}", stream);

            this.sfp.UpdateFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this.fileWrapper.Verify(fw => fw.Create(Constants.FOLDER_ValidFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Calls_FileWrapper_Create()");
        }

        private Dictionary<string, string> GetPortalSettingsDictionaryMock()
        {
            var portalSettingsDictionary = new Dictionary<string, string>();
            portalSettingsDictionary.Add("AddCachebusterToResourceUris", true.ToString());

            return portalSettingsDictionary;
        }

        private PortalSettings GetPortalSettingsMock()
        {
			mockEventLogger.LogDebug(">>> Entering GetPortalSettingsMock()");
            var portalSettingsMock = new Mock<PortalSettings>();
            portalSettingsMock.Object.HomeDirectory = "/portals/" + Constants.CONTENT_ValidPortalId;
			mockEventLogger.LogDebug("GetPortalSettingsMock(): portalSettingsMock → {PortalSettingsMock}", portalSettingsMock);
            portalSettingsMock.Object.PortalId = Constants.CONTENT_ValidPortalId;
            portalSettingsMock.Object.EnableUrlLanguage = false;
            portalSettingsMock.Object.GUID = Guid.NewGuid();

			mockEventLogger.LogDebug("<<< Exiting GetPortalSettingsMock()");
            return portalSettingsMock.Object;
        }
    }
}
