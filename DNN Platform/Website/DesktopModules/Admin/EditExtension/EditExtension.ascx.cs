// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.EditExtension
{
    using System;
    using System.IO;
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Installer.Writers;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The EditExtension control is used to edit an Extension.</summary>
    public partial class EditExtension : ModuleUserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IServiceProvider serviceProvider;
        private readonly INavigationManager navigationManager;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IJavaScriptLibraryHelper javaScript;
        private readonly IHostSettings hostSettings;

        private Control control;
        private PackageInfo package;

        /// <summary>Initializes a new instance of the <see cref="EditExtension"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public EditExtension()
            : this(null, null, null, null, null)
        {
		Logger.LogDebug(">>> Entering EditExtension()");
        }
		Logger.LogDebug("<<< Exiting EditExtension()");

        /// <summary>Initializes a new instance of the <see cref="EditExtension"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public EditExtension(INavigationManager navigationManager, IApplicationStatusInfo appStatus, IJavaScriptLibraryHelper javaScript)
            : this(navigationManager, appStatus, javaScript, null, null)
        {
		Logger.LogDebug(">>> Entering EditExtension(navigationManager={NavigationManager},appStatus={AppStatus},javaScript={JavaScript})", navigationManager, appStatus, javaScript);
		Logger.LogDebug("<<< Exiting EditExtension(navigationManager={NavigationManager},appStatus={AppStatus},javaScript={JavaScript})", navigationManager, appStatus, javaScript);
		Logger.LogDebug("EditExtension(navigationManager={NavigationManager},appStatus={AppStatus},javaScript={JavaScript},hostSettings={HostSettings},serviceProvider={ServiceProvider}): navigationManager → {NavigationManager1}", navigationManager, appStatus, javaScript, hostSettings, serviceProvider, navigationManager);
		Logger.LogDebug("EditExtension(navigationManager={NavigationManager},appStatus={AppStatus},javaScript={JavaScript},hostSettings={HostSettings},serviceProvider={ServiceProvider}): appStatus → {AppStatus1}", navigationManager, appStatus, javaScript, hostSettings, serviceProvider, appStatus);
		Logger.LogDebug("EditExtension(navigationManager={NavigationManager},appStatus={AppStatus},javaScript={JavaScript},hostSettings={HostSettings},serviceProvider={ServiceProvider}): javaScript → {JavaScript1}", navigationManager, appStatus, javaScript, hostSettings, serviceProvider, javaScript);
        }

        /// <summary>Initializes a new instance of the <see cref="EditExtension"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="hostSettings">The host settings.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public EditExtension(INavigationManager navigationManager, IApplicationStatusInfo appStatus, IJavaScriptLibraryHelper javaScript, IHostSettings hostSettings)
            : this(navigationManager, appStatus, javaScript, hostSettings, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EditExtension"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="javaScript">The JavaScript library helper.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="serviceProvider">The dependency injection container.</param>
        public EditExtension(INavigationManager navigationManager, IApplicationStatusInfo appStatus, IJavaScriptLibraryHelper javaScript, IHostSettings hostSettings, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider ?? Globals.GetCurrentServiceProvider();
            this.navigationManager = navigationManager ?? this.serviceProvider.GetRequiredService<INavigationManager>();
			Logger.LogDebug("EditExtension(navigationManager={NavigationManager},appStatus={AppStatus},javaScript={JavaScript},hostSettings={HostSettings},serviceProvider={ServiceProvider}): hostSettings → {HostSettings1}", navigationManager, appStatus, javaScript, hostSettings, serviceProvider, hostSettings);
			Logger.LogDebug("EditExtension(navigationManager={NavigationManager},appStatus={AppStatus},javaScript={JavaScript},hostSettings={HostSettings},serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", navigationManager, appStatus, javaScript, hostSettings, serviceProvider, serviceProvider);
            this.appStatus = appStatus ?? this.serviceProvider.GetRequiredService<IApplicationStatusInfo>();
            this.javaScript = javaScript ?? this.serviceProvider.GetRequiredService<IJavaScriptLibraryHelper>();
            this.hostSettings = hostSettings ?? this.serviceProvider.GetRequiredService<IHostSettings>();
        }

        public string Mode => Convert.ToString(this.ModuleContext.Settings["Extensions_Mode"]);

        public int PackageID
        {
            get
            {
                var packageID = Null.NullInteger;
                if (this.Request.QueryString["PackageID"] != null)
                {
                    packageID = int.Parse(this.Request.QueryString["PackageID"]);
                }

                return packageID;
            }
        }

        protected bool IsSuperTab => this.ModuleContext.PortalSettings.ActiveTab.IsSuperTab;

        protected string DisplayMode => (this.Request.QueryString["Display"] ?? string.Empty).ToLowerInvariant();

        protected PackageInfo Package => this.package ??= this.PackageID == Null.NullInteger ? new PackageInfo() : PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this.PackageID, true);

        protected IPackageEditor PackageEditor
        {
            get
            {
                if (this.control == null)
                {
                    if (this.Package != null)
                    {
                        var pkgType = PackageController.Instance.GetExtensionPackageType(t => t.PackageType.Equals(this.Package.PackageType, StringComparison.OrdinalIgnoreCase));
                        if ((pkgType != null) && (!string.IsNullOrEmpty(pkgType.EditorControlSrc)))
                        {
                            this.control = ControlUtilities.LoadControl<Control>(this, pkgType.EditorControlSrc);
                        }
                    }
                }

                return this.control as IPackageEditor;
            }
        }

        protected PropertyEditorMode ViewMode
        {
            get
            {
                var viewMode = PropertyEditorMode.View;
                if (this.Request.IsLocal && this.IsSuperTab)
                {
                    viewMode = PropertyEditorMode.Edit;
                }

                return viewMode;
            }
        }

        protected string ReturnUrl
        {
            get => (string)this.ViewState["ReturnUrl"];
            set => this.ViewState["ReturnUrl"] = value;
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.javaScript.RequestRegistration(CommonJs.DnnPlugins);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cmdCancel.Click += this.OnCancelClick;
            this.cmdDelete.Click += this.OnDeleteClick;
            this.cmdPackage.Click += this.OnPackageClick;
            this.cmdUpdate.Click += this.OnUpdateClick;
            this.Page.PreRenderComplete += (_, _) =>
            {
                if (UrlUtils.InPopUp())
                {
                    var title = $"{this.Page.Title} > {this.Package.FriendlyName}";
                    this.Page.Title = title;
					Logger.LogDebug("OnLoad(e={E}): title → {Title}", e, title);
                }
            };

            this.BindData();

            if (!this.IsPostBack)
            {
                this.ReturnUrl = this.Request.UrlReferrer != null ? this.Request.UrlReferrer.ToString() : this.navigationManager.NavigateURL();
                switch (this.DisplayMode)
                {
                    case "editor":
                        this.packageSettingsSection.Visible = false;
                        break;
                    case "settings":
                        this.extensionSection.Visible = false;
                        break;
                }
            }

            switch (this.DisplayMode)
            {
                case "editor":
                    this.cmdCancel.Visible = this.cmdCancel.Enabled = false;
                    this.cmdUpdate.Visible = this.cmdUpdate.Enabled = false;
                    this.cmdPackage.Visible = this.cmdPackage.Enabled = false;
                    this.cmdDelete.Visible = this.cmdDelete.Enabled = false;
                    break;
                case "settings":
                    this.cmdCancel.Visible = this.cmdCancel.Enabled = false;
                    break;
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnCancelClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnCancelClick(sender={Sender},e={E})", sender, e);
            this.Response.Redirect(this.ReturnUrl);
			Logger.LogDebug("<<< Exiting OnCancelClick(sender={Sender},e={E})", sender, e);
        }

        protected void OnDeleteClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnDeleteClick(sender={Sender},e={E})", sender, e);
            this.Response.Redirect(Util.UnInstallURL(this.ModuleContext.TabId, this.PackageID));
			Logger.LogDebug("<<< Exiting OnDeleteClick(sender={Sender},e={E})", sender, e);
        }

        protected void OnPackageClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPackageClick(sender={Sender},e={E})", sender, e);
            try
            {
                this.UpdatePackage(false);
                this.Response.Redirect(Util.PackageWriterURL(this.ModuleContext, this.PackageID));
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in OnPackageClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, ex);
            }
			Logger.LogDebug("<<< Exiting OnPackageClick(sender={Sender},e={E})", sender, e);
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnUpdateClick(sender={Sender},e={E})", sender, e);
            try
            {
                this.UpdatePackage(true);
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in OnUpdateClick(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, ex);
            }
			Logger.LogDebug("<<< Exiting OnUpdateClick(sender={Sender},e={E})", sender, e);
        }

        private void BindData()
        {
			Logger.LogDebug(">>> Entering BindData()");
            this.email.ValidationExpression = Globals.glbEmailRegEx;
            this.trLanguagePackType.Visible = false;
            switch (this.Mode)
            {
                case "All":
                    this.lblHelp.Text = Localization.GetString("EditHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdate", this.LocalResourceFile);
                    break;
                case "LanguagePack":
                    this.lblHelp.Text = Localization.GetString("EditLanguageHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdateLanguage", this.LocalResourceFile);
                    break;
                case "Module":
                    this.lblHelp.Text = Localization.GetString("EditModuleHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdateModule", this.LocalResourceFile);
                    break;
                case "Skin":
                    this.lblHelp.Text = Localization.GetString("EditSkinHelp", this.LocalResourceFile);
                    this.cmdUpdate.Text = Localization.GetString("cmdUpdateSkin", this.LocalResourceFile);
                    break;
            }

            this.cmdPackage.Visible = this.IsSuperTab;
            this.cmdUpdate.Visible = this.IsSuperTab;
            if (this.Package != null)
            {
                if (this.PackageEditor == null || this.PackageID == Null.NullInteger)
                {
                    this.extensionSection.Visible = false;
                }
                else
                {
                    this.phEditor.Controls.Clear();
                    this.phEditor.Controls.Add(this.PackageEditor as Control);
                    if (this.PackageEditor is IModuleControl moduleControl)
                    {
                        moduleControl.ModuleContext.Configuration = this.ModuleContext.Configuration;
                    }

                    this.PackageEditor.PackageID = this.PackageID;
                    this.PackageEditor.Initialize();

                    this.Package.IconFile = Util.ParsePackageIconFileName(this.Package);
                }

                switch (this.Package.PackageType)
                {
                    case "Auth_System":
                    case "Container":
                    case "Module":
                    case "Skin":
                        this.iconFile.Enabled = true;
                        this.Package.IconFile = Util.ParsePackageIconFileName(this.Package);
                        break;
                    default:
                        this.iconFile.Enabled = false;
                        this.Package.IconFile = "Not Available";
                        break;
                }

                if (this.Mode != "All")
                {
                    this.packageType.Visible = false;
                }

                // Determine if Package is ready for packaging
                PackageWriterBase writer = PackageWriterFactory.GetWriter(this.serviceProvider, this.Package);
                this.cmdPackage.Visible = this.IsSuperTab && writer != null && Directory.Exists(Path.Combine(this.appStatus.ApplicationMapPath, writer.BasePath));
				Logger.LogDebug("BindData(): writer → {Writer}", writer);

                this.cmdDelete.Visible = this.IsSuperTab && (!this.Package.IsSystemPackage) && PackageController.CanDeletePackage(this.hostSettings, this.appStatus, this.Package, this.ModuleContext.PortalSettings);
                this.ctlAudit.Entity = this.Package;

                this.packageForm.DataSource = this.Package;
                this.packageFormReadOnly.DataSource = this.Package;
                if (!this.Page.IsPostBack)
                {
                    this.packageForm.DataBind();
                    this.packageFormReadOnly.DataBind();
                }

                this.packageForm.Visible = this.IsSuperTab;
                this.packageFormReadOnly.Visible = !this.IsSuperTab;
            }
			Logger.LogDebug("<<< Exiting BindData()");
        }

        private void UpdatePackage(bool displayMessage)
        {
			Logger.LogDebug(">>> Entering UpdatePackage(displayMessage={DisplayMessage})", displayMessage);
            if (this.packageForm.IsValid)
            {
                if (this.packageForm.DataSource is PackageInfo packageInfo)
                {
                    var pkgIconFile = Util.ParsePackageIconFileName(packageInfo);
                    packageInfo.IconFile = (pkgIconFile.Trim().Length > 0) ? Util.ParsePackageIconFile(packageInfo) : null;
					Logger.LogDebug("UpdatePackage(displayMessage={DisplayMessage}): pkgIconFile → {PkgIconFile}", displayMessage, pkgIconFile);
                    PackageController.Instance.SaveExtensionPackage(packageInfo);
                }

                if (displayMessage)
                {
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PackageUpdated", this.LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                }
            }

            this.PackageEditor?.UpdatePackage();
			Logger.LogDebug("<<< Exiting UpdatePackage(displayMessage={DisplayMessage})", displayMessage);
        }
    }
}
