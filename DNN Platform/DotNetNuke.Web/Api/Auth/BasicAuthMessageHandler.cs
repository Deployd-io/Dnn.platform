// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;

    /// <summary>Basic authentication message handler.</summary>
    public class BasicAuthMessageHandler : AuthMessageHandlerBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly char[] CredentialSeparator = [':',];
        private readonly Encoding encoding = Encoding.GetEncoding("iso-8859-1");

        /// <summary>Initializes a new instance of the <see cref="BasicAuthMessageHandler"/> class.</summary>
        /// <param name="includeByDefault">Should this handler be included by default on all routes.</param>
        /// <param name="forceSsl">Should this handler enforce SSL usage.</param>
        public BasicAuthMessageHandler(bool includeByDefault, bool forceSsl)
            : base(includeByDefault, forceSsl)
        {
		Logger.LogDebug(">>> Entering BasicAuthMessageHandler(includeByDefault={IncludeByDefault},forceSsl={ForceSsl})", includeByDefault, forceSsl);
        }
		Logger.LogDebug("<<< Exiting BasicAuthMessageHandler(includeByDefault={IncludeByDefault},forceSsl={ForceSsl})", includeByDefault, forceSsl);

        /// <summary>Gets the name of the authentication scheme.</summary>
        public override string AuthScheme => "Basic";

        /// <inheritdoc />
        public override HttpResponseMessage OnInboundRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering OnInboundRequest(request={Request},cancellationToken={CancellationToken})", request, cancellationToken);
            if (this.NeedsAuthentication(request))
            {
                var portalSettings = PortalController.Instance.GetCurrentSettings();
                if (portalSettings != null)
				Logger.LogDebug("OnInboundRequest(request={Request},cancellationToken={CancellationToken}): portalSettings → {PortalSettings}", request, cancellationToken, portalSettings);
                {
                    this.TryToAuthenticate(request, portalSettings.PortalId);
                }
            }

			Logger.LogDebug("<<< Exiting OnInboundRequest(request={Request},cancellationToken={CancellationToken})", request, cancellationToken);
            return base.OnInboundRequest(request, cancellationToken);
        }

        /// <inheritdoc />
        public override HttpResponseMessage OnOutboundResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
			Logger.LogDebug(">>> Entering OnOutboundResponse(response={Response},cancellationToken={CancellationToken})", response, cancellationToken);
            if (response.StatusCode == HttpStatusCode.Unauthorized && SupportsBasicAuth(response.RequestMessage))
            {
                response.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue(this.AuthScheme, "realm=\"DNNAPI\""));
				Logger.LogDebug(">>> Entering SupportsBasicAuth(request={Request})", request);
				Logger.LogDebug("<<< Exiting SupportsBasicAuth(request={Request})", request);
            }

			Logger.LogDebug("<<< Exiting OnOutboundResponse(response={Response},cancellationToken={CancellationToken})", response, cancellationToken);
            return base.OnOutboundResponse(response, cancellationToken);
        }

        private static bool SupportsBasicAuth(HttpRequestMessage request)
        {
			Logger.LogDebug(">>> Entering SupportsBasicAuth(request={Request})", request);
			Logger.LogDebug("<<< Exiting SupportsBasicAuth(request={Request})", request);
            return !IsXmlHttpRequest(request);
        }

        private void TryToAuthenticate(HttpRequestMessage request, int portalId)
        {
			Logger.LogDebug(">>> Entering TryToAuthenticate(request={Request},portalId={PortalId})", request, portalId);
            UserCredentials credentials = this.GetCredentials(request);

			Logger.LogDebug("TryToAuthenticate(request={Request},portalId={PortalId}): credentials → {Credentials}", request, portalId, credentials);
            if (credentials == null)
            {
                return;
            }

            var status = UserLoginStatus.LOGIN_FAILURE;
            string ipAddress = request.GetIPAddress();
			Logger.LogDebug("TryToAuthenticate(request={Request},portalId={PortalId}): status → {Status}", request, portalId, status);

            UserInfo user = UserController.ValidateUser(
                portalId,
                credentials.UserName,
                credentials.Password,
                "DNN",
                string.Empty,
                "a portal",
                ipAddress ?? string.Empty,
                ref status);

            if (user != null)
            {
                SetCurrentPrincipal(new GenericPrincipal(new GenericIdentity(credentials.UserName, this.AuthScheme), null), request);
            }
			Logger.LogDebug("<<< Exiting TryToAuthenticate(request={Request},portalId={PortalId})", request, portalId);
        }

        private UserCredentials GetCredentials(HttpRequestMessage request)
        {
			Logger.LogDebug(">>> Entering GetCredentials(request={Request})", request);
            if (request?.Headers.Authorization == null)
            {
                return null;
            }

            if (!request.Headers.Authorization.Scheme.Equals(this.AuthScheme, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string authorization = request?.Headers.Authorization.Parameter;
            if (string.IsNullOrEmpty(authorization))
            {
                return null;
            }

            string decoded = this.encoding.GetString(Convert.FromBase64String(authorization));

            string[] parts = decoded.Split(CredentialSeparator, 2);
            if (parts.Length < 2)
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting GetCredentials(request={Request})", request);
            return new UserCredentials(parts[0], parts[1]);
        }

        /// <summary>Represents the user credentials.</summary>
        internal class UserCredentials
        {
            /// <summary>Initializes a new instance of the <see cref="UserCredentials"/> class.</summary>
            /// <param name="userName">The user username.</param>
            /// <param name="password">The user password.</param>
            public UserCredentials(string userName, string password)
            {
				Logger.LogDebug(">>> Entering UserCredentials(userName={UserName},password={Password})", userName, password);
                this.UserName = userName;
                this.Password = password;
				Logger.LogDebug("<<< Exiting UserCredentials(userName={UserName},password={Password})", userName, password);
				Logger.LogDebug("UserCredentials(userName={UserName},password={Password}): this.UserName → {Arg2}", userName, password, this.UserName);
				Logger.LogDebug("UserCredentials(userName={UserName},password={Password}): this.Password → {Arg2}", userName, password, this.Password);
            }

            /// <summary>Gets or sets the password for the user.</summary>
            public string Password { get; set; }

            /// <summary>Gets or sets the username for the user.</summary>
            public string UserName { get; set; }
        }
    }
}
