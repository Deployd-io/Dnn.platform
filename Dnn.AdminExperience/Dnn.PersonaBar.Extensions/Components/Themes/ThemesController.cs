// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Themes.Components;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

using Dnn.PersonaBar.Themes.Components.DTO;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Skins;

using Microsoft.Extensions.DependencyInjection;

using Image = System.Drawing.Image;

/// <summary>The default <see cref="IThemesController"/> implementation.</summary>
public class ThemesController : ServiceLocator<IThemesController, ThemesController>, IThemesController
{
    /// <summary>The image extensions.</summary>
    internal static readonly IList<string> ImageExtensions = [".jpg", ".png", ".jpeg",];

    /// <summary>The default theme layout names.</summary>
    internal static readonly IList<string> DefaultLayoutNames = ["Default", "2-Col", "Home", "Index", "Main",];

    /// <summary>The default container names.</summary>
    internal static readonly IList<string> DefaultContainerNames = ["Title-h2", "NoTitle", "Main", "Default",];
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));
    private static readonly object ThreadLocker = new object();

    private readonly IHostSettings hostSettings = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
    private readonly IApplicationStatusInfo appStatus = Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();

    /// <inheritdoc />
    public IList<ThemeInfo> GetLayouts(PortalSettings portalSettings, ThemeLevel level)
    {
		Logger.LogDebug(">>> Entering GetLayouts(portalSettings={PortalSettings},level={Level})", portalSettings, level);
        var themes = new List<ThemeInfo>();
        if ((level & ThemeLevel.Site) == ThemeLevel.Site)
		Logger.LogDebug("GetLayouts(portalSettings={PortalSettings},level={Level}): themes → {Themes}", portalSettings, level, themes);
        {
            themes.AddRange(this.GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootSkin)));
        }

        if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
        {
            themes.AddRange(this.GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootSkin)));
        }

        if ((level & ThemeLevel.Global) == ThemeLevel.Global)
        {
            themes.AddRange(this.GetThemes(ThemeType.Skin, Path.Combine(Globals.HostMapPath, SkinController.RootSkin)));
        }

		Logger.LogDebug("<<< Exiting GetLayouts(portalSettings={PortalSettings},level={Level})", portalSettings, level);
        return themes;
    }

    /// <inheritdoc />
    public IList<ThemeInfo> GetContainers(PortalSettings portalSettings, ThemeLevel level)
    {
		Logger.LogDebug(">>> Entering GetContainers(portalSettings={PortalSettings},level={Level})", portalSettings, level);
        var themes = new List<ThemeInfo>();
        if ((level & ThemeLevel.Site) == ThemeLevel.Site)
		Logger.LogDebug("GetContainers(portalSettings={PortalSettings},level={Level}): themes → {Themes}", portalSettings, level, themes);
        {
            themes.AddRange(this.GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootContainer)));
        }

        if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
        {
            themes.AddRange(this.GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootContainer)));
        }

        if ((level & ThemeLevel.Global) == ThemeLevel.Global)
        {
            themes.AddRange(this.GetThemes(ThemeType.Container, Path.Combine(Globals.HostMapPath, SkinController.RootContainer)));
        }

		Logger.LogDebug("<<< Exiting GetContainers(portalSettings={PortalSettings},level={Level})", portalSettings, level);
        return themes;
    }

    /// <inheritdoc />
    public IList<ThemeFileInfo> GetThemeFiles(PortalSettings portalSettings, ThemeInfo theme)
    {
		Logger.LogDebug(">>> Entering GetThemeFiles(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
        var themePath = Path.Combine(this.appStatus.ApplicationMapPath, theme.Path);
        var themeFiles = new List<ThemeFileInfo>();
		Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFiles → {ThemeFiles}", portalSettings, theme, themeFiles);
		Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themePath → {ThemePath}", portalSettings, theme, themePath);

        if (Directory.Exists(themePath))
        {
            var fallbackSkin = theme.Type == ThemeType.Skin ? this.IsFallbackSkin(themePath) : this.IsFallbackContainer(themePath);
			Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): fallbackSkin → {FallbackSkin}", portalSettings, theme, fallbackSkin);

            var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";
			Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): strSkinType → {StrSkinType}", portalSettings, theme, strSkinType);

            var canDeleteSkin = SkinController.CanDeleteSkin(this.hostSettings, themePath, portalSettings.HomeDirectoryMapPath);
			Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): canDeleteSkin → {CanDeleteSkin}", portalSettings, theme, canDeleteSkin);
            var arrFiles = Directory.GetFiles(themePath, "*.ascx");
			Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): arrFiles → {ArrFiles}", portalSettings, theme, arrFiles);

            foreach (var strFile in arrFiles)
            {
                var file = strFile.ToLowerInvariant();
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): file → {File}", portalSettings, theme, file);

                var themeFile = new ThemeFileInfo();
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile → {ThemeFile}", portalSettings, theme, themeFile);
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile.ThemeName → {Arg2}", portalSettings, theme, themeFile.ThemeName);
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile.Type → {Arg2}", portalSettings, theme, themeFile.Type);
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile.Level → {Arg2}", portalSettings, theme, themeFile.Level);
                themeFile.ThemeName = theme.PackageName;
                themeFile.Type = theme.Type;
                themeFile.Level = theme.Level;

                var imagePath = string.Empty;
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): imagePath → {ImagePath}", portalSettings, theme, imagePath);
                foreach (var ext in ImageExtensions)
                {
                    var path = Path.ChangeExtension(file, ext);
					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): path → {Path}", portalSettings, theme, path);
                    if (File.Exists(path))
                    {
                        imagePath = path;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(imagePath))
                {
                    themeFile.Thumbnail = this.CreateThumbnail(imagePath);
                }

                themeFile.Name = Path.GetFileNameWithoutExtension(file);
                themeFile.Path = FormatThemePath(portalSettings, themePath, Path.GetFileName(strFile), theme.Type);
                themeFile.CanDelete = (UserController.Instance.GetCurrentUserInfo().IsSuperUser || strSkinType == "L") && !fallbackSkin && canDeleteSkin;

                themeFiles.Add(themeFile);
            }
        }

		Logger.LogDebug("<<< Exiting GetThemeFiles(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
        return themeFiles;
    }

    /// <inheritdoc />
    public ThemeFileInfo GetThemeFile(PortalSettings portalSettings, string filePath, ThemeType type)
    {
		Logger.LogDebug(">>> Entering GetThemeFile(portalSettings={PortalSettings},filePath={FilePath},type={Type})", portalSettings, filePath, type);
        var themeName = SkinController.FormatSkinPath(filePath)
            .Substring(filePath.IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1)
            .Replace("/", string.Empty);
        var themeLevel = GetThemeLevel(filePath);
		Logger.LogDebug("GetThemeFile(portalSettings={PortalSettings},filePath={FilePath},type={Type}): themeLevel → {ThemeLevel}", portalSettings, filePath, type, themeLevel);

        var themeInfo = (type == ThemeType.Skin ? this.GetLayouts(portalSettings, ThemeLevel.All)
                : this.GetContainers(portalSettings, ThemeLevel.All))
            .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == themeLevel);

        if (themeInfo != null)
        {
		Logger.LogDebug("<<< Exiting GetThemeFile(portalSettings={PortalSettings},filePath={FilePath},type={Type})", portalSettings, filePath, type);
            return this.GetThemeFiles(portalSettings, themeInfo).FirstOrDefault(f => (f.Path + ".ascx").Equals(filePath, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }

    /// <inheritdoc />
    public void ApplyTheme(int portalId, ThemeFileInfo themeFile, ApplyThemeScope scope)
    {
		Logger.LogDebug(">>> Entering ApplyTheme(portalId={PortalId},themeFile={ThemeFile},scope={Scope})", portalId, themeFile, scope);
        var skinPath = $"{themeFile.Path}.ascx";

		Logger.LogDebug("ApplyTheme(portalId={PortalId},themeFile={ThemeFile},scope={Scope}): skinPath → {SkinPath}", portalId, themeFile, scope, skinPath);
        switch (themeFile.Type)
        {
            case ThemeType.Container:
                if ((scope & ApplyThemeScope.Site) == ApplyThemeScope.Site)
                {
                    SkinController.SetSkin(SkinController.RootContainer, portalId, SkinType.Portal, skinPath);
                }

                if ((scope & ApplyThemeScope.Edit) == ApplyThemeScope.Edit)
                {
                    SkinController.SetSkin(SkinController.RootContainer, portalId, SkinType.Admin, skinPath);
                }

                break;
            case ThemeType.Skin:
                if ((scope & ApplyThemeScope.Site) == ApplyThemeScope.Site)
                {
                    SkinController.SetSkin(SkinController.RootSkin, portalId, SkinType.Portal, skinPath);
                }

                if ((scope & ApplyThemeScope.Edit) == ApplyThemeScope.Edit)
                {
                    SkinController.SetSkin(SkinController.RootSkin, portalId, SkinType.Admin, skinPath);
                }

                DataCache.ClearPortalCache(portalId, true);
                break;
        }
		Logger.LogDebug("<<< Exiting ApplyTheme(portalId={PortalId},themeFile={ThemeFile},scope={Scope})", portalId, themeFile, scope);
    }

    /// <inheritdoc />
    public void ApplyDefaultTheme(PortalSettings portalSettings, string themeName, ThemeLevel level)
    {
		Logger.LogDebug(">>> Entering ApplyDefaultTheme(portalSettings={PortalSettings},themeName={ThemeName},level={Level})", portalSettings, themeName, level);
        var skin = this.GetLayouts(portalSettings, ThemeLevel.All)
            .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);
        if (skin != null)
        {
            var skinFile = this.GetThemeFiles(portalSettings, skin).FirstOrDefault(t => t.Path == skin.DefaultThemeFile);
			Logger.LogDebug("ApplyDefaultTheme(portalSettings={PortalSettings},themeName={ThemeName},level={Level}): skinFile → {SkinFile}", portalSettings, themeName, level, skinFile);
            if (skinFile != null)
            {
                this.ApplyTheme(portalSettings.PortalId, skinFile, ApplyThemeScope.Site | ApplyThemeScope.Edit);
            }
        }

        var container = this.GetContainers(portalSettings, ThemeLevel.All)
            .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);
        if (container != null)
        {
            var containerFile = this.GetThemeFiles(portalSettings, container).FirstOrDefault(t => t.Path == container.DefaultThemeFile);
			Logger.LogDebug("ApplyDefaultTheme(portalSettings={PortalSettings},themeName={ThemeName},level={Level}): containerFile → {ContainerFile}", portalSettings, themeName, level, containerFile);
            if (containerFile != null)
            {
                this.ApplyTheme(portalSettings.PortalId, containerFile, ApplyThemeScope.Site | ApplyThemeScope.Edit);
            }
        }
		Logger.LogDebug("<<< Exiting ApplyDefaultTheme(portalSettings={PortalSettings},themeName={ThemeName},level={Level})", portalSettings, themeName, level);
    }

    /// <inheritdoc />
    public void DeleteTheme(PortalSettings portalSettings, ThemeFileInfo themeFile)
    {
		Logger.LogDebug(">>> Entering DeleteTheme(portalSettings={PortalSettings},themeFile={ThemeFile})", portalSettings, themeFile);
        var themePath = SkinController.FormatSkinSrc(themeFile.Path, portalSettings);
        var user = UserController.Instance.GetCurrentUserInfo();
		Logger.LogDebug("DeleteTheme(portalSettings={PortalSettings},themeFile={ThemeFile}): user → {User}", portalSettings, themeFile, user);
		Logger.LogDebug("DeleteTheme(portalSettings={PortalSettings},themeFile={ThemeFile}): themePath → {ThemePath}", portalSettings, themeFile, themePath);

        if (!user.IsSuperUser && themePath.IndexOf("\\portals\\_default\\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
        {
			Logger.LogWarning("Throwing SecurityException from DeleteTheme(portalSettings={PortalSettings},themeFile={ThemeFile})", portalSettings, themeFile);
            throw new SecurityException("NoPermission");
        }

        File.Delete(Path.Combine(this.appStatus.ApplicationMapPath, themePath));
        DataCache.ClearPortalCache(portalSettings.PortalId, true);
		Logger.LogDebug("<<< Exiting DeleteTheme(portalSettings={PortalSettings},themeFile={ThemeFile})", portalSettings, themeFile);
    }

    /// <inheritdoc />
    public void DeleteThemePackage(PortalSettings portalSettings, ThemeInfo theme)
    {
		Logger.LogDebug(">>> Entering DeleteThemePackage(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
        var themePath = Path.Combine(this.appStatus.ApplicationMapPath, theme.Path);
        var user = UserController.Instance.GetCurrentUserInfo();
		Logger.LogDebug("DeleteThemePackage(portalSettings={PortalSettings},theme={Theme}): user → {User}", portalSettings, theme, user);
		Logger.LogDebug("DeleteThemePackage(portalSettings={PortalSettings},theme={Theme}): themePath → {ThemePath}", portalSettings, theme, themePath);

        if (!user.IsSuperUser && themePath.IndexOf(@"\portals\_default\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
        {
			Logger.LogWarning("Throwing SecurityException from DeleteThemePackage(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
            throw new SecurityException("NoPermission");
        }

        if (theme.Type == ThemeType.Skin)
        {
            var skinPackage = SkinController.GetSkinPackage(portalSettings.PortalId, theme.PackageName, "Skin");
			Logger.LogDebug("DeleteThemePackage(portalSettings={PortalSettings},theme={Theme}): skinPackage → {SkinPackage}", portalSettings, theme, skinPackage);
            if (skinPackage != null)
            {
				Logger.LogWarning("Throwing InvalidOperationException from DeleteThemePackage(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
                throw new InvalidOperationException("UsePackageUninstall");
            }

            if (Directory.Exists(themePath))
            {
                Globals.DeleteFolderRecursive(themePath);
            }

            if (Directory.Exists(themePath.Replace($@"\{SkinController.RootSkin.ToLowerInvariant()}\", $@"\{SkinController.RootContainer}\")))
            {
                Globals.DeleteFolderRecursive(themePath.Replace($@"\{SkinController.RootSkin.ToLowerInvariant()}\", $@"\{SkinController.RootContainer}\"));
            }
        }
        else if (theme.Type == ThemeType.Container)
        {
            var skinPackage = SkinController.GetSkinPackage(portalSettings.PortalId, theme.PackageName, "Container");
            if (skinPackage != null)
            {
                throw new InvalidOperationException("UsePackageUninstall");
            }

            if (Directory.Exists(themePath))
            {
                Globals.DeleteFolderRecursive(themePath);
            }
        }
		Logger.LogDebug("<<< Exiting DeleteThemePackage(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
    }

    /// <inheritdoc />
    public void UpdateTheme(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
    {
		Logger.LogDebug(">>> Entering UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme})", portalSettings, updateTheme);
        var themePath = SkinController.FormatSkinSrc(updateTheme.Path + ".ascx", portalSettings);
        themePath = Path.Combine(this.appStatus.ApplicationMapPath, themePath.TrimStart('/'));
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): themePath → {ThemePath}", portalSettings, updateTheme, themePath);

        var objStreamReader = File.OpenText(themePath);
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objStreamReader → {ObjStreamReader}", portalSettings, updateTheme, objStreamReader);
        var strSkin = objStreamReader.ReadToEnd();
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strSkin → {StrSkin}", portalSettings, updateTheme, strSkin);
        objStreamReader.Close();
        var strTag = $"<dnn:{updateTheme.Token} runat=\"server\" id=\"dnn{updateTheme.Token}\"";
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strTag → {StrTag}", portalSettings, updateTheme, strTag);
        var intOpenTag = strSkin.IndexOf(strTag, StringComparison.OrdinalIgnoreCase);
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intOpenTag → {IntOpenTag}", portalSettings, updateTheme, intOpenTag);
        if (intOpenTag == -1)
        {
            return;
        }

        var intCloseTag = strSkin.IndexOf(" />", intOpenTag, StringComparison.Ordinal);
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intCloseTag → {IntCloseTag}", portalSettings, updateTheme, intCloseTag);
        var strAttribute = updateTheme.Setting;
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strAttribute → {StrAttribute}", portalSettings, updateTheme, strAttribute);
        var intStartAttribute = strSkin.IndexOf(strAttribute, intOpenTag, StringComparison.OrdinalIgnoreCase);
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intStartAttribute → {IntStartAttribute}", portalSettings, updateTheme, intStartAttribute);
        var strValue = updateTheme.Value;
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strValue → {StrValue}", portalSettings, updateTheme, strValue);
        if (intStartAttribute != -1 && intStartAttribute < intCloseTag)
        {
            // remove attribute
            var intEndAttribute = strSkin.IndexOf("\" ", intStartAttribute, StringComparison.Ordinal);
			Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intEndAttribute → {IntEndAttribute}", portalSettings, updateTheme, intEndAttribute);
            strSkin = strSkin.Substring(0, intStartAttribute) + strSkin.Substring(intEndAttribute + 2);
        }

        // add attribute
        strAttribute = HttpUtility.HtmlAttributeEncode(strAttribute);
        strValue = HttpUtility.HtmlAttributeEncode(strValue);
        strSkin = strSkin.Insert(intOpenTag + strTag.Length, $" {strAttribute}=\"{strValue}\"");

        File.SetAttributes(themePath, FileAttributes.Normal);
        var objStream = File.CreateText(themePath);
		Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objStream → {ObjStream}", portalSettings, updateTheme, objStream);
        objStream.WriteLine(strSkin);
        objStream.Close();

        UpdateManifest(portalSettings, updateTheme);
    }

    /// <inheritdoc />
    public void ParseTheme(PortalSettings portalSettings, ThemeInfo theme, ParseType parseType)
    {
		Logger.LogDebug(">>> Entering ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType})", portalSettings, theme, parseType);
        var strRootPath = theme.Level switch
        {
            ThemeLevel.Global => // global
				Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): ThemeLevel.Global → {Arg3}", portalSettings, theme, parseType, ThemeLevel.Global);
                Globals.HostMapPath,
            ThemeLevel.Site => // local
				Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): ThemeLevel.Site → {Arg3}", portalSettings, theme, parseType, ThemeLevel.Site);
                portalSettings.HomeDirectoryMapPath,
            _ => Null.NullString,
        };

        var objSkinFiles = new SkinFileProcessor(strRootPath, theme.Type == ThemeType.Container ? SkinController.RootContainer : SkinController.RootSkin, theme.PackageName);
		Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): objSkinFiles → {ObjSkinFiles}", portalSettings, theme, parseType, objSkinFiles);
        var arrSkinFiles = new ArrayList();
		Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): arrSkinFiles → {ArrSkinFiles}", portalSettings, theme, parseType, arrSkinFiles);

        var strFolder = Path.Combine(this.appStatus.ApplicationMapPath, theme.Path);
		Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): strFolder → {StrFolder}", portalSettings, theme, parseType, strFolder);
        if (Directory.Exists(strFolder))
        {
            var arrFiles = Directory.GetFiles(strFolder);
			Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): arrFiles → {ArrFiles}", portalSettings, theme, parseType, arrFiles);
            foreach (var strFile in arrFiles)
            {
                switch (Path.GetExtension(strFile))
                {
                    case ".htm":
                    case ".html":
                    case ".css":
                        if (strFile.IndexOf(Globals.glbAboutPage, StringComparison.CurrentCultureIgnoreCase) < 0)
                        {
                            arrSkinFiles.Add(strFile);
                        }

                        break;
                    case ".ascx":
                        if (!File.Exists(strFile.Replace(".ascx", ".htm")) && !File.Exists(strFile.Replace(".ascx", ".html")))
                        {
                            arrSkinFiles.Add(strFile);
                        }

                        break;
                }
            }
        }

        switch (parseType)
        {
            case ParseType.Localized: // localized
                objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Localized);
                break;
            case ParseType.Portable: // portable
                objSkinFiles.ProcessList(arrSkinFiles, SkinParser.Portable);
                break;
        }
		Logger.LogDebug("<<< Exiting ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType})", portalSettings, theme, parseType);
    }

    /// <summary>Gets the <see cref="ThemeLevel"/> for the given <paramref name="themeFilePath"/>.</summary>
    /// <param name="themeFilePath">The file path to the theme.</param>
    /// <returns>The <see cref="ThemeLevel"/> value.</returns>
    internal static ThemeLevel GetThemeLevel(string themeFilePath)
    {
		Logger.LogDebug(">>> Entering GetThemeLevel(themeFilePath={ThemeFilePath})", themeFilePath);
        themeFilePath = themeFilePath.Replace(@"\", "/");
        if (!string.IsNullOrEmpty(Globals.ApplicationPath)
            && !themeFilePath.StartsWith("[", StringComparison.Ordinal)
            && !themeFilePath.StartsWith(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase))
        {
			Logger.LogDebug("GetThemeLevel(themeFilePath={ThemeFilePath}): themeFilePath → {ThemeFilePath1}", themeFilePath, themeFilePath);
            var needSlash = !Globals.ApplicationPath.EndsWith("/", StringComparison.Ordinal) && !themeFilePath.StartsWith("/", StringComparison.Ordinal);
			Logger.LogDebug("GetThemeLevel(themeFilePath={ThemeFilePath}): needSlash → {NeedSlash}", themeFilePath, needSlash);
            themeFilePath = $"{Globals.ApplicationPath}{(needSlash ? "/" : string.Empty)}{themeFilePath}";
        }

        if (themeFilePath.IndexOf(Globals.HostPath.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger
            || themeFilePath.StartsWith("[G]", StringComparison.OrdinalIgnoreCase))
        {
            return ThemeLevel.Global;
        }

        if ((PortalSettings.Current != null &&
             themeFilePath.IndexOf(PortalSettings.Current.HomeSystemDirectory.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger)
            || themeFilePath.StartsWith("[S]", StringComparison.OrdinalIgnoreCase))
        {
		Logger.LogDebug("<<< Exiting GetThemeLevel(themeFilePath={ThemeFilePath})", themeFilePath);
            return ThemeLevel.SiteSystem;
        }

        return ThemeLevel.Site;
    }

    /// <summary>Creates a thumbnail version of the theme image.</summary>
    /// <param name="strImage">The path to the theme image.</param>
    /// <returns>The path to the thumbnail image.</returns>
    internal string CreateThumbnail(string strImage)
    {
		Logger.LogDebug(">>> Entering CreateThumbnail(strImage={StrImage})", strImage);
        var imageFileName = Path.GetFileName(strImage);
        if (string.IsNullOrEmpty(imageFileName) || imageFileName.StartsWith("thumbnail_", StringComparison.OrdinalIgnoreCase))
		Logger.LogDebug("CreateThumbnail(strImage={StrImage}): imageFileName → {ImageFileName}", strImage, imageFileName);
        {
            strImage = strImage.Substring(this.appStatus.ApplicationMapPath.Length);
			Logger.LogDebug("CreateThumbnail(strImage={StrImage}): strImage → {StrImage1}", strImage, strImage);
            strImage = strImage.Replace(@"\", "/");
            return strImage;
        }

        var strThumbnail = strImage.Replace(Path.GetFileName(strImage), "thumbnail_" + imageFileName);
		Logger.LogDebug("CreateThumbnail(strImage={StrImage}): strThumbnail → {StrThumbnail}", strImage, strThumbnail);

        if (NeedCreateThumbnail(strThumbnail, strImage))
        {
            lock (ThreadLocker)
            {
                if (NeedCreateThumbnail(strThumbnail, strImage))
                {
                    const int intSize = 150; // size of the thumbnail
                    try
                    {
                        var objImage = Image.FromFile(strImage);
						Logger.LogDebug("CreateThumbnail(strImage={StrImage}): objImage → {ObjImage}", strImage, objImage);

                        // scale the image to prevent distortion
                        int intHeight;
                        int intWidth;
                        double dblScale;
                        if (objImage.Height > objImage.Width)
                        {
                            // The height was larger, so scale the width
                            dblScale = (double)intSize / objImage.Height;
                            intHeight = intSize;
                            intWidth = Convert.ToInt32(objImage.Width * dblScale);
                        }
                        else
						Logger.LogDebug("CreateThumbnail(strImage={StrImage}): intHeight → {IntHeight}", strImage, intHeight);
						Logger.LogDebug("CreateThumbnail(strImage={StrImage}): intWidth → {IntWidth}", strImage, intWidth);
                        {
                            // The width was larger, so scale the height
                            dblScale = (double)intSize / objImage.Width;
                            intWidth = intSize;
                            intHeight = Convert.ToInt32(objImage.Height * dblScale);
                        }

                        // create the thumbnail image
                        var objThumbnail = objImage.GetThumbnailImage(intWidth, intHeight, null, IntPtr.Zero);
						Logger.LogDebug("CreateThumbnail(strImage={StrImage}): objThumbnail → {ObjThumbnail}", strImage, objThumbnail);

                        // delete the old file ( if it exists )
                        if (File.Exists(strThumbnail))
                        {
                            File.Delete(strThumbnail);
                        }

                        // save the thumbnail image
                        objThumbnail.Save(strThumbnail, objImage.RawFormat);

                        // set the file attributes
                        File.SetAttributes(strThumbnail, FileAttributes.Normal);
                        File.SetLastWriteTime(strThumbnail, File.GetLastWriteTime(strImage));

                        // tidy up
                        objImage.Dispose();
                        objThumbnail.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // problem creating thumbnail
                        Logger.Error(ex);
                    }
                }
            }
        }

        strThumbnail = strThumbnail.Substring(this.appStatus.ApplicationMapPath.Length);
        strThumbnail = strThumbnail.Replace(@"\", "/");

        // return thumbnail filename
		Logger.LogDebug("<<< Exiting CreateThumbnail(strImage={StrImage})", strImage);
        return strThumbnail;
    }

    /// <inheritdoc/>
    protected override Func<IThemesController> GetFactory()
    {
		Logger.LogDebug(">>> Entering GetFactory()");
		Logger.LogDebug("<<< Exiting GetFactory()");
        return () => new ThemesController();
    }

    private static bool NeedCreateThumbnail(string thumbnailPath, string imagePath)
    {
		Logger.LogDebug(">>> Entering NeedCreateThumbnail(thumbnailPath={ThumbnailPath},imagePath={ImagePath})", thumbnailPath, imagePath);
		Logger.LogDebug("<<< Exiting NeedCreateThumbnail(thumbnailPath={ThumbnailPath},imagePath={ImagePath})", thumbnailPath, imagePath);
        return !File.Exists(thumbnailPath) || File.GetLastWriteTime(thumbnailPath) != File.GetLastWriteTime(imagePath);
    }

    private static string FormatThemePath(PortalSettings portalSettings, string themePath, string fileName, ThemeType type)
    {
		Logger.LogDebug(">>> Entering FormatThemePath(portalSettings={PortalSettings},themePath={ThemePath},fileName={FileName},type={Type})", portalSettings, themePath, fileName, type);
        var filePath = Path.Combine(themePath, fileName);
        var lowercasePath = filePath.ToLowerInvariant();
		Logger.LogDebug("FormatThemePath(portalSettings={PortalSettings},themePath={ThemePath},fileName={FileName},type={Type}): lowercasePath → {LowercasePath}", portalSettings, themePath, fileName, type, lowercasePath);
		Logger.LogDebug("FormatThemePath(portalSettings={PortalSettings},themePath={ThemePath},fileName={FileName},type={Type}): filePath → {FilePath}", portalSettings, themePath, fileName, type, filePath);
        var strRootSkin = type == ThemeType.Skin ? SkinController.RootSkin : SkinController.RootContainer;

        var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";
		Logger.LogDebug("FormatThemePath(portalSettings={PortalSettings},themePath={ThemePath},fileName={FileName},type={Type}): strSkinType → {StrSkinType}", portalSettings, themePath, fileName, type, strSkinType);
        if (themePath.IndexOf(portalSettings.HomeSystemDirectoryMapPath, StringComparison.OrdinalIgnoreCase) > -1)
        {
            strSkinType = "S";
        }

        var strUrl = lowercasePath.Substring(filePath.IndexOf($@"\{strRootSkin}\", StringComparison.OrdinalIgnoreCase))
            .Replace(".ascx", string.Empty)
            .Replace(@"\", "/")
            .TrimStart('/');

        return $"[{strSkinType}]{strUrl}";
    }

    private static void UpdateManifest(PortalSettings portalSettings, UpdateThemeInfo updateTheme)
    {
		Logger.LogDebug(">>> Entering UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme})", portalSettings, updateTheme);
        var themePath = SkinController.FormatSkinSrc(updateTheme.Path, portalSettings);
        if (!File.Exists(themePath.Replace(".ascx", ".htm")))
		Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): themePath → {ThemePath}", portalSettings, updateTheme, themePath);
        {
            return;
        }

        var strFile = themePath.Replace(".ascx", ".xml");
		Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strFile → {StrFile}", portalSettings, updateTheme, strFile);
        if (!File.Exists(strFile))
        {
            strFile = strFile.Replace(Path.GetFileName(strFile), "skin.xml");
        }

        var xmlDoc = new XmlDocument { XmlResolver = null };
        try
        {
            using var manifestReader = XmlReader.Create(strFile, new XmlReaderSettings { XmlResolver = null, });
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): manifestReader → {ManifestReader}", portalSettings, updateTheme, manifestReader);
            xmlDoc.Load(manifestReader);
        }
        catch
        {
			Logger.LogError("Exception in UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): {Arg2}", portalSettings, updateTheme, ex.Message);
            using var objectsReader = XmlReader.Create(new StringReader("<Objects></Objects>"), new XmlReaderSettings { XmlResolver = null, });
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objectsReader → {ObjectsReader}", portalSettings, updateTheme, objectsReader);
            xmlDoc.Load(objectsReader);
        }

        var xmlToken = xmlDoc.DocumentElement?.CreateNavigator().SelectSingleNode(XmlUtils.CreateXPathExpression("descendant::Object[Token='[$token]']", new KeyValuePair<string, object>("token", updateTheme.Token)));
		Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): xmlToken → {XmlToken}", portalSettings, updateTheme, xmlToken);
        if (xmlToken == null)
        {
            // add token
            string strToken = $"<Token>[{updateTheme.Token}]</Token><Settings></Settings>";
            xmlToken = xmlDoc.CreateElement("Object").CreateNavigator();
            xmlToken.InnerXml = strToken;
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): xmlToken.InnerXml → {Arg2}", portalSettings, updateTheme, xmlToken.InnerXml);
            xmlDoc.SelectSingleNode("Objects")?.CreateNavigator().AppendChild(xmlToken);
            xmlToken = xmlDoc.DocumentElement?.CreateNavigator().SelectSingleNode(XmlUtils.CreateXPathExpression("descendant::Object[Token='[$token]']", new KeyValuePair<string, object>("token", updateTheme.Token)));
        }

        var strValue = updateTheme.Value;

        var blnUpdate = false;
		Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): blnUpdate → {BlnUpdate}", portalSettings, updateTheme, blnUpdate);
        var settings = xmlToken?.Select(".//Settings/Setting");
		Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): settings → {Settings}", portalSettings, updateTheme, settings);
        if (settings is not null)
        {
            foreach (XmlNode xmlSetting in settings)
            {
                if (xmlSetting.SelectSingleNode("Name")?.InnerText == updateTheme.Setting)
                {
                    xmlSetting.SelectSingleNode("Value")?.InnerText = strValue;
                    blnUpdate = true;
                }
            }
        }

        if (!blnUpdate)
        {
            var strSetting = $"<Name>{updateTheme.Setting}</Name><Value>{strValue}</Value>";
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strSetting → {StrSetting}", portalSettings, updateTheme, strSetting);
            var xmlSetting = xmlDoc.CreateElement("Setting");
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): xmlSetting → {XmlSetting}", portalSettings, updateTheme, xmlSetting);
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): xmlSetting.InnerXml → {Arg2}", portalSettings, updateTheme, xmlSetting.InnerXml);
            xmlSetting.InnerXml = strSetting;
            xmlToken.SelectSingleNode("Settings").AppendChild(xmlSetting.CreateNavigator());
        }

        try
        {
            if (File.Exists(strFile))
            {
                File.SetAttributes(strFile, FileAttributes.Normal);
            }

            var objStream = File.CreateText(strFile);
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objStream → {ObjStream}", portalSettings, updateTheme, objStream);
            objStream.WriteLine(xmlDoc.InnerXml.Replace("><", ">" + Environment.NewLine + "<"));
            objStream.Close();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
		Logger.LogDebug("<<< Exiting UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme})", portalSettings, updateTheme);
    }

    private List<ThemeInfo> GetThemes(ThemeType type, string strRoot)
    {
		Logger.LogDebug(">>> Entering GetThemes(type={Type},strRoot={StrRoot})", type, strRoot);
        if (!Directory.Exists(strRoot))
        {
		Logger.LogDebug("<<< Exiting GetThemes(type={Type},strRoot={StrRoot})", type, strRoot);
            return [];
        }

        return (
            from strFolder in Directory.GetDirectories(strRoot)
            let strName = strFolder.Substring(strFolder.LastIndexOf(@"\", StringComparison.Ordinal) + 1)
            where strName != "_default"
            let themePath = strFolder.Replace(this.appStatus.ApplicationMapPath, string.Empty).TrimStart('\\').ToLowerInvariant()
            let isFallback = type == ThemeType.Skin ? this.IsFallbackSkin(themePath) : this.IsFallbackContainer(themePath)
            let canDelete = !isFallback && SkinController.CanDeleteSkin(this.hostSettings, strFolder, PortalSettings.Current.HomeDirectoryMapPath)
            let defaultThemeFile = this.GetDefaultThemeFileName(themePath, type)
            select new ThemeInfo
            {
                PackageName = strName,
                Type = type,
                Path = themePath,
                DefaultThemeFile = FormatThemePath(PortalSettings.Current, strFolder, defaultThemeFile, type),
                Thumbnail = this.GetThumbnail(themePath, defaultThemeFile),
                CanDelete = canDelete,
            }).ToList();
    }

    private string GetDefaultThemeFileName(string themePath, ThemeType type)
    {
		Logger.LogDebug(">>> Entering GetDefaultThemeFileName(themePath={ThemePath},type={Type})", themePath, type);
        var themeFiles = new List<string>();
        var folderPath = Path.Combine(this.appStatus.ApplicationMapPath, themePath);
		Logger.LogDebug("GetDefaultThemeFileName(themePath={ThemePath},type={Type}): folderPath → {FolderPath}", themePath, type, folderPath);
		Logger.LogDebug("GetDefaultThemeFileName(themePath={ThemePath},type={Type}): themeFiles → {ThemeFiles}", themePath, type, themeFiles);
        themeFiles.AddRange(Directory.GetFiles(folderPath, "*.ascx"));

        var defaultFile = themeFiles.FirstOrDefault(i =>
        {
			Logger.LogDebug("GetDefaultThemeFileName(themePath={ThemePath},type={Type}): defaultFile → {DefaultFile}", themePath, type, defaultFile);
            var fileName = Path.GetFileNameWithoutExtension(i);
			Logger.LogDebug("GetDefaultThemeFileName(themePath={ThemePath},type={Type}): fileName → {FileName}", themePath, type, fileName);
            return type == ThemeType.Skin ? DefaultLayoutNames.Contains(fileName, StringComparer.OrdinalIgnoreCase)
                : DefaultContainerNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
        });

        if (string.IsNullOrEmpty(defaultFile))
        {
            defaultFile = themeFiles.FirstOrDefault();
        }

		Logger.LogDebug("<<< Exiting GetDefaultThemeFileName(themePath={ThemePath},type={Type})", themePath, type);
        return !string.IsNullOrEmpty(defaultFile) ? Path.GetFileName(defaultFile) : string.Empty;
    }

    private string GetThumbnail(string themePath, string themeFileName)
    {
		Logger.LogDebug(">>> Entering GetThumbnail(themePath={ThemePath},themeFileName={ThemeFileName})", themePath, themeFileName);
        var folderPath = Path.Combine(this.appStatus.ApplicationMapPath, themePath);
        var filePath = Path.Combine(folderPath, themeFileName);
		Logger.LogDebug("GetThumbnail(themePath={ThemePath},themeFileName={ThemeFileName}): filePath → {FilePath}", themePath, themeFileName, filePath);
        var imagePath = string.Empty;
		Logger.LogDebug("GetThumbnail(themePath={ThemePath},themeFileName={ThemeFileName}): imagePath → {ImagePath}", themePath, themeFileName, imagePath);
		Logger.LogDebug("GetThumbnail(themePath={ThemePath},themeFileName={ThemeFileName}): folderPath → {FolderPath}", themePath, themeFileName, folderPath);
        foreach (var ext in ImageExtensions)
        {
            var path = Path.ChangeExtension(filePath, ext);
			Logger.LogDebug("GetThumbnail(themePath={ThemePath},themeFileName={ThemeFileName}): path → {Path}", themePath, themeFileName, path);
            if (File.Exists(path))
            {
                imagePath = path;
                break;
            }
        }

		Logger.LogDebug("<<< Exiting GetThumbnail(themePath={ThemePath},themeFileName={ThemeFileName})", themePath, themeFileName);
        return !string.IsNullOrEmpty(imagePath) ? this.CreateThumbnail(imagePath) : string.Empty;
    }

    private bool IsFallbackContainer(string skinPath)
    {
		Logger.LogDebug(">>> Entering IsFallbackContainer(skinPath={SkinPath})", skinPath);
        var strDefaultContainerPath = (Globals.HostMapPath + SkinController.RootContainer + SkinDefaults.GetSkinDefaults(this.hostSettings, SkinDefaultType.SkinInfo).Folder).Replace("/", @"\");
        if (strDefaultContainerPath.EndsWith(@"\", StringComparison.Ordinal))
        {
			Logger.LogDebug("IsFallbackContainer(skinPath={SkinPath}): strDefaultContainerPath → {StrDefaultContainerPath}", skinPath, strDefaultContainerPath);
            strDefaultContainerPath = strDefaultContainerPath.Substring(0, strDefaultContainerPath.Length - 1);
        }

        return skinPath.IndexOf(strDefaultContainerPath, StringComparison.CurrentCultureIgnoreCase) != -1;
    }

    private bool IsFallbackSkin(string skinPath)
    {
		Logger.LogDebug(">>> Entering IsFallbackSkin(skinPath={SkinPath})", skinPath);
        var strDefaultSkinPath = (Globals.HostMapPath + SkinController.RootSkin + SkinDefaults.GetSkinDefaults(this.hostSettings, SkinDefaultType.SkinInfo).Folder).Replace("/", @"\");
        if (strDefaultSkinPath.EndsWith(@"\", StringComparison.Ordinal))
        {
			Logger.LogDebug("IsFallbackSkin(skinPath={SkinPath}): strDefaultSkinPath → {StrDefaultSkinPath}", skinPath, strDefaultSkinPath);
            strDefaultSkinPath = strDefaultSkinPath.Substring(0, strDefaultSkinPath.Length - 1);
        }

        return skinPath.Equals(strDefaultSkinPath, StringComparison.OrdinalIgnoreCase);
    }
}
