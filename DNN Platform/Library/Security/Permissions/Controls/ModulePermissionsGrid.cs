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
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A permissions grid for module permissions.</summary>
    public class ModulePermissionsGrid : PermissionsGrid
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly string[] PermissionKeySeparator = ["##",];
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private bool inheritViewPermissionsFromTab;
        private int moduleId = -1;
        private ModulePermissionCollection modulePermissions;
        private List<IPermissionInfo> permissionCollection;
        private int viewColumnIndex;

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionsGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public ModulePermissionsGrid()
            : this(null)
        {
		Logger.LogDebug(">>> Entering ModulePermissionsGrid()");
        }
		Logger.LogDebug("<<< Exiting ModulePermissionsGrid()");

        /// <summary>Initializes a new instance of the <see cref="ModulePermissionsGrid"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public ModulePermissionsGrid(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
            this.TabId = -1;
			Logger.LogDebug("ModulePermissionsGrid(permissionDefinitionService={PermissionDefinitionService}): this.TabId → {Arg1}", permissionDefinitionService, this.TabId);
			Logger.LogDebug("ModulePermissionsGrid(permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", permissionDefinitionService, permissionDefinitionService);
        }

        /// <summary>Gets the ModulePermission Collection.</summary>
        public ModulePermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the ModulePermissions
                return this.modulePermissions;
            }
        }

        /// <summary>Gets or sets a value indicating whether the Module inherits the Page's(Tab's) permissions.</summary>
        public bool InheritViewPermissionsFromTab
        {
            get
            {
                return this.inheritViewPermissionsFromTab;
            }

            set
            {
                this.inheritViewPermissionsFromTab = value;
                this.permissionCollection = null;
            }
        }

        /// <summary>Gets or sets the ID of the Module.</summary>
        public int ModuleID
        {
            get
            {
                return this.moduleId;
            }

            set
            {
                this.moduleId = value;
                if (!this.Page.IsPostBack)
                {
                    this.GetModulePermissions();
                }
            }
        }

        /// <summary>Gets or sets the ID of the Tab associated with this module.</summary>
        public int TabId { get; set; }

        /// <inheritdoc />
        protected override bool SupportsPermissionsAbstractions => true;

        /// <inheritdoc />
        protected override IList<IPermissionInfo> PermissionCollection
        {
            get
            {
                if (this.permissionCollection == null && this.modulePermissions != null)
                {
                    this.permissionCollection = this.modulePermissions.Cast<IPermissionInfo>().ToList();
                }

                return this.permissionCollection;
            }
        }

        /// <inheritdoc />
        public override void GenerateDataGrid()
        {
		Logger.LogDebug(">>> Entering GenerateDataGrid()");
		Logger.LogDebug("<<< Exiting GenerateDataGrid()");
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += this.RolePermissionsGrid_ItemDataBound;
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
			Logger.LogDebug(">>> Entering AddPermission(permissionsList={PermissionsList},user={User})", permissionsList, user);
            if (this.modulePermissions.Any((IPermissionInfo mp) => mp.UserId == user.UserID))
            {
                return;
            }

            // user not found so add new
            foreach (var objPermission in permissionsList)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, user.UserID, user.DisplayName, true);
                }
            }
			Logger.LogDebug("<<< Exiting AddPermission(permissionsList={PermissionsList},user={User})", permissionsList, user);
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, RoleInfo role)
        {
			Logger.LogDebug(">>> Entering AddPermission(permissionsList={PermissionsList},role={Role})", permissionsList, role);
            // Search TabPermission Collection for the user
            if (this.modulePermissions.Any((IPermissionInfo p) => p.RoleId == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (var objPermission in permissionsList)
            {
                if (objPermission.PermissionKey == "VIEW")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
        }

        /// <inheritdoc />
        protected override void AddPermission(IPermissionDefinitionInfo permissionDefinition, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
            var objPermission = new ModulePermissionInfo(permissionDefinition)
            {
                ModuleID = this.ModuleID,
                RoleName = roleName,
                AllowAccess = allowAccess,
                DisplayName = displayName,
            };
            ((IPermissionInfo)objPermission).RoleId = roleId;
            ((IPermissionInfo)objPermission).UserId = userId;
            this.modulePermissions.Add(objPermission, true);

            // Clear Permission List
            this.permissionCollection = null;
        }

        /// <inheritdoc />
        protected override void UpdatePermission(IPermissionDefinitionInfo permission, int roleId, string roleName, string stateKey)
        {
            if (this.InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }

            base.UpdatePermission(permission, roleId, roleName, stateKey);
			Logger.LogDebug("<<< Exiting UpdatePermission(permission={Permission},roleId={RoleId},roleName={RoleName},stateKey={StateKey})", permission, roleId, roleName, stateKey);
        }

        /// <inheritdoc />
        protected override void UpdatePermission(IPermissionDefinitionInfo permission, string displayName, int userId, string stateKey)
        {
            if (this.InheritViewPermissionsFromTab && permission.PermissionKey == "VIEW")
            {
                return;
            }

            base.UpdatePermission(permission, displayName, userId, stateKey);
        }

        /// <inheritdoc />
        protected override bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
        {
			Logger.LogDebug(">>> Entering GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column})", permissionDefinition, role, column);
            bool enabled;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                enabled = false;
				Logger.LogInformation("GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column}): enabled → {Enabled}", permissionDefinition, role, column, enabled);
				Logger.LogDebug("GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column}): enabled → {Enabled}", permissionDefinition, role, column, enabled);
            }
            else
            {
                enabled = !IsImplicitRole(role.PortalID, role.RoleID);
            }

			Logger.LogDebug("<<< Exiting GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column})", permissionDefinition, role, column);
            return enabled;
        }

        /// <inheritdoc />
        protected override bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column)
        {
            bool enabled;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                enabled = false;
            }
            else
            {
                enabled = true;
            }

			Logger.LogDebug("<<< Exiting GetEnabled(permissionDefinition={PermissionDefinition},user={User},column={Column})", permissionDefinition, user, column);
            return enabled;
        }

        /// <inheritdoc />
        protected override string GetPermission(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column, string defaultState)
        {
			Logger.LogDebug(">>> Entering GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState})", permissionDefinition, role, column, defaultState);
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                return PermissionTypeNull;
            }

			Logger.LogDebug("<<< Exiting GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState})", permissionDefinition, role, column, defaultState);
            return role.RoleID == this.AdministratorRoleId
                ? PermissionTypeGrant
                : base.GetPermission(permissionDefinition, role, column, defaultState);
        }

        /// <inheritdoc />
        protected override string GetPermission(IPermissionDefinitionInfo permissionDefinition, UserInfo user, int column, string defaultState)
        {
            string permission;
            if (this.InheritViewPermissionsFromTab && column == this.viewColumnIndex)
            {
                permission = PermissionTypeNull;
            }
            else
            {
                // Call base class method to handle standard permissions
                permission = base.GetPermission(permissionDefinition, user, column, defaultState);
            }

			Logger.LogDebug("<<< Exiting GetPermission(permissionDefinition={PermissionDefinition},user={User},column={Column},defaultState={DefaultState})", permissionDefinition, user, column, defaultState);
            return permission;
        }

        /// <inheritdoc />
        protected override IList<IPermissionDefinitionInfo> GetPermissionDefinitions()
        {
			Logger.LogDebug(">>> Entering GetPermissionDefinitions()");
            var moduleInfo = ModuleController.Instance.GetModule(this.ModuleID, this.TabId, false);

			Logger.LogDebug("GetPermissionDefinitions(): moduleInfo → {ModuleInfo}", moduleInfo);
            var permissions = this.permissionDefinitionService.GetDefinitionsByModule(this.ModuleID, this.TabId).ToList();

			Logger.LogDebug("GetPermissionDefinitions(): permissions → {Permissions}", permissions);
            var permissionList = new List<IPermissionDefinitionInfo>();
            for (int i = 0; i <= permissions.Count - 1; i++)
			Logger.LogDebug("GetPermissionDefinitions(): permissionList → {PermissionList}", permissionList);
            {
                var permission = (PermissionInfo)permissions[i];
                if (permission.PermissionKey == "VIEW")
				Logger.LogWarning("GetPermissionDefinitions(): permission → {Permission}", permission);
                {
                    this.viewColumnIndex = i + 1;
					Logger.LogDebug("GetPermissionDefinitions(): viewColumnIndex → {ViewColumnIndex}", viewColumnIndex);
					Logger.LogDebug("GetPermissionDefinitions(): this.viewColumnIndex → {Arg0}", this.viewColumnIndex);
                    permissionList.Add(permission);
                }
                else
                {
                    if (!(moduleInfo.IsShared && moduleInfo.IsShareableViewOnly))
                    {
                        permissionList.Add(permission);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetPermissionDefinitions()");
            return permissionList;
        }

        /// <inheritdoc />
        protected override bool IsFullControl(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return (permissionDefinition.PermissionKey == "EDIT") && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <inheritdoc />
        protected override bool IsViewPermission(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return permissionDefinition.PermissionKey == "VIEW";
        }

        /// <inheritdoc />
        protected override void LoadViewState(object savedState)
        {
			Logger.LogDebug(">>> Entering LoadViewState(savedState={SavedState})", savedState);
            if (savedState != null)
            {
                // Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;

				Logger.LogDebug("LoadViewState(savedState={SavedState}): myState → {MyState}", savedState, myState);
                // Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load ModuleID
                if (myState[1] != null)
                {
                    this.ModuleID = Convert.ToInt32(myState[1], CultureInfo.InvariantCulture);
					Logger.LogDebug("LoadViewState(savedState={SavedState}): ModuleID → {ModuleID}", savedState, ModuleID);
                }

                // Load TabId
                if (myState[2] != null)
                {
                    this.TabId = Convert.ToInt32(myState[2], CultureInfo.InvariantCulture);
                }

                // Load InheritViewPermissionsFromTab
                if (myState[3] != null)
                {
                    this.InheritViewPermissionsFromTab = Convert.ToBoolean(myState[3], CultureInfo.InvariantCulture);
                }

                // Load ModulePermissions
                if (myState[4] != null)
                {
                    this.modulePermissions = new ModulePermissionCollection();
					Logger.LogDebug("LoadViewState(savedState={SavedState}): modulePermissions → {ModulePermissions}", savedState, modulePermissions);
                    string state = Convert.ToString(myState[4], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(PermissionKeySeparator, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.modulePermissions.Add(this.ParseKeys(settings));
                        }
                    }
                }
            }
			Logger.LogDebug("<<< Exiting LoadViewState(savedState={SavedState})", savedState);
        }

        /// <inheritdoc />
        protected override void RemovePermission(int permissionID, int roleID, int userID)
        {
			Logger.LogDebug(">>> Entering RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
            this.modulePermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this.permissionCollection = null;
			Logger.LogDebug("<<< Exiting RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
			Logger.LogDebug("RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID}): permissionCollection → {PermissionCollection}", permissionID, roleID, userID, permissionCollection);
			Logger.LogDebug("RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID}): this.permissionCollection → {Arg3}", permissionID, roleID, userID, this.permissionCollection);
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
			Logger.LogDebug(">>> Entering SaveViewState()");
            var allStates = new object[5];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the ModuleID
            allStates[1] = this.ModuleID;

            // Save the TabID
            allStates[2] = this.TabId;

            // Save the InheritViewPermissionsFromTab
            allStates[3] = this.InheritViewPermissionsFromTab;

            // Persist the ModulePermissions
            var sb = new StringBuilder();
            if (this.modulePermissions != null)
			Logger.LogDebug("SaveViewState(): sb → {Sb}", sb);
            {
                bool addDelimiter = false;
                foreach (ModulePermissionInfo modulePermission in this.modulePermissions)
                {
				Logger.LogDebug("SaveViewState(): addDelimiter → {AddDelimiter}", addDelimiter);
                    if (addDelimiter)
                    {
                        sb.Append("##");
                    }
                    else
                    {
                        addDelimiter = true;
                    }

                    sb.Append(this.BuildKey(
                        modulePermission.AllowAccess,
                        ((IPermissionInfo)modulePermission).PermissionId,
                        modulePermission.ModulePermissionID,
                        ((IPermissionInfo)modulePermission).RoleId,
                        modulePermission.RoleName,
                        ((IPermissionInfo)modulePermission).UserId,
                        modulePermission.DisplayName));
                }
            }

            allStates[4] = sb.ToString();
			Logger.LogDebug("<<< Exiting SaveViewState()");
            return allStates;
        }

        /// <inheritdoc />
        protected override bool SupportsDenyPermissions(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering SupportsDenyPermissions(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting SupportsDenyPermissions(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return true;
        }

        /// <summary>Check if a role is implicit for Module Permissions.</summary>
        private static bool IsImplicitRole(int portalId, int roleId)
        {
            return ModulePermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        /// <summary>Gets the ModulePermissions from the Data Store.</summary>
        private void GetModulePermissions()
        {
			Logger.LogDebug(">>> Entering GetModulePermissions()");
            this.modulePermissions = new ModulePermissionCollection(ModulePermissionController.GetModulePermissions(this.ModuleID, this.TabId));
            this.permissionCollection = null;
			Logger.LogDebug("<<< Exiting GetModulePermissions()");
			Logger.LogDebug("GetModulePermissions(): permissionCollection → {PermissionCollection}", permissionCollection);
			Logger.LogDebug("GetModulePermissions(): this.permissionCollection → {Arg0}", this.permissionCollection);
			Logger.LogDebug("GetModulePermissions(): modulePermissions → {ModulePermissions}", modulePermissions);
        }

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
        private ModulePermissionInfo ParseKeys(string[] settings)
        {
			Logger.LogDebug(">>> Entering ParseKeys(settings={Settings})", settings);
            var objModulePermission = new ModulePermissionInfo();

			Logger.LogDebug("ParseKeys(settings={Settings}): objModulePermission → {ObjModulePermission}", settings, objModulePermission);
            // Call base class to load base properties
            this.ParsePermissionKeys((IPermissionInfo)objModulePermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                objModulePermission.ModulePermissionID = -1;
				Logger.LogDebug("ParseKeys(settings={Settings}): objModulePermission.ModulePermissionID → {Arg1}", settings, objModulePermission.ModulePermissionID);
            }
            else
            {
                objModulePermission.ModulePermissionID = Convert.ToInt32(settings[2], CultureInfo.InvariantCulture);
            }

            objModulePermission.ModuleID = this.ModuleID;
			Logger.LogDebug("ParseKeys(settings={Settings}): objModulePermission.ModuleID → {Arg1}", settings, objModulePermission.ModuleID);
			Logger.LogDebug("<<< Exiting ParseKeys(settings={Settings})", settings);
            return objModulePermission;
        }

        private void RolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			Logger.LogDebug(">>> Entering RolePermissionsGrid_ItemDataBound(sender={Sender},e={E})", sender, e);
            var item = e.Item;

            if (item.ItemType is ListItemType.Item or ListItemType.AlternatingItem or ListItemType.SelectedItem)
            {
                var roleId = int.Parse(((DataRowView)item.DataItem)[0].ToString(), CultureInfo.InvariantCulture);
                if (IsImplicitRole(PortalSettings.Current.PortalId, roleId))
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
    }
}
