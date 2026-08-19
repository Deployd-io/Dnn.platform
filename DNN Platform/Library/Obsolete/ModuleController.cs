// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <content>The deprecated methods for ModuleController.</content>
    public partial class ModuleController
    {
        /// <summary>Deserializes the module.</summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="module">ModuleInfo of current module.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial void DeserializeModule(
            XmlNode nodeModule,
            ModuleInfo module,
            int portalId,
            int tabId)
        {
			Logger.LogDebug(">>> Entering DeserializeModule(nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId})", nodeModule, module, portalId, tabId);
            using var scope = Globals.GetOrCreateServiceScope();
			Logger.LogDebug("DeserializeModule(nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId}): scope → {Scope}", nodeModule, module, portalId, tabId, scope);
            DeserializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), nodeModule, module, portalId, tabId);
			Logger.LogDebug("<<< Exiting DeserializeModule(nodeModule={NodeModule},module={Module},portalId={PortalId},tabId={TabId})", nodeModule, module, portalId, tabId);
        }

        /// <summary>Deserializes the module.</summary>
        /// <param name="nodeModule">The node module.</param>
        /// <param name="nodePane">The node pane.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        /// <param name="mergeTabs">The merge tabs.</param>
        /// <param name="hModules">The modules.</param>
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial void DeserializeModule(
            XmlNode nodeModule,
            XmlNode nodePane,
            int portalId,
            int tabId,
            PortalTemplateModuleAction mergeTabs,
            Hashtable hModules)
        {
			Logger.LogDebug(">>> Entering DeserializeModule(nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules})", nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);
            using var scope = Globals.GetOrCreateServiceScope();
			Logger.LogDebug("DeserializeModule(nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules}): scope → {Scope}", nodeModule, nodePane, portalId, tabId, mergeTabs, hModules, scope);
            DeserializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);
			Logger.LogDebug("<<< Exiting DeserializeModule(nodeModule={NodeModule},nodePane={NodePane},portalId={PortalId},tabId={TabId},mergeTabs={MergeTabs},hModules={HModules})", nodeModule, nodePane, portalId, tabId, mergeTabs, hModules);
        }

        /// <summary>Serializes the metadata of a module (and optionally its contents) to an XML node.</summary>
        /// <param name="xmlModule">The XML Document to use for the Module.</param>
        /// <param name="module">The ModuleInfo object to serialize.</param>
        /// <param name="includeContent">A flag that determines whether the content of the module is serialized.</param>
        /// <returns>An <see cref="XmlNode"/> representing the module.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with IBusinessControllerProvider")]
        public static partial XmlNode SerializeModule(XmlDocument xmlModule, ModuleInfo module, bool includeContent)
        {
			Logger.LogDebug(">>> Entering SerializeModule(xmlModule={XmlModule},module={Module},includeContent={IncludeContent})", xmlModule, module, includeContent);
            using var scope = Globals.GetOrCreateServiceScope();
			Logger.LogDebug("SerializeModule(xmlModule={XmlModule},module={Module},includeContent={IncludeContent}): scope → {Scope}", xmlModule, module, includeContent, scope);
			Logger.LogDebug("<<< Exiting SerializeModule(xmlModule={XmlModule},module={Module},includeContent={IncludeContent})", xmlModule, module, includeContent);
            return SerializeModule(scope.ServiceProvider.GetRequiredService<IBusinessControllerProvider>(), xmlModule, module, includeContent);
        }
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleController));
    }
}
