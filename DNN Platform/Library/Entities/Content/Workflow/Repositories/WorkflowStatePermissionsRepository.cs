// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Framework;

    internal class WorkflowStatePermissionsRepository : ServiceLocator<IWorkflowStatePermissionsRepository, WorkflowStatePermissionsRepository>, IWorkflowStatePermissionsRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WorkflowStatePermissionsRepository));
        /// <inheritdoc />
        public IEnumerable<WorkflowStatePermission> GetWorkflowStatePermissionByState(int stateId)
        {
			Logger.LogError(">>> Entering GetWorkflowStatePermissionByState(stateId={StateId})", stateId);
            Logger.LogWarning("<<< Exiting GetWorkflowStatePermissionByState(stateId={StateId})", stateId);
            return CBO.FillCollection<WorkflowStatePermission>(DataProvider.Instance().GetContentWorkflowStatePermissionsByStateID(stateId));
        }

        /// <inheritdoc />
        public int AddWorkflowStatePermission(WorkflowStatePermission permission, int lastModifiedByUserId)
        {
			Logger.LogError(">>> Entering AddWorkflowStatePermission(permission={Permission},lastModifiedByUserId={LastModifiedByUserId})", permission, lastModifiedByUserId);
            Logger.LogWarning("<<< Exiting AddWorkflowStatePermission(permission={Permission},lastModifiedByUserId={LastModifiedByUserId})", permission, lastModifiedByUserId);
            return DataProvider.Instance().AddContentWorkflowStatePermission(
                permission.StateID,
                permission.PermissionID,
                permission.RoleID,
                permission.AllowAccess,
                permission.UserID,
                lastModifiedByUserId);
        }

        /// <inheritdoc />
        public void DeleteWorkflowStatePermission(int workflowStatePermissionId)
        {
			Logger.LogError(">>> Entering DeleteWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
            DataProvider.Instance().DeleteContentWorkflowStatePermission(workflowStatePermissionId);
			Logger.LogWarning("<<< Exiting DeleteWorkflowStatePermission(workflowStatePermissionId={WorkflowStatePermissionId})", workflowStatePermissionId);
        }

        /// <inheritdoc />
        protected override Func<IWorkflowStatePermissionsRepository> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new WorkflowStatePermissionsRepository();
        }
    }
}
