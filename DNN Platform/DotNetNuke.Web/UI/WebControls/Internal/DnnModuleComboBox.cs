// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Installer.Packages;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>This control is only for internal use, please don't reference it in any other place as it may be removed in the future.</summary>
    /// <param name="appStatus">The application status.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="clientResourceController">The client resource controller.</param>
    /// <param name="hostSettings">The host settings.</param>
    public class DnnModuleComboBox(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController, IHostSettings hostSettings)
        : DnnComboBox(
            appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
            eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
            clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>())
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnModuleComboBox));
        private const string DefaultExtensionImage = "icon_extensions_32px.png";
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        private DnnComboBox moduleCombo;
        private string originalValue;

        /// <summary>Initializes a new instance of the <see cref="DnnModuleComboBox"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnModuleComboBox()
            : this(null, null, null)
        {
		Logger.LogError(">>> Entering DnnModuleComboBox()");
        Logger.LogWarning("<<< Exiting DnnModuleComboBox()");
        }

        /// <summary>Initializes a new instance of the <see cref="DnnModuleComboBox"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="clientResourceController">The client resource controller.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DnnModuleComboBox(IApplicationStatusInfo appStatus, IEventLogger eventLogger, IClientResourceController clientResourceController)
            : this(appStatus, eventLogger, clientResourceController, null)
        {
        }

        /// <summary>An event which triggers when the item changes.</summary>
        public event EventHandler ItemChanged;

        /// <summary>Gets the item count.</summary>
        public int ItemCount => this.moduleCombo.Items.Count;

        /// <summary>Gets the client ID of the <see cref="DnnComboBox"/>.</summary>
        public string RadComboBoxClientId => this.moduleCombo.ClientID;

        /// <summary>Gets or sets the filter.</summary>
        public Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool> Filter { get; set; }

        /// <inheritdoc />
        public override string SelectedValue => this.moduleCombo.SelectedValue;

        /// <inheritdoc />
        public override bool Enabled
        {
            get => this.moduleCombo.Enabled;
            set => this.moduleCombo.Enabled = value;
        }

        /// <summary>Binds the portal desktop modules to the list.</summary>
        public void BindAllPortalDesktopModules()
        {
			Logger.LogError(">>> Entering BindAllPortalDesktopModules()");
            this.moduleCombo.SelectedValue = null;
            this.moduleCombo.DataSource = this.GetPortalDesktopModules();
            this.moduleCombo.DataBind();
            BindPortalDesktopModuleImages(this.hostSettings);
			Logger.LogWarning("<<< Exiting BindAllPortalDesktopModules()");
        }

        /// <summary>Binds the modules from the page to the list.</summary>
        /// <param name="tabID">The tab ID.</param>
        public void BindTabModulesByTabID(int tabID)
        {
			Logger.LogError(">>> Entering BindTabModulesByTabID(tabID={TabID})", tabID);
            this.moduleCombo.SelectedValue = null;
            this.moduleCombo.DataSource = GetTabModules(tabID);
            this.moduleCombo.DataBind();
            BindTabModuleImages(this.hostSettings, tabID);
			Logger.LogWarning("<<< Exiting BindTabModulesByTabID(tabID={TabID})", tabID);
        }

        /// <summary>Sets the module.</summary>
        /// <param name="code">The item's value.</param>
        public void SetModule(string code)
        {
			Logger.LogError(">>> Entering SetModule(code={Code})", code);
            this.moduleCombo.SelectedIndex = this.moduleCombo.FindItemIndexByValue(code);
			Logger.LogWarning("<<< Exiting SetModule(code={Code})", code);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.moduleCombo = new DnnComboBox(this.AppStatus, this.EventLogger, this.ClientResourceController);
			Logger.LogDebug("OnInit(e={E}): moduleCombo → {ModuleCombo}", e, moduleCombo);
            this.moduleCombo.DataValueField = "key";
            this.moduleCombo.DataTextField = "value";
            this.Controls.Add(this.moduleCombo);
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            this.originalValue = this.SelectedValue;
        }

        /// <summary>A method which triggers the <see cref="ItemChanged"/> event.</summary>
        protected virtual void OnItemChanged()
        {
			Logger.LogError(">>> Entering OnItemChanged()");
            if (this.ItemChanged != null)
            {
                this.ItemChanged(this, new EventArgs());
            }
			Logger.LogWarning("<<< Exiting OnItemChanged()");
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            if (this.moduleCombo.FindItemByValue(this.originalValue) != null)
            {
                this.moduleCombo.FindItemByValue(this.originalValue).Selected = true;
            }

            this.moduleCombo.Width = this.Width;
            base.OnPreRender(e);
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        private static Dictionary<int, string> GetTabModules(int tabID)
        {
            var tabModules = ModuleController.Instance.GetTabModules(tabID);

            // Is this tab from another site?
            var isRemote = TabController.Instance.GetTab(tabID, Null.NullInteger, false).PortalID != PortalSettings.Current.PortalId;

            var pageModules = tabModules.Values.Where(m => !isRemote || ModuleSuportsSharing(m)).Where(m => ModulePermissionController.CanAdminModule(m) && m.IsDeleted == false).ToList();

            return pageModules.ToDictionary(module => module.ModuleID, module => module.ModuleTitle);
        }

        private static bool ModuleSuportsSharing(ModuleInfo moduleInfo)
        {
			Logger.LogError(">>> Entering ModuleSuportsSharing(moduleInfo={ModuleInfo})", moduleInfo);
            switch (moduleInfo.DesktopModule.Shareable)
            {
                case ModuleSharing.Supported:
                case ModuleSharing.Unknown:
                    return moduleInfo.IsShareable;
                default:
					Logger.LogWarning("<<< Exiting ModuleSuportsSharing(moduleInfo={ModuleInfo})", moduleInfo);
                    return false;
            }
        }

        private static void BindPortalDesktopModuleImages(IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering BindPortalDesktopModuleImages(hostSettings={HostSettings})", hostSettings);
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(hostSettings, PortalSettings.Current.PortalId);
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);
			Logger.LogError("BindPortalDesktopModuleImages(hostSettings={HostSettings}): packages → {Packages}", hostSettings, packages);
			Logger.LogError("BindPortalDesktopModuleImages(hostSettings={HostSettings}): portalDesktopModules → {PortalDesktopModules}", hostSettings, portalDesktopModules);

            ////foreach (var item in _moduleCombo.Items)
            ////{
            ////   string imageUrl =
            ////       (from pkgs in packages
            ////        join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
            ////        where portMods.Value.DesktopModuleID.ToString() == item.Value
            ////        select pkgs.IconFile).FirstOrDefault();
            ////
            ////item.ImageUrl = String.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            ////}
			Logger.LogWarning("<<< Exiting BindPortalDesktopModuleImages(hostSettings={HostSettings})", hostSettings);
        }

        private static void BindTabModuleImages(IHostSettings hostSettings, int tabId)
        {
			Logger.LogError(">>> Entering BindTabModuleImages(hostSettings={HostSettings},tabId={TabId})", hostSettings, tabId);
            var tabModules = ModuleController.Instance.GetTabModules(tabId);
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(hostSettings, PortalSettings.Current.PortalId);
			Logger.LogError("BindTabModuleImages(hostSettings={HostSettings},tabId={TabId}): portalDesktopModules → {PortalDesktopModules}", hostSettings, tabId, portalDesktopModules);
            var moduleDefinitions = ModuleDefinitionController.GetModuleDefinitions(hostSettings);
			Logger.LogError("BindTabModuleImages(hostSettings={HostSettings},tabId={TabId}): moduleDefinitions → {ModuleDefinitions}", hostSettings, tabId, moduleDefinitions);
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.Current.PortalId);
			Logger.LogError("BindTabModuleImages(hostSettings={HostSettings},tabId={TabId}): packages → {Packages}", hostSettings, tabId, packages);
			Logger.LogError("BindTabModuleImages(hostSettings={HostSettings},tabId={TabId}): tabModules → {TabModules}", hostSettings, tabId, tabModules);

            ////foreach (RadComboBoxItem item in _moduleCombo.Items)
            ////{
            ////   string imageUrl = (from pkgs in packages
            ////                      join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
            ////                      join modDefs in moduleDefinitions on portMods.Value.DesktopModuleID equals modDefs.Value.DesktopModuleID
            ////                      join tabMods in tabModules on modDefs.Value.DesktopModuleID equals tabMods.Value.DesktopModuleID
            ////                      where tabMods.Value.ModuleID.ToString() == item.Value
            ////                      select pkgs.IconFile).FirstOrDefault();
            ////
            ////item.ImageUrl = String.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            ////}
			Logger.LogWarning("<<< Exiting BindTabModuleImages(hostSettings={HostSettings},tabId={TabId})", hostSettings, tabId);
        }

        private Dictionary<int, string> GetPortalDesktopModules()
        {
            IOrderedEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> portalModulesList;
            if (this.Filter == null)
            {
                portalModulesList = DesktopModuleController.GetPortalDesktopModules(this.hostSettings, PortalSettings.Current.PortalId)
                    .Where((kvp) => kvp.Value.DesktopModule.Category == "Uncategorised" || string.IsNullOrEmpty(kvp.Value.DesktopModule.Category))
                    .OrderBy(c => c.Key);
            }
            else
            {
                portalModulesList = DesktopModuleController.GetPortalDesktopModules(this.hostSettings, PortalSettings.Current.PortalId)
                    .Where(this.Filter)
                    .OrderBy(c => c.Key);
            }

            return portalModulesList.ToDictionary(
                portalModule => portalModule.Value.DesktopModuleID,
                portalModule => portalModule.Key);
        }
    }
}
