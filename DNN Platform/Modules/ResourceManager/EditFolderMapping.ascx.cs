// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager
{
    using System;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides editing capabilities for a folder mapping.</summary>
    public partial class EditFolderMapping : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditFolderMapping.ascx));
        private readonly INavigationManager navigationManager;
        private readonly IFolderMappingController folderMappingController = FolderMappingController.Instance;
        private int folderMappingID = Null.NullInteger;

        /// <summary>Initializes a new instance of the <see cref="EditFolderMapping"/> class.</summary>
        public EditFolderMapping()
        {
			Logger.LogDebug(">>> Entering EditFolderMapping()");
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogDebug("EditFolderMapping(): navigationManager → {NavigationManager}", navigationManager);
			Logger.LogDebug("<<< Exiting EditFolderMapping()");
        }

        /// <summary>Gets the portalId for the folder.</summary>
        public int FolderPortalID
        {
            get
            {
                return this.IsHostMenu ? Null.NullInteger : this.PortalId;
            }
        }

        /// <summary>Gets the id of the folder mapping.</summary>
        public int FolderMappingID
        {
            get
            {
                if (this.folderMappingID == Null.NullInteger)
                {
                    if (!string.IsNullOrEmpty(this.Request.QueryString["ItemID"]))
                    {
                        int.TryParse(this.Request.QueryString["ItemID"], out this.folderMappingID);
                    }
                }

                return this.folderMappingID;
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            if (!this.UserInfo.IsSuperUser && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }

            this.UpdateButton.Text = (this.FolderMappingID == Null.NullInteger) ? Localization.GetString("Add") : Localization.GetString("Update", this.LocalResourceFile);
            this.CancelHyperLink.NavigateUrl = this.navigationManager.NavigateURL();

            var controlTitle = Localization.GetString("ControlTitle", this.LocalResourceFile);
			Logger.LogDebug("OnInit(e={E}): controlTitle → {ControlTitle}", e, controlTitle);
            var controlTitlePrefix = (this.FolderMappingID == Null.NullInteger) ? Localization.GetString("New") : Localization.GetString("Edit");
			Logger.LogDebug("OnInit(e={E}): controlTitlePrefix → {ControlTitlePrefix}", e, controlTitlePrefix);

            this.SyncWarningPlaceHolder.Visible = this.FolderMappingID != Null.NullInteger;

            this.ModuleConfiguration.ModuleControl.ControlTitle = string.Format(controlTitle, controlTitlePrefix);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.UpdateButton.Click += this.CmdUpdate_Click;

            try
            {
                this.BindFolderMappingSettings();

                if (!this.IsPostBack)
                {
                    this.BindFolderProviders();

                    if (this.FolderMappingID != Null.NullInteger)
                    {
                        this.BindFolderMapping();

                        if (this.ProviderSettingsPlaceHolder.Controls.Count > 0 && this.ProviderSettingsPlaceHolder.Controls[0] is FolderMappingSettingsControlBase)
                        {
                            var folderMapping = this.folderMappingController.GetFolderMapping(this.FolderMappingID);
							Logger.LogDebug("OnLoad(e={E}): folderMapping → {FolderMapping}", e, folderMapping);
                            var settingsControl = (FolderMappingSettingsControlBase)this.ProviderSettingsPlaceHolder.Controls[0];
							Logger.LogDebug("OnLoad(e={E}): settingsControl → {SettingsControl}", e, settingsControl);
                            settingsControl.LoadSettings(folderMapping.FolderMappingSettings);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, exc.getMessage());
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <summary>Handles the folder providers selection change.</summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        protected void CboFolderProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CboFolderProviders_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            this.BindFolderMappingSettings();
			Logger.LogDebug("<<< Exiting CboFolderProviders_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
        }

        private void CmdUpdate_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering CmdUpdate_Click(sender={Sender},e={E})", sender, e);
            this.Page.Validate("vgEditFolderMapping");

            if (!this.Page.IsValid)
            {
                return;
            }

            try
            {
                var folderMapping = new FolderMappingInfo();
				Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): folderMapping → {FolderMapping}", sender, e, folderMapping);

                if (this.FolderMappingID != Null.NullInteger)
                {
                    folderMapping = this.folderMappingController.GetFolderMapping(this.FolderMappingID) ?? new FolderMappingInfo();
                }

                folderMapping.FolderMappingID = this.FolderMappingID;
				Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): folderMapping.FolderMappingID → {Arg2}", sender, e, folderMapping.FolderMappingID);
				Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): folderMapping.MappingName → {Arg2}", sender, e, folderMapping.MappingName);
				Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): folderMapping.FolderProviderType → {Arg2}", sender, e, folderMapping.FolderProviderType);
				Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): folderMapping.PortalID → {Arg2}", sender, e, folderMapping.PortalID);
                folderMapping.MappingName = this.NameTextbox.Text;
                folderMapping.FolderProviderType = this.FolderProvidersComboBox.SelectedValue;
                folderMapping.PortalID = this.FolderPortalID;

                var originalSettings = folderMapping.FolderMappingSettings;

                try
                {
                    var folderMappingID = this.FolderMappingID;
					Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): folderMappingID → {FolderMappingID}", sender, e, folderMappingID);

                    if (folderMappingID == Null.NullInteger)
                    {
                        folderMappingID = this.folderMappingController.AddFolderMapping(folderMapping);
                    }
                    else
                    {
                        this.folderMappingController.UpdateFolderMapping(folderMapping);
                    }

                    if (this.ProviderSettingsPlaceHolder.Controls.Count > 0 && this.ProviderSettingsPlaceHolder.Controls[0] is FolderMappingSettingsControlBase)
                    {
                        var settingsControl = (FolderMappingSettingsControlBase)this.ProviderSettingsPlaceHolder.Controls[0];
						Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): settingsControl → {SettingsControl}", sender, e, settingsControl);

                        try
                        {
                            settingsControl.UpdateSettings(folderMappingID);
                        }
                        catch
                        {
							Logger.LogError("Exception in CmdUpdate_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.Message);
                            if (this.FolderMappingID == Null.NullInteger)
                            {
                                this.folderMappingController.DeleteFolderMapping(this.FolderPortalID, folderMappingID);
                            }

                            return;
                        }
                    }

                    if (this.FolderMappingID != Null.NullInteger)
                    {
                        // Check if some setting has changed
                        var updatedSettings = this.folderMappingController.GetFolderMappingSettings(this.FolderMappingID);
						Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): updatedSettings → {UpdatedSettings}", sender, e, updatedSettings);

                        if (originalSettings.Keys.Cast<object>().Any(key => updatedSettings.ContainsKey(key) && !originalSettings[key].ToString().Equals(updatedSettings[key].ToString())))
                        {
                            // Re-synchronize folders using the existing mapping. It's important to synchronize them in descending order
                            var folders = FolderManager.Instance.GetFolders(this.FolderPortalID).Where(f => f.FolderMappingID == this.FolderMappingID).OrderByDescending(f => f.FolderPath);
							Logger.LogDebug("CmdUpdate_Click(sender={Sender},e={E}): folders → {Folders}", sender, e, folders);

                            foreach (var folder in folders)
                            {
                                FolderManager.Instance.Synchronize(this.FolderPortalID, folder.FolderPath, false, true);
                            }
                        }
                    }
                }
                catch
                {
                    DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("DuplicateMappingName", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    return;
                }

                if (!this.Response.IsRequestBeingRedirected)
                {
                    this.Response.Redirect(this.navigationManager.NavigateURL());
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in CmdUpdate_Click(sender={Sender},e={E}): {Arg2}", sender, e, exc.getMessage());
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting CmdUpdate_Click(sender={Sender},e={E})", sender, e);
        }

        private void BindFolderProviders()
        {
			Logger.LogDebug(">>> Entering BindFolderProviders()");
            var defaultProviders = DefaultFolderProviders.GetDefaultProviders();

			Logger.LogDebug("BindFolderProviders(): defaultProviders → {DefaultProviders}", defaultProviders);
            foreach (var provider in FolderProvider.GetProviderList().Keys.Where(provider => !defaultProviders.Contains(provider)).OrderBy(provider => provider))
            {
                this.FolderProvidersComboBox.Items.Add(new ListItem(provider, provider));
            }

            this.FolderProvidersComboBox.Items.Insert(0, new ListItem(string.Empty, string.Empty));
			Logger.LogDebug("<<< Exiting BindFolderProviders()");
        }

        private void BindFolderMapping()
        {
			Logger.LogDebug(">>> Entering BindFolderMapping()");
            var folderMapping = this.folderMappingController.GetFolderMapping(this.FolderMappingID);

			Logger.LogDebug("BindFolderMapping(): folderMapping → {FolderMapping}", folderMapping);
            this.NameTextbox.Text = folderMapping.MappingName;

            this.FolderProvidersComboBox.SelectedValue = folderMapping.FolderProviderType;
            this.FolderProvidersComboBox.Enabled = false;
			Logger.LogDebug("<<< Exiting BindFolderMapping()");
        }

        private void BindFolderMappingSettings()
        {
			Logger.LogDebug(">>> Entering BindFolderMappingSettings()");
            string folderProviderType;

            if (this.FolderMappingID != Null.NullInteger)
            {
                var folderMapping = this.folderMappingController.GetFolderMapping(this.FolderMappingID);
				Logger.LogDebug("BindFolderMappingSettings(): folderMapping → {FolderMapping}", folderMapping);
                folderProviderType = folderMapping.FolderProviderType;
            }
            else
            {
                folderProviderType = this.FolderProvidersComboBox.SelectedValue;
            }

            if (string.IsNullOrEmpty(folderProviderType))
            {
                return;
            }

            var settingsControlVirtualPath = FolderProvider.Instance(folderProviderType).GetSettingsControlVirtualPath();
			Logger.LogDebug("BindFolderMappingSettings(): settingsControlVirtualPath → {SettingsControlVirtualPath}", settingsControlVirtualPath);
            if (string.IsNullOrEmpty(settingsControlVirtualPath))
            {
                return;
            }

            var settingsControl = this.LoadControl(settingsControlVirtualPath);
			Logger.LogDebug("BindFolderMappingSettings(): settingsControl → {SettingsControl}", settingsControl);
            if (settingsControl == null || !(settingsControl is FolderMappingSettingsControlBase))
            {
                return;
            }

            // This is important to allow settings control to be localizable
            var baseType = settingsControl.GetType().BaseType;
			Logger.LogDebug("BindFolderMappingSettings(): baseType → {BaseType}", baseType);
            if (baseType != null)
            {
                settingsControl.ID = baseType.Name;
				Logger.LogDebug("BindFolderMappingSettings(): settingsControl.ID → {Arg0}", settingsControl.ID);
            }

            this.ProviderSettingsPlaceHolder.Controls.Clear();
            this.ProviderSettingsPlaceHolder.Controls.Add(settingsControl);
			Logger.LogDebug("<<< Exiting BindFolderMappingSettings()");
        }
    }
}
