// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.DDRMenu.Localisation
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    using MenuNode = DotNetNuke.Web.DDRMenu.MenuNode;

    /// <summary>Implements the localization logic for all providers.</summary>
    public partial class Localiser : ILocaliser
    {
        private static bool apiChecked;
        private static ILocalisation localisationApi;

        private readonly IEnumerable<ILocalisation> localizations;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="Localiser"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public Localiser()
            : this(null, null)
        {
		eventLogger.LogDebug(">>> Entering Localiser()");
		eventLogger.LogDebug("<<< Exiting Localiser()");
		eventLogger.LogDebug(">>> Entering Localiser(localizations={Localizations},portalController={PortalController})", localizations, portalController);
		eventLogger.LogDebug("Localiser(localizations={Localizations},portalController={PortalController}): portalController → {PortalController1}", localizations, portalController, portalController);
        }

        /// <summary>Initializes a new instance of the <see cref="Localiser"/> class.</summary>
        /// <param name="localizations">The localization strategies to use.</param>
        /// <param name="portalController">The portal controller.</param>
        public Localiser(IEnumerable<ILocalisation> localizations, IPortalController portalController)
        {
            this.localizations = localizations ?? Globals.GetCurrentServiceProvider().GetServices<ILocalisation>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
			eventLogger.LogDebug("<<< Exiting Localiser(localizations={Localizations},portalController={PortalController})", localizations, portalController);
        }

        private ILocalisation LocalisationApi
        {
            get
            {
                if (!apiChecked)
                {
                    foreach (var api in this.localizations)
                    {
                        if (api.HaveApi())
                        {
                            localisationApi = api;
                            break;
                        }
                    }

                    apiChecked = true;
                }

                return localisationApi;
            }
        }

        /// <inheritdoc />
        public DNNNodeCollection LocaliseDNNNodeCollection(DNNNodeCollection nodes)
        {
			eventLogger.LogDebug(">>> Entering LocaliseDNNNodeCollection(nodes={Nodes})", nodes);
			eventLogger.LogDebug("<<< Exiting LocaliseDNNNodeCollection(nodes={Nodes})", nodes);
            return this.LocalisationApi?.LocaliseNodes(nodes) ?? nodes;
        }

        /// <summary>Localizes a single node.</summary>
        /// <param name="node">The node to localize.</param>
        [DnnDeprecated(10, 0, 0, "Use overload taking a portalId.")]
        public partial void LocaliseNode(MenuNode node)
        {
			eventLogger.LogDebug(">>> Entering LocaliseNode(node={Node})", node);
            this.LocaliseNode(node, this.portalController.GetCurrentSettings().PortalId);
			eventLogger.LogDebug("<<< Exiting LocaliseNode(node={Node})", node);
        }

        /// <inheritdoc />
        public void LocaliseNode(MenuNode node, int portalId)
        {
			eventLogger.LogDebug(">>> Entering LocaliseNode(node={Node},portalId={PortalId})", node, portalId);
            var tab = (node.TabId > 0) ? TabController.Instance.GetTab(node.TabId, Null.NullInteger, false) : null;
            if (tab != null)
            {
				eventLogger.LogDebug("<<< Exiting LocaliseNode(node={Node})", node);
                var localised = this.LocaliseTab(tab, portalId);
				eventLogger.LogDebug("LocaliseNode(node={Node},portalId={PortalId}): localised → {Localised}", node, portalId, localised);
                tab = localised ?? tab;

                if (localised != null)
                {
                    node.TabId = tab.TabID;
					eventLogger.LogDebug("LocaliseNode(node={Node},portalId={PortalId}): node.TabId → {Arg2}", node, portalId, node.TabId);
					eventLogger.LogDebug("LocaliseNode(node={Node},portalId={PortalId}): node.Text → {Arg2}", node, portalId, node.Text);
					eventLogger.LogDebug("LocaliseNode(node={Node},portalId={PortalId}): node.Enabled → {Arg2}", node, portalId, node.Enabled);
                    node.Text = tab.TabName;
                    node.Enabled = !tab.DisableLink;
                    if (!tab.IsVisible)
                    {
                        node.TabId = -1;
                    }
                }

                node.Title = tab.Title;
				eventLogger.LogDebug("LocaliseNode(node={Node},portalId={PortalId}): node.Title → {Arg2}", node, portalId, node.Title);
				eventLogger.LogDebug("LocaliseNode(node={Node},portalId={PortalId}): node.Description → {Arg2}", node, portalId, node.Description);
				eventLogger.LogDebug("LocaliseNode(node={Node},portalId={PortalId}): node.Keywords → {Arg2}", node, portalId, node.Keywords);
                node.Description = tab.Description;
                node.Keywords = tab.KeyWords;
            }
            else
            {
                node.TabId = -1;
            }

            node.Children.ForEach(child => this.LocaliseNode(child, portalId));
        }

        private TabInfo LocaliseTab(TabInfo tab, int portalId)
        {
			eventLogger.LogDebug(">>> Entering LocaliseTab(tab={Tab},portalId={PortalId})", tab, portalId);
			eventLogger.LogDebug("<<< Exiting LocaliseTab(tab={Tab},portalId={PortalId})", tab, portalId);
            return this.LocalisationApi?.LocaliseTab(tab, portalId);
        }
    }
}
