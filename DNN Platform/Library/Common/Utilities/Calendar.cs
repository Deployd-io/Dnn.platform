// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    public class Calendar
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Opens a popup Calendar.</summary>
        /// <param name="field">TextBox to return the date value.</param>
        /// <returns>A JavaScript URL.</returns>
        public static string InvokePopupCal(TextBox field)
        {
			Logger.LogDebug(">>> Entering InvokePopupCal(field={Field})", field);
            // Define character array to trim from language strings
            char[] trimChars = { ',', ' ' };

            // Get culture array of month names and convert to string for
            // passing to the popup calendar
            var monthBuilder = new StringBuilder();
            foreach (string month in DateTimeFormatInfo.CurrentInfo.MonthNames)
            {
			Logger.LogDebug("InvokePopupCal(field={Field}): monthBuilder → {MonthBuilder}", field, monthBuilder);
                monthBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0},", month);
            }

            var monthNameString = monthBuilder.ToString().TrimEnd(trimChars);

			Logger.LogDebug("InvokePopupCal(field={Field}): monthNameString → {MonthNameString}", field, monthNameString);
            // Get culture array of day names and convert to string for
            // passing to the popup calendar
            var dayBuilder = new StringBuilder();
            foreach (string day in DateTimeFormatInfo.CurrentInfo.AbbreviatedDayNames)
            {
			Logger.LogDebug("InvokePopupCal(field={Field}): dayBuilder → {DayBuilder}", field, dayBuilder);
                dayBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0},", day);
            }

            var dayNameString = dayBuilder.ToString().TrimEnd(trimChars);

			Logger.LogDebug("InvokePopupCal(field={Field}): dayNameString → {DayNameString}", field, dayNameString);
            // Get the short date pattern for the culture
            string formatString = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            if (!field.Page.ClientScript.IsClientScriptIncludeRegistered("PopupCalendar.js"))
            {
                ScriptManager.RegisterClientScriptInclude(field.Page, field.Page.GetType(), "PopupCalendar.js", ClientAPI.ScriptPath + "PopupCalendar.js");
            }

            string strToday = ClientAPI.GetSafeJSString(Localization.GetString("Today"));
            string strClose = ClientAPI.GetSafeJSString(Localization.GetString("Close"));
            string strCalendar = ClientAPI.GetSafeJSString(Localization.GetString("Calendar"));
				Logger.LogDebug("<<< Exiting InvokePopupCal(field={Field})", field);
            return
                string.Format(
                    CultureInfo.InvariantCulture,
                    "javascript:popupCal('Cal','{0}','{1}','{2}','{3}','{4}','{5}','{6}',{7});",
                    HttpUtility.JavaScriptStringEncode(field.ClientID),
                    HttpUtility.JavaScriptStringEncode(formatString),
                    HttpUtility.JavaScriptStringEncode(monthNameString),
                    HttpUtility.JavaScriptStringEncode(dayNameString),
                    HttpUtility.JavaScriptStringEncode(strToday),
                    HttpUtility.JavaScriptStringEncode(strClose),
                    HttpUtility.JavaScriptStringEncode(strCalendar),
                    (int)DateTimeFormatInfo.CurrentInfo.FirstDayOfWeek);
        }
    }
}
