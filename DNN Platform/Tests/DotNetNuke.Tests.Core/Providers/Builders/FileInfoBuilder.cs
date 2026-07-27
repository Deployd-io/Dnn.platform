// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Builders
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Tests.Utilities;

    internal class FileInfoBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private int fileId;
        private int portalId;
        private int folderId;
        private int contentItemId;
        private DateTime startDate;
        private DateTime endDate;
        private bool enablePublishPeriod;
        private int folderMappingID;

        internal FileInfoBuilder()
        {
			Logger.LogDebug(">>> Entering FileInfoBuilder()");
            this.fileId = Constants.FOLDER_ValidFileId;
            this.portalId = Constants.CONTENT_ValidPortalId;
			Logger.LogDebug("FileInfoBuilder(): this.fileId → {Arg0}", this.fileId);
			Logger.LogDebug("FileInfoBuilder(): portalId → {PortalId}", portalId);
			Logger.LogDebug("FileInfoBuilder(): this.portalId → {Arg0}", this.portalId);
            this.startDate = DateTime.Today;
            this.endDate = DateTime.MaxValue;
            this.enablePublishPeriod = false;
			Logger.LogDebug("FileInfoBuilder(): startDate → {StartDate}", startDate);
			Logger.LogDebug("FileInfoBuilder(): this.startDate → {Arg0}", this.startDate);
			Logger.LogDebug("FileInfoBuilder(): endDate → {EndDate}", endDate);
			Logger.LogDebug("FileInfoBuilder(): this.endDate → {Arg0}", this.endDate);
			Logger.LogDebug("FileInfoBuilder(): this.enablePublishPeriod → {Arg0}", this.enablePublishPeriod);
            this.contentItemId = Null.NullInteger;
            this.folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            this.folderId = Constants.FOLDER_ValidFolderId;
			Logger.LogDebug("<<< Exiting FileInfoBuilder()");
			Logger.LogDebug("FileInfoBuilder(): contentItemId → {ContentItemId}", contentItemId);
			Logger.LogDebug("FileInfoBuilder(): folderMappingID → {FolderMappingID}", folderMappingID);
			Logger.LogDebug("FileInfoBuilder(): this.folderMappingID → {Arg0}", this.folderMappingID);
			Logger.LogDebug("FileInfoBuilder(): folderId → {FolderId}", folderId);
			Logger.LogDebug("FileInfoBuilder(): this.folderId → {Arg0}", this.folderId);
			Logger.LogDebug("FileInfoBuilder(): fileId → {FileId}", fileId);
			Logger.LogDebug("FileInfoBuilder(): this.contentItemId → {Arg0}", this.contentItemId);
        }

        internal FileInfoBuilder WithFileId(int fileId)
        {
			Logger.LogDebug(">>> Entering WithFileId(fileId={FileId})", fileId);
            this.fileId = fileId;
			Logger.LogDebug("<<< Exiting WithFileId(fileId={FileId})", fileId);
            return this;
        }

        internal FileInfoBuilder WithContentItemId(int contentItemId)
        {
			Logger.LogDebug(">>> Entering WithContentItemId(contentItemId={ContentItemId})", contentItemId);
            this.contentItemId = contentItemId;
			Logger.LogDebug("WithContentItemId(contentItemId={ContentItemId}): this.contentItemId → {Arg1}", contentItemId, this.contentItemId);
			Logger.LogDebug("WithContentItemId(contentItemId={ContentItemId}): contentItemId → {ContentItemId1}", contentItemId, contentItemId);
			Logger.LogDebug("<<< Exiting WithContentItemId(contentItemId={ContentItemId})", contentItemId);
            return this;
        }

        internal FileInfoBuilder WithStartDate(DateTime startDate)
        {
            this.startDate = startDate;
			Logger.LogDebug("WithStartDate(startDate={StartDate}): this.startDate → {Arg1}", startDate, this.startDate);
			Logger.LogDebug("WithStartDate(startDate={StartDate}): startDate → {StartDate1}", startDate, startDate);
			Logger.LogDebug("<<< Exiting WithStartDate(startDate={StartDate})", startDate);
            return this;
        }

        internal FileInfoBuilder WithEndDate(DateTime endDate)
        {
			Logger.LogDebug(">>> Entering WithEndDate(endDate={EndDate})", endDate);
            this.endDate = endDate;
			Logger.LogDebug("WithEndDate(endDate={EndDate}): this.endDate → {Arg1}", endDate, this.endDate);
			Logger.LogDebug("WithEndDate(endDate={EndDate}): endDate → {EndDate1}", endDate, endDate);
			Logger.LogDebug("<<< Exiting WithEndDate(endDate={EndDate})", endDate);
            return this;
        }

        internal FileInfoBuilder WithEnablePublishPeriod(bool enablePublishPeriod)
        {
			Logger.LogDebug(">>> Entering WithEnablePublishPeriod(enablePublishPeriod={EnablePublishPeriod})", enablePublishPeriod);
            this.enablePublishPeriod = enablePublishPeriod;
			Logger.LogDebug("WithEnablePublishPeriod(enablePublishPeriod={EnablePublishPeriod}): this.enablePublishPeriod → {Arg1}", enablePublishPeriod, this.enablePublishPeriod);
			Logger.LogDebug("<<< Exiting WithEnablePublishPeriod(enablePublishPeriod={EnablePublishPeriod})", enablePublishPeriod);
            return this;
        }

        internal FileInfo Build()
        {
			Logger.LogDebug(">>> Entering Build()");
			Logger.LogDebug("<<< Exiting Build()");
            return new FileInfo
            {
                FileId = this.fileId,
                PortalId = this.portalId,
                StartDate = this.startDate,
                EnablePublishPeriod = this.enablePublishPeriod,
                EndDate = this.endDate,
                ContentItemID = this.contentItemId,
                FolderMappingID = this.folderMappingID,
                FolderId = this.folderId,
            };
        }
    }
}
