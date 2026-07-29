// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <inheritdoc cref="IClearCacheStep" />
    internal sealed class ClearCacheStep : StepBase, IClearCacheStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IDataCache dataCache;

        /// <summary>Initializes a new instance of the <see cref="ClearCacheStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="dataCache">An instance of <see cref="IDataCache"/>.</param>
        public ClearCacheStep(ILoggerSource loggerSource, ILocalizer localizer, IDataCache dataCache)
            : base(loggerSource, localizer)
        {
			Logger.LogDebug(">>> Entering ClearCacheStep(loggerSource={LoggerSource},localizer={Localizer},dataCache={DataCache})", loggerSource, localizer, dataCache);
            this.dataCache = dataCache ??
				Logger.LogDebug("<<< Exiting ClearCacheStep(loggerSource={LoggerSource},localizer={Localizer},dataCache={DataCache})", loggerSource, localizer, dataCache);
				Logger.LogDebug("ClearCacheStep(loggerSource={LoggerSource},localizer={Localizer},dataCache={DataCache}): dataCache → {DataCache1}", loggerSource, localizer, dataCache, dataCache);
				Logger.LogDebug("ClearCacheStep(loggerSource={LoggerSource},localizer={Localizer},dataCache={DataCache}): this.dataCache → {Arg3}", loggerSource, localizer, dataCache, this.dataCache);
                throw new ArgumentNullException(nameof(dataCache));

            this.Quiet = true;
			Logger.LogDebug("ClearCacheStep(loggerSource={LoggerSource},localizer={Localizer},dataCache={DataCache}): this.Quiet → {Arg3}", loggerSource, localizer, dataCache, this.Quiet);
        }

        /// <inheritdoc />
        public override string Name => this.Localize("UninstallStepClearCache");

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			Logger.LogDebug(">>> Entering ExecuteInternal()");
            this.dataCache.ClearCache();
            this.Success = true;
			Logger.LogDebug("<<< Exiting ExecuteInternal()");
			Logger.LogDebug("ExecuteInternal(): this.Success → {Arg0}", this.Success);
        }
    }
}
