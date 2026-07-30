// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization.Persian
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class PersianController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static CultureInfo GetPersianCultureInfo()
        {
			Logger.LogDebug(">>> Entering GetPersianCultureInfo()");
            var persianCultureInfo = new CultureInfo("fa-IR");

			Logger.LogDebug("GetPersianCultureInfo(): persianCultureInfo → {PersianCultureInfo}", persianCultureInfo);
            SetPersianDateTimeFormatInfo(persianCultureInfo.DateTimeFormat);
            SetNumberFormatInfo(persianCultureInfo.NumberFormat);

            var cal = new PersianCalendar();

			Logger.LogDebug("GetPersianCultureInfo(): cal → {Cal}", cal);
            FieldInfo fieldInfo = persianCultureInfo.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
			Logger.LogDebug("GetPersianCultureInfo(): fieldInfo → {FieldInfo}", fieldInfo);
            {
                fieldInfo.SetValue(persianCultureInfo, cal);
            }

            FieldInfo info = persianCultureInfo.DateTimeFormat.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
			Logger.LogDebug("GetPersianCultureInfo(): info → {Info}", info);
            {
                info.SetValue(persianCultureInfo.DateTimeFormat, cal);
            }

			Logger.LogDebug("<<< Exiting GetPersianCultureInfo()");
            return persianCultureInfo;
        }

        public static void SetPersianDateTimeFormatInfo(DateTimeFormatInfo persianDateTimeFormatInfo)
        {
			Logger.LogDebug(">>> Entering SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo})", persianDateTimeFormatInfo);
            persianDateTimeFormatInfo.MonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", string.Empty };
            persianDateTimeFormatInfo.MonthGenitiveNames = persianDateTimeFormatInfo.MonthNames;
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.MonthGenitiveNames → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.MonthGenitiveNames);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.AbbreviatedMonthNames → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.AbbreviatedMonthNames);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.AbbreviatedMonthGenitiveNames → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.AbbreviatedMonthGenitiveNames);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.MonthNames → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.MonthNames);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.DayNames → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.DayNames);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.AbbreviatedDayNames → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.AbbreviatedDayNames);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.ShortestDayNames → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.ShortestDayNames);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.FirstDayOfWeek → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.FirstDayOfWeek);
            persianDateTimeFormatInfo.AbbreviatedMonthNames = persianDateTimeFormatInfo.MonthNames;
            persianDateTimeFormatInfo.AbbreviatedMonthGenitiveNames = persianDateTimeFormatInfo.MonthNames;

            persianDateTimeFormatInfo.DayNames = new[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
            persianDateTimeFormatInfo.AbbreviatedDayNames = new[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            persianDateTimeFormatInfo.ShortestDayNames = persianDateTimeFormatInfo.AbbreviatedDayNames;
            persianDateTimeFormatInfo.FirstDayOfWeek = DayOfWeek.Saturday;

            persianDateTimeFormatInfo.AMDesignator = "ق.ظ";
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.AMDesignator → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.AMDesignator);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.PMDesignator → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.PMDesignator);
            persianDateTimeFormatInfo.PMDesignator = "ب.ظ";

            persianDateTimeFormatInfo.DateSeparator = "/";
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.DateSeparator → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.DateSeparator);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.TimeSeparator → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.TimeSeparator);
            persianDateTimeFormatInfo.TimeSeparator = ":";

            persianDateTimeFormatInfo.FullDateTimePattern = "tt hh:mm:ss yyyy mmmm dd dddd";
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.FullDateTimePattern → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.FullDateTimePattern);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.YearMonthPattern → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.YearMonthPattern);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.MonthDayPattern → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.MonthDayPattern);
            persianDateTimeFormatInfo.YearMonthPattern = "yyyy, MMMM";
            persianDateTimeFormatInfo.MonthDayPattern = "dd MMMM";

            persianDateTimeFormatInfo.LongDatePattern = "dddd, dd MMMM,yyyy";
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.LongDatePattern → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.LongDatePattern);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.ShortDatePattern → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.ShortDatePattern);
            persianDateTimeFormatInfo.ShortDatePattern = "yyyy/MM/dd";

            persianDateTimeFormatInfo.LongTimePattern = "hh:mm:ss tt";
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.LongTimePattern → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.LongTimePattern);
			Logger.LogDebug("SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo}): persianDateTimeFormatInfo.ShortTimePattern → {Arg1}", persianDateTimeFormatInfo, persianDateTimeFormatInfo.ShortTimePattern);
            persianDateTimeFormatInfo.ShortTimePattern = "hh:mm tt";
			Logger.LogDebug("<<< Exiting SetPersianDateTimeFormatInfo(persianDateTimeFormatInfo={PersianDateTimeFormatInfo})", persianDateTimeFormatInfo);
        }

        public static void SetNumberFormatInfo(NumberFormatInfo persianNumberFormatInfo)
        {
			Logger.LogDebug(">>> Entering SetNumberFormatInfo(persianNumberFormatInfo={PersianNumberFormatInfo})", persianNumberFormatInfo);
            persianNumberFormatInfo.NumberDecimalSeparator = "/";
            persianNumberFormatInfo.DigitSubstitution = DigitShapes.NativeNational;
			Logger.LogDebug("SetNumberFormatInfo(persianNumberFormatInfo={PersianNumberFormatInfo}): persianNumberFormatInfo.DigitSubstitution → {Arg1}", persianNumberFormatInfo, persianNumberFormatInfo.DigitSubstitution);
			Logger.LogDebug("SetNumberFormatInfo(persianNumberFormatInfo={PersianNumberFormatInfo}): persianNumberFormatInfo.NumberNegativePattern → {Arg1}", persianNumberFormatInfo, persianNumberFormatInfo.NumberNegativePattern);
			Logger.LogDebug("SetNumberFormatInfo(persianNumberFormatInfo={PersianNumberFormatInfo}): persianNumberFormatInfo.NumberDecimalSeparator → {Arg1}", persianNumberFormatInfo, persianNumberFormatInfo.NumberDecimalSeparator);
            persianNumberFormatInfo.NumberNegativePattern = 0;
			Logger.LogDebug("<<< Exiting SetNumberFormatInfo(persianNumberFormatInfo={PersianNumberFormatInfo})", persianNumberFormatInfo);
        }
    }
}
