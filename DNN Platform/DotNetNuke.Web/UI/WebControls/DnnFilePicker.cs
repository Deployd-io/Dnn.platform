// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    /// <summary>The FilePicker Class provides a File Picker Control for DotNetNuke.</summary>
    public class DnnFilePicker : CompositeControl, ILocalizable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnFilePicker));
        private readonly IHostSettings hostSettings;
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;

        private Panel pnlContainer;
        private Panel pnlLeftDiv;
        private Panel pnlFolder;

        private Label lblFolder;
        private DropDownList cboFolders;
        private Panel pnlFile;
        private Label lblFile;
        private DropDownList cboFiles;
        private Panel pnlUpload;
        private HtmlInputFile txtFile;
        private Panel pnlButtons;
        private LinkButton cmdCancel;
        private LinkButton cmdSave;
        private LinkButton cmdUpload;
        private Panel pnlMessage;
        private Label lblMessage;
        private Panel pnlRightDiv;
        private Image imgPreview;
        private bool localize = true;

        /// <summary>Initializes a new instance of the <see cref="DnnFilePicker"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public DnnFilePicker()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFilePicker"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        public DnnFilePicker(IHostSettings hostSettings, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        {
			Logger.LogError(">>> Entering DnnFilePicker(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController})", hostSettings, portalController, appStatus, portalGroupController);
			Logger.LogDebug("DnnFilePicker(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalController → {PortalController1}", hostSettings, portalController, appStatus, portalGroupController, portalController);
			Logger.LogDebug("DnnFilePicker(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): appStatus → {AppStatus1}", hostSettings, portalController, appStatus, portalGroupController, appStatus);
			Logger.LogDebug("DnnFilePicker(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController}): portalGroupController → {PortalGroupController1}", hostSettings, portalController, appStatus, portalGroupController, portalGroupController);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogWarning("<<< Exiting DnnFilePicker(hostSettings={HostSettings},portalController={PortalController},appStatus={AppStatus},portalGroupController={PortalGroupController})", hostSettings, portalController, appStatus, portalGroupController);
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
        }

        /// <summary>  Represents a possible mode for the File Control.</summary>
        protected enum FileControlMode
        {
            /// <summary>  The File Control is in its Normal mode.</summary>
            Normal = 0,

            /// <summary>  The File Control is in the Upload File mode.</summary>
            UpLoadFile = 1,

            /// <summary>  The File Control is in the Preview mode.</summary>
            Preview = 2,
        }

        /// <summary>Gets or sets the maximum height in pixels.</summary>
        public int MaxHeight { get; set; } = 100;

        /// <summary>Gets or sets the maximum width in pixels.</summary>
        public int MaxWidth { get; set; } = 135;

        /// <summary>Gets or sets the class to be used for the Labels.</summary>
        /// <remarks>Defaults to 'CommandButton'.</remarks>
        /// <value>A String.</value>
        public string CommandCssClass
        {
            get
            {
                var cssClass = Convert.ToString(this.ViewState["CommandCssClass"], CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(cssClass) ? "dnnSecondaryAction" : cssClass;
            }

            set
            {
                this.ViewState["CommandCssClass"] = value;
            }
        }

        /// <summary>Gets or sets the file Filter to use.</summary>
        /// <remarks>Defaults to <see cref="string.Empty"/>.</remarks>
        /// <value>a comma separated list of file extensions no wildcards or periods e.g. "jpg,png,gif".</value>
        public string FileFilter
        {
            get => this.ViewState["FileFilter"] != null ? (string)this.ViewState["FileFilter"] : string.Empty;
            set => this.ViewState["FileFilter"] = value;
        }

        /// <summary>  Gets or sets the FileID for the control.</summary>
        /// <value>An Integer.</value>
        public int FileID
        {
            get
            {
                this.EnsureChildControls();
                if (this.ViewState["FileID"] == null)
                {
                    // Get FileId from the file combo
                    var fileId = Null.NullInteger;
                    if (this.cboFiles.SelectedItem != null)
                    {
                        fileId = int.Parse(this.cboFiles.SelectedItem.Value, CultureInfo.InvariantCulture);
                    }

                    this.ViewState["FileID"] = fileId;
                }

                return Convert.ToInt32(this.ViewState["FileID"], CultureInfo.InvariantCulture);
            }

            set
            {
                this.EnsureChildControls();
                this.ViewState["FileID"] = value;
                if (string.IsNullOrEmpty(this.FilePath))
                {
                    var fileInfo = FileManager.Instance.GetFile(value);
                    if (fileInfo != null)
                    {
                        this.SetFilePath(fileInfo.Folder + fileInfo.FileName);
                    }
                }
            }
        }

        /// <summary>Gets or sets the FilePath for the control.</summary>
        /// <value>A String.</value>
        public string FilePath
        {
            get => Convert.ToString(this.ViewState["FilePath"], CultureInfo.InvariantCulture);
            set => this.ViewState["FilePath"] = value;
        }

        /// <summary>Gets or sets a value indicating whether to Include Personal Folder.</summary>
        /// <remarks>Defaults to <see langword="false"/>.</remarks>
        /// <value>A Boolean.</value>
        public bool UsePersonalFolder
        {
            get => this.ViewState["UsePersonalFolder"] != null && Convert.ToBoolean(this.ViewState["UsePersonalFolder"], CultureInfo.InvariantCulture);
            set => this.ViewState["UsePersonalFolder"] = value;
        }

        /// <summary>  Gets or sets the class to be used for the Labels.</summary>
        /// <value>A String.</value>
        public string LabelCssClass
        {
            get
            {
                var cssClass = Convert.ToString(this.ViewState["LabelCssClass"], CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(cssClass) ? string.Empty : cssClass;
            }

            set
            {
                this.ViewState["LabelCssClass"] = value;
            }
        }

        /// <summary>Gets or sets the permission keys.</summary>
        public string Permissions
        {
            get
            {
                var permissions = Convert.ToString(this.ViewState["Permissions"], CultureInfo.InvariantCulture);
                return string.IsNullOrEmpty(permissions) ? "BROWSE,ADD" : permissions;
            }

            set
            {
                this.ViewState["Permissions"] = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether the combos have a "Not Specified" option.</summary>
        /// <remarks>Defaults to <see langword="true"/> (i.e. no "Not Specified").</remarks>
        /// <value>A Boolean.</value>
        public bool Required
        {
            get => this.ViewState["Required"] != null && Convert.ToBoolean(this.ViewState["Required"], CultureInfo.InvariantCulture);
            set => this.ViewState["Required"] = value;
        }

        /// <summary>Gets or sets a value indicating whether to show folders.</summary>
        public bool ShowFolders
        {
            get => this.ViewState["ShowFolders"] == null || Convert.ToBoolean(this.ViewState["ShowFolders"], CultureInfo.InvariantCulture);
            set => this.ViewState["ShowFolders"] = value;
        }

        /// <summary>Gets or sets a value indicating whether to Show the Upload Button.</summary>
        /// <remarks>Defaults to <see langword="true"/>.</remarks>
        /// <value>A Boolean.</value>
        public bool ShowUpLoad
        {
            get => this.ViewState["ShowUpLoad"] == null || Convert.ToBoolean(this.ViewState["ShowUpLoad"], CultureInfo.InvariantCulture);
            set => this.ViewState["ShowUpLoad"] = value;
        }

        /// <summary>Gets or sets the user.</summary>
        public UserInfo User { get; set; }

        /// <inheritdoc />
        public bool Localize
        {
            get => this.localize;
            set => this.localize = value;
        }

        /// <inheritdoc />
        public string LocalResourceFile { get; set; }

        /// <summary>Gets a value indicating whether the control is on a Host or Portal Tab.</summary>
        protected bool IsHost
        {
            get
            {
                var isHost = Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);

                // if not host tab but current edit user is a host user, then return true
                if (!isHost && this.User != null && this.User.IsSuperUser)
                {
                    isHost = true;
                }

                return isHost;
            }
        }

        /// <summary>  Gets the root folder for the control.</summary>
        /// <value>A String.</value>
        protected string ParentFolder => this.IsHost ? Globals.HostMapPath : this.PortalSettings.HomeDirectoryMapPath;

        /// <summary>  Gets the file PortalId to use.</summary>
        /// <remarks>
        ///   Defaults to PortalSettings.PortalId.
        /// </remarks>
        /// <value>An Integer.</value>
        protected int PortalId
        {
            get
            {
                if ((this.Page.Request.QueryString["pid"] != null) && (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    return int.Parse(this.Page.Request.QueryString["pid"], CultureInfo.InvariantCulture);
                }

                if (!this.IsHost)
                {
                    return this.PortalSettings.PortalId;
                }

                return Null.NullInteger;
            }
        }

        /// <summary>Gets the current Portal Settings.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings PortalSettings => PortalSettings.Current;

        /// <summary>Gets or sets the current mode of the control.</summary>
        /// <remarks>Defaults to <see cref="FileControlMode.Normal"/>.</remarks>
        /// <value>A FileControlMode enum.</value>
        protected FileControlMode Mode
        {
            get => this.ViewState["Mode"] == null ? FileControlMode.Normal : (FileControlMode)this.ViewState["Mode"];
            set => this.ViewState["Mode"] = value;
        }

        /// <inheritdoc />
        public virtual void LocalizeStrings()
        {
		Logger.LogError(">>> Entering LocalizeStrings()");
        Logger.LogWarning("<<< Exiting LocalizeStrings()");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);
            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
            this.EnsureChildControls();
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <summary>
        ///   CreateChildControls overrides the Base class's method to correctly build the
        ///   control based on the configuration.
        /// </summary>
        protected override void CreateChildControls()
        {
			Logger.LogError(">>> Entering CreateChildControls()");
            // First clear the controls collection
            this.Controls.Clear();

            this.pnlContainer = new Panel { CssClass = "dnnFilePicker" };
			Logger.LogDebug("CreateChildControls(): pnlContainer → {PnlContainer}", pnlContainer);
			Logger.LogDebug("CreateChildControls(): this.pnlContainer → {Arg0}", this.pnlContainer);

            this.pnlLeftDiv = new Panel { CssClass = "dnnLeft" };
			Logger.LogDebug("CreateChildControls(): pnlLeftDiv → {PnlLeftDiv}", pnlLeftDiv);
			Logger.LogDebug("CreateChildControls(): this.pnlLeftDiv → {Arg0}", this.pnlLeftDiv);

            this.AddFolderArea();
            this.AddFileAndUploadArea();
            this.AddButtonArea();
            this.AddMessageRow();

            this.pnlContainer.Controls.Add(this.pnlLeftDiv);

            this.pnlRightDiv = new Panel { CssClass = "dnnLeft" };
			Logger.LogDebug("CreateChildControls(): pnlRightDiv → {PnlRightDiv}", pnlRightDiv);
			Logger.LogDebug("CreateChildControls(): this.pnlRightDiv → {Arg0}", this.pnlRightDiv);

            this.GeneratePreviewImage();

            this.pnlContainer.Controls.Add(this.pnlRightDiv);

            this.Controls.Add(this.pnlContainer);

            base.CreateChildControls();
			Logger.LogWarning("<<< Exiting CreateChildControls()");
        }

        /// <summary>OnPreRender runs just before the control is rendered.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (this.cboFolders.Items.Count > 0)
            {
                // Configure Labels
                this.lblFolder.Text = Utilities.GetLocalizedString("Folder");
                this.lblFolder.CssClass = this.LabelCssClass;
                this.lblFile.Text = Utilities.GetLocalizedString("File");
                this.lblFile.CssClass = this.LabelCssClass;

                // select folder
                string fileName;
                string folderPath;
                if (!string.IsNullOrEmpty(this.FilePath))
                {
                    fileName = this.FilePath.Substring(this.FilePath.LastIndexOf("/", StringComparison.Ordinal) + 1);
					Logger.LogError("OnPreRender(e={E}): fileName → {FileName}", e, fileName);
                    folderPath = string.IsNullOrEmpty(fileName) ? this.FilePath : this.FilePath.Replace(fileName, string.Empty);
					Logger.LogDebug("OnPreRender(e={E}): folderPath → {FolderPath}", e, folderPath);
                }
                else
                {
                    fileName = this.FilePath;
					Logger.LogDebug("OnPreRender(e={E}): fileName → {FileName}", e, fileName);
                    folderPath = string.Empty;
                }

                if (this.cboFolders.Items.FindByValue(folderPath) != null)
                {
                    this.cboFolders.SelectedIndex = -1;
                    this.cboFolders.Items.FindByValue(folderPath).Selected = true;
                }

                // Get Files
                this.LoadFiles();
                if (this.cboFiles.Items.FindByText(fileName) != null)
                {
                    this.cboFiles.Items.FindByText(fileName).Selected = true;
                }

                if (this.cboFiles.SelectedItem == null || string.IsNullOrEmpty(this.cboFiles.SelectedItem.Value))
                {
                    this.FileID = -1;
                }
                else
                {
                    this.FileID = int.Parse(this.cboFiles.SelectedItem.Value, CultureInfo.InvariantCulture);
					Logger.LogDebug("OnPreRender(e={E}): this.FileID → {Arg1}", e, this.FileID);
                }

                if (this.cboFolders.Items.Count > 1 && this.ShowFolders)
                {
                    this.pnlFolder.Visible = true;
                }
                else
                {
                    this.pnlFolder.Visible = false;
                }

                // Configure Mode
                switch (this.Mode)
                {
                    case FileControlMode.Normal:
                        this.pnlFile.Visible = true;
                        this.pnlUpload.Visible = false;
                        this.pnlRightDiv.Visible = true;
                        this.ShowImage();

                        if ((FolderPermissionController.HasFolderPermission(this.PortalId, this.cboFolders.SelectedItem.Value, "ADD") || this.IsUserFolder(this.cboFolders.SelectedItem.Value)) && this.ShowUpLoad)
                        {
                            this.ShowButton(this.cmdUpload, "Upload");
                        }

                        break;

                    case FileControlMode.UpLoadFile:
                        this.pnlFile.Visible = false;
                        this.pnlUpload.Visible = true;
                        this.pnlRightDiv.Visible = false;
                        this.ShowButton(this.cmdSave, "Save");
                        this.ShowButton(this.cmdCancel, "Cancel");
                        break;
                }
            }
            else
            {
                this.lblMessage.Text = Utilities.GetLocalizedString("NoPermission");
            }

            // Show message Row
            this.pnlMessage.Visible = !string.IsNullOrEmpty(this.lblMessage.Text);
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>  AddButton adds a button to the Command Row.</summary>
        /// <param name="button">The button to add to the Row.</param>
        private void AddButton(ref LinkButton button)
        {
			Logger.LogError(">>> Entering AddButton(button={Button})", button);
            button = new LinkButton { EnableViewState = false, CausesValidation = false };
            button.ControlStyle.CssClass = this.CommandCssClass;
            button.Visible = false;
			Logger.LogDebug("AddButton(button={Button}): button.Visible → {Arg1}", button, button.Visible);

            this.pnlButtons.Controls.Add(button);
			Logger.LogWarning("<<< Exiting AddButton(button={Button})", button);
        }

        /// <summary>  AddCommandRow adds the Command Row.</summary>
        private void AddButtonArea()
        {
			Logger.LogError(">>> Entering AddButtonArea()");
            this.pnlButtons = new Panel { Visible = false };

			Logger.LogDebug("AddButtonArea(): this.pnlButtons → {Arg0}", this.pnlButtons);
			Logger.LogDebug("AddButtonArea(): pnlButtons → {PnlButtons}", pnlButtons);
            this.AddButton(ref this.cmdUpload);
            this.cmdUpload.Click += this.UploadFile;

            this.AddButton(ref this.cmdSave);
            this.cmdSave.Click += this.SaveFile;

            this.AddButton(ref this.cmdCancel);
            this.cmdCancel.Click += this.CancelUpload;

            this.pnlLeftDiv.Controls.Add(this.pnlButtons);
			Logger.LogWarning("<<< Exiting AddButtonArea()");
        }

        /// <summary>  AddFileRow adds the Files Row.</summary>
        private void AddFileAndUploadArea()
        {
			Logger.LogError(">>> Entering AddFileAndUploadArea()");
            // Create Url Div
            this.pnlFile = new Panel { CssClass = "dnnFormItem" };
			Logger.LogDebug("AddFileAndUploadArea(): pnlFile → {PnlFile}", pnlFile);
			Logger.LogDebug("AddFileAndUploadArea(): this.pnlFile → {Arg0}", this.pnlFile);

            // Create File Label
            this.lblFile = new Label { EnableViewState = false };
			Logger.LogDebug("AddFileAndUploadArea(): lblFile → {LblFile}", lblFile);
			Logger.LogDebug("AddFileAndUploadArea(): this.lblFile → {Arg0}", this.lblFile);
            this.pnlFile.Controls.Add(this.lblFile);

            // Create Files Combo
            this.cboFiles = new DropDownList { ID = "File", DataTextField = "Text", DataValueField = "Value", AutoPostBack = true };
			Logger.LogDebug("AddFileAndUploadArea(): cboFiles → {CboFiles}", cboFiles);
			Logger.LogDebug("AddFileAndUploadArea(): this.cboFiles → {Arg0}", this.cboFiles);
            this.cboFiles.SelectedIndexChanged += this.FileChanged;
            this.pnlFile.Controls.Add(this.cboFiles);

            this.pnlLeftDiv.Controls.Add(this.pnlFile);

            // Create Upload Div
            this.pnlUpload = new Panel { CssClass = "dnnFormItem" };
			Logger.LogDebug("AddFileAndUploadArea(): pnlUpload → {PnlUpload}", pnlUpload);
			Logger.LogDebug("AddFileAndUploadArea(): this.pnlUpload → {Arg0}", this.pnlUpload);

            // Create Upload Box
            this.txtFile = new HtmlInputFile();
			Logger.LogDebug("AddFileAndUploadArea(): txtFile → {TxtFile}", txtFile);
            this.txtFile.Attributes.Add("size", "13");
            this.pnlUpload.Controls.Add(this.txtFile);

            this.pnlLeftDiv.Controls.Add(this.pnlUpload);
			Logger.LogWarning("<<< Exiting AddFileAndUploadArea()");
        }

        /// <summary>  AddFolderRow adds the Folders Row.</summary>
        private void AddFolderArea()
        {
			Logger.LogError(">>> Entering AddFolderArea()");
            // Create Url Div
            this.pnlFolder = new Panel { CssClass = "dnnFormItem" };
			Logger.LogDebug("AddFolderArea(): pnlFolder → {PnlFolder}", pnlFolder);
			Logger.LogDebug("AddFolderArea(): this.pnlFolder → {Arg0}", this.pnlFolder);

            // Create Folder Label
            this.lblFolder = new Label { EnableViewState = false };
			Logger.LogDebug("AddFolderArea(): lblFolder → {LblFolder}", lblFolder);
			Logger.LogDebug("AddFolderArea(): this.lblFolder → {Arg0}", this.lblFolder);
            this.pnlFolder.Controls.Add(this.lblFolder);

            // Create Folders Combo
            this.cboFolders = new DropDownList { ID = "Folder", AutoPostBack = true };
			Logger.LogDebug("AddFolderArea(): cboFolders → {CboFolders}", cboFolders);
			Logger.LogDebug("AddFolderArea(): this.cboFolders → {Arg0}", this.cboFolders);
            this.cboFolders.SelectedIndexChanged += this.FolderChanged;
            this.pnlFolder.Controls.Add(this.cboFolders);

            // add to left div
            this.pnlLeftDiv.Controls.Add(this.pnlFolder);

            // Load Folders
            this.LoadFolders();
			Logger.LogWarning("<<< Exiting AddFolderArea()");
        }

        /// <summary>Adds an empty preview image to the <see cref="pnlRightDiv"/> control.</summary>
        private void GeneratePreviewImage()
        {
			Logger.LogError(">>> Entering GeneratePreviewImage()");
            this.imgPreview = new Image();
			Logger.LogWarning("<<< Exiting GeneratePreviewImage()");
			Logger.LogDebug("GeneratePreviewImage(): imgPreview → {ImgPreview}", imgPreview);
            this.pnlRightDiv.Controls.Add(this.imgPreview);
        }

        /// <summary>  AddMessageRow adds the Message Row.</summary>
        private void AddMessageRow()
        {
			Logger.LogError(">>> Entering AddMessageRow()");
            this.pnlMessage = new Panel { CssClass = "dnnFormMessage dnnFormWarning" };

			Logger.LogDebug("AddMessageRow(): this.pnlMessage → {Arg0}", this.pnlMessage);
			Logger.LogDebug("AddMessageRow(): pnlMessage → {PnlMessage}", pnlMessage);
            // Create Label
            this.lblMessage = new Label { EnableViewState = false, Text = string.Empty };
			Logger.LogDebug("AddMessageRow(): lblMessage → {LblMessage}", lblMessage);
			Logger.LogDebug("AddMessageRow(): this.lblMessage → {Arg0}", this.lblMessage);
            this.pnlMessage.Controls.Add(this.lblMessage);

            this.pnlLeftDiv.Controls.Add(this.pnlMessage);
			Logger.LogWarning("<<< Exiting AddMessageRow()");
        }

        private bool IsUserFolder(string folderPath)
        {
			Logger.LogError(">>> Entering IsUserFolder(folderPath={FolderPath})", folderPath);
            UserInfo user = this.User ?? UserController.Instance.GetCurrentUserInfo();
			Logger.LogError("IsUserFolder(folderPath={FolderPath}): user → {User}", folderPath, user);
			Logger.LogWarning("<<< Exiting IsUserFolder(folderPath={FolderPath})", folderPath);
            return folderPath.StartsWith("users/", StringComparison.OrdinalIgnoreCase) && folderPath.EndsWith(string.Format(CultureInfo.InvariantCulture, "/{0}/", user.UserID), StringComparison.OrdinalIgnoreCase);
        }

        private void LoadFiles()
        {
			Logger.LogError(">>> Entering LoadFiles()");
            int effectivePortalId = this.PortalId;
			Logger.LogError("LoadFiles(): effectivePortalId → {EffectivePortalId}", effectivePortalId);
            if (this.IsUserFolder(this.cboFolders.SelectedItem.Value))
            {
                effectivePortalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, this.PortalId);
            }

            this.cboFiles.DataSource = Globals.GetFileList(effectivePortalId, this.FileFilter, !this.Required, this.cboFolders.SelectedItem.Value);
            this.cboFiles.DataBind();
			Logger.LogWarning("<<< Exiting LoadFiles()");
        }

        /// <summary>LoadFolders fetches the list of folders from the Database.</summary>
        private void LoadFolders()
        {
			Logger.LogError(">>> Entering LoadFolders()");
            UserInfo user = this.User ?? UserController.Instance.GetCurrentUserInfo();
            this.cboFolders.Items.Clear();
			Logger.LogError("LoadFolders(): user → {User}", user);

            // Add Personal Folder
            if (this.UsePersonalFolder)
            {
                var userFolder = FolderManager.Instance.GetUserFolder(user).FolderPath;
				Logger.LogError("LoadFolders(): userFolder → {UserFolder}", userFolder);
                var userFolderItem = this.cboFolders.Items.FindByValue(userFolder);
				Logger.LogError("LoadFolders(): userFolderItem → {UserFolderItem}", userFolderItem);
                if (userFolderItem != null)
                {
                    userFolderItem.Text = Utilities.GetLocalizedString("MyFolder");
                }
                else
                {
                    // Add DummyFolder
                    this.cboFolders.Items.Add(new ListItem(Utilities.GetLocalizedString("MyFolder"), userFolder));
                }
            }
            else
            {
                var folders = FolderManager.Instance.GetFolders(this.PortalId, "READ,ADD", user.UserID);
				Logger.LogError("LoadFolders(): folders → {Folders}", folders);
                foreach (FolderInfo folder in folders)
                {
                    var folderItem = new ListItem
                    {
                        Text =
                                                 folder.FolderPath == Null.NullString
                                                     ? Utilities.GetLocalizedString("PortalRoot")
                                                     : folder.DisplayPath,
                        Value = folder.FolderPath,
                    };
                    this.cboFolders.Items.Add(folderItem);
                }
            }
			Logger.LogWarning("<<< Exiting LoadFolders()");
        }

        /// <summary>  SetFilePath sets the FilePath property.</summary>
        /// <remarks>
        ///   This overload uses the selected item in the Folder combo.
        /// </remarks>
        private void SetFilePath()
        {
			Logger.LogError(">>> Entering SetFilePath()");
            this.SetFilePath(this.cboFiles.SelectedItem.Text);
			Logger.LogWarning("<<< Exiting SetFilePath()");
        }

        /// <summary>  SetFilePath sets the FilePath property.</summary>
        /// <remarks>
        ///   This overload allows the caller to specify a file.
        /// </remarks>
        /// <param name="fileName">The filename to use in setting the property.</param>
        private void SetFilePath(string fileName)
        {
			Logger.LogError(">>> Entering SetFilePath(fileName={FileName})", fileName);
            if (string.IsNullOrEmpty(this.cboFolders.SelectedItem.Value))
            {
				Logger.LogWarning("<<< Exiting SetFilePath()");
                this.FilePath = fileName;
            }
            else
            {
                this.FilePath = (this.cboFolders.SelectedItem.Value + "/") + fileName;
				Logger.LogDebug("SetFilePath(fileName={FileName}): this.FilePath → {Arg1}", fileName, this.FilePath);
            }
        }

        /// <summary>  ShowButton configures and displays a button.</summary>
        /// <param name="button">The button to configure.</param>
        /// <param name="command">The command name (amd key) of the button.</param>
        private void ShowButton(LinkButton button, string command)
        {
			Logger.LogError(">>> Entering ShowButton(button={Button},command={Command})", button, command);
            button.Visible = true;
			Logger.LogDebug("ShowButton(button={Button},command={Command}): button.Visible → {Arg2}", button, command, button.Visible);
            if (!string.IsNullOrEmpty(command))
            {
                button.Text = Utilities.GetLocalizedString(command);
            }

            AJAX.RegisterPostBackControl(button);
            this.pnlButtons.Visible = true;
			Logger.LogWarning("<<< Exiting ShowButton(button={Button},command={Command})", button, command);
        }

        /// <summary>  ShowImage displays the Preview Image.</summary>
        private void ShowImage()
        {
			Logger.LogError(">>> Entering ShowImage()");
            var image = (FileInfo)FileManager.Instance.GetFile(this.FileID);

			Logger.LogError("ShowImage(): image → {Image}", image);
            if (image != null)
            {
                this.imgPreview.ImageUrl = FileManager.Instance.GetUrl(image);
                try
                {
                    Utilities.CreateThumbnail(image, this.imgPreview, this.MaxWidth, this.MaxHeight);
                }
                catch (Exception)
                {
                    Logger.WarnFormat(CultureInfo.InvariantCulture, "Unable to create thumbnail for {0}", image.PhysicalPath);
                    this.pnlRightDiv.Visible = false;
                }
            }
            else
            {
                this.imgPreview.Visible = false;

                Panel imageHolderPanel = new Panel { CssClass = "dnnFilePickerImageHolder", };
				Logger.LogError("ShowImage(): imageHolderPanel → {ImageHolderPanel}", imageHolderPanel);
                this.pnlRightDiv.Controls.Add(imageHolderPanel);
                this.pnlRightDiv.Visible = true;
            }
			Logger.LogWarning("<<< Exiting ShowImage()");
        }

        private void CancelUpload(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering CancelUpload(sender={Sender},e={E})", sender, e);
            this.Mode = FileControlMode.Normal;
        }

        private void FileChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering FileChanged(sender={Sender},e={E})", sender, e);
            this.SetFilePath();
			Logger.LogWarning("<<< Exiting FileChanged(sender={Sender},e={E})", sender, e);
        }

        private void FolderChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering FolderChanged(sender={Sender},e={E})", sender, e);
            this.LoadFiles();
            this.SetFilePath();
			Logger.LogWarning("<<< Exiting FolderChanged(sender={Sender},e={E})", sender, e);
        }

        private void SaveFile(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering SaveFile(sender={Sender},e={E})", sender, e);
            // if file is selected exit
            if (!string.IsNullOrEmpty(this.txtFile.PostedFile.FileName))
            {
                var extension = Path.GetExtension(this.txtFile.PostedFile.FileName).Replace(".", string.Empty);

                if (!string.IsNullOrEmpty(this.FileFilter) && !this.FileFilter.ToLowerInvariant().Contains(extension.ToLowerInvariant()))
                {
                    // trying to upload a file not allowed for current filter
                    var localizedString = Localization.GetString("UploadError", this.LocalResourceFile);
                    if (string.IsNullOrEmpty(localizedString))
                    {
                        localizedString = Utilities.GetLocalizedString("UploadError");
                    }

                    this.lblMessage.Text = string.Format(CultureInfo.CurrentCulture, localizedString, this.FileFilter, extension);
                }
                else
                {
                    var folderManager = FolderManager.Instance;

                    var folderPath = PathUtils.Instance.GetRelativePath(this.PortalId, this.ParentFolder) + this.cboFolders.SelectedItem.Value;

                    // Check if this is a User Folder
                    IFolderInfo folder;
                    if (this.IsUserFolder(this.cboFolders.SelectedItem.Value))
                    {
                        // Make sure the user folder exists
                        folder = folderManager.GetFolder(PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, this.PortalId), folderPath);
						Logger.LogDebug("SaveFile(sender={Sender},e={E}): folder → {Folder}", sender, e, folder);
                        if (folder == null)
                        {
                            // Add User folder
                            var user = this.User ?? UserController.Instance.GetCurrentUserInfo();

                            // fix user's portal id
                            user.PortalID = this.PortalId;
							Logger.LogDebug("SaveFile(sender={Sender},e={E}): user.PortalID → {Arg2}", sender, e, user.PortalID);
                            folder = ((FolderManager)folderManager).AddUserFolder(user);
                        }
                    }
                    else
                    {
                        folder = folderManager.GetFolder(this.PortalId, folderPath);
                    }

                    var fileName = Path.GetFileName(this.txtFile.PostedFile.FileName);

                    try
                    {
                        FileManager.Instance.AddFile(folder, fileName, this.txtFile.PostedFile.InputStream, true, true, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)));
                    }
                    catch (PermissionsNotMetException)
                    {
						Logger.LogError("Exception in SaveFile(sender={Sender},e={E}): {Arg2}", sender, e, PermissionsNotMetException.Message);
                        this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
                    }
                    catch (NoSpaceAvailableException)
                    {
						Logger.LogError("Exception in SaveFile(sender={Sender},e={E}): {Arg2}", sender, e, NoSpaceAvailableException.Message);
                        this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("DiskSpaceExceeded"), fileName);
                    }
                    catch (InvalidFileExtensionException)
                    {
                        this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("RestrictedFileType"), fileName, this.hostSettings.AllowedExtensionAllowList.ToDisplayString());
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);

                        this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("SaveFileError"), fileName);
                    }
                }

                if (string.IsNullOrEmpty(this.lblMessage.Text))
                {
                    var fileName = this.txtFile.PostedFile.FileName.Substring(this.txtFile.PostedFile.FileName.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
                    this.SetFilePath(fileName);
                }
            }

			Logger.LogWarning("<<< Exiting SaveFile(sender={Sender},e={E})", sender, e);
            this.Mode = FileControlMode.Normal;
			Logger.LogDebug("SaveFile(sender={Sender},e={E}): this.Mode → {Arg2}", sender, e, this.Mode);
        }

        private void UploadFile(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering UploadFile(sender={Sender},e={E})", sender, e);
            this.Mode = FileControlMode.UpLoadFile;
        }
    }
}
