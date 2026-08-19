// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>The DNNMultiStateBoxColumn control provides a DNNMultiState Box column for a Data Grid.</summary>
    public class DNNMultiStateBoxColumn : TemplateColumn
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNMultiStateBoxColumn));
        private DNNMultiStateCollection mStates;

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNMultiStateBoxColumn"/> class.
        /// Constructs the DNNMultiStateBoxColumn.
        /// </summary>
        public DNNMultiStateBoxColumn()
            : this(false)
        {
		Logger.LogDebug(">>> Entering DNNMultiStateBoxColumn()");
		Logger.LogDebug("<<< Exiting DNNMultiStateBoxColumn()");
		Logger.LogDebug(">>> Entering DNNMultiStateBoxColumn(autoPostBack={AutoPostBack})", autoPostBack);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DNNMultiStateBoxColumn"/> class.
        /// Constructs the MultiStateBoxColumn, with an optional AutoPostBack (where each change
        /// of state of the control causes a Post Back).
        /// </summary>
        /// <param name="autoPostBack">Optional set the control to postback.</param>
        public DNNMultiStateBoxColumn(bool autoPostBack)
        {
            this.AutoPostBack = autoPostBack;
			Logger.LogDebug("<<< Exiting DNNMultiStateBoxColumn(autoPostBack={AutoPostBack})", autoPostBack);
        }

        /// <summary>Gets or sets a value indicating whether the column fires a postback when the control changes.</summary>
        /// <value>A Boolean.</value>
        public bool AutoPostBack { get; set; } = true;

        /// <summary>Gets or sets the selected state of the DNNMultiStateBox (unless DataBound).</summary>
        /// <value>A Boolean.</value>
        public string SelectedStateKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Data Field that the column should bind to.
        /// </summary>
        /// <value>A Boolean.</value>
        public string DataField { get; set; } = Null.NullString;

        /// <summary>
        /// Gets or sets a value indicating whether the control is enabled (this is overridden if
        /// the EnabledField is set).
        /// </summary>
        /// <value>A Boolean.</value>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the Data Field that determines whether the control is Enabled
        /// changed.
        /// </summary>
        /// <value>A String.</value>
        public string EnabledField { get; set; } = Null.NullString;

        /// <summary>Gets or sets the image path of the DNNMultiStateBox.</summary>
        /// <value>A Boolean.</value>
        public string ImagePath { get; set; } = string.Empty;

        /// <summary>Gets or sets the state collection of the DNNMultiStateBox.</summary>
        /// <value>A Boolean.</value>
        public DNNMultiStateCollection States
        {
            get
            {
                if (this.mStates == null)
                {
                    this.mStates = new DNNMultiStateCollection(new DNNMultiStateBox());
                }

                return this.mStates;
            }

            set
            {
                this.mStates = value;
            }
        }

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

        /// <summary>Creates a DNNMultiStateBoxColumnTemplate.</summary>
        /// <returns>A DNNMultiStateBoxColumnTemplate.</returns>
        private DNNMultiStateBoxColumnTemplate CreateTemplate(ListItemType type)
        {
			Logger.LogDebug(">>> Entering CreateTemplate(type={Type})", type);
            bool isDesignMode = false;
            if (HttpContext.Current == null)
			Logger.LogDebug("CreateTemplate(type={Type}): isDesignMode → {IsDesignMode}", type, isDesignMode);
            {
                isDesignMode = true;
            }

            var template = new DNNMultiStateBoxColumnTemplate(type);
			Logger.LogDebug("CreateTemplate(type={Type}): template → {Template}", type, template);
            if (type != ListItemType.Header)
            {
                template.AutoPostBack = this.AutoPostBack;
				Logger.LogDebug("CreateTemplate(type={Type}): template.AutoPostBack → {Arg1}", type, template.AutoPostBack);
            }

            template.DataField = this.DataField;
			Logger.LogDebug("CreateTemplate(type={Type}): template.DataField → {Arg1}", type, template.DataField);
			Logger.LogDebug("CreateTemplate(type={Type}): template.Enabled → {Arg1}", type, template.Enabled);
			Logger.LogDebug("CreateTemplate(type={Type}): template.EnabledField → {Arg1}", type, template.EnabledField);
			Logger.LogDebug("CreateTemplate(type={Type}): template.ImagePath → {Arg1}", type, template.ImagePath);
            template.Enabled = this.Enabled;
            template.EnabledField = this.EnabledField;
            template.ImagePath = this.ImagePath;
            foreach (DNNMultiState objState in this.States)
            {
                template.States.Add(objState);
            }

            template.SelectedStateKey = this.SelectedStateKey;
			Logger.LogDebug("CreateTemplate(type={Type}): template.SelectedStateKey → {Arg1}", type, template.SelectedStateKey);
            if (type == ListItemType.Header)
            {
                template.Text = this.HeaderText;
				Logger.LogDebug("CreateTemplate(type={Type}): template.Text → {Arg1}", type, template.Text);
                template.AutoPostBack = true;
            }

            template.DesignMode = isDesignMode;
			Logger.LogDebug("CreateTemplate(type={Type}): template.DesignMode → {Arg1}", type, template.DesignMode);
			Logger.LogDebug("<<< Exiting CreateTemplate(type={Type})", type);
            return template;
        }
    }
}
