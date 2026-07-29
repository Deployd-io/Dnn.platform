// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// <summary>Represents one item in the scheduler.</summary>
    [Serializable]
    public class ScheduleItem : BaseEntityInfo, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly DateTime MinNextTime = DateTime.Now;
        private DateTime? nextStart;
        private Hashtable scheduleItemSettings;

        /// <summary>Initializes a new instance of the <see cref="ScheduleItem"/> class.</summary>
        public ScheduleItem()
        {
			Logger.LogDebug(">>> Entering ScheduleItem()");
            this.ScheduleID = Null.NullInteger;
            this.TypeFullName = Null.NullString;
			Logger.LogDebug("ScheduleItem(): this.ScheduleID → {Arg0}", this.ScheduleID);
			Logger.LogDebug("ScheduleItem(): this.TypeFullName → {Arg0}", this.TypeFullName);
            this.TimeLapse = Null.NullInteger;
            this.TimeLapseMeasurement = Null.NullString;
            this.RetryTimeLapse = Null.NullInteger;
			Logger.LogDebug("ScheduleItem(): this.TimeLapse → {Arg0}", this.TimeLapse);
			Logger.LogDebug("ScheduleItem(): this.TimeLapseMeasurement → {Arg0}", this.TimeLapseMeasurement);
			Logger.LogDebug("ScheduleItem(): this.RetryTimeLapse → {Arg0}", this.RetryTimeLapse);
            this.RetryTimeLapseMeasurement = Null.NullString;
            this.ObjectDependencies = Null.NullString;
            this.RetainHistoryNum = Null.NullInteger;
            this.CatchUpEnabled = Null.NullBoolean;
            this.Enabled = Null.NullBoolean;
            this.AttachToEvent = Null.NullString;
			Logger.LogDebug("ScheduleItem(): this.RetryTimeLapseMeasurement → {Arg0}", this.RetryTimeLapseMeasurement);
			Logger.LogDebug("ScheduleItem(): this.ObjectDependencies → {Arg0}", this.ObjectDependencies);
			Logger.LogDebug("ScheduleItem(): this.RetainHistoryNum → {Arg0}", this.RetainHistoryNum);
			Logger.LogDebug("ScheduleItem(): this.CatchUpEnabled → {Arg0}", this.CatchUpEnabled);
			Logger.LogDebug("ScheduleItem(): this.Enabled → {Arg0}", this.Enabled);
			Logger.LogDebug("ScheduleItem(): this.AttachToEvent → {Arg0}", this.AttachToEvent);
            this.ThreadID = Null.NullInteger;
            this.ProcessGroup = Null.NullInteger;
            this.Servers = Null.NullString;
            this.ScheduleStartDate = Null.NullDate;
			Logger.LogDebug("<<< Exiting ScheduleItem()");
			Logger.LogDebug("ScheduleItem(): this.ThreadID → {Arg0}", this.ThreadID);
			Logger.LogDebug("ScheduleItem(): this.ProcessGroup → {Arg0}", this.ProcessGroup);
			Logger.LogDebug("ScheduleItem(): this.Servers → {Arg0}", this.Servers);
			Logger.LogDebug("ScheduleItem(): this.ScheduleStartDate → {Arg0}", this.ScheduleStartDate);
        }

        /// <summary>Gets or sets the event this item attaches to.</summary>
        public string AttachToEvent { get; set; }

        /// <summary>Gets or sets a value indicating whether catch-up is enabled.</summary>
        public bool CatchUpEnabled { get; set; }

        /// <summary>Gets or sets a value indicating whether the item is enabled.</summary>
        public bool Enabled { get; set; }

        /// <summary>Gets or sets the schedule start date.</summary>
        public DateTime ScheduleStartDate { get; set; }

        /// <summary>Gets or sets the friendly name for the item.</summary>
        public string FriendlyName { get; set; }

        /// <summary>Gets or sets the next start date.</summary>
        public virtual DateTime NextStart
        {
            get
            {
                if (!this.nextStart.HasValue)
                {
                    this.nextStart = MinNextTime;
                }

                return this.nextStart.Value > MinNextTime ? this.nextStart.Value : MinNextTime;
            }

            set
            {
                this.nextStart = value;
            }
        }

        /// <summary>Gets or sets the object dependencies.</summary>
        public string ObjectDependencies { get; set; }

        /// <summary>Gets or sets a value indicating how many history items to keep.</summary>
        public int RetainHistoryNum { get; set; }

        /// <summary>Gets or sets the retry time-lapse value.</summary>
        public int RetryTimeLapse { get; set; }

        /// <summary>Gets or sets the unit of measure for the retry time-lapse value.</summary>
        /// <value>
        /// Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list></value>
        public string RetryTimeLapseMeasurement { get; set; }

        /// <summary>Gets or sets the ID of the scheduled item.</summary>
        public int ScheduleID { get; set; }

        /// <summary>Gets or sets the servers this task should run on.</summary>
        public string Servers { get; set; }

        /// <summary>Gets or sets the recurrence time-lapse value.</summary>
        public int TimeLapse { get; set; }

        /// <summary>Gets or sets the unit of measure for the recurrence time-lapse value.</summary>
        /// <value>
        /// Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list></value>
        public string TimeLapseMeasurement { get; set; }

        /// <summary>Gets or sets the full type name.</summary>
        public string TypeFullName { get; set; }

        /// <summary>Gets or sets the process group.</summary>
        public int ProcessGroup { get; set; }

        /// <summary>Gets or sets the <see cref="ScheduleSource"/>.</summary>
        public ScheduleSource ScheduleSource { get; set; }

        /// <summary>Gets or sets the ID of the running thread.</summary>
        public int ThreadID { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get => this.ScheduleID;
            set => this.ScheduleID = value;
        }

        /// <inheritdoc />
        public virtual void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <summary>Gets or sets a value indicating whether the item has object dependencies.</summary>
        /// <param name="strObjectDependencies">A string representing the name of the object dependencies.</param>
        /// <returns>A value indicating whether the item has object dependencies.</returns>
        public bool HasObjectDependencies(string strObjectDependencies)
        {
			Logger.LogDebug(">>> Entering HasObjectDependencies(strObjectDependencies={StrObjectDependencies})", strObjectDependencies);
            if (strObjectDependencies.IndexOf(",", StringComparison.Ordinal) > -1)
            {
                var dependencies = strObjectDependencies.Split(',');
                for (var i = 0; i <= dependencies.Length - 1; i++)
				Logger.LogDebug("HasObjectDependencies(strObjectDependencies={StrObjectDependencies}): dependencies → {Dependencies}", strObjectDependencies, dependencies);
                {
                    if (this.ObjectDependencies.Contains(dependencies[i].Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            else if (this.ObjectDependencies.Contains(strObjectDependencies, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

			Logger.LogDebug("<<< Exiting HasObjectDependencies(strObjectDependencies={StrObjectDependencies})", strObjectDependencies);
            return false;
        }

        /// <summary>Adds a schedule item setting value.</summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The value of the setting.</param>
        public void AddSetting(string key, string value)
        {
			Logger.LogDebug(">>> Entering AddSetting(key={Key},value={Value})", key, value);
            this.scheduleItemSettings.Add(key, value);
			Logger.LogDebug("<<< Exiting AddSetting(key={Key},value={Value})", key, value);
        }

        /// <summary>Gets a specific setting.</summary>
        /// <param name="key">The key of the setting to get.</param>
        /// <returns>The value of the setting.</returns>
        public virtual string GetSetting(string key)
        {
			Logger.LogDebug(">>> Entering GetSetting(key={Key})", key);
            if (this.scheduleItemSettings == null)
            {
                this.GetSettings();
            }

            if (this.scheduleItemSettings != null && this.scheduleItemSettings.ContainsKey(key))
            {
                return Convert.ToString(this.scheduleItemSettings[key], CultureInfo.InvariantCulture);
            }
            else
            {
				Logger.LogDebug("<<< Exiting GetSetting(key={Key})", key);
                return string.Empty;
            }
        }

        /// <summary>Gets all the item settings.</summary>
        /// <returns>An <see cref="Hashtable"/> of all the settings.</returns>
        public virtual Hashtable GetSettings()
        {
			Logger.LogDebug(">>> Entering GetSettings()");
            this.scheduleItemSettings = SchedulingProvider.Instance().GetScheduleItemSettings(this.ScheduleID);
			Logger.LogDebug("GetSettings(): scheduleItemSettings → {ScheduleItemSettings}", scheduleItemSettings);
			Logger.LogDebug("<<< Exiting GetSettings()");
            return this.scheduleItemSettings;
        }

        /// <inheritdoc />
        protected override void FillInternal(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering FillInternal(dr={Dr})", dr);
            this.ScheduleID = Null.SetNullInteger(dr["ScheduleID"]);
            this.FriendlyName = Null.SetNullString(dr["FriendlyName"]);
            this.TypeFullName = Null.SetNullString(dr["TypeFullName"]);
            this.TimeLapse = Null.SetNullInteger(dr["TimeLapse"]);
            this.TimeLapseMeasurement = Null.SetNullString(dr["TimeLapseMeasurement"]);
            this.RetryTimeLapse = Null.SetNullInteger(dr["RetryTimeLapse"]);
            this.RetryTimeLapseMeasurement = Null.SetNullString(dr["RetryTimeLapseMeasurement"]);
            this.ObjectDependencies = Null.SetNullString(dr["ObjectDependencies"]);
            this.AttachToEvent = Null.SetNullString(dr["AttachToEvent"]);
            this.RetainHistoryNum = Null.SetNullInteger(dr["RetainHistoryNum"]);
            this.CatchUpEnabled = Null.SetNullBoolean(dr["CatchUpEnabled"]);
            this.Enabled = Null.SetNullBoolean(dr["Enabled"]);
            this.Servers = Null.SetNullString(dr["Servers"]);

            var schema = dr.GetSchemaTable();
            if (schema != null)
			Logger.LogDebug("FillInternal(dr={Dr}): schema → {Schema}", dr, schema);
            {
                if (schema.Select("ColumnName = 'NextStart'").Length > 0)
                {
                    this.NextStart = Null.SetNullDateTime(dr["NextStart"]);
                }

                if (schema.Select("ColumnName = 'ScheduleStartDate'").Length > 0)
                {
                    this.ScheduleStartDate = Null.SetNullDateTime(dr["ScheduleStartDate"]);
                }
            }

            // Fill BaseEntityInfo
            base.FillInternal(dr);
			Logger.LogDebug("<<< Exiting FillInternal(dr={Dr})", dr);
        }
    }
}
