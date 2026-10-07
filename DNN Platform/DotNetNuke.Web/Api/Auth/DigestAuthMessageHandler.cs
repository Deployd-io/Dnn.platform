// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Web.Api.Internal.Auth;

    /// <summary>Digest authentication message handler.</summary>
    [DnnDeprecated(10, 2, 2, "Use JWT or API token authentication")]
    public partial class DigestAuthMessageHandler : AuthMessageHandlerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DigestAuthMessageHandler));
        /// <summary>Initializes a new instance of the <see cref="DigestAuthMessageHandler"/> class.</summary>
        /// <param name="includeByDefault">Should this handler be included by default on all routes.</param>
        /// <param name="forceSsl">Should this handler enforce SSL usage.</param>
        public DigestAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
		Logger.LogError(">>> Entering DigestAuthMessageHandler(includeByDefault={IncludeByDefault},forceSsl={ForceSsl})", includeByDefault, forceSsl);
        Logger.LogWarning("<<< Exiting DigestAuthMessageHandler(includeByDefault={IncludeByDefault},forceSsl={ForceSsl})", includeByDefault, forceSsl);
        }

        /// <inheritdoc />
        public override string AuthScheme => DigestAuthentication.AuthenticationScheme;

        /// <inheritdoc />
        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
			Logger.LogError(">>> Entering OnInboundRequest(request={Request},cancellationToken=<redacted>)", request);
            if (this.NeedsAuthentication(request))
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
				Logger.LogError("OnInboundRequest(request={Request},cancellationToken=<redacted>): portalSettings → {PortalSettings}", request, portalSettings);
                if (portalSettings != null)
                {
                    var isStale = TryToAuthenticate(request, portalSettings.PortalId);
					Logger.LogError("OnInboundRequest(request={Request},cancellationToken=<redacted>): isStale → {IsStale}", request, isStale);

                    if (isStale)
                    {
                        var staleResponse = request.CreateResponse(HttpStatusCode.Unauthorized);
						Logger.LogError("OnInboundRequest(request={Request},cancellationToken=<redacted>): staleResponse → {StaleResponse}", request, staleResponse);
                        this.AddStaleWwwAuthenticateHeader(staleResponse);

                        return staleResponse;
                    }
                }
            }

			Logger.LogWarning("<<< Exiting OnInboundRequest(request={Request},cancellationToken=<redacted>)", request);
            return base.OnInboundRequest(request, cancellationToken);
        }

        /// <inheritdoc />
        public override HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
			Logger.LogError(">>> Entering OnOutboundResponse(response={Response},cancellationToken=<redacted>)", response);
            if (response.StatusCode == HttpStatusCode.Unauthorized && SupportsDigestAuth(response.RequestMessage))
            {
                this.AddWwwAuthenticateHeader(response);
            }

			Logger.LogWarning("<<< Exiting OnOutboundResponse(response={Response},cancellationToken=<redacted>)", response);
            return base.OnOutboundResponse(response, cancellationToken);
        }

        private static string CreateNewNonce()
        {
			Logger.LogError(">>> Entering CreateNewNonce()");
            DateTime nonceTime = DateTime.Now + TimeSpan.FromMinutes(1);
            string expireStr = nonceTime.ToString("G", CultureInfo.InvariantCulture);
			Logger.LogError("CreateNewNonce(): nonceTime → {NonceTime}", nonceTime);

            byte[] expireBytes = Encoding.Default.GetBytes(expireStr);
            string nonce = Convert.ToBase64String(expireBytes);

            nonce = nonce.TrimEnd('=');
			Logger.LogWarning("<<< Exiting CreateNewNonce()");
            return nonce;
        }

        private static bool TryToAuthenticate(HttpRequestMessage request, int portalId)
        {
			Logger.LogError(">>> Entering TryToAuthenticate(request={Request},portalId={PortalId})", request, portalId);
            if (request?.Headers.Authorization == null)
            {
                return false;
            }

            string authHeader = request?.Headers.Authorization.ToString();

            var digestAuthentication = new DigestAuthentication(new DigestAuthenticationRequest(authHeader, request.Method.Method), portalId, request.GetIPAddress());
			Logger.LogError("TryToAuthenticate(request={Request},portalId={PortalId}): digestAuthentication → {DigestAuthentication}", request, portalId, digestAuthentication);

            if (digestAuthentication.IsValid)
            {
                SetCurrentPrincipal(digestAuthentication.User, request);
            }
            else if (digestAuthentication.IsNonceStale)
            {
                return true;
            }

			Logger.LogWarning("<<< Exiting TryToAuthenticate(request={Request},portalId={PortalId})", request, portalId);
            return false;
        }

        private static bool SupportsDigestAuth(HttpRequestMessage request)
        {
			Logger.LogError(">>> Entering SupportsDigestAuth(request={Request})", request);
            Logger.LogWarning("<<< Exiting SupportsDigestAuth(request={Request})", request);
            return !IsXmlHttpRequest(request) && MembershipProviderConfig.PasswordRetrievalEnabled;
        }

        private void AddStaleWwwAuthenticateHeader(HttpResponseMessage response)
        {
			Logger.LogError(">>> Entering AddStaleWwwAuthenticateHeader(response={Response})", response);
            this.AddWwwAuthenticateHeader(response, true);
			Logger.LogWarning("<<< Exiting AddStaleWwwAuthenticateHeader(response={Response})", response);
        }

        private void AddWwwAuthenticateHeader(HttpResponseMessage response, bool isStale = false)
        {
			Logger.LogError(">>> Entering AddWwwAuthenticateHeader(response={Response},isStale={IsStale})", response, isStale);
            var value = $"realm=\"DNNAPI\", nonce=\"{CreateNewNonce()}\",  opaque=\"0000000000000000\", stale={isStale}, algorithm=MD5, qop=\"auth\"";
			Logger.LogWarning("<<< Exiting AddWwwAuthenticateHeader(response={Response},isStale={IsStale})", response, isStale);
			Logger.LogError("AddWwwAuthenticateHeader(response={Response},isStale={IsStale}): value → {Value}", response, isStale, value);
            response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(this.AuthScheme, value));
        }
    }
}
