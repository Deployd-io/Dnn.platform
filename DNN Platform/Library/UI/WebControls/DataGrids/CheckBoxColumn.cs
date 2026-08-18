// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>The CheckBoxColumn control provides a Check Box column for a Data Grid.</summary>
    public class CheckBoxColumn : TemplateColumn
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CheckBoxColumn));
        private bool mAutoPostBack = true;
        private string mDataField = Null.NullString;
        private bool mEnabled = true;
        private string mEnabledField = Null.NullString;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxColumn"/> class.
        /// Constructs the CheckBoxColumn.
        /// </summary>
        public CheckBoxColumn()
            : this(false)
        {
		Logger.LogDebug(">>> Entering CheckBoxColumn()");
		Logger.LogDebug("<<< Exiting CheckBoxColumn()");
		Logger.LogDebug(">>> Entering CheckBoxColumn(autoPostBack={AutoPostBack})", autoPostBack);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxColumn"/> class.
        /// Constructs the CheckBoxColumn, with an optional AutoPostBack (where each change
        /// of state of a check box causes a Post Back).
        /// </summary>
        /// <param name="autoPostBack">Optional set the checkboxes to postback.</param>
        public CheckBoxColumn(bool autoPostBack)
        {
            this.AutoPostBack = autoPostBack;
			Logger.LogDebug("<<< Exiting CheckBoxColumn(autoPostBack={AutoPostBack})", autoPostBack);
        }

        public event DNNDataGridCheckedColumnEventHandler CheckedChanged;

        /// <summary>
        /// Gets or sets a value indicating whether the column fires a postback when any check box is
        /// changed.
        /// </summary>
        /// <value>A Boolean.</value>
        public bool AutoPostBack
        {
            get
            {
                return this.mAutoPostBack;
            }

            set
            {
                this.mAutoPostBack = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the checkbox is checked (unless DataBound).</summary>
        /// <value>A Boolean.</value>
        public bool Checked { get; set; }

        /// <summary>
        /// Gets or sets the Data Field that the column should bind to
        /// changed.
        /// </summary>
        /// <value>A Boolean.</value>
        public string DataField
        {
            get
            {
                return this.mDataField;
            }

            set
            {
                this.mDataField = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the checkboxes are enabled (this is overridden if
        /// the EnabledField is set).
        /// </summary>
        /// <value>A Boolean.</value>
        public bool Enabled
        {
            get
            {
                return this.mEnabled;
            }

            set
            {
                this.mEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the Data Field that determines whether the checkbox is Enabled
        /// changed.
        /// </summary>
        /// <value>A String.</value>
        public string EnabledField
        {
            get
            {
                return this.mEnabledField;
            }

            set
            {
                this.mEnabledField = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether there is a checkbox in the Header that sets all
        /// the checkboxes.
        /// </summary>
        /// <value>A Boolean.</value>
        public bool HeaderCheckBox { get; set; } = true;

        /// <summary>Initialises the Column.</summary>
        public override void Initialize()
        {
			Logger.LogDebug(">>> Entering Initialize()");
            this.ItemTemplate = this.CreateTemplate(ListItemType.Item);
            this.EditItemTemplate = this.CreateTemplate(ListItemType.EditItem);
            this.HeaderTemplate = this.CreateTemplate(ListItemType.Header);
            if (HttpContext.Current == null)
            {
                this.HeaderStyle.Font.Names = new[] { "Tahoma, Verdana, Arial" };
                this.HeaderStyle.Font.Size = new FontUnit("10pt");
                this.HeaderStyle.Font.Bold = true;
            }

            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
			Logger.LogDebug("<<< Exiting Initialize()");
        }

        /// <summary>Creates a CheckBoxColumnTemplate.</summary>
        /// <returns>A CheckBoxColumnTemplate.</returns>
        private CheckBoxColumnTemplate CreateTemplate(ListItemType type)
        {
			Logger.LogDebug(">>> Entering CreateTemplate(type={Type})", type);
            bool isDesignMode = false;
            if (HttpContext.Current == null)
			Logger.LogDebug("CreateTemplate(type={Type}): isDesignMode → {IsDesignMode}", type, isDesignMode);
            {
                isDesignMode = true;
            }

            var template = new CheckBoxColumnTemplate(type);
			Logger.LogDebug("CreateTemplate(type={Type}): template → {Template}", type, template);
            if (type != ListItemType.Header)
            {
                template.AutoPostBack = this.AutoPostBack;
				Logger.LogDebug("CreateTemplate(type={Type}): template.AutoPostBack → {Arg1}", type, template.AutoPostBack);
            }

            template.Checked = this.Checked;
			Logger.LogDebug("CreateTemplate(type={Type}): template.Checked → {Arg1}", type, template.Checked);
			Logger.LogDebug("CreateTemplate(type={Type}): template.DataField → {Arg1}", type, template.DataField);
			Logger.LogDebug("CreateTemplate(type={Type}): template.Enabled → {Arg1}", type, template.Enabled);
			Logger.LogDebug("CreateTemplate(type={Type}): template.EnabledField → {Arg1}", type, template.EnabledField);
            template.DataField = this.DataField;
            template.Enabled = this.Enabled;
            template.EnabledField = this.EnabledField;
            template.CheckedChanged += this.OnCheckedChanged;
            if (type == ListItemType.Header)
            {
                template.Text = this.HeaderText;
				Logger.LogDebug("CreateTemplate(type={Type}): template.Text → {Arg1}", type, template.Text);
                template.AutoPostBack = true;
                template.HeaderCheckBox = this.HeaderCheckBox;
				Logger.LogDebug("CreateTemplate(type={Type}): template.HeaderCheckBox → {Arg1}", type, template.HeaderCheckBox);
            }

            template.DesignMode = isDesignMode;
			Logger.LogDebug("CreateTemplate(type={Type}): template.DesignMode → {Arg1}", type, template.DesignMode);
			Logger.LogDebug("<<< Exiting CreateTemplate(type={Type})", type);
            return template;
        }

        /// <summary>Centralised Event that is raised whenever a check box is changed.</summary>
        private void OnCheckedChanged(object sender, DNNDataGridCheckChangedEventArgs e)
        {
			Logger.LogDebug(">>> Entering OnCheckedChanged(sender={Sender},e={E})", sender, e);
            // Add the column to the Event Args
            e.Column = this;
			Logger.LogDebug("OnCheckedChanged(sender={Sender},e={E}): e.Column → {Arg2}", sender, e, e.Column);
            if (this.CheckedChanged != null)
            {
                this.CheckedChanged(sender, e);
            }
			Logger.LogDebug("<<< Exiting OnCheckedChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
