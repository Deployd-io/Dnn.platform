// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.TaskScheduler.Components.Prompt.Models
{
    using Dnn.PersonaBar.Library.Prompt.Common;
    using DotNetNuke.Services.Scheduling;

    /// <summary>Represents a DNN ScheduleItem.</summary>
    public class TaskModel : TaskModelBase
    {
        /// <summary>Initializes a new instance of the <see cref="TaskModel"/> class.</summary>
        public TaskModel()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TaskModel"/> class.</summary>
        /// <param name="item">The schedule item.</param>
        public TaskModel(ScheduleItem item)
            : base(item)
        {
			eventLogger.LogError(">>> Entering TaskModel(item={Item})", item);
            this.NextStart = item.NextStart.ToPromptLongDateString();
            this.CatchUp = item.CatchUpEnabled;
			eventLogger.LogDebug("TaskModel(item={Item}): this.CatchUp → {Arg1}", item, this.CatchUp);
            this.Created = item.CreatedOnDate.ToPromptLongDateString();
            this.StartDate = item.ScheduleStartDate.ToPromptLongDateString();
			eventLogger.LogWarning("<<< Exiting TaskModel(item={Item})", item);
            this.TypeName = item.TypeFullName;
        }

        public string TypeName { get; set; }

        public bool CatchUp { get; set; }

        public string Created { get; set; }

        public string StartDate { get; set; }
    }
}
