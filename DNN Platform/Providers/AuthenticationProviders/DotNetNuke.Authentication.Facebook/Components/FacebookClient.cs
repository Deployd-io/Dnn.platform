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

    /// <inheritdoc />
    public class FacebookClient : OAuthClientBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="FacebookClient"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="mode">The auth mode.</param>
        public FacebookClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Facebook")
        {
			Logger.LogDebug(">>> Entering FacebookClient(portalId={PortalId},mode={Mode})", portalId, mode);
            this.TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token");
            this.TokenMethod = HttpMethod.GET;
			Logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.TokenMethod → {Arg2}", portalId, mode, this.TokenMethod);
            this.AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize");
            this.MeGraphEndpoint = new Uri("https://graph.facebook.com/me?fields=id,name,email,first_name,last_name,link,birthday,gender,locale,timezone,updated_time");

            this.Scope = "email";
			Logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.Scope → {Arg2}", portalId, mode, this.Scope);

            this.AuthTokenName = "FacebookUserToken";
			Logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.AuthTokenName → {Arg2}", portalId, mode, this.AuthTokenName);

            this.OAuthVersion = "2.0";
			Logger.LogDebug("FacebookClient(portalId={PortalId},mode={Mode}): this.OAuthVersion → {Arg2}", portalId, mode, this.OAuthVersion);

            this.LoadTokenCookie(string.Empty);
			Logger.LogDebug("<<< Exiting FacebookClient(portalId={PortalId},mode={Mode})", portalId, mode);
        }

        /// <inheritdoc />
        protected override TimeSpan GetExpiry(string responseText)
        {
			Logger.LogDebug(">>> Entering GetExpiry(responseText={ResponseText})", responseText);
            TimeSpan expiry = TimeSpan.MinValue;
            if (!string.IsNullOrEmpty(responseText))
			Logger.LogDebug("GetExpiry(responseText={ResponseText}): expiry → {Expiry}", responseText, expiry);
            {
                var dictionary = Json.Deserialize<IDictionary<string, object>>(responseText);
                expiry = new TimeSpan(0, 0, Convert.ToInt32(dictionary["expires_in"]));
				Logger.LogDebug("GetExpiry(responseText={ResponseText}): dictionary → {Dictionary}", responseText, dictionary);
            }

			Logger.LogDebug("<<< Exiting GetExpiry(responseText={ResponseText})", responseText);
            return expiry;
        }

        /// <inheritdoc />
        protected override string GetToken(string responseText)
        {
			Logger.LogDebug(">>> Entering GetToken(responseText={ResponseText})", responseText);
            string authToken = string.Empty;
            if (!string.IsNullOrEmpty(responseText))
			Logger.LogDebug("GetToken(responseText={ResponseText}): authToken → {AuthToken}", responseText, authToken);
            {
                var dictionary = Json.Deserialize<IDictionary<string, object>>(responseText);
                authToken = Convert.ToString(dictionary["access_token"]);
				Logger.LogDebug("GetToken(responseText={ResponseText}): dictionary → {Dictionary}", responseText, dictionary);
            }

			Logger.LogDebug("<<< Exiting GetToken(responseText={ResponseText})", responseText);
            return authToken;
        }
    }
}
