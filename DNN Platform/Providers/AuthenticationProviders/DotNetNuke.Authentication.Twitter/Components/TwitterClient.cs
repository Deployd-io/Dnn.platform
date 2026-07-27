// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Twitter.Components
{
    using System;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;

    /// <inheritdoc />
    public class TwitterClient : OAuthClientBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TwitterClient"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="mode">The auth mode.</param>
        public TwitterClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Twitter")
        {
			Logger.LogDebug(">>> Entering TwitterClient(portalId={PortalId},mode={Mode})", portalId, mode);
            this.AuthorizationEndpoint = new Uri("https://api.twitter.com/oauth/authorize");
            this.RequestTokenEndpoint = new Uri("https://api.twitter.com/oauth/request_token");
            this.RequestTokenMethod = HttpMethod.POST;
			Logger.LogDebug("TwitterClient(portalId={PortalId},mode={Mode}): this.RequestTokenMethod → {Arg2}", portalId, mode, this.RequestTokenMethod);
            this.TokenEndpoint = new Uri("https://api.twitter.com/oauth/access_token");
            this.MeGraphEndpoint = new Uri("https://api.twitter.com/1.1/account/verify_credentials.json");

            this.AuthTokenName = "TwitterUserToken";
			Logger.LogDebug("TwitterClient(portalId={PortalId},mode={Mode}): this.AuthTokenName → {Arg2}", portalId, mode, this.AuthTokenName);

            this.OAuthVersion = "1.0";
			Logger.LogDebug("TwitterClient(portalId={PortalId},mode={Mode}): this.OAuthVersion → {Arg2}", portalId, mode, this.OAuthVersion);

            this.LoadTokenCookie(string.Empty);
			Logger.LogDebug("<<< Exiting TwitterClient(portalId={PortalId},mode={Mode})", portalId, mode);
        }
    }
}
