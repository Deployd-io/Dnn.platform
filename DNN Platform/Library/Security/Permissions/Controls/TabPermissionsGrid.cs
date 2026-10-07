// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A permissions grid for tab/page permissions.</summary>
    public class TabPermissionsGrid : PermissionsGrid
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabPermissionsGrid));
        private static readonly string[] PermissionKeySeparator = ["##",];
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private List<IPermissionInfo> permissionCollection;
        private int tabId = -1;
        private TabPermissionCollection tabPermissions;

        /// <summary>Initializes a new instance of the <see cref="TabPermissionsGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public TabPermissionsGrid()
            : this(null)
        {
		Logger.LogError(">>> Entering TabPermissionsGrid()");
        Logger.LogWarning("<<< Exiting TabPermissionsGrid()");
        }

        /// <summary>Initializes a new instance of the <see cref="TabPermissionsGrid"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public TabPermissionsGrid(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
			Logger.LogDebug("TabPermissionsGrid(permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", permissionDefinitionService, permissionDefinitionService);
        Logger.LogWarning("<<< Exiting TabPermissionsGrid(permissionDefinitionService={PermissionDefinitionService})", permissionDefinitionService);
        }

        /// <summary>Gets the Permissions Collection.</summary>
        public TabPermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the TabPermissions
                return this.tabPermissions;
            }
        }

        /// <summary>Gets or sets the ID of the Tab.</summary>
        public int TabID
        {
            get
            {
                return this.tabId;
            }

            set
            {
                this.tabId = value;
                if (!this.Page.IsPostBack)
                {
                    this.GetTabPermissions();
                }
            }
        }

        /// <inheritdoc />
        protected override bool SupportsPermissionsAbstractions => true;

        /// <inheritdoc />
        protected override IList<IPermissionInfo> PermissionCollection
        {
            get
            {
                if (this.permissionCollection == null && this.tabPermissions != null)
                {
                    this.permissionCollection = this.tabPermissions.Cast<IPermissionInfo>().ToList();
                }

                return this.permissionCollection;
            }
        }

        /// <inheritdoc />
        public override void DataBind()
        {
			Logger.LogError(">>> Entering DataBind()");
            this.GetTabPermissions();
            base.DataBind();
			Logger.LogWarning("<<< Exiting DataBind()");
        }

        /// <inheritdoc />
        public override void GenerateDataGrid()
        {
		Logger.LogError(">>> Entering GenerateDataGrid()");
        Logger.LogWarning("<<< Exiting GenerateDataGrid()");
        }

        /// <inheritdoc />
        protected override bool IsFullControl(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogError(">>> Entering IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
            Logger.LogWarning("<<< Exiting IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return permissionDefinition.PermissionKey == "EDIT" && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <inheritdoc />
        protected override bool IsViewPermission(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogError(">>> Entering IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
            Logger.LogWarning("<<< Exiting IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return permissionDefinition.PermissionKey == "VIEW";
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogError(">>> Entering CreateChildControls()");
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += RolePermissionsGrid_ItemDataBound;
			Logger.LogWarning("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void AddPermission(IPermissionDefinitionInfo permissionDefinition, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
			Logger.LogError(">>> Entering AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess})", permissionDefinition, roleId, roleName, userId, displayName, allowAccess);
            var objPermission = new TabPermissionInfo(permissionDefinition)
            {
                TabID = this.TabID,
                RoleName = roleName,
                AllowAccess = allowAccess,
                DisplayName = displayName,
            };
            ((IPermissionInfo)objPermission).RoleId = roleId;
            ((IPermissionInfo)objPermission).UserId = userId;
            this.tabPermissions.Add(objPermission, true);

            // Clear Permission List
            this.permissionCollection = null;
			Logger.LogDebug("AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess}): this.permissionCollection → {Arg6}", permissionDefinition, roleId, roleName, userId, displayName, allowAccess, this.permissionCollection);
			Logger.LogDebug("AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess}): permissionCollection → {PermissionCollection}", permissionDefinition, roleId, roleName, userId, displayName, allowAccess, permissionCollection);
			Logger.LogWarning("<<< Exiting AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess})", permissionDefinition, roleId, roleName, userId, displayName, allowAccess);
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
			Logger.LogError(">>> Entering AddPermission(permissionsList={PermissionsList},user={User})", permissionsList, user);
            // Search TabPermission Collection for the user
            bool isMatch = false;
			Logger.LogError("AddPermission(permissionsList={PermissionsList},user={User}): isMatch → {IsMatch}", permissionsList, user, isMatch);
            foreach (IPermissionInfo objTabPermission in this.tabPermissions)
            {
                if (objTabPermission.UserId == user.UserID)
                {
                    isMatch = true;
                    break;
                }
            }

            // user not found so add new
            if (!isMatch)
            {
                foreach (var objPermission in permissionsList)
                {
                    if (objPermission.PermissionKey == "VIEW")
                    {
                        this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, user.UserID, user.DisplayName, true);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting AddPermission(permissionsList={PermissionsList},user={User})", permissionsList, user);
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, RoleInfo role)
        {
			Logger.LogError(">>> Entering AddPermission(permissionsList={PermissionsList},role={Role})", permissionsList, role);
            // Search TabPermission Collection for the user
            if (this.tabPermissions.Any((IPermissionInfo tp) => tp.RoleId == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (var permission in permissionsList)
            {
                if (permission.PermissionKey == "VIEW")
                {
                    this.AddPermission(permission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
			Logger.LogWarning("<<< Exiting AddPermission(permissionsList={PermissionsList},role={Role})", permissionsList, role);
        }

        /// <inheritdoc />
        protected override bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
        {
			Logger.LogError(">>> Entering GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column})", permissionDefinition, role, column);
            Logger.LogWarning("<<< Exiting GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column})", permissionDefinition, role, column);
            return !IsImplicitRole(role.PortalID, role.RoleID);
        }

        /// <inheritdoc />
        protected override string GetPermission(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column, string defaultState)
        {
			Logger.LogError(">>> Entering GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState})", permissionDefinition, role, column, defaultState);
            string permission;

            if (role.RoleID == this.AdministratorRoleId)
            {
                permission = PermissionTypeGrant;
				Logger.LogError("GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState}): permission → {Permission}", permissionDefinition, role, column, defaultState, permission);
            }
            else
            {
				Logger.LogWarning("GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState}): permission → {Permission}", permissionDefinition, role, column, defaultState, permission);
                // Call base class method to handle standard permissions
                permission = base.GetPermission(permissionDefinition, role, column, PermissionTypeNull);
            }

			Logger.LogWarning("<<< Exiting GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState})", permissionDefinition, role, column, defaultState);
            return permission;
        }

        /// <inheritdoc />
        protected override IList<IPermissionDefinitionInfo> GetPermissionDefinitions()
        {
			Logger.LogError(">>> Entering GetPermissionDefinitions()");
            Logger.LogWarning("<<< Exiting GetPermissionDefinitions()");
            return this.permissionDefinitionService.GetDefinitionsByTab().ToList();
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

                // Load TabId
                if (myState[1] != null)
                {
                    this.TabID = Convert.ToInt32(myState[1], CultureInfo.InvariantCulture);
					Logger.LogDebug("LoadViewState(savedState={SavedState}): TabID → {TabID}", savedState, TabID);
                }

                // Load TabPermissions
                if (myState[2] != null)
                {
                    this.tabPermissions = new TabPermissionCollection();
					Logger.LogDebug("LoadViewState(savedState={SavedState}): tabPermissions → {TabPermissions}", savedState, tabPermissions);
                    string state = Convert.ToString(myState[2], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(PermissionKeySeparator, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.tabPermissions.Add(this.ParseKeys(settings));
                        }
                    }
                }
            }
			Logger.LogWarning("<<< Exiting LoadViewState(savedState={SavedState})", savedState);
        }

        /// <inheritdoc />
        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
			Logger.LogError(">>> Entering RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
            this.tabPermissions.Remove(permissionID, roleID, userID);

			Logger.LogWarning("<<< Exiting RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
			Logger.LogDebug("RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID}): this.permissionCollection → {Arg3}", permissionID, roleID, userID, this.permissionCollection);
			Logger.LogDebug("RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID}): permissionCollection → {PermissionCollection}", permissionID, roleID, userID, permissionCollection);
            // Clear Permission List
            this.permissionCollection = null;
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
			Logger.LogError(">>> Entering SaveViewState()");
            var allStates = new object[3];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the Tab Id
            allStates[1] = this.TabID;

            // Persist the TabPermisisons
            var sb = new StringBuilder();
			Logger.LogError("SaveViewState(): sb → {Sb}", sb);
            if (this.tabPermissions != null)
            {
                bool addDelimiter = false;
				Logger.LogError("SaveViewState(): addDelimiter → {AddDelimiter}", addDelimiter);
                foreach (TabPermissionInfo objTabPermission in this.tabPermissions)
                {
                    if (addDelimiter)
                    {
                        sb.Append("##");
                    }
                    else
                    {
                        addDelimiter = true;
                    }

                    sb.Append(this.BuildKey(
                        objTabPermission.AllowAccess,
                        ((IPermissionInfo)objTabPermission).PermissionId,
                        objTabPermission.TabPermissionID,
                        ((IPermissionInfo)objTabPermission).RoleId,
                        objTabPermission.RoleName,
                        ((IPermissionInfo)objTabPermission).UserId,
                        objTabPermission.DisplayName));
                }
            }

            allStates[2] = sb.ToString();
			Logger.LogWarning("<<< Exiting SaveViewState()");
            return allStates;
        }

        /// <inheritdoc />
        protected override bool SupportsDenyPermissions(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogError(">>> Entering SupportsDenyPermissions(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogWarning("<<< Exiting SupportsDenyPermissions(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return true;
        }

        private static void RolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			Logger.LogError(">>> Entering RolePermissionsGrid_ItemDataBound(sender={Sender},e={E})", sender, e);
            var item = e.Item;

            if (item.ItemType is ListItemType.Item or ListItemType.AlternatingItem or ListItemType.SelectedItem)
            {
                var roleId = int.Parse(((DataRowView)item.DataItem)[0].ToString(), CultureInfo.InvariantCulture);
				Logger.LogError("RolePermissionsGrid_ItemDataBound(sender={Sender},e={E}): roleId → {RoleId}", sender, e, roleId);
                if (IsImplicitRole(PortalSettings.Current.PortalId, roleId))
                {
                    if (item.Controls.Cast<Control>().Last().Controls[0] is ImageButton actionImage)
                    {
                        actionImage.Visible = false;
						Logger.LogDebug("RolePermissionsGrid_ItemDataBound(sender={Sender},e={E}): actionImage.Visible → {Arg2}", sender, e, actionImage.Visible);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting RolePermissionsGrid_ItemDataBound(sender={Sender},e={E})", sender, e);
        }

        private static bool IsImplicitRole(int portalId, int roleId)
        {
			Logger.LogError(">>> Entering IsImplicitRole(portalId={PortalId},roleId={RoleId})", portalId, roleId);
			Logger.LogWarning("<<< Exiting IsImplicitRole(portalId={PortalId},roleId={RoleId})", portalId, roleId);
            return TabPermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        /// <summary>Gets the TabPermissions from the Data Store.</summary>
        private void GetTabPermissions()
        {
			Logger.LogError(">>> Entering GetTabPermissions()");
			Logger.LogDebug("GetTabPermissions(): this.permissionCollection → {Arg0}", this.permissionCollection);
			Logger.LogDebug("GetTabPermissions(): permissionCollection → {PermissionCollection}", permissionCollection);
        Logger.LogWarning("<<< Exiting GetTabPermissions()");
            this.tabPermissions = new TabPermissionCollection(TabPermissionController.GetTabPermissions(this.TabID, this.PortalId));
            this.permissionCollection = null;
        }

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
        private TabPermissionInfo ParseKeys(string[] settings)
        {
			Logger.LogError(">>> Entering ParseKeys(settings={Settings})", settings);
            var objTabPermission = new TabPermissionInfo();

			Logger.LogError("ParseKeys(settings={Settings}): objTabPermission → {ObjTabPermission}", settings, objTabPermission);
            // Call base class to load base properties
            this.ParsePermissionKeys(objTabPermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                objTabPermission.TabPermissionID = -1;
				Logger.LogDebug("ParseKeys(settings={Settings}): objTabPermission.TabPermissionID → {Arg1}", settings, objTabPermission.TabPermissionID);
            }
            else
            {
                objTabPermission.TabPermissionID = Convert.ToInt32(settings[2], CultureInfo.InvariantCulture);
            }

            objTabPermission.TabID = this.TabID;
			Logger.LogDebug("ParseKeys(settings={Settings}): objTabPermission.TabID → {Arg1}", settings, objTabPermission.TabID);

			Logger.LogWarning("<<< Exiting ParseKeys(settings={Settings})", settings);
            return objTabPermission;
        }
    }
}
