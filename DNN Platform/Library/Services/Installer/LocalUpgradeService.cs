// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Services.Installer.Installers;
using DotNetNuke.Services.Installer.Packages;

using Microsoft.Extensions.DependencyInjection;

/// <summary>The default <see cref="ILocalUpgradeService"/> implementation.</summary>
public class LocalUpgradeService : ILocalUpgradeService
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalUpgradeService));
    private static readonly string[] DefaultUpgradeExclude = new[]
    {
        "App_Data/Database.mdf",
        "Config/DotNetNuke.config",
        "Install/InstallWizard",
        "favicon.ico",
        "robots.txt",
        "web.config",
    };

    private readonly IApplicationInfo application;
    private readonly IApplicationStatusInfo appStatus;
    private readonly IDirectory directory;

    /// <summary>Initializes a new instance of the <see cref="LocalUpgradeService"/> class.</summary>
    /// <param name="application">The application.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="directory">The directory.</param>
    public LocalUpgradeService(IApplicationInfo application, IApplicationStatusInfo appStatus, IDirectory directory)
    {
		Logger.LogDebug(">>> Entering LocalUpgradeService(application={Application},appStatus={AppStatus},directory={Directory})", application, appStatus, directory);
        this.application = application ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
        this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
		Logger.LogDebug("LocalUpgradeService(application={Application},appStatus={AppStatus},directory={Directory}): appStatus → {AppStatus1}", application, appStatus, directory, appStatus);
		Logger.LogDebug("LocalUpgradeService(application={Application},appStatus={AppStatus},directory={Directory}): application → {Application1}", application, appStatus, directory, application);
        this.directory = directory ?? Globals.GetCurrentServiceProvider().GetRequiredService<IDirectory>();
		Logger.LogDebug("LocalUpgradeService(application={Application},appStatus={AppStatus},directory={Directory}): directory → {Directory1}", application, appStatus, directory, directory);
		Logger.LogDebug("<<< Exiting LocalUpgradeService(application={Application},appStatus={AppStatus},directory={Directory})", application, appStatus, directory);
    }

    private string UpgradeDirectoryPath => Path.Combine(this.appStatus.ApplicationMapPath, "App_Data", "Upgrade");

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocalUpgradeInfo>> GetLocalUpgrades(CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering GetLocalUpgrades(cancellationToken=<redacted>)");
        IReadOnlyList<LocalUpgradeInfo> localUpgrades;
        if (this.directory.Exists(this.UpgradeDirectoryPath))
        {
            // clean up any old temp files
            var tempFiles = this.directory.GetFiles(this.UpgradeDirectoryPath, "*.resources", SearchOption.TopDirectoryOnly);
			Logger.LogDebug("GetLocalUpgrades(cancellationToken=<redacted>): tempFiles → {TempFiles}", tempFiles);
            foreach (var tempFile in tempFiles)
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in GetLocalUpgrades(cancellationToken=<redacted>): {Arg1}", ex.getMessage());
                    Exceptions.LogException(ex);
                }
            }

            var upgradeFiles = this.directory.GetFiles(this.UpgradeDirectoryPath, "*.zip", SearchOption.TopDirectoryOnly);
			Logger.LogDebug("GetLocalUpgrades(cancellationToken=<redacted>): upgradeFiles → {UpgradeFiles}", upgradeFiles);
            localUpgrades = await Task.WhenAll(upgradeFiles.Select(file => this.GetLocalUpgradeInfo(file, cancellationToken)));
        }
        else
        {
            localUpgrades = [];
        }

		Logger.LogDebug("<<< Exiting GetLocalUpgrades(cancellationToken=<redacted>)");
        return localUpgrades;
    }

    /// <inheritdoc />
    public async Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string file, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering GetLocalUpgradeInfo(file={File},cancellationToken=<redacted>)", file);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var archiveStream = File.OpenRead(file);
			Logger.LogDebug("GetLocalUpgradeInfo(file={File},cancellationToken=<redacted>): archiveStream → {ArchiveStream}", file, archiveStream);
            return await this.GetLocalUpgradeInfo(Path.GetFileNameWithoutExtension(file), archiveStream, cancellationToken);
        }
        catch (Exception exception)
        {
			Logger.LogError("Exception in GetLocalUpgradeInfo(file={File},cancellationToken=<redacted>): {Arg2}", file, exception.Message);
            Exceptions.LogException(exception);
			Logger.LogInformation("GetLocalUpgradeInfo(file,cancellationToken)={File},{CancellationToken}: external service call took {Arg2} ms", file, cancellationToken, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting GetLocalUpgradeInfo(file={File},cancellationToken=<redacted>)", file);
            return new LocalUpgradeInfo
            {
                PackageName = Path.GetFileNameWithoutExtension(file),
                Version = null,
                IsValid = false,
                IsOutdated = false,
            };
		Logger.LogDebug("<<< Exiting GetLocalUpgradeInfo(packageName={PackageName},archiveStream={ArchiveStream},cancellationToken=<redacted>)", packageName, archiveStream);
        }
    }

    /// <inheritdoc />
    public async Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string packageName, Stream archiveStream, CancellationToken cancellationToken)
    {
        try
        {
            var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read);
            var mainAssemblyEntry = archive.FileEntries()
                .Where(entry => string.Equals("DotNetNuke.dll", entry.Name, StringComparison.OrdinalIgnoreCase))
                .Where(entry => string.Equals("bin", Path.GetDirectoryName(entry.FullName), StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();

            Version mainAssemblyVersion = null;
            if (mainAssemblyEntry is not null)
            {
                mainAssemblyVersion = await ReadZippedAssemblyVersion(mainAssemblyEntry, cancellationToken);
            }

            var upgradeInfo = archive.FileEntries()
                .Where(entry => string.Equals("App_Data/Upgrade/upgrade.json", entry.FullName, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();
            var upgradeSettings = new LocalUpgradeSettings
            {
                MinimumDnnVersion = "0.0.0",
            };
            if (upgradeInfo is not null)
            {
                var upgradeInfoFile = upgradeInfo.ReadTextFile();
                upgradeSettings = Json.Deserialize<LocalUpgradeSettings>(upgradeInfoFile);
            }

            var res = new LocalUpgradeInfo
            {
                PackageName = packageName,
                Version = mainAssemblyVersion,
                IsValid = mainAssemblyVersion is not null,
                IsOutdated = mainAssemblyVersion is not null && mainAssemblyVersion <= this.application.Version,
                MinDnnVersion = Version.Parse(upgradeSettings.MinimumDnnVersion),
            };
            res.CanInstall = res.IsValid
                && !res.IsOutdated
                && (res.MinDnnVersion is null || res.MinDnnVersion <= this.application.Version);

            return res;
        }
        catch (Exception exception)
        {
            Exceptions.LogException(exception);
            return new LocalUpgradeInfo
            {
                PackageName = packageName,
                Version = null,
                IsValid = false,
                IsOutdated = false,
                MinDnnVersion = null,
                CanInstall = false,
            };
        }
    }

    /// <inheritdoc />
    public async Task DeleteLocalUpgrade(string packageName, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering DeleteLocalUpgrade(packageName={PackageName},cancellationToken=<redacted>)", packageName);
        var upgrades = await this.GetLocalUpgrades(cancellationToken);
        var upgrade = upgrades.FirstOrDefault(u => u.PackageName.Equals(packageName, StringComparison.OrdinalIgnoreCase));
		Logger.LogDebug("DeleteLocalUpgrade(packageName={PackageName},cancellationToken=<redacted>): upgrade → {Upgrade}", packageName, upgrade);
        var packagePath = Path.Combine(this.UpgradeDirectoryPath, upgrade.PackageName + ".zip");
		Logger.LogDebug("DeleteLocalUpgrade(packageName={PackageName},cancellationToken=<redacted>): packagePath → {PackagePath}", packageName, packagePath);
		Logger.LogDebug("DeleteLocalUpgrade(packageName={PackageName},cancellationToken=<redacted>): upgrades → {Upgrades}", packageName, upgrades);

        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }
		Logger.LogDebug("<<< Exiting DeleteLocalUpgrade(packageName={PackageName},cancellationToken=<redacted>)", packageName);
    }

    /// <inheritdoc />
    public async Task StartLocalUpgrade(IReadOnlyList<LocalUpgradeInfo> upgrades, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering StartLocalUpgrade(upgrades={Upgrades},cancellationToken=<redacted>)", upgrades);
        var upgrade = upgrades.Where(u => u.IsValid && !u.IsOutdated).OrderBy(u => u.Version).First();
        await this.StartLocalUpgrade(upgrade, cancellationToken);
		Logger.LogDebug("StartLocalUpgrade(upgrades={Upgrades},cancellationToken=<redacted>): upgrade → {Upgrade}", upgrades, upgrade);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
		Logger.LogDebug("<<< Exiting StartLocalUpgrade(upgrades={Upgrades},cancellationToken=<redacted>)", upgrades);
    }

    /// <inheritdoc />
    public async Task StartLocalUpgrade(LocalUpgradeInfo upgrade, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>)", upgrade);
        var upgradeZipPath = Path.Combine(this.UpgradeDirectoryPath, upgrade.PackageName + ".zip");
        using var fileStream = File.OpenRead(upgradeZipPath);
		Logger.LogDebug("StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>): upgradeZipPath → {UpgradeZipPath}", upgrade, upgradeZipPath);
		Logger.LogDebug("StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>): fileStream → {FileStream}", upgrade, fileStream);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        var upgradeInfo = archive.FileEntries()
            .Where(entry => string.Equals("App_Data/Upgrade/upgrade.json", entry.FullName, StringComparison.OrdinalIgnoreCase))
            .SingleOrDefault();
			Logger.LogDebug("StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>): archive → {Archive}", upgrade, archive);
        var upgradeSettings = new LocalUpgradeSettings
		Logger.LogDebug("StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>): upgradeSettings → {UpgradeSettings}", upgrade, upgradeSettings);
        {
            UpgradeExclude = DefaultUpgradeExclude,
        };
        if (upgradeInfo is not null)
        {
            var upgradeInfoFile = upgradeInfo.ReadTextFile();
			Logger.LogDebug("StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>): upgradeInfoFile → {UpgradeInfoFile}", upgrade, upgradeInfoFile);
            upgradeSettings = Json.Deserialize<LocalUpgradeSettings>(upgradeInfoFile);
        }

        var assemblyEntries = archive.FileEntries()
            .Where(entry => string.Equals(".dll", Path.GetExtension(entry.Name), StringComparison.OrdinalIgnoreCase))
            .Where(entry => string.Equals("bin", Path.GetDirectoryName(entry.FullName), StringComparison.OrdinalIgnoreCase))
            .ToHashSet();
        var installer = new LocalUpgradeAssemblyInstaller(new InstallerInfo(this.appStatus.ApplicationMapPath, InstallMode.Install) { IgnoreWhiteList = true, });
		Logger.LogDebug("StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>): installer → {Installer}", upgrade, installer);
        await installer.AddFiles(assemblyEntries, cancellationToken);
        installer.Install();
        installer.Commit();

        await FileSystemUtils.UnzipResourcesAsync(
            archive.FileEntries()
                .Where(entry => !assemblyEntries.Contains(entry))
                .Where(entry => !upgradeSettings.UpgradeExclude.Any(filter => entry.FullName.StartsWith(filter, StringComparison.OrdinalIgnoreCase))),
            this.appStatus.ApplicationMapPath,
            cancellationToken);
			Logger.LogInformation("StartLocalUpgrade(upgrade,cancellationToken)={Upgrade},{CancellationToken}: external service call took {Arg2} ms", upgrade, cancellationToken, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting StartLocalUpgrade(upgrade={Upgrade},cancellationToken=<redacted>)", upgrade);
    }

    private static async Task<Version> ReadZippedAssemblyVersion(ZipArchiveEntry assemblyEntry, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken=<redacted>)", assemblyEntry);
        var tempPath = Path.Combine(Globals.InstallMapPath, "Temp", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        var tempAssemblyPath = Path.Combine(tempPath, assemblyEntry.Name);
		Logger.LogDebug("ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken=<redacted>): tempAssemblyPath → {TempAssemblyPath}", assemblyEntry, tempAssemblyPath);
		Logger.LogDebug("ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken=<redacted>): tempPath → {TempPath}", assemblyEntry, tempPath);

        Directory.CreateDirectory(tempPath);
        try
        {
            using (var tempAssemblyFileStream = File.Create(tempAssemblyPath))
            using (var assemblyStream = assemblyEntry.Open())
            {
                const int DefaultBufferSize = 81920;
                await assemblyStream.CopyToAsync(
                    tempAssemblyFileStream,
                    DefaultBufferSize,
                    cancellationToken);
            }

			Logger.LogDebug("<<< Exiting ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken=<redacted>)", assemblyEntry);
            return AssemblyName.GetAssemblyName(tempAssemblyPath).Version;
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    private class LocalUpgradeAssemblyInstaller : AssemblyInstaller
    {
        public LocalUpgradeAssemblyInstaller(InstallerInfo installerInfo)
        {
			Logger.LogDebug(">>> Entering LocalUpgradeAssemblyInstaller(installerInfo={InstallerInfo})", installerInfo);
            this.Package = new PackageInfo();
            this.Package.AttachInstallerInfo(installerInfo);
			Logger.LogDebug("<<< Exiting LocalUpgradeAssemblyInstaller(installerInfo={InstallerInfo})", installerInfo);
        }

        public async Task AddFiles(IEnumerable<ZipArchiveEntry> assemblyEntries, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering AddFiles(assemblyEntries={AssemblyEntries},cancellationToken=<redacted>)", assemblyEntries);
            foreach (var entry in assemblyEntries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var installFile = new InstallFile(entry, this.Package.InstallerInfo);
				Logger.LogDebug("AddFiles(assemblyEntries={AssemblyEntries},cancellationToken=<redacted>): installFile → {InstallFile}", assemblyEntries, installFile);
                installFile.SetVersion(await ReadZippedAssemblyVersion(entry, cancellationToken));
                this.Files.Add(installFile);
            }
			Logger.LogDebug("<<< Exiting AddFiles(assemblyEntries={AssemblyEntries},cancellationToken=<redacted>)", assemblyEntries);
        }
    }
}
