// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls

// ReSharper restore CheckNamespace
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;

    public class RolesSelectionGrid : Control, INamingContainer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RolesSelectionGrid));
        private static readonly string[] RoleSeparator = ["##",];
        private readonly DataTable dtRoleSelections = new DataTable();
        private IList<RoleInfo> roles;
        private IList<string> selectedRoles;
        private DropDownList cboRoleGroups;
        private DataGrid dgRoleSelection;
        private Label lblGroups;
        private Panel pnlRoleSlections;

        public TableItemStyle AlternatingItemStyle
        {
            get
            {
                return this.dgRoleSelection.AlternatingItemStyle;
            }
        }

        public DataGridColumnCollection Columns
        {
            get
            {
                return this.dgRoleSelection.Columns;
            }
        }

        public TableItemStyle FooterStyle
        {
            get
            {
                return this.dgRoleSelection.FooterStyle;
            }
        }

        public TableItemStyle HeaderStyle
        {
            get
            {
                return this.dgRoleSelection.HeaderStyle;
            }
        }

        public TableItemStyle ItemStyle
        {
            get
            {
                return this.dgRoleSelection.ItemStyle;
            }
        }

        public DataGridItemCollection Items
        {
            get
            {
                return this.dgRoleSelection.Items;
            }
        }

        public TableItemStyle SelectedItemStyle
        {
            get
            {
                return this.dgRoleSelection.SelectedItemStyle;
            }
        }

        public bool AutoGenerateColumns
        {
            get
            {
                return this.dgRoleSelection.AutoGenerateColumns;
            }

            set
            {
                this.dgRoleSelection.AutoGenerateColumns = value;
            }
        }

        public int CellSpacing
        {
            get
            {
                return this.dgRoleSelection.CellSpacing;
            }

            set
            {
                this.dgRoleSelection.CellSpacing = value;
            }
        }

        public GridLines GridLines
        {
            get
            {
                return this.dgRoleSelection.GridLines;
            }

            set
            {
                this.dgRoleSelection.GridLines = value;
            }
        }

        /// <summary>Gets or sets list of the Names of the selected Roles.</summary>
        public ArrayList SelectedRoleNames
        {
            get
            {
                this.UpdateRoleSelections();
                return new ArrayList(this.CurrentRoleSelection.ToArray());
            }

            set
            {
                this.CurrentRoleSelection = value.Cast<string>().ToList();
            }
        }

        /// <summary>Gets or sets the ResourceFile to localize permissions.</summary>
        public string ResourceFile { get; set; }

        /// <summary>Gets or sets a value indicating whether enable ShowAllUsers to display the virtual "Unauthenticated Users" role.</summary>
        public bool ShowUnauthenticatedUsers
        {
            get
            {
                if (this.ViewState["ShowUnauthenticatedUsers"] == null)
                {
                    return false;
                }

                return Convert.ToBoolean(this.ViewState["ShowUnauthenticatedUsers"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.ViewState["ShowUnauthenticatedUsers"] = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether enable ShowAllUsers to display the virtual "All Users" role.</summary>
        public bool ShowAllUsers
        {
            get
            {
                if (this.ViewState["ShowAllUsers"] == null)
                {
                    return false;
                }

                return Convert.ToBoolean(this.ViewState["ShowAllUsers"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.ViewState["ShowAllUsers"] = value;
            }
        }

        private DataTable DtRolesSelection
        {
            get
            {
                return this.dtRoleSelections;
            }
        }

        private IList<string> CurrentRoleSelection
        {
            get
            {
                return this.selectedRoles ?? (this.selectedRoles = new List<string>());
            }

            set
            {
                this.selectedRoles = value;
            }
        }

        /// <summary>Load the ViewState.</summary>
        /// <param name="savedState">The saved state.</param>
        protected override void LoadViewState(object savedState)
        {
			Logger.LogError(">>> Entering LoadViewState(savedState={SavedState})", savedState);
            if (savedState != null)
            {
                // Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;
				Logger.LogError("LoadViewState(savedState={SavedState}): myState → {MyState}", savedState, myState);

                // Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load TabPermissions
                if (myState[1] != null)
                {
                    string state = Convert.ToString(myState[1], CultureInfo.InvariantCulture);
                    this.CurrentRoleSelection = state != string.Empty
                                        ? new List<string>(state.Split(RoleSeparator, StringSplitOptions.None))
                                        : new List<string>();
                }
            }
			Logger.LogWarning("<<< Exiting LoadViewState(savedState={SavedState})", savedState);
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
			Logger.LogError(">>> Entering SaveViewState()");
            var allStates = new object[2];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Persist the TabPermisisons
            var sb = new StringBuilder();
			Logger.LogError("SaveViewState(): sb → {Sb}", sb);
            bool addDelimiter = false;
			Logger.LogError("SaveViewState(): addDelimiter → {AddDelimiter}", addDelimiter);
            foreach (string role in this.CurrentRoleSelection)
            {
                if (addDelimiter)
                {
                    sb.Append("##");
                }
                else
                {
                    addDelimiter = true;
                }

                sb.Append(role);
            }

            allStates[1] = sb.ToString();
			Logger.LogWarning("<<< Exiting SaveViewState()");
            return allStates;
        }

        /// <summary>Creates the Child Controls.</summary>
        protected override void CreateChildControls()
        {
			Logger.LogError(">>> Entering CreateChildControls()");
            this.pnlRoleSlections = new Panel { CssClass = "dnnRolesGrid" };

			Logger.LogDebug("CreateChildControls(): this.pnlRoleSlections → {Arg0}", this.pnlRoleSlections);
			Logger.LogDebug("CreateChildControls(): pnlRoleSlections → {PnlRoleSlections}", pnlRoleSlections);
            // Optionally Add Role Group Filter
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			Logger.LogError("CreateChildControls(): portalSettings → {PortalSettings}", portalSettings);
            ArrayList arrGroups = RoleController.GetRoleGroups(portalSettings.PortalId);
			Logger.LogError("CreateChildControls(): arrGroups → {ArrGroups}", arrGroups);
            if (arrGroups.Count > 0)
            {
                this.lblGroups = new Label { Text = Localization.GetString("RoleGroupFilter") };
				Logger.LogDebug("CreateChildControls(): lblGroups → {LblGroups}", lblGroups);
                this.pnlRoleSlections.Controls.Add(this.lblGroups);

                this.pnlRoleSlections.Controls.Add(new LiteralControl("&nbsp;&nbsp;"));

                this.cboRoleGroups = new DropDownList { AutoPostBack = true };
				Logger.LogDebug("CreateChildControls(): cboRoleGroups → {CboRoleGroups}", cboRoleGroups);
				Logger.LogDebug("CreateChildControls(): this.cboRoleGroups → {Arg0}", this.cboRoleGroups);

                this.cboRoleGroups.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
                var liItem = new ListItem(Localization.GetString("GlobalRoles"), "-1") { Selected = true };
				Logger.LogError("CreateChildControls(): liItem → {LiItem}", liItem);
                this.cboRoleGroups.Items.Add(liItem);
                foreach (RoleGroupInfo roleGroup in arrGroups)
                {
                    this.cboRoleGroups.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString(CultureInfo.InvariantCulture)));
                }

                this.pnlRoleSlections.Controls.Add(this.cboRoleGroups);

                this.pnlRoleSlections.Controls.Add(new LiteralControl("<br/><br/>"));
            }

            this.dgRoleSelection = new DataGrid { AutoGenerateColumns = false, CellSpacing = 0, GridLines = GridLines.None };
			Logger.LogDebug("CreateChildControls(): dgRoleSelection → {DgRoleSelection}", dgRoleSelection);
			Logger.LogDebug("CreateChildControls(): this.dgRoleSelection → {Arg0}", this.dgRoleSelection);
            this.dgRoleSelection.FooterStyle.CssClass = "dnnGridFooter";
            this.dgRoleSelection.HeaderStyle.CssClass = "dnnGridHeader";
            this.dgRoleSelection.ItemStyle.CssClass = "dnnGridItem";
            this.dgRoleSelection.AlternatingItemStyle.CssClass = "dnnGridAltItem";
            this.SetUpRolesGrid();
            this.pnlRoleSlections.Controls.Add(this.dgRoleSelection);

            this.Controls.Add(this.pnlRoleSlections);
			Logger.LogWarning("<<< Exiting CreateChildControls()");
        }

        /// <summary>Overrides the base OnPreRender method to Bind the Grid to the Permissions.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            this.BindData();
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>Updates a Selection.</summary>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="selected">Whether it's selected.</param>
        protected virtual void UpdateSelection(string roleName, bool selected)
        {
			Logger.LogError(">>> Entering UpdateSelection(roleName={RoleName},selected={Selected})", roleName, selected);
            var isMatch = false;
			Logger.LogError("UpdateSelection(roleName={RoleName},selected={Selected}): isMatch → {IsMatch}", roleName, selected, isMatch);
            foreach (string currentRoleName in this.CurrentRoleSelection)
            {
                if (currentRoleName == roleName)
                {
                    // role is in collection
                    if (!selected)
                    {
                        // Remove from collection as we only keep selected roles
                        this.CurrentRoleSelection.Remove(currentRoleName);
                    }

                    isMatch = true;
                    break;
                }
            }

            // Rolename not found so add new
            if (!isMatch && selected)
            {
                this.CurrentRoleSelection.Add(roleName);
            }
			Logger.LogWarning("<<< Exiting UpdateSelection(roleName={RoleName},selected={Selected})", roleName, selected);
        }

        /// <summary>Updates the Selections.</summary>
        protected void UpdateSelections()
        {
			Logger.LogError(">>> Entering UpdateSelections()");
            this.EnsureChildControls();

            this.UpdateRoleSelections();
			Logger.LogWarning("<<< Exiting UpdateSelections()");
        }

        /// <summary>Updates the permissions.</summary>
        protected void UpdateRoleSelections()
        {
			Logger.LogError(">>> Entering UpdateRoleSelections()");
            if (this.dgRoleSelection != null)
            {
                foreach (DataGridItem dgi in this.dgRoleSelection.Items)
                {
                    const int i = 2;
                    if (dgi.Cells[i].Controls.Count > 0)
                    {
                        var cb = (CheckBox)dgi.Cells[i].Controls[0];
						Logger.LogError("UpdateRoleSelections(): cb → {Cb}", cb);
                        this.UpdateSelection(dgi.Cells[0].Text, cb.Checked);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting UpdateRoleSelections()");
        }

        /// <summary>Bind the data to the controls.</summary>
        private void BindData()
        {
			Logger.LogError(">>> Entering BindData()");
            this.EnsureChildControls();

            this.BindRolesGrid();
			Logger.LogWarning("<<< Exiting BindData()");
        }

        /// <summary>Bind the Roles data to the Grid.</summary>
        private void BindRolesGrid()
        {
			Logger.LogError(">>> Entering BindRolesGrid()");
            this.DtRolesSelection.Columns.Clear();
            this.DtRolesSelection.Rows.Clear();

            // Add Roles Column
            var col = new DataColumn("RoleId", typeof(string));
			Logger.LogError("BindRolesGrid(): col → {Col}", col);
            this.DtRolesSelection.Columns.Add(col);

            // Add Roles Column
            col = new DataColumn("RoleName", typeof(string));
            this.DtRolesSelection.Columns.Add(col);

            // Add Selected Column
            col = new DataColumn("Selected", typeof(bool));
            this.DtRolesSelection.Columns.Add(col);

            this.GetRoles();

            this.UpdateRoleSelections();
            for (int i = 0; i <= this.roles.Count - 1; i++)
            {
                var role = this.roles[i];
                DataRow row = this.DtRolesSelection.NewRow();
				Logger.LogError("BindRolesGrid(): row → {Row}", row);
                row["RoleId"] = role.RoleID;
                row["RoleName"] = Localization.LocalizeRole(role.RoleName);
                row["Selected"] = this.GetSelection(role.RoleName);

                this.DtRolesSelection.Rows.Add(row);
            }

            this.dgRoleSelection.DataSource = this.DtRolesSelection;
            this.dgRoleSelection.DataBind();
			Logger.LogWarning("<<< Exiting BindRolesGrid()");
        }

        private bool GetSelection(string roleName)
        {
			Logger.LogError(">>> Entering GetSelection(roleName={RoleName})", roleName);
			Logger.LogWarning("<<< Exiting GetSelection(roleName={RoleName})", roleName);
            return this.CurrentRoleSelection.Any(r => r == roleName);
        }

        /// <summary>Gets the roles from the Database and loads them into the Roles property.</summary>
        private void GetRoles()
        {
			Logger.LogError(">>> Entering GetRoles()");
            int roleGroupId = -2;
			Logger.LogError("GetRoles(): roleGroupId → {RoleGroupId}", roleGroupId);
            if (this.cboRoleGroups is { SelectedValue: not null, })
            {
                roleGroupId = int.Parse(this.cboRoleGroups.SelectedValue, CultureInfo.InvariantCulture);
            }

            this.roles = roleGroupId > -2
                    ? RoleController.Instance.GetRoles(PortalController.Instance.GetCurrentSettings().PortalId, r => r.RoleGroupID == roleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved)
                    : RoleController.Instance.GetRoles(PortalController.Instance.GetCurrentSettings().PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved);
					Logger.LogDebug("GetRoles(): roles → {Roles}", roles);
					Logger.LogDebug("GetRoles(): this.roles → {Arg0}", this.roles);

            if (roleGroupId < 0)
            {
                if (this.ShowUnauthenticatedUsers)
                {
                    this.roles.Add(new RoleInfo { RoleID = int.Parse(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture), RoleName = Globals.glbRoleUnauthUserName });
                }

                if (this.ShowAllUsers)
                {
                    this.roles.Add(new RoleInfo { RoleID = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture), RoleName = Globals.glbRoleAllUsersName });
                }
            }

			Logger.LogWarning("<<< Exiting GetRoles()");
            this.roles = this.roles.OrderBy(r => r.RoleName).ToList();
        }

        /// <summary>Sets up the columns for the Grid.</summary>
        private void SetUpRolesGrid()
        {
			Logger.LogError(">>> Entering SetUpRolesGrid()");
            this.dgRoleSelection.Columns.Clear();
            var textCol = new BoundColumn { HeaderText = "&nbsp;", DataField = "RoleName" };
			Logger.LogError("SetUpRolesGrid(): textCol → {TextCol}", textCol);
            textCol.ItemStyle.Width = Unit.Parse("150px", CultureInfo.InvariantCulture);
            this.dgRoleSelection.Columns.Add(textCol);
            var idCol = new BoundColumn { HeaderText = string.Empty, DataField = "roleid", Visible = false };
			Logger.LogError("SetUpRolesGrid(): idCol → {IdCol}", idCol);
            this.dgRoleSelection.Columns.Add(idCol);
            var checkCol = new TemplateColumn();
			Logger.LogError("SetUpRolesGrid(): checkCol → {CheckCol}", checkCol);
			Logger.LogDebug("SetUpRolesGrid(): checkCol.ItemTemplate → {Arg0}", checkCol.ItemTemplate);
            var columnTemplate = new CheckBoxColumnTemplate { DataField = "Selected" };
			Logger.LogError("SetUpRolesGrid(): columnTemplate → {ColumnTemplate}", columnTemplate);
            checkCol.ItemTemplate = columnTemplate;
            checkCol.HeaderText = Localization.GetString("SelectedRole");
            checkCol.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            checkCol.HeaderStyle.Wrap = true;
            this.dgRoleSelection.Columns.Add(checkCol);
			Logger.LogWarning("<<< Exiting SetUpRolesGrid()");
        }
    }
}
