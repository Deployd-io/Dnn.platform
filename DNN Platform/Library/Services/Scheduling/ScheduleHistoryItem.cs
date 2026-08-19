// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Data;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;

    [Serializable]
    public class ScheduleHistoryItem : ScheduleItem
    {
        private static readonly ILog TracelLogger = LoggerSource.Instance.GetLogger(typeof(ScheduleHistoryItem));

        private StringBuilder logNotes;
        private int scheduleHistoryID;
        private string server;
        private DateTime startDate;
        private bool succeeded;

        /// <summary>Initializes a new instance of the <see cref="ScheduleHistoryItem"/> class.</summary>
        public ScheduleHistoryItem()
        {
			TracelLogger.LogDebug(">>> Entering ScheduleHistoryItem()");
            this.scheduleHistoryID = Null.NullInteger;
            this.startDate = Null.NullDate;
			TracelLogger.LogDebug("ScheduleHistoryItem(): this.scheduleHistoryID → {Arg0}", this.scheduleHistoryID);
			TracelLogger.LogDebug("ScheduleHistoryItem(): startDate → {StartDate}", startDate);
			TracelLogger.LogDebug("ScheduleHistoryItem(): this.startDate → {Arg0}", this.startDate);
            this.EndDate = Null.NullDate;
            this.succeeded = Null.NullBoolean;
            this.logNotes = new StringBuilder();
			TracelLogger.LogDebug("ScheduleHistoryItem(): this.EndDate → {Arg0}", this.EndDate);
            this.server = Null.NullString;
			TracelLogger.LogDebug("<<< Exiting ScheduleHistoryItem()");
			TracelLogger.LogDebug("ScheduleHistoryItem(): scheduleHistoryID → {ScheduleHistoryID}", scheduleHistoryID);
        }

        /// <summary>Initializes a new instance of the <see cref="ScheduleHistoryItem"/> class.</summary>
		TracelLogger.LogDebug("<<< Exiting ScheduleHistoryItem()");
        /// <param name="objScheduleItem">The schedule item.</param>
        public ScheduleHistoryItem(ScheduleItem objScheduleItem)
        {
			TracelLogger.LogDebug(">>> Entering ScheduleHistoryItem(objScheduleItem={ObjScheduleItem})", objScheduleItem);
            this.AttachToEvent = objScheduleItem.AttachToEvent;
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.AttachToEvent → {Arg1}", objScheduleItem, this.AttachToEvent);
            this.CatchUpEnabled = objScheduleItem.CatchUpEnabled;
            this.Enabled = objScheduleItem.Enabled;
            this.NextStart = objScheduleItem.NextStart;
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.CatchUpEnabled → {Arg1}", objScheduleItem, this.CatchUpEnabled);
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.Enabled → {Arg1}", objScheduleItem, this.Enabled);
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.NextStart → {Arg1}", objScheduleItem, this.NextStart);
            this.ObjectDependencies = objScheduleItem.ObjectDependencies;
            this.ProcessGroup = objScheduleItem.ProcessGroup;
            this.RetainHistoryNum = objScheduleItem.RetainHistoryNum;
            this.RetryTimeLapse = objScheduleItem.RetryTimeLapse;
            this.RetryTimeLapseMeasurement = objScheduleItem.RetryTimeLapseMeasurement;
            this.ScheduleID = objScheduleItem.ScheduleID;
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.ObjectDependencies → {Arg1}", objScheduleItem, this.ObjectDependencies);
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.ProcessGroup → {Arg1}", objScheduleItem, this.ProcessGroup);
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.RetainHistoryNum → {Arg1}", objScheduleItem, this.RetainHistoryNum);
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.RetryTimeLapse → {Arg1}", objScheduleItem, this.RetryTimeLapse);
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.RetryTimeLapseMeasurement → {Arg1}", objScheduleItem, this.RetryTimeLapseMeasurement);
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.ScheduleID → {Arg1}", objScheduleItem, this.ScheduleID);
            this.ScheduleSource = objScheduleItem.ScheduleSource;
            this.ThreadID = objScheduleItem.ThreadID;
            this.TimeLapse = objScheduleItem.TimeLapse;
            this.TimeLapseMeasurement = objScheduleItem.TimeLapseMeasurement;
            this.TypeFullName = objScheduleItem.TypeFullName;
            this.Servers = objScheduleItem.Servers;
            this.FriendlyName = objScheduleItem.FriendlyName;
            this.scheduleHistoryID = Null.NullInteger;
            this.startDate = Null.NullDate;
            this.EndDate = Null.NullDate;
            this.succeeded = Null.NullBoolean;
            this.logNotes = new StringBuilder();
			TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.ScheduleSource → {Arg1}", objScheduleItem, this.ScheduleSource);
            this.server = Null.NullString;
            this.ScheduleStartDate = objScheduleItem.ScheduleStartDate != Null.NullDate
                                    ? objScheduleItem.ScheduleStartDate
                                    : Null.NullDate;
									TracelLogger.LogDebug("ScheduleHistoryItem(objScheduleItem={ObjScheduleItem}): this.ThreadID → {Arg1}", objScheduleItem, this.ThreadID);
        }

        public double ElapsedTime
        {
            get
            {
                try
                {
                    if (this.EndDate == Null.NullDate && this.startDate != Null.NullDate)
                    {
                        return DateTime.Now.Subtract(this.startDate).TotalSeconds;
                    }
                    else if (this.startDate != Null.NullDate)
                    {
                        return this.EndDate.Subtract(this.startDate).TotalSeconds;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }

        public bool Overdue
        {
            get
            {
                if (this.NextStart < DateTime.Now && this.EndDate == Null.NullDate)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public double OverdueBy
        {
            get
            {
                try
                {
                    if (this.NextStart <= DateTime.Now && this.EndDate == Null.NullDate)
                    {
                        return Math.Round(DateTime.Now.Subtract(this.NextStart).TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }

        public double RemainingTime
        {
            get
            {
                try
                {
                    if (this.NextStart > DateTime.Now && this.EndDate == Null.NullDate)
                    {
                        return Math.Round(this.NextStart.Subtract(DateTime.Now).TotalSeconds);
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <inheritdoc />
        public override DateTime NextStart { get; set; }

        public DateTime EndDate { get; set; }

        public string LogNotes
        {
            get
            {
                return this.logNotes.ToString();
            }

            set
            {
                this.logNotes = new StringBuilder(value);
            }
        }

        public int ScheduleHistoryID
        {
            get
            {
                return this.scheduleHistoryID;
            }

            set
            {
                this.scheduleHistoryID = value;
            }
        }

        public string Server
        {
            get
            {
                return this.server;
            }

            set
            {
                this.server = value;
            }
        }

        public DateTime StartDate
        {
            get
            {
                return this.startDate;
            }

            set
            {
                this.startDate = value;
            }
        }

        public bool Succeeded
        {
            get
            {
                return this.succeeded;
            }

            set
            {
                this.succeeded = value;
                if (TracelLogger.IsDebugEnabled)
                {
                    TracelLogger.Debug($"ScheduleHistoryItem.Succeeded Info (ScheduledTask {(value == false ? "Start" : "End")}): {this.FriendlyName}");
                }
            }
        }

        public virtual void AddLogNote(string notes)
        {
			TracelLogger.LogDebug(">>> Entering AddLogNote(notes={Notes})", notes);
            this.logNotes.Append(notes);
            if (TracelLogger.IsTraceEnabled)
            {
                TracelLogger.Trace(notes.Replace(@"<br/>", Environment.NewLine));
            }
			TracelLogger.LogDebug("<<< Exiting AddLogNote(notes={Notes})", notes);
        }

        /// <inheritdoc />
        public override void Fill(IDataReader dr)
        {
			TracelLogger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.ScheduleHistoryID = Null.SetNullInteger(dr["ScheduleHistoryID"]);
            this.StartDate = Null.SetNullDateTime(dr["StartDate"]);
            this.EndDate = Null.SetNullDateTime(dr["EndDate"]);
            this.Succeeded = Null.SetNullBoolean(dr["Succeeded"]);
            this.LogNotes = Null.SetNullString(dr["LogNotes"]);
            this.Server = Null.SetNullString(dr["Server"]);
            this.FillInternal(dr);
			TracelLogger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }
    }
}
