// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Dto.Taxonomy;
    using Dnn.ExportImport.Dto.Workflow;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    /// <summary>An export service for workflows.</summary>
    public class WorkflowsExportService : BasePortableService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowsExportService));
        /// <inheritdoc />
        public override string Category => Constants.Category_Workflows;

        /// <inheritdoc />
        public override string ParentCategory => null;

        /// <inheritdoc />
        public override uint Priority => 6;

        /// <inheritdoc />
        public override int GetImportTotal()
        {
			Logger.LogError(">>> Entering GetImportTotal()");
            Logger.LogWarning("<<< Exiting GetImportTotal()");
            return this.Repository.GetCount<TaxonomyVocabulary>() + this.Repository.GetCount<TaxonomyTerm>();
        }

        /// <inheritdoc />
        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
			Logger.LogError(">>> Entering ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
			Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): fromDate → {FromDate}", exportJob, exportDto, fromDate);
            var toDate = exportDto.ToDateUtc.ToLocalTime();
			Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): toDate → {ToDate}", exportJob, exportDto, toDate);

            var contentWorkflows = GetWorkflows(exportDto.PortalId, exportDto.IncludeDeletions);
			Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): contentWorkflows → {ContentWorkflows}", exportJob, exportDto, contentWorkflows);
            if (contentWorkflows.Count > 0)
            {
                var defaultWorkflowId = TabWorkflowSettings.Instance.GetDefaultTabWorkflowId(exportDto.PortalId);
				Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): defaultWorkflowId → {DefaultWorkflowId}", exportJob, exportDto, defaultWorkflowId);
                var defaultWorkflow = contentWorkflows.FirstOrDefault(w => w.WorkflowID == defaultWorkflowId);
				Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): defaultWorkflow → {DefaultWorkflow}", exportJob, exportDto, defaultWorkflow);
                defaultWorkflow?.IsDefault = true;

                this.CheckPoint.TotalItems = contentWorkflows.Count;
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("ExportData(exportJob,exportDto)={ExportJob},{ExportDto}: database query executed in {Arg2} ms", exportJob, exportDto, __logStart.ElapsedMilliseconds);
                this.Repository.CreateItems(contentWorkflows);
                this.Result.AddLogEntry("Exported ContentWorkflows", contentWorkflows.Count.ToString(CultureInfo.InvariantCulture));

                foreach (var workflow in contentWorkflows)
                {
                    var contentWorkflowStates = GetWorkflowStates(workflow.WorkflowID);
					Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): contentWorkflowStates → {ContentWorkflowStates}", exportJob, exportDto, contentWorkflowStates);
                    this.Repository.CreateItems(contentWorkflowStates, workflow.Id);

                    foreach (var workflowState in contentWorkflowStates)
                    {
                        var contentWorkflowStatePermissions = GetWorkflowStatePermissions(workflowState.StateID, toDate, fromDate);
						Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): contentWorkflowStatePermissions → {ContentWorkflowStatePermissions}", exportJob, exportDto, contentWorkflowStatePermissions);
                        this.Repository.CreateItems(contentWorkflowStatePermissions, workflowState.Id);
                    }
                }
            }

            this.CheckPoint.Progress = 100;
            this.CheckPoint.Completed = true;
            this.CheckPoint.Stage++;
            this.CheckPoint.StageData = null;
            this.CheckPointStageCallback(this);
			Logger.LogWarning("<<< Exiting ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
        }

        /// <inheritdoc />
        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
			Logger.LogError(">>> Entering ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
            if (this.CheckCancelled(importJob) || this.CheckPoint.Stage >= 1 || this.CheckPoint.Completed || this.CheckPointStageCallback(this))
            {
                return;
            }

            var workflowManager = WorkflowManager.Instance;
            var workflowStateManager = WorkflowStateManager.Instance;
            var portalId = importJob.PortalId;
            var importWorkflows = this.Repository.GetAllItems<ExportWorkflow>().ToList();
			Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): importWorkflows → {ImportWorkflows}", importJob, importDto, importWorkflows);
            var existWorkflows = workflowManager.GetWorkflows(portalId).ToList();
			Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): existWorkflows → {ExistWorkflows}", importJob, importDto, existWorkflows);
            var defaultTabWorkflowId = importWorkflows.FirstOrDefault(w => w.IsDefault)?.WorkflowID ?? 1;
			Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): defaultTabWorkflowId → {DefaultTabWorkflowId}", importJob, importDto, defaultTabWorkflowId);
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? importWorkflows.Count : this.CheckPoint.TotalItems;
            foreach (var importWorkflow in importWorkflows)
            {
                var workflow = existWorkflows.FirstOrDefault(w => w.WorkflowName == importWorkflow.WorkflowName);
				Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): workflow → {Workflow}", importJob, importDto, workflow);
                if (workflow != null)
                {
                    if (!importWorkflow.IsSystem && importDto.CollisionResolution == CollisionResolution.Overwrite)
                    {
                        if (workflow.Description != importWorkflow.Description ||
                            workflow.WorkflowKey != importWorkflow.WorkflowKey)
                        {
                            workflow.Description = importWorkflow.Description;
							Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): workflow.Description → {Arg2}", importJob, importDto, workflow.Description);
                            workflow.WorkflowKey = importWorkflow.WorkflowKey;
							Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): workflow.WorkflowKey → {Arg2}", importJob, importDto, workflow.WorkflowKey);
                            workflowManager.UpdateWorkflow(workflow);
                            this.Result.AddLogEntry("Updated workflow", workflow.WorkflowName);
                        }
                    }
                }
                else
                {
                    workflow = new Workflow
                    {
                        PortalID = portalId,
                        WorkflowName = importWorkflow.WorkflowName,
                        Description = importWorkflow.Description,
                        WorkflowKey = importWorkflow.WorkflowKey,
                    };

                    workflowManager.AddWorkflow(workflow);
                    this.Result.AddLogEntry("Added workflow", workflow.WorkflowName);

                    if (importWorkflow.WorkflowID == defaultTabWorkflowId)
                    {
                        TabWorkflowSettings.Instance.SetDefaultTabWorkflowId(portalId, workflow.WorkflowID);
                    }
                }

                importWorkflow.LocalId = workflow.WorkflowID;
				Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): importWorkflow.LocalId → {Arg2}", importJob, importDto, importWorkflow.LocalId);
                var importStates = this.Repository.GetRelatedItems<ExportWorkflowState>(importWorkflow.Id).ToList();
				Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): importStates → {ImportStates}", importJob, importDto, importStates);
                foreach (var importState in importStates)
                {
                    var workflowState = workflow.States.FirstOrDefault(s => s.StateName == importState.StateName);
					Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): workflowState → {WorkflowState}", importJob, importDto, workflowState);
                    if (workflowState != null)
                    {
                        if (!workflowState.IsSystem)
                        {
                            workflowState.Order = importState.Order;
							Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): workflowState.Order → {Arg2}", importJob, importDto, workflowState.Order);
							Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): workflowState.SendNotification → {Arg2}", importJob, importDto, workflowState.SendNotification);
							Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): workflowState.SendNotificationToAdministrators → {Arg2}", importJob, importDto, workflowState.SendNotificationToAdministrators);
                            workflowState.IsSystem = false;
							Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): workflowState.IsSystem → {Arg2}", importJob, importDto, workflowState.IsSystem);
                            workflowState.SendNotification = importState.SendNotification;
                            workflowState.SendNotificationToAdministrators = importState.SendNotificationToAdministrators;
                            workflowStateManager.UpdateWorkflowState(workflowState);
                            this.Result.AddLogEntry("Updated workflow state", workflowState.StateID.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    else
                    {
                        workflowState = new WorkflowState
                        {
                            StateName = importState.StateName,
                            WorkflowID = workflow.WorkflowID,
                            Order = importState.Order,
                            IsSystem = importState.IsSystem,
                            SendNotification = importState.SendNotification,
                            SendNotificationToAdministrators = importState.SendNotificationToAdministrators,
                        };
                        WorkflowStateManager.Instance.AddWorkflowState(workflowState);
                        this.Result.AddLogEntry("Added workflow state", workflowState.StateID.ToString(CultureInfo.InvariantCulture));
                    }

                    importState.LocalId = workflowState.StateID;
					Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): importState.LocalId → {Arg2}", importJob, importDto, importState.LocalId);
                    if (!workflowState.IsSystem)
                    {
                        var importPermissions = this.Repository.GetRelatedItems<ExportWorkflowStatePermission>(importState.Id).ToList();
						Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): importPermissions → {ImportPermissions}", importJob, importDto, importPermissions);
                        foreach (var importPermission in importPermissions)
                        {
                            var permissionId = DataProvider.Instance().GetPermissionId(
                                importPermission.PermissionCode, importPermission.PermissionKey, importPermission.PermissionName);

                            if (permissionId != null)
                            {
                                var noRole = Convert.ToInt32(Globals.glbRoleNothing, CultureInfo.InvariantCulture);
								Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): noRole → {NoRole}", importJob, importDto, noRole);
                                var userId = UserController.GetUserByName(importDto.PortalId, importPermission.Username)?.UserID;
								Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): userId → {UserId}", importJob, importDto, userId);
                                var roleId = Util.GetRoleIdByName(importDto.PortalId, importPermission.RoleID ?? noRole, importPermission.RoleName);
								Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): roleId → {RoleId}", importJob, importDto, roleId);

                                IPermissionInfo permission = new WorkflowStatePermission
                                {
                                    StateID = workflowState.StateID,
                                    AllowAccess = importPermission.AllowAccess,
                                };
                                permission.PermissionId = permissionId ?? -1;
								Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): permission.PermissionId → {Arg2}", importJob, importDto, permission.PermissionId);
								Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): permission.UserId → {Arg2}", importJob, importDto, permission.UserId);
                                permission.RoleId = noRole;
								Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): permission.RoleId → {Arg2}", importJob, importDto, permission.RoleId);
                                permission.UserId = -1;

                                if (importPermission.UserID is > 0 && !string.IsNullOrEmpty(importPermission.Username))
                                {
                                    if (userId == null)
                                    {
                                        this.Result.AddLogEntry(
                                            "Couldn't add tab permission; User is undefined!",
                                            $"{importPermission.PermissionKey} - {importPermission.PermissionID}",
                                            ReportLevel.Warn);
                                        continue;
                                    }

                                    permission.UserId = userId.Value;
                                }

                                if (importPermission.RoleID != null && importPermission.RoleID > noRole && !string.IsNullOrEmpty(importPermission.RoleName))
                                {
                                    if (roleId == null)
                                    {
                                        this.Result.AddLogEntry(
                                            "Couldn't add tab permission; Role is undefined!",
                                            $"{importPermission.PermissionKey} - {importPermission.PermissionID}",
                                            ReportLevel.Warn);
                                        continue;
                                    }

                                    permission.RoleId = roleId.Value;
                                }

                                try
                                {
                                    var existingPermissions = workflowStateManager.GetWorkflowStatePermissionByState(workflowState.StateID);
									Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): existingPermissions → {ExistingPermissions}", importJob, importDto, existingPermissions);
                                    var local = existingPermissions.FirstOrDefault(
                                        x => x.PermissionCode == importPermission.PermissionCode && x.PermissionKey == importPermission.PermissionKey
                                        && x.PermissionName.Equals(importPermission.PermissionName, StringComparison.OrdinalIgnoreCase) &&
                                        ((IPermissionInfo)x).RoleId == roleId && ((IPermissionInfo)x).UserId == userId);

                                    if (local == null)
                                    {
                                        var workflowStatePermission = (WorkflowStatePermission)permission;
										Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): workflowStatePermission → {WorkflowStatePermission}", importJob, importDto, workflowStatePermission);
                                        workflowStateManager.AddWorkflowStatePermission(workflowStatePermission, -1);
                                        importPermission.LocalId = workflowStatePermission.WorkflowStatePermissionID;
										Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): importPermission.LocalId → {Arg2}", importJob, importDto, importPermission.LocalId);
                                        this.Result.AddLogEntry(
                                            "Added workflow state permission",
                                            workflowStatePermission.WorkflowStatePermissionID.ToString(CultureInfo.InvariantCulture));
                                    }
                                    else
                                    {
                                        importPermission.LocalId = local.WorkflowStatePermissionID;
                                    }
                                }
                                catch (Exception ex)
                                {
									Logger.LogError("Exception in ImportData(importJob={ImportJob},importDto={ImportDto}): {Arg2}", importJob, importDto, ex.Message);
                                    this.Result.AddLogEntry("Exception adding workflow state permission", ex.Message, ReportLevel.Error);
                                }
                            }
                        }
                    }
                }

				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogError("ImportData(importJob,importDto)={ImportJob},{ImportDto}: update query executed in {Arg2} ms", importJob, importDto, __logStart.ElapsedMilliseconds);
                this.Repository.UpdateItems(importStates);
                this.Result.AddSummary("Imported Workflow", importWorkflows.Count.ToString(CultureInfo.InvariantCulture));
                this.CheckPoint.ProcessedItems++;
                this.CheckPointStageCallback(this); // no need to return; very small amount of data processed
            }

            this.Repository.UpdateItems(importWorkflows);

            this.CheckPoint.Stage++;
            this.CheckPoint.StageData = null;
            this.CheckPoint.Progress = 100;
            this.CheckPoint.TotalItems = importWorkflows.Count;
            this.CheckPoint.ProcessedItems = importWorkflows.Count;
            this.CheckPointStageCallback(this);
			Logger.LogWarning("<<< Exiting ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
        }

        private static List<ExportWorkflow> GetWorkflows(int portalId, bool includeDeletions)
        {
			Logger.LogError(">>> Entering GetWorkflows(portalId={PortalId},includeDeletions={IncludeDeletions})", portalId, includeDeletions);
            Logger.LogWarning("<<< Exiting GetWorkflows(portalId={PortalId},includeDeletions={IncludeDeletions})", portalId, includeDeletions);
            return CBO.FillCollection<ExportWorkflow>(
                DataProvider.Instance().GetAllWorkflows(portalId, includeDeletions));
        }

        private static List<ExportWorkflowState> GetWorkflowStates(int workflowId)
        {
			Logger.LogError(">>> Entering GetWorkflowStates(workflowId={WorkflowId})", workflowId);
            Logger.LogWarning("<<< Exiting GetWorkflowStates(workflowId={WorkflowId})", workflowId);
            return CBO.FillCollection<ExportWorkflowState>(
                DataProvider.Instance().GetAllWorkflowStates(workflowId));
        }

        private static List<ExportWorkflowStatePermission> GetWorkflowStatePermissions(
            int stateId, DateTime toDate, DateTime? fromDate)
        {
			Logger.LogError(">>> Entering GetWorkflowStatePermissions(stateId={StateId},toDate={ToDate},fromDate={FromDate})", stateId, toDate, fromDate);
            Logger.LogWarning("<<< Exiting GetWorkflowStatePermissions(stateId={StateId},toDate={ToDate},fromDate={FromDate})", stateId, toDate, fromDate);
            return CBO.FillCollection<ExportWorkflowStatePermission>(
                DataProvider.Instance().GetAllWorkflowStatePermissions(stateId, toDate, fromDate));
        }
    }
}
