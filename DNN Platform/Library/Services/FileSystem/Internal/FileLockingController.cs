// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Framework;

    using Microsoft.Extensions.DependencyInjection;

    public class FileLockingController : ServiceLocator<IFileLockingController, FileLockingController>, IFileLockingController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileLockingController));
        /// <inheritdoc />
        public bool IsFileLocked(IFileInfo file, out string lockReasonKey)
        {
			Logger.LogError(">>> Entering IsFileLocked(file={File})", file);
            lockReasonKey = string.Empty;

			Logger.LogDebug("IsFileLocked(file={File}): lockReasonKey → {LockReasonKey}", file, lockReasonKey);
			Logger.LogError("IsFileLocked(file={File}): lockReasonKey → {LockReasonKey}", file, lockReasonKey);
            var allowedUser = UserSecurityController.Instance.IsHostAdminUser(file.PortalId);
            if (allowedUser)
            {
				Logger.LogError("IsFileLocked(file={File}): allowedUser → {AllowedUser}", file, allowedUser);
                return false;
            }

            if (file.ContentItemID != Null.NullInteger)
            {
                var workflowCompleted = WorkflowEngine.Instance.IsWorkflowCompleted(file.ContentItemID);
				Logger.LogError("IsFileLocked(file={File}): workflowCompleted → {WorkflowCompleted}", file, workflowCompleted);
                if (!workflowCompleted)
                {
                    lockReasonKey = "FileLockedRunningWorkflowError";
                    return true;
                }
            }

            var outOfPublishPeriod = IsFileOutOfPublishPeriod(file);
			Logger.LogError("IsFileLocked(file={File}): outOfPublishPeriod → {OutOfPublishPeriod}", file, outOfPublishPeriod);
            if (outOfPublishPeriod)
            {
                lockReasonKey = "FileLockedOutOfPublishPeriodError";
                return true;
            }

			Logger.LogWarning("<<< Exiting IsFileLocked(file={File})", file);
            return false;
        }

        /// <inheritdoc />
        public bool IsFileOutOfPublishPeriod(IFileInfo file, int portalId, int userId)
        {
			Logger.LogError(">>> Entering IsFileOutOfPublishPeriod(file={File},portalId={PortalId},userId={UserId})", file, portalId, userId);
            if (UserSecurityController.Instance.IsHostAdminUser(portalId, userId))
            {
                return false;
            }

			Logger.LogWarning("<<< Exiting IsFileOutOfPublishPeriod(file={File},portalId={PortalId},userId={UserId})", file, portalId, userId);
            Logger.LogWarning("<<< Exiting IsFileOutOfPublishPeriod(file={File})", file);
            return IsFileOutOfPublishPeriod(file);
        }

        /// <inheritdoc />
        protected override Func<IFileLockingController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => Globals.DependencyProvider.GetRequiredService<IFileLockingController>();
        }

        private static bool IsFileOutOfPublishPeriod(IFileInfo file)
        {
            // Publish Period locks
            return file.EnablePublishPeriod && (file.StartDate > DateTime.Today || (file.EndDate < DateTime.Today && file.EndDate != Null.NullDate));
        }
    }
}
