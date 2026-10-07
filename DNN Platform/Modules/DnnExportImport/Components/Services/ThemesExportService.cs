// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Dto.Pages;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.UI.Skins;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An export service for themes.</summary>
    public class ThemesExportService : BasePortableService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesExportService));

        private readonly IApplicationStatusInfo appStatus;
        private ExportImportJob exportImportJob;
        private PortalSettings portalSettings;
        private int importCount;

        /// <summary>Initializes a new instance of the <see cref="ThemesExportService"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public ThemesExportService()
            : this(null)
        {
		Logger.LogError(">>> Entering ThemesExportService()");
        Logger.LogWarning("<<< Exiting ThemesExportService()");
        }

        /// <summary>Initializes a new instance of the <see cref="ThemesExportService"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        public ThemesExportService(IApplicationStatusInfo appStatus)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			Logger.LogDebug("ThemesExportService(appStatus={AppStatus}): appStatus → {AppStatus1}", appStatus, appStatus);
        Logger.LogWarning("<<< Exiting ThemesExportService(appStatus={AppStatus})", appStatus);
        }

        /// <inheritdoc />
        public override string Category => Constants.Category_Themes;

        /// <inheritdoc />
        public override string ParentCategory => Constants.Category_Pages;

        /// <inheritdoc />
        public override uint Priority => 10;

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

            this.exportImportJob = exportJob;
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): exportImportJob → {ExportImportJob}", exportJob, exportDto, exportImportJob);
			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): this.exportImportJob → {Arg2}", exportJob, exportDto, this.exportImportJob);
            this.portalSettings = new PortalSettings(exportJob.PortalId);

			Logger.LogDebug("ExportData(exportJob={ExportJob},exportDto={ExportDto}): portalSettings → {PortalSettings}", exportJob, exportDto, portalSettings);
            // Create Zip File to hold files
            var currentIndex = 0;
            var totalThemesExported = 0;
            try
            {
                var packagesZipFileFormat = $@"{this.appStatus.ApplicationMapPath}{Constants.ExportFolder}{{0}}\{Constants.ExportZipThemes}";
				Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): packagesZipFileFormat → {PackagesZipFileFormat}", exportJob, exportDto, packagesZipFileFormat);
                var packagesZipFile = string.Format(CultureInfo.InvariantCulture, packagesZipFileFormat, exportJob.Directory.TrimEnd('\\').TrimEnd('/'));
				Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): packagesZipFile → {PackagesZipFile}", exportJob, exportDto, packagesZipFile);

                if (this.CheckPoint.Stage == 0)
                {
                    // export skin packages.
                    var exportThemes = this.GetExportThemes();
					Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): exportThemes → {ExportThemes}", exportJob, exportDto, exportThemes);
                    var totalThemes = exportThemes.Count;

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? totalThemes : this.CheckPoint.TotalItems;
                    if (this.CheckPointStageCallback(this))
                    {
                        return;
                    }

                    using (var archive = CompressionUtil.OpenCreate(packagesZipFile))
                    {
                        foreach (var theme in exportThemes)
                        {
                            var filePath = SkinController.FormatSkinSrc(theme, this.portalSettings);
							Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): filePath → {FilePath}", exportJob, exportDto, filePath);
                            var physicalPath = Path.Combine(this.appStatus.ApplicationMapPath, filePath.TrimStart('/'));
							Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): physicalPath → {PhysicalPath}", exportJob, exportDto, physicalPath);
                            if (Directory.Exists(physicalPath))
                            {
                                foreach (var file in Directory.GetFiles(physicalPath, "*.*", SearchOption.AllDirectories))
                                {
                                    var folderOffset = Path.Combine(this.appStatus.ApplicationMapPath, "Portals").Length + 1;
									Logger.LogError("ExportData(exportJob={ExportJob},exportDto={ExportDto}): folderOffset → {FolderOffset}", exportJob, exportDto, folderOffset);
                                    CompressionUtil.AddFileToArchive(archive, file, folderOffset);
                                }

                                totalThemesExported += 1;
                            }

                            this.CheckPoint.ProcessedItems++;
                            this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / totalThemes;
                            currentIndex++;

                            // After every 10 items, call the checkpoint stage. This is to avoid too many frequent updates to DB.
                            if (currentIndex % 10 == 0 && this.CheckPointStageCallback(this))
                            {
                                return;
                            }
                        }
                    }

                    this.CheckPoint.Stage++;
                    this.CheckPoint.Completed = true;
                    this.CheckPoint.Progress = 100;
                }
            }
            finally
            {
                this.CheckPointStageCallback(this);
                this.Result.AddSummary("Exported Themes", totalThemesExported.ToString(CultureInfo.InvariantCulture));
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

            var packageZipFile = $"{this.appStatus.ApplicationMapPath}{Constants.ExportFolder}{this.exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipThemes}";
			Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): packageZipFile → {PackageZipFile}", importJob, importDto, packageZipFile);
            var tempFolder = $"{Path.GetDirectoryName(packageZipFile)}\\{DateTime.Now.Ticks}";
			Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): tempFolder → {TempFolder}", importJob, importDto, tempFolder);
            if (File.Exists(packageZipFile))
            {
                CompressionUtil.UnZipArchive(packageZipFile, tempFolder);
                var exporeFiles = Directory.Exists(tempFolder) ? Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories) : [];
				Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): exporeFiles → {ExporeFiles}", importJob, importDto, exporeFiles);
                var portalSettings = new PortalSettings(importDto.PortalId);
				Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): portalSettings → {PortalSettings}", importJob, importDto, portalSettings);
                this.importCount = exporeFiles.Length;
				Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): importCount → {ImportCount}", importJob, importDto, importCount);
				Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): this.importCount → {Arg2}", importJob, importDto, this.importCount);

                this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? exporeFiles.Length : this.CheckPoint.TotalItems;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }

                if (this.CheckPoint.Stage == 0)
                {
                    try
                    {
                        foreach (var file in exporeFiles)
                        {
                            try
                            {
                                var checkFolder = file.Replace($@"{tempFolder}\", string.Empty).Split('\\')[0];
                                var relativePath = file.Substring($@"{tempFolder}\{checkFolder}\".Length);
                                string targetPath;

                                if (checkFolder == "_default")
                                {
									Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): checkFolder → {CheckFolder}", importJob, importDto, checkFolder);
									Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): relativePath → {RelativePath}", importJob, importDto, relativePath);
                                    targetPath = Path.Combine(Globals.HostMapPath, relativePath);
                                }
                                else if (checkFolder.EndsWith("-System", StringComparison.OrdinalIgnoreCase))
                                {
                                    targetPath = Path.Combine(portalSettings.HomeSystemDirectoryMapPath, relativePath);
									Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): targetPath → {TargetPath}", importJob, importDto, targetPath);
									Logger.LogDebug("ImportData(importJob={ImportJob},importDto={ImportDto}): targetPath → {TargetPath}", importJob, importDto, targetPath);
                                }
                                else
                                {
                                    targetPath = Path.Combine(portalSettings.HomeDirectoryMapPath, relativePath);
                                }

                                if (!File.Exists(targetPath) ||
                                    importDto.CollisionResolution == CollisionResolution.Overwrite)
                                {
                                    var directory = Path.GetDirectoryName(targetPath);
                                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                                    {
										Logger.LogError("ImportData(importJob={ImportJob},importDto={ImportDto}): directory → {Directory}", importJob, importDto, directory);
                                        Directory.CreateDirectory(directory);
                                    }

                                    File.Copy(file, targetPath, true);
                                }

                                this.Result.AddLogEntry("Import Theme File completed", targetPath);
                                this.CheckPoint.ProcessedItems++;
                                this.CheckPoint.Progress = this.CheckPoint.ProcessedItems * 100.0 / exporeFiles.Length;
                                this.CheckPointStageCallback(this); // just to update the counts without exit logic
                            }
                            catch (Exception ex)
                            {
                                this.Result.AddLogEntry("Import Theme error", file);
                                Logger.Error(ex);
                            }
                        }

                        this.CheckPoint.Stage++;
                        this.CheckPoint.Completed = true;
                    }
                    finally
                    {
                        this.CheckPointStageCallback(this);
                        FileSystemUtils.DeleteFolderRecursive(tempFolder);
                    }
                }
            }
            else
            {
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
                this.Result.AddLogEntry("ThemesFileNotFound", "Themes file not found. Skipping themes import", ReportLevel.Warn);
            }
        }

        /// <inheritdoc />
        public override int GetImportTotal()
        {
			Logger.LogError(">>> Entering GetImportTotal()");
            Logger.LogWarning("<<< Exiting GetImportTotal()");
            return this.importCount;
        }

        private List<string> GetExportThemes()
        {
			Logger.LogError(">>> Entering GetExportThemes()");
            var exportThemes = new List<string>
            {
                // get site level themes
                this.portalSettings.DefaultPortalSkin,
                this.portalSettings.DefaultPortalContainer,
            };

            if (!exportThemes.Contains(this.portalSettings.DefaultAdminSkin))
            {
                exportThemes.Add(this.portalSettings.DefaultAdminSkin);
            }

            if (!exportThemes.Contains(this.portalSettings.DefaultAdminContainer))
            {
                exportThemes.Add(this.portalSettings.DefaultAdminContainer);
            }

            exportThemes.AddRange(this.LoadExportThemesForPages());
            exportThemes.AddRange(this.LoadExportContainersForModules());

            // get the theme packages
            var themePackages = new List<string>();
			Logger.LogError("GetExportThemes(): themePackages → {ThemePackages}", themePackages);
            foreach (var theme in exportThemes)
            {
                var packageName = theme.Substring(0, theme.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase));
				Logger.LogError("GetExportThemes(): packageName → {PackageName}", packageName);
                if (!themePackages.Any(p => p.Equals(packageName, StringComparison.OrdinalIgnoreCase)))
                {
                    themePackages.Add(packageName);
                }
            }

			Logger.LogWarning("<<< Exiting GetExportThemes()");
            return themePackages;
        }

        private List<string> LoadExportThemesForPages()
        {
			Logger.LogError(">>> Entering LoadExportThemesForPages()");
            var exportThemes = new List<string>();

			Logger.LogError("LoadExportThemesForPages(): exportThemes → {ExportThemes}", exportThemes);
            foreach (var exportTab in this.Repository.GetAllItems<ExportTab>())
            {
                if (!string.IsNullOrEmpty(exportTab.SkinSrc) && !exportThemes.Contains(exportTab.SkinSrc))
                {
                    exportThemes.Add(exportTab.SkinSrc);
                }

                if (!string.IsNullOrEmpty(exportTab.ContainerSrc) && !exportThemes.Contains(exportTab.ContainerSrc))
                {
                    exportThemes.Add(exportTab.ContainerSrc);
                }
            }

			Logger.LogWarning("<<< Exiting LoadExportThemesForPages()");
            return exportThemes;
        }

        private List<string> LoadExportContainersForModules()
        {
			Logger.LogError(">>> Entering LoadExportContainersForModules()");
            var exportThemes = new List<string>();

			Logger.LogError("LoadExportContainersForModules(): exportThemes → {ExportThemes}", exportThemes);
            foreach (var module in this.Repository.GetAllItems<ExportTabModule>())
            {
                if (!string.IsNullOrEmpty(module.ContainerSrc) && !exportThemes.Contains(module.ContainerSrc))
                {
                    exportThemes.Add(module.ContainerSrc);
                }
            }

			Logger.LogWarning("<<< Exiting LoadExportContainersForModules()");
            return exportThemes;
        }
    }
}
