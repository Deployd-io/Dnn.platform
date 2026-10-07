// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Twitter
{
    using System;

    using DotNetNuke.Authentication.Twitter.Components;
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
            get { return "Twitter"; }
        }

        /// <inheritdoc />
        protected override UserData GetCurrentUser()
        {
			_logger.LogError(">>> Entering GetCurrentUser()");
            _logger.LogWarning("<<< Exiting GetCurrentUser()");
            return this.OAuthClient.GetCurrentUser<TwitterUserData>();
        }

        /// <inheritdoc />
        protected override void AddCustomProperties(System.Collections.Specialized.NameValueCollection properties)
        {
			_logger.LogError(">>> Entering AddCustomProperties(properties={Properties})", properties);
            base.AddCustomProperties(properties);

            properties.Add("Twitter", string.Format("http://twitter.com/{0}", this.OAuthClient.GetCurrentUser<TwitterUserData>().ScreenName));
			_logger.LogWarning("<<< Exiting AddCustomProperties(properties={Properties})", properties);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			_logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.loginButton.Click += this.LoginButton_Click;
            this.registerButton.Click += this.LoginButton_Click;

            this.OAuthClient = new TwitterClient(this.PortalId, this.Mode);

            this.loginItem.Visible = this.Mode == AuthMode.Login;
            this.registerItem.Visible = this.Mode == AuthMode.Register;
			_logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
			_logger.LogError(">>> Entering LoginButton_Click(sender={Sender},e={E})", sender, e);
            this.OAuthClient.CallbackUri = new Uri(this.OAuthClient.CallbackUri + "?state=Twitter");
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
