// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    using Globals = DotNetNuke.Common.Globals;

    public abstract class UrlControl : UserControlBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel ErrorRow;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel FileRow;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel ImagesRow;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel TabRow;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel TypeRow;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel URLRow;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel UserRow;

        // ReSharper restore InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DropDownList cboFiles;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DropDownList cboFolders;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DropDownList cboImages;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DropDownList cboTabs;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected DropDownList cboUrls;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected CheckBox chkLog;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected CheckBox chkNewWindow;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected CheckBox chkTrack;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdAdd;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdCancel;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdDelete;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdSave;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdSelect;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected LinkButton cmdUpload;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Image imgStorageLocationType;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblFile;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblFolder;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblImages;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblMessage;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblTab;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblURL;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblURLType;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblUser;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected RadioButtonList optType;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected HtmlInputFile txtFile;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected TextBox txtUrl;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected TextBox txtUser;
        private bool doChangeURL;
        private bool doReloadFiles;
        private bool doReloadFolders;
        private bool doRenderTypeControls;
        private bool doRenderTypes;
        private string localResourceFile;
        private PortalInfo objPortal;

        public bool Log
        {
            get
            {
                if (this.chkLog.Visible)
                {
                    return this.chkLog.Checked;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Track
        {
            get
            {
                if (this.chkTrack.Visible)
                {
                    return this.chkTrack.Checked;
                }
                else
                {
                    return false;
                }
            }
        }

        public string FileFilter
        {
            get
            {
                if (this.ViewState["FileFilter"] != null)
                {
                    return Convert.ToString(this.ViewState["FileFilter"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return string.Empty;
                }
            }

            set
            {
                this.ViewState["FileFilter"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doReloadFiles = true;
                }
            }
        }

        public bool IncludeActiveTab
        {
            get
            {
                if (this.ViewState["IncludeActiveTab"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["IncludeActiveTab"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return false; // Set as default
                }
            }

            set
            {
                this.ViewState["IncludeActiveTab"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypeControls = true;
                }
            }
        }

        public string LocalResourceFile
        {
            get
            {
                string fileRoot;
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    fileRoot = this.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/URLControl.ascx";
                }
                else
                {
                    fileRoot = this.localResourceFile;
                }

                return fileRoot;
            }

            set
            {
                this.localResourceFile = value;
            }
        }

        public int ModuleID
        {
            get
            {
                int myMid = -2;
                if (this.ViewState["ModuleId"] != null)
                {
                    myMid = Convert.ToInt32(this.ViewState["ModuleId"], CultureInfo.InvariantCulture);
                }
                else if (this.Request.QueryString["mid"] != null)
                {
                    if (!int.TryParse(this.Request.QueryString["mid"], out myMid))
                    {
                        myMid = -2;
                    }
                }

                return myMid;
            }

            set
            {
                this.ViewState["ModuleId"] = value;
            }
        }

        public bool NewWindow
        {
            get
            {
                return this.chkNewWindow.Visible && this.chkNewWindow.Checked;
            }

            set
            {
                this.chkNewWindow.Checked = this.chkNewWindow.Visible && value;
            }
        }

        public bool Required
        {
            get
            {
                if (this.ViewState["Required"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["Required"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["Required"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypeControls = true;
                }
            }
        }

        public bool ShowFiles
        {
            get
            {
                if (this.ViewState["ShowFiles"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowFiles"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowFiles"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypes = true;
                }
            }
        }

        public bool ShowImages
        {
            get
            {
                if (this.ViewState["ShowImages"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowImages"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return false;
                }
            }

            set
            {
                this.ViewState["ShowImages"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypes = true;
                }
            }
        }

        public bool ShowLog
        {
            get
            {
                return this.chkLog.Visible;
            }

            set
            {
                this.chkLog.Visible = value;
            }
        }

        public bool ShowNewWindow
        {
            get
            {
                return this.chkNewWindow.Visible;
            }

            set
            {
                this.chkNewWindow.Visible = value;
            }
        }

        public bool ShowNone
        {
            get
            {
                if (this.ViewState["ShowNone"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowNone"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return false; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowNone"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypes = true;
                }
            }
        }

        public bool ShowTabs
        {
            get
            {
                if (this.ViewState["ShowTabs"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowTabs"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowTabs"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypes = true;
                }
            }
        }

        public bool ShowTrack
        {
            get
            {
                return this.chkTrack.Visible;
            }

            set
            {
                this.chkTrack.Visible = value;
            }
        }

        public bool ShowUpLoad
        {
            get
            {
                if (this.ViewState["ShowUpLoad"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowUpLoad"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowUpLoad"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypeControls = true;
                }
            }
        }

        public bool ShowUrls
        {
            get
            {
                if (this.ViewState["ShowUrls"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowUrls"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return true; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowUrls"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypes = true;
                }
            }
        }

        public bool ShowUsers
        {
            get
            {
                if (this.ViewState["ShowUsers"] != null)
                {
                    return Convert.ToBoolean(this.ViewState["ShowUsers"], CultureInfo.InvariantCulture);
                }
                else
                {
                    return false; // Set as default in the old variable
                }
            }

            set
            {
                this.ViewState["ShowUsers"] = value;
                if (this.IsTrackingViewState)
                {
                    this.doRenderTypes = true;
                }
            }
        }

        public string Url
        {
            get
            {
                string r = string.Empty;
                string strCurrentType = string.Empty;
                if (this.optType.Items.Count > 0 && this.optType.SelectedIndex >= 0)
                {
                    strCurrentType = this.optType.SelectedItem.Value;
                }

                switch (strCurrentType)
                {
                    case "I":
                        if (this.cboImages.SelectedItem != null)
                        {
                            r = this.cboImages.SelectedItem.Value;
                        }

                        break;
                    case "U":
                        if (this.cboUrls.Visible)
                        {
                            if (this.cboUrls.SelectedItem != null)
                            {
                                r = this.cboUrls.SelectedItem.Value;
                                this.txtUrl.Text = r;
                            }
                        }
                        else
                        {
                            string mCustomUrl = this.txtUrl.Text;
                            if (mCustomUrl.Equals("http://", StringComparison.OrdinalIgnoreCase))
                            {
                                r = string.Empty;
                            }
                            else
                            {
                                r = Globals.AddHTTP(mCustomUrl);
                            }
                        }

                        break;
                    case "T":
                        string strTab = string.Empty;
                        if (this.cboTabs.SelectedItem != null)
                        {
                            strTab = this.cboTabs.SelectedItem.Value;
                            if (Globals.NumberMatchRegex.IsMatch(strTab) && (Convert.ToInt32(strTab, CultureInfo.InvariantCulture) >= 0))
                            {
                                r = strTab;
                            }
                        }

                        break;
                    case "F":
                        if (this.cboFiles.SelectedItem != null)
                        {
                            if (!string.IsNullOrEmpty(this.cboFiles.SelectedItem.Value))
                            {
                                r = "FileID=" + this.cboFiles.SelectedItem.Value;
                            }
                            else
                            {
                                r = string.Empty;
                            }
                        }

                        break;
                    case "M":
                        if (!string.IsNullOrEmpty(this.txtUser.Text))
                        {
                            UserInfo objUser = UserController.GetCachedUser(this.objPortal.PortalID, this.txtUser.Text);
                            if (objUser != null)
                            {
                                r = "UserID=" + objUser.UserID;
                            }
                            else
                            {
                                this.lblMessage.Text = Localization.GetString("NoUser", this.LocalResourceFile);
                                this.ErrorRow.Visible = true;
                                this.txtUser.Text = string.Empty;
                            }
                        }

                        break;
                }

                return r;
            }

            set
            {
                this.ViewState["Url"] = value;
                this.txtUrl.Text = string.Empty;

                if (this.IsTrackingViewState)
                {
                    this.doChangeURL = true;
                    this.doReloadFiles = true;
                }
            }
        }

        public string UrlType
        {
            get
            {
                return Convert.ToString(this.ViewState["UrlType"], CultureInfo.InvariantCulture);
            }

            set
            {
                if (value != null && !string.IsNullOrEmpty(value.Trim()))
                {
                    this.ViewState["UrlType"] = value;
                    if (this.IsTrackingViewState)
                    {
                        this.doChangeURL = true;
                    }
                }
            }
        }

        public string Width
        {
            get
            {
                return Convert.ToString(this.ViewState["SkinControlWidth"], CultureInfo.InvariantCulture);
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.cboUrls.Width = Unit.Parse(value, CultureInfo.InvariantCulture);
                    this.txtUrl.Width = Unit.Parse(value, CultureInfo.InvariantCulture);
                    this.cboImages.Width = Unit.Parse(value, CultureInfo.InvariantCulture);
                    this.cboTabs.Width = Unit.Parse(value, CultureInfo.InvariantCulture);
                    this.cboFolders.Width = Unit.Parse(value, CultureInfo.InvariantCulture);
                    this.cboFiles.Width = Unit.Parse(value, CultureInfo.InvariantCulture);
                    this.txtUser.Width = Unit.Parse(value, CultureInfo.InvariantCulture);
                    this.ViewState["SkinControlWidth"] = value;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            AJAX.RegisterPostBackControl(this.FindControl("cmdSave"));

            // prevent unauthorized access
            if (this.Request.IsAuthenticated == false)
            {
                this.Visible = false;
				Logger.LogDebug("OnInit(e={E}): this.Visible → {Arg1}", e, this.Visible);
            }
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);

            this.cboFolders.SelectedIndexChanged += this.cboFolders_SelectedIndexChanged;
            this.optType.SelectedIndexChanged += this.optType_SelectedIndexChanged;
            this.cmdAdd.Click += this.cmdAdd_Click;
            this.cmdCancel.Click += this.cmdCancel_Click;
            this.cmdDelete.Click += this.cmdDelete_Click;
            this.cmdSave.Click += this.cmdSave_Click;
            this.cmdSelect.Click += this.cmdSelect_Click;
            this.cmdUpload.Click += this.cmdUpload_Click;

            this.ErrorRow.Visible = false;

            try
            {
                if ((this.Request.QueryString["pid"] != null) && (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID) || UserController.Instance.GetCurrentUserInfo().IsSuperUser))
                {
                    this.objPortal = PortalController.Instance.GetPortal(int.Parse(this.Request.QueryString["pid"], CultureInfo.InvariantCulture));
					Logger.LogDebug("OnLoad(e={E}): objPortal → {ObjPortal}", e, objPortal);
                }
                else
                {
                    this.objPortal = PortalController.Instance.GetPortal(this.PortalSettings.PortalId);
                }

                if (this.ViewState["IsUrlControlLoaded"] == null)
                {
                    // If Not Page.IsPostBack Then
                    // let's make at least an initialization
                    // The type radio button must be initialized
                    // The url must be initialized no matter its value
                    this.doRenderTypes = true;
					Logger.LogDebug("OnLoad(e={E}): this.doRenderTypes → {Arg1}", e, this.doRenderTypes);
                    this.doChangeURL = true;
                    ClientAPI.AddButtonConfirm(this.cmdDelete, Localization.GetString("DeleteItem"));
					Logger.LogDebug("OnLoad(e={E}): this.doChangeURL → {Arg1}", e, this.doChangeURL);

                    // The following line was mover to the pre-render event to ensure render for the first time
                    ////ViewState("IsUrlControlLoaded") = "Loaded"
                }
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnLoad(e={E}): {Arg1}", e, ex.Message);
                Exceptions.ProcessModuleLoadException(this, exc);
            }
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            try
            {
                if (this.doRenderTypes)
                {
                    this.DoRenderTypes();
                }

                if (this.doChangeURL)
                {
                    this.DoChangeURL();
                }

                if (this.doReloadFolders || this.doReloadFiles)
                {
                    this.DoCorrectRadioButtonList();
                    this.doRenderTypeControls = true;
					Logger.LogDebug("OnPreRender(e={E}): this.doRenderTypeControls → {Arg1}", e, this.doRenderTypeControls);
                }

                if (this.doRenderTypeControls)
                {
                    if (!(this.doReloadFolders || this.doReloadFiles))
                    {
                        this.DoCorrectRadioButtonList();
                    }

                    this.DoRenderTypeControls();
                }

                this.ViewState["Url"] = null;
                this.ViewState["IsUrlControlLoaded"] = "Loaded";
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in OnPreRender(e={E}): {Arg1}", e, ex.Message);
                // Let's detect possible problems
                Exceptions.LogException(new RenderException("Error rendering URLControl subcontrols.", exc));
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cboFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cboFolders_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            int portalId = Null.NullInteger;

			Logger.LogDebug("cboFolders_SelectedIndexChanged(sender={Sender},e={E}): portalId → {PortalId}", sender, e, portalId);
            if (!this.IsHostMenu || this.Request.QueryString["pid"] != null)
            {
                portalId = this.objPortal.PortalID;
            }

            var objFolder = FolderManager.Instance.GetFolder(portalId, this.cboFolders.SelectedValue);
            if (FolderPermissionController.CanAddFolder((FolderInfo)objFolder))
			Logger.LogDebug("cboFolders_SelectedIndexChanged(sender={Sender},e={E}): objFolder → {ObjFolder}", sender, e, objFolder);
            {
                if (!this.txtFile.Visible)
                {
                    this.cmdSave.Visible = false;

                    // only show if not already in upload mode and not disabled
                    this.cmdUpload.Visible = this.ShowUpLoad;
                }
            }
            else
            {
                // reset controls
                this.cboFiles.Visible = true;
                this.cmdUpload.Visible = false;
                this.txtFile.Visible = false;
                this.cmdSave.Visible = false;
                this.cmdCancel.Visible = false;
            }

            this.cboFiles.Items.Clear();
            this.cboFiles.DataSource = this.GetFileList(!this.Required);
            this.cboFiles.DataBind();
            this.SetStorageLocationType();
            if (this.cboFolders.SelectedIndex >= 0)
            {
                this.ViewState["LastFolderPath"] = this.cboFolders.SelectedValue;
            }
            else
            {
                this.ViewState["LastFolderPath"] = string.Empty;
            }

            if (this.cboFiles.SelectedIndex >= 0)
            {
                this.ViewState["LastFileName"] = this.cboFiles.SelectedValue;
            }
            else
            {
                this.ViewState["LastFileName"] = string.Empty;
            }

            this.doRenderTypeControls = false; // Must not render on this postback
			Logger.LogDebug("cboFolders_SelectedIndexChanged(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
			Logger.LogDebug("cboFolders_SelectedIndexChanged(sender={Sender},e={E}): this.doChangeURL → {Arg2}", sender, e, this.doChangeURL);
			Logger.LogDebug("cboFolders_SelectedIndexChanged(sender={Sender},e={E}): this.doReloadFiles → {Arg2}", sender, e, this.doReloadFiles);
            this.doRenderTypes = false;
            this.doChangeURL = false;
			Logger.LogDebug("cboFolders_SelectedIndexChanged(sender={Sender},e={E}): this.doRenderTypes → {Arg2}", sender, e, this.doRenderTypes);
            this.doReloadFolders = false;
            this.doReloadFiles = false;
			Logger.LogDebug("<<< Exiting cboFolders_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
			Logger.LogDebug("cboFolders_SelectedIndexChanged(sender={Sender},e={E}): this.doReloadFolders → {Arg2}", sender, e, this.doReloadFolders);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdAdd_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cmdAdd_Click(sender={Sender},e={E})", sender, e);
            this.cboUrls.Visible = false;
            this.cmdSelect.Visible = true;
            this.txtUrl.Visible = true;
            this.cmdAdd.Visible = false;
            this.cmdDelete.Visible = false;
            this.doRenderTypeControls = false; // Must not render on this postback
			Logger.LogDebug("cmdAdd_Click(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
			Logger.LogDebug("cmdAdd_Click(sender={Sender},e={E}): this.doChangeURL → {Arg2}", sender, e, this.doChangeURL);
			Logger.LogDebug("cmdAdd_Click(sender={Sender},e={E}): this.doReloadFolders → {Arg2}", sender, e, this.doReloadFolders);
			Logger.LogDebug("cmdAdd_Click(sender={Sender},e={E}): this.doReloadFiles → {Arg2}", sender, e, this.doReloadFiles);
            this.doRenderTypes = false;
            this.doChangeURL = false;
			Logger.LogDebug("cmdAdd_Click(sender={Sender},e={E}): this.doRenderTypes → {Arg2}", sender, e, this.doRenderTypes);
            this.doReloadFolders = false;
            this.doReloadFiles = false;
			Logger.LogDebug("<<< Exiting cmdAdd_Click(sender={Sender},e={E})", sender, e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdCancel_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cmdCancel_Click(sender={Sender},e={E})", sender, e);
            this.cboFiles.Visible = true;
            this.cmdUpload.Visible = true;
            this.txtFile.Visible = false;
            this.cmdSave.Visible = false;
            this.cmdCancel.Visible = false;
            this.doRenderTypeControls = false; // Must not render on this postback
			Logger.LogDebug("cmdCancel_Click(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
			Logger.LogDebug("cmdCancel_Click(sender={Sender},e={E}): this.doChangeURL → {Arg2}", sender, e, this.doChangeURL);
			Logger.LogDebug("cmdCancel_Click(sender={Sender},e={E}): this.doReloadFolders → {Arg2}", sender, e, this.doReloadFolders);
			Logger.LogDebug("cmdCancel_Click(sender={Sender},e={E}): this.doReloadFiles → {Arg2}", sender, e, this.doReloadFiles);
            this.doRenderTypes = false;
            this.doChangeURL = false;
			Logger.LogDebug("cmdCancel_Click(sender={Sender},e={E}): this.doRenderTypes → {Arg2}", sender, e, this.doRenderTypes);
            this.doReloadFolders = false;
            this.doReloadFiles = false;
			Logger.LogDebug("<<< Exiting cmdCancel_Click(sender={Sender},e={E})", sender, e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdDelete_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cmdDelete_Click(sender={Sender},e={E})", sender, e);
            if (this.cboUrls.SelectedItem != null)
            {
                var objUrls = new UrlController();
                objUrls.DeleteUrl(this.objPortal.PortalID, this.cboUrls.SelectedItem.Value);
				Logger.LogDebug("cmdDelete_Click(sender={Sender},e={E}): objUrls → {ObjUrls}", sender, e, objUrls);
                this.LoadUrls(); // we must reload the url list
            }

            this.doRenderTypeControls = false; // Must not render on this postback
			Logger.LogDebug("cmdDelete_Click(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
			Logger.LogDebug("cmdDelete_Click(sender={Sender},e={E}): this.doChangeURL → {Arg2}", sender, e, this.doChangeURL);
			Logger.LogDebug("cmdDelete_Click(sender={Sender},e={E}): this.doReloadFolders → {Arg2}", sender, e, this.doReloadFolders);
			Logger.LogDebug("cmdDelete_Click(sender={Sender},e={E}): this.doReloadFiles → {Arg2}", sender, e, this.doReloadFiles);
            this.doRenderTypes = false;
            this.doChangeURL = false;
			Logger.LogDebug("cmdDelete_Click(sender={Sender},e={E}): this.doRenderTypes → {Arg2}", sender, e, this.doRenderTypes);
            this.doReloadFolders = false;
            this.doReloadFiles = false;
			Logger.LogDebug("<<< Exiting cmdDelete_Click(sender={Sender},e={E})", sender, e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdSave_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cmdSave_Click(sender={Sender},e={E})", sender, e);
            this.cmdUpload.Visible = false;

            // if no file is selected exit
            if (string.IsNullOrEmpty(this.txtFile.PostedFile.FileName))
            {
                return;
            }

            string parentFolderName;
            if (Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID))
            {
                parentFolderName = Globals.HostMapPath;
				Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): parentFolderName → {ParentFolderName}", sender, e, parentFolderName);
            }
            else
            {
                parentFolderName = this.PortalSettings.HomeDirectoryMapPath;
            }

            parentFolderName += this.cboFolders.SelectedItem.Value;

            string strExtension = Path.GetExtension(this.txtFile.PostedFile.FileName).Replace(".", string.Empty);
            if (!string.IsNullOrEmpty(this.FileFilter) && !("," + this.FileFilter).Contains("," + strExtension, StringComparison.OrdinalIgnoreCase))
            {
                // trying to upload a file not allowed for current filter
                this.lblMessage.Text = string.Format(CultureInfo.CurrentCulture, Localization.GetString("UploadError", this.LocalResourceFile), this.FileFilter, strExtension);
                this.ErrorRow.Visible = true;
            }
            else
            {
                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;

                var settings = PortalController.Instance.GetCurrentSettings();
                var currentPage = TabController.CurrentPage;
				Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): settings → {Settings}", sender, e, settings);
                var portalId = (currentPage.ParentId == settings.SuperTabId) ? Null.NullInteger : settings.PortalId;

				Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): portalId → {PortalId}", sender, e, portalId);
                var fileName = Path.GetFileName(this.txtFile.PostedFile.FileName);
                var folderPath = Globals.GetSubFolderPath(parentFolderName.Replace("/", @"\") + fileName, portalId);
				Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): fileName → {FileName}", sender, e, fileName);
				Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): folderPath → {FolderPath}", sender, e, folderPath);

                var folder = folderManager.GetFolder(portalId, folderPath);
                this.ErrorRow.Visible = false;
				Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): folder → {Folder}", sender, e, folder);

                try
                {
                    fileManager.AddFile(folder, fileName, this.txtFile.PostedFile.InputStream, true, true, ((FileManager)fileManager).GetContentType(Path.GetExtension(fileName)));
                }
                catch (Services.FileSystem.PermissionsNotMetException)
                {
					Logger.LogError("Exception in cmdSave_Click(sender={Sender},e={E}): {Arg2}", sender, e, ex.Message);
                    this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("InsufficientFolderPermission"), folder.FolderPath);
                    this.ErrorRow.Visible = true;
                }
                catch (NoSpaceAvailableException)
                {
                    this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("DiskSpaceExceeded"), fileName);
                    this.ErrorRow.Visible = true;
                }
                catch (InvalidFileExtensionException)
                {
                    this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("RestrictedFileType"), fileName, Host.AllowedExtensionWhitelist.ToDisplayString());
                    this.ErrorRow.Visible = true;
                }
                catch (Exception)
                {
                    this.lblMessage.Text += "<br />" + string.Format(CultureInfo.CurrentCulture, Localization.GetString("SaveFileError"), fileName);
                    this.ErrorRow.Visible = true;
                }
            }

            if (this.lblMessage.Text == string.Empty)
            {
                this.cboFiles.Visible = true;
                this.cmdUpload.Visible = this.ShowUpLoad;
                this.txtFile.Visible = false;
                this.cmdSave.Visible = false;
                this.cmdCancel.Visible = false;
                this.ErrorRow.Visible = false;

                var root = new DirectoryInfo(parentFolderName);
                this.cboFiles.Items.Clear();
				Logger.LogWarning("cmdSave_Click(sender={Sender},e={E}): root → {Root}", sender, e, root);
                this.cboFiles.DataSource = this.GetFileList(false);
                this.cboFiles.DataBind();

                string fileName1 = this.txtFile.PostedFile.FileName.Substring(this.txtFile.PostedFile.FileName.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
                if (this.cboFiles.Items.FindByText(fileName1) != null)
                {
                    this.cboFiles.Items.FindByText(fileName1).Selected = true;
                }

                if (this.cboFiles.SelectedIndex >= 0)
                {
                    this.ViewState["LastFileName"] = this.cboFiles.SelectedValue;
                }
                else
                {
                    this.ViewState["LastFileName"] = string.Empty;
                }
            }

            this.doRenderTypeControls = false; // Must not render on this postback
			Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
			Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): this.doChangeURL → {Arg2}", sender, e, this.doChangeURL);
			Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): this.doReloadFolders → {Arg2}", sender, e, this.doReloadFolders);
			Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): this.doReloadFiles → {Arg2}", sender, e, this.doReloadFiles);
            this.doRenderTypes = false;
            this.doChangeURL = false;
			Logger.LogDebug("cmdSave_Click(sender={Sender},e={E}): this.doRenderTypes → {Arg2}", sender, e, this.doRenderTypes);
            this.doReloadFolders = false;
            this.doReloadFiles = false;
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdSelect_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cmdSelect_Click(sender={Sender},e={E})", sender, e);
            this.cboUrls.Visible = true;
            this.cmdSelect.Visible = false;
            this.txtUrl.Visible = false;
            this.cmdAdd.Visible = true;
            this.cmdDelete.Visible = PortalSecurity.IsInRole(this.objPortal.AdministratorRoleName);
            this.LoadUrls();
            if (this.cboUrls.Items.FindByValue(this.txtUrl.Text) != null)
            {
                this.cboUrls.ClearSelection();
                this.cboUrls.Items.FindByValue(this.txtUrl.Text).Selected = true;
            }

            this.doRenderTypeControls = false; // Must not render on this postback
			Logger.LogDebug("cmdSelect_Click(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
			Logger.LogDebug("cmdSelect_Click(sender={Sender},e={E}): this.doChangeURL → {Arg2}", sender, e, this.doChangeURL);
			Logger.LogDebug("cmdSelect_Click(sender={Sender},e={E}): this.doReloadFolders → {Arg2}", sender, e, this.doReloadFolders);
			Logger.LogDebug("cmdSelect_Click(sender={Sender},e={E}): this.doReloadFiles → {Arg2}", sender, e, this.doReloadFiles);
            this.doRenderTypes = false;
            this.doChangeURL = false;
			Logger.LogDebug("cmdSelect_Click(sender={Sender},e={E}): this.doRenderTypes → {Arg2}", sender, e, this.doRenderTypes);
            this.doReloadFolders = false;
            this.doReloadFiles = false;
			Logger.LogDebug("<<< Exiting cmdSelect_Click(sender={Sender},e={E})", sender, e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void cmdUpload_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering cmdUpload_Click(sender={Sender},e={E})", sender, e);
            string strSaveFolder = this.cboFolders.SelectedValue;
            this.LoadFolders("WRITE");
            if (this.cboFolders.Items.FindByValue(strSaveFolder) != null)
            {
                this.cboFolders.Items.FindByValue(strSaveFolder).Selected = true;
                this.cboFiles.Visible = false;
                this.cmdUpload.Visible = false;
                this.txtFile.Visible = true;
                this.cmdSave.Visible = true;
                this.cmdCancel.Visible = true;
            }
            else
            {
                if (this.cboFolders.Items.Count > 0)
                {
                    this.cboFolders.Items[0].Selected = true;
                    this.cboFiles.Visible = false;
                    this.cmdUpload.Visible = false;
                    this.txtFile.Visible = true;
                    this.cmdSave.Visible = true;
                    this.cmdCancel.Visible = true;
                }
                else
                {
                    // reset controls
                    this.LoadFolders("BROWSE,WRITE");
                    this.cboFolders.Items.FindByValue(strSaveFolder).Selected = true;
                    this.cboFiles.Visible = true;
                    this.cmdUpload.Visible = false;
                    this.txtFile.Visible = false;
                    this.cmdSave.Visible = false;
                    this.cmdCancel.Visible = false;
                    this.lblMessage.Text = Localization.GetString("NoWritePermission", this.LocalResourceFile);
                    this.ErrorRow.Visible = true;
                }
            }

            this.doRenderTypeControls = false; // Must not render on this postback
			Logger.LogDebug("cmdUpload_Click(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
			Logger.LogDebug("cmdUpload_Click(sender={Sender},e={E}): this.doChangeURL → {Arg2}", sender, e, this.doChangeURL);
			Logger.LogDebug("cmdUpload_Click(sender={Sender},e={E}): this.doReloadFolders → {Arg2}", sender, e, this.doReloadFolders);
			Logger.LogDebug("cmdUpload_Click(sender={Sender},e={E}): this.doReloadFiles → {Arg2}", sender, e, this.doReloadFiles);
            this.doRenderTypes = false;
            this.doChangeURL = false;
			Logger.LogDebug("cmdUpload_Click(sender={Sender},e={E}): this.doRenderTypes → {Arg2}", sender, e, this.doRenderTypes);
            this.doReloadFolders = false;
            this.doReloadFiles = false;
			Logger.LogDebug("<<< Exiting cmdUpload_Click(sender={Sender},e={E})", sender, e);
        }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        protected void optType_SelectedIndexChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering optType_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
            // Type changed, render the correct control set
            this.ViewState["UrlType"] = this.optType.SelectedItem.Value;
            this.doRenderTypeControls = true;
			Logger.LogDebug("<<< Exiting optType_SelectedIndexChanged(sender={Sender},e={E})", sender, e);
			Logger.LogDebug("optType_SelectedIndexChanged(sender={Sender},e={E}): this.doRenderTypeControls → {Arg2}", sender, e, this.doRenderTypeControls);
        }

        private ArrayList GetFileList(bool noneSpecified)
        {
			Logger.LogDebug(">>> Entering GetFileList(noneSpecified={NoneSpecified})", noneSpecified);
            int portalId = Null.NullInteger;

			Logger.LogDebug("GetFileList(noneSpecified={NoneSpecified}): portalId → {PortalId}", noneSpecified, portalId);
            if ((!this.IsHostMenu) || (this.Request.QueryString["pid"] != null))
            {
                portalId = this.objPortal.PortalID;
            }

			Logger.LogDebug("<<< Exiting GetFileList(noneSpecified={NoneSpecified})", noneSpecified);
            return Globals.GetFileList(portalId, this.FileFilter, noneSpecified, this.cboFolders.SelectedItem.Value, false);
        }

        private void LoadFolders(string permissions)
        {
			Logger.LogDebug(">>> Entering LoadFolders(permissions={Permissions})", permissions);
            int portalId = Null.NullInteger;
            this.cboFolders.Items.Clear();
			Logger.LogDebug("LoadFolders(permissions={Permissions}): portalId → {PortalId}", permissions, portalId);

            if ((!this.IsHostMenu) || (this.Request.QueryString["pid"] != null))
            {
                portalId = this.objPortal.PortalID;
            }

            var folders = FolderManager.Instance.GetFolders(UserController.Instance.GetCurrentUserInfo(), permissions);
            foreach (FolderInfo folder in folders)
            {
			Logger.LogDebug("LoadFolders(permissions={Permissions}): folders → {Folders}", permissions, folders);
                var folderItem = new ListItem();
                if (folder.FolderPath == Null.NullString)
				Logger.LogDebug("LoadFolders(permissions={Permissions}): folderItem → {FolderItem}", permissions, folderItem);
                {
                    folderItem.Text = Localization.GetString("Root", this.LocalResourceFile);
                }
                else
                {
                    folderItem.Text = folder.DisplayPath;
					Logger.LogDebug("LoadFolders(permissions={Permissions}): folderItem.Text → {Arg1}", permissions, folderItem.Text);
                }

                folderItem.Value = folder.FolderPath;
				Logger.LogDebug("LoadFolders(permissions={Permissions}): folderItem.Value → {Arg1}", permissions, folderItem.Value);
                this.cboFolders.Items.Add(folderItem);
            }
			Logger.LogDebug("<<< Exiting LoadFolders(permissions={Permissions})", permissions);
        }

        private void LoadUrls()
        {
			Logger.LogDebug(">>> Entering LoadUrls()");
            var objUrls = new UrlController();
            this.cboUrls.Items.Clear();
			Logger.LogDebug("LoadUrls(): objUrls → {ObjUrls}", objUrls);
            this.cboUrls.DataSource = objUrls.GetUrls(this.objPortal.PortalID);
            this.cboUrls.DataBind();
			Logger.LogDebug("<<< Exiting LoadUrls()");
        }

        private void SetStorageLocationType()
        {
			Logger.LogDebug(">>> Entering SetStorageLocationType()");
            string folderName = this.cboFolders.SelectedValue;

			Logger.LogDebug("SetStorageLocationType(): folderName → {FolderName}", folderName);
            // Check to see if this is the 'Root' folder, if so we cannot rely on its text value because it is something and not an empty string that we need to lookup the 'root' folder
            if (this.cboFolders.SelectedValue == string.Empty)
            {
                folderName = string.Empty;
            }

            var objFolderInfo = FolderManager.Instance.GetFolder(this.PortalSettings.PortalId, folderName);
            if (objFolderInfo != null)
			Logger.LogDebug("SetStorageLocationType(): objFolderInfo → {ObjFolderInfo}", objFolderInfo);
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(objFolderInfo.PortalID, objFolderInfo.FolderMappingID);
                if (folderMapping.MappingName == "Standard")
				Logger.LogDebug("SetStorageLocationType(): folderMapping → {FolderMapping}", folderMapping);
                {
                    this.imgStorageLocationType.Visible = false;
                }
                else
                {
                    this.imgStorageLocationType.Visible = true;
                    this.imgStorageLocationType.ImageUrl = FolderProvider.Instance(folderMapping.FolderProviderType).GetFolderProviderIconPath();
                }
            }
			Logger.LogDebug("<<< Exiting SetStorageLocationType()");
        }

        private void DoChangeURL()
        {
			Logger.LogDebug(">>> Entering DoChangeURL()");
            string url = Convert.ToString(this.ViewState["Url"], CultureInfo.InvariantCulture);
            string urltype = Convert.ToString(this.ViewState["UrlType"], CultureInfo.InvariantCulture);
			Logger.LogDebug("DoChangeURL(): url → {Url}", url);
			Logger.LogDebug("DoChangeURL(): urltype → {Urltype}", urltype);
            if (!string.IsNullOrEmpty(url))
            {
                var objUrls = new UrlController();
                string trackingUrl = url;
				Logger.LogDebug("DoChangeURL(): objUrls → {ObjUrls}", objUrls);
				Logger.LogDebug("DoChangeURL(): trackingUrl → {TrackingUrl}", trackingUrl);

                urltype = Globals.GetURLType(url).ToString("g").Substring(0, 1);
                if (urltype == "U" && url.StartsWith("~/" + this.PortalSettings.DefaultIconLocation, StringComparison.InvariantCultureIgnoreCase))
                {
                    urltype = "I";
                }

                this.ViewState["UrlType"] = urltype;
                if (urltype == "F")
                {
                    if (url.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        trackingUrl = url;
                        var objFile = FileManager.Instance.GetFile(int.Parse(url.Substring(7), CultureInfo.InvariantCulture));
						Logger.LogDebug("DoChangeURL(): objFile → {ObjFile}", objFile);
                        if (objFile != null)
                        {
                            url = objFile.Folder + objFile.FileName;
                        }
                    }
                    else
                    {
                        // to handle legacy scenarios before the introduction of the FileServerHandler
                        var fileName = Path.GetFileName(url);
                        var folderPath = url.Substring(0, url.LastIndexOf(fileName, StringComparison.OrdinalIgnoreCase));
						Logger.LogDebug("DoChangeURL(): fileName → {FileName}", fileName);
						Logger.LogDebug("DoChangeURL(): folderPath → {FolderPath}", folderPath);
                        var folder = FolderManager.Instance.GetFolder(this.objPortal.PortalID, folderPath);
                        var fileId = -1;
						Logger.LogDebug("DoChangeURL(): folder → {Folder}", folder);
						Logger.LogDebug("DoChangeURL(): fileId → {FileId}", fileId);
                        if (folder != null)
                        {
                            var file = FileManager.Instance.GetFile(folder, fileName);
                            if (file != null)
							Logger.LogDebug("DoChangeURL(): file → {File}", file);
                            {
                                fileId = file.FileId;
                            }
                        }

                        trackingUrl = "FileID=" + fileId.ToString(CultureInfo.InvariantCulture);
                    }
                }

                if (urltype == "M")
                {
                    if (url.StartsWith("userid=", StringComparison.InvariantCultureIgnoreCase))
                    {
                        UserInfo objUser = UserController.GetUserById(this.objPortal.PortalID, int.Parse(url.Substring(7), CultureInfo.InvariantCulture));
                        if (objUser != null)
						Logger.LogDebug("DoChangeURL(): objUser → {ObjUser}", objUser);
                        {
                            url = objUser.Username;
                        }
                    }
                }

                UrlTrackingInfo objUrlTracking = objUrls.GetUrlTracking(this.objPortal.PortalID, trackingUrl, this.ModuleID);
                if (objUrlTracking != null)
				Logger.LogDebug("DoChangeURL(): objUrlTracking → {ObjUrlTracking}", objUrlTracking);
                {
                    this.chkNewWindow.Checked = objUrlTracking.NewWindow;
                    this.chkTrack.Checked = objUrlTracking.TrackClicks;
                    this.chkLog.Checked = objUrlTracking.LogActivity;
                }
                else
                {
                    // the url does not exist in the tracking table
                    this.chkTrack.Checked = false;
                    this.chkLog.Checked = false;
                }

                this.ViewState["Url"] = url;
            }
            else
            {
                if (!string.IsNullOrEmpty(urltype))
                {
                    this.optType.ClearSelection();
                    if (this.optType.Items.FindByValue(urltype) != null)
                    {
                        this.optType.Items.FindByValue(urltype).Selected = true;
                    }
                    else
                    {
                        this.optType.Items[0].Selected = true;
                    }
                }
                else
                {
                    if (this.optType.Items.Count > 0)
                    {
                        this.optType.ClearSelection();
                        this.optType.Items[0].Selected = true;
                    }
                }

                this.chkNewWindow.Checked = false; // Need check
                this.chkTrack.Checked = false; // Need check
                this.chkLog.Checked = false; // Need check
            }

            // Url type changed, then we must draw the controlos for that type
            this.doRenderTypeControls = true;
			Logger.LogDebug("<<< Exiting DoChangeURL()");
			Logger.LogDebug("DoChangeURL(): this.doRenderTypeControls → {Arg0}", this.doRenderTypeControls);
        }

        private void DoRenderTypes()
        {
			Logger.LogDebug(">>> Entering DoRenderTypes()");
            // We must clear the list to keep the same item order
            string strCurrent = string.Empty;
            if (this.optType.SelectedIndex >= 0)
			Logger.LogDebug("DoRenderTypes(): strCurrent → {StrCurrent}", strCurrent);
            {
                strCurrent = this.optType.SelectedItem.Value; // Save current selected value
            }

            this.optType.Items.Clear();
            if (this.ShowNone)
            {
                if (this.optType.Items.FindByValue("N") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("NoneType", this.LocalResourceFile), "N"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("N") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("N"));
                }
            }

            if (this.ShowUrls)
            {
                if (this.optType.Items.FindByValue("U") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("URLType", this.LocalResourceFile), "U"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("U") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("U"));
                }
            }

            if (this.ShowTabs)
            {
                if (this.optType.Items.FindByValue("T") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("TabType", this.LocalResourceFile), "T"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("T") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("T"));
                }
            }

            if (this.ShowFiles)
            {
                if (this.optType.Items.FindByValue("F") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("FileType", this.LocalResourceFile), "F"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("F") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("F"));
                }
            }

            if (this.ShowImages)
            {
                if (this.optType.Items.FindByValue("I") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("ImageType", this.LocalResourceFile), "I"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("I") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("I"));
                }
            }

            if (this.ShowUsers)
            {
                if (this.optType.Items.FindByValue("M") == null)
                {
                    this.optType.Items.Add(new ListItem(Localization.GetString("UserType", this.LocalResourceFile), "M"));
                }
            }
            else
            {
                if (this.optType.Items.FindByValue("M") != null)
                {
                    this.optType.Items.Remove(this.optType.Items.FindByValue("M"));
                }
            }

            if (this.optType.Items.Count > 0)
            {
                if (!string.IsNullOrEmpty(strCurrent))
                {
                    if (this.optType.Items.FindByValue(strCurrent) != null)
                    {
                        this.optType.Items.FindByValue(strCurrent).Selected = true;
                    }
                    else
                    {
                        this.optType.Items[0].Selected = true;
                        this.doRenderTypeControls = true; // Type changed, re-draw
						Logger.LogDebug("DoRenderTypes(): this.doRenderTypeControls → {Arg0}", this.doRenderTypeControls);
                    }
                }
                else
                {
                    this.optType.Items[0].Selected = true;
                    this.doRenderTypeControls = true; // Type changed, re-draw
                }

                this.TypeRow.Visible = this.optType.Items.Count > 1;
            }
            else
            {
                this.TypeRow.Visible = false;
            }
			Logger.LogDebug("<<< Exiting DoRenderTypes()");
        }

        private void DoCorrectRadioButtonList()
        {
			Logger.LogDebug(">>> Entering DoCorrectRadioButtonList()");
            string urltype = Convert.ToString(this.ViewState["UrlType"], CultureInfo.InvariantCulture);

			Logger.LogDebug("DoCorrectRadioButtonList(): urltype → {Urltype}", urltype);
            if (this.optType.Items.Count > 0)
            {
                this.optType.ClearSelection();
                if (!string.IsNullOrEmpty(urltype))
                {
                    if (this.optType.Items.FindByValue(urltype) != null)
                    {
                        this.optType.Items.FindByValue(urltype).Selected = true;
                    }
                    else
                    {
                        this.optType.Items[0].Selected = true;
                        urltype = this.optType.Items[0].Value;
                        this.ViewState["UrlType"] = urltype;
                    }
                }
                else
                {
                    this.optType.Items[0].Selected = true;
                    urltype = this.optType.Items[0].Value;
                    this.ViewState["UrlType"] = urltype;
                }
            }
			Logger.LogDebug("<<< Exiting DoCorrectRadioButtonList()");
        }

        private void DoRenderTypeControls()
        {
			Logger.LogDebug(">>> Entering DoRenderTypeControls()");
            string url = Convert.ToString(this.ViewState["Url"], CultureInfo.InvariantCulture);
            string urltype = Convert.ToString(this.ViewState["UrlType"], CultureInfo.InvariantCulture);
			Logger.LogDebug("DoRenderTypeControls(): url → {Url}", url);
			Logger.LogDebug("DoRenderTypeControls(): urltype → {Urltype}", urltype);
            var objUrls = new UrlController();
            if (!string.IsNullOrEmpty(urltype))
			Logger.LogDebug("DoRenderTypeControls(): objUrls → {ObjUrls}", objUrls);
            {
                // load listitems
                switch (this.optType.SelectedItem.Value)
                {
                    case "N": // None
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;
                        break;
                    case "I": // System Image
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = true;

                        this.cboImages.Items.Clear();

                        string strImagesFolder = Path.Combine(Globals.ApplicationMapPath, this.PortalSettings.DefaultIconLocation.Replace('/', '\\'));
                        foreach (string strImage in Directory.GetFiles(strImagesFolder))
                        {
                            string img = strImage.Replace(strImagesFolder, string.Empty).Trim('/').Trim('\\');
                            this.cboImages.Items.Add(new ListItem(img, $"~/{this.PortalSettings.DefaultIconLocation}/{img}".ToLowerInvariant()));
                        }

                        var selectedItem = this.cboImages.Items.FindByValue(url.ToLowerInvariant());
                        selectedItem?.Selected = true;
						Logger.LogDebug("DoRenderTypeControls(): selectedItem → {SelectedItem}", selectedItem);

                        break;

                    case "U": // Url
                        this.URLRow.Visible = true;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;
                        if (string.IsNullOrEmpty(this.txtUrl.Text))
                        {
                            this.txtUrl.Text = url;
                        }

                        if (string.IsNullOrEmpty(this.txtUrl.Text))
                        {
                            this.txtUrl.Text = "http://";
                        }

                        this.txtUrl.Visible = true;

                        this.cmdSelect.Visible = true;

                        this.cboUrls.Visible = false;
                        this.cmdAdd.Visible = false;
                        this.cmdDelete.Visible = false;
                        break;
                    case "T": // tab
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = true;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;

                        this.cboTabs.Items.Clear();

                        var currentPage = TabController.CurrentPage;
                        var settings = PortalController.Instance.GetCurrentSettings();
                        this.cboTabs.DataSource = TabController.GetPortalTabs(settings.PortalId, Null.NullInteger, !this.Required, "none available", true, false, false, true, false);
						Logger.LogDebug("DoRenderTypeControls(): settings → {Settings}", settings);
                        this.cboTabs.DataBind();
                        if (this.cboTabs.Items.FindByValue(url) != null)
                        {
                            this.cboTabs.Items.FindByValue(url).Selected = true;
                        }

                        if (!this.IncludeActiveTab && this.cboTabs.Items.FindByValue(currentPage.TabID.ToString(CultureInfo.InvariantCulture)) != null)
                        {
                            this.cboTabs.Items.FindByValue(currentPage.TabID.ToString(CultureInfo.InvariantCulture)).Attributes.Add("disabled", "disabled");
                        }

                        break;
                    case "F": // file
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = true;
                        this.UserRow.Visible = false;
                        this.ImagesRow.Visible = false;

                        if (this.ViewState["FoldersLoaded"] == null || this.doReloadFolders)
                        {
                            this.LoadFolders("BROWSE,WRITE");
                            this.ViewState["FoldersLoaded"] = "Y";
                        }

                        if (this.cboFolders.Items.Count == 0)
                        {
                            this.lblMessage.Text = Localization.GetString("NoPermission", this.LocalResourceFile);
                            this.ErrorRow.Visible = true;
                            this.FileRow.Visible = false;
                            return;
                        }

                        // select folder
                        // We Must check if selected folder has changed because of a property change (Secure, Database)
                        string fileName = string.Empty;
                        string folderPath = string.Empty;
						Logger.LogDebug("DoRenderTypeControls(): fileName → {FileName}", fileName);
						Logger.LogDebug("DoRenderTypeControls(): folderPath → {FolderPath}", folderPath);
                        string lastFileName = string.Empty;
                        string lastFolderPath = string.Empty;
						Logger.LogDebug("DoRenderTypeControls(): lastFileName → {LastFileName}", lastFileName);
						Logger.LogDebug("DoRenderTypeControls(): lastFolderPath → {LastFolderPath}", lastFolderPath);
                        bool mustRedrawFiles = false;

						Logger.LogDebug("DoRenderTypeControls(): mustRedrawFiles → {MustRedrawFiles}", mustRedrawFiles);
                        // Let's try to remember last selection
                        if (this.ViewState["LastFolderPath"] != null)
                        {
                            lastFolderPath = Convert.ToString(this.ViewState["LastFolderPath"], CultureInfo.InvariantCulture);
                        }

                        if (this.ViewState["LastFileName"] != null)
                        {
                            lastFileName = Convert.ToString(this.ViewState["LastFileName"], CultureInfo.InvariantCulture);
                        }

                        if (url != string.Empty)
                        {
                            // Let's use the new URL
                            fileName = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                            folderPath = url.Replace(fileName, string.Empty);
                        }
                        else
                        {
                            // Use last settings
                            fileName = lastFileName;
                            folderPath = lastFolderPath;
                        }

                        if (this.cboFolders.Items.FindByValue(folderPath) != null)
                        {
                            this.cboFolders.ClearSelection();
                            this.cboFolders.Items.FindByValue(folderPath).Selected = true;
                        }
                        else if (this.cboFolders.Items.Count > 0)
                        {
                            this.cboFolders.ClearSelection();
                            this.cboFolders.Items[0].Selected = true;
                            folderPath = this.cboFolders.Items[0].Value;
                        }

                        if (this.ViewState["FilesLoaded"] == null || folderPath != lastFolderPath || this.doReloadFiles)
                        {
                            // Reload files only if property change or not same folder
                            mustRedrawFiles = true;
                            this.ViewState["FilesLoaded"] = "Y";
                        }
                        else
                        {
                            if (this.cboFiles.Items.Count > 0)
                            {
                                if ((this.Required && string.IsNullOrEmpty(this.cboFiles.Items[0].Value)) || (!this.Required && !string.IsNullOrEmpty(this.cboFiles.Items[0].Value)))
                                {
                                    // Required state has changed, so we need to reload files
                                    mustRedrawFiles = true;
                                }
                            }
                            else if (!this.Required)
                            {
                                // Required state has changed, so we need to reload files
                                mustRedrawFiles = true;
                            }
                        }

                        if (mustRedrawFiles)
                        {
                            this.cboFiles.DataSource = this.GetFileList(!this.Required);
                            this.cboFiles.DataBind();
                            if (this.cboFiles.Items.FindByText(fileName) != null)
                            {
                                this.cboFiles.ClearSelection();
                                this.cboFiles.Items.FindByText(fileName).Selected = true;
                            }
                        }

                        this.cboFiles.Visible = true;
                        this.txtFile.Visible = false;

                        FolderInfo objFolder = (FolderInfo)FolderManager.Instance.GetFolder(this.objPortal.PortalID, folderPath);
                        this.cmdUpload.Visible = this.ShowUpLoad && FolderPermissionController.CanAddFolder(objFolder);
						Logger.LogDebug("DoRenderTypeControls(): objFolder → {ObjFolder}", objFolder);

                        this.SetStorageLocationType();
                        this.txtUrl.Visible = false;
                        this.cmdSave.Visible = false;
                        this.cmdCancel.Visible = false;

                        if (this.cboFolders.SelectedIndex >= 0)
                        {
                            this.ViewState["LastFolderPath"] = this.cboFolders.SelectedValue;
                        }
                        else
                        {
                            this.ViewState["LastFolderPath"] = string.Empty;
                        }

                        if (this.cboFiles.SelectedIndex >= 0)
                        {
                            this.ViewState["LastFileName"] = this.cboFiles.SelectedValue;
                        }
                        else
                        {
                            this.ViewState["LastFileName"] = string.Empty;
                        }

                        break;
                    case "M": // membership users
                        this.URLRow.Visible = false;
                        this.TabRow.Visible = false;
                        this.FileRow.Visible = false;
                        this.UserRow.Visible = true;
                        this.ImagesRow.Visible = false;
                        if (string.IsNullOrEmpty(this.txtUser.Text))
                        {
                            this.txtUser.Text = url;
                        }

                        break;
						Logger.LogDebug("<<< Exiting cmdSave_Click(sender={Sender},e={E})", sender, e);
                }
            }
            else
            {
                this.URLRow.Visible = false;
                this.ImagesRow.Visible = false;
                this.TabRow.Visible = false;
                this.FileRow.Visible = false;
                this.UserRow.Visible = false;
            }
        }
    }
}
