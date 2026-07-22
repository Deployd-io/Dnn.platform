// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.UI.WebControls.Internal;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>A base class for permissions grid controls.</summary>
    public abstract partial class PermissionsGrid : Control, INamingContainer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected const string PermissionTypeGrant = "True";
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected const string PermissionTypeDeny = "False";
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected const string PermissionTypeNull = "Null";
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DataGrid rolePermissionsGrid;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DataGrid userPermissionsGrid;

        private readonly IServicesFramework servicesFramework;
        private readonly int unAuthUsersRoleId = int.Parse(Globals.glbRoleUnauthUser, CultureInfo.InvariantCulture);
        private readonly int allUsersRoleId = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);

        private IList<IPermissionDefinitionInfo> permissions;
        private ArrayList users;
        private DropDownList cboRoleGroups;
        private DropDownList cboSelectRole;
        private LinkButton cmdUser;
        private LinkButton cmdRole;
        private Label lblGroups;
        private Label lblSelectRole;
        private Label lblErrorMessage;
        private Panel pnlPermissions;
        private TextBox txtUser;
        private HiddenField hiddenUserIds;
        private HiddenField roleField;

        /// <summary>Initializes a new instance of the <see cref="PermissionsGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public PermissionsGrid()
            : this(null)
        {
		Logger.LogDebug(">>> Entering PermissionsGrid()");
        }
		Logger.LogDebug("<<< Exiting PermissionsGrid()");

        /// <summary>Initializes a new instance of the <see cref="PermissionsGrid"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        public PermissionsGrid(IServicesFramework servicesFramework)
        {
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
            this.dtUserPermissions = new DataTable();
			Logger.LogDebug("PermissionsGrid(servicesFramework={ServicesFramework}): servicesFramework → {ServicesFramework1}", servicesFramework, servicesFramework);
            this.dtRolePermissions = new DataTable();
        }

        public TableItemStyle AlternatingItemStyle => this.rolePermissionsGrid.AlternatingItemStyle;

        public DataGridColumnCollection Columns => this.rolePermissionsGrid.Columns;

        public TableItemStyle FooterStyle => this.rolePermissionsGrid.FooterStyle;

        public TableItemStyle HeaderStyle => this.rolePermissionsGrid.HeaderStyle;

        public TableItemStyle ItemStyle => this.rolePermissionsGrid.ItemStyle;

        public DataGridItemCollection Items => this.rolePermissionsGrid.Items;

        public TableItemStyle SelectedItemStyle => this.rolePermissionsGrid.SelectedItemStyle;

        /// <summary>Gets the ID of the Administrator Role.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int AdministratorRoleId => PortalController.Instance.GetCurrentSettings().AdministratorRoleId;

        /// <summary>Gets the ID of the Registered Users Role.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int RegisteredUsersRoleId => PortalController.Instance.GetCurrentSettings().RegisteredRoleId;

        /// <summary>Gets the ID of the Portal.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int PortalId
        {
            get
            {
                // Obtain PortalSettings from Current Context
                var portalSettings = PortalController.Instance.GetCurrentSettings();
                int portalId;
                if (Globals.IsHostTab(TabController.CurrentPage.TabID))
                {
                    // if we are in host filemanager then we need to pass a null portal id
                    portalId = Null.NullInteger;
                }
                else
                {
                    portalId = portalSettings.PortalId;
                }

                return portalId;
            }
        }

        public bool AutoGenerateColumns
        {
            get
            {
                return this.rolePermissionsGrid.AutoGenerateColumns;
            }

            set
            {
                this.rolePermissionsGrid.AutoGenerateColumns = value;
                this.userPermissionsGrid.AutoGenerateColumns = value;
            }
        }

        public int CellSpacing
        {
            get
            {
                return this.rolePermissionsGrid.CellSpacing;
            }

            set
            {
                this.rolePermissionsGrid.CellSpacing = value;
                this.userPermissionsGrid.CellSpacing = value;
            }
        }

        public GridLines GridLines
        {
            get
            {
                return this.rolePermissionsGrid.GridLines;
            }

            set
            {
                this.rolePermissionsGrid.GridLines = value;
                this.userPermissionsGrid.GridLines = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether a Dynamic Column has been added.</summary>
        public bool DynamicColumnAdded
        {
            get => this.ViewState["ColumnAdded"] != null;
            set => this.ViewState["ColumnAdded"] = value;
        }

        /// <summary>Gets the underlying Permissions Data Table.</summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public DataTable dtRolePermissions { get; private set; }

        /// <summary>Gets the underlying Permissions Data Table.</summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public DataTable dtUserPermissions { get; private set; }

        /// <summary>Gets or sets the collection of Roles to display.</summary>
        public ArrayList Roles { get; set; }

        /// <summary>Gets or sets the ResourceFile to localize permissions.</summary>
        public string ResourceFile { get; set; }

        /// <summary>Gets a value indicating whether the implementation supports the APIs using <see cref="IPermissionDefinitionInfo"/>, rather than <see cref="PermissionInfoBase"/>.</summary>
        protected virtual bool SupportsPermissionsAbstractions => false;

        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use PermissionCollection property instead. Scheduled removal in v12.0.0.")]
        protected virtual List<PermissionInfoBase> PermissionsList => null;

        protected virtual IList<IPermissionInfo> PermissionCollection => this.PermissionsList?.Cast<IPermissionInfo>().ToList();

        protected virtual bool RefreshGrid => false;

        private int UnAuthUsersRoleId => this.unAuthUsersRoleId;

        private int AllUsersRoleId => this.allUsersRoleId;

        /// <summary>Registers the scripts necessary to make the tri-state controls work inside a RadAjaxPanel.</summary>
        /// <remarks>
        /// No need to call this unless using the PermissionGrid inside an ajax control that omits scripts on postback
        /// See DesktopModules/Admin/Tabs.ascx.cs for an example of usage.
        /// </remarks>
        public void RegisterScriptsForAjaxPanel()
        {
			Logger.LogDebug(">>> Entering RegisterScriptsForAjaxPanel()");
            PermissionTriState.RegisterScripts(this.Page, this);
			Logger.LogDebug("<<< Exiting RegisterScriptsForAjaxPanel()");
        }

        /// <summary>Generate the Data Grid.</summary>
        public abstract void GenerateDataGrid();

        /// <summary>Add the permission to the permission grid's collection.</summary>
        /// <param name="permission">The permission definition.</param>
        /// <param name="roleId">The ID of the role or <see cref="Globals.glbRoleNothing"/>.</param>
        /// <param name="roleName">The role name or <see cref="Null.NullString"/>.</param>
        /// <param name="userId">The user ID or <see cref="Null.NullInteger"/>.</param>
        /// <param name="displayName">The user's display name or <see cref="Null.NullString"/>.</param>
        /// <param name="allowAccess">Whether the permission allows or denies access.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial void AddPermission(PermissionInfo permission, int roleId, string roleName, int userId, string displayName, bool allowAccess)
            => this.AddPermission((IPermissionDefinitionInfo)permission, roleId, roleName, userId, displayName, allowAccess);

        /// <summary>Add the permission to the permission grid's collection.</summary>
        /// <param name="permissionDefinition">The permission definition.</param>
        /// <param name="roleId">The ID of the role or <see cref="Globals.glbRoleNothing"/>.</param>
        /// <param name="roleName">The role name or <see cref="Null.NullString"/>.</param>
        /// <param name="userId">The user ID or <see cref="Null.NullInteger"/>.</param>
        /// <param name="displayName">The user's display name or <see cref="Null.NullString"/>.</param>
        /// <param name="allowAccess">Whether the permission allows or denies access.</param>
        protected virtual void AddPermission(IPermissionDefinitionInfo permissionDefinition, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
        }

        /// <summary>Updates a Permission.</summary>
        /// <param name="permissions">The collection of permissions.</param>
        /// <param name="user">The user to add.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IList<IPermissionDefinitionInfo>")]
        protected virtual partial void AddPermission(ArrayList permissions, UserInfo user)
        {
        }

        /// <summary>Updates a Permission.</summary>
        /// <param name="permissionsList">The collection of permissions.</param>
        /// <param name="user">The user to add.</param>
        protected virtual void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
        }

        /// <summary>Updates a Permission.</summary>
        /// <param name="permissions">The collection of permissions.</param>
        /// <param name="role">The role to add.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IList<IPermissionDefinitionInfo>")]
        protected virtual partial void AddPermission(ArrayList permissions, RoleInfo role)
        {
        }

        /// <summary>Updates a Permission.</summary>
        /// <param name="permissionsList">The collection of permissions.</param>
        /// <param name="role">The role to add.</param>
        protected virtual void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, RoleInfo role)
        {
        }

        /// <summary>Builds the key used to store the "permission" information in the ViewState.</summary>
        /// <param name="allowAccess">The type of permission ( grant / deny ).</param>
        /// <param name="permissionId">The Id of the permission.</param>
        /// <param name="objectPermissionId">The Id of the object permission.</param>
        /// <param name="roleId">The role id.</param>
        /// <param name="roleName">The role name.</param>
        /// <returns>The permission state key.</returns>
        protected string BuildKey(bool allowAccess, int permissionId, int objectPermissionId, int roleId, string roleName)
        {
            return this.BuildKey(allowAccess, permissionId, objectPermissionId, roleId, roleName, Null.NullInteger, Null.NullString);
        }

        /// <summary>Builds the key used to store the "permission" information in the ViewState.</summary>
        /// <param name="allowAccess">The type of permission ( grant / deny ).</param>
        /// <param name="permissionId">The ID of the permission.</param>
        /// <param name="objectPermissionId">The ID of the object permission.</param>
        /// <param name="roleId">The role id.</param>
        /// <param name="roleName">The role name.</param>
        /// <param name="userID">The user id.</param>
        /// <param name="displayName">The user display name.</param>
        /// <returns>The permission state key.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string BuildKey(bool allowAccess, int permissionId, int objectPermissionId, int roleId, string roleName, int userID, string displayName)
        {
            string key;
            if (allowAccess)
            {
                key = "True";
            }
            else
            {
                key = "False";
            }

            key += "|" + Convert.ToString(permissionId, CultureInfo.InvariantCulture);
            key += "|";
            if (objectPermissionId > -1)
            {
                key += Convert.ToString(objectPermissionId, CultureInfo.InvariantCulture);
            }

            key += "|" + roleName;
            key += "|" + roleId;
            key += "|" + userID;
            key += "|" + displayName;

            return key;
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            this.permissions = this.SupportsPermissionsAbstractions
                ? this.GetPermissionDefinitions()
#pragma warning disable CS0618 // Type or member is obsolete
				Logger.LogDebug("CreateChildControls(): this.permissions → {Arg0}", this.permissions);
				Logger.LogDebug("CreateChildControls(): permissions → {Permissions}", permissions);
                : this.GetPermissions().Cast<IPermissionDefinitionInfo>().ToList();
#pragma warning restore CS0618 // Type or member is obsolete

            this.pnlPermissions = new Panel { CssClass = "dnnGrid dnnPermissionsGrid", };
			Logger.LogDebug("CreateChildControls(): pnlPermissions → {PnlPermissions}", pnlPermissions);
			Logger.LogDebug("CreateChildControls(): this.pnlPermissions → {Arg0}", this.pnlPermissions);

            // Optionally Add Role Group Filter
            this.CreateAddRoleControls();

            this.rolePermissionsGrid = new DataGrid
			Logger.LogDebug("CreateChildControls(): rolePermissionsGrid → {RolePermissionsGrid}", rolePermissionsGrid);
			Logger.LogDebug("CreateChildControls(): this.rolePermissionsGrid → {Arg0}", this.rolePermissionsGrid);
            {
                AutoGenerateColumns = false,
                CellSpacing = 0,
                CellPadding = 2,
                GridLines = GridLines.None,
                CssClass = "dnnPermissionsGrid",
            };
            this.rolePermissionsGrid.FooterStyle.CssClass = "dnnGridFooter";
            this.rolePermissionsGrid.HeaderStyle.CssClass = "dnnGridHeader";
            this.rolePermissionsGrid.ItemStyle.CssClass = "dnnGridItem";
            this.rolePermissionsGrid.AlternatingItemStyle.CssClass = "dnnGridAltItem";
            this.rolePermissionsGrid.ItemDataBound += this.RolePermissionsGrid_ItemDataBound;
            this.SetUpRolesGrid();
            this.pnlPermissions.Controls.Add(this.rolePermissionsGrid);

            this.users = this.GetUsers();
			Logger.LogDebug("CreateChildControls(): users → {Users}", users);
            if (this.users != null)
            {
                this.userPermissionsGrid = new DataGrid
				Logger.LogDebug("CreateChildControls(): userPermissionsGrid → {UserPermissionsGrid}", userPermissionsGrid);
				Logger.LogDebug("CreateChildControls(): this.userPermissionsGrid → {Arg0}", this.userPermissionsGrid);
                {
                    AutoGenerateColumns = false,
                    CellSpacing = 0,
                    GridLines = GridLines.None,
                    CssClass = "dnnPermissionsGrid",
                };
                this.userPermissionsGrid.FooterStyle.CssClass = "dnnGridFooter";
                this.userPermissionsGrid.HeaderStyle.CssClass = "dnnGridHeader";
                this.userPermissionsGrid.ItemStyle.CssClass = "dnnGridItem";
                this.userPermissionsGrid.AlternatingItemStyle.CssClass = "dnnGridAltItem";

                this.SetUpUsersGrid();
                this.pnlPermissions.Controls.Add(this.userPermissionsGrid);

                var divAddUser = new Panel { CssClass = "dnnFormItem" };

				Logger.LogDebug("CreateChildControls(): divAddUser → {DivAddUser}", divAddUser);
                this.lblErrorMessage = new Label { Text = Localization.GetString("DisplayName") };
				Logger.LogDebug("CreateChildControls(): lblErrorMessage → {LblErrorMessage}", lblErrorMessage);
                this.txtUser = new TextBox { ID = "txtUser" };
				Logger.LogDebug("CreateChildControls(): txtUser → {TxtUser}", txtUser);
				Logger.LogDebug("CreateChildControls(): this.txtUser → {Arg0}", this.txtUser);
                this.lblErrorMessage.AssociatedControlID = this.txtUser.ID;

                this.hiddenUserIds = new HiddenField { ID = "hiddenUserIds" };
				Logger.LogDebug("CreateChildControls(): hiddenUserIds → {HiddenUserIds}", hiddenUserIds);
				Logger.LogDebug("CreateChildControls(): this.hiddenUserIds → {Arg0}", this.hiddenUserIds);

                divAddUser.Controls.Add(this.lblErrorMessage);
                divAddUser.Controls.Add(this.txtUser);
                divAddUser.Controls.Add(this.hiddenUserIds);

                this.cmdUser = new LinkButton { Text = Localization.GetString("Add"), CssClass = "dnnSecondaryAction" };
				Logger.LogDebug("CreateChildControls(): cmdUser → {CmdUser}", cmdUser);
                divAddUser.Controls.Add(this.cmdUser);
                this.cmdUser.Click += this.AddUser;

                this.pnlPermissions.Controls.Add(divAddUser);
            }

            this.Controls.Add(this.pnlPermissions);
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <summary>Gets the Enabled status of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is enabled, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial bool GetEnabled(PermissionInfo objPerm, RoleInfo role, int column)
            => this.GetEnabled((IPermissionDefinitionInfo)objPerm, role, column);

        /// <summary>Gets the Enabled status of the permission.</summary>
        /// <param name="permissionDefinition">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is enabled, otherwise <see langword="false"/>.</returns>
        protected virtual bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
            => true;

        /// <summary>Gets the Enabled status of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is enabled, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial bool GetEnabled(PermissionInfo objPerm, UserInfo user, int column)
            => this.GetEnabled((IPermissionDefinitionInfo)objPerm, user, column);

        /// <summary>Gets the Enabled status of the permission.</summary>
        /// <param name="permissionDefinition">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is enabled, otherwise <see langword="false"/>.</returns>
        protected virtual bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column)
            => true;

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is granted, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial bool GetPermission(PermissionInfo objPerm, RoleInfo role, int column)
            => this.GetPermission((IPermissionDefinitionInfo)objPerm, role, column);

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="permissionDefinition">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is granted, otherwise <see langword="false"/>.</returns>
        protected virtual bool GetPermission(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
        {
            return Convert.ToBoolean(this.GetPermission(permissionDefinition, role, column, PermissionTypeDeny));
        }

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>The permission state (one of <see cref="PermissionTypeGrant"/>, <see cref="PermissionTypeDeny"/> or <paramref name="defaultState"/>).</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial string GetPermission(PermissionInfo objPerm, RoleInfo role, int column, string defaultState)
            => this.GetPermission((IPermissionDefinitionInfo)objPerm, role, column, defaultState);

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="permissionDefinition">The permission being loaded.</param>
        /// <param name="role">The role.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>The permission state (one of <see cref="PermissionTypeGrant"/>, <see cref="PermissionTypeDeny"/> or <paramref name="defaultState"/>).</returns>
        protected virtual string GetPermission(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column, string defaultState)
        {
            if (this.PermissionCollection == null)
            {
                return defaultState;
            }

            foreach (var permission in this.PermissionCollection)
            {
                if (permission.PermissionId == permissionDefinition.PermissionId && permission.RoleId == role.RoleID)
                {
                    return permission.AllowAccess ? PermissionTypeGrant : PermissionTypeDeny;
                }
            }

            return defaultState;
        }

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is granted, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial bool GetPermission(PermissionInfo objPerm, UserInfo user, int column)
            => this.GetPermission((IPermissionDefinitionInfo)objPerm, user, column);

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="permissionDefinition">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <returns><see langword="true"/> if the permission is granted, otherwise <see langword="false"/>.</returns>
        protected virtual bool GetPermission(IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column)
        {
            return Convert.ToBoolean(this.GetPermission(permissionDefinition, user, column, PermissionTypeDeny));
        }

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="objPerm">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>The permission state (one of <see cref="PermissionTypeGrant"/>, <see cref="PermissionTypeDeny"/> or <paramref name="defaultState"/>).</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial string GetPermission(PermissionInfo objPerm, UserInfo user, int column, string defaultState)
            => this.GetPermission((IPermissionDefinitionInfo)objPerm, user, column, defaultState);

        /// <summary>Gets the Value of the permission.</summary>
        /// <param name="permissionDefinition">The permission being loaded.</param>
        /// <param name="user">The user.</param>
        /// <param name="column">The column of the Grid.</param>
        /// <param name="defaultState">Default State.</param>
        /// <returns>The permission state (one of <see cref="PermissionTypeGrant"/>, <see cref="PermissionTypeDeny"/> or <paramref name="defaultState"/>).</returns>
        protected virtual string GetPermission(IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column, string defaultState)
        {
            IEnumerable<IPermissionInfo> permissionCollection = this.SupportsPermissionsAbstractions ? this.PermissionCollection : this.PermissionsList;
            if (permissionCollection == null)
            {
                return defaultState;
            }

            foreach (IPermissionInfo permission in permissionCollection)
            {
                if (permission.PermissionId == permissionDefinition.PermissionId && permission.UserId == user.UserID)
                {
                    return permission.AllowAccess ? PermissionTypeGrant : PermissionTypeDeny;
                }
            }

            return defaultState;
        }

        /// <summary>Gets the permissions from the Database.</summary>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PermissionInfo"/> instances.</returns>
        [DnnDeprecated(10, 2, 2, "Use GetPermissionDefinitions")]
        protected virtual partial ArrayList GetPermissions() => null;

        /// <summary>Gets the permissions from the Database.</summary>
        /// <remarks>If method does not return <see langword="null"/>, this value is used instead of the value from <see cref="GetPermissions"/>.</remarks>
        /// <returns>An <see cref="IList{T}"/> of <see cref="IPermissionDefinitionInfo"/> instances.</returns>
        protected virtual IList<IPermissionDefinitionInfo> GetPermissionDefinitions() => null;

        /// <summary>Gets the users from the Database.</summary>
        /// <returns>An <see cref="ArrayList"/> of <see cref="UserInfo"/> instances.</returns>
        protected virtual ArrayList GetUsers()
        {
			Logger.LogDebug(">>> Entering GetUsers()");
            IEnumerable<IPermissionInfo> permissionCollection = this.SupportsPermissionsAbstractions ? this.PermissionCollection : this.PermissionsList;
            if (permissionCollection == null)
            {
                return [];
            }

            var usersMap = new Dictionary<int, UserInfo>();
            foreach (var permission in permissionCollection.Where(permission => !Null.IsNull(permission.UserId)))
            {
			Logger.LogDebug("GetUsers(): usersMap → {UsersMap}", usersMap);
                if (!usersMap.ContainsKey(permission.UserId))
                {
                    usersMap.Add(
                        permission.UserId,
                        new UserInfo
                        {
                            UserID = permission.UserId,
                            Username = permission.Username,
                            DisplayName = permission.DisplayName,
                        });
                }
            }

			Logger.LogDebug("<<< Exiting GetUsers()");
            return new ArrayList(usersMap.Values);
        }

        /// <summary>Gets a value indicating whether the given permission definition represents full control.</summary>
        /// <param name="permissionInfo">The permission definition.</param>
        /// <returns><see langword="true"/> if it represents full control, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial bool IsFullControl(PermissionInfo permissionInfo)
            => this.IsFullControl((IPermissionDefinitionInfo)permissionInfo);

        /// <summary>Gets a value indicating whether the given permission definition represents full control.</summary>
        /// <param name="permissionDefinition">The permission definition.</param>
        /// <returns><see langword="true"/> if it represents full control, otherwise <see langword="false"/>.</returns>
        protected virtual bool IsFullControl(IPermissionDefinitionInfo permissionDefinition) => false;

        /// <summary>Gets a value indicating whether the given permission definition represents the view permission.</summary>
        /// <param name="permissionInfo">The permission definition.</param>
        /// <returns><see langword="true"/> if it represents the view permission, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial bool IsViewPermisison(PermissionInfo permissionInfo)
            => this.IsViewPermission((IPermissionDefinitionInfo)permissionInfo);

        /// <summary>Gets a value indicating whether the given permission definition represents the view permission.</summary>
        /// <param name="permissionDefinition">The permission definition.</param>
        /// <returns><see langword="true"/> if it represents the view permission, otherwise <see langword="false"/>.</returns>
        protected virtual bool IsViewPermission(IPermissionDefinitionInfo permissionDefinition) => false;

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.servicesFramework.RequestAjaxAntiForgerySupport();

            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Components/Tokeninput/jquery.tokeninput.js");
            ClientResourceManager.RegisterScript(this.Page, "~/js/dnn.permissiongrid.js");

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/Components/Tokeninput/Themes/token-input-facebook.css", FileOrder.Css.ResourceCss);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>Overrides the base OnPreRender method to Bind the Grid to the Permissions.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            this.BindData();

            var script = "var pgm = new dnn.permissionGridManager('" + this.ClientID + "');";
            if (ScriptManager.GetCurrent(this.Page) != null)
			Logger.LogDebug("OnPreRender(e={E}): script → {Script}", e, script);
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), this.ClientID + "PermissionGridManager", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "PermissionGridManager", script, true);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>Update the <paramref name="permission"/> with the <paramref name="settings"/>.</summary>
        /// <param name="permission">The permission.</param>
        /// <param name="settings">The settings.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionInfo")]
        protected virtual partial void ParsePermissionKeys(PermissionInfoBase permission, string[] settings)
            => this.ParsePermissionKeys((IPermissionInfo)permission, settings);
			Logger.LogDebug(">>> Entering ParsePermissionKeys(permission={Permission},settings={Settings})", permission, settings);

        /// <summary>Update the <paramref name="permission"/> with the <paramref name="settings"/>.</summary>
        /// <param name="permission">The permission.</param>
        /// <param name="settings">The settings.</param>
        protected virtual void ParsePermissionKeys(IPermissionInfo permission, string[] settings)
        {
            permission.PermissionId = Convert.ToInt32(settings[1], CultureInfo.InvariantCulture);
            permission.RoleId = Convert.ToInt32(settings[4], CultureInfo.InvariantCulture);
            permission.RoleName = settings[3];
			Logger.LogDebug("ParsePermissionKeys(permission={Permission},settings={Settings}): permission.RoleName → {Arg2}", permission, settings, permission.RoleName);
            permission.AllowAccess = Convert.ToBoolean(settings[0], CultureInfo.InvariantCulture);
            permission.UserId = Convert.ToInt32(settings[5], CultureInfo.InvariantCulture);
            permission.DisplayName = settings[6];
			Logger.LogDebug("<<< Exiting ParsePermissionKeys(permission={Permission},settings={Settings})", permission, settings);
			Logger.LogDebug("ParsePermissionKeys(permission={Permission},settings={Settings}): permission.DisplayName → {Arg2}", permission, settings, permission.DisplayName);
        }

        protected virtual void RemovePermission(int permissionID, int roleID, int userID)
        {
		Logger.LogDebug(">>> Entering RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
		Logger.LogDebug("<<< Exiting RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
        }

        /// <summary>Gets a value indicating whether the permission supports deny permissions.</summary>
        /// <param name="permissionInfo">The permission definition.</param>
        /// <returns><see langword="true"/> if deny permissions are supported, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial bool SupportsDenyPermissions(PermissionInfo permissionInfo) => this.SupportsDenyPermissions((IPermissionDefinitionInfo)permissionInfo);

        /// <summary>Gets a value indicating whether the permission supports deny permissions.</summary>
        /// <param name="permissionDefinition">The permission definition.</param>
        /// <returns><see langword="true"/> if deny permissions are supported, otherwise <see langword="false"/>.</returns>
        protected virtual bool SupportsDenyPermissions(IPermissionDefinitionInfo permissionDefinition) => false;

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="roleId">Role Id.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="allowAccess">The value of the permission.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial void UpdatePermission(PermissionInfo permission, int roleId, string roleName, bool allowAccess)
            => this.UpdatePermission((IPermissionDefinitionInfo)permission, roleId, roleName, allowAccess);

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="roleId">Role Id.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="allowAccess">The value of the permission.</param>
        protected virtual void UpdatePermission(IPermissionDefinitionInfo permission, int roleId, string roleName, bool allowAccess)
            => this.UpdatePermission(permission, roleId, roleName, allowAccess ? PermissionTypeGrant : PermissionTypeNull);

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="roleId">Role Id.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="stateKey">The permission state.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial void UpdatePermission(PermissionInfo permission, int roleId, string roleName, string stateKey)
            => this.UpdatePermission((IPermissionDefinitionInfo)permission, roleId, roleName, stateKey);

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="roleId">Role Id.</param>
        /// <param name="roleName">The name of the role.</param>
        /// <param name="stateKey">The permission state.</param>
        protected virtual void UpdatePermission(IPermissionDefinitionInfo permission, int roleId, string roleName, string stateKey)
        {
            this.RemovePermission(permission.PermissionId, roleId, Null.NullInteger);
            switch (stateKey)
            {
                case PermissionTypeGrant:
                    this.AddPermission(permission, roleId, roleName, Null.NullInteger, Null.NullString, true);
                    break;
                case PermissionTypeDeny:
                    this.AddPermission(permission, roleId, roleName, Null.NullInteger, Null.NullString, false);
                    break;
            }
        }

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="displayName">The user's display name.</param>
        /// <param name="userId">The user's id.</param>
        /// <param name="allowAccess">The value of the permission.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial void UpdatePermission(PermissionInfo permission, string displayName, int userId, bool allowAccess)
            => this.UpdatePermission((IPermissionDefinitionInfo)permission, displayName, userId, allowAccess);

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="displayName">The user's display name.</param>
        /// <param name="userId">The user's id.</param>
        /// <param name="allowAccess">The value of the permission.</param>
        protected virtual void UpdatePermission(IPermissionDefinitionInfo permission, string displayName, int userId, bool allowAccess)
            => this.UpdatePermission(permission, displayName, userId, allowAccess ? PermissionTypeGrant : PermissionTypeNull);

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="displayName">The user's display name.</param>
        /// <param name="userId">The user's id.</param>
        /// <param name="stateKey">The permission state.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IPermissionDefinitionInfo")]
        protected virtual partial void UpdatePermission(PermissionInfo permission, string displayName, int userId, string stateKey)
            => this.UpdatePermission((IPermissionDefinitionInfo)permission, displayName, userId, stateKey);

        /// <summary>Updates a Permission.</summary>
        /// <param name="permission">The permission being updated.</param>
        /// <param name="displayName">The user's display name.</param>
        /// <param name="userId">The user's id.</param>
        /// <param name="stateKey">The permission state.</param>
        protected virtual void UpdatePermission(IPermissionDefinitionInfo permission, string displayName, int userId, string stateKey)
        {
            this.RemovePermission(permission.PermissionId, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), userId);
            switch (stateKey)
            {
                case PermissionTypeGrant:
                    this.AddPermission(permission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, userId, displayName, true);
                    break;
                case PermissionTypeDeny:
                    this.AddPermission(permission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, userId, displayName, false);
                    break;
            }
        }

        /// <summary>Updates the permissions.</summary>
        protected void UpdatePermissions()
        {
			Logger.LogDebug(">>> Entering UpdatePermissions()");
            this.EnsureChildControls();
            this.GetRoles();
            this.UpdateRolePermissions();
            this.users = this.GetUsers();
			Logger.LogDebug("UpdatePermissions(): users → {Users}", users);
            this.UpdateUserPermissions();
			Logger.LogDebug("<<< Exiting UpdatePermissions()");
        }

        /// <summary>Updates the permissions.</summary>
        protected void UpdateRolePermissions()
        {
			Logger.LogDebug(">>> Entering UpdateRolePermissions()");
            if (this.rolePermissionsGrid != null && !this.RefreshGrid)
            {
                var rolesList = this.Roles.Cast<RoleInfo>().ToList();
                foreach (DataGridItem dgi in this.rolePermissionsGrid.Items)
                {
				Logger.LogDebug("UpdateRolePermissions(): rolesList → {RolesList}", rolesList);
                    var roleId = int.Parse(dgi.Cells[1].Text, CultureInfo.InvariantCulture);
                    if (rolesList.All(r => r.RoleID != roleId))
					Logger.LogDebug("UpdateRolePermissions(): roleId → {RoleId}", roleId);
                    {
                        continue;
                    }

                    for (int i = 2; i <= dgi.Cells.Count - 2; i++)
                    {
                        // all except first two cells which is role names and role ids and last column is Actions
                        if (dgi.Cells[i].Controls.Count > 0)
                        {
                            var permissionInfo = this.permissions[i - 2];
                            var triState = (PermissionTriState)dgi.Cells[i].Controls[0];
                            if (SupportsDenyPermissions(this, permissionInfo))
							Logger.LogDebug("UpdateRolePermissions(): triState → {TriState}", triState);
                            {
                                UpdatePermission(this, permissionInfo, roleId, dgi.Cells[0].Text, triState.Value);
                            }
                            else
                            {
                                UpdatePermission(this, permissionInfo, roleId, dgi.Cells[0].Text, triState.Value == PermissionTypeGrant);
                            }
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting UpdateRolePermissions()");
        }

        /// <summary>Updates the permissions.</summary>
        protected void UpdateUserPermissions()
        {
			Logger.LogDebug(">>> Entering UpdateUserPermissions()");
            if (this.userPermissionsGrid != null && !this.RefreshGrid)
            {
                var usersList = this.users.Cast<UserInfo>().ToList();
                foreach (DataGridItem dgi in this.userPermissionsGrid.Items)
                {
				Logger.LogDebug("UpdateUserPermissions(): usersList → {UsersList}", usersList);
                    var userId = int.Parse(dgi.Cells[1].Text, CultureInfo.InvariantCulture);
                    if (usersList.All(u => u.UserID != userId))
					Logger.LogDebug("UpdateUserPermissions(): userId → {UserId}", userId);
                    {
                        continue;
                    }

                    for (int i = 2; i <= dgi.Cells.Count - 2; i++)
                    {
                        // all except first two cells which is display name and user ID and Last column is Actions
                        if (dgi.Cells[i].Controls.Count > 0)
                        {
                            var permissionInfo = this.permissions[i - 2];
                            var triState = (PermissionTriState)dgi.Cells[i].Controls[0];
                            if (SupportsDenyPermissions(this, permissionInfo))
							Logger.LogDebug("UpdateUserPermissions(): triState → {TriState}", triState);
                            {
                                UpdatePermission(this, permissionInfo, dgi.Cells[0].Text, userId, triState.Value);
                            }
                            else
                            {
                                UpdatePermission(this, permissionInfo, dgi.Cells[0].Text, userId, triState.Value == PermissionTypeGrant);
                            }
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting UpdateUserPermissions()");
        }

        /// <summary>RoleGroupsSelectedIndexChanged runs when the Role Group is changed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void RoleGroupsSelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering RoleGroupsSelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.FillSelectRoleComboBox(int.Parse(this.cboRoleGroups.SelectedValue, CultureInfo.InvariantCulture));
			Logger.LogDebug("<<< Exiting RoleGroupsSelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        /// <summary>AddUser runs when the Add user linkbutton is clicked.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected virtual void AddUser(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering AddUser(sender={Sender},e={E})", sender, e);
            this.UpdatePermissions();
            if (!string.IsNullOrEmpty(this.hiddenUserIds.Value))
            {
                foreach (var id in this.hiddenUserIds.Value.Split(','))
                {
                    var userId = Convert.ToInt32(id, CultureInfo.InvariantCulture);
                    var user = UserController.GetUserById(this.PortalId, userId);
					Logger.LogDebug("AddUser(sender={Sender},e={E}): userId → {UserId}", sender, e, userId);
					Logger.LogDebug("AddUser(sender={Sender},e={E}): user → {User}", sender, e, user);
                    if (user != null)
                    {
                        AddPermission(this, this.permissions, user);
                        this.BindData();
                    }
                }

                this.txtUser.Text = this.hiddenUserIds.Value = string.Empty;
            }
			Logger.LogDebug("<<< Exiting AddUser(sender={Sender},e={E})", sender, e);
        }

        private static bool SupportsDenyPermissions(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinitionInfo)
        {
			Logger.LogDebug(">>> Entering SupportsDenyPermissions(permissionDefinitionInfo={PermissionDefinitionInfo})", permissionDefinitionInfo);
			Logger.LogDebug("<<< Exiting SupportsDenyPermissions(permissionDefinitionInfo={PermissionDefinitionInfo})", permissionDefinitionInfo);
            return @this.SupportsPermissionsAbstractions
                ? @this.SupportsDenyPermissions(permissionDefinitionInfo)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.SupportsDenyPermissions((PermissionInfo)permissionDefinitionInfo);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool GetEnabled(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
        {
            return @this.SupportsPermissionsAbstractions
                ? @this.GetEnabled(permissionDefinition, role, column)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.GetEnabled((PermissionInfo)permissionDefinition, role, column);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool GetEnabled(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column)
        {
            return @this.SupportsPermissionsAbstractions
                ? @this.GetEnabled(permissionDefinition, user, column)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.GetEnabled((PermissionInfo)permissionDefinition, user, column);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool GetPermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
        {
            return @this.SupportsPermissionsAbstractions
                ? @this.GetPermission(permissionDefinition, role, column)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.GetPermission((PermissionInfo)permissionDefinition, role, column);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool GetPermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column)
        {
            return @this.SupportsPermissionsAbstractions
                ? @this.GetPermission(permissionDefinition, user, column)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.GetPermission((PermissionInfo)permissionDefinition, user, column);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static string GetPermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column, string defaultState)
        {
            return @this.SupportsPermissionsAbstractions
                ? @this.GetPermission(permissionDefinition, role, column, defaultState)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.GetPermission((PermissionInfo)permissionDefinition, role, column, defaultState);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static string GetPermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column, string defaultState)
        {
            return @this.SupportsPermissionsAbstractions
                ? @this.GetPermission(permissionDefinition, user, column, defaultState)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.GetPermission((PermissionInfo)permissionDefinition, user, column, defaultState);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool IsViewPermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return @this.SupportsPermissionsAbstractions
                ? @this.IsViewPermission(permissionDefinition)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.IsViewPermisison((PermissionInfo)permissionDefinition);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static bool IsFullControl(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return @this.SupportsPermissionsAbstractions
                ? @this.IsFullControl(permissionDefinition)
#pragma warning disable CS0618 // Type or member is obsolete
                : @this.IsFullControl((PermissionInfo)permissionDefinition);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static void AddPermission(PermissionsGrid @this, IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
            if (@this.SupportsPermissionsAbstractions)
            {
                @this.AddPermission(permissionsList, user);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                var arrayList = new ArrayList(permissionsList.ToList());
                @this.AddPermission(arrayList, user);
                permissionsList.Clear();
                foreach (IPermissionDefinitionInfo item in arrayList)
                {
                    permissionsList.Add(item);
                }
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private static void UpdatePermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinitionInfo, int roleId, string roleName, string stateKey)
        {
            if (@this.SupportsPermissionsAbstractions)
            {
                @this.UpdatePermission(permissionDefinitionInfo, roleId, roleName, stateKey);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                @this.UpdatePermission((PermissionInfo)permissionDefinitionInfo, roleId, roleName, stateKey);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private static void UpdatePermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinitionInfo, int roleId, string roleName, bool allowAccess)
        {
            if (@this.SupportsPermissionsAbstractions)
            {
                @this.UpdatePermission(permissionDefinitionInfo, roleId, roleName, allowAccess);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                @this.UpdatePermission((PermissionInfo)permissionDefinitionInfo, roleId, roleName, allowAccess);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private static void UpdatePermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinitionInfo, string displayName, int userId, string stateKey)
        {
            if (@this.SupportsPermissionsAbstractions)
            {
                @this.UpdatePermission(permissionDefinitionInfo, displayName, userId, stateKey);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                @this.UpdatePermission((PermissionInfo)permissionDefinitionInfo, displayName, userId, stateKey);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private static void UpdatePermission(PermissionsGrid @this, IPermissionDefinitionInfo permissionDefinitionInfo, string displayName, int userId, bool allowAccess)
        {
            if (@this.SupportsPermissionsAbstractions)
            {
                @this.UpdatePermission(permissionDefinitionInfo, displayName, userId, allowAccess);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                @this.UpdatePermission((PermissionInfo)permissionDefinitionInfo, displayName, userId, allowAccess);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        private static string GetCommandType(string commandName)
        {
			Logger.LogDebug(">>> Entering GetCommandType(commandName={CommandName})", commandName);
            var command = commandName.ToLower(CultureInfo.InvariantCulture);
            if (command.Contains("rolename"))
			Logger.LogDebug("GetCommandType(commandName={CommandName}): command → {Command}", commandName, command);
            {
                return "ROLE";
            }

            if (command.Contains("displayname"))
            {
                return "USER";
            }

			Logger.LogDebug("<<< Exiting GetCommandType(commandName={CommandName})", commandName);
            return Null.NullString;
        }

        private static string GetGridCommand(string commandName)
        {
			Logger.LogDebug(">>> Entering GetGridCommand(commandName={CommandName})", commandName);
            var commandParts = commandName.Split('/');
			Logger.LogDebug("GetGridCommand(commandName={CommandName}): commandParts → {CommandParts}", commandName, commandParts);
			Logger.LogDebug("<<< Exiting GetGridCommand(commandName={CommandName})", commandName);
            return commandParts[0].ToUpper(CultureInfo.InvariantCulture);
        }

        private void BindData()
        {
			Logger.LogDebug(">>> Entering BindData()");
            this.EnsureChildControls();
            this.BindRolesGrid();
            this.BindUsersGrid();
			Logger.LogDebug("<<< Exiting BindData()");
        }

        private void BindRolesGrid()
        {
			Logger.LogDebug(">>> Entering BindRolesGrid()");
            this.dtRolePermissions.Columns.Clear();
            this.dtRolePermissions.Rows.Clear();

            // Add Roles Column
            this.dtRolePermissions.Columns.Add(new DataColumn("RoleId"));

            // Add Roles Column
            this.dtRolePermissions.Columns.Add(new DataColumn("RoleName"));

            foreach (var permissionInfo in this.permissions)
            {
                // Add Enabled Column
                this.dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName + "_Enabled"));

                // Add Permission Column
                this.dtRolePermissions.Columns.Add(new DataColumn(permissionInfo.PermissionName));
            }

            this.GetRoles();

            this.UpdateRolePermissions();
            for (int i = 0; i <= this.Roles.Count - 1; i++)
            {
                var role = (RoleInfo)this.Roles[i];
                var row = this.dtRolePermissions.NewRow();
				Logger.LogDebug("BindRolesGrid(): role → {Role}", role);
				Logger.LogDebug("BindRolesGrid(): row → {Row}", row);
                row["RoleId"] = role.RoleID;
                row["RoleName"] = Localization.LocalizeRole(role.RoleName);
                int j;
                for (j = 0; j <= this.permissions.Count - 1; j++)
                {
                    var objPerm = this.permissions[j];
                    row[objPerm.PermissionName + "_Enabled"] = GetEnabled(this, objPerm, role, j + 1);
                    if (this.SupportsDenyPermissions(objPerm))
                    {
                        row[objPerm.PermissionName] = GetPermission(this, objPerm, role, j + 1, PermissionTypeNull);
                    }
                    else
                    {
                        if (GetPermission(this, objPerm, role, j + 1))
                        {
                            row[objPerm.PermissionName] = PermissionTypeGrant;
                        }
                        else
                        {
                            row[objPerm.PermissionName] = PermissionTypeNull;
                        }
                    }
                }

                this.dtRolePermissions.Rows.Add(row);
            }

            this.rolePermissionsGrid.DataSource = this.dtRolePermissions;
            this.rolePermissionsGrid.DataBind();
			Logger.LogDebug("<<< Exiting BindRolesGrid()");
        }

        private void BindUsersGrid()
        {
			Logger.LogDebug(">>> Entering BindUsersGrid()");
            this.dtUserPermissions.Columns.Clear();
            this.dtUserPermissions.Rows.Clear();

            // Add Roles Column
            var col = new DataColumn("UserId");
            this.dtUserPermissions.Columns.Add(col);
			Logger.LogDebug("BindUsersGrid(): col → {Col}", col);

            // Add Roles Column
            col = new DataColumn("DisplayName");
            this.dtUserPermissions.Columns.Add(col);
            int i;
            for (i = 0; i <= this.permissions.Count - 1; i++)
            {
                PermissionInfo objPerm;
                objPerm = (PermissionInfo)this.permissions[i];

                // Add Enabled Column
                col = new DataColumn(objPerm.PermissionName + "_Enabled");
                this.dtUserPermissions.Columns.Add(col);

                // Add Permission Column
                col = new DataColumn(objPerm.PermissionName);
                this.dtUserPermissions.Columns.Add(col);
            }

            if (this.userPermissionsGrid != null)
            {
                this.users = this.GetUsers();
				Logger.LogDebug("BindUsersGrid(): users → {Users}", users);

                if (this.users.Count != 0)
                {
                    this.userPermissionsGrid.Visible = true;
                    DataRow row;
                    for (i = 0; i <= this.users.Count - 1; i++)
                    {
                        var user = (UserInfo)this.users[i];
                        row = this.dtUserPermissions.NewRow();
						Logger.LogDebug("BindUsersGrid(): user → {User}", user);
						Logger.LogDebug("BindUsersGrid(): row → {Row}", row);
                        row["UserId"] = user.UserID;
                        row["DisplayName"] = user.DisplayName;
                        int j;
                        for (j = 0; j <= this.permissions.Count - 1; j++)
                        {
                            var objPerm = this.permissions[j];
                            row[objPerm.PermissionName + "_Enabled"] = GetEnabled(this, objPerm, user, j + 1);
                            if (this.SupportsDenyPermissions(objPerm))
                            {
                                row[objPerm.PermissionName] = GetPermission(this, objPerm, user, j + 1, PermissionTypeNull);
                            }
                            else
                            {
                                if (GetPermission(this, objPerm, user, j + 1))
                                {
                                    row[objPerm.PermissionName] = PermissionTypeGrant;
                                }
                                else
                                {
                                    row[objPerm.PermissionName] = PermissionTypeNull;
                                }
                            }
                        }

                        this.dtUserPermissions.Rows.Add(row);
                    }

                    this.userPermissionsGrid.DataSource = this.dtUserPermissions;
                    this.userPermissionsGrid.DataBind();
                }
                else
                {
                    this.dtUserPermissions.Rows.Clear();
                    this.userPermissionsGrid.DataSource = this.dtUserPermissions;
                    this.userPermissionsGrid.DataBind();
                    this.userPermissionsGrid.Visible = false;
                }
            }
			Logger.LogDebug("<<< Exiting BindUsersGrid()");
        }

        private void EnsureRole(RoleInfo role)
        {
            if (this.Roles.Cast<RoleInfo>().All(r => r.RoleID != role.RoleID))
            {
                this.Roles.Add(role);
            }
        }

        private void GetRoles()
        {
			Logger.LogDebug(">>> Entering GetRoles()");
            var checkedRoles = this.GetCheckedRoles();
            var portalSettings = PortalController.Instance.GetCurrentSettings();
			Logger.LogDebug("GetRoles(): checkedRoles → {CheckedRoles}", checkedRoles);
			Logger.LogDebug("GetRoles(): portalSettings → {PortalSettings}", portalSettings);
            this.Roles = new ArrayList(RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved && checkedRoles.Contains(r.RoleID)).ToArray());

            if (checkedRoles.Contains(this.UnAuthUsersRoleId))
            {
                this.Roles.Add(new RoleInfo { RoleID = this.UnAuthUsersRoleId, RoleName = Globals.glbRoleUnauthUserName });
            }

            if (checkedRoles.Contains(this.AllUsersRoleId))
            {
                this.Roles.Add(new RoleInfo { RoleID = this.AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName = Globals.glbRoleAllUsersName });
            }

            // Administrators Role always has implicit permissions, then it should be always in
            this.EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId));

            // Show also default roles
            this.EnsureRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.RegisteredRoleId));
            this.EnsureRole(new RoleInfo { RoleID = this.AllUsersRoleId, PortalID = portalSettings.PortalId, RoleName = Globals.glbRoleAllUsersName });

            this.Roles.Reverse();
            this.Roles.Sort(new RoleComparer());
			Logger.LogDebug("<<< Exiting GetRoles()");
        }

        private IEnumerable<int> GetCheckedRoles()
        {
			Logger.LogDebug(">>> Entering GetCheckedRoles()");
            IEnumerable<IPermissionInfo> permissionCollection = this.SupportsPermissionsAbstractions ? this.PermissionCollection : this.PermissionsList;
			Logger.LogDebug("<<< Exiting GetCheckedRoles()");
            return permissionCollection?.Select(p => p.RoleId).Distinct() ?? new List<int>();
        }

        private void SetUpGrid(DataGrid grid, string nameColumnDataField, string idColumnDataField, string permissionHeaderText)
        {
			Logger.LogDebug(">>> Entering SetUpGrid(grid={Grid},nameColumnDataField={NameColumnDataField},idColumnDataField={IdColumnDataField},permissionHeaderText={PermissionHeaderText})", grid, nameColumnDataField, idColumnDataField, permissionHeaderText);
            grid.Columns.Clear();
            grid.Columns.Add(new BoundColumn
            {
                HeaderText = permissionHeaderText,
                DataField = nameColumnDataField,
                ItemStyle = { CssClass = "permissionHeader", },
                HeaderStyle = { CssClass = "permissionHeader", },
            });
            grid.Columns.Add(new BoundColumn
            {
                HeaderText = string.Empty,
                DataField = idColumnDataField,
                Visible = false,
            });

            foreach (var permission in this.permissions)
            {
                var locName = permission.ModuleDefId <= 0
                    ? Localization.GetString(permission.PermissionName + ".Permission", PermissionProvider.Instance().LocalResourceFile) // system permission
                    : !string.IsNullOrEmpty(this.ResourceFile)
                        ? Localization.GetString(permission.PermissionName + ".Permission", this.ResourceFile) // custom permission
                        : string.Empty;
                var templateCol = new TemplateColumn
                {
                    ItemTemplate = new PermissionTriStateTemplate(permission)
                    {
                        IsFullControl = IsFullControl(this, permission),
                        IsView = IsViewPermission(this, permission),
                        SupportDenyMode = SupportsDenyPermissions(this, permission),
                    },
                    HeaderText = !string.IsNullOrEmpty(locName) ? locName : permission.PermissionName,
                    HeaderStyle = { Wrap = true, },
                };
                grid.Columns.Add(templateCol);
            }

            var actionsColumn = new ImageCommandColumn
            {
                CommandName = "Delete/" + nameColumnDataField,
                KeyField = idColumnDataField,
                IconKey = "Delete",
                IconSize = "16x16",
                IconStyle = "PermissionGrid",
                HeaderText = Localization.GetString("PermissionActionsHeader.Text", PermissionProvider.Instance().LocalResourceFile),
            };
            grid.Columns.Add(actionsColumn);
            grid.ItemCommand += this.Grid_ItemCommand;
			Logger.LogDebug("<<< Exiting SetUpGrid(grid={Grid},nameColumnDataField={NameColumnDataField},idColumnDataField={IdColumnDataField},permissionHeaderText={PermissionHeaderText})", grid, nameColumnDataField, idColumnDataField, permissionHeaderText);
        }

        private void Grid_ItemCommand(object source, DataGridCommandEventArgs e)
        {
			Logger.LogDebug(">>> Entering Grid_ItemCommand(source={Source},e={E})", source, e);
            var entityId = int.Parse(e.CommandArgument.ToString(), CultureInfo.InvariantCulture);
            var command = GetGridCommand(e.CommandName);
			Logger.LogDebug("Grid_ItemCommand(source={Source},e={E}): entityId → {EntityId}", source, e, entityId);
			Logger.LogDebug("Grid_ItemCommand(source={Source},e={E}): command → {Command}", source, e, command);
            var entityType = GetCommandType(e.CommandName);
            switch (command)
			Logger.LogDebug("Grid_ItemCommand(source={Source},e={E}): entityType → {EntityType}", source, e, entityType);
            {
                case "DELETE":
                    if (entityType == "ROLE")
                    {
                        this.DeleteRolePermissions(entityId);
                    }
                    else if (entityType == "USER")
                    {
                        this.DeleteUserPermissions(entityId);
                    }

                    this.BindData();
                    break;
            }
			Logger.LogDebug("<<< Exiting Grid_ItemCommand(source={Source},e={E})", source, e);
        }

        private void DeleteRolePermissions(int entityId)
        {
            ////PermissionsList.RemoveAll(p => p.RoleID == entityID);
            IEnumerable<IPermissionInfo> permissionCollection = this.SupportsPermissionsAbstractions ? this.PermissionCollection : this.PermissionsList;
            var permissionToDelete = permissionCollection.Where(p => p.RoleId == entityId);
            foreach (var permission in permissionToDelete)
            {
                this.RemovePermission(permission.PermissionId, entityId, permission.UserId);
            }
        }

        private void DeleteUserPermissions(int entityId)
        {
            IEnumerable<IPermissionInfo> permissionCollection = this.SupportsPermissionsAbstractions ? this.PermissionCollection : this.PermissionsList;
            var permissionToDelete = permissionCollection.Where(p => p.UserId == entityId);
            foreach (var permission in permissionToDelete)
            {
                this.RemovePermission(permission.PermissionId, permission.RoleId, entityId);
            }
        }

        private void SetUpRolesGrid()
        {
			Logger.LogDebug(">>> Entering SetUpRolesGrid()");
            this.SetUpGrid(this.rolePermissionsGrid, "RoleName", "roleid", Localization.GetString("PermissionRoleHeader.Text", PermissionProvider.Instance().LocalResourceFile));
			Logger.LogDebug("<<< Exiting SetUpRolesGrid()");
        }

        private void SetUpUsersGrid()
        {
			Logger.LogDebug(">>> Entering SetUpUsersGrid()");
            if (this.userPermissionsGrid != null)
            {
                this.SetUpGrid(this.userPermissionsGrid, "DisplayName", "userid", Localization.GetString("PermissionUserHeader.Text", PermissionProvider.Instance().LocalResourceFile));
            }
			Logger.LogDebug("<<< Exiting SetUpUsersGrid()");
        }

        private void FillSelectRoleComboBox(int selectedRoleGroupId)
        {
			Logger.LogDebug(">>> Entering FillSelectRoleComboBox(selectedRoleGroupId={SelectedRoleGroupId})", selectedRoleGroupId);
            this.cboSelectRole.Items.Clear();
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var groupRoles = (selectedRoleGroupId > -2) ? RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.RoleGroupID == selectedRoleGroupId && r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved)
                : RoleController.Instance.GetRoles(portalSettings.PortalId, r => r.SecurityMode != SecurityMode.SocialGroup && r.Status == RoleStatus.Approved);
				Logger.LogDebug("FillSelectRoleComboBox(selectedRoleGroupId={SelectedRoleGroupId}): portalSettings → {PortalSettings}", selectedRoleGroupId, portalSettings);

            if (selectedRoleGroupId < 0)
            {
                groupRoles.Add(new RoleInfo { RoleID = this.UnAuthUsersRoleId, RoleName = Globals.glbRoleUnauthUserName });
                groupRoles.Add(new RoleInfo { RoleID = this.AllUsersRoleId, RoleName = Globals.glbRoleAllUsersName });
            }

            var roleList = new SortedList<string, int>();

			Logger.LogDebug("FillSelectRoleComboBox(selectedRoleGroupId={SelectedRoleGroupId}): roleList → {RoleList}", selectedRoleGroupId, roleList);
            foreach (var role in groupRoles)
            {
                roleList[Localization.LocalizeRole(role.RoleName)] = role.RoleID;
            }

            foreach (var role in roleList)
            {
                this.cboSelectRole.Items.Add(new ListItem(role.Key, role.Value.ToString(CultureInfo.InvariantCulture)));
            }

            int[] defaultRoleIds = [this.AllUsersRoleId, portalSettings.RegisteredRoleId, portalSettings.AdministratorRoleId,];
            var itemToSelect = this.cboSelectRole.Items.Cast<ListItem>().FirstOrDefault(i => !defaultRoleIds.Contains(int.Parse(i.Value, CultureInfo.InvariantCulture)));
            if (itemToSelect != null)
			Logger.LogDebug("FillSelectRoleComboBox(selectedRoleGroupId={SelectedRoleGroupId}): itemToSelect → {ItemToSelect}", selectedRoleGroupId, itemToSelect);
            {
                this.cboSelectRole.SelectedValue = itemToSelect.Value;
            }
			Logger.LogDebug("<<< Exiting FillSelectRoleComboBox(selectedRoleGroupId={SelectedRoleGroupId})", selectedRoleGroupId);
        }

        private void SetErrorMessage(string errorKey)
        {
			Logger.LogDebug(">>> Entering SetErrorMessage(errorKey={ErrorKey})", errorKey);
            this.lblErrorMessage = new Label
            {
				Logger.LogDebug("SetErrorMessage(errorKey={ErrorKey}): this.lblErrorMessage → {Arg1}", errorKey, this.lblErrorMessage);
				Logger.LogDebug("SetErrorMessage(errorKey={ErrorKey}): lblErrorMessage → {LblErrorMessage}", errorKey, lblErrorMessage);
                // TODO Remove DEBUG test
                Text = "<br />" + (errorKey.StartsWith("DEBUG", StringComparison.Ordinal) ? errorKey : Localization.GetString(errorKey)),
                CssClass = "NormalRed",
            };
            this.pnlPermissions.Controls.Add(this.lblErrorMessage);
			Logger.LogDebug("<<< Exiting SetErrorMessage(errorKey={ErrorKey})", errorKey);
        }

        private void RolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			Logger.LogDebug(">>> Entering RolePermissionsGrid_ItemDataBound(sender={Sender},e={E})", sender, e);
            var item = e.Item;

            if (item.ItemType is ListItemType.Item or ListItemType.AlternatingItem or ListItemType.SelectedItem)
            {
                var roleId = int.Parse(((DataRowView)item.DataItem)[0].ToString(), CultureInfo.InvariantCulture);
                if (roleId == PortalSettings.Current.AdministratorRoleId || roleId == this.AllUsersRoleId || roleId == PortalSettings.Current.RegisteredRoleId)
				Logger.LogDebug("RolePermissionsGrid_ItemDataBound(sender={Sender},e={E}): roleId → {RoleId}", sender, e, roleId);
                {
                    if (item.Controls.Cast<Control>().Last().Controls[0] is ImageButton actionImage)
                    {
                        actionImage.Visible = false;
						Logger.LogDebug("RolePermissionsGrid_ItemDataBound(sender={Sender},e={E}): actionImage.Visible → {Arg2}", sender, e, actionImage.Visible);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting RolePermissionsGrid_ItemDataBound(sender={Sender},e={E})", sender, e);
        }

        private void CreateAddRoleControls()
        {
			Logger.LogDebug(">>> Entering CreateAddRoleControls()");
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var arrGroups = RoleController.GetRoleGroups(portalSettings.PortalId);
			Logger.LogDebug("CreateAddRoleControls(): portalSettings → {PortalSettings}", portalSettings);
			Logger.LogDebug("CreateAddRoleControls(): arrGroups → {ArrGroups}", arrGroups);

            var addRoleControls = new Panel { CssClass = "dnnFormItem" };
            var divRoleGroups = new Panel { CssClass = "leftGroup" };
			Logger.LogDebug("CreateAddRoleControls(): addRoleControls → {AddRoleControls}", addRoleControls);
			Logger.LogDebug("CreateAddRoleControls(): divRoleGroups → {DivRoleGroups}", divRoleGroups);
            var divSelectRole = new Panel { CssClass = "rightGroup" };

			Logger.LogDebug("CreateAddRoleControls(): divSelectRole → {DivSelectRole}", divSelectRole);
            this.lblGroups = new Label { Text = Localization.GetString("RoleGroupFilter") };
			Logger.LogDebug("CreateAddRoleControls(): lblGroups → {LblGroups}", lblGroups);
            this.cboRoleGroups = new DropDownList { AutoPostBack = true, ID = "cboRoleGroups", ViewStateMode = ViewStateMode.Disabled };
			Logger.LogDebug("CreateAddRoleControls(): cboRoleGroups → {CboRoleGroups}", cboRoleGroups);
			Logger.LogDebug("CreateAddRoleControls(): this.cboRoleGroups → {Arg0}", this.cboRoleGroups);
            this.lblGroups.AssociatedControlID = this.cboRoleGroups.ID;
            divRoleGroups.Controls.Add(this.lblGroups);

            this.cboRoleGroups.SelectedIndexChanged += this.RoleGroupsSelectedIndexChanged;
            this.cboRoleGroups.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
            var liItem = new ListItem(Localization.GetString("GlobalRoles"), "-1") { Selected = true };
            this.cboRoleGroups.Items.Add(liItem);
			Logger.LogDebug("CreateAddRoleControls(): liItem → {LiItem}", liItem);
            foreach (RoleGroupInfo roleGroup in arrGroups)
            {
                this.cboRoleGroups.Items.Add(new ListItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString(CultureInfo.InvariantCulture)));
            }

            divRoleGroups.Controls.Add(this.cboRoleGroups);
            addRoleControls.Controls.Add(divRoleGroups);

            this.lblSelectRole = new Label { Text = Localization.GetString("RoleSelect") };
			Logger.LogDebug("CreateAddRoleControls(): lblSelectRole → {LblSelectRole}", lblSelectRole);
            this.cboSelectRole = new DropDownList { ID = "cboSelectRole", ViewStateMode = ViewStateMode.Disabled };
			Logger.LogDebug("CreateAddRoleControls(): cboSelectRole → {CboSelectRole}", cboSelectRole);
			Logger.LogDebug("CreateAddRoleControls(): this.cboSelectRole → {Arg0}", this.cboSelectRole);
            this.lblSelectRole.AssociatedControlID = this.cboSelectRole.ID;
            divSelectRole.Controls.Add(this.lblSelectRole);

            this.FillSelectRoleComboBox(-1); // Default Role Group is Global Roles
            divSelectRole.Controls.Add(this.cboSelectRole);

            this.cmdRole = new LinkButton { Text = Localization.GetString("Add"), CssClass = "dnnSecondaryAction" };
			Logger.LogDebug("CreateAddRoleControls(): cmdRole → {CmdRole}", cmdRole);
            this.cmdRole.Click += this.AddRole;
            divSelectRole.Controls.Add(this.cmdRole);
            this.roleField = new HiddenField() { ID = "roleField" };
			Logger.LogDebug("CreateAddRoleControls(): roleField → {RoleField}", roleField);
            addRoleControls.Controls.Add(this.roleField);

            addRoleControls.Controls.Add(divSelectRole);

            this.pnlPermissions.Controls.Add(addRoleControls);
			Logger.LogDebug("<<< Exiting CreateAddRoleControls()");
        }

        /// <summary>AddRole runs when the Add Role linkbutton is clicked.</summary>
        private void AddRole(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering AddRole(sender={Sender},e={E})", sender, e);
            this.UpdatePermissions();
            if (!int.TryParse(this.roleField.Value, out var selectedRoleId))
            {
                // Role not selected
                this.SetErrorMessage("InvalidRoleId");
                return;
            }

            // verify role
            var role = this.GetSelectedRole(selectedRoleId);
            if (role != null)
			Logger.LogDebug("AddRole(sender={Sender},e={E}): role → {Role}", sender, e, role);
            {
                this.AddPermission(this.permissions, role);
                this.BindData();
            }
            else
            {
                // role does not exist
                this.SetErrorMessage("RoleNotFound");
            }
			Logger.LogDebug("<<< Exiting AddRole(sender={Sender},e={E})", sender, e);
        }

        private RoleInfo GetSelectedRole(int selectedRoleId)
        {
			Logger.LogDebug(">>> Entering GetSelectedRole(selectedRoleId={SelectedRoleId})", selectedRoleId);
            RoleInfo role = null;
            if (selectedRoleId == this.AllUsersRoleId)
			Logger.LogDebug("GetSelectedRole(selectedRoleId={SelectedRoleId}): role → {Role}", selectedRoleId, role);
            {
                role = new RoleInfo
                {
                    RoleID = this.AllUsersRoleId,
                    RoleName = Globals.glbRoleAllUsersName,
                };
            }
            else if (selectedRoleId == this.UnAuthUsersRoleId)
            {
                role = new RoleInfo
                {
                    RoleID = this.UnAuthUsersRoleId,
                    RoleName = Globals.glbRoleUnauthUserName,
                };
            }
            else
            {
                role = RoleController.Instance.GetRoleById(this.PortalId, selectedRoleId);
            }

			Logger.LogDebug("<<< Exiting GetSelectedRole(selectedRoleId={SelectedRoleId})", selectedRoleId);
            return role;
        }
    }
}
