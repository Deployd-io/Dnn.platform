// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
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

    /// <summary>A permissions grid for folder permissions.</summary>
    public class FolderPermissionsGrid : PermissionsGrid
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FolderPermissionsGrid));
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected FolderPermissionCollection FolderPermissions;

        private static readonly string[] PermissionKeySeparator = ["##",];

        private readonly IPermissionDefinitionService permissionDefinitionService;
        private string folderPath = string.Empty;
        private List<IPermissionInfo> permissionCollection;
        private bool refreshGrid;
        private List<IPermissionDefinitionInfo> systemFolderPermissions;

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionsGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public FolderPermissionsGrid()
            : this(null)
        {
		Logger.LogDebug(">>> Entering FolderPermissionsGrid()");
        }
		Logger.LogDebug("<<< Exiting FolderPermissionsGrid()");

        /// <summary>Initializes a new instance of the <see cref="FolderPermissionsGrid"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public FolderPermissionsGrid(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
			Logger.LogDebug("FolderPermissionsGrid(permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", permissionDefinitionService, permissionDefinitionService);
        }

        /// <summary>Gets the Permission Collection.</summary>
        public FolderPermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the FolderPermissions
                return this.FolderPermissions;
            }
        }

        /// <summary>Gets or sets the path of the Folder.</summary>
        public string FolderPath
        {
            get
            {
                return this.folderPath;
            }

            set
            {
                this.folderPath = value;
                this.refreshGrid = true;
                this.GetFolderPermissions();
            }
        }

        /// <inheritdoc />
        protected override bool SupportsPermissionsAbstractions => true;

        /// <inheritdoc />
        protected override IList<IPermissionInfo> PermissionCollection
        {
            get
            {
                if (this.permissionCollection == null && this.FolderPermissions != null)
                {
                    this.permissionCollection = this.FolderPermissions.Cast<IPermissionInfo>().ToList();
                }

                return this.permissionCollection;
            }
        }

        /// <inheritdoc />
        protected override bool RefreshGrid
        {
            get
            {
                return this.refreshGrid;
            }
        }

        /// <inheritdoc />
        public override void GenerateDataGrid()
        {
		Logger.LogDebug(">>> Entering GenerateDataGrid()");
		Logger.LogDebug("<<< Exiting GenerateDataGrid()");
        }

        /// <summary>Gets the TabPermissions from the Data Store.</summary>
        protected virtual void GetFolderPermissions()
        {
			Logger.LogDebug(">>> Entering GetFolderPermissions()");
            this.FolderPermissions = new FolderPermissionCollection(FolderPermissionController.GetFolderPermissionsCollectionByFolder(this.PortalId, this.FolderPath));
            this.permissionCollection = null;
			Logger.LogDebug("GetFolderPermissions(): this.permissionCollection → {Arg0}", this.permissionCollection);
			Logger.LogDebug("GetFolderPermissions(): permissionCollection → {PermissionCollection}", permissionCollection);
			Logger.LogDebug("<<< Exiting GetFolderPermissions()");
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            base.CreateChildControls();
            this.rolePermissionsGrid.ItemDataBound += RolePermissionsGrid_ItemDataBound;
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void AddPermission(IPermissionDefinitionInfo permissionDefinition, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
			Logger.LogDebug(">>> Entering AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess})", permissionDefinition, roleId, roleName, userId, displayName, allowAccess);
            var objPermission = new FolderPermissionInfo(permissionDefinition)
            {
                FolderPath = this.FolderPath,
                RoleName = roleName,
                AllowAccess = allowAccess,
                DisplayName = displayName,
            };
            ((IPermissionInfo)objPermission).RoleId = roleId;
            ((IPermissionInfo)objPermission).UserId = userId;
            this.FolderPermissions.Add(objPermission, true);

            // Clear Permission List
            this.permissionCollection = null;
			Logger.LogDebug("AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess}): this.permissionCollection → {Arg6}", permissionDefinition, roleId, roleName, userId, displayName, allowAccess, this.permissionCollection);
			Logger.LogDebug("AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess}): permissionCollection → {PermissionCollection}", permissionDefinition, roleId, roleName, userId, displayName, allowAccess, permissionCollection);
			Logger.LogDebug("<<< Exiting AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess})", permissionDefinition, roleId, roleName, userId, displayName, allowAccess);
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
			Logger.LogDebug(">>> Entering AddPermission(permissionsList={PermissionsList},user={User})", permissionsList, user);
            bool isMatch = false;
            foreach (IPermissionInfo objFolderPermission in this.FolderPermissions)
            {
			Logger.LogDebug("AddPermission(permissionsList={PermissionsList},user={User}): isMatch → {IsMatch}", permissionsList, user, isMatch);
                if (objFolderPermission.UserId == user.UserID)
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
                    if (objPermission.PermissionKey == "READ")
                    {
                        this.AddPermission(objPermission, int.Parse(Globals.glbRoleNothing, CultureInfo.InvariantCulture), Null.NullString, user.UserID, user.DisplayName, true);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting AddPermission(permissionsList={PermissionsList},user={User})", permissionsList, user);
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, RoleInfo role)
        {
			Logger.LogDebug(">>> Entering AddPermission(permissionsList={PermissionsList},role={Role})", permissionsList, role);
            // Search TabPermission Collection for the user
            if (this.FolderPermissions.Any((IPermissionInfo p) => p.RoleId == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (var objPermission in permissionsList)
            {
                if (objPermission.PermissionKey == "READ")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
			Logger.LogDebug("<<< Exiting AddPermission(permissionsList={PermissionsList},role={Role})", permissionsList, role);
        }

        /// <inheritdoc />
        protected override bool GetEnabled(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column)
        {
			Logger.LogDebug(">>> Entering GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column})", permissionDefinition, role, column);
			Logger.LogDebug("<<< Exiting GetEnabled(permissionDefinition={PermissionDefinition},role={Role},column={Column})", permissionDefinition, role, column);
            return !IsImplicitRole(role.PortalID, role.RoleID);
        }

        /// <inheritdoc />
        protected override string GetPermission(IPermissionDefinitionInfo permissionDefinition, RoleInfo role, int column, string defaultState)
        {
			Logger.LogDebug(">>> Entering GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState})", permissionDefinition, role, column, defaultState);
            string permission;
            if (role.RoleID == this.AdministratorRoleId && this.IsPermissionAlwaysGrantedToAdmin(permissionDefinition))
            {
                permission = PermissionTypeGrant;
            }
            else
            {
                // Call base class method to handle standard permissions
                permission = base.GetPermission(permissionDefinition, role, column, defaultState);
            }

			Logger.LogDebug("<<< Exiting GetPermission(permissionDefinition={PermissionDefinition},role={Role},column={Column},defaultState={DefaultState})", permissionDefinition, role, column, defaultState);
            return permission;
        }

        /// <inheritdoc />
        protected override bool IsFullControl(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsFullControl(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return permissionDefinition.PermissionKey == "WRITE" && PermissionProvider.Instance().SupportsFullControl();
        }

        /// <inheritdoc />
        protected override bool IsViewPermission(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsViewPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return permissionDefinition.PermissionKey == "READ";
        }

        /// <inheritdoc />
        protected override IList<IPermissionDefinitionInfo> GetPermissionDefinitions()
        {
			Logger.LogDebug(">>> Entering GetPermissionDefinitions()");
            this.systemFolderPermissions = this.permissionDefinitionService.GetDefinitionsByFolder().ToList();
			Logger.LogDebug("GetPermissionDefinitions(): systemFolderPermissions → {SystemFolderPermissions}", systemFolderPermissions);
			Logger.LogDebug("<<< Exiting GetPermissionDefinitions()");
            return [..this.systemFolderPermissions];
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

                // Load FolderPath
                if (myState[1] != null)
                {
                    this.folderPath = Convert.ToString(myState[1], CultureInfo.InvariantCulture);
                }

                // Load FolderPermissions
                if (myState[2] != null)
                {
                    this.FolderPermissions = new FolderPermissionCollection();
					Logger.LogDebug("LoadViewState(savedState={SavedState}): FolderPermissions → {FolderPermissions}", savedState, FolderPermissions);
                    string state = Convert.ToString(myState[2], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(PermissionKeySeparator, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.FolderPermissions.Add(this.ParseKeys(settings));
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
            this.FolderPermissions.Remove(permissionID, roleID, userID);

            // Clear Permission List
            this.permissionCollection = null;
			Logger.LogDebug("RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID}): permissionCollection → {PermissionCollection}", permissionID, roleID, userID, permissionCollection);
			Logger.LogDebug("RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID}): this.permissionCollection → {Arg3}", permissionID, roleID, userID, this.permissionCollection);
			Logger.LogDebug("<<< Exiting RemovePermission(permissionID={PermissionID},roleID={RoleID},userID={UserID})", permissionID, roleID, userID);
        }

        /// <inheritdoc />
        protected override object SaveViewState()
        {
			Logger.LogDebug(">>> Entering SaveViewState()");
            var allStates = new object[3];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the Tab Id
            allStates[1] = this.FolderPath;

            // Persist the TabPermisisons
            var sb = new StringBuilder();
			Logger.LogDebug("SaveViewState(): sb → {Sb}", sb);
            if (this.FolderPermissions != null)
            {
                bool addDelimiter = false;
				Logger.LogDebug("SaveViewState(): addDelimiter → {AddDelimiter}", addDelimiter);
                foreach (IFolderPermissionInfo objFolderPermission in this.FolderPermissions)
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
                        objFolderPermission.AllowAccess,
                        objFolderPermission.PermissionId,
                        objFolderPermission.FolderPermissionId,
                        objFolderPermission.RoleId,
                        objFolderPermission.RoleName,
                        objFolderPermission.UserId,
                        objFolderPermission.DisplayName));
                }
            }

            allStates[2] = sb.ToString();
			Logger.LogDebug("<<< Exiting SaveViewState()");
            return allStates;
        }

        /// <inheritdoc />
        protected override bool SupportsDenyPermissions(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering SupportsDenyPermissions(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting SupportsDenyPermissions(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return this.IsSystemFolderPermission(permissionDefinition);
        }

        private static bool IsImplicitRole(int portalId, int roleId)
        {
			Logger.LogDebug(">>> Entering IsImplicitRole(portalId={PortalId},roleId={RoleId})", portalId, roleId);
			Logger.LogDebug("<<< Exiting IsImplicitRole(portalId={PortalId},roleId={RoleId})", portalId, roleId);
            return FolderPermissionController.ImplicitRoles(portalId).Any(r => r.RoleID == roleId);
        }

        private static void RolePermissionsGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			Logger.LogDebug(">>> Entering RolePermissionsGrid_ItemDataBound(sender={Sender},e={E})", sender, e);
            var item = e.Item;

            if (item.ItemType is ListItemType.Item or ListItemType.AlternatingItem or ListItemType.SelectedItem)
            {
                var roleId = int.Parse(((DataRowView)item.DataItem)[0].ToString(), CultureInfo.InvariantCulture);
				Logger.LogDebug("RolePermissionsGrid_ItemDataBound(sender={Sender},e={E}): roleId → {RoleId}", sender, e, roleId);
                if (IsImplicitRole(PortalSettings.Current.PortalId, roleId))
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

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
        private FolderPermissionInfo ParseKeys(string[] settings)
        {
			Logger.LogDebug(">>> Entering ParseKeys(settings={Settings})", settings);
            var objFolderPermission = new FolderPermissionInfo();

			Logger.LogDebug("ParseKeys(settings={Settings}): objFolderPermission → {ObjFolderPermission}", settings, objFolderPermission);
            // Call base class to load base properties
            this.ParsePermissionKeys((IPermissionInfo)objFolderPermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                ((IFolderPermissionInfo)objFolderPermission).FolderPermissionId = -1;
            }
            else
            {
                ((IFolderPermissionInfo)objFolderPermission).FolderPermissionId = Convert.ToInt32(settings[2], CultureInfo.InvariantCulture);
            }

            objFolderPermission.FolderPath = this.FolderPath;
			Logger.LogDebug("ParseKeys(settings={Settings}): objFolderPermission.FolderPath → {Arg1}", settings, objFolderPermission.FolderPath);
			Logger.LogDebug("<<< Exiting ParseKeys(settings={Settings})", settings);
            return objFolderPermission;
        }

        private bool IsPermissionAlwaysGrantedToAdmin(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsPermissionAlwaysGrantedToAdmin(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsPermissionAlwaysGrantedToAdmin(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return this.IsSystemFolderPermission(permissionDefinition);
        }

        private bool IsSystemFolderPermission(IPermissionDefinitionInfo permissionDefinition)
        {
			Logger.LogDebug(">>> Entering IsSystemFolderPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
			Logger.LogDebug("<<< Exiting IsSystemFolderPermission(permissionDefinition={PermissionDefinition})", permissionDefinition);
            return this.systemFolderPermissions.Any(pi => pi.PermissionId == permissionDefinition.PermissionId);
        }
    }
}
