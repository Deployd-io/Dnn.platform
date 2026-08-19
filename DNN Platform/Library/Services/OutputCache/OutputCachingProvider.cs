// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.OutputCache
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.ComponentModel;

    public abstract class OutputCachingProvider
    {
        public static Dictionary<string, OutputCachingProvider> GetProviderList()
        {
            return ComponentFactory.GetComponents<OutputCachingProvider>();
        }

        public static OutputCachingProvider Instance(string friendlyName)
        {
			Logger.LogDebug(">>> Entering Instance(friendlyName={FriendlyName})", friendlyName);
			Logger.LogDebug("<<< Exiting Instance(friendlyName={FriendlyName})", friendlyName);
            return ComponentFactory.GetComponent<OutputCachingProvider>(friendlyName);
        }

        public static void RemoveItemFromAllProviders(int tabId)
        {
			Logger.LogDebug(">>> Entering RemoveItemFromAllProviders(tabId={TabId})", tabId);
            foreach (KeyValuePair<string, OutputCachingProvider> kvp in GetProviderList())
            {
                kvp.Value.Remove(tabId);
            }
			Logger.LogDebug("<<< Exiting RemoveItemFromAllProviders(tabId={TabId})", tabId);
        }

        public abstract int GetItemCount(int tabId);

        public abstract byte[] GetOutput(int tabId, string cacheKey);

        public abstract OutputCacheResponseFilter GetResponseFilter(int tabId, int maxVaryByCount, Stream responseFilter, string cacheKey, TimeSpan cacheDuration);

        public abstract void Remove(int tabId);

        public abstract void SetOutput(int tabId, string cacheKey, TimeSpan duration, byte[] output);

        public abstract bool StreamOutput(int tabId, string cacheKey, HttpContext context);

        public virtual string GenerateCacheKey(int tabId, StringCollection includeVaryByKeys, StringCollection excludeVaryByKeys, SortedDictionary<string, string> varyBy)
        {
			Logger.LogDebug(">>> Entering GenerateCacheKey(tabId={TabId},includeVaryByKeys={IncludeVaryByKeys},excludeVaryByKeys={ExcludeVaryByKeys},varyBy={VaryBy})", tabId, includeVaryByKeys, excludeVaryByKeys, varyBy);
            var cacheKey = new StringBuilder();
            if (varyBy != null)
			Logger.LogDebug("GenerateCacheKey(tabId={TabId},includeVaryByKeys={IncludeVaryByKeys},excludeVaryByKeys={ExcludeVaryByKeys},varyBy={VaryBy}): cacheKey → {CacheKey}", tabId, includeVaryByKeys, excludeVaryByKeys, varyBy, cacheKey);
            {
                SortedDictionary<string, string>.Enumerator varyByParms = varyBy.GetEnumerator();
				Logger.LogDebug("GenerateCacheKey(tabId={TabId},includeVaryByKeys={IncludeVaryByKeys},excludeVaryByKeys={ExcludeVaryByKeys},varyBy={VaryBy}): varyByParms → {VaryByParms}", tabId, includeVaryByKeys, excludeVaryByKeys, varyBy, varyByParms);
                while (varyByParms.MoveNext())
                {
                    string key = varyByParms.Current.Key.ToLowerInvariant();
                    if (includeVaryByKeys.Contains(key) && !excludeVaryByKeys.Contains(key))
                    {
                        cacheKey.Append(string.Concat(key, "=", varyByParms.Current.Value, "|"));
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GenerateCacheKey(tabId={TabId},includeVaryByKeys={IncludeVaryByKeys},excludeVaryByKeys={ExcludeVaryByKeys},varyBy={VaryBy})", tabId, includeVaryByKeys, excludeVaryByKeys, varyBy);
            return this.GenerateCacheKeyHash(tabId, cacheKey.ToString());
        }

        public virtual void PurgeCache(int portalId)
        {
		Logger.LogDebug(">>> Entering PurgeCache(portalId={PortalId})", portalId);
		Logger.LogDebug("<<< Exiting PurgeCache(portalId={PortalId})", portalId);
        }

        public virtual void PurgeExpiredItems(int portalId)
        {
		Logger.LogDebug(">>> Entering PurgeExpiredItems(portalId={PortalId})", portalId);
		Logger.LogDebug("<<< Exiting PurgeExpiredItems(portalId={PortalId})", portalId);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected string ByteArrayToString(byte[] arrInput)
        {
			Logger.LogDebug(">>> Entering ByteArrayToString(arrInput={ArrInput})", arrInput);
            int i = 0;
            var sOutput = new StringBuilder(arrInput.Length);
			Logger.LogDebug("ByteArrayToString(arrInput={ArrInput}): sOutput → {SOutput}", arrInput, sOutput);
            for (i = 0; i <= arrInput.Length - 1; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2", CultureInfo.InvariantCulture));
            }

			Logger.LogDebug("<<< Exiting ByteArrayToString(arrInput={ArrInput})", arrInput);
            return sOutput.ToString();
        }

        protected string GenerateCacheKeyHash(int tabId, string cacheKey)
        {
			Logger.LogDebug(">>> Entering GenerateCacheKeyHash(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            byte[] hash = Encoding.ASCII.GetBytes(cacheKey);
            using var sha256 = new SHA256CryptoServiceProvider();
			Logger.LogDebug("GenerateCacheKeyHash(tabId={TabId},cacheKey={CacheKey}): sha256 → {Sha256}", tabId, cacheKey, sha256);
            hash = sha256.ComputeHash(hash);
			Logger.LogDebug("GenerateCacheKeyHash(tabId={TabId},cacheKey={CacheKey}): hash → {Hash}", tabId, cacheKey, hash);
			Logger.LogDebug("<<< Exiting GenerateCacheKeyHash(tabId={TabId},cacheKey={CacheKey})", tabId, cacheKey);
            return string.Concat(tabId.ToString(CultureInfo.InvariantCulture), "_", this.ByteArrayToString(hash));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected void WriteStreamAsText(HttpContext context, Stream stream, long offset, long length)
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OutputCachingProvider));
            if (length < 0)
			Logger.LogDebug(">>> Entering WriteStreamAsText(context={Context},stream={Stream},offset={Offset},length={Length})", context, stream, offset, length);
            {
                length = stream.Length - offset;
            }

            if (length > 0)
            {
                if (offset > 0)
                {
                    stream.Seek(offset, SeekOrigin.Begin);
                }

                var buffer = new byte[Convert.ToInt32(length)];
				Logger.LogDebug("WriteStreamAsText(context={Context},stream={Stream},offset={Offset},length={Length}): buffer → {Buffer}", context, stream, offset, length, buffer);
                int count = stream.Read(buffer, 0, Convert.ToInt32(length));
				Logger.LogDebug("WriteStreamAsText(context={Context},stream={Stream},offset={Offset},length={Length}): count → {Count}", context, stream, offset, length, count);
                char[] output = Encoding.UTF8.GetChars(buffer, 0, count);
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.Output.Write(output);
            }
			Logger.LogDebug("<<< Exiting WriteStreamAsText(context={Context},stream={Stream},offset={Offset},length={Length})", context, stream, offset, length);
        }
    }
}
