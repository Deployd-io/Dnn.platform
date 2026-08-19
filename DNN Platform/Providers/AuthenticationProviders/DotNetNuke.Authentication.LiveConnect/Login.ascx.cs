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
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public partial class Login : OAuthLoginBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
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
			_logger.LogDebug(">>> Entering GetCurrentUser()");
			_logger.LogDebug("<<< Exiting GetCurrentUser()");
            return this.OAuthClient.GetCurrentUser<LiveUserData>();
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			_logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.loginButton.Click += this.LoginButton_Click;
            this.registerButton.Click += this.LoginButton_Click;

            this.OAuthClient = new LiveClient(this.PortalId, this.Mode);

            this.loginItem.Visible = this.Mode == AuthMode.Login;
            this.registerItem.Visible = this.Mode == AuthMode.Register;
			_logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
			_logger.LogDebug(">>> Entering LoginButton_Click(sender={Sender},e={E})", sender, e);
            AuthorisationResult result = this.OAuthClient.Authorize();
            if (result == AuthorisationResult.Denied)
			_logger.LogDebug("LoginButton_Click(sender={Sender},e={E}): result → {Result}", sender, e, result);
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
			_logger.LogDebug("<<< Exiting LoginButton_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
