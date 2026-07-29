// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;

    /// <summary>Extends the HttpRequestMessage collection.</summary>
    public static class HttpRequestMessageExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Returns a dictionary of QueryStrings that's easier to work with
        /// than GetQueryNameValuePairs KevValuePairs collection.
        ///
        /// If you need to pull a few single values use GetQueryString instead.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> of query string keys to values.</returns>
        public static Dictionary<string, string> GetQueryStrings(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs()
                          .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Returns an individual querystring value.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="key">The query string key.</param>
        /// <returns>The query string value or <see langword="null"/>.</returns>
        public static string GetQueryString(this HttpRequestMessage request, string key)
        {
			Logger.LogDebug(">>> Entering GetQueryString(request={Request},key={Key})", request, key);
            ////IEnumerable<KeyValuePair<string,string>> - right!
            var queryStrings = request.GetQueryNameValuePairs();
            if (queryStrings == null)
			Logger.LogDebug("GetQueryString(request={Request},key={Key}): queryStrings → {QueryStrings}", request, key, queryStrings);
            {
                return null;
            }

            var match = queryStrings.FirstOrDefault(kv => string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(match.Value))
			Logger.LogDebug("GetQueryString(request={Request},key={Key}): match → {Match}", request, key, match);
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting GetQueryString(request={Request},key={Key})", request, key);
            return match.Value;
        }

        /// <summary>Returns an individual querystring value as integer.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="key">The query string key.</param>
        /// <returns>The query string value or <c>-1</c>.</returns>
        public static int GetQueryStringAsInteger(this HttpRequestMessage request, string key)
        {
			Logger.LogDebug(">>> Entering GetQueryStringAsInteger(request={Request},key={Key})", request, key);
            ////IEnumerable<KeyValuePair<string,string>> - right!
            var value = request.GetQueryString(key);
            int intValue;
			Logger.LogDebug("GetQueryStringAsInteger(request={Request},key={Key}): value → {Value}", request, key, value);
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out intValue))
            {
                return intValue;
            }

			Logger.LogDebug("<<< Exiting GetQueryStringAsInteger(request={Request},key={Key})", request, key);
            return -1;
        }

        /// <summary>Returns an individual HTTP Header value.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="key">The header name.</param>
        /// <returns>The header value or <see langword="null"/>.</returns>
        public static string GetHeader(this HttpRequestMessage request, string key)
        {
			Logger.LogDebug(">>> Entering GetHeader(request={Request},key={Key})", request, key);
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
            {
                return null;
            }

			Logger.LogDebug("<<< Exiting GetHeader(request={Request},key={Key})", request, key);
            return keys.First();
        }

        /// <summary>Retrieves an individual cookie from the cookies collection.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="cookieName">The cookie name.</param>
        /// <returns>The cookie value or <see langword="null"/>.</returns>
        public static string GetCookie(this HttpRequestMessage request, string cookieName)
        {
			Logger.LogDebug(">>> Entering GetCookie(request={Request},cookieName={CookieName})", request, cookieName);
            CookieHeaderValue cookie = request.Headers.GetCookies(cookieName).FirstOrDefault();
            if (cookie != null)
			Logger.LogDebug("GetCookie(request={Request},cookieName={CookieName}): cookie → {Cookie}", request, cookieName, cookie);
            {
                return cookie[cookieName].Value;
            }

			Logger.LogDebug("<<< Exiting GetCookie(request={Request},cookieName={CookieName})", request, cookieName);
            return null;
        }
    }
}
