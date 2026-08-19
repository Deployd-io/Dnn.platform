// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Entities;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A data provider.</summary>
    internal sealed class DataProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataProvider));
        private static readonly DataProvider Provider;

        private readonly DotNetNuke.Data.DataProvider dataProvider = DotNetNuke.Data.DataProvider.Instance();
        private readonly IHostSettings hostSettings;

        static DataProvider()
        {
			Logger.LogDebug(">>> Entering DataProvider()");
            Provider = new DataProvider(Globals.DependencyProvider.GetRequiredService<IHostSettings>());
			Logger.LogDebug("<<< Exiting DataProvider()");
			Logger.LogDebug("DataProvider(): Provider → {Provider}", Provider);
        }

        private DataProvider(IHostSettings hostSettings)
        {
			Logger.LogDebug(">>> Entering DataProvider(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings;
			Logger.LogDebug("DataProvider(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
			Logger.LogDebug("<<< Exiting DataProvider(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Gets the singleton instance of <see cref="DataProvider"/>.</summary>
        /// <returns>The instance.</returns>
        public static DataProvider Instance()
        {
			Logger.LogDebug(">>> Entering Instance()");
			Logger.LogDebug("<<< Exiting Instance()");
            return Provider;
        }

        /// <summary>Update record changers.</summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="primaryKeyName">The primary key name.</param>
        /// <param name="primaryKeyId">The primary key ID.</param>
        /// <param name="createdBy">The ID of the creating user.</param>
        /// <param name="modifiedBy">The ID of the modifying user.</param>
        public void UpdateRecordChangers(string tableName, string primaryKeyName, int primaryKeyId, int? createdBy, int? modifiedBy)
        {
			Logger.LogDebug(">>> Entering UpdateRecordChangers(tableName={TableName},primaryKeyName={PrimaryKeyName},primaryKeyId={PrimaryKeyId},createdBy={CreatedBy},modifiedBy={ModifiedBy})", tableName, primaryKeyName, primaryKeyId, createdBy, modifiedBy);
            this.dataProvider.ExecuteNonQuery(
                "Export_GenericUpdateRecordChangers", tableName, primaryKeyName, primaryKeyId, createdBy, modifiedBy);
				Logger.LogInformation("UpdateRecordChangers(tableName,primaryKeyName,primaryKeyId,createdBy,modifiedBy)={TableName},{PrimaryKeyName},{PrimaryKeyId},{CreatedBy},{ModifiedBy}: database query executed in {Arg5} ms", tableName, primaryKeyName, primaryKeyId, createdBy, modifiedBy, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting UpdateRecordChangers(tableName={TableName},primaryKeyName={PrimaryKeyName},primaryKeyId={PrimaryKeyId},createdBy={CreatedBy},modifiedBy={ModifiedBy})", tableName, primaryKeyName, primaryKeyId, createdBy, modifiedBy);
        }

        /// <summary>Update the unique ID.</summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="primaryKeyName">The primary key name.</param>
        /// <param name="primaryKeyId">The primary key ID.</param>
        /// <param name="uniqueId">The unique ID.</param>
        public void UpdateUniqueId(string tableName, string primaryKeyName, int primaryKeyId, Guid uniqueId)
        {
			Logger.LogDebug(">>> Entering UpdateUniqueId(tableName={TableName},primaryKeyName={PrimaryKeyName},primaryKeyId={PrimaryKeyId},uniqueId={UniqueId})", tableName, primaryKeyName, primaryKeyId, uniqueId);
            this.dataProvider.ExecuteNonQuery("Export_UpdateUniqueId", tableName, primaryKeyName, primaryKeyId, uniqueId);
			Logger.LogDebug("<<< Exiting UpdateUniqueId(tableName={TableName},primaryKeyName={PrimaryKeyName},primaryKeyId={PrimaryKeyId},uniqueId={UniqueId})", tableName, primaryKeyName, primaryKeyId, uniqueId);
        }

        /// <summary>Update setting record changers.</summary>
        /// <param name="tableName">The table name.</param>
        /// <param name="primaryKeyName">The primary key name.</param>
        /// <param name="parentKeyId">The parent key ID.</param>
        /// <param name="settingName">The setting name.</param>
        /// <param name="createdBy">The ID of the creating user.</param>
        /// <param name="modifiedBy">The ID of the modifying user.</param>
        public void UpdateSettingRecordChangers(string tableName, string primaryKeyName, int parentKeyId, string settingName, int? createdBy, int? modifiedBy)
        {
			Logger.LogDebug(">>> Entering UpdateSettingRecordChangers(tableName={TableName},primaryKeyName={PrimaryKeyName},parentKeyId={ParentKeyId},settingName={SettingName},createdBy={CreatedBy},modifiedBy={ModifiedBy})", tableName, primaryKeyName, parentKeyId, settingName, createdBy, modifiedBy);
            this.dataProvider.ExecuteNonQuery(
                "Export_GenedicUpdateSettingsRecordChangers", tableName, primaryKeyName, parentKeyId, settingName, createdBy, modifiedBy);
				Logger.LogInformation("UpdateSettingRecordChangers(tableName,primaryKeyName,parentKeyId,settingName,createdBy,modifiedBy)={TableName},{PrimaryKeyName},{ParentKeyId},{SettingName},{CreatedBy},{ModifiedBy}: database query executed in {Arg6} ms", tableName, primaryKeyName, parentKeyId, settingName, createdBy, modifiedBy, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting UpdateSettingRecordChangers(tableName={TableName},primaryKeyName={PrimaryKeyName},parentKeyId={ParentKeyId},settingName={SettingName},createdBy={CreatedBy},modifiedBy={ModifiedBy})", tableName, primaryKeyName, parentKeyId, settingName, createdBy, modifiedBy);
        }

        /// <summary>Add a new import/export job.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="jobType">The job type.</param>
        /// <param name="jobName">The job name.</param>
        /// <param name="jobDescription">The job description.</param>
        /// <param name="directory">The directory.</param>
        /// <param name="serializedObject">The job object as JSON.</param>
        /// <returns>The job ID.</returns>
        public int AddNewJob(int portalId, int userId, JobType jobType, string jobName, string jobDescription, string directory, string serializedObject)
        {
			Logger.LogDebug(">>> Entering AddNewJob(portalId={PortalId},userId={UserId},jobType={JobType},jobName={JobName},jobDescription={JobDescription},directory={Directory},serializedObject={SerializedObject})", portalId, userId, jobType, jobName, jobDescription, directory, serializedObject);
			Logger.LogDebug("<<< Exiting AddNewJob(portalId={PortalId},userId={UserId},jobType={JobType},jobName={JobName},jobDescription={JobDescription},directory={Directory},serializedObject={SerializedObject})", portalId, userId, jobType, jobName, jobDescription, directory, serializedObject);
            return this.dataProvider.ExecuteScalar<int>(
                "ExportImportJobs_Add",
                portalId,
                (int)jobType,
                userId,
                jobName,
                jobDescription,
                directory,
                serializedObject);
        }

        /// <summary>Update a job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="name">The job name.</param>
        /// <param name="description">The job description.</param>
        public void UpdateJobInfo(int jobId, string name, string description)
        {
			Logger.LogDebug(">>> Entering UpdateJobInfo(jobId={JobId},name={Name},description={Description})", jobId, name, description);
            this.dataProvider.ExecuteNonQuery("ExportImportJobs_UpdateInfo", jobId, name, description);
			Logger.LogDebug("<<< Exiting UpdateJobInfo(jobId={JobId},name={Name},description={Description})", jobId, name, description);
        }

        /// <summary>Update a job status.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="jobStatus">The job status.</param>
        public void UpdateJobStatus(int jobId, JobStatus jobStatus)
        {
			Logger.LogDebug(">>> Entering UpdateJobStatus(jobId={JobId},jobStatus={JobStatus})", jobId, jobStatus);
            DateTime? completeDate = null;
            if (jobStatus == JobStatus.Failed || jobStatus == JobStatus.Successful)
            {
                completeDate = DateUtils.GetDatabaseUtcTime();
            }
			Logger.LogDebug("UpdateJobStatus(jobId={JobId},jobStatus={JobStatus}): completeDate → {CompleteDate}", jobId, jobStatus, completeDate);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();

            this.dataProvider.ExecuteNonQuery(
                "ExportImportJobs_UpdateStatus", jobId, jobStatus, completeDate);
				Logger.LogInformation("UpdateJobStatus(jobId,jobStatus)={JobId},{JobStatus}: database query executed in {Arg2} ms", jobId, jobStatus, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting UpdateJobStatus(jobId={JobId},jobStatus={JobStatus})", jobId, jobStatus);
        }

        /// <summary>Set a job as canceled.</summary>
        /// <param name="jobId">The job ID.</param>
        public void SetJobCancelled(int jobId)
        {
			Logger.LogDebug(">>> Entering SetJobCancelled(jobId={JobId})", jobId);
            this.dataProvider.ExecuteNonQuery("ExportImportJobs_SetCancelled", jobId);
			Logger.LogDebug("<<< Exiting SetJobCancelled(jobId={JobId})", jobId);
        }

        /// <summary>Remove a job.</summary>
        /// <param name="jobId">The job ID.</param>
        public void RemoveJob(int jobId)
        {
			Logger.LogDebug(">>> Entering RemoveJob(jobId={JobId})", jobId);
            // using 60 sec timeout because cascading deletes in logs might take a lot of time
            this.dataProvider.ExecuteNonQuery(60, "ExportImportJobs_Remove", jobId);
			Logger.LogInformation("RemoveJob(jobId)={JobId}: database query executed in {Arg1} ms", jobId, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting RemoveJob(jobId={JobId})", jobId);
        }

        /// <summary>Get the import/export settings.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetExportImportSettings()
        {
			Logger.LogDebug(">>> Entering GetExportImportSettings()");
			Logger.LogDebug("<<< Exiting GetExportImportSettings()");
            return this.dataProvider.ExecuteReader("ExportImport_Settings");
        }

        /// <summary>Add a setting.</summary>
        /// <param name="exportImportSetting">The setting.</param>
        public void AddExportImportSetting(ExportImportSetting exportImportSetting)
        {
			Logger.LogDebug(">>> Entering AddExportImportSetting(exportImportSetting={ExportImportSetting})", exportImportSetting);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.dataProvider.ExecuteNonQuery(
                "ExportImport_AddSetting",
                exportImportSetting.SettingName,
                exportImportSetting.SettingValue,
                exportImportSetting.SettingIsSecure,
                exportImportSetting.CreatedByUserId);
				Logger.LogInformation("AddExportImportSetting(exportImportSetting)={ExportImportSetting}: database query executed in {Arg1} ms", exportImportSetting, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting AddExportImportSetting(exportImportSetting={ExportImportSetting})", exportImportSetting);
        }

        /// <summary>Get the first active job.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetFirstActiveJob()
        {
			Logger.LogDebug(">>> Entering GetFirstActiveJob()");
			Logger.LogDebug("<<< Exiting GetFirstActiveJob()");
            return this.dataProvider.ExecuteReader("ExportImportJobs_FirstActive");
        }

        /// <summary>Gets a job by ID.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobById(int jobId)
        {
			Logger.LogDebug(">>> Entering GetJobById(jobId={JobId})", jobId);
			Logger.LogDebug("<<< Exiting GetJobById(jobId={JobId})", jobId);
            return this.dataProvider.ExecuteReader("ExportImportJobs_GetById", jobId);
        }

        /// <summary>Gets the summary logs for a job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobSummaryLog(int jobId)
        {
			Logger.LogDebug(">>> Entering GetJobSummaryLog(jobId={JobId})", jobId);
			Logger.LogDebug("<<< Exiting GetJobSummaryLog(jobId={JobId})", jobId);
            return this.dataProvider.ExecuteReader("ExportImportJobLogs_Summary", jobId);
        }

        /// <summary>Gets full logs for a job.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobFullLog(int jobId)
        {
			Logger.LogDebug(">>> Entering GetJobFullLog(jobId={JobId})", jobId);
			Logger.LogDebug("<<< Exiting GetJobFullLog(jobId={JobId})", jobId);
            return this.dataProvider.ExecuteReader("ExportImportJobLogs_Full", jobId);
        }

        /// <summary>Get the count of all jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type ID.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>The count.</returns>
        public int GetAllJobsCount(int? portalId, int? jobType, string keywords)
        {
			Logger.LogDebug(">>> Entering GetAllJobsCount(portalId={PortalId},jobType={JobType},keywords={Keywords})", portalId, jobType, keywords);
			Logger.LogDebug("<<< Exiting GetAllJobsCount(portalId={PortalId},jobType={JobType},keywords={Keywords})", portalId, jobType, keywords);
            return this.dataProvider.ExecuteScalar<int>("ExportImport_GetJobsCount", portalId, jobType, keywords);
        }

        /// <summary>Get all jobs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="jobType">The job type ID.</param>
        /// <param name="keywords">Keywords.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllJobs(int? portalId, int? pageSize, int? pageIndex, int? jobType, string keywords)
        {
			Logger.LogDebug(">>> Entering GetAllJobs(portalId={PortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portalId, pageSize, pageIndex, jobType, keywords);
			Logger.LogDebug("<<< Exiting GetAllJobs(portalId={PortalId},pageSize={PageSize},pageIndex={PageIndex},jobType={JobType},keywords={Keywords})", portalId, pageSize, pageIndex, jobType, keywords);
            return this.dataProvider.ExecuteReader(
                "ExportImportJobs_GetAll", portalId, pageSize, pageIndex, jobType, keywords);
        }

        /// <summary>Get a job's checkpoints.</summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetJobChekpoints(int jobId)
        {
			Logger.LogDebug(">>> Entering GetJobChekpoints(jobId={JobId})", jobId);
			Logger.LogDebug("<<< Exiting GetJobChekpoints(jobId={JobId})", jobId);
            return this.dataProvider.ExecuteReader("ExportImportCheckpoints_GetByJob", jobId);
        }

        /// <summary>Get the last job time.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="jobType">The job type.</param>
        /// <returns>The date/time or <see langword="null"/>.</returns>
        public DateTime? GetLastJobTime(int portalId, JobType jobType)
        {
			Logger.LogDebug(">>> Entering GetLastJobTime(portalId={PortalId},jobType={JobType})", portalId, jobType);
            var datim = this.dataProvider.ExecuteScalar<DateTime?>("ExportImportJobLogs_LastJobTime", portalId, jobType);
            if (datim.HasValue)
            {
                var d = datim.Value;
                datim = new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second, d.Millisecond, DateTimeKind.Utc);
            }

			Logger.LogDebug("<<< Exiting GetLastJobTime(portalId={PortalId},jobType={JobType})", portalId, jobType);
            return datim;
        }

        /// <summary>Updates or inserts a job checkpoint.</summary>
        /// <param name="checkpoint">The checkpoint.</param>
        public void UpsertJobChekpoint(ExportImportChekpoint checkpoint)
        {
			Logger.LogDebug(">>> Entering UpsertJobChekpoint(checkpoint={Checkpoint})", checkpoint);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.dataProvider.ExecuteNonQuery(
                "ExportImportCheckpoints_Upsert",
                checkpoint.JobId,
                checkpoint.AssemblyName,
                checkpoint.Category,
                checkpoint.Stage,
                checkpoint.StageData,
                Null.SetNullInteger(Math.Floor(checkpoint.Progress)),
                checkpoint.TotalItems,
                checkpoint.ProcessedItems,
                this.dataProvider.GetNull(checkpoint.StartDate),
                checkpoint.Completed);
				Logger.LogInformation("UpsertJobChekpoint(checkpoint)={Checkpoint}: database query executed in {Arg1} ms", checkpoint, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting UpsertJobChekpoint(checkpoint={Checkpoint})", checkpoint);
        }

        /// <summary>Get all scope types.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllScopeTypes()
        {
			Logger.LogDebug(">>> Entering GetAllScopeTypes()");
			Logger.LogDebug("<<< Exiting GetAllScopeTypes()");
            return this.dataProvider.ExecuteReader("ExportTaxonomy_ScopeTypes");
        }

        /// <summary>Gets all vocabulary types.</summary>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllVocabularyTypes()
        {
			Logger.LogDebug(">>> Entering GetAllVocabularyTypes()");
			Logger.LogDebug("<<< Exiting GetAllVocabularyTypes()");
            return this.dataProvider.ExecuteReader("ExportTaxonomy_VocabularyTypes");
        }

        /// <summary>Get all terms.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTerms(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllTerms(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllTerms(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("ExportTaxonomy_Terms", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all vocabularies.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllVocabularies(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllVocabularies(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllVocabularies(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("ExportTaxonomy_Vocabularies", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all role groups.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllRoleGroups(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllRoleGroups(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllRoleGroups(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_RoleGroups", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all roles.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllRoles(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllRoles(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllRoles(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_Roles", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all role settings.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllRoleSettings(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllRoleSettings(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllRoleSettings(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_RoleSettings", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Gets the ID of a role.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="roleName">The role name.</param>
        /// <returns>The ID or <see cref="Globals.glbRoleNothing"/>.</returns>
        public int GetRoleIdByName(int portalId, string roleName)
        {
			Logger.LogDebug(">>> Entering GetRoleIdByName(portalId={PortalId},roleName={RoleName})", portalId, roleName);
			Logger.LogDebug("<<< Exiting GetRoleIdByName(portalId={PortalId},roleName={RoleName})", portalId, roleName);
            return this.dataProvider.ExecuteScalar<int>("Export_RoleIdByName", this.dataProvider.GetNull(portalId), roleName);
        }

        /// <summary>Set role auto-assign.</summary>
        /// <param name="roleId">The role ID.</param>
        public void SetRoleAutoAssign(int roleId)
        {
			Logger.LogDebug(">>> Entering SetRoleAutoAssign(roleId={RoleId})", roleId);
            this.dataProvider.ExecuteNonQuery("Export_RoleSetAutoAssign", roleId);
			Logger.LogDebug("<<< Exiting SetRoleAutoAssign(roleId={RoleId})", roleId);
        }

        /// <summary>Get property definitions for the portal.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted properties.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPropertyDefinitionsByPortal(int portalId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetPropertyDefinitionsByPortal(portalId={PortalId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", portalId, includeDeleted, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetPropertyDefinitionsByPortal(portalId={PortalId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", portalId, includeDeleted, toDate, fromDate);
            return this.dataProvider.ExecuteReader(
                "Export_GetPropertyDefinitionsByPortal",
                portalId,
                includeDeleted,
                toDate,
                this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get all users.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="includeDeleted">Whether to include deleted users.</param>
        /// <param name="toDateUtc">The end date.</param>
        /// <param name="fromDateUtc">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllUsers(int portalId, int pageIndex, int pageSize, bool includeDeleted, DateTime toDateUtc, DateTime? fromDateUtc)
        {
			Logger.LogDebug(">>> Entering GetAllUsers(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},toDateUtc={ToDateUtc},fromDateUtc={FromDateUtc})", portalId, pageIndex, pageSize, includeDeleted, toDateUtc, fromDateUtc);
			Logger.LogDebug("<<< Exiting GetAllUsers(portalId={PortalId},pageIndex={PageIndex},pageSize={PageSize},includeDeleted={IncludeDeleted},toDateUtc={ToDateUtc},fromDateUtc={FromDateUtc})", portalId, pageIndex, pageSize, includeDeleted, toDateUtc, fromDateUtc);
            return this.dataProvider.ExecuteReader(
                "Export_GetAllUsers",
                portalId,
                pageIndex,
                pageSize,
                includeDeleted,
                toDateUtc,
                this.dataProvider.GetNull(fromDateUtc),
                false);
        }

        /// <summary>Gets the users count.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted users.</param>
        /// <param name="toDateUtc">The end date.</param>
        /// <param name="fromDateUtc">The start date or <see langword="null"/>.</param>
        /// <returns>The count.</returns>
        public int GetUsersCount(int portalId, bool includeDeleted, DateTime toDateUtc, DateTime? fromDateUtc)
        {
			Logger.LogDebug(">>> Entering GetUsersCount(portalId={PortalId},includeDeleted={IncludeDeleted},toDateUtc={ToDateUtc},fromDateUtc={FromDateUtc})", portalId, includeDeleted, toDateUtc, fromDateUtc);
			Logger.LogDebug("<<< Exiting GetUsersCount(portalId={PortalId},includeDeleted={IncludeDeleted},toDateUtc={ToDateUtc},fromDateUtc={FromDateUtc})", portalId, includeDeleted, toDateUtc, fromDateUtc);
            return this.dataProvider
                .ExecuteScalar<int>("Export_GetAllUsers", portalId, 0, 0, includeDeleted, toDateUtc, this.dataProvider.GetNull(fromDateUtc), true);
        }

        /// <summary>Update users changers.</summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="createdByUserName">The username of the creating user.</param>
        /// <param name="modifiedByUserName">The username of the modifying user.</param>
        public void UpdateUserChangers(int userId, string createdByUserName, string modifiedByUserName)
        {
			Logger.LogDebug(">>> Entering UpdateUserChangers(userId={UserId},createdByUserName={CreatedByUserName},modifiedByUserName={ModifiedByUserName})", userId, createdByUserName, modifiedByUserName);
            this.dataProvider.ExecuteNonQuery(
                "Export_UpdateUsersChangers", userId, createdByUserName, modifiedByUserName);
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogInformation("UpdateUserChangers(userId,createdByUserName,modifiedByUserName)={UserId},{CreatedByUserName},{ModifiedByUserName}: database query executed in {Arg3} ms", userId, createdByUserName, modifiedByUserName, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting UpdateUserChangers(userId={UserId},createdByUserName={CreatedByUserName},modifiedByUserName={ModifiedByUserName})", userId, createdByUserName, modifiedByUserName);
        }

        /// <summary>Get portal settings.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPortalSettings(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetPortalSettings(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetPortalSettings(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_GetPortalSettings", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get portal languages.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPortalLanguages(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetPortalLanguages(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetPortalLanguages(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_GetPortalLanguages", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get portal localizations.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPortalLocalizations(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetPortalLocalizations(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetPortalLocalizations(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_GetPortalLocalizations", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get folders.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFolders(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetFolders(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetFolders(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_GetFolders", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get folder permissions by path.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFolderPermissionsByPath(int portalId, string folderPath, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetFolderPermissionsByPath(portalId={PortalId},folderPath={FolderPath},toDate={ToDate},fromDate={FromDate})", portalId, folderPath, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetFolderPermissionsByPath(portalId={PortalId},folderPath={FolderPath},toDate={ToDate},fromDate={FromDate})", portalId, folderPath, toDate, fromDate);
            return this.dataProvider
                .ExecuteReader("Export_GetFolderPermissionsByPath", portalId, folderPath, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get folder mappings.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFolderMappings(int portalId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetFolderMappings(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetFolderMappings(portalId={PortalId},toDate={ToDate},fromDate={FromDate})", portalId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_GetFolderMappings", portalId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Get files.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="folderId">The folder ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetFiles(int portalId, int? folderId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetFiles(portalId={PortalId},folderId={FolderId},toDate={ToDate},fromDate={FromDate})", portalId, folderId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetFiles(portalId={PortalId},folderId={FolderId},toDate={ToDate},fromDate={FromDate})", portalId, folderId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_GetFiles", portalId, folderId, toDate, this.dataProvider.GetNull(fromDate));
        }

        /// <summary>Gets the permission ID.</summary>
        /// <param name="permissionCode">The permission code.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <param name="permissionName">The permission name.</param>
        /// <returns>The permission ID or <see langword="null"/>.</returns>
        public int? GetPermissionId(string permissionCode, string permissionKey, string permissionName)
        {
			Logger.LogDebug(">>> Entering GetPermissionId(permissionCode={PermissionCode},permissionKey={PermissionKey},permissionName={PermissionName})", permissionCode, permissionKey, permissionName);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var permissions = CBO.GetCachedObject<IEnumerable<PermissionInfo>>(
                this.hostSettings,
                new CacheItemArgs(DataCache.PermissionsCacheKey, DataCache.PermissionsCacheTimeout, DataCache.PermissionsCachePriority),
                c => CBO.FillCollection<PermissionInfo>(this.dataProvider.ExecuteReader("GetPermissions")));
				Logger.LogInformation("GetPermissionId(permissionCode,permissionKey,permissionName)={PermissionCode},{PermissionKey},{PermissionName}: database query executed in {Arg3} ms", permissionCode, permissionKey, permissionName, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting GetPermissionId(permissionCode={PermissionCode},permissionKey={PermissionKey},permissionName={PermissionName})", permissionCode, permissionKey, permissionName);
            return
                (from IPermissionDefinitionInfo x in permissions
                where x.PermissionCode == permissionCode
                where x.PermissionKey == permissionKey
                where x.PermissionName.Equals(permissionName, StringComparison.OrdinalIgnoreCase)
                select (int?)x.PermissionId)
                .FirstOrDefault();
        }

        /// <summary>Get all portal tabs.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted tabs.</param>
        /// <param name="includeSystem">Whether to include system tabs.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllPortalTabs(int portalId, bool includeDeleted, bool includeSystem, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllPortalTabs(portalId={PortalId},includeDeleted={IncludeDeleted},includeSystem={IncludeSystem},toDate={ToDate},fromDate={FromDate})", portalId, includeDeleted, includeSystem, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllPortalTabs(portalId={PortalId},includeDeleted={IncludeDeleted},includeSystem={IncludeSystem},toDate={ToDate},fromDate={FromDate})", portalId, includeDeleted, includeSystem, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_Tabs", portalId, includeDeleted, includeSystem, toDate, fromDate);
        }

        /// <summary>Get all tab settings.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabSettings(int tabId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllTabSettings(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllTabSettings(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_TabSettings", tabId, toDate, fromDate);
        }

        /// <summary>Get all tab permissions.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabPermissions(int tabId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllTabPermissions(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllTabPermissions(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_TabPermissions", tabId, toDate, fromDate);
        }

        /// <summary>Get all tab URLs.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabUrls(int tabId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllTabUrls(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllTabUrls(tabId={TabId},toDate={ToDate},fromDate={FromDate})", tabId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_TabUrls", tabId, toDate, fromDate);
        }

        /// <summary>Get all modules for a page.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include deleted modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_Modules", tabId, includeDeleted, toDate, fromDate);
        }

        /// <summary>Get all module settings.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllModuleSettings(int moduleId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllModuleSettings(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllModuleSettings(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_ModuleSettings", moduleId, toDate, fromDate);
        }

        /// <summary>Get all module permissions.</summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllModulePermissions(int moduleId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllModulePermissions(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllModulePermissions(moduleId={ModuleId},toDate={ToDate},fromDate={FromDate})", moduleId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_ModulePermissions", moduleId, toDate, fromDate);
        }

        /// <summary>Get all tab modules.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include deleted tab-modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabModules(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllTabModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllTabModules(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_TabModules", tabId, includeDeleted, toDate, fromDate);
        }

        /// <summary>Check if a tab-module unique ID exists.</summary>
        /// <param name="uniqueId">The unique ID.</param>
        /// <returns><see langword="true"/> if it exists, otherwise <see langword="false"/>.</returns>
        public bool CheckTabModuleUniqueIdExists(Guid uniqueId)
        {
			Logger.LogDebug(">>> Entering CheckTabModuleUniqueIdExists(uniqueId={UniqueId})", uniqueId);
			Logger.LogDebug("<<< Exiting CheckTabModuleUniqueIdExists(uniqueId={UniqueId})", uniqueId);
            return this.dataProvider.ExecuteScalar<int?>("ExportImport_CheckTabModuleUniqueIdExists", uniqueId) > 0;
        }

        /// <summary>Check if a tab unique ID exists.</summary>
        /// <param name="uniqueId">The unique ID.</param>
        /// <returns><see langword="true"/> if it exists, otherwise <see langword="false"/>.</returns>
        public bool CheckTabUniqueIdExists(Guid uniqueId)
        {
			Logger.LogDebug(">>> Entering CheckTabUniqueIdExists(uniqueId={UniqueId})", uniqueId);
			Logger.LogDebug("<<< Exiting CheckTabUniqueIdExists(uniqueId={UniqueId})", uniqueId);
            return this.dataProvider.ExecuteScalar<int?>("ExportImport_CheckTabUniqueIdExists", uniqueId) > 0;
        }

        /// <summary>Get all tab-module settings.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="includeDeleted">Whether to include settings for deleted tab-modules.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllTabModuleSettings(int tabId, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllTabModuleSettings(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllTabModuleSettings(tabId={TabId},includeDeleted={IncludeDeleted},toDate={ToDate},fromDate={FromDate})", tabId, includeDeleted, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_TabModuleSettings", tabId, includeDeleted, toDate, fromDate);
        }

        /// <summary>Sets specific data on the tab.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="isDeleted">Whether the tab is deleted.</param>
        /// <param name="isVisible">Whether the tab is visible.</param>
        public void SetTabSpecificData(int tabId, bool isDeleted, bool isVisible)
        {
			Logger.LogDebug(">>> Entering SetTabSpecificData(tabId={TabId},isDeleted={IsDeleted},isVisible={IsVisible})", tabId, isDeleted, isVisible);
            this.dataProvider.ExecuteNonQuery("Export_SetTabSpecificData", tabId, isDeleted, isVisible);
			Logger.LogDebug("<<< Exiting SetTabSpecificData(tabId={TabId},isDeleted={IsDeleted},isVisible={IsVisible})", tabId, isDeleted, isVisible);
        }

        /// <summary>Sets whether a tab-module is deleted.</summary>
        /// <param name="tabModuleId">The tab-module ID.</param>
        /// <param name="isDeleted">Whether it is deleted.</param>
        public void SetTabModuleDeleted(int tabModuleId, bool isDeleted)
        {
			Logger.LogDebug(">>> Entering SetTabModuleDeleted(tabModuleId={TabModuleId},isDeleted={IsDeleted})", tabModuleId, isDeleted);
            this.dataProvider.ExecuteNonQuery("Export_SetTabModuleDeleted", tabModuleId, isDeleted);
			Logger.LogDebug("<<< Exiting SetTabModuleDeleted(tabModuleId={TabModuleId},isDeleted={IsDeleted})", tabModuleId, isDeleted);
        }

        /// <summary>Sets whether a user is deleted.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="isDeleted">Whether it is deleted.</param>
        public void SetUserDeleted(int portalId, int userId, bool isDeleted)
        {
			Logger.LogDebug(">>> Entering SetUserDeleted(portalId={PortalId},userId={UserId},isDeleted={IsDeleted})", portalId, userId, isDeleted);
            this.dataProvider.ExecuteNonQuery("Export_SetUserDeleted", portalId, userId, isDeleted);
			Logger.LogDebug("<<< Exiting SetUserDeleted(portalId={PortalId},userId={UserId},isDeleted={IsDeleted})", portalId, userId, isDeleted);
        }

        /// <summary>Gets the permission info.</summary>
        /// <param name="permissionCode">The permission code.</param>
        /// <param name="permissionKey">The permission key.</param>
        /// <param name="permissionName">The permission name.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetPermissionInfo(string permissionCode, string permissionKey, string permissionName)
        {
			Logger.LogDebug(">>> Entering GetPermissionInfo(permissionCode={PermissionCode},permissionKey={PermissionKey},permissionName={PermissionName})", permissionCode, permissionKey, permissionName);
			Logger.LogDebug("<<< Exiting GetPermissionInfo(permissionCode={PermissionCode},permissionKey={PermissionKey},permissionName={PermissionName})", permissionCode, permissionKey, permissionName);
            return this.dataProvider.ExecuteReader("Export_GetPermissionInfo", permissionCode, permissionKey, permissionName);
        }

        /// <summary>Update the tab URL changers.</summary>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="seqNum">The sequence number.</param>
        /// <param name="createdBy">The ID of the creating user.</param>
        /// <param name="modifiedBy">The ID of the modifying user.</param>
        public void UpdateTabUrlChangers(int tabId, int seqNum, int? createdBy, int? modifiedBy)
        {
			Logger.LogDebug(">>> Entering UpdateTabUrlChangers(tabId={TabId},seqNum={SeqNum},createdBy={CreatedBy},modifiedBy={ModifiedBy})", tabId, seqNum, createdBy, modifiedBy);
            this.dataProvider.ExecuteNonQuery("Export_UpdateTabUrlChangers", tabId, seqNum, createdBy, modifiedBy);
			Logger.LogDebug("<<< Exiting UpdateTabUrlChangers(tabId={TabId},seqNum={SeqNum},createdBy={CreatedBy},modifiedBy={ModifiedBy})", tabId, seqNum, createdBy, modifiedBy);
        }

        /// <summary>Gets all workflows.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="includeDeleted">Whether to include deleted workflows.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflows(int portalId, bool includeDeleted)
        {
			Logger.LogDebug(">>> Entering GetAllWorkflows(portalId={PortalId},includeDeleted={IncludeDeleted})", portalId, includeDeleted);
			Logger.LogDebug("<<< Exiting GetAllWorkflows(portalId={PortalId},includeDeleted={IncludeDeleted})", portalId, includeDeleted);
            return this.dataProvider.ExecuteReader("Export_ContentWorkflows", portalId, includeDeleted);
        }

        /// <summary>Gets all workflow sources.</summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflowSources(int workflowId)
        {
			Logger.LogDebug(">>> Entering GetAllWorkflowSources(workflowId={WorkflowId})", workflowId);
			Logger.LogDebug("<<< Exiting GetAllWorkflowSources(workflowId={WorkflowId})", workflowId);
            return this.dataProvider.ExecuteReader("Export_ContentWorkflowSources", workflowId);
        }

        /// <summary>Gets all workflow states.</summary>
        /// <param name="workflowId">The workflow ID.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflowStates(int workflowId)
        {
			Logger.LogDebug(">>> Entering GetAllWorkflowStates(workflowId={WorkflowId})", workflowId);
			Logger.LogDebug("<<< Exiting GetAllWorkflowStates(workflowId={WorkflowId})", workflowId);
            return this.dataProvider.ExecuteReader("Export_ContentWorkflowStates", workflowId);
        }

        /// <summary>Gets all workflow state permissions.</summary>
        /// <param name="workflowStateId">The state ID.</param>
        /// <param name="toDate">The end date.</param>
        /// <param name="fromDate">The start date or <see langword="null"/>.</param>
        /// <returns>A data reader.</returns>
        public IDataReader GetAllWorkflowStatePermissions(int workflowStateId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogDebug(">>> Entering GetAllWorkflowStatePermissions(workflowStateId={WorkflowStateId},toDate={ToDate},fromDate={FromDate})", workflowStateId, toDate, fromDate);
			Logger.LogDebug("<<< Exiting GetAllWorkflowStatePermissions(workflowStateId={WorkflowStateId},toDate={ToDate},fromDate={FromDate})", workflowStateId, toDate, fromDate);
            return this.dataProvider.ExecuteReader("Export_ContentWorkflowStatePermissions", workflowStateId, toDate, fromDate);
        }
    }
}
