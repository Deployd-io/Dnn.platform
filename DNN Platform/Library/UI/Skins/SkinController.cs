// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Handles the Business Control Layer for Skins.</summary>
    public partial class SkinController
    {
        private const string GlobalSkinPrefix = "[G]";
        private const string PortalSystemSkinPrefix = "[S]";
        private const string PortalSkinPrefix = "[L]";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinController));
        private static readonly Regex GdirRegex = new Regex("\\[g]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SdirRegex = new Regex("\\[s]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex LdirRegex = new Regex("\\[l]", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly string[] MessageSeparator = ["<br />",];
        private static readonly List<string> LegacySkinExtensions = [".ASCX", ".HTM", ".HTML", ".CSS", ".SWF", ".RESX", ".XAML", ".JS",];

        public static string RootSkin => "Skins";

        public static string RootContainer => "Containers";

        public static int AddSkin(int skinPackageID, string skinSrc)
        {
			Logger.LogDebug(">>> Entering AddSkin(skinPackageID={SkinPackageID},skinSrc={SkinSrc})", skinPackageID, skinSrc);
			Logger.LogDebug("<<< Exiting AddSkin(skinPackageID={SkinPackageID},skinSrc={SkinSrc})", skinPackageID, skinSrc);
            return DataProvider.Instance().AddSkin(skinPackageID, skinSrc);
        }

        /// <summary>Adds the skin package.</summary>
        /// <param name="skinPackage">The skin package.</param>
        /// <returns>The skin package ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int AddSkinPackage(SkinPackageInfo skinPackage)
            => AddSkinPackage(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), skinPackage);
			Logger.LogDebug(">>> Entering AddSkinPackage(skinPackage={SkinPackage})", skinPackage);

        /// <summary>Adds the skin package.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="skinPackage">The skin package.</param>
        /// <returns>The skin package ID.</returns>
        public static int AddSkinPackage(IEventLogger eventLogger, SkinPackageInfo skinPackage)
        {
            eventLogger.AddLog(skinPackage, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.SKINPACKAGE_CREATED);
			Logger.LogDebug("<<< Exiting AddSkinPackage(skinPackage={SkinPackage})", skinPackage);
            return DataProvider.Instance().AddSkinPackage(skinPackage.PackageID, skinPackage.PortalID, skinPackage.SkinName, skinPackage.SkinType, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        /// <summary>Determines whether the given theme folder can be deleted.</summary>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="portalHomeDirMapPath">The physical path to the portal's home directory.</param>
        /// <returns><see langword="true"/> if the folder can be deleted, <see langword="false"/> otherwise.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial bool CanDeleteSkin(string folderPath, string portalHomeDirMapPath)
            => CanDeleteSkin(null, folderPath, portalHomeDirMapPath);
			Logger.LogDebug(">>> Entering CanDeleteSkin(folderPath={FolderPath},portalHomeDirMapPath={PortalHomeDirMapPath})", folderPath, portalHomeDirMapPath);

        /// <summary>Determines whether the given theme folder can be deleted.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="folderPath">The folder path.</param>
        /// <param name="portalHomeDirMapPath">The physical path to the portal's home directory.</param>
        /// <returns><see langword="true"/> if the folder can be deleted, <see langword="false"/> otherwise.</returns>
        public static bool CanDeleteSkin(IHostSettings hostSettings, string folderPath, string portalHomeDirMapPath)
        {
            hostSettings ??= Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("<<< Exiting CanDeleteSkin(folderPath={FolderPath},portalHomeDirMapPath={PortalHomeDirMapPath})", folderPath, portalHomeDirMapPath);

            string skinType;
            string skinFolder;
            bool canDelete = true;
            if (folderPath.IndexOf(Globals.HostMapPath, StringComparison.InvariantCultureIgnoreCase) != -1)
			Logger.LogDebug("CanDeleteSkin(hostSettings={HostSettings},folderPath={FolderPath},portalHomeDirMapPath={PortalHomeDirMapPath}): canDelete → {CanDelete}", hostSettings, folderPath, portalHomeDirMapPath, canDelete);
            {
                skinType = "G";
				Logger.LogDebug("CanDeleteSkin(hostSettings={HostSettings},folderPath={FolderPath},portalHomeDirMapPath={PortalHomeDirMapPath}): skinType → {SkinType}", hostSettings, folderPath, portalHomeDirMapPath, skinType);
                skinFolder = folderPath.ToLowerInvariant().Replace(Globals.HostMapPath.ToLowerInvariant(), string.Empty).Replace(@"\", "/");
            }
			Logger.LogDebug("CanDeleteSkin(hostSettings={HostSettings},folderPath={FolderPath},portalHomeDirMapPath={PortalHomeDirMapPath}): skinFolder → {SkinFolder}", hostSettings, folderPath, portalHomeDirMapPath, skinFolder);
            else if (folderPath.IndexOf(PortalSettings.Current.HomeSystemDirectoryMapPath, StringComparison.InvariantCultureIgnoreCase) != -1)
            {
                skinType = "S";
                skinFolder = folderPath.ToLowerInvariant().Replace(portalHomeDirMapPath.ToLowerInvariant(), string.Empty).Replace(@"\", "/");
            }
            else
            {
                // to be compliant with all versions
                skinType = "L";
                skinFolder = folderPath.ToLowerInvariant().Replace(portalHomeDirMapPath.ToLowerInvariant(), string.Empty).Replace(@"\", "/");
            }

            var portalSettings = PortalController.Instance.GetCurrentSettings();

			Logger.LogDebug("CanDeleteSkin(hostSettings={HostSettings},folderPath={FolderPath},portalHomeDirMapPath={PortalHomeDirMapPath}): portalSettings → {PortalSettings}", hostSettings, folderPath, portalHomeDirMapPath, portalSettings);
            string skin = "[" + skinType.ToLowerInvariant() + "]" + skinFolder.ToLowerInvariant();
            if (skinFolder.ToLowerInvariant().Contains("skins"))
            {
                if (hostSettings.DefaultAdminSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || hostSettings.DefaultPortalSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) ||
                    portalSettings.DefaultAdminSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || portalSettings.DefaultPortalSkin.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase))
                {
                    canDelete = false;
                }
            }
            else
            {
                if (hostSettings.DefaultAdminContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || hostSettings.DefaultPortalContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) ||
                    portalSettings.DefaultAdminContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase) || portalSettings.DefaultPortalContainer.StartsWith(skin, StringComparison.InvariantCultureIgnoreCase))
                {
                    canDelete = false;
                }
            }

            if (canDelete)
            {
                // Check if used for Tabs or Modules
                canDelete = DataProvider.Instance().CanDeleteSkin(skinType, skinFolder);
            }

			Logger.LogDebug(">>> Entering DeleteSkin(skinID={SkinID})", skinID);
			Logger.LogDebug("<<< Exiting DeleteSkin(skinID={SkinID})", skinID);
            return canDelete;
        }

        public static void DeleteSkin(int skinID)
        {
            DataProvider.Instance().DeleteSkin(skinID);
        }

        /// <summary>Deletes the skin package.</summary>
        /// <param name="skinPackage">The skin package.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteSkinPackage(SkinPackageInfo skinPackage)
            => DeleteSkinPackage(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), skinPackage);
			Logger.LogDebug(">>> Entering DeleteSkinPackage(skinPackage={SkinPackage})", skinPackage);

        /// <summary>Deletes the skin package.</summary>
        /// <param name="eventLogger">The event logger.</param>
		Logger.LogDebug("<<< Exiting DeleteSkinPackage(skinPackage={SkinPackage})", skinPackage);
        /// <param name="skinPackage">The skin package.</param>
        public static void DeleteSkinPackage(IEventLogger eventLogger, SkinPackageInfo skinPackage)
        {
            DataProvider.Instance().DeleteSkinPackage(skinPackage.SkinPackageID);
            eventLogger.AddLog(skinPackage, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.SKINPACKAGE_DELETED);
        }

        public static string FormatMessage(string title, string body, int level, bool isError)
        {
			Logger.LogDebug(">>> Entering FormatMessage(title={Title},body={Body},level={Level},isError={IsError})", title, body, level, isError);
            string message = title;
            if (isError)
			Logger.LogDebug("FormatMessage(title={Title},body={Body},level={Level},isError={IsError}): message → {Message}", title, body, level, isError, message);
            {
                message = $"""<span class="NormalRed">{title}</span>""";
            }

            switch (level)
            {
                case -1:
                    message = $"<hr /><br /><strong>{message}</strong>";
                    break;
                case 0:
                    message = $"<br /><br /><strong>{message}</strong>";
                    break;
                case 1:
                    message = $"<br /><strong>{message}</strong>";
                    break;
                default:
                    message = $"<br /><li>{message}</li>";
                    break;
            }

			Logger.LogDebug("<<< Exiting FormatMessage(title={Title},body={Body},level={Level},isError={IsError})", title, body, level, isError);
            return $"{message}: {body}{Environment.NewLine}";
        }

        public static string FormatSkinPath(string skinSrc)
        {
			Logger.LogDebug(">>> Entering FormatSkinPath(skinSrc={SkinSrc})", skinSrc);
            string strSkinSrc = skinSrc;
            if (!string.IsNullOrEmpty(strSkinSrc))
			Logger.LogDebug("FormatSkinPath(skinSrc={SkinSrc}): strSkinSrc → {StrSkinSrc}", skinSrc, strSkinSrc);
            {
                strSkinSrc = strSkinSrc.Substring(0, strSkinSrc.LastIndexOf("/", StringComparison.Ordinal) + 1);
            }

			Logger.LogDebug("<<< Exiting FormatSkinPath(skinSrc={SkinSrc})", skinSrc);
            return strSkinSrc;
        }

        public static string FormatSkinSrc(string skinSrc, PortalSettings portalSettings)
        {
            string strSkinSrc = skinSrc;
            if (!string.IsNullOrEmpty(strSkinSrc))
            {
                switch (strSkinSrc.Substring(0, 3).ToLowerInvariant())
                {
                    case "[g]":
                        strSkinSrc = GdirRegex.Replace(strSkinSrc, Globals.HostPath);
                        break;
                    case "[s]":
                        strSkinSrc = SdirRegex.Replace(strSkinSrc, portalSettings.HomeSystemDirectory);
                        break;
                    case "[l]": // to be compliant with all versions
                        strSkinSrc = LdirRegex.Replace(strSkinSrc, portalSettings.HomeDirectory);
                        break;
                }
            }

            return strSkinSrc;
        }

        public static string GetDefaultAdminContainer()
        {
			Logger.LogDebug(">>> Entering GetDefaultAdminContainer()");
            SkinDefaults defaultContainer = SkinDefaults.GetSkinDefaults(SkinDefaultType.ContainerInfo);
			Logger.LogDebug("GetDefaultAdminContainer(): defaultContainer → {DefaultContainer}", defaultContainer);
			Logger.LogDebug("<<< Exiting GetDefaultAdminContainer()");
            return "[G]" + RootContainer + defaultContainer.Folder + defaultContainer.AdminDefaultName;
        }

        public static string GetDefaultAdminSkin()
        {
			Logger.LogDebug(">>> Entering GetDefaultAdminSkin()");
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
			Logger.LogDebug("GetDefaultAdminSkin(): defaultSkin → {DefaultSkin}", defaultSkin);
			Logger.LogDebug("<<< Exiting GetDefaultAdminSkin()");
            return "[G]" + RootSkin + defaultSkin.Folder + defaultSkin.AdminDefaultName;
        }

        public static string GetDefaultPortalContainer()
        {
			Logger.LogDebug(">>> Entering GetDefaultPortalContainer()");
            SkinDefaults defaultContainer = SkinDefaults.GetSkinDefaults(SkinDefaultType.ContainerInfo);
			Logger.LogDebug("GetDefaultPortalContainer(): defaultContainer → {DefaultContainer}", defaultContainer);
			Logger.LogDebug("<<< Exiting GetDefaultPortalContainer()");
            return "[G]" + RootContainer + defaultContainer.Folder + defaultContainer.DefaultName;
        }

        public static string GetDefaultPortalSkin()
        {
			Logger.LogDebug(">>> Entering GetDefaultPortalSkin()");
            SkinDefaults defaultSkin = SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo);
			Logger.LogDebug("GetDefaultPortalSkin(): defaultSkin → {DefaultSkin}", defaultSkin);
			Logger.LogDebug("<<< Exiting GetDefaultPortalSkin()");
            return "[G]" + RootSkin + defaultSkin.Folder + defaultSkin.DefaultName;
        }

        public static SkinPackageInfo GetSkinByPackageID(int packageID)
        {
			Logger.LogDebug(">>> Entering GetSkinByPackageID(packageID={PackageID})", packageID);
			Logger.LogDebug("<<< Exiting GetSkinByPackageID(packageID={PackageID})", packageID);
            return CBO.FillObject<SkinPackageInfo>(DataProvider.Instance().GetSkinByPackageID(packageID));
        }

        public static SkinPackageInfo GetSkinPackage(int portalId, string skinName, string skinType)
        {
			Logger.LogDebug(">>> Entering GetSkinPackage(portalId={PortalId},skinName={SkinName},skinType={SkinType})", portalId, skinName, skinType);
			Logger.LogDebug("<<< Exiting GetSkinPackage(portalId={PortalId},skinName={SkinName},skinType={SkinType})", portalId, skinName, skinType);
            return CBO.FillObject<SkinPackageInfo>(DataProvider.Instance().GetSkinPackage(portalId, skinName, skinType));
        }

        public static List<KeyValuePair<string, string>> GetSkins(PortalInfo portalInfo, string skinRoot, SkinScope scope)
        {
            var skins = new List<KeyValuePair<string, string>>();
            switch (scope)
            {
                case SkinScope.Host: // load host skins
                    skins = GetHostSkins(skinRoot);
                    break;
                case SkinScope.Site: // load portal skins
                    skins = GetPortalSkins(portalInfo, skinRoot);
                    break;
                case SkinScope.All:
                    skins = GetHostSkins(skinRoot);
                    skins.AddRange(GetPortalSkins(portalInfo, skinRoot));
                    break;
            }

            return skins;
        }

        /// <summary>Determines if a given skin is defined as a global skin.</summary>
        /// <param name="skinSrc">This is the app relative path and filename of the skin to be checked.</param>
        /// <returns>True if the skin is located in the HostPath child directories.</returns>
        /// <remarks>This function performs a quick check to detect the type of skin that is
        /// passed as a parameter.  Using this method abstracts knowledge of the actual location
        /// of skins in the file system.
        /// </remarks>
        public static bool IsGlobalSkin(string skinSrc)
        {
			Logger.LogDebug(">>> Entering IsGlobalSkin(skinSrc={SkinSrc})", skinSrc);
			Logger.LogDebug("<<< Exiting IsGlobalSkin(skinSrc={SkinSrc})", skinSrc);
            return skinSrc.Contains(Globals.HostPath);
        }

        public static void SetSkin(string skinRoot, int portalId, SkinType skinType, string skinSrc)
        {
			Logger.LogDebug(">>> Entering SetSkin(skinRoot={SkinRoot},portalId={PortalId},skinType={SkinType},skinSrc={SkinSrc})", skinRoot, portalId, skinType, skinSrc);
            var selectedCultureCode = LocaleController.Instance.GetCurrentLocale(portalId).Code;
            switch (skinRoot)
			Logger.LogDebug("SetSkin(skinRoot={SkinRoot},portalId={PortalId},skinType={SkinType},skinSrc={SkinSrc}): selectedCultureCode → {SelectedCultureCode}", skinRoot, portalId, skinType, skinSrc, selectedCultureCode);
            {
                case "Skins":
                    if (skinType == SkinType.Admin)
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultAdminSkin", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultAdminSkin", skinSrc, true, selectedCultureCode);
                        }
                    }
                    else
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultPortalSkin", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultPortalSkin", skinSrc, true, selectedCultureCode);
                        }
                    }

                    break;
                case "Containers":
                    if (skinType == SkinType.Admin)
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultAdminContainer", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultAdminContainer", skinSrc, true, selectedCultureCode);
                        }
                    }
                    else
                    {
                        if (portalId == Null.NullInteger)
                        {
                            HostController.Instance.Update("DefaultPortalContainer", skinSrc);
                        }
                        else
                        {
                            PortalController.UpdatePortalSetting(portalId, "DefaultPortalContainer", skinSrc, true, selectedCultureCode);
                        }
                    }

                    break;
            }
			Logger.LogDebug("<<< Exiting SetSkin(skinRoot={SkinRoot},portalId={PortalId},skinType={SkinType},skinSrc={SkinSrc})", skinRoot, portalId, skinType, skinSrc);
        }

        public static void UpdateSkin(int skinID, string skinSrc)
        {
			Logger.LogDebug(">>> Entering UpdateSkin(skinID={SkinID},skinSrc={SkinSrc})", skinID, skinSrc);
            DataProvider.Instance().UpdateSkin(skinID, skinSrc);
			Logger.LogDebug("<<< Exiting UpdateSkin(skinID={SkinID},skinSrc={SkinSrc})", skinID, skinSrc);
        }

        /// <summary>Updates the skin package.</summary>
        /// <param name="skinPackage">The skin package.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void UpdateSkinPackage(SkinPackageInfo skinPackage)
            => UpdateSkinPackage(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), skinPackage);
			Logger.LogDebug(">>> Entering UpdateSkinPackage(skinPackage={SkinPackage})", skinPackage);

        /// <summary>Updates the skin package.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="skinPackage">The skin package.</param>
        public static void UpdateSkinPackage(IEventLogger eventLogger, SkinPackageInfo skinPackage)
        {
            DataProvider.Instance().UpdateSkinPackage(
                skinPackage.SkinPackageID,
                skinPackage.PackageID,
                skinPackage.PortalID,
                skinPackage.SkinName,
                skinPackage.SkinType,
                UserController.Instance.GetCurrentUserInfo().UserID);
				Logger.LogDebug("<<< Exiting UpdateSkinPackage(skinPackage={SkinPackage})", skinPackage);
            eventLogger.AddLog(skinPackage, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogType.SKINPACKAGE_UPDATED);
            foreach (KeyValuePair<int, string> kvp in skinPackage.Skins)
            {
                UpdateSkin(kvp.Key, kvp.Value);
            }
        }

        public static string UploadLegacySkin(string rootPath, string skinRoot, string skinName, Stream inputStream)
        {
			Logger.LogDebug(">>> Entering UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream})", rootPath, skinRoot, skinName, inputStream);
            var objZipInputStream = new ZipArchive(inputStream, ZipArchiveMode.Read);

			Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): objZipInputStream → {ObjZipInputStream}", rootPath, skinRoot, skinName, inputStream, objZipInputStream);
            string strExtension;
            string strFileName;
            FileStream fileStream;
            var arrData = new byte[2048];
            string strMessage = string.Empty;
            var arrSkinFiles = new ArrayList();

			Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): arrSkinFiles → {ArrSkinFiles}", rootPath, skinRoot, skinName, inputStream, arrSkinFiles);
            // Localized Strings
            PortalSettings resourcePortalSettings = Globals.GetPortalSettings();
            string beginMessage = Localization.GetString("BeginZip", resourcePortalSettings);
			Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): resourcePortalSettings → {ResourcePortalSettings}", rootPath, skinRoot, skinName, inputStream, resourcePortalSettings);
            string createDir = Localization.GetString("CreateDir", resourcePortalSettings);
            string writeFile = Localization.GetString("WriteFile", resourcePortalSettings);
            string fileError = Localization.GetString("FileError", resourcePortalSettings);
            string endMessage = Localization.GetString("EndZip", resourcePortalSettings);
            string fileRestricted = Localization.GetString("FileRestricted", resourcePortalSettings);

            strMessage += FormatMessage(beginMessage, skinName, -1, false);

            foreach (var objZipEntry in objZipInputStream.FileEntries())
            {
                // validate file extension
                strExtension = objZipEntry.FullName.Substring(objZipEntry.FullName.LastIndexOf(".", StringComparison.Ordinal) + 1);
				Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): strExtension → {StrExtension}", rootPath, skinRoot, skinName, inputStream, strExtension);
                if (Host.AllowedExtensionWhitelist.IsAllowedExtension(strExtension, LegacySkinExtensions))
                {
                    // process embedded zip files
                    if (objZipEntry.FullName.Equals(RootSkin.ToLowerInvariant() + ".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        using var objMemoryStream = new MemoryStream();
                        objZipEntry.Open().CopyToStream(objMemoryStream, 25000);
						Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): objMemoryStream → {ObjMemoryStream}", rootPath, skinRoot, skinName, inputStream, objMemoryStream);
                        objMemoryStream.Seek(0, SeekOrigin.Begin);
                        strMessage += UploadLegacySkin(rootPath, RootSkin, skinName, objMemoryStream);
                    }
                    else if (objZipEntry.FullName.Equals(RootContainer.ToLowerInvariant() + ".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        using var objMemoryStream = new MemoryStream();
                        objZipEntry.Open().CopyToStream(objMemoryStream, 25000);
                        objMemoryStream.Seek(0, SeekOrigin.Begin);
                        strMessage += UploadLegacySkin(rootPath, RootContainer, skinName, objMemoryStream);
                    }
                    else
                    {
                        strFileName = $@"{rootPath}{skinRoot}\{skinName}\{objZipEntry.FullName}";
						Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): strFileName → {StrFileName}", rootPath, skinRoot, skinName, inputStream, strFileName);

                        // create the directory if it does not exist
                        if (!Directory.Exists(Path.GetDirectoryName(strFileName)))
                        {
                            strMessage += FormatMessage(createDir, Path.GetDirectoryName(strFileName), 2, false);
                            Directory.CreateDirectory(Path.GetDirectoryName(strFileName));
                        }

                        // remove the old file
                        if (File.Exists(strFileName))
                        {
                            File.SetAttributes(strFileName, FileAttributes.Normal);
                            File.Delete(strFileName);
                        }

                        // create the new file
                        fileStream = File.Create(strFileName);
						Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): fileStream → {FileStream}", rootPath, skinRoot, skinName, inputStream, fileStream);

                        // unzip the file
                        strMessage += FormatMessage(writeFile, Path.GetFileName(strFileName), 2, false);
                        objZipEntry.Open().CopyToStream(fileStream, 25000);
                        fileStream.Close();

                        // save the skin file
                        switch (Path.GetExtension(strFileName))
                        {
                            case ".htm":
                            case ".html":
                            case ".ascx":
                            case ".css":
                                if (strFileName.IndexOf(Globals.glbAboutPage, StringComparison.InvariantCultureIgnoreCase) < 0)
                                {
                                    arrSkinFiles.Add(strFileName);
                                }

                                break;
                        }

                        break;
                    }
                }
                else
                {
                    strMessage += string.Format(CultureInfo.CurrentCulture, fileRestricted, objZipEntry.FullName, Host.AllowedExtensionWhitelist.ToStorageString(), ",", ", *.").Replace("2", "true");
                }
            }

            strMessage += FormatMessage(endMessage, skinName + ".zip", 1, false);
            objZipInputStream.Dispose();

            // process the list of skin files
            var newSkin = new SkinFileProcessor(rootPath, skinRoot, skinName);
            strMessage += newSkin.ProcessList(arrSkinFiles, SkinParser.Portable);
			Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): newSkin → {NewSkin}", rootPath, skinRoot, skinName, inputStream, newSkin);

            // log installation event
            try
            {
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT), };
                log.LogProperties.Add(new LogDetailInfo("Install Skin:", skinName));
				Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): log → {Log}", rootPath, skinRoot, skinName, inputStream, log);
                Array arrMessage = strMessage.Split(MessageSeparator, StringSplitOptions.None);
                foreach (string strRow in arrMessage)
                {
				Logger.LogDebug("UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream}): arrMessage → {ArrMessage}", rootPath, skinRoot, skinName, inputStream, arrMessage);
                    log.LogProperties.Add(new LogDetailInfo("Info:", HtmlUtils.StripTags(strRow, true)));
                }

                LogController.Instance.AddLog(log);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

			Logger.LogDebug("<<< Exiting UploadLegacySkin(rootPath={RootPath},skinRoot={SkinRoot},skinName={SkinName},inputStream={InputStream})", rootPath, skinRoot, skinName, inputStream);
            return strMessage;
        }

        private static void AddSkinFiles(List<KeyValuePair<string, string>> skins, string skinRoot, string skinFolder, string skinPrefix)
        {
            foreach (string skinFile in Directory.GetFiles(skinFolder, "*.ascx"))
            {
                string folder = skinFolder.Substring(skinFolder.LastIndexOf(@"\", StringComparison.Ordinal) + 1);

                string key = (skinPrefix is PortalSystemSkinPrefix or PortalSkinPrefix ? "Site: " : "Host: ")
                                + FormatSkinName(folder, Path.GetFileNameWithoutExtension(skinFile));
                string value = $"{skinPrefix}{skinRoot}/{folder}/{Path.GetFileName(skinFile)}";
                skins.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        private static List<KeyValuePair<string, string>> GetHostSkins(string skinRoot)
        {
            var skins = new List<KeyValuePair<string, string>>();

            string root = Globals.HostMapPath + skinRoot;
            if (Directory.Exists(root))
            {
                foreach (string skinFolder in Directory.GetDirectories(root))
                {
                    if (!skinFolder.EndsWith(Globals.glbHostSkinFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        AddSkinFiles(skins, skinRoot, skinFolder, GlobalSkinPrefix);
                    }
                }
            }

            return skins;
        }

        private static List<KeyValuePair<string, string>> GetPortalSkins(PortalInfo portalInfo, string skinRoot)
        {
            var skins = new List<KeyValuePair<string, string>>();

            if (portalInfo != null)
            {
                ProcessSkinsFolder(skins, portalInfo.HomeSystemDirectoryMapPath + skinRoot, skinRoot, PortalSystemSkinPrefix);
                ProcessSkinsFolder(skins, portalInfo.HomeDirectoryMapPath + skinRoot, skinRoot, PortalSkinPrefix); // to be compliant with all versions
            }

            return skins;
        }

        private static void ProcessSkinsFolder(List<KeyValuePair<string, string>> skins, string skinsFolder, string skinRoot, string skinPrefix)
        {
            if (Directory.Exists(skinsFolder))
            {
                foreach (string skinFolder in Directory.GetDirectories(skinsFolder))
                {
                    AddSkinFiles(skins, skinRoot, skinFolder, skinPrefix);
                }
            }
        }

        /// <summary>format skin name.</summary>
        /// <param name="skinFolder">The Folder Name.</param>
        /// <param name="skinFile">The File Name without extension.</param>
        private static string FormatSkinName(string skinFolder, string skinFile)
        {
			Logger.LogDebug(">>> Entering FormatSkinName(skinFolder={SkinFolder},skinFile={SkinFile})", skinFolder, skinFile);
            if (skinFolder.Equals("_default", StringComparison.OrdinalIgnoreCase))
            {
                // host folder
                return skinFile;
            }

            // portal folder
            switch (skinFile.ToLowerInvariant())
            {
                case "skin":
                case "container":
                case "default":
                    return skinFolder;
                default:
					Logger.LogDebug("<<< Exiting FormatSkinName(skinFolder={SkinFolder},skinFile={SkinFile})", skinFolder, skinFile);
                    return skinFolder + " - " + skinFile;
            }
        }
    }
}
