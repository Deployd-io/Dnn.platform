// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Components.Controllers
{
    using System;
    using System.Collections.Generic;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Interfaces;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Components.Scheduler;
    using Dnn.ExportImport.Dto.Pages;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Scheduling;

    /// <inheritdoc cref="IEntitiesController"/>
    /// <seealso cref="ServiceLocator{TContract,TSelf}"/>
    public class EntitiesController : ServiceLocator<IEntitiesController, EntitiesController>, IEntitiesController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EntitiesController));
        private readonly DataProvider dataProvider = DataProvider.Instance();

        /// <inheritdoc />
        public ExportImportJob GetFirstActiveJob()
        {
			Logger.LogError(">>> Entering GetFirstActiveJob()");
            Logger.LogWarning("<<< Exiting GetFirstActiveJob()");
            return CBO.Instance.FillObject<ExportImportJob>(this.dataProvider.GetFirstActiveJob());
        }

        /// <inheritdoc />
        public ExportImportJob GetJobById(int jobId)
        {
			Logger.LogError(">>> Entering GetJobById(jobId={JobId})", jobId);
            var job = CBO.Instance.FillObject<ExportImportJob>(this.dataProvider.GetJobById(jobId));

			Logger.LogError("GetJobById(jobId={JobId}): job → {Job}", jobId, job);
            ////System.Diagnostics.Trace.WriteLine($"xxxxxxxxx job id={job?.JobId} IsCancelled={job?.IsCancelled} xxxxxxxxx");
			Logger.LogWarning("<<< Exiting GetJobById(jobId={JobId})", jobId);
            return job;
        }

        /// <inheritdoc />
        public IList<ExportImportJobLog> GetJobSummaryLog(int jobId)
        {
			Logger.LogError(">>> Entering GetJobSummaryLog(jobId={JobId})", jobId);
            Logger.LogWarning("<<< Exiting GetJobSummaryLog(jobId={JobId})", jobId);
            return CBO.Instance.FillCollection<ExportImportJobLog>(this.dataProvider.GetJobSummaryLog(jobId));
        }

        /// <inheritdoc />
        public IList<ExportImportJobLog> GetJobFullLog(int jobId)
        {
			Logger.LogError(">>> Entering GetJobFullLog(jobId={JobId})", jobId);
            Logger.LogWarning("<<< Exiting GetJobFullLog(jobId={JobId})", jobId);
            return CBO.Instance.FillCollection<ExportImportJobLog>(this.dataProvider.GetJobFullLog(jobId));
        }

        /// <inheritdoc />
        public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
        {
			Logger.LogError(">>> Entering GetAllJobsCount(portalId={PortalId},jobType={JobType},keywords={Keywords})", portalId, jobType, keywords);
            Logger.LogWarning("<<< Exiting GetAllJobsCount(portalId={PortalId},jobType={JobType},keywords={Keywords})", portalId, jobType, keywords);
            return this.dataProvider.GetAllJobsCount(portalId, jobType, keywords);
        }

        /// <inheritdoc />
        public IList<ExportImportJob> GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
			Logger.LogError(">>> Entering GetAllJobs(portalId={PortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portalId, pageSize, pageIndex, jobType, keywords);
            Logger.LogWarning("<<< Exiting GetAllJobs(portalId={PortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portalId, pageSize, pageIndex, jobType, keywords);
            return CBO.Instance.FillCollection<ExportImportJob>(
                this.dataProvider.GetAllJobs(portalId, pageSize, pageIndex, jobType, keywords));
        }

        /// <inheritdoc />
        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
			Logger.LogError(">>> Entering GetLastJobTime(portalId={PortalId},jobType={JobType})", portalId, jobType);
            Logger.LogWarning("<<< Exiting GetLastJobTime(portalId={PortalId},jobType={JobType})", portalId, jobType);
            return this.dataProvider.GetLastJobTime(portalId, jobType);
        }

        /// <inheritdoc />
        public void UpdateJobInfo(ExportImportJob job)
        {
			Logger.LogError(">>> Entering UpdateJobInfo(job={Job})", job);
            this.dataProvider.UpdateJobInfo(job.JobId, job.Name, job.Description);
			Logger.LogWarning("<<< Exiting UpdateJobInfo(job={Job})", job);
        }

        /// <inheritdoc />
        public void UpdateJobStatus(ExportImportJob job)
        {
			Logger.LogError(">>> Entering UpdateJobStatus(job={Job})", job);
            this.dataProvider.UpdateJobStatus(job.JobId, job.JobStatus);
			Logger.LogWarning("<<< Exiting UpdateJobStatus(job={Job})", job);
        }

        /// <inheritdoc />
        public void SetJobCancelled(ExportImportJob job)
        {
			Logger.LogError(">>> Entering SetJobCancelled(job={Job})", job);
            this.dataProvider.SetJobCancelled(job.JobId);
			Logger.LogWarning("<<< Exiting SetJobCancelled(job={Job})", job);
        }

        /// <inheritdoc />
        public void RemoveJob(ExportImportJob job)
        {
			Logger.LogError(">>> Entering RemoveJob(job={Job})", job);
            this.dataProvider.RemoveJob(job.JobId);
			Logger.LogWarning("<<< Exiting RemoveJob(job={Job})", job);
        }

        /// <inheritdoc />
        public IList<ExportImportChekpoint> GetJobChekpoints(int jobId)
        {
			Logger.LogError(">>> Entering GetJobChekpoints(jobId={JobId})", jobId);
            Logger.LogWarning("<<< Exiting GetJobChekpoints(jobId={JobId})", jobId);
            return CBO.Instance.FillCollection<ExportImportChekpoint>(this.dataProvider.GetJobChekpoints(jobId));
        }

        /// <inheritdoc />
        public void UpdateJobChekpoint(ExportImportChekpoint checkpoint)
        {
			Logger.LogError(">>> Entering UpdateJobChekpoint(checkpoint={Checkpoint})", checkpoint);
            this.dataProvider.UpsertJobChekpoint(checkpoint);
			Logger.LogWarning("<<< Exiting UpdateJobChekpoint(checkpoint={Checkpoint})", checkpoint);
        }

        /// <inheritdoc />
        public IList<ExportTabInfo> GetPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetPortalTabs(portalId={PortalId},includeDeleted={IncludeDeleted},includeSystem={IncludeSystem},toDate={ToDate},fromDate={FromDate})", portalId, includeDeleted, includeSystem, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetPortalTabs(portalId={PortalId},includeDeleted={IncludeDeleted},includeSystem={IncludeSystem},toDate={ToDate},fromDate={FromDate})", portalId, includeDeleted, includeSystem, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportTabInfo>(
                this.dataProvider.GetAllPortalTabs(portalId, includeDeleted, includeSystem, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportTabSetting> GetTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetTabSettings(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetTabSettings(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportTabSetting>(
                this.dataProvider.GetAllTabSettings(tabId, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportTabPermission> GetTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetTabPermissions(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetTabPermissions(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportTabPermission>(
                this.dataProvider.GetAllTabPermissions(tabId, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportTabUrl> GetTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetTabUrls(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetTabUrls(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportTabUrl>(
                this.dataProvider.GetAllTabUrls(tabId, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportModule> GetModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportModule>(
                this.dataProvider.GetAllModules(tabId, includeDeleted, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportModuleSetting> GetModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetModuleSettings(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetModuleSettings(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportModuleSetting>(
                this.dataProvider.GetAllModuleSettings(moduleId, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportModulePermission> GetModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetModulePermissions(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetModulePermissions(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportModulePermission>(
                this.dataProvider.GetAllModulePermissions(moduleId, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportTabModule> GetTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetTabModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetTabModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
            return CBO.Instance.FillCollection<ExportTabModule>(
                this.dataProvider.GetAllTabModules(tabId, includeDeleted, toDate, fromDate));
        }

        /// <inheritdoc />
        public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetTabModuleSettings(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetTabModuleSettings(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            return this.GetTabModuleSettings(tabId, true, toDate, fromDate);
        }

        /// <inheritdoc />
        public IList<ExportTabModuleSetting> GetTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            return CBO.Instance.FillCollection<ExportTabModuleSetting>(
                this.dataProvider.GetAllTabModuleSettings(tabId, includeDeleted, toDate, fromDate));
        }

        /// <inheritdoc />
        public PermissionInfo GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
        {
			Logger.LogError(">>> Entering GetPermissionInfo(permissionCode={PermissionCode},permissionKey={PermissionKey},permissionName={PermissionName})", permissionCode, permissionKey, permissionName);
            Logger.LogWarning("<<< Exiting GetPermissionInfo(permissionCode={PermissionCode},permissionKey={PermissionKey},permissionName={PermissionName})", permissionCode, permissionKey, permissionName);
            return CBO.Instance.FillObject<PermissionInfo>(
                this.dataProvider.GetPermissionInfo(permissionCode, permissionKey, permissionName));
        }

        /// <inheritdoc />
        public void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible)
        {
			Logger.LogError(">>> Entering SetTabSpecificData(tabId={TabId},isDeleted={IsDeleted},isVisible={IsVisible})", tabId, isDeleted, isVisible);
            this.dataProvider.SetTabSpecificData(tabId, isDeleted, isVisible);
			Logger.LogWarning("<<< Exiting SetTabSpecificData(tabId={TabId},isDeleted={IsDeleted},isVisible={IsVisible})", tabId, isDeleted, isVisible);
        }

        /// <inheritdoc />
        public void SetTabModuleDeleted(int tabModuleId, bool isDeleted)
        {
			Logger.LogError(">>> Entering SetTabModuleDeleted(tabModuleId={TabModuleId},isDeleted={IsDeleted})", tabModuleId, isDeleted);
            this.dataProvider.SetTabModuleDeleted(tabModuleId, isDeleted);
			Logger.LogWarning("<<< Exiting SetTabModuleDeleted(tabModuleId={TabModuleId},isDeleted={IsDeleted})", tabModuleId, isDeleted);
        }

        /// <inheritdoc />
        public void SetUserDeleted(int portalId, int userId, bool isDeleted)
        {
			Logger.LogError(">>> Entering SetUserDeleted(portalId={PortalId},userId={UserId},isDeleted={IsDeleted})", portalId, userId, isDeleted);
            this.dataProvider.SetUserDeleted(portalId, userId, isDeleted);
			Logger.LogWarning("<<< Exiting SetUserDeleted(portalId={PortalId},userId={UserId},isDeleted={IsDeleted})", portalId, userId, isDeleted);
        }

        /// <inheritdoc />
        public void RunSchedule()
        {
			Logger.LogError(">>> Entering RunSchedule()");
            var executingServer = ServerController.GetExecutingServerName();

			Logger.LogError("RunSchedule(): executingServer → {ExecutingServer}", executingServer);
            var scheduleItem = SchedulingController.GetSchedule(GetSchedulerTypeFullName(), executingServer);
			Logger.LogError("RunSchedule(): scheduleItem → {ScheduleItem}", scheduleItem);
            if (scheduleItem != null)
            {
                SchedulingProvider.Instance().RunScheduleItemNow(scheduleItem, true);

                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                {
                    SchedulingProvider.Instance().ReStart("Change made to schedule.");
                }
            }
			Logger.LogWarning("<<< Exiting RunSchedule()");
        }

        /// <inheritdoc />
        protected override Func<IEntitiesController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new EntitiesController();
        }

        private static string GetSchedulerTypeFullName()
        {
			Logger.LogError(">>> Entering GetSchedulerTypeFullName()");
            var type = typeof(ExportImportScheduler);
			Logger.LogError("GetSchedulerTypeFullName(): type → {Type}", type);
			Logger.LogWarning("<<< Exiting GetSchedulerTypeFullName()");
            return $"{type.FullName}, {type.Assembly.GetName().Name}";
        }
    }
}
