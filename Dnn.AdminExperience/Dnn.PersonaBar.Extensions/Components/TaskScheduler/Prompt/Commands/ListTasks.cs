// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Commands
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("list-tasks", Constants.SchedulerCategory, "Prompt_ListTasks_Description")]
    public class ListTasks : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [FlagParameter("enabled", "Prompt_ListTasks_FlagEnabled", "Boolean")]
        private const string FlagEnabled = "enabled";

        [FlagParameter("name", "Prompt_ListTasks_FlagName", "String")]
        private const string FlagName = "name";

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private bool? Enabled { get; set; }

        private string TaskName { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.Enabled = this.GetFlagValue<bool?>(FlagEnabled, "Enabled", null, false, true);
            this.TaskName = this.GetFlagValue(FlagName, "Task Name", string.Empty, false, !this.Enabled.HasValue);
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            var controller = new TaskSchedulerController();
            var tasks = new List<TaskModelBase>();
			Logger.LogDebug("Run(): controller → {Controller}", controller);
			Logger.LogDebug("Run(): tasks → {Tasks}", tasks);
            var schedulerItems = controller.GetScheduleItems(this.Enabled, string.Empty, this.TaskName?.Replace("*", string.Empty));
            tasks.AddRange(schedulerItems.Select(x => new TaskModelBase(x)));
			Logger.LogDebug("Run(): schedulerItems → {SchedulerItems}", schedulerItems);
			Logger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel(string.Format(CultureInfo.InvariantCulture, this.LocalizeString("Prompt_TasksFound"), tasks.Count))
            {
                Data = tasks,
                Records = tasks.Count,
            };
        }
    }
}
