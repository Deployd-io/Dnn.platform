// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.Google.Components
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Script.Serialization;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class GoogleClient : OAuthClientBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Initializes a new instance of the <see cref="GoogleClient"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="mode">The auth mode.</param>
        public GoogleClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Google")
        {
			_logger.LogError(">>> Entering GoogleClient(portalId={PortalId},mode={Mode})", portalId, mode);
            this.TokenEndpoint = new Uri("https://accounts.google.com/o/oauth2/token");
            this.TokenMethod = HttpMethod.POST;
			_logger.LogDebug("GoogleClient(portalId={PortalId},mode={Mode}): this.TokenMethod → {Arg2}", portalId, mode, this.TokenMethod);
            this.AuthorizationEndpoint = new Uri("https://accounts.google.com/o/oauth2/auth");
            this.MeGraphEndpoint = new Uri("https://www.googleapis.com/oauth2/v1/userinfo");

            this.Scope = HttpUtility.UrlEncode("https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email");

            this.AuthTokenName = "GoogleUserToken";
			_logger.LogDebug("GoogleClient(portalId={PortalId},mode={Mode}): this.AuthTokenName → {Arg2}", portalId, mode, this.AuthTokenName);

            this.OAuthVersion = "2.0";
			_logger.LogDebug("GoogleClient(portalId={PortalId},mode={Mode}): this.OAuthVersion → {Arg2}", portalId, mode, this.OAuthVersion);

            this.LoadTokenCookie(string.Empty);
			_logger.LogWarning("<<< Exiting GoogleClient(portalId={PortalId},mode={Mode})", portalId, mode);
        }

        /// <inheritdoc />
        protected override TimeSpan GetExpiry(string responseText)
        {
			_logger.LogError(">>> Entering GetExpiry(responseText={ResponseText})", responseText);
            var jsonSerializer = new JavaScriptSerializer();
            var tokenDictionary = jsonSerializer.DeserializeObject(responseText) as Dictionary<string, object>;
			_logger.LogError("GetExpiry(responseText={ResponseText}): tokenDictionary → {TokenDictionary}", responseText, tokenDictionary);
			_logger.LogError("GetExpiry(responseText={ResponseText}): jsonSerializer → {JsonSerializer}", responseText, jsonSerializer);

			_logger.LogWarning("<<< Exiting GetExpiry(responseText={ResponseText})", responseText);
            return new TimeSpan(0, 0, Convert.ToInt32(tokenDictionary["expires_in"]));
        }

        /// <inheritdoc />
        protected override string GetToken(string responseText)
        {
			_logger.LogError(">>> Entering GetToken(responseText={ResponseText})", responseText);
            var jsonSerializer = new JavaScriptSerializer();
            var tokenDictionary = jsonSerializer.DeserializeObject(responseText) as Dictionary<string, object>;
			_logger.LogError("GetToken(responseText={ResponseText}): tokenDictionary → {TokenDictionary}", responseText, tokenDictionary);
			_logger.LogError("GetToken(responseText={ResponseText}): jsonSerializer → {JsonSerializer}", responseText, jsonSerializer);
			_logger.LogWarning("<<< Exiting GetToken(responseText={ResponseText})", responseText);
            return Convert.ToString(tokenDictionary["access_token"]);
        }
    }
}
