// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Cookies
{
    using System;
    using System.Web.Caching;
    using System.Web.Security;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class AuthCookieController : ServiceLocator<IAuthCookieController, AuthCookieController>, IAuthCookieController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AuthCookieController));
        private readonly DataProvider dataProvider = DataProvider.Instance();

        /// <inheritdoc />
        public void Update(string cookieValue, DateTime utcExpiry, int userId)
        {
			Logger.LogError(">>> Entering Update(cookieValue=<redacted>,utcExpiry={UtcExpiry},userId={UserId})", utcExpiry, userId);
            if (string.IsNullOrEmpty(cookieValue))
            {
                return;
            }

            DataCache.ClearCache(GetKey(cookieValue));
            this.dataProvider.UpdateAuthCookie(cookieValue, utcExpiry, userId);
			Logger.LogWarning("<<< Exiting Update(cookieValue=<redacted>,utcExpiry={UtcExpiry},userId={UserId})", utcExpiry, userId);
        }

        /// <inheritdoc />
        public PersistedAuthCookie Find(string cookieValue)
        {
			Logger.LogError(">>> Entering Find(cookieValue=<redacted>)");
            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

			Logger.LogWarning("<<< Exiting Find(cookieValue=<redacted>)");
            return CBO.Instance.GetCachedObject<PersistedAuthCookie>(
                new CacheItemArgs(
                    GetKey(cookieValue),
                    (int)FormsAuthentication.Timeout.TotalMinutes,
                    CacheItemPriority.AboveNormal),
                _ => CBO.Instance.FillObject<PersistedAuthCookie>(this.dataProvider.FindAuthCookie(cookieValue)),
                false);
        }

        /// <inheritdoc />
        public void DeleteByValue(string cookieValue)
        {
			Logger.LogError(">>> Entering DeleteByValue(cookieValue=<redacted>)");
            if (string.IsNullOrEmpty(cookieValue))
            {
                return;
            }

            // keep in cache so hacking tries don't hit the database; it will expire automatically
            ////DataCache.ClearCache(GetKey(cookieValue));
            this.dataProvider.DeleteAuthCookie(cookieValue);
			Logger.LogWarning("<<< Exiting DeleteByValue(cookieValue=<redacted>)");
        }

        /// <inheritdoc />
        public void DeleteExpired(DateTime utcExpiredBefore)
        {
			Logger.LogError(">>> Entering DeleteExpired(utcExpiredBefore={UtcExpiredBefore})", utcExpiredBefore);
            this.dataProvider.DeleteExpiredAuthCookies(utcExpiredBefore);
			Logger.LogWarning("<<< Exiting DeleteExpired(utcExpiredBefore={UtcExpiredBefore})", utcExpiredBefore);
        }

        /// <inheritdoc />
        protected override Func<IAuthCookieController> GetFactory()
        {
			Logger.LogError(">>> Entering GetFactory()");
			Logger.LogWarning("<<< Exiting GetFactory()");
            return () => new AuthCookieController();
        }

        private static string GetKey(string cookieValue)
        {
			Logger.LogError(">>> Entering GetKey(cookieValue=<redacted>)");
            Logger.LogWarning("<<< Exiting GetKey(cookieValue=<redacted>)");
            return $"AuthCookie:{cookieValue}";
        }
    }
}
