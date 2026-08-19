// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.PageTemplates;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    using DataProvider = Dnn.ExportImport.Components.Providers.DataProvider;

    /// <summary>An export service for templates.</summary>
    public class PageTemplatesExportService : AssetsExportService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PageTemplatesExportService));
        private readonly string templatesFolder;

        /// <summary>Initializes a new instance of the <see cref="PageTemplatesExportService"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public PageTemplatesExportService()
            : this(null)
        {
		Logger.LogDebug(">>> Entering PageTemplatesExportService()");
		Logger.LogDebug("<<< Exiting PageTemplatesExportService()");
		Logger.LogDebug(">>> Entering PageTemplatesExportService(appStatus={AppStatus})", appStatus);
		Logger.LogDebug("<<< Exiting PageTemplatesExportService(appStatus={AppStatus})", appStatus);
        }

        /// <summary>Initializes a new instance of the <see cref="PageTemplatesExportService"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PageTemplatesExportService(IApplicationStatusInfo appStatus)
            : this(appStatus, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PageTemplatesExportService"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="hostSettings">The host settings.</param>
        public PageTemplatesExportService(IApplicationStatusInfo appStatus, IHostSettings hostSettings)
            : base(appStatus, hostSettings)
        {
            appStatus ??= Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.templatesFolder = $"{appStatus.ApplicationMapPath}{Constants.ExportFolder}{{0}}\\{Constants.ExportZipTemplates}";
        }

        /// <inheritdoc />
        public override string Category => Constants.Category_Templates;

        /// <inheritdoc />
        public override string ParentCategory => null;

        /// <inheritdoc />
        public override uint Priority => 10;

        /// <inheritdoc />
        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
			Logger.LogDebug(">>> Entering ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
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
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): skip → {Skip}", exportJob, exportDto, skip);
            var currentIndex = skip;
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): currentIndex → {CurrentIndex}", exportJob, exportDto, currentIndex);
            var totalTemplatesExported = 0;
            var portalId = exportJob.PortalId;
            try
            {
                var templatesFile = string.Format(CultureInfo.InvariantCulture, this.templatesFolder, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));
				Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): templatesFile → {TemplatesFile}", exportJob, exportDto, templatesFile);

                if (this.CheckPoint.Stage == 0)
                {
                    var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
					Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): fromDate → {FromDate}", exportJob, exportDto, fromDate);
                    var toDate = exportDto.ToDateUtc.ToLocalTime();
					Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): toDate → {ToDate}", exportJob, exportDto, toDate);
                    var portal = PortalController.Instance.GetPortal(portalId);
					Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): portal → {Portal}", exportJob, exportDto, portal);

                    var templates =
                        CBO.FillCollection<ExportPageTemplate>(
                            DataProvider.Instance()
                                .GetFiles(portalId, null, toDate, fromDate))
                            .Where(x => x.Extension == Constants.TemplatesExtension)
                            .ToList();
                    var totalTemplates = templates.Count;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalTemplates : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    foreach (var template in templates)
                    {
						var __logStart = System.Diagnostics.Stopwatch.StartNew();
						Logger.LogInformation("ExportData(exportJob,exportDto)={ExportJob},{ExportDto}: database query executed in {Arg2} ms", exportJob, exportDto, __logStart.ElapsedMilliseconds);
                        this.Repository.CreateItem(template, null);
                        totalTemplatesExported += 1;
                        var folderOffset = portal.HomeDirectoryMapPath.Length +
                                           (portal.HomeDirectoryMapPath.EndsWith(@"\", StringComparison.Ordinal) ? 0 : 1);

                        var folder = FolderManager.Instance.GetFolder(template.FolderId);
                        CompressionUtil.AddFileToArchive(
                            portal.HomeDirectoryMapPath + folder.FolderPath + GetActualFileName(template),
                            templatesFile,
                            folderOffset);

                        this.CheckPoint.ProcessedItems++;
                        this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalTemplates;
                        currentIndex++;

                        // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                        if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                        {
							Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): folder → {Folder}", exportJob, exportDto, folder);
                            return;
                        }
                    }

                    this.CheckPoint.Stage++;
                    currentIndex = 0;
                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Progress = 100;
                }
            }
            finally
            {
                this.CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex, }) : null;
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Templates", totalTemplatesExported.ToString(CultureInfo.InvariantCulture));
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

            // Skip the export if all the templates have been processed already.
            if (this.CheckPoint.Stage >= 2 || this.CheckPoint.Completed)
            {
                return;
            }

            var portalId = importJob.PortalId;
            var templatesFile = string.Format(CultureInfo.InvariantCulture, this.templatesFolder, importJob.Directory.TrimEnd('\\').TrimEnd('/'));
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): templatesFile → {TemplatesFile}", importJob, importDto, templatesFile);
            var totalTemplates = this.GetImportTotal();
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): totalTemplates → {TotalTemplates}", importJob, importDto, totalTemplates);

            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalTemplates : this.CheckPoint.TotalItems;
            if (this.CheckPointStageCallback(this))
            {
                return;
            }

            if (this.CheckPoint.Stage == 0)
            {
                if (!File.Exists(templatesFile))
                {
                    this.Result.AddLogEntry(
                        "TemplatesFileNotFound",
                        "Templates file not found. Skipping templates import",
                        ReportLevel.Warn);
                    this.CheckPoint.Completed = true;
                    this.CheckPointStageCallback(this);
                }
                else
                {
                    var portal = PortalController.Instance.GetPortal(portalId);
					Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): portal → {Portal}", importJob, importDto, portal);

                    CompressionUtil.UnZipArchive(
                        templatesFile,
                        portal.HomeDirectoryMapPath,
                        importDto.CollisionResolution == CollisionResolution.Overwrite);

                    this.Result.AddSummary("Imported templates", totalTemplates.ToString(CultureInfo.InvariantCulture));
                    this.CheckPoint.Stage++;
                    this.CheckPoint.StageData = null;
                    this.CheckPoint.Progress = 90;
                    this.CheckPoint.TotalItems = totalTemplates;
                    this.CheckPoint.ProcessedItems = totalTemplates;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }
                }
            }

            if (this.CheckPoint.Stage == 1)
            {
                Func<ExportPageTemplate, object> predicate = x => x.Folder;
                var templates = this.Repository.GetAllItems(predicate).Select(x => x.Folder).Distinct();
                templates.ForEach(x => FolderManager.Instance.Synchronize(importJob.PortalId, x));
				Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): templates → {Templates}", importJob, importDto, templates);
				Logger.LogInformation("ImportData(importJob,importDto)={ImportJob},{ImportDto}: find query executed in {Arg2} ms", importJob, importDto, __logStart.ElapsedMilliseconds);
                this.CheckPoint.Stage++;
                this.CheckPoint.Completed = true;
                this.CheckPoint.Progress = 100;
                this.CheckPointStageCallback(this);
            }
        }

        /// <inheritdoc />
        public override int GetImportTotal()
        {
			Logger.LogDebug(">>> Entering GetImportTotal()");
			Logger.LogDebug("<<< Exiting GetImportTotal()");
            return this.Repository.GetCount<ExportPageTemplate>();
        }

        private static string GetActualFileName(ExportPageTemplate objFile)
        {
			Logger.LogDebug(">>> Entering GetActualFileName(objFile={ObjFile})", objFile);
			Logger.LogDebug("<<< Exiting GetActualFileName(objFile={ObjFile})", objFile);
            return (objFile.StorageLocation == (int)FolderController.StorageLocationTypes.SecureFileSystem)
                ? objFile.FileName + Globals.glbProtectedExtension
                : objFile.FileName;
        }

        private int GetCurrentSkip()
        {
			Logger.LogDebug(">>> Entering GetCurrentSkip()");
            if (!string.IsNullOrEmpty(this.CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(this.CheckPoint.StageData);
			Logger.LogDebug("<<< Exiting GetCurrentSkip()");
                return Convert.ToInt32(stageData.skip) ?? 0;
            }

            return 0;
        }
    }
}
