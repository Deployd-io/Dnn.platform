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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ApiTokenRepository));
        /// <inheritdoc />
        public ApiToken GetApiToken(int portalId, string tokenHash)
        {
			Logger.LogDebug(">>> Entering GetApiToken(portalId={PortalId},tokenHash=<redacted>)", portalId);
            var rep = dataContext.GetRepository<ApiToken>();
			Logger.LogDebug("GetApiToken(portalId={PortalId},tokenHash=<redacted>): rep → {Rep}", portalId, rep);
			Logger.LogDebug("<<< Exiting GetApiToken(portalId={PortalId},tokenHash=<redacted>)", portalId);
			Logger.LogDebug("<<< Exiting GetApiToken(apiTokenId=<redacted>)");
            return rep.Find("WHERE (PortalId=@0 OR PortalId=-1) AND TokenHash=@1 AND IsDeleted=0", portalId, tokenHash).FirstOrDefault();
        }

        /// <inheritdoc />
        public ApiToken GetApiToken(int apiTokenId)
        {
            var rep = dataContext.GetRepository<ApiToken>();
			Logger.LogDebug("GetApiToken(apiTokenId=<redacted>): rep → {Rep}", rep);
            return rep.GetById(apiTokenId);
        }

        /// <inheritdoc />
        public List<string> GetApiTokenKeys(int apiTokenId)
        {
			Logger.LogDebug(">>> Entering GetApiTokenKeys(apiTokenId=<redacted>)");
            var rep = dataContext.GetRepository<ApiTokenKey>();
			Logger.LogDebug("GetApiTokenKeys(apiTokenId=<redacted>): rep → {Rep}", rep);
			Logger.LogDebug("<<< Exiting GetApiTokenKeys(apiTokenId=<redacted>)");
            return rep.Find("WHERE ApiTokenId=@0", apiTokenId).Select(k => k.TokenKey).ToList();
        }

        /// <inheritdoc />
        public ApiToken AddApiToken(ApiTokenBase apiToken, string apiKeys, int userId)
        {
			Logger.LogDebug(">>> Entering AddApiToken(apiToken=<redacted>,apiKeys=<redacted>,userId={UserId})", userId);
            Requires.NotNull(apiToken);
            apiToken.CreatedByUserId = userId;
			Logger.LogDebug("AddApiToken(apiToken=<redacted>,apiKeys=<redacted>,userId={UserId}): apiToken.CreatedByUserId → {Arg3}", userId, apiToken.CreatedByUserId);
            apiToken.CreatedOnDate = DateUtils.GetDatabaseUtcTime();
            var rep = dataContext.GetRepository<ApiTokenBase>();
			Logger.LogDebug("AddApiToken(apiToken=<redacted>,apiKeys=<redacted>,userId={UserId}): rep → {Rep}", userId, rep);
            rep.Insert(apiToken);

            var repKeys = dataContext.GetRepository<ApiTokenKey>();
			Logger.LogDebug("AddApiToken(apiToken=<redacted>,apiKeys=<redacted>,userId={UserId}): repKeys → {RepKeys}", userId, repKeys);
            foreach (var key in apiKeys.Split(','))
            {
                repKeys.Insert(new ApiTokenKey { ApiTokenId = apiToken.ApiTokenId, TokenKey = key });
            }

			Logger.LogDebug("<<< Exiting AddApiToken(apiToken=<redacted>,apiKeys=<redacted>,userId={UserId})", userId);
            return this.GetApiToken(apiToken.ApiTokenId);
        }

        /// <inheritdoc />
        public void RevokeApiToken(ApiTokenBase apiToken, int userId)
        {
			Logger.LogDebug(">>> Entering RevokeApiToken(apiToken=<redacted>,userId={UserId})", userId);
            Requires.NotNull(apiToken);
            apiToken.IsRevoked = true;
			Logger.LogDebug("RevokeApiToken(apiToken=<redacted>,userId={UserId}): apiToken.IsRevoked → {Arg2}", userId, apiToken.IsRevoked);
            apiToken.RevokedOnDate = DateUtils.GetDatabaseUtcTime();
            apiToken.RevokedByUserId = userId;
			Logger.LogDebug("RevokeApiToken(apiToken=<redacted>,userId={UserId}): apiToken.RevokedByUserId → {Arg2}", userId, apiToken.RevokedByUserId);
            var rep = dataContext.GetRepository<ApiTokenBase>();
			Logger.LogDebug("RevokeApiToken(apiToken=<redacted>,userId={UserId}): rep → {Rep}", userId, rep);
            rep.Update(apiToken);
			Logger.LogDebug("<<< Exiting RevokeApiToken(apiToken=<redacted>,userId={UserId})", userId);
        }

        /// <inheritdoc />
        public void DeleteApiToken(ApiTokenBase apiToken)
        {
			Logger.LogDebug(">>> Entering DeleteApiToken(apiToken=<redacted>)");
            Requires.NotNull(apiToken);
            var rep = dataContext.GetRepository<ApiTokenBase>();
			Logger.LogDebug("DeleteApiToken(apiToken=<redacted>): rep → {Rep}", rep);
			Logger.LogDebug("DeleteApiToken(apiToken=<redacted>): apiToken.IsDeleted → {Arg1}", apiToken.IsDeleted);
            apiToken.IsDeleted = true;
            rep.Update(apiToken);
			Logger.LogDebug("<<< Exiting DeleteApiToken(apiToken=<redacted>)");
        }

        /// <inheritdoc />
        public void DeleteExpiredAndRevokedApiTokens(int portalId, int userId)
        {
			Logger.LogDebug(">>> Entering DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId})", portalId, userId);
            var now = DateUtils.GetDatabaseUtcTime();
            var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
			Logger.LogDebug("<<< Exiting DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId})", portalId, userId);
			Logger.LogDebug("DeleteExpiredAndRevokedApiTokens(portalId={PortalId},userId={UserId}): now → {Now}", portalId, userId, now);
SET IsDeleted=1
WHERE (IsRevoked=1 OR ExpiresOn<@0)
AND (PortalId=@1 OR @1=-1)
AND (CreatedByUserId=@2 OR @2=-1)
";
            dataContext.Execute(System.Data.CommandType.Text, sql, now, portalId, userId);
        }

        /// <inheritdoc />
        public IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize)
        {
			Logger.LogDebug(">>> Entering GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey=<redacted>,pageIndex={PageIndex},pageSize={PageSize})", scope, includeNarrowerScopes, portalId, userId, filter, pageIndex, pageSize);
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
			Logger.LogDebug("GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey=<redacted>,pageIndex={PageIndex},pageSize={PageSize}): sql → {Sql}", scope, includeNarrowerScopes, portalId, userId, filter, pageIndex, pageSize, sql);
            if (!string.IsNullOrEmpty(apiKey))
            {
                sql = "INNER JOIN {databaseOwner}[{objectQualifier}ApiTokenKeys] atk ON atk.ApiTokenId = {objectQualifier}vw_ApiTokens.ApiTokenId ";
                wheres.Add("atk.TokenKey=@3");
            }

            sql += $"WHERE {string.Join(" AND ", wheres)} ORDER BY CreatedOnDate DESC";

            var rep = dataContext.GetRepository<ApiToken>();
			Logger.LogDebug("GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey=<redacted>,pageIndex={PageIndex},pageSize={PageSize}): rep → {Rep}", scope, includeNarrowerScopes, portalId, userId, filter, pageIndex, pageSize, rep);
			Logger.LogDebug("<<< Exiting GetApiTokens(scope={Scope},includeNarrowerScopes={IncludeNarrowerScopes},portalId={PortalId},userId={UserId},filter={Filter},apiKey=<redacted>,pageIndex={PageIndex},pageSize={PageSize})", scope, includeNarrowerScopes, portalId, userId, filter, pageIndex, pageSize);
            return rep.Find(pageIndex, pageSize, sql, portalId, userId, (int)scope, apiKey);
        }

        /// <inheritdoc />
        public void SetApiTokenLastUsed(ApiTokenBase apiToken)
        {
			Logger.LogDebug(">>> Entering SetApiTokenLastUsed(apiToken=<redacted>)");
			Logger.LogDebug("<<< Exiting SetApiTokenLastUsed(apiToken=<redacted>)");
            var sql = @"UPDATE {databaseOwner}{objectQualifier}ApiTokens
SET LastUsedOnDate=GETUTCDATE()
WHERE ApiTokenId=@0";
			Logger.LogDebug("SetApiTokenLastUsed(apiToken=<redacted>): ApiTokenId → {ApiTokenId}", ApiTokenId);
            dataContext.Execute(System.Data.CommandType.Text, sql, apiToken.ApiTokenId);
        }
    }
}
