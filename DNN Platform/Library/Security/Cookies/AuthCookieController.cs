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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly DataProvider dataProvider = DataProvider.Instance();

        /// <inheritdoc />
        public void Update(string cookieValue, DateTime utcExpiry, int userId)
        {
			Logger.LogDebug(">>> Entering Update(cookieValue={CookieValue},utcExpiry={UtcExpiry},userId={UserId})", cookieValue, utcExpiry, userId);
            if (string.IsNullOrEmpty(cookieValue))
            {
                return;
            }

            DataCache.ClearCache(GetKey(cookieValue));
            this.dataProvider.UpdateAuthCookie(cookieValue, utcExpiry, userId);
			Logger.LogDebug("<<< Exiting Update(cookieValue={CookieValue},utcExpiry={UtcExpiry},userId={UserId})", cookieValue, utcExpiry, userId);
        }

        /// <inheritdoc />
        public PersistedAuthCookie Find(string cookieValue)
        {
			Logger.LogDebug(">>> Entering Find(cookieValue={CookieValue})", cookieValue);
            if (string.IsNullOrEmpty(cookieValue))
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting Find(cookieValue={CookieValue})", cookieValue);
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
			Logger.LogDebug(">>> Entering DeleteByValue(cookieValue={CookieValue})", cookieValue);
            if (string.IsNullOrEmpty(cookieValue))
            {
                return;
            }

            // keep in cache so hacking tries don't hit the database; it will expire automatically
            ////DataCache.ClearCache(GetKey(cookieValue));
            this.dataProvider.DeleteAuthCookie(cookieValue);
			Logger.LogDebug("<<< Exiting DeleteByValue(cookieValue={CookieValue})", cookieValue);
        }

        /// <inheritdoc />
        public void DeleteExpired(DateTime utcExpiredBefore)
        {
			Logger.LogDebug(">>> Entering DeleteExpired(utcExpiredBefore={UtcExpiredBefore})", utcExpiredBefore);
            this.dataProvider.DeleteExpiredAuthCookies(utcExpiredBefore);
			Logger.LogDebug("<<< Exiting DeleteExpired(utcExpiredBefore={UtcExpiredBefore})", utcExpiredBefore);
        }

        /// <inheritdoc />
        protected override Func<IAuthCookieController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new AuthCookieController();
        }

        private static string GetKey(string cookieValue)
        {
			Logger.LogDebug(">>> Entering GetKey(cookieValue={CookieValue})", cookieValue);
			Logger.LogDebug("<<< Exiting GetKey(cookieValue={CookieValue})", cookieValue);
            return $"AuthCookie:{cookieValue}";
        }
    }
}
