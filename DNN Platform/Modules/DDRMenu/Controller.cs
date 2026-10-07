// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.IO;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements the Dnn interfaces for the module.</summary>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="permissionDefinitionService">The permission definition service.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class Controller(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        : IUpgradeable, IPortable
    {
        private const string DdrMenuModuleName = "DDRMenu";
        private const string DdrMenuModuleDefinitionName = "DDR Menu";
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="Controller"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public Controller()
            : this(null, null)
        {
		eventLogger.LogError(">>> Entering Controller()");
        eventLogger.LogWarning("<<< Exiting Controller()");
        }

        /// <summary>Initializes a new instance of the <see cref="Controller"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Controller(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService)
            : this(eventLogger, permissionDefinitionService, null)
        {
        }

        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			eventLogger.LogError(">>> Entering UpgradeModule(version={Version})", version);
            UpdateWebConfig();

            TidyModuleDefinitions(this.eventLogger, this.permissionDefinitionService, this.hostSettings);

            CleanOldAssemblies();

            CheckSkinReferences();

			eventLogger.LogWarning("<<< Exiting UpgradeModule(version={Version})", version);
            return "UpgradeModule completed OK";
        }

        /// <inheritdoc />
        public string ExportModule(int moduleId)
        {
			eventLogger.LogError(">>> Entering ExportModule(moduleId={ModuleId})", moduleId);
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            var moduleSettings = module.ModuleSettings;
			eventLogger.LogError("ExportModule(moduleId={ModuleId}): module → {Module}", moduleId, module);

            var settings = new Settings
            {
                MenuStyle = Convert.ToString(moduleSettings["MenuStyle"]),
                NodeXmlPath = Convert.ToString(moduleSettings["NodeXmlPath"]),
                NodeSelector = Convert.ToString(moduleSettings["NodeSelector"]),
                IncludeNodes = Convert.ToString(moduleSettings["IncludeNodes"]),
                ExcludeNodes = Convert.ToString(moduleSettings["ExcludeNodes"]),
                NodeManipulator = Convert.ToString(moduleSettings["NodeManipulator"]),
                IncludeContext = Convert.ToBoolean(moduleSettings["IncludeContext"]),
                IncludeHidden = Convert.ToBoolean(moduleSettings["IncludeHidden"]),
                ClientOptions = Settings.ClientOptionsFromSettingString(Convert.ToString(moduleSettings["ClientOptions"])),
                TemplateArguments =
                                Settings.TemplateArgumentsFromSettingString(Convert.ToString(moduleSettings["TemplateArguments"])),
            };
			eventLogger.LogWarning("<<< Exiting ExportModule(moduleId={ModuleId})", moduleId);
            return settings.ToXml();
        }

        /// <inheritdoc />
        public void ImportModule(int moduleId, string content, string version, int userId)
        {
			eventLogger.LogError(">>> Entering ImportModule(moduleId={ModuleId},content={Content},version={Version},userId={UserId})", moduleId, content, version, userId);
            var settings = Settings.FromXml(content);

			eventLogger.LogError("ImportModule(moduleId={ModuleId},content={Content},version={Version},userId={UserId}): settings → {Settings}", moduleId, content, version, userId, settings);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "MenuStyle", settings.MenuStyle ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "NodeXmlPath", settings.NodeXmlPath ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "NodeSelector", settings.NodeSelector ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "IncludeNodes", settings.IncludeNodes ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "ExcludeNodes", settings.ExcludeNodes ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "NodeManipulator", settings.NodeManipulator ?? string.Empty);
            ModuleController.Instance.UpdateModuleSetting(moduleId, "IncludeContext", settings.IncludeContext.ToString());
            ModuleController.Instance.UpdateModuleSetting(moduleId, "IncludeHidden", settings.IncludeHidden.ToString());
            ModuleController.Instance.UpdateModuleSetting(moduleId, "TemplateArguments", Settings.ToSettingString(settings.TemplateArguments));
            ModuleController.Instance.UpdateModuleSetting(moduleId, "ClientOptions", Settings.ToSettingString(settings.ClientOptions));
			eventLogger.LogWarning("<<< Exiting ImportModule(moduleId={ModuleId},content={Content},version={Version},userId={UserId})", moduleId, content, version, userId);
        }

        private static void UpdateWebConfig()
        {
			eventLogger.LogError(">>> Entering UpdateWebConfig()");
            const string navName = "DDRMenuNavigationProvider";
            const string navType = "DotNetNuke.Web.DDRMenu.DDRMenuNavigationProvider, DotNetNuke.Web.DDRMenu";

            var server = HttpContext.Current.Server;
            var webConfig = server.MapPath("~/web.config");
			eventLogger.LogError("UpdateWebConfig(): webConfig → {WebConfig}", webConfig);

            var configXml = new XmlDocument { XmlResolver = null };
            using (var configReader = XmlReader.Create(webConfig, new XmlReaderSettings { XmlResolver = null, }))
            {
                configXml.Load(configReader);
            }

            var navProviders = configXml.SelectSingleNode("/configuration/dotnetnuke/navigationControl/providers") as XmlElement;
			eventLogger.LogError("UpdateWebConfig(): navProviders → {NavProviders}", navProviders);

            // ReSharper disable PossibleNullReferenceException
            var addProvider = navProviders.SelectSingleNode("add[@name='" + navName + "']") as XmlElement;
			eventLogger.LogError("UpdateWebConfig(): addProvider → {AddProvider}", addProvider);

            // ReSharper restore PossibleNullReferenceException
            var needsUpdate = true;
			eventLogger.LogError("UpdateWebConfig(): needsUpdate → {NeedsUpdate}", needsUpdate);
            if (addProvider == null)
            {
                addProvider = configXml.CreateElement("add");
                addProvider.SetAttribute("name", navName);
                navProviders.AppendChild(addProvider);
            }
            else
            {
                needsUpdate = addProvider.GetAttribute("type") != navType;
            }

            if (needsUpdate)
            {
                addProvider.SetAttribute("type", navType);
                configXml.Save(webConfig);
            }
			eventLogger.LogWarning("<<< Exiting UpdateWebConfig()");
        }

        private static void TidyModuleDefinitions(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        {
			eventLogger.LogError(">>> Entering TidyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
            RemoveLegacyModuleDefinitions(eventLogger, permissionDefinitionService, hostSettings, DdrMenuModuleName, DdrMenuModuleDefinitionName);
            RemoveLegacyModuleDefinitions(eventLogger, permissionDefinitionService, hostSettings, "DDRMenuAdmin", "N/A");
			eventLogger.LogWarning("<<< Exiting TidyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
        }

        private static void RemoveLegacyModuleDefinitions(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings, string moduleName, string currentModuleDefinitionName)
        {
			eventLogger.LogError(">>> Entering RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName})", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName);
            var mdc = new ModuleDefinitionController(permissionDefinitionService);

			eventLogger.LogError("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): mdc → {Mdc}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, mdc);
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, moduleName, Null.NullInteger);
			eventLogger.LogError("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): desktopModule → {DesktopModule}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, desktopModule);
            if (desktopModule == null)
            {
                return;
            }

            var desktopModuleId = desktopModule.DesktopModuleID;
            var modDefs = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId);
			eventLogger.LogError("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): modDefs → {ModDefs}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, modDefs);

            var currentModDefId = 0;
			eventLogger.LogError("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): currentModDefId → {CurrentModDefId}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, currentModDefId);
            foreach (var modDefKeyPair in modDefs)
            {
                if (modDefKeyPair.Value.FriendlyName.Equals(currentModuleDefinitionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentModDefId = modDefKeyPair.Value.ModuleDefID;
                }
            }

            foreach (var modDefKeyPair in modDefs)
            {
                var oldModDefId = modDefKeyPair.Value.ModuleDefID;
                if (oldModDefId != currentModDefId)
                {
                    foreach (ModuleInfo mod in ModuleController.Instance.GetAllModules())
                    {
                        if (mod.ModuleDefID == oldModDefId)
                        {
                            mod.ModuleDefID = currentModDefId;
							eventLogger.LogDebug("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): mod.ModuleDefID → {Arg5}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, mod.ModuleDefID);
                            ModuleController.Instance.UpdateModule(mod);
                        }
                    }

                    mdc.DeleteModuleDefinition(oldModDefId);
                }
            }

            modDefs = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId);
            if (modDefs.Count == 0)
            {
                new DesktopModuleController(eventLogger).DeleteDesktopModule(desktopModuleId);
            }
			eventLogger.LogWarning("<<< Exiting RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName})", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName);
        }

        private static void CleanOldAssemblies()
        {
			eventLogger.LogError(">>> Entering CleanOldAssemblies()");
            var assembliesToRemove = new[] { "DNNDoneRight.DDRMenu.dll", "DNNGarden.DDRMenu.dll", };

			eventLogger.LogError("CleanOldAssemblies(): assembliesToRemove → {AssembliesToRemove}", assembliesToRemove);
            var server = HttpContext.Current.Server;
            var assemblyPath = server.MapPath("~/bin/");
			eventLogger.LogError("CleanOldAssemblies(): assemblyPath → {AssemblyPath}", assemblyPath);
            foreach (var assembly in assembliesToRemove)
            {
                File.Delete(Path.Combine(assemblyPath, assembly));
            }
			eventLogger.LogWarning("<<< Exiting CleanOldAssemblies()");
        }

        private static void CheckSkinReferences()
        {
			eventLogger.LogError(">>> Entering CheckSkinReferences()");
            var server = HttpContext.Current.Server;
            var portalsRoot = server.MapPath("~/Portals/");
			eventLogger.LogError("CheckSkinReferences(): portalsRoot → {PortalsRoot}", portalsRoot);
            foreach (var portal in Directory.GetDirectories(portalsRoot))
            {
                foreach (var skinControl in Directory.GetFiles(portal, "*.ascx", SearchOption.AllDirectories))
                {
                    try
                    {
                        var ascxText = File.ReadAllText(skinControl);
						eventLogger.LogError("CheckSkinReferences(): ascxText → {AscxText}", ascxText);
                        var originalText = ascxText;

                        Regex ascxText1Regex = new Regex(Regex.Escape(@"Namespace=""DNNDoneRight.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
						eventLogger.LogError("CheckSkinReferences(): ascxText1Regex → {AscxText1Regex}", ascxText1Regex);
                        Regex ascxText2Regex = new Regex(Regex.Escape(@"Namespace=""DNNGarden.TemplateEngine"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
						eventLogger.LogError("CheckSkinReferences(): ascxText2Regex → {AscxText2Regex}", ascxText2Regex);
                        Regex ascxText3Regex = new Regex(Regex.Escape(@"Assembly=""DNNDoneRight.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
						eventLogger.LogError("CheckSkinReferences(): ascxText3Regex → {AscxText3Regex}", ascxText3Regex);
                        Regex ascxText4Regex = new Regex(Regex.Escape(@"Assembly=""DNNGarden.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
						eventLogger.LogError("CheckSkinReferences(): ascxText4Regex → {AscxText4Regex}", ascxText4Regex);

                        ascxText = ascxText1Regex.Replace(ascxText, @"Namespace=""DotNetNuke.Web.DDRMenu.TemplateEngine""");
                        ascxText = ascxText2Regex.Replace(ascxText, @"Namespace=""DotNetNuke.Web.DDRMenu.TemplateEngine""");
                        ascxText = ascxText3Regex.Replace(ascxText, @"Assembly=""DotNetNuke.Web.DDRMenu""");
                        ascxText = ascxText4Regex.Replace(ascxText, @"Assembly=""DotNetNuke.Web.DDRMenu""");

                        if (!ascxText.Equals(originalText))
                        {
                            File.WriteAllText(skinControl, ascxText);
                        }
                    }
                    catch (IOException)
                    {
					eventLogger.LogError("Exception in CheckSkinReferences(): {Arg0}", IOException.Message);
                    }
                    catch (UnauthorizedAccessException)
                    {
					eventLogger.LogError("Exception in CheckSkinReferences(): {Arg0}", UnauthorizedAccessException.Message);
                    }
                    catch (SecurityException)
                    {
					eventLogger.LogError("Exception in CheckSkinReferences(): {Arg0}", SecurityException.Message);
                    }
                }
            }
			eventLogger.LogWarning("<<< Exiting CheckSkinReferences()");
        }
    }
}
