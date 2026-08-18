// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Data;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc cref="IExecuteSqlStep" />
    internal sealed class ExecuteSqlStep : StepBase, IExecuteSqlStep
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IDataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="ExecuteSqlStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="dataProvider">An instance of <see cref="IDataProvider"/>.</param>
        public ExecuteSqlStep(ILoggerSource loggerSource, ILocalizer localizer, IDataProvider dataProvider)
            : base(loggerSource, localizer)
        {
			_logger.LogDebug(">>> Entering ExecuteSqlStep(loggerSource={LoggerSource},localizer={Localizer},dataProvider={DataProvider})", loggerSource, localizer, dataProvider);
            this.dataProvider = dataProvider ??
				_logger.LogDebug("<<< Exiting ExecuteSqlStep(loggerSource={LoggerSource},localizer={Localizer},dataProvider={DataProvider})", loggerSource, localizer, dataProvider);
				_logger.LogDebug("ExecuteSqlStep(loggerSource={LoggerSource},localizer={Localizer},dataProvider={DataProvider}): dataProvider → {DataProvider1}", loggerSource, localizer, dataProvider, dataProvider);
				_logger.LogDebug("ExecuteSqlStep(loggerSource={LoggerSource},localizer={Localizer},dataProvider={DataProvider}): this.dataProvider → {Arg3}", loggerSource, localizer, dataProvider, this.dataProvider);
				_logger.LogWarning("Throwing ArgumentNullException from ExecuteSqlStep(loggerSource={LoggerSource},localizer={Localizer},dataProvider={DataProvider})", loggerSource, localizer, dataProvider);
		_logger.LogWarning("Throwing ArgumentNullException from ExecuteSqlStep(loggerSource={LoggerSource},localizer={Localizer},dataProvider={DataProvider})", loggerSource, localizer, dataProvider);
                throw new ArgumentNullException(nameof(dataProvider));
        }

        /// <inheritdoc />
        [Required]
        public string CommandText { get; set; }

        /// <inheritdoc />
        public IDataReader Result { get; private set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			_logger.LogDebug(">>> Entering ExecuteInternal()");
            this.Result = this.dataProvider.ExecuteSQL(this.CommandText);
            this.Notes = this.LocalizeFormat("UninstallStepCountOfRecordsAffected", this.Result.RecordsAffected);
            this.Success = true;
			_logger.LogDebug("ExecuteInternal(): this.Success → {Arg0}", this.Success);
			_logger.LogDebug("<<< Exiting ExecuteInternal()");
        }
    }
}
