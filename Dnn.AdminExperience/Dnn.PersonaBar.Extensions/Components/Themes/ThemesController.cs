// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Themes.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using Dnn.PersonaBar.Themes.Components.DTO;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Skins;

    using Image = System.Drawing.Image;

    public class ThemesController : ServiceLocator<IThemesController, ThemesController>, IThemesController
    {
        internal static readonly IList<string> ImageExtensions = new List<string>() { ".jpg", ".png", ".jpeg" };

        internal static readonly IList<string> DefaultLayoutNames = new List<string>() { "Default", "2-Col", "Home", "Index", "Main" };
        internal static readonly IList<string> DefaultContainerNames = new List<string>() { "Title-h2", "NoTitle", "Main", "Default" };
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ThemesController));

        private static readonly object ThreadLocker = new object();

        /// <inheritdoc />
        public IList<ThemeInfo> GetLayouts(PortalSettings portalSettings, ThemeLevel level)
        {
			Logger.LogDebug(">>> Entering GetLayouts(portalSettings={PortalSettings},level={Level})", portalSettings, level);
            var themes = new List<ThemeInfo>();
            if ((level & ThemeLevel.Site) == ThemeLevel.Site)
			Logger.LogDebug("GetLayouts(portalSettings={PortalSettings},level={Level}): themes → {Themes}", portalSettings, level, themes);
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootSkin)));
            }

            if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootSkin)));
            }

            if ((level & ThemeLevel.Global) == ThemeLevel.Global)
            {
                themes.AddRange(GetThemes(ThemeType.Skin, Path.Combine(Globals.HostMapPath, SkinController.RootSkin)));
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
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeDirectoryMapPath, SkinController.RootContainer)));
            }

            if ((level & ThemeLevel.SiteSystem) == ThemeLevel.SiteSystem)
            {
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(portalSettings.HomeSystemDirectoryMapPath, SkinController.RootContainer)));
            }

            if ((level & ThemeLevel.Global) == ThemeLevel.Global)
            {
                themes.AddRange(GetThemes(ThemeType.Container, Path.Combine(Globals.HostMapPath, SkinController.RootContainer)));
            }

			Logger.LogDebug("<<< Exiting GetContainers(portalSettings={PortalSettings},level={Level})", portalSettings, level);
            return themes;
        }

        /// <inheritdoc />
        public IList<ThemeFileInfo> GetThemeFiles(PortalSettings portalSettings, ThemeInfo theme)
        {
			Logger.LogDebug(">>> Entering GetThemeFiles(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
            var themePath = Path.Combine(Globals.ApplicationMapPath, theme.Path);
            var themeFiles = new List<ThemeFileInfo>();
			Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themePath → {ThemePath}", portalSettings, theme, themePath);
			Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFiles → {ThemeFiles}", portalSettings, theme, themeFiles);

            if (Directory.Exists(themePath))
            {
                bool fallbackSkin;
                if (theme.Type == ThemeType.Skin)
                {
                    fallbackSkin = IsFallbackSkin(themePath);
					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): fallbackSkin → {FallbackSkin}", portalSettings, theme, fallbackSkin);
                }
                else
                {
                    fallbackSkin = IsFallbackContainer(themePath);
                }

                var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";

				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): strSkinType → {StrSkinType}", portalSettings, theme, strSkinType);
                var canDeleteSkin = SkinController.CanDeleteSkin(themePath, portalSettings.HomeDirectoryMapPath);
                var arrFiles = Directory.GetFiles(themePath, "*.ascx");
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): canDeleteSkin → {CanDeleteSkin}", portalSettings, theme, canDeleteSkin);
				Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): arrFiles → {ArrFiles}", portalSettings, theme, arrFiles);

                foreach (var strFile in arrFiles)
                {
                    var file = strFile.ToLowerInvariant();

					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): file → {File}", portalSettings, theme, file);
                    var themeFile = new ThemeFileInfo();
                    themeFile.ThemeName = theme.PackageName;
					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile → {ThemeFile}", portalSettings, theme, themeFile);
					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile.ThemeName → {Arg2}", portalSettings, theme, themeFile.ThemeName);
					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile.Type → {Arg2}", portalSettings, theme, themeFile.Type);
					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): themeFile.Level → {Arg2}", portalSettings, theme, themeFile.Level);
                    themeFile.Type = theme.Type;
                    themeFile.Level = theme.Level;

                    var imagePath = string.Empty;
                    foreach (var ext in ImageExtensions)
                    {
					Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): imagePath → {ImagePath}", portalSettings, theme, imagePath);
                        var path = Path.ChangeExtension(file, ext);
                        if (File.Exists(path))
						Logger.LogDebug("GetThemeFiles(portalSettings={PortalSettings},theme={Theme}): path → {Path}", portalSettings, theme, path);
                        {
                            imagePath = path;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        themeFile.Thumbnail = CreateThumbnail(imagePath);
                    }

                    themeFile.Name = Path.GetFileNameWithoutExtension(file);
                    themeFile.Path = FormatThemePath(portalSettings, themePath, Path.GetFileName(strFile), theme.Type);
                    themeFile.CanDelete = (UserController.Instance.GetCurrentUserInfo().IsSuperUser || strSkinType == "L")
                                          && (!fallbackSkin && canDeleteSkin);

                    themeFiles.Add(themeFile);
                }
            }

			Logger.LogDebug("<<< Exiting GetThemeFiles(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
            return themeFiles;
        }

        /// <inheritdoc />
        public ThemeFileInfo GetThemeFile(PortalSettings portalSettings, string filePath, ThemeType type)
        {
            var themeName = SkinController.FormatSkinPath(filePath)
                            .Substring(filePath.IndexOf("/", StringComparison.OrdinalIgnoreCase) + 1)
                            .Replace("/", string.Empty);
            var themeLevel = GetThemeLevel(filePath);

            var themeInfo = (type == ThemeType.Skin ? this.GetLayouts(portalSettings, ThemeLevel.All)
                                                    : this.GetContainers(portalSettings, ThemeLevel.All))
                            .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == themeLevel);

            if (themeInfo != null)
            {
                return this.GetThemeFiles(portalSettings, themeInfo).FirstOrDefault(f => (f.Path + ".ascx").Equals(filePath, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        /// <inheritdoc />
        public void ApplyTheme(int portalId, ThemeFileInfo themeFile, ApplyThemeScope scope)
        {
            var skinPath = themeFile.Path + ".ascx";

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
        }

        /// <inheritdoc />
        public void ApplyDefaultTheme(PortalSettings portalSettings, string themeName, ThemeLevel level)
        {
            var skin = this.GetLayouts(portalSettings, ThemeLevel.All)
                .FirstOrDefault(t => t.PackageName.Equals(themeName, StringComparison.OrdinalIgnoreCase) && t.Level == level);
            if (skin != null)
            {
                var skinFile = this.GetThemeFiles(portalSettings, skin).FirstOrDefault(t => t.Path == skin.DefaultThemeFile);
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
                if (containerFile != null)
                {
                    this.ApplyTheme(portalSettings.PortalId, containerFile, ApplyThemeScope.Site | ApplyThemeScope.Edit);
                }
            }
        }

        /// <inheritdoc />
        public void DeleteTheme(PortalSettings portalSettings, ThemeFileInfo themeFile)
        {
            var themePath = SkinController.FormatSkinSrc(themeFile.Path, portalSettings);
            var user = UserController.Instance.GetCurrentUserInfo();

            if (!user.IsSuperUser && themePath.IndexOf("\\portals\\_default\\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
            {
                throw new SecurityException("NoPermission");
            }

            File.Delete(Path.Combine(Globals.ApplicationMapPath, themePath));
            DataCache.ClearPortalCache(portalSettings.PortalId, true);
        }

        /// <inheritdoc />
        public void DeleteThemePackage(PortalSettings portalSettings, ThemeInfo theme)
        {
			Logger.LogDebug(">>> Entering DeleteThemePackage(portalSettings={PortalSettings},theme={Theme})", portalSettings, theme);
            var themePath = Path.Combine(Globals.ApplicationMapPath, theme.Path);
            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("DeleteThemePackage(portalSettings={PortalSettings},theme={Theme}): themePath → {ThemePath}", portalSettings, theme, themePath);
			Logger.LogDebug("DeleteThemePackage(portalSettings={PortalSettings},theme={Theme}): user → {User}", portalSettings, theme, user);

            if (!user.IsSuperUser && themePath.IndexOf(@"\portals\_default\", StringComparison.OrdinalIgnoreCase) != Null.NullInteger)
            {
                throw new SecurityException("NoPermission");
            }

            if (theme.Type == ThemeType.Skin)
            {
                var skinPackage = SkinController.GetSkinPackage(portalSettings.PortalId, theme.PackageName, "Skin");
                if (skinPackage != null)
				Logger.LogDebug("DeleteThemePackage(portalSettings={PortalSettings},theme={Theme}): skinPackage → {SkinPackage}", portalSettings, theme, skinPackage);
                {
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
            themePath = Path.Combine(Globals.ApplicationMapPath, themePath.TrimStart('/'));
			Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): themePath → {ThemePath}", portalSettings, updateTheme, themePath);

            var objStreamReader = File.OpenText(themePath);
            var strSkin = objStreamReader.ReadToEnd();
			Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objStreamReader → {ObjStreamReader}", portalSettings, updateTheme, objStreamReader);
			Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strSkin → {StrSkin}", portalSettings, updateTheme, strSkin);
            objStreamReader.Close();
            var strTag = $"<dnn:{updateTheme.Token} runat=\"server\" id=\"dnn{updateTheme.Token}\"";
            var intOpenTag = strSkin.IndexOf(strTag, StringComparison.OrdinalIgnoreCase);
			Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strTag → {StrTag}", portalSettings, updateTheme, strTag);
			Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intOpenTag → {IntOpenTag}", portalSettings, updateTheme, intOpenTag);
            if (intOpenTag != -1)
            {
                var intCloseTag = strSkin.IndexOf(" />", intOpenTag, StringComparison.Ordinal);
                var strAttribute = updateTheme.Setting;
				Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intCloseTag → {IntCloseTag}", portalSettings, updateTheme, intCloseTag);
                var intStartAttribute = strSkin.IndexOf(strAttribute, intOpenTag, StringComparison.OrdinalIgnoreCase);
                string strValue = updateTheme.Value;
				Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intStartAttribute → {IntStartAttribute}", portalSettings, updateTheme, intStartAttribute);
                if (intStartAttribute != -1 && intStartAttribute < intCloseTag)
                {
                    // remove attribute
                    var intEndAttribute = strSkin.IndexOf("\" ", intStartAttribute, StringComparison.Ordinal);
                    strSkin = strSkin.Substring(0, intStartAttribute) + strSkin.Substring(intEndAttribute + 2);
					Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): intEndAttribute → {IntEndAttribute}", portalSettings, updateTheme, intEndAttribute);
                }

                // add attribute
                strSkin = strSkin.Insert(intOpenTag + strTag.Length, $" {strAttribute}=\"{strValue}\"");

                File.SetAttributes(themePath, FileAttributes.Normal);
                var objStream = File.CreateText(themePath);
                objStream.WriteLine(strSkin);
				Logger.LogDebug("UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objStream → {ObjStream}", portalSettings, updateTheme, objStream);
                objStream.Close();

                UpdateManifest(portalSettings, updateTheme);
            }
			Logger.LogDebug("<<< Exiting UpdateTheme(portalSettings={PortalSettings},updateTheme={UpdateTheme})", portalSettings, updateTheme);
        }

        /// <inheritdoc />
        public void ParseTheme(PortalSettings portalSettings, ThemeInfo theme, ParseType parseType)
        {
			Logger.LogDebug(">>> Entering ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType})", portalSettings, theme, parseType);
            var strRootPath = Null.NullString;
            switch (theme.Level)
			Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): strRootPath → {StrRootPath}", portalSettings, theme, parseType, strRootPath);
            {
                case ThemeLevel.Global: // global
                    strRootPath = Globals.HostMapPath;
                    break;
                case ThemeLevel.Site: // local
                    strRootPath = portalSettings.HomeDirectoryMapPath;
                    break;
            }

            var objSkinFiles = new SkinFileProcessor(strRootPath, theme.Type == ThemeType.Container ? SkinController.RootContainer : SkinController.RootSkin, theme.PackageName);
            var arrSkinFiles = new ArrayList();
			Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): objSkinFiles → {ObjSkinFiles}", portalSettings, theme, parseType, objSkinFiles);
			Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): arrSkinFiles → {ArrSkinFiles}", portalSettings, theme, parseType, arrSkinFiles);

            var strFolder = Path.Combine(Globals.ApplicationMapPath, theme.Path);
            if (Directory.Exists(strFolder))
			Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): strFolder → {StrFolder}", portalSettings, theme, parseType, strFolder);
            {
                var arrFiles = Directory.GetFiles(strFolder);
                foreach (var strFile in arrFiles)
                {
				Logger.LogDebug("ParseTheme(portalSettings={PortalSettings},theme={Theme},parseType={ParseType}): arrFiles → {ArrFiles}", portalSettings, theme, parseType, arrFiles);
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
                            if (File.Exists(strFile.Replace(".ascx", ".htm")) == false && File.Exists(strFile.Replace(".ascx", ".html")) == false)
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

        internal static string CreateThumbnail(string strImage)
        {
			Logger.LogDebug(">>> Entering CreateThumbnail(strImage={StrImage})", strImage);
            var imageFileName = Path.GetFileName(strImage);
            if (string.IsNullOrEmpty(imageFileName) || imageFileName.StartsWith("thumbnail_", StringComparison.OrdinalIgnoreCase))
			Logger.LogDebug("CreateThumbnail(strImage={StrImage}): imageFileName → {ImageFileName}", strImage, imageFileName);
            {
                strImage = strImage.Substring(Globals.ApplicationMapPath.Length);
                strImage = strImage.Replace(@"\", "/");
				Logger.LogDebug("CreateThumbnail(strImage={StrImage}): strImage → {StrImage1}", strImage, strImage);
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
								Logger.LogDebug("CreateThumbnail(strImage={StrImage}): dblScale → {DblScale}", strImage, dblScale);
                                intHeight = intSize;
                                intWidth = Convert.ToInt32(objImage.Width * dblScale);
								Logger.LogDebug("CreateThumbnail(strImage={StrImage}): intHeight → {IntHeight}", strImage, intHeight);
								Logger.LogDebug("CreateThumbnail(strImage={StrImage}): intWidth → {IntWidth}", strImage, intWidth);
                            }
                            else
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

            strThumbnail = strThumbnail.Substring(Globals.ApplicationMapPath.Length);
            strThumbnail = strThumbnail.Replace(@"\", "/");

            // return thumbnail filename
			Logger.LogDebug("<<< Exiting CreateThumbnail(strImage={StrImage})", strImage);
            return strThumbnail;
        }

        internal static ThemeLevel GetThemeLevel(string themeFilePath)
        {
			Logger.LogDebug(">>> Entering GetThemeLevel(themeFilePath={ThemeFilePath})", themeFilePath);
            themeFilePath = themeFilePath.Replace(@"\", "/");
            if (!string.IsNullOrEmpty(Globals.ApplicationPath)
				Logger.LogDebug("GetThemeLevel(themeFilePath={ThemeFilePath}): themeFilePath → {ThemeFilePath1}", themeFilePath, themeFilePath);
                && !themeFilePath.StartsWith("[", StringComparison.Ordinal)
                && !themeFilePath.StartsWith(Globals.ApplicationPath, StringComparison.InvariantCultureIgnoreCase))
            {
                var needSlash = !Globals.ApplicationPath.EndsWith("/", StringComparison.Ordinal) && !themeFilePath.StartsWith("/", StringComparison.Ordinal);
                themeFilePath = $"{Globals.ApplicationPath}{(needSlash ? "/" : string.Empty)}{themeFilePath}";
				Logger.LogDebug("GetThemeLevel(themeFilePath={ThemeFilePath}): needSlash → {NeedSlash}", themeFilePath, needSlash);
            }

            if (themeFilePath.IndexOf(Globals.HostPath.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger
                || themeFilePath.StartsWith("[G]", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeLevel.Global;
            }
            else if ((PortalSettings.Current != null &&
                        themeFilePath.IndexOf(PortalSettings.Current.HomeSystemDirectory.TrimStart('/'), StringComparison.OrdinalIgnoreCase) > Null.NullInteger)
                        || themeFilePath.StartsWith("[S]", StringComparison.OrdinalIgnoreCase))
            {
                return ThemeLevel.SiteSystem;
            }
            else
            {
				Logger.LogDebug("<<< Exiting GetThemeLevel(themeFilePath={ThemeFilePath})", themeFilePath);
                return ThemeLevel.Site;
            }
        }

        protected override Func<IThemesController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new ThemesController();
        }

        private static List<ThemeInfo> GetThemes(ThemeType type, string strRoot)
        {
            var themes = new List<ThemeInfo>();
            if (Directory.Exists(strRoot))
            {
                foreach (var strFolder in Directory.GetDirectories(strRoot))
                {
                    var strName = strFolder.Substring(strFolder.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
                    if (strName != "_default")
                    {
                        var themePath = strFolder.Replace(Globals.ApplicationMapPath, string.Empty).TrimStart('\\').ToLowerInvariant();
                        var isFallback = type == ThemeType.Skin ? IsFallbackSkin(themePath) : IsFallbackContainer(themePath);
						Logger.LogDebug("GetThemes(type={Type},strRoot={StrRoot}): themePath → {ThemePath}", type, strRoot, themePath);
						Logger.LogDebug("GetThemes(type={Type},strRoot={StrRoot}): isFallback → {IsFallback}", type, strRoot, isFallback);
                        var canDelete = !isFallback && SkinController.CanDeleteSkin(strFolder, PortalSettings.Current.HomeDirectoryMapPath);
                        var defaultThemeFile = GetDefaultThemeFileName(themePath, type);
						Logger.LogDebug("GetThemes(type={Type},strRoot={StrRoot}): canDelete → {CanDelete}", type, strRoot, canDelete);
						Logger.LogDebug("GetThemes(type={Type},strRoot={StrRoot}): defaultThemeFile → {DefaultThemeFile}", type, strRoot, defaultThemeFile);
                        themes.Add(new ThemeInfo()
                        {
                            PackageName = strName,
                            Type = type,
                            Path = themePath,
                            DefaultThemeFile = FormatThemePath(PortalSettings.Current, strFolder, defaultThemeFile, type),
                            Thumbnail = GetThumbnail(themePath, defaultThemeFile),
                            CanDelete = canDelete,
                        });
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetThemes(type={Type},strRoot={StrRoot})", type, strRoot);
            return themes;
        }

        private static string GetDefaultThemeFileName(string themePath, ThemeType type)
        {
            var themeFiles = new List<string>();
            var folderPath = Path.Combine(Globals.ApplicationMapPath, themePath);
            themeFiles.AddRange(Directory.GetFiles(folderPath, "*.ascx"));

            var defaultFile = themeFiles.FirstOrDefault(i =>
            {
                var fileName = Path.GetFileNameWithoutExtension(i);
                return type == ThemeType.Skin ? DefaultLayoutNames.Contains(fileName, StringComparer.OrdinalIgnoreCase)
                                              : DefaultContainerNames.Contains(fileName, StringComparer.OrdinalIgnoreCase);
            });

            if (string.IsNullOrEmpty(defaultFile))
            {
                defaultFile = themeFiles.FirstOrDefault();
            }

            return !string.IsNullOrEmpty(defaultFile) ? Path.GetFileName(defaultFile) : string.Empty;
        }

        private static string GetThumbnail(string themePath, string themeFileName)
        {
            var folderPath = Path.Combine(Globals.ApplicationMapPath, themePath);
            var filePath = Path.Combine(folderPath, themeFileName);
            var imagePath = string.Empty;
            foreach (var ext in ImageExtensions)
            {
                var path = Path.ChangeExtension(filePath, ext);
                if (File.Exists(path))
                {
                    imagePath = path;
                    break;
                }
            }

            return !string.IsNullOrEmpty(imagePath) ? CreateThumbnail(imagePath) : string.Empty;
        }

        private static bool NeedCreateThumbnail(string thumbnailPath, string imagePath)
        {
            return !File.Exists(thumbnailPath) || File.GetLastWriteTime(thumbnailPath) != File.GetLastWriteTime(imagePath);
        }

        private static bool IsFallbackContainer(string skinPath)
        {
            var strDefaultContainerPath = (Globals.HostMapPath + SkinController.RootContainer + SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo).Folder).Replace("/", @"\");
            if (strDefaultContainerPath.EndsWith(@"\", StringComparison.Ordinal))
            {
                strDefaultContainerPath = strDefaultContainerPath.Substring(0, strDefaultContainerPath.Length - 1);
            }

            return skinPath.IndexOf(strDefaultContainerPath, StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        private static bool IsFallbackSkin(string skinPath)
        {
            var strDefaultSkinPath = (Globals.HostMapPath + SkinController.RootSkin + SkinDefaults.GetSkinDefaults(SkinDefaultType.SkinInfo).Folder).Replace("/", @"\");
            if (strDefaultSkinPath.EndsWith(@"\", StringComparison.Ordinal))
            {
                strDefaultSkinPath = strDefaultSkinPath.Substring(0, strDefaultSkinPath.Length - 1);
            }

            return skinPath.Equals(strDefaultSkinPath, StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatThemePath(PortalSettings portalSettings, string themePath, string fileName, ThemeType type)
        {
            var filePath = Path.Combine(themePath, fileName);
            var lowercasePath = filePath.ToLowerInvariant();
            var strRootSkin = type == ThemeType.Skin ? SkinController.RootSkin : SkinController.RootContainer;

            var strSkinType = themePath.IndexOf(Globals.HostMapPath, StringComparison.OrdinalIgnoreCase) != -1 ? "G" : "L";
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
            if (File.Exists(themePath.Replace(".ascx", ".htm")))
			Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): themePath → {ThemePath}", portalSettings, updateTheme, themePath);
            {
                var strFile = themePath.Replace(".ascx", ".xml");
                if (!File.Exists(strFile))
				Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strFile → {StrFile}", portalSettings, updateTheme, strFile);
                {
                    strFile = strFile.Replace(Path.GetFileName(strFile), "skin.xml");
                }

                var xmlDoc = new XmlDocument { XmlResolver = null };
                try
                {
                    using var manifestReader = XmlReader.Create(strFile, new XmlReaderSettings { XmlResolver = null, });
                    xmlDoc.Load(manifestReader);
					Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): manifestReader → {ManifestReader}", portalSettings, updateTheme, manifestReader);
                }
                catch
                {
					Logger.LogError("Exception in UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): {Arg2}", portalSettings, updateTheme, ex.Message);
                    using var objectsReader = XmlReader.Create(new StringReader("<Objects></Objects>"), new XmlReaderSettings { XmlResolver = null, });
                    xmlDoc.Load(objectsReader);
					Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objectsReader → {ObjectsReader}", portalSettings, updateTheme, objectsReader);
                }

                var xmlToken = xmlDoc.DocumentElement?.CreateNavigator().SelectSingleNode(XmlUtils.CreateXPathExpression("descendant::Object[Token='[$token]']", new KeyValuePair<string, object>("token", updateTheme.Token)));
                if (xmlToken == null)
				Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): xmlToken → {XmlToken}", portalSettings, updateTheme, xmlToken);
                {
                    // add token
                    string strToken = $"<Token>[{updateTheme.Token}]</Token><Settings></Settings>";
                    xmlToken = xmlDoc.CreateElement("Object").CreateNavigator();
					Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): xmlToken.InnerXml → {Arg2}", portalSettings, updateTheme, xmlToken.InnerXml);
                    xmlToken.InnerXml = strToken;
                    xmlDoc.SelectSingleNode("Objects")?.CreateNavigator().AppendChild(xmlToken);
                    xmlToken = xmlDoc.DocumentElement?.CreateNavigator().SelectSingleNode(XmlUtils.CreateXPathExpression("descendant::Object[Token='[$token]']", new KeyValuePair<string, object>("token", updateTheme.Token)));
                }

                var strValue = updateTheme.Value;

                var blnUpdate = false;
                var settings = xmlToken?.Select(".//Settings/Setting");
				Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): blnUpdate → {BlnUpdate}", portalSettings, updateTheme, blnUpdate);
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
                    var xmlSetting = xmlDoc.CreateElement("Setting");
					Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): strSetting → {StrSetting}", portalSettings, updateTheme, strSetting);
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
                    objStream.WriteLine(xmlDoc.InnerXml.Replace("><", ">" + Environment.NewLine + "<"));
					Logger.LogDebug("UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme}): objStream → {ObjStream}", portalSettings, updateTheme, objStream);
                    objStream.Close();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
			Logger.LogDebug("<<< Exiting UpdateManifest(portalSettings={PortalSettings},updateTheme={UpdateTheme})", portalSettings, updateTheme);
        }
    }
}
