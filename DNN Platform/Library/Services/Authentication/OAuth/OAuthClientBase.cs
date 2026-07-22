// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Authentication.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Localization;

    /// <summary>A base class for an OAuth client.</summary>
    public abstract class OAuthClientBase
    {
        protected const string OAuthTokenKey = "oauth_token";

        // oAuth 1
        private const string OAuthParameterPrefix = "oauth_";
        private const string OAuthConsumerKeyKey = "oauth_consumer_key";
        private const string OAuthCallbackKey = "oauth_callback";
        private const string OAuthVersionKey = "oauth_version";
        private const string OAuthSignatureKey = "oauth_signature";
        private const string OAuthSignatureMethodKey = "oauth_signature_method";
        private const string OAuthTimestampKey = "oauth_timestamp";
        private const string OAuthNonceKey = "oauth_nonce";
        private const string OAuthTokenSecretKey = "oauth_token_secret";
        private const string OAuthVerifierKey = "oauth_verifier";
        private const string OAuthCallbackConfirmedKey = "oauth_callback_confirmed";

        // oAuth 2
        private const string OAuthClientIdKey = "client_id";
        private const string OAuthClientSecretKey = "client_secret";
        private const string OAuthRedirectUriKey = "redirect_uri";
        private const string OAuthGrantTypeKey = "grant_type";
        private const string OAuthCodeKey = "code";

        private const string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

        // DNN-6265 - Support OAuth V2 optional parameter resource, which is required by Microsoft Azure Active
        // Directory implementation of OAuth V2
        private const string OAuthResourceKey = "resource";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OAuthClientBase));

        private readonly Random random = new Random();

        /// <summary>Initializes a new instance of the <see cref="OAuthClientBase"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="mode">The auth mode.</param>
        /// <param name="service">The service name.</param>
        protected OAuthClientBase(int portalId, AuthMode mode, string service)
        {
			Logger.LogDebug(">>> Entering OAuthClientBase(portalId={PortalId},mode={Mode},service={Service})", portalId, mode, service);
            // Set default Expiry to 14 days
            // oAuth v1 tokens do not expire
            // oAuth v2 tokens have an expiry
            this.AuthTokenExpiry = new TimeSpan(14, 0, 0, 0);
            this.Service = service;
			Logger.LogDebug("OAuthClientBase(portalId={PortalId},mode={Mode},service={Service}): this.Service → {Arg3}", portalId, mode, service, this.Service);

            this.APIKey = OAuthConfigBase.GetConfig(this.Service, portalId).APIKey;
            this.APISecret = OAuthConfigBase.GetConfig(this.Service, portalId).APISecret;
            this.Mode = mode;
			Logger.LogDebug("OAuthClientBase(portalId={PortalId},mode={Mode},service={Service}): this.Mode → {Arg3}", portalId, mode, service, this.Mode);

            this.CallbackUri = this.Mode == AuthMode.Login
                                    ? new Uri(Globals.LoginURL(string.Empty, false))
                                    : new Uri(Globals.RegisterURL(string.Empty, string.Empty));
									Logger.LogDebug("<<< Exiting OAuthClientBase(portalId={PortalId},mode={Mode},service={Service})", portalId, mode, service);
        }

        public virtual bool PrefixServiceToUserName
        {
            get { return true; }
        }

        public virtual bool AutoMatchExistingUsers
        {
            get { return false; }
        }

        public Uri CallbackUri { get; set; }

        public string Service { get; set; }

        protected virtual string UserGuidKey => string.Empty;

        // oAuth 1
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string OAuthVerifier => HttpContext.Current.Request.Params[OAuthVerifierKey];

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string VerificationCode => HttpContext.Current.Request.Params[OAuthCodeKey];

        protected string APIKey { get; set; }

        protected string APISecret { get; set; }

        protected AuthMode Mode { get; set; }

        protected string OAuthVersion { get; set; }

        protected HttpMethod TokenMethod { get; set; }

        protected Uri RequestTokenEndpoint { get; set; }

        protected HttpMethod RequestTokenMethod { get; set; }

        protected string TokenSecret { get; set; }

        protected string UserGuid { get; set; }

        protected string SignatureMethod { get; set; } = "HMAC-SHA1";

#pragma warning disable CA5350
        protected Func<KeyedHashAlgorithm> SignatureMethodAlgorithm { get; set; } = () => new HMACSHA1();
#pragma warning restore CA5350

        // oAuth 1 and 2
        protected Uri AuthorizationEndpoint { get; set; }

        protected string AuthToken { get; set; }

        protected TimeSpan AuthTokenExpiry { get; set; }

        protected Uri MeGraphEndpoint { get; set; }

        protected Uri TokenEndpoint { get; set; }

        protected string OAuthHeaderCode { get; set; }

        // oAuth 2
        protected string AuthTokenName { get; set; }

        protected string Scope { get; set; }

        protected string AccessToken { get; set; }

        // DNN-6265 Support "Optional" Resource Parameter required by Azure AD Oauth V2 Solution
        protected string APIResource { get; set; }

        /// <summary>
        /// This is a different URL Encode implementation since the default .NET one outputs the percent encoding in lower case.
        /// While this is not a problem with the percent encoding spec, it is used in upper case throughout OAuth.
        /// </summary>
        /// <param name="value">The value to URL encode.</param>
        /// <returns>Returns a URL encoded string.</returns>
        public static string UrlEncode(string value)
        {
			Logger.LogDebug(">>> Entering UrlEncode(value={Value})", value);
            var result = new StringBuilder();

			Logger.LogDebug("UrlEncode(value={Value}): result → {Result}", value, result);
            foreach (char symbol in value)
            {
                if (UnreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)symbol));
                }
            }

			Logger.LogDebug("<<< Exiting UrlEncode(value={Value})", value);
            return result.ToString();
        }

        public virtual void AuthenticateUser(UserData user, PortalSettings settings, string ipAddress, Action<NameValueCollection> addCustomProperties, Action<UserAuthenticatedEventArgs> onAuthenticated)
        {
            var loginStatus = UserLoginStatus.LOGIN_FAILURE;

            string userName = this.PrefixServiceToUserName ? this.Service + "-" + user.Id : user.Id;
            string token = this.Service + "-" + user.Id;

            UserInfo objUserInfo;

            if (this.AutoMatchExistingUsers)
            {
                objUserInfo = MembershipProvider.Instance().GetUserByUserName(settings.PortalId, userName);
                if (objUserInfo != null)
                {
                    // user already exists... lets check for a token next...
                    var dnnAuthToken = MembershipProvider.Instance().GetUserByAuthToken(settings.PortalId, token, this.Service);
                    if (dnnAuthToken == null)
                    {
                        DataProvider.Instance().AddUserAuthentication(objUserInfo.UserID, this.Service, token, objUserInfo.UserID);
                    }
                }
            }

            objUserInfo = UserController.ValidateUser(
                settings.PortalId,
                userName,
                string.Empty,
                this.Service,
                token,
                settings.PortalName,
                ipAddress,
                ref loginStatus);

            // Raise UserAuthenticated Event
            var eventArgs = new UserAuthenticatedEventArgs(objUserInfo, token, loginStatus, this.Service)
            {
                AutoRegister = true,
                UserName = userName,
            };

            var profileProperties = new NameValueCollection();

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.FirstName) && !string.IsNullOrEmpty(user.FirstName)))
            {
                profileProperties.Add("FirstName", user.FirstName);
            }

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.LastName) && !string.IsNullOrEmpty(user.LastName)))
            {
                profileProperties.Add("LastName", user.LastName);
            }

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Email) && !string.IsNullOrEmpty(user.Email)))
            {
                profileProperties.Add("Email", user.PreferredEmail);
            }

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.DisplayName) && !string.IsNullOrEmpty(user.DisplayName)))
            {
                profileProperties.Add("DisplayName", user.DisplayName);
            }

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("ProfileImage")) && !string.IsNullOrEmpty(user.ProfileImage)))
            {
                profileProperties.Add("ProfileImage", user.ProfileImage);
            }

            if (objUserInfo == null || (string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("Website")) && !string.IsNullOrEmpty(user.Website)))
            {
                profileProperties.Add("Website", user.Website);
            }

            if ((objUserInfo == null || string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("PreferredLocale"))) && !string.IsNullOrEmpty(user.Locale))
            {
                if (LocaleController.IsValidCultureName(user.Locale.Replace('_', '-')))
                {
                    profileProperties.Add("PreferredLocale", user.Locale.Replace('_', '-'));
                }
                else
                {
                    profileProperties.Add("PreferredLocale", settings.CultureCode);
                }
            }

            if (objUserInfo == null || string.IsNullOrEmpty(objUserInfo.Profile.GetPropertyValue("PreferredTimeZone")))
            {
                if (string.IsNullOrEmpty(user.TimeZoneInfo))
                {
                    int timeZone;
                    if (int.TryParse(user.Timezone, out timeZone))
                    {
                        var timeZoneInfo = Localization.ConvertLegacyTimeZoneOffsetToTimeZoneInfo(timeZone);

                        profileProperties.Add("PreferredTimeZone", timeZoneInfo.Id);
                    }
                }
                else
                {
                    profileProperties.Add("PreferredTimeZone", user.TimeZoneInfo);
                }
            }

            addCustomProperties(profileProperties);

            eventArgs.Profile = profileProperties;

            if (this.Mode == AuthMode.Login)
            {
                this.SaveTokenCookie(string.Empty);
            }

            onAuthenticated(eventArgs);
        }

        public virtual AuthorisationResult Authorize()
        {
			Logger.LogDebug(">>> Entering Authorize()");
            if (this.OAuthVersion == "1.0")
            {
                return this.AuthorizeV1();
            }

			Logger.LogDebug("<<< Exiting Authorize()");
            return this.AuthorizeV2();
        }

        /// <summary>Generates a signature using the <see cref="SignatureMethodAlgorithm"/>.</summary>
        /// <param name="url">The full url that needs to be signed including its non OAuth url parameters.</param>
        /// <param name="token">The token, if available. If not available pass <see langword="null"/> or <see cref="string.Empty"/>.</param>
        /// <param name="tokenSecret">The token secret, if available. If not available pass <see langword="null"/> or <see cref="string.Empty"/>.</param>
        /// <param name="callbackurl">The callback URL.</param>
        /// <param name="oauthVerifier">This value MUST be included when exchanging Request Tokens for Access Tokens. Otherwise, pass <see langword="null"/> or <see cref="string.Empty"/>.</param>
        /// <param name="httpMethod">The http method used. Must be a valid HTTP method verb (POST, GET, PUT, etc.).</param>
        /// <param name="timeStamp">The Unix-formatted timestamp.</param>
        /// <param name="nonce">The nonce.</param>
        /// <param name="normalizedUrl">A normalized version of <paramref name="url"/>.</param>
        /// <param name="requestParameters">The OAuth request parameters.</param>
        /// <returns>A base64 string of the hash value.</returns>
        public string GenerateSignature(Uri url, string token, string tokenSecret, string callbackurl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out List<QueryParameter> requestParameters)
        {
            string signatureBase = this.GenerateSignatureBase(url, token, callbackurl, oauthVerifier, httpMethod, timeStamp, nonce, out normalizedUrl, out requestParameters);

            using var hashAlgorithm = this.SignatureMethodAlgorithm();
            hashAlgorithm.Key = Encoding.ASCII.GetBytes($"{UrlEncode(this.APISecret)}&{(string.IsNullOrEmpty(tokenSecret) ? string.Empty : UrlEncode(tokenSecret))}");

            return GenerateSignatureUsingHash(signatureBase, hashAlgorithm);
        }

        public virtual TUserData GetCurrentUser<TUserData>()
            where TUserData : UserData
        {
			Logger.LogDebug(">>> Entering GetCurrentUser()");
            this.LoadTokenCookie(string.Empty);

            if (!this.IsCurrentUserAuthorized())
            {
                return null;
            }

            var accessToken = string.IsNullOrEmpty(this.AccessToken) ? "access_token=" + this.AuthToken : this.AccessToken + "=" + this.AuthToken;
            string responseText = (this.OAuthVersion == "1.0")
                            ? this.ExecuteAuthorizedRequest(HttpMethod.GET, this.MeGraphEndpoint)
                            : this.ExecuteWebRequest(HttpMethod.GET, GenerateRequestUri(this.MeGraphEndpoint.ToString(), accessToken), null, string.Empty);
							Logger.LogDebug("GetCurrentUser(): accessToken → {AccessToken}", accessToken);
            var user = Json.Deserialize<TUserData>(responseText);
            return user;
        }

        public bool HaveVerificationCode()
        {
			Logger.LogDebug(">>> Entering HaveVerificationCode()");
			Logger.LogDebug("<<< Exiting HaveVerificationCode()");
            return (this.OAuthVersion == "1.0") ? (this.OAuthVerifier != null) : (this.VerificationCode != null);
        }

        public bool IsCurrentService()
        {
			Logger.LogDebug(">>> Entering IsCurrentService()");
            string service = HttpContext.Current.Request.Params["state"];
			Logger.LogDebug("<<< Exiting IsCurrentService()");
            return !string.IsNullOrEmpty(service) && service == this.Service;
        }

        public bool IsCurrentUserAuthorized()
        {
			Logger.LogDebug(">>> Entering IsCurrentUserAuthorized()");
			Logger.LogDebug("<<< Exiting IsCurrentUserAuthorized()");
            return !string.IsNullOrEmpty(this.AuthToken);
        }

        public void RemoveToken()
        {
			Logger.LogDebug(">>> Entering RemoveToken()");
            var authTokenCookie = new HttpCookie(this.AuthTokenName)
            {
                Expires = DateTime.Now.AddDays(-30),
                Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/",
            };
            HttpContext.Current.Response.SetCookie(authTokenCookie);
			Logger.LogDebug("<<< Exiting RemoveToken()");
        }

        /// <summary>Generate the timestamp for the signature.</summary>
        /// <returns>A string representation of <see cref="DateTime.Now"/> in the Unix timestamp format.</returns>
        protected virtual string GenerateTimeStamp()
        {
			Logger.LogDebug(">>> Entering GenerateTimeStamp()");
            // Default implementation of UNIX time of the current UTC time
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			Logger.LogDebug("GenerateTimeStamp(): ts → {Ts}", ts);
			Logger.LogDebug("<<< Exiting GenerateTimeStamp()");
            return Convert.ToInt64(ts.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Generate a nonce.</summary>
        /// <returns>A 6-7 character length nonce string.</returns>
        protected virtual string GenerateNonce()
        {
			Logger.LogDebug(">>> Entering GenerateNonce()");
            // Just a simple implementation of a random number between 123400 and 9999999
			Logger.LogDebug("<<< Exiting GenerateNonce()");
            return this.random.Next(123400, 9999999).ToString(CultureInfo.InvariantCulture);
        }

        protected virtual TimeSpan GetExpiry(string responseText)
        {
            return TimeSpan.MinValue;
        }

        protected virtual string GetToken(string responseText)
        {
            return responseText;
        }

        protected void LoadTokenCookie(string suffix)
        {
			Logger.LogDebug(">>> Entering LoadTokenCookie(suffix={Suffix})", suffix);
            HttpCookie authTokenCookie = HttpContext.Current.Request.Cookies[this.AuthTokenName + suffix];
            if (authTokenCookie != null)
            {
                if (authTokenCookie.HasKeys)
                {
                    this.AuthToken = authTokenCookie.Values[OAuthTokenKey];
					Logger.LogDebug("LoadTokenCookie(suffix={Suffix}): this.AuthToken → {Arg1}", suffix, this.AuthToken);
                    this.TokenSecret = authTokenCookie.Values[OAuthTokenSecretKey];
                    this.UserGuid = authTokenCookie.Values[this.UserGuidKey];
					Logger.LogDebug("LoadTokenCookie(suffix={Suffix}): this.TokenSecret → {Arg1}", suffix, this.TokenSecret);
					Logger.LogDebug("LoadTokenCookie(suffix={Suffix}): this.UserGuid → {Arg1}", suffix, this.UserGuid);
                }
            }
			Logger.LogDebug("<<< Exiting LoadTokenCookie(suffix={Suffix})", suffix);
        }

        private static string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
			Logger.LogDebug(">>> Entering ComputeHash(hashAlgorithm={HashAlgorithm},data={Data})", hashAlgorithm, data);
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException(nameof(hashAlgorithm));
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            byte[] dataBuffer = Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

			Logger.LogDebug("<<< Exiting ComputeHash(hashAlgorithm={HashAlgorithm},data={Data})", hashAlgorithm, data);
            return Convert.ToBase64String(hashBytes);
        }

        private static string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }

        private static List<QueryParameter> GetQueryParameters(string parameters)
        {
            if (parameters.StartsWith("?", StringComparison.Ordinal))
            {
                parameters = parameters.Remove(0, 1);
            }
			Logger.LogDebug("GetQueryParameters(parameters={Parameters}): parameters → {Parameters1}", parameters, parameters);

            var result = new List<QueryParameter>();

			Logger.LogDebug("GetQueryParameters(parameters={Parameters}): result → {Result}", parameters, result);
            if (!string.IsNullOrEmpty(parameters))
            {
                string[] p = parameters.Split('&');
                foreach (string s in p)
                {
                    if (!string.IsNullOrEmpty(s) && !s.StartsWith(OAuthParameterPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        if (s.IndexOf('=') > -1)
                        {
                            string[] temp = s.Split('=');
                            result.Add(new QueryParameter(temp[0], temp[1]));
                        }
                        else
                        {
                            result.Add(new QueryParameter(s, string.Empty));
                        }
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetQueryParameters(parameters={Parameters})", parameters);
            return result;
        }

        private static Uri GenerateRequestUri(string url, string parameters)
        {
            if (string.IsNullOrEmpty(parameters))
            {
                return new Uri(url);
            }

            var separator = url.Contains("?") ? "&" : "?";
            return new Uri($"{url}{separator}{parameters}");
        }

        private AuthorisationResult AuthorizeV1()
        {
			Logger.LogDebug(">>> Entering AuthorizeV1()");
            if (!this.IsCurrentUserAuthorized())
            {
                if (!this.HaveVerificationCode())
                {
                    string ret = null;

                    string response = this.RequestToken();

					Logger.LogDebug("AuthorizeV1(): response → {Response}", response);
                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        // response contains token and token secret. We only need the token.
                        NameValueCollection qs = HttpUtility.ParseQueryString(response);

						Logger.LogDebug("AuthorizeV1(): qs → {Qs}", qs);
                        if (qs[OAuthCallbackConfirmedKey] != null)
                        {
                            if (qs[OAuthCallbackConfirmedKey] != "true")
                            {
                                throw new OAuthException("OAuth callback not confirmed.");
                            }
                        }

                        if (qs[OAuthTokenKey] != null)
                        {
                            ret = this.AuthorizationEndpoint + "?" + OAuthTokenKey + "=" + qs[OAuthTokenKey];

							Logger.LogDebug("AuthorizeV1(): this.TokenSecret → {Arg0}", this.TokenSecret);
                            this.AuthToken = qs[OAuthTokenKey];
							Logger.LogDebug("AuthorizeV1(): this.AuthToken → {Arg0}", this.AuthToken);
                            this.TokenSecret = qs[OAuthTokenSecretKey];
                            this.SaveTokenCookie("_request");
                        }
                    }

                    if (ret != null)
                    {
                        HttpContext.Current.Response.Redirect(ret, true);
                    }

                    return AuthorisationResult.RequestingCode;
                }

                this.ExchangeRequestTokenForToken();
            }

			Logger.LogDebug("<<< Exiting AuthorizeV1()");
            return AuthorisationResult.Authorized;
        }

        private AuthorisationResult AuthorizeV2()
        {
			Logger.LogDebug(">>> Entering AuthorizeV2()");
            string errorReason = HttpContext.Current.Request.Params["error_reason"];
            bool userDenied = errorReason != null;
            if (userDenied)
            {
                return AuthorisationResult.Denied;
            }

            if (!this.HaveVerificationCode())
            {
                var parameters = new List<QueryParameter>
                {
                    new QueryParameter("scope", this.Scope),
                    new QueryParameter(OAuthClientIdKey, this.APIKey),
                    new QueryParameter(OAuthRedirectUriKey, HttpContext.Current.Server.UrlEncode(this.CallbackUri.ToString())),
                    new QueryParameter("state", this.Service),
                    new QueryParameter("response_type", "code"),
                };

                HttpContext.Current.Response.Redirect(this.AuthorizationEndpoint + "?" + parameters.ToNormalizedString(), false);
                return AuthorisationResult.RequestingCode;
            }

            this.ExchangeCodeForToken();

			Logger.LogDebug("<<< Exiting AuthorizeV2()");
            return string.IsNullOrEmpty(this.AuthToken) ? AuthorisationResult.Denied : AuthorisationResult.Authorized;
        }

        private void ExchangeCodeForToken()
        {
			Logger.LogDebug(">>> Entering ExchangeCodeForToken()");
            var parameters = new List<QueryParameter>
            {
                new QueryParameter(OAuthClientIdKey, this.APIKey),
                new QueryParameter(OAuthRedirectUriKey, HttpContext.Current.Server.UrlEncode(this.CallbackUri.ToString())),

                // DNN-6265 Support for OAuth V2 Secrets which are not URL Friendly
                new QueryParameter(OAuthClientSecretKey, HttpContext.Current.Server.UrlEncode(this.APISecret)),
                new QueryParameter(OAuthGrantTypeKey, "authorization_code"),
                new QueryParameter(OAuthCodeKey, this.VerificationCode),
            };

            // DNN-6265 Support for OAuth V2 optional parameter
            if (!string.IsNullOrEmpty(this.APIResource))
            {
                parameters.Add(new QueryParameter("resource", this.APIResource));
            }

            var responseText = this.ExecuteWebRequest(this.TokenMethod, this.TokenEndpoint, parameters.ToNormalizedString(), string.Empty);

			Logger.LogDebug("ExchangeCodeForToken(): responseText → {ResponseText}", responseText);
            this.AuthToken = this.GetToken(responseText);
            this.AuthTokenExpiry = this.GetExpiry(responseText);
			Logger.LogDebug("<<< Exiting ExchangeCodeForToken()");
        }

        private void ExchangeRequestTokenForToken()
        {
			Logger.LogDebug(">>> Entering ExchangeRequestTokenForToken()");
            this.LoadTokenCookie("_request");

            string response = this.ExecuteAuthorizedRequest(HttpMethod.POST, this.TokenEndpoint);

			Logger.LogDebug("ExchangeRequestTokenForToken(): response → {Response}", response);
            if (response.Length > 0)
            {
                // Store the Token and Token Secret
                NameValueCollection qs = HttpUtility.ParseQueryString(response);
                if (qs[OAuthTokenKey] != null)
				Logger.LogDebug("ExchangeRequestTokenForToken(): qs → {Qs}", qs);
                {
                    this.AuthToken = qs[OAuthTokenKey];
					Logger.LogDebug("ExchangeRequestTokenForToken(): this.AuthToken → {Arg0}", this.AuthToken);
                }

                if (qs[OAuthTokenSecretKey] != null)
                {
                    this.TokenSecret = qs[OAuthTokenSecretKey];
					Logger.LogDebug("ExchangeRequestTokenForToken(): this.TokenSecret → {Arg0}", this.TokenSecret);
                }

                if (qs[this.UserGuidKey] != null)
                {
                    this.UserGuid = qs[this.UserGuidKey];
					Logger.LogDebug("ExchangeRequestTokenForToken(): this.UserGuid → {Arg0}", this.UserGuid);
                }
            }
			Logger.LogDebug("<<< Exiting ExchangeRequestTokenForToken()");
        }

        private string ExecuteAuthorizedRequest(HttpMethod method, Uri uri)
        {
			Logger.LogDebug(">>> Entering ExecuteAuthorizedRequest(method={Method},uri={Uri})", method, uri);
            string outUrl;
            List<QueryParameter> requestParameters;

            string nonce = this.GenerateNonce();
            string timeStamp = this.GenerateTimeStamp();

            string verifier = (uri == this.TokenEndpoint) ? this.OAuthVerifier : string.Empty;

            // Generate Signature
            string sig = this.GenerateSignature(
                uri,
                this.AuthToken,
                this.TokenSecret,
                string.Empty,
                verifier,
                method.ToString(),
                timeStamp,
                nonce,
                out outUrl,
                out requestParameters);

            var headerParameters = new List<QueryParameter>
                                       {
                                           new QueryParameter(OAuthConsumerKeyKey, this.APIKey),
                                           new QueryParameter(OAuthNonceKey, nonce),
                                           new QueryParameter(OAuthSignatureKey, sig),
                                           new QueryParameter(OAuthSignatureMethodKey, this.SignatureMethod),
                                           new QueryParameter(OAuthTimestampKey, timeStamp),
                                           new QueryParameter(OAuthTokenKey, this.AuthToken),
                                           new QueryParameter(OAuthVersionKey, this.OAuthVersion),
                                       };
            if (uri == this.TokenEndpoint)
            {
                headerParameters.Add(new QueryParameter(OAuthVerifierKey, this.OAuthVerifier));
            }

            string ret = this.ExecuteWebRequest(method, uri, string.Empty, headerParameters.ToAuthorizationString());

			Logger.LogDebug("<<< Exiting ExecuteAuthorizedRequest(method={Method},uri={Uri})", method, uri);
            return ret;
        }

        private string ExecuteWebRequest(HttpMethod method, Uri uri, string parameters, string authHeader)
        {
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            WebRequest request;

            if (method == HttpMethod.POST)
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(parameters);

                request = WebRequest.CreateDefault(uri);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                ////request.ContentType = "text/xml";
                request.ContentLength = byteArray.Length;

                if (!string.IsNullOrEmpty(this.OAuthHeaderCode))
                {
                    byte[] api64 = Encoding.UTF8.GetBytes(this.APIKey + ":" + this.APISecret);
                    string api64Encoded = System.Convert.ToBase64String(api64);

                    // Authentication providers needing an "Authorization: Basic/bearer base64(clientID:clientSecret)" header. OAuthHeaderCode might be: Basic/Bearer/empty.
                    request.Headers.Add("Authorization: " + this.OAuthHeaderCode + " " + api64Encoded);
                }

                if (!string.IsNullOrEmpty(parameters))
                {
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }
            }
            else
            {
                request = WebRequest.CreateDefault(GenerateRequestUri(uri.ToString(), parameters));
            }

            // Add Headers
            if (!string.IsNullOrEmpty(authHeader))
            {
                request.Headers.Add(HttpRequestHeader.Authorization, authHeader);
            }

            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                        {
                            using (var responseReader = new StreamReader(responseStream))
                            {
                                return responseReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                using var responseStream = ex.Response.GetResponseStream();
                if (responseStream != null)
                {
                    using var responseReader = new StreamReader(responseStream);
                    Logger.ErrorFormat(CultureInfo.InvariantCulture, "WebResponse exception: {0}", responseReader.ReadToEnd());
                }
            }

            return null;
        }

        private string GenerateSignatureBase(Uri url, string token, string callbackUrl, string oauthVerifier, string httpMethod, string timeStamp, string nonce, out string normalizedUrl, out List<QueryParameter> requestParameters)
        {
            if (token == null)
            {
                token = string.Empty;
            }
			Logger.LogWarning("GenerateSignatureBase(url={Url},token={Token},callbackUrl={CallbackUrl},oauthVerifier={OauthVerifier},httpMethod={HttpMethod},timeStamp={TimeStamp},nonce={Nonce}): token → {Token1}", url, token, callbackUrl, oauthVerifier, httpMethod, timeStamp, nonce, token);

            if (string.IsNullOrEmpty(httpMethod))
            {
                throw new ArgumentNullException(nameof(httpMethod));
            }

            requestParameters = GetQueryParameters(url.Query);
			Logger.LogDebug("GenerateSignatureBase(url={Url},token={Token},callbackUrl={CallbackUrl},oauthVerifier={OauthVerifier},httpMethod={HttpMethod},timeStamp={TimeStamp},nonce={Nonce}): requestParameters → {RequestParameters}", url, token, callbackUrl, oauthVerifier, httpMethod, timeStamp, nonce, requestParameters);
            requestParameters.Add(new QueryParameter(OAuthVersionKey, this.OAuthVersion));
            requestParameters.Add(new QueryParameter(OAuthNonceKey, nonce));
            requestParameters.Add(new QueryParameter(OAuthTimestampKey, timeStamp));
            requestParameters.Add(new QueryParameter(OAuthSignatureMethodKey, this.SignatureMethod));
            requestParameters.Add(new QueryParameter(OAuthConsumerKeyKey, this.APIKey));

            if (!string.IsNullOrEmpty(callbackUrl))
            {
                requestParameters.Add(new QueryParameter(OAuthCallbackKey, UrlEncode(callbackUrl)));
            }

            if (!string.IsNullOrEmpty(oauthVerifier))
            {
                requestParameters.Add(new QueryParameter(OAuthVerifierKey, oauthVerifier));
            }

            if (!string.IsNullOrEmpty(token))
            {
                requestParameters.Add(new QueryParameter(OAuthTokenKey, token));
            }

            requestParameters.Sort(new QueryParameterComparer());

            normalizedUrl = $"{url.Scheme}://{url.Host}";
			Logger.LogDebug("GenerateSignatureBase(url={Url},token={Token},callbackUrl={CallbackUrl},oauthVerifier={OauthVerifier},httpMethod={HttpMethod},timeStamp={TimeStamp},nonce={Nonce}): normalizedUrl → {NormalizedUrl}", url, token, callbackUrl, oauthVerifier, httpMethod, timeStamp, nonce, normalizedUrl);
            if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443)))
            {
                normalizedUrl += ":" + url.Port;
            }

            normalizedUrl += url.AbsolutePath;
            string normalizedRequestParameters = requestParameters.ToNormalizedString();

            var signatureBase = new StringBuilder();
            signatureBase.AppendFormat(CultureInfo.InvariantCulture, "{0}&", httpMethod.ToUpperInvariant());
			Logger.LogDebug("GenerateSignatureBase(url={Url},token={Token},callbackUrl={CallbackUrl},oauthVerifier={OauthVerifier},httpMethod={HttpMethod},timeStamp={TimeStamp},nonce={Nonce}): signatureBase → {SignatureBase}", url, token, callbackUrl, oauthVerifier, httpMethod, timeStamp, nonce, signatureBase);
            signatureBase.AppendFormat(CultureInfo.InvariantCulture, "{0}&", UrlEncode(normalizedUrl));
            signatureBase.AppendFormat(CultureInfo.InvariantCulture, "{0}", UrlEncode(normalizedRequestParameters));

			Logger.LogDebug("<<< Exiting GenerateSignatureBase(url={Url},token={Token},callbackUrl={CallbackUrl},oauthVerifier={OauthVerifier},httpMethod={HttpMethod},timeStamp={TimeStamp},nonce={Nonce})", url, token, callbackUrl, oauthVerifier, httpMethod, timeStamp, nonce);
            return signatureBase.ToString();
        }

        private string RequestToken()
        {
			Logger.LogDebug(">>> Entering RequestToken()");
            string nonce = this.GenerateNonce();
            string timeStamp = this.GenerateTimeStamp();

            string sig = this.GenerateSignature(
                this.RequestTokenEndpoint,
                string.Empty,
                string.Empty,
                this.CallbackUri.OriginalString,
                string.Empty,
                this.RequestTokenMethod.ToString(),
                timeStamp,
                nonce,
                out var outUrl,
                out _);

            var headerParameters = new List<QueryParameter>
                                       {
                                           new QueryParameter(OAuthCallbackKey, this.CallbackUri.OriginalString),
                                           new QueryParameter(OAuthConsumerKeyKey, this.APIKey),
                                           new QueryParameter(OAuthNonceKey, nonce),
                                           new QueryParameter(OAuthSignatureKey, sig),
                                           new QueryParameter(OAuthSignatureMethodKey, this.SignatureMethod),
                                           new QueryParameter(OAuthTimestampKey, timeStamp),
                                           new QueryParameter(OAuthVersionKey, this.OAuthVersion),
                                       };

            string ret = this.ExecuteWebRequest(this.RequestTokenMethod, new Uri(outUrl), string.Empty, headerParameters.ToAuthorizationString());

			Logger.LogDebug("<<< Exiting RequestToken()");
            return ret;
        }

        private void SaveTokenCookie(string suffix)
        {
			Logger.LogDebug(">>> Entering SaveTokenCookie(suffix={Suffix})", suffix);
            var authTokenCookie = new HttpCookie(this.AuthTokenName + suffix) { Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" };
            authTokenCookie.Values[OAuthTokenKey] = this.AuthToken;
			Logger.LogDebug("SaveTokenCookie(suffix={Suffix}): authTokenCookie → {AuthTokenCookie}", suffix, authTokenCookie);
            authTokenCookie.Values[OAuthTokenSecretKey] = this.TokenSecret;
            authTokenCookie.Values[this.UserGuidKey] = this.UserGuid;

            authTokenCookie.Expires = DateTime.Now.Add(this.AuthTokenExpiry);
            HttpContext.Current.Response.SetCookie(authTokenCookie);
			Logger.LogDebug("<<< Exiting SaveTokenCookie(suffix={Suffix})", suffix);
        }
    }
}
