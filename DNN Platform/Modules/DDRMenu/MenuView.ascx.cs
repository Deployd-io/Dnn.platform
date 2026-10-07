// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.Localisation;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A module view to display a menu.</summary>
    public partial class MenuView : ModuleBase
    {
        private readonly ILocaliser localiser;
        private readonly IHostSettings hostSettings;
        private readonly ITabController tabController;
        private MenuBase menu;

        /// <summary>Initializes a new instance of the <see cref="MenuView"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with ILocaliser. Scheduled removal in v12.0.0.")]
        public MenuView()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MenuView"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public MenuView(ILocaliser localiser)
            : this(localiser, null, null)
        {
		eventLogger.LogError(">>> Entering MenuView(localiser={Localiser})", localiser);
        eventLogger.LogWarning("<<< Exiting MenuView(localiser={Localiser})", localiser);
        }

        /// <summary>Initializes a new instance of the <see cref="MenuView"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="tabController">The tab controller.</param>
        public MenuView(ILocaliser localiser, IHostSettings hostSettings, ITabController tabController)
        {
			eventLogger.LogError(">>> Entering MenuView(localiser={Localiser},hostSettings={HostSettings},tabController={TabController})", localiser, hostSettings, tabController);
			eventLogger.LogDebug("MenuView(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): hostSettings → {HostSettings1}", localiser, hostSettings, tabController, hostSettings);
			eventLogger.LogDebug("MenuView(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): tabController → {TabController1}", localiser, hostSettings, tabController, tabController);
            this.localiser = localiser ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>();
			eventLogger.LogWarning("<<< Exiting MenuView(localiser={Localiser},hostSettings={HostSettings},tabController={TabController})", localiser, hostSettings, tabController);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogError(">>> Entering OnPreRender(e={E})", e);
            using (new DNNContext(this))
            {
                try
                {
                    base.OnPreRender(e);

                    var menuStyle = this.GetStringSetting("MenuStyle");
                    if (string.IsNullOrEmpty(menuStyle))
                    {
                        this.menu = null;
                        return;
                    }

                    var menuSettings = new Settings
                    {
                        MenuStyle = this.GetStringSetting("MenuStyle"),
                        NodeXmlPath = this.GetStringSetting("NodeXmlPath"),
                        NodeSelector = this.GetStringSetting("NodeSelector"),
                        IncludeContext = this.GetBoolSetting("IncludeContext"),
                        IncludeHidden = this.GetBoolSetting("IncludeHidden"),
                        IncludeNodes = this.GetStringSetting("IncludeNodes"),
                        ExcludeNodes = this.GetStringSetting("ExcludeNodes"),
                        NodeManipulator = this.GetStringSetting("NodeManipulator"),
                        TemplateArguments =
                                            DDRMenu.Settings.TemplateArgumentsFromSettingString(this.GetStringSetting("TemplateArguments")),
                        ClientOptions =
                                            DDRMenu.Settings.ClientOptionsFromSettingString(this.GetStringSetting("ClientOptions")),
                    };

                    MenuNode rootNode = null;
                    if (string.IsNullOrEmpty(menuSettings.NodeXmlPath))
                    {
                        rootNode =
                            new MenuNode(
                                this.localiser.LocaliseDNNNodeCollection(
                                    Navigation.GetNavigationNodes(
                                        this.ClientID,
                                        Navigation.ToolTipSource.None,
                                        -1,
                                        -1,
                                        DNNAbstract.GetNavNodeOptions(true))));
                    }

                    this.menu = MenuBase.Instantiate(this.localiser, this.hostSettings, this.tabController, menuStyle);
                    this.menu.RootNode = rootNode;
                    this.menu.ApplySettings(menuSettings);

                    this.menu.PreRender();
                }
                catch (Exception exc)
                {
					eventLogger.LogError("Exception in OnPreRender(e={E}): {Arg1}", e, exc.Message);
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
			eventLogger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter htmlWriter)
        {
			eventLogger.LogError(">>> Entering Render(htmlWriter={HtmlWriter})", htmlWriter);
            using (new DNNContext(this))
            {
                try
                {
                    base.Render(htmlWriter);
                    if (this.menu == null)
                    {
                        htmlWriter.WriteEncodedText("Please specify menu style in settings.");
                    }
                    else
                    {
                        this.menu.Render(htmlWriter);
                    }
                }
                catch (Exception exc)
                {
					eventLogger.LogError("Exception in Render(htmlWriter={HtmlWriter}): {Arg1}", htmlWriter, exc.Message);
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
			eventLogger.LogWarning("<<< Exiting Render(htmlWriter={HtmlWriter})", htmlWriter);
        }
    }
}
