// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Maintenance.Telerik.Steps;

    /// <summary>A data transfer object with an uninstallation summary item.</summary>
    public class UninstallSummaryItem
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="UninstallSummaryItem"/> class.</summary>
        /// <param name="step">The step to summarize.</param>
        internal UninstallSummaryItem(IStep step)
        {
			Logger.LogDebug(">>> Entering UninstallSummaryItem(step={Step})", step);
            this.Notes = step.Notes;
            this.StepName = step.Name;
			Logger.LogDebug("UninstallSummaryItem(step={Step}): this.Notes → {Arg1}", step, this.Notes);
			Logger.LogDebug("UninstallSummaryItem(step={Step}): this.StepName → {Arg1}", step, this.StepName);
            this.Success = step.Success;
			Logger.LogDebug("<<< Exiting UninstallSummaryItem(step={Step})", step);
			Logger.LogDebug("UninstallSummaryItem(step={Step}): this.Success → {Arg1}", step, this.Success);
        }

        /// <summary>Gets any remarks collected during step execution.</summary>
        public string Notes { get; private set; }

        /// <summary>Gets a value indicating whether the step succeeded, failed or was not executed.</summary>
        public bool? Success { get; private set; }

        /// <summary>Gets the step name.</summary>
        public string StepName { get; private set; }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of type <see cref="UninstallSummaryItem"/>
        /// containing the summary of the given <see cref="IStep"/> instance execution.
        /// </summary>
        /// <param name="step">The step to summarize.</param>
        /// <returns>
        /// A <see cref="UninstallSummaryItem"/> with data from the given <see cref="IStep"/> instance.
        /// </returns>
        internal static IEnumerable<UninstallSummaryItem> FromStep(IStep step)
        {
			Logger.LogDebug(">>> Entering FromStep(step={Step})", step);
            if (step.Quiet && step.Success.HasValue && step.Success.Value)
            {
                return [];
            }

			Logger.LogDebug("<<< Exiting FromStep(step={Step})", step);
            return step is IStepArray stepArray && stepArray.Steps.Any()
                ? stepArray.Steps.SelectMany(s => FromStep(s)).ToArray()
                : [new UninstallSummaryItem(step)];
        }
    }
}
