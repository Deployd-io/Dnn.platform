// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models
{
    using Dnn.PersonaBar.Library.Prompt.Common;
    using DotNetNuke.Services.Scheduling;

    public class TaskModelBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public TaskModelBase()
        {
		Logger.LogDebug(">>> Entering TaskModelBase()");
		Logger.LogDebug("<<< Exiting TaskModelBase()");
        }

        public TaskModelBase(ScheduleItem item)
        {
			Logger.LogDebug(">>> Entering TaskModelBase(item={Item})", item);
            this.Enabled = item.Enabled;
            this.FriendlyName = item.FriendlyName;
			Logger.LogDebug("TaskModelBase(item={Item}): this.Enabled → {Arg1}", item, this.Enabled);
			Logger.LogDebug("TaskModelBase(item={Item}): this.FriendlyName → {Arg1}", item, this.FriendlyName);
            this.NextStart = item.NextStart.ToPromptShortDateAndTimeString();
            this.ScheduleId = item.ScheduleID;
			Logger.LogDebug("<<< Exiting TaskModelBase(item={Item})", item);
			Logger.LogDebug("TaskModelBase(item={Item}): this.ScheduleId → {Arg1}", item, this.ScheduleId);
        }

        public int ScheduleId { get; set; }

        public string FriendlyName { get; set; }

        public string NextStart { get; set; }

        public bool Enabled { get; set; }
    }
}
