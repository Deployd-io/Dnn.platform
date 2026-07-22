// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.DDRMenu.DNNCommon;
    using DotNetNuke.Web.DDRMenu.Localisation;
    using DotNetNuke.Web.DDRMenu.TemplateEngine;

    using Microsoft.Extensions.DependencyInjection;

    public class SkinObject : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IHostSettings hostSettings;
        private readonly ILocaliser localiser;
        private readonly ITabController tabController;
        private MenuBase menu;

        /// <summary>Initializes a new instance of the <see cref="SkinObject"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with ILocaliser. Scheduled removal in v12.0.0.")]
        public SkinObject()
            : this(null, null, null)
        {
		Logger.LogDebug(">>> Entering SkinObject()");
		Logger.LogDebug("<<< Exiting SkinObject()");
		Logger.LogDebug(">>> Entering SkinObject(localiser={Localiser})", localiser);
		Logger.LogDebug("<<< Exiting SkinObject(localiser={Localiser})", localiser);
        }

        /// <summary>Initializes a new instance of the <see cref="SkinObject"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public SkinObject(ILocaliser localiser)
            : this(localiser, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SkinObject"/> class.</summary>
        /// <param name="localiser">The tab localizer.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="tabController">The tab controller.</param>
        public SkinObject(ILocaliser localiser, IHostSettings hostSettings, ITabController tabController)
        {
            this.localiser = localiser ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocaliser>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("SkinObject(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): hostSettings → {HostSettings1}", localiser, hostSettings, tabController, hostSettings);
			Logger.LogDebug("SkinObject(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): localiser → {Localiser1}", localiser, hostSettings, tabController, localiser);
            this.tabController = tabController ?? Globals.GetCurrentServiceProvider().GetRequiredService<ITabController>();
			Logger.LogDebug("SkinObject(localiser={Localiser},hostSettings={HostSettings},tabController={TabController}): tabController → {TabController1}", localiser, hostSettings, tabController, tabController);
        }

        public string MenuStyle { get; set; }

        public string NodeXmlPath { get; set; }

        public string NodeSelector { get; set; }

        public bool IncludeContext { get; set; }

        public bool IncludeHidden { get; set; }

        public string IncludeNodes { get; set; }

        public string ExcludeNodes { get; set; }

        public string NodeManipulator { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<ClientOption> ClientOptions { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public List<TemplateArgument> TemplateArguments { get; set; }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            using (new DNNContext(this))
            {
                try
                {
                    base.OnPreRender(e);

                    this.menu = MenuBase.Instantiate(this.localiser, this.hostSettings, this.tabController, this.MenuStyle);
                    this.menu.ApplySettings(
                        new Settings
                        {
                            MenuStyle = this.MenuStyle,
                            NodeXmlPath = this.NodeXmlPath,
                            NodeSelector = this.NodeSelector,
                            IncludeContext = this.IncludeContext,
                            IncludeHidden = this.IncludeHidden,
                            IncludeNodes = this.IncludeNodes,
                            ExcludeNodes = this.ExcludeNodes,
                            NodeManipulator = this.NodeManipulator,
                            ClientOptions = this.ClientOptions,
                            TemplateArguments = this.TemplateArguments,
                        });

                    if (string.IsNullOrEmpty(this.NodeXmlPath))
                    {
                        this.menu.RootNode =
                            new MenuNode(
                                this.localiser.LocaliseDNNNodeCollection(
                                    Navigation.GetNavigationNodes(
                                        this.ClientID,
                                        Navigation.ToolTipSource.None,
                                        -1,
                                        -1,
                                        DNNAbstract.GetNavNodeOptions(true))));
                    }

                    this.menu.PreRender();
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in OnPreRender(e={E}): {Arg1}", e, ex.Message);
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            using (new DNNContext(this))
            {
                try
                {
                    base.Render(writer);
                    this.menu.Render(writer);
                }
                catch (Exception exc)
                {
					Logger.LogError("Exception in Render(writer={Writer}): {Arg1}", writer, ex.Message);
                    Exceptions.ProcessModuleLoadException(this, exc);
                }
            }
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
