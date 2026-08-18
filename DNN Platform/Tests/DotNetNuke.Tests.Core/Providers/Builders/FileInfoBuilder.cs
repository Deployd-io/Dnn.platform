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
			mockEventLogger.LogDebug(">>> Entering FileInfoBuilder()");
            this.fileId = Constants.FOLDER_ValidFileId;
            this.portalId = Constants.CONTENT_ValidPortalId;
			mockEventLogger.LogDebug("FileInfoBuilder(): this.fileId → {Arg0}", this.fileId);
			mockEventLogger.LogDebug("FileInfoBuilder(): portalId → {PortalId}", portalId);
			mockEventLogger.LogDebug("FileInfoBuilder(): this.portalId → {Arg0}", this.portalId);
            this.startDate = DateTime.Today;
            this.endDate = DateTime.MaxValue;
            this.enablePublishPeriod = false;
			mockEventLogger.LogDebug("FileInfoBuilder(): startDate → {StartDate}", startDate);
			mockEventLogger.LogDebug("FileInfoBuilder(): this.startDate → {Arg0}", this.startDate);
			mockEventLogger.LogDebug("FileInfoBuilder(): endDate → {EndDate}", endDate);
			mockEventLogger.LogDebug("FileInfoBuilder(): this.endDate → {Arg0}", this.endDate);
            this.contentItemId = Null.NullInteger;
            this.folderMappingID = Constants.FOLDER_ValidFolderMappingID;
            this.folderId = Constants.FOLDER_ValidFolderId;
			mockEventLogger.LogDebug("<<< Exiting FileInfoBuilder()");
        }

        internal FileInfoBuilder WithFileId(int fileId)
        {
			mockEventLogger.LogDebug(">>> Entering WithFileId(fileId={FileId})", fileId);
            this.fileId = fileId;
			mockEventLogger.LogDebug("<<< Exiting WithFileId(fileId={FileId})", fileId);
            return this;
        }

        internal FileInfoBuilder WithContentItemId(int contentItemId)
        {
			mockEventLogger.LogDebug(">>> Entering WithContentItemId(contentItemId={ContentItemId})", contentItemId);
            this.contentItemId = contentItemId;
			mockEventLogger.LogDebug("WithContentItemId(contentItemId={ContentItemId}): contentItemId → {ContentItemId1}", contentItemId, contentItemId);
			mockEventLogger.LogDebug("WithContentItemId(contentItemId={ContentItemId}): this.contentItemId → {Arg1}", contentItemId, this.contentItemId);
			mockEventLogger.LogDebug("<<< Exiting WithContentItemId(contentItemId={ContentItemId})", contentItemId);
            return this;
        }

        internal FileInfoBuilder WithStartDate(DateTime startDate)
        {
			mockEventLogger.LogDebug(">>> Entering WithStartDate(startDate={StartDate})", startDate);
            this.startDate = startDate;
			mockEventLogger.LogDebug("WithStartDate(startDate={StartDate}): startDate → {StartDate1}", startDate, startDate);
			mockEventLogger.LogDebug("WithStartDate(startDate={StartDate}): this.startDate → {Arg1}", startDate, this.startDate);
			mockEventLogger.LogDebug("<<< Exiting WithStartDate(startDate={StartDate})", startDate);
            return this;
        }

        internal FileInfoBuilder WithEndDate(DateTime endDate)
        {
			mockEventLogger.LogDebug(">>> Entering WithEndDate(endDate={EndDate})", endDate);
            this.endDate = endDate;
			mockEventLogger.LogDebug("WithEndDate(endDate={EndDate}): this.endDate → {Arg1}", endDate, this.endDate);
			mockEventLogger.LogDebug("WithEndDate(endDate={EndDate}): endDate → {EndDate1}", endDate, endDate);
			mockEventLogger.LogDebug("<<< Exiting WithEndDate(endDate={EndDate})", endDate);
            return this;
        }

        internal FileInfoBuilder WithEnablePublishPeriod(bool enablePublishPeriod)
        {
			mockEventLogger.LogDebug(">>> Entering WithEnablePublishPeriod(enablePublishPeriod={EnablePublishPeriod})", enablePublishPeriod);
            this.enablePublishPeriod = enablePublishPeriod;
			mockEventLogger.LogDebug("WithEnablePublishPeriod(enablePublishPeriod={EnablePublishPeriod}): this.enablePublishPeriod → {Arg1}", enablePublishPeriod, this.enablePublishPeriod);
			mockEventLogger.LogDebug("<<< Exiting WithEnablePublishPeriod(enablePublishPeriod={EnablePublishPeriod})", enablePublishPeriod);
            return this;
        }

        internal FileInfo Build()
        {
			mockEventLogger.LogDebug(">>> Entering Build()");
			mockEventLogger.LogDebug("<<< Exiting Build()");
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
