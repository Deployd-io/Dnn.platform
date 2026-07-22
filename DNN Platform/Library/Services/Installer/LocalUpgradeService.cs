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
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
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
		Logger.LogDebug("<<< Exiting LocalUpgradeService(application={Application},appStatus={AppStatus},directory={Directory})", application, appStatus, directory);
		Logger.LogDebug("LocalUpgradeService(application={Application},appStatus={AppStatus},directory={Directory}): directory → {Directory1}", application, appStatus, directory, directory);
    }

    private string UpgradeDirectoryPath => Path.Combine(this.appStatus.ApplicationMapPath, "App_Data", "Upgrade");

    /// <inheritdoc />
    public async Task<IReadOnlyList<LocalUpgradeInfo>> GetLocalUpgrades(CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering GetLocalUpgrades(cancellationToken={CancellationToken})", cancellationToken);
        IReadOnlyList<LocalUpgradeInfo> localUpgrades;
        if (this.directory.Exists(this.UpgradeDirectoryPath))
        {
            // clean up any old temp files
            var tempFiles = this.directory.GetFiles(this.UpgradeDirectoryPath, "*.resources", SearchOption.TopDirectoryOnly);
            foreach (var tempFile in tempFiles)
            {
			Logger.LogDebug("GetLocalUpgrades(cancellationToken={CancellationToken}): tempFiles → {TempFiles}", cancellationToken, tempFiles);
                try
                {
                    File.Delete(tempFile);
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in GetLocalUpgrades(cancellationToken={CancellationToken}): {Arg1}", cancellationToken, ex.Message);
                    Exceptions.LogException(ex);
                }
            }

            var upgradeFiles = this.directory.GetFiles(this.UpgradeDirectoryPath, "*.zip", SearchOption.TopDirectoryOnly);
            localUpgrades = await Task.WhenAll(upgradeFiles.Select(file => this.GetLocalUpgradeInfo(file, cancellationToken)));
			Logger.LogDebug("GetLocalUpgrades(cancellationToken={CancellationToken}): upgradeFiles → {UpgradeFiles}", cancellationToken, upgradeFiles);
			Logger.LogDebug("GetLocalUpgrades(cancellationToken={CancellationToken}): localUpgrades → {LocalUpgrades}", cancellationToken, localUpgrades);
        }
        else
        {
            localUpgrades = [];
        }

		Logger.LogDebug("<<< Exiting GetLocalUpgrades(cancellationToken={CancellationToken})", cancellationToken);
        return localUpgrades;
    }

    /// <inheritdoc />
    public async Task<LocalUpgradeInfo> GetLocalUpgradeInfo(string file, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering GetLocalUpgradeInfo(file={File},cancellationToken={CancellationToken})", file, cancellationToken);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var archiveStream = File.OpenRead(file);
            return await this.GetLocalUpgradeInfo(Path.GetFileNameWithoutExtension(file), archiveStream, cancellationToken);
        }
        catch (Exception exception)
        {
			Logger.LogError("Exception in GetLocalUpgradeInfo(file={File},cancellationToken={CancellationToken}): {Arg2}", file, cancellationToken, ex.Message);
            Exceptions.LogException(exception);
			Logger.LogInformation("GetLocalUpgradeInfo(file,cancellationToken)={File},{CancellationToken}: external service call took {Arg2} ms", file, cancellationToken, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting GetLocalUpgradeInfo(file={File},cancellationToken={CancellationToken})", file, cancellationToken);
            return new LocalUpgradeInfo
            {
                PackageName = Path.GetFileNameWithoutExtension(file),
                Version = null,
                IsValid = false,
                IsOutdated = false,
            };
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
			Logger.LogDebug("<<< Exiting GetLocalUpgradeInfo(packageName={PackageName},archiveStream={ArchiveStream},cancellationToken={CancellationToken})", packageName, archiveStream, cancellationToken);
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
		Logger.LogDebug(">>> Entering DeleteLocalUpgrade(packageName={PackageName},cancellationToken={CancellationToken})", packageName, cancellationToken);
        var upgrades = await this.GetLocalUpgrades(cancellationToken);
        var upgrade = upgrades.FirstOrDefault(u => u.PackageName.Equals(packageName, StringComparison.OrdinalIgnoreCase));
		Logger.LogDebug("DeleteLocalUpgrade(packageName={PackageName},cancellationToken={CancellationToken}): upgrades → {Upgrades}", packageName, cancellationToken, upgrades);
		Logger.LogDebug("DeleteLocalUpgrade(packageName={PackageName},cancellationToken={CancellationToken}): upgrade → {Upgrade}", packageName, cancellationToken, upgrade);
        var packagePath = Path.Combine(this.UpgradeDirectoryPath, upgrade.PackageName + ".zip");

		Logger.LogDebug("DeleteLocalUpgrade(packageName={PackageName},cancellationToken={CancellationToken}): packagePath → {PackagePath}", packageName, cancellationToken, packagePath);
        if (File.Exists(packagePath))
        {
            File.Delete(packagePath);
        }
		Logger.LogDebug("<<< Exiting DeleteLocalUpgrade(packageName={PackageName},cancellationToken={CancellationToken})", packageName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task StartLocalUpgrade(IReadOnlyList<LocalUpgradeInfo> upgrades, CancellationToken cancellationToken)
    {
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        var upgrade = upgrades.Where(u => u.IsValid && !u.IsOutdated).OrderBy(u => u.Version).First();
        await this.StartLocalUpgrade(upgrade, cancellationToken);
    }

    /// <inheritdoc />
    public async Task StartLocalUpgrade(LocalUpgradeInfo upgrade, CancellationToken cancellationToken)
    {
        var upgradeZipPath = Path.Combine(this.UpgradeDirectoryPath, upgrade.PackageName + ".zip");
        using var fileStream = File.OpenRead(upgradeZipPath);
		var __logStart = System.Diagnostics.Stopwatch.StartNew();
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        var upgradeInfo = archive.FileEntries()
            .Where(entry => string.Equals("App_Data/Upgrade/upgrade.json", entry.FullName, StringComparison.OrdinalIgnoreCase))
            .SingleOrDefault();
        var upgradeSettings = new LocalUpgradeSettings
        {
            UpgradeExclude = DefaultUpgradeExclude,
        };
        if (upgradeInfo is not null)
        {
            var upgradeInfoFile = upgradeInfo.ReadTextFile();
            upgradeSettings = Json.Deserialize<LocalUpgradeSettings>(upgradeInfoFile);
        }

        var assemblyEntries = archive.FileEntries()
            .Where(entry => string.Equals(".dll", Path.GetExtension(entry.Name), StringComparison.OrdinalIgnoreCase))
            .Where(entry => string.Equals("bin", Path.GetDirectoryName(entry.FullName), StringComparison.OrdinalIgnoreCase))
            .ToHashSet();
        var installer = new LocalUpgradeAssemblyInstaller(new InstallerInfo(this.appStatus.ApplicationMapPath, InstallMode.Install) { IgnoreWhiteList = true, });
        await installer.AddFiles(assemblyEntries, cancellationToken);
        installer.Install();
        installer.Commit();

        await FileSystemUtils.UnzipResourcesAsync(
            archive.FileEntries()
                .Where(entry => !assemblyEntries.Contains(entry))
                .Where(entry => !upgradeSettings.UpgradeExclude.Any(filter => entry.FullName.StartsWith(filter, StringComparison.OrdinalIgnoreCase))),
            this.appStatus.ApplicationMapPath,
            cancellationToken);
    }

    private static async Task<Version> ReadZippedAssemblyVersion(ZipArchiveEntry assemblyEntry, CancellationToken cancellationToken)
    {
		Logger.LogDebug(">>> Entering ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken={CancellationToken})", assemblyEntry, cancellationToken);
        var tempPath = Path.Combine(Globals.InstallMapPath, "Temp", Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
        var tempAssemblyPath = Path.Combine(tempPath, assemblyEntry.Name);
		Logger.LogDebug("ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken={CancellationToken}): tempPath → {TempPath}", assemblyEntry, cancellationToken, tempPath);
		Logger.LogDebug("ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken={CancellationToken}): tempAssemblyPath → {TempAssemblyPath}", assemblyEntry, cancellationToken, tempAssemblyPath);

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

			Logger.LogDebug("<<< Exiting ReadZippedAssemblyVersion(assemblyEntry={AssemblyEntry},cancellationToken={CancellationToken})", assemblyEntry, cancellationToken);
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
            foreach (var entry in assemblyEntries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var installFile = new InstallFile(entry, this.Package.InstallerInfo);
                installFile.SetVersion(await ReadZippedAssemblyVersion(entry, cancellationToken));
                this.Files.Add(installFile);
            }
        }
    }
}
