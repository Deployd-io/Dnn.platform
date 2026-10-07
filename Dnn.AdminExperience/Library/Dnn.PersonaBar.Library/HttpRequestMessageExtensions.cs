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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HttpRequestMessageExtensions));
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
			Logger.LogError(">>> Entering GetQueryString(request={Request},key={Key})", request, key);
            ////IEnumerable<KeyValuePair<string,string>> - right!
            var queryStrings = request.GetQueryNameValuePairs();
			Logger.LogError("GetQueryString(request={Request},key={Key}): queryStrings → {QueryStrings}", request, key, queryStrings);
            if (queryStrings == null)
            {
                return null;
            }

            var match = queryStrings.FirstOrDefault(kv => string.Equals(kv.Key, key, StringComparison.OrdinalIgnoreCase));
			Logger.LogError("GetQueryString(request={Request},key={Key}): match → {Match}", request, key, match);
            if (string.IsNullOrEmpty(match.Value))
            {
                return null;
            }

			Logger.LogWarning("<<< Exiting GetQueryString(request={Request},key={Key})", request, key);
            return match.Value;
        }

        /// <summary>Returns an individual querystring value as integer.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="key">The query string key.</param>
        /// <returns>The query string value or <c>-1</c>.</returns>
        public static int GetQueryStringAsInteger(this HttpRequestMessage request, string key)
        {
			Logger.LogError(">>> Entering GetQueryStringAsInteger(request={Request},key={Key})", request, key);
            ////IEnumerable<KeyValuePair<string,string>> - right!
            var value = request.GetQueryString(key);
			Logger.LogError("GetQueryStringAsInteger(request={Request},key={Key}): value → {Value}", request, key, value);
            int intValue;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out intValue))
            {
                return intValue;
            }

			Logger.LogWarning("<<< Exiting GetQueryStringAsInteger(request={Request},key={Key})", request, key);
            return -1;
        }

        /// <summary>Returns an individual HTTP Header value.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="key">The header name.</param>
        /// <returns>The header value or <see langword="null"/>.</returns>
        public static string GetHeader(this HttpRequestMessage request, string key)
        {
			Logger.LogError(">>> Entering GetHeader(request={Request},key={Key})", request, key);
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
            {
                return null;
            }

			Logger.LogWarning("<<< Exiting GetHeader(request={Request},key={Key})", request, key);
            return keys.First();
        }

        /// <summary>Retrieves an individual cookie from the cookies collection.</summary>
        /// <param name="request">The HTTP request.</param>
        /// <param name="cookieName">The cookie name.</param>
        /// <returns>The cookie value or <see langword="null"/>.</returns>
        public static string GetCookie(this HttpRequestMessage request, string cookieName)
        {
			Logger.LogError(">>> Entering GetCookie(request={Request},cookieName=<redacted>)", request);
            CookieHeaderValue cookie = request.Headers.GetCookies(cookieName).FirstOrDefault();
			Logger.LogError("GetCookie(request={Request},cookieName=<redacted>): cookie → {Cookie}", request, cookie);
            if (cookie != null)
            {
                return cookie[cookieName].Value;
            }

			Logger.LogWarning("<<< Exiting GetCookie(request={Request},cookieName=<redacted>)", request);
            return null;
        }
    }
}
