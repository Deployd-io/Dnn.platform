// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.ExtensionPoints.Filters;

    public class ExtensionPointManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly object SyncRoot = new object();
        private static readonly CompositionContainer MefCompositionContainer = InitializeMefCompositionContainer();

#pragma warning disable 649

        [ImportMany]
        private IEnumerable<Lazy<IScriptItemExtensionPoint, IExtensionPointData>> scripts;

        [ImportMany]
        private IEnumerable<Lazy<IEditPageTabExtensionPoint, IExtensionPointData>> editPageTabExtensionPoint;

        [ImportMany]
        private IEnumerable<Lazy<IToolBarButtonExtensionPoint, IExtensionPointData>> toolbarButtonExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IEditPagePanelExtensionPoint, IExtensionPointData>> editPagePanelExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuItemExtensionPoint, IExtensionPointData>> ctxMenuItemExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IUserControlExtensionPoint, IExtensionPointData>> userControlExtensionPoints;

        [ImportMany]
        private IEnumerable<Lazy<IMenuItemExtensionPoint, IExtensionPointData>> menuItems;

        [ImportMany]
        private IEnumerable<Lazy<IGridColumnExtensionPoint, IExtensionPointData>> gridColumns;

#pragma warning restore 649

        /// <summary>Initializes a new instance of the <see cref="ExtensionPointManager"/> class.</summary>
        public ExtensionPointManager()
        {
			Logger.LogDebug(">>> Entering ExtensionPointManager()");
            ComposeParts(this);
			Logger.LogDebug("<<< Exiting ExtensionPointManager()");
        }

        public static void ComposeParts(params object[] attributeParts)
        {
			Logger.LogDebug(">>> Entering ComposeParts(attributeParts={AttributeParts})", attributeParts);
            lock (SyncRoot)
            {
                MefCompositionContainer.ComposeParts(attributeParts);
            }
			Logger.LogDebug("<<< Exiting ComposeParts(attributeParts={AttributeParts})", attributeParts);
        }

        public IEnumerable<IEditPageTabExtensionPoint> GetEditPageTabExtensionPoints(string module)
        {
			Logger.LogDebug(">>> Entering GetEditPageTabExtensionPoints(module={Module})", module);
			Logger.LogDebug("<<< Exiting GetEditPageTabExtensionPoints(module={Module})", module);
            return this.GetEditPageTabExtensionPoints(module, null);
        }

        public IEnumerable<IEditPageTabExtensionPoint> GetEditPageTabExtensionPoints(string module, string group)
        {
			Logger.LogDebug("<<< Exiting GetEditPageTabExtensionPoints(module={Module},group={Group})", module, group);
            return from e in this.editPageTabExtensionPoint
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module)
        {
			Logger.LogDebug(">>> Entering GetToolBarButtonExtensionPoints(module={Module})", module);
			Logger.LogDebug("<<< Exiting GetToolBarButtonExtensionPoints(module={Module})", module);
            return this.GetToolBarButtonExtensionPoints(module, null);
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module, string group)
        {
			Logger.LogDebug("<<< Exiting GetToolBarButtonExtensionPoints(module={Module},group={Group})", module, group);
            return this.GetToolBarButtonExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IToolBarButtonExtensionPoint> GetToolBarButtonExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
			Logger.LogDebug("<<< Exiting GetToolBarButtonExtensionPoints(module={Module},group={Group},filter={Filter})", module, group, filter);
            return from e in this.toolbarButtonExtensionPoints
                   where FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IToolBarButtonExtensionPoint GetToolBarButtonExtensionPointFirstByPriority(string module, string name)
        {
			Logger.LogDebug(">>> Entering GetToolBarButtonExtensionPointFirstByPriority(module={Module},name={Name})", module, name);
			Logger.LogDebug("<<< Exiting GetToolBarButtonExtensionPointFirstByPriority(module={Module},name={Name})", module, name);
            return (from e in this.toolbarButtonExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();
        }

        public IEnumerable<IScriptItemExtensionPoint> GetScriptItemExtensionPoints(string module)
        {
			Logger.LogDebug(">>> Entering GetScriptItemExtensionPoints(module={Module})", module);
			Logger.LogDebug("<<< Exiting GetScriptItemExtensionPoints(module={Module})", module);
            return this.GetScriptItemExtensionPoints(module, null);
        }

        public IEnumerable<IScriptItemExtensionPoint> GetScriptItemExtensionPoints(string module, string group)
        {
			Logger.LogDebug("<<< Exiting GetScriptItemExtensionPoints(module={Module},group={Group})", module, group);
            return from e in this.scripts
                   where e.Metadata.Module == module
                       && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IEditPagePanelExtensionPoint> GetEditPagePanelExtensionPoints(string module)
        {
			Logger.LogDebug(">>> Entering GetEditPagePanelExtensionPoints(module={Module})", module);
			Logger.LogDebug("<<< Exiting GetEditPagePanelExtensionPoints(module={Module})", module);
            return this.GetEditPagePanelExtensionPoints(module, null);
        }

        public IEnumerable<IEditPagePanelExtensionPoint> GetEditPagePanelExtensionPoints(string module, string group)
        {
			Logger.LogDebug("<<< Exiting GetEditPagePanelExtensionPoints(module={Module},group={Group})", module, group);
            return from e in this.editPagePanelExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEditPagePanelExtensionPoint GetEditPagePanelExtensionPointFirstByPriority(string module, string name)
        {
			Logger.LogDebug(">>> Entering GetEditPagePanelExtensionPointFirstByPriority(module={Module},name={Name})", module, name);
			Logger.LogDebug("<<< Exiting GetEditPagePanelExtensionPointFirstByPriority(module={Module},name={Name})", module, name);
            return (from e in this.editPagePanelExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();
        }

        public IEnumerable<IContextMenuItemExtensionPoint> GetContextMenuItemExtensionPoints(string module)
        {
			Logger.LogDebug(">>> Entering GetContextMenuItemExtensionPoints(module={Module})", module);
			Logger.LogDebug("<<< Exiting GetContextMenuItemExtensionPoints(module={Module})", module);
            return this.GetContextMenuItemExtensionPoints(module, null);
        }

        public IEnumerable<IContextMenuItemExtensionPoint> GetContextMenuItemExtensionPoints(string module, string group)
        {
			Logger.LogDebug("<<< Exiting GetContextMenuItemExtensionPoints(module={Module},group={Group})", module, group);
            return from e in this.ctxMenuItemExtensionPoints
                   where e.Metadata.Module == module
                        && (string.IsNullOrEmpty(@group) || e.Metadata.Group == @group)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IUserControlExtensionPoint GetUserControlExtensionPointFirstByPriority(string module, string name)
        {
			Logger.LogDebug(">>> Entering GetUserControlExtensionPointFirstByPriority(module={Module},name={Name})", module, name);
			Logger.LogDebug("<<< Exiting GetUserControlExtensionPointFirstByPriority(module={Module},name={Name})", module, name);
            return (from e in this.userControlExtensionPoints
                    where e.Metadata.Module == module && e.Metadata.Name == name
                    orderby e.Metadata.Priority
                    select e.Value).FirstOrDefault();
        }

        public IEnumerable<IUserControlExtensionPoint> GetUserControlExtensionPoints(string module, string group)
        {
			Logger.LogDebug(">>> Entering GetUserControlExtensionPoints(module={Module},group={Group})", module, group);
			Logger.LogDebug("<<< Exiting GetUserControlExtensionPoints(module={Module},group={Group})", module, group);
            return from e in this.userControlExtensionPoints
                   where e.Metadata.Module == module && e.Metadata.Group == @group
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module)
        {
			Logger.LogDebug(">>> Entering GetMenuItemExtensionPoints(module={Module})", module);
			Logger.LogDebug("<<< Exiting GetMenuItemExtensionPoints(module={Module})", module);
            return this.GetMenuItemExtensionPoints(module, null);
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module, string group)
        {
			Logger.LogDebug("<<< Exiting GetMenuItemExtensionPoints(module={Module},group={Group})", module, group);
            return this.GetMenuItemExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IMenuItemExtensionPoint> GetMenuItemExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
			Logger.LogDebug("<<< Exiting GetMenuItemExtensionPoints(module={Module},group={Group},filter={Filter})", module, group, filter);
            return from e in this.menuItems
                   where FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module)
        {
			Logger.LogDebug(">>> Entering GetGridColumnExtensionPoints(module={Module})", module);
			Logger.LogDebug("<<< Exiting GetGridColumnExtensionPoints(module={Module})", module);
            return this.GetGridColumnExtensionPoints(module, null);
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module, string group)
        {
			Logger.LogDebug("<<< Exiting GetGridColumnExtensionPoints(module={Module},group={Group})", module, group);
            return this.GetGridColumnExtensionPoints(module, group, new NoFilter());
        }

        public IEnumerable<IGridColumnExtensionPoint> GetGridColumnExtensionPoints(string module, string group, IExtensionPointFilter filter)
        {
            return from e in this.gridColumns
                   where FilterCondition(e.Metadata, module, @group) && filter.Condition(e.Metadata)
                   orderby e.Value.Order
                   select e.Value;
        }

        private static CompositionContainer InitializeMefCompositionContainer()
        {
			Logger.LogDebug(">>> Entering InitializeMefCompositionContainer()");
            var catalog = new AggregateCatalog();
            var path = Path.Combine(Globals.ApplicationMapPath, "bin");
			Logger.LogDebug("InitializeMefCompositionContainer(): catalog → {Catalog}", catalog);
			Logger.LogDebug("InitializeMefCompositionContainer(): path → {Path}", path);
            catalog.Catalogs.Add(new SafeDirectoryCatalog(path));
			Logger.LogDebug("<<< Exiting InitializeMefCompositionContainer()");
            return new CompositionContainer(catalog, true);
        }

        private static bool FilterCondition(IExtensionPointData data, string module, string group)
        {
			Logger.LogDebug(">>> Entering FilterCondition(data={Data},module={Module},group={Group})", data, module, group);
			Logger.LogDebug("<<< Exiting FilterCondition(data={Data},module={Module},group={Group})", data, module, group);
            return data.Module == module && (string.IsNullOrEmpty(@group) || data.Group == @group);
        }
    }
}
