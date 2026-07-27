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
		eventLogger.LogDebug(">>> Entering Controller(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
        private const string DdrMenuModuleName = "DDRMenu";
        private const string DdrMenuModuleDefinitionName = "DDR Menu";
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
		eventLogger.LogDebug("Controller(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): eventLogger → {EventLogger1}", eventLogger, permissionDefinitionService, hostSettings, eventLogger);
		eventLogger.LogDebug("Controller(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): permissionDefinitionService → {PermissionDefinitionService1}", eventLogger, permissionDefinitionService, hostSettings, permissionDefinitionService);
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

		eventLogger.LogDebug("Controller(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): hostSettings → {HostSettings1}", eventLogger, permissionDefinitionService, hostSettings, hostSettings);
        /// <summary>Initializes a new instance of the <see cref="Controller"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public Controller()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Controller"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public Controller(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService)
            : this(eventLogger, permissionDefinitionService, null)
        {
		eventLogger.LogError("Exception in Controller(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): {Arg3}", eventLogger, permissionDefinitionService, hostSettings, ex.Message);
		eventLogger.LogDebug("<<< Exiting Controller(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
        }

        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			eventLogger.LogDebug(">>> Entering UpgradeModule(version={Version})", version);
            UpdateWebConfig();

            TidyModuleDefinitions(this.eventLogger, this.permissionDefinitionService, this.hostSettings);

            CleanOldAssemblies();

            CheckSkinReferences();

			eventLogger.LogDebug("<<< Exiting UpgradeModule(version={Version})", version);
            return "UpgradeModule completed OK";
        }

        /// <inheritdoc />
        public string ExportModule(int moduleId)
        {
			eventLogger.LogDebug(">>> Entering ExportModule(moduleId={ModuleId})", moduleId);
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            var moduleSettings = module.ModuleSettings;
			eventLogger.LogDebug("ExportModule(moduleId={ModuleId}): module → {Module}", moduleId, module);

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
			eventLogger.LogDebug("<<< Exiting ExportModule(moduleId={ModuleId})", moduleId);
            return settings.ToXml();
        }

        /// <inheritdoc />
        public void ImportModule(int moduleId, string content, string version, int userId)
        {
			eventLogger.LogDebug(">>> Entering ImportModule(moduleId={ModuleId},content={Content},version={Version},userId={UserId})", moduleId, content, version, userId);
            var settings = Settings.FromXml(content);

			eventLogger.LogDebug("ImportModule(moduleId={ModuleId},content={Content},version={Version},userId={UserId}): settings → {Settings}", moduleId, content, version, userId, settings);
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
			eventLogger.LogDebug("<<< Exiting ImportModule(moduleId={ModuleId},content={Content},version={Version},userId={UserId})", moduleId, content, version, userId);
        }

        private static void UpdateWebConfig()
        {
			eventLogger.LogDebug(">>> Entering UpdateWebConfig()");
            const string navName = "DDRMenuNavigationProvider";
            const string navType = "DotNetNuke.Web.DDRMenu.DDRMenuNavigationProvider, DotNetNuke.Web.DDRMenu";

            var server = HttpContext.Current.Server;
            var webConfig = server.MapPath("~/web.config");

			eventLogger.LogDebug("UpdateWebConfig(): webConfig → {WebConfig}", webConfig);
            var configXml = new XmlDocument { XmlResolver = null };
            using (var configReader = XmlReader.Create(webConfig, new XmlReaderSettings { XmlResolver = null, }))
            {
                configXml.Load(configReader);
            }

            var navProviders = configXml.SelectSingleNode("/configuration/dotnetnuke/navigationControl/providers") as XmlElement;

			eventLogger.LogDebug("UpdateWebConfig(): navProviders → {NavProviders}", navProviders);
            // ReSharper disable PossibleNullReferenceException
            var addProvider = navProviders.SelectSingleNode("add[@name='" + navName + "']") as XmlElement;

			eventLogger.LogDebug("UpdateWebConfig(): addProvider → {AddProvider}", addProvider);
            // ReSharper restore PossibleNullReferenceException
            var needsUpdate = true;
            if (addProvider == null)
			eventLogger.LogDebug("UpdateWebConfig(): needsUpdate → {NeedsUpdate}", needsUpdate);
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
			eventLogger.LogDebug("<<< Exiting UpdateWebConfig()");
        }

        private static void TidyModuleDefinitions(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        {
			eventLogger.LogDebug(">>> Entering TidyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
            RemoveLegacyModuleDefinitions(eventLogger, permissionDefinitionService, hostSettings, DdrMenuModuleName, DdrMenuModuleDefinitionName);
            RemoveLegacyModuleDefinitions(eventLogger, permissionDefinitionService, hostSettings, "DDRMenuAdmin", "N/A");
			eventLogger.LogDebug("<<< Exiting TidyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings})", eventLogger, permissionDefinitionService, hostSettings);
        }

        private static void RemoveLegacyModuleDefinitions(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings, string moduleName, string currentModuleDefinitionName)
        {
			eventLogger.LogDebug(">>> Entering RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName})", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName);
            var mdc = new ModuleDefinitionController(permissionDefinitionService);

			eventLogger.LogDebug("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): mdc → {Mdc}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, mdc);
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(hostSettings, moduleName, Null.NullInteger);
            if (desktopModule == null)
			eventLogger.LogDebug("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): desktopModule → {DesktopModule}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, desktopModule);
            {
                return;
            }

            var desktopModuleId = desktopModule.DesktopModuleID;
            var modDefs = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId);

			eventLogger.LogDebug("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): modDefs → {ModDefs}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, modDefs);
            var currentModDefId = 0;
            foreach (var modDefKeyPair in modDefs)
            {
			eventLogger.LogDebug("RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName}): currentModDefId → {CurrentModDefId}", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName, currentModDefId);
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
			eventLogger.LogDebug("<<< Exiting RemoveLegacyModuleDefinitions(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings},moduleName={ModuleName},currentModuleDefinitionName={CurrentModuleDefinitionName})", eventLogger, permissionDefinitionService, hostSettings, moduleName, currentModuleDefinitionName);
        }

        private static void CleanOldAssemblies()
        {
			eventLogger.LogDebug(">>> Entering CleanOldAssemblies()");
            var assembliesToRemove = new[] { "DNNDoneRight.DDRMenu.dll", "DNNGarden.DDRMenu.dll", };

			eventLogger.LogDebug("CleanOldAssemblies(): assembliesToRemove → {AssembliesToRemove}", assembliesToRemove);
            var server = HttpContext.Current.Server;
            var assemblyPath = server.MapPath("~/bin/");
            foreach (var assembly in assembliesToRemove)
            {
			eventLogger.LogDebug("CleanOldAssemblies(): assemblyPath → {AssemblyPath}", assemblyPath);
                File.Delete(Path.Combine(assemblyPath, assembly));
            }
			eventLogger.LogDebug("<<< Exiting CleanOldAssemblies()");
        }

        private static void CheckSkinReferences()
        {
			eventLogger.LogDebug(">>> Entering CheckSkinReferences()");
            var server = HttpContext.Current.Server;
            var portalsRoot = server.MapPath("~/Portals/");
            foreach (var portal in Directory.GetDirectories(portalsRoot))
            {
			eventLogger.LogDebug("CheckSkinReferences(): portalsRoot → {PortalsRoot}", portalsRoot);
                foreach (var skinControl in Directory.GetFiles(portal, "*.ascx", SearchOption.AllDirectories))
                {
                    try
                    {
                        var ascxText = File.ReadAllText(skinControl);
                        var originalText = ascxText;
						eventLogger.LogDebug("CheckSkinReferences(): ascxText → {AscxText}", ascxText);

                        Regex ascxText1Regex = new Regex(Regex.Escape(@"Namespace=""DNNDoneRight.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        Regex ascxText2Regex = new Regex(Regex.Escape(@"Namespace=""DNNGarden.TemplateEngine"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
						eventLogger.LogDebug("CheckSkinReferences(): ascxText1Regex → {AscxText1Regex}", ascxText1Regex);
						eventLogger.LogDebug("CheckSkinReferences(): ascxText2Regex → {AscxText2Regex}", ascxText2Regex);
                        Regex ascxText3Regex = new Regex(Regex.Escape(@"Assembly=""DNNDoneRight.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
                        Regex ascxText4Regex = new Regex(Regex.Escape(@"Assembly=""DNNGarden.DDRMenu"""), RegexOptions.IgnoreCase | RegexOptions.Compiled);
						eventLogger.LogDebug("CheckSkinReferences(): ascxText3Regex → {AscxText3Regex}", ascxText3Regex);
						eventLogger.LogDebug("CheckSkinReferences(): ascxText4Regex → {AscxText4Regex}", ascxText4Regex);

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
					eventLogger.LogError("Exception in Controller(eventLogger={EventLogger},permissionDefinitionService={PermissionDefinitionService},hostSettings={HostSettings}): {Arg3}", eventLogger, permissionDefinitionService, hostSettings, ex.Message);
                    }
					eventLogger.LogError("Exception in CheckSkinReferences(): {Arg0}", ex.Message);
                    catch (UnauthorizedAccessException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting CheckSkinReferences()");
        }
    }
}
