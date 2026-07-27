// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Taxonomy
{
    public class TermUsage
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Initializes a new instance of the <see cref="TermUsage"/> class.
        /// parameterless constructor, so that it can be used in CBO.
        /// </summary>
        public TermUsage()
        {
		Logger.LogDebug(">>> Entering TermUsage()");
        }

        /// <summary>Initializes a new instance of the <see cref="TermUsage"/> class.</summary>
        /// <param name="total">The total usage count.</param>
        /// <param name="month">The usage count for the month.</param>
        /// <param name="week">The usage count for the week.</param>
        /// <param name="day">The usage count for the day.</param>
        internal TermUsage(int total, int month, int week, int day)
        {
			Logger.LogDebug(">>> Entering TermUsage(total={Total},month={Month},week={Week},day={Day})", total, month, week, day);
            this.TotalTermUsage = total;

			Logger.LogDebug("TermUsage(total={Total},month={Month},week={Week},day={Day}): this.TotalTermUsage → {Arg4}", total, month, week, day, this.TotalTermUsage);
            this.MonthTermUsage = month;
			Logger.LogDebug("TermUsage(total={Total},month={Month},week={Week},day={Day}): this.MonthTermUsage → {Arg4}", total, month, week, day, this.MonthTermUsage);

            this.WeekTermUsage = week;
			Logger.LogDebug("TermUsage(total={Total},month={Month},week={Week},day={Day}): this.WeekTermUsage → {Arg4}", total, month, week, day, this.WeekTermUsage);

            this.DayTermUsage = day;
			Logger.LogDebug("<<< Exiting TermUsage(total={Total},month={Month},week={Week},day={Day})", total, month, week, day);
			Logger.LogDebug("TermUsage(total={Total},month={Month},week={Week},day={Day}): this.DayTermUsage → {Arg4}", total, month, week, day, this.DayTermUsage);
			Logger.LogDebug("<<< Exiting TermUsage()");
        }

        public int TotalTermUsage { get; set; }

        public int MonthTermUsage { get; set; }

        public int WeekTermUsage { get; set; }

        public int DayTermUsage { get; set; }
    }
}
