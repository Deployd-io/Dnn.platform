// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Admin.Security
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The SecurityRoles PortalModuleBase is used to manage the users and roles they have.</summary>
    public partial class SecurityRoles : PortalModuleBase, IActionable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SecurityRoles));
        private readonly INavigationManager navigationManager;
        private readonly RoleProvider roleProvider;
        private readonly IRoleController roleController;
        private readonly IEventManager eventManager;
        private readonly IPortalController portalController;
        private readonly IUserController userController;
        private readonly IEventLogger eventLogger;
        private readonly IHostSettings hostSettings;

        private int roleId = Null.NullInteger;
        private int userId = Null.NullInteger;
        private RoleInfo role;
        private int selectedUserId = Null.NullInteger;
        private UserInfo user;
        private int totalRecords;

        /// <summary>Initializes a new instance of the <see cref="SecurityRoles"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public SecurityRoles()
            : this(null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="SecurityRoles"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventManager">The event manager.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public SecurityRoles(INavigationManager navigationManager, RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger)
            : this(navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, null)
        {
		Logger.LogError(">>> Entering SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger})", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger);
        Logger.LogWarning("<<< Exiting SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger})", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger);
        }

        /// <summary>Initializes a new instance of the <see cref="SecurityRoles"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventManager">The event manager.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        public SecurityRoles(INavigationManager navigationManager, RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings})", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings);
			Logger.LogDebug("SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): roleProvider → {RoleProvider1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, roleProvider);
			Logger.LogDebug("SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): roleController → {RoleController1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, roleController);
			Logger.LogDebug("SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): eventManager → {EventManager1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, eventManager);
			Logger.LogDebug("SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): portalController → {PortalController1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, portalController);
			Logger.LogDebug("SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): userController → {UserController1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, userController);
			Logger.LogDebug("SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): eventLogger → {EventLogger1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, eventLogger);
			Logger.LogDebug("SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings}): hostSettings → {HostSettings1}", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings, hostSettings);
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogWarning("<<< Exiting SecurityRoles(navigationManager={NavigationManager},roleProvider={RoleProvider},roleController={RoleController},eventManager={EventManager},portalController={PortalController},userController={UserController},eventLogger={EventLogger},hostSettings={HostSettings})", navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, hostSettings);
            this.roleProvider = roleProvider ?? this.DependencyProvider.GetRequiredService<RoleProvider>();
            this.roleController = roleController ?? this.DependencyProvider.GetRequiredService<IRoleController>();
            this.eventManager = eventManager ?? this.DependencyProvider.GetRequiredService<IEventManager>();
            this.portalController = portalController ?? this.DependencyProvider.GetRequiredService<IPortalController>();
            this.userController = userController ?? this.DependencyProvider.GetRequiredService<IUserController>();
            this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        /// <inheritdoc />
        public ModuleActionCollection ModuleActions => new();

        /// <summary>Gets or sets the ParentModule (if one exists).</summary>
        public PortalModuleBase ParentModule { get; set; }

        /// <summary>Gets the Return Url for the page.</summary>
        protected string ReturnUrl
        {
            get
            {
                string returnURL;
                var filterParams = new string[string.IsNullOrEmpty(this.Request.QueryString["filterproperty"]) ? 2 : 3];

                if (string.IsNullOrEmpty(this.Request.QueryString["filterProperty"]))
                {
                    filterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                    filterParams.SetValue("currentpage=" + this.Request.QueryString["currentpage"], 1);
                }
                else
                {
                    filterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                    filterParams.SetValue("filterProperty=" + this.Request.QueryString["filterProperty"], 1);
                    filterParams.SetValue("currentpage=" + this.Request.QueryString["currentpage"], 2);
                }

                if (string.IsNullOrEmpty(this.Request.QueryString["filter"]))
                {
                    returnURL = this.navigationManager.NavigateURL(this.TabId);
                }
                else
                {
                    returnURL = this.navigationManager.NavigateURL(this.TabId, string.Empty, filterParams);
                }

                return returnURL;
            }
        }

        protected RoleInfo Role
        {
            get
            {
                if (this.role == null)
                {
                    if (this.roleId != Null.NullInteger)
                    {
                        this.role = this.roleController.GetRole(this.PortalId, r => r.RoleID == this.roleId);
                    }
                    else if (this.cboRoles.SelectedItem != null)
                    {
                        this.role = this.roleController.GetRole(this.PortalId, r => r.RoleID == Convert.ToInt32(this.cboRoles.SelectedItem.Value));
                    }
                }

                return this.role;
            }
        }

        protected UserInfo User
        {
            get
            {
                if (this.user == null)
                {
                    if (this.userId != Null.NullInteger)
                    {
                        this.user = UserController.GetUserById(this.hostSettings, this.PortalId, this.userId);
                    }
                    else if (this.UsersControl == UsersControl.TextBox && !string.IsNullOrEmpty(this.txtUsers.Text))
                    {
                        this.user = UserController.GetUserByName(this.PortalId, this.txtUsers.Text);
                    }
                    else if (this.UsersControl == UsersControl.Combo && (this.cboUsers.SelectedItem != null))
                    {
                        this.user = UserController.GetUserById(this.hostSettings, this.PortalId, Convert.ToInt32(this.cboUsers.SelectedItem.Value));
                    }
                }

                return this.user;
            }
        }

        /// <summary>Gets the control should use a Combo Box or Text Box to display the users.</summary>
        protected UsersControl UsersControl
        {
            get
            {
                var setting = UserModuleBase.GetSetting(this.PortalId, "Security_UsersControl");
                return (UsersControl)setting;
            }
        }

        protected int PageSize
        {
            get
            {
                var setting = UserModuleBase.GetSetting(this.PortalId, "Records_PerPage");
                return Convert.ToInt32(setting);
            }
        }

        protected int SelectedUserID
        {
            get
            {
                return this.selectedUserId;
            }

            set
            {
                this.selectedUserId = value;
            }
        }

        protected int CurrentPage { get; set; }

        /// <summary>DataBind binds the data to the controls.</summary>
        public override void DataBind()
        {
			Logger.LogError(">>> Entering DataBind()");
            if (!ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration))
            {
                this.Response.Redirect(this.navigationManager.NavigateURL("Access Denied"), true);
            }

            base.DataBind();

            // Localize Headers
            Localization.LocalizeDataGrid(ref this.grdUserRoles, this.LocalResourceFile);

            // Bind the role data to the datalist
            this.BindData();

            this.BindGrid();
			Logger.LogWarning("<<< Exiting DataBind()");
        }

        /// <summary>
        /// DeleteButtonVisible returns a boolean indicating if the delete button for
        /// the specified UserID, RoleID pair should be shown.
        /// </summary>
        /// <param name="userID">The ID of the user to check delete button visibility for.</param>
        /// <param name="roleID">The ID of the role to check delete button visibility for.</param>
        /// <returns><see langword="true"/> if the delete button should be shown, <see langword="false"/> to hide the delete button.</returns>
        public bool DeleteButtonVisible(int userID, int roleID)
        {
			Logger.LogError(">>> Entering DeleteButtonVisible(userID={UserID},roleID={RoleID})", userID, roleID);
            // [DNN-4285] Check if the role can be removed (only handles case of Administrator and Administrator Role
            bool canDelete = RoleController.CanRemoveUserFromRole(this.PortalSettings, userID, roleID);
			Logger.LogError("DeleteButtonVisible(userID={UserID},roleID={RoleID}): canDelete → {CanDelete}", userID, roleID, canDelete);
            if (roleID == this.PortalSettings.AdministratorRoleId && canDelete)
            {
                // User can only delete if in Admin role
                canDelete = PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
            }

			Logger.LogWarning("<<< Exiting DeleteButtonVisible(userID={UserID},roleID={RoleID})", userID, roleID);
            return canDelete;
        }

        /// <summary>FormatExpiryDate formats the expiry/effective date and filters out nulls.</summary>
        /// <param name="dateTime">The Date object to format.</param>
        /// <returns>The short date string or <see cref="string.Empty"/>.</returns>
        public string FormatDate(DateTime dateTime)
        {
			Logger.LogError(">>> Entering FormatDate(dateTime={DateTime})", dateTime);
            if (!Null.IsNull(dateTime))
            {
                return dateTime.ToShortDateString();
            }
            else
            {
				Logger.LogWarning("<<< Exiting FormatDate(dateTime={DateTime})", dateTime);
                return string.Empty;
            }
        }

        /// <summary>Generates HTML to link to a user.</summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="displayName">The display name for the link.</param>
        /// <returns>A string with an HTML link to edit the user.</returns>
        public string FormatUser(int userID, string displayName)
        {
			Logger.LogError(">>> Entering FormatUser(userID={UserID},displayName={DisplayName})", userID, displayName);
            return "<a href=\"" + Globals.LinkClick("userid=" + userID, this.TabId, this.ModuleId) + "\" class=\"CommandButton\">" + displayName + "</a>";
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        public void cmdDeleteUserRole_click(object sender, ImageClickEventArgs e)
        {
			Logger.LogError(">>> Entering cmdDeleteUserRole_click(sender={Sender},e={E})", sender, e);
            if (PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName) == false)
            {
                return;
            }

            try
            {
                var cmdDeleteUserRole = (ImageButton)sender;
				Logger.LogError("cmdDeleteUserRole_click(sender={Sender},e={E}): cmdDeleteUserRole → {CmdDeleteUserRole}", sender, e, cmdDeleteUserRole);
                var roleId = Convert.ToInt32(cmdDeleteUserRole.Attributes["roleId"]);
				Logger.LogError("cmdDeleteUserRole_click(sender={Sender},e={E}): roleId → {RoleId}", sender, e, roleId);
                var userId = Convert.ToInt32(cmdDeleteUserRole.Attributes["userId"]);
				Logger.LogError("cmdDeleteUserRole_click(sender={Sender},e={E}): userId → {UserId}", sender, e, userId);

                var role = this.roleController.GetRole(this.PortalId, r => r.RoleID == roleId);
				Logger.LogError("cmdDeleteUserRole_click(sender={Sender},e={E}): role → {Role}", sender, e, role);
                if (!RoleController.DeleteUserRole(this.roleProvider, this.roleController, this.eventManager, this.portalController, this.userController, this.eventLogger, this.userController.GetUserById(this.PortalId, userId), role, this.PortalSettings, this.chkNotify.Checked))
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("RoleRemoveError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }

                this.BindGrid();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in cmdDeleteUserRole_click(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.LogException(exc);
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("RoleRemoveError", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
            }
			Logger.LogWarning("<<< Exiting cmdDeleteUserRole_click(sender={Sender},e={E})", sender, e);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (this.Request.QueryString["RoleId"] != null)
            {
                this.roleId = int.Parse(this.Request.QueryString["RoleId"]);
				Logger.LogDebug("OnInit(e={E}): roleId → {RoleId}", e, roleId);
            }

            if (this.Request.QueryString["UserId"] != null)
            {
                int userId;

                // Use Int32.MaxValue as invalid UserId
                this.userId = int.TryParse(this.Request.QueryString["UserId"], out userId) ? userId : int.MaxValue;
				Logger.LogDebug("OnInit(e={E}): userId → {UserId}", e, userId);
            }

            this.CurrentPage = 1;
            if (this.Request.QueryString["CurrentPage"] != null)
            {
				Logger.LogDebug("OnInit(e={E}): this.CurrentPage → {Arg1}", e, this.CurrentPage);
                var currentPage = 0;
                if (int.TryParse(this.Request.QueryString["CurrentPage"], out currentPage)
                    && currentPage > 0)
                {
                    this.CurrentPage = currentPage;
                }
                else
                {
                    this.CurrentPage = 1;
                }
            }

            this.cboRoles.SelectedIndexChanged += this.CboRoles_SelectedIndexChanged;
            this.cboUsers.SelectedIndexChanged += this.CboUsers_SelectedIndexChanged;
            this.cmdAdd.Click += this.CmdAdd_Click;
            this.cmdValidate.Click += this.CmdValidate_Click;
            this.grdUserRoles.ItemCreated += this.GrdUserRoles_ItemCreated;
            this.grdUserRoles.ItemDataBound += this.grdUserRoles_ItemDataBound;
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            try
            {
                this.cmdCancel.NavigateUrl = this.ReturnUrl;
                if (this.ParentModule == null)
                {
                    this.DataBind();
                }

                if (this.Role == null)
                {
                    return;
                }

                this.placeIsOwner.Visible = (this.Role.SecurityMode == SecurityMode.SocialGroup) || (this.Role.SecurityMode == SecurityMode.Both);
                this.placeIsOwnerHeader.Visible = (this.Role.SecurityMode == SecurityMode.SocialGroup) || (this.Role.SecurityMode == SecurityMode.Both);
            }
            catch (ThreadAbortException exc)
            {
                // Do nothing if ThreadAbort as this is caused by a redirect
                Logger.Debug(exc);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
        protected void grdUserRoles_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
			Logger.LogError(">>> Entering grdUserRoles_ItemDataBound(sender={Sender},e={E})", sender, e);
            DataGridItem item = e.Item;
            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                var userRole = (UserRoleInfo)item.DataItem;
				Logger.LogError("grdUserRoles_ItemDataBound(sender={Sender},e={E}): userRole → {UserRole}", sender, e, userRole);
                if (this.roleId == Null.NullInteger)
                {
                    if (userRole.RoleID == Convert.ToInt32(this.cboRoles.SelectedValue))
                    {
                        this.cmdAdd.Text = Localization.GetString("UpdateRole.Text", this.LocalResourceFile);
                    }
                }

                if (this.userId == Null.NullInteger)
                {
                    if (userRole.UserID == this.SelectedUserID)
                    {
                        this.cmdAdd.Text = Localization.GetString("UpdateRole.Text", this.LocalResourceFile);
                    }
                }
            }
			Logger.LogWarning("<<< Exiting grdUserRoles_ItemDataBound(sender={Sender},e={E})", sender, e);
        }

        /// <summary>BindData loads the controls from the Database.</summary>
        private void BindData()
        {
			Logger.LogError(">>> Entering BindData()");
            // bind all portal roles to dropdownlist
            if (this.roleId == Null.NullInteger)
            {
                if (this.cboRoles.Items.Count == 0)
                {
                    var roles = this.roleController.GetRoles(this.PortalId, x => x.Status == RoleStatus.Approved);
					Logger.LogError("BindData(): roles → {Roles}", roles);

                    // Remove access to Admin Role if use is not a member of the role
                    int roleIndex = Null.NullInteger;
					Logger.LogError("BindData(): roleIndex → {RoleIndex}", roleIndex);
                    foreach (RoleInfo tmpRole in roles)
                    {
                        if (tmpRole.RoleName == this.PortalSettings.AdministratorRoleName)
                        {
                            if (!PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName))
                            {
                                roleIndex = roles.IndexOf(tmpRole);
                            }
                        }

                        break;
                    }

                    if (roleIndex > Null.NullInteger)
                    {
                        roles.RemoveAt(roleIndex);
                    }

                    this.cboRoles.DataSource = roles;
                    this.cboRoles.DataBind();
                }
            }
            else
            {
                if (!this.Page.IsPostBack)
                {
                    if (this.Role != null)
                    {
                        ////cboRoles.Items.Add(new ListItem(Role.RoleName, Role.RoleID.ToString()));
                        this.cboRoles.AddItem(this.Role.RoleName, this.Role.RoleID.ToString());
                        this.cboRoles.Items[0].Selected = true;
                        this.lblTitle.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("RoleTitle.Text", this.LocalResourceFile), this.Role.RoleName, this.Role.RoleID);
                    }

                    this.cboRoles.Visible = false;
                    this.plRoles.Visible = false;
                }
            }

            // bind all portal users to dropdownlist
            if (this.userId == -1)
            {
                // Make sure user has enough permissions
                if (this.Role.RoleName == this.PortalSettings.AdministratorRoleName && !PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NotAuthorized", this.LocalResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
                    this.pnlRoles.Visible = false;
                    this.pnlUserRoles.Visible = false;
                    this.chkNotify.Visible = false;
                    return;
                }

                if (this.UsersControl == UsersControl.Combo)
                {
                    if (this.cboUsers.Items.Count == 0)
                    {
                        foreach (UserInfo objUser in UserController.GetUsers(this.PortalId))
                        {
                            ////cboUsers.Items.Add(new ListItem(objUser.DisplayName + " (" + objUser.Username + ")", objUser.UserID.ToString()));
                            this.cboUsers.AddItem(objUser.DisplayName + " (" + objUser.Username + ")", objUser.UserID.ToString());
                        }
                    }

                    this.txtUsers.Visible = false;
                    this.cboUsers.Visible = true;
                    this.cmdValidate.Visible = false;
                }
                else
                {
                    this.txtUsers.Visible = true;
                    this.cboUsers.Visible = false;
                    this.cmdValidate.Visible = true;
                }
            }
            else
            {
                if (this.User != null)
                {
                    this.txtUsers.Text = this.User.UserID.ToString();
                    this.lblTitle.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("UserTitle.Text", this.LocalResourceFile), this.User.Username, this.User.UserID);
                }

                this.txtUsers.Visible = false;
                this.cboUsers.Visible = false;
                this.cmdValidate.Visible = false;
                this.plUsers.Visible = false;
            }
			Logger.LogWarning("<<< Exiting BindData()");
        }

        /// <summary>BindGrid loads the data grid from the Database.</summary>
        private void BindGrid()
        {
			Logger.LogError(">>> Entering BindGrid()");
            if (this.roleId != Null.NullInteger)
            {
                this.cmdAdd.Text = Localization.GetString("AddUser.Text", this.LocalResourceFile);
                this.grdUserRoles.DataKeyField = "UserId";
                this.grdUserRoles.Columns[2].Visible = false;
            }

            if (this.userId != Null.NullInteger)
            {
                this.cmdAdd.Text = Localization.GetString("AddRole.Text", this.LocalResourceFile);
                this.grdUserRoles.DataKeyField = "RoleId";
                this.grdUserRoles.Columns[1].Visible = false;
            }

            this.grdUserRoles.DataSource = this.GetPagedDataSource();
            this.grdUserRoles.DataBind();

            this.ctlPagingControl.TotalRecords = this.totalRecords;
            this.ctlPagingControl.PageSize = this.PageSize;
            this.ctlPagingControl.CurrentPage = this.CurrentPage;
            this.ctlPagingControl.TabID = this.TabId;
            this.ctlPagingControl.QuerystringParams = System.Web.HttpUtility.UrlDecode(string.Join("&", this.Request.QueryString.ToString().Split('&').
                                                                        ToList().
                                                                        Where(s => s.StartsWith("ctl", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("mid", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("RoleId", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("UserId", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("filter", StringComparison.OrdinalIgnoreCase)
                                                                            || s.StartsWith("popUp", StringComparison.OrdinalIgnoreCase)).ToArray()));
																			Logger.LogWarning("<<< Exiting BindGrid()");
        }

        private IList<UserRoleInfo> GetPagedDataSource()
        {
			Logger.LogError(">>> Entering GetPagedDataSource()");
            var roleName = this.roleId != Null.NullInteger ? this.Role.RoleName : Null.NullString;
            var userName = this.userId != Null.NullInteger ? this.User.Username : Null.NullString;
			Logger.LogError("GetPagedDataSource(): userName → {UserName}", userName);

            var userList = this.roleController.GetUserRoles(this.PortalId, userName, roleName);
			Logger.LogError("GetPagedDataSource(): userList → {UserList}", userList);
            this.totalRecords = userList.Count;
			Logger.LogDebug("GetPagedDataSource(): totalRecords → {TotalRecords}", totalRecords);
			Logger.LogDebug("GetPagedDataSource(): this.totalRecords → {Arg0}", this.totalRecords);

			Logger.LogWarning("<<< Exiting GetPagedDataSource()");
            return userList.Skip((this.CurrentPage - 1) * this.PageSize).Take(this.PageSize).ToList();
        }

        /// <summary>GetDates gets the expiry/effective Dates of a Users Role membership.</summary>
        /// <param name="userId">The ID of the User.</param>
        /// <param name="roleId">The ID of the Role.</param>
        private void GetDates(int userId, int roleId)
        {
			Logger.LogError(">>> Entering GetDates(userId={UserId},roleId={RoleId})", userId, roleId);
            DateTime? expiryDate = null;
            DateTime? effectiveDate = null;

            var userRole = this.roleController.GetUserRole(this.PortalId, userId, roleId);
			Logger.LogError("GetDates(userId={UserId},roleId={RoleId}): userRole → {UserRole}", userId, roleId, userRole);
            if (userRole != null)
            {
                if (Null.IsNull(userRole.EffectiveDate) == false)
                {
                    effectiveDate = userRole.EffectiveDate;
					Logger.LogDebug("GetDates(userId={UserId},roleId={RoleId}): effectiveDate → {EffectiveDate}", userId, roleId, effectiveDate);
                }

                if (Null.IsNull(userRole.ExpiryDate) == false)
                {
                    expiryDate = userRole.ExpiryDate;
					Logger.LogError("GetDates(userId={UserId},roleId={RoleId}): expiryDate → {ExpiryDate}", userId, roleId, expiryDate);
					Logger.LogDebug("GetDates(userId={UserId},roleId={RoleId}): expiryDate → {ExpiryDate}", userId, roleId, expiryDate);
                }
            }
            else
            {
                // new role assignment
                var objRole = this.roleController.GetRole(this.PortalId, r => r.RoleID == roleId);
				Logger.LogError("GetDates(userId={UserId},roleId={RoleId}): objRole → {ObjRole}", userId, roleId, objRole);
                if (objRole.BillingPeriod > 0)
                {
                    switch (objRole.BillingFrequency)
                    {
                        case "D":
                            expiryDate = DateTime.Now.AddDays(objRole.BillingPeriod);
                            break;
                        case "W":
                            expiryDate = DateTime.Now.AddDays(objRole.BillingPeriod * 7);
                            break;
                        case "M":
                            expiryDate = DateTime.Now.AddMonths(objRole.BillingPeriod);
                            break;
                        case "Y":
                            expiryDate = DateTime.Now.AddYears(objRole.BillingPeriod);
                            break;
                    }
                }
            }

            this.effectiveDatePicker.Text = effectiveDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            this.expiryDatePicker.Text = expiryDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
			Logger.LogWarning("<<< Exiting GetDates(userId={UserId},roleId={RoleId})", userId, roleId);
        }

        private void CboUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering CboUsers_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            if ((this.cboUsers.SelectedItem != null) && (this.cboRoles.SelectedItem != null))
            {
                this.SelectedUserID = int.Parse(this.cboUsers.SelectedItem.Value);
                this.GetDates(this.SelectedUserID, int.Parse(this.cboRoles.SelectedItem.Value));
            }

            this.BindGrid();
			Logger.LogWarning("<<< Exiting CboUsers_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        private void CmdValidate_Click(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering CmdValidate_Click(sender={Sender},e={E})", sender, e);
            if (PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName) == false)
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.txtUsers.Text))
            {
                // validate username
                UserInfo objUser = UserController.GetUserByName(this.PortalId, this.txtUsers.Text);
				Logger.LogError("CmdValidate_Click(sender={Sender},e={E}): objUser → {ObjUser}", sender, e, objUser);
                if (objUser != null)
                {
                    this.GetDates(objUser.UserID, this.roleId);
                    this.SelectedUserID = objUser.UserID;
                }
                else
                {
					Logger.LogDebug("CmdValidate_Click(sender={Sender},e={E}): this.SelectedUserID → {Arg2}", sender, e, this.SelectedUserID);
                    this.txtUsers.Text = string.Empty;
                }
            }

            this.BindGrid();
			Logger.LogWarning("<<< Exiting CmdValidate_Click(sender={Sender},e={E})", sender, e);
        }

        private void CboRoles_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering CboRoles_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.GetDates(this.userId, int.Parse(this.cboRoles.SelectedItem.Value));
            this.BindGrid();
			Logger.LogWarning("<<< Exiting CboRoles_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        private void CmdAdd_Click(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering CmdAdd_Click(sender={Sender},e={E})", sender, e);
            if (PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName) == false)
            {
                return;
            }

            try
            {
                if (this.Page.IsValid)
                {
                    if ((this.Role != null) && (this.User != null))
                    {
                        // do not modify the portal Administrator account dates
                        if (this.User.UserID == this.PortalSettings.AdministratorId && this.Role.RoleID == this.PortalSettings.AdministratorRoleId)
                        {
                            this.effectiveDatePicker.Text = null;
                            this.expiryDatePicker.Text = null;
                        }

                        if (!DateTime.TryParseExact(this.effectiveDatePicker.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var datEffectiveDate))
                        {
                            datEffectiveDate = Null.NullDate;
							Logger.LogDebug("CmdAdd_Click(sender={Sender},e={E}): datEffectiveDate → {DatEffectiveDate}", sender, e, datEffectiveDate);
                        }

                        if (!DateTime.TryParseExact(this.expiryDatePicker.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var datExpiryDate))
                        {
                            datExpiryDate = Null.NullDate;
							Logger.LogDebug("CmdAdd_Click(sender={Sender},e={E}): datExpiryDate → {DatExpiryDate}", sender, e, datExpiryDate);
                        }

                        // Add User to Role
                        var isOwner = false;
						Logger.LogError("CmdAdd_Click(sender={Sender},e={E}): isOwner → {IsOwner}", sender, e, isOwner);
                        if (this.Role.SecurityMode is SecurityMode.SocialGroup or SecurityMode.Both)
                        {
                            isOwner = this.chkIsOwner.Checked;
                        }

                        RoleController.AddUserRole(this.roleController, this.userController, this.eventLogger, this.User, this.Role, this.PortalSettings, RoleStatus.Approved, datEffectiveDate, datExpiryDate, this.chkNotify.Checked, isOwner);
                        this.chkIsOwner.Checked = false; // reset the checkbox
                    }
                }

                this.BindGrid();
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdAdd_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting CmdAdd_Click(sender={Sender},e={E})", sender, e);
        }

        private void GrdUserRoles_ItemCreated(object sender, DataGridItemEventArgs e)
        {
			Logger.LogError(">>> Entering GrdUserRoles_ItemCreated(sender={Sender},e={E})", sender, e);
            try
            {
                if (e.Item.FindControl("cmdDeleteUserRole") is ImageButton cmdDeleteUserRole)
                {
                    var userRoleInfo = (UserRoleInfo)e.Item.DataItem;
					Logger.LogError("GrdUserRoles_ItemCreated(sender={Sender},e={E}): userRoleInfo → {UserRoleInfo}", sender, e, userRoleInfo);
                    if (this.roleId == Null.NullInteger)
                    {
                        ClientAPI.AddButtonConfirm(cmdDeleteUserRole, string.Format(CultureInfo.CurrentCulture, Localization.GetString("DeleteRoleFromUser.Text", this.LocalResourceFile), userRoleInfo.FullName, userRoleInfo.RoleName));
                    }
                    else
                    {
                        ClientAPI.AddButtonConfirm(cmdDeleteUserRole, string.Format(CultureInfo.CurrentCulture, Localization.GetString("DeleteUsersFromRole.Text", this.LocalResourceFile), userRoleInfo.FullName, userRoleInfo.RoleName));
                    }

                    cmdDeleteUserRole.Attributes.Add("roleId", userRoleInfo.RoleID.ToString());
                    cmdDeleteUserRole.Attributes.Add("userId", userRoleInfo.UserID.ToString());
                }

                e.Item.Cells[5].Visible = this.Role.SecurityMode is SecurityMode.SocialGroup or SecurityMode.Both;
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in GrdUserRoles_ItemCreated(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting GrdUserRoles_ItemCreated(sender={Sender},e={E})", sender, e);
        }
    }
}
