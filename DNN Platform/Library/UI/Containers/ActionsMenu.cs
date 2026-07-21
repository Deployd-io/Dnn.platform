// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>ActionsMenu provides a menu for a collection of actions.</summary>
    /// <remarks>Inherits from <see cref="Control"/>, and implements the <see cref="IActionControl"/> Interface. It uses the Navigation Providers to implement the Menu.</remarks>
    public class ActionsMenu : Control, IActionControl
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IEventLogger eventLogger;
        private ActionManager actionManager;
        private ModuleAction actionRoot;
        private int expandDepth = -1;

        /// <summary>Initializes a new instance of the <see cref="ActionsMenu"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public ActionsMenu()
            : this(Globals.GetCurrentServiceProvider())
        {
		eventLogger.LogDebug(">>> Entering ActionsMenu()");
		eventLogger.LogDebug("<<< Exiting ActionsMenu()");
		eventLogger.LogDebug(">>> Entering ActionsMenu(serviceProvider={ServiceProvider})", serviceProvider);
		eventLogger.LogDebug("<<< Exiting ActionsMenu(serviceProvider={ServiceProvider})", serviceProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="ActionsMenu"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public ActionsMenu(IServiceProvider serviceProvider)
            : this(serviceProvider, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ActionsMenu"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="eventLogger">The event logger.</param>
        public ActionsMenu(IServiceProvider serviceProvider, IEventLogger eventLogger)
        {
            this.serviceProvider = serviceProvider;
            this.eventLogger = eventLogger ?? serviceProvider.GetRequiredService<IEventLogger>();
			eventLogger.LogDebug("ActionsMenu(serviceProvider={ServiceProvider},eventLogger={EventLogger}): this.serviceProvider → {Arg2}", serviceProvider, eventLogger, this.serviceProvider);
			eventLogger.LogDebug("ActionsMenu(serviceProvider={ServiceProvider},eventLogger={EventLogger}): eventLogger → {EventLogger1}", serviceProvider, eventLogger, eventLogger);
			eventLogger.LogDebug("ActionsMenu(serviceProvider={ServiceProvider},eventLogger={EventLogger}): serviceProvider → {ServiceProvider1}", serviceProvider, eventLogger, serviceProvider);
        }

        /// <inheritdoc />
        public event ActionEventHandler Action;

        /// <summary>Gets the ActionManager instance for this Action control.</summary>
        /// <returns>An ActionManager object.</returns>
        public ActionManager ActionManager => this.actionManager ??= new ActionManager(this.eventLogger, this);

        /// <summary>Gets or sets the Expansion Depth for the Control.</summary>
        /// <returns>An Integer.</returns>
        public int ExpandDepth
        {
            get
            {
                if (!this.PopulateNodesFromClient || !this.ProviderControl.SupportsPopulateOnDemand)
                {
                    return -1;
                }

                return this.expandDepth;
            }

            set
            {
                this.expandDepth = value;
            }
        }

        /// <summary>Gets or sets the Path to the Script Library for the provider.</summary>
        /// <returns>A String.</returns>
        public string PathSystemScript { get; set; }

        /// <summary>Gets or sets a value indicating whether the Menu should be populated from the client.</summary>
        /// <returns>A Boolean.</returns>
        public bool PopulateNodesFromClient { get; set; }

        /// <summary>Gets or sets the Name of the provider to use.</summary>
        /// <returns>A String.</returns>
        public string ProviderName { get; set; } = "DNNMenuNavigationProvider";

        /// <summary>Gets or sets the ModuleControl instance for this Action control.</summary>
        /// <returns>An IModuleControl object.</returns>
        public IModuleControl ModuleControl { get; set; }

        /// <summary>Gets the ActionRoot.</summary>
        /// <returns>A ModuleActionCollection.</returns>
        protected ModuleAction ActionRoot =>
            this.actionRoot ??= new ModuleAction(
                this.ModuleControl.ModuleContext.GetNextActionID(),
                " ",
                string.Empty,
                string.Empty,
                "action.gif");

        /// <summary>Gets the Provider Control.</summary>
        /// <returns>A NavigationProvider.</returns>
        protected NavigationProvider ProviderControl { get; private set; }

        /// <summary>BindMenu binds the Navigation Provider to the Node Collection.</summary>
        protected void BindMenu()
        {
			eventLogger.LogDebug(">>> Entering BindMenu()");
            this.BindMenu(Navigation.GetActionNodes(this.ActionRoot, this, this.ExpandDepth));
			eventLogger.LogDebug("<<< Exiting BindMenu()");
        }

        /// <summary>OnAction raises the <see cref="Action"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnAction(ActionEventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnAction(e={E})", e);
            this.Action?.Invoke(this, e);
			eventLogger.LogDebug("<<< Exiting OnAction(e={E})", e);
        }

        /// <summary>OnInit runs during the controls initialization phase.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnInit(e={E})", e);
            this.ProviderControl = NavigationProvider.Instance(this.serviceProvider, this.ProviderName);
            this.ProviderControl.PopulateOnDemand += this.ProviderControl_PopulateOnDemand;
            base.OnInit(e);
            this.ProviderControl.ControlID = "ctl" + this.ID;
            this.ProviderControl.Initialize();
            this.Controls.Add(this.ProviderControl.NavigationControl);
			eventLogger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>OnLoad runs during the controls load phase.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            // Add the Actions to the Action Root
            this.ActionRoot.Actions.AddRange(this.ModuleControl.ModuleContext.Actions);

            // Set Menu Defaults
            this.SetMenuDefaults();
			eventLogger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>OnPreRender runs during the controls pre-render phase.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            this.BindMenu();
			eventLogger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>BindMenu binds the Navigation Provider to the Node Collection.</summary>
        /// <param name="objNodes">The Nodes collection to bind.</param>
        private void BindMenu(DNNNodeCollection objNodes)
        {
			eventLogger.LogDebug(">>> Entering BindMenu(objNodes={ObjNodes})", objNodes);
            this.Visible = this.ActionManager.DisplayControl(objNodes);
            if (this.Visible)
            {
                // since we always bind we need to clear the nodes for providers that maintain their state
                this.ProviderControl.ClearNodes();
                foreach (DNNNode objNode in objNodes)
                {
                    this.ProcessNodes(objNode);
                }

                this.ProviderControl.Bind(objNodes);
            }
			eventLogger.LogDebug("<<< Exiting BindMenu(objNodes={ObjNodes})", objNodes);
        }

        /// <summary>ProcessNodes processes a single node and its children.</summary>
        /// <param name="objParent">The Node to process.</param>
        private void ProcessNodes(DNNNode objParent)
        {
			eventLogger.LogDebug(">>> Entering ProcessNodes(objParent={ObjParent})", objParent);
            if (!string.IsNullOrEmpty(objParent.JSFunction))
            {
                var postBackEventReference = this.Page.ClientScript.GetPostBackEventReference(this.ProviderControl.NavigationControl, objParent.ID);
                objParent.JSFunction = $"if({objParent.JSFunction}){{{postBackEventReference}}};";
				eventLogger.LogDebug("ProcessNodes(objParent={ObjParent}): postBackEventReference → {PostBackEventReference}", objParent, postBackEventReference);
            }

            foreach (DNNNode objNode in objParent.DNNNodes)
            {
                this.ProcessNodes(objNode);
            }
			eventLogger.LogDebug("<<< Exiting ProcessNodes(objParent={ObjParent})", objParent);
        }

        /// <summary>SetMenuDefaults sets up the default values.</summary>
        private void SetMenuDefaults()
        {
			eventLogger.LogDebug(">>> Entering SetMenuDefaults()");
            try
            {
                //--- original page set attributes ---
                this.ProviderControl.StyleIconWidth = 15;
                this.ProviderControl.MouseOutHideDelay = 500;
                this.ProviderControl.MouseOverAction = NavigationProvider.HoverAction.Expand;
                this.ProviderControl.MouseOverDisplay = NavigationProvider.HoverDisplay.None;

                // style sheet settings
                this.ProviderControl.CSSControl = "ModuleTitle_MenuBar";
                this.ProviderControl.CSSContainerRoot = "ModuleTitle_MenuContainer";
                this.ProviderControl.CSSNode = "ModuleTitle_MenuItem";
                this.ProviderControl.CSSIcon = "ModuleTitle_MenuIcon";
                this.ProviderControl.CSSContainerSub = "ModuleTitle_SubMenu";
                this.ProviderControl.CSSBreak = "ModuleTitle_MenuBreak";
                this.ProviderControl.CSSNodeHover = "ModuleTitle_MenuItemSel";
                this.ProviderControl.CSSIndicateChildSub = "ModuleTitle_MenuArrow";
                this.ProviderControl.CSSIndicateChildRoot = "ModuleTitle_RootMenuArrow";

                this.ProviderControl.PathImage = Globals.ApplicationPath + "/Images/";
                this.ProviderControl.PathSystemImage = Globals.ApplicationPath + "/Images/";
                this.ProviderControl.IndicateChildImageSub = "action_right.gif";
                this.ProviderControl.IndicateChildren = true;
                this.ProviderControl.StyleRoot = "background-color: Transparent; font-size: 1pt;";
                this.ProviderControl.NodeClick += this.MenuItem_Click;
            }
            catch (Exception exc)
            {
				eventLogger.LogError("Exception in SetMenuDefaults(): {Arg0}", ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			eventLogger.LogDebug("<<< Exiting SetMenuDefaults()");
        }

        /// <summary>MenuItem_Click handles the Menu Click event.</summary>
        private void MenuItem_Click(NavigationEventArgs args)
        {
			eventLogger.LogDebug(">>> Entering MenuItem_Click(args={Args})", args);
            if (Globals.NumberMatchRegex.IsMatch(args.ID))
            {
                var action = this.ModuleControl.ModuleContext.Actions.GetActionByID(Convert.ToInt32(args.ID, CultureInfo.InvariantCulture));
                if (!this.ActionManager.ProcessAction(action))
				eventLogger.LogDebug("MenuItem_Click(args={Args}): action → {Action}", args, action);
                {
                    this.OnAction(new ActionEventArgs(action, this.ModuleControl.ModuleContext.Configuration));
                }
            }
			eventLogger.LogDebug("<<< Exiting MenuItem_Click(args={Args})", args);
        }

        /// <summary>ProviderControl_PopulateOnDemand handles the Populate On Demand Event.</summary>
        private void ProviderControl_PopulateOnDemand(NavigationEventArgs args)
        {
			eventLogger.LogDebug(">>> Entering ProviderControl_PopulateOnDemand(args={Args})", args);
            this.SetMenuDefaults();
            this.ActionRoot.Actions.AddRange(this.ModuleControl.ModuleContext.Actions); // Modules how add custom actions in control lifecycle will not have those actions populated...

            var objAction = this.ActionRoot;
            if (this.ActionRoot.ID != Convert.ToInt32(args.ID, CultureInfo.InvariantCulture))
			eventLogger.LogDebug("ProviderControl_PopulateOnDemand(args={Args}): objAction → {ObjAction}", args, objAction);
            {
                objAction = this.ModuleControl.ModuleContext.Actions.GetActionByID(Convert.ToInt32(args.ID, CultureInfo.InvariantCulture));
            }

            if (args.Node == null)
            {
                args.Node = Navigation.GetActionNode(args.ID, this.ProviderControl.ID, objAction, this);
            }

            this.ProviderControl.ClearNodes(); // since we always bind we need to clear the nodes for providers that maintain their state
            this.BindMenu(Navigation.GetActionNodes(objAction, args.Node, this, this.ExpandDepth));
			eventLogger.LogDebug("<<< Exiting ProviderControl_PopulateOnDemand(args={Args})", args);
        }
    }
}
