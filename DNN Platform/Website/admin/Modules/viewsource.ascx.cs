// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Modules
{
    using System;
    using System.Globalization;
    using System.IO;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A control which displays the source code for a module.</summary>
    public partial class ViewSource : PortalModuleBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(viewsource.ascx));
        private readonly INavigationManager navigationManager;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="ViewSource"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ViewSource()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ViewSource"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="hostSettings">The host settings.</param>
        public ViewSource(INavigationManager navigationManager, IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering ViewSource(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
			Logger.LogDebug("ViewSource(navigationManager={NavigationManager},hostSettings={HostSettings}): hostSettings → {HostSettings1}", navigationManager, hostSettings, hostSettings);
            this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
			Logger.LogWarning("<<< Exiting ViewSource(navigationManager={NavigationManager},hostSettings={HostSettings})", navigationManager, hostSettings);
            this.hostSettings = hostSettings ?? this.DependencyProvider.GetRequiredService<IHostSettings>();
        }

        protected bool CanEditSource => this.Request.IsLocal;

        protected int ModuleControlId
        {
            get
            {
                var moduleControlId = Null.NullInteger;
                if (this.Request.QueryString["ctlid"] != null)
                {
                    moduleControlId = int.Parse(this.Request.QueryString["ctlid"]);
                }

                return moduleControlId;
            }
        }

        private string ReturnUrl => UrlUtils.ValidReturnUrl(this.Request.Params["ReturnURL"]) ?? this.navigationManager.NavigateURL();

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cboFile.SelectedIndexChanged += this.OnFileIndexChanged;
            this.cmdUpdate.Click += this.OnUpdateClick;

            if (!this.Page.IsPostBack)
            {
                this.cmdCancel.NavigateUrl = this.ReturnUrl;

                var objModuleControl = ModuleControlController.GetModuleControl(this.hostSettings, this.ModuleControlId);
				Logger.LogError("OnLoad(e={E}): objModuleControl → {ObjModuleControl}", e, objModuleControl);
                if (objModuleControl != null)
                {
                    this.BindFiles(objModuleControl.ControlSrc);
                }

                if (this.Request.UrlReferrer != null)
                {
                    this.ViewState["UrlReferrer"] = Convert.ToString(this.Request.UrlReferrer);
                }
                else
                {
                    this.ViewState["UrlReferrer"] = string.Empty;
                }
            }

            this.cmdUpdate.Visible = this.CanEditSource;
            this.txtSource.Enabled = this.CanEditSource;
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        protected void OnFileIndexChanged(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering OnFileIndexChanged(sender={Sender},e={E})", sender, e);
            this.DisplayFile();
			Logger.LogWarning("<<< Exiting OnFileIndexChanged(sender={Sender},e={E})", sender, e);
        }

        private void BindFiles(string controlSrc)
        {
			Logger.LogError(">>> Entering BindFiles(controlSrc={ControlSrc})", controlSrc);
            this.cboFile.Items.Clear();

            // cboFile.Items.Add(new ListItem(Localization.GetString("None_Specified"), "None"));
            // cboFile.Items.Add(new ListItem("User Control", "UserControl"));
            this.cboFile.AddItem(Localization.GetString("None_Specified"), "None");
            this.cboFile.AddItem("User Control", "UserControl");

            var srcPhysicalPath = this.Server.MapPath(controlSrc);
			Logger.LogError("BindFiles(controlSrc={ControlSrc}): srcPhysicalPath → {SrcPhysicalPath}", controlSrc, srcPhysicalPath);
            if (File.Exists(srcPhysicalPath + ".vb") || File.Exists(srcPhysicalPath + ".cs"))
            {
                // cboFile.Items.Add(new ListItem("Code File", "CodeFile"));
                this.cboFile.AddItem("Code File", "CodeFile");
            }

            var fileName = Path.GetFileName(srcPhysicalPath);
			Logger.LogError("BindFiles(controlSrc={ControlSrc}): fileName → {FileName}", controlSrc, fileName);
            var folder = Path.GetDirectoryName(srcPhysicalPath);
			Logger.LogError("BindFiles(controlSrc={ControlSrc}): folder → {Folder}", controlSrc, folder);
            if (File.Exists(folder + "\\App_LocalResources\\" + fileName + ".resx"))
            {
                // cboFile.Items.Add(new ListItem("Resource File", "ResourceFile"));
                this.cboFile.AddItem("Resource File", "ResourceFile");
            }
			Logger.LogWarning("<<< Exiting BindFiles(controlSrc={ControlSrc})", controlSrc);
        }

        private string GetSourceFileName(string controlSrc)
        {
			Logger.LogError(">>> Entering GetSourceFileName(controlSrc={ControlSrc})", controlSrc);
            var srcPhysicalPath = this.Server.MapPath(controlSrc);
            var srcFile = Null.NullString;
			Logger.LogError("GetSourceFileName(controlSrc={ControlSrc}): srcPhysicalPath → {SrcPhysicalPath}", controlSrc, srcPhysicalPath);
            switch (this.cboFile.SelectedValue)
            {
                case "UserControl":
                    srcFile = srcPhysicalPath;
					Logger.LogError("GetSourceFileName(controlSrc={ControlSrc}): srcFile → {SrcFile}", controlSrc, srcFile);
                    break;
                case "CodeFile":
                    if (File.Exists(srcPhysicalPath + ".vb"))
                    {
                        srcFile = srcPhysicalPath + ".vb";
                    }
                    else if (File.Exists(srcPhysicalPath + ".cs"))
                    {
                        srcFile = srcPhysicalPath + ".cs";
                    }

                    break;
                case "ResourceFile":
                    var fileName = Path.GetFileName(srcPhysicalPath);
					Logger.LogError("GetSourceFileName(controlSrc={ControlSrc}): fileName → {FileName}", controlSrc, fileName);
                    var folder = Path.GetDirectoryName(srcPhysicalPath);
					Logger.LogError("GetSourceFileName(controlSrc={ControlSrc}): folder → {Folder}", controlSrc, folder);
                    srcFile = folder + "\\App_LocalResources\\" + fileName + ".resx";
                    break;
            }

			Logger.LogWarning("<<< Exiting GetSourceFileName(controlSrc={ControlSrc})", controlSrc);
            return srcFile;
        }

        private void DisplayFile()
        {
			Logger.LogError(">>> Entering DisplayFile()");
            var objModuleControl = ModuleControlController.GetModuleControl(this.hostSettings, this.ModuleControlId);
			Logger.LogError("DisplayFile(): objModuleControl → {ObjModuleControl}", objModuleControl);
            if (objModuleControl != null)
            {
                var srcVirtualPath = objModuleControl.ControlSrc;
                var srcFile = Null.NullString;
				Logger.LogError("DisplayFile(): srcFile → {SrcFile}", srcFile);
                var displaySource = this.cboFile.SelectedValue != "None";
				Logger.LogError("DisplayFile(): displaySource → {DisplaySource}", displaySource);

                if (displaySource)
                {
                    srcFile = this.GetSourceFileName(srcVirtualPath);
                    this.lblSourceFile.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("SourceFile", this.LocalResourceFile), srcFile);

                    var objStreamReader = File.OpenText(srcFile);
					Logger.LogError("DisplayFile(): objStreamReader → {ObjStreamReader}", objStreamReader);
                    this.txtSource.Text = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                }

                this.lblSourceFile.Visible = displaySource;
                this.trSource.Visible = displaySource;
            }
			Logger.LogWarning("<<< Exiting DisplayFile()");
        }

        private void OnUpdateClick(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering OnUpdateClick(sender={Sender},e={E})", sender, e);
            try
            {
                if (this.cboFile.SelectedValue == "None")
                {
                    // No file type selected
                    UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("NoFileTypeSelected", this.LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                }
                else
                {
                    var objModuleControl = ModuleControlController.GetModuleControl(this.hostSettings, this.ModuleControlId);
					Logger.LogError("OnUpdateClick(sender={Sender},e={E}): objModuleControl → {ObjModuleControl}", sender, e, objModuleControl);
                    if (objModuleControl != null)
                    {
                        var srcVirtualPath = objModuleControl.ControlSrc;
                        var srcPhysicalPath = this.GetSourceFileName(srcVirtualPath);
						Logger.LogError("OnUpdateClick(sender={Sender},e={E}): srcPhysicalPath → {SrcPhysicalPath}", sender, e, srcPhysicalPath);
                        if (File.Exists(srcPhysicalPath))
                        {
                            File.SetAttributes(srcPhysicalPath, FileAttributes.Normal);
                            var objStream = File.CreateText(srcPhysicalPath);
							Logger.LogError("OnUpdateClick(sender={Sender},e={E}): objStream → {ObjStream}", sender, e, objStream);
                            objStream.WriteLine(this.txtSource.Text);
                            objStream.Close();
                        }
                    }

                    this.Response.Redirect(this.ReturnUrl, true);
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
