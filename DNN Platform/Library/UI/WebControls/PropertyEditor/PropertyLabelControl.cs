// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// The PropertyLabelControl control provides a standard UI component for displaying
    /// a label for a property. It contains a Label and Help Text and can be Data Bound.
    /// </summary>
    [ToolboxData("<{0}:PropertyLabelControl runat=server></{0}:PropertyLabelControl>")]
    public class PropertyLabelControl : WebControl
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdHelp;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected HtmlGenericControl label;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblHelp;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblLabel;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel pnlTooltip;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel pnlHelp;

        private readonly IClientResourceController clientResourceController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        private string resourceKey;

        /// <summary>Initializes a new instance of the <see cref="PropertyLabelControl"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public PropertyLabelControl()
            : this(null, null, null)
        {
		eventLogger.LogDebug(">>> Entering PropertyLabelControl()");
        }
		eventLogger.LogDebug("<<< Exiting PropertyLabelControl()");

        public PropertyLabelControl(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
			eventLogger.LogDebug("PropertyLabelControl(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger}): appStatus → {AppStatus1}", clientResourceController, appStatus, eventLogger, appStatus);
			eventLogger.LogDebug("PropertyLabelControl(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger}): clientResourceController → {ClientResourceController1}", clientResourceController, appStatus, eventLogger, clientResourceController);
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			eventLogger.LogDebug("PropertyLabelControl(clientResourceController={ClientResourceController},appStatus={AppStatus},eventLogger={EventLogger}): eventLogger → {EventLogger1}", clientResourceController, appStatus, eventLogger, eventLogger);
        }

        /// <summary>Gets and sets the value of the Label Style.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Help Text.")]
        public Style HelpStyle
        {
            get
            {
                this.EnsureChildControls();
                return this.pnlHelp.ControlStyle;
            }
        }

        /// <summary>Gets and sets the value of the Label Style.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Label Text")]
        public Style LabelStyle
        {
            get
            {
                this.EnsureChildControls();
                return this.lblLabel.ControlStyle;
            }
        }

        /// <summary>Gets or sets the Caption Text if no ResourceKey is provided.</summary>
        /// <value>A string representing the Caption.</value>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("Property")]
        [Description("Enter Caption for the control.")]
        public string Caption
        {
            get
            {
                this.EnsureChildControls();
                return this.lblLabel.Text;
            }

            set
            {
                this.EnsureChildControls();
                this.lblLabel.Text = value;
            }
        }

        public string AssociatedControlId
        {
            get
            {
                this.EnsureChildControls();
                return this.lblLabel.AssociatedControlID;
            }

            set
            {
                this.EnsureChildControls();
                this.lblLabel.AssociatedControlID = value;
            }
        }

        /// <summary>Gets or sets the related Edit Control.</summary>
        /// <value>A Control.</value>
        [Browsable(false)]
        public Control EditControl { get; set; }

        /// <summary>Gets or sets text is value of the Label Text if no ResourceKey is provided.</summary>
        /// <value>A string representing the Text.</value>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("Enter Help Text for the control.")]
        public string HelpText
        {
            get
            {
                this.EnsureChildControls();
                return this.lblHelp.Text;
            }

            set
            {
                this.EnsureChildControls();
                this.lblHelp.Text = value;
            }
        }

        /// <summary>Gets or sets resourceKey is the root localization key for this control.</summary>
        /// <value>A string representing the Resource Key.</value>
        /// <remarks>This control will "standardize" the resource key names, so for instance
        /// if the resource key is "Control", Control.Text is the label text key, Control.Help
        /// is the label help text, Control.ErrorMessage is the Validation Error Message for the
        /// control.
        /// </remarks>
        [Browsable(true)]
        [Category("Localization")]
        [DefaultValue("")]
        [Description("Enter the Resource key for the control.")]
        public string ResourceKey
        {
            get
            {
                return this.resourceKey;
            }

            set
            {
                this.resourceKey = value;

                this.EnsureChildControls();

                // Localize the Label and the Help text
                this.lblHelp.Attributes["resourcekey"] = this.resourceKey + ".Help";
                this.lblLabel.Attributes["resourcekey"] = this.resourceKey + ".Text";
            }
        }

        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Set whether the Help icon is displayed.")]
        public bool ShowHelp
        {
            get
            {
                this.EnsureChildControls();
                return this.cmdHelp.Visible;
            }

            set
            {
                this.EnsureChildControls();
                this.cmdHelp.Visible = value;
            }
        }

        /// <summary>Gets or sets the value of the Field that is bound to the Label.</summary>
        /// <value>A string representing the Name of the Field.</value>
        [Browsable(true)]
        [Category("Data")]
        [DefaultValue("")]
        [Description("Enter the name of the field that is data bound to the Label's Text property.")]
        public string DataField { get; set; }

        /// <summary>Gets or sets the DataSource that is bound to this control.</summary>
        /// <value>The DataSource object.</value>
        [Browsable(false)]
        public object DataSource { get; set; }

        public bool Required { get; set; }

        /// <inheritdoc />
        protected override HtmlTextWriterTag TagKey => HtmlTextWriterTag.Div;

        /// <summary>CreateChildControls creates the control collection.</summary>
        protected override void CreateChildControls()
        {
			eventLogger.LogDebug(">>> Entering CreateChildControls()");
            this.CssClass += "dnnLabel";

            this.label = new HtmlGenericControl { TagName = "label" };
			eventLogger.LogDebug("CreateChildControls(): label → {Label}", label);
			eventLogger.LogDebug("CreateChildControls(): this.label → {Arg0}", this.label);

            if (!this.DesignMode)
            {
                this.cmdHelp = new LinkButton { ID = this.ID + "_cmdHelp", CssClass = "dnnFormHelp", CausesValidation = false, EnableViewState = false, TabIndex = -1 };
				eventLogger.LogDebug("CreateChildControls(): cmdHelp → {CmdHelp}", cmdHelp);
				eventLogger.LogDebug("CreateChildControls(): this.cmdHelp → {Arg0}", this.cmdHelp);
                this.cmdHelp.Attributes.Add("aria-label", "Help");
                this.lblLabel = new Label { ID = this.ID + "_label", EnableViewState = false };
				eventLogger.LogDebug("CreateChildControls(): lblLabel → {LblLabel}", lblLabel);
				eventLogger.LogDebug("CreateChildControls(): this.lblLabel → {Arg0}", this.lblLabel);

                this.label.Controls.Add(this.lblLabel);

                this.Controls.Add(this.label);
                this.Controls.Add(this.cmdHelp);
            }

            this.pnlTooltip = new Panel { CssClass = "dnnTooltip" };
			eventLogger.LogDebug("CreateChildControls(): pnlTooltip → {PnlTooltip}", pnlTooltip);
			eventLogger.LogDebug("CreateChildControls(): this.pnlTooltip → {Arg0}", this.pnlTooltip);

            this.pnlHelp = new Panel { ID = this.ID + "_pnlHelp", EnableViewState = false, CssClass = "dnnFormHelpContent dnnClear" };
			eventLogger.LogDebug("CreateChildControls(): pnlHelp → {PnlHelp}", pnlHelp);
			eventLogger.LogDebug("CreateChildControls(): this.pnlHelp → {Arg0}", this.pnlHelp);

            this.pnlTooltip.Controls.Add(this.pnlHelp);

            this.lblHelp = new Label { ID = this.ID + "_lblHelp", EnableViewState = false };
			eventLogger.LogDebug("CreateChildControls(): lblHelp → {LblHelp}", lblHelp);
			eventLogger.LogDebug("CreateChildControls(): this.lblHelp → {Arg0}", this.lblHelp);
            this.pnlHelp.Controls.Add(this.lblHelp);

            var aHelpPin = new HyperLink();
            aHelpPin.CssClass = "pinHelp";
			eventLogger.LogDebug("CreateChildControls(): aHelpPin → {AHelpPin}", aHelpPin);
			eventLogger.LogDebug("CreateChildControls(): aHelpPin.CssClass → {Arg0}", aHelpPin.CssClass);
            aHelpPin.Attributes.Add("href", "#");
            aHelpPin.Attributes.Add("aria-label", "Pin");
            this.pnlHelp.Controls.Add(aHelpPin);

            ////Controls.Add(label);
            this.Controls.Add(this.pnlTooltip);
			eventLogger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <summary>
        /// OnDataBinding runs when the Control is being Data Bound (It is triggered by
        /// a call to Control.DataBind().
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDataBinding(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnDataBinding(e={E})", e);
            // If there is a DataSource bind the relevant Properties
            if (this.DataSource != null)
            {
                this.EnsureChildControls();
                if (!string.IsNullOrEmpty(this.DataField))
                {
                    // DataBind the Label (via the Resource Key)
                    var dataRow = (DataRowView)this.DataSource;
                    if (this.ResourceKey == string.Empty)
					eventLogger.LogDebug("OnDataBinding(e={E}): dataRow → {DataRow}", e, dataRow);
                    {
                        this.ResourceKey = Convert.ToString(dataRow[this.DataField], CultureInfo.InvariantCulture);
                    }

                    if (this.DesignMode)
                    {
                        this.label.InnerText = Convert.ToString(dataRow[this.DataField], CultureInfo.InvariantCulture);
                    }
                }
            }
			eventLogger.LogDebug("<<< Exiting OnDataBinding(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.DnnPlugins);
            this.clientResourceController.RegisterScript("~/Resources/Shared/Scripts/initTooltips.js");
			eventLogger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>
        /// OnLoad runs just before the Control is rendered, and makes sure that any
        /// properties are set properly before the control is rendered.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			eventLogger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            // Make sure the Child Controls are created before assigning any properties
            this.EnsureChildControls();

            if (this.Required)
            {
                this.lblLabel.CssClass += " dnnFormRequired";
            }

            ////DNNClientAPI.EnableMinMax(cmdHelp, pnlHelp, true, DNNClientAPI.MinMaxPersistanceType.None);
            if (this.EditControl != null)
            {
                this.label.Attributes.Add("for", this.EditControl is EditControl editControl ? editControl.EditControlClientId : this.EditControl.ClientID);
            }

            // make sure the help container have the default css class to active js handler.
            if (!this.pnlHelp.ControlStyle.CssClass.Contains("dnnClear"))
            {
                this.pnlHelp.ControlStyle.CssClass = $"dnnClear {this.pnlHelp.ControlStyle.CssClass}";
            }

            if (!this.pnlHelp.ControlStyle.CssClass.Contains("dnnFormHelpContent"))
            {
                this.pnlHelp.ControlStyle.CssClass = $"dnnFormHelpContent {this.pnlHelp.ControlStyle.CssClass}";
            }
			eventLogger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }
    }
}
