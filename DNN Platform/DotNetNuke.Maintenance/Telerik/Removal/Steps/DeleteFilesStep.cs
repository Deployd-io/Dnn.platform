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

    /// <summary>Deletes files.</summary>
    internal sealed class DeleteFilesStep : StepBase, IDeleteFilesStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
            this.fileSystemProvider = fileSystemProvider ??
                throw new ArgumentNullException(nameof(fileSystemProvider));

            this.applicationStatusInfo = applicationStatusInfo ??
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
			Logger.LogDebug(">>> Entering ExecuteInternal()");
            var count = 0;
            var appPath = this.applicationStatusInfo.ApplicationMapPath + @"\";
			Logger.LogDebug("ExecuteInternal(): count → {Count}", count);
			Logger.LogDebug("ExecuteInternal(): appPath → {AppPath}", appPath);
            var fullPath = Path.GetFullPath(Path.Combine(appPath, this.RelativePath));
            var files = this.fileSystemProvider.EnumerateFiles(fullPath, this.SearchPattern, this.SearchOption);
			Logger.LogDebug("ExecuteInternal(): fullPath → {FullPath}", fullPath);
			Logger.LogDebug("ExecuteInternal(): files → {Files}", files);

            foreach (var file in files)
            {
                this.fileSystemProvider.DeleteFile(file);
                count++;
            }

            this.Success = true;
			Logger.LogDebug("ExecuteInternal(): this.Success → {Arg0}", this.Success);
            this.Notes = this.LocalizeFormat("UninstallStepCountOfFilesDeleted", count);
			Logger.LogDebug("<<< Exiting ExecuteInternal()");
        }
    }
}
