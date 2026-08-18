// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;
	using Microsoft.Extensions.Logging;

    /// <summary>A container for an array of steps.</summary>
    internal sealed class RemoveExtensionStep : StepArrayBase, IRemoveExtensionStep
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IServiceProvider serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="RemoveExtensionStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public RemoveExtensionStep(ILoggerSource loggerSource, ILocalizer localizer, IServiceProvider serviceProvider)
            : base(loggerSource, localizer)
        {
			_logger.LogDebug(">>> Entering RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
            this.serviceProvider = serviceProvider ??
				_logger.LogDebug("<<< Exiting RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
				_logger.LogDebug("RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", loggerSource, localizer, serviceProvider, serviceProvider);
				_logger.LogDebug("RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider}): this.serviceProvider → {Arg3}", loggerSource, localizer, serviceProvider, this.serviceProvider);
				_logger.LogWarning("Throwing ArgumentNullException from RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
		_logger.LogWarning("Throwing ArgumentNullException from RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
                throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public override string Name => this.LocalizeFormat("UninstallStepRemoveExtension", this.PackageName);

        /// <inheritdoc />
        [Required]
        public string PackageName { get; set; }

        /// <inheritdoc />
        [Required]
        public bool DeleteFiles { get; set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			_logger.LogDebug(">>> Entering ExecuteInternal()");
            this.Steps = new List<IStep>(new[]
            {
                this.RemoveSystemAttributeFromPackage(),
                this.ClearCache(),
                this.UninstallExtension(),
                this.DeleteDependencyRecords(),
                this.ClearCache(),
            });

            base.ExecuteInternal();
			_logger.LogDebug("<<< Exiting ExecuteInternal()");
        }

        private IStep RemoveSystemAttributeFromPackage()
        {
			_logger.LogDebug(">>> Entering RemoveSystemAttributeFromPackage()");
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.LocalizeFormat("UninstallStepRemoveSystemAttribute", this.PackageName);
			_logger.LogDebug("RemoveSystemAttributeFromPackage(): step → {Step}", step);
            step.CommandText = $$"""
                                 UPDATE {databaseOwner}{objectQualifier}Packages
                                 SET IsSystemPackage = 0
                                 WHERE [Name] = '{{this.PackageName}}'
                                 """;
								_logger.LogDebug("RemoveSystemAttributeFromPackage(): step.CommandText → {Arg0}", step.CommandText);

			_logger.LogDebug("<<< Exiting RemoveSystemAttributeFromPackage()");
            return step;
        }

        private IStep ClearCache()
        {
			_logger.LogDebug(">>> Entering ClearCache()");
			_logger.LogDebug("<<< Exiting ClearCache()");
            return this.GetService<IClearCacheStep>();
        }

        private IStep UninstallExtension()
        {
			_logger.LogDebug(">>> Entering UninstallExtension()");
            var step = this.GetService<IUninstallPackageStep>();
            step.PackageName = this.PackageName;
			_logger.LogDebug("UninstallExtension(): step.PackageName → {Arg0}", step.PackageName);
			_logger.LogDebug("UninstallExtension(): step.DeleteFiles → {Arg0}", step.DeleteFiles);
			_logger.LogDebug("UninstallExtension(): step → {Step}", step);
            step.DeleteFiles = this.DeleteFiles;

			_logger.LogDebug("<<< Exiting UninstallExtension()");
            return step;
        }

        private IStep DeleteDependencyRecords()
        {
			_logger.LogDebug(">>> Entering DeleteDependencyRecords()");
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.LocalizeFormat("UninstallStepCleanupDependencyRecords", this.PackageName);
			_logger.LogDebug("DeleteDependencyRecords(): step → {Step}", step);
            step.CommandText = $$"""
                                 DELETE FROM {databaseOwner}{objectQualifier}PackageDependencies
                                 WHERE PackageName = '{{this.PackageName}}'
                                 """;
								_logger.LogDebug("DeleteDependencyRecords(): step.CommandText → {Arg0}", step.CommandText);

			_logger.LogDebug("<<< Exiting DeleteDependencyRecords()");
            return step;
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
