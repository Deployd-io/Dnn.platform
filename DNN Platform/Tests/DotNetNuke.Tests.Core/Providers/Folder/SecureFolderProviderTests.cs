// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SecureFolderProviderTests
    {
        private SecureFolderProvider _sfp;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFileInfo> _fileInfo;
        private Mock<IFile> _fileWrapper;
        private Mock<IDirectory> _directoryWrapper;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFileManager> _fileManager;
        private Mock<IPathUtils> _pathUtils;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            this._sfp = new SecureFolderProvider();
            this._folderInfo = new Mock<IFolderInfo>();
			mockEventLogger.LogDebug("Setup(): _folderInfo → {_folderInfo}", _folderInfo);
			mockEventLogger.LogDebug("Setup(): _sfp → {_sfp}", _sfp);
            this._fileInfo = new Mock<IFileInfo>();
			mockEventLogger.LogDebug("Setup(): _fileInfo → {_fileInfo}", _fileInfo);
            this._fileWrapper = new Mock<IFile>();
			mockEventLogger.LogDebug("Setup(): _fileWrapper → {_fileWrapper}", _fileWrapper);
            this._directoryWrapper = new Mock<IDirectory>();
			mockEventLogger.LogDebug("Setup(): _directoryWrapper → {_directoryWrapper}", _directoryWrapper);
            this._folderManager = new Mock<IFolderManager>();
			mockEventLogger.LogDebug("Setup(): _folderManager → {_folderManager}", _folderManager);
            this._fileManager = new Mock<IFileManager>();
			mockEventLogger.LogDebug("Setup(): _fileManager → {_fileManager}", _fileManager);
            this._pathUtils = new Mock<IPathUtils>();
			mockEventLogger.LogDebug("Setup(): _pathUtils → {_pathUtils}", _pathUtils);

            FileWrapper.RegisterInstance(this._fileWrapper.Object);
            DirectoryWrapper.RegisterInstance(this._directoryWrapper.Object);
            FolderManager.RegisterInstance(this._folderManager.Object);
            FileManager.RegisterInstance(this._fileManager.Object);
            PathUtils.RegisterInstance(this._pathUtils.Object);
			mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            MockComponentProvider.ResetContainer();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void AddFile_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering AddFile_Throws_On_Null_Folder()");
            var stream = new Mock<Stream>();

			mockEventLogger.LogDebug("AddFile_Throws_On_Null_Folder(): stream → {Stream}", stream);
            Assert.Throws<ArgumentNullException>(() => this._sfp.AddFile(null, Constants.FOLDER_ValidFileName, stream.Object));
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
            Assert.Throws<ArgumentException>(() => this._sfp.AddFile(this._folderInfo.Object, fileName, stream.Object));
			mockEventLogger.LogDebug("<<< Exiting AddFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void AddFile_Throws_On_Null_Content()
        {
			mockEventLogger.LogDebug(">>> Entering AddFile_Throws_On_Null_Content()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.AddFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, null));
			mockEventLogger.LogDebug("<<< Exiting AddFile_Throws_On_Null_Content()");
        }

        [Test]
        public void DeleteFile_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering DeleteFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.DeleteFile(null));
			mockEventLogger.LogDebug("<<< Exiting DeleteFile_Throws_On_Null_File()");
        }

        [Test]
        public void DeleteFile_Calls_FileWrapper_Delete_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering DeleteFile_Calls_FileWrapper_Delete_When_File_Exists()");
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(true);

            this._sfp.DeleteFile(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidSecureFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting DeleteFile_Calls_FileWrapper_Delete_When_File_Exists()");
        }

        [Test]
        public void DeleteFile_Does_Not_Call_FileWrapper_Delete_When_File_Does_Not_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering DeleteFile_Does_Not_Call_FileWrapper_Delete_When_File_Does_Not_Exists()");
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidFilePath)).Returns(false);

            this._sfp.DeleteFile(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidFilePath), Times.Never());
			mockEventLogger.LogDebug("<<< Exiting DeleteFile_Does_Not_Call_FileWrapper_Delete_When_File_Does_Not_Exists()");
        }

        [Test]
        public void ExistsFile_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.FileExists(null, Constants.FOLDER_ValidFileName));
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_Folder()");
        }

        [Test]
        public void ExistsFile_Throws_On_Null_FileName()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Throws_On_Null_FileName()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.FileExists(this._folderInfo.Object, null));
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_FileName()");
        }

        [Test]
        public void ExistsFile_Calls_FileWrapper_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Calls_FileWrapper_Exists()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileWrapper.Verify(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Calls_FileWrapper_Exists()");
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Returns_True_When_File_Exists()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(true);

            var result = this._sfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);
			mockEventLogger.LogDebug("ExistsFile_Returns_True_When_File_Exists(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Returns_True_When_File_Exists()");
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFile_Returns_False_When_File_Does_Not_Exist()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(false);

            var result = this._sfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);
			mockEventLogger.LogDebug("ExistsFile_Returns_False_When_File_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting ExistsFile_Returns_False_When_File_Does_Not_Exist()");
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderMapping()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Throws_On_Null_FolderMapping()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.FolderExists(Constants.FOLDER_ValidFolderPath, null));
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Throws_On_Null_FolderMapping()");
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Throws_On_Null_FolderPath()");
            var folderMapping = new FolderMappingInfo();

			mockEventLogger.LogDebug("ExistsFolder_Throws_On_Null_FolderPath(): folderMapping → {FolderMapping}", folderMapping);
            Assert.Throws<ArgumentNullException>(() => this._sfp.FolderExists(null, folderMapping));
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Throws_On_Null_FolderPath()");
        }

        [Test]
        public void ExistsFolder_Calls_DirectoryWrapper_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Calls_DirectoryWrapper_Exists()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

            this._directoryWrapper.Verify(dw => dw.Exists(Constants.FOLDER_ValidFolderPath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Calls_DirectoryWrapper_Exists()");
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Returns_True_When_Folder_Exists()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(true);

            var result = this._sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);
			mockEventLogger.LogDebug("ExistsFolder_Returns_True_When_Folder_Exists(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Returns_True_When_Folder_Exists()");
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._directoryWrapper.Setup(dw => dw.Exists(Constants.FOLDER_ValidFolderPath)).Returns(false);

            var result = this._sfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);
			mockEventLogger.LogDebug("ExistsFolder_Returns_False_When_Folder_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()");
        }

        [Test]
        public void GetFileAttributes_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileAttributes_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.GetFileAttributes(null));
			mockEventLogger.LogDebug("<<< Exiting GetFileAttributes_Throws_On_Null_File()");
        }

        [Test]
        public void GetFileAttributes_Calls_FileWrapper_GetAttributes()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileAttributes_Calls_FileWrapper_GetAttributes()");
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._sfp.GetFileAttributes(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.GetAttributes(Constants.FOLDER_ValidSecureFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting GetFileAttributes_Calls_FileWrapper_GetAttributes()");
        }

        [Test]
        public void GetFileAttributes_Returns_File_Attributes_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileAttributes_Returns_File_Attributes_When_File_Exists()");
            var expectedFileAttributes = FileAttributes.Normal;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidSecureFilePath)).Returns(expectedFileAttributes);

            var result = this._sfp.GetFileAttributes(this._fileInfo.Object);
			mockEventLogger.LogDebug("GetFileAttributes_Returns_File_Attributes_When_File_Exists(): result → {Result}", result);

            Assert.That(result, Is.EqualTo(expectedFileAttributes));
			mockEventLogger.LogDebug("<<< Exiting GetFileAttributes_Returns_File_Attributes_When_File_Exists()");
        }

        [Test]
        public void GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()");
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetAttributes(Constants.FOLDER_ValidSecureFilePath)).Throws<FileNotFoundException>();

            var result = this._sfp.GetFileAttributes(this._fileInfo.Object);
			mockEventLogger.LogDebug("GetFileAttributes_Returns_Null_When_File_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.Null);
			mockEventLogger.LogDebug("<<< Exiting GetFileAttributes_Returns_Null_When_File_Does_Not_Exist()");
        }

        [Test]
        public void GetFiles_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.GetFiles(null));
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Throws_On_Null_Folder()");
        }

        [Test]
        public void GetFiles_Calls_DirectoryWrapper_GetFiles()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Calls_DirectoryWrapper_GetFiles()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.GetFiles(this._folderInfo.Object);

            this._directoryWrapper.Verify(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath));
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Calls_DirectoryWrapper_GetFiles()");
        }

        [Test]
        public void GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new string[] { "C:\\folder\\file1.txt.resources", "C:\\folder\\file2.txt.resources", "C:\\folder\\file3.txt.resources" };
			mockEventLogger.LogDebug("GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count(): filesReturned → {FilesReturned}", filesReturned);

            this._directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this._sfp.GetFiles(this._folderInfo.Object);
			mockEventLogger.LogDebug("GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count(): files → {Files}", files);

            Assert.That(files, Has.Length.EqualTo(filesReturned.Length));
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Count_Equals_DirectoryWrapper_GetFiles_Count()");
        }

        [Test]
        public void GetFiles_Return_Valid_FileNames_When_Folder_Contains_Files()
        {
			mockEventLogger.LogDebug(">>> Entering GetFiles_Return_Valid_FileNames_When_Folder_Contains_Files()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            var filesReturned = new string[] { "C:\\folder\\file1.txt.resources", "C:\\folder\\file2.txt.resources", "C:\\folder\\file3.txt.resources" };
			mockEventLogger.LogDebug("GetFiles_Return_Valid_FileNames_When_Folder_Contains_Files(): filesReturned → {FilesReturned}", filesReturned);
            var expectedValues = new string[] { "file1.txt", "file2.txt", "file3.txt" };
			mockEventLogger.LogDebug("GetFiles_Return_Valid_FileNames_When_Folder_Contains_Files(): expectedValues → {ExpectedValues}", expectedValues);

            this._directoryWrapper.Setup(dw => dw.GetFiles(Constants.FOLDER_ValidFolderPath)).Returns(filesReturned);

            var files = this._sfp.GetFiles(this._folderInfo.Object);
			mockEventLogger.LogDebug("GetFiles_Return_Valid_FileNames_When_Folder_Contains_Files(): files → {Files}", files);

            Assert.That(files, Is.EqualTo(expectedValues).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting GetFiles_Return_Valid_FileNames_When_Folder_Contains_Files()");
        }

        [Test]
        public void GetFileStream_Throws_On_Null_Folder()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.GetFileStream(null, Constants.FOLDER_ValidFileName));
			mockEventLogger.LogDebug("<<< Exiting GetFileStream_Throws_On_Null_Folder()");
        }

        [Test]
        public void GetFileStream_Throws_On_Null_FileName()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Throws_On_Null_FileName()");
            Assert.Throws<ArgumentException>(() => this._sfp.GetFileStream(this._folderInfo.Object, null));
			mockEventLogger.LogDebug("<<< Exiting GetFileStream_Throws_On_Null_FileName()");
        }

        [Test]
        public void GetFileStream_Calls_FileWrapper_OpenRead()
        {
			mockEventLogger.LogDebug(">>> Entering GetFileStream_Calls_FileWrapper_OpenRead()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileWrapper.Verify(fw => fw.OpenRead(Constants.FOLDER_ValidSecureFilePath), Times.Once());
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

            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidSecureFilePath)).Returns(memoryStream);

            var result = this._sfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);
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
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.OpenRead(Constants.FOLDER_ValidSecureFilePath)).Throws<FileNotFoundException>();

            var result = this._sfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);
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

            this._sfp.GetFolderProviderIconPath();

            iconControllerWrapper.Verify(icw => icw.IconURL("FolderSecure", "32x32"), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting GetImageUrl_Calls_IconControllerWrapper_IconURL()");
        }

        [Test]
        public void GetLastModificationTime_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering GetLastModificationTime_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.GetLastModificationTime(null));
			mockEventLogger.LogDebug("<<< Exiting GetLastModificationTime_Throws_On_Null_File()");
        }

        [Test]
        public void GetLastModificationTime_Calls_FileWrapper_GetLastWriteTime()
        {
			mockEventLogger.LogDebug(">>> Entering GetLastModificationTime_Calls_FileWrapper_GetLastWriteTime()");
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._sfp.GetLastModificationTime(this._fileInfo.Object);

            this._fileWrapper.Verify(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidSecureFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting GetLastModificationTime_Calls_FileWrapper_GetLastWriteTime()");
        }

        [Test]
        public void GetLastModificationTime_Returns_Valid_Date_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering GetLastModificationTime_Returns_Valid_Date_When_File_Exists()");
            var expectedDate = DateTime.Now;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidSecureFilePath)).Returns(expectedDate);

            var result = this._sfp.GetLastModificationTime(this._fileInfo.Object);
			mockEventLogger.LogDebug("GetLastModificationTime_Returns_Valid_Date_When_File_Exists(): result → {Result}", result);

            Assert.That(result, Is.EqualTo(expectedDate));
			mockEventLogger.LogDebug("<<< Exiting GetLastModificationTime_Returns_Valid_Date_When_File_Exists()");
        }

        [Test]
        public void GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()
        {
			mockEventLogger.LogDebug(">>> Entering GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()");
            var expectedDate = Null.NullDate;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._fileWrapper.Setup(fw => fw.GetLastWriteTime(Constants.FOLDER_ValidSecureFilePath)).Throws<FileNotFoundException>();

            var result = this._sfp.GetLastModificationTime(this._fileInfo.Object);
			mockEventLogger.LogDebug("GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist(): result → {Result}", result);

            Assert.That(result, Is.EqualTo(expectedDate));
			mockEventLogger.LogDebug("<<< Exiting GetLastModificationTime_Returns_Null_Date_When_File_Does_Not_Exist()");
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderMapping()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Throws_On_Null_FolderMapping()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderPath, null).ToList());
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Throws_On_Null_FolderMapping()");
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderPath()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Throws_On_Null_FolderPath()");
            var folderMapping = new FolderMappingInfo();

			mockEventLogger.LogDebug("GetSubFolders_Throws_On_Null_FolderPath(): folderMapping → {FolderMapping}", folderMapping);
            Assert.Throws<ArgumentNullException>(() => this._sfp.GetSubFolders(null, folderMapping).ToList());
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Throws_On_Null_FolderPath()");
        }

        [Test]
        public void GetSubFolders_Calls_DirectoryWrapper_GetFiles()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Calls_DirectoryWrapper_GetFiles()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(folderMapping.PortalID, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            this._directoryWrapper.Verify(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Calls_DirectoryWrapper_GetFiles()");
        }

        [Test]
        public void GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count()
        {
			mockEventLogger.LogDebug(">>> Entering GetSubFolders_Count_Equals_DirectoryWrapper_GetDirectories_Count()");
            var folderMapping = new FolderMappingInfo { PortalID = Constants.CONTENT_ValidPortalId };

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this._directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();
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

            this._pathUtils.Setup(pu => pu.GetPhysicalPath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(Constants.FOLDER_ValidFolderPath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidSubFolderPath)).Returns(Constants.FOLDER_ValidSubFolderRelativePath);
            this._pathUtils.Setup(pu => pu.GetRelativePath(Constants.CONTENT_ValidPortalId, Constants.FOLDER_OtherValidSubFolderPath)).Returns(Constants.FOLDER_OtherValidSubFolderRelativePath);

            var subFolders = new[]
            {
                Constants.FOLDER_ValidSubFolderPath,
                Constants.FOLDER_OtherValidSubFolderPath,
            };

            this._directoryWrapper.Setup(dw => dw.GetDirectories(Constants.FOLDER_ValidFolderPath)).Returns(subFolders);

            var result = this._sfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();
			mockEventLogger.LogDebug("GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty(): result → {Result}", result);

            Assert.That(result, Is.EqualTo(expectedSubFolders).AsCollection);
			mockEventLogger.LogDebug("<<< Exiting GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()");
        }

        [Test]
        public void IsInSync_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering IsInSync_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.IsInSync(null));
			mockEventLogger.LogDebug("<<< Exiting IsInSync_Throws_On_Null_File()");
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_In_Sync()
        {
			mockEventLogger.LogDebug(">>> Entering IsInSync_Returns_True_When_File_Is_In_Sync()");
            this._fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<SecureFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this._fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var result = sfp.Object.IsInSync(this._fileInfo.Object);
			mockEventLogger.LogDebug("IsInSync_Returns_True_When_File_Is_In_Sync(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting IsInSync_Returns_True_When_File_Is_In_Sync()");
        }

        [Test]
        public void IsInSync_Returns_True_When_File_Is_Not_In_Sync()
        {
			mockEventLogger.LogDebug(">>> Entering IsInSync_Returns_True_When_File_Is_Not_In_Sync()");
            this._fileInfo.Setup(fi => fi.SHA1Hash).Returns(Constants.FOLDER_UnmodifiedFileHash);

            var sfp = new Mock<SecureFolderProvider> { CallBase = true };
            sfp.Setup(fp => fp.GetHash(this._fileInfo.Object)).Returns(Constants.FOLDER_ModifiedFileHash);

            var result = sfp.Object.IsInSync(this._fileInfo.Object);
			mockEventLogger.LogDebug("IsInSync_Returns_True_When_File_Is_Not_In_Sync(): result → {Result}", result);

            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting IsInSync_Returns_True_When_File_Is_Not_In_Sync()");
        }

        [Test]
        public void RenameFile_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.RenameFile(null, Constants.FOLDER_ValidFileName));
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Throws_On_Null_File()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void RenameFile_Throws_On_NullOrEmpty_NewFileName(string newFileName)
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Throws_On_NullOrEmpty_NewFileName(newFileName={NewFileName})", newFileName);
            Assert.Throws<ArgumentException>(() => this._sfp.RenameFile(this._fileInfo.Object, newFileName));
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Throws_On_NullOrEmpty_NewFileName(newFileName={NewFileName})", newFileName);
        }

        [Test]
        public void RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()");
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this._fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this._folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this._folderInfo.Object);
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._sfp.RenameFile(this._fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            this._fileWrapper.Verify(fw => fw.Move(Constants.FOLDER_ValidSecureFilePath, Constants.FOLDER_OtherValidSecureFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Calls_FileWrapper_Move_When_FileNames_Are_Not_Equal()");
        }

        [Test]
        public void RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()
        {
			mockEventLogger.LogDebug(">>> Entering RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()");
            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);
            this._fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            this._sfp.RenameFile(this._fileInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileWrapper.Verify(fw => fw.Move(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			mockEventLogger.LogDebug("<<< Exiting RenameFile_Does_Not_Call_FileWrapper_Move_When_FileNames_Are_Equal()");
        }

        [Test]
        public void SetFileAttributes_Throws_On_Null_File()
        {
			mockEventLogger.LogDebug(">>> Entering SetFileAttributes_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.SetFileAttributes(null, FileAttributes.Archive));
			mockEventLogger.LogDebug("<<< Exiting SetFileAttributes_Throws_On_Null_File()");
        }

        [Test]
        public void SetFileAttributes_Calls_FileWrapper_SetAttributes()
        {
			mockEventLogger.LogDebug(">>> Entering SetFileAttributes_Calls_FileWrapper_SetAttributes()");
            const FileAttributes validFileAttributes = FileAttributes.Archive;

            this._fileInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFilePath);

            this._sfp.SetFileAttributes(this._fileInfo.Object, validFileAttributes);

            this._fileWrapper.Verify(fw => fw.SetAttributes(Constants.FOLDER_ValidSecureFilePath, validFileAttributes), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting SetFileAttributes_Calls_FileWrapper_SetAttributes()");
        }

        [Test]
        public void SupportsFileAttributes_Returns_True()
        {
			mockEventLogger.LogDebug(">>> Entering SupportsFileAttributes_Returns_True()");
            var result = this._sfp.SupportsFileAttributes();

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
            Assert.Throws<ArgumentNullException>(() => this._sfp.UpdateFile(null, Constants.FOLDER_ValidFileName, stream.Object));
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
            Assert.Throws<ArgumentException>(() => this._sfp.UpdateFile(this._folderInfo.Object, fileName, stream.Object));
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void UpdateFile_Throws_On_Null_Content()
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Throws_On_Null_Content()");
            Assert.Throws<ArgumentNullException>(() => this._sfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, null));
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Throws_On_Null_Content()");
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(true);

            var stream = new Mock<Stream>();
			mockEventLogger.LogDebug("UpdateFile_Calls_FileWrapper_Delete_When_File_Exists(): stream → {Stream}", stream);

            this._sfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this._fileWrapper.Verify(fw => fw.Delete(Constants.FOLDER_ValidSecureFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Calls_FileWrapper_Delete_When_File_Exists()");
        }

        [Test]
        public void UpdateFile_Calls_FileWrapper_Create()
        {
			mockEventLogger.LogDebug(">>> Entering UpdateFile_Calls_FileWrapper_Create()");
            this._folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);

            this._fileWrapper.Setup(fw => fw.Exists(Constants.FOLDER_ValidSecureFilePath)).Returns(false);

            var stream = new Mock<Stream>();
			mockEventLogger.LogDebug("UpdateFile_Calls_FileWrapper_Create(): stream → {Stream}", stream);

            this._sfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, stream.Object);

            this._fileWrapper.Verify(fw => fw.Create(Constants.FOLDER_ValidSecureFilePath), Times.Once());
			mockEventLogger.LogDebug("<<< Exiting UpdateFile_Calls_FileWrapper_Create()");
        }
    }
}
