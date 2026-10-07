// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.DDRMenu.TemplateEngine;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Base class for multiple DDR Menu classes.</summary>
    public partial class MenuBase
    {
        private readonly Dictionary<string, string> nodeSelectorAliases = new Dictionary<string, string>
        {
            { "rootonly", "*,0,0" },
            { "rootchildren", "+0" },
            { "currentchildren", "." },
        };

        private readonly ILocaliser localiser;
        private readonly IHostSettings hostSettings;
        private readonly ITabController tabController;
        private Settings menuSettings;
        private HttpContext currentContext;
        private PortalSettings hostPortalSettings;

        /// <summary>Initializes a new instance of the <see cref="MenuBase"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with ILocaliser. Scheduled removal in v12.0.0.")]
        public MenuBase()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MenuBase"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public MenuBase(ILocaliser localiser)
            : this(localiser, null, null)
        {
		eventLogger.LogError(">>> Entering MenuBase(localiser={Localiser})", localiser);
        eventLogger.LogWarning("<<< Exiting MenuBase(localiser={Localiser})", localiser);
        }

        /// <summary>Initializes a new instance of the <see cref="MenuBase"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="tabController">The tab controller.</param>
        public MenuBase(ILocaliser localiser, IHostSettings hostSettings, ITabController tabController)
        {
			eventLogger.LogError(">>> Entering MenuBase(localiser={Localiser},hostSettings={HostSettings},tabController={TabController})", localiser, hostSettings, tabController);
			eventLogger.LogDebug("MenuBase(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): hostSettings → {HostSettings1}", localiser, hostSettings, tabController, hostSettings);
			eventLogger.LogDebug("MenuBase(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): tabController → {TabController1}", localiser, hostSettings, tabController, tabController);
            this.localiser = localiser ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>();
			eventLogger.LogWarning("<<< Exiting MenuBase(localiser={Localiser},hostSettings={HostSettings},tabController={TabController})", localiser, hostSettings, tabController);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        /// <summary>Gets or sets the template definition.</summary>
        public TemplateDefinition TemplateDef { get; set; }

        /// <summary>Gets the portal settings for the current portal.</summary>
        // TODO: In v11 we should replace this by IPortalSettings and make it private or instantiate PortalSettings in the constructor.
        [Obsolete("Deprecated in DotNetNuke 9.8.1. This should not have been public. Scheduled removal in v11.0.0.")]
        internal PortalSettings HostPortalSettings => this.hostPortalSettings ??= PortalSettings.Current;

        /// <summary>Gets or sets the root node.</summary>
        internal MenuNode RootNode { get; set; }

        /// <summary>Gets or sets a value indicating whether to skip localization.</summary>
        internal bool SkipLocalisation { get; set; }

        private HttpContext CurrentContext => this.currentContext ??= HttpContext.Current;

        /// <summary>Instantiates the MenuBase.</summary>
        /// <param name="menuStyle">The menu style to use.</param>
        /// <returns>A new instance of <see cref="MenuBase"/> using the provided menu style.</returns>
        [DnnDeprecated(10, 0, 0, "Please use overload with ILocaliser")]
        public static partial MenuBase Instantiate(string menuStyle) =>
            Instantiate(Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>(), menuStyle);

        /// <summary>Instantiates the MenuBase.</summary>
        /// <param name="localiser">The tab localizer.</param>
        /// <param name="menuStyle">The menu style to use.</param>
        /// <returns>A new instance of <see cref="MenuBase"/> using the provided menu style.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload with IHostSettings")]
        public static partial MenuBase Instantiate(ILocaliser localiser, string menuStyle)
            => Instantiate(localiser, Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>(), menuStyle);

        /// <summary>Instantiates the MenuBase.</summary>
        /// <param name="localiser">The tab localizer.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="tabController">The tab controller.</param>
        /// <param name="menuStyle">The menu style to use.</param>
        /// <returns>A new instance of <see cref="MenuBase"/> using the provided menu style.</returns>
        public static MenuBase Instantiate(ILocaliser localiser, IHostSettings hostSettings, ITabController tabController, string menuStyle)
        {
			eventLogger.LogError(">>> Entering Instantiate(menuStyle={MenuStyle})", menuStyle);
            try
            {
                var templateDef = TemplateDefinition.FromName(hostSettings, menuStyle, "*menudef.xml");
				eventLogger.LogError("Instantiate(menuStyle={MenuStyle}): templateDef → {TemplateDef}", menuStyle, templateDef);
                return new MenuBase(localiser, hostSettings, tabController) { TemplateDef = templateDef, };
            }
            catch (Exception exc)
            {
                throw new ApplicationException($"Couldn't load menu style '{menuStyle}': {exc}");
            }
        }

        /// <summary>Applies the provided settings.</summary>
        /// <param name="settings">The settings to apply.</param>
        internal void ApplySettings(Settings settings)
        {
			eventLogger.LogError(">>> Entering ApplySettings(settings={Settings})", settings);
            this.menuSettings = settings;
			eventLogger.LogDebug("ApplySettings(settings={Settings}): menuSettings → {MenuSettings}", settings, menuSettings);
        eventLogger.LogDebug("ApplySettings(settings={Settings}): this.menuSettings → {Arg1}", settings, this.menuSettings);
        eventLogger.LogWarning("<<< Exiting ApplySettings(settings={Settings})", settings);
        }

        /// <summary>Runs menu initialization logic that should be done before rendering the menu.</summary>
        internal virtual void PreRender()
        {
			eventLogger.LogError(">>> Entering PreRender()");
            this.TemplateDef.AddTemplateArguments(this.menuSettings.TemplateArguments, true);
            this.TemplateDef.AddClientOptions(this.menuSettings.ClientOptions, true);

            if (!string.IsNullOrEmpty(this.menuSettings.NodeXmlPath))
            {
                this.LoadNodeXml();
            }

            if (!string.IsNullOrEmpty(this.menuSettings.NodeSelector))
            {
                this.ApplyNodeSelector();
            }

            if (!string.IsNullOrEmpty(this.menuSettings.IncludeNodes))
            {
                this.FilterNodes(this.menuSettings.IncludeNodes, false);
            }

            if (!string.IsNullOrEmpty(this.menuSettings.ExcludeNodes))
            {
                this.FilterNodes(this.menuSettings.ExcludeNodes, true);
            }

            if (string.IsNullOrEmpty(this.menuSettings.NodeXmlPath) && !this.SkipLocalisation)
            {
#pragma warning disable CS0618 // Type or member is obsolete

                // TODO: In Dnn v11, replace this to use IPortalSettings private field instantiate in constructor
                this.localiser.LocaliseNode(this.RootNode, this.HostPortalSettings.PortalId);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            if (!string.IsNullOrEmpty(this.menuSettings.NodeManipulator))
            {
                this.ApplyNodeManipulator();
            }

            if (!this.menuSettings.IncludeHidden)
            {
                this.FilterHiddenNodes(this.RootNode);
            }

            var imagePathOption =
                this.menuSettings.ClientOptions.Find(o => o.Name.Equals("PathImage", StringComparison.InvariantCultureIgnoreCase));
            this.RootNode.ApplyContext(
                imagePathOption == null ? DNNContext.Current.PortalSettings.HomeDirectory : imagePathOption.Value);

            this.TemplateDef.PreRender();
        }

        /// <summary>Renders the menu.</summary>
        /// <param name="htmlWriter">The HTML writer to which to render the menu.</param>
        internal void Render(HtmlTextWriter htmlWriter)
        {
			eventLogger.LogError(">>> Entering Render(htmlWriter={HtmlWriter})", htmlWriter);
            if (this.hostSettings.DebugMode)
            {
                htmlWriter.Write("<!-- DDRmenu v07.04.01 - {0} template -->", this.menuSettings.MenuStyle);
            }

            UserInfo user = null;
			eventLogger.LogError("Render(htmlWriter={HtmlWriter}): user → {User}", htmlWriter, user);
            if (this.menuSettings.IncludeContext)
            {
                user = UserController.Instance.GetCurrentUserInfo();
                user.Roles = user.Roles; // Touch roles to populate
				eventLogger.LogDebug("Render(htmlWriter={HtmlWriter}): user.Roles → {Arg1}", htmlWriter, user.Roles);
            }

            this.TemplateDef.AddClientOptions(new List<ClientOption> { new ClientString("MenuStyle", this.menuSettings.MenuStyle) }, false);

			eventLogger.LogWarning("<<< Exiting Render(htmlWriter={HtmlWriter})", htmlWriter);
            this.TemplateDef.Render(new MenuXml { root = this.RootNode, user = user }, htmlWriter);
        }

        /// <summary>Gets the physical path from a virtual path.</summary>
        /// <param name="path">The virtual path to map.</param>
        /// <returns>The full physical path to the file.</returns>
        protected string MapPath(string path)
        {
			eventLogger.LogError(">>> Entering MapPath(path={Path})", path);
            eventLogger.LogWarning("<<< Exiting MapPath(path={Path})", path);
            return string.IsNullOrEmpty(path) ? string.Empty : Path.GetFullPath(this.CurrentContext.Server.MapPath(path));
        }

        private static List<string> SplitAndTrim(string str)
        {
			eventLogger.LogError(">>> Entering SplitAndTrim(str={Str})", str);
			eventLogger.LogWarning("<<< Exiting SplitAndTrim(str={Str})", str);
            return new List<string>(str.Split(',')).ConvertAll(s => s.Trim().ToLowerInvariant());
        }

        private void LoadNodeXml()
        {
			eventLogger.LogError(">>> Entering LoadNodeXml()");
            this.menuSettings.NodeXmlPath =
                this.MapPath(
                    new PathResolver(this.hostSettings, this.TemplateDef.Folder).Resolve(
                        this.menuSettings.NodeXmlPath,
                        PathResolver.RelativeTo.Manifest,
                        PathResolver.RelativeTo.Skin,
                        PathResolver.RelativeTo.Module,
                        PathResolver.RelativeTo.Portal,
                        PathResolver.RelativeTo.Dnn));

            var cache = this.CurrentContext.Cache;
            this.RootNode = cache[this.menuSettings.NodeXmlPath] as MenuNode;
            if (this.RootNode != null)
            {
				eventLogger.LogDebug("LoadNodeXml(): this.RootNode → {Arg0}", this.RootNode);
                return;
            }

            using (var reader = XmlReader.Create(this.menuSettings.NodeXmlPath))
            {
                reader.ReadToFollowing("root");
                this.RootNode = (MenuNode)new XmlSerializer(typeof(MenuNode), string.Empty).Deserialize(reader);
            }

            cache.Insert(this.menuSettings.NodeXmlPath, this.RootNode, new CacheDependency(this.menuSettings.NodeXmlPath));
			eventLogger.LogWarning("<<< Exiting LoadNodeXml()");
        }

        private void FilterNodes(string nodeString, bool exclude)
        {
			eventLogger.LogError(">>> Entering FilterNodes(nodeString={NodeString},exclude={Exclude})", nodeString, exclude);
            var nodeTextStrings = SplitAndTrim(nodeString);
            var filteredNodes = new List<MenuNode>();
			eventLogger.LogError("FilterNodes(nodeString={NodeString},exclude={Exclude}): filteredNodes → {FilteredNodes}", nodeString, exclude, filteredNodes);
            var flattenedNodes = new MenuNode();
			eventLogger.LogError("FilterNodes(nodeString={NodeString},exclude={Exclude}): flattenedNodes → {FlattenedNodes}", nodeString, exclude, flattenedNodes);
			eventLogger.LogError("FilterNodes(nodeString={NodeString},exclude={Exclude}): nodeTextStrings → {NodeTextStrings}", nodeString, exclude, nodeTextStrings);

            foreach (var nodeText in nodeTextStrings)
            {
                if (nodeText.StartsWith("["))
                {
                    var roleName = nodeText.Substring(1, nodeText.Length - 2);
					eventLogger.LogError("FilterNodes(nodeString={NodeString},exclude={Exclude}): roleName → {RoleName}", nodeString, exclude, roleName);
                    filteredNodes.AddRange(
                        this.RootNode.Children.FindAll(
                            n =>
                            {
                                // no need to check when the node is not a page
                                if (n.TabId <= 0)
                                {
                                    return false;
                                }

                                var tab = TabController.Instance.GetTab(n.TabId, Null.NullInteger, false);
								eventLogger.LogError("FilterNodes(nodeString={NodeString},exclude={Exclude}): tab → {Tab}", nodeString, exclude, tab);
                                foreach (IPermissionInfo perm in tab.TabPermissions)
                                {
                                    if (perm.AllowAccess &&
                                        perm.PermissionKey == "VIEW" &&
                                        (perm.RoleId == -1 || perm.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }));
                }
                else if (nodeText.StartsWith("#"))
                {
                    var tagName = nodeText.Substring(1, nodeText.Length - 1);
					eventLogger.LogError("FilterNodes(nodeString={NodeString},exclude={Exclude}): tagName → {TagName}", nodeString, exclude, tagName);
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        // flatten nodes first. tagged pages should be flattened and not heirarchical
                        if (flattenedNodes != new MenuNode())
                        {
                            flattenedNodes.Children = this.RootNode.FlattenChildren(this.RootNode);
                        }

                        filteredNodes.AddRange(
                            flattenedNodes.Children.FindAll(
                                n =>
                                {
                                    var tab = this.tabController.GetTab(n.TabId, Null.NullInteger, false);
                                    return tab.Terms.Any(x => x.Name.ToLowerInvariant() == tagName);
                                }));
                    }
                }
                else
                {
                    filteredNodes.AddRange(this.RootNode.FindAllByNameOrId(nodeText));
                }
            }

            // if filtered for folksonomy tags, use flat tree to get all related pages in nodeselection
            if (flattenedNodes.HasChildren())
            {
                this.RootNode = flattenedNodes;
				eventLogger.LogDebug("FilterNodes(nodeString={NodeString},exclude={Exclude}): this.RootNode → {Arg2}", nodeString, exclude, this.RootNode);
            }

            if (exclude)
            {
                this.RootNode.RemoveAll(filteredNodes);
            }
            else
            {
                this.RootNode.Children.RemoveAll(n => filteredNodes.Contains(n) == exclude);
            }
			eventLogger.LogWarning("<<< Exiting FilterNodes(nodeString={NodeString},exclude={Exclude})", nodeString, exclude);
        }

        private void FilterHiddenNodes(MenuNode parentNode)
        {
			eventLogger.LogError(">>> Entering FilterHiddenNodes(parentNode={ParentNode})", parentNode);
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var filteredNodes = new List<MenuNode>();
			eventLogger.LogError("FilterHiddenNodes(parentNode={ParentNode}): filteredNodes → {FilteredNodes}", parentNode, filteredNodes);
            filteredNodes.AddRange(
                parentNode.Children.FindAll(
                    n =>
                    {
						eventLogger.LogError("FilterHiddenNodes(parentNode={ParentNode}): portalSettings → {PortalSettings}", parentNode, portalSettings);
                        var tab = TabController.Instance.GetTab(n.TabId, portalSettings.PortalId);
                        return tab == null || !tab.IsVisible;
                    }));

            parentNode.Children.RemoveAll(n => filteredNodes.Contains(n));

            parentNode.Children.ForEach(this.FilterHiddenNodes);
			eventLogger.LogWarning("<<< Exiting FilterHiddenNodes(parentNode={ParentNode})", parentNode);
        }

        private void ApplyNodeSelector()
        {
			eventLogger.LogError(">>> Entering ApplyNodeSelector()");
            string selector;
            if (!this.nodeSelectorAliases.TryGetValue(this.menuSettings.NodeSelector.ToLowerInvariant(), out selector))
            {
                selector = this.menuSettings.NodeSelector;
				eventLogger.LogDebug("ApplyNodeSelector(): selector → {Selector}", selector);
            }

            var selectorSplit = SplitAndTrim(selector);
			eventLogger.LogError("ApplyNodeSelector(): selectorSplit → {SelectorSplit}", selectorSplit);

            var currentTabId = TabController.CurrentPage.TabID;

            var newRoot = this.RootNode;
			eventLogger.LogError("ApplyNodeSelector(): newRoot → {NewRoot}", newRoot);

            var rootSelector = selectorSplit[0];
            if (rootSelector != "*")
            {
                if (rootSelector.StartsWith("+"))
                {
                    var depth = Convert.ToInt32(rootSelector);
					eventLogger.LogError("ApplyNodeSelector(): depth → {Depth}", depth);
                    newRoot = this.RootNode;
                    for (var i = 0; i <= depth; i++)
                    {
                        newRoot = newRoot.Children.Find(n => n.Breadcrumb);
                        if (newRoot == null)
                        {
                            this.RootNode = new MenuNode();
                            return;
                        }
                    }
                }
                else if (rootSelector.StartsWith("-") || rootSelector == "0" || rootSelector == ".")
                {
                    newRoot = this.RootNode.FindById(currentTabId);
                    if (newRoot == null)
                    {
                        this.RootNode = new MenuNode();
                        return;
                    }

                    if (rootSelector.StartsWith("-"))
                    {
						eventLogger.LogError("ApplyNodeSelector(): n → {N}", n);
                        for (var n = Convert.ToInt32(rootSelector); n < 0; n++)
                        {
                            if (newRoot.Parent != null)
                            {
                                newRoot = newRoot.Parent;
                            }
                        }
                    }
                }
                else
                {
                    newRoot = this.RootNode.FindByNameOrId(rootSelector);
                    if (newRoot == null)
                    {
                        this.RootNode = new MenuNode();
                        return;
                    }
                }
            }

            // ReSharper disable PossibleNullReferenceException
            this.RootNode = new MenuNode(newRoot.Children);

            // ReSharper restore PossibleNullReferenceException
            if (selectorSplit.Count > 1)
            {
                for (var n = Convert.ToInt32(selectorSplit[1]); n > 0; n--)
                {
                    var newChildren = new List<MenuNode>();
					eventLogger.LogError("ApplyNodeSelector(): newChildren → {NewChildren}", newChildren);
                    foreach (var child in this.RootNode.Children)
                    {
                        newChildren.AddRange(child.Children);
                    }

                    this.RootNode = new MenuNode(newChildren);
                }
            }

            if (selectorSplit.Count > 2)
            {
                var newChildren = this.RootNode.Children;
                for (var n = Convert.ToInt32(selectorSplit[2]); n > 0; n--)
                {
                    var nextChildren = new List<MenuNode>();
					eventLogger.LogError("ApplyNodeSelector(): nextChildren → {NextChildren}", nextChildren);
                    foreach (var child in newChildren)
                    {
                        nextChildren.AddRange(child.Children);
                    }

                    newChildren = nextChildren;
                }

                foreach (var node in newChildren)
                {
                    node.Children = null;
					eventLogger.LogDebug("ApplyNodeSelector(): node.Children → {Arg0}", node.Children);
                }
            }
			eventLogger.LogWarning("<<< Exiting ApplyNodeSelector()");
        }

        private void ApplyNodeManipulator()
        {
			eventLogger.LogError(">>> Entering ApplyNodeManipulator()");
			eventLogger.LogWarning("<<< Exiting ApplyNodeManipulator()");
            // TODO: In Dnn v11, replace this.HostPortalSettings to use IPortalSettings private field instantiate in constructor
#pragma warning disable CS0618 // Type or member is obsolete
            this.RootNode =
                new MenuNode(
                    ((INodeManipulator)Activator.CreateInstance(BuildManager.GetType(this.menuSettings.NodeManipulator, true, true))).
                        ManipulateNodes(this.RootNode.Children, this.HostPortalSettings));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
