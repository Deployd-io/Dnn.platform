// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Pages;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;

    using Microsoft.Extensions.DependencyInjection;

    using Newtonsoft.Json;

    /// <summary>An export service for extension packages.</summary>
    public class PackagesExportService : BasePortableService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PackagesExportService));

        private static readonly Regex ExtensionPackageFilesRegex = new Regex(@"^(.+?)_(.+?)_(\d+\.\d+\.\d+).resources$", RegexOptions.Compiled);

        private readonly IApplicationStatusInfo appStatus;
        private ExportImportJob exportImportJob;

        /// <summary>Initializes a new instance of the <see cref="PackagesExportService"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public PackagesExportService()
            : this(null)
        {
		Logger.LogError(">>> Entering PackagesExportService()");
        Logger.LogWarning("<<< Exiting PackagesExportService()");
        }

        /// <summary>Initializes a new instance of the <see cref="PackagesExportService"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        public PackagesExportService(IApplicationStatusInfo appStatus)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			Logger.LogDebug("PackagesExportService(appStatus={AppStatus}): appStatus → {AppStatus1}", appStatus, appStatus);
        Logger.LogWarning("<<< Exiting PackagesExportService(appStatus={AppStatus})", appStatus);
        }

        /// <inheritdoc />
        public override string Category => Constants.Category_Packages;

        /// <inheritdoc />
        public override string ParentCategory => null;

        /// <inheritdoc />
        public override uint Priority => 18; // execute before pages service.

        /// <inheritdoc />
        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
			Logger.LogError(">>> Entering ExportData(exportJob={ExportJob},exportDto={ExportDto})", exportJob, exportDto);
            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            // Skip the export if all the folders have been processed already.
            if (this.CheckPoint.Stage >= 1)
            {
                return;
            }

            // Create Zip File to hold files
            var skip = this.GetCurrentSkip();
			Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): skip → {Skip}", exportJob, exportDto, skip);
            var currentIndex = skip;
			Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): currentIndex → {CurrentIndex}", exportJob, exportDto, currentIndex);
            var totalPackagesExported = 0;
            try
            {
                var packagesZipFileFormat = $@"{this.appStatus.ApplicationMapPath}{Constants.ExportFolder}{{0}}\{Constants.ExportZipPackages}";
				Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): packagesZipFileFormat → {PackagesZipFileFormat}", exportJob, exportDto, packagesZipFileFormat);
                var packagesZipFile = string.Format(CultureInfo.InvariantCulture, packagesZipFileFormat, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));
				Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): packagesZipFile → {PackagesZipFile}", exportJob, exportDto, packagesZipFile);

                if (this.CheckPoint.Stage == 0)
                {
                    var fromDate = exportDto.FromDateUtc ?? Constants.MinDbTime;
                    var toDate = exportDto.ToDateUtc;

                    // export skin packages.
                    var extensionPackagesBackupFolder = Path.Combine(this.appStatus.ApplicationMapPath, DotNetNuke.Services.Installer.Util.BackupInstallPackageFolder);
					Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): extensionPackagesBackupFolder → {ExtensionPackagesBackupFolder}", exportJob, exportDto, extensionPackagesBackupFolder);
                    var skinPackageFiles = Directory.GetFiles(extensionPackagesBackupFolder).Where(f => IsValidPackage(f, fromDate, toDate)).ToList();
					Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): skinPackageFiles → {SkinPackageFiles}", exportJob, exportDto, skinPackageFiles);
                    var totalPackages = skinPackageFiles.Count;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalPackages : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    using (var zipArchive = CompressionUtil.OpenCreate(packagesZipFile))
                    {
                        foreach (var file in skinPackageFiles)
                        {
                            var exportPackage = GenerateExportPackage(file);
							Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): exportPackage → {ExportPackage}", exportJob, exportDto, exportPackage);
                            if (exportPackage != null)
                            {
								var __logStart = System.Diagnostics.Stopwatch.StartNew();
								Logger.LogError("ExportData(exportJob,exportDto)={ExportJob},{ExportDto}: database query executed in {Arg2} ms", exportJob, exportDto, __logStart.ElapsedMilliseconds);
                                this.Repository.CreateItem(exportPackage, null);
                                totalPackagesExported += 1;
                                var folderOffset = Path.GetDirectoryName(file)?.Length + 1;
								Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): folderOffset → {FolderOffset}", exportJob, exportDto, folderOffset);

                                CompressionUtil.AddFileToArchive(zipArchive, file, folderOffset.GetValueOrDefault(0));
                            }

                            this.CheckPoint.ProcessedItems++;
                            this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalPackages;
                            currentIndex++;

                            // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                            if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                            {
                                return;
                            }
                        }
                    }

                    this.CheckPoint.Stage++;
                    currentIndex = 0;
                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Progress = 100;
                }
            }
            finally
            {
                this.CheckPoint.StageData = currentIndex > 0 ? JsonConvert.SerializeObject(new { skip = currentIndex, }) : null;
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Packages", totalPackagesExported.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <inheritdoc />
        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
			Logger.LogError(">>> Entering ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Skip the export if all the templates have been processed already.
            if (this.CheckPoint.Stage >= 1 || this.CheckPoint.Completed)
            {
                return;
            }

            this.exportImportJob = importJob;
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): exportImportJob → {ExportImportJob}", importJob, importDto, exportImportJob);
			Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): this.exportImportJob → {Arg2}", importJob, importDto, this.exportImportJob);

            this.ProcessImportModulePackages(importDto);
			Logger.LogWarning("<<< Exiting ImportData(importJob={ImportJob},importDto={ImportDto})", importJob, importDto);
        }

        /// <inheritdoc />
        public override int GetImportTotal()
        {
			Logger.LogError(">>> Entering GetImportTotal()");
            Logger.LogWarning("<<< Exiting GetImportTotal()");
            return this.Repository.GetCount<ExportPackage>();
        }

        /// <summary>Install the package.</summary>
        /// <param name="filePath">The file path to the installer.</param>
        public void InstallPackage(string filePath)
        {
			Logger.LogError(">>> Entering InstallPackage(filePath={FilePath})", filePath);
            using var stream = new FileStream(filePath, FileMode.Open);
			Logger.LogError("InstallPackage(filePath={FilePath}): stream → {Stream}", filePath, stream);
            try
            {
                var installer = GetInstaller(this.appStatus, stream);
				Logger.LogError("InstallPackage(filePath={FilePath}): installer → {Installer}", filePath, installer);

                if (installer.IsValid)
                {
                    // Reset Log
                    installer.InstallerInfo.Log.Logs.Clear();

                    // Set the IgnoreWhiteList flag
                    installer.InstallerInfo.IgnoreWhiteList = true;

                    // Set the Repair flag
                    installer.InstallerInfo.RepairInstall = true;

                    // Install
                    installer.Install();
                }
            }
            catch (Exception ex)
            {
                this.Result.AddLogEntry("Import Package error", $"{filePath}. ERROR: {ex.Message}");
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting InstallPackage(filePath={FilePath})", filePath);
        }

        private static Installer GetInstaller(IApplicationStatusInfo appStatus, Stream stream)
        {
			Logger.LogError(">>> Entering GetInstaller(appStatus={AppStatus},stream={Stream})", appStatus, stream);
            var installer = new Installer(stream, appStatus.ApplicationMapPath, false, false)
            {
                InstallerInfo = { PortalID = Null.NullInteger },
            };

            // Read the manifest
            if (installer.InstallerInfo.ManifestFile != null)
            {
                installer.ReadManifest(true);
            }

            return installer;
        }

        private static bool IsValidPackage(string filePath, DateTime fromDate, DateTime toDate)
        {
			Logger.LogError(">>> Entering IsValidPackage(filePath={FilePath},fromDate={FromDate},toDate={ToDate})", filePath, fromDate, toDate);
            var fileInfo = new FileInfo(filePath);
			Logger.LogError("IsValidPackage(filePath={FilePath},fromDate={FromDate},toDate={ToDate}): fileInfo → {FileInfo}", filePath, fromDate, toDate, fileInfo);
            if (string.IsNullOrEmpty(fileInfo.Name) || fileInfo.LastWriteTimeUtc < fromDate || fileInfo.LastWriteTimeUtc > toDate)
            {
                return false;
            }

			Logger.LogWarning("<<< Exiting IsValidPackage(filePath={FilePath},fromDate={FromDate},toDate={ToDate})", filePath, fromDate, toDate);
            return fileInfo.Name.StartsWith("Skin_", StringComparison.OrdinalIgnoreCase) || fileInfo.Name.StartsWith("Container_", StringComparison.OrdinalIgnoreCase);
        }

        private static ExportPackage GenerateExportPackage(string filePath)
        {
			Logger.LogError(">>> Entering GenerateExportPackage(filePath={FilePath})", filePath);
            var fileName = Path.GetFileName(filePath);
			Logger.LogError("GenerateExportPackage(filePath={FilePath}): fileName → {FileName}", filePath, fileName);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var match = ExtensionPackageFilesRegex.Match(fileName);
			Logger.LogError("GenerateExportPackage(filePath={FilePath}): match → {Match}", filePath, match);
            if (!match.Success)
            {
                return null;
            }

            var packageType = match.Groups[1].Value;
            var packageName = match.Groups[2].Value;
            var version = new Version(match.Groups[3].Value);
			Logger.LogError("GenerateExportPackage(filePath={FilePath}): version → {Version}", filePath, version);

			Logger.LogWarning("<<< Exiting GenerateExportPackage(filePath={FilePath})", filePath);
            return new ExportPackage { PackageFileName = fileName, PackageName = packageName, PackageType = packageType, Version = version };
        }

        private int GetCurrentSkip()
        {
			Logger.LogError(">>> Entering GetCurrentSkip()");
            if (!string.IsNullOrEmpty(this.CheckPoint.StageData))
            {
                dynamic stageData = JsonConvert.DeserializeObject(this.CheckPoint.StageData);
                return Convert.ToInt32(stageData.skip) ?? 0;
            }

			Logger.LogWarning("<<< Exiting GetCurrentSkip()");
            return 0;
        }

        private void ProcessImportModulePackage(ExportPackage exportPackage, string tempFolder, CollisionResolution collisionResolution)
        {
			Logger.LogError(">>> Entering ProcessImportModulePackage(exportPackage={ExportPackage},tempFolder={TempFolder},collisionResolution={CollisionResolution})", exportPackage, tempFolder, collisionResolution);
            try
            {
                var filePath = Path.Combine(tempFolder, exportPackage.PackageFileName);
				Logger.LogError("ProcessImportModulePackage(exportPackage={ExportPackage},tempFolder={TempFolder},collisionResolution={CollisionResolution}): filePath → {FilePath}", exportPackage, tempFolder, collisionResolution, filePath);
                if (!File.Exists(filePath))
                {
                    return;
                }

                var packageType = exportPackage.PackageType;
                var packageName = exportPackage.PackageName;
                var version = exportPackage.Version;

                var existPackage = PackageController.Instance.GetExtensionPackage(
                    Null.NullInteger,
                    p => p.PackageType == packageType && p.Name == packageName);
                if (existPackage != null &&
                    (existPackage.Version > version ||
                     (existPackage.Version == version &&
                      collisionResolution == CollisionResolution.Ignore)))
                {
                    this.Result.AddLogEntry(
                        "Import Package ignores",
                        $"{packageName} has higher version {existPackage.Version} installed, ignore import it");
                    return;
                }

                this.InstallPackage(filePath);
                this.Result.AddLogEntry("Import Package completed", $"{packageName} version: {version}");
            }
            catch (Exception ex)
            {
                this.Result.AddLogEntry(
                    "Import Package error",
                    $"{exportPackage.PackageName} : {exportPackage.Version} - {ex.Message}");
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting ProcessImportModulePackage(exportPackage={ExportPackage},tempFolder={TempFolder},collisionResolution={CollisionResolution})", exportPackage, tempFolder, collisionResolution);
        }

        private void ProcessImportModulePackages(ImportDto importDto)
        {
			Logger.LogError(">>> Entering ProcessImportModulePackages(importDto={ImportDto})", importDto);
            var packageZipFile = $"{this.appStatus.ApplicationMapPath}{Constants.ExportFolder}{this.exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipPackages}";
            var tempFolder = $"{Path.GetDirectoryName(packageZipFile)}\\{DateTime.Now.Ticks}";
			Logger.LogError("ProcessImportModulePackages(importDto={ImportDto}): packageZipFile → {PackageZipFile}", importDto, packageZipFile);
            if (File.Exists(packageZipFile))
            {
				Logger.LogError("ProcessImportModulePackages(importDto={ImportDto}): tempFolder → {TempFolder}", importDto, tempFolder);
                CompressionUtil.UnZipArchive(packageZipFile, tempFolder);
                var exportPackages = this.Repository.GetAllItems<ExportPackage>().ToList();
				Logger.LogError("ProcessImportModulePackages(importDto={ImportDto}): exportPackages → {ExportPackages}", importDto, exportPackages);

                this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? exportPackages.Count : this.CheckPoint.TotalItems;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }

                if (this.CheckPoint.Stage == 0)
                {
                    try
                    {
                        foreach (var exportPackage in exportPackages)
                        {
                            this.ProcessImportModulePackage(exportPackage, tempFolder, importDto.CollisionResolution);

                            this.CheckPoint.ProcessedItems++;
                            this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / exportPackages.Count;
                            if (this.CheckPointStageCallback(this))
                            {
                                break;
                            }
                        }

                        this.CheckPoint.Stage++;
                        this.CheckPoint.Completed = true;
                    }
                    finally
                    {
                        this.CheckPointStageCallback(this);
                        try
                        {
                            FileSystemUtils.DeleteFolderRecursive(tempFolder);
                        }
                        catch (Exception)
                        {
							Logger.LogError("Exception in ProcessImportModulePackages(importDto={ImportDto}): {Arg1}", importDto, Exception.Message);
                            // ignore
                        }
                    }
                }
            }
            else
            {
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
                this.Result.AddLogEntry("PackagesFileNotFound", "Packages file not found. Skipping packages import", ReportLevel.Warn);
            }
			Logger.LogWarning("<<< Exiting ProcessImportModulePackages(importDto={ImportDto})", importDto);
        }
    }
}
