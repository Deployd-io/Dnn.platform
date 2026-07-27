// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using Microsoft.Extensions.DependencyInjection;

    public class CreateModuleController : ServiceLocator<ICreateModuleController, CreateModuleController>, ICreateModuleController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="CreateModuleController"/> class.</summary>
        public CreateModuleController()
        {
			Logger.LogDebug(">>> Entering CreateModuleController()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("<<< Exiting CreateModuleController()");
        }

        protected INavigationManager NavigationManager { get; }

        /// <summary>create new module.</summary>
        /// <param name="createModuleDto">Information about the module to create.</param>
        /// <param name="newPageUrl">The new page URL or <see cref="string.Empty"/>.</param>
        /// <param name="errorMessage">The error message or <see cref="string.Empty"/>.</param>
        /// <returns>return the new package id.</returns>
        public int CreateModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering CreateModule(createModuleDto={CreateModuleDto})", createModuleDto);
            errorMessage = string.Empty;
            newPageUrl = string.Empty;
			Logger.LogDebug("CreateModule(createModuleDto={CreateModuleDto}): newPageUrl → {NewPageUrl}", createModuleDto, newPageUrl);
			Logger.LogDebug("CreateModule(createModuleDto={CreateModuleDto}): errorMessage → {ErrorMessage}", createModuleDto, errorMessage);
            var packageId = Null.NullInteger;
            switch (createModuleDto.Type)
			Logger.LogDebug("CreateModule(createModuleDto={CreateModuleDto}): packageId → {PackageId}", createModuleDto, packageId);
            {
                case CreateModuleType.New:
                    packageId = this.CreateNewModule(createModuleDto, out newPageUrl, out errorMessage);
                    break;
                case CreateModuleType.Control:
                    packageId = this.CreateModuleFromControl(createModuleDto, out newPageUrl, out errorMessage);
                    break;
                case CreateModuleType.Manifest:
                    packageId = this.CreateModuleFromManifest(createModuleDto, out newPageUrl, out errorMessage);
                    break;
            }

			Logger.LogDebug("<<< Exiting CreateModule(createModuleDto={CreateModuleDto})", createModuleDto);
            return packageId;
        }

        /// <inheritdoc />
        protected override Func<ICreateModuleController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new CreateModuleController();
        }

        private static bool InvalidFilename(string fileName)
        {
			Logger.LogDebug(">>> Entering InvalidFilename(fileName={FileName})", fileName);
            var invalidFilenameChars = RegexUtils.GetCachedRegex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
			Logger.LogDebug("InvalidFilename(fileName={FileName}): invalidFilenameChars → {InvalidFilenameChars}", fileName, invalidFilenameChars);
			Logger.LogDebug("<<< Exiting InvalidFilename(fileName={FileName})", fileName);
            return invalidFilenameChars.IsMatch(fileName);
        }

        private static string CreateControl(CreateModuleDto createModuleDto)
        {
			Logger.LogDebug(">>> Entering CreateControl(createModuleDto={CreateModuleDto})", createModuleDto);
            var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
            var className = GetClassName(createModuleDto);
			Logger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): folder → {Folder}", createModuleDto, folder);
			Logger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): className → {ClassName}", createModuleDto, className);
            var moduleControlPath = Path.Combine(Globals.ApplicationMapPath, $"DesktopModules/{folder}/{createModuleDto.FileName}");
            var message = Null.NullString;
			Logger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): moduleControlPath → {ModuleControlPath}", createModuleDto, moduleControlPath);
			Logger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): message → {Message}", createModuleDto, message);

            var source = string.Format(CultureInfo.InvariantCulture, LoadControlTemplate(), createModuleDto.Language, className);

			Logger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): source → {Source}", createModuleDto, source);
            // reset attributes
            if (File.Exists(moduleControlPath))
            {
                message = "FileExists";
            }
            else
            {
                using var stream = File.CreateText(moduleControlPath);
                stream.WriteLine(source);
				Logger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): stream → {Stream}", createModuleDto, stream);
            }

			Logger.LogDebug("<<< Exiting CreateControl(createModuleDto={CreateModuleDto})", createModuleDto);
            return message;
        }

        private static string LoadControlTemplate()
        {
			Logger.LogDebug(">>> Entering LoadControlTemplate()");
            var personaBarFolder = Library.Constants.PersonaBarRelativePath.Replace("~/", string.Empty);
            var filePath = Path.Combine(Globals.ApplicationMapPath, personaBarFolder, "Modules/Dnn.Extensions/data/ModuleControlTemplate.resources");
			Logger.LogDebug("LoadControlTemplate(): personaBarFolder → {PersonaBarFolder}", personaBarFolder);
			Logger.LogDebug("LoadControlTemplate(): filePath → {FilePath}", filePath);
			Logger.LogDebug("<<< Exiting LoadControlTemplate()");
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        private static string GetSourceFolder(CreateModuleDto createModuleDto)
        {
			Logger.LogDebug(">>> Entering GetSourceFolder(createModuleDto={CreateModuleDto})", createModuleDto);
            var folder = Null.NullString;
            if (!string.IsNullOrEmpty(createModuleDto.OwnerFolder))
            {
                folder += createModuleDto.OwnerFolder + "/";
            }

            if (!string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                folder += createModuleDto.ModuleFolder + "/";
            }

			Logger.LogDebug("<<< Exiting GetSourceFolder(createModuleDto={CreateModuleDto})", createModuleDto);
            return folder;
        }

        private static string GetClassName(CreateModuleDto createModuleDto)
        {
			Logger.LogDebug(">>> Entering GetClassName(createModuleDto={CreateModuleDto})", createModuleDto);
            var className = Null.NullString;
            if (!string.IsNullOrEmpty(createModuleDto.OwnerFolder))
            {
                className += createModuleDto.OwnerFolder + ".";
            }

            if (!string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                className += createModuleDto.ModuleFolder;
            }

            // return class and remove any spaces that might appear in folder structure
			Logger.LogDebug("<<< Exiting GetClassName(createModuleDto={CreateModuleDto})", createModuleDto);
            return className.Replace(" ", string.Empty);
        }

        private int CreateNewModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering CreateNewModule(createModuleDto={CreateModuleDto})", createModuleDto);
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
			Logger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): errorMessage → {ErrorMessage}", createModuleDto, errorMessage);
			Logger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): newPageUrl → {NewPageUrl}", createModuleDto, newPageUrl);
            if (string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                errorMessage = "NoModuleFolder";
                return Null.NullInteger;
            }

            if (string.IsNullOrEmpty(createModuleDto.Language))
            {
                errorMessage = "LanguageError";
                return Null.NullInteger;
            }

            // remove spaces so file is created correctly
            var controlSrc = createModuleDto.FileName.Replace(" ", string.Empty);
            if (InvalidFilename(controlSrc))
			Logger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): controlSrc → {ControlSrc}", createModuleDto, controlSrc);
            {
                errorMessage = "InvalidFilename";
                return Null.NullInteger;
            }

            if (string.IsNullOrEmpty(controlSrc))
            {
                errorMessage = "MissingControl";
                return Null.NullInteger;
            }

            if (string.IsNullOrEmpty(createModuleDto.ModuleName))
            {
                errorMessage = "MissingFriendlyname";
                return Null.NullInteger;
            }

            if (!controlSrc.EndsWith(".ascx", StringComparison.Ordinal))
            {
                controlSrc += ".ascx";
            }

            var uniqueName = true;
            foreach (var package in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
            {
			Logger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): uniqueName → {UniqueName}", createModuleDto, uniqueName);
                if (package.Name.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase)
                    || package.FriendlyName.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase))
                {
                    uniqueName = false;
                    break;
                }
            }

            if (!uniqueName)
            {
                errorMessage = "NonuniqueName";
                return Null.NullInteger;
            }

            // First create the control
            createModuleDto.FileName = controlSrc;
			Logger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): createModuleDto.FileName → {Arg1}", createModuleDto, createModuleDto.FileName);
            var message = CreateControl(createModuleDto);
            if (string.IsNullOrEmpty(message))
			Logger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): message → {Message}", createModuleDto, message);
            {
                // Next import the control
                return this.CreateModuleFromControl(createModuleDto, out newPageUrl, out errorMessage);
            }

			Logger.LogDebug("<<< Exiting CreateNewModule(createModuleDto={CreateModuleDto})", createModuleDto);
            return Null.NullInteger;
        }

        private int CreateModuleFromControl(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering CreateModuleFromControl(createModuleDto={CreateModuleDto})", createModuleDto);
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
			Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): newPageUrl → {NewPageUrl}", createModuleDto, newPageUrl);
			Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): errorMessage → {ErrorMessage}", createModuleDto, errorMessage);
            if (string.IsNullOrEmpty(createModuleDto.FileName))
            {
                errorMessage = "NoControl";
                return Null.NullInteger;
            }

            try
            {
                var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
                var friendlyName = createModuleDto.ModuleName;
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): folder → {Folder}", createModuleDto, folder);
                var name = createModuleDto.ModuleName;
                var moduleControl = "DesktopModules/" + folder + "/" + createModuleDto.FileName;
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): name → {Name}", createModuleDto, name);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleControl → {ModuleControl}", createModuleDto, moduleControl);

                var packageInfo = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p =>
                                    p.Name.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase)
                                     || p.FriendlyName.Equals(createModuleDto.ModuleName, StringComparison.OrdinalIgnoreCase));
                if (packageInfo != null)
                {
                    errorMessage = "NonuniqueName";
                    return Null.NullInteger;
                }

                var package = new PackageInfo
                {
                    Name = name,
                    FriendlyName = friendlyName,
                    Description = createModuleDto.Description,
                    Version = new Version(1, 0, 0),
                    PackageType = "Module",
                    License = Util.PACKAGE_NoLicense,
                };

                // Save Package
                PackageController.Instance.SaveExtensionPackage(package);

                var objDesktopModule = new DesktopModuleInfo
                {
                    DesktopModuleID = Null.NullInteger,
                    ModuleName = name,
                    FolderName = folder,
                    FriendlyName = friendlyName,
                    Description = createModuleDto.Description,
                    IsPremium = false,
                    IsAdmin = false,
                    Version = "01.00.00",
                    BusinessControllerClass = string.Empty,
                    CompatibleVersions = string.Empty,
                    Dependencies = string.Empty,
                    Permissions = string.Empty,
                    PackageID = package.PackageID,
                };

                objDesktopModule.DesktopModuleID = DesktopModuleController.SaveDesktopModule(objDesktopModule, false, true);

                // Add module to all portals
                DesktopModuleController.AddDesktopModuleToPortals(objDesktopModule.DesktopModuleID);

                // Save module definition
                var moduleDefinition = new ModuleDefinitionInfo();

				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition → {ModuleDefinition}", createModuleDto, moduleDefinition);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.ModuleDefID → {Arg1}", createModuleDto, moduleDefinition.ModuleDefID);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.DesktopModuleID → {Arg1}", createModuleDto, moduleDefinition.DesktopModuleID);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.FriendlyName → {Arg1}", createModuleDto, moduleDefinition.FriendlyName);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.DefaultCacheTime → {Arg1}", createModuleDto, moduleDefinition.DefaultCacheTime);
                moduleDefinition.ModuleDefID = Null.NullInteger;
                moduleDefinition.DesktopModuleID = objDesktopModule.DesktopModuleID;
                moduleDefinition.FriendlyName = friendlyName;
                moduleDefinition.DefaultCacheTime = 0;

                moduleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, true);

                // Save module control
                var objModuleControl = new ModuleControlInfo();

				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl → {ObjModuleControl}", createModuleDto, objModuleControl);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ModuleControlID → {Arg1}", createModuleDto, objModuleControl.ModuleControlID);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ModuleDefID → {Arg1}", createModuleDto, objModuleControl.ModuleDefID);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlKey → {Arg1}", createModuleDto, objModuleControl.ControlKey);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlSrc → {Arg1}", createModuleDto, objModuleControl.ControlSrc);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlTitle → {Arg1}", createModuleDto, objModuleControl.ControlTitle);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlType → {Arg1}", createModuleDto, objModuleControl.ControlType);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.HelpURL → {Arg1}", createModuleDto, objModuleControl.HelpURL);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.IconFile → {Arg1}", createModuleDto, objModuleControl.IconFile);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ViewOrder → {Arg1}", createModuleDto, objModuleControl.ViewOrder);
				Logger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.SupportsPartialRendering → {Arg1}", createModuleDto, objModuleControl.SupportsPartialRendering);
                objModuleControl.ModuleControlID = Null.NullInteger;
                objModuleControl.ModuleDefID = moduleDefinition.ModuleDefID;
                objModuleControl.ControlKey = string.Empty;
                objModuleControl.ControlSrc = moduleControl;
                objModuleControl.ControlTitle = string.Empty;
                objModuleControl.ControlType = SecurityAccessLevel.View;
                objModuleControl.HelpURL = string.Empty;
                objModuleControl.IconFile = string.Empty;
                objModuleControl.ViewOrder = 0;
                objModuleControl.SupportsPartialRendering = false;

                ModuleControlController.AddModuleControl(objModuleControl);

                if (createModuleDto.AddPage)
                {
                    newPageUrl = this.CreateNewPage(moduleDefinition);
                }

                return package.PackageID;
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in CreateModuleFromControl(createModuleDto={CreateModuleDto}): {Arg1}", createModuleDto, ex.Message);
                Exceptions.LogException(ex);
                errorMessage = "CreateModuleFailed";
				Logger.LogDebug("<<< Exiting CreateModuleFromControl(createModuleDto={CreateModuleDto})", createModuleDto);
                return Null.NullInteger;
            }
        }

        private int CreateModuleFromManifest(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering CreateModuleFromManifest(createModuleDto={CreateModuleDto})", createModuleDto);
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
			Logger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): newPageUrl → {NewPageUrl}", createModuleDto, newPageUrl);
			Logger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): errorMessage → {ErrorMessage}", createModuleDto, errorMessage);
            if (string.IsNullOrEmpty(createModuleDto.Manifest))
            {
                errorMessage = "MissingManifest";
                return Null.NullInteger;
            }

            try
            {
                var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
                var manifest = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", folder, createModuleDto.Manifest);
				Logger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): folder → {Folder}", createModuleDto, folder);
				Logger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): manifest → {Manifest}", createModuleDto, manifest);
                var installer = new Installer(manifest, Globals.ApplicationMapPath, true);

				Logger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): installer → {Installer}", createModuleDto, installer);
                if (installer.IsValid)
                {
                    installer.InstallerInfo.Log.Logs.Clear();
                    installer.Install();

                    if (installer.IsValid)
                    {
                        if (createModuleDto.AddPage)
                        {
                            var desktopModule =
                                DesktopModuleController.GetDesktopModuleByPackageID(installer.InstallerInfo.PackageID);
                            if (desktopModule != null && desktopModule.ModuleDefinitions.Count > 0)
                            {
                                foreach (var kvp in desktopModule.ModuleDefinitions)
                                {
                                    var moduleDefinition = kvp.Value;

                                    newPageUrl = this.CreateNewPage(moduleDefinition);
                                    break;
                                }
                            }
                        }

                        return installer.InstallerInfo.PackageID;
                    }
                    else
                    {
                        errorMessage = "InstallError";
                        return Null.NullInteger;
                    }
                }
                else
                {
                    errorMessage = "InstallError";
                    return Null.NullInteger;
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in CreateModuleFromManifest(createModuleDto={CreateModuleDto}): {Arg1}", createModuleDto, ex.Message);
                Exceptions.LogException(ex);
                errorMessage = "CreateModuleFailed";
				Logger.LogDebug("<<< Exiting CreateModuleFromManifest(createModuleDto={CreateModuleDto})", createModuleDto);
                return Null.NullInteger;
            }
        }

        private string CreateNewPage(ModuleDefinitionInfo moduleDefinition)
        {
            if (PortalSettings.Current == null)
            {
                return string.Empty;
            }

            var portalId = PortalSettings.Current.PortalId;
            var tabName = "Test " + moduleDefinition.FriendlyName + " Page";
            var tabPath = Globals.GenerateTabPath(Null.NullInteger, tabName);
            var tabId = TabController.GetTabByTabPath(portalId, tabPath, Null.NullString);
            if (tabId == Null.NullInteger)
            {
                // Create a new page
                var newTab = new TabInfo();
                newTab.TabName = tabName;
                newTab.ParentId = Null.NullInteger;
                newTab.PortalID = portalId;
                newTab.IsVisible = true;
                newTab.TabID = TabController.Instance.AddTabBefore(newTab, PortalSettings.Current.AdminTabId);
                var objModule = new ModuleInfo();
                objModule.Initialize(portalId);
                objModule.PortalID = portalId;
                objModule.TabID = newTab.TabID;
                objModule.ModuleOrder = Null.NullInteger;
                objModule.ModuleTitle = moduleDefinition.FriendlyName;
                objModule.PaneName = Globals.glbDefaultPane;
                objModule.ModuleDefID = moduleDefinition.ModuleDefID;
                objModule.InheritViewPermissions = true;
                objModule.AllTabs = false;
                ModuleController.Instance.AddModule(objModule);

                return this.NavigationManager.NavigateURL(newTab.TabID);
            }

            return string.Empty;
        }
    }
}
