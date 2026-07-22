// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI.Modules;

    /// <summary>ActionLink provides a button for a single action.</summary>
    /// <remarks>
    /// ActionBase inherits from HyperLink.
    /// </remarks>
    public class ActionLink : HyperLink
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ActionLink"/> class.</summary>
        public ActionLink()
        {
			Logger.LogDebug(">>> Entering ActionLink()");
            this.RequireEditMode = false;
            this.Security = "Edit";
			Logger.LogDebug("ActionLink(): this.RequireEditMode → {Arg0}", this.RequireEditMode);
			Logger.LogDebug("ActionLink(): this.Security → {Arg0}", this.Security);
            this.ControlKey = string.Empty;
            this.Title = string.Empty;
			Logger.LogDebug("<<< Exiting ActionLink()");
			Logger.LogDebug("ActionLink(): this.ControlKey → {Arg0}", this.ControlKey);
			Logger.LogDebug("ActionLink(): this.Title → {Arg0}", this.Title);
        }

        public string Title { get; set; }

        public string ControlKey { get; set; }

        public string KeyName { get; set; }

        public string KeyValue { get; set; }

        public string Security { get; set; }

        public bool RequireEditMode { get; set; }

        public IModuleControl ModuleControl { get; set; }

        /// <summary>CreateChildControls builds the control tree.</summary>
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            // Call base class method to ensure Control Tree is built
            base.CreateChildControls();

            // Set Causes Validation and Enables ViewState to false
            this.EnableViewState = false;
			Logger.LogDebug("<<< Exiting CreateChildControls()");
			Logger.LogDebug("CreateChildControls(): this.EnableViewState → {Arg0}", this.EnableViewState);
        }

        /// <summary>OnPreRender runs when just before the Render phase of the Page Lifecycle.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (this.Visible && this.IsVisible((SecurityAccessLevel)Enum.Parse(typeof(SecurityAccessLevel), this.Security)))
            {
                this.Text = this.Title;
				Logger.LogDebug("OnPreRender(e={E}): this.Text → {Arg1}", e, this.Text);
                this.NavigateUrl = this.ControlKey != string.Empty
                                ? this.ModuleControl.ModuleContext.EditUrl(this.KeyName, this.KeyValue, this.ControlKey)
                                : this.ModuleControl.ModuleContext.EditUrl(this.Title);

                if (this.CssClass == string.Empty)
                {
                    this.CssClass = "dnnPrimaryAction";
					Logger.LogDebug("OnPreRender(e={E}): this.CssClass → {Arg1}", e, this.CssClass);
                }
            }
            else
            {
                this.Visible = false;
				Logger.LogDebug("OnPreRender(e={E}): this.Visible → {Arg1}", e, this.Visible);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private bool IsVisible(SecurityAccessLevel security)
        {
			Logger.LogDebug(">>> Entering IsVisible(security={Security})", security);
            bool isVisible = false;
            if (ModulePermissionController.HasModuleAccess(security, Null.NullString, this.ModuleControl.ModuleContext.Configuration))
			Logger.LogDebug("IsVisible(security={Security}): isVisible → {IsVisible}", security, isVisible);
            {
                if ((this.RequireEditMode != true || Personalization.GetUserMode() == PortalSettings.Mode.Edit) || (security == SecurityAccessLevel.Anonymous || security == SecurityAccessLevel.View))
                {
                    isVisible = true;
                }
            }

			Logger.LogDebug("<<< Exiting IsVisible(security={Security})", security);
            return isVisible;
        }
    }
}
