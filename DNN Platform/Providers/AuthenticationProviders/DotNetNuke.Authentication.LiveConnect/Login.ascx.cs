// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.LiveConnect
{
    using System;

    using DotNetNuke.Authentication.LiveConnect.Components;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins.Controls;

    /// <inheritdoc />
    public partial class Login : OAuthLoginBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override bool SupportsRegistration
        {
            get { return true; }
        }

        /// <inheritdoc />
        protected override string AuthSystemApplicationName
        {
            get { return "Live"; }
        }

        /// <inheritdoc />
        protected override UserData GetCurrentUser()
        {
			Logger.LogDebug(">>> Entering GetCurrentUser()");
			Logger.LogDebug("<<< Exiting GetCurrentUser()");
            return this.OAuthClient.GetCurrentUser<LiveUserData>();
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.loginButton.Click += this.LoginButton_Click;
            this.registerButton.Click += this.LoginButton_Click;

            this.OAuthClient = new LiveClient(this.PortalId, this.Mode);

            this.loginItem.Visible = this.Mode == AuthMode.Login;
            this.registerItem.Visible = this.Mode == AuthMode.Register;
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering LoginButton_Click(sender={Sender},e={E})", sender, e);
            AuthorisationResult result = this.OAuthClient.Authorize();
            if (result == AuthorisationResult.Denied)
			Logger.LogDebug("LoginButton_Click(sender={Sender},e={E}): result → {Result}", sender, e, result);
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
			Logger.LogDebug("<<< Exiting LoginButton_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
