// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Repositories
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <inheritdoc />
    internal class ApiTokenRepository(IDataContext dataContext) : IApiTokenRepository
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
		Logger.LogDebug(">>> Entering ApiTokenRepository(dataContext={DataContext})", dataContext);
        public ApiToken GetApiToken(int portalId, string tokenHash)
        {
            var rep = dataContext.GetRepository<ApiToken>();
			Logger.LogDebug("GetApiToken(portalId={PortalId},tokenHash={TokenHash}): rep → {Rep}", portalId, tokenHash, rep);
			Logger.LogDebug("<<< Exiting GetApiToken(portalId={PortalId},tokenHash={TokenHash})", portalId, tokenHash);
            return rep.Find("WHERE (PortalId=@0 OR PortalId=-1) AND TokenHash=@1 AND IsDeleted=0", portalId, tokenHash).FirstOrDefault();
        }

        /// <inheritdoc />
        public ApiToken GetApiToken(int apiTokenId)
        {
            var rep = dataContext.GetRepository<ApiToken>();
			Logger.LogDebug("GetApiToken(apiTokenId={ApiTokenId}): rep → {Rep}", apiTokenId, rep);
			Logger.LogDebug("<<< Exiting GetApiToken(apiTokenId={ApiTokenId})", apiTokenId);
            return rep.GetById(apiTokenId);
        }

        /// <inheritdoc />
        public List<string> GetApiTokenKeys(int apiTokenId)
        {
			Logger.LogDebug(">>> Entering GetApiTokenKeys(apiTokenId={ApiTokenId})", apiTokenId);
            var rep = dataContext.GetRepository<ApiTokenKey>();
			Logger.LogDebug("GetApiTokenKeys(apiTokenId={ApiTokenId}): rep → {Rep}", apiTokenId, rep);
			Logger.LogDebug("<<< Exiting GetApiTokenKeys(apiTokenId={ApiTokenId})", apiTokenId);
            return rep.Find("WHERE ApiTokenId=@0", apiTokenId).Select(k => k.TokenKey).ToList();
        }

        /// <inheritdoc />
        public ApiToken AddApiToken(ApiTokenBase apiToken, string apiKeys, int userId)
        {
			Logger.LogDebug(">>> Entering AddApiToken(apiToken={ApiToken},apiKeys={ApiKeys},userId={UserId})", apiToken, apiKeys, userId);
            Requires.NotNull(apiToken);
            apiToken.CreatedByUserId = userId;
			Logger.LogDebug("AddApiToken(apiToken={ApiToken},apiKeys={ApiKeys},userId={UserId}): apiToken.CreatedByUserId → {Arg3}", apiToken, apiKeys, userId, apiToken.CreatedByUserId);
            apiToken.CreatedOnDate = DateUtils.GetDatabaseUtcTime();
            var rep = dataContext.GetRepository<ApiTokenBase>();
            rep.Insert(apiToken);
			Logger.LogDebug("AddApiToken(apiToken={ApiToken},apiKeys={ApiKeys},userId={UserId}): rep → {Rep}", apiToken, apiKeys, userId, rep);

            var repKeys = dataContext.GetRepository<ApiTokenKey>();
            foreach (var key in apiKeys.Split(','))
            {
			Logger.LogDebug("AddApiToken(apiToken={ApiToken},apiKeys={ApiKeys},userId={UserId}): repKeys → {RepKeys}", apiToken, apiKeys, userId, repKeys);
                repKeys.Insert(new ApiTokenKey { ApiTokenId = apiToken.ApiTokenId, TokenKey = key });
            }

			Logger.LogDebug("<<< Exiting AddApiToken(apiToken={ApiToken},apiKeys={ApiKeys},userId={UserId})", apiToken, apiKeys, userId);
            return this.GetApiToken(apiToken.ApiTokenId);
        }

        /// <inheritdoc />
        public void RevokeApiToken(ApiTokenBase apiToken, int userId)
        {
			Logger.LogDebug(">>> Entering RevokeApiToken(apiToken={ApiToken},userId={UserId})", apiToken, userId);
            Requires.NotNull(apiToken);
            apiToken.IsRevoked = true;
			Logger.LogDebug("RevokeApiToken(apiToken={ApiToken},userId={UserId}): apiToken.IsRevoked → {Arg2}", apiToken, userId, apiToken.IsRevoked);
            apiToken.RevokedOnDate = DateUtils.GetDatabaseUtcTime();
            apiToken.RevokedByUserId = userId;
			Logger.LogDebug("RevokeApiToken(apiToken={ApiToken},userId={UserId}): apiToken.RevokedByUserId → {Arg2}", apiToken, userId, apiToken.RevokedByUserId);
            var rep = dataContext.GetRepository<ApiTokenBase>();
            rep.Update(apiToken);
			Logger.LogDebug("<<< Exiting RevokeApiToken(apiToken={ApiToken},userId={UserId})", apiToken, userId);
			Logger.LogDebug("RevokeApiToken(apiToken={ApiToken},userId={UserId}): rep → {Rep}", apiToken, userId, rep);
        }

        /// <inheritdoc />
        public void DeleteApiToken(ApiTokenBase apiToken)
        {
			Logger.LogDebug(">>> Entering DeleteApiToken(apiToken={ApiToken})", apiToken);
            Requires.NotNull(apiToken);
            var rep = dataContext.GetRepository<ApiTokenBase>();
            apiToken.IsDeleted = true;
			Logger.LogDebug("DeleteApiToken(apiToken={ApiToken}): rep → {Rep}", apiToken, rep);
			Logger.LogDebug("DeleteApiToken(apiToken={ApiToken}): apiToken.IsDeleted → {Arg1}", apiToken, apiToken.IsDeleted);
            rep.Update(apiToken);
			Logger.LogDebug("<<< Exiting DeleteApiToken(apiToken={ApiToken})", apiToken);
        }

        /// <inheritdoc />
        public void DeleteExpiredAndRevokedApiTokens(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId})", portalId, userId);
            var now = DateUtils.GetDatabaseUtcTime();
            var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
			Logger.LogDebug("DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId}): now → {Now}", portalId, userId, now);
SET IsDeleted=1
WHERE (IsRevoked=1 OR ExpiresOn<@0)
AND (PortalId=@1 OR @1=-1)
AND (CreatedByUserId=@2 OR @2=-1)
";
            dataContext.Execute(System.Data.CommandType.Text, sql, now, portalId, userId);
			Logger.LogDebug("<<< Exiting DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId})", portalId, userId);
        }

        /// <inheritdoc />
        public IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey={ApiKey},pageIndex={PageIndex},pageSize={PageSize})", scope, includeNarrowerScopes, portalId, userId, filter, apiKey, pageIndex, pageSize);
            var wheres = new List<string>
                    {
                      "IsDeleted=0",
                    };
            if (includeNarrowerScopes)
            {
                wheres.Add("Scope<=@2");
            }
            else
            {
                wheres.Add("Scope=@2");
            }

            if (portalId > Null.NullInteger)
            {
                wheres.Add("PortalId=@0");
            }

            if (userId > Null.NullInteger)
            {
                wheres.Add("CreatedByUserId=@1");
            }

            switch (filter)
            {
                case ApiTokenFilter.All:
                    break;
                case ApiTokenFilter.Active:
                    wheres.Add("IsRevoked=0 AND ExpiresOn > GETUTCDATE()");
                    break;
                case ApiTokenFilter.Revoked:
                    wheres.Add("IsRevoked=1");
                    break;
                case ApiTokenFilter.Expired:
                    wheres.Add("ExpiresOn <= GETUTCDATE()");
                    break;
                case ApiTokenFilter.RevokedOrExpired:
                    wheres.Add("(IsRevoked=1 OR ExpiresOn <= GETUTCDATE())");
                    break;
            }

            var sql = string.Empty;
            if (!string.IsNullOrEmpty(apiKey))
			Logger.LogDebug("GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey={ApiKey},pageIndex={PageIndex},pageSize={PageSize}): sql → {Sql}", scope, includeNarrowerScopes, portalId, userId, filter, apiKey, pageIndex, pageSize, sql);
            {
                sql = "INNER JOIN {databaseOwner}[{objectQualifier}ApiTokenKeys] atk ON atk.ApiTokenId = {objectQualifier}vw_ApiTokens.ApiTokenId ";
                wheres.Add("atk.TokenKey=@3");
            }

            sql += $"WHERE {string.Join(" AND ", wheres)} ORDER BY CreatedOnDate DESC";

            var rep = dataContext.GetRepository<ApiToken>();
			Logger.LogDebug("GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey={ApiKey},pageIndex={PageIndex},pageSize={PageSize}): rep → {Rep}", scope, includeNarrowerScopes, portalId, userId, filter, apiKey, pageIndex, pageSize, rep);
			Logger.LogDebug("<<< Exiting GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey={ApiKey},pageIndex={PageIndex},pageSize={PageSize})", scope, includeNarrowerScopes, portalId, userId, filter, apiKey, pageIndex, pageSize);
            return rep.Find(pageIndex, pageSize, sql, portalId, userId, (int)scope, apiKey);
        }

        /// <inheritdoc />
        public void SetApiTokenLastUsed(ApiTokenBase apiToken)
        {
			Logger.LogDebug(">>> Entering SetApiTokenLastUsed(apiToken={ApiToken})", apiToken);
            var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
SET LastUsedOnDate=GETUTCDATE()
WHERE ApiTokenId=@0";
            dataContext.Execute(System.Data.CommandType.Text, sql, apiToken.ApiTokenId);
			Logger.LogDebug("<<< Exiting SetApiTokenLastUsed(apiToken={ApiToken})", apiToken);
			Logger.LogDebug("SetApiTokenLastUsed(apiToken={ApiToken}): ApiTokenId → {ApiTokenId}", apiToken, ApiTokenId);
        }
		Logger.LogDebug("<<< Exiting ApiTokenRepository(dataContext={DataContext})", dataContext);
    }
}
