// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Assets;
    using Dnn.ExportImport.Dto.Workflow;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

    /// <summary>An export service for assets.</summary>
    public class AssetsExportService : BasePortableService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string DefaultUsersFoldersPath = "Users";

        private const string UsersAssetsTempFolder = "{0}\\TempUsers\\";

        private static readonly Regex UserFolderEx = new Regex(
            @"users/\d+/\d+/(\d+)/",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IHostSettings hostSettings;
        private readonly string assetsFolder;

        /// <summary>Initializes a new instance of the <see cref="AssetsExportService"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public AssetsExportService()
            : this(null)
        {
		Logger.LogDebug(">>> Entering AssetsExportService()");
        }
		Logger.LogDebug("<<< Exiting AssetsExportService()");

        /// <summary>Initializes a new instance of the <see cref="AssetsExportService"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public AssetsExportService(IApplicationStatusInfo appStatus)
            : this(appStatus, null)
        {
		Logger.LogDebug(">>> Entering AssetsExportService(appStatus={AppStatus})", appStatus);
		Logger.LogDebug("<<< Exiting AssetsExportService(appStatus={AppStatus})", appStatus);
        }

        /// <summary>Initializes a new instance of the <see cref="AssetsExportService"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        public AssetsExportService(IApplicationStatusInfo appStatus, IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            appStatus ??= Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			Logger.LogDebug("AssetsExportService(appStatus={AppStatus},hostSettings={HostSettings}): hostSettings → {HostSettings1}", appStatus, hostSettings, hostSettings);
            this.assetsFolder = $"{appStatus.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipFiles}";
			Logger.LogDebug("AssetsExportService(appStatus={AppStatus},hostSettings={HostSettings}): this.assetsFolder → {Arg2}", appStatus, hostSettings, this.assetsFolder);
        }

        /// <inheritdoc />
        public override string Category => Constants.Category_Assets;

        /// <inheritdoc />
        public override string ParentCategory => null;

        /// <inheritdoc />
        public override uint Priority => 50;

        /// <inheritdoc />
        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
			Logger.LogDebug(">>> Entering ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            // Skip the export if all the folders have been processed already.
            if (this.CheckPoint.Stage >= 1)
            {
                return;
            }

            // Create Zip File to hold files
            var skip = this.GetCurrentSkip();
            var currentIndex = skip;
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): skip → {Skip}", exportJob, exportDto, skip);
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): currentIndex → {CurrentIndex}", exportJob, exportDto, currentIndex);
            var totalFolderExported = 0;
            var totalFolderPermissionsExported = 0;
            var totalFilesExported = 0;
            var portalId = exportJob.PortalId;
            try
            {
                var assetsFile = string.Format(CultureInfo.InvariantCulture, this.assetsFolder, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));

				Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): assetsFile → {AssetsFile}", exportJob, exportDto, assetsFile);
                if (this.CheckPoint.Stage == 0)
                {
                    var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
                    var toDate = exportDto.ToDateUtc.ToLocalTime();
					Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): fromDate → {FromDate}", exportJob, exportDto, fromDate);
					Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): toDate → {ToDate}", exportJob, exportDto, toDate);
                    var portal = PortalController.Instance.GetPortal(portalId);

					Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): portal → {Portal}", exportJob, exportDto, portal);
                    var folders =
                        CBO.FillCollection<ExportFolder>(DataProvider.Instance()
                            .GetFolders(portalId, toDate, fromDate)).ToList();
							Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): folders → {Folders}", exportJob, exportDto, folders);
                    var totalFolders = folders.Count != 0 ? folders.Count : 0;
                    folders = folders.Skip(skip).ToList();

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalFolders : this.CheckPoint.TotalItems;
                    this.CheckPoint.ProcessedItems = skip;
                    this.CheckPoint.Progress = this.CheckPoint.TotalItems > 0 ? skip * 100.0 / this.CheckPoint.TotalItems : 0;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    using (var zipArchive = CompressionUtil.OpenCreate(assetsFile))
                    {
                        foreach (var folder in folders)
                        {
                            if (this.CheckCancelled(exportJob))
                            {
                                break;
                            }

                            var isUserFolder = false;

							Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): isUserFolder → {IsUserFolder}", exportJob, exportDto, isUserFolder);
                            var files =
                                CBO.FillCollection<ExportFile>(
                                    DataProvider.Instance()
                                        .GetFiles(portalId, folder.FolderId, toDate, fromDate)).Where(x => x.Extension != Constants.TemplatesExtension).ToList();
                            if (IsUserFolder(folder.FolderPath, out var userId))
                            {
                                isUserFolder = true;
                                folder.UserId = userId;
								Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): folder.UserId → {Arg2}", exportJob, exportDto, folder.UserId);
                                folder.Username =
                                    UserController.GetUserById(this.hostSettings, portalId, userId)?.Username;
                            }

                            if (folder.ParentId is > 0)
                            {
                                // If parent id exists then change the parent folder id to parent id.
                                folder.ParentId = this.Repository.GetItem<ExportFolder>(x => x.FolderId == folder.ParentId)?.Id;
                            }

                            this.Repository.CreateItem(folder, null);
							Logger.LogInformation("ExportData(exportJob,exportDto)={ExportJob},{ExportDto}: database query executed in {Arg2} ms", exportJob, exportDto, __logStart.ElapsedMilliseconds);
                            totalFolderExported++;

                            // Include permissions only if IncludePermissions=true
                            if (exportDto.IncludePermissions)
                            {
                                var permissions =
                                    CBO.FillCollection<ExportFolderPermission>(DataProvider.Instance()
                                        .GetFolderPermissionsByPath(portalId, folder.FolderPath, toDate, fromDate));
                                this.Repository.CreateItems(permissions, folder.Id);
                                totalFolderPermissionsExported += permissions.Count;
                            }

                            this.Repository.CreateItems(files, folder.Id);
                            totalFilesExported += files.Count;
                            var folderOffset = portal.HomeDirectoryMapPath.Length + (portal.HomeDirectoryMapPath.EndsWith(@"\", StringComparison.Ordinal) ? 0 : 1);

							Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): folderOffset → {FolderOffset}", exportJob, exportDto, folderOffset);
                            if (folder.StorageLocation != (int)FolderController.StorageLocationTypes.DatabaseSecure)
                            {
                                CompressionUtil.AddFilesToArchive(
                                    zipArchive,
                                    files.Select(
                                        file => portal.HomeDirectoryMapPath
												Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): file → {File}", exportJob, exportDto, file);
                                                + folder.FolderPath
                                                + GetActualFileName(file)),
                                    folderOffset,
                                    isUserFolder ? "TempUsers" : null);
                            }

                            this.CheckPoint.ProcessedItems++;
                            this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalFolders;
                            this.CheckPoint.StageData = null;
                            currentIndex++;

                            // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                            if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                            {
                                return;
                            }

                            this.Repository.RebuildIndex<ExportFolder>(x => x.Id, true);
                            this.Repository.RebuildIndex<ExportFolder>(x => x.UserId);
                            this.Repository.RebuildIndex<ExportFile>(x => x.ReferenceId);
                        }
                    }

                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Stage++;
                    currentIndex = 0;
                    this.CheckPoint.Progress = 100;
                }
            }
            finally
            {
                this.CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex }) : null;
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Folders", totalFolderExported.ToString(CultureInfo.InvariantCulture));
                this.Result.AddSummary("Exported Folder Permissions", totalFolderPermissionsExported.ToString(CultureInfo.InvariantCulture));
                this.Result.AddSummary("Exported Files", totalFilesExported.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <inheritdoc />
        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
			Logger.LogDebug(">>> Entering ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Stage 1: Portals files unzipped.
            // Stage 2: All folders and files imported.
            // Stage 3: Synchronization completed.
            // Skip the export if all the folders have been processed already.
            if (this.CheckPoint.Stage >= 2 || this.CheckPoint.Completed)
            {
                return;
            }

            var totalFolderImported = 0;
            var totalFolderPermissionsImported = 0;
            var totalFilesImported = 0;
            var skip = this.GetCurrentSkip();
            var currentIndex = skip;
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): skip → {Skip}", importJob, importDto, skip);
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): currentIndex → {CurrentIndex}", importJob, importDto, currentIndex);
            var portalId = importJob.PortalId;
            var portal = PortalController.Instance.GetPortal(portalId);
            var assetsFile = string.Format(CultureInfo.InvariantCulture, this.assetsFolder, importJob.Directory.TrimEnd('\\').TrimEnd('/'));
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): portal → {Portal}", importJob, importDto, portal);
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): assetsFile → {AssetsFile}", importJob, importDto, assetsFile);
            var userFolderPath = string.Format(CultureInfo.InvariantCulture, UsersAssetsTempFolder, portal.HomeDirectoryMapPath.TrimEnd('\\'));
            if (this.CheckPoint.Stage == 0)
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): userFolderPath → {UserFolderPath}", importJob, importDto, userFolderPath);
            {
                if (!File.Exists(assetsFile))
                {
                    this.Result.AddLogEntry(
                        "AssetsFileNotFound",
                        "Assets file not found. Skipping assets import",
                        ReportLevel.Warn);
                    this.CheckPoint.Completed = true;
                    this.CheckPointStageCallback(this);
                }
                else
                {
                    CompressionUtil.UnZipArchive(
                        assetsFile,
                        portal.HomeDirectoryMapPath,
                        importDto.CollisionResolution == CollisionResolution.Overwrite);

                    // Stage 1: Once unzipping of portal files is completed.
                    this.CheckPoint.Stage++;
                    this.CheckPoint.StageData = null;
                    this.CheckPoint.Progress = 10;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }
                }
            }

            if (this.CheckPoint.Stage == 1)
            {
                try
                {
                    // Stage 2 starts
                    var sourceFolders = this.Repository.GetAllItems<ExportFolder>(x => x.CreatedOnDate, true, skip).ToList();

					Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): sourceFolders → {SourceFolders}", importJob, importDto, sourceFolders);
                    var totalFolders = sourceFolders.Count != 0 ? sourceFolders.Count : 0;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalFolders : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    foreach (var sourceFolder in sourceFolders)
                    {
                        if (this.CheckCancelled(importJob))
                        {
                            break;
                        }

                        // PROCESS FOLDERS
                        // Create new or update existing folder
                        if (this.ProcessFolder(importJob, importDto, sourceFolder))
                        {
                            totalFolderImported++;

                            // Include permissions only if permissions were exported in package.
                            if (importDto.ExportDto.IncludePermissions)
                            {
                                // PROCESS FOLDER PERMISSIONS
                                var sourceFolderPermissions =
                                    this.Repository.GetRelatedItems<ExportFolderPermission>(sourceFolder.Id).ToList();

                                // Replace folderId for each permission with new one.
                                sourceFolderPermissions.ForEach(x =>
                                {
                                    x.FolderId = Convert.ToInt32(sourceFolder.FolderId);
                                    x.FolderPath = sourceFolder.FolderPath;
									Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): x.FolderPath → {Arg2}", importJob, importDto, x.FolderPath);
                                });

                                // PROCESS FOLDER PERMISSIONS
                                // File local files in the system related to the folder path.
                                var localPermissions = CBO.FillCollection<ExportFolderPermission>(
                                    DataProvider.Instance()
                                        .GetFolderPermissionsByPath(
                                            portalId,
                                            sourceFolder.FolderPath,
                                            DateUtils.GetDatabaseUtcTime()
                                                .AddYears(1),
                                            null));

                                foreach (var folderPermission in sourceFolderPermissions)
                                {
                                    ProcessFolderPermission(
                                        importJob,
                                        importDto,
                                        folderPermission,
                                        localPermissions);
                                }

                                totalFolderPermissionsImported += sourceFolderPermissions.Count;
                            }

                            // PROCESS FILES
                            var sourceFiles =
                                this.Repository.GetRelatedItems<ExportFile>(sourceFolder.Id).ToList();

                            // Replace folderId for each file with new one.
                            sourceFiles.ForEach(x =>
                            {
                                x.FolderId = Convert.ToInt32(sourceFolder.FolderId);
                                x.Folder = sourceFolder.FolderPath;
								Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): x.Folder → {Arg2}", importJob, importDto, x.Folder);
                            });

                            // File local files in the system related to the folder
                            var localFiles = CBO.FillCollection<ExportFile>(
                                DataProvider.Instance()
                                    .GetFiles(
                                        portalId,
                                        sourceFolder.FolderId,
                                        DateUtils.GetDatabaseUtcTime()
                                            .AddYears(1),
                                        null));

                            foreach (var file in sourceFiles)
                            {
                                ProcessFiles(importJob, importDto, file, localFiles);
                            }

                            totalFilesImported += sourceFiles.Count;
                        }

                        currentIndex++;
                        this.CheckPoint.ProcessedItems++;
                        this.CheckPoint.Progress = 10 + (this.CheckPoint.ProcessedItems * 90.0 / totalFolders);

                        // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                        {
                            return;
                        }
                    }

                    currentIndex = 0;
                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Stage++;
                    this.CheckPoint.Progress = 100;
                }
                finally
                {
                    this.CheckPoint.StageData = currentIndex > 0
                        ? JsonConvert.SerializeObject(new { skip = currentIndex })
                        : null;
                    this.CheckPointStageCallback(this);

                    this.Result.AddSummary("Imported Folders", totalFolderImported.ToString(CultureInfo.InvariantCulture));
                    this.Result.AddSummary("Imported Folder Permissions", totalFolderPermissionsImported.ToString(CultureInfo.InvariantCulture));
                    this.Result.AddSummary("Imported Files", totalFilesImported.ToString(CultureInfo.InvariantCulture));

                    if (Directory.Exists(userFolderPath) && currentIndex == 0)
                    {
                        Directory.Delete(userFolderPath, true);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override int GetImportTotal()
        {
			Logger.LogDebug(">>> Entering GetImportTotal()");
			Logger.LogDebug("<<< Exiting GetImportTotal()");
            return this.Repository.GetCount<ExportFolder>();
        }

        private static bool IsUserFolder(string folderPath, out int userId)
        {
			Logger.LogDebug(">>> Entering IsUserFolder(folderPath={FolderPath})", folderPath);
            userId = -1;
            var match = UserFolderEx.Match(folderPath);
			Logger.LogDebug("IsUserFolder(folderPath={FolderPath}): userId → {UserId}", folderPath, userId);
			Logger.LogDebug("IsUserFolder(folderPath={FolderPath}): match → {Match}", folderPath, match);
            if (match.Success)
            {
                userId = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            }

			Logger.LogDebug("<<< Exiting IsUserFolder(folderPath={FolderPath})", folderPath);
            return match.Success;
        }

        private static void ProcessFolderPermission(ExportImportJob importJob, ImportDto importDto, ExportFolderPermission folderPermission, IEnumerable<ExportFolderPermission> localPermissions)
        {
            var portalId = importJob.PortalId;
            var noRole = Convert.ToInt32(Globals.glbRoleNothing, CultureInfo.InvariantCulture);
            if (folderPermission == null)
            {
                return;
            }

            var roleId = Util.GetRoleIdByName(portalId, folderPermission.RoleId ?? noRole, folderPermission.RoleName);
            var userId = UserController.GetUserByName(portalId, folderPermission.Username)?.UserID;

            var existingFolderPermission = localPermissions.FirstOrDefault(
                x =>
                    (x.FolderPath == folderPermission.FolderPath ||
                     (string.IsNullOrEmpty(x.FolderPath) && string.IsNullOrEmpty(folderPermission.FolderPath))) &&
                    x.PermissionCode == folderPermission.PermissionCode && x.PermissionKey == folderPermission.PermissionKey
                    && x.PermissionName.Equals(folderPermission.PermissionName, StringComparison.OrdinalIgnoreCase) &&
                    x.RoleId == roleId && x.UserId == userId);

            var isUpdate = false;
            if (existingFolderPermission != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }

            if (isUpdate)
            {
                var modifiedBy = Util.GetUserIdByName(
                    importJob,
                    folderPermission.LastModifiedByUserId,
                    folderPermission.LastModifiedByUserName);

                DotNetNuke.Data.DataProvider.Instance()
                    .UpdateFolderPermission(
                        existingFolderPermission.FolderPermissionId,
                        folderPermission.FolderId,
                        existingFolderPermission.PermissionId,
                        existingFolderPermission.RoleId ?? Convert.ToInt32(Globals.glbRoleNothing, CultureInfo.InvariantCulture),
                        folderPermission.AllowAccess,
                        existingFolderPermission.UserId ?? Null.NullInteger,
                        modifiedBy);

                folderPermission.FolderPermissionId = existingFolderPermission.FolderPermissionId;
            }
            else
            {
                var permissionId = DataProvider.Instance()
                    .GetPermissionId(
                        folderPermission.PermissionCode,
                        folderPermission.PermissionKey,
                        folderPermission.PermissionName);

                if (permissionId != null)
                {
                    folderPermission.PermissionId = permissionId.Value;
                    if (folderPermission.UserId is > 0 && !string.IsNullOrEmpty(folderPermission.Username))
                    {
                        folderPermission.UserId = userId;
                        if (folderPermission.UserId == null)
                        {
                            return;
                        }
                    }

                    if (folderPermission.RoleId != null && folderPermission.RoleId > noRole && !string.IsNullOrEmpty(folderPermission.RoleName))
                    {
                        folderPermission.RoleId = roleId;
                        if (folderPermission.RoleId == null)
                        {
                            return;
                        }
                    }

                    var createdBy = Util.GetUserIdByName(
                        importJob,
                        folderPermission.CreatedByUserId,
                        folderPermission.CreatedByUserName);

                    folderPermission.FolderPermissionId = DotNetNuke.Data.DataProvider.Instance()
                        .AddFolderPermission(
                            folderPermission.FolderId,
                            folderPermission.PermissionId,
                            folderPermission.RoleId ?? noRole,
                            folderPermission.AllowAccess,
                            folderPermission.UserId ?? Null.NullInteger,
                            createdBy);
                }
            }

            folderPermission.LocalId = folderPermission.FolderPermissionId;
			Logger.LogDebug("<<< Exiting ProcessFolderPermission(importJob={ImportJob},importDto={ImportDto},folderPermission={FolderPermission},localPermissions={LocalPermissions})", importJob, importDto, folderPermission, localPermissions);
        }

        private static void ProcessFiles(ExportImportJob importJob, ImportDto importDto, ExportFile file, IEnumerable<ExportFile> localFiles)
        {
            if (file == null)
            {
                return;
            }

            var existingFile = localFiles.FirstOrDefault(x => x.FileName == file.FileName);
            var isUpdate = false;
            if (existingFile != null)
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        return;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }

            if (isUpdate)
            {
                var modifiedBy = Util.GetUserIdByName(importJob, file.LastModifiedByUserId, file.LastModifiedByUserName);
                file.FileId = existingFile.FileId;
                DotNetNuke.Data.DataProvider.Instance()
                    .UpdateFile(
                        existingFile.FileId,
                        file.VersionGuid,
                        file.FileName,
                        file.Extension,
                        file.Size,
                        file.Width ?? Null.NullInteger,
                        file.Height ?? Null.NullInteger,
                        file.ContentType,
                        file.FolderId,
                        modifiedBy,
                        file.Sha1Hash,
                        DateUtils.GetDatabaseLocalTime(),
                        file.Title,
                        file.Description,
                        file.StartDate,
                        file.EndDate ?? Null.NullDate,
                        file.EnablePublishPeriod,
                        existingFile.ContentItemId ?? Null.NullInteger);

                if ((file.Content != null && existingFile.Content == null) ||
                    (existingFile.Content != null && file.Content == null) ||
                    (file.Content != null && existingFile.Content != null &&
                     file.Content.SequenceEqual(existingFile.Content)))
                {
                    DotNetNuke.Data.DataProvider.Instance().UpdateFileContent(file.FileId, file.Content);
                }
            }
            else
            {
                var createdBy = Util.GetUserIdByName(importJob, file.CreatedByUserId, file.CreatedByUserName);
                file.FileId = DotNetNuke.Data.DataProvider.Instance()
                    .AddFile(
                        importJob.PortalId,
                        Guid.NewGuid(),
                        file.VersionGuid,
                        file.FileName,
                        file.Extension,
                        file.Size,
                        file.Width ?? Null.NullInteger,
                        file.Height ?? Null.NullInteger,
                        file.ContentType,
                        file.Folder,
                        file.FolderId,
                        createdBy,
                        file.Sha1Hash,
                        DateUtils.GetDatabaseLocalTime(),
                        file.Title,
                        file.Description,
                        file.StartDate,
                        file.EndDate ?? Null.NullDate,
                        file.EnablePublishPeriod,
                        Null.NullInteger);
                        ////file.ContentItemId ?? Null.NullInteger);--If we keep it we will see FK_PK relationship errors.

                if (file.Content != null)
                {
                    DotNetNuke.Data.DataProvider.Instance().UpdateFileContent(file.FileId, file.Content);
                }
            }
			Logger.LogDebug("<<< Exiting ProcessFiles(importJob={ImportJob},importDto={ImportDto},file={File},localFiles={LocalFiles})", importJob, importDto, file, localFiles);
        }

        private static void SyncUserFolder(int portalId, ExportFolder folder)
        {
            var portal = PortalController.Instance.GetPortal(portalId);
            var tempUsersFolderPath =
                string.Format(CultureInfo.InvariantCulture, UsersAssetsTempFolder, portal.HomeDirectoryMapPath.TrimEnd('\\')) + folder.FolderPath;
            var newUsersFolderPath = $"{portal.HomeDirectoryMapPath}{folder.FolderPath}";
            if (!Directory.Exists(tempUsersFolderPath))
            {
                return;
            }

            if (!Directory.Exists(newUsersFolderPath))
            {
                Directory.CreateDirectory(newUsersFolderPath);
            }

            var files = Directory.GetFiles(tempUsersFolderPath, "*.*", SearchOption.AllDirectories);
            var dirInfo = new DirectoryInfo(newUsersFolderPath);
            foreach (
                var mFile in
                    files.Select(file => new System.IO.FileInfo(file)))
            {
                if (File.Exists(dirInfo + @"\" + mFile.Name))
                {
                    File.Delete(dirInfo + @"\" + mFile.Name);
                }

                mFile.MoveTo(dirInfo + @"\" + mFile.Name);
            }
        }

        private static string GetActualFileName(ExportFile objFile)
        {
			Logger.LogDebug(">>> Entering GetActualFileName(objFile={ObjFile})", objFile);
			Logger.LogDebug("<<< Exiting GetActualFileName(objFile={ObjFile})", objFile);
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }

        private bool ProcessFolder(ExportImportJob importJob, ImportDto importDto, ExportFolder folder)
        {
			Logger.LogDebug(">>> Entering ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder})", importJob, importDto, folder);
            var portalId = importJob.PortalId;
            if (folder == null)
            {
                return false;
            }

            var existingFolder = CBO.FillObject<ExportFolder>(DotNetNuke.Data.DataProvider.Instance().GetFolder(portalId, folder.FolderPath ?? string.Empty));
            var isUpdate = false;
			Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): existingFolder → {ExistingFolder}", importJob, importDto, folder, existingFolder);
			Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): isUpdate → {IsUpdate}", importJob, importDto, folder, isUpdate);
            var modifiedBy = Util.GetUserIdByName(importJob, folder.LastModifiedByUserId, folder.LastModifiedByUserName);
            if (existingFolder != null)
			Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): modifiedBy → {ModifiedBy}", importJob, importDto, folder, modifiedBy);
            {
                switch (importDto.CollisionResolution)
                {
                    case CollisionResolution.Overwrite:
                        isUpdate = true;
                        break;
                    case CollisionResolution.Ignore:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                }
            }

            folder.FolderPath = string.IsNullOrEmpty(folder.FolderPath) ? string.Empty : folder.FolderPath;
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folder.FolderMappingName);
            if (folderMapping == null)
			Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): folderMapping → {FolderMapping}", importJob, importDto, folder, folderMapping);
            {
                return false;
            }

            var workFlowId = this.GetLocalWorkFlowId(folder.WorkflowId);
            if (isUpdate)
			Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): workFlowId → {WorkFlowId}", importJob, importDto, folder, workFlowId);
            {
                Util.FixDateTime(existingFolder);
                DotNetNuke.Data.DataProvider.Instance()
                    .UpdateFolder(
                        importJob.PortalId,
                        folder.VersionGuid,
                        existingFolder.FolderId,
                        folder.FolderPath,
                        folder.StorageLocation,
                        folder.MappedPath,
                        folder.IsProtected,
                        folder.IsCached,
                        DateUtils.GetDatabaseLocalTime(),
                        modifiedBy,
                        folderMapping.FolderMappingID,
                        folder.IsVersioned,
                        workFlowId,
                        existingFolder.ParentId ?? Null.NullInteger);

                folder.FolderId = existingFolder.FolderId;
				Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): folder.FolderId → {Arg3}", importJob, importDto, folder, folder.FolderId);

                if (folder.UserId != null && folder.UserId > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    SyncUserFolder(importJob.PortalId, folder);
                }
            }
            else
            {
                folder.FolderMappingId = folderMapping.FolderMappingID;
				Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): folder.FolderMappingId → {Arg3}", importJob, importDto, folder, folder.FolderMappingId);
                var createdBy = Util.GetUserIdByName(importJob, folder.CreatedByUserId, folder.CreatedByUserName);
                if (folder.ParentId is > 0)
				Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): createdBy → {CreatedBy}", importJob, importDto, folder, createdBy);
                {
                    // Find the previously created parent folder id.
                    folder.ParentId = CBO.FillObject<ExportFolder>(DotNetNuke.Data.DataProvider.Instance().GetFolder(portalId, folder.ParentFolderPath ?? string.Empty))?.FolderId;
                }

                // ignore folders which start with Users but are not user folders.
                if (!folder.FolderPath.StartsWith(DefaultUsersFoldersPath, StringComparison.OrdinalIgnoreCase))
                {
                    folder.FolderId = DotNetNuke.Data.DataProvider.Instance()
                        .AddFolder(
                            importJob.PortalId,
                            Guid.NewGuid(),
                            folder.VersionGuid,
                            folder.FolderPath,
                            folder.MappedPath,
                            folder.StorageLocation,
                            folder.IsProtected,
                            folder.IsCached,
                            DateUtils.GetDatabaseLocalTime(),
                            createdBy,
                            folderMapping.FolderMappingID,
                            folder.IsVersioned,
                            workFlowId,
                            folder.ParentId ?? Null.NullInteger);
                }

                // Case when the folder is a user folder.
                else if (folder.UserId is > 0 && !string.IsNullOrEmpty(folder.Username))
                {
                    var userInfo = UserController.GetUserByName(portalId, folder.Username);
                    if (userInfo == null)
					Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): userInfo → {UserInfo}", importJob, importDto, folder, userInfo);
                    {
                        folder.FolderId = 0;
                        return false;
                    }

                    userInfo.IsSuperUser = false;
					Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): userInfo.IsSuperUser → {Arg3}", importJob, importDto, folder, userInfo.IsSuperUser);
                    var newFolder = FolderManager.Instance.GetUserFolder(userInfo);
                    folder.FolderId = newFolder.FolderID;
					Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): newFolder → {NewFolder}", importJob, importDto, folder, newFolder);
					Logger.LogDebug("ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder}): folder.FolderPath → {Arg3}", importJob, importDto, folder, folder.FolderPath);
                    folder.FolderPath = newFolder.FolderPath;
                    SyncUserFolder(importJob.PortalId, folder);
                    return true;
                }
                else
                {
                    folder.FolderId = 0;
                    return false;
                }
            }

			Logger.LogDebug("<<< Exiting ProcessFolder(importJob={ImportJob},importDto={ImportDto},folder={Folder})", importJob, importDto, folder);
            return true;
        }

        private int GetCurrentSkip()
        {
			Logger.LogDebug(">>> Entering GetCurrentSkip()");
            if (!string.IsNullOrEmpty(this.CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(this.CheckPoint.StageData);
                return Convert.ToInt32(stageData.skip) ?? 0;
            }

			Logger.LogDebug("<<< Exiting GetCurrentSkip()");
            return 0;
        }

        private int GetLocalWorkFlowId(int? exportedWorkFlowId)
        {
			Logger.LogDebug(">>> Entering GetLocalWorkFlowId(exportedWorkFlowId={ExportedWorkFlowId})", exportedWorkFlowId);
            const int directPublishWorkflowId = 1;
            if (exportedWorkFlowId is > directPublishWorkflowId)
            {
                var state = this.Repository.GetItem<ExportWorkflow>(item => item.WorkflowID == exportedWorkFlowId);
                return state?.LocalId ?? -1;
            }

			Logger.LogDebug("<<< Exiting GetLocalWorkFlowId(exportedWorkFlowId={ExportedWorkFlowId})", exportedWorkFlowId);
            return -1;
        }
    }
}
