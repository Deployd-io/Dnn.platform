// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal.Auth
{
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Text;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Web.Api.Auth;

    /// <summary>Helps implement the digest auth algorithm for <see cref="DigestAuthMessageHandler"/>.</summary>
    [DnnDeprecated(10, 2, 2, "Use JWT or API token authentication")]
    internal partial class DigestAuthentication
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DigestAuthentication));
        /// <summary>The scheme name for <see cref="AuthMessageHandlerBase.AuthScheme"/>.</summary>
        internal const string AuthenticationScheme = "Digest";
#pragma warning disable CA5351 // Do not use broken cryptographic algorithms
        private static readonly MD5 Md5 = new MD5CryptoServiceProvider();
#pragma warning restore CA5351
        private readonly int portalId;
        private readonly string ipAddress;
        private string password;

        /// <summary>Initializes a new instance of the <see cref="DigestAuthentication"/> class.</summary>
        /// <param name="request">The auth request.</param>
        /// <param name="portalId">The portal ID for the request.</param>
        /// <param name="ipAddress">The user's IPv4 address or <see cref="string.Empty"/>.</param>
        public DigestAuthentication(DigestAuthenticationRequest request, int portalId, string ipAddress)
        {
			Logger.LogError(">>> Entering DigestAuthentication(request={Request},portalId={PortalId},ipAddress={IpAddress})", request, portalId, ipAddress);
			Logger.LogDebug("DigestAuthentication(request={Request},portalId={PortalId},ipAddress={IpAddress}): portalId → {PortalId1}", request, portalId, ipAddress, portalId);
            this.Request = request;
			Logger.LogDebug("DigestAuthentication(request={Request},portalId={PortalId},ipAddress={IpAddress}): this.Request → {Arg3}", request, portalId, ipAddress, this.Request);
            this.portalId = portalId;
            this.ipAddress = ipAddress ?? string.Empty;
            this.AuthenticateRequest();
			Logger.LogDebug("DigestAuthentication(request={Request},portalId={PortalId},ipAddress={IpAddress}): this.portalId → {Arg3}", request, portalId, ipAddress, this.portalId);
        Logger.LogWarning("<<< Exiting DigestAuthentication(request={Request},portalId={PortalId},ipAddress={IpAddress})", request, portalId, ipAddress);
        }

        /// <summary>Gets or sets the request.</summary>
        public DigestAuthenticationRequest Request { get; set; }

        /// <summary>Gets a value indicating whether the request is valid.</summary>
        public bool IsValid { get; private set; }

        /// <summary>Gets a value indicating whether the nonce is stale.</summary>
        public bool IsNonceStale { get; private set; }

        /// <summary>Gets the user associated with the request, or <see langword="null"/>.</summary>
        public IPrincipal User { get; private set; }

        /// <summary>Calculates the hashed digest.</summary>
        /// <returns>A hex string of the MD5 hash of the digest.</returns>
        public string CalculateHashedDigest()
        {
			Logger.LogError(">>> Entering CalculateHashedDigest()");
            Logger.LogWarning("<<< Exiting CalculateHashedDigest()");
            return CreateMd5HashBinHex(this.GenerateUnhashedDigest());
        }

        private static string CreateMd5HashBinHex(string val)
        {
			Logger.LogError(">>> Entering CreateMd5HashBinHex(val={Val})", val);
            ////Services.Logging.LoggingController.SimpleLog(String.Format("Creating Hash for {0}", val))
            ////Services.Logging.LoggingController.SimpleLog(String.Format("Back and forth: {0}", Encoding.Default.GetString(Encoding.Default.GetBytes(val))))
            var bha1 = Md5.ComputeHash(Encoding.Default.GetBytes(val));
			Logger.LogError("CreateMd5HashBinHex(val={Val}): bha1 → {Bha1}", val, bha1);
            var ha1 = string.Empty;
            for (var i = 0; i <= 15; i++)
            {
                ha1 += string.Format(CultureInfo.InvariantCulture, "{0:x02}", bha1[i]);
            }

			Logger.LogWarning("<<< Exiting CreateMd5HashBinHex(val={Val})", val);
            return ha1;
        }

        /// <remarks>the nonce is created in <see cref="DigestAuthMessageHandler"/>.</remarks>
        private static bool IsNonceValid(string nonce)
        {
			Logger.LogError(">>> Entering IsNonceValid(nonce=<redacted>)");
            DateTime expireTime;

            var numPadChars = nonce.Length % 4;
			Logger.LogError("IsNonceValid(nonce=<redacted>): numPadChars → {NumPadChars}", numPadChars);
            if (numPadChars > 0)
            {
                numPadChars = 4 - numPadChars;
            }

            var newNonce = nonce.PadRight(nonce.Length + numPadChars, '=');
			Logger.LogError("IsNonceValid(nonce=<redacted>): newNonce → {NewNonce}", newNonce);

            try
            {
                var decodedBytes = Convert.FromBase64String(newNonce);
				Logger.LogError("IsNonceValid(nonce=<redacted>): decodedBytes → {DecodedBytes}", decodedBytes);
                var expireStr = Encoding.Default.GetString(decodedBytes);
				Logger.LogError("IsNonceValid(nonce=<redacted>): expireStr → {ExpireStr}", expireStr);
                expireTime = DateTime.Parse(expireStr, CultureInfo.InvariantCulture);
				Logger.LogError("IsNonceValid(nonce=<redacted>): expireTime → {ExpireTime}", expireTime);
            }
            catch (FormatException)
            {
				Logger.LogError("Exception in IsNonceValid(nonce=<redacted>): {Arg1}", FormatException.Message);
				Logger.LogDebug("IsNonceValid(nonce=<redacted>): expireTime → {ExpireTime}", expireTime);
			Logger.LogWarning("<<< Exiting IsNonceValid(nonce=<redacted>)");
                return false;
            }

            return DateTime.Now <= expireTime;
        }

        private void AuthenticateRequest()
        {
			Logger.LogError(">>> Entering AuthenticateRequest()");
            this.password = this.GetPassword(this.Request);
            if (this.password != null)
            {
                this.IsNonceStale = !IsNonceValid(this.Request.RequestParams["nonce"]);

                ////Services.Logging.LoggingController.SimpleLog(String.Format("Request hash: {0} - Response Hash: {1}", _request.RequestParams("response"), HashedDigest))
                if ((!this.IsNonceStale) && this.Request.RequestParams["response"] == this.CalculateHashedDigest())
                {
                    this.IsValid = true;
					Logger.LogDebug("AuthenticateRequest(): this.IsValid → {Arg0}", this.IsValid);
                    this.User = new GenericPrincipal(new GenericIdentity(this.Request.RawUsername, AuthenticationScheme), null);
                }
            }
			Logger.LogWarning("<<< Exiting AuthenticateRequest()");
        }

        private string GetPassword(DigestAuthenticationRequest request)
        {
			Logger.LogError(">>> Entering GetPassword(request={Request})", request);
            var user = UserController.GetUserByName(this.portalId, request.CleanUsername) ??
                       UserController.GetUserByName(this.portalId, request.RawUsername);
            if (user == null)
            {
                return null;
            }

            var userPassword = UserController.GetPassword(ref user, string.Empty);
			Logger.LogError("GetPassword(request={Request}): userPassword → {UserPassword}", request, userPassword);

            // Try to validate user
            var loginStatus = UserLoginStatus.LOGIN_FAILURE;
            user = UserController.ValidateUser(
                this.portalId,
                user.Username,
                userPassword,
                "DNN",
                string.Empty,
                this.ipAddress,
                ref loginStatus);

			Logger.LogWarning("<<< Exiting GetPassword(request={Request})", request);
            return user != null ? userPassword : null;
        }

        private string GenerateUnhashedDigest()
        {
            var cleanUsername = this.Request.RequestParams["username"].Replace(@"\\", @"\");
            var realm = this.Request.RequestParams["realm"];
            var ha1 = CreateMd5HashBinHex($"{cleanUsername}:{realm}:{this.password}");

            var uri = this.Request.RequestParams["uri"];
            var ha2 = CreateMd5HashBinHex($"{this.Request.HttpMethod}:{uri}");

            var nonce = this.Request.RequestParams["nonce"];
            var nc = this.Request.RequestParams["nc"];
            var cnonce = this.Request.RequestParams["cnonce"];
            var qop = this.Request.RequestParams["qop"];

            ////Services.Logging.LoggingController.SimpleLog(A1, HA1, A2, HA2, unhashedDigest)
            return qop != null ? $"{ha1}:{nonce}:{nc}:{cnonce}:{qop}:{ha2}" : $"{ha1}:{nonce}:{ha2}";
        }
    }
}
