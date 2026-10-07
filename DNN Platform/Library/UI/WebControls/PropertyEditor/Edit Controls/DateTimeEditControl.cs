// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// The DateTimeEditControl control provides a standard UI component for editing
    /// date and time properties.
    /// </summary>
    [ToolboxData("<{0}:DateTimeEditControl runat=server></{0}:DateTimeEditControl>")]
    public class DateTimeEditControl : DateEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DateTimeEditControl));
        private DropDownList ampmField;
        private DropDownList hourField;
        private bool is24HourClock;
        private DropDownList minutesField;

        /// <summary>
        /// Gets defaultFormat is a string that will be used to format the date in the absence of a
        /// FormatAttribute.
        /// </summary>
        /// <value>A String representing the default format to use to render the date.</value>
        /// <returns>A Format String.</returns>
        protected override string DefaultFormat
        {
            get
            {
                return "g";
            }
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogError(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            bool dataChanged = false;
            DateTime presentValue = this.OldDateValue;
			Logger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);
            string postedDate = postCollection[postDataKey + "date"];
            string postedHours = postCollection[postDataKey + "hours"];
            string postedMinutes = postCollection[postDataKey + "minutes"];
            string postedAMPM = postCollection[postDataKey + "ampm"];
            DateTime postedValue = Null.NullDate;
			Logger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): postedValue → {PostedValue}", postDataKey, postCollection, postedValue);
            if (!string.IsNullOrEmpty(postedDate))
            {
                if (!DateTime.TryParse(postedDate, out postedValue))
                {
                    postedValue = Null.NullDate;
                }
            }

            if (postedHours != "12" || this.is24HourClock)
            {
                if (int.TryParse(postedHours, out var hours))
                {
                    postedValue = postedValue.AddHours(hours);
                }
            }

            postedValue = postedValue.AddMinutes(int.Parse(postedMinutes, CultureInfo.InvariantCulture));
            if (!this.is24HourClock && postedAMPM.Equals("PM", StringComparison.Ordinal))
            {
                postedValue = postedValue.AddHours(12);
            }

            if (!presentValue.Equals(postedValue))
            {
                this.Value = postedValue.ToString(CultureInfo.InvariantCulture);
                dataChanged = true;
            }

			Logger.LogWarning("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return dataChanged;
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            if (string.IsNullOrEmpty(CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator))
            {
				Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
                this.is24HourClock = true;
				Logger.LogDebug("OnInit(e={E}): this.is24HourClock → {Arg1}", e, this.is24HourClock);
            }
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogError(">>> Entering CreateChildControls()");
            base.CreateChildControls();
            this.Controls.Add(new LiteralControl("<br/>"));
            this.hourField = new DropDownList();
			Logger.LogDebug("CreateChildControls(): hourField → {HourField}", hourField);
            int maxHour = 12;
			Logger.LogError("CreateChildControls(): maxHour → {MaxHour}", maxHour);
            int minHour = 1;
			Logger.LogError("CreateChildControls(): minHour → {MinHour}", minHour);
            if (this.is24HourClock)
            {
                minHour = 0;
                maxHour = 23;
            }

            for (int i = minHour; i <= maxHour; i++)
            {
                this.hourField.Items.Add(new ListItem(i.ToString("00", CultureInfo.CurrentCulture), i.ToString(CultureInfo.InvariantCulture)));
            }

            this.hourField.ControlStyle.CopyFrom(this.ControlStyle);
            this.hourField.ID = this.ID + "hours";
            this.Controls.Add(this.hourField);
            this.Controls.Add(new LiteralControl("&nbsp"));
            this.minutesField = new DropDownList();
			Logger.LogDebug("CreateChildControls(): minutesField → {MinutesField}", minutesField);
            for (int i = 0; i <= 59; i++)
            {
                this.minutesField.Items.Add(new ListItem(i.ToString("00", CultureInfo.CurrentCulture), i.ToString(CultureInfo.InvariantCulture)));
            }

            this.minutesField.ControlStyle.CopyFrom(this.ControlStyle);
            this.minutesField.ID = this.ID + "minutes";
            this.Controls.Add(this.minutesField);
            if (!this.is24HourClock)
            {
                this.Controls.Add(new LiteralControl("&nbsp"));
                this.ampmField = new DropDownList();
				Logger.LogDebug("CreateChildControls(): ampmField → {AmpmField}", ampmField);
                this.ampmField.Items.Add(new ListItem("AM", "AM"));
                this.ampmField.Items.Add(new ListItem("PM", "PM"));
                this.ampmField.ControlStyle.CopyFrom(this.ControlStyle);
                this.ampmField.ID = this.ID + "ampm";
                this.Controls.Add(this.ampmField);
            }
			Logger.LogWarning("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void LoadDateControls()
        {
			Logger.LogError(">>> Entering LoadDateControls()");
            base.LoadDateControls();
            int hour = this.DateValue.Hour;
			Logger.LogError("LoadDateControls(): hour → {Hour}", hour);
            int minute = this.DateValue.Minute;
            bool isAM = true;
			Logger.LogError("LoadDateControls(): isAM → {IsAM}", isAM);
            if (!this.is24HourClock)
            {
                if (hour >= 12)
                {
                    hour -= 12;
                    isAM = false;
                }

                if (hour == 0)
                {
                    hour = 12;
                }
            }

            this.hourField.Items.FindByValue(hour.ToString(CultureInfo.InvariantCulture))?.Selected = true;
            this.minutesField.Items.FindByValue(minute.ToString(CultureInfo.InvariantCulture))?.Selected = true;

            if (!this.is24HourClock)
            {
                if (isAM)
                {
                    this.ampmField.SelectedIndex = 0;
                }
                else
                {
                    this.ampmField.SelectedIndex = 1;
                }
            }
			Logger.LogWarning("<<< Exiting LoadDateControls()");
        }
    }
}
