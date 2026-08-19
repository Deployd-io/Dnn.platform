// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Facebook.Components
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class FacebookClient : OAuthClientBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Initializes a new instance of the <see cref="FacebookClient"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="mode">The auth mode.</param>
        public FacebookClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Facebook")
        {
			_logger.LogDebug(">>> Entering FacebookClient(portalId={PortalId},mode={Mode})", portalId, mode);
            this.TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token");
            this.TokenMethod = HttpMethod.GET;
			_logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.TokenMethod → {Arg2}", portalId, mode, this.TokenMethod);
            this.AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize");
            this.MeGraphEndpoint = new Uri("https://graph.facebook.com/me?fields=id,name,email,first_name,last_name,link,birthday,gender,locale,timezone,updated_time");

            this.Scope = "email";
			_logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.Scope → {Arg2}", portalId, mode, this.Scope);

            this.AuthTokenName = "FacebookUserToken";
			_logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.AuthTokenName → {Arg2}", portalId, mode, this.AuthTokenName);

            this.OAuthVersion = "2.0";
			_logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.OAuthVersion → {Arg2}", portalId, mode, this.OAuthVersion);

            this.LoadTokenCookie(string.Empty);
			_logger.LogDebug("<<< Exiting FacebookClient(portalId={PortalId},mode={Mode})", portalId, mode);
        }

        /// <inheritdoc />
        protected override TimeSpan GetExpiry(string responseText)
        {
			_logger.LogDebug(">>> Entering GetExpiry(responseText={ResponseText})", responseText);
            TimeSpan expiry = TimeSpan.MinValue;
            if (!string.IsNullOrEmpty(responseText))
			_logger.LogDebug("GetExpiry(responseText={ResponseText}): expiry → {Expiry}", responseText, expiry);
            {
                var dictionary = Json.Deserialize<IDictionary<string, object>>(responseText);
				_logger.LogDebug("GetExpiry(responseText={ResponseText}): dictionary → {Dictionary}", responseText, dictionary);
                expiry = new TimeSpan(0, 0, Convert.ToInt32(dictionary["expires_in"]));
            }

			_logger.LogDebug("<<< Exiting GetExpiry(responseText={ResponseText})", responseText);
            return expiry;
        }

        /// <inheritdoc />
        protected override string GetToken(string responseText)
        {
			_logger.LogDebug(">>> Entering GetToken(responseText={ResponseText})", responseText);
            string authToken = string.Empty;
            if (!string.IsNullOrEmpty(responseText))
			_logger.LogDebug("GetToken(responseText={ResponseText}): authToken → {AuthToken}", responseText, authToken);
            {
                var dictionary = Json.Deserialize<IDictionary<string, object>>(responseText);
				_logger.LogDebug("GetToken(responseText={ResponseText}): dictionary → {Dictionary}", responseText, dictionary);
                authToken = Convert.ToString(dictionary["access_token"]);
            }

			_logger.LogDebug("<<< Exiting GetToken(responseText={ResponseText})", responseText);
            return authToken;
        }
    }
}
