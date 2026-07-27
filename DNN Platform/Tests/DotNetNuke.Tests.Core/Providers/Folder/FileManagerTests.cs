// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    [TestFixture]
    public class FileManagerTests
    {
        private FileManager fileManager;
        private Mock<IFolderManager> folderManager;
        private Mock<IFolderPermissionController> folderPermissionController;
        private Mock<IPortalController> portalController;
        private Mock<IFolderMappingController> folderMappingController;
        private Mock<IGlobals> globals;
        private Mock<ICBO> cbo;
        private Mock<DataProvider> mockData;
        private Mock<FolderProvider> mockFolder;
        private Mock<CachingProvider> mockCache;
        private Mock<FileManager> mockFileManager;
        private Mock<IFolderInfo> folderInfo;
        private Mock<IFileInfo> fileInfo;
        private Mock<IPathUtils> pathUtils;
        private Mock<IFileVersionController> fileVersionController;
        private Mock<IWorkflowManager> workflowManager;
        private Mock<IEventHandlersContainer<IFileEventHandlers>> fileEventHandlersContainer;
        private Mock<IFileLockingController> mockFileLockingController;
        private Mock<IFileDeletionController> mockFileDeletionController;
        private FakeHostController hostController;
        private Mock<IUserController> mockUserController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			eventManager.LogDebug(">>> Entering Setup()");
            this.mockData = MockComponentProvider.CreateDataProvider();
            this.mockFolder = MockComponentProvider.CreateFolderProvider(Constants.FOLDER_ValidFolderProviderType);
			eventManager.LogDebug("Setup(): mockFolder → {MockFolder}", mockFolder);
			eventManager.LogDebug("Setup(): mockData → {MockData}", mockData);
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
			eventManager.LogDebug("Setup(): mockCache → {MockCache}", mockCache);

            this.folderManager = new Mock<IFolderManager>();
			eventManager.LogDebug("Setup(): folderManager → {FolderManager}", folderManager);
            this.folderPermissionController = new Mock<IFolderPermissionController>();
			eventManager.LogDebug("Setup(): folderPermissionController → {FolderPermissionController}", folderPermissionController);
            this.portalController = new Mock<IPortalController>();
			eventManager.LogDebug("Setup(): portalController → {PortalController}", portalController);
            this.hostController = new FakeHostController(new Dictionary<string, IConfigurationSetting>());
			eventManager.LogDebug("Setup(): hostController → {HostController}", hostController);
            this.folderMappingController = new Mock<IFolderMappingController>();
			eventManager.LogDebug("Setup(): folderMappingController → {FolderMappingController}", folderMappingController);
            this.fileVersionController = new Mock<IFileVersionController>();
			eventManager.LogDebug("Setup(): fileVersionController → {FileVersionController}", fileVersionController);
            this.workflowManager = new Mock<IWorkflowManager>();
			eventManager.LogDebug("Setup(): workflowManager → {WorkflowManager}", workflowManager);
            this.fileEventHandlersContainer = new Mock<IEventHandlersContainer<IFileEventHandlers>>();
			eventManager.LogDebug("Setup(): fileEventHandlersContainer → {FileEventHandlersContainer}", fileEventHandlersContainer);
            this.globals = new Mock<IGlobals>();
			eventManager.LogDebug("Setup(): globals → {Globals}", globals);
            this.cbo = new Mock<ICBO>();
			eventManager.LogDebug("Setup(): cbo → {Cbo}", cbo);
            this.pathUtils = new Mock<IPathUtils>();
			eventManager.LogDebug("Setup(): pathUtils → {PathUtils}", pathUtils);
            this.mockFileLockingController = new Mock<IFileLockingController>();
			eventManager.LogDebug("Setup(): mockFileLockingController → {MockFileLockingController}", mockFileLockingController);
            this.mockFileDeletionController = new Mock<IFileDeletionController>();
			eventManager.LogDebug("Setup(): mockFileDeletionController → {MockFileDeletionController}", mockFileDeletionController);
            this.mockUserController = new Mock<IUserController>();
			eventManager.LogDebug("Setup(): mockUserController → {MockUserController}", mockUserController);

            EventLogController.SetTestableInstance(Mock.Of<IEventLogController>());
            FolderManager.RegisterInstance(this.folderManager.Object);
            FolderPermissionController.SetTestableInstance(this.folderPermissionController.Object);
            PortalController.SetTestableInstance(this.portalController.Object);
            FolderMappingController.RegisterInstance(this.folderMappingController.Object);
            TestableGlobals.SetTestableInstance(this.globals.Object);
            CBO.SetTestableInstance(this.cbo.Object);
            PathUtils.RegisterInstance(this.pathUtils.Object);
            FileVersionController.RegisterInstance(this.fileVersionController.Object);
            WorkflowManager.SetTestableInstance(this.workflowManager.Object);
            EventHandlersContainer<IFileEventHandlers>.RegisterInstance(this.fileEventHandlersContainer.Object);
            UserController.SetTestableInstance(this.mockUserController.Object);

            this.folderInfo = new Mock<IFolderInfo>();
			eventManager.LogDebug("Setup(): folderInfo → {FolderInfo}", folderInfo);
            this.fileInfo = new Mock<IFileInfo>();
			eventManager.LogDebug("Setup(): fileInfo → {FileInfo}", fileInfo);

            var hostSettings = new HostSettings(this.hostController);
            var eventManager = new EventManager(Mock.Of<IEventLogger>());
			eventManager.LogDebug("Setup(): hostSettings → {HostSettings}", hostSettings);
			eventManager.LogDebug("Setup(): eventManager → {EventManager}", eventManager);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				eventManager.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
				eventManager.LogDebug("Setup(): services → {Services}", services);
                    services.AddSingleton(this.mockData.Object);
                    services.AddSingleton(this.mockFolder.Object);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton(this.folderManager.Object);
                    services.AddSingleton(this.folderPermissionController.Object);
                    services.AddSingleton(this.portalController.Object);
                    services.AddSingleton<IHostController>(this.hostController);
                    services.AddSingleton<IHostSettingsService>(this.hostController);
                    services.AddSingleton(this.folderMappingController.Object);
                    services.AddSingleton(this.fileVersionController.Object);
                    services.AddSingleton(this.workflowManager.Object);
                    services.AddSingleton(this.fileEventHandlersContainer.Object);
                    services.AddSingleton(this.globals.Object);
                    services.AddSingleton(this.cbo.Object);
                    services.AddSingleton(this.pathUtils.Object);
                    services.AddSingleton(this.mockFileLockingController.Object);
                    services.AddSingleton(this.mockFileDeletionController.Object);
                    services.AddSingleton(new ListController(Mock.Of<IEventLogger>(), hostSettings));
                    services.AddSingleton<IHostSettings>(hostSettings);
                    services.AddSingleton<IEventManager>(eventManager);
                    services.AddSingleton(Mock.Of<IApplicationStatusInfo>());
                    services.AddSingleton(Mock.Of<IPortalGroupController>());
                    services.AddSingleton(Mock.Of<IEventLogger>());
                    services.AddSingleton(Mock.Of<ITermController>());
                    services.AddSingleton(Mock.Of<IContentTypeController>());
                    services.AddSingleton(Mock.Of<IContentController>());
                    services.AddSingleton(Mock.Of<IWorkflowEngine>());
                    services.AddSingleton(Mock.Of<IWorkflowSecurity>());
                    services.AddSingleton(Mock.Of<ISystemWorkflowManager>());
                    services.AddSingleton(Mock.Of<IFileContentTypeManager>());
                    services.AddTransient<IFileSecurityController, FileSecurityController>();
                });

            this.fileManager = new FileManager(
                this.serviceProvider.GetRequiredService<IFileSecurityController>(),
                this.mockFileLockingController.Object,
                this.fileVersionController.Object,
                this.mockFileDeletionController.Object,
                Mock.Of<IFileContentTypeManager>(),
                this.folderMappingController.Object,
                this.folderPermissionController.Object,
                Mock.Of<ISystemWorkflowManager>(),
                this.workflowManager.Object,
                Mock.Of<IWorkflowEngine>(),
                Mock.Of<IWorkflowSecurity>(),
                this.mockUserController.Object,
                Mock.Of<IContentController>(),
                Mock.Of<IContentTypeController>(),
                Mock.Of<ITermController>(),
                hostSettings,
                this.cbo.Object,
                eventManager,
                Mock.Of<IEventLogger>(),
                this.portalController.Object,
                this.mockData.Object,
                this.pathUtils.Object);
				eventManager.LogDebug("Setup(): fileManager → {FileManager}", fileManager);
            this.mockFileManager = new Mock<FileManager>(
                this.serviceProvider.GetRequiredService<IFileSecurityController>(),
                this.mockFileLockingController.Object,
                this.fileVersionController.Object,
                this.mockFileDeletionController.Object,
                Mock.Of<IFileContentTypeManager>(),
                this.folderMappingController.Object,
                this.folderPermissionController.Object,
                Mock.Of<ISystemWorkflowManager>(),
                this.workflowManager.Object,
                Mock.Of<IWorkflowEngine>(),
                Mock.Of<IWorkflowSecurity>(),
                this.mockUserController.Object,
                Mock.Of<IContentController>(),
                Mock.Of<IContentTypeController>(),
                Mock.Of<ITermController>(),
                hostSettings,
                this.cbo.Object,
                eventManager,
                Mock.Of<IEventLogger>(),
                this.portalController.Object,
                this.mockData.Object,
                this.pathUtils.Object) { CallBase = true, };
				eventManager.LogDebug("Setup(): mockFileManager → {MockFileManager}", mockFileManager);

            FileLockingController.SetTestableInstance(this.mockFileLockingController.Object);
            FileDeletionController.SetTestableInstance(this.mockFileDeletionController.Object);

            this.mockUserController.Setup(uc => uc.GetCurrentUserInfo()).Returns(new UserInfo());
			eventManager.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			eventManager.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            TestableGlobals.ClearInstance();
            CBO.ClearInstance();

            FolderPermissionController.ClearInstance();
            FileLockingController.ClearInstance();
            FileDeletionController.ClearInstance();
            MockComponentProvider.ResetContainer();
            PortalController.ClearInstance();
			eventManager.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void AddFile_Throws_On_Null_Folder()
        {
			eventManager.LogDebug(">>> Entering AddFile_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.AddFile(null, It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>()));
			eventManager.LogDebug("<<< Exiting AddFile_Throws_On_Null_Folder()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void AddFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
			eventManager.LogDebug(">>> Entering AddFile_Throws_On_Null_Or_Empty_FileName(fileName={FileName})", fileName);
            Assert.Throws<ArgumentException>(() => this.fileManager.AddFile(this.folderInfo.Object, fileName, It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>()));
			eventManager.LogDebug("<<< Exiting AddFile_Throws_On_Null_Or_Empty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void AddFile_Throws_On_Null_FileContent()
        {
			eventManager.LogDebug(">>> Entering AddFile_Throws_On_Null_FileContent()");
            Assert.Throws<ArgumentException>(() => this.fileManager.AddFile(this.folderInfo.Object, It.IsAny<string>(), null, It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>()));
			eventManager.LogDebug("<<< Exiting AddFile_Throws_On_Null_FileContent()");
        }

        [Test]
        public void AddFile_Throws_When_Permissions_Are_Not_Met()
        {
			eventManager.LogDebug(">>> Entering AddFile_Throws_When_Permissions_Are_Not_Met()");
            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(false);

            Assert.Throws<PermissionsNotMetException>(() => this.fileManager.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, new MemoryStream(), It.IsAny<bool>(), true, It.IsAny<string>()));
			eventManager.LogDebug("<<< Exiting AddFile_Throws_When_Permissions_Are_Not_Met()");
        }

        [Test]
        public void AddFile_Throws_When_Portal_Has_No_Space_Available()
        {
			eventManager.LogDebug(">>> Entering AddFile_Throws_When_Portal_Has_No_Space_Available()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockData.Setup(c => c.GetProviderPath()).Returns(string.Empty);

            var fileContent = new MemoryStream();

			eventManager.LogDebug("AddFile_Throws_When_Portal_Has_No_Space_Available(): fileContent → {FileContent}", fileContent);
            this.globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(false);

            Assert.Throws<NoSpaceAvailableException>(() => this.fileManager.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType));
			eventManager.LogDebug("<<< Exiting AddFile_Throws_When_Portal_Has_No_Space_Available()");
        }

        [Test]
        public void AddFile_Checks_Space_For_Stream_Length()
        {
			eventManager.LogDebug(">>> Entering AddFile_Checks_Space_For_Stream_Length()");
            // Arrange
            this.PrepareFileSecurityCheck();
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream("some data here"u8.ToArray());

			eventManager.LogDebug("AddFile_Checks_Space_For_Stream_Length(): fileContent → {FileContent}", fileContent);
            this.hostController.AddSetting("FileExtensions", string.Empty);

            this.portalController.Setup(pc => pc.HasSpaceAvailable(It.IsAny<int>(), It.IsAny<long>())).Returns(true);

            this.globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);
            this.mockFolder.Setup(mf => mf.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();

            this.workflowManager.Setup(we => we.GetWorkflow(It.IsAny<int>())).Returns((Workflow)null);

            // Act
            this.fileManager.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, false, Constants.CONTENTTYPE_ValidContentType);

            // Assert
            this.portalController.Verify(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length));
			eventManager.LogDebug("<<< Exiting AddFile_Checks_Space_For_Stream_Length()");
        }

        [Test]
        public void AddFile_Throws_When_Extension_Is_Invalid()
        {
			eventManager.LogDebug(">>> Entering AddFile_Throws_When_Extension_Is_Invalid()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            var fileContent = new MemoryStream();

			eventManager.LogDebug("AddFile_Throws_When_Extension_Is_Invalid(): fileContent → {FileContent}", fileContent);
            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);
            this.hostController.AddSetting("FileExtensions", "abc");

            Assert.Throws<InvalidFileExtensionException>(() => this.fileManager.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType));
			eventManager.LogDebug("<<< Exiting AddFile_Throws_When_Extension_Is_Invalid()");
        }

        [TestCase("invalid_script.svg")]
        [TestCase("invalid_namespaced-script.svg")]
        [TestCase("invalid_a-href.svg")]
        [TestCase("invalid_onload.svg")]
        [TestCase("invalid_onload-uppercase.svg")]
        [TestCase("invalid_onerror.svg")]
        [TestCase("invalid_onerror-uppercase.svg")]
        [TestCase("invalid_foreignObject-iframe-src-data.svg")]
        [TestCase("invalid_foreignObject-iframe-srcdoc.svg")]
        [TestCase("invalid_encoded-parenthesis.svg")]
        [TestCase("DOMPurify/invalid_attribute-mXSS_1.svg")]
        [TestCase("DOMPurify/invalid_attribute-mXSS_2.svg")]
        [TestCase("DOMPurify/invalid_embedded-MathML.svg")]
        [TestCase("DOMPurify/invalid_fake-element-based-namespace-confusion.svg")]
        [TestCase("DOMPurify/invalid_mXSS-Chrome-77_1.svg")]
        [TestCase("DOMPurify/invalid_mXSS-Chrome-77_2.svg")]
        [TestCase("DOMPurify/invalid_mXSS-template-Chrome-77.svg")]
        [TestCase("XSS-Payloads/Password_steal.svg")]
        [TestCase("XSS-Payloads/hero-xss.svg")]
        [TestCase("XSS-Payloads/xss-ww.svg")]
        [TestCase("XSS_SCRIPTS/desc.svg")]
        [TestCase("XSS_SCRIPTS/foreignObject.svg")]
        [TestCase("XSS_SCRIPTS/onload.svg")]
        [TestCase("XSS_SCRIPTS/title.svg")]
        [TestCase("OWASP/chameleon.svg")]
        [TestCase("OWASP/foreignObject.svg")]
        [TestCase("OWASP/handler.svg")]
        [TestCase("OWASP/href.svg")]
        [TestCase("OWASP/onload.svg")]
        [TestCase("OWASP/set.svg")]
        public void AddFile_Throws_When_File_Content_Is_Invalid(string fileName)
        {
			eventManager.LogDebug(">>> Entering AddFile_Throws_When_File_Content_Is_Invalid(fileName={FileName})", fileName);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.PrepareFileSecurityCheck();

            using var fileContent = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources/{fileName}"));
            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);
			eventManager.LogDebug("AddFile_Throws_When_File_Content_Is_Invalid(fileName={FileName}): fileContent → {FileContent}", fileName, fileContent);
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidSvgFileName)).Returns(true);
            this.hostController.AddSetting("FileExtensions", string.Empty);

            Assert.Throws<InvalidFileContentException>(() =>
                this.fileManager.AddFile(
                    this.folderInfo.Object,
                    Constants.FOLDER_ValidSvgFileName,
                    fileContent,
                    false,
                    false,
                    Constants.CONTENTTYPE_ValidContentType));
					eventManager.LogDebug("<<< Exiting AddFile_Throws_When_File_Content_Is_Invalid(fileName={FileName})", fileName);
					eventManager.LogInformation("AddFile_Throws_When_File_Content_Is_Invalid(fileName)={FileName}: external service call took {Arg1} ms", fileName, __logStart.ElapsedMilliseconds);
        }

        [TestCase("valid.svg")]
        [TestCase("DOMPurify/valid_data-URI.svg")]
        [TestCase("DOMPurify/valid_data-URI-href.svg")]
        [TestCase("DOMPurify/valid_filter.svg")]
        public void AddFile_No_Error_When_File_Content_Is_Valid(string fileName)
        {
			eventManager.LogDebug(">>> Entering AddFile_No_Error_When_File_Content_Is_Valid(fileName={FileName})", fileName);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.PrepareFileSecurityCheck();

            using var fileContent = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Resources/{fileName}"));
            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);
			eventManager.LogDebug("AddFile_No_Error_When_File_Content_Is_Valid(fileName={FileName}): fileContent → {FileContent}", fileName, fileContent);
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidSvgFileName)).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.IsImageFile(It.IsAny<IFileInfo>())).Returns(false);
            this.hostController.AddSetting("FileExtensions", string.Empty);

            this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidSvgFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);
			eventManager.LogDebug("<<< Exiting AddFile_No_Error_When_File_Content_Is_Valid(fileName={FileName})", fileName);
			eventManager.LogInformation("AddFile_No_Error_When_File_Content_Is_Valid(fileName)={FileName}: external service call took {Arg1} ms", fileName, __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwriting_And_File_Exists()
        {
			eventManager.LogDebug(">>> Entering AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwriting_And_File_Exists()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.PhysicalPath).Returns(Constants.FOLDER_ValidFolderPath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.WorkflowID).Returns(Null.NullInteger);

            var fileContent = new MemoryStream();

			eventManager.LogDebug("AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwriting_And_File_Exists(): fileContent → {FileContent}", fileContent);
            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, fileContent.Length)).Returns(true);

            this.globals.Setup(g => g.GetSubFolderPath(Constants.FOLDER_ValidFilePath, Constants.CONTENT_ValidPortalId)).Returns(Constants.FOLDER_ValidFolderRelativePath);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);
            this.mockFolder.Setup(mf => mf.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent));
            this.mockFolder.Setup(mf => mf.GetHashCode(It.IsAny<IFileInfo>())).Returns("aaa");

            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_ValidFileName)).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(It.IsAny<IFileInfo>(), It.IsAny<Stream>()));
            this.mockFileManager.Setup(mfm => mfm.CreateFileContentItem()).Returns(new ContentItem());

            this.workflowManager.Setup(wc => wc.GetWorkflow(It.IsAny<int>())).Returns((Workflow)null);

            this.mockData.Setup(
                md =>
                md.AddFile(
                    It.IsAny<int>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<bool>(),
                    It.IsAny<int>()))
               .Returns(Constants.FOLDER_ValidFileId);

            this.mockData.Setup(md => md.UpdateFileLastModificationTime(It.IsAny<int>(), It.IsAny<DateTime>()));

            this.mockFileManager.Object.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, false, false, Constants.CONTENTTYPE_ValidContentType);

            this.mockFolder.Verify(mf => mf.AddFile(It.IsAny<IFolderInfo>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());
			eventManager.LogDebug("<<< Exiting AddFile_Does_Not_Call_FolderProvider_AddFile_When_Not_Overwriting_And_File_Exists()");
        }

        [Test]
        public void CopyFile_Throws_On_Null_File()
        {
			eventManager.LogDebug(">>> Entering CopyFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.CopyFile(null, this.folderInfo.Object));
			eventManager.LogDebug("<<< Exiting CopyFile_Throws_On_Null_File()");
        }

        [Test]
        public void CopyFile_Throws_On_Null_DestinationFolder()
        {
			eventManager.LogDebug(">>> Entering CopyFile_Throws_On_Null_DestinationFolder()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.CopyFile(this.fileInfo.Object, null));
			eventManager.LogDebug("<<< Exiting CopyFile_Throws_On_Null_DestinationFolder()");
        }

        [Test]
        public void CopyFile_Calls_FileManager_AddFile_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Not_Equal()
        {
			eventManager.LogDebug(">>> Entering CopyFile_Calls_FileManager_AddFile_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Not_Equal()");
            // Arrange
            const int SourceFolderMappingId = Constants.FOLDER_ValidFolderMappingID;
            const int DestinationFolderMappingId = Constants.FOLDER_ValidFolderMappingID + 1;
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.ContentType).Returns(Constants.CONTENTTYPE_ValidContentType);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(SourceFolderMappingId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(DestinationFolderMappingId);
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var fileContent = new MemoryStream(bytes);
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
			eventManager.LogDebug("CopyFile_Calls_FileManager_AddFile_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Not_Equal(): fileContent → {FileContent}", fileContent);
            this.mockFileManager.Setup(mfm => mfm.CopyContentItem(It.IsAny<int>())).Returns(Constants.CONTENT_ValidContentItemId);
            this.mockFileManager.Setup(mfm => mfm.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<Stream>(), true, true, Constants.CONTENTTYPE_ValidContentType))
                .Returns(this.fileInfo.Object);
            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(true);

            // Act
            this.mockFileManager.Object.CopyFile(this.fileInfo.Object, this.folderInfo.Object);

            // Assert
            this.mockFileManager.Verify(fm => fm.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent, true, true, Constants.CONTENTTYPE_ValidContentType), Times.Once());
			eventManager.LogDebug("<<< Exiting CopyFile_Calls_FileManager_AddFile_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Not_Equal()");
        }

        [Test]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Cannot_Add_Folder()
        {
			eventManager.LogDebug(">>> Entering CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Cannot_Add_Folder()");
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(false);

            Assert.Throws<PermissionsNotMetException>(() => this.fileManager.CopyFile(this.fileInfo.Object, this.folderInfo.Object));
			eventManager.LogDebug("<<< Exiting CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Cannot_Add_Folder()");
        }

        [Test]
        public void CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Portal_Has_No_Space_Available()
        {
			eventManager.LogDebug(">>> Entering CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Portal_Has_No_Space_Available()");
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.Size).Returns(Constants.FOLDER_ValidFileSize);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);

            this.folderPermissionController.Setup(fpc => fpc.CanAddFolder(this.folderInfo.Object)).Returns(true);
            this.portalController.Setup(pc => pc.HasSpaceAvailable(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileSize)).Returns(false);

            Assert.Throws<NoSpaceAvailableException>(() => this.fileManager.CopyFile(this.fileInfo.Object, this.folderInfo.Object));
			eventManager.LogDebug("<<< Exiting CopyFile_Throws_When_FolderMapping_Of_Source_And_Destination_Folders_Are_Equal_And_Portal_Has_No_Space_Available()");
        }

        [Test]
        public void DeleteFile_Throws_On_Null_File()
        {
			eventManager.LogDebug(">>> Entering DeleteFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.DeleteFile(null));
			eventManager.LogDebug("<<< Exiting DeleteFile_Throws_On_Null_File()");
        }

        [Test]
        public void DeleteFile_Calls_FileDeletionControllerDeleteFile()
        {
			eventManager.LogDebug(">>> Entering DeleteFile_Calls_FileDeletionControllerDeleteFile()");
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileDeletionController.Setup(fdc => fdc.DeleteFile(this.fileInfo.Object)).Verifiable();

            this.mockFileManager.Object.DeleteFile(this.fileInfo.Object);

            this.mockFileDeletionController.Verify();
			eventManager.LogDebug("<<< Exiting DeleteFile_Calls_FileDeletionControllerDeleteFile()");
        }

        [Test]
        public void DeleteFile_Throws_WhenFileDeletionControllerThrows()
        {
			eventManager.LogDebug(">>> Entering DeleteFile_Throws_WhenFileDeletionControllerThrows()");
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.mockFileDeletionController.Setup(fdc => fdc.DeleteFile(this.fileInfo.Object))
                                       .Throws<FolderProviderException>();

            Assert.Throws<FolderProviderException>(() => this.mockFileManager.Object.DeleteFile(this.fileInfo.Object));
			eventManager.LogDebug("<<< Exiting DeleteFile_Throws_WhenFileDeletionControllerThrows()");
        }

        [Test]
        public void DownloadFile_Throws_On_Null_File()
        {
			eventManager.LogDebug(">>> Entering DownloadFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.WriteFileToResponse(null, ContentDisposition.Inline));
			eventManager.LogDebug("<<< Exiting DownloadFile_Throws_On_Null_File()");
        }

        [Test]
        public void DownloadFile_Throws_When_Permissions_Are_Not_Met()
        {
			eventManager.LogDebug(">>> Entering DownloadFile_Throws_When_Permissions_Are_Not_Met()");
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(false);

            Assert.Throws<PermissionsNotMetException>(() => this.fileManager.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline));
			eventManager.LogDebug("<<< Exiting DownloadFile_Throws_When_Permissions_Are_Not_Met()");
        }

        [Test]
        public void DownloadFile_Calls_FileManager_AutoSyncFile_When_File_AutoSync_Is_Enabled()
        {
			eventManager.LogDebug(">>> Entering DownloadFile_Calls_FileManager_AutoSyncFile_When_File_AutoSync_Is_Enabled()");
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(true);

            this.mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.AutoSyncFile(this.fileInfo.Object)).Verifiable();
            this.mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this.fileInfo.Object, It.IsAny<ContentDisposition>()));

            this.mockFileManager.Object.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline);

            this.mockFileManager.Verify();
			eventManager.LogDebug("<<< Exiting DownloadFile_Calls_FileManager_AutoSyncFile_When_File_AutoSync_Is_Enabled()");
        }

        [Test]
        public void DownloadFile_Does_Not_Call_FileManager_AutoSyncFile_When_File_AutoSync_Is_Not_Enabled()
        {
			eventManager.LogDebug(">>> Entering DownloadFile_Does_Not_Call_FileManager_AutoSyncFile_When_File_AutoSync_Is_Not_Enabled()");
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(true);

            this.mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this.fileInfo.Object, It.IsAny<ContentDisposition>()));

            this.mockFileManager.Object.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline);

            this.mockFileManager.Verify(mfm => mfm.AutoSyncFile(this.fileInfo.Object), Times.Never());
			eventManager.LogDebug("<<< Exiting DownloadFile_Does_Not_Call_FileManager_AutoSyncFile_When_File_AutoSync_Is_Not_Enabled()");
        }

        [Test]
        public void DownloadFile_Calls_FileManager_WriteBytesToHttpContext()
        {
			eventManager.LogDebug(">>> Entering DownloadFile_Calls_FileManager_WriteBytesToHttpContext()");
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.folderPermissionController.Setup(fpc => fpc.CanViewFolder(this.folderInfo.Object)).Returns(true);

            this.mockFileManager.Setup(mfm => mfm.IsFileAutoSyncEnabled()).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.WriteFileToHttpContext(this.fileInfo.Object, It.IsAny<ContentDisposition>())).Verifiable();

            this.mockFileManager.Object.WriteFileToResponse(this.fileInfo.Object, ContentDisposition.Inline);

            this.mockFileManager.Verify();
			eventManager.LogDebug("<<< Exiting DownloadFile_Calls_FileManager_WriteBytesToHttpContext()");
        }

        [Test]
        public void ExistsFile_Throws_On_Null_Folder()
        {
			eventManager.LogDebug(">>> Entering ExistsFile_Throws_On_Null_Folder()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.FileExists(null, It.IsAny<string>()));
			eventManager.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_Folder()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void ExistsFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
			eventManager.LogDebug(">>> Entering ExistsFile_Throws_On_Null_Or_Empty_FileName(fileName={FileName})", fileName);
            Assert.Throws<ArgumentException>(() => this.fileManager.FileExists(this.folderInfo.Object, fileName));
			eventManager.LogDebug("<<< Exiting ExistsFile_Throws_On_Null_Or_Empty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void ExistsFile_Calls_FileManager_GetFile()
        {
			eventManager.LogDebug(">>> Entering ExistsFile_Calls_FileManager_GetFile()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns<IFileInfo>(null!).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockFileManager.Verify();
			eventManager.LogDebug("<<< Exiting ExistsFile_Calls_FileManager_GetFile()");
        }

        [Test]
        public void ExistsFile_Calls_FolderProvider_ExistsFile()
        {
			eventManager.LogDebug(">>> Entering ExistsFile_Calls_FolderProvider_ExistsFile()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true).Verifiable();

            this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockFolder.Verify();
			eventManager.LogDebug("<<< Exiting ExistsFile_Calls_FolderProvider_ExistsFile()");
        }

        [Test]
        public void ExistsFile_Returns_True_When_File_Exists()
        {
			eventManager.LogDebug(">>> Entering ExistsFile_Returns_True_When_File_Exists()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(true);

            var result = this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

			eventManager.LogDebug("ExistsFile_Returns_True_When_File_Exists(): result → {Result}", result);
            Assert.That(result, Is.True);
			eventManager.LogDebug("<<< Exiting ExistsFile_Returns_True_When_File_Exists()");
        }

        [Test]
        public void ExistsFile_Returns_False_When_File_Does_Not_Exist()
        {
			eventManager.LogDebug(">>> Entering ExistsFile_Returns_False_When_File_Does_Not_Exist()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Returns(false);

            var result = this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

			eventManager.LogDebug("ExistsFile_Returns_False_When_File_Does_Not_Exist(): result → {Result}", result);
            Assert.That(result, Is.False);
			eventManager.LogDebug("<<< Exiting ExistsFile_Returns_False_When_File_Does_Not_Exist()");
        }

        [Test]
        public void ExistsFile_Throws_When_FolderProvider_Throws()
        {
			eventManager.LogDebug(">>> Entering ExistsFile_Throws_When_FolderProvider_Throws()");
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(this.fileInfo.Object);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName)).Throws<Exception>();

            Assert.Throws<FolderProviderException>(() => this.mockFileManager.Object.FileExists(this.folderInfo.Object, Constants.FOLDER_ValidFileName));
			eventManager.LogDebug("<<< Exiting ExistsFile_Throws_When_FolderProvider_Throws()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void GetFile_Throws_On_Null_Or_Empty_FileName(string fileName)
        {
			eventManager.LogDebug(">>> Entering GetFile_Throws_On_Null_Or_Empty_FileName(fileName={FileName})", fileName);
            Assert.Throws<ArgumentException>(() => this.fileManager.GetFile(this.folderInfo.Object, fileName));
			eventManager.LogDebug("<<< Exiting GetFile_Throws_On_Null_Or_Empty_FileName(fileName={FileName})", fileName);
        }

        [Test]
        public void GetFile_Calls_DataProvider_GetFile()
        {
			eventManager.LogDebug(">>> Entering GetFile_Calls_DataProvider_GetFile()");
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);

            this.mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this.fileManager.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockData.Verify();
			eventManager.LogDebug("<<< Exiting GetFile_Calls_DataProvider_GetFile()");
        }

        [Test]
        public void GetFile_Handles_Path_In_Portal_Root()
        {
			eventManager.LogDebug(">>> Entering GetFile_Handles_Path_In_Portal_Root()");
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, string.Empty)).Returns(this.folderInfo.Object).Verifiable();
            this.mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this.fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFileName);

            this.folderManager.Verify();
            this.mockData.Verify();
			eventManager.LogDebug("<<< Exiting GetFile_Handles_Path_In_Portal_Root()");
        }

        [Test]
        public void GetFile_Handles_Path_Beyond_Portal_Root()
        {
			eventManager.LogDebug(">>> Entering GetFile_Handles_Path_Beyond_Portal_Root()");
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_ValidFolderId);
            this.folderManager.Setup(x => x.GetFolder(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath)).Returns(this.folderInfo.Object).Verifiable();
            this.mockData.Setup(md => md.GetFile(Constants.FOLDER_ValidFileName, Constants.FOLDER_ValidFolderId, It.IsAny<bool>())).Returns(It.IsAny<IDataReader>()).Verifiable();

            this.fileManager.GetFile(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderRelativePath + Constants.FOLDER_ValidFileName);

            this.folderManager.Verify();
            this.mockData.Verify();
			eventManager.LogDebug("<<< Exiting GetFile_Handles_Path_Beyond_Portal_Root()");
        }

        [Test]
        public void GetFileByID_Does_Not_Call_DataCache_GetCache_If_FileId_Is_Not_Valid()
        {
			eventManager.LogDebug(">>> Entering GetFileByID_Does_Not_Call_DataCache_GetCache_If_FileId_Is_Not_Valid()");
            this.mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(this.fileInfo.Object).Verifiable();

            this.fileManager.GetFile(Constants.FOLDER_InvalidFileId);

            this.mockCache.Verify(mc => mc.GetItem(It.IsAny<string>()), Times.Never());
			eventManager.LogDebug("<<< Exiting GetFileByID_Does_Not_Call_DataCache_GetCache_If_FileId_Is_Not_Valid()");
        }

        [Test]
        public void GetFileByID_Calls_DataCache_GetCache_First()
        {
			eventManager.LogDebug(">>> Entering GetFileByID_Calls_DataCache_GetCache_First()");
            this.mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(this.fileInfo.Object).Verifiable();

            this.fileManager.GetFile(Constants.FOLDER_ValidFileId);

            this.mockCache.Verify();
			eventManager.LogDebug("<<< Exiting GetFileByID_Calls_DataCache_GetCache_First()");
        }

        [Test]
        public void GetFileByID_Calls_DataProvider_GetFileById_When_File_Is_Not_In_Cache()
        {
			eventManager.LogDebug(">>> Entering GetFileByID_Calls_DataProvider_GetFileById_When_File_Is_Not_In_Cache()");
            this.mockCache.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null!);

            this.fileManager.GetFile(Constants.FOLDER_ValidFileId);

            this.mockData.Verify(md => md.GetFileById(Constants.FOLDER_ValidFileId, It.IsAny<bool>()), Times.Once());
			eventManager.LogDebug("<<< Exiting GetFileByID_Calls_DataProvider_GetFileById_When_File_Is_Not_In_Cache()");
        }

        [Test]
        public void MoveFile_Throws_On_Null_File()
        {
			eventManager.LogDebug(">>> Entering MoveFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.MoveFile(null, this.folderInfo.Object));
			eventManager.LogDebug("<<< Exiting MoveFile_Throws_On_Null_File()");
        }

        [Test]
        public void MoveFile_Throws_On_Null_DestinationFolder()
        {
			eventManager.LogDebug(">>> Entering MoveFile_Throws_On_Null_DestinationFolder()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.MoveFile(this.fileInfo.Object, null));
			eventManager.LogDebug("<<< Exiting MoveFile_Throws_On_Null_DestinationFolder()");
        }

        [Test]
        public void MoveFile_Calls_FolderProvider_AddFile_And_DeleteFile_And_FileManager_UpdateFile()
        {
			eventManager.LogDebug(">>> Entering MoveFile_Calls_FolderProvider_AddFile_And_DeleteFile_And_FileManager_UpdateFile()");
            // Arrange
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);
            var fileContent = new MemoryStream();
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
			eventManager.LogDebug("MoveFile_Calls_FolderProvider_AddFile_And_DeleteFile_And_FileManager_UpdateFile(): fileContent → {FileContent}", fileContent);
            string someString;
            this.mockFileLockingController.Setup(flc => flc.IsFileLocked(this.fileInfo.Object, out someString)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.MoveVersions(this.fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));
            this.mockFolder.Setup(mf => mf.AddFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, fileContent)).Verifiable();
            this.mockFolder.Setup(mf => mf.DeleteFile(this.fileInfo.Object)).Verifiable();
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(this.fileInfo.Object)).Verifiable();

            // Act
            this.mockFileManager.Object.MoveFile(this.fileInfo.Object, this.folderInfo.Object);

            // Assert
            this.mockFolder.Verify();
            this.mockFileManager.Verify();
			eventManager.LogDebug("<<< Exiting MoveFile_Calls_FolderProvider_AddFile_And_DeleteFile_And_FileManager_UpdateFile()");
        }

        [Test]
        public void MoveFile_Updates_FolderId_And_Folder()
        {
			eventManager.LogDebug(">>> Entering MoveFile_Updates_FolderId_And_Folder()");
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.Folder).Returns(Constants.FOLDER_ValidFolderRelativePath);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderPath).Returns(Constants.FOLDER_OtherValidFolderRelativePath);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.fileInfo.SetupSet(fi => fi.FolderId = Constants.FOLDER_OtherValidFolderId).Verifiable();
            this.fileInfo.SetupSet(fi => fi.Folder = Constants.FOLDER_OtherValidFolderRelativePath).Verifiable();

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

			eventManager.LogDebug("MoveFile_Updates_FolderId_And_Folder(): fileContent → {FileContent}", fileContent);
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
            string someString;
            this.mockFileLockingController.Setup(flc => flc.IsFileLocked(this.fileInfo.Object, out someString)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.MoveVersions(this.fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));
            this.mockFileManager.Object.MoveFile(this.fileInfo.Object, this.folderInfo.Object);

            this.fileInfo.Verify();
			eventManager.LogDebug("<<< Exiting MoveFile_Updates_FolderId_And_Folder()");
        }

        [Test]
        public void MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder()
        {
			eventManager.LogDebug(">>> Entering MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder()");
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderID).Returns(Constants.FOLDER_OtherValidFolderId);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            var fileContent = new MemoryStream();

			eventManager.LogDebug("MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder(): fileContent → {FileContent}", fileContent);
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(fileContent);
            string someString;
            this.mockFileLockingController.Setup(flc => flc.IsFileLocked(this.fileInfo.Object, out someString)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.MoveVersions(this.fileInfo.Object, It.IsAny<IFolderInfo>(), It.IsAny<FolderProvider>(), It.IsAny<FolderProvider>()));

            var existingFile = new FileInfo();
            this.mockFileManager.Setup(mfm => mfm.GetFile(this.folderInfo.Object, Constants.FOLDER_ValidFileName, It.IsAny<bool>())).Returns(existingFile);
			eventManager.LogDebug("MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder(): existingFile → {ExistingFile}", existingFile);

            this.mockFileManager.Setup(mfm => mfm.DeleteFile(existingFile)).Verifiable();

            this.mockFileManager.Object.MoveFile(this.fileInfo.Object, this.folderInfo.Object);

            this.mockFileManager.Verify();
			eventManager.LogDebug("<<< Exiting MoveFile_Calls_DeleteFile_When_A_File_With_The_Same_Name_Exists_On_The_Destination_Folder()");
        }

        [Test]
        public void RenameFile_Throws_On_Null_File()
        {
			eventManager.LogDebug(">>> Entering RenameFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.RenameFile(null, It.IsAny<string>()));
			eventManager.LogDebug("<<< Exiting RenameFile_Throws_On_Null_File()");
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        public void RenameFile_Throws_On_Null_Or_Empty_NewFileName(string newFileName)
        {
			eventManager.LogDebug(">>> Entering RenameFile_Throws_On_Null_Or_Empty_NewFileName(newFileName={NewFileName})", newFileName);
            Assert.Throws<ArgumentException>(() => this.fileManager.RenameFile(this.fileInfo.Object, newFileName));
			eventManager.LogDebug("<<< Exiting RenameFile_Throws_On_Null_Or_Empty_NewFileName(newFileName={NewFileName})", newFileName);
        }

        [Test]
        public void RenameFile_Calls_FolderProvider_RenameFile_When_FileNames_Are_Distinct_And_NewFileName_Does_Not_Exist()
        {
			eventManager.LogDebug(">>> Entering RenameFile_Calls_FolderProvider_RenameFile_When_FileNames_Are_Distinct_And_NewFileName_Does_Not_Exist()");
            // Arrange
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);
            this.mockFileManager.Setup(mfm => mfm.FileExists(this.folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(this.fileInfo.Object));
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);
            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType, };
            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            // Act
            this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName);

            // Assert
            this.mockFolder.Verify(mf => mf.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName), Times.Once());
			eventManager.LogDebug("<<< Exiting RenameFile_Calls_FolderProvider_RenameFile_When_FileNames_Are_Distinct_And_NewFileName_Does_Not_Exist()");
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_FileNames_Are_Equal()
        {
			eventManager.LogDebug(">>> Entering RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_FileNames_Are_Equal()");
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);

            this.fileManager.RenameFile(this.fileInfo.Object, Constants.FOLDER_ValidFileName);

            this.mockFolder.Verify(mf => mf.RenameFile(this.fileInfo.Object, It.IsAny<string>()), Times.Never());
			eventManager.LogDebug("<<< Exiting RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_FileNames_Are_Equal()");
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_NewFileName_Exists()
        {
			eventManager.LogDebug(">>> Entering RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_NewFileName_Exists()");
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.mockFileManager.Setup(mfm => mfm.FileExists(this.folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);

            Assert.Throws<FileAlreadyExistsException>(() => this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName));
			eventManager.LogDebug("<<< Exiting RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_NewFileName_Exists()");
        }

        [Test]
        public void RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_InvalidExtensionType()
        {
			eventManager.LogDebug(">>> Entering RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_InvalidExtensionType()");
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);
            this.mockFileManager.Setup(fm => fm.IsAllowedExtension(It.IsAny<string>())).Returns(false);

            Assert.Throws<InvalidFileExtensionException>(() => this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherInvalidFileNameExtension));
			eventManager.LogDebug("<<< Exiting RenameFile_Does_Not_Call_FolderProvider_RenameFile_When_InvalidExtensionType()");
        }

        [Test]
        public void RenameFile_Throws_When_FolderProvider_Throws()
        {
			eventManager.LogDebug(">>> Entering RenameFile_Throws_When_FolderProvider_Throws()");
            this.fileInfo.Setup(fi => fi.FileName).Returns(Constants.FOLDER_ValidFileName);
            this.fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            this.fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            this.fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);

            this.folderManager.Setup(fm => fm.GetFolder(Constants.FOLDER_ValidFolderId)).Returns(this.folderInfo.Object);

            this.mockFileManager.Setup(mfm => mfm.FileExists(this.folderInfo.Object, Constants.FOLDER_OtherValidFileName, It.IsAny<bool>())).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.UpdateFile(this.fileInfo.Object));
            this.mockFileManager.Setup(mfm => mfm.IsAllowedExtension(Constants.FOLDER_OtherValidFileName)).Returns(true);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType, };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.mockFolder.Setup(mf => mf.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName)).Throws<Exception>();

            Assert.Throws<FolderProviderException>(() => this.mockFileManager.Object.RenameFile(this.fileInfo.Object, Constants.FOLDER_OtherValidFileName));
			eventManager.LogDebug("<<< Exiting RenameFile_Throws_When_FolderProvider_Throws()");
        }

        [Test]
        public void UnzipFile_Throws_On_Null_File()
        {
			eventManager.LogDebug(">>> Entering UnzipFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UnzipFile(null, It.IsAny<IFolderInfo>()));
			eventManager.LogDebug("<<< Exiting UnzipFile_Throws_On_Null_File()");
        }

        [Test]
        public void UnzipFile_Throws_On_Null_DestinationFolder()
        {
			eventManager.LogDebug(">>> Entering UnzipFile_Throws_On_Null_DestinationFolder()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UnzipFile(It.IsAny<IFileInfo>(), null));
			eventManager.LogDebug("<<< Exiting UnzipFile_Throws_On_Null_DestinationFolder()");
        }

        [Test]
        public void UnzipFile_Throws_When_File_Extension_Is_Not_Zip()
        {
			eventManager.LogDebug(">>> Entering UnzipFile_Throws_When_File_Extension_Is_Not_Zip()");
            this.fileInfo.Setup(fi => fi.Extension).Returns("txt");

            Assert.Throws<ArgumentNullException>(() => this.fileManager.UnzipFile(this.fileInfo.Object, It.IsAny<IFolderInfo>()));
			eventManager.LogDebug("<<< Exiting UnzipFile_Throws_When_File_Extension_Is_Not_Zip()");
        }

        [Test]
        public void UnzipFile_Calls_FileManager_ExtractFiles()
        {
			eventManager.LogDebug(">>> Entering UnzipFile_Calls_FileManager_ExtractFiles()");
            // Arrange
            this.fileInfo.Setup(fi => fi.Extension).Returns("zip");
            this.mockFileManager.Setup(mfm => mfm.ExtractFiles(this.fileInfo.Object, this.folderInfo.Object, null, true)).Verifiable();
            var stream = Constants.ValidZipFileContent;
            this.mockFileManager.Setup(mfm => mfm.GetFileContent(this.fileInfo.Object)).Returns(stream);

            // Act
            this.mockFileManager.Object.UnzipFile(this.fileInfo.Object, this.folderInfo.Object);

            // Assert
            this.mockFileManager.Verify();
			eventManager.LogDebug("<<< Exiting UnzipFile_Calls_FileManager_ExtractFiles()");
        }

        [Test]
        public void UpdateFile_Throws_On_Null_File()
        {
			eventManager.LogDebug(">>> Entering UpdateFile_Throws_On_Null_File()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UpdateFile(null));
			eventManager.LogDebug("<<< Exiting UpdateFile_Throws_On_Null_File()");
        }

        [Test]
        public void UpdateFile_Calls_DataProvider_UpdateFile()
        {
			eventManager.LogDebug(">>> Entering UpdateFile_Calls_DataProvider_UpdateFile()");
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));
            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object);

            this.mockData.Verify(
                md => md.UpdateFile(
                It.IsAny<int>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<bool>(),
                It.IsAny<int>()),
                Times.Once());
				eventManager.LogDebug("<<< Exiting UpdateFile_Calls_DataProvider_UpdateFile()");
        }

        [Test]
        public void UpdateFile_Throws_On_Null_File_Overload()
        {
			eventManager.LogDebug(">>> Entering UpdateFile_Throws_On_Null_File_Overload()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.UpdateFile(null, It.IsAny<Stream>()));
			eventManager.LogDebug("<<< Exiting UpdateFile_Throws_On_Null_File_Overload()");
        }

        [Test]
        public void UpdateFile_Sets_With_And_Height_When_File_Is_Image()
        {
			eventManager.LogDebug(">>> Entering UpdateFile_Sets_With_And_Height_When_File_Is_Image()");
            var image = new Bitmap(10, 20);

			eventManager.LogDebug("UpdateFile_Sets_With_And_Height_When_File_Is_Image(): image → {Image}", image);
            this.mockFileManager.Setup(mfm => mfm.IsImageFile(this.fileInfo.Object)).Returns(true);
            this.mockFileManager.Setup(mfm => mfm.GetImageFromStream(It.IsAny<Stream>())).Returns(image);
            this.mockFileManager.Setup(mfm => mfm.GetHash(this.fileInfo.Object));

            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

			eventManager.LogDebug("UpdateFile_Sets_With_And_Height_When_File_Is_Image(): stream → {Stream}", stream);
            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this.mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object, stream);

            this.fileInfo.VerifySet(fi => fi.Width = 10);
            this.fileInfo.VerifySet(fi => fi.Height = 20);
			eventManager.LogDebug("<<< Exiting UpdateFile_Sets_With_And_Height_When_File_Is_Image()");
        }

        [Test]
        public void UpdateFile_Sets_SHA1Hash()
        {
			eventManager.LogDebug(">>> Entering UpdateFile_Sets_SHA1Hash()");
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

			eventManager.LogDebug("UpdateFile_Sets_SHA1Hash(): stream → {Stream}", stream);
            this.mockFileManager.Setup(mfm => mfm.IsImageFile(this.fileInfo.Object)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.GetHash(stream)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this.mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object, stream);

            this.fileInfo.VerifySet(fi => fi.SHA1Hash = Constants.FOLDER_UnmodifiedFileHash);
			eventManager.LogDebug("<<< Exiting UpdateFile_Sets_SHA1Hash()");
        }

        [Test]
        public void UpdateFile_Calls_FileManager_UpdateFile_Overload()
        {
			eventManager.LogDebug(">>> Entering UpdateFile_Calls_FileManager_UpdateFile_Overload()");
            var bytes = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var stream = new MemoryStream(bytes);

			eventManager.LogDebug("UpdateFile_Calls_FileManager_UpdateFile_Overload(): stream → {Stream}", stream);
            this.mockFileManager.Setup(mfm => mfm.IsImageFile(this.fileInfo.Object)).Returns(false);
            this.mockFileManager.Setup(mfm => mfm.GetHash(this.fileInfo.Object)).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Parse(Constants.FOLDER_FileStartDate));

            this.folderMappingController.Setup(mp => mp.GetFolderMapping(It.IsAny<int>())).Returns(new FolderMappingInfo() { FolderProviderType = Constants.FOLDER_ValidFolderProviderType });
            this.mockFolder.Setup(fp => fp.GetHashCode(It.IsAny<IFileInfo>(), It.IsAny<Stream>())).Returns(Constants.FOLDER_UnmodifiedFileHash);

            this.mockFileManager.Object.UpdateFile(this.fileInfo.Object, stream);

            this.mockFileManager.Verify(mfm => mfm.UpdateFile(this.fileInfo.Object), Times.Once());
			eventManager.LogDebug("<<< Exiting UpdateFile_Calls_FileManager_UpdateFile_Overload()");
        }

        [Test]
        public void GetSeekableStream_Throws_On_Null_Stream()
        {
			eventManager.LogDebug(">>> Entering GetSeekableStream_Throws_On_Null_Stream()");
            Assert.Throws<ArgumentNullException>(() => this.fileManager.GetSeekableStream(null));
			eventManager.LogDebug("<<< Exiting GetSeekableStream_Throws_On_Null_Stream()");
        }

        [Test]
        public void GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable()
        {
			eventManager.LogDebug(">>> Entering GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable()");
            var inputStream = new MemoryStream();
            var seekableStream = this.fileManager.GetSeekableStream(inputStream);
			eventManager.LogDebug("GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable(): inputStream → {InputStream}", inputStream);
			eventManager.LogDebug("GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable(): seekableStream → {SeekableStream}", seekableStream);

            Assert.That(seekableStream, Is.EqualTo(inputStream));
			eventManager.LogDebug("<<< Exiting GetSeekableStream_Returns_The_Same_Stream_If_It_Is_Seekable()");
        }

        [Test]
        public void GetSeekableStream_Calls_GetHostMapPath_And_Creates_A_Temporary_FileStream_With_Resx_Extension()
        {
			eventManager.LogDebug(">>> Entering GetSeekableStream_Calls_GetHostMapPath_And_Creates_A_Temporary_FileStream_With_Resx_Extension()");
            var inputStream = new Mock<Stream>();
            inputStream.Setup(s => s.CanSeek).Returns(false);
			eventManager.LogDebug("GetSeekableStream_Calls_GetHostMapPath_And_Creates_A_Temporary_FileStream_With_Resx_Extension(): inputStream → {InputStream}", inputStream);
            inputStream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>())).Returns(0);

            this.mockFileManager.Setup(mfm => mfm.GetHostMapPath()).Returns(string.Empty).Verifiable();
            this.mockFileManager.Setup(mfm => mfm.GetAutoDeleteFileStream(It.Is((string x) => x.EndsWith(".resx")))).Returns(new MemoryStream()).Verifiable();

            this.mockFileManager.Object.GetSeekableStream(inputStream.Object);

            this.mockFileManager.Verify();
			eventManager.LogDebug("<<< Exiting GetSeekableStream_Calls_GetHostMapPath_And_Creates_A_Temporary_FileStream_With_Resx_Extension()");
        }

        private void PrepareFileSecurityCheck()
        {
			eventManager.LogDebug(">>> Entering PrepareFileSecurityCheck()");
            this.mockData.Setup(p => p.GetListEntriesByListName("FileSecurityChecker", string.Empty, Null.NullInteger)).Returns(() =>
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("EntryID", typeof(int));
				eventManager.LogDebug("PrepareFileSecurityCheck(): dataTable → {DataTable}", dataTable);
                dataTable.Columns.Add("ListName", typeof(string));
                dataTable.Columns.Add("Value", typeof(string));
                dataTable.Columns.Add("Text", typeof(string));
                dataTable.Columns.Add("Level", typeof(int));
                dataTable.Columns.Add("SortOrder", typeof(int));
                dataTable.Columns.Add("DefinitionID", typeof(int));
                dataTable.Columns.Add("ParentID", typeof(int));
                dataTable.Columns.Add("Description", typeof(string));
                dataTable.Columns.Add("PortalID", typeof(int));
                dataTable.Columns.Add("SystemList", typeof(bool));
                dataTable.Columns.Add("ParentKey", typeof(string));
                dataTable.Columns.Add("Parent", typeof(string));
                dataTable.Columns.Add("ParentList", typeof(string));
                dataTable.Columns.Add("MaxSortOrder", typeof(int));
                dataTable.Columns.Add("EntryCount", typeof(int));
                dataTable.Columns.Add("HasChildren", typeof(int));
                dataTable.Columns.Add("CreatedByUserID", typeof(int));
                dataTable.Columns.Add("CreatedOnDate", typeof(DateTime));
                dataTable.Columns.Add("LastModifiedByUserID", typeof(int));
                dataTable.Columns.Add("LastModifiedOnDate", typeof(DateTime));

                dataTable.Rows.Add(1, "FileSecurityChecker", "svg",
                    "DotNetNuke.Services.FileSystem.Internal.SecurityCheckers.SvgFileChecker, DotNetNuke",
                    0, 0, -1, -0, string.Empty, -1, 1, string.Empty, string.Empty, string.Empty, 0, 1, 0, -1, DateTime.Now, -1, DateTime.Now);

                return dataTable.CreateDataReader();
            });
            this.hostController.AddSetting("PerformanceSetting", "NoCaching");
            this.globals.Setup(g => g.HostMapPath).Returns(AppDomain.CurrentDomain.BaseDirectory);

            var folderMapping = new FolderMappingInfo { FolderProviderType = Constants.FOLDER_ValidFolderProviderType };

            this.folderMappingController.Setup(fmc => fmc.GetFolderMapping(Constants.CONTENT_ValidPortalId, Constants.FOLDER_ValidFolderMappingID)).Returns(folderMapping);

            this.folderInfo.Setup(fi => fi.PortalID).Returns(Constants.CONTENT_ValidPortalId);
            this.folderInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
			eventManager.LogDebug("<<< Exiting PrepareFileSecurityCheck()");
        }

        private class UnSeekableStream : MemoryStream
        {
            public override bool CanSeek
            {
                get { return false; }
            }
        }
    }
}
