// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Components.Services;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using Newtonsoft.Json;

    /// <summary>The export controller.</summary>
    public class ExportController : BaseController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportController));
        /// <summary>Initializes a new instance of the <see cref="ExportController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IEnumerable<BasePortableService>. Scheduled removal in v12.0.0.")]
        public ExportController()
            : base(null)
        {
		Logger.LogDebug(">>> Entering ExportController()");
		Logger.LogDebug("<<< Exiting ExportController()");
        }

        /// <summary>Initializes a new instance of the <see cref="ExportController"/> class.</summary>
        /// <param name="portableServices">The portable service implementations.</param>
        public ExportController(IEnumerable<BasePortableService> portableServices)
            : base(portableServices)
        {
        }

        /// <summary>Queues an export operation.</summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="exportDto">The export DTO.</param>
        /// <returns>The job ID.</returns>
        public int QueueOperation(int userId, ExportDto exportDto)
        {
			Logger.LogDebug(">>> Entering QueueOperation(userId={UserId},exportDto={ExportDto})", userId, exportDto);
            exportDto.ProductSku = DotNetNuke.Application.DotNetNukeContext.Current.Application.SKU;
            exportDto.ProductVersion = Globals.FormatVersion(DotNetNuke.Application.DotNetNukeContext.Current.Application.Version, true);
			Logger.LogDebug("QueueOperation(userId={UserId},exportDto={ExportDto}): exportDto.ProductSku → {Arg2}", userId, exportDto, exportDto.ProductSku);
            var dbTime = DateUtils.GetDatabaseUtcTime();
			Logger.LogDebug("QueueOperation(userId={UserId},exportDto={ExportDto}): dbTime → {DbTime}", userId, exportDto, dbTime);
            exportDto.ToDateUtc = dbTime.AddMilliseconds(-dbTime.Millisecond);
            var directory = dbTime.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture);
			Logger.LogDebug("QueueOperation(userId={UserId},exportDto={ExportDto}): directory → {Directory}", userId, exportDto, directory);
            if (exportDto.ExportMode == ExportMode.Differential)
            {
                exportDto.FromDateUtc = this.GetLastJobTime(exportDto.PortalId, JobType.Export);
            }

            var dataObject = JsonConvert.SerializeObject(exportDto);
			Logger.LogDebug("QueueOperation(userId={UserId},exportDto={ExportDto}): dataObject → {DataObject}", userId, exportDto, dataObject);
			Logger.LogDebug("QueueOperation(userId={UserId},exportDto={ExportDto}): exportDto.IsDirty → {Arg2}", userId, exportDto, exportDto.IsDirty);
            exportDto.IsDirty = false; // This should be set to false for new job.
            var jobId = DataProvider.Instance()
                .AddNewJob(
                    exportDto.PortalId,
                    userId,
                    JobType.Export,
                    exportDto.ExportName,
                    exportDto.ExportDescription,
                    directory,
                    dataObject);

            // Run the scheduler if required.
            if (exportDto.RunNow)
            {
                EntitiesController.Instance.RunSchedule();
            }

            this.AddEventLog(exportDto.PortalId, userId, jobId, Constants.LogTypeSiteExport);
			Logger.LogDebug("<<< Exiting QueueOperation(userId={UserId},exportDto={ExportDto})", userId, exportDto);
            return jobId;
        }

        /// <summary>Creates a package manifest.</summary>
        /// <param name="exportJob">The export job.</param>
        /// <param name="exportFileInfo">The export file info.</param>
        /// <param name="summary">The summary.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void CreatePackageManifest(ExportImportJob exportJob, ExportFileInfo exportFileInfo, ImportExportSummary summary)
        {
			Logger.LogDebug(">>> Entering CreatePackageManifest(exportJob={ExportJob},exportFileInfo={ExportFileInfo},summary={Summary})", exportJob, exportFileInfo, summary);
            var filePath = Path.Combine(ExportFolder, exportJob.Directory, Constants.ExportManifestName);
            var portal = PortalController.Instance.GetPortal(exportJob.PortalId);
			Logger.LogDebug("CreatePackageManifest(exportJob={ExportJob},exportFileInfo={ExportFileInfo},summary={Summary}): portal → {Portal}", exportJob, exportFileInfo, summary, portal);
			Logger.LogDebug("CreatePackageManifest(exportJob={ExportJob},exportFileInfo={ExportFileInfo},summary={Summary}): filePath → {FilePath}", exportJob, exportFileInfo, summary, filePath);
            var packageInfo = new ImportPackageInfo
            {
                Summary = summary,
                PackageId = exportJob.Directory,
                Name = exportJob.Name,
                Description = exportJob.Description,
                ExporTime = exportJob.CreatedOnDate,
                PortalName = portal?.PortalName,
            };
            Util.WriteJson(filePath, packageInfo);
			Logger.LogDebug("<<< Exiting CreatePackageManifest(exportJob={ExportJob},exportFileInfo={ExportFileInfo},summary={Summary})", exportJob, exportFileInfo, summary);
        }
    }
}
