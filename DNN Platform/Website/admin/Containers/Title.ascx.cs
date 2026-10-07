// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers
{
    using System;
    using System.Net;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.WebControls;

    /// <summary>A container object which displays the module's title.</summary>
    public partial class Title : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Title.ascx));
        private const string MyFileName = "Title.ascx";
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="Title"/> class.</summary>
        public Title()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Title"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public Title(IHostSettings hostSettings)
        {
			Logger.LogError(">>> Entering Title(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings;
			Logger.LogDebug("Title(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        Logger.LogWarning("<<< Exiting Title(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Gets or sets the CSS class applied to the element containing the title.</summary>
        public string CssClass { get; set; }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.titleLabel.UpdateLabel += this.UpdateTitle;
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            // public attributes
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.titleLabel.CssClass = this.CssClass;
            }

            string moduleTitle = Null.NullString;
			Logger.LogError("OnPreRender(e={E}): moduleTitle → {ModuleTitle}", e, moduleTitle);
            if (this.ModuleControl != null)
            {
                moduleTitle = Localization.LocalizeControlTitle(this.ModuleControl);
            }

            if (moduleTitle == Null.NullString)
            {
                moduleTitle = " ";
            }

            if (this.hostSettings.AllowRichTextModuleTitle)
            {
                this.titleLabel.Text = moduleTitle;
            }
            else
            {
                this.titleLabel.Text = WebUtility.HtmlEncode(moduleTitle);
            }

            this.titleLabel.EditEnabled = false;
            this.titleToolbar.Visible = false;

            if (this.CanEditModule() && this.PortalSettings.InlineEditorEnabled)
            {
                this.titleLabel.EditEnabled = true;
                this.titleToolbar.Visible = true;
            }
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        private bool CanEditModule()
        {
			Logger.LogError(">>> Entering CanEditModule()");
            var canEdit = false;
			Logger.LogError("CanEditModule(): canEdit → {CanEdit}", canEdit);
            if (this.ModuleControl != null && this.ModuleControl.ModuleContext.ModuleId > Null.NullInteger)
            {
                canEdit = (Personalization.GetUserMode() == PortalSettings.Mode.Edit) && TabPermissionController.CanAdminPage() && !Globals.IsAdminControl();
            }

			Logger.LogWarning("<<< Exiting CanEditModule()");
            return canEdit;
        }

        private void UpdateTitle(object source, DNNLabelEditEventArgs e)
        {
			Logger.LogError(">>> Entering UpdateTitle(source={Source},e={E})", source, e);
            if (this.CanEditModule())
            {
                ModuleInfo moduleInfo = ModuleController.Instance.GetModule(this.ModuleControl.ModuleContext.ModuleId, this.ModuleControl.ModuleContext.TabId, false);
				Logger.LogError("UpdateTitle(source={Source},e={E}): moduleInfo → {ModuleInfo}", source, e, moduleInfo);

                if (this.hostSettings.AllowRichTextModuleTitle)
                {
#pragma warning disable CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated
                    var ps = PortalSecurity.Instance;
                    var mt = ps.InputFilter(e.Text, PortalSecurity.FilterFlag.NoScripting);
					Logger.LogError("UpdateTitle(source={Source},e={E}): mt → {Mt}", source, e, mt);
                    moduleInfo.ModuleTitle = mt;
#pragma warning restore CS0618 // PortalSecurity.FilterFlag.NoScripting is deprecated
				Logger.LogDebug("UpdateTitle(source={Source},e={E}): moduleInfo.ModuleTitle → {Arg2}", source, e, moduleInfo.ModuleTitle);
                }
                else
                {
                    moduleInfo.ModuleTitle = WebUtility.HtmlDecode(e.Text);
                }

                ModuleController.Instance.UpdateModule(moduleInfo);
            }
        }
    }
}
