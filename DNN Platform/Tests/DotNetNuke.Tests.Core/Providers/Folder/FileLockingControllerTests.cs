// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Core.Providers.Builders;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FileLockingControllerTests
    {
        private Mock<IWorkflowEngine> mockWorkFlowEngine;
        private Mock<IUserSecurityController> mockUserSecurityController;
        private FileLockingController fileLockingController;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            this.mockWorkFlowEngine = new Mock<IWorkflowEngine>();
            this.mockUserSecurityController = new Mock<IUserSecurityController>();
			mockEventLogger.LogDebug("Setup(): mockUserSecurityController → {MockUserSecurityController}", mockUserSecurityController);
			mockEventLogger.LogDebug("Setup(): mockWorkFlowEngine → {MockWorkFlowEngine}", mockWorkFlowEngine);

            WorkflowEngine.SetTestableInstance(this.mockWorkFlowEngine.Object);
            UserSecurityController.SetTestableInstance(this.mockUserSecurityController.Object);

            this.fileLockingController = new FileLockingController();
			mockEventLogger.LogDebug("Setup(): fileLockingController → {FileLockingController}", fileLockingController);
			mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            UserSecurityController.ClearInstance();
            MockComponentProvider.ResetContainer();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void IsFileLocked_ReturnsTrue_WhenPublishPeriodIsOut()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileLocked_ReturnsTrue_WhenPublishPeriodIsOut()");
            // Arrange
            ////_fileInfo.Setup(fi => fi.PortalId).Returns(Constants.CONTENT_ValidPortalId);
            ////_fileInfo.Setup(fi => fi.FolderId).Returns(Constants.FOLDER_ValidFolderId);
            ////_fileInfo.Setup(fi => fi.FolderMappingID).Returns(Constants.FOLDER_ValidFolderMappingID);
            ////_fileInfo.Setup(fi => fi.EnablePublishPeriod).Returns(true);
            ////_fileInfo.Setup(fi => fi.StartDate).Returns(DateTime.Today.AddDays(-2));
            ////_fileInfo.Setup(fi => fi.EndDate).Returns(DateTime.Today.AddDays(-1));
            ////_fileInfo.Setup(fi => fi.ContentItemID).Returns(Null.NullInteger);
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = this.fileLockingController.IsFileLocked(fileInfo, out someReason);
			mockEventLogger.LogDebug("IsFileLocked_ReturnsTrue_WhenPublishPeriodIsOut(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.True);
                Assert.That(someReason, Is.EqualTo("FileLockedOutOfPublishPeriodError"));
            }
			mockEventLogger.LogDebug("<<< Exiting IsFileLocked_ReturnsTrue_WhenPublishPeriodIsOut()");
        }

        [Test]
        public void IsFileLocked_ReturnsTrue_WhenWorkflowIsNotComplete()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileLocked_ReturnsTrue_WhenWorkflowIsNotComplete()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .WithContentItemId(It.IsAny<int>())
                .Build();

            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);
            this.mockWorkFlowEngine.Setup(mwc => mwc.IsWorkflowCompleted(It.IsAny<int>())).Returns(false);

            // Act
            string someReason;
            var result = this.fileLockingController.IsFileLocked(fileInfo, out someReason);
			mockEventLogger.LogDebug("IsFileLocked_ReturnsTrue_WhenWorkflowIsNotComplete(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.True);
                Assert.That(someReason, Is.EqualTo("FileLockedRunningWorkflowError"));
            }
			mockEventLogger.LogDebug("<<< Exiting IsFileLocked_ReturnsTrue_WhenWorkflowIsNotComplete()");
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenUserIsHostOrAdmin()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileLocked_ReturnsFalse_WhenUserIsHostOrAdmin()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(true);

            // Act
            string someReason;
            var result = this.fileLockingController.IsFileLocked(fileInfo, out someReason);
			mockEventLogger.LogDebug("IsFileLocked_ReturnsFalse_WhenUserIsHostOrAdmin(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileLocked_ReturnsFalse_WhenUserIsHostOrAdmin()");
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodIsIn()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileLocked_ReturnsFalse_WhenPublishPeriodIsIn()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = this.fileLockingController.IsFileLocked(fileInfo, out someReason);
			mockEventLogger.LogDebug("IsFileLocked_ReturnsFalse_WhenPublishPeriodIsIn(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileLocked_ReturnsFalse_WhenPublishPeriodIsIn()");
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodHasNotEndDate()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileLocked_ReturnsFalse_WhenPublishPeriodHasNotEndDate()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = this.fileLockingController.IsFileLocked(fileInfo, out someReason);
			mockEventLogger.LogDebug("IsFileLocked_ReturnsFalse_WhenPublishPeriodHasNotEndDate(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileLocked_ReturnsFalse_WhenPublishPeriodHasNotEndDate()");
        }

        [Test]
        public void IsFileLocked_ReturnsFalse_WhenPublishPeriodIsDisabled()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileLocked_ReturnsFalse_WhenPublishPeriodIsDisabled()");
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();
			mockEventLogger.LogDebug("IsFileLocked_ReturnsFalse_WhenPublishPeriodIsDisabled(): fileInfo → {FileInfo}", fileInfo);
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(Constants.CONTENT_ValidPortalId)).Returns(false);

            // Act
            string someReason;
            var result = this.fileLockingController.IsFileLocked(fileInfo, out someReason);
			mockEventLogger.LogDebug("IsFileLocked_ReturnsFalse_WhenPublishPeriodIsDisabled(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileLocked_ReturnsFalse_WhenPublishPeriodIsDisabled()");
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsTrue_WhenPublishPeriodIsOut()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileOutOfPublishPeriod_ReturnsTrue_WhenPublishPeriodIsOut()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = this.fileLockingController.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());
			mockEventLogger.LogDebug("IsFileOutOfPublishPeriod_ReturnsTrue_WhenPublishPeriodIsOut(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.True);
			mockEventLogger.LogDebug("<<< Exiting IsFileOutOfPublishPeriod_ReturnsTrue_WhenPublishPeriodIsOut()");
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenUserIsHostOrAdmin()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileOutOfPublishPeriod_ReturnsFalse_WhenUserIsHostOrAdmin()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithStartDate(DateTime.Today.AddDays(-2))
                .WithEndDate(DateTime.Today.AddDays(-1))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(true);

            // Act
            var result = this.fileLockingController.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());
			mockEventLogger.LogDebug("IsFileOutOfPublishPeriod_ReturnsFalse_WhenUserIsHostOrAdmin(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileOutOfPublishPeriod_ReturnsFalse_WhenUserIsHostOrAdmin()");
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsIn()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsIn()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = this.fileLockingController.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());
			mockEventLogger.LogDebug("IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsIn(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsIn()");
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodHasNotEndDate()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodHasNotEndDate()");
            // Arrange
            var fileInfo = new FileInfoBuilder()
                .WithEndDate(DateTime.Today.AddDays(2))
                .WithEnablePublishPeriod(true)
                .Build();
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = this.fileLockingController.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());
			mockEventLogger.LogDebug("IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodHasNotEndDate(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodHasNotEndDate()");
        }

        [Test]
        public void IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsDisabled()
        {
			mockEventLogger.LogDebug(">>> Entering IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsDisabled()");
            // Arrange
            var fileInfo = new FileInfoBuilder().Build();
			mockEventLogger.LogDebug("IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsDisabled(): fileInfo → {FileInfo}", fileInfo);
            this.mockUserSecurityController.Setup(msc => msc.IsHostAdminUser(It.IsAny<int>(), It.IsAny<int>())).Returns(false);

            // Act
            var result = this.fileLockingController.IsFileOutOfPublishPeriod(fileInfo, It.IsAny<int>(), It.IsAny<int>());
			mockEventLogger.LogDebug("IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsDisabled(): result → {Result}", result);

            // Assert
            Assert.That(result, Is.False);
			mockEventLogger.LogDebug("<<< Exiting IsFileOutOfPublishPeriod_ReturnsFalse_WhenPublishPeriodIsDisabled()");
        }
    }
}
