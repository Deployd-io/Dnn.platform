// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.ModuleCache;
    using DotNetNuke.Services.OutputCache;
    using DotNetNuke.Services.Search.Entities;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ModuleController provides the Business Layer for Modules.</summary>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="permissionDefinitionService">The permission definition service.</param>
    /// <param name="hostSettings">The host settings.</param>
    public partial class ModuleController(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService, IHostSettings hostSettings)
        : ServiceLocator<IModuleController, ModuleController>, IModuleController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleController));
        private static readonly DataProvider DataProvider = DataProvider.Instance();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="ModuleController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ModuleController()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering ModuleController()");
        }

        /// <summary>Initializes a new instance of the <see cref="ModuleController"/> class.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModuleController(IEventLogger eventLogger, IPermissionDefinitionService permissionDefinitionService)
            : this(eventLogger, permissionDefinitionService, null)
        {
		Logger.LogDebug("<<< Exiting ModuleController()");
        }

        private static Hashtable ParsedLocalizedModuleGuid => (Hashtable)(HttpContext.Current.Items["ParsedLocalizedModuleGuid"] ??= new Hashtable());

        /// <summary>Deserializes the module.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="module">ModuleInfo of current module.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionService")]
        public static partial void DeserializeModule(IBusinessControllerProvider businessControllerProvider, XmlNode nodeModule, ModuleInfo module, int portalId, int tabId)
            => DeserializeModule(businessControllerProvider, Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), nodeModule, module, portalId, tabId);

        /// <summary>Deserializes the module.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="module">ModuleInfo of current module.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        public static void DeserializeModule(IBusinessControllerProvider businessControllerProvider, IPermissionDefinitionService permissionDefinitionService, XmlNode nodeModule, ModuleInfo module, int portalId, int tabId)
        {
			Logger.LogDebug(">>> Entering DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId})", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId);
            var moduleDefinition = GetModuleDefinition(nodeModule);

			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): moduleDefinition → {ModuleDefinition}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, moduleDefinition);
            // Create pane node for private DeserializeModule method
			Logger.LogDebug("<<< Exiting DeserializeModule(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId})", businessControllerProvider, nodeModule, module, portalId, tabId);
            var paneXml = $"<pane><name>{module.PaneName}</name></pane>";
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): paneXml → {PaneXml}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, paneXml);
            var docPane = new XmlDocument { XmlResolver = null };
            using (var paneReader = XmlReader.Create(new StringReader(paneXml), new XmlReaderSettings { XmlResolver = null, }))
            {
                docPane.Load(paneReader);
            }

            // Create ModuleInfo of Xml
            ModuleInfo sourceModule = DeserializeModule(nodeModule, docPane.DocumentElement, portalId, tabId, moduleDefinition.ModuleDefID);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): sourceModule → {SourceModule}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, sourceModule);

            // Copy properties from sourceModule to given (actual) module
            module.ModuleTitle = sourceModule.ModuleTitle;
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.ModuleTitle → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.ModuleTitle);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.ModuleDefID → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.ModuleDefID);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.CacheTime → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.CacheTime);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.CacheMethod → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.CacheMethod);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.Alignment → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.Alignment);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.IconFile → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.IconFile);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.AllTabs → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.AllTabs);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.Visibility → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.Visibility);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.Color → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.Color);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.Border → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.Border);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.Header → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.Header);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.Footer → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.Footer);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.InheritViewPermissions → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.InheritViewPermissions);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.IsShareable → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.IsShareable);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.IsShareableViewOnly → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.IsShareableViewOnly);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.StartDate → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.StartDate);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.EndDate → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.EndDate);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.ContainerSrc → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.ContainerSrc);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.DisplayTitle → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.DisplayTitle);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.DisplayPrint → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.DisplayPrint);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.DisplaySyndicate → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.DisplaySyndicate);
            module.ModuleDefID = sourceModule.ModuleDefID;
            module.CacheTime = sourceModule.CacheTime;
            module.CacheMethod = sourceModule.CacheMethod;
            module.Alignment = sourceModule.Alignment;
            module.IconFile = sourceModule.IconFile;
            module.AllTabs = sourceModule.AllTabs;
            module.Visibility = sourceModule.Visibility;
            module.Color = sourceModule.Color;
            module.Border = sourceModule.Border;
            module.Header = sourceModule.Header;
            module.Footer = sourceModule.Footer;
            module.InheritViewPermissions = sourceModule.InheritViewPermissions;
            module.IsShareable = sourceModule.IsShareable;
            module.IsShareableViewOnly = sourceModule.IsShareableViewOnly;
            module.StartDate = sourceModule.StartDate;
            module.EndDate = sourceModule.EndDate;
            module.ContainerSrc = sourceModule.ContainerSrc;
            module.DisplayTitle = sourceModule.DisplayTitle;
            module.DisplayPrint = sourceModule.DisplayPrint;
            module.DisplaySyndicate = sourceModule.DisplaySyndicate;

            // DNN-24983 get culture from page
            var tabInfo = TabController.Instance.GetTab(tabId, portalId, false);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): tabInfo → {TabInfo}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, tabInfo);
            if (tabInfo != null)
            {
                module.CultureCode = tabInfo.CultureCode;
				Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): module.CultureCode → {Arg6}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, module.CultureCode);
            }

            // save changes
            Instance.UpdateModule(module);

            // deserialize Module's settings
            XmlNodeList nodeModuleSettings = nodeModule.SelectNodes("modulesettings/modulesetting");
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): nodeModuleSettings → {NodeModuleSettings}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, nodeModuleSettings);
            DeserializeModuleSettings(nodeModuleSettings, module);

            XmlNodeList nodeTabModuleSettings = nodeModule.SelectNodes("tabmodulesettings/tabmodulesetting");
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): nodeTabModuleSettings → {NodeTabModuleSettings}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, nodeTabModuleSettings);
            DeserializeTabModuleSettings(nodeTabModuleSettings, module);

            // deserialize Content (if included)
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "content")))
            {
                GetModuleContent(businessControllerProvider, nodeModule, module.ModuleID, tabId, portalId);
            }

            // deserialize Permissions
            XmlNodeList nodeModulePermissions = nodeModule.SelectNodes("modulepermissions/permission");
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): nodeModulePermissions → {NodeModulePermissions}", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId, nodeModulePermissions);
            DeserializeModulePermissions(permissionDefinitionService, nodeModulePermissions, portalId, module);

            // Persist the permissions to the Data base
            ModulePermissionController.SaveModulePermissions(module);
			Logger.LogDebug("<<< Exiting DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId})", businessControllerProvider, permissionDefinitionService, nodeModule, module, portalId, tabId);
        }

        /// <summary>Deserializes the module.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="nodePane">The node pane.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="hModules">The modules.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionService")]
        public static partial void DeserializeModule(IBusinessControllerProvider businessControllerProvider, XmlNode nodeModule, XmlNode nodePane, int portalId, int tabId, PortalTemplateModuleAction mergeTabs, Hashtable hModules)
            => DeserializeModule(businessControllerProvider, Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>(), nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);

        /// <summary>Deserializes the module.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="nodePane">The node pane.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="hModules">The modules.</param>
        public static void DeserializeModule(IBusinessControllerProvider businessControllerProvider, IPermissionDefinitionService permissionDefinitionService, XmlNode nodeModule, XmlNode nodePane, int portalId, int tabId, PortalTemplateModuleAction mergeTabs, Hashtable hModules)
        {
			Logger.LogDebug(">>> Entering DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules})", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);
            var moduleDefinition = GetModuleDefinition(nodeModule);

			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): moduleDefinition → {ModuleDefinition}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, moduleDefinition);
            // will be instance or module?
			Logger.LogDebug("<<< Exiting DeserializeModule(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules})", businessControllerProvider, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);
            int templateModuleId = XmlUtils.GetNodeValueInt(nodeModule, "moduleID");
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): templateModuleId → {TemplateModuleId}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, templateModuleId);
            bool isInstance = CheckIsInstance(templateModuleId, hModules);
			Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): isInstance → {IsInstance}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, isInstance);
            if (moduleDefinition != null)
            {
                // If Mode is Merge Check if Module exists
                if (!FindModule(nodeModule, tabId, mergeTabs))
                {
                    ModuleInfo module = DeserializeModule(nodeModule, nodePane, portalId, tabId, moduleDefinition.ModuleDefID);
					Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): module → {Module}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, module);

                    // if the module is marked as show on all tabs, then check whether the module is exist in current website and it also
                    // still marked as shown on all tabs, this action will make sure there is no duplicate modules created on new tab.
                    if (module.AllTabs)
                    {
                        var existModule = Instance.GetModule(templateModuleId, Null.NullInteger, false);
						Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): existModule → {ExistModule}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, existModule);
                        if (existModule != null && !existModule.IsDeleted && existModule.AllTabs && existModule.PortalID == portalId)
                        {
                            return;
                        }
                    }

                    // deserialize Module's settings
                    XmlNodeList nodeModuleSettings = nodeModule.SelectNodes("modulesettings/modulesetting");
					Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): nodeModuleSettings → {NodeModuleSettings}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, nodeModuleSettings);
                    DeserializeModuleSettings(nodeModuleSettings, module);
                    XmlNodeList nodeTabModuleSettings = nodeModule.SelectNodes("tabmodulesettings/tabmodulesetting");
					Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): nodeTabModuleSettings → {NodeTabModuleSettings}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, nodeTabModuleSettings);
                    DeserializeTabModuleSettings(nodeTabModuleSettings, module);

                    // DNN-24983 get culture from page
                    var tabInfo = TabController.Instance.GetTab(tabId, portalId, false);
					Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): tabInfo → {TabInfo}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, tabInfo);
                    if (tabInfo != null)
                    {
                        module.CultureCode = tabInfo.CultureCode;
						Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): module.CultureCode → {Arg8}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, module.CultureCode);
                    }

                    int intModuleId;
                    if (!isInstance)
                    {
                        // Add new module
                        intModuleId = Instance.AddModule(module);
                        if (templateModuleId > 0)
						Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): intModuleId → {IntModuleId}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, intModuleId);
                        {
                            hModules.Add(templateModuleId, intModuleId);
                        }
                    }
                    else
                    {
                        // Add instance
                        module.ModuleID = Convert.ToInt32(hModules[templateModuleId], CultureInfo.InvariantCulture);
                        intModuleId = Instance.AddModule(module);
                    }

                    // save localization info
                    string oldGuid = XmlUtils.GetNodeValue(nodeModule, "uniqueId");
					Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): oldGuid → {OldGuid}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, oldGuid);
                    if (!ParsedLocalizedModuleGuid.ContainsKey(oldGuid))
                    {
                        ParsedLocalizedModuleGuid.Add(oldGuid, module.UniqueId.ToString());
                    }

                    if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "content")) && !isInstance)
                    {
                        GetModuleContent(businessControllerProvider, nodeModule, intModuleId, tabId, portalId);
                    }

                    // Process permissions only once
                    if (!isInstance && portalId != Null.NullInteger)
                    {
                        XmlNodeList nodeModulePermissions = nodeModule.SelectNodes("modulepermissions/permission");
						Logger.LogDebug("DeserializeModule(businessControllerProvider={BusinessControllerProvider},permissionDefinitionService={PermissionDefinitionService},nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): nodeModulePermissions → {NodeModulePermissions}", businessControllerProvider, permissionDefinitionService, nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, nodeModulePermissions);
                        DeserializeModulePermissions(permissionDefinitionService, nodeModulePermissions, portalId, module);

                        // Persist the permissions to the Data base
                        ModulePermissionController.SaveModulePermissions(module);
                    }
                }
            }
        }

        /// <summary>Serializes the metadata of a module (and optionally its contents) to an XML node.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="xmlModule">The XML Document to use for the Module.</param>
        /// <param name="module">The ModuleInfo object to serialize.</param>
        /// <param name="includeContent">A flag that determines whether the content of the module is serialized.</param>
        /// <returns>An <see cref="XmlNode"/> representing the module.</returns>
        [DnnDeprecated(10, 2, 4, "Use overload taking IHostSettings")]
        public static partial XmlNode SerializeModule(IBusinessControllerProvider businessControllerProvider, XmlDocument xmlModule, ModuleInfo module, bool includeContent)
            => SerializeModule(businessControllerProvider, Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), xmlModule, module, includeContent);

        /// <summary>Serializes the metadata of a module (and optionally its contents) to an XML node.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="xmlModule">The XML Document to use for the Module.</param>
        /// <param name="module">The ModuleInfo object to serialize.</param>
        /// <param name="includeContent">A flag that determines whether the content of the module is serialized.</param>
        /// <returns>An <see cref="XmlNode"/> representing the module.</returns>
        public static XmlNode SerializeModule(IBusinessControllerProvider businessControllerProvider, IHostSettings hostSettings, XmlDocument xmlModule, ModuleInfo module, bool includeContent)
        {
			Logger.LogDebug(">>> Entering SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent})", businessControllerProvider, hostSettings, xmlModule, module, includeContent);
            var serializer = new XmlSerializer(typeof(ModuleInfo));
            var sw = new StringWriter();
			Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): sw → {Sw}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, sw);
			Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): serializer → {Serializer}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, serializer);
            serializer.Serialize(sw, module);
            using (var xmlReader = XmlReader.Create(new StringReader(sw.GetStringBuilder().ToString()), new XmlReaderSettings { XmlResolver = null, }))
            {
                xmlModule.Load(xmlReader);
            }

            XmlNode moduleNode = xmlModule.SelectSingleNode("module");
			Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): moduleNode → {ModuleNode}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, moduleNode);
            if (moduleNode != null)
            {
                // ReSharper disable AssignNullToNotNullAttribute
                if (moduleNode.Attributes != null)
                {
                    moduleNode.Attributes.Remove(moduleNode.Attributes["xmlns:xsd"]);
                    moduleNode.Attributes.Remove(moduleNode.Attributes["xmlns:xsi"]);
                }

                // remove unwanted elements
                moduleNode.RemoveChildNode("portalid");
                moduleNode.RemoveChildNode("tabid");
                moduleNode.RemoveChildNode("tabmoduleid");
                moduleNode.RemoveChildNode("moduleorder");
                moduleNode.RemoveChildNode("panename");
                moduleNode.RemoveChildNode("isdeleted");
                moduleNode.RemoveChildNode("versionGuid");
                moduleNode.RemoveChildNode("localizedVersionGuid");
                moduleNode.RemoveChildNode("content");

                // support for localized templates
                ////moduleNode.RemoveChild(moduleNode.SelectSingleNode("uniqueId"));
                ////moduleNode.RemoveChild(moduleNode.SelectSingleNode("defaultLanguageGuid"));
                ////moduleNode.RemoveChild(moduleNode.SelectSingleNode("cultureCode"));
                if (Null.IsNull(module.DefaultLanguageGuid))
                {
                    moduleNode.RemoveChild(moduleNode.SelectSingleNode("defaultLanguageGuid"));
                }

                var xmlNodeList = moduleNode.SelectNodes("modulepermissions/permission");
				Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): xmlNodeList → {XmlNodeList}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, xmlNodeList);
                if (xmlNodeList != null)
                {
                    foreach (XmlNode nodePermission in xmlNodeList)
                    {
                        nodePermission.RemoveChildNode("modulepermissionid");
                        nodePermission.RemoveChildNode("permissionid");
                        nodePermission.RemoveChildNode("moduleid");
                        nodePermission.RemoveChildNode("roleid");
                        nodePermission.RemoveChildNode("userid");
                        nodePermission.RemoveChildNode("username");
                        nodePermission.RemoveChildNode("displayname");
                    }
                }

                if (includeContent)
                {
                    AddContent(businessControllerProvider, moduleNode, module);
                }

                // serialize ModuleSettings and TabModuleSettings
                XmlUtils.SerializeHashtable(module.ModuleSettings, xmlModule, moduleNode, "modulesetting", "settingname", "settingvalue");
                XmlUtils.SerializeHashtable(module.TabModuleSettings, xmlModule, moduleNode, "tabmodulesetting", "settingname", "settingvalue");

                // ReSharper restore AssignNullToNotNullAttribute
            }

            var newNode = xmlModule.CreateElement("definition");
			Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): newNode → {NewNode}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, newNode);
            var objModuleDef = ModuleDefinitionController.GetModuleDefinitionByID(module.ModuleDefID);
			Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): objModuleDef → {ObjModuleDef}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, objModuleDef);
            newNode.InnerText = DesktopModuleController.GetDesktopModule(hostSettings, objModuleDef.DesktopModuleID, module.PortalID).ModuleName;
            moduleNode?.AppendChild(newNode);

            // Add Module Definition Info
            var definitionNode = xmlModule.CreateElement("moduledefinition");
			Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): definitionNode → {DefinitionNode}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, definitionNode);
			Logger.LogDebug("SerializeModule(businessControllerProvider={BusinessControllerProvider},hostSettings={HostSettings},xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): definitionNode.InnerText → {Arg5}", businessControllerProvider, hostSettings, xmlModule, module, includeContent, definitionNode.InnerText);
            definitionNode.InnerText = objModuleDef.FriendlyName;
            moduleNode?.AppendChild(definitionNode);

			Logger.LogDebug("<<< Exiting SerializeModule(businessControllerProvider={BusinessControllerProvider},xmlModule={XmlModule},module={Module},includeContent={IncludeContent})", businessControllerProvider, xmlModule, module, includeContent);
            return moduleNode;
        }

        /// <summary>Synchronizes the module content between cache and database.</summary>
        /// <param name="moduleID">The module ID.</param>
        public static void SynchronizeModule(int moduleID)
        {
			Logger.LogDebug(">>> Entering SynchronizeModule(moduleID={ModuleID})", moduleID);
            var modules = Instance.GetTabModulesByModule(moduleID);
            foreach (ModuleInfo module in modules)
            {
			Logger.LogDebug("SynchronizeModule(moduleID={ModuleID}): modules → {Modules}", moduleID, modules);
                Hashtable tabSettings = TabController.Instance.GetTabSettings(module.TabID);
				Logger.LogDebug("SynchronizeModule(moduleID={ModuleID}): tabSettings → {TabSettings}", moduleID, tabSettings);
                if (tabSettings["CacheProvider"] != null && tabSettings["CacheProvider"].ToString().Length > 0)
                {
                    var outputProvider = OutputCachingProvider.Instance(tabSettings["CacheProvider"].ToString());
					Logger.LogDebug("SynchronizeModule(moduleID={ModuleID}): outputProvider → {OutputProvider}", moduleID, outputProvider);
                    if (outputProvider != null)
                    {
                        outputProvider.Remove(module.TabID);
                    }
                }

                if (module.CacheTime > 0)
                {
                    var moduleProvider = ModuleCachingProvider.Instance(module.GetEffectiveCacheMethod());
					Logger.LogDebug("SynchronizeModule(moduleID={ModuleID}): moduleProvider → {ModuleProvider}", moduleID, moduleProvider);
                    if (moduleProvider != null)
                    {
                        moduleProvider.Remove(module.TabModuleID);
                    }
                }

                // Synchronize module is called when a module needs to indicate that the content
                // has changed and the cache's should be refreshed.  So we can update the Version
                // and also the LastContentModificationDate
                UpdateTabModuleVersion(module.TabModuleID);
                DataProvider.UpdateModuleLastContentModifiedOnDate(module.ModuleID);

                // We should also indicate that the Transalation Status has changed
                if (PortalController.GetPortalSettingAsBoolean("ContentLocalizationEnabled", module.PortalID, false))
                {
                    Instance.UpdateTranslationStatus(module, false);
                }

                // and clear the cache
                Instance.ClearCache(module.TabID);
            }
			Logger.LogDebug("<<< Exiting SynchronizeModule(moduleID={ModuleID})", moduleID);
        }

        /// <summary>Check if a ModuleInfo belongs to the referenced Tab or not.</summary>
        /// <param name="module">A ModuleInfo object to be checked.</param>
        /// <returns>True is TabId points to a different tab from initial Tab where the module was added. Otherwise, False.</returns>
        public bool IsSharedModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering IsSharedModule(module={Module})", module);
            var contentController = Util.GetContentController();
            var content = contentController.GetContentItem(module.ContentItemId);
			Logger.LogDebug("IsSharedModule(module={Module}): content → {Content}", module, content);
			Logger.LogDebug("IsSharedModule(module={Module}): contentController → {ContentController}", module, contentController);
			Logger.LogDebug("<<< Exiting IsSharedModule(module={Module})", module);
            return module.TabID != content.TabID;
        }

        /// <summary>Get the Tab ID corresponding to the initial Tab where the module was added.</summary>
        /// <param name="module">A ModuleInfo object to be checked.</param>
        /// <returns>The Tab Id from initial Tab where the module was added.</returns>
        public int GetMasterTabId(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering GetMasterTabId(module={Module})", module);
            var contentController = Util.GetContentController();
            var content = contentController.GetContentItem(module.ContentItemId);
			Logger.LogDebug("GetMasterTabId(module={Module}): content → {Content}", module, content);
			Logger.LogDebug("GetMasterTabId(module={Module}): contentController → {ContentController}", module, contentController);
			Logger.LogDebug("<<< Exiting GetMasterTabId(module={Module})", module);
            return content.TabID;
        }

        /// <summary>add a module to a page.</summary>
        /// <param name="module">moduleInfo for the module to create.</param>
        /// <returns>ID of the created module.</returns>
        public int AddModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering AddModule(module={Module})", module);
            // add module
            this.AddModuleInternal(module);

            var currentUser = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("AddModule(module={Module}): currentUser → {CurrentUser}", module, currentUser);

            // Lets see if the module already exists
            ModuleInfo tmpModule = this.GetModule(module.ModuleID, module.TabID, false);
			Logger.LogDebug("AddModule(module={Module}): tmpModule → {TmpModule}", module, tmpModule);
            if (tmpModule != null)
            {
                // Module Exists already
                if (tmpModule.IsDeleted)
                {
                    var order = module.ModuleOrder;
                    var pane = module.PaneName;

                    // Restore Module
                    this.RestoreModule(module);

                    TabChangeTracker.Instance.TrackModuleAddition(module, 1, currentUser.UserID);

                    // Set Module Order as expected
                    this.UpdateModuleOrder(module.TabID, module.ModuleID, order, pane);
                    this.UpdateTabModuleOrder(module.TabID);
                }
            }
            else
            {
                // add tabmodule
                DataProvider.AddTabModule(
                    module.TabID,
                    module.ModuleID,
                    module.ModuleTitle,
                    module.Header,
                    module.Footer,
                    module.ModuleOrder,
                    module.PaneName,
                    module.CacheTime,
                    module.CacheMethod,
                    module.Alignment,
                    module.Color,
                    module.Border,
                    module.IconFile,
                    (int)module.Visibility,
                    module.ContainerSrc,
                    module.DisplayTitle,
                    module.DisplayPrint,
                    module.DisplaySyndicate,
                    module.UniqueId,
                    module.VersionGuid,
                    module.DefaultLanguageGuid,
                    module.LocalizedVersionGuid,
                    module.CultureCode,
                    currentUser.UserID);

                var log = new LogInfo
                {
                    LogTypeKey = nameof(EventLogType.TABMODULE_CREATED),
                    LogPortalID = module.PortalID,
                };
                log.LogProperties.Add(new LogDetailInfo("TabPath", module.ParentTab.TabPath));
                log.LogProperties.Add(new LogDetailInfo("Module Type", module.ModuleDefinition.FriendlyName));
                log.LogProperties.Add(new LogDetailInfo("TabId", module.TabID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("ModuleID", module.ModuleID.ToString(CultureInfo.InvariantCulture)));
                LogController.Instance.AddLog(log);

                TabChangeTracker.Instance.TrackModuleAddition(module, 1, currentUser.UserID);

                if (module.ModuleOrder == -1)
                {
                    // position module at bottom of pane
                    this.UpdateModuleOrder(module.TabID, module.ModuleID, module.ModuleOrder, module.PaneName);
                }
                else
                {
                    // position module in pane
                    this.UpdateTabModuleOrder(module.TabID);
                }
            }

            // Save ModuleSettings
            if (module.TabModuleID == -1)
            {
                if (tmpModule == null)
                {
                    tmpModule = this.GetModule(module.ModuleID, module.TabID, false);
                }

                module.TabModuleID = tmpModule.TabModuleID;
				Logger.LogDebug("AddModule(module={Module}): module.TabModuleID → {Arg1}", module, module.TabModuleID);
            }

            this.UpdateTabModuleSettings(module);

            this.ClearCache(module.TabID);
			Logger.LogDebug("<<< Exiting AddModule(module={Module})", module);
            return module.ModuleID;
        }

        /// <summary>Clears the cache.</summary>
        /// <param name="tabId">The tab id.</param>
        public void ClearCache(int tabId)
        {
			Logger.LogDebug(">>> Entering ClearCache(tabId={TabId})", tabId);
            DataCache.ClearModuleCache(tabId);
			Logger.LogDebug("<<< Exiting ClearCache(tabId={TabId})", tabId);
        }

        /// <summary>Copies the module to a new page.</summary>
        /// <param name="sourceModule">The source module.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="toPaneName">Name of to pane.</param>
        /// <param name="includeSettings">if set to <see langword="true"/> include settings.</param>
        public void CopyModule(ModuleInfo sourceModule, TabInfo destinationTab, string toPaneName, bool includeSettings)
        {
			Logger.LogDebug(">>> Entering CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings})", sourceModule, destinationTab, toPaneName, includeSettings);
            PortalInfo portal = PortalController.Instance.GetPortal(destinationTab.PortalID);

			Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): portal → {Portal}", sourceModule, destinationTab, toPaneName, includeSettings, portal);
            // Clone Module
            ModuleInfo destinationModule = sourceModule.Clone();
			Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): destinationModule → {DestinationModule}", sourceModule, destinationTab, toPaneName, includeSettings, destinationModule);
            if (!string.IsNullOrEmpty(toPaneName))
            {
                destinationModule.PaneName = toPaneName;
				Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): destinationModule.PaneName → {Arg4}", sourceModule, destinationTab, toPaneName, includeSettings, destinationModule.PaneName);
            }

            destinationModule.TabID = destinationTab.TabID;
			Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): destinationModule.TabID → {Arg4}", sourceModule, destinationTab, toPaneName, includeSettings, destinationModule.TabID);

            // The new reference copy should have the same culture as the destination Tab
            destinationModule.UniqueId = Guid.NewGuid();
            destinationModule.CultureCode = destinationTab.CultureCode;
			Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): destinationModule.CultureCode → {Arg4}", sourceModule, destinationTab, toPaneName, includeSettings, destinationModule.CultureCode);
            destinationModule.VersionGuid = Guid.NewGuid();
            destinationModule.LocalizedVersionGuid = Guid.NewGuid();

            // Figure out the DefaultLanguage Guid
            if (!string.IsNullOrEmpty(sourceModule.CultureCode) && sourceModule.CultureCode == portal.DefaultLanguage && destinationModule.CultureCode != sourceModule.CultureCode &&
                !string.IsNullOrEmpty(destinationModule.CultureCode))
            {
                // Tab is localized so set Default language Guid reference
                destinationModule.DefaultLanguageGuid = sourceModule.UniqueId;
				Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): destinationModule.DefaultLanguageGuid → {Arg4}", sourceModule, destinationTab, toPaneName, includeSettings, destinationModule.DefaultLanguageGuid);
            }
            else if (!string.IsNullOrEmpty(sourceModule.CultureCode) && sourceModule.CultureCode != portal.DefaultLanguage && destinationModule.CultureCode != sourceModule.CultureCode &&
                     !string.IsNullOrEmpty(destinationModule.CultureCode))
            {
                // tab is localized, but the source is not in the default language (it was on a single culture page)
                // this wires up all the connections
                sourceModule.DefaultLanguageGuid = destinationModule.UniqueId;
				Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): sourceModule.DefaultLanguageGuid → {Arg4}", sourceModule, destinationTab, toPaneName, includeSettings, sourceModule.DefaultLanguageGuid);
                this.UpdateModule(sourceModule);
            }
            else if (sourceModule.AllTabs && sourceModule.CultureCode != portal.DefaultLanguage)
            {
                if (sourceModule.DefaultLanguageModule != null && destinationTab.DefaultLanguageTab != null)
                {
                    ModuleInfo defaultLanguageModule = this.GetModule(sourceModule.DefaultLanguageModule.ModuleID, destinationTab.DefaultLanguageTab.TabID, false);
					Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): defaultLanguageModule → {DefaultLanguageModule}", sourceModule, destinationTab, toPaneName, includeSettings, defaultLanguageModule);

                    if (defaultLanguageModule != null)
                    {
                        destinationModule.DefaultLanguageGuid = defaultLanguageModule.UniqueId;
                    }
                }
            }

            // This will fail if the page already contains this module
            try
            {
                var userId = UserController.Instance.GetCurrentUserInfo().UserID;
				Logger.LogDebug("CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings}): userId → {UserId}", sourceModule, destinationTab, toPaneName, includeSettings, userId);

                // Add a copy of the module to the bottom of the Pane for the new Tab
                DataProvider.AddTabModule(
                    destinationModule.TabID,
                    destinationModule.ModuleID,
                    destinationModule.ModuleTitle,
                    destinationModule.Header,
                    destinationModule.Footer,
                    destinationModule.ModuleOrder,
                    destinationModule.PaneName,
                    destinationModule.CacheTime,
                    destinationModule.CacheMethod,
                    destinationModule.Alignment,
                    destinationModule.Color,
                    destinationModule.Border,
                    destinationModule.IconFile,
                    (int)destinationModule.Visibility,
                    destinationModule.ContainerSrc,
                    destinationModule.DisplayTitle,
                    destinationModule.DisplayPrint,
                    destinationModule.DisplaySyndicate,
                    destinationModule.UniqueId,
                    destinationModule.VersionGuid,
                    destinationModule.DefaultLanguageGuid,
                    destinationModule.LocalizedVersionGuid,
                    destinationModule.CultureCode,
                    userId);
                TabChangeTracker.Instance.TrackModuleCopy(destinationModule, 1, sourceModule.TabID, userId);

                // Optionally copy the TabModuleSettings
                if (includeSettings)
                {
                    this.CopyTabModuleSettingsInternal(sourceModule, destinationModule);
                }
            }
            catch (Exception exc)
            {
                // module already in the page, ignore error
                Logger.Error(exc);
            }

            this.ClearCache(sourceModule.TabID);
            this.ClearCache(destinationTab.TabID);

            // Optionally copy the TabModuleSettings
            if (includeSettings)
            {
                destinationModule = this.GetModule(destinationModule.ModuleID, destinationModule.TabID, false);
                this.CopyTabModuleSettingsInternal(sourceModule, destinationModule);
            }
			Logger.LogDebug("<<< Exiting CopyModule(sourceModule={SourceModule},destinationTab={DestinationTab},toPaneName={ToPaneName},includeSettings={IncludeSettings})", sourceModule, destinationTab, toPaneName, includeSettings);
        }

        /// <summary>Copies all modules in source page to a new page.</summary>
        /// <param name="sourceTab">The source tab.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="asReference">if set to <see langword="true"/> will use source module directly, else will create new module info by source module.</param>
        public void CopyModules(TabInfo sourceTab, TabInfo destinationTab, bool asReference)
        {
			Logger.LogDebug(">>> Entering CopyModules(sourceTab={SourceTab},destinationTab={DestinationTab},asReference={AsReference})", sourceTab, destinationTab, asReference);
            this.CopyModules(sourceTab, destinationTab, asReference, false);
			Logger.LogDebug("<<< Exiting CopyModules(sourceTab={SourceTab},destinationTab={DestinationTab},asReference={AsReference})", sourceTab, destinationTab, asReference);
        }

        /// <summary>Copies all modules in source page to a new page.</summary>
        /// <param name="sourceTab">The source tab.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="asReference">if set to <see langword="true"/> will use source module directly, else will create new module info by source module.</param>
        /// <param name="includeAllTabsMobules">if set to <see langword="true"/> will include modules which shown on all pages, this is used when create localized copy.</param>
        public void CopyModules(TabInfo sourceTab, TabInfo destinationTab, bool asReference, bool includeAllTabsMobules)
        {
			Logger.LogDebug(">>> Entering CopyModules(sourceTab={SourceTab},destinationTab={DestinationTab},asReference={AsReference},includeAllTabsMobules={IncludeAllTabsMobules})", sourceTab, destinationTab, asReference, includeAllTabsMobules);
            foreach (KeyValuePair<int, ModuleInfo> kvp in this.GetTabModules(sourceTab.TabID))
            {
                ModuleInfo sourceModule = kvp.Value;

                // if the module shows on all pages does not need to be copied since it will
                // be already added to this page
                if ((includeAllTabsMobules || !sourceModule.AllTabs) && !sourceModule.IsDeleted)
                {
                    if (!asReference)
                    {
                        // Deep Copy
                        var newModule = sourceModule.Clone();
						Logger.LogDebug("CopyModules(sourceTab={SourceTab},destinationTab={DestinationTab},asReference={AsReference},includeAllTabsMobules={IncludeAllTabsMobules}): newModule → {NewModule}", sourceTab, destinationTab, asReference, includeAllTabsMobules, newModule);
						Logger.LogDebug("CopyModules(sourceTab={SourceTab},destinationTab={DestinationTab},asReference={AsReference},includeAllTabsMobules={IncludeAllTabsMobules}): newModule.ModuleID → {Arg4}", sourceTab, destinationTab, asReference, includeAllTabsMobules, newModule.ModuleID);
						Logger.LogDebug("CopyModules(sourceTab={SourceTab},destinationTab={DestinationTab},asReference={AsReference},includeAllTabsMobules={IncludeAllTabsMobules}): newModule.TabID → {Arg4}", sourceTab, destinationTab, asReference, includeAllTabsMobules, newModule.TabID);
                        newModule.ModuleID = Null.NullInteger;
                        newModule.TabID = destinationTab.TabID;
                        this.AddModule(newModule);
                    }
                    else
                    {
                        // Shallow (Reference Copy)
                        this.CopyModule(sourceModule, destinationTab, Null.NullString, true);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting CopyModules(sourceTab={SourceTab},destinationTab={DestinationTab},asReference={AsReference},includeAllTabsMobules={IncludeAllTabsMobules})", sourceTab, destinationTab, asReference, includeAllTabsMobules);
        }

        /// <inheritdoc />
        public void CreateContentItem(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering CreateContentItem(module={Module})", module);
            ContentType contentType = ContentType.Module;

            // This module does not have a valid ContentItem
            // create ContentItem
            IContentController contentController = Util.GetContentController();
			Logger.LogDebug("CreateContentItem(module={Module}): contentController → {ContentController}", module, contentController);
			Logger.LogDebug("CreateContentItem(module={Module}): module.Content → {Arg1}", module, module.Content);
			Logger.LogDebug("CreateContentItem(module={Module}): module.Indexed → {Arg1}", module, module.Indexed);
            module.Content = module.ModuleTitle;
            module.Indexed = false;
            if (contentType != null)
            {
                module.ContentTypeId = contentType.ContentTypeId;
				Logger.LogDebug("CreateContentItem(module={Module}): module.ContentTypeId → {Arg1}", module, module.ContentTypeId);
            }

            module.ContentItemId = contentController.AddContentItem(module);
			Logger.LogDebug("<<< Exiting CreateContentItem(module={Module})", module);
        }

        /// <inheritdoc />
        public void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs, bool softDelete, bool includeCurrent, bool deleteBaseModule)
        {
			Logger.LogDebug(">>> Entering DeleteAllModules(moduleId={ModuleId},tabId={TabId},fromTabs={FromTabs},softDelete={SoftDelete},includeCurrent={IncludeCurrent},deleteBaseModule={DeleteBaseModule})", moduleId, tabId, fromTabs, softDelete, includeCurrent, deleteBaseModule);
            var moduleInfo = this.GetModule(moduleId, tabId, false);

			Logger.LogDebug("DeleteAllModules(moduleId={ModuleId},tabId={TabId},fromTabs={FromTabs},softDelete={SoftDelete},includeCurrent={IncludeCurrent},deleteBaseModule={DeleteBaseModule}): moduleInfo → {ModuleInfo}", moduleId, tabId, fromTabs, softDelete, includeCurrent, deleteBaseModule, moduleInfo);
            // Iterate through collection deleting the module from each Tab (except the current)
            foreach (TabInfo objTab in fromTabs)
            {
                if (objTab.TabID != tabId || includeCurrent)
                {
                    this.UncopyModule(objTab.TabID, moduleId, softDelete, tabId); // uncopy existing modules
                }
            }

            // Optionally delete the Module
            if (includeCurrent && deleteBaseModule && !softDelete)
            {
                this.DeleteModule(moduleId);
                this.ClearCache(tabId);
            }
            else
            {
                this.ClearCache(tabId);

                // ModuleRemove is only raised when doing a soft delete of the module
                if (softDelete)
                {
                    EventManager.Instance.OnModuleRemoved(new ModuleEventArgs { Module = moduleInfo });
                }
            }
			Logger.LogDebug("<<< Exiting DeleteAllModules(moduleId={ModuleId},tabId={TabId},fromTabs={FromTabs},softDelete={SoftDelete},includeCurrent={IncludeCurrent},deleteBaseModule={DeleteBaseModule})", moduleId, tabId, fromTabs, softDelete, includeCurrent, deleteBaseModule);
        }

        /// <summary>Delete a module instance permanently from the database.</summary>
        /// <param name="moduleId">ID of the module instance.</param>
        public void DeleteModule(int moduleId)
        {
			Logger.LogDebug(">>> Entering DeleteModule(moduleId={ModuleId})", moduleId);
            // Get the module
            ModuleInfo module = this.GetModule(moduleId, Null.NullInteger, true);
			Logger.LogDebug("DeleteModule(moduleId={ModuleId}): module → {Module}", moduleId, module);

            // Delete Module
            DataProvider.DeleteModule(moduleId);

            // Remove the Content Item
            if (module != null && module.ContentItemId > Null.NullInteger)
            {
                IContentController contentController = Util.GetContentController();
				Logger.LogDebug("DeleteModule(moduleId={ModuleId}): contentController → {ContentController}", moduleId, contentController);
                contentController.DeleteContentItem(module);
            }

            // Log deletion
            this.eventLogger.AddLog(
                "ModuleId",
                moduleId.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.MODULE_DELETED);

            // queue remove module from search index
            var document = new SearchDocumentToDelete
            {
                ModuleId = moduleId,
            };

            DataProvider.Instance().AddSearchDeletedItems(document);

            EventManager.Instance.OnModuleDeleted(new ModuleEventArgs { Module = module });
			Logger.LogDebug("<<< Exiting DeleteModule(moduleId={ModuleId})", moduleId);
        }

        /// <summary>Delete a Setting of a module instance.</summary>
        /// <param name="moduleId">ID of the affected module.</param>
        /// <param name="settingName">Name of the setting to be deleted.</param>
        public void DeleteModuleSetting(int moduleId, string settingName)
        {
			Logger.LogDebug(">>> Entering DeleteModuleSetting(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
            DataProvider.DeleteModuleSetting(moduleId, settingName);
            var log = new LogInfo { LogTypeKey = nameof(EventLogType.MODULE_SETTING_DELETED) };
			Logger.LogDebug("DeleteModuleSetting(moduleId={ModuleId},settingName={SettingName}): log → {Log}", moduleId, settingName, log);
            log.LogProperties.Add(new LogDetailInfo("ModuleId", moduleId.ToString(CultureInfo.InvariantCulture)));
            log.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            LogController.Instance.AddLog(log);
            this.UpdateTabModuleVersionsByModuleID(moduleId);
            ClearModuleSettingsCache(moduleId);
			Logger.LogDebug("<<< Exiting DeleteModuleSetting(moduleId={ModuleId},settingName={SettingName})", moduleId, settingName);
        }

        /// <summary>
        /// Delete a module reference permanently from the database.
        /// if there are no other references, the module instance is deleted as well.
        /// </summary>
        /// <param name="tabId">ID of the page.</param>
        /// <param name="moduleId">ID of the module instance.</param>
        /// <param name="softDelete">A flag that determines whether the instance should be soft-deleted.</param>
        public void DeleteTabModule(int tabId, int moduleId, bool softDelete)
        {
			Logger.LogDebug(">>> Entering DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete})", tabId, moduleId, softDelete);
            ModuleInfo moduleInfo = this.GetModule(moduleId, tabId, false);
            this.DeleteTabModuleInternal(moduleInfo, softDelete);
			Logger.LogDebug("DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete}): moduleInfo → {ModuleInfo}", tabId, moduleId, softDelete, moduleInfo);
            var userId = UserController.Instance.GetCurrentUserInfo().UserID;
			Logger.LogDebug("DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete}): userId → {UserId}", tabId, moduleId, softDelete, userId);
            if (softDelete)
            {
                TabChangeTracker.Instance.TrackModuleDeletion(moduleInfo, Null.NullInteger, userId);
            }
			Logger.LogDebug("<<< Exiting DeleteTabModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete})", tabId, moduleId, softDelete);
        }

        /// <summary>Delete a specific setting of a tabmodule reference.</summary>
        /// <param name="tabModuleId">ID of the affected tabmodule.</param>
        /// <param name="settingName">Name of the setting to remove.</param>
        public void DeleteTabModuleSetting(int tabModuleId, string settingName)
        {
			Logger.LogDebug(">>> Entering DeleteTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
            DataProvider.DeleteTabModuleSetting(tabModuleId, settingName);
            UpdateTabModuleVersion(tabModuleId);
            var log = new LogInfo
            {
                LogTypeKey = nameof(EventLogType.TABMODULE_SETTING_DELETED),
            };
            log.LogProperties.Add(new LogDetailInfo("TabModuleId", tabModuleId.ToString(CultureInfo.InvariantCulture)));
            log.LogProperties.Add(new LogDetailInfo("SettingName", settingName));
            LogController.Instance.AddLog(log);
            ClearTabModuleSettingsCache(tabModuleId, settingName);
			Logger.LogDebug("<<< Exiting DeleteTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
        }

        /// <summary>Des the localize module.</summary>
        /// <param name="sourceModule">The source module.</param>
        /// <returns>new module id.</returns>
        public int DeLocalizeModule(ModuleInfo sourceModule)
        {
			Logger.LogDebug(">>> Entering DeLocalizeModule(sourceModule={SourceModule})", sourceModule);
            int moduleId = Null.NullInteger;

			Logger.LogDebug("DeLocalizeModule(sourceModule={SourceModule}): moduleId → {ModuleId}", sourceModule, moduleId);
            if (sourceModule is { DefaultLanguageModule: not null, })
            {
                // clone the module object ( to avoid creating an object reference to the data cache )
                ModuleInfo newModule = sourceModule.Clone();
				Logger.LogDebug("DeLocalizeModule(sourceModule={SourceModule}): newModule → {NewModule}", sourceModule, newModule);

                // Get the Module ID of the default language instance
                newModule.ModuleID = sourceModule.DefaultLanguageModule.ModuleID;
				Logger.LogDebug("DeLocalizeModule(sourceModule={SourceModule}): newModule.ModuleID → {Arg1}", sourceModule, newModule.ModuleID);

                if (newModule.ModuleID != sourceModule.ModuleID)
                {
                    // update tabmodule
                    DataProvider.UpdateTabModule(
                        newModule.TabModuleID,
                        newModule.TabID,
                        newModule.ModuleID,
                        newModule.ModuleTitle,
                        newModule.Header,
                        newModule.Footer,
                        newModule.ModuleOrder,
                        newModule.PaneName,
                        newModule.CacheTime,
                        newModule.CacheMethod,
                        newModule.Alignment,
                        newModule.Color,
                        newModule.Border,
                        newModule.IconFile,
                        (int)newModule.Visibility,
                        newModule.ContainerSrc,
                        newModule.DisplayTitle,
                        newModule.DisplayPrint,
                        newModule.DisplaySyndicate,
                        newModule.VersionGuid,
                        newModule.DefaultLanguageGuid,
                        newModule.LocalizedVersionGuid,
                        newModule.CultureCode,
                        UserController.Instance.GetCurrentUserInfo().UserID);

                    DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.SingleTabModuleCacheKey, newModule.TabModuleID));

                    // Update tab version details of old and new modules
                    var userId = UserController.Instance.GetCurrentUserInfo().UserID;
					Logger.LogDebug("DeLocalizeModule(sourceModule={SourceModule}): userId → {UserId}", sourceModule, userId);
                    TabChangeTracker.Instance.TrackModuleDeletion(sourceModule, Null.NullInteger, userId);
                    TabChangeTracker.Instance.TrackModuleCopy(newModule, Null.NullInteger, newModule.TabID, userId);

                    // check if all modules instances have been deleted
                    if (this.GetModule(sourceModule.ModuleID, Null.NullInteger, true).TabID == Null.NullInteger)
                    {
                        // delete the deep copy "module info"
                        this.DeleteModule(sourceModule.ModuleID);
                    }
                }

                moduleId = newModule.ModuleID;

                // Clear Caches
                this.ClearCache(newModule.TabID);
                this.ClearCache(sourceModule.TabID);
            }

			Logger.LogDebug(">>> Entering LocalizeModule(sourceModule={SourceModule},locale={Locale})", sourceModule, locale);
			Logger.LogDebug("LocalizeModule(sourceModule={SourceModule},locale={Locale}): localModule.ModuleID → {Arg2}", sourceModule, locale, localModule.ModuleID);
			Logger.LogDebug("<<< Exiting LocalizeModule(sourceModule={SourceModule},locale={Locale})", sourceModule, locale);
			Logger.LogDebug("<<< Exiting DeLocalizeModule(sourceModule={SourceModule})", sourceModule);
            return moduleId;
        }

        /// <summary>get info of all modules in any portal of the installation.</summary>
        /// <returns>moduleInfo of all modules.</returns>
        /// <remarks>created for upgrade purposes.</remarks>
        public ArrayList GetAllModules()
        {
			Logger.LogDebug(">>> Entering GetAllModules()");
			Logger.LogDebug("<<< Exiting GetAllModules()");
            return CBO.FillCollection(DataProvider.GetAllModules(), typeof(ModuleInfo));
        }

        /// <summary>get Module objects of a portal, either only those, to be placed on all tabs or not.</summary>
        /// <param name="portalID">ID of the portal.</param>
        /// <param name="allTabs">specify, whether to return modules to be shown on all tabs or those to be shown on specified tabs.</param>
        /// <returns>ArrayList of TabModuleInfo objects.</returns>
        public ArrayList GetAllTabsModules(int portalID, bool allTabs)
        {
			Logger.LogDebug(">>> Entering GetAllTabsModules(portalID={PortalID},allTabs={AllTabs})", portalID, allTabs);
			Logger.LogDebug("<<< Exiting GetAllTabsModules(portalID={PortalID},allTabs={AllTabs})", portalID, allTabs);
            return CBO.FillCollection(DataProvider.GetAllTabsModules(portalID, allTabs), typeof(ModuleInfo));
        }

        /// <summary>  get TabModule objects that are linked to a particular ModuleID.</summary>
        /// <param name="moduleID">ID of the module.</param>
        /// <returns>ArrayList of TabModuleInfo objects.</returns>
        public ArrayList GetAllTabsModulesByModuleID(int moduleID)
        {
			Logger.LogDebug(">>> Entering GetAllTabsModulesByModuleID(moduleID={ModuleID})", moduleID);
			Logger.LogDebug("<<< Exiting GetAllTabsModulesByModuleID(moduleID={ModuleID})", moduleID);
            return CBO.FillCollection(DataProvider.GetAllTabsModulesByModuleID(moduleID), typeof(ModuleInfo));
        }

        /// <summary>get a Module object.</summary>
        /// <param name="moduleID">ID of the module.</param>
        /// <returns>ModuleInfo object.</returns>
        /// <remarks>This overload ignores any cached values and always retrieves the latest data
        /// from the database.</remarks>
        public ModuleInfo GetModule(int moduleID)
        {
			Logger.LogDebug(">>> Entering GetModule(moduleID={ModuleID})", moduleID);
			Logger.LogDebug("<<< Exiting GetModule(moduleID={ModuleID})", moduleID);
			Logger.LogDebug("<<< Exiting GetModule(moduleID={ModuleID},tabID={TabID})", moduleID, tabID);
			Logger.LogDebug("<<< Exiting GetModule(moduleID={ModuleID},tabID={TabID},ignoreCache={IgnoreCache})", moduleID, tabID, ignoreCache);
            return this.GetModule(moduleID, Null.NullInteger, true);
        }

        /// <summary>get a Module object.</summary>
        /// <param name="moduleID">ID of the module.</param>
        /// <param name="tabID">ID of the page.</param>
        /// <returns>ModuleInfo object.</returns>
        public ModuleInfo GetModule(int moduleID, int tabID)
        {
            return this.GetModule(moduleID, tabID, false);
        }

        /// <summary>get a Module object.</summary>
        /// <param name="moduleID">ID of the module.</param>
        /// <param name="tabID">ID of the page.</param>
        /// <param name="ignoreCache">flag, if data shall not be taken from cache.</param>
        /// <returns>ModuleInfo object.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public ModuleInfo GetModule(int moduleID, int tabID, bool ignoreCache)
        {
            ModuleInfo modInfo = null;
            bool bFound = false;
            if (!ignoreCache)
            {
                // First try the cache
                var dicModules = this.GetTabModules(tabID);
                bFound = dicModules.TryGetValue(moduleID, out modInfo);
            }

            if (ignoreCache || !bFound)
            {
                modInfo = CBO.FillObject<ModuleInfo>(DataProvider.GetModule(moduleID, tabID));
            }

            return modInfo;
        }

        /// <summary>  get Module by specific locale.</summary>
        /// <param name="moduleId">ID of the module.</param>
        /// <param name="tabid">ID of the tab.</param>
        /// <param name="portalId">ID of the portal.</param>
        /// <param name="locale">The wanted locale.</param>
        /// <returns>ModuleInfo associated to submitted locale.</returns>
        public ModuleInfo GetModuleByCulture(int moduleId, int tabid, int portalId, Locale locale)
        {
			Logger.LogDebug(">>> Entering GetModuleByCulture(moduleId={ModuleId},tabid={Tabid},portalId={PortalId},locale={Locale})", moduleId, tabid, portalId, locale);
            ModuleInfo localizedModule = null;

			Logger.LogDebug("GetModuleByCulture(moduleId={ModuleId},tabid={Tabid},portalId={PortalId},locale={Locale}): localizedModule → {LocalizedModule}", moduleId, tabid, portalId, locale, localizedModule);
            // Get Module specified by Id
            ModuleInfo originalModule = this.GetModule(moduleId, tabid, false);
			Logger.LogDebug("GetModuleByCulture(moduleId={ModuleId},tabid={Tabid},portalId={PortalId},locale={Locale}): originalModule → {OriginalModule}", moduleId, tabid, portalId, locale, originalModule);

            if (locale != null && originalModule != null)
            {
                // Check if tab is in the requested culture
                if (string.IsNullOrEmpty(originalModule.CultureCode) || originalModule.CultureCode == locale.Code)
                {
                    localizedModule = originalModule;
                }
                else
                {
                    // See if tab exists for culture
                    if (originalModule.IsDefaultLanguage)
                    {
                        originalModule.LocalizedModules.TryGetValue(locale.Code, out localizedModule);
                    }
                    else
                    {
                        if (originalModule.DefaultLanguageModule != null)
                        {
                            if (originalModule.DefaultLanguageModule.CultureCode == locale.Code)
                            {
                                localizedModule = originalModule.DefaultLanguageModule;
                            }
                            else
                            {
                                if (!originalModule.DefaultLanguageModule.LocalizedModules.TryGetValue(locale.Code, out localizedModule))
                                {
                                    localizedModule = originalModule.DefaultLanguageModule;
                                }
                            }
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetModuleByCulture(moduleId={ModuleId},tabid={Tabid},portalId={PortalId},locale={Locale})", moduleId, tabid, portalId, locale);
            return localizedModule;
        }

        /// <summary>Get ModuleInfo object of first module instance with a given name of the module definition.</summary>
        /// <param name="portalId">ID of the portal, where to look for the module.</param>
        /// <param name="definitionName">The name of module definition (NOTE: this looks at <see cref="ModuleDefinitionInfo.DefinitionName"/>, not <see cref="ModuleDefinitionInfo.FriendlyName"/>).</param>
        /// <returns>ModuleInfo of first module instance.</returns>
        /// <remarks>preferably used for admin and host modules.</remarks>
        public ModuleInfo GetModuleByDefinition(int portalId, string definitionName)
        {
			Logger.LogDebug(">>> Entering GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName})", portalId, definitionName);
            // declare return object
            ModuleInfo module;

            // format cache key
            string key = string.Format(CultureInfo.InvariantCulture, DataCache.ModuleCacheKey, portalId);

            // get module dictionary from cache
            var modules = DataCache.GetCache<Dictionary<string, ModuleInfo>>(key) ?? new Dictionary<string, ModuleInfo>();
			Logger.LogDebug("GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName}): modules → {Modules}", portalId, definitionName, modules);
            if (!modules.TryGetValue(definitionName, out module))
            {
                // clone the dictionary so that we have a local copy
                var clonemodules = new Dictionary<string, ModuleInfo>();
				Logger.LogDebug("GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName}): clonemodules → {Clonemodules}", portalId, definitionName, clonemodules);
                foreach (ModuleInfo m in modules.Values)
                {
                    clonemodules[m.ModuleDefinition.DefinitionName] = m;
                }

                // get from database
                IDataReader dr = DataProvider.Instance().GetModuleByDefinition(portalId, definitionName);
				Logger.LogDebug("GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName}): dr → {Dr}", portalId, definitionName, dr);
                try
                {
                    // hydrate object
                    module = CBO.FillObject<ModuleInfo>(dr);
                }
                finally
                {
                    // close connection
                    CBO.CloseDataReader(dr, true);
                }

                if (module != null)
                {
                    // add the module to the dictionary
                    clonemodules[module.ModuleDefinition.FriendlyName] = module;

                    // set module caching settings
                    int timeOut = DataCache.ModuleCacheTimeOut * (int)Host.Host.PerformanceSetting;
					Logger.LogDebug("GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName}): timeOut → {TimeOut}", portalId, definitionName, timeOut);

                    // cache module dictionary
                    if (timeOut > 0)
                    {
                        DataCache.SetCache(key, clonemodules, TimeSpan.FromMinutes(timeOut));
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetModuleByDefinition(portalId={PortalId},definitionName={DefinitionName})", portalId, definitionName);
            return module;
        }

        /// <inheritdoc />
        public ModuleInfo GetModuleByUniqueID(Guid uniqueID)
        {
			Logger.LogDebug(">>> Entering GetModuleByUniqueID(uniqueID={UniqueID})", uniqueID);
			Logger.LogDebug("<<< Exiting GetModuleByUniqueID(uniqueID={UniqueID})", uniqueID);
            return CBO.FillObject<ModuleInfo>(DataProvider.GetModuleByUniqueID(uniqueID));
        }

        /// <summary>get all Module objects of a portal.</summary>
        /// <param name="portalID">ID of the portal.</param>
        /// <returns>ArrayList of ModuleInfo objects.</returns>
        public ArrayList GetModules(int portalID)
        {
			Logger.LogDebug(">>> Entering GetModules(portalID={PortalID})", portalID);
			Logger.LogDebug("<<< Exiting GetModules(portalID={PortalID})", portalID);
            return CBO.FillCollection(DataProvider.GetModules(portalID), typeof(ModuleInfo));
        }

        /// <summary>Gets the modules by definition.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="definitionName">The name of the module definition.</param>
        /// <returns>module collection.</returns>
        public ArrayList GetModulesByDefinition(int portalID, string definitionName)
        {
			Logger.LogDebug(">>> Entering GetModulesByDefinition(portalID={PortalID},definitionName={DefinitionName})", portalID, definitionName);
			Logger.LogDebug("<<< Exiting GetModulesByDefinition(portalID={PortalID},definitionName={DefinitionName})", portalID, definitionName);
            return CBO.FillCollection(DataProvider.Instance().GetModuleByDefinition(portalID, definitionName), typeof(ModuleInfo));
        }

        /// <summary>Gets the modules by DesktopModuleId.</summary>
        /// <param name="desktopModuleId">The Desktop Module Id.</param>
        /// <returns>module collection.</returns>
        public ArrayList GetModulesByDesktopModuleId(int desktopModuleId)
        {
			Logger.LogDebug(">>> Entering GetModulesByDesktopModuleId(desktopModuleId={DesktopModuleId})", desktopModuleId);
            var moduleDefinitions = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModuleId);
            var modules = new ArrayList();
			Logger.LogDebug("GetModulesByDesktopModuleId(desktopModuleId={DesktopModuleId}): modules → {Modules}", desktopModuleId, modules);
			Logger.LogDebug("GetModulesByDesktopModuleId(desktopModuleId={DesktopModuleId}): moduleDefinitions → {ModuleDefinitions}", desktopModuleId, moduleDefinitions);
            foreach (var moduleDefinition in moduleDefinitions)
            {
                var portals = PortalController.Instance.GetPortals();
				Logger.LogDebug("GetModulesByDesktopModuleId(desktopModuleId={DesktopModuleId}): portals → {Portals}", desktopModuleId, portals);
                foreach (PortalInfo portal in portals)
                {
                    modules.AddRange(this.GetModulesByDefinition(portal.PortalID, moduleDefinition.Value.DefinitionName));
                }
            }

			Logger.LogDebug("<<< Exiting GetModulesByDesktopModuleId(desktopModuleId={DesktopModuleId})", desktopModuleId);
            return modules;
        }

        /// <summary>
        /// For a portal get a list of all active module and tabmodule references that are Searchable
        /// either by inheriting from ModuleSearchBase or implementing the older ISearchable interface.
        /// </summary>
        /// <param name="portalID">ID of the portal to be searched.</param>
        /// <returns>Arraylist of ModuleInfo for modules supporting search.</returns>
        public ArrayList GetSearchModules(int portalID)
        {
			Logger.LogDebug(">>> Entering GetSearchModules(portalID={PortalID})", portalID);
			Logger.LogDebug("<<< Exiting GetSearchModules(portalID={PortalID})", portalID);
            return CBO.FillCollection(DataProvider.GetSearchModules(portalID), typeof(ModuleInfo));
        }

        /// <summary>  get a Module object.</summary>
        /// <param name="tabModuleID">ID of the tabmodule.</param>
        /// <returns>An ModuleInfo object.</returns>
        public ModuleInfo GetTabModule(int tabModuleID)
        {
			Logger.LogDebug(">>> Entering GetTabModule(tabModuleID={TabModuleID})", tabModuleID);
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.SingleTabModuleCacheKey, tabModuleID);
			Logger.LogDebug("GetTabModule(tabModuleID={TabModuleID}): cacheKey → {CacheKey}", tabModuleID, cacheKey);
			Logger.LogDebug("<<< Exiting GetTabModule(tabModuleID={TabModuleID})", tabModuleID);
            return CBO.GetCachedObject<ModuleInfo>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.TabModuleCacheTimeOut, DataCache.TabModuleCachePriority),
                _ => CBO.FillObject<ModuleInfo>(DataProvider.GetTabModule(tabModuleID)));
        }

        /// <inheritdoc />
        public Dictionary<int, ModuleInfo> GetTabModules(int tabId)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.TabModuleCacheKey, tabId);
            return CBO.GetCachedObject<Dictionary<int, ModuleInfo>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.TabModuleCacheTimeOut, DataCache.TabModuleCachePriority),
                _ => GetModulesCurrentPage(tabId));
        }

        /// <summary>  Get a list of all TabModule references of a module instance.</summary>
        /// <param name="moduleID">ID of the Module.</param>
        /// <returns>ArrayList of ModuleInfo.</returns>
        public IList<ModuleInfo> GetTabModulesByModule(int moduleID)
        {
			Logger.LogDebug(">>> Entering GetTabModulesByModule(moduleID={ModuleID})", moduleID);
			Logger.LogDebug("<<< Exiting GetTabModulesByModule(moduleID={ModuleID})", moduleID);
            return CBO.FillCollection<ModuleInfo>(DataProvider.GetModule(moduleID, Null.NullInteger));
        }

        /// <inheritdoc />
        public void InitialModulePermission(ModuleInfo module, int tabId, int permissionType)
        {
			Logger.LogDebug(">>> Entering InitialModulePermission(module={Module},tabId={TabId},permissionType={PermissionType})", module, tabId, permissionType);
            var tabPermissions = TabPermissionController.GetTabPermissions(tabId, module.PortalID);

			Logger.LogDebug("InitialModulePermission(module={Module},tabId={TabId},permissionType={PermissionType}): tabPermissions → {TabPermissions}", module, tabId, permissionType, tabPermissions);
            module.InheritViewPermissions = permissionType == 0;

            // get the default module view permissions
            var systemModuleViewPermission = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW").FirstOrDefault();
			Logger.LogDebug("InitialModulePermission(module={Module},tabId={TabId},permissionType={PermissionType}): systemModuleViewPermission → {SystemModuleViewPermission}", module, tabId, permissionType, systemModuleViewPermission);

            // get the permissions from the page
            foreach (IPermissionInfo tabPermission in tabPermissions)
            {
                if (tabPermission.PermissionKey == "VIEW" && permissionType == 0)
                {
                    // Don't need to explicitly add View permissions if "Same As Page"
                    continue;
                }

                // get the system module permissions for the permission key
                var systemModulePermissions = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", tabPermission.PermissionKey);
				Logger.LogDebug("InitialModulePermission(module={Module},tabId={TabId},permissionType={PermissionType}): systemModulePermissions → {SystemModulePermissions}", module, tabId, permissionType, systemModulePermissions);

                // loop through the system module permissions
                foreach (var systemModulePermission in systemModulePermissions)
                {
                    // create the module permission
                    if (systemModulePermission.PermissionKey == "VIEW" && permissionType == 1 && tabPermission.PermissionKey != "EDIT")
                    {
                        // Only Page Editors get View permissions if "Page Editors Only"
                        continue;
                    }

                    var modulePermission = AddModulePermission(module, systemModulePermission, tabPermission.RoleId, tabPermission.UserId, tabPermission.AllowAccess);
					Logger.LogDebug("InitialModulePermission(module={Module},tabId={TabId},permissionType={PermissionType}): modulePermission → {ModulePermission}", module, tabId, permissionType, modulePermission);

                    // ensure that every EDIT permission which allows access also provides VIEW permission
                    if (modulePermission.PermissionKey == "EDIT" && modulePermission.AllowAccess)
                    {
                        AddModulePermission(module, systemModuleViewPermission, modulePermission.RoleId, modulePermission.UserId, true);
                    }
                }

                // Get the custom Module Permissions,  Assume that roles with Edit Tab Permissions
                // are automatically assigned to the Custom Module Permissions
                if (tabPermission.PermissionKey == "EDIT")
                {
                    var customModulePermissions = this.permissionDefinitionService.GetDefinitionsByModuleDefId(module.ModuleDefID);
					Logger.LogDebug("InitialModulePermission(module={Module},tabId={TabId},permissionType={PermissionType}): customModulePermissions → {CustomModulePermissions}", module, tabId, permissionType, customModulePermissions);

                    // loop through the custom module permissions
                    foreach (var customModulePermission in customModulePermissions)
                    {
                        AddModulePermission(module, customModulePermission, tabPermission.RoleId, tabPermission.UserId, tabPermission.AllowAccess);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting InitialModulePermission(module={Module},tabId={TabId},permissionType={PermissionType})", module, tabId, permissionType);
        }

        /// <inheritdoc />
        public void LocalizeModule(ModuleInfo sourceModule, Locale locale)
        {
            try
            {
                // we could be working from a single culture page that is not in the default language,
                // so we need to test whether the module is going to be localized for the default locale
                var defaultLocale = LocaleController.Instance.GetDefaultLocale(sourceModule.PortalID);
                ModuleInfo defaultModule = locale.Code == defaultLocale.Code ? sourceModule : sourceModule.DefaultLanguageModule;

                if (defaultModule != null)
                {
                    var alreadyLocalized = defaultModule.LocalizedModules.TryGetValue(locale.Code, out var localizedModule)
                                           && localizedModule.ModuleID != defaultModule.ModuleID;
                    var tabModules = this.GetTabModulesByModule(defaultModule.ModuleID);
                    if (tabModules.Count > 1)
                    {
                        // default language version is a reference copy

                        // Localize first tabModule
                        var newModuleId = alreadyLocalized ? localizedModule.ModuleID : this.LocalizeModuleInternal(sourceModule);

                        // Update Reference Copies
                        foreach (ModuleInfo tm in tabModules)
                        {
                            if (tm.IsDefaultLanguage)
                            {
                                ModuleInfo localModule;
                                if (tm.LocalizedModules.TryGetValue(locale.Code, out localModule))
                                {
                                    localModule.ModuleID = newModuleId;
                                    this.UpdateModule(localModule);
                                }
                            }
                        }
                    }
                    else if (!alreadyLocalized)
                    {
                        this.LocalizeModuleInternal(sourceModule);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(CultureInfo.InvariantCulture, "Error localizing module, moduleId: {0}, full exception: {1}", sourceModule.ModuleID, ex);
            }
        }

        /// <summary>
        /// MoveModule moves a Module from one Tab to another including all the
        ///     TabModule settings.
        /// </summary>
        /// <param name="moduleId">The ID of the module to move.</param>
        /// <param name="fromTabId">The ID of the source tab.</param>
        /// <param name="toTabId">The ID of the destination tab.</param>
        /// <param name="toPaneName">The name of the Pane on the destination tab where the module will end up.</param>
        public void MoveModule(int moduleId, int fromTabId, int toTabId, string toPaneName)
        {
			Logger.LogDebug(">>> Entering MoveModule(moduleId={ModuleId},fromTabId={FromTabId},toTabId={ToTabId},toPaneName={ToPaneName})", moduleId, fromTabId, toTabId, toPaneName);
            // Move the module to the Tab
            DataProvider.MoveTabModule(fromTabId, moduleId, toTabId, toPaneName, UserController.Instance.GetCurrentUserInfo().UserID);

            // Update the Tab reference for the module's ContentItems
            var contentController = Util.GetContentController();
			Logger.LogDebug("MoveModule(moduleId={ModuleId},fromTabId={FromTabId},toTabId={ToTabId},toPaneName={ToPaneName}): contentController → {ContentController}", moduleId, fromTabId, toTabId, toPaneName, contentController);
            var contentItems = contentController.GetContentItemsByModuleId(moduleId);
			Logger.LogDebug("MoveModule(moduleId={ModuleId},fromTabId={FromTabId},toTabId={ToTabId},toPaneName={ToPaneName}): contentItems → {ContentItems}", moduleId, fromTabId, toTabId, toPaneName, contentItems);
            if (contentItems != null)
            {
                foreach (var item in contentItems)
                {
                    if (item.TabID != toTabId)
                    {
                        item.TabID = toTabId;
						Logger.LogDebug("MoveModule(moduleId={ModuleId},fromTabId={FromTabId},toTabId={ToTabId},toPaneName={ToPaneName}): item.TabID → {Arg4}", moduleId, fromTabId, toTabId, toPaneName, item.TabID);
                        contentController.UpdateContentItem(item);
                    }
                }
            }

            // Update Module Order for source tab, also updates the tabmodule version guid
            this.UpdateTabModuleOrder(fromTabId);

            // Update Module Order for target tab, also updates the tabmodule version guid
            this.UpdateTabModuleOrder(toTabId);
			Logger.LogDebug("<<< Exiting MoveModule(moduleId={ModuleId},fromTabId={FromTabId},toTabId={ToTabId},toPaneName={ToPaneName})", moduleId, fromTabId, toTabId, toPaneName);
        }

        /// <summary>Restores the module.</summary>
        /// <param name="objModule">The module.</param>
        public void RestoreModule(ModuleInfo objModule)
        {
			Logger.LogDebug(">>> Entering RestoreModule(objModule={ObjModule})", objModule);
            DataProvider.RestoreTabModule(objModule.TabID, objModule.ModuleID);
            var userId = UserController.Instance.GetCurrentUserInfo().UserID;
			Logger.LogDebug("RestoreModule(objModule={ObjModule}): userId → {UserId}", objModule, userId);
            TabChangeTracker.Instance.TrackModuleAddition(objModule, 1, userId);
            this.ClearCache(objModule.TabID);
			Logger.LogDebug("<<< Exiting RestoreModule(objModule={ObjModule})", objModule);
        }

        /// <summary>Update module settings and permissions in database from ModuleInfo.</summary>
        /// <param name="module">ModuleInfo of the module to update.</param>
        public void UpdateModule(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering UpdateModule(module={Module})", module);
            // Update ContentItem If necessary
            if (module.ModuleID != Null.NullInteger)
            {
                if (module.ContentItemId == Null.NullInteger)
                {
                    this.CreateContentItem(module);
                }
                else
                {
                    UpdateContentItem(module);
                }
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("UpdateModule(module={Module}): currentUser → {CurrentUser}", module, currentUser);

            // update module
            DataProvider.UpdateModule(
                module.ModuleID,
                module.ModuleDefID,
                module.ContentItemId,
                module.AllTabs,
                module.StartDate,
                module.EndDate,
                module.InheritViewPermissions,
                module.IsShareable,
                module.IsShareableViewOnly,
                module.IsDeleted,
                currentUser.UserID);

            // Update Tags
            ITermController termController = Util.GetTermController();
			Logger.LogDebug("UpdateModule(module={Module}): termController → {TermController}", module, termController);
            termController.RemoveTermsFromContent(module);
            foreach (Term term in module.Terms)
            {
                termController.AddTermToContent(term, module);
            }

            this.eventLogger.AddLog(module, PortalController.Instance.GetCurrentSettings(), currentUser.UserID, string.Empty, EventLogType.MODULE_UPDATED);

            // save module permissions
            ModulePermissionController.SaveModulePermissions(module);
            this.UpdateModuleSettings(module);
            module.VersionGuid = Guid.NewGuid();
            module.LocalizedVersionGuid = Guid.NewGuid();

            if (!Null.IsNull(module.TabID))
            {
                var hasModuleOrderOrPaneChanged = this.HasModuleOrderOrPaneChanged(module);
				Logger.LogDebug("UpdateModule(module={Module}): hasModuleOrderOrPaneChanged → {HasModuleOrderOrPaneChanged}", module, hasModuleOrderOrPaneChanged);

                // update tabmodule
                DataProvider.UpdateTabModule(
                    module.TabModuleID,
                    module.TabID,
                    module.ModuleID,
                    module.ModuleTitle,
                    module.Header,
                    module.Footer,
                    module.ModuleOrder,
                    module.PaneName,
                    module.CacheTime,
                    module.CacheMethod,
                    module.Alignment,
                    module.Color,
                    module.Border,
                    module.IconFile,
                    (int)module.Visibility,
                    module.ContainerSrc,
                    module.DisplayTitle,
                    module.DisplayPrint,
                    module.DisplaySyndicate,
                    module.VersionGuid,
                    module.DefaultLanguageGuid,
                    module.LocalizedVersionGuid,
                    module.CultureCode,
                    currentUser.UserID);

                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.SingleTabModuleCacheKey, module.TabModuleID));

                this.eventLogger.AddLog(module, PortalController.Instance.GetCurrentSettings(), currentUser.UserID, string.Empty, EventLogType.TABMODULE_UPDATED);

                if (hasModuleOrderOrPaneChanged)
                {
                    // update module order in pane
                    this.UpdateModuleOrder(module.TabID, module.ModuleID, module.ModuleOrder, module.PaneName);
                }

                // set the default module
                if (PortalSettings.Current != null)
                {
                    if (module.IsDefaultModule)
                    {
                        if (module.ModuleID != PortalSettings.Current.DefaultModuleId)
                        {
                            // Update Setting
                            PortalController.UpdatePortalSetting(module.PortalID, "defaultmoduleid", module.ModuleID.ToString(CultureInfo.InvariantCulture));
                        }

                        if (module.TabID != PortalSettings.Current.DefaultTabId)
                        {
                            // Update Setting
                            PortalController.UpdatePortalSetting(module.PortalID, "defaulttabid", module.TabID.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    else
                    {
                        if (module.ModuleID == PortalSettings.Current.DefaultModuleId && module.TabID == PortalSettings.Current.DefaultTabId)
                        {
                            // Clear setting
                            PortalController.DeletePortalSetting(module.PortalID, "defaultmoduleid");
                            PortalController.DeletePortalSetting(module.PortalID, "defaulttabid");
                        }
                    }
                }

                // apply settings to all desktop modules in portal
                if (module.AllModules)
                {
                    foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(module.PortalID))
                    {
                        TabInfo tab = tabPair.Value;
                        foreach (KeyValuePair<int, ModuleInfo> modulePair in this.GetTabModules(tab.TabID))
                        {
                            var targetModule = modulePair.Value;
                            targetModule.VersionGuid = Guid.NewGuid();
                            targetModule.LocalizedVersionGuid = Guid.NewGuid();

                            DataProvider.UpdateTabModule(
                                targetModule.TabModuleID,
                                targetModule.TabID,
                                targetModule.ModuleID,
                                targetModule.ModuleTitle,
                                targetModule.Header,
                                targetModule.Footer,
                                targetModule.ModuleOrder,
                                targetModule.PaneName,
                                targetModule.CacheTime,
                                targetModule.CacheMethod,
                                module.Alignment,
                                module.Color,
                                module.Border,
                                module.IconFile,
                                (int)module.Visibility,
                                module.ContainerSrc,
                                module.DisplayTitle,
                                module.DisplayPrint,
                                module.DisplaySyndicate,
                                targetModule.VersionGuid,
                                targetModule.DefaultLanguageGuid,
                                targetModule.LocalizedVersionGuid,
                                targetModule.CultureCode,
                                currentUser.UserID);

                            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.SingleTabModuleCacheKey, targetModule.TabModuleID));
                            this.ClearCache(targetModule.TabID);
                        }
                    }
                }
            }

            // Clear Cache for all TabModules
            foreach (ModuleInfo tabModule in this.GetTabModulesByModule(module.ModuleID))
            {
                this.ClearCache(tabModule.TabID);
            }

            EventManager.Instance.OnModuleUpdated(new ModuleEventArgs { Module = module });
			Logger.LogDebug("<<< Exiting UpdateModule(module={Module})", module);
        }

        /// <summary>set/change the module position within a pane on a page.</summary>
        /// <param name="tabId">ID of the page.</param>
        /// <param name="moduleId">ID of the module on the page.</param>
        /// <param name="moduleOrder">position within the controls list on page, -1 if to be added at the end.</param>
        /// <param name="paneName">name of the pane, the module is placed in on the page.</param>
        public void UpdateModuleOrder(int tabId, int moduleId, int moduleOrder, string paneName)
        {
			Logger.LogDebug(">>> Entering UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName})", tabId, moduleId, moduleOrder, paneName);
            ModuleInfo moduleInfo = this.GetModule(moduleId, tabId, false);
            if (moduleInfo != null)
			Logger.LogDebug("UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName}): moduleInfo → {ModuleInfo}", tabId, moduleId, moduleOrder, paneName, moduleInfo);
            {
                // adding a module to a new pane - places the module at the bottom of the pane
                if (moduleOrder == -1)
                {
                    IDataReader dr = null;
					Logger.LogDebug("UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName}): dr → {Dr}", tabId, moduleId, moduleOrder, paneName, dr);
                    try
                    {
                        dr = DataProvider.GetTabModuleOrder(tabId, paneName);
                        while (dr.Read())
                        {
                            moduleOrder = Convert.ToInt32(dr["ModuleOrder"], CultureInfo.InvariantCulture);
							Logger.LogDebug("UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName}): moduleOrder → {ModuleOrder1}", tabId, moduleId, moduleOrder, paneName, moduleOrder);
                        }
                    }
                    catch (Exception ex)
                    {
						Logger.LogError("Exception in UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName}): {Arg4}", tabId, moduleId, moduleOrder, paneName, ex.getMessage());
                        Exceptions.LogException(ex);
                    }
                    finally
                    {
                        CBO.CloseDataReader(dr, true);
                    }

                    moduleOrder += 2;
                }

                DataProvider.UpdateModuleOrder(tabId, moduleId, moduleOrder, paneName);
                TabChangeTracker.Instance.TrackModuleModification(this.GetModule(moduleId, tabId, true), Null.NullInteger, UserController.Instance.GetCurrentUserInfo().UserID);

                // clear cache
                this.ClearCache(tabId);
            }
			Logger.LogDebug("<<< Exiting UpdateModuleOrder(tabId={TabId},moduleId={ModuleId},moduleOrder={ModuleOrder},paneName={PaneName})", tabId, moduleId, moduleOrder, paneName);
        }

        /// <summary>Adds or updates a module's setting value.</summary>
        /// <param name="moduleId">ID of the module, the setting belongs to.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>empty SettingValue will remove the setting, if not preserveIfEmpty is true.</remarks>
        public void UpdateModuleSetting(int moduleId, string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering UpdateModuleSetting(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue})", moduleId, settingName, settingValue);
            this.UpdateModuleSettingInternal(moduleId, settingName, settingValue, true);
			Logger.LogDebug("<<< Exiting UpdateModuleSetting(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue})", moduleId, settingName, settingValue);
        }

        /// <summary>set/change all module's positions within a page.</summary>
        /// <param name="tabId">ID of the page.</param>
        public void UpdateTabModuleOrder(int tabId)
        {
			Logger.LogDebug(">>> Entering UpdateTabModuleOrder(tabId={TabId})", tabId);
            IDataReader dr = DataProvider.GetTabPanes(tabId);
            try
			Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): dr → {Dr}", tabId, dr);
            {
                while (dr.Read())
                {
                    int moduleCounter = 0;
                    IDataReader dr2 = DataProvider.GetTabModuleOrder(tabId, Convert.ToString(dr["PaneName"], CultureInfo.InvariantCulture));
					Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): dr2 → {Dr2}", tabId, dr2);
                    try
                    {
                        while (dr2.Read())
                        {
                            moduleCounter += 1;

                            var moduleId = Convert.ToInt32(dr2["ModuleID"], CultureInfo.InvariantCulture);
							Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): moduleId → {ModuleId}", tabId, moduleId);
                            var paneName = Convert.ToString(dr["PaneName"], CultureInfo.InvariantCulture);
							Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): paneName → {PaneName}", tabId, paneName);
                            var isDeleted = Convert.ToBoolean(dr2["IsDeleted"], CultureInfo.InvariantCulture);
							Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): isDeleted → {IsDeleted}", tabId, isDeleted);
                            var existingOrder = Convert.ToInt32(dr2["ModuleOrder"], CultureInfo.InvariantCulture);
							Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): existingOrder → {ExistingOrder}", tabId, existingOrder);
                            var newOrder = (moduleCounter * 2) - 1;
							Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): newOrder → {NewOrder}", tabId, newOrder);

                            if (existingOrder == newOrder)
                            {
                                continue;
                            }

                            DataProvider.UpdateModuleOrder(tabId, moduleId, newOrder, paneName);

                            if (!isDeleted)
                            {
                                var moduleInfo = this.GetModule(moduleId, tabId, true);
								Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): moduleInfo → {ModuleInfo}", tabId, moduleInfo);
                                var userInfo = UserController.Instance.GetCurrentUserInfo();
								Logger.LogDebug("UpdateTabModuleOrder(tabId={TabId}): userInfo → {UserInfo}", tabId, userInfo);
                                TabChangeTracker.Instance.TrackModuleModification(
                                    moduleInfo,
                                    Null.NullInteger,
                                    userInfo.UserID);
                            }
                        }
                    }
                    catch (Exception ex2)
                    {
						Logger.LogError("Exception in UpdateTabModuleOrder(tabId={TabId}): {Arg1}", tabId, ex2.getMessage());
                        Exceptions.LogException(ex2);
                    }
                    finally
                    {
                        CBO.CloseDataReader(dr2, true);
                    }
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in UpdateTabModuleOrder(tabId={TabId}): {Arg1}", tabId, ex.getMessage());
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            // clear module cache
            this.ClearCache(tabId);
			Logger.LogDebug("<<< Exiting UpdateTabModuleOrder(tabId={TabId})", tabId);
        }

        /// <summary>Adds or updates a module's setting value.</summary>
        /// <param name="tabModuleId">ID of the tabmodule, the setting belongs to.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>empty SettingValue will relove the setting.</remarks>
        public void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue)
        {
			Logger.LogDebug(">>> Entering UpdateTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue})", tabModuleId, settingName, settingValue);
            IDataReader dr = DataProvider.GetTabModuleSetting(tabModuleId, settingName);
            try
			Logger.LogDebug("UpdateTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue}): dr → {Dr}", tabModuleId, settingName, settingValue, dr);
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
				Logger.LogDebug("UpdateTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue}): currentUser → {CurrentUser}", tabModuleId, settingName, settingValue, currentUser);
                if (dr.Read())
                {
                    if (dr.GetString(1) != settingValue)
                    {
                        DataProvider.UpdateTabModuleSetting(tabModuleId, settingName, settingValue, currentUser.UserID);
                        this.eventLogger.AddSettingLog(
                            EventLogType.MODULE_SETTING_UPDATED,
                            "TabModuleId",
                            tabModuleId,
                            settingName,
                            settingValue,
                            currentUser.UserID);
                        UpdateTabModuleVersion(tabModuleId);
                    }
                }
                else
                {
                    DataProvider.UpdateTabModuleSetting(tabModuleId, settingName, settingValue, currentUser.UserID);
                    this.eventLogger.AddSettingLog(
                        EventLogType.TABMODULE_SETTING_CREATED,
                        "TabModuleId",
                        tabModuleId,
                        settingName,
                        settingValue,
                        currentUser.UserID);
                    UpdateTabModuleVersion(tabModuleId);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in UpdateTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue}): {Arg3}", tabModuleId, settingName, settingValue, ex.getMessage());
                Exceptions.LogException(ex);
            }
            finally
            {
                // Ensure DataReader is closed
                CBO.CloseDataReader(dr, true);
            }

            ClearTabModuleSettingsCache(tabModuleId, settingName);
			Logger.LogDebug("<<< Exiting UpdateTabModuleSetting(tabModuleId={TabModuleId},settingName={SettingName},settingValue={SettingValue})", tabModuleId, settingName, settingValue);
        }

        /// <summary>Updates the translation status.</summary>
        /// <param name="localizedModule">The localized module.</param>
        /// <param name="isTranslated">if set to <see langword="true"/> will mark the module as translated].</param>
        public void UpdateTranslationStatus(ModuleInfo localizedModule, bool isTranslated)
        {
			Logger.LogDebug(">>> Entering UpdateTranslationStatus(localizedModule={LocalizedModule},isTranslated={IsTranslated})", localizedModule, isTranslated);
            if (isTranslated && (localizedModule.DefaultLanguageModule != null))
            {
                localizedModule.LocalizedVersionGuid = localizedModule.DefaultLanguageModule.LocalizedVersionGuid;
				Logger.LogDebug("UpdateTranslationStatus(localizedModule={LocalizedModule},isTranslated={IsTranslated}): localizedModule.LocalizedVersionGuid → {Arg2}", localizedModule, isTranslated, localizedModule.LocalizedVersionGuid);
            }
            else
            {
                localizedModule.LocalizedVersionGuid = Guid.NewGuid();
            }

            DataProvider.Instance().UpdateTabModuleTranslationStatus(localizedModule.TabModuleID, localizedModule.LocalizedVersionGuid, UserController.Instance.GetCurrentUserInfo().UserID);

            // Clear Tab Caches
            this.ClearCache(localizedModule.TabID);
			Logger.LogDebug("<<< Exiting UpdateTranslationStatus(localizedModule={LocalizedModule},isTranslated={IsTranslated})", localizedModule, isTranslated);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal Hashtable GetModuleSettings(int moduleId, int tabId)
        {
			Logger.LogDebug(">>> Entering GetModuleSettings(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.ModuleSettingsCacheKey, tabId);

			Logger.LogDebug("GetModuleSettings(moduleId={ModuleId},tabId={TabId}): cacheKey → {CacheKey}", moduleId, tabId, cacheKey);
            var moduleSettings = CBO.GetCachedObject<Dictionary<int, Hashtable>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.ModuleCacheTimeOut, DataCache.ModuleCachePriority),
                _ =>
                {
                    var moduleSettingsDic = new Dictionary<int, Hashtable>();
					Logger.LogDebug("GetModuleSettings(moduleId={ModuleId},tabId={TabId}): moduleSettingsDic → {ModuleSettingsDic}", moduleId, tabId, moduleSettingsDic);
                    IDataReader dr = DataProvider.Instance().GetModuleSettingsByTab(tabId);
					Logger.LogDebug("GetModuleSettings(moduleId={ModuleId},tabId={TabId}): dr → {Dr}", moduleId, tabId, dr);
                    while (dr.Read())
                    {
                        int mId = dr.GetInt32(0);
						Logger.LogDebug("GetModuleSettings(moduleId={ModuleId},tabId={TabId}): mId → {MId}", moduleId, tabId, mId);
                        Hashtable settings;
                        if (!moduleSettingsDic.TryGetValue(mId, out settings))
                        {
                            settings = new Hashtable();
                            moduleSettingsDic[mId] = settings;
							Logger.LogDebug("GetModuleSettings(moduleId={ModuleId},tabId={TabId}): settings → {Settings}", moduleId, tabId, settings);
                        }

                        if (!dr.IsDBNull(2))
                        {
                            settings[dr.GetString(1)] = dr.GetString(2);
                        }
                        else
                        {
                            settings[dr.GetString(1)] = string.Empty;
                        }
                    }

                    CBO.CloseDataReader(dr, true);
			Logger.LogDebug("<<< Exiting GetModuleSettings(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
                    return moduleSettingsDic;
                });

            return moduleSettings.TryGetValue(moduleId, out var setting) ? setting : new Hashtable();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal Hashtable GetTabModuleSettings(int tabModuleId, int tabId)
        {
			Logger.LogDebug(">>> Entering GetTabModuleSettings(tabModuleId={TabModuleId},tabId={TabId})", tabModuleId, tabId);
            string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.TabModuleSettingsCacheKey, tabId);

			Logger.LogDebug("GetTabModuleSettings(tabModuleId={TabModuleId},tabId={TabId}): cacheKey → {CacheKey}", tabModuleId, tabId, cacheKey);
            var tabModuleSettings = CBO.GetCachedObject<Dictionary<int, Hashtable>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.TabModuleCacheTimeOut, DataCache.TabModuleCachePriority),
                _ =>
                {
                    var tabModuleSettingsDic = new Dictionary<int, Hashtable>();
					Logger.LogDebug("GetTabModuleSettings(tabModuleId={TabModuleId},tabId={TabId}): tabModuleSettingsDic → {TabModuleSettingsDic}", tabModuleId, tabId, tabModuleSettingsDic);
                    using var dr = DataProvider.Instance().GetTabModuleSettingsByTab(tabId);
					Logger.LogDebug("GetTabModuleSettings(tabModuleId={TabModuleId},tabId={TabId}): dr → {Dr}", tabModuleId, tabId, dr);
                    while (dr.Read())
                    {
                        int tMId = dr.GetInt32(0);
						Logger.LogDebug("GetTabModuleSettings(tabModuleId={TabModuleId},tabId={TabId}): tMId → {TMId}", tabModuleId, tabId, tMId);
                        if (!tabModuleSettingsDic.TryGetValue(tMId, out var settings))
                        {
                            settings = new Hashtable();
                            tabModuleSettingsDic[tMId] = settings;
							Logger.LogDebug("GetTabModuleSettings(tabModuleId={TabModuleId},tabId={TabId}): settings → {Settings}", tabModuleId, tabId, settings);
                        }

                        if (!dr.IsDBNull(2))
                        {
                            settings[dr.GetString(1)] = dr.GetString(2);
                        }
                        else
                        {
                            settings[dr.GetString(1)] = string.Empty;
                        }
                    }

			Logger.LogDebug("<<< Exiting GetTabModuleSettings(tabModuleId={TabModuleId},tabId={TabId})", tabModuleId, tabId);
                    return tabModuleSettingsDic;
                });

            return tabModuleSettings.TryGetValue(tabModuleId, out var setting) ? setting : new Hashtable();
        }

        /// <inheritdoc />
        protected override Func<IModuleController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return Globals.DependencyProvider.GetRequiredService<IModuleController>;
        }

        private static void AddContent(IBusinessControllerProvider businessControllerProvider, XmlNode nodeModule, ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module})", businessControllerProvider, nodeModule, module);
            if (string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) || !module.DesktopModule.IsPortable)
            {
                return;
            }

            try
            {
                var controller = businessControllerProvider.GetInstance<IPortable>(module);
				Logger.LogDebug("AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module}): controller → {Controller}", businessControllerProvider, nodeModule, module, controller);
                if (controller is null)
                {
                    return;
                }

                var content = Convert.ToString(controller.ExportModule(module.ModuleID));
				Logger.LogDebug("AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module}): content → {Content}", businessControllerProvider, nodeModule, module, content);
                if (string.IsNullOrEmpty(content))
                {
                    return;
                }

                content = XmlUtils.RemoveInvalidXmlCharacters(content);

                // add attributes to XML document
                if (nodeModule.OwnerDocument == null)
                {
                    return;
                }

                var existing = nodeModule.OwnerDocument.GetElementById("content");
				Logger.LogDebug("AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module}): existing → {Existing}", businessControllerProvider, nodeModule, module, existing);
                if (existing != null)
                {
                    nodeModule.OwnerDocument.RemoveChild(existing);
                }

                var newNode = nodeModule.OwnerDocument.CreateElement("content");
				Logger.LogDebug("AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module}): newNode → {NewNode}", businessControllerProvider, nodeModule, module, newNode);
                var xmlAttr = nodeModule.OwnerDocument.CreateAttribute("type");
				Logger.LogDebug("AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module}): xmlAttr → {XmlAttr}", businessControllerProvider, nodeModule, module, xmlAttr);
                xmlAttr.Value = Globals.CleanName(module.DesktopModule.ModuleName);
                newNode.Attributes?.Append(xmlAttr);

                xmlAttr = nodeModule.OwnerDocument.CreateAttribute("version");
                xmlAttr.Value = module.DesktopModule.Version;
				Logger.LogDebug("AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module}): xmlAttr.Value → {Arg3}", businessControllerProvider, nodeModule, module, xmlAttr.Value);
                newNode.Attributes?.Append(xmlAttr);

                content = HttpContext.Current.Server.HtmlEncode(content);
                newNode.InnerXml = XmlUtils.XMLEncode(content);
                nodeModule.AppendChild(newNode);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
			Logger.LogDebug("<<< Exiting AddContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},module={Module})", businessControllerProvider, nodeModule, module);
        }

        private static void AddModulePermission(ref ModuleInfo module, int portalId, string roleName, IPermissionDefinitionInfo permission, string permissionKey)
        {
			Logger.LogDebug(">>> Entering AddModulePermission(module={Module},portalId={PortalId},roleName={RoleName},permission={Permission},permissionKey={PermissionKey})", module, portalId, roleName, permission, permissionKey);
            var perm = module.ModulePermissions.SingleOrDefault(tp => tp.RoleName == roleName && tp.PermissionKey == permissionKey);
            if (permission != null && perm == null)
			Logger.LogDebug("AddModulePermission(module={Module},portalId={PortalId},roleName={RoleName},permission={Permission},permissionKey={PermissionKey}): perm → {Perm}", module, portalId, roleName, permission, permissionKey, perm);
            {
                var modulePermission = new ModulePermissionInfo(permission);
				Logger.LogDebug("AddModulePermission(module={Module},portalId={PortalId},roleName={RoleName},permission={Permission},permissionKey={PermissionKey}): modulePermission → {ModulePermission}", module, portalId, roleName, permission, permissionKey, modulePermission);

                // ReSharper disable ImplicitlyCapturedClosure
                var role = RoleController.Instance.GetRole(portalId, r => (r.RoleName == roleName));
				Logger.LogDebug("AddModulePermission(module={Module},portalId={PortalId},roleName={RoleName},permission={Permission},permissionKey={PermissionKey}): role → {Role}", module, portalId, roleName, permission, permissionKey, role);

                // ReSharper restore ImplicitlyCapturedClosure
                if (role != null)
                {
                    ((IPermissionInfo)modulePermission).RoleId = role.RoleID;
                    modulePermission.AllowAccess = true;
					Logger.LogDebug("AddModulePermission(module={Module},portalId={PortalId},roleName={RoleName},permission={Permission},permissionKey={PermissionKey}): modulePermission.AllowAccess → {Arg5}", module, portalId, roleName, permission, permissionKey, modulePermission.AllowAccess);

                    module.ModulePermissions.Add(modulePermission);
                }
            }
			Logger.LogDebug("<<< Exiting AddModulePermission(module={Module},portalId={PortalId},roleName={RoleName},permission={Permission},permissionKey={PermissionKey})", module, portalId, roleName, permission, permissionKey);
        }

        private static bool CheckIsInstance(int templateModuleId, Hashtable hModules)
        {
			Logger.LogDebug(">>> Entering CheckIsInstance(templateModuleId={TemplateModuleId},hModules={HModules})", templateModuleId, hModules);
            // will be instance or module?
            bool isInstance = false;
			Logger.LogDebug("CheckIsInstance(templateModuleId={TemplateModuleId},hModules={HModules}): isInstance → {IsInstance}", templateModuleId, hModules, isInstance);
            if (templateModuleId > 0)
            {
                if (hModules[templateModuleId] != null)
                {
                    // this module has already been processed -> process as instance
                    isInstance = true;
                }
            }

			Logger.LogDebug("<<< Exiting CheckIsInstance(templateModuleId={TemplateModuleId},hModules={HModules})", templateModuleId, hModules);
            return isInstance;
        }

        private static void ClearModuleSettingsCache(int moduleId)
        {
			Logger.LogDebug(">>> Entering ClearModuleSettingsCache(moduleId={ModuleId})", moduleId);
            foreach (var tab in TabController.Instance.GetTabsByModuleID(moduleId).Values)
            {
                string cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.ModuleSettingsCacheKey, tab.TabID);
				Logger.LogDebug("ClearModuleSettingsCache(moduleId={ModuleId}): cacheKey → {CacheKey}", moduleId, cacheKey);
                DataCache.RemoveCache(cacheKey);
            }
			Logger.LogDebug("<<< Exiting ClearModuleSettingsCache(moduleId={ModuleId})", moduleId);
        }

        private static void ClearTabModuleSettingsCache(int tabModuleId, string settingName)
        {
			Logger.LogDebug(">>> Entering ClearTabModuleSettingsCache(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
            var portalId = -1;
            foreach (var tab in TabController.Instance.GetTabsByTabModuleID(tabModuleId).Values)
            {
			Logger.LogDebug("ClearTabModuleSettingsCache(tabModuleId={TabModuleId},settingName={SettingName}): portalId → {PortalId}", tabModuleId, settingName, portalId);
                var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.TabModuleSettingsCacheKey, tab.TabID);
				Logger.LogDebug("ClearTabModuleSettingsCache(tabModuleId={TabModuleId},settingName={SettingName}): cacheKey → {CacheKey}", tabModuleId, settingName, cacheKey);
                DataCache.RemoveCache(cacheKey);

                if (portalId != tab.PortalID)
                {
                    portalId = tab.PortalID;
                    cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.TabModuleSettingsNameCacheKey, portalId, settingName ?? string.Empty);
                    DataCache.RemoveCache(cacheKey);
                }
            }
			Logger.LogDebug("<<< Exiting ClearTabModuleSettingsCache(tabModuleId={TabModuleId},settingName={SettingName})", tabModuleId, settingName);
        }

        private static ModuleInfo DeserializeModule(XmlNode nodeModule, XmlNode nodePane, int portalId, int tabId, int moduleDefId)
        {
			Logger.LogDebug(">>> Entering DeserializeModule(nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},moduleDefId={ModuleDefId})", nodeModule, nodePane, portalId, tabId, moduleDefId);
            // Create New Module
            var module = new ModuleInfo
            {
                PortalID = portalId,
                TabID = tabId,
                ModuleOrder = -1,
                ModuleTitle = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "title"),
                PaneName = XmlUtils.GetNodeValue(nodePane.CreateNavigator(), "name"),
                ModuleDefID = moduleDefId,
                CacheTime = XmlUtils.GetNodeValueInt(nodeModule, "cachetime"),
                CacheMethod = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "cachemethod"),
                Alignment = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "alignment"),
                IconFile = Globals.ImportFile(portalId, XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "iconfile")),
                AllTabs = XmlUtils.GetNodeValueBoolean(nodeModule, "alltabs"),
                CultureCode = XmlUtils.GetNodeValue(nodeModule, "cultureCode"),
            };

            // Localization
            var oldGuid = XmlUtils.GetNodeValue(nodeModule, "defaultLanguageGuid");
			Logger.LogDebug("DeserializeModule(nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},moduleDefId={ModuleDefId}): oldGuid → {OldGuid}", nodeModule, nodePane, portalId, tabId, moduleDefId, oldGuid);
            if (!string.IsNullOrEmpty(oldGuid))
            {
                // get new default module language guid
                if (ParsedLocalizedModuleGuid.ContainsKey(oldGuid))
                {
                    module.DefaultLanguageGuid = new Guid(ParsedLocalizedModuleGuid[oldGuid].ToString());
                }
            }

            switch (XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "visibility"))
            {
                case "Maximized":
                    module.Visibility = VisibilityState.Maximized;
					Logger.LogDebug("DeserializeModule(nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},moduleDefId={ModuleDefId}): module.Visibility → {Arg5}", nodeModule, nodePane, portalId, tabId, moduleDefId, module.Visibility);
                    break;
                case "Minimized":
                    module.Visibility = VisibilityState.Minimized;
                    break;
                case "None":
                    module.Visibility = VisibilityState.None;
                    break;
            }

            module.Color = XmlUtils.GetNodeValue(nodeModule, "color", string.Empty);
            module.Border = XmlUtils.GetNodeValue(nodeModule, "border", string.Empty);
            module.Header = XmlUtils.GetNodeValue(nodeModule, "header", string.Empty);
            module.Footer = XmlUtils.GetNodeValue(nodeModule, "footer", string.Empty);
            module.InheritViewPermissions = XmlUtils.GetNodeValueBoolean(nodeModule, "inheritviewpermissions", false);
            module.IsShareable = XmlUtils.GetNodeValueBoolean(nodeModule, "isshareable", true);
            module.IsShareableViewOnly = XmlUtils.GetNodeValueBoolean(nodeModule, "isshareableviewonly", true);
            module.StartDate = XmlUtils.GetNodeValueDate(nodeModule, "startdate", Null.NullDate);
            module.EndDate = XmlUtils.GetNodeValueDate(nodeModule, "enddate", Null.NullDate);
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeModule, "containersrc", string.Empty)))
            {
                module.ContainerSrc = XmlUtils.GetNodeValue(nodeModule, "containersrc", string.Empty);
            }

            module.DisplayTitle = XmlUtils.GetNodeValueBoolean(nodeModule, "displaytitle", true);
            module.DisplayPrint = XmlUtils.GetNodeValueBoolean(nodeModule, "displayprint", true);
            module.DisplaySyndicate = XmlUtils.GetNodeValueBoolean(nodeModule, "displaysyndicate", false);

			Logger.LogDebug("<<< Exiting DeserializeModule(nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},moduleDefId={ModuleDefId})", nodeModule, nodePane, portalId, tabId, moduleDefId);
            return module;
        }

        private static void DeserializeModulePermissions(IPermissionDefinitionService permissionDefinitionService, XmlNodeList nodeModulePermissions, int portalId, ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering DeserializeModulePermissions(permissionDefinitionService={PermissionDefinitionService},nodeModulePermissions={NodeModulePermissions},portalId={PortalId},module={Module})", permissionDefinitionService, nodeModulePermissions, portalId, module);
            foreach (XmlNode node in nodeModulePermissions)
            {
                string permissionKey = XmlUtils.GetNodeValue(node.CreateNavigator(), "permissionkey");
                string permissionCode = XmlUtils.GetNodeValue(node.CreateNavigator(), "permissioncode");
                string roleName = XmlUtils.GetNodeValue(node.CreateNavigator(), "rolename");
                int roleID = int.MinValue;
				Logger.LogDebug("DeserializeModulePermissions(permissionDefinitionService={PermissionDefinitionService},nodeModulePermissions={NodeModulePermissions},portalId={PortalId},module={Module}): roleID → {RoleID}", permissionDefinitionService, nodeModulePermissions, portalId, module, roleID);
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleID = Convert.ToInt32(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleID = Convert.ToInt32(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);
                        break;
                    default:
                        var role = RoleController.Instance.GetRole(portalId, r => r.RoleName == roleName);
						Logger.LogDebug("DeserializeModulePermissions(permissionDefinitionService={PermissionDefinitionService},nodeModulePermissions={NodeModulePermissions},portalId={PortalId},module={Module}): role → {Role}", permissionDefinitionService, nodeModulePermissions, portalId, module, role);
                        if (role != null)
                        {
                            roleID = role.RoleID;
                        }

                        break;
                }

                if (roleID != int.MinValue)
                {
                    int permissionId = -1;
					Logger.LogDebug("DeserializeModulePermissions(permissionDefinitionService={PermissionDefinitionService},nodeModulePermissions={NodeModulePermissions},portalId={PortalId},module={Module}): permissionId → {PermissionId}", permissionDefinitionService, nodeModulePermissions, portalId, module, permissionId);
                    var permissions = permissionDefinitionService.GetDefinitionsByCodeAndKey(permissionCode, permissionKey);
					Logger.LogDebug("DeserializeModulePermissions(permissionDefinitionService={PermissionDefinitionService},nodeModulePermissions={NodeModulePermissions},portalId={PortalId},module={Module}): permissions → {Permissions}", permissionDefinitionService, nodeModulePermissions, portalId, module, permissions);
                    foreach (var permission in permissions)
                    {
                        permissionId = permission.PermissionId;
                    }

                    // if role was found add, otherwise ignore
                    if (permissionId != -1)
                    {
                        var modulePermission = new ModulePermissionInfo
                        {
                            ModuleID = module.ModuleID,
                            AllowAccess = Convert.ToBoolean(XmlUtils.GetNodeValue(node.CreateNavigator(), "allowaccess")),
                        };
                        ((IPermissionInfo)modulePermission).PermissionId = permissionId;
                        ((IPermissionInfo)modulePermission).RoleId = roleID;

                        // do not add duplicate ModulePermissions
                        bool canAdd = !module.ModulePermissions
                            .Any(mp => mp.ModuleID == modulePermission.ModuleID &&
                                       ((IPermissionInfo)mp).PermissionId == ((IPermissionInfo)modulePermission).PermissionId &&
                                       ((IPermissionInfo)mp).RoleId == ((IPermissionInfo)modulePermission).RoleId &&
                                       ((IPermissionInfo)mp).UserId == ((IPermissionInfo)modulePermission).UserId);
                        if (canAdd)
                        {
                            module.ModulePermissions.Add(modulePermission);
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting DeserializeModulePermissions(permissionDefinitionService={PermissionDefinitionService},nodeModulePermissions={NodeModulePermissions},portalId={PortalId},module={Module})", permissionDefinitionService, nodeModulePermissions, portalId, module);
        }

        private static void DeserializeModuleSettings(XmlNodeList nodeModuleSettings, ModuleInfo objModule)
        {
			Logger.LogDebug(">>> Entering DeserializeModuleSettings(nodeModuleSettings={NodeModuleSettings},objModule={ObjModule})", nodeModuleSettings, objModule);
            foreach (XmlNode moduleSettingNode in nodeModuleSettings)
            {
                string key = XmlUtils.GetNodeValue(moduleSettingNode.CreateNavigator(), "settingname");
                string value = XmlUtils.GetNodeValue(moduleSettingNode.CreateNavigator(), "settingvalue");
                objModule.ModuleSettings[key] = value;
            }
			Logger.LogDebug("<<< Exiting DeserializeModuleSettings(nodeModuleSettings={NodeModuleSettings},objModule={ObjModule})", nodeModuleSettings, objModule);
        }

        private static void DeserializeTabModuleSettings(XmlNodeList nodeTabModuleSettings, ModuleInfo objModule)
        {
			Logger.LogDebug(">>> Entering DeserializeTabModuleSettings(nodeTabModuleSettings={NodeTabModuleSettings},objModule={ObjModule})", nodeTabModuleSettings, objModule);
            foreach (XmlNode tabModuleSettingNode in nodeTabModuleSettings)
            {
                string key = XmlUtils.GetNodeValue(tabModuleSettingNode.CreateNavigator(), "settingname");
                string value = XmlUtils.GetNodeValue(tabModuleSettingNode.CreateNavigator(), "settingvalue");
                objModule.TabModuleSettings[key] = value;
            }
			Logger.LogDebug("<<< Exiting DeserializeTabModuleSettings(nodeTabModuleSettings={NodeTabModuleSettings},objModule={ObjModule})", nodeTabModuleSettings, objModule);
        }

        private static bool FindModule(XmlNode nodeModule, int tabId, PortalTemplateModuleAction mergeTabs)
        {
			Logger.LogDebug(">>> Entering FindModule(nodeModule={NodeModule},tabId={TabId},mergeTabs={MergeTabs})", nodeModule, tabId, mergeTabs);
            var modules = Instance.GetTabModules(tabId);

			Logger.LogDebug("FindModule(nodeModule={NodeModule},tabId={TabId},mergeTabs={MergeTabs}): modules → {Modules}", nodeModule, tabId, mergeTabs, modules);
            bool moduleFound = false;
			Logger.LogDebug("FindModule(nodeModule={NodeModule},tabId={TabId},mergeTabs={MergeTabs}): moduleFound → {ModuleFound}", nodeModule, tabId, mergeTabs, moduleFound);
            string modTitle = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "title");
            if (mergeTabs == PortalTemplateModuleAction.Merge)
            {
                if (modules.Select(kvp => kvp.Value).Any(module => modTitle == module.ModuleTitle))
                {
                    moduleFound = true;
                }
            }

			Logger.LogDebug("<<< Exiting FindModule(nodeModule={NodeModule},tabId={TabId},mergeTabs={MergeTabs})", nodeModule, tabId, mergeTabs);
            return moduleFound;
        }

        private static void GetModuleContent(IBusinessControllerProvider businessControllerProvider, XmlNode nodeModule, int moduleId, int tabId, int portalId)
        {
			Logger.LogDebug(">>> Entering GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId})", businessControllerProvider, nodeModule, moduleId, tabId, portalId);
            ModuleInfo module = Instance.GetModule(moduleId, tabId, true);
            if (nodeModule == null)
			Logger.LogDebug("GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId}): module → {Module}", businessControllerProvider, nodeModule, moduleId, tabId, portalId, module);
            {
                return;
            }

            string version = nodeModule.SelectSingleNode("content").Attributes["version"].Value;
            string content = nodeModule.SelectSingleNode("content").InnerXml;
			Logger.LogDebug("GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId}): content → {Content}", businessControllerProvider, nodeModule, moduleId, tabId, portalId, content);
            content = content.Substring(9, content.Length - 12);
            if (string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) || string.IsNullOrEmpty(content))
            {
                return;
            }

            var portal = PortalController.Instance.GetPortal(portalId);
			Logger.LogDebug("GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId}): portal → {Portal}", businessControllerProvider, nodeModule, moduleId, tabId, portalId, portal);

            // Determine if the Module is completely installed
            // (ie are we running in the same request that installed the module).
            if (module.DesktopModule.SupportedFeatures == Null.NullInteger)
            {
                // save content in eventqueue for processing after an app restart,
                // as modules Supported Features are not updated yet so we
                // cannot determine if the module supports IsPortable
                EventMessageProcessor.CreateImportModuleMessage(module, content, version, portal.AdministratorId);
            }
            else
            {
                if (!module.DesktopModule.IsPortable)
                {
                    return;
                }

                try
                {
                    var controller = businessControllerProvider.GetInstance<IPortable>(module);
					Logger.LogDebug("GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId}): controller → {Controller}", businessControllerProvider, nodeModule, moduleId, tabId, portalId, controller);
                    if (controller is not null)
                    {
                        var decodedContent = WebUtility.HtmlDecode(content);
						Logger.LogDebug("GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId}): decodedContent → {DecodedContent}", businessControllerProvider, nodeModule, moduleId, tabId, portalId, decodedContent);
                        controller.ImportModule(module.ModuleID, decodedContent, version, portal.AdministratorId);
                    }
                }
                catch
                {
					Logger.LogError("Exception in GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId}): {Arg5}", businessControllerProvider, nodeModule, moduleId, tabId, portalId, exc.Message);
                    // if there is an error then the type cannot be loaded at this time, so add to EventQueue
                    EventMessageProcessor.CreateImportModuleMessage(module, content, version, portal.AdministratorId);
                }
            }
			Logger.LogDebug("<<< Exiting GetModuleContent(businessControllerProvider={BusinessControllerProvider},nodeModule={NodeModule},moduleId={ModuleId},tabId={TabId},portalId={PortalId})", businessControllerProvider, nodeModule, moduleId, tabId, portalId);
        }

        private static ModuleDefinitionInfo GetModuleDefinition(XmlNode nodeModule)
        {
			Logger.LogDebug(">>> Entering GetModuleDefinition(nodeModule={NodeModule})", nodeModule);
            ModuleDefinitionInfo moduleDefinition = null;

			Logger.LogDebug("GetModuleDefinition(nodeModule={NodeModule}): moduleDefinition → {ModuleDefinition}", nodeModule, moduleDefinition);
            // Templates prior to v4.3.5 only have the <definition> node to define the Module Type
            // This <definition> node was populated with the DesktopModuleInfo.ModuleName property
            // Thus there is no mechanism to determine to which module definition the module belongs.
            //
            // Template from v4.3.5 on also have the <moduledefinition> element that is populated
            // with the ModuleDefinitionInfo.FriendlyName.  Therefore the module Instance identifies
            // which Module Definition it belongs to.

            // Get the DesktopModule defined by the <definition> element
            var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "definition"), Null.NullInteger);
			Logger.LogDebug("GetModuleDefinition(nodeModule={NodeModule}): desktopModule → {DesktopModule}", nodeModule, desktopModule);
            if (desktopModule != null)
            {
                // Get the moduleDefinition from the <moduledefinition> element
                string friendlyName = XmlUtils.GetNodeValue(nodeModule.CreateNavigator(), "moduledefinition");
                if (string.IsNullOrEmpty(friendlyName))
                {
                    // Module is pre 4.3.5 so get the first Module Definition (at least it won't throw an error then)
                    foreach (ModuleDefinitionInfo md in ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(desktopModule.DesktopModuleID).Values)
                    {
                        moduleDefinition = md;
                        break;
                    }
                }
                else
                {
                    // Module is 4.3.5 or later so get the Module Defeinition by its friendly name
                    moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(friendlyName, desktopModule.DesktopModuleID);
                }
            }

			Logger.LogDebug("<<< Exiting GetModuleDefinition(nodeModule={NodeModule})", nodeModule);
            return moduleDefinition;
        }

        private static void SetCloneModuleContext(bool cloneModuleContext)
        {
			Logger.LogDebug(">>> Entering SetCloneModuleContext(cloneModuleContext={CloneModuleContext})", cloneModuleContext);
            Thread.SetData(
                Thread.GetNamedDataSlot("CloneModuleContext"),
                cloneModuleContext ? bool.TrueString : bool.FalseString);
				Logger.LogDebug("<<< Exiting SetCloneModuleContext(cloneModuleContext={CloneModuleContext})", cloneModuleContext);
        }

        private static void UpdateTabModuleVersion(int tabModuleId)
        {
			Logger.LogDebug(">>> Entering UpdateTabModuleVersion(tabModuleId={TabModuleId})", tabModuleId);
            DataProvider.UpdateTabModuleVersion(tabModuleId, Guid.NewGuid());
			Logger.LogDebug("<<< Exiting UpdateTabModuleVersion(tabModuleId={TabModuleId})", tabModuleId);
        }

        private static IPermissionInfo AddModulePermission(ModuleInfo module, IPermissionDefinitionInfo permission, int roleId, int userId, bool allowAccess)
        {
			Logger.LogDebug(">>> Entering AddModulePermission(module={Module},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess})", module, permission, roleId, userId, allowAccess);
            IPermissionInfo modulePermission = new ModulePermissionInfo
            {
                ModuleID = module.ModuleID,
                PermissionKey = permission.PermissionKey,
                AllowAccess = allowAccess,
            };
            modulePermission.PermissionId = permission.PermissionId;
			Logger.LogDebug("AddModulePermission(module={Module},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess}): modulePermission.PermissionId → {Arg5}", module, permission, roleId, userId, allowAccess, modulePermission.PermissionId);
			Logger.LogDebug("AddModulePermission(module={Module},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess}): modulePermission.RoleId → {Arg5}", module, permission, roleId, userId, allowAccess, modulePermission.RoleId);
			Logger.LogDebug("AddModulePermission(module={Module},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess}): modulePermission.UserId → {Arg5}", module, permission, roleId, userId, allowAccess, modulePermission.UserId);
            modulePermission.RoleId = roleId;
            modulePermission.UserId = userId;

            // add the permission to the collection
            if (!module.ModulePermissions.Contains(modulePermission))
            {
                module.ModulePermissions.Add((ModulePermissionInfo)modulePermission);
            }

			Logger.LogDebug("<<< Exiting AddModulePermission(module={Module},permission={Permission},roleId={RoleId},userId={UserId},allowAccess={AllowAccess})", module, permission, roleId, userId, allowAccess);
            return modulePermission;
        }

        /// <summary>Checks whether module VIEW permission is inherited from its tab.</summary>
        /// <param name="module">The module.</param>
        /// <param name="permission">The module permission.</param>
        private static bool IsModuleViewPermissionInherited(ModuleInfo module, ModulePermissionInfo permission)
        {
			Logger.LogDebug(">>> Entering IsModuleViewPermissionInherited(module={Module},permission={Permission})", module, permission);
            Requires.NotNull(module);

            Requires.NotNull(permission);

            var permissionViewKey = "VIEW";
			Logger.LogDebug("IsModuleViewPermissionInherited(module={Module},permission={Permission}): permissionViewKey → {PermissionViewKey}", module, permission, permissionViewKey);

            if (!module.InheritViewPermissions || permission.PermissionKey != permissionViewKey)
            {
                return false;
            }

            var tabPermissions = TabPermissionController.GetTabPermissions(module.TabID, module.PortalID);
			Logger.LogDebug("IsModuleViewPermissionInherited(module={Module},permission={Permission}): tabPermissions → {TabPermissions}", module, permission, tabPermissions);

			Logger.LogDebug("<<< Exiting IsModuleViewPermissionInherited(module={Module},permission={Permission})", module, permission);
            return tabPermissions?.Where(x => x.RoleID == permission.RoleID && x.PermissionKey == permissionViewKey).Any() == true;
        }

        /// <summary>Checks whether given permission is granted for translator role.</summary>
        /// <param name="permission">The module permission.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="culture">The culture code.</param>
        private static bool IsTranslatorRolePermission(ModulePermissionInfo permission, int portalId, string culture)
        {
			Logger.LogDebug(">>> Entering IsTranslatorRolePermission(permission={Permission},portalId={PortalId},culture={Culture})", permission, portalId, culture);
            Requires.NotNull(permission);

            if (string.IsNullOrWhiteSpace(culture) || portalId == Null.NullInteger)
            {
                return false;
            }

            var translatorSettingKey = $"DefaultTranslatorRoles-{culture}";
			Logger.LogDebug("IsTranslatorRolePermission(permission={Permission},portalId={PortalId},culture={Culture}): translatorSettingKey → {TranslatorSettingKey}", permission, portalId, culture, translatorSettingKey);

            var translatorSettingValue =
                PortalController.GetPortalSetting(translatorSettingKey, portalId, null) ??
                HostController.Instance.GetString(translatorSettingKey, null);

            var translatorRoles =
                translatorSettingValue?.Split([';',], StringSplitOptions.RemoveEmptyEntries);

			Logger.LogDebug("<<< Exiting IsTranslatorRolePermission(permission={Permission},portalId={PortalId},culture={Culture})", permission, portalId, culture);
            return translatorRoles?.Any(r => r.Equals(permission.RoleName, StringComparison.OrdinalIgnoreCase)) == true;
        }

        /// <summary>Copies permissions from source to new tab.</summary>
        /// <param name="sourceModule">Source module.</param>
        /// <param name="newModule">New module.</param>
        private static void CopyModulePermissions(ModuleInfo sourceModule, ModuleInfo newModule)
        {
			Logger.LogDebug(">>> Entering CopyModulePermissions(sourceModule={SourceModule},newModule={NewModule})", sourceModule, newModule);
            Requires.NotNull(sourceModule);

            Requires.NotNull(newModule);

            foreach (ModulePermissionInfo permission in sourceModule.ModulePermissions)
            {
                // skip inherited view and translator permissions
                if (IsModuleViewPermissionInherited(newModule, permission) ||
                    IsTranslatorRolePermission(permission, sourceModule.PortalID, sourceModule.CultureCode))
                {
                    continue;
                }

                // need to force vew permission to be copied
                permission.PermissionKey = newModule.InheritViewPermissions && permission.PermissionKey == "VIEW" ?
                    null :
                    permission.PermissionKey;

                AddModulePermission(
                    newModule,
                    permission,
                    permission.RoleID,
                    permission.UserID,
                    permission.AllowAccess);
            }
			Logger.LogDebug("<<< Exiting CopyModulePermissions(sourceModule={SourceModule},newModule={NewModule})", sourceModule, newModule);
        }

        /// <summary>Update content item when the module title changed.</summary>
        /// <param name="module">The module info.</param>
        private static void UpdateContentItem(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering UpdateContentItem(module={Module})", module);
            IContentController contentController = Util.GetContentController();
            if (module.Content != module.ModuleTitle)
			Logger.LogDebug("UpdateContentItem(module={Module}): contentController → {ContentController}", module, contentController);
            {
                module.Content = module.ModuleTitle;
				Logger.LogDebug("UpdateContentItem(module={Module}): module.Content → {Arg1}", module, module.Content);
                contentController.UpdateContentItem(module);
            }
			Logger.LogDebug("<<< Exiting UpdateContentItem(module={Module})", module);
        }

        private static Dictionary<int, ModuleInfo> GetModulesCurrentPage(int tabId)
        {
            var modules = CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));

            var dictionary = new Dictionary<int, ModuleInfo>();
            foreach (var module in modules)
            {
                dictionary[module.ModuleID] = module;
            }

            return dictionary;
        }

        private void AddModuleInternal(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering AddModuleInternal(module={Module})", module);
            // add module
            if (Null.IsNull(module.ModuleID))
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
				Logger.LogDebug("AddModuleInternal(module={Module}): currentUser → {CurrentUser}", module, currentUser);
                this.CreateContentItem(module);

                // Add Module
                module.ModuleID = DataProvider.AddModule(
                    module.ContentItemId,
                    module.PortalID,
                    module.ModuleDefID,
                    module.AllTabs,
                    module.StartDate,
                    module.EndDate,
                    module.InheritViewPermissions,
                    module.IsShareable,
                    module.IsShareableViewOnly,
                    module.IsDeleted,
                    currentUser.UserID);

                // Now we have the ModuleID - update the contentItem
                var contentController = Util.GetContentController();
				Logger.LogDebug("AddModuleInternal(module={Module}): contentController → {ContentController}", module, contentController);
                contentController.UpdateContentItem(module);

                this.eventLogger.AddLog(module, PortalController.Instance.GetCurrentSettings(), currentUser.UserID, string.Empty, EventLogType.MODULE_CREATED);

                // set module permissions
                ModulePermissionController.SaveModulePermissions(module);
            }

            // Save ModuleSettings
            this.UpdateModuleSettings(module);

            EventManager.Instance.OnModuleCreated(new ModuleEventArgs { Module = module });
			Logger.LogDebug("<<< Exiting AddModuleInternal(module={Module})", module);
        }

        private void CopyTabModuleSettingsInternal(ModuleInfo fromModule, ModuleInfo toModule)
        {
			Logger.LogDebug(">>> Entering CopyTabModuleSettingsInternal(fromModule={FromModule},toModule={ToModule})", fromModule, toModule);
            // Copy each setting to the new TabModule instance
            foreach (DictionaryEntry setting in fromModule.TabModuleSettings)
            {
                this.UpdateTabModuleSetting(toModule.TabModuleID, Convert.ToString(setting.Key, CultureInfo.InvariantCulture), Convert.ToString(setting.Value, CultureInfo.InvariantCulture));
            }
			Logger.LogDebug("<<< Exiting CopyTabModuleSettingsInternal(fromModule={FromModule},toModule={ToModule})", fromModule, toModule);
        }

        private int LocalizeModuleInternal(ModuleInfo sourceModule)
        {
			Logger.LogDebug(">>> Entering LocalizeModuleInternal(sourceModule={SourceModule})", sourceModule);
            int moduleId = Null.NullInteger;

			Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): moduleId → {ModuleId}", sourceModule, moduleId);
            if (sourceModule != null)
            {
                // clone the module object ( to avoid creating an object reference to the data cache )
                var newModule = sourceModule.Clone();
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): newModule → {NewModule}", sourceModule, newModule);
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): newModule.ModuleID → {Arg1}", sourceModule, newModule.ModuleID);
                newModule.ModuleID = Null.NullInteger;

                string translatorRoles = PortalController.GetPortalSetting($"DefaultTranslatorRoles-{sourceModule.CultureCode}", sourceModule.PortalID, string.Empty).TrimEnd(';');
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): translatorRoles → {TranslatorRoles}", sourceModule, translatorRoles);

                // Add the default translators for this language, view and edit permissions
                var viewPermissionsList = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", "VIEW");
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): viewPermissionsList → {ViewPermissionsList}", sourceModule, viewPermissionsList);
                var editPermissionsList = this.permissionDefinitionService.GetDefinitionsByCodeAndKey("SYSTEM_MODULE_DEFINITION", "EDIT");
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): editPermissionsList → {EditPermissionsList}", sourceModule, editPermissionsList);
                var viewPermission = viewPermissionsList.FirstOrDefault();
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): viewPermission → {ViewPermission}", sourceModule, viewPermission);
                var editPermission = editPermissionsList.FirstOrDefault();
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): editPermission → {EditPermission}", sourceModule, editPermission);
                if (viewPermission != null || editPermission != null)
                {
                    foreach (string translatorRole in translatorRoles.Split(';'))
                    {
                        AddModulePermission(ref newModule, sourceModule.PortalID, translatorRole, viewPermission, "VIEW");
                        AddModulePermission(ref newModule, sourceModule.PortalID, translatorRole, editPermission, "EDIT");
                    }
                }

                // copy permisions from source to new module
                CopyModulePermissions(sourceModule, newModule);

                // Add Module
                this.AddModuleInternal(newModule);

                // copy module settings
                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.ModuleSettingsCacheKey, sourceModule.TabID));
                var settings = this.GetModuleSettings(sourceModule.ModuleID, sourceModule.TabID);
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): settings → {Settings}", sourceModule, settings);

                // update tabmodule
                var currentUser = UserController.Instance.GetCurrentUserInfo();
				Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): currentUser → {CurrentUser}", sourceModule, currentUser);
                DataProvider.UpdateTabModule(
                    newModule.TabModuleID,
                    newModule.TabID,
                    newModule.ModuleID,
                    newModule.ModuleTitle,
                    newModule.Header,
                    newModule.Footer,
                    newModule.ModuleOrder,
                    newModule.PaneName,
                    newModule.CacheTime,
                    newModule.CacheMethod,
                    newModule.Alignment,
                    newModule.Color,
                    newModule.Border,
                    newModule.IconFile,
                    (int)newModule.Visibility,
                    newModule.ContainerSrc,
                    newModule.DisplayTitle,
                    newModule.DisplayPrint,
                    newModule.DisplaySyndicate,
                    newModule.VersionGuid,
                    newModule.DefaultLanguageGuid,
                    newModule.LocalizedVersionGuid,
                    newModule.CultureCode,
                    currentUser.UserID);

                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.SingleTabModuleCacheKey, newModule.TabModuleID));

                // Copy each setting to the new TabModule instance
                foreach (DictionaryEntry setting in settings)
                {
                    this.UpdateModuleSetting(newModule.ModuleID, Convert.ToString(setting.Key, CultureInfo.InvariantCulture), Convert.ToString(setting.Value, CultureInfo.InvariantCulture));
                }

                try
                {
                    using var serviceScope = Globals.GetOrCreateServiceScope();
					Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): serviceScope → {ServiceScope}", sourceModule, serviceScope);
                    var businessControllerProvider = serviceScope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>();
					Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): businessControllerProvider → {BusinessControllerProvider}", sourceModule, businessControllerProvider);
                    var portableModule = businessControllerProvider.GetInstance<IPortable>(newModule);
					Logger.LogDebug("LocalizeModuleInternal(sourceModule={SourceModule}): portableModule → {PortableModule}", sourceModule, portableModule);
                    if (portableModule is not null)
                    {
                        try
                        {
                            SetCloneModuleContext(true);
                            string moduleContent = portableModule.ExportModule(sourceModule.ModuleID);
                            if (!string.IsNullOrEmpty(moduleContent))
                            {
                                portableModule.ImportModule(
                                    newModule.ModuleID,
                                    moduleContent,
                                    newModule.DesktopModule.Version,
                                    currentUser.UserID);
                            }
                        }
                        finally
                        {
                            SetCloneModuleContext(false);
                        }
                    }
                }
                catch (Exception ex)
                {
					Logger.LogError("Exception in LocalizeModuleInternal(sourceModule={SourceModule}): {Arg1}", sourceModule, ex.getMessage());
                    Exceptions.LogException(ex);
                }

                moduleId = newModule.ModuleID;

                // Clear Caches
                this.ClearCache(newModule.TabID);
                this.ClearCache(sourceModule.TabID);
            }

			Logger.LogDebug("<<< Exiting LocalizeModuleInternal(sourceModule={SourceModule})", sourceModule);
            return moduleId;
        }

        private void UpdateModuleSettingInternal(int moduleId, string settingName, string settingValue, bool updateVersion)
        {
			Logger.LogDebug(">>> Entering UpdateModuleSettingInternal(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},updateVersion={UpdateVersion})", moduleId, settingName, settingValue, updateVersion);
            IDataReader dr = null;
            try
			Logger.LogDebug("UpdateModuleSettingInternal(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},updateVersion={UpdateVersion}): dr → {Dr}", moduleId, settingName, settingValue, updateVersion, dr);
            {
                var currentUser = UserController.Instance.GetCurrentUserInfo();
				Logger.LogDebug("UpdateModuleSettingInternal(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},updateVersion={UpdateVersion}): currentUser → {CurrentUser}", moduleId, settingName, settingValue, updateVersion, currentUser);
                dr = DataProvider.GetModuleSetting(moduleId, settingName);

                string existValue = null;
				Logger.LogDebug("UpdateModuleSettingInternal(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},updateVersion={UpdateVersion}): existValue → {ExistValue}", moduleId, settingName, settingValue, updateVersion, existValue);
                const int SettingValueColumnIndex = 1;
                if (dr.Read() && !dr.IsDBNull(SettingValueColumnIndex))
                {
                    existValue = dr.GetString(SettingValueColumnIndex);
                }

                dr.Close();

                if (existValue == null)
                {
                    DataProvider.UpdateModuleSetting(moduleId, settingName, settingValue, currentUser.UserID);
                    this.eventLogger.AddSettingLog(
                        EventLogType.MODULE_SETTING_CREATED,
                        "ModuleId",
                        moduleId,
                        settingName,
                        settingValue,
                        currentUser.UserID);
                }
                else if (existValue != settingValue)
                {
                    DataProvider.UpdateModuleSetting(moduleId, settingName, settingValue, currentUser.UserID);
                    this.eventLogger.AddSettingLog(
                        EventLogType.MODULE_SETTING_UPDATED,
                        "ModuleId",
                        moduleId,
                        settingName,
                        settingValue,
                        currentUser.UserID);
                }

                if (updateVersion)
                {
                    this.UpdateTabModuleVersionsByModuleID(moduleId);
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in UpdateModuleSettingInternal(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},updateVersion={UpdateVersion}): {Arg4}", moduleId, settingName, settingValue, updateVersion, ex.getMessage());
                Exceptions.LogException(ex);
            }
            finally
            {
                // Ensure DataReader is closed
                if (dr != null && !dr.IsClosed)
                {
                    CBO.CloseDataReader(dr, true);
                }
            }

            ClearModuleSettingsCache(moduleId);
			Logger.LogDebug("<<< Exiting UpdateModuleSettingInternal(moduleId={ModuleId},settingName={SettingName},settingValue={SettingValue},updateVersion={UpdateVersion})", moduleId, settingName, settingValue, updateVersion);
        }

        private void UpdateModuleSettings(ModuleInfo updatedModule)
        {
			Logger.LogDebug(">>> Entering UpdateModuleSettings(updatedModule={UpdatedModule})", updatedModule);
            foreach (string key in updatedModule.ModuleSettings.Keys)
            {
                this.UpdateModuleSettingInternal(updatedModule.ModuleID, key, Convert.ToString(updatedModule.ModuleSettings[key], CultureInfo.InvariantCulture), false);
            }

            this.UpdateTabModuleVersionsByModuleID(updatedModule.ModuleID);
			Logger.LogDebug("<<< Exiting UpdateModuleSettings(updatedModule={UpdatedModule})", updatedModule);
        }

        private void UpdateTabModuleSettings(ModuleInfo updatedTabModule)
        {
			Logger.LogDebug(">>> Entering UpdateTabModuleSettings(updatedTabModule={UpdatedTabModule})", updatedTabModule);
            foreach (string sKey in updatedTabModule.TabModuleSettings.Keys)
            {
                this.UpdateTabModuleSetting(updatedTabModule.TabModuleID, sKey, Convert.ToString(updatedTabModule.TabModuleSettings[sKey], CultureInfo.InvariantCulture));
            }
			Logger.LogDebug("<<< Exiting UpdateTabModuleSettings(updatedTabModule={UpdatedTabModule})", updatedTabModule);
        }

        private void UpdateTabModuleVersionsByModuleID(int moduleID)
        {
			Logger.LogDebug(">>> Entering UpdateTabModuleVersionsByModuleID(moduleID={ModuleID})", moduleID);
            // Update the version guid of each TabModule linked to the updated module
            foreach (ModuleInfo modInfo in this.GetAllTabsModulesByModuleID(moduleID))
            {
                this.ClearCache(modInfo.TabID);
            }

            DataProvider.UpdateTabModuleVersionByModule(moduleID);
			Logger.LogDebug("<<< Exiting UpdateTabModuleVersionsByModuleID(moduleID={ModuleID})", moduleID);
        }

        private bool HasModuleOrderOrPaneChanged(ModuleInfo module)
        {
			Logger.LogDebug(">>> Entering HasModuleOrderOrPaneChanged(module={Module})", module);
            var storedModuleInfo = this.GetTabModule(module.TabModuleID);
			Logger.LogDebug("<<< Exiting HasModuleOrderOrPaneChanged(module={Module})", module);
            return storedModuleInfo == null || storedModuleInfo.ModuleOrder != module.ModuleOrder || storedModuleInfo.PaneName != module.PaneName;
        }

        private void UncopyModule(int tabId, int moduleId, bool softDelete, int originalTabId)
        {
			Logger.LogDebug(">>> Entering UncopyModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete},originalTabId={OriginalTabId})", tabId, moduleId, softDelete, originalTabId);
            ModuleInfo moduleInfo = this.GetModule(moduleId, tabId, false);
            this.DeleteTabModuleInternal(moduleInfo, softDelete, true);
			Logger.LogDebug("UncopyModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete},originalTabId={OriginalTabId}): moduleInfo → {ModuleInfo}", tabId, moduleId, softDelete, originalTabId, moduleInfo);
            var userId = UserController.Instance.GetCurrentUserInfo().UserID;
			Logger.LogDebug("UncopyModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete},originalTabId={OriginalTabId}): userId → {UserId}", tabId, moduleId, softDelete, originalTabId, userId);
            TabChangeTracker.Instance.TrackModuleUncopy(moduleInfo, Null.NullInteger, originalTabId, userId);
			Logger.LogDebug("<<< Exiting UncopyModule(tabId={TabId},moduleId={ModuleId},softDelete={SoftDelete},originalTabId={OriginalTabId})", tabId, moduleId, softDelete, originalTabId);
        }

        private void DeleteTabModuleInternal(ModuleInfo moduleInfo, bool softDelete, bool uncopy = false)
        {
			Logger.LogDebug(">>> Entering DeleteTabModuleInternal(moduleInfo={ModuleInfo},softDelete={SoftDelete},uncopy={Uncopy})", moduleInfo, softDelete, uncopy);
            // save moduleinfo
            if (moduleInfo != null)
            {
                // delete the module instance for the tab
                DataProvider.DeleteTabModule(moduleInfo.TabID, moduleInfo.ModuleID, softDelete, UserController.Instance.GetCurrentUserInfo().UserID);
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.TABMODULE_DELETED) };
				Logger.LogDebug("DeleteTabModuleInternal(moduleInfo={ModuleInfo},softDelete={SoftDelete},uncopy={Uncopy}): log → {Log}", moduleInfo, softDelete, uncopy, log);
                log.LogProperties.Add(new LogDetailInfo("tabId", moduleInfo.TabID.ToString(CultureInfo.InvariantCulture)));
                log.LogProperties.Add(new LogDetailInfo("moduleId", moduleInfo.ModuleID.ToString(CultureInfo.InvariantCulture)));
                LogController.Instance.AddLog(log);

                // reorder all modules on tab
                if (!uncopy)
                {
                    this.UpdateTabModuleOrder(moduleInfo.TabID);

                    // ModuleRemove is only raised when doing a soft delete of the module
                    if (softDelete)
                    {
                        EventManager.Instance.OnModuleRemoved(new ModuleEventArgs { Module = moduleInfo });
                    }
                }

                // check if all modules instances have been deleted
                if (this.GetModule(moduleInfo.ModuleID, Null.NullInteger, true).TabID == Null.NullInteger)
                {
                    // hard delete the module
                    this.DeleteModule(moduleInfo.ModuleID);
                }

                DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.SingleTabModuleCacheKey, moduleInfo.TabModuleID));
                this.ClearCache(moduleInfo.TabID);
            }
			Logger.LogDebug("<<< Exiting DeleteTabModuleInternal(moduleInfo={ModuleInfo},softDelete={SoftDelete},uncopy={Uncopy})", moduleInfo, softDelete, uncopy);
        }
    }
}
