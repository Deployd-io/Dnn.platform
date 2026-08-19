// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization.Persian
{
    using System;

    public class PersianCalendar : System.Globalization.PersianCalendar
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PersianCalendar));
        /// <inheritdoc />
        public override int GetYear(DateTime time)
        {
			Logger.LogDebug(">>> Entering GetYear(time={Time})", time);
            try
            {
                return base.GetYear(time);
            }
            catch
            {
				Logger.LogError("Exception in GetYear(time={Time}): {Arg1}", time, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting GetYear(time={Time})", time);
            return time.Year;
        }

        /// <inheritdoc />
        public override int GetMonth(DateTime time)
        {
			Logger.LogDebug(">>> Entering GetMonth(time={Time})", time);
            try
            {
                return base.GetMonth(time);
            }
            catch
            {
				Logger.LogError("Exception in GetMonth(time={Time}): {Arg1}", time, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting GetMonth(time={Time})", time);
            return time.Month;
        }

        /// <inheritdoc />
        public override int GetDayOfMonth(DateTime time)
        {
			Logger.LogDebug(">>> Entering GetDayOfMonth(time={Time})", time);
            try
            {
                return base.GetDayOfMonth(time);
            }
            catch
            {
				Logger.LogError("Exception in GetDayOfMonth(time={Time}): {Arg1}", time, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting GetDayOfMonth(time={Time})", time);
            return time.Day;
        }

        /// <inheritdoc />
        public override int GetDayOfYear(DateTime time)
        {
			Logger.LogDebug(">>> Entering GetDayOfYear(time={Time})", time);
            try
            {
                return base.GetDayOfYear(time);
            }
            catch
            {
				Logger.LogError("Exception in GetDayOfYear(time={Time}): {Arg1}", time, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting GetDayOfYear(time={Time})", time);
            return time.DayOfYear;
        }

        /// <inheritdoc />
        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
			Logger.LogDebug(">>> Entering GetDayOfWeek(time={Time})", time);
            try
            {
                return base.GetDayOfWeek(time);
            }
            catch
            {
				Logger.LogError("Exception in GetDayOfWeek(time={Time}): {Arg1}", time, ex.Message);
                // ignore
            }

			Logger.LogDebug("<<< Exiting GetDayOfWeek(time={Time})", time);
            return time.DayOfWeek;
        }
    }
}
