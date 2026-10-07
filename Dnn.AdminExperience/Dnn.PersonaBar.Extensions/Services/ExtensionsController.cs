// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Xml;
    using System.Xml.XPath;

    using Dnn.PersonaBar.Extensions.Components;
    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Editors;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Installer.Writers;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    using Constants = Dnn.PersonaBar.Extensions.Components.Constants;
    using Util = DotNetNuke.Entities.Content.Common.Util;

    [MenuPermission(Scope = ServiceScope.Admin)]
    public class ExtensionsController : PersonaBarApiController
    {
        private const string AuthFailureMessage = "Authorization has been denied for this request.";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExtensionsController));
        private static readonly Regex ManifestExensionsRegex = new Regex(@"dnn\d*$");
        private static readonly string[] SpecialModuleFolders = new[] { "mvc" };
        private readonly Components.ExtensionsController controller = new Components.ExtensionsController();

        /// GET: api/Extensions/GetPackageTypes
        /// <summary>Get installed package types.</summary>
        /// <returns>List of package types.</returns>
        [HttpGet]
        public HttpResponseMessage GetPackageTypes()
        {
			Logger.LogError(">>> Entering GetPackageTypes()");
            try
            {
                var packageTypes = this.controller.GetPackageTypes()
                    .OrderBy(t => t.Value.PackageType != "Module")
                    .Select(t =>
                    {
                        var packageType = t.Value.PackageType;
                        string rootPath;
                        return new
                        {
                            Type = packageType,
                            HasAvailablePackages = this.controller.HasAvailablePackage(packageType, out rootPath),
                            DisplayName = Localization.GetString(packageType + ".Type", Constants.SharedResources),
                        };
                    });
                var response = new
                {
                    Success = true,
                    Results = packageTypes,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetPackageTypes()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetAllPackagesListExceptLangPacks
        /// <summary>Get installed packages list except language packs.</summary>
        /// <returns>List of [Id,Name] pairs of all system packages.</returns>
        [HttpGet]
        public HttpResponseMessage GetAllPackagesListExceptLangPacks()
        {
			Logger.LogError(">>> Entering GetAllPackagesListExceptLangPacks()");
            try
            {
                var packages = Utility.GetAllPackagesListExceptLangPacks();
				Logger.LogError("GetAllPackagesListExceptLangPacks(): packages → {Packages}", packages);
                return this.Request.CreateResponse(HttpStatusCode.OK, packages);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetAllPackagesListExceptLangPacks()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetInstalledPackages
        /// <summary>Gets installed packages.</summary>
        /// <param name="packageType">The package type.</param>
        /// <returns>List of installed packages.</returns>
        [HttpGet]
        public HttpResponseMessage GetInstalledPackages(string packageType)
        {
			Logger.LogError(">>> Entering GetInstalledPackages(packageType={PackageType})", packageType);
            try
            {
                var packages = this.controller.GetInstalledPackages(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, packageType);
				Logger.LogError("GetInstalledPackages(packageType={PackageType}): packages → {Packages}", packageType, packages);
                var response = new
                {
                    Success = true,
                    Results = packages,
                    TotalResults = packages.Count,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetInstalledPackages(packageType={PackageType})", packageType);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// GET: api/Extensions/GetAvailablePackages
        /// <summary>Gets available packages.</summary>
        /// <param name="packageType">The package type.</param>
        /// <returns>List of available packages.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAvailablePackages(string packageType)
        {
			Logger.LogError(">>> Entering GetAvailablePackages(packageType={PackageType})", packageType);
            try
            {
                var packages = this.controller.GetAvailablePackages(packageType);
				Logger.LogError("GetAvailablePackages(packageType={PackageType}): packages → {Packages}", packageType, packages);
                var response = new
                {
                    Success = true,
                    Results = packages,
                    TotalResults = packages.Count,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetAvailablePackages(packageType={PackageType})", packageType);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //// /// GET: api/Extensions/GetPackageUsage
        ////        /// <summary>
        ////        /// Gets package usage
        ////        /// </summary>
        ////        /// <param name="portalId"></param>
        ////        /// <param name="packageId"></param>
        ////        /// <returns>List of tabs using a specific package</returns>
        ////        [HttpGet]
        ////        public HttpResponseMessage GetPackageUsage(int portalId, int packageId)
        ////        {
        ////            try
        ////            {
        ////                var packages = _controller.GetPackageUsage(portalId, packageId).Select(t => new
        ////                {
        ////                    TabLink = _controller.GetFormattedTabLink(portalId, t)
        ////                }).ToList();
        ////
        ////                var response = new
        ////                {
        ////                    Success = true,
        ////                    Results = packages,
        ////                    TotalResults = packages.Count
        ////                };
        ////                return Request.CreateResponse(HttpStatusCode.OK, response);
        ////            }
        ////            catch (Exception ex)
        ////            {
        ////                Logger.Error(ex);
        ////                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        ////            }
        ////        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSourceFolders(/*int moduleControlId*/)
        {
			Logger.LogError(">>> Entering GetSourceFolders()");
            var path = Path.Combine(Globals.ApplicationMapPath, "DesktopModules");
            var controlFolders = (
                from subdirectory in Directory.GetDirectories(path, "*", SearchOption.AllDirectories)
                select subdirectory).ToList();
				Logger.LogError("GetSourceFolders(): path → {Path}", path);

            controlFolders.Insert(0, Path.Combine(Globals.ApplicationMapPath, @"Admin\Skins"));

            ////var moduleControl = ModuleControlController.GetModuleControl(moduleControlId);
            ////var currentControlFolder = moduleControl == null ? "" :
            ////   (Path.GetDirectoryName(moduleControl.ControlSrc.ToLower()) ?? "").Replace('\\', '/');
            var response = new List<KeyValuePair<string, string>>();
			Logger.LogError("GetSourceFolders(): response → {Response}", response);
            var appPathLen = Globals.ApplicationMapPath.Length + 1;
            foreach (var folder in controlFolders)
            {
                var moduleControls = Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly)
                    .Count(s => s.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase) ||
                                s.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                                s.EndsWith(".vbhtml", StringComparison.OrdinalIgnoreCase) ||
                                s.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                                s.EndsWith(".htm", StringComparison.OrdinalIgnoreCase));
                if (moduleControls > 0)
                {
                    var shortFolder = folder.Substring(appPathLen).Replace('\\', '/');
					Logger.LogError("GetSourceFolders(): shortFolder → {ShortFolder}", shortFolder);
                    var item = new KeyValuePair<string, string>(shortFolder.ToLowerInvariant(), shortFolder);
					Logger.LogError("GetSourceFolders(): item → {Item}", item);
                    response.Add(item);
                }
            }

			Logger.LogWarning("<<< Exiting GetSourceFolders()");
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetSourceFiles(string root)
        {
			Logger.LogError(">>> Entering GetSourceFiles(root={Root})", root);
            var response = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(string.Empty, "<" + Localization.GetString("None_Specified") + ">"),
            };

            if (!string.IsNullOrEmpty(root))
            {
                var path = Path.Combine(Globals.ApplicationMapPath, root.Replace('/', '\\'));
				Logger.LogError("GetSourceFiles(root={Root}): path → {Path}", root, path);
                if (Directory.Exists(path))
                {
                    AddFiles(response, path, root, "*.ascx");
                    AddFiles(response, path, root, "*.cshtml");
                    AddFiles(response, path, root, "*.vbhtml");
                    AddFiles(response, path, root, "*.html");
                    AddFiles(response, path, root, "*.htm");
                }
            }

			Logger.LogWarning("<<< Exiting GetSourceFiles(root={Root})", root);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage LoadIcons(string controlPath)
        {
			Logger.LogError(">>> Entering LoadIcons(controlPath={ControlPath})", controlPath);
            var response = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(string.Empty, "<" + Localization.GetString("None_Specified") + ">"),
            };

            if (!string.IsNullOrEmpty(controlPath))
            {
                var idx = controlPath.LastIndexOf("/", StringComparison.Ordinal);
				Logger.LogError("LoadIcons(controlPath={ControlPath}): idx → {Idx}", controlPath, idx);
                var root = controlPath.Substring(0, Math.Max(0, idx));
				Logger.LogError("LoadIcons(controlPath={ControlPath}): root → {Root}", controlPath, root);
                var path = Path.Combine(Globals.ApplicationMapPath, root.Replace('/', '\\'));
				Logger.LogError("LoadIcons(controlPath={ControlPath}): path → {Path}", controlPath, path);
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
					Logger.LogError("LoadIcons(controlPath={ControlPath}): files → {Files}", controlPath, files);
                    if (files.Length > 0)
                    {
                        var extensions = Globals.glbImageFileTypes.ToLowerInvariant().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
						Logger.LogError("LoadIcons(controlPath={ControlPath}): extensions → {Extensions}", controlPath, extensions);
                        foreach (var file in files)
                        {
                            var ext = Path.GetExtension(file) ?? string.Empty;
							Logger.LogError("LoadIcons(controlPath={ControlPath}): ext → {Ext}", controlPath, ext);
                            var extension = ext.Length <= 1 ? string.Empty : ext.Substring(1).ToLowerInvariant();
							Logger.LogError("LoadIcons(controlPath={ControlPath}): extension → {Extension}", controlPath, extension);
                            if (extensions.Contains(extension))
                            {
                                path = Path.GetFileName(file);
                                if (path != null)
                                {
                                    var item = new KeyValuePair<string, string>(path.ToLowerInvariant(), Path.GetFileName(file));
									Logger.LogError("LoadIcons(controlPath={ControlPath}): item → {Item}", controlPath, item);
                                    response.Add(item);
                                }
                            }
                        }
                    }
                }
            }

			Logger.LogWarning("<<< Exiting LoadIcons(controlPath={ControlPath})", controlPath);
            return this.Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        public HttpResponseMessage GetLanguagesList()
        {
			Logger.LogError(">>> Entering GetLanguagesList()");
            Logger.LogWarning("<<< Exiting GetLanguagesList()");
            return this.Request.CreateResponse(HttpStatusCode.OK, Utility.GetAllLanguagesList());
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleCategories()
        {
			Logger.LogError(">>> Entering GetModuleCategories()");
            var termController = Util.GetTermController();
            var categories = termController.GetTermsByVocabulary("Module_Categories").OrderBy(t => t.Weight).Select(t => t.Name);
			Logger.LogError("GetModuleCategories(): categories → {Categories}", categories);
			Logger.LogError("GetModuleCategories(): termController → {TermController}", termController);

			Logger.LogWarning("<<< Exiting GetModuleCategories()");
            return this.Request.CreateResponse(HttpStatusCode.OK, categories);
        }

        [HttpGet]
        public HttpResponseMessage GetPackageSettings(int siteId, int packageId)
        {
			Logger.LogError(">>> Entering GetPackageSettings(siteId={SiteId},packageId={PackageId})", siteId, packageId);
            var portalId = siteId;
            if (portalId == Null.NullInteger && !this.UserInfo.IsSuperUser)
            {
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
			Logger.LogError("GetPackageSettings(siteId={SiteId},packageId={PackageId}): package → {Package}", siteId, packageId, package);
            if (package == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                var packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
				Logger.LogError("GetPackageSettings(siteId={SiteId},packageId={PackageId}): packageEditor → {PackageEditor}", siteId, packageId, packageEditor);
                var packageDetail = packageEditor?.GetPackageDetail(portalId, package) ?? new PackageInfoDto(portalId, package);
				Logger.LogError("GetPackageSettings(siteId={SiteId},packageId={PackageId}): packageDetail → {PackageDetail}", siteId, packageId, packageDetail);
                return this.Request.CreateResponse(HttpStatusCode.OK, packageDetail);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetPackageSettings(siteId={SiteId},packageId={PackageId})", siteId, packageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SavePackageSettings(PackageSettingsDto packageSettings)
        {
			Logger.LogError(">>> Entering SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageSettings.PackageId);
				Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): package → {Package}", packageSettings, package);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.InternalServerError,
                        Localization.GetString("SavePackageSettings.PackageNotFound", Constants.SharedResources));
                }

                if (this.UserInfo.IsSuperUser)
                {
                    var authService = AuthenticationController.GetAuthenticationServiceByPackageID(package.PackageID);
					Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): authService → {AuthService}", packageSettings, authService);
                    var isReadOnly = authService != null && authService.AuthenticationType == Constants.DnnAuthTypeName;
					Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): isReadOnly → {IsReadOnly}", packageSettings, isReadOnly);
                    if (isReadOnly)
                    {
                        return this.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest,
                            Localization.GetString("ReadOnlyPackage.SaveErrorMessage", Constants.SharedResources));
                    }

                    var type = package.GetType();
					Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): type → {Type}", packageSettings, type);
                    var needUpdate = false;
					Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): needUpdate → {NeedUpdate}", packageSettings, needUpdate);
                    foreach (var kvp in packageSettings.Settings)
                    {
                        var property = type.GetProperty(kvp.Key, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
						Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): property → {Property}", packageSettings, property);
                        if (property != null && property.CanWrite)
                        {
                            var value = kvp.Value;
                            var propValue = property.GetValue(package);
							Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): propValue → {PropValue}", packageSettings, propValue);
                            if (propValue == null || propValue.ToString() != value)
                            {
                                var nativeValue = property.PropertyType == typeof(Version)
                                    ? new Version(value) : Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture);
                                property.SetValue(package, nativeValue);
                                needUpdate = true;
                            }
                        }
                    }

                    if (needUpdate)
                    {
                        PackageController.Instance.SaveExtensionPackage(package);
                    }
                }

                var packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
				Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): packageEditor → {PackageEditor}", packageSettings, packageEditor);
                if (packageEditor != null)
                {
                    string error;
                    packageEditor.SavePackageSettings(packageSettings, out error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Error = error });
                    }
                }

                var packageDetail = packageEditor?.GetPackageDetail(packageSettings.PortalId, package) ?? new PackageInfoDto(packageSettings.PortalId, package);
				Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): packageDetail → {PackageDetail}", packageSettings, packageDetail);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, PackageDetail = packageDetail });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetAvailableControls(int packageId)
        {
			Logger.LogError(">>> Entering GetAvailableControls(packageId={PackageId})", packageId);
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
			Logger.LogError("GetAvailableControls(packageId={PackageId}): package → {Package}", packageId, package);
            if (package == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
            }

            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(packageId);
				Logger.LogError("GetAvailableControls(packageId={PackageId}): desktopModule → {DesktopModule}", packageId, desktopModule);
                var rootFolder = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", desktopModule.FolderName);
				Logger.LogError("GetAvailableControls(packageId={PackageId}): rootFolder → {RootFolder}", packageId, rootFolder);
                var controls = Directory.GetFiles(rootFolder, "*.ascx", SearchOption.AllDirectories)
                    .Select(f => f.Replace(Globals.ApplicationMapPath, "~").Replace(@"\", "/"))
                    .ToList();

                return this.Request.CreateResponse(HttpStatusCode.OK, controls);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetAvailableControls(packageId={PackageId})", packageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DeletePackage(DeletePackageDto deletePackage)
        {
			Logger.LogError(">>> Entering DeletePackage(deletePackage={DeletePackage})", deletePackage);
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == deletePackage.Id);
				Logger.LogError("DeletePackage(deletePackage={DeletePackage}): package → {Package}", deletePackage, package);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, string.Empty);
                }

                var installer = new Installer(package, Globals.ApplicationMapPath);
				Logger.LogError("DeletePackage(deletePackage={DeletePackage}): installer → {Installer}", deletePackage, installer);
                installer.UnInstall(deletePackage.DeleteFiles);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting DeletePackage(deletePackage={DeletePackage})", deletePackage);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        [RequireHost]
        public Task<HttpResponseMessage> InstallPackage([FromUri] string legacySkin = null, [FromUri] bool isPortalPackage = false)
        {
			Logger.LogError(">>> Entering InstallPackage(legacySkin={LegacySkin},isPortalPackage={IsPortalPackage})", legacySkin, isPortalPackage);
            try
            {
                return
                    this.UploadFileAction((portalSettings, userInfo, filePath, stream) =>
                        InstallController.Instance.InstallPackage(portalSettings, userInfo, legacySkin, filePath, stream, isPortalPackage));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting InstallPackage(legacySkin={LegacySkin},isPortalPackage={IsPortalPackage})", legacySkin, isPortalPackage);
                return Task.FromResult(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        [RequireHost]
        public Task<HttpResponseMessage> ParsePackage()
        {
			Logger.LogError(">>> Entering ParsePackage()");
            try
            {
                return
                    this.UploadFileAction((portalSettings, userInfo, filePath, stream) =>
                        InstallController.Instance.ParsePackage(portalSettings, userInfo, filePath, stream));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting ParsePackage()");
                return Task.FromResult(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ParsePackageFile(DownloadPackageDto package)
        {
			Logger.LogError(">>> Entering ParsePackageFile(package={Package})", package);
            try
            {
                var installFolder = GetPackageInstallFolder(package.PackageType);
				Logger.LogError("ParsePackageFile(package={Package}): installFolder → {InstallFolder}", package, installFolder);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(package.FileName))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, package.FileName);
				Logger.LogError("ParsePackageFile(package={Package}): packagePath → {PackagePath}", package, packagePath);
                if (!File.Exists(packagePath))
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                var result = this.ParsePackageFile(packagePath);
				Logger.LogError("ParsePackageFile(package={Package}): result → {Result}", package, result);
                return this.Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting ParsePackageFile(package={Package})", package);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ParseLanguagePackage([FromUri] string cultureCode)
        {
			Logger.LogError(">>> Entering ParseLanguagePackage(cultureCode={CultureCode})", cultureCode);
            try
            {
                DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                const string packageFileName = "installlanguage.resources";
                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);
				Logger.LogError("ParseLanguagePackage(cultureCode={CultureCode}): packagePath → {PackagePath}", cultureCode, packagePath);
                var result = this.ParsePackageFile(packagePath);
				Logger.LogError("ParseLanguagePackage(cultureCode={CultureCode}): result → {Result}", cultureCode, result);
                return this.Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting ParseLanguagePackage(cultureCode={CultureCode})", cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>Install a package that is already included under one of the installation folders.</summary>
        /// <param name="package">The package info.</param>
        /// <returns>An <see cref="InstallResultDto"/> response.</returns>
        [HttpPost]
        [RequireHost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage InstallAvailablePackage(DownloadPackageDto package)
        {
			Logger.LogError(">>> Entering InstallAvailablePackage(package={Package})", package);
            try
            {
                var installFolder = GetPackageInstallFolder(package.PackageType);
				Logger.LogError("InstallAvailablePackage(package={Package}): installFolder → {InstallFolder}", package, installFolder);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(package.FileName))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, package.FileName);
				Logger.LogError("InstallAvailablePackage(package={Package}): packagePath → {PackagePath}", package, packagePath);
                if (!File.Exists(packagePath))
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                var installResult = this.InstallPackageFile(packagePath);
				Logger.LogError("InstallAvailablePackage(package={Package}): installResult → {InstallResult}", package, installResult);
                return this.Request.CreateResponse(installResult.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, installResult);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting InstallAvailablePackage(package={Package})", package);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>Download install package.</summary>
        /// <param name="packageType">The package type.</param>
        /// <param name="fileName">The file name.</param>
        /// <returns>A response that streams the package zip file.</returns>
        [HttpGet]
        [RequireHost]
        public HttpResponseMessage DownloadPackage(string packageType, string fileName)
        {
			Logger.LogError(">>> Entering DownloadPackage(packageType={PackageType},fileName={FileName})", packageType, fileName);
            try
            {
                var installFolder = GetPackageInstallFolder(packageType);
				Logger.LogError("DownloadPackage(packageType={PackageType},fileName={FileName}): installFolder → {InstallFolder}", packageType, fileName, installFolder);
                if (string.IsNullOrEmpty(installFolder) || string.IsNullOrEmpty(fileName))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidPackage");
                }

                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install", installFolder, fileName);
				Logger.LogError("DownloadPackage(packageType={PackageType},fileName={FileName}): packagePath → {PackagePath}", packageType, fileName, packagePath);
                if (!File.Exists(packagePath))
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }

                if (fileName.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                {
                    fileName = fileName.Replace(".resources", ".zip");
					Logger.LogError("DownloadPackage(packageType={PackageType},fileName={FileName}): fileName → {FileName1}", packageType, fileName, fileName);
                }

                var response = this.Request.CreateResponse(HttpStatusCode.OK);
				Logger.LogError("DownloadPackage(packageType={PackageType},fileName={FileName}): response → {Response}", packageType, fileName, response);
                var stream = new FileStream(packagePath, FileMode.Open);
				Logger.LogError("DownloadPackage(packageType={PackageType},fileName={FileName}): stream → {Stream}", packageType, fileName, stream);
                response.Content = new StreamContent(stream);
                response.Content.Headers.ContentLength = stream.Length;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
                return response;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Error = ex.Message });
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage DownloadLanguagePackage([FromUri] string cultureCode)
        {
			Logger.LogError(">>> Entering DownloadLanguagePackage(cultureCode={CultureCode})", cultureCode);
            try
            {
                const string packageFileName = "installlanguage.resources";
                var packagePath = Path.Combine(Globals.ApplicationMapPath, "Install/Language/" + packageFileName);
				Logger.LogError("DownloadLanguagePackage(cultureCode={CultureCode}): packagePath → {PackagePath}", cultureCode, packagePath);

                var parsePackage = this.ParsePackageFile(packagePath);
				Logger.LogError("DownloadLanguagePackage(cultureCode={CultureCode}): parsePackage → {ParsePackage}", cultureCode, parsePackage);
                var invalidPackage = !parsePackage.Success
                                        || !parsePackage.PackageType.Equals("CoreLanguagePack", StringComparison.Ordinal)
                                        || !parsePackage.Name.EndsWith(cultureCode, StringComparison.InvariantCultureIgnoreCase);

                if (invalidPackage)
                {
                    DotNetNuke.Services.Upgrade.Internals.InstallController.Instance.IsAvailableLanguagePack(cultureCode);
                }

                return this.DownLoadFile(packagePath);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting DownloadLanguagePackage(cultureCode={CultureCode})", cultureCode);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetPackageUsageFilter()
        {
			Logger.LogError(">>> Entering GetPackageUsageFilter()");
            try
            {
                var portals = this.UserInfo.IsSuperUser ? PortalController.Instance.GetPortals().OfType<PortalInfo>() : PortalController.Instance.GetPortals().OfType<PortalInfo>().Where(p => p.PortalID == this.PortalId);
				Logger.LogError("GetPackageUsageFilter(): portals → {Portals}", portals);
                var availablePortals = portals.Select(v => new
                {
                    v.PortalID,
                    v.PortalName,
                    IsCurrentPortal = this.PortalId == v.PortalID,
                }).ToList();

                if (this.UserInfo.IsSuperUser)
                {
                    availablePortals.Insert(0, new
                    {
                        PortalID = -2,
                        PortalName = "Host",
                        IsCurrentPortal = false,
                    });
                }

                var response = new
                {
                    Success = true,
                    Results = availablePortals,
                    TotalResults = availablePortals.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetPackageUsageFilter()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetPackageUsage(int portalId, int packageId)
        {
			Logger.LogError(">>> Entering GetPackageUsage(portalId={PortalId},packageId={PackageId})", portalId, packageId);
            try
            {
                var pid = portalId == -2 ? Null.NullInteger : portalId;
                if (!this.UserInfo.IsSuperUser && pid != this.PortalId)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var tabsWithModule = TabController.Instance.GetTabsByPackageID(pid, packageId, false);
				Logger.LogError("GetPackageUsage(portalId={PortalId},packageId={PackageId}): tabsWithModule → {TabsWithModule}", portalId, packageId, tabsWithModule);
                var allPortalTabs = TabController.Instance.GetTabsByPortal(pid);
				Logger.LogError("GetPackageUsage(portalId={PortalId},packageId={PackageId}): allPortalTabs → {AllPortalTabs}", portalId, packageId, allPortalTabs);
                IDictionary<int, TabInfo> tabsInOrder = new Dictionary<int, TabInfo>();
				Logger.LogError("GetPackageUsage(portalId={PortalId},packageId={PackageId}): tabsInOrder → {TabsInOrder}", portalId, packageId, tabsInOrder);

                foreach (var tab in allPortalTabs.Values)
                {
                    AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
                }

                var response = new
                {
					Logger.LogDebug("GetPackageUsage(portalId={PortalId},packageId={PackageId}): Results → {Results}", portalId, packageId, Results);
                    Success = true,
                    Results = tabsInOrder.Select(t => new
                    {
                        TabUrl = this.GetFormattedLink(t.Value),
                    }).ToList(),
                    TotalResults = tabsInOrder.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
				Logger.LogWarning("<<< Exiting GetPackageUsage(portalId={PortalId},packageId={PackageId})", portalId, packageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateExtension(PackageSettingsDto packageSettings)
        {
			Logger.LogError(">>> Entering CreateExtension(packageSettings={PackageSettings})", packageSettings);
            try
            {
                var package = new PackageInfo { PortalID = packageSettings.PortalId, };
                var type = package.GetType();
				Logger.LogError("CreateExtension(packageSettings={PackageSettings}): type → {Type}", packageSettings, type);
                foreach (var kvp in packageSettings.Settings.Where(kpv => kpv.Value != null))
                {
                    var property = type.GetProperty(kvp.Key, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
					Logger.LogError("CreateExtension(packageSettings={PackageSettings}): property → {Property}", packageSettings, property);
                    if (property != null && property.CanWrite)
                    {
                        var value = kvp.Value;
                        var propValue = property.GetValue(package);
						Logger.LogError("CreateExtension(packageSettings={PackageSettings}): propValue → {PropValue}", packageSettings, propValue);
                        if (propValue == null || propValue.ToString() != value)
                        {
                            var nativeValue = property.PropertyType == typeof(Version)
                                ? new Version(value)
                                : Convert.ChangeType(value, property.PropertyType, CultureInfo.InvariantCulture);
                            property.SetValue(package, nativeValue);
                        }
                    }
                }

                var tmpPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name == package.Name);
				Logger.LogError("CreateExtension(packageSettings={PackageSettings}): tmpPackage → {TmpPackage}", packageSettings, tmpPackage);
                if (tmpPackage != null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Error = "DuplicateName" });
                }

                PackageController.Instance.SaveExtensionPackage(package);
                packageSettings.PackageId = package.PackageID;
				Logger.LogDebug("CreateExtension(packageSettings={PackageSettings}): packageSettings.PackageId → {Arg1}", packageSettings, packageSettings.PackageId);

                Locale locale;
                LanguagePackInfo languagePack;
                PackageTypes pkgType;
                Enum.TryParse(package.PackageType, true, out pkgType);

                switch (pkgType)
                {
                    case PackageTypes.AuthSystem:
                        // Create a new Auth System
                        var authSystem = new AuthenticationInfo
                        {
                            AuthenticationType = package.Name,
                            IsEnabled = Null.NullBoolean,
                            PackageID = package.PackageID,
                        };
                        AuthenticationController.AddAuthentication(authSystem);
                        break;
                    case PackageTypes.Container:
                    case PackageTypes.Skin:
                        var skinPackage = new SkinPackageInfo
                        {
                            SkinName = package.Name,
                            PackageID = package.PackageID,
                            SkinType = package.PackageType,
                        };
                        SkinController.AddSkinPackage(skinPackage);
                        break;
                    case PackageTypes.CoreLanguagePack:
                        locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentSettings().DefaultLanguage);
						Logger.LogDebug("CreateExtension(packageSettings={PackageSettings}): locale → {Locale}", packageSettings, locale);
                        languagePack = new LanguagePackInfo
                        {
							Logger.LogError("CreateExtension(packageSettings={PackageSettings}): locale → {Locale}", packageSettings, locale);
                            PackageID = package.PackageID,
                            LanguageID = locale.LanguageId,
                            DependentPackageID = -2,
                        };
						Logger.LogDebug("CreateExtension(packageSettings={PackageSettings}): languagePack → {LanguagePack}", packageSettings, languagePack);
                        LanguagePackController.SaveLanguagePack(languagePack);
                        break;
                    case PackageTypes.ExtensionLanguagePack:
                        locale = LocaleController.Instance.GetLocale(PortalController.Instance.GetCurrentSettings().DefaultLanguage);
                        languagePack = new LanguagePackInfo
                        {
                            PackageID = package.PackageID,
                            LanguageID = locale.LanguageId,
                            DependentPackageID = Null.NullInteger,
                        };
                        LanguagePackController.SaveLanguagePack(languagePack);
                        break;
                    case PackageTypes.Module:
                        // Create a new DesktopModule
                        var desktopModule = new DesktopModuleInfo
                        {
                            PackageID = package.PackageID,
                            ModuleName = package.Name,
                            FriendlyName = package.FriendlyName,
                            FolderName = package.Name,
                            Description = package.Description,
                            Version = package.Version.ToString(3),
                            SupportedFeatures = 0,
                        };
                        var desktopModuleId = DesktopModuleController.SaveDesktopModule(desktopModule, false, true);
						Logger.LogError("CreateExtension(packageSettings={PackageSettings}): desktopModuleId → {DesktopModuleId}", packageSettings, desktopModuleId);
                        if (desktopModuleId > Null.NullInteger)
                        {
                            DesktopModuleController.AddDesktopModuleToPortals(desktopModuleId);
                        }

                        break;
                    case PackageTypes.SkinObject:
                        var skinControl = new SkinControlInfo { PackageID = package.PackageID, ControlKey = package.Name };
                        SkinControlController.SaveSkinControl(skinControl);
                        break;
                }

                var packageEditor = PackageEditorFactory.GetPackageEditor(package.PackageType);
				Logger.LogError("CreateExtension(packageSettings={PackageSettings}): packageEditor → {PackageEditor}", packageSettings, packageEditor);
                if (packageEditor != null)
                {
                    string error;
                    packageEditor.SavePackageSettings(packageSettings, out error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { Success = false, Error = error });
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, PackageId = package.PackageID });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting CreateExtension(packageSettings={PackageSettings})", packageSettings);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetOwnerFolders()
        {
			Logger.LogError(">>> Entering GetOwnerFolders()");
            try
            {
                var folders = new List<string>();
				Logger.LogError("GetOwnerFolders(): folders → {Folders}", folders);
                foreach (var folder in GetRootModuleDefinitionFolders())
                {
                    var files = Directory.GetFiles(folder.Path, "*.ascx");
					Logger.LogError("GetOwnerFolders(): files → {Files}", files);

                    // exclude module folders
                    if (files.Length == 0 || folder.Path.Equals("admin", StringComparison.OrdinalIgnoreCase))
                    {
                        var path = GetFolderPath(folder);
						Logger.LogError("GetOwnerFolders(): path → {Path}", path);
                        folders.Add(path);
                    }
					Logger.LogError(">>> Entering GetRootModuleDefinitionFolders()");
					Logger.LogWarning("<<< Exiting GetRootModuleDefinitionFolders()");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, folders);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetOwnerFolders()");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleFolders(string ownerFolder)
        {
			Logger.LogError(">>> Entering GetModuleFolders(ownerFolder={OwnerFolder})", ownerFolder);
            if (!string.IsNullOrEmpty(ownerFolder) &&
                (ownerFolder.Replace(@"\", "/").Contains("/", StringComparison.Ordinal)
                 || ownerFolder.StartsWith(".", StringComparison.Ordinal)))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var folders = new List<string>();
				Logger.LogError("GetModuleFolders(ownerFolder={OwnerFolder}): folders → {Folders}", ownerFolder, folders);
                foreach (var moduleFolder in GetModulesFolders(ownerFolder))
                {
                    var path = GetFolderPath(moduleFolder);
					Logger.LogError("GetModuleFolders(ownerFolder={OwnerFolder}): path → {Path}", ownerFolder, path);
                    folders.Add(path);
					Logger.LogError(">>> Entering GetModulesFolders(ownerFolder={OwnerFolder})", ownerFolder);
					Logger.LogWarning("<<< Exiting GetModulesFolders(ownerFolder={OwnerFolder})", ownerFolder);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, folders);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetModuleFolders(ownerFolder={OwnerFolder})", ownerFolder);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetModuleFiles(string ownerFolder, string moduleFolder, FileType type)
        {
			Logger.LogError(">>> Entering GetModuleFiles(ownerFolder={OwnerFolder},moduleFolder={ModuleFolder},type={Type})", ownerFolder, moduleFolder, type);
            if ((!string.IsNullOrEmpty(ownerFolder) && (ownerFolder.Replace(@"\", "/").Contains("/", StringComparison.Ordinal) || ownerFolder.StartsWith(".", StringComparison.Ordinal))) || string.IsNullOrEmpty(moduleFolder) || moduleFolder.Replace(@"\", "/").Contains("/", StringComparison.Ordinal) || moduleFolder.StartsWith(".", StringComparison.Ordinal))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var files = new List<string>();
				Logger.LogError("GetModuleFiles(ownerFolder={OwnerFolder},moduleFolder={ModuleFolder},type={Type}): files → {Files}", ownerFolder, moduleFolder, type, files);
                var folder = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", ownerFolder ?? string.Empty, moduleFolder);
				Logger.LogError("GetModuleFiles(ownerFolder={OwnerFolder},moduleFolder={ModuleFolder},type={Type}): folder → {Folder}", ownerFolder, moduleFolder, type, folder);
                switch (type)
                {
                    case FileType.Control:
                        files.AddRange(GetFiles(folder, "*.ascx"));
                        files.AddRange(GetFiles(folder, "*.html"));
                        files.AddRange(GetFiles(folder, "*.htm"));
                        files.AddRange(GetFiles(folder, "*.cshtml"));
                        files.AddRange(GetFiles(folder, "*.vbhtml"));
                        break;
                    case FileType.Template:
                        files.AddRange(GetFiles(Globals.HostMapPath + "Templates\\", ".module.template"));
                        break;
                    case FileType.Manifest:
                        files.AddRange(GetFiles(folder, "*.dnn*").Where(file => ManifestExensionsRegex.IsMatch(file)));
                        break;
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, files);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateFolder([FromUri] string ownerFolder, [FromUri] string moduleFolder)
        {
			Logger.LogError(">>> Entering CreateFolder(ownerFolder={OwnerFolder},moduleFolder={ModuleFolder})", ownerFolder, moduleFolder);
            if ((!string.IsNullOrEmpty(ownerFolder) && (ownerFolder.Replace(@"\", "/").Contains("/", StringComparison.Ordinal) || ownerFolder.StartsWith(".", StringComparison.Ordinal)))
                || (!string.IsNullOrEmpty(moduleFolder) && (moduleFolder.Replace(@"\", "/").Contains("/", StringComparison.Ordinal) || moduleFolder.StartsWith(".", StringComparison.Ordinal))))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var parentFolderPath = $@"{Globals.ApplicationMapPath}\DesktopModules";
				Logger.LogError("CreateFolder(ownerFolder={OwnerFolder},moduleFolder={ModuleFolder}): parentFolderPath → {ParentFolderPath}", ownerFolder, moduleFolder, parentFolderPath);
                if (!string.IsNullOrEmpty(ownerFolder))
                {
                    parentFolderPath += @"\" + ownerFolder;

                    if (!Directory.Exists(parentFolderPath))
                    {
                        Directory.CreateDirectory(parentFolderPath);
                    }
                }

                if (!string.IsNullOrEmpty(moduleFolder))
                {
                    parentFolderPath += @"\" + moduleFolder;

                    if (!Directory.Exists(parentFolderPath))
                    {
                        Directory.CreateDirectory(parentFolderPath);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateModule(CreateModuleDto createModuleDto)
        {
			Logger.LogError(">>> Entering CreateModule(createModuleDto={CreateModuleDto})", createModuleDto);
            try
            {
                string errorMessage;
                string newPageUrl;

                var packageId = CreateModuleController.Instance.CreateModule(createModuleDto, out newPageUrl, out errorMessage);
				Logger.LogError("CreateModule(createModuleDto={CreateModuleDto}): packageId → {PackageId}", createModuleDto, packageId);
                var newPackage = packageId > Null.NullInteger
                    ? PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId)
                    : null;

                var result = new
                {
                    Success = packageId > Null.NullInteger,
                    PackageInfo = newPackage != null ? new PackageInfoDto(Null.NullInteger, newPackage) : null,
                    NewPageUrl = newPageUrl,
                    Error = errorMessage,
                };

                return this.Request.CreateResponse(result.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest, result);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting CreateModule(createModuleDto={CreateModuleDto})", createModuleDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [RequireHost]
        public HttpResponseMessage GetPackageManifest(int packageId)
        {
			Logger.LogError(">>> Entering GetPackageManifest(packageId={PackageId})", packageId);
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageId);
				Logger.LogError("GetPackageManifest(packageId={PackageId}): package → {Package}", packageId, package);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "PackageNotFound");
                }

                switch (package.PackageType.ToLowerInvariant())
                {
                    case "corelanguagepack":
                        package.IconFile = "N\\A";
						Logger.LogDebug("GetPackageManifest(packageId={PackageId}): package.IconFile → {Arg1}", packageId, package.IconFile);
                        break;
                    default:
                        package.IconFile = DotNetNuke.Services.Installer.Util.ParsePackageIconFileName(package);
                        break;
                }

                var packageManifestDto = new PackageManifestDto(Null.NullInteger, package);
				Logger.LogError("GetPackageManifest(packageId={PackageId}): packageManifestDto → {PackageManifestDto}", packageId, packageManifestDto);

                var writer = PackageWriterFactory.GetWriter(package);
				Logger.LogError("GetPackageManifest(packageId={PackageId}): writer → {Writer}", packageId, writer);

                packageManifestDto.BasePath = writer.BasePath;
				Logger.LogDebug("GetPackageManifest(packageId={PackageId}): packageManifestDto.BasePath → {Arg1}", packageId, packageManifestDto.BasePath);

                // Load Manifests
                if (!string.IsNullOrEmpty(package.Manifest))
                {
                    // Use Database
                    var sb = new StringBuilder();
					Logger.LogError("GetPackageManifest(packageId={PackageId}): sb → {Sb}", packageId, sb);
                    var settings = new XmlWriterSettings();
					Logger.LogError("GetPackageManifest(packageId={PackageId}): settings → {Settings}", packageId, settings);
					Logger.LogDebug("GetPackageManifest(packageId={PackageId}): settings.OmitXmlDeclaration → {Arg1}", packageId, settings.OmitXmlDeclaration);
					Logger.LogDebug("GetPackageManifest(packageId={PackageId}): settings.Indent → {Arg1}", packageId, settings.Indent);
                    settings.ConformanceLevel = ConformanceLevel.Fragment;
					Logger.LogDebug("GetPackageManifest(packageId={PackageId}): settings.ConformanceLevel → {Arg1}", packageId, settings.ConformanceLevel);
                    settings.OmitXmlDeclaration = true;
                    settings.Indent = true;

                    writer.WriteManifest(XmlWriter.Create(sb, settings), package.Manifest);

                    packageManifestDto.Manifests.Add("Database version", sb.ToString());
                }

                var filePath = Path.Combine(Globals.ApplicationMapPath, writer.BasePath);
				Logger.LogError("GetPackageManifest(packageId={PackageId}): filePath → {FilePath}", packageId, filePath);
                if (!string.IsNullOrEmpty(filePath))
                {
                    if (Directory.Exists(filePath))
                    {
                        foreach (var file in Directory.GetFiles(filePath, "*.dnn"))
                        {
                            var fileName = file.Replace(filePath + @"\", string.Empty);
                            packageManifestDto.Manifests.Add(fileName, GetFileContent(writer.BasePath, fileName));
                        }

                        foreach (var file in Directory.GetFiles(filePath, "*.dnn.resources"))
                        {
                            var fileName = file.Replace(filePath + @"\", string.Empty);
                            packageManifestDto.Manifests.Add(fileName, GetFileContent(writer.BasePath, fileName));
                        }
                    }
                }

                // get assemblies
                foreach (var file in writer.Assemblies.Values)
                {
                    packageManifestDto.Assemblies.Add(file.FullName);
                }

                // get files
                writer.GetFiles(true);

                // Display App Code files
                foreach (var file in writer.AppCodeFiles.Values)
                {
                    packageManifestDto.Files.Add("[app_code]" + file.FullName);
                }

                // Display Script files
                foreach (var file in writer.Scripts.Values)
                {
                    packageManifestDto.Files.Add(file.FullName);
                }

                // Display regular files
                foreach (var file in writer.Files.Values)
                {
                    if (file.Path.StartsWith(".git", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!file.Name.StartsWith(".git", StringComparison.OrdinalIgnoreCase))
                    {
                        packageManifestDto.Files.Add(file.FullName);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, packageManifestDto);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting GetPackageManifest(packageId={PackageId})", packageId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateManifest(PackageManifestDto packageManifestDto)
        {
			Logger.LogError(">>> Entering CreateManifest(packageManifestDto={PackageManifestDto})", packageManifestDto);
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(
                    Null.NullInteger, p => p.PackageID == packageManifestDto.PackageId);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "PackageNotFound");
                }

                return this.CreateManifestInternal(package, packageManifestDto);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting CreateManifest(packageManifestDto={PackageManifestDto})", packageManifestDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreateNewManifest(PackageManifestDto packageManifestDto)
        {
			Logger.LogError(">>> Entering CreateNewManifest(packageManifestDto={PackageManifestDto})", packageManifestDto);
            try
            {
                var package = packageManifestDto.ToPackageInfo();
				Logger.LogError("CreateNewManifest(packageManifestDto={PackageManifestDto}): package → {Package}", packageManifestDto, package);
                return this.CreateManifestInternal(package, packageManifestDto);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting CreateNewManifest(packageManifestDto={PackageManifestDto})", packageManifestDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage CreatePackage(PackageManifestDto packageManifestDto)
        {
			Logger.LogError(">>> Entering CreatePackage(packageManifestDto={PackageManifestDto})", packageManifestDto);
            try
            {
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == packageManifestDto.PackageId);
				Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): package → {Package}", packageManifestDto, package);
                if (package == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "PackageNotFound");
                }

                // Save package manifest
                if (packageManifestDto.Manifests.Any())
                {
                    XPathDocument doc;
                    using (var manifestReader = XmlReader.Create(new StringReader(packageManifestDto.Manifests.Values.FirstOrDefault())))
                    {
                        doc = new XPathDocument(manifestReader);
                    }

                    XPathNavigator nav = doc.CreateNavigator();
					Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): nav → {Nav}", packageManifestDto, nav);
					Logger.LogDebug("CreatePackage(packageManifestDto={PackageManifestDto}): package.Manifest → {Arg1}", packageManifestDto, package.Manifest);
                    XPathNavigator packageNav = nav.SelectSingleNode("dotnetnuke/packages");
					Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): packageNav → {PackageNav}", packageManifestDto, packageNav);
                    package.Manifest = packageNav.InnerXml;
                    var pkgIconFile = DotNetNuke.Services.Installer.Util.ParsePackageIconFileName(package);
					Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): pkgIconFile → {PkgIconFile}", packageManifestDto, pkgIconFile);
                    package.IconFile = (pkgIconFile.Trim().Length > 0) ? DotNetNuke.Services.Installer.Util.ParsePackageIconFile(package) : null;

                    PackageController.Instance.SaveExtensionPackage(package);
                }

                var writer = PackageWriterFactory.GetWriter(package);
				Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): writer → {Writer}", packageManifestDto, writer);

                var manifestName = packageManifestDto.ManifestName;
				Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): manifestName → {ManifestName}", packageManifestDto, manifestName);
                if (string.IsNullOrEmpty(manifestName))
                {
                    manifestName = packageManifestDto.ArchiveName.ToLowerInvariant().Replace("zip", "dnn");
                }

                // Use the installer to parse the manifest and load the files that need to be packaged
                var installer = new Installer(package, Globals.ApplicationMapPath);
				Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): installer → {Installer}", packageManifestDto, installer);
                foreach (var file in installer.InstallerInfo.Files.Values)
                {
                    writer.AddFile(file);
                }

                string basePath;
                switch (package.PackageType.ToLowerInvariant())
                {
                    case "auth_system":
                        basePath = Globals.InstallMapPath + "AuthSystem";
						Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): basePath → {BasePath}", packageManifestDto, basePath);
						Logger.LogDebug("CreatePackage(packageManifestDto={PackageManifestDto}): basePath → {BasePath}", packageManifestDto, basePath);
                        break;
                    case "container":
                        basePath = Globals.InstallMapPath + "Container";
                        break;
                    case "corelanguagepack":
                    case "extensionlanguagepack":
                        basePath = Globals.InstallMapPath + "Language";
                        break;
                    case "module":
                        basePath = Globals.InstallMapPath + "Module";
                        break;
                    case "provider":
                        basePath = Globals.InstallMapPath + "Provider";
                        break;
                    case "skin":
                        basePath = Globals.InstallMapPath + "Skin";
                        break;
                    default:
                        basePath = Globals.HostMapPath;
                        break;
                }

                if (!manifestName.EndsWith(".dnn", StringComparison.Ordinal))
                {
                    manifestName += ".dnn";
                }

                if (!packageManifestDto.ArchiveName.EndsWith(".zip", StringComparison.Ordinal))
                {
                    packageManifestDto.ArchiveName += ".zip";
                }

                writer.CreatePackage(Path.Combine(basePath, packageManifestDto.ArchiveName), manifestName, package.Manifest, true);

                var logs = writer.Log.Logs.Select(l => l.ToString()).ToList();
				Logger.LogError("CreatePackage(packageManifestDto={PackageManifestDto}): logs → {Logs}", packageManifestDto, logs);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Logs = logs });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting CreatePackage(packageManifestDto={PackageManifestDto})", packageManifestDto);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage RefreshPackageFiles(PackageFilesQueryDto packageData)
        {
			Logger.LogError(">>> Entering RefreshPackageFiles(packageData={PackageData})", packageData);
            var baseFolder = Path.Combine(Globals.ApplicationMapPath, packageData.PackageFolder.Replace('/', '\\'));
			Logger.LogError("RefreshPackageFiles(packageData={PackageData}): baseFolder → {BaseFolder}", packageData, baseFolder);
            if (!Directory.Exists(baseFolder))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidFolder");
            }

            try
            {
                var package = packageData.ToPackageInfo();
				Logger.LogError("RefreshPackageFiles(packageData={PackageData}): package → {Package}", packageData, package);
                var writer = PackageWriterFactory.GetWriter(package);
				Logger.LogError("RefreshPackageFiles(packageData={PackageData}): writer → {Writer}", packageData, writer);
                writer.BasePath = packageData.PackageFolder;
				Logger.LogDebug("RefreshPackageFiles(packageData={PackageData}): writer.BasePath → {Arg1}", packageData, writer.BasePath);
                writer.GetFiles(packageData.IncludeSource);

                var files = new List<string>();
				Logger.LogError("RefreshPackageFiles(packageData={PackageData}): files → {Files}", packageData, files);
                if (packageData.IncludeAppCode)
                {
                    // add app-code files
                    files.AddRange(writer.AppCodeFiles.Values.Select(file => "[app_code]" + file.FullName));
                }

                // add script files
                files.AddRange(writer.Scripts.Values.Select(f => f.FullName));

                // add code files
                files.AddRange(writer.Files.Values.Where(
                    f => !f.Path.StartsWith(".", StringComparison.Ordinal) && !f.Name.StartsWith(".", StringComparison.Ordinal)).Select(file => file.FullName));

                return this.Request.CreateResponse(HttpStatusCode.OK, files);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
				Logger.LogWarning("<<< Exiting RefreshPackageFiles(packageData={PackageData})", packageData);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        protected string GetFormattedLink(object dataItem)
        {
			Logger.LogError(">>> Entering GetFormattedLink(dataItem={DataItem})", dataItem);
            var returnValue = new StringBuilder();
			Logger.LogError("GetFormattedLink(dataItem={DataItem}): returnValue → {ReturnValue}", dataItem, returnValue);
            if (dataItem is TabInfo)
            {
                var tab = (TabInfo)dataItem;
                {
                    var index = 0;
                    TabController.Instance.PopulateBreadCrumbs(ref tab);
                    foreach (TabInfo t in tab.BreadCrumbs)
                    {
                        if (index > 0)
                        {
                            returnValue.Append(" > ");
                        }

                        if (tab.BreadCrumbs.Count - 1 == index)
                        {
                            var url = Globals.AddHTTP(t.PortalID == Null.NullInteger ? this.PortalSettings.PortalAlias.HTTPAlias : PortalAliasController.Instance.GetPortalAliasesByPortalId(t.PortalID).ToList().OrderByDescending(a => a.IsPrimary).FirstOrDefault().HTTPAlias) + "/Default.aspx?tabId=" + t.TabID;
							Logger.LogError("GetFormattedLink(dataItem={DataItem}): url → {Url}", dataItem, url);
                            returnValue.AppendFormat(CultureInfo.InvariantCulture, "<a target=\"_blank\" href=\"{0}\">{1}</a>", url, t.LocalizedTabName);
                        }
                        else
                        {
                            returnValue.Append(t.LocalizedTabName);
                        }

                        index = index + 1;
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetFormattedLink(dataItem={DataItem})", dataItem);
            return returnValue.ToString();
        }

        private static void AddFiles(List<KeyValuePair<string, string>> collection, string path, string root, string filter)
        {
			Logger.LogError(">>> Entering AddFiles(collection={Collection},path={Path},root={Root},filter={Filter})", collection, path, root, filter);
            var files = Directory.GetFiles(path, filter);
			Logger.LogError("AddFiles(collection={Collection},path={Path},root={Root},filter={Filter}): files → {Files}", collection, path, root, filter, files);
            foreach (var strFile in files)
            {
                var file = root.Replace('\\', '/') + "/" + Path.GetFileName(strFile);
				Logger.LogError("AddFiles(collection={Collection},path={Path},root={Root},filter={Filter}): file → {File}", collection, path, root, filter, file);
                var item = new KeyValuePair<string, string>(file.ToLowerInvariant(), file);
				Logger.LogError("AddFiles(collection={Collection},path={Path},root={Root},filter={Filter}): item → {Item}", collection, path, root, filter, item);
                collection.Add(item);
            }
			Logger.LogWarning("<<< Exiting AddFiles(collection={Collection},path={Path},root={Root},filter={Filter})", collection, path, root, filter);
        }

        private static void AddChildTabsToList(TabInfo currentTab, ref TabCollection allPortalTabs, ref IDictionary<int, TabInfo> tabsWithModule, ref IDictionary<int, TabInfo> tabsInOrder)
        {
			Logger.LogError(">>> Entering AddChildTabsToList(currentTab={CurrentTab},allPortalTabs={AllPortalTabs},tabsWithModule={TabsWithModule},tabsInOrder={TabsInOrder})", currentTab, allPortalTabs, tabsWithModule, tabsInOrder);
            if (!tabsWithModule.ContainsKey(currentTab.TabID) || tabsInOrder.ContainsKey(currentTab.TabID))
            {
                return;
            }

            tabsInOrder.Add(currentTab.TabID, currentTab);
            foreach (var tab in allPortalTabs.WithParentId(currentTab.TabID))
            {
                AddChildTabsToList(tab, ref allPortalTabs, ref tabsWithModule, ref tabsInOrder);
            }
			Logger.LogWarning("<<< Exiting AddChildTabsToList(currentTab={CurrentTab},allPortalTabs={AllPortalTabs},tabsWithModule={TabsWithModule},tabsInOrder={TabsInOrder})", currentTab, allPortalTabs, tabsWithModule, tabsInOrder);
        }

        private static string GetFolderPath(ModuleFolderDto folder)
        {
			Logger.LogError(">>> Entering GetFolderPath(folder={Folder})", folder);
            var path = folder.Path.Replace(Path.GetDirectoryName(folder.Path) + @"\", string.Empty);
            if (folder.IsSpecial)
            {
                path = folder.SpecialType + @"\" + path;
				Logger.LogError("GetFolderPath(folder={Folder}): path → {Path}", folder, path);
            Logger.LogWarning("<<< Exiting GetFolderPath(folder={Folder})", folder);
            }

            return path;
        }

        private static List<ModuleFolderDto> GetRootModuleDefinitionFolders()
        {
            var moduleFolders = new List<ModuleFolderDto>();
            var rootFolders = Directory.GetDirectories($@"{Globals.ApplicationMapPath}\DesktopModules\").ToList();

            foreach (var folderPath in rootFolders)
            {
				Logger.LogError("GetRootModuleDefinitionFolders(): moduleFolders → {ModuleFolders}", moduleFolders);
                var folderName = folderPath.Replace($@"{Path.GetDirectoryName(folderPath)}\", string.Empty);
                if (IsSpecialFolder(folderName))
                {
					Logger.LogError("GetRootModuleDefinitionFolders(): rootFolders → {RootFolders}", rootFolders);
                    Directory.GetDirectories(folderPath).ToList()
                        .ForEach(specialFolderChild =>
                            moduleFolders.Add(new ModuleFolderDto
                            {
								Logger.LogError("GetRootModuleDefinitionFolders(): folderName → {FolderName}", folderName);
                                Path = specialFolderChild,
                                IsSpecial = true,
                                SpecialType = folderName,
                            }));
                }
                else
                {
                    moduleFolders.Add(new ModuleFolderDto
                    {
                        Path = folderPath,
                        IsSpecial = false,
                    });
                }
            }

            return moduleFolders;
        }

        private static bool IsSpecialFolder(string folderName)
        {
			Logger.LogError(">>> Entering IsSpecialFolder(folderName={FolderName})", folderName);
			Logger.LogWarning("<<< Exiting IsSpecialFolder(folderName={FolderName})", folderName);
            return SpecialModuleFolders.Any(specialFolder => specialFolder.Equals(folderName, StringComparison.OrdinalIgnoreCase));
        }

        private static List<ModuleFolderDto> GetModulesFolders(string ownerFolder)
        {
			Logger.LogError(">>> Entering GetModulesFolders(ownerFolder={OwnerFolder})", ownerFolder);
            if (!string.IsNullOrEmpty(ownerFolder))
            {
                return Directory.GetDirectories($@"{Globals.ApplicationMapPath}\DesktopModules\{ownerFolder}")
                    .Select(folder => new ModuleFolderDto { Path = folder, IsSpecial = false, })
                    .ToList();
            }

			Logger.LogWarning("<<< Exiting GetModulesFolders(ownerFolder={OwnerFolder})", ownerFolder);
            return GetRootModuleDefinitionFolders();
        }

        private static List<string> GetFiles(string rootFolder, string extension)
        {
			Logger.LogError(">>> Entering GetFiles(rootFolder={RootFolder},extension={Extension})", rootFolder, extension);
            Logger.LogWarning("<<< Exiting GetFiles(rootFolder={RootFolder},extension={Extension})", rootFolder, extension);
            return Directory.GetFiles(rootFolder, extension).Select(Path.GetFileName).ToList();
        }

        private static string GetPackageInstallFolder(string packageType)
        {
			Logger.LogError(">>> Entering GetPackageInstallFolder(packageType={PackageType})", packageType);
            switch ((packageType ?? string.Empty).ToLowerInvariant())
            {
                case "authsystem":
                case "auth_system":
                    return "AuthSystem";
                case "corelanguagepack":
                case "extensionlanguagepack":
                    return "Language";
                case "javascriptlibrary":
                case "javascript_library":
                    return "JavaScriptLibrary";
                case "module":
                case "skin":
                case "container":
                case "provider":
                case "library":
                    return packageType;
                default:
					Logger.LogWarning("<<< Exiting GetPackageInstallFolder(packageType={PackageType})", packageType);
                    return string.Empty;
            }
        }

        private static string GetFileContent(string basePath, string fileName)
        {
			Logger.LogError(">>> Entering GetFileContent(basePath={BasePath},fileName={FileName})", basePath, fileName);
            var filename = Path.Combine(Globals.ApplicationMapPath, basePath, fileName);
            using var objStreamReader = File.OpenText(filename);
			Logger.LogError("GetFileContent(basePath={BasePath},fileName={FileName}): objStreamReader → {ObjStreamReader}", basePath, fileName, objStreamReader);
			Logger.LogError("GetFileContent(basePath={BasePath},fileName={FileName}): filename → {Filename}", basePath, fileName, filename);
			Logger.LogWarning("<<< Exiting GetFileContent(basePath={BasePath},fileName={FileName})", basePath, fileName);
            return objStreamReader.ReadToEnd();
        }

        private HttpResponseMessage CreateManifestInternal(PackageInfo package, PackageManifestDto packageManifestDto)
        {
			Logger.LogError(">>> Entering CreateManifestInternal(package={Package},packageManifestDto={PackageManifestDto})", package, packageManifestDto);
            var writer = PackageWriterFactory.GetWriter(package);

			Logger.LogError("CreateManifestInternal(package={Package},packageManifestDto={PackageManifestDto}): writer → {Writer}", package, packageManifestDto, writer);
            foreach (var fileName in packageManifestDto.Files)
            {
                var name = fileName.Trim();
				Logger.LogError("CreateManifestInternal(package={Package},packageManifestDto={PackageManifestDto}): name → {Name}", package, packageManifestDto, name);
                if (!string.IsNullOrEmpty(name))
                {
                    writer.AddFile(new InstallFile(name));
                }
            }

            foreach (var fileName in packageManifestDto.Assemblies)
            {
                var name = fileName.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    writer.AddFile(new InstallFile(name));
                }
            }

            string manifestContent;
            if (!string.IsNullOrEmpty(packageManifestDto.ManifestName))
            {
                writer.WriteManifest(packageManifestDto.ManifestName, package.Manifest);
                manifestContent = package.Manifest;
				Logger.LogError("CreateManifestInternal(package={Package},packageManifestDto={PackageManifestDto}): manifestContent → {ManifestContent}", package, packageManifestDto, manifestContent);
            }
            else
            {
                manifestContent = writer.WriteManifest(false);
				Logger.LogDebug("CreateManifestInternal(package={Package},packageManifestDto={PackageManifestDto}): manifestContent → {ManifestContent}", package, packageManifestDto, manifestContent);
            }

			Logger.LogWarning("<<< Exiting CreateManifestInternal(package={Package},packageManifestDto={PackageManifestDto})", package, packageManifestDto);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Content = manifestContent });
        }

        private Task<HttpResponseMessage> UploadFileAction(Func<PortalSettings, UserInfo, string, Stream, object> action)
        {
			Logger.LogError(">>> Entering UploadFileAction(action={Action})", action);
            var request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
				Logger.LogError("Throwing HttpResponseException from UploadFileAction(action={Action})", action);
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();
			Logger.LogError("UploadFileAction(action={Action}): provider → {Provider}", action, provider);

            // local references for use in closure
            var portalSettings = this.PortalSettings;
            var currentSynchronizationContext = SynchronizationContext.Current;
            var userInfo = this.UserInfo;

            var task = request.Content.ReadAsMultipartAsync(provider)
                .ContinueWith(o =>
                {
                    object result = null;
					Logger.LogError("UploadFileAction(action={Action}): result → {Result}", action, result);

                    var fileName = string.Empty;
					Logger.LogError("UploadFileAction(action={Action}): fileName → {FileName}", action, fileName);
                    Stream stream = null;
					Logger.LogError("UploadFileAction(action={Action}): stream → {Stream}", action, stream);

                    foreach (var item in provider.Contents)
                    {
                        var name = item.Headers.ContentDisposition.Name;
						Logger.LogError("UploadFileAction(action={Action}): name → {Name}", action, name);
                        switch (name.ToUpperInvariant())
                        {
                            case "\"POSTFILE\"":
                                fileName = item.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                                if (fileName.IndexOf(@"\", StringComparison.Ordinal) != -1)
                                {
                                    fileName = Path.GetFileName(fileName);
                                }

                                if (!Globals.FileEscapingRegex.IsMatch(fileName))
                                {
                                    stream = item.ReadAsStreamAsync().Result;
                                }

                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(fileName) && stream != null)
                    {
                        // The SynchronizationContext keeps the main thread context. Send method is synchronous
                        currentSynchronizationContext.Send(
                            state =>
                            {
                                result = action(portalSettings, userInfo, fileName, stream);
                            },
                            null);
                    }

                    var mediaTypeFormatter = new JsonMediaTypeFormatter();
					Logger.LogError("UploadFileAction(action={Action}): mediaTypeFormatter → {MediaTypeFormatter}", action, mediaTypeFormatter);
                    mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                    // Response Content Type cannot be application/json
                    // because IE9 with iframe-transport manages the response
                    // as a file download
                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        result,
                        mediaTypeFormatter,
                        "text/plain");
                });

			Logger.LogWarning("<<< Exiting UploadFileAction(action={Action})", action);
            return task;
        }

        private ParseResultDto ParsePackageFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new ParseResultDto() { Success = false, Message = "FileNotFound" };
            }

            using var stream = new FileStream(filePath, FileMode.Open);
            return InstallController.Instance.ParsePackage(this.PortalSettings, this.UserInfo, filePath, stream);
        }

        private InstallResultDto InstallPackageFile(string filePath)
        {
			Logger.LogError(">>> Entering InstallPackageFile(filePath={FilePath})", filePath);
            if (!File.Exists(filePath))
            {
                return new InstallResultDto() { Success = false, Message = "FileNotFound" };
            }

            using var stream = new FileStream(filePath, FileMode.Open);
			Logger.LogError("InstallPackageFile(filePath={FilePath}): stream → {Stream}", filePath, stream);
            return InstallController.Instance.InstallPackage(this.PortalSettings, this.UserInfo, null, filePath, stream);
        }

        private HttpResponseMessage DownLoadFile(string packagePath)
        {
			Logger.LogError(">>> Entering DownLoadFile(packagePath={PackagePath})", packagePath);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            if (!File.Exists(packagePath))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "FileNotFound");
            }

            var stream = FileWrapper.Instance.OpenRead(packagePath);
			Logger.LogError("DownLoadFile(packagePath={PackagePath}): stream → {Stream}", packagePath, stream);
            var fileName = Path.GetFileNameWithoutExtension(packagePath) + ".zip";
			Logger.LogError("DownLoadFile(packagePath={PackagePath}): fileName → {FileName}", packagePath, fileName);

            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(stream) };
			Logger.LogError("DownLoadFile(packagePath={PackagePath}): result → {Result}", packagePath, result);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = fileName;
            return result;
        }
    }
}
