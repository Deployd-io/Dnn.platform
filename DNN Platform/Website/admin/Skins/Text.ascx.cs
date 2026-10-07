// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.IO;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Tokens;

    /// <summary>A skin/theme object which allows displaying localized text with token replacement.</summary>
    public partial class Text : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Text.ascx));
        public string ShowText { get; set; }

        public string CssClass { get; set; }

        public string ResourceKey { get; set; }

        public bool ReplaceTokens { get; set; }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            string strText = this.ShowText;
			Logger.LogError("OnLoad(e={E}): strText → {StrText}", e, strText);

            // load resources
            if (!string.IsNullOrEmpty(this.ResourceKey))
            {
                // localization
                string strFile = Path.GetFileName(this.Server.MapPath(this.PortalSettings.ActiveTab.SkinSrc));
                strFile = this.PortalSettings.ActiveTab.SkinPath + Localization.LocalResourceDirectory + "/" + strFile;
				Logger.LogError("OnLoad(e={E}): strFile → {StrFile}", e, strFile);
                string strLocalization = Localization.GetString(this.ResourceKey, strFile);
                if (!string.IsNullOrEmpty(strLocalization))
                {
                    strText = strLocalization;
                }
            }

            // If no value is found then use the value set the the Text attribute
            if (string.IsNullOrEmpty(strText))
            {
                strText = this.ShowText;
            }

            // token replace
            if (this.ReplaceTokens)
            {
                var tr = new TokenReplace();
				Logger.LogError("OnLoad(e={E}): tr → {Tr}", e, tr);
                tr.AccessingUser = this.PortalSettings.UserInfo;
				Logger.LogDebug("OnLoad(e={E}): tr.AccessingUser → {Arg1}", e, tr.AccessingUser);
                strText = tr.ReplaceEnvironmentTokens(strText);
            }

            this.lblText.Text = strText;
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.lblText.CssClass = this.CssClass;
            }
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }
    }
}
