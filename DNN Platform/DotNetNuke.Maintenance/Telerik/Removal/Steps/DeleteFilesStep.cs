// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.IO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;
	using Microsoft.Extensions.Logging;

    /// <summary>Deletes files.</summary>
    internal sealed class DeleteFilesStep : StepBase, IDeleteFilesStep
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IFileSystemProvider fileSystemProvider;
        private readonly IApplicationStatusInfo applicationStatusInfo;

        /// <summary>Initializes a new instance of the <see cref="DeleteFilesStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="fileSystemProvider">An instance of <see cref="IFileSystemProvider"/>.</param>
        /// <param name="applicationStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        public DeleteFilesStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IFileSystemProvider fileSystemProvider,
            IApplicationStatusInfo applicationStatusInfo)
            : base(loggerSource, localizer)
        {
			_logger.LogDebug(">>> Entering DeleteFilesStep(loggerSource={LoggerSource},localizer={Localizer},fileSystemProvider={FileSystemProvider},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, fileSystemProvider, applicationStatusInfo);
			_logger.LogDebug("DeleteFilesStep(loggerSource={LoggerSource},localizer={Localizer},fileSystemProvider={FileSystemProvider},applicationStatusInfo={ApplicationStatusInfo}): applicationStatusInfo → {ApplicationStatusInfo1}", loggerSource, localizer, fileSystemProvider, applicationStatusInfo, applicationStatusInfo);
            this.fileSystemProvider = fileSystemProvider ??
				_logger.LogWarning("Throwing ArgumentNullException from DeleteFilesStep(loggerSource={LoggerSource},localizer={Localizer},fileSystemProvider={FileSystemProvider},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, fileSystemProvider, applicationStatusInfo);
				_logger.LogDebug("DeleteFilesStep(loggerSource={LoggerSource},localizer={Localizer},fileSystemProvider={FileSystemProvider},applicationStatusInfo={ApplicationStatusInfo}): fileSystemProvider → {FileSystemProvider1}", loggerSource, localizer, fileSystemProvider, applicationStatusInfo, fileSystemProvider);
				_logger.LogDebug("DeleteFilesStep(loggerSource={LoggerSource},localizer={Localizer},fileSystemProvider={FileSystemProvider},applicationStatusInfo={ApplicationStatusInfo}): this.fileSystemProvider → {Arg4}", loggerSource, localizer, fileSystemProvider, applicationStatusInfo, this.fileSystemProvider);
                throw new ArgumentNullException(nameof(fileSystemProvider));

            this.applicationStatusInfo = applicationStatusInfo ??
				_logger.LogDebug("<<< Exiting DeleteFilesStep(loggerSource={LoggerSource},localizer={Localizer},fileSystemProvider={FileSystemProvider},applicationStatusInfo={ApplicationStatusInfo})", loggerSource, localizer, fileSystemProvider, applicationStatusInfo);
				_logger.LogDebug("DeleteFilesStep(loggerSource={LoggerSource},localizer={Localizer},fileSystemProvider={FileSystemProvider},applicationStatusInfo={ApplicationStatusInfo}): this.applicationStatusInfo → {Arg4}", loggerSource, localizer, fileSystemProvider, applicationStatusInfo, this.applicationStatusInfo);
                throw new ArgumentNullException(nameof(applicationStatusInfo));
        }

        /// <inheritdoc />
        [Required]
        public string RelativePath { get; set; }

        /// <inheritdoc />
        [Required]
        public string SearchPattern { get; set; }

        /// <inheritdoc />
        [Required]
        public SearchOption SearchOption { get; set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			_logger.LogDebug(">>> Entering ExecuteInternal()");
            var count = 0;
            var appPath = this.applicationStatusInfo.ApplicationMapPath + @"\";
			_logger.LogDebug("ExecuteInternal(): appPath → {AppPath}", appPath);
            var fullPath = Path.GetFullPath(Path.Combine(appPath, this.RelativePath));
			_logger.LogDebug("ExecuteInternal(): fullPath → {FullPath}", fullPath);
            var files = this.fileSystemProvider.EnumerateFiles(fullPath, this.SearchPattern, this.SearchOption);
			_logger.LogDebug("ExecuteInternal(): files → {Files}", files);
			_logger.LogDebug("ExecuteInternal(): count → {Count}", count);

            foreach (var file in files)
            {
                this.fileSystemProvider.DeleteFile(file);
                count++;
            }

            this.Success = true;
			_logger.LogDebug("ExecuteInternal(): this.Success → {Arg0}", this.Success);
			_logger.LogDebug("<<< Exiting ExecuteInternal()");
            this.Notes = this.LocalizeFormat("UninstallStepCountOfFilesDeleted", count);
        }
    }
}
