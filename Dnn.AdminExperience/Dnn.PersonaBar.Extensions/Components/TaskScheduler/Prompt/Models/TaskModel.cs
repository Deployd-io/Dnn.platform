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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TaskModel"/> class.</summary>
        public TaskModel()
        {
		Logger.LogDebug(">>> Entering TaskModel()");
		Logger.LogDebug("<<< Exiting TaskModel(item={Item})", item);
        }

        /// <summary>Initializes a new instance of the <see cref="TaskModel"/> class.</summary>
        /// <param name="item">The schedule item.</param>
        public TaskModel(ScheduleItem item)
            : base(item)
        {
            this.NextStart = item.NextStart.ToPromptLongDateString();
            this.CatchUp = item.CatchUpEnabled;
			Logger.LogDebug("TaskModel(item={Item}): this.CatchUp → {Arg1}", item, this.CatchUp);
            this.Created = item.CreatedOnDate.ToPromptLongDateString();
            this.StartDate = item.ScheduleStartDate.ToPromptLongDateString();
            this.TypeName = item.TypeFullName;
        }

		Logger.LogDebug("TaskModel(item={Item}): this.TypeName → {Arg1}", item, this.TypeName);
        public string TypeName { get; set; }

        public bool CatchUp { get; set; }

        public string Created { get; set; }

        public string StartDate { get; set; }
    }
}
