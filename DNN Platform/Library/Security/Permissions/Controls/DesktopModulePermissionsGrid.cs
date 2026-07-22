// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A permissions grid for desktop module permissions.</summary>
    public class DesktopModulePermissionsGrid : PermissionsGrid
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly string[] PermissionKeySeparator = ["##",];
        private readonly IPermissionDefinitionService permissionDefinitionService;
        private DesktopModulePermissionCollection desktopModulePermissions;
        private List<IPermissionInfo> permissionCollection;
        private int portalDesktopModuleId = -1;

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionsGrid"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public DesktopModulePermissionsGrid()
            : this(null)
        {
		Logger.LogDebug(">>> Entering DesktopModulePermissionsGrid()");
		Logger.LogDebug("<<< Exiting DesktopModulePermissionsGrid()");
		Logger.LogDebug(">>> Entering DesktopModulePermissionsGrid(permissionDefinitionService={PermissionDefinitionService})", permissionDefinitionService);
		Logger.LogDebug("<<< Exiting DesktopModulePermissionsGrid(permissionDefinitionService={PermissionDefinitionService})", permissionDefinitionService);
        }

        /// <summary>Initializes a new instance of the <see cref="DesktopModulePermissionsGrid"/> class.</summary>
        /// <param name="permissionDefinitionService">The permission definition service.</param>
        public DesktopModulePermissionsGrid(IPermissionDefinitionService permissionDefinitionService)
        {
            this.permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
			Logger.LogDebug("DesktopModulePermissionsGrid(permissionDefinitionService={PermissionDefinitionService}): permissionDefinitionService → {PermissionDefinitionService1}", permissionDefinitionService, permissionDefinitionService);
        }

        /// <summary>Gets the Permissions Collection.</summary>
        public DesktopModulePermissionCollection Permissions
        {
            get
            {
                // First Update Permissions in case they have been changed
                this.UpdatePermissions();

                // Return the DesktopModulePermissions
                return this.desktopModulePermissions;
            }
        }

        /// <summary>Gets or sets the ID of the PortalDesktopModule.</summary>
        public int PortalDesktopModuleID
        {
            get
            {
                return this.portalDesktopModuleId;
            }

            set
            {
                int oldValue = this.portalDesktopModuleId;
                this.portalDesktopModuleId = value;
                if (this.desktopModulePermissions == null || oldValue != value)
                {
                    this.GetDesktopModulePermissions();
                }
            }
        }

        /// <inheritdoc />
        protected override bool SupportsPermissionsAbstractions => true;

        /// <inheritdoc />
        protected override IList<IPermissionInfo> PermissionCollection => this.permissionCollection ??= this.desktopModulePermissions?.Cast<IPermissionInfo>().ToList();

        /// <summary>Resets the permissions collection.</summary>
        public void ResetPermissions()
        {
			Logger.LogDebug(">>> Entering ResetPermissions()");
            this.GetDesktopModulePermissions();
            this.permissionCollection = null;
			Logger.LogDebug("<<< Exiting ResetPermissions()");
			Logger.LogDebug("ResetPermissions(): permissionCollection → {PermissionCollection}", permissionCollection);
			Logger.LogDebug("ResetPermissions(): this.permissionCollection → {Arg0}", this.permissionCollection);
        }

        /// <inheritdoc />
        public override void GenerateDataGrid()
        {
		Logger.LogDebug(">>> Entering GenerateDataGrid()");
		Logger.LogDebug("<<< Exiting GenerateDataGrid()");
        }

        /// <inheritdoc />
        protected override void AddPermission(IPermissionDefinitionInfo permissionDefinition, int roleId, string roleName, int userId, string displayName, bool allowAccess)
        {
			Logger.LogDebug(">>> Entering AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess})", permissionDefinition, roleId, roleName, userId, displayName, allowAccess);
            var objPermission = new DesktopModulePermissionInfo(permissionDefinition)
            {
                PortalDesktopModuleID = this.PortalDesktopModuleID,
                RoleName = roleName,
                AllowAccess = allowAccess,
                DisplayName = displayName,
            };
            ((IPermissionInfo)objPermission).RoleId = roleId;
            ((IPermissionInfo)objPermission).UserId = userId;
            this.desktopModulePermissions.Add(objPermission, true);

            // Clear Permission List
            this.permissionCollection = null;
			Logger.LogDebug("AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess}): permissionCollection → {PermissionCollection}", permissionDefinition, roleId, roleName, userId, displayName, allowAccess, permissionCollection);
			Logger.LogDebug("AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess}): this.permissionCollection → {Arg6}", permissionDefinition, roleId, roleName, userId, displayName, allowAccess, this.permissionCollection);
			Logger.LogDebug("<<< Exiting AddPermission(permissionDefinition={PermissionDefinition},roleId={RoleId},roleName={RoleName},userId={UserId},displayName={DisplayName},allowAccess={AllowAccess})", permissionDefinition, roleId, roleName, userId, displayName, allowAccess);
        }

        /// <inheritdoc />
        protected override void AddPermission(IList<IPermissionDefinitionInfo> permissionsList, UserInfo user)
        {
			Logger.LogDebug(">>> Entering AddPermission(permissionsList={PermissionsList},user={User})", permissionsList, user);
            // Search DesktopModulePermission Collection for the user
            bool isMatch = false;
            foreach (IPermissionInfo objDesktopModulePermission in this.desktopModulePermissions)
            {
			Logger.LogDebug("AddPermission(permissionsList={PermissionsList},user={User}): isMatch → {IsMatch}", permissionsList, user, isMatch);
                if (objDesktopModulePermission.UserId == user.UserID)
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
                    if (objPermission.PermissionKey == "DEPLOY")
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
            if (this.desktopModulePermissions.Any((IPermissionInfo p) => p.RoleId == role.RoleID))
            {
                return;
            }

            // role not found so add new
            foreach (var objPermission in permissionsList)
            {
                if (objPermission.PermissionKey == "DEPLOY")
                {
                    this.AddPermission(objPermission, role.RoleID, role.RoleName, Null.NullInteger, Null.NullString, true);
                }
            }
			Logger.LogDebug("<<< Exiting AddPermission(permissionsList={PermissionsList},role={Role})", permissionsList, role);
        }

        /// <inheritdoc />
        protected override IList<IPermissionDefinitionInfo> GetPermissionDefinitions()
        {
			Logger.LogDebug(">>> Entering GetPermissionDefinitions()");
			Logger.LogDebug("<<< Exiting GetPermissionDefinitions()");
            return this.permissionDefinitionService.GetDefinitionsByPortalDesktopModule().ToList();
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

                // Load DesktopModuleId
                if (myState[1] != null)
                {
                    this.PortalDesktopModuleID = Convert.ToInt32(myState[1], CultureInfo.InvariantCulture);
					Logger.LogDebug("LoadViewState(savedState={SavedState}): PortalDesktopModuleID → {PortalDesktopModuleID}", savedState, PortalDesktopModuleID);
                }

                // Load DesktopModulePermissions
                if (myState[2] != null)
                {
                    this.desktopModulePermissions = new DesktopModulePermissionCollection();
					Logger.LogDebug("LoadViewState(savedState={SavedState}): desktopModulePermissions → {DesktopModulePermissions}", savedState, desktopModulePermissions);
                    string state = Convert.ToString(myState[2], CultureInfo.InvariantCulture);
                    if (!string.IsNullOrEmpty(state))
                    {
                        // First Break the String into individual Keys
                        string[] permissionKeys = state.Split(PermissionKeySeparator, StringSplitOptions.None);
                        foreach (string key in permissionKeys)
                        {
                            string[] settings = key.Split('|');
                            this.desktopModulePermissions.Add(this.ParseKeys(settings));
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
            this.desktopModulePermissions.Remove(permissionID, roleID, userID);

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
            var allStates = new object[3];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            // Save the DesktopModule ID
            allStates[1] = this.PortalDesktopModuleID;

            // Persist the DesktopModulePermissions
            var sb = new StringBuilder();
            if (this.desktopModulePermissions != null)
			Logger.LogDebug("SaveViewState(): sb → {Sb}", sb);
            {
                bool addDelimiter = false;
                foreach (DesktopModulePermissionInfo objDesktopModulePermission in this.desktopModulePermissions)
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
                        objDesktopModulePermission.AllowAccess,
                        ((IPermissionInfo)objDesktopModulePermission).PermissionId,
                        objDesktopModulePermission.DesktopModulePermissionID,
                        ((IPermissionInfo)objDesktopModulePermission).RoleId,
                        objDesktopModulePermission.RoleName,
                        ((IPermissionInfo)objDesktopModulePermission).UserId,
                        objDesktopModulePermission.DisplayName));
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
            return true;
        }

        /// <summary>Gets the DesktopModulePermissions from the Data Store.</summary>
        private void GetDesktopModulePermissions()
        {
			Logger.LogDebug(">>> Entering GetDesktopModulePermissions()");
            this.desktopModulePermissions = new DesktopModulePermissionCollection(DesktopModulePermissionController.GetDesktopModulePermissions(this.PortalDesktopModuleID));
			Logger.LogDebug("<<< Exiting GetDesktopModulePermissions()");
			Logger.LogDebug("GetDesktopModulePermissions(): desktopModulePermissions → {DesktopModulePermissions}", desktopModulePermissions);
        }

        /// <summary>Parse the Permission Keys used to persist the Permissions in the ViewState.</summary>
        /// <param name="settings">A string array of settings.</param>
        private DesktopModulePermissionInfo ParseKeys(string[] settings)
        {
			Logger.LogDebug(">>> Entering ParseKeys(settings={Settings})", settings);
            var objDesktopModulePermission = new DesktopModulePermissionInfo();

			Logger.LogDebug("ParseKeys(settings={Settings}): objDesktopModulePermission → {ObjDesktopModulePermission}", settings, objDesktopModulePermission);
            // Call base class to load base properties
            this.ParsePermissionKeys((IPermissionInfo)objDesktopModulePermission, settings);
            if (string.IsNullOrEmpty(settings[2]))
            {
                objDesktopModulePermission.DesktopModulePermissionID = -1;
				Logger.LogDebug("ParseKeys(settings={Settings}): objDesktopModulePermission.DesktopModulePermissionID → {Arg1}", settings, objDesktopModulePermission.DesktopModulePermissionID);
            }
            else
            {
                objDesktopModulePermission.DesktopModulePermissionID = Convert.ToInt32(settings[2], CultureInfo.InvariantCulture);
            }

            objDesktopModulePermission.PortalDesktopModuleID = this.PortalDesktopModuleID;
			Logger.LogDebug("ParseKeys(settings={Settings}): objDesktopModulePermission.PortalDesktopModuleID → {Arg1}", settings, objDesktopModulePermission.PortalDesktopModuleID);
			Logger.LogDebug("<<< Exiting ParseKeys(settings={Settings})", settings);
            return objDesktopModulePermission;
        }
    }
}
