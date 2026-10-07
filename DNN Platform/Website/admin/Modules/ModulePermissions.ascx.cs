// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Admin.Modules

// ReSharper restore CheckNamespace
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// The ModuleSettingsPage PortalModuleBase is used to edit the settings for a
    /// module.
    /// </summary>
    public partial class ModulePermissions : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModulePermissions.ascx));
        private readonly INavigationManager navigationManager;

        private int moduleId = -1;
        private ModuleInfo module;

        /// <summary>Initializes a new instance of the <see cref="ModulePermissions"/> class.</summary>
        public ModulePermissions()
        {
			Logger.LogError(">>> Entering ModulePermissions()");
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogDebug("ModulePermissions(): navigationManager → {NavigationManager}", navigationManager);
        Logger.LogWarning("<<< Exiting ModulePermissions()");
        }

        private ModuleInfo Module
        {
            get { return this.module ?? (this.module = ModuleController.Instance.GetModule(this.moduleId, this.TabId, false)); }
        }

        private string ReturnURL
        {
            get
            {
                return UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this.navigationManager.NavigateURL();
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            // get ModuleId
            if (this.Request.QueryString["ModuleId"] != null)
            {
                this.moduleId = int.Parse(this.Request.QueryString["ModuleId"]);
				Logger.LogDebug("OnInit(e={E}): moduleId → {ModuleId}", e, moduleId);
            }

            // Verify that the current user has access to edit this module
            if (!ModulePermissionController.HasModuleAccess(SecurityAccessLevel.ViewPermissions, string.Empty, this.Module))
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdUpdate.Click += this.OnUpdateClick;

            try
            {
                this.cancelHyperLink.NavigateUrl = this.ReturnURL;

                if (this.Page.IsPostBack == false)
                {
                    this.dgPermissions.TabId = this.PortalSettings.ActiveTab.TabID;
                    this.dgPermissions.ModuleID = this.moduleId;

                    if (this.Module != null)
                    {
                        this.cmdUpdate.Visible = ModulePermissionController.HasModulePermission(this.Module.ModulePermissions, "EDIT,MANAGE") || TabPermissionController.CanAddContentToPage();
                        this.permissionsRow.Visible = ModulePermissionController.CanAdminModule(this.Module) || TabPermissionController.CanAddContentToPage();
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering OnUpdateClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (this.Page.IsValid)
                {
                    this.Module.ModulePermissions.Clear();
                    this.Module.ModulePermissions.AddRange(this.dgPermissions.Permissions);

                    ModulePermissionController.SaveModulePermissions(this.Module);

                    // Navigate back to admin page
                    this.Response.Redirect(this.ReturnURL, true);
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnUpdateClick(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogWarning("<<< Exiting OnUpdateClick(sender={Sender},e={E})", sender, e);
        }
    }
}
