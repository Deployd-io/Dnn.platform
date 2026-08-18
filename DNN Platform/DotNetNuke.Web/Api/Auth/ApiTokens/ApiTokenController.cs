// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Repositories;

    using Microsoft.Extensions.DependencyInjection;

    /// <inheritdoc />
    public class ApiTokenController : IApiTokenController
    {
        private const string AuthScheme = "Bearer";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ApiTokenController));
        private static readonly Encoding TextEncoder = Encoding.UTF8;

        private readonly IApiTokenRepository apiTokenRepository;
        private readonly IEventLogger eventLogger;
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="ApiTokenController"/> class.</summary>
        /// <param name="apiTokenRepository">The API token repository.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ApiTokenController(IApiTokenRepository apiTokenRepository, IEventLogger eventLogger)
            : this(apiTokenRepository, eventLogger, null)
        {
		Logger.LogDebug(">>> Entering ApiTokenController(apiTokenRepository=<redacted>,eventLogger={EventLogger})", eventLogger);
		Logger.LogDebug("ApiTokenController(apiTokenRepository=<redacted>,eventLogger={EventLogger},hostSettings={HostSettings}): apiTokenRepository → {ApiTokenRepository1}", eventLogger, hostSettings, apiTokenRepository);
        }

		Logger.LogDebug("<<< Exiting ApiTokenController(apiTokenRepository=<redacted>,eventLogger={EventLogger})", eventLogger);
        /// <summary>Initializes a new instance of the <see cref="ApiTokenController"/> class.</summary>
        /// <param name="apiTokenRepository">The API token repository.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="hostSettings">The host settings.</param>
        public ApiTokenController(IApiTokenRepository apiTokenRepository, IEventLogger eventLogger, IHostSettings hostSettings)
        {
            this.apiTokenRepository = apiTokenRepository ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApiTokenRepository>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
			Logger.LogDebug("ApiTokenController(apiTokenRepository=<redacted>,eventLogger={EventLogger},hostSettings={HostSettings}): eventLogger → {EventLogger1}", eventLogger, hostSettings, eventLogger);
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("ApiTokenController(apiTokenRepository=<redacted>,eventLogger={EventLogger},hostSettings={HostSettings}): hostSettings → {HostSettings1}", eventLogger, hostSettings, hostSettings);
        }

        /// <inheritdoc />
        public string SchemeType => "ApiToken";

        private static Abstractions.Portals.IPortalSettings PortalSettings => PortalController.Instance.GetCurrentSettings();

        /// <inheritdoc />
        public (ApiToken Token, UserInfo User) ValidateToken(HttpRequestMessage request)
        {
			Logger.LogDebug(">>> Entering ValidateToken(Token=<redacted>,User={User})", User);
            if (!ApiTokenAuthMessageHandler.IsEnabled)
            {
                Logger.Trace(this.SchemeType + " is not registered/enabled in web.config file");
                return (null, null);
            }

            var authorization = ValidateAuthHeader(request?.Headers.Authorization);
			Logger.LogDebug("ValidateToken(Token=<redacted>,User={User}): authorization → {Authorization}", User, authorization);
			Logger.LogDebug("<<< Exiting ValidateToken(Token=<redacted>,User={User})", User);
            return string.IsNullOrEmpty(authorization) ? (null, null) : this.ValidateAuthorizationValue(authorization);
        }

        /// <inheritdoc />
        public void SetApiTokenForRequest(ApiToken token)
        {
			Logger.LogDebug(">>> Entering SetApiTokenForRequest(token=<redacted>)");
            HttpContext.Current.Items["ApiToken"] = token;
			Logger.LogDebug("<<< Exiting SetApiTokenForRequest(token=<redacted>)");
        }

        /// <inheritdoc />
        public ApiToken GetCurrentThreadApiToken()
        {
			Logger.LogDebug(">>> Entering GetCurrentThreadApiToken()");
            if (HttpContext.Current != null && HttpContext.Current.Items["ApiToken"] is ApiToken token)
            {
			Logger.LogDebug("<<< Exiting GetCurrentThreadApiToken()");
                return token;
            }

            return null;
        }

        /// <inheritdoc />
        public SortedDictionary<string, ApiTokenAttribute> ApiTokenKeyList(ApiTokenScope scope, string locale)
        {
            var res = new SortedDictionary<string, ApiTokenAttribute>();
            var typeLocator = new TypeLocator();
            var attributes =
                from type in typeLocator.GetAllMatchingTypes(t => t is { IsClass: true, IsAbstract: false, IsVisible: true, })
                let typeAttributes = type.GetCustomAttributes<ApiTokenAuthorizeAttribute>(inherit: false)
                from method in type.GetMethods()
                let methodAttributes = method.GetCustomAttributes<ApiTokenAuthorizeAttribute>(inherit: false)
                from attribute in typeAttributes.Concat(methodAttributes)
                where attribute.Scope <= scope
                select attribute;

            foreach (var attr in attributes)
            {
                var key = attr.Key.ToLowerInvariant();
                var k = $"{attr.Scope}{key}";
                if (!res.ContainsKey(k))
                {
                    var name = DotNetNuke.Services.Localization.Localization.GetString(attr.Key + ".Text", attr.ResourceFile, locale);
                    if (string.IsNullOrEmpty(name))
                    {
                        name = attr.Key;
                    }

                    var description = DotNetNuke.Services.Localization.Localization.GetString(attr.Key + ".Help", attr.ResourceFile, locale);
                    res.Add(k, new ApiTokenAttribute((int)attr.Scope, key, name, description));
                }
            }

            return res;
        }

        /// <inheritdoc />
        public IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey=<redacted>,pageIndex={PageIndex},pageSize={PageSize})", scope, includeNarrowerScopes, portalId, userId, filter, pageIndex, pageSize);
			Logger.LogDebug("<<< Exiting GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey=<redacted>,pageIndex={PageIndex},pageSize={PageSize})", scope, includeNarrowerScopes, portalId, userId, filter, pageIndex, pageSize);
            return this.apiTokenRepository.GetApiTokens(scope, includeNarrowerScopes, portalId, userId, filter, apiKey, pageIndex, pageSize);
        }

        /// <inheritdoc />
        public string CreateApiToken(int portalId, string tokenName, ApiTokenScope scope, DateTime expiresOn, string apiKeys, int userId)
        {
			Logger.LogDebug(">>> Entering CreateApiToken(portalId={PortalId},tokenName=<redacted>,scope={Scope},expiresOn={ExpiresOn},apiKeys=<redacted>,userId={UserId})", portalId, scope, expiresOn, userId);
            if (scope == ApiTokenScope.Host)
            {
                portalId = Null.NullInteger;
            }

            string newToken;
            using (var generator = RandomNumberGenerator.Create())
            {
                var tokenBytes = new byte[32];
                generator.GetBytes(tokenBytes);
                newToken = EncodeBase64(tokenBytes);
            }
			Logger.LogDebug("CreateApiToken(portalId={PortalId},tokenName=<redacted>,scope={Scope},expiresOn={ExpiresOn},apiKeys=<redacted>,userId={UserId}): newToken → {NewToken}", portalId, scope, expiresOn, userId, newToken);

            var tokenAndHostGuid = newToken + this.hostSettings.Guid;
            var hashedToken = GetHashedStr(tokenAndHostGuid);
			Logger.LogDebug("CreateApiToken(portalId={PortalId},tokenName=<redacted>,scope={Scope},expiresOn={ExpiresOn},apiKeys=<redacted>,userId={UserId}): hashedToken → {HashedToken}", portalId, scope, expiresOn, userId, hashedToken);

            var token = new ApiTokenBase()
            {
                PortalId = portalId,
                TokenName = tokenName,
                Scope = scope,
                ExpiresOn = expiresOn,
                TokenHash = hashedToken,
            };
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var ret = this.apiTokenRepository.AddApiToken(token, apiKeys, userId);
            this.eventLogger.AddLog(ret.ToLogProps(), PortalSettings, userId, nameof(EventLogType.APITOKEN_CREATED), false);
			Logger.LogDebug("CreateApiToken(portalId={PortalId},tokenName=<redacted>,scope={Scope},expiresOn={ExpiresOn},apiKeys=<redacted>,userId={UserId}): ret → {Ret}", portalId, scope, expiresOn, userId, ret);
			Logger.LogInformation("CreateApiToken(portalId,tokenName,scope,expiresOn,apiKeys,userId)={PortalId},{TokenName},{Scope},{ExpiresOn},{ApiKeys},{UserId}: database query executed in {Arg6} ms", portalId, tokenName, scope, expiresOn, apiKeys, userId, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting CreateApiToken(portalId={PortalId},tokenName=<redacted>,scope={Scope},expiresOn={ExpiresOn},apiKeys=<redacted>,userId={UserId})", portalId, scope, expiresOn, userId);
            return newToken;
        }

        /// <inheritdoc />
        public ApiToken GetApiToken(int apiTokenId)
        {
			Logger.LogDebug(">>> Entering GetApiToken(apiTokenId=<redacted>)");
			Logger.LogDebug("<<< Exiting GetApiToken(apiTokenId=<redacted>)");
            return this.apiTokenRepository.GetApiToken(apiTokenId);
        }

        /// <inheritdoc />
        public void RevokeOrDeleteApiToken(ApiToken token, bool delete, int userId)
        {
			Logger.LogDebug(">>> Entering RevokeOrDeleteApiToken(token=<redacted>,delete={Delete},userId={UserId})", delete, userId);
            if (delete)
            {
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
				Logger.LogInformation("RevokeOrDeleteApiToken(token,delete,userId)={Token},{Delete},{UserId}: delete query executed in {Arg3} ms", token, delete, userId, __logStart.ElapsedMilliseconds);
                this.apiTokenRepository.DeleteApiToken(token.ToBase());
                this.eventLogger.AddLog(token.ToLogProps(), PortalSettings, userId, nameof(EventLogType.APITOKEN_DELETED), false);
            }
            else
            {
				Logger.LogInformation("RevokeOrDeleteApiToken(token,delete,userId)={Token},{Delete},{UserId}: database query executed in {Arg3} ms", token, delete, userId, __logStart.ElapsedMilliseconds);
                this.apiTokenRepository.RevokeApiToken(token.ToBase(), userId);
                this.eventLogger.AddLog(token.ToLogProps(), PortalSettings, userId, nameof(EventLogType.APITOKEN_REVOKED), false);
            }
			Logger.LogDebug("<<< Exiting RevokeOrDeleteApiToken(token=<redacted>,delete={Delete},userId={UserId})", delete, userId);
        }

        /// <inheritdoc />
        public void DeleteExpiredAndRevokedApiTokens(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId})", portalId, userId);
            this.apiTokenRepository.DeleteExpiredAndRevokedApiTokens(portalId, userId);
			Logger.LogDebug("<<< Exiting DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        private static string ValidateAuthHeader(AuthenticationHeaderValue authHdr)
        {
			Logger.LogDebug(">>> Entering ValidateAuthHeader(authHdr={AuthHdr})", authHdr);
            if (authHdr == null)
            {
                return null;
            }

            if (!string.Equals(authHdr.Scheme, AuthScheme, StringComparison.OrdinalIgnoreCase))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Authorization header scheme in the request is not equal to " + AuthScheme);
                }

                return null;
            }

            var authorization = authHdr.Parameter;
            if (string.IsNullOrEmpty(authorization))
            {
                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("Missing authorization header value in the request");
                }

			Logger.LogDebug("<<< Exiting ValidateAuthHeader(authHdr={AuthHdr})", authHdr);
                return null;
            }

            return authorization;
        }

        private static string EncodeBase64(byte[] data)
        {
			Logger.LogDebug(">>> Entering EncodeBase64(data={Data})", data);
			Logger.LogDebug("<<< Exiting EncodeBase64(data={Data})", data);
            return Convert.ToBase64String(data).TrimEnd('=');
        }

        private static string GetHashedStr(string data)
        {
			Logger.LogDebug(">>> Entering GetHashedStr(data={Data})", data);
            using var hasher = SHA384.Create();
			Logger.LogDebug("GetHashedStr(data={Data}): hasher → {Hasher}", data, hasher);
			Logger.LogDebug("<<< Exiting GetHashedStr(data={Data})", data);
            return EncodeBase64(hasher.ComputeHash(TextEncoder.GetBytes(data)));
        }

        private (ApiToken Token, UserInfo User) ValidateAuthorizationValue(string authorization)
        {
			Logger.LogDebug(">>> Entering ValidateAuthorizationValue(Token=<redacted>,User={User})", User);
            var tokenAndHostGuid = authorization + this.hostSettings.Guid;
            var hashedToken = GetHashedStr(tokenAndHostGuid);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			Logger.LogInformation("ValidateAuthorizationValue(Token,User)={Token},{User}: find query executed in {Arg2} ms", Token, User, __logStart.ElapsedMilliseconds);
            var apiToken = this.apiTokenRepository.GetApiToken(PortalSettings.PortalId, hashedToken);
            if (apiToken != null)
            {
                if (apiToken.ExpiresOn < DateUtils.GetDatabaseUtcTime() || apiToken.IsRevoked)
                {
                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace("Token expired");
                    }

                    this.eventLogger.AddLog("Token Auth", authorization, EventLogType.APITOKEN_AUTHENTICATION_FAILED);
                    return (null, null);
                }

                apiToken.TokenKeys = apiToken.Keys.Split(',').ToList();

                switch (apiToken.Scope)
                {
                    case ApiTokenScope.User:
                        var userInfo = UserController.GetUserById(this.hostSettings, PortalSettings.PortalId, apiToken.CreatedByUserId);
                        if (userInfo == null)
                        {
                            if (Logger.IsTraceEnabled)
                            {
                                Logger.Trace("Invalid user");
                            }

                            return (null, null);
                        }

                        this.apiTokenRepository.SetApiTokenLastUsed(apiToken);
                        return (apiToken, userInfo);

                    case ApiTokenScope.Portal:
                        if (apiToken.PortalId == PortalSettings.PortalId)
                        {
                            this.apiTokenRepository.SetApiTokenLastUsed(apiToken);
                            return (apiToken, null);
                        }

                        break;
                    case ApiTokenScope.Host:
                        if (apiToken.PortalId == -1)
                        {
                            this.apiTokenRepository.SetApiTokenLastUsed(apiToken);
                            return (apiToken, null);
                        }

                        break;
                }
            }

            this.eventLogger.AddLog("Token Auth", authorization, EventLogType.APITOKEN_AUTHENTICATION_FAILED);
			Logger.LogDebug("<<< Exiting ValidateAuthorizationValue(Token=<redacted>,User={User})", User);
            return (null, null);
        }
    }
}
