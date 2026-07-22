// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.DDRMenu.TemplateEngine;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Implements the DDR Menu navigation provider.</summary>
    public class DDRMenuNavigationProvider : NavigationProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly ILocaliser localiser;
        private readonly IHostSettings hostSettings;
        private readonly ITabController tabController;
        private DDRMenuControl menuControl;

        /// <summary>Initializes a new instance of the <see cref="DDRMenuNavigationProvider"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with ILocaliser. Scheduled removal in v12.0.0.")]
        public DDRMenuNavigationProvider()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering DDRMenuNavigationProvider()");
		Logger.LogDebug("<<< Exiting DDRMenuNavigationProvider()");
		Logger.LogDebug(">>> Entering DDRMenuNavigationProvider(localiser={Localiser})", localiser);
		Logger.LogDebug("<<< Exiting DDRMenuNavigationProvider(localiser={Localiser})", localiser);
        }

        /// <summary>Initializes a new instance of the <see cref="DDRMenuNavigationProvider"/> class.</summary>
        /// <param name="localiser">The localizer.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DDRMenuNavigationProvider(ILocaliser localiser)
            : this(localiser, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DDRMenuNavigationProvider"/> class.</summary>
        /// <param name="localiser">The localizer.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="tabController">The tab controller.</param>
        public DDRMenuNavigationProvider(ILocaliser localiser, IHostSettings hostSettings, ITabController tabController)
        {
            this.localiser = localiser ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("DDRMenuNavigationProvider(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): hostSettings → {HostSettings1}", localiser, hostSettings, tabController, hostSettings);
			Logger.LogDebug("DDRMenuNavigationProvider(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): localiser → {Localiser1}", localiser, hostSettings, tabController, localiser);
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
			Logger.LogDebug("DDRMenuNavigationProvider(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): tabController → {TabController1}", localiser, hostSettings, tabController, tabController);
        }

        /// <inheritdoc />
        public override Control NavigationControl
        {
            get { return this.menuControl; }
        }

        /// <inheritdoc />
        public override bool SupportsPopulateOnDemand
        {
            get { return false; }
        }

        /// <inheritdoc />
        public override Alignment ControlAlignment { get; set; }

        /// <inheritdoc />
        public override bool IndicateChildren { get; set; }

        /// <inheritdoc />
        public override bool PopulateNodesFromClient { get; set; }

        /// <inheritdoc />
        public override decimal MouseOutHideDelay { get; set; }

        /// <inheritdoc />
        public override decimal StyleBorderWidth { get; set; }

        /// <inheritdoc />
        public override decimal StyleControlHeight { get; set; }

        /// <inheritdoc />
        public override decimal StyleFontSize { get; set; }

        /// <inheritdoc />
        public override decimal StyleIconWidth { get; set; }

        /// <inheritdoc />
        public override decimal StyleNodeHeight { get; set; }

        /// <inheritdoc />
        public override double EffectsDuration { get; set; }

        /// <inheritdoc />
        public override HoverAction MouseOverAction { get; set; }

        /// <inheritdoc />
        public override HoverDisplay MouseOverDisplay { get; set; }

        /// <inheritdoc />
        public override int EffectsShadowStrength { get; set; }

        /// <inheritdoc />
        public override Orientation ControlOrientation { get; set; }

        /// <inheritdoc />
        public override string CSSBreadCrumbRoot { get; set; }

        /// <inheritdoc />
        public override string CSSBreadCrumbSub { get; set; }

        /// <inheritdoc />
        public override string CSSBreak { get; set; }

        /// <inheritdoc />
        public override string CSSContainerRoot { get; set; }

        /// <inheritdoc />
        public override string CSSContainerSub { get; set; }

        /// <inheritdoc />
        public override string CSSIcon { get; set; }

        /// <inheritdoc />
        public override string CSSIndicateChildRoot { get; set; }

        /// <inheritdoc />
        public override string CSSIndicateChildSub { get; set; }

        /// <inheritdoc />
        public override string CSSLeftSeparator { get; set; }

        /// <inheritdoc />
        public override string CSSLeftSeparatorBreadCrumb { get; set; }

        /// <inheritdoc />
        public override string CSSLeftSeparatorSelection { get; set; }

        /// <inheritdoc />
        public override string CSSNode { get; set; }

        /// <inheritdoc />
        public override string CSSNodeHover { get; set; }

        /// <inheritdoc />
        public override string CSSNodeHoverRoot { get; set; }

        /// <inheritdoc />
        public override string CSSNodeHoverSub { get; set; }

        /// <inheritdoc />
        public override string CSSNodeRoot { get; set; }

        /// <inheritdoc />
        public override string CSSNodeSelectedRoot { get; set; }

        /// <inheritdoc />
        public override string CSSNodeSelectedSub { get; set; }

        /// <inheritdoc />
        public override string CSSRightSeparator { get; set; }

        /// <inheritdoc />
        public override string CSSRightSeparatorBreadCrumb { get; set; }

        /// <inheritdoc />
        public override string CSSRightSeparatorSelection { get; set; }

        /// <inheritdoc />
        public override string CSSSeparator { get; set; }

        /// <inheritdoc />
        public override string EffectsShadowColor { get; set; }

        /// <inheritdoc />
        public override string EffectsShadowDirection { get; set; }

        /// <inheritdoc />
        public override string EffectsStyle { get; set; }

        /// <inheritdoc />
        public override string EffectsTransition { get; set; }

        /// <inheritdoc />
        public override string ForceCrawlerDisplay { get; set; }

        /// <inheritdoc />
        public override string ForceDownLevel { get; set; }

        /// <inheritdoc />
        public override string IndicateChildImageExpandedRoot { get; set; }

        /// <inheritdoc />
        public override string IndicateChildImageExpandedSub { get; set; }

        /// <inheritdoc />
        public override string IndicateChildImageRoot { get; set; }

        /// <inheritdoc />
        public override string IndicateChildImageSub { get; set; }

        /// <inheritdoc />
        public override string NodeLeftHTMLBreadCrumbRoot { get; set; }

        /// <inheritdoc />
        public override string NodeLeftHTMLBreadCrumbSub { get; set; }

        /// <inheritdoc />
        public override string NodeLeftHTMLRoot { get; set; }

        /// <inheritdoc />
        public override string NodeLeftHTMLSub { get; set; }

        /// <inheritdoc />
        public override string NodeRightHTMLBreadCrumbRoot { get; set; }

        /// <inheritdoc />
        public override string NodeRightHTMLBreadCrumbSub { get; set; }

        /// <inheritdoc />
        public override string NodeRightHTMLRoot { get; set; }

        /// <inheritdoc />
        public override string NodeRightHTMLSub { get; set; }

        /// <inheritdoc />
        public override string PathImage { get; set; }

        /// <inheritdoc />
        public override string PathSystemImage { get; set; }

        /// <inheritdoc />
        public override string PathSystemScript { get; set; }

        /// <inheritdoc />
        public override string SeparatorHTML { get; set; }

        /// <inheritdoc />
        public override string SeparatorLeftHTML { get; set; }

        /// <inheritdoc />
        public override string SeparatorLeftHTMLActive { get; set; }

        /// <inheritdoc />
        public override string SeparatorLeftHTMLBreadCrumb { get; set; }

        /// <inheritdoc />
        public override string SeparatorRightHTML { get; set; }

        /// <inheritdoc />
        public override string SeparatorRightHTMLActive { get; set; }

        /// <inheritdoc />
        public override string SeparatorRightHTMLBreadCrumb { get; set; }

        /// <inheritdoc />
        public override string StyleBackColor { get; set; }

        /// <inheritdoc />
        public override string StyleFontBold { get; set; }

        /// <inheritdoc />
        public override string StyleFontNames { get; set; }

        /// <inheritdoc />
        public override string StyleForeColor { get; set; }

        /// <inheritdoc />
        public override string StyleHighlightColor { get; set; }

        /// <inheritdoc />
        public override string StyleIconBackColor { get; set; }

        /// <inheritdoc />
        public override string StyleRoot { get; set; }

        /// <inheritdoc />
        public override string StyleSelectionBorderColor { get; set; }

        /// <inheritdoc />
        public override string StyleSelectionColor { get; set; }

        /// <inheritdoc />
        public override string StyleSelectionForeColor { get; set; }

        /// <inheritdoc />
        public override string StyleSub { get; set; }

        /// <inheritdoc />
        public override string WorkImage { get; set; }

        /// <inheritdoc />
        public override List<CustomAttribute> CustomAttributes { get; set; }

        /// <inheritdoc />
        public override string ControlID { get; set; }

        /// <inheritdoc />
        public override string CSSControl { get; set; }

        /// <summary>Gets or sets the style of the menu.</summary>
        public string MenuStyle { get; set; }

        /// <summary>Gets or sets the template arguments, <see cref="TemplateArgument"/>.</summary>
        public List<TemplateArgument> TemplateArguments { get; set; }

        /// <inheritdoc />
        public override void Initialize()
        {
			Logger.LogDebug(">>> Entering Initialize()");
            this.menuControl = new DDRMenuControl(this.localiser, this.hostSettings, this.tabController) { ID = this.ControlID, EnableViewState = false, };
            this.menuControl.NodeClick += this.RaiseEvent_NodeClick;
			Logger.LogDebug("<<< Exiting Initialize()");
			Logger.LogDebug("Initialize(): menuControl → {MenuControl}", menuControl);
        }

        /// <inheritdoc />
        public override void Bind(DNNNodeCollection objNodes)
        {
			Logger.LogDebug(">>> Entering Bind(objNodes={ObjNodes})", objNodes);
            this.Bind(objNodes, true);
			Logger.LogDebug("<<< Exiting Bind(objNodes={ObjNodes})", objNodes);
        }

        /// <summary>Binds the menu nodes.</summary>
        /// <param name="objNodes">The collection of menu nodes.</param>
        /// <param name="localise">A value indicating whether the menu should be localized.</param>
        public void Bind(DNNNodeCollection objNodes, bool localise)
        {
			Logger.LogDebug(">>> Entering Bind(objNodes={ObjNodes},localise={Localise})", objNodes, localise);
            var clientOptions = new List<ClientOption>();
			Logger.LogDebug("Bind(objNodes={ObjNodes},localise={Localise}): clientOptions → {ClientOptions}", objNodes, localise, clientOptions);

            var ignoreProperties = new List<string>
                                   {
                                    "CustomAttributes",
                                    "NavigationControl",
                                    "SupportsPopulateOnDemand",
                                    "NodeXmlPath",
                                    "NodeSelector",
                                    "IncludeContext",
                                    "IncludeHidden",
                                    "IncludeNodes",
                                    "ExcludeNodes",
                                    "NodeManipulator",
                                   };

            foreach (var prop in
                typeof(NavigationProvider).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance))
            {
                if (!string.IsNullOrEmpty(prop.Name) && !ignoreProperties.Contains(prop.Name))
                {
                    var propValue = prop.GetValue(this, null);
                    if (propValue != null)
					Logger.LogDebug("Bind(objNodes={ObjNodes},localise={Localise}): propValue → {PropValue}", objNodes, localise, propValue);
                    {
                        if (propValue is bool)
                        {
                            clientOptions.Add(new ClientBoolean(prop.Name, propValue.ToString()));
                        }
                        else if (propValue is int || propValue is decimal || propValue is double)
                        {
                            clientOptions.Add(new ClientNumber(prop.Name, propValue.ToString()));
                        }
                        else
                        {
                            clientOptions.Add(new ClientString(prop.Name, propValue.ToString()));
                        }
                    }
                }
            }

            if (this.CustomAttributes != null)
            {
                foreach (var attr in this.CustomAttributes)
                {
                    if (!string.IsNullOrEmpty(attr.Name))
                    {
                        clientOptions.Add(new ClientString(attr.Name, attr.Value));
                    }
                }
            }

            if (localise)
            {
                objNodes = this.localiser.LocaliseDNNNodeCollection(objNodes);
            }
			Logger.LogDebug("Bind(objNodes={ObjNodes},localise={Localise}): objNodes → {ObjNodes1}", objNodes, localise, objNodes);

            this.menuControl.RootNode = new MenuNode(objNodes);
            this.menuControl.SkipLocalisation = !localise;
            this.menuControl.MenuSettings = new Settings
            {
                MenuStyle = this.GetCustomAttribute("MenuStyle") ?? this.MenuStyle ?? "DNNMenu",
                NodeXmlPath = this.GetCustomAttribute("NodeXmlPath"),
                NodeSelector = this.GetCustomAttribute("NodeSelector"),
                IncludeContext = Convert.ToBoolean(this.GetCustomAttribute("IncludeContext") ?? "false"),
                IncludeHidden = Convert.ToBoolean(this.GetCustomAttribute("IncludeHidden") ?? "false"),
                IncludeNodes = this.GetCustomAttribute("IncludeNodes"),
                ExcludeNodes = this.GetCustomAttribute("ExcludeNodes"),
                NodeManipulator = this.GetCustomAttribute("NodeManipulator"),
                ClientOptions = clientOptions,
                TemplateArguments = this.TemplateArguments,
            };
			Logger.LogDebug("<<< Exiting Bind(objNodes={ObjNodes},localise={Localise})", objNodes, localise);
        }

        private string GetCustomAttribute(string attributeName)
        {
			Logger.LogDebug(">>> Entering GetCustomAttribute(attributeName={AttributeName})", attributeName);
            string xmlValue = null;
            if (this.CustomAttributes != null)
			Logger.LogDebug("GetCustomAttribute(attributeName={AttributeName}): xmlValue → {XmlValue}", attributeName, xmlValue);
            {
                var xmlAttr = this.CustomAttributes.Find(a => a.Name.Equals(attributeName, StringComparison.InvariantCultureIgnoreCase));
                if (xmlAttr != null)
				Logger.LogDebug("GetCustomAttribute(attributeName={AttributeName}): xmlAttr → {XmlAttr}", attributeName, xmlAttr);
                {
                    xmlValue = xmlAttr.Value;
                }
            }

			Logger.LogDebug("<<< Exiting GetCustomAttribute(attributeName={AttributeName})", attributeName);
            return xmlValue;
        }
    }
}
