// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc cref="IReplaceTabModuleStep" />
    internal sealed class ReplaceTabModuleStep : StepBase, IReplaceTabModuleStep, IStepArray
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IServiceProvider serviceProvider;
        private readonly List<IReplacePortalTabModuleStep> steps;

        /// <summary>Initializes a new instance of the <see cref="ReplaceTabModuleStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public ReplaceTabModuleStep(ILoggerSource loggerSource, ILocalizer localizer, IServiceProvider serviceProvider)
            : base(loggerSource, localizer)
        {
			_logger.LogDebug(">>> Entering ReplaceTabModuleStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
            this.serviceProvider = serviceProvider ??
				_logger.LogDebug("<<< Exiting ReplaceTabModuleStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
				_logger.LogDebug("ReplaceTabModuleStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", loggerSource, localizer, serviceProvider, serviceProvider);
				_logger.LogDebug("ReplaceTabModuleStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider}): this.serviceProvider → {Arg3}", loggerSource, localizer, serviceProvider, this.serviceProvider);
				_logger.LogWarning("Throwing ArgumentNullException from ReplaceTabModuleStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
                throw new ArgumentNullException(nameof(serviceProvider));

            this.steps = [];
        }

        /// <inheritdoc />
        public override string Name => this.LocalizeFormat(
            "UninstallStepReplacePageModule",
            this.OldModuleName,
            this.NewModuleName);

        /// <inheritdoc />
        [Required]
        public string OldModuleName { get; set; }

        /// <inheritdoc />
        [Required]
        public string NewModuleName { get; set; }

        /// <inheritdoc />
        public IEnumerable<IStep> Steps => this.steps;

        /// <inheritdoc />
        public Func<Hashtable, Hashtable> MigrateSettings { get; set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			_logger.LogDebug(">>> Entering ExecuteInternal()");
            var portalController = this.GetService<IPortalController>();

			_logger.LogDebug("ExecuteInternal(): portalController → {PortalController}", portalController);
            var steps = portalController.GetPortals()
                .Cast<IPortalInfo>()
                .Select(info =>
                {
                    var step = this.GetService<IReplacePortalTabModuleStep>();
					_logger.LogDebug("ExecuteInternal(): step → {Step}", step);
					_logger.LogDebug("ExecuteInternal(): step.ParentStep → {Arg0}", step.ParentStep);
					_logger.LogDebug("ExecuteInternal(): step.PortalId → {Arg0}", step.PortalId);
                    step.ParentStep = this;
                    step.PortalId = info.PortalId;
                    return step;
                });

            foreach (var step in steps)
            {
                this.steps.Add(step);
                step.Execute();
            }
			_logger.LogDebug("<<< Exiting ExecuteInternal()");
        }

        private T GetService<T>()
            where T : class
        {
			_logger.LogDebug(">>> Entering GetService()");
			_logger.LogDebug("<<< Exiting GetService()");
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
