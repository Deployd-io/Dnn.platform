// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Authentication.LiveConnect.Components
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Script.Serialization;

    using DotNetNuke.Services.Authentication;
    using DotNetNuke.Services.Authentication.OAuth;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class LiveClient : OAuthClientBase
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Initializes a new instance of the <see cref="LiveClient"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="mode">The auth mode.</param>
        public LiveClient(int portalId, AuthMode mode)
            : base(portalId, mode, "Live")
        {
			_logger.LogError(">>> Entering LiveClient(portalId={PortalId},mode={Mode})", portalId, mode);
            // DNN-6464 Correct TokenEndpoint and AuthorizationEndpoint Urls
            // Add TokenMethod of Post to conform to other OAuth extensions
            this.TokenMethod = HttpMethod.POST;
			_logger.LogDebug("LiveClient(portalId={PortalId},mode={Mode}): this.TokenMethod → {Arg2}", portalId, mode, this.TokenMethod);
            this.TokenEndpoint = new Uri("https://login.live.com/oauth20_token.srf");
            this.AuthorizationEndpoint = new Uri("https://login.live.com/oauth20_authorize.srf");
            this.MeGraphEndpoint = new Uri("https://apis.live.net/v5.0/me");

            this.Scope = HttpContext.Current.Server.UrlEncode("wl.signin wl.basic wl.emails");

            this.AuthTokenName = "LiveUserToken";
			_logger.LogDebug("LiveClient(portalId={PortalId},mode={Mode}): this.AuthTokenName → {Arg2}", portalId, mode, this.AuthTokenName);

            this.OAuthVersion = "2.0";
			_logger.LogDebug("LiveClient(portalId={PortalId},mode={Mode}): this.OAuthVersion → {Arg2}", portalId, mode, this.OAuthVersion);

            this.LoadTokenCookie(string.Empty);
			_logger.LogWarning("<<< Exiting LiveClient(portalId={PortalId},mode={Mode})", portalId, mode);
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
