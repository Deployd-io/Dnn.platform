// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Facebook
{
    using System;
    using System.Collections.Specialized;

    using DotNetNuke.Authentication.Facebook.Components;
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
            get { return "Facebook"; }
        }

        /// <inheritdoc />
        protected override UserData GetCurrentUser()
        {
			_logger.LogError(">>> Entering GetCurrentUser()");
            _logger.LogWarning("<<< Exiting GetCurrentUser()");
            return this.OAuthClient.GetCurrentUser<FacebookUserData>();
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			_logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.loginButton.Click += this.LoginButton_Click;
            this.registerButton.Click += this.LoginButton_Click;

            this.OAuthClient = new FacebookClient(this.PortalId, this.Mode);

            this.loginItem.Visible = this.Mode == AuthMode.Login;
            this.registerItem.Visible = this.Mode == AuthMode.Register;
			_logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void AddCustomProperties(NameValueCollection properties)
        {
			_logger.LogError(">>> Entering AddCustomProperties(properties={Properties})", properties);
            base.AddCustomProperties(properties);

            var facebookUserData = this.OAuthClient.GetCurrentUser<FacebookUserData>();
			_logger.LogError("AddCustomProperties(properties={Properties}): facebookUserData → {FacebookUserData}", properties, facebookUserData);
            if (facebookUserData.Link != null)
            {
                properties.Add("Facebook", facebookUserData.Link.ToString());
            }
			_logger.LogWarning("<<< Exiting AddCustomProperties(properties={Properties})", properties);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
			_logger.LogError(">>> Entering LoginButton_Click(sender={Sender},e={E})", sender, e);
            AuthorisationResult result = this.OAuthClient.Authorize();
			_logger.LogError("LoginButton_Click(sender={Sender},e={E}): result → {Result}", sender, e, result);
            if (result == AuthorisationResult.Denied)
            {
                UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile), ModuleMessage.ModuleMessageType.YellowWarning);
            }
			_logger.LogWarning("<<< Exiting LoginButton_Click(sender={Sender},e={E})", sender, e);
        }
    }
}
