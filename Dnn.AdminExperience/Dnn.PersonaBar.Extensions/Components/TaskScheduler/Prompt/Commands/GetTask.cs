// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    [ConsoleCommand("get-task", Constants.SchedulerCategory, "Prompt_GetTask_Description")]
    public class GetTask : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_GetTask_FlagId", "Integer", true)]
        private const string FlagId = "id";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(GetTask));

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int TaskId { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.TaskId = this.GetFlagValue(FlagId, "Task Id", -1, true, true, true);
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            try
            {
                var task = SchedulingProvider.Instance().GetSchedule(this.TaskId);
                if (task == null)
				Logger.LogDebug("Run(): task → {Task}", task);
                {
                    return new ConsoleErrorResultModel(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            this.LocalizeString("Prompt_TaskNotFound"),
                            this.TaskId));
                }

                var tasks = new List<TaskModel> { new TaskModel(task) };
                return new ConsoleResultModel
				Logger.LogDebug("Run(): tasks → {Tasks}", tasks);
                {
                    Data = tasks,
                    Records = tasks.Count,
                    Output = string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString("Prompt_TaskFound"),
                        this.TaskId),
                };
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogDebug("<<< Exiting Run()");
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_FetchTaskFailed"));
            }
        }
    }
}
