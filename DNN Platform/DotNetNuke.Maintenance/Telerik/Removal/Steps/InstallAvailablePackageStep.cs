// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Shims;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;

    /// <inheritdoc cref="IInstallAvailablePackageStep" />
    internal sealed class InstallAvailablePackageStep : StepBase, IInstallAvailablePackageStep
    {
        private readonly IApplicationStatusInfo appStatusInfo;
        private readonly IFileSystemProvider fsProvider;
        private readonly IPackageController packageController;

        /// <summary>Initializes a new instance of the <see cref="InstallAvailablePackageStep"/> class.</summary>
        /// <param name="loggerSource">An instance of <see cref="ILoggerSource"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        /// <param name="appStatusInfo">An instance of <see cref="IApplicationStatusInfo"/>.</param>
        /// <param name="fsProvider">An instance of <see cref="IFileSystemProvider"/>.</param>
        /// <param name="packageController">An instance of <see cref="IPackageController"/>.</param>
        public InstallAvailablePackageStep(
            ILoggerSource loggerSource,
            ILocalizer localizer,
            IApplicationStatusInfo appStatusInfo,
            IFileSystemProvider fsProvider,
            IPackageController packageController)
            : base(loggerSource, localizer)
        {
			_logger.LogDebug(">>> Entering InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController})", loggerSource, localizer, appStatusInfo, fsProvider, packageController);
            this.appStatusInfo = appStatusInfo ??
				_logger.LogWarning("Throwing ArgumentNullException from InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController})", loggerSource, localizer, appStatusInfo, fsProvider, packageController);
				_logger.LogDebug("InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController}): appStatusInfo → {AppStatusInfo1}", loggerSource, localizer, appStatusInfo, fsProvider, packageController, appStatusInfo);
				_logger.LogDebug("InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController}): this.appStatusInfo → {Arg5}", loggerSource, localizer, appStatusInfo, fsProvider, packageController, this.appStatusInfo);
                throw new ArgumentNullException(nameof(appStatusInfo));

            this.fsProvider = fsProvider ??
				_logger.LogDebug("InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController}): fsProvider → {FsProvider1}", loggerSource, localizer, appStatusInfo, fsProvider, packageController, fsProvider);
				_logger.LogDebug("InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController}): this.fsProvider → {Arg5}", loggerSource, localizer, appStatusInfo, fsProvider, packageController, this.fsProvider);
				_logger.LogDebug("InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController}): packageController → {PackageController1}", loggerSource, localizer, appStatusInfo, fsProvider, packageController, packageController);
                throw new ArgumentNullException(nameof(fsProvider));

            this.packageController = packageController ??
				_logger.LogDebug("<<< Exiting InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController})", loggerSource, localizer, appStatusInfo, fsProvider, packageController);
				_logger.LogDebug("InstallAvailablePackageStep(loggerSource={LoggerSource},localizer={Localizer},appStatusInfo={AppStatusInfo},fsProvider={FsProvider},packageController={PackageController}): this.packageController → {Arg5}", loggerSource, localizer, appStatusInfo, fsProvider, packageController, this.packageController);
                throw new ArgumentNullException(nameof(packageController));
        }

        /// <inheritdoc />
        public override string Name => this.LocalizeFormat("UninstallStepInstallPackage", this.PackageName);

        /// <inheritdoc />
        [Required]
        public string PackageFileNamePattern { get; set; }

        /// <inheritdoc />
        [Required]
        public string PackageName { get; set; }

        /// <inheritdoc />
        [Required]
        public string PackageType { get; set; }

        /// <inheritdoc />
        protected override void ExecuteInternal()
        {
			_logger.LogDebug(">>> Entering ExecuteInternal()");
            if (this.PackageIsAlreadyInstalled())
            {
                this.Success = true;
				_logger.LogDebug("ExecuteInternal(): this.Success → {Arg0}", this.Success);
                this.Notes = this.LocalizeFormat("UninstallStepPackageAlreadyInstalled", this.PackageName);
                return;
            }

            var availableFiles = this.fsProvider.EnumerateFiles(
                Path.Combine(this.appStatusInfo.ApplicationMapPath, "Install", this.PackageType),
                this.PackageFileNamePattern,
                SearchOption.TopDirectoryOnly);

            var filePath = availableFiles
                .OrderByDescending(path => path)
                .FirstOrDefault();

            if (filePath is null)
            {
                this.Success = false;
                this.Notes = this.LocalizeFormat("UninstallStepPackageNotFound", this.PackageFileNamePattern);
                return;
            }

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			using Microsoft.Extensions.Logging;
            {
                var installer = new Installer(
                    stream,
                    this.appStatusInfo.ApplicationMapPath,
                    loadManifest: false,
                    deleteTemp: false);

                try
                {
					private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
                    installer.InstallerInfo.PortalID = Null.NullInteger;

                    if (installer.InstallerInfo.ManifestFile != null)
                    {
                        installer.ReadManifest(true);
                    }

                    installer.InstallerInfo.Log.Logs.Clear();
                    installer.InstallerInfo.IgnoreWhiteList = true;
                    installer.InstallerInfo.RepairInstall = true;

                    installer.Install();

                    if (!installer.IsValid)
                    {
                        this.Success = false;
                        this.Notes = string.Join(Environment.NewLine, installer.InstallerInfo.Log.Logs);
                    }
                    else
                    {
                        this.DeleteInstallFile(filePath);

                        this.Success = true;
                    }
                }
                finally
                {
                    this.DeleteTempInstallFiles(installer);
                }
            }
			_logger.LogDebug("<<< Exiting ExecuteInternal()");
        }

        private void DeleteInstallFile(string installerFile)
        {
			_logger.LogDebug(">>> Entering DeleteInstallFile(installerFile={InstallerFile})", installerFile);
            if (this.fsProvider.FileExists(installerFile))
            {
                this.fsProvider.SetFileAttributes(installerFile, FileAttributes.Normal);
                this.fsProvider.DeleteFile(installerFile);
            }
			_logger.LogDebug("<<< Exiting DeleteInstallFile(installerFile={InstallerFile})", installerFile);
        }

        private void DeleteTempInstallFiles(Installer installer)
        {
            var tempFolder = installer.TempInstallFolder;

            if (!string.IsNullOrEmpty(tempFolder) && this.fsProvider.DirectoryExists(tempFolder))
            {
                this.fsProvider.DeleteFolderRecursive(tempFolder);
            }
			_logger.LogDebug("<<< Exiting DeleteTempInstallFiles(installer={Installer})", installer);
        }

        private bool PackageIsAlreadyInstalled()
        {
			_logger.LogDebug(">>> Entering PackageIsAlreadyInstalled()");
            bool WithMatchingPackageName(PackageInfo p) =>
                "Module".Equals(p.PackageType, StringComparison.OrdinalIgnoreCase) &&
                this.PackageName.Equals(p.Name, StringComparison.OrdinalIgnoreCase);

			_logger.LogDebug("<<< Exiting PackageIsAlreadyInstalled()");
            return this.packageController.GetExtensionPackages(Null.NullInteger, WithMatchingPackageName).Any();
        }
    }
}
