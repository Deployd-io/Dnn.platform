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
        /// <summary>Initializes a new instance of the <see cref="CreateModuleController"/> class.</summary>
        public CreateModuleController()
        {
			eventLogger.LogDebug(">>> Entering CreateModuleController()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			eventLogger.LogDebug("<<< Exiting CreateModuleController()");
        }

        protected INavigationManager NavigationManager { get; }

        /// <summary>create new module.</summary>
        /// <param name="createModuleDto">Information about the module to create.</param>
        /// <param name="newPageUrl">The new page URL or <see cref="string.Empty"/>.</param>
        /// <param name="errorMessage">The error message or <see cref="string.Empty"/>.</param>
        /// <returns>return the new package id.</returns>
        public int CreateModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering CreateModule(createModuleDto={CreateModuleDto})", createModuleDto);
            errorMessage = string.Empty;
            newPageUrl = string.Empty;
			eventLogger.LogDebug("CreateModule(createModuleDto={CreateModuleDto}): newPageUrl → {NewPageUrl}", createModuleDto, newPageUrl);
			eventLogger.LogDebug("CreateModule(createModuleDto={CreateModuleDto}): errorMessage → {ErrorMessage}", createModuleDto, errorMessage);
            var packageId = Null.NullInteger;
			eventLogger.LogDebug("CreateModule(createModuleDto={CreateModuleDto}): packageId → {PackageId}", createModuleDto, packageId);
            switch (createModuleDto.Type)
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

			eventLogger.LogDebug("<<< Exiting CreateModule(createModuleDto={CreateModuleDto})", createModuleDto);
            return packageId;
        }

        /// <inheritdoc />
        protected override Func<ICreateModuleController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => new CreateModuleController();
        }

        private static bool InvalidFilename(string fileName)
        {
			eventLogger.LogDebug(">>> Entering InvalidFilename(fileName={FileName})", fileName);
            var invalidFilenameChars = RegexUtils.GetCachedRegex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
			eventLogger.LogDebug("InvalidFilename(fileName={FileName}): invalidFilenameChars → {InvalidFilenameChars}", fileName, invalidFilenameChars);
			eventLogger.LogDebug("<<< Exiting InvalidFilename(fileName={FileName})", fileName);
            return invalidFilenameChars.IsMatch(fileName);
        }

        private static string CreateControl(CreateModuleDto createModuleDto)
        {
			eventLogger.LogDebug(">>> Entering CreateControl(createModuleDto={CreateModuleDto})", createModuleDto);
            var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
            var className = GetClassName(createModuleDto);
			eventLogger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): className → {ClassName}", createModuleDto, className);
            var moduleControlPath = Path.Combine(Globals.ApplicationMapPath, $"DesktopModules/{folder}/{createModuleDto.FileName}");
			eventLogger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): moduleControlPath → {ModuleControlPath}", createModuleDto, moduleControlPath);
            var message = Null.NullString;
			eventLogger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): message → {Message}", createModuleDto, message);
			eventLogger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): folder → {Folder}", createModuleDto, folder);

            var source = string.Format(CultureInfo.InvariantCulture, LoadControlTemplate(), createModuleDto.Language, className);
			eventLogger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): source → {Source}", createModuleDto, source);

            // reset attributes
            if (File.Exists(moduleControlPath))
            {
                message = "FileExists";
            }
            else
            {
                using var stream = File.CreateText(moduleControlPath);
				eventLogger.LogDebug("CreateControl(createModuleDto={CreateModuleDto}): stream → {Stream}", createModuleDto, stream);
                stream.WriteLine(source);
            }

			eventLogger.LogDebug("<<< Exiting CreateControl(createModuleDto={CreateModuleDto})", createModuleDto);
            return message;
        }

        private static string LoadControlTemplate()
        {
			eventLogger.LogDebug(">>> Entering LoadControlTemplate()");
            var personaBarFolder = Library.Constants.PersonaBarRelativePath.Replace("~/", string.Empty);
            var filePath = Path.Combine(Globals.ApplicationMapPath, personaBarFolder, "Modules/Dnn.Extensions/data/ModuleControlTemplate.resources");
			eventLogger.LogDebug("LoadControlTemplate(): filePath → {FilePath}", filePath);
			eventLogger.LogDebug("LoadControlTemplate(): personaBarFolder → {PersonaBarFolder}", personaBarFolder);
			eventLogger.LogDebug("<<< Exiting LoadControlTemplate()");
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        private static string GetSourceFolder(CreateModuleDto createModuleDto)
        {
			eventLogger.LogDebug(">>> Entering GetSourceFolder(createModuleDto={CreateModuleDto})", createModuleDto);
            var folder = Null.NullString;
            if (!string.IsNullOrEmpty(createModuleDto.OwnerFolder))
            {
                folder += createModuleDto.OwnerFolder + "/";
            }

            if (!string.IsNullOrEmpty(createModuleDto.ModuleFolder))
            {
                folder += createModuleDto.ModuleFolder + "/";
            }

			eventLogger.LogDebug("<<< Exiting GetSourceFolder(createModuleDto={CreateModuleDto})", createModuleDto);
            return folder;
        }

        private static string GetClassName(CreateModuleDto createModuleDto)
        {
			eventLogger.LogDebug(">>> Entering GetClassName(createModuleDto={CreateModuleDto})", createModuleDto);
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
			eventLogger.LogDebug("<<< Exiting GetClassName(createModuleDto={CreateModuleDto})", createModuleDto);
            return className.Replace(" ", string.Empty);
        }

        private int CreateNewModule(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering CreateNewModule(createModuleDto={CreateModuleDto})", createModuleDto);
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
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
			eventLogger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): controlSrc → {ControlSrc}", createModuleDto, controlSrc);
            if (InvalidFilename(controlSrc))
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
			eventLogger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): uniqueName → {UniqueName}", createModuleDto, uniqueName);
            foreach (var package in PackageController.Instance.GetExtensionPackages(Null.NullInteger))
            {
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
			eventLogger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): createModuleDto.FileName → {Arg1}", createModuleDto, createModuleDto.FileName);
            var message = CreateControl(createModuleDto);
			eventLogger.LogDebug("CreateNewModule(createModuleDto={CreateModuleDto}): message → {Message}", createModuleDto, message);
            if (string.IsNullOrEmpty(message))
            {
                // Next import the control
			eventLogger.LogDebug("<<< Exiting CreateNewModule(createModuleDto={CreateModuleDto})", createModuleDto);
                return this.CreateModuleFromControl(createModuleDto, out newPageUrl, out errorMessage);
            }

            return Null.NullInteger;
        }

        private int CreateModuleFromControl(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering CreateModuleFromControl(createModuleDto={CreateModuleDto})", createModuleDto);
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.FileName))
            {
                errorMessage = "NoControl";
                return Null.NullInteger;
            }

            try
            {
                var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): folder → {Folder}", createModuleDto, folder);
                var friendlyName = createModuleDto.ModuleName;
                var name = createModuleDto.ModuleName;
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): name → {Name}", createModuleDto, name);
                var moduleControl = "DesktopModules/" + folder + "/" + createModuleDto.FileName;
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleControl → {ModuleControl}", createModuleDto, moduleControl);

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
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition → {ModuleDefinition}", createModuleDto, moduleDefinition);

                moduleDefinition.ModuleDefID = Null.NullInteger;
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.ModuleDefID → {Arg1}", createModuleDto, moduleDefinition.ModuleDefID);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.DesktopModuleID → {Arg1}", createModuleDto, moduleDefinition.DesktopModuleID);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.FriendlyName → {Arg1}", createModuleDto, moduleDefinition.FriendlyName);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): moduleDefinition.DefaultCacheTime → {Arg1}", createModuleDto, moduleDefinition.DefaultCacheTime);
                moduleDefinition.DesktopModuleID = objDesktopModule.DesktopModuleID;
                moduleDefinition.FriendlyName = friendlyName;
                moduleDefinition.DefaultCacheTime = 0;

                moduleDefinition.ModuleDefID = ModuleDefinitionController.SaveModuleDefinition(moduleDefinition, false, true);

                // Save module control
                var objModuleControl = new ModuleControlInfo();
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl → {ObjModuleControl}", createModuleDto, objModuleControl);

                objModuleControl.ModuleControlID = Null.NullInteger;
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ModuleControlID → {Arg1}", createModuleDto, objModuleControl.ModuleControlID);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ModuleDefID → {Arg1}", createModuleDto, objModuleControl.ModuleDefID);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlKey → {Arg1}", createModuleDto, objModuleControl.ControlKey);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlSrc → {Arg1}", createModuleDto, objModuleControl.ControlSrc);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlTitle → {Arg1}", createModuleDto, objModuleControl.ControlTitle);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ControlType → {Arg1}", createModuleDto, objModuleControl.ControlType);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.HelpURL → {Arg1}", createModuleDto, objModuleControl.HelpURL);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.IconFile → {Arg1}", createModuleDto, objModuleControl.IconFile);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.ViewOrder → {Arg1}", createModuleDto, objModuleControl.ViewOrder);
				eventLogger.LogDebug("CreateModuleFromControl(createModuleDto={CreateModuleDto}): objModuleControl.SupportsPartialRendering → {Arg1}", createModuleDto, objModuleControl.SupportsPartialRendering);
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
				eventLogger.LogError("Exception in CreateModuleFromControl(createModuleDto={CreateModuleDto}): {Arg1}", createModuleDto, ex.getMessage());
                Exceptions.LogException(ex);
                errorMessage = "CreateModuleFailed";
				eventLogger.LogDebug("<<< Exiting CreateModuleFromControl(createModuleDto={CreateModuleDto})", createModuleDto);
                return Null.NullInteger;
            }
        }

        private int CreateModuleFromManifest(CreateModuleDto createModuleDto, out string newPageUrl, out string errorMessage)
        {
			eventLogger.LogDebug(">>> Entering CreateModuleFromManifest(createModuleDto={CreateModuleDto})", createModuleDto);
            newPageUrl = string.Empty;
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(createModuleDto.Manifest))
            {
                errorMessage = "MissingManifest";
                return Null.NullInteger;
            }

            try
            {
                var folder = PathUtils.Instance.RemoveTrailingSlash(GetSourceFolder(createModuleDto));
				eventLogger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): folder → {Folder}", createModuleDto, folder);
                var manifest = Path.Combine(Globals.ApplicationMapPath, "DesktopModules", folder, createModuleDto.Manifest);
				eventLogger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): manifest → {Manifest}", createModuleDto, manifest);
                var installer = new Installer(manifest, Globals.ApplicationMapPath, true);
				eventLogger.LogDebug("CreateModuleFromManifest(createModuleDto={CreateModuleDto}): installer → {Installer}", createModuleDto, installer);

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
				eventLogger.LogError("Exception in CreateModuleFromManifest(createModuleDto={CreateModuleDto}): {Arg1}", createModuleDto, ex.getMessage());
                Exceptions.LogException(ex);
                errorMessage = "CreateModuleFailed";
				eventLogger.LogDebug("<<< Exiting CreateModuleFromManifest(createModuleDto={CreateModuleDto})", createModuleDto);
                return Null.NullInteger;
            }
        }

        private string CreateNewPage(ModuleDefinitionInfo moduleDefinition)
        {
			eventLogger.LogDebug(">>> Entering CreateNewPage(moduleDefinition={ModuleDefinition})", moduleDefinition);
            if (PortalSettings.Current == null)
            {
                return string.Empty;
            }

            var portalId = PortalSettings.Current.PortalId;
            var tabName = "Test " + moduleDefinition.FriendlyName + " Page";
			eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): tabName → {TabName}", moduleDefinition, tabName);
            var tabPath = Globals.GenerateTabPath(Null.NullInteger, tabName);
			eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): tabPath → {TabPath}", moduleDefinition, tabPath);
            var tabId = TabController.GetTabByTabPath(portalId, tabPath, Null.NullString);
			eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): tabId → {TabId}", moduleDefinition, tabId);
            if (tabId == Null.NullInteger)
            {
                // Create a new page
                var newTab = new TabInfo();
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): newTab → {NewTab}", moduleDefinition, newTab);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): newTab.TabName → {Arg1}", moduleDefinition, newTab.TabName);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): newTab.ParentId → {Arg1}", moduleDefinition, newTab.ParentId);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): newTab.PortalID → {Arg1}", moduleDefinition, newTab.PortalID);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): newTab.IsVisible → {Arg1}", moduleDefinition, newTab.IsVisible);
                newTab.TabName = tabName;
                newTab.ParentId = Null.NullInteger;
                newTab.PortalID = portalId;
                newTab.IsVisible = true;
                newTab.TabID = TabController.Instance.AddTabBefore(newTab, PortalSettings.Current.AdminTabId);
                var objModule = new ModuleInfo();
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule → {ObjModule}", moduleDefinition, objModule);
                objModule.Initialize(portalId);
                objModule.PortalID = portalId;
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.PortalID → {Arg1}", moduleDefinition, objModule.PortalID);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.TabID → {Arg1}", moduleDefinition, objModule.TabID);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.ModuleOrder → {Arg1}", moduleDefinition, objModule.ModuleOrder);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.ModuleTitle → {Arg1}", moduleDefinition, objModule.ModuleTitle);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.PaneName → {Arg1}", moduleDefinition, objModule.PaneName);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.ModuleDefID → {Arg1}", moduleDefinition, objModule.ModuleDefID);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.InheritViewPermissions → {Arg1}", moduleDefinition, objModule.InheritViewPermissions);
				eventLogger.LogDebug("CreateNewPage(moduleDefinition={ModuleDefinition}): objModule.AllTabs → {Arg1}", moduleDefinition, objModule.AllTabs);
                objModule.TabID = newTab.TabID;
                objModule.ModuleOrder = Null.NullInteger;
                objModule.ModuleTitle = moduleDefinition.FriendlyName;
                objModule.PaneName = Globals.glbDefaultPane;
                objModule.ModuleDefID = moduleDefinition.ModuleDefID;
                objModule.InheritViewPermissions = true;
                objModule.AllTabs = false;
                ModuleController.Instance.AddModule(objModule);

			eventLogger.LogDebug("<<< Exiting CreateNewPage(moduleDefinition={ModuleDefinition})", moduleDefinition);
                return this.NavigationManager.NavigateURL(newTab.TabID);
            }

            return string.Empty;
        }
    }
}
