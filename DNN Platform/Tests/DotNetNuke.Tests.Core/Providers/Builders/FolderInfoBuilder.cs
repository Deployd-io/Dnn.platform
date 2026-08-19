// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Builders
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities;
    using Moq;

    internal class FolderInfoBuilder
    {
        private int portalId;
        private int folderId;
        private string folderPath;
        private string physicalPath;
        private int folderMappingID;

        internal FolderInfoBuilder()
        {
			mockEventLogger.LogDebug(">>> Entering FolderInfoBuilder()");
            this.portalId = Constants.CONTENT_ValidPortalId;
            this.folderPath = Constants.FOLDER_ValidFolderRelativePath;
			mockEventLogger.LogDebug("FolderInfoBuilder(): this.portalId → {Arg0}", this.portalId);
			mockEventLogger.LogDebug("FolderInfoBuilder(): this.folderPath → {Arg0}", this.folderPath);
            this.physicalPath = Constants.FOLDER_ValidFolderPath;
            this.folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            this.folderId = Constants.FOLDER_ValidFolderId;
			mockEventLogger.LogDebug("FolderInfoBuilder(): folderMappingID → {FolderMappingID}", folderMappingID);
            this.physicalPath = string.Empty;
			mockEventLogger.LogDebug("FolderInfoBuilder(): this.physicalPath → {Arg0}", this.physicalPath);
			mockEventLogger.LogDebug("<<< Exiting FolderInfoBuilder()");
        }

        internal FolderInfoBuilder WithPhysicalPath(string phisicalPath)
        {
			mockEventLogger.LogDebug(">>> Entering WithPhysicalPath(phisicalPath={PhisicalPath})", phisicalPath);
            this.physicalPath = phisicalPath;
			mockEventLogger.LogDebug("<<< Exiting WithPhysicalPath(phisicalPath={PhisicalPath})", phisicalPath);
            return this;
        }

        internal FolderInfoBuilder WithFolderId(int folderId)
        {
			mockEventLogger.LogDebug(">>> Entering WithFolderId(folderId={FolderId})", folderId);
            this.folderId = folderId;
			mockEventLogger.LogDebug("WithFolderId(folderId={FolderId}): this.folderId → {Arg1}", folderId, this.folderId);
			mockEventLogger.LogDebug("WithFolderId(folderId={FolderId}): folderId → {FolderId1}", folderId, folderId);
			mockEventLogger.LogDebug("<<< Exiting WithFolderId(folderId={FolderId})", folderId);
            return this;
        }

        internal IFolderInfo Build()
        {
			mockEventLogger.LogDebug(">>> Entering Build()");
            var mock = new Mock<IFolderInfo>();
            mock.Setup(f => f.FolderID).Returns(this.folderId);
			mockEventLogger.LogDebug("Build(): mock → {Mock}", mock);
            mock.Setup(f => f.PortalID).Returns(this.portalId);
            mock.Setup(f => f.FolderPath).Returns(this.folderPath);
            mock.Setup(f => f.PhysicalPath).Returns(this.physicalPath);
            mock.Setup(f => f.FolderMappingID).Returns(this.folderMappingID);

			mockEventLogger.LogDebug("<<< Exiting Build()");
            return mock.Object;
        }
    }
}
