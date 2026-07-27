// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;

    /// <summary>A container for an array of steps.</summary>
    internal sealed class RemoveExtensionStep : StepArrayBase, IRemoveExtensionStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IServiceProvider serviceProvider;

        /// <summary>Initializes a new instance of the <see cref="RemoveExtensionStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        public RemoveExtensionStep(ILoggerSource loggerSource, ILocalizer localizer, IServiceProvider serviceProvider)
            : base(loggerSource, localizer)
        {
			Logger.LogDebug(">>> Entering RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
            this.serviceProvider = serviceProvider ??
				Logger.LogDebug("<<< Exiting RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider})", loggerSource, localizer, serviceProvider);
				Logger.LogDebug("RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", loggerSource, localizer, serviceProvider, serviceProvider);
				Logger.LogDebug("RemoveExtensionStep(loggerSource={LoggerSource},localizer={Localizer},serviceProvider={ServiceProvider}): this.serviceProvider → {Arg3}", loggerSource, localizer, serviceProvider, this.serviceProvider);
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
			Logger.LogDebug(">>> Entering ExecuteInternal()");
            this.Steps = new List<IStep>(new[]
            {
                this.RemoveSystemAttributeFromPackage(),
                this.ClearCache(),
                this.UninstallExtension(),
                this.DeleteDependencyRecords(),
                this.ClearCache(),
            });

            base.ExecuteInternal();
			Logger.LogDebug("<<< Exiting ExecuteInternal()");
        }

        private IStep RemoveSystemAttributeFromPackage()
        {
			Logger.LogDebug(">>> Entering RemoveSystemAttributeFromPackage()");
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.LocalizeFormat("UninstallStepRemoveSystemAttribute", this.PackageName);
			Logger.LogDebug("RemoveSystemAttributeFromPackage(): step → {Step}", step);
			Logger.LogDebug("RemoveSystemAttributeFromPackage(): step.CommandText → {Arg0}", step.CommandText);
            step.CommandText = $$"""
                                 UPDATE {databaseOwner}{objectQualifier}Packages
                                 SET IsSystemPackage = 0
                                 WHERE [Name] = '{{this.PackageName}}'
                                 """;

			Logger.LogDebug("<<< Exiting RemoveSystemAttributeFromPackage()");
            return step;
        }

        private IStep ClearCache()
        {
			Logger.LogDebug(">>> Entering ClearCache()");
			Logger.LogDebug("<<< Exiting ClearCache()");
            return this.GetService<IClearCacheStep>();
        }

        private IStep UninstallExtension()
        {
			Logger.LogDebug(">>> Entering UninstallExtension()");
            var step = this.GetService<IUninstallPackageStep>();
            step.PackageName = this.PackageName;
			Logger.LogDebug("UninstallExtension(): step → {Step}", step);
			Logger.LogDebug("UninstallExtension(): step.PackageName → {Arg0}", step.PackageName);
			Logger.LogDebug("UninstallExtension(): step.DeleteFiles → {Arg0}", step.DeleteFiles);
            step.DeleteFiles = this.DeleteFiles;

			Logger.LogDebug("<<< Exiting UninstallExtension()");
            return step;
        }

        private IStep DeleteDependencyRecords()
        {
			Logger.LogDebug(">>> Entering DeleteDependencyRecords()");
            var step = this.GetService<IExecuteSqlStep>();
            step.Name = this.LocalizeFormat("UninstallStepCleanupDependencyRecords", this.PackageName);
			Logger.LogDebug("DeleteDependencyRecords(): step → {Step}", step);
			Logger.LogDebug("DeleteDependencyRecords(): step.CommandText → {Arg0}", step.CommandText);
            step.CommandText = $$"""
                                 DELETE FROM {databaseOwner}{objectQualifier}PackageDependencies
                                 WHERE PackageName = '{{this.PackageName}}'
                                 """;

			Logger.LogDebug("<<< Exiting DeleteDependencyRecords()");
            return step;
        }

        private T GetService<T>()
            where T : class
        {
			Logger.LogDebug(">>> Entering GetService()");
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
