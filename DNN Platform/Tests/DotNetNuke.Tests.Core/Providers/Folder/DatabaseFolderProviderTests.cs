// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    [TestFixture]
    public class DatabaseFolderProviderTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private DatabaseFolderProvider _dfp;
        private Mock<DataProvider> _mockData;
        private Mock<IFolderInfo> _folderInfo;
        private Mock<IFileInfo> _fileInfo;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFileManager> _fileManager;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            this._dfp = new DatabaseFolderProvider();
            this._mockData = MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("Setup(): _mockData → {_mockData}", _mockData);
			Logger.LogDebug("Setup(): _dfp → {_dfp}", _dfp);
            this._folderInfo = new Mock<IFolderInfo>();
			Logger.LogDebug("Setup(): _folderInfo → {_folderInfo}", _folderInfo);
            this._fileInfo = new Mock<IFileInfo>();
			Logger.LogDebug("Setup(): _fileInfo → {_fileInfo}", _fileInfo);
            this._folderManager = new Mock<IFolderManager>();
			Logger.LogDebug("Setup(): _folderManager → {_folderManager}", _folderManager);
            this._fileManager = new Mock<IFileManager>();
			Logger.LogDebug("Setup(): _fileManager → {_fileManager}", _fileManager);

            FolderManager.RegisterInstance(this._folderManager.Object);
            FileManager.RegisterInstance(this._fileManager.Object);
			Logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            MockComponentProvider.ResetContainer();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void AddFile_Throws_On_Null_Folder()
        {
			Logger.LogDebug(">>> Entering AddFile_Throws_On_Null_Folder()");
            var stream = new Mock<Stream>();

			Logger.LogDebug("AddFile_Throws_On_Null_Folder(): stream → {Stream}", stream);
            Assert.Throws<ArgumentNullException>(() => this._dfp.AddFile(null, Constants.FOLDER_ValidFileName, stream.Object));
			Logger.LogDebug("<<< Exiting AddFile_Throws_On_Null_Folder()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
			Logger.LogDebug(">>> Entering AddFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
            var stream = new Mock<Stream>();

			Logger.LogDebug("AddFile_Throws_On_NullOrEmpty_FileName(fileName={FileName}): stream → {Stream}", fileName, stream);
            Assert.Throws<ArgumentException>(() => this._dfp.AddFile(this._folderInfo.Object, fileName, stream.Object));
			Logger.LogDebug("<<< Exiting AddFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void DeleteFile_Throws_On_Null_File()
        {
			Logger.LogDebug(">>> Entering DeleteFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this._dfp.DeleteFile(null));
			Logger.LogDebug("<<< Exiting DeleteFile_Throws_On_Null_File()");
        }

        [Test]
        public void DeleteFile_Calls_DataProvider_ClearFileContent()
        {
			Logger.LogDebug(">>> Entering DeleteFile_Calls_DataProvider_ClearFileContent()");
            this._fileInfo.Setup(fi => fi.FileId).Returns(Constants.FOLDER_ValidFileId);

            this._dfp.DeleteFile(this._fileInfo.Object);

            this._mockData.Verify(md => md.ClearFileContent(Constants.FOLDER_ValidFileId), Times.Once());
			Logger.LogDebug("<<< Exiting DeleteFile_Calls_DataProvider_ClearFileContent()");
        }

        [Test]
        public void ExistsFile_Throws_On_Null_Folder()
        {
			Logger.LogDebug(">>> Entering ExistsFile_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this._dfp.FileExists(null, Constants.FOLDER_ValidFileName));
			Logger.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_Folder()");
        }

        [Test]
        public void ExistsFile_Throws_On_Null_FileName()
        {
			Logger.LogDebug(">>> Entering ExistsFile_Throws_On_Null_FileName()");
            Assert.Throws<ArgumentNullException>(() => this._dfp.FileExists(this._folderInfo.Object, null));
			Logger.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_FileName()");
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
			Logger.LogDebug(">>> Entering ExistsFile_Returns_True_When_File_Exists()");
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true)).Returns(new FileInfo());

            var result = this._dfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

			Logger.LogDebug("ExistsFile_Returns_True_When_File_Exists(): result → {Result}", result);
            Assert.That(result, Is.True);
			Logger.LogDebug("<<< Exiting ExistsFile_Returns_True_When_File_Exists()");
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
			Logger.LogDebug(">>> Entering ExistsFile_Returns_False_When_File_Does_Not_Exist()");
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            FileInfo file = null;
            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(file);

            var result = this._dfp.FileExists(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

			Logger.LogDebug("ExistsFile_Returns_False_When_File_Does_Not_Exist(): result → {Result}", result);
            Assert.That(result, Is.False);
			Logger.LogDebug("<<< Exiting ExistsFile_Returns_False_When_File_Does_Not_Exist()");
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderMapping()
        {
			Logger.LogDebug(">>> Entering ExistsFolder_Throws_On_Null_FolderMapping()");
            Assert.Throws<ArgumentNullException>(() => this._dfp.FolderExists(Constants.FOLDER_ValidFolderPath, null));
			Logger.LogDebug("<<< Exiting ExistsFolder_Throws_On_Null_FolderMapping()");
        }

        [Test]
        public void ExistsFolder_Throws_On_Null_FolderPath()
        {
			Logger.LogDebug(">>> Entering ExistsFolder_Throws_On_Null_FolderPath()");
            var folderMapping = new FolderMappingInfo();

			Logger.LogDebug("ExistsFolder_Throws_On_Null_FolderPath(): folderMapping → {FolderMapping}", folderMapping);
            Assert.Throws<ArgumentNullException>(() => this._dfp.FolderExists(null, folderMapping));
			Logger.LogDebug("<<< Exiting ExistsFolder_Throws_On_Null_FolderPath()");
        }

        [Test]
        public void ExistsFolder_Returns_True_When_Folder_Exists()
        {
			Logger.LogDebug(">>> Entering ExistsFolder_Returns_True_When_Folder_Exists()");
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;
			Logger.LogDebug("ExistsFolder_Returns_True_When_Folder_Exists(): folderMapping → {FolderMapping}", folderMapping);
			Logger.LogDebug("ExistsFolder_Returns_True_When_Folder_Exists(): folderMapping.PortalID → {Arg0}", folderMapping.PortalID);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(new FolderInfo());

            var result = this._dfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

			Logger.LogDebug("ExistsFolder_Returns_True_When_Folder_Exists(): result → {Result}", result);
            Assert.That(result, Is.True);
			Logger.LogDebug("<<< Exiting ExistsFolder_Returns_True_When_Folder_Exists()");
        }

        [Test]
        public void ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()
        {
			Logger.LogDebug(">>> Entering ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()");
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;
			Logger.LogDebug("ExistsFolder_Returns_False_When_Folder_Does_Not_Exist(): folderMapping → {FolderMapping}", folderMapping);
			Logger.LogDebug("ExistsFolder_Returns_False_When_Folder_Does_Not_Exist(): folderMapping.PortalID → {Arg0}", folderMapping.PortalID);

            FolderInfo folder = null;
            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(folder);

            var result = this._dfp.FolderExists(Constants.FOLDER_ValidFolderRelativePath, folderMapping);

			Logger.LogDebug("ExistsFolder_Returns_False_When_Folder_Does_Not_Exist(): result → {Result}", result);
            Assert.That(result, Is.False);
			Logger.LogDebug("<<< Exiting ExistsFolder_Returns_False_When_Folder_Does_Not_Exist()");
        }

        [Test]
        public void GetFileAttributes_Returns_Null()
        {
			Logger.LogDebug(">>> Entering GetFileAttributes_Returns_Null()");
            var result = this._dfp.GetFileAttributes(It.IsAny<IFileInfo>());

			Logger.LogDebug("GetFileAttributes_Returns_Null(): result → {Result}", result);
            Assert.That(result, Is.Null);
			Logger.LogDebug("<<< Exiting GetFileAttributes_Returns_Null()");
        }

        [Test]
        public void GetFiles_Throws_On_Null_Folder()
        {
			Logger.LogDebug(">>> Entering GetFiles_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this._dfp.GetFiles(null));
			Logger.LogDebug("<<< Exiting GetFiles_Throws_On_Null_Folder()");
        }

        [Test]
        public void GetFiles_Calls_FolderManager_GetFilesByFolder()
        {
			Logger.LogDebug(">>> Entering GetFiles_Calls_FolderManager_GetFilesByFolder()");
            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
            };

            this._folderManager.Setup(fm => fm.GetFiles(this._folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            this._dfp.GetFiles(this._folderInfo.Object);

            this._folderManager.Verify(fm => fm.GetFiles(this._folderInfo.Object), Times.Once());
			Logger.LogDebug("<<< Exiting GetFiles_Calls_FolderManager_GetFilesByFolder()");
        }

        [Test]
        public void GetFiles_Count_Equals_DataProvider_GetFiles_Count()
        {
			Logger.LogDebug(">>> Entering GetFiles_Count_Equals_DataProvider_GetFiles_Count()");
            var expectedFiles = new string[] { string.Empty, string.Empty, string.Empty };

            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
                new FileInfo { FileName = string.Empty },
            };

            this._folderManager.Setup(fm => fm.GetFiles(this._folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            var files = this._dfp.GetFiles(this._folderInfo.Object);

			Logger.LogDebug("GetFiles_Count_Equals_DataProvider_GetFiles_Count(): files → {Files}", files);
            Assert.That(files, Has.Length.EqualTo(expectedFiles.Length));
			Logger.LogDebug("<<< Exiting GetFiles_Count_Equals_DataProvider_GetFiles_Count()");
        }

        [Test]
        public void GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()
        {
			Logger.LogDebug(">>> Entering GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()");
            var expectedFiles = new string[] { "file1.txt", "file2.txt", "file3.txt" };

			Logger.LogDebug("GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files(): expectedFiles → {ExpectedFiles}", expectedFiles);
            var fileInfos = new List<IFileInfo>
            {
                new FileInfo { FileName = "file1.txt" },
                new FileInfo { FileName = "file2.txt" },
                new FileInfo { FileName = "file3.txt" },
            };

            this._folderManager.Setup(fm => fm.GetFiles(this._folderInfo.Object)).Returns((IList<IFileInfo>)fileInfos);

            var files = this._dfp.GetFiles(this._folderInfo.Object);

			Logger.LogDebug("GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files(): files → {Files}", files);
            Assert.That(files, Is.EqualTo(expectedFiles).AsCollection);
			Logger.LogDebug("<<< Exiting GetFiles_Returns_Valid_FileNames_When_Folder_Contains_Files()");
        }

        [Test]
        public void GetFileStream_Throws_On_Null_Folder()
        {
			Logger.LogDebug(">>> Entering GetFileStream_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this._dfp.GetFileStream(null, Constants.FOLDER_ValidFileName));
			Logger.LogDebug("<<< Exiting GetFileStream_Throws_On_Null_Folder()");
        }

        [Test]
        public void GetFileStream_Throws_On_NullOrEmpty_FileName()
        {
			Logger.LogDebug(">>> Entering GetFileStream_Throws_On_NullOrEmpty_FileName()");
            Assert.Throws<ArgumentException>(() => this._dfp.GetFileStream(this._folderInfo.Object, null));
			Logger.LogDebug("<<< Exiting GetFileStream_Throws_On_NullOrEmpty_FileName()");
        }

        [Test]
        public void GetFileStream_Calls_FileManager_GetFile()
        {
			Logger.LogDebug(">>> Entering GetFileStream_Calls_FileManager_GetFile()");
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns((IFileInfo)null);

            this._dfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

            this._fileManager.Verify(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true), Times.Once());
			Logger.LogDebug("<<< Exiting GetFileStream_Calls_FileManager_GetFile()");
        }

        [Test]
        public void GetFileStream_Returns_Valid_Stream_When_File_Exists()
        {
			Logger.LogDebug(">>> Entering GetFileStream_Returns_Valid_Stream_When_File_Exists()");
            var validFileBytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            var _filesTable = new DataTable("Files");
            _filesTable.Columns.Add("Content", typeof(byte[]));
			Logger.LogDebug("GetFileStream_Returns_Valid_Stream_When_File_Exists(): _filesTable → {_filesTable}", _filesTable);

            _filesTable.Rows.Add(validFileBytes);

            this._mockData.Setup(md => md.GetFileContent(Constants.FOLDER_ValidFileId)).Returns(_filesTable.CreateDataReader());

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true))
                .Returns(new FileInfo { FileId = Constants.FOLDER_ValidFileId, PortalId = Constants.CONTENT_ValidPortalId });

            var result = this._dfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

			Logger.LogDebug("GetFileStream_Returns_Valid_Stream_When_File_Exists(): result → {Result}", result);
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
			Logger.LogDebug("<<< Exiting GetFileStream_Returns_Valid_Stream_When_File_Exists()");
        }

        [Test]
        public void GetFileStream_Returns_Null_When_File_Does_Not_Exist()
        {
			Logger.LogDebug(">>> Entering GetFileStream_Returns_Null_When_File_Does_Not_Exist()");
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>()))
                .Returns((IFileInfo)null);

            var result = this._dfp.GetFileStream(this._folderInfo.Object, Constants.FOLDER_ValidFileName);

			Logger.LogDebug("GetFileStream_Returns_Null_When_File_Does_Not_Exist(): result → {Result}", result);
            Assert.That(result, Is.Null);
			Logger.LogDebug("<<< Exiting GetFileStream_Returns_Null_When_File_Does_Not_Exist()");
        }

        [Test]
        public void GetImageUrl_Calls_IconControllerWrapper_IconURL()
        {
			Logger.LogDebug(">>> Entering GetImageUrl_Calls_IconControllerWrapper_IconURL()");
            var iconControllerWrapper = new Mock<IIconController>();
            IconControllerWrapper.RegisterInstance(iconControllerWrapper.Object);
			Logger.LogDebug("GetImageUrl_Calls_IconControllerWrapper_IconURL(): iconControllerWrapper → {IconControllerWrapper}", iconControllerWrapper);

            this._dfp.GetFolderProviderIconPath();

            iconControllerWrapper.Verify(icw => icw.IconURL("FolderDatabase", "32x32"), Times.Once());
			Logger.LogDebug("<<< Exiting GetImageUrl_Calls_IconControllerWrapper_IconURL()");
        }

        [Test]
        public void GetLastModificationTime_Returns_Null_Date()
        {
			Logger.LogDebug(">>> Entering GetLastModificationTime_Returns_Null_Date()");
            var expectedResult = Null.NullDate;

            var result = this._dfp.GetLastModificationTime(this._fileInfo.Object);

			Logger.LogDebug("GetLastModificationTime_Returns_Null_Date(): result → {Result}", result);
            Assert.That(result, Is.EqualTo(expectedResult));
			Logger.LogDebug("<<< Exiting GetLastModificationTime_Returns_Null_Date()");
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderMapping()
        {
			Logger.LogDebug(">>> Entering GetSubFolders_Throws_On_Null_FolderMapping()");
            Assert.Throws<ArgumentNullException>(() => this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderPath, null).ToList());
			Logger.LogDebug("<<< Exiting GetSubFolders_Throws_On_Null_FolderMapping()");
        }

        [Test]
        public void GetSubFolders_Throws_On_Null_FolderPath()
        {
			Logger.LogDebug(">>> Entering GetSubFolders_Throws_On_Null_FolderPath()");
            var folderMappingInfo = new FolderMappingInfo();

			Logger.LogDebug("GetSubFolders_Throws_On_Null_FolderPath(): folderMappingInfo → {FolderMappingInfo}", folderMappingInfo);
            Assert.Throws<ArgumentNullException>(() => this._dfp.GetSubFolders(null, folderMappingInfo).ToList());
			Logger.LogDebug("<<< Exiting GetSubFolders_Throws_On_Null_FolderPath()");
        }

        [Test]
        public void GetSubFolders_Calls_FolderManager_GetFoldersByParentFolder()
        {
			Logger.LogDebug(">>> Entering GetSubFolders_Calls_FolderManager_GetFoldersByParentFolder()");
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;
			Logger.LogDebug("GetSubFolders_Calls_FolderManager_GetFoldersByParentFolder(): folderMapping → {FolderMapping}", folderMapping);
			Logger.LogDebug("GetSubFolders_Calls_FolderManager_GetFoldersByParentFolder(): folderMapping.PortalID → {Arg0}", folderMapping.PortalID);

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this._folderInfo.Object);
            this._folderManager.Setup(fm => fm.GetFolders(this._folderInfo.Object)).Returns(new List<IFolderInfo>());

            this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

            this._folderManager.Verify(fm => fm.GetFolders(this._folderInfo.Object), Times.Once());
			Logger.LogDebug("<<< Exiting GetSubFolders_Calls_FolderManager_GetFoldersByParentFolder()");
        }

        [Test]
        public void GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count()
        {
			Logger.LogDebug(">>> Entering GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count()");
            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;
			Logger.LogDebug("GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count(): folderMapping → {FolderMapping}", folderMapping);
			Logger.LogDebug("GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count(): folderMapping.PortalID → {Arg0}", folderMapping.PortalID);

            var subFolders = new List<IFolderInfo>()
            {
                new FolderInfo { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath },
                new FolderInfo { FolderPath = Constants.FOLDER_OtherValidSubFolderRelativePath },
            };

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this._folderInfo.Object);
            this._folderManager.Setup(fm => fm.GetFolders(this._folderInfo.Object)).Returns(subFolders);

            var result = this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

			Logger.LogDebug("GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count(): result → {Result}", result);
            Assert.That(result, Has.Count.EqualTo(subFolders.Count));
			Logger.LogDebug("<<< Exiting GetSubFolders_Count_Equals_DataProvider_GetFoldersByParentFolder_Count()");
        }

        [Test]
        public void GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()
        {
			Logger.LogDebug(">>> Entering GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()");
            var expectedResult = new List<string>
            {
                Constants.FOLDER_ValidSubFolderRelativePath,
                Constants.FOLDER_OtherValidSubFolderRelativePath,
            };

            var folderMapping = new FolderMappingInfo();
            folderMapping.PortalID = Constants.CONTENT_ValidPortalId;
			Logger.LogDebug("GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty(): folderMapping → {FolderMapping}", folderMapping);
			Logger.LogDebug("GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty(): folderMapping.PortalID → {Arg0}", folderMapping.PortalID);

            var subFolders = new List<IFolderInfo>
            {
                new FolderInfo { FolderPath = Constants.FOLDER_ValidSubFolderRelativePath },
                new FolderInfo { FolderPath = Constants.FOLDER_OtherValidSubFolderRelativePath },
            };

            this._folderManager.Setup(fm => fm.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this._folderInfo.Object);
            this._folderManager.Setup(fm => fm.GetFolders(this._folderInfo.Object)).Returns(subFolders);

            var result = this._dfp.GetSubFolders(Constants.FOLDER_ValidFolderRelativePath, folderMapping).ToList();

			Logger.LogDebug("GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty(): result → {Result}", result);
            Assert.That(result, Is.EqualTo(expectedResult).AsCollection);
			Logger.LogDebug("<<< Exiting GetSubFolders_Returns_Valid_SubFolders_When_Folder_Is_Not_Empty()");
        }

        [Test]
        public void IsInSync_Returns_True()
        {
			Logger.LogDebug(">>> Entering IsInSync_Returns_True()");
            var result = this._dfp.IsInSync(It.IsAny<IFileInfo>());

			Logger.LogDebug("IsInSync_Returns_True(): result → {Result}", result);
            Assert.That(result, Is.True);
			Logger.LogDebug("<<< Exiting IsInSync_Returns_True()");
        }

        [Test]
        public void SupportsFileAttributes_Returns_False()
        {
			Logger.LogDebug(">>> Entering SupportsFileAttributes_Returns_False()");
            var result = this._dfp.SupportsFileAttributes();

			Logger.LogDebug("SupportsFileAttributes_Returns_False(): result → {Result}", result);
            Assert.That(result, Is.False);
			Logger.LogDebug("<<< Exiting SupportsFileAttributes_Returns_False()");
        }

        [Test]
        public void UpdateFile_Throws_On_Null_Folder()
        {
			Logger.LogDebug(">>> Entering UpdateFile_Throws_On_Null_Folder()");
            var stream = new Mock<Stream>();

			Logger.LogDebug("UpdateFile_Throws_On_Null_Folder(): stream → {Stream}", stream);
            Assert.Throws<ArgumentNullException>(() => this._dfp.UpdateFile(null, Constants.FOLDER_ValidFileName, stream.Object));
			Logger.LogDebug("<<< Exiting UpdateFile_Throws_On_Null_Folder()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void UpdateFile_Throws_On_NullOrEmpty_FileName(string fileName)
        {
			Logger.LogDebug(">>> Entering UpdateFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
            var stream = new Mock<Stream>();

			Logger.LogDebug("UpdateFile_Throws_On_NullOrEmpty_FileName(fileName={FileName}): stream → {Stream}", fileName, stream);
            Assert.Throws<ArgumentException>(() => this._dfp.UpdateFile(this._folderInfo.Object, fileName, stream.Object));
			Logger.LogDebug("<<< Exiting UpdateFile_Throws_On_NullOrEmpty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFileContent_When_File_Exists()
        {
			Logger.LogDebug(">>> Entering UpdateFile_Calls_DataProvider_UpdateFileContent_When_File_Exists()");
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileInfo.Setup(fi => fi.FileId).Returns(Constants.FOLDER_ValidFileId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, true)).Returns(this._fileInfo.Object);

            this._dfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(new byte[16 * 1024]));

            this._mockData.Verify(md => md.UpdateFileContent(Constants.FOLDER_ValidFileId, It.IsAny<byte[]>()), Times.Once());
			Logger.LogDebug("<<< Exiting UpdateFile_Calls_DataProvider_UpdateFileContent_When_File_Exists()");
        }

        [Test]
        public void UpdateFile_Does_Not_Call_DataProvider_UpdateFileContent_When_File_Does_Not_Exist()
        {
			Logger.LogDebug(">>> Entering UpdateFile_Does_Not_Call_DataProvider_UpdateFileContent_When_File_Does_Not_Exist()");
            this._folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this._folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this._fileManager.Setup(fm => fm.GetFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns((IFileInfo)null);

            this._dfp.UpdateFile(this._folderInfo.Object, Constants.FOLDER_ValidFileName, null);

            this._mockData.Verify(md => md.UpdateFileContent(It.IsAny<int>(), It.IsAny<byte[]>()), Times.Never());
			Logger.LogDebug("<<< Exiting UpdateFile_Does_Not_Call_DataProvider_UpdateFileContent_When_File_Does_Not_Exist()");
        }
    }
}
