// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System.Collections.Generic;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <summary>A base class that implements <see cref="IStepArray"/>.</summary>
    internal abstract class StepArrayBase : StepBase, IStepArray
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="StepArrayBase"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        public StepArrayBase(ILoggerSource loggerSource, ILocalizer localizer)
            : base(loggerSource, localizer)
        {
		Logger.LogDebug(">>> Entering StepArrayBase(loggerSource={LoggerSource},localizer={Localizer})", loggerSource, localizer);
		Logger.LogDebug("<<< Exiting StepArrayBase(loggerSource={LoggerSource},localizer={Localizer})", loggerSource, localizer);
        }

        /// <inheritdoc />
        public IEnumerable<IStep> Steps { get; protected set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			Logger.LogDebug(">>> Entering ExecuteInternal()");
            this.Success = true;

			Logger.LogDebug("ExecuteInternal(): this.Success → {Arg0}", this.Success);
            foreach (var step in this.Steps)
            {
                step.Execute();

                if (step.Success.HasValue && step.Success.Value == false)
                {
                    this.Success = false;
                    break;
                }
            }
			Logger.LogDebug("<<< Exiting ExecuteInternal()");
        }
    }
}
