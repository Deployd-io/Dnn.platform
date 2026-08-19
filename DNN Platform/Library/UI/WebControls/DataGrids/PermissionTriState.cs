// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls.Internal
{
    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    /// <summary>
    /// A TriState permission control built specifically for use in the PermissionGrid control
    /// This control is not general in any way shape of form and should NOT be used outside
    /// of the PermissionGrid.
    /// </summary>
    public class PermissionTriState : HiddenField
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PermissionTriState));
        private readonly string grantImagePath;
        private readonly string denyImagePath;
        private readonly string nullImagePath;
        private readonly string lockImagePath;
        private readonly string grantAltText;
        private readonly string denyAltText;
        private readonly string nullAltText;

        /// <summary>Initializes a new instance of the <see cref="PermissionTriState"/> class.</summary>
        public PermissionTriState()
        {
			Logger.LogDebug(">>> Entering PermissionTriState()");
            // kind of ugly to lookup this data each time, but doesn't seem worth the effort to
            // maintain statics for the paths but require a control instance to initialize them
            // and lazy load the text bits when the page instance (or localization) changes
            LookupScriptValues(this, out this.grantImagePath, out this.denyImagePath, out this.nullImagePath, out this.lockImagePath, out this.grantAltText, out this.denyAltText, out this.nullAltText);
			Logger.LogDebug("<<< Exiting PermissionTriState()");
        }

        public bool IsFullControl { get; set; }

        public bool IsView { get; set; }

        // Locked is currently not used on a post-back and therefore the
        // value on postback is undefined at this time
        public bool Locked { get; set; }

        public string PermissionKey { get; set; }

        public bool SupportsDenyMode { get; set; }

        public static void RegisterScripts(Page page, Control ctl)
        {
			Logger.LogDebug(">>> Entering RegisterScripts(page={Page},ctl={Ctl})", page, ctl);
            const string scriptKey = "initTriState";
            if (!ClientAPI.IsClientScriptBlockRegistered(page, scriptKey))
            {
                AJAX.RegisterScriptManager();
                JavaScript.RequestRegistration(CommonJs.jQuery);
                ClientAPI.RegisterClientScriptBlock(page, "dnn.permissiontristate.js");

                ClientAPI.RegisterStartUpScript(page, scriptKey, "<script type='text/javascript'>" + GetInitScript(ctl) + "</script>");
            }
			Logger.LogDebug("<<< Exiting RegisterScripts(page={Page},ctl={Ctl})", page, ctl);
        }

        public static string GetInitScript(Control ctl)
        {
			Logger.LogDebug(">>> Entering GetInitScript(ctl={Ctl})", ctl);
            LookupScriptValues(ctl, out var grantImagePath, out var denyImagePath, out var nullImagePath, out _, out var grantAltText, out var denyAltText, out var nullAltText);

            return $$"""
                     jQuery(document).ready(
                     function() {
                         var images = { 'True': '{{HttpUtility.JavaScriptStringEncode(grantImagePath)}}', 'False': '{{HttpUtility.JavaScriptStringEncode(denyImagePath)}}', 'Null': '{{HttpUtility.JavaScriptStringEncode(nullImagePath)}}' };
                         var toolTips = { 'True': '{{HttpUtility.JavaScriptStringEncode(grantAltText)}}', 'False': '{{HttpUtility.JavaScriptStringEncode(denyAltText)}}', 'Null': '{{HttpUtility.JavaScriptStringEncode(nullAltText)}}' };
                         var tsm = dnn.controls.triStateManager(images, toolTips);
                         jQuery('.tristate').each( function(i, elem) {
                           tsm.initControl( elem );
                         });
                      });
                     """;
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            RegisterScripts(this.Page, this);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            string imagePath;
            string altText;
            switch (this.Value)
            {
                case "True":
                    imagePath = this.grantImagePath;
                    altText = this.grantAltText;
					Logger.LogDebug("Render(writer={Writer}): imagePath → {ImagePath}", writer, imagePath);
					Logger.LogDebug("Render(writer={Writer}): altText → {AltText}", writer, altText);
                    break;

                case "False":
                    imagePath = this.denyImagePath;
                    altText = this.denyAltText;
                    break;

                default:
                    imagePath = this.nullImagePath;
                    altText = this.nullAltText;
                    break;
            }

            string cssClass = "tristate";
            if (this.Locked)
            {
                imagePath = this.lockImagePath;
                cssClass += " lockedPerm";

                // altText is set based on Value
            }

            if (!this.SupportsDenyMode)
            {
                cssClass += " noDenyPerm";
            }

            if (this.IsFullControl)
            {
                cssClass += " fullControl";
            }

            if (this.IsView && !this.Locked)
            {
                cssClass += " view";
            }

            if (!string.IsNullOrEmpty(this.PermissionKey) && !this.IsView && !this.IsFullControl)
            {
                cssClass += " " + this.PermissionKey.ToLowerInvariant();
            }

            writer.Write("<img src='{0}' alt='{1}' />", imagePath, altText);

            writer.AddAttribute("class", cssClass);
            base.Render(writer);
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }

        private static void LookupScriptValues(Control ctl, out string grantImagePath, out string denyImagePath, out string nullImagePath, out string lockImagePath, out string grantAltText, out string denyAltText, out string nullAltText)
        {
			Logger.LogDebug(">>> Entering LookupScriptValues(ctl={Ctl})", ctl);
            grantImagePath = IconController.IconURL("Grant");
            denyImagePath = IconController.IconURL("Deny");
            nullImagePath = IconController.IconURL("Unchecked");
			Logger.LogDebug("LookupScriptValues(ctl={Ctl}): nullImagePath → {NullImagePath}", ctl, nullImagePath);
			Logger.LogDebug("LookupScriptValues(ctl={Ctl}): grantImagePath → {GrantImagePath}", ctl, grantImagePath);
			Logger.LogDebug("LookupScriptValues(ctl={Ctl}): denyImagePath → {DenyImagePath}", ctl, denyImagePath);
            lockImagePath = IconController.IconURL("Lock");
			Logger.LogDebug("LookupScriptValues(ctl={Ctl}): lockImagePath → {LockImagePath}", ctl, lockImagePath);

            grantAltText = Localization.GetString("PermissionTypeGrant");
			Logger.LogDebug("LookupScriptValues(ctl={Ctl}): grantAltText → {GrantAltText}", ctl, grantAltText);
            denyAltText = Localization.GetString("PermissionTypeDeny");
			Logger.LogDebug("LookupScriptValues(ctl={Ctl}): denyAltText → {DenyAltText}", ctl, denyAltText);
            nullAltText = Localization.GetString("PermissionTypeNull");
			Logger.LogDebug("LookupScriptValues(ctl={Ctl}): nullAltText → {NullAltText}", ctl, nullAltText);
			Logger.LogDebug("<<< Exiting LookupScriptValues(ctl={Ctl})", ctl);
        }
    }
}
