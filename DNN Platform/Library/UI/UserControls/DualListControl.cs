// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    public abstract class DualListControl : UserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected Label Label1;

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected Label Label2;

        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdAdd;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdAddAll;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdRemove;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdRemoveAll;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected ListBox lstAssigned;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected ListBox lstAvailable;

        private string myFileName = "DualListControl.ascx";
        private ArrayList assigned;
        private ArrayList available;
        private string dataTextField = string.Empty;
        private string dataValueField = string.Empty;
        private bool enabled = true;
        private string listBoxHeight = string.Empty;
        private string listBoxWidth = string.Empty;

        public string ListBoxWidth
        {
            get
            {
                return Convert.ToString(this.ViewState[this.ClientID + "_ListBoxWidth"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.listBoxWidth = value;
            }
        }

        public string ListBoxHeight
        {
            get
            {
                return Convert.ToString(this.ViewState[this.ClientID + "_ListBoxHeight"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.listBoxHeight = value;
            }
        }

        public ArrayList Available
        {
            get
            {
                var objList = new ArrayList();
                foreach (ListItem objListItem in this.lstAvailable.Items)
                {
                    objList.Add(objListItem);
                }

                return objList;
            }

            set
            {
                this.available = value;
            }
        }

        public ArrayList Assigned
        {
            get
            {
                var objList = new ArrayList();
                foreach (ListItem objListItem in this.lstAssigned.Items)
                {
                    objList.Add(objListItem);
                }

                return objList;
            }

            set
            {
                this.assigned = value;
            }
        }

        public string DataTextField
        {
            set
            {
                this.dataTextField = value;
            }
        }

        public string DataValueField
        {
            set
            {
                this.dataValueField = value;
            }
        }

        public bool Enabled
        {
            set
            {
                this.enabled = value;
            }
        }

        /// <summary>The Page_Load server event handler on this page is used to populate the role information for the page.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdAdd.Click += this.CmdAdd_Click;
            this.cmdAddAll.Click += this.CmdAddAll_Click;
            this.cmdRemove.Click += this.CmdRemove_Click;
            this.cmdRemoveAll.Click += this.CmdRemoveAll_Click;

            try
            {
                // Localization
                this.Label1.Text = Localization.GetString("Available", Localization.GetResourceFile(this, this.myFileName));
                this.Label2.Text = Localization.GetString("Assigned", Localization.GetResourceFile(this, this.myFileName));
                this.cmdAdd.ToolTip = Localization.GetString("Add", Localization.GetResourceFile(this, this.myFileName));
                this.cmdAddAll.ToolTip = Localization.GetString("AddAll", Localization.GetResourceFile(this, this.myFileName));
                this.cmdRemove.ToolTip = Localization.GetString("Remove", Localization.GetResourceFile(this, this.myFileName));
                this.cmdRemoveAll.ToolTip = Localization.GetString("RemoveAll", Localization.GetResourceFile(this, this.myFileName));

                if (!this.Page.IsPostBack)
                {
                    // set dimensions of control
                    if (!string.IsNullOrEmpty(this.ListBoxWidth))
                    {
                        this.lstAvailable.Width = Unit.Parse(this.ListBoxWidth, CultureInfo.InvariantCulture);
                        this.lstAssigned.Width = Unit.Parse(this.ListBoxWidth, CultureInfo.InvariantCulture);
                    }

                    if (!string.IsNullOrEmpty(this.ListBoxHeight))
                    {
                        this.lstAvailable.Height = Unit.Parse(this.ListBoxHeight, CultureInfo.InvariantCulture);
                        this.lstAssigned.Height = Unit.Parse(this.ListBoxHeight, CultureInfo.InvariantCulture);
                    }

                    // load available
                    this.lstAvailable.DataTextField = this.dataTextField;
                    this.lstAvailable.DataValueField = this.dataValueField;
                    this.lstAvailable.DataSource = this.Available;
                    this.lstAvailable.DataBind();
                    Sort(this.lstAvailable);

                    // load selected
                    this.lstAssigned.DataTextField = this.dataTextField;
                    this.lstAssigned.DataValueField = this.dataValueField;
                    this.lstAssigned.DataSource = this.Assigned;
                    this.lstAssigned.DataBind();
                    Sort(this.lstAssigned);

                    // set enabled
                    this.lstAvailable.Enabled = this.enabled;
                    this.lstAssigned.Enabled = this.enabled;

                    // save persistent values
                    this.ViewState[this.ClientID + "_ListBoxWidth"] = this.ListBoxWidth;
                    this.ViewState[this.ClientID + "_ListBoxHeight"] = this.ListBoxHeight;
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        private static void Sort(ListBox ctlListBox)
        {
			Logger.LogDebug(">>> Entering Sort(ctlListBox={CtlListBox})", ctlListBox);
            var arrListItems = new ArrayList();
            foreach (ListItem objListItem in ctlListBox.Items)
            {
			Logger.LogDebug("Sort(ctlListBox={CtlListBox}): arrListItems → {ArrListItems}", ctlListBox, arrListItems);
                arrListItems.Add(objListItem);
            }

            arrListItems.Sort(new ListItemComparer());
            ctlListBox.Items.Clear();
            foreach (ListItem objListItem in arrListItems)
            {
                ctlListBox.Items.Add(objListItem);
            }
			Logger.LogDebug("<<< Exiting Sort(ctlListBox={CtlListBox})", ctlListBox);
        }

        private void CmdAdd_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdAdd_Click(sender={Sender},e={E})", sender, e);
            var objList = new ArrayList();
            foreach (ListItem objListItem in this.lstAvailable.Items)
            {
			Logger.LogDebug("CmdAdd_Click(sender={Sender},e={E}): objList → {ObjList}", sender, e, objList);
                objList.Add(objListItem);
            }

            foreach (ListItem objListItem in objList)
            {
                if (objListItem.Selected)
                {
                    this.lstAvailable.Items.Remove(objListItem);
                    this.lstAssigned.Items.Add(objListItem);
                }
            }

            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            Sort(this.lstAssigned);
			Logger.LogDebug("<<< Exiting CmdAdd_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdRemove_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdRemove_Click(sender={Sender},e={E})", sender, e);
            var objList = new ArrayList();
            foreach (ListItem objListItem in this.lstAssigned.Items)
            {
			Logger.LogDebug("CmdRemove_Click(sender={Sender},e={E}): objList → {ObjList}", sender, e, objList);
                objList.Add(objListItem);
            }

            foreach (ListItem objListItem in objList)
            {
                if (objListItem.Selected)
                {
                    this.lstAssigned.Items.Remove(objListItem);
                    this.lstAvailable.Items.Add(objListItem);
                }
            }

            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            Sort(this.lstAvailable);
			Logger.LogDebug("<<< Exiting CmdRemove_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdAddAll_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdAddAll_Click(sender={Sender},e={E})", sender, e);
            foreach (ListItem objListItem in this.lstAvailable.Items)
            {
                this.lstAssigned.Items.Add(objListItem);
            }

            this.lstAvailable.Items.Clear();
            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            Sort(this.lstAssigned);
			Logger.LogDebug("<<< Exiting CmdAddAll_Click(sender={Sender},e={E})", sender, e);
        }

        private void CmdRemoveAll_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdRemoveAll_Click(sender={Sender},e={E})", sender, e);
            foreach (ListItem objListItem in this.lstAssigned.Items)
            {
                this.lstAvailable.Items.Add(objListItem);
            }

            this.lstAssigned.Items.Clear();
            this.lstAvailable.ClearSelection();
            this.lstAssigned.ClearSelection();
            Sort(this.lstAvailable);
			Logger.LogDebug("<<< Exiting CmdRemoveAll_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
