// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Modules.NavigationProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>A control which renders module actions as a drop-down list.</summary>
    public partial class DropDownActions : ActionBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DropDownActions.ascx));
        private readonly IServiceProvider serviceProvider;
        private string strProviderName = "DNNDropDownNavigationProvider";

        /// <summary>Initializes a new instance of the <see cref="DropDownActions"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public DropDownActions()
            : this(Globals.GetCurrentServiceProvider())
        {
		Logger.LogDebug(">>> Entering DropDownActions()");
        }

		Logger.LogDebug("<<< Exiting DropDownActions()");
        /// <summary>Initializes a new instance of the <see cref="DropDownActions"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IEventLogger. Scheduled removal in v12.0.0.")]
        public DropDownActions(IServiceProvider serviceProvider)
            : this(serviceProvider, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DropDownActions"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DropDownActions(IServiceProvider serviceProvider, IEventLogger eventLogger)
            : base(eventLogger ?? serviceProvider.GetRequiredService<IEventLogger>())
        {
            this.serviceProvider = serviceProvider;
			Logger.LogDebug("DropDownActions(serviceProvider={ServiceProvider},eventLogger={EventLogger}): serviceProvider → {ServiceProvider1}", serviceProvider, eventLogger, serviceProvider);
        }

        public NavigationProvider Control { get; private set; }

        public string ProviderName
        {
            get { return this.strProviderName; }
            set { }
        }

        public void BindDropDown()
        {
			Logger.LogDebug(">>> Entering BindDropDown()");
            var objNodes = Navigation.GetActionNodes(this.ActionRoot, this);
            foreach (DNNNode objNode in objNodes)
            {
			Logger.LogDebug("BindDropDown(): objNodes → {ObjNodes}", objNodes);
                this.ProcessNodes(objNode);
            }

            this.Control.Bind(objNodes);

            this.Visible = this.DisplayControl(objNodes);
			Logger.LogDebug("<<< Exiting BindDropDown()");
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdGo.Click += this.CmdGo_Click;

            try
            {
                this.BindDropDown();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.Control = NavigationProvider.Instance(this.serviceProvider, this.ProviderName);
            this.Control.ControlID = "ctl" + this.ID;
            this.Control.Initialize();
            this.spActions.Controls.Add(this.Control.NavigationControl);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void Page_Load(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering Page_Load(sender={Sender},e={E})", sender, e);
            this.cmdGo.Attributes.Add("onclick", "if (cmdGo_OnClick(dnn.dom.getById('" + this.Control.NavigationControl.ClientID + "')) == false) return false;");
			Logger.LogDebug("<<< Exiting Page_Load(sender={Sender},e={E})", sender, e);
        }

        private void CmdGo_Click(object sender, ImageClickEventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdGo_Click(sender={Sender},e={E})", sender, e);
            try
            {
                var cboActions = (DropDownList)this.Control.NavigationControl;
				Logger.LogDebug("CmdGo_Click(sender={Sender},e={E}): cboActions → {CboActions}", sender, e, cboActions);
                if (cboActions.SelectedIndex != -1)
                {
                    this.ProcessAction(cboActions.SelectedItem.Value);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdGo_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.getMessage());
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdGo_Click(sender={Sender},e={E})", sender, e);
        }

        private void ProcessNodes(DNNNode objParent)
        {
			Logger.LogDebug(">>> Entering ProcessNodes(objParent={ObjParent})", objParent);
            if (!string.IsNullOrEmpty(objParent.JSFunction))
            {
                ClientAPI.RegisterClientVariable(this.Page, "__dnn_CSAction_" + this.Control.NavigationControl.ClientID + "_" + objParent.ID, objParent.JSFunction, true);
            }

            objParent.ClickAction = eClickAction.None; // since GO button is handling actions don't allow selected index change fire postback
			Logger.LogDebug("ProcessNodes(objParent={ObjParent}): objParent.ClickAction → {Arg1}", objParent, objParent.ClickAction);

            foreach (DNNNode objNode in objParent.DNNNodes)
            {
                this.ProcessNodes(objNode);
            }
			Logger.LogDebug("<<< Exiting ProcessNodes(objParent={ObjParent})", objParent);
        }
    }
}
