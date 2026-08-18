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
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Dto.Jobs;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Services;
    using Dnn.ExportImport.Interfaces;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    /// <summary>The import/export controller.</summary>
    public class BaseController
    {
        /// <summary>The full path to the folder used for import/export.</summary>
        public static readonly string ExportFolder;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(BaseController));

        static BaseController()
        {
			Logger.LogDebug(">>> Entering BaseController()");
            var appStatus = Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            ExportFolder = appStatus.ApplicationMapPath + Constants.ExportFolder;
			Logger.LogDebug("BaseController(): ExportFolder → {ExportFolder}", ExportFolder);
			Logger.LogDebug("BaseController(): appStatus → {AppStatus}", appStatus);
            if (!Directory.Exists(ExportFolder))
            {
                Directory.CreateDirectory(ExportFolder);
            }
			Logger.LogDebug("<<< Exiting BaseController()");
        }

        /// <summary>Initializes a new instance of the <see cref="BaseController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IEnumerable<BasePortableService>. Scheduled removal in v12.0.0.")]
        public BaseController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="BaseController"/> class.</summary>
        /// <param name="portableServices">The portable services.</param>
        public BaseController(IEnumerable<BasePortableService> portableServices)
        {
			Logger.LogDebug(">>> Entering BaseController(portableServices={PortableServices})", portableServices);
            this.PortableServices = portableServices ?? Globals.GetCurrentServiceProvider().GetServices<BasePortableService>();
			Logger.LogDebug("<<< Exiting BaseController(portableServices={PortableServices})", portableServices);
        }

        /// <summary>Gets the portable services.</summary>
        protected IEnumerable<BasePortableService> PortableServices { get; }

        /// <summary>Cancels the job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A value indicating whether the job was found.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool CancelJob(int portalId, int jobId)
        {
			Logger.LogDebug(">>> Entering CancelJob(portalId={PortalId},jobId={JobId})", portalId, jobId);
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
			Logger.LogDebug("CancelJob(portalId={PortalId},jobId={JobId}): job → {Job}", portalId, jobId, job);
            if (job == null || (job.PortalId != portalId && portalId != -1))
            {
                return false;
            }

            controller.SetJobCancelled(job);
            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));
			Logger.LogDebug("<<< Exiting CancelJob(portalId={PortalId},jobId={JobId})", portalId, jobId);
            return true;
        }

        /// <summary>Removes the job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A value indicating whether the job was found.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool RemoveJob(int portalId, int jobId)
        {
			Logger.LogDebug(">>> Entering RemoveJob(portalId={PortalId},jobId={JobId})", portalId, jobId);
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
			Logger.LogDebug("RemoveJob(portalId={PortalId},jobId={JobId}): job → {Job}", portalId, jobId, job);
            if (job == null || (job.PortalId != portalId && portalId != -1))
            {
                return false;
            }

            CachingProvider.Instance().Remove(Util.GetExpImpJobCacheKey(job));

            // if the job is running; then it will create few exceptions in the log file
            controller.RemoveJob(job);
            DeleteJobData(job);
			Logger.LogDebug("<<< Exiting RemoveJob(portalId={PortalId},jobId={JobId})", portalId, jobId);
            return true;
        }

        /// <summary>Retrieves one page of paginated processed jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="currentPortalId">The current portal ID.</param>
        /// <param name="pageSize">The page size (between <c>1</c> and <c>100</c>).</param>
        /// <param name="pageIndex">The page index (zero-based).</param>
        /// <param name="jobType">The job type.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>An <see cref="AllJobsResult"/> instance.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public AllJobsResult GetAllJobs(int portalId, int currentPortalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
			Logger.LogDebug(">>> Entering GetAllJobs(portalId={PortalId},currentPortalId={CurrentPortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portalId, currentPortalId, pageSize, pageIndex, jobType, keywords);
            if (pageIndex < 0)
            {
                pageIndex = 0;
            }

            if (pageSize < 1)
            {
                pageSize = 1;
				Logger.LogDebug("GetAllJobs(portalId={PortalId},currentPortalId={CurrentPortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords}): pageSize → {PageSize1}", portalId, currentPortalId, pageSize, pageIndex, jobType, keywords, pageSize);
            }
            else if (pageSize > 100)
            {
                pageSize = 100;
            }

            var count = EntitiesController.Instance.GetAllJobsCount(portalId, jobType, keywords);
			Logger.LogDebug("GetAllJobs(portalId={PortalId},currentPortalId={CurrentPortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords}): count → {Count}", portalId, currentPortalId, pageSize, pageIndex, jobType, keywords, count);
            var jobs = count <= 0
                ? null
                : EntitiesController.Instance.GetAllJobs(portalId, pageSize, pageIndex, jobType, keywords);

            var portal = PortalController.Instance.GetPortal(currentPortalId);
			Logger.LogDebug("GetAllJobs(portalId={PortalId},currentPortalId={CurrentPortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords}): portal → {Portal}", portalId, currentPortalId, pageSize, pageIndex, jobType, keywords, portal);
			Logger.LogDebug("<<< Exiting GetAllJobs(portalId={PortalId},currentPortalId={CurrentPortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portalId, currentPortalId, pageSize, pageIndex, jobType, keywords);
            return new AllJobsResult
            {
                LastExportTime = portalId > -1 ? EntitiesController.Instance.GetLastJobTime(portalId, JobType.Export) : null,
                LastImportTime = portalId > -1 ? EntitiesController.Instance.GetLastJobTime(portalId, JobType.Import) : null,
                PortalId = portalId,
                PortalName = portal.PortalName,
                TotalJobs = count,
                Jobs = jobs?.Select(ToJobItem),
            };
        }

        /// <summary>Gets details about a job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A <see cref="JobItem"/> instance.</returns>
        public JobItem GetJobDetails(int portalId, int jobId)
        {
			Logger.LogDebug(">>> Entering GetJobDetails(portalId={PortalId},jobId={JobId})", portalId, jobId);
            var controller = EntitiesController.Instance;
            var job = controller.GetJobById(jobId);
			Logger.LogDebug("GetJobDetails(portalId={PortalId},jobId={JobId}): job → {Job}", portalId, jobId, job);
            if (portalId != -1 && job?.PortalId != portalId)
            {
                return null;
            }

            var jobItem = ToJobItem(job);
			Logger.LogDebug("GetJobDetails(portalId={PortalId},jobId={JobId}): jobItem → {JobItem}", portalId, jobId, jobItem);
            jobItem.Summary = BuildJobSummary(this.PortableServices, jobId);
			Logger.LogDebug("<<< Exiting GetJobDetails(portalId={PortalId},jobId={JobId})", portalId, jobId);
            return jobItem;
        }

        /// <summary>
        /// Get the last time a successful export job has started.
        /// This date/time is in uts and can be used to set the next
        /// differential date/time to start the job from.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type.</param>
        /// <returns>The last job time.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
			Logger.LogDebug(">>> Entering GetLastJobTime(portalId={PortalId},jobType={JobType})", portalId, jobType);
			Logger.LogDebug("<<< Exiting GetLastJobTime(portalId={PortalId},jobType={JobType})", portalId, jobType);
            return EntitiesController.Instance.GetLastJobTime(portalId, jobType);
        }

        /// <summary>Builds a job summary.</summary>
        /// <param name="portableServices">The <see cref="BasePortableService"/> implementations.</param>
        /// <param name="packageId">The package ID.</param>
        /// <param name="repository">The repository.</param>
        /// <param name="summary">The summary to build.</param>
        protected internal static void BuildJobSummary(IEnumerable<BasePortableService> portableServices, string packageId, IExportImportRepository repository, ImportExportSummary summary)
        {
			Logger.LogDebug(">>> Entering BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary})", portableServices, packageId, repository, summary);
            var summaryItems = new SummaryList();
            var exportDto = repository.GetSingleItem<ExportDto>();
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): exportDto → {ExportDto}", portableServices, packageId, repository, summary, exportDto);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summaryItems → {SummaryItems}", portableServices, packageId, repository, summary, summaryItems);

            foreach (var implementor in portableServices)
            {
                implementor.Repository = repository;
				Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): implementor.Repository → {Arg4}", portableServices, packageId, repository, summary, implementor.Repository);
                summaryItems.Add(new SummaryItem
                {
                    TotalItems = implementor.GetImportTotal(),
                    Category = implementor.Category,
                    Order = implementor.Priority,
                });
            }

            summary.ExportFileInfo = GetExportFileInfo(Path.Combine(ExportFolder, Path.GetFileName(packageId), Constants.ExportManifestName));
            summary.FromDate = exportDto.FromDateUtc;
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.FromDate → {Arg4}", portableServices, packageId, repository, summary, summary.FromDate);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.ToDate → {Arg4}", portableServices, packageId, repository, summary, summary.ToDate);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.SummaryItems → {Arg4}", portableServices, packageId, repository, summary, summary.SummaryItems);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.IncludeDeletions → {Arg4}", portableServices, packageId, repository, summary, summary.IncludeDeletions);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.IncludeContent → {Arg4}", portableServices, packageId, repository, summary, summary.IncludeContent);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.IncludeExtensions → {Arg4}", portableServices, packageId, repository, summary, summary.IncludeExtensions);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.IncludePermissions → {Arg4}", portableServices, packageId, repository, summary, summary.IncludePermissions);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.IncludeProfileProperties → {Arg4}", portableServices, packageId, repository, summary, summary.IncludeProfileProperties);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary}): summary.ExportMode → {Arg4}", portableServices, packageId, repository, summary, summary.ExportMode);
            summary.ToDate = exportDto.ToDateUtc;
            summary.SummaryItems = summaryItems;
            summary.IncludeDeletions = exportDto.IncludeDeletions;
            summary.IncludeContent = exportDto.IncludeContent;
            summary.IncludeExtensions = exportDto.IncludeExtensions;
            summary.IncludePermissions = exportDto.IncludePermissions;
            summary.IncludeProfileProperties = exportDto.IncludeProperfileProperties;
            summary.ExportMode = exportDto.ExportMode;
			Logger.LogDebug("<<< Exiting BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary})", portableServices, packageId, repository, summary);
        }

        /// <summary>Builds a job summary.</summary>
        /// <param name="portableServices">The portable service implementations.</param>
        /// <param name="jobId">The job ID.</param>
        /// <returns>An <see cref="ImportExportSummary"/> instance.</returns>
        protected static ImportExportSummary BuildJobSummary(IEnumerable<BasePortableService> portableServices, int jobId)
        {
			Logger.LogDebug(">>> Entering BuildJobSummary(portableServices={PortableServices},jobId={JobId})", portableServices, jobId);
            var summaryItems = new SummaryList();
            var controller = EntitiesController.Instance;
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},jobId={JobId}): summaryItems → {SummaryItems}", portableServices, jobId, summaryItems);
            var job = controller.GetJobById(jobId);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},jobId={JobId}): job → {Job}", portableServices, jobId, job);
            var exportDto = job.JobType == JobType.Export
                ? JsonConvert.DeserializeObject<ExportDto>(job.JobObject)
                : JsonConvert.DeserializeObject<ImportDto>(job.JobObject).ExportDto;
				Logger.LogDebug("<<< Exiting BuildJobSummary(portableServices={PortableServices},packageId={PackageId},repository={Repository},summary={Summary})", portableServices, packageId, repository, summary);

            var importExportSummary = new ImportExportSummary
            {
                IncludeDeletions = exportDto.IncludeDeletions,
                IncludeExtensions = exportDto.IncludeExtensions,
                IncludePermissions = exportDto.IncludePermissions,
                IncludeProfileProperties = exportDto.IncludeProperfileProperties,
                IncludeContent = exportDto.IncludeContent,
                FromDate = exportDto.FromDateUtc,
                ToDate = exportDto.ToDateUtc,
                ExportMode = exportDto.ExportMode,
                ExportFileInfo = job.JobType == JobType.Export
                    ? GetExportFileInfo(Path.Combine(ExportFolder, job.Directory, Constants.ExportManifestName))
                    : JsonConvert.DeserializeObject<ImportDto>(job.JobObject).ExportFileInfo,
            };

            var checkpoints = EntitiesController.Instance.GetJobChekpoints(jobId);
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},jobId={JobId}): checkpoints → {Checkpoints}", portableServices, jobId, checkpoints);
            if (!checkpoints.Any())
            {
                return importExportSummary;
            }

            summaryItems.AddRange(checkpoints.Select(checkpoint => new SummaryItem
            {
                TotalItems = checkpoint.TotalItems,
                ProcessedItems = checkpoint.ProcessedItems <= checkpoint.TotalItems ? checkpoint.ProcessedItems : checkpoint.TotalItems,
                ProgressPercentage = Convert.ToInt32(checkpoint.Progress),
                Category = checkpoint.Category,
                Order = portableServices.FirstOrDefault(x => x.Category == checkpoint.Category)?.Priority ?? 0,
                Completed = checkpoint.Completed,
            }));
            importExportSummary.SummaryItems = summaryItems;
			Logger.LogDebug("BuildJobSummary(portableServices={PortableServices},jobId={JobId}): importExportSummary.SummaryItems → {Arg2}", portableServices, jobId, importExportSummary.SummaryItems);
			Logger.LogDebug("<<< Exiting BuildJobSummary(portableServices={PortableServices},jobId={JobId})", portableServices, jobId);
            return importExportSummary;
        }

        /// <summary>Gets the export file info.</summary>
        /// <param name="manifestPath">The manifest path.</param>
        /// <returns>An <see cref="ExportFileInfo"/> instance.</returns>
        protected static ExportFileInfo GetExportFileInfo(string manifestPath)
        {
			Logger.LogDebug(">>> Entering GetExportFileInfo(manifestPath={ManifestPath})", manifestPath);
            ImportPackageInfo packageInfo = null;
            Util.ReadJson(manifestPath, ref packageInfo);
			Logger.LogDebug("<<< Exiting GetExportFileInfo(manifestPath={ManifestPath})", manifestPath);
            return packageInfo?.Summary.ExportFileInfo;
        }

        /// <summary>Gets the package info.</summary>
        /// <param name="manifestPath">The manifest path.</param>
        /// <returns>An <see cref="ImportPackageInfo"/> instance.</returns>
        protected static ImportPackageInfo GetPackageInfo(string manifestPath)
        {
			Logger.LogDebug(">>> Entering GetPackageInfo(manifestPath={ManifestPath})", manifestPath);
            ImportPackageInfo packageInfo = null;
            Util.ReadJson(manifestPath, ref packageInfo);
			Logger.LogDebug("<<< Exiting GetPackageInfo(manifestPath={ManifestPath})", manifestPath);
            return packageInfo;
        }

        /// <summary>Adds an event log.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="jobId">The job ID.</param>
        /// <param name="logTypeKey">The log type key.</param>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected void AddEventLog(int portalId, int userId, int jobId, string logTypeKey)
        {
			Logger.LogDebug(">>> Entering AddEventLog(portalId={PortalId},userId={UserId},jobId={JobId},logTypeKey={LogTypeKey})", portalId, userId, jobId, logTypeKey);
            var objSecurity = PortalSecurity.Instance;
            var portalInfo = PortalController.Instance.GetPortal(portalId);
			Logger.LogDebug("AddEventLog(portalId={PortalId},userId={UserId},jobId={JobId},logTypeKey={LogTypeKey}): portalInfo → {PortalInfo}", portalId, userId, jobId, logTypeKey, portalInfo);
            var userInfo = UserController.Instance.GetUser(portalId, userId);
			Logger.LogDebug("<<< Exiting AddEventLog(portalId={PortalId},userId={UserId},jobId={JobId},logTypeKey={LogTypeKey})", portalId, userId, jobId, logTypeKey);
			Logger.LogDebug("AddEventLog(portalId={PortalId},userId={UserId},jobId={JobId},logTypeKey={LogTypeKey}): userInfo → {UserInfo}", portalId, userId, jobId, logTypeKey, userInfo);
#pragma warning disable CS0618 // Type or member is obsolete
            var username = objSecurity.InputFilter(
                userInfo.Username,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete

            ILogInfo log = new LogInfo
            {
                LogTypeKey = logTypeKey,
                LogPortalName = portalInfo.PortalName,
                LogUserName = username,
            };
            log.LogPortalId = portalId;
			Logger.LogDebug("AddEventLog(portalId={PortalId},userId={UserId},jobId={JobId},logTypeKey={LogTypeKey}): log.LogPortalId → {Arg4}", portalId, userId, jobId, logTypeKey, log.LogPortalId);
			Logger.LogDebug("AddEventLog(portalId={PortalId},userId={UserId},jobId={JobId},logTypeKey={LogTypeKey}): log.LogUserId → {Arg4}", portalId, userId, jobId, logTypeKey, log.LogUserId);
            log.LogUserId = userId;

            log.AddProperty("JobID", jobId.ToString(CultureInfo.InvariantCulture));
            LogController.Instance.AddLog((LogInfo)log);
        }

        private static JobItem ToJobItem(ExportImportJob job)
        {
            var user = UserController.Instance.GetUserById(job.PortalId, job.CreatedByUserId);
            var name = job.JobType == JobType.Import ? JsonConvert.DeserializeObject<ImportDto>(job.JobObject)?.ExportDto?.ExportName : job.Name;

            return new JobItem
            {
                JobId = job.JobId,
                PortalId = job.PortalId,
                User = user?.DisplayName ?? user?.Username ?? job.CreatedByUserId.ToString(CultureInfo.InvariantCulture),
                JobType = Localization.GetString("JobType_" + job.JobType, Constants.SharedResources),
                Status = (int)job.JobStatus,
                Cancelled = job.IsCancelled,
                JobStatus = Localization.GetString("JobStatus_" + job.JobStatus, Constants.SharedResources),
                Name = name,
                Description = job.Description,
                CreatedOn = job.CreatedOnDate,
                CompletedOn = job.CompletedOnDate,
                ExportFile = job.CompletedOnDate.HasValue ? job.Directory : null,
            };
        }

        private static void DeleteJobData(ExportImportJob job)
        {
            if (job.JobType != JobType.Export)
            {
                return;
            }

            var jobFolder = Path.Combine(ExportFolder, job.Directory);
            try
            {
                if (Directory.Exists(jobFolder))
                {
                    Directory.Delete(jobFolder, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"Failed to delete the job data. Error:{ex.Message}. It will need to be deleted manually. Folder Path:{jobFolder}");
            }
			Logger.LogDebug("<<< Exiting DeleteJobData(job={Job})", job);
        }
    }
}
