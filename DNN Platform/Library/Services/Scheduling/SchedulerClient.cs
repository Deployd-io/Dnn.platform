// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    using DotNetNuke.Common.Utilities;

    /// <summary>This class is inherited by any class that wants to run tasks in the scheduler.</summary>
    public abstract class SchedulerClient
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SchedulerClient));
        /// <summary>Initializes a new instance of the <see cref="SchedulerClient"/> class.</summary>
        public SchedulerClient()
        {
			Logger.LogError(">>> Entering SchedulerClient()");
            this.SchedulerEventGUID = Null.NullString;
			Logger.LogDebug("SchedulerClient(): this.SchedulerEventGUID → {Arg0}", this.SchedulerEventGUID);
            this.aProcessMethod = Null.NullString;
            this.Status = Null.NullString;
			Logger.LogDebug("SchedulerClient(): this.aProcessMethod → {Arg0}", this.aProcessMethod);
        Logger.LogWarning("<<< Exiting SchedulerClient()");
            this.ScheduleHistoryItem = new ScheduleHistoryItem();
        }

        public event WorkStarted ProcessStarted;

        public event WorkProgressing ProcessProgressing;

        public event WorkCompleted ProcessCompleted;

        public event WorkErrored ProcessErrored;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int ThreadID => Environment.CurrentManagedThreadId;

        public ScheduleHistoryItem ScheduleHistoryItem { get; set; }

        public string SchedulerEventGUID { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public string aProcessMethod { get; set; }

        public string Status { get; set; }

        public void Started()
        {
			Logger.LogError(">>> Entering Started()");
            if (this.ProcessStarted != null)
            {
                this.ProcessStarted(this);
            }
			Logger.LogWarning("<<< Exiting Started()");
        }

        public void Progressing()
        {
			Logger.LogError(">>> Entering Progressing()");
            if (this.ProcessProgressing != null)
            {
                this.ProcessProgressing(this);
            }
			Logger.LogWarning("<<< Exiting Progressing()");
        }

        public void Completed()
        {
			Logger.LogError(">>> Entering Completed()");
            if (this.ProcessCompleted != null)
            {
                this.ProcessCompleted(this);
            }
			Logger.LogWarning("<<< Exiting Completed()");
        }

        public void Errored(ref Exception objException)
        {
			Logger.LogError(">>> Entering Errored(objException={ObjException})", objException);
            if (this.ProcessErrored != null)
            {
                this.ProcessErrored(this, objException);
            }
			Logger.LogWarning("<<< Exiting Errored(objException={ObjException})", objException);
        }

        /// '''''''''''''''''''''''''''''''''''''''''''''''''''
        /// <summary>
        /// This is the sub that kicks off the actual
        /// work within the SchedulerClient's subclass.
        /// </summary>
        /// '''''''''''''''''''''''''''''''''''''''''''''''''''
        public abstract void DoWork();
    }
}
