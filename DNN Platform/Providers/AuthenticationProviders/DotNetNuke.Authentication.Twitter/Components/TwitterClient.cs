// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Twitter.Components
{
    using System;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class TwitterClient : OAuthClientBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Initializes a new instance of the <see cref="TwitterClient"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="mode">The auth mode.</param>
        public TwitterClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Twitter")
        {
			_logger.LogDebug(">>> Entering TwitterClient(portalId={PortalId},mode={Mode})", portalId, mode);
            this.AuthorizationEndpoint = new Uri("https://api.twitter.com/oauth/authorize");
            this.RequestTokenEndpoint = new Uri("https://api.twitter.com/oauth/request_token");
            this.RequestTokenMethod = HttpMethod.POST;
			_logger.LogDebug("TwitterClient(portalId={PortalId},mode={Mode}): this.RequestTokenMethod → {Arg2}", portalId, mode, this.RequestTokenMethod);
            this.TokenEndpoint = new Uri("https://api.twitter.com/oauth/access_token");
            this.MeGraphEndpoint = new Uri("https://api.twitter.com/1.1/account/verify_credentials.json");

            this.AuthTokenName = "TwitterUserToken";
			_logger.LogDebug("TwitterClient(portalId={PortalId},mode={Mode}): this.AuthTokenName → {Arg2}", portalId, mode, this.AuthTokenName);

            this.OAuthVersion = "1.0";
			_logger.LogDebug("TwitterClient(portalId={PortalId},mode={Mode}): this.OAuthVersion → {Arg2}", portalId, mode, this.OAuthVersion);

            this.LoadTokenCookie(string.Empty);
			_logger.LogDebug("<<< Exiting TwitterClient(portalId={PortalId},mode={Mode})", portalId, mode);
        }
    }
}
