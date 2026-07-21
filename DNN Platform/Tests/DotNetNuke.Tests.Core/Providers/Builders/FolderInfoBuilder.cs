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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int portalId;
        private int folderId;
        private string folderPath;
        private string physicalPath;
        private int folderMappingID;

        internal FolderInfoBuilder()
        {
			Logger.LogDebug(">>> Entering FolderInfoBuilder()");
            this.portalId = Constants.CONTENT_ValidPortalId;
            this.folderPath = Constants.FOLDER_ValidFolderRelativePath;
			Logger.LogDebug("FolderInfoBuilder(): this.portalId → {Arg0}", this.portalId);
			Logger.LogDebug("FolderInfoBuilder(): this.folderPath → {Arg0}", this.folderPath);
            this.physicalPath = Constants.FOLDER_ValidFolderPath;
            this.folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            this.folderId = Constants.FOLDER_ValidFolderId;
			Logger.LogDebug("FolderInfoBuilder(): folderMappingID → {FolderMappingID}", folderMappingID);
			Logger.LogDebug("FolderInfoBuilder(): this.folderMappingID → {Arg0}", this.folderMappingID);
            this.physicalPath = string.Empty;
			Logger.LogDebug("<<< Exiting FolderInfoBuilder()");
			Logger.LogDebug("FolderInfoBuilder(): this.physicalPath → {Arg0}", this.physicalPath);
			Logger.LogDebug("FolderInfoBuilder(): folderId → {FolderId}", folderId);
			Logger.LogDebug("FolderInfoBuilder(): portalId → {PortalId}", portalId);
			Logger.LogDebug("FolderInfoBuilder(): this.folderId → {Arg0}", this.folderId);
        }

        internal FolderInfoBuilder WithPhysicalPath(string phisicalPath)
        {
			Logger.LogDebug(">>> Entering WithPhysicalPath(phisicalPath={PhisicalPath})", phisicalPath);
            this.physicalPath = phisicalPath;
			Logger.LogDebug("<<< Exiting WithPhysicalPath(phisicalPath={PhisicalPath})", phisicalPath);
            return this;
        }

        internal FolderInfoBuilder WithFolderId(int folderId)
        {
			Logger.LogDebug(">>> Entering WithFolderId(folderId={FolderId})", folderId);
            this.folderId = folderId;
			Logger.LogDebug("WithFolderId(folderId={FolderId}): this.folderId → {Arg1}", folderId, this.folderId);
			Logger.LogDebug("WithFolderId(folderId={FolderId}): folderId → {FolderId1}", folderId, folderId);
			Logger.LogDebug("<<< Exiting WithFolderId(folderId={FolderId})", folderId);
            return this;
        }

        internal IFolderInfo Build()
        {
			Logger.LogDebug(">>> Entering Build()");
            var mock = new Mock<IFolderInfo>();
            mock.Setup(f => f.FolderID).Returns(this.folderId);
			Logger.LogDebug("Build(): mock → {Mock}", mock);
            mock.Setup(f => f.PortalID).Returns(this.portalId);
            mock.Setup(f => f.FolderPath).Returns(this.folderPath);
            mock.Setup(f => f.PhysicalPath).Returns(this.physicalPath);
            mock.Setup(f => f.FolderMappingID).Returns(this.folderMappingID);

			Logger.LogDebug("<<< Exiting Build()");
            return mock.Object;
        }
    }
}
