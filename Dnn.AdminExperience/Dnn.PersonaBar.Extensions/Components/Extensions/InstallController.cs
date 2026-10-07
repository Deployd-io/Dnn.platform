// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components
{
    using System;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;

    public class InstallController : ServiceLocator<IInstallController, InstallController>, IInstallController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallController));

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public ParseResultDto ParsePackage(PortalSettings portalSettings, UserInfo user, string filePath, Stream stream)
        {
			Logger.LogError(">>> Entering ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream})", portalSettings, user, filePath, stream);
            var parseResult = new ParseResultDto();
            var fileName = Path.GetFileName(filePath);
			Logger.LogError("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): fileName → {FileName}", portalSettings, user, filePath, stream, fileName);
            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
			Logger.LogError("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): extension → {Extension}", portalSettings, user, filePath, stream, extension);
			Logger.LogError("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): parseResult → {ParseResult}", portalSettings, user, filePath, stream, parseResult);

            if (extension != ".zip" && extension != ".resources")
            {
                parseResult.Failed("InvalidExt");
            }
            else
            {
                try
                {
                    var installer = GetInstaller(stream, fileName, portalSettings.PortalId);
					Logger.LogError("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): installer → {Installer}", portalSettings, user, filePath, stream, installer);

                    try
                    {
                        if (installer.IsValid)
                        {
                            if (installer.Packages.Count > 0)
                            {
                                parseResult = new ParseResultDto(installer.Packages[0].Package);
                            }

                            parseResult.AzureCompact = AzureCompact(installer).GetValueOrDefault(false);
                            parseResult.NoManifest = string.IsNullOrEmpty(installer.InstallerInfo.ManifestFile.TempFileName);
                            parseResult.LegacyError = installer.InstallerInfo.LegacyError;
							Logger.LogDebug("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): parseResult.LegacyError → {Arg4}", portalSettings, user, filePath, stream, parseResult.LegacyError);
							Logger.LogDebug("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): parseResult.AlreadyInstalled → {Arg4}", portalSettings, user, filePath, stream, parseResult.AlreadyInstalled);
                            parseResult.HasInvalidFiles = !installer.InstallerInfo.HasValidFiles;
							Logger.LogDebug("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): parseResult.HasInvalidFiles → {Arg4}", portalSettings, user, filePath, stream, parseResult.HasInvalidFiles);
                            parseResult.AlreadyInstalled = installer.InstallerInfo.Installed;
                            parseResult.AddLogs(installer.InstallerInfo.Log.Logs);
                        }
                        else
                        {
                            if (installer.InstallerInfo.ManifestFile == null)
                            {
                                parseResult.LegacySkinInstalled = CheckIfSkinAlreadyInstalled(fileName, installer, "Skin");
                                parseResult.LegacyContainerInstalled = CheckIfSkinAlreadyInstalled(fileName, installer, "Container");
                            }

                            parseResult.Failed("InvalidFile", installer.InstallerInfo.Log.Logs);
                            parseResult.NoManifest = string.IsNullOrEmpty(installer.InstallerInfo.ManifestFile?.TempFileName);
                            if (parseResult.NoManifest)
                            {
                                // we still can install when the manifest is missing
                                parseResult.Success = true;
								Logger.LogDebug("ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): parseResult.Success → {Arg4}", portalSettings, user, filePath, stream, parseResult.Success);
                            }
                        }
                    }
                    finally
                    {
                        DeleteTempInstallFiles(installer);
                    }
                }
                catch (InvalidDataException)
                {
					Logger.LogError("Exception in ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream}): {Arg4}", portalSettings, user, filePath, stream, InvalidDataException.Message);
                    parseResult.Failed("ZipCriticalError");
                }
            }

			Logger.LogWarning("<<< Exiting ParsePackage(portalSettings={PortalSettings},user={User},filePath={FilePath},stream={Stream})", portalSettings, user, filePath, stream);
            return parseResult;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public InstallResultDto InstallPackage(PortalSettings portalSettings, UserInfo user, string legacySkin, string filePath, Stream stream, bool isPortalPackage = false)
        {
			Logger.LogError(">>> Entering InstallPackage(portalSettings={PortalSettings},user={User},legacySkin={LegacySkin},filePath={FilePath},stream={Stream},isPortalPackage={IsPortalPackage})", portalSettings, user, legacySkin, filePath, stream, isPortalPackage);
            var installResult = new InstallResultDto();
            var fileName = Path.GetFileName(filePath);
			Logger.LogError("InstallPackage(portalSettings={PortalSettings},user={User},legacySkin={LegacySkin},filePath={FilePath},stream={Stream},isPortalPackage={IsPortalPackage}): fileName → {FileName}", portalSettings, user, legacySkin, filePath, stream, isPortalPackage, fileName);
            var extension = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();
			Logger.LogError("InstallPackage(portalSettings={PortalSettings},user={User},legacySkin={LegacySkin},filePath={FilePath},stream={Stream},isPortalPackage={IsPortalPackage}): extension → {Extension}", portalSettings, user, legacySkin, filePath, stream, isPortalPackage, extension);
			Logger.LogError("InstallPackage(portalSettings={PortalSettings},user={User},legacySkin={LegacySkin},filePath={FilePath},stream={Stream},isPortalPackage={IsPortalPackage}): installResult → {InstallResult}", portalSettings, user, legacySkin, filePath, stream, isPortalPackage, installResult);

            if (extension != ".zip" && extension != ".resources")
            {
                installResult.Failed("InvalidExt");
            }
            else
            {
                try
                {
                    var installer = GetInstaller(stream, fileName, portalSettings.PortalId, legacySkin, isPortalPackage);
					Logger.LogError("InstallPackage(portalSettings={PortalSettings},user={User},legacySkin={LegacySkin},filePath={FilePath},stream={Stream},isPortalPackage={IsPortalPackage}): installer → {Installer}", portalSettings, user, legacySkin, filePath, stream, isPortalPackage, installer);

                    try
                    {
                        if (installer.IsValid)
                        {
                            // Reset Log
                            installer.InstallerInfo.Log.Logs.Clear();

                            // Set the IgnnoreWhiteList flag
                            installer.InstallerInfo.IgnoreWhiteList = true;

                            // Set the Repair flag
                            installer.InstallerInfo.RepairInstall = true;

                            // Install
                            installer.Install();

                            installResult.AddLogs(installer.InstallerInfo.Log.Logs);
                            if (!installer.IsValid)
                            {
                                installResult.Failed("InstallError");
                            }
                            else
                            {
                                installResult.NewPackageId = installer.Packages.Count == 0
                                    ? Null.NullInteger
                                    : installer.Packages.First().Value.Package.PackageID;
                                installResult.Succeed();
                                DeleteInstallFile(filePath);
                            }
                        }
                        else
                        {
                            installResult.Failed("InstallError");
                        }
                    }
                    finally
                    {
                        DeleteTempInstallFiles(installer);
                    }
                }
                catch (InvalidDataException)
                {
					Logger.LogError("Exception in InstallPackage(portalSettings={PortalSettings},user={User},legacySkin={LegacySkin},filePath={FilePath},stream={Stream},isPortalPackage={IsPortalPackage}): {Arg6}", portalSettings, user, legacySkin, filePath, stream, isPortalPackage, InvalidDataException.Message);
                    installResult.Failed("ZipCriticalError");
                }
            }

			Logger.LogWarning("<<< Exiting InstallPackage(portalSettings={PortalSettings},user={User},legacySkin={LegacySkin},filePath={FilePath},stream={Stream},isPortalPackage={IsPortalPackage})", portalSettings, user, legacySkin, filePath, stream, isPortalPackage);
            return installResult;
        }

        /// <inheritdoc />
        protected override Func<IInstallController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new InstallController();
        }

        private static Installer GetInstaller(Stream stream, string fileName, int portalId, string legacySkin = null, bool isPortalPackage = false)
        {
			Logger.LogError(">>> Entering GetInstaller(stream={Stream},fileName={FileName},portalId={PortalId},legacySkin={LegacySkin},isPortalPackage={IsPortalPackage})", stream, fileName, portalId, legacySkin, isPortalPackage);
            var installer = new Installer(stream, Globals.ApplicationMapPath, false, false);
			Logger.LogError("GetInstaller(stream={Stream},fileName={FileName},portalId={PortalId},legacySkin={LegacySkin},isPortalPackage={IsPortalPackage}): installer → {Installer}", stream, fileName, portalId, legacySkin, isPortalPackage, installer);
            if (string.IsNullOrEmpty(installer.InstallerInfo.ManifestFile?.TempFileName) && !string.IsNullOrEmpty(legacySkin))
            {
                var manifestFile = CreateManifest(installer, fileName, legacySkin);
				Logger.LogError("GetInstaller(stream={Stream},fileName={FileName},portalId={PortalId},legacySkin={LegacySkin},isPortalPackage={IsPortalPackage}): manifestFile → {ManifestFile}", stream, fileName, portalId, legacySkin, isPortalPackage, manifestFile);

                // Re-evaluate the package after creating a temporary manifest
                installer = new Installer(installer.TempInstallFolder, manifestFile, Globals.ApplicationMapPath, false);
            }

            // We always assume we are installing from //Host/Extensions (in the previous releases)
            // This will not work when we try to install a skin/container under a specific portal.
            installer.InstallerInfo.PortalID = isPortalPackage ? portalId : Null.NullInteger;

            // Read the manifest
            if (installer.InstallerInfo.ManifestFile != null)
            {
                installer.ReadManifest(true);
            }

            return installer;
        }

        private static string CreateManifest(Installer installer, string fileName, string legacySkin)
        {
			Logger.LogError(">>> Entering CreateManifest(installer={Installer},fileName={FileName},legacySkin={LegacySkin})", installer, fileName, legacySkin);
            var manifestFile = Path.Combine(installer.TempInstallFolder, Path.GetFileNameWithoutExtension(fileName) + ".dnn");
			Logger.LogError("CreateManifest(installer={Installer},fileName={FileName},legacySkin={LegacySkin}): manifestFile → {ManifestFile}", installer, fileName, legacySkin, manifestFile);
            using (var manifestWriter = new StreamWriter(manifestFile))
            {
                manifestWriter.Write(LegacyUtil.CreateSkinManifest(fileName, legacySkin ?? "Skin", installer.TempInstallFolder));
                manifestWriter.Close();
            }

			Logger.LogWarning("<<< Exiting CreateManifest(installer={Installer},fileName={FileName},legacySkin={LegacySkin})", installer, fileName, legacySkin);
            return manifestFile;
        }

        private static bool CheckIfSkinAlreadyInstalled(string fileName, Installer installer, string legacySkin)
        {
			Logger.LogError(">>> Entering CheckIfSkinAlreadyInstalled(fileName={FileName},installer={Installer},legacySkin={LegacySkin})", fileName, installer, legacySkin);
            // this whole thing is to check for if already installed
            var manifestFile = CreateManifest(installer, fileName, legacySkin);
			Logger.LogError("CheckIfSkinAlreadyInstalled(fileName={FileName},installer={Installer},legacySkin={LegacySkin}): manifestFile → {ManifestFile}", fileName, installer, legacySkin, manifestFile);
            var installer2 = new Installer(installer.TempInstallFolder, manifestFile, Globals.ApplicationMapPath, false);
			Logger.LogError("CheckIfSkinAlreadyInstalled(fileName={FileName},installer={Installer},legacySkin={LegacySkin}): installer2 → {Installer2}", fileName, installer, legacySkin, installer2);
            installer2.InstallerInfo.PortalID = installer.InstallerInfo.PortalID;
            if (installer2.InstallerInfo.ManifestFile != null)
            {
                installer2.ReadManifest(true);
            }

			Logger.LogWarning("<<< Exiting CheckIfSkinAlreadyInstalled(fileName={FileName},installer={Installer},legacySkin={LegacySkin})", fileName, installer, legacySkin);
            return installer2.IsValid && installer2.InstallerInfo.Installed;
        }

        private static void DeleteTempInstallFiles(Installer installer)
        {
			Logger.LogError(">>> Entering DeleteTempInstallFiles(installer={Installer})", installer);
            try
            {
                var tempFolder = installer.TempInstallFolder;
                if (!string.IsNullOrEmpty(tempFolder) && Directory.Exists(tempFolder))
                {
                    Globals.DeleteFolderRecursive(tempFolder);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting DeleteTempInstallFiles(installer={Installer})", installer);
        }

        private static void DeleteInstallFile(string installerFile)
        {
			Logger.LogError(">>> Entering DeleteInstallFile(installerFile={InstallerFile})", installerFile);
            try
            {
                if (File.Exists(installerFile))
                {
                    File.SetAttributes(installerFile, FileAttributes.Normal);
                    File.Delete(installerFile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
			Logger.LogWarning("<<< Exiting DeleteInstallFile(installerFile={InstallerFile})", installerFile);
        }

        private static bool? AzureCompact(Installer installer)
        {
			Logger.LogError(">>> Entering AzureCompact(installer={Installer})", installer);
            bool? compact = null;
            string manifestFile = null;
            if (installer.InstallerInfo.ManifestFile != null)
            {
                manifestFile = installer.InstallerInfo.ManifestFile.TempFileName;
				Logger.LogError("AzureCompact(installer={Installer}): manifestFile → {ManifestFile}", installer, manifestFile);
            }

            if (installer.Packages.Count > 0)
            {
                if (installer.Packages[0].Package.PackageType.Equals("CoreLanguagePack", StringComparison.OrdinalIgnoreCase)
                        || installer.Packages[0].Package.PackageType.Equals("ExtensionLanguagePack", StringComparison.OrdinalIgnoreCase))
                {
                    compact = true;
					Logger.LogError("AzureCompact(installer={Installer}): compact → {Compact}", installer, compact);
					Logger.LogDebug("AzureCompact(installer={Installer}): compact → {Compact}", installer, compact);
                }
            }

            if (!IsAzureDatabase())
            {
                compact = true;
            }
            else if (manifestFile != null && File.Exists(manifestFile))
            {
                try
                {
                    var document = new XmlDocument { XmlResolver = null };
                    using (var xmlReader = XmlReader.Create(manifestFile, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        document.Load(xmlReader);
                    }

                    var compactNode = document.SelectSingleNode("/dotnetnuke/packages/package/azureCompatible");
					Logger.LogError("AzureCompact(installer={Installer}): compactNode → {CompactNode}", installer, compactNode);
                    if (compactNode != null && !string.IsNullOrEmpty(compactNode.InnerText))
                    {
                        compact = compactNode.InnerText.Equals("true", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

			Logger.LogWarning("<<< Exiting AzureCompact(installer={Installer})", installer);
            return compact;
        }

        private static bool IsAzureDatabase()
        {
			Logger.LogError(">>> Entering IsAzureDatabase()");
            Logger.LogWarning("<<< Exiting IsAzureDatabase()");
            return PetaPocoHelper.ExecuteScalar<int>(DataProvider.Instance().ConnectionString, CommandType.Text, "SELECT CAST(ServerProperty('EngineEdition') as INT)") == 5;
        }
    }
}
