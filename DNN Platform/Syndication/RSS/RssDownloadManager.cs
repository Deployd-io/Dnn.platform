// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Instrumentation;

    /// <summary>Helper class that provides memory and disk caching of the downloaded feeds.</summary>
    internal sealed class RssDownloadManager
    {
        private const string RSSDir = "/RSS/";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RssDownloadManager));
        private static readonly RssDownloadManager TheManager = new RssDownloadManager();

        private readonly Dictionary<string, RssChannelDom> cache;
        private readonly int defaultTtlMinutes;
        private readonly string directoryOnDisk;

        private RssDownloadManager()
        {
			Logger.LogError(">>> Entering RssDownloadManager()");
            // create in-memory cache
            this.cache = new Dictionary<string, RssChannelDom>();
			Logger.LogDebug("RssDownloadManager(): cache → {Cache}", cache);

            this.defaultTtlMinutes = 2;
			Logger.LogDebug("RssDownloadManager(): defaultTtlMinutes → {DefaultTtlMinutes}", defaultTtlMinutes);
			Logger.LogDebug("RssDownloadManager(): this.defaultTtlMinutes → {Arg0}", this.defaultTtlMinutes);

            // prepare disk directory
			Logger.LogWarning("<<< Exiting RssDownloadManager()");
            this.directoryOnDisk = PrepareTempDir();
        }

        /// <summary>Gets the RSS channel at the given <paramref name="url"/>.</summary>
        /// <param name="url">The URL.</param>
        /// <returns>The RSS feed, or an empty RSS feed if there's an error loading it.</returns>
        public static RssChannelDom GetChannel(string url)
        {
			Logger.LogError(">>> Entering GetChannel(url={Url})", url);
            Logger.LogWarning("<<< Exiting GetChannel(url={Url})", url);
            return TheManager.GetChannelDom(url);
        }

        private static int GetTtlFromString(string ttlString, int defaultTtlMinutes)
        {
			Logger.LogError(">>> Entering GetTtlFromString(ttlString={TtlString},defaultTtlMinutes={DefaultTtlMinutes})", ttlString, defaultTtlMinutes);
            if (!string.IsNullOrEmpty(ttlString))
            {
                int ttlMinutes;
                if (int.TryParse(ttlString, out ttlMinutes))
                {
                    if (ttlMinutes >= 0)
                    {
                        return ttlMinutes;
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetTtlFromString(ttlString={TtlString},defaultTtlMinutes={DefaultTtlMinutes})", ttlString, defaultTtlMinutes);
            return defaultTtlMinutes;
        }

        private static string PrepareTempDir()
        {
			Logger.LogError(">>> Entering PrepareTempDir()");
            string tempDir = null;

			Logger.LogError("PrepareTempDir(): tempDir → {TempDir}", tempDir);
            try
            {
                string d = HttpContext.Current.Server.MapPath(Settings.CacheRoot + RSSDir);

                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }

                tempDir = d;
            }
            catch
            {
				Logger.LogError("Exception in PrepareTempDir(): {Arg0}", ex.Message);
                // don't cache on disk if can't do it
            }

			Logger.LogWarning("<<< Exiting PrepareTempDir()");
            return tempDir;
        }

        private static string GetTempFileNamePrefixFromUrl(string url)
        {
			Logger.LogError(">>> Entering GetTempFileNamePrefixFromUrl(url={Url})", url);
            try
            {
                var uri = new Uri(url);
				Logger.LogError("GetTempFileNamePrefixFromUrl(url={Url}): uri → {Uri}", url, uri);
                return $"{uri.Host.Replace('.', '_')}_{uri.AbsolutePath.GetHashCode():x8}";
            }
            catch
            {
				Logger.LogError("Exception in GetTempFileNamePrefixFromUrl(url={Url}): {Arg1}", url, ex.Message);
				Logger.LogWarning("<<< Exiting GetTempFileNamePrefixFromUrl(url={Url})", url);
                return "rss";
            }
        }

        private RssChannelDom DownloadChannelDom(string url)
        {
			Logger.LogError(">>> Entering DownloadChannelDom(url={Url})", url);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // look for disk cache first
            RssChannelDom dom = this.TryLoadFromDisk(url);
			Logger.LogError("DownloadChannelDom(url={Url}): dom → {Dom}", url, dom);

            if (dom != null)
            {
                return dom;
            }

            // download the feed
            byte[] feed = new WebClient().DownloadData(url);

            // parse it as XML
            var doc = new XmlDocument { XmlResolver = null };
            using (var feedReader = XmlReader.Create(new MemoryStream(feed), new XmlReaderSettings { XmlResolver = null, }))
            {
                doc.Load(feedReader);
            }

            // parse into DOM
            dom = RssXmlHelper.ParseChannelXml(doc);

            // set expiry
            string ttlString = null;
            dom.Channel.TryGetValue("ttl", out ttlString);
            int ttlMinutes = GetTtlFromString(ttlString, this.defaultTtlMinutes);
			Logger.LogError("DownloadChannelDom(url={Url}): ttlMinutes → {TtlMinutes}", url, ttlMinutes);
            DateTime utcExpiry = DateTime.UtcNow.AddMinutes(ttlMinutes);
			Logger.LogError("DownloadChannelDom(url={Url}): utcExpiry → {UtcExpiry}", url, utcExpiry);
            dom.SetExpiry(utcExpiry);

            // save to disk
            this.TrySaveToDisk(doc, url, utcExpiry);

			Logger.LogError("DownloadChannelDom(url)={Url}: external service call {Url1} took {Arg2} ms", url, url, __logStart.ElapsedMilliseconds);
			Logger.LogWarning("<<< Exiting DownloadChannelDom(url={Url})", url);
            return dom;
        }

        private RssChannelDom TryLoadFromDisk(string url)
        {
			Logger.LogError(">>> Entering TryLoadFromDisk(url={Url})", url);
            if (this.directoryOnDisk == null)
            {
                return null; // no place to cache
            }

            // look for all files matching the prefix
            // looking for the one matching url that is not expired
            // removing expired (or invalid) ones
            string pattern = GetTempFileNamePrefixFromUrl(url) + "_*.rss.resources";
            string[] files = Directory.GetFiles(this.directoryOnDisk, pattern, SearchOption.TopDirectoryOnly);

            foreach (string rssFilename in files)
            {
                XmlDocument rssDoc = null;
				Logger.LogError("TryLoadFromDisk(url={Url}): rssDoc → {RssDoc}", url, rssDoc);
                bool isRssFileValid = false;
				Logger.LogError("TryLoadFromDisk(url={Url}): isRssFileValid → {IsRssFileValid}", url, isRssFileValid);
                DateTime utcExpiryFromRssFile = DateTime.MinValue;
				Logger.LogError("TryLoadFromDisk(url={Url}): utcExpiryFromRssFile → {UtcExpiryFromRssFile}", url, utcExpiryFromRssFile);
                string urlFromRssFile = null;
				Logger.LogError("TryLoadFromDisk(url={Url}): urlFromRssFile → {UrlFromRssFile}", url, urlFromRssFile);

                try
                {
                    rssDoc = new XmlDocument { XmlResolver = null };
                    using (var rssReader = XmlReader.Create(rssFilename, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        rssDoc.Load(rssReader);
                    }

                    // look for special XML comment (before the root tag)'
                    // containing expiration and url
                    var comment = rssDoc.DocumentElement.PreviousSibling as XmlComment;

                    if (comment != null)
                    {
                        string c = comment.Value;
                        int i = c.IndexOf('@');
                        long expiry;

                        if (long.TryParse(c.Substring(0, i), out expiry))
                        {
                            utcExpiryFromRssFile = DateTime.FromBinary(expiry);
                            urlFromRssFile = c.Substring(i + 1);
                            isRssFileValid = true;
                        }
                    }
                }
                catch
                {
					Logger.LogError("Exception in TryLoadFromDisk(url={Url}): {Arg1}", url, ex.Message);
                    // error processing one file shouldn't stop processing other files
                }

                // remove invalid or expired file
                if (!isRssFileValid || utcExpiryFromRssFile < DateTime.UtcNow)
                {
                    try
                    {
                        File.Delete(rssFilename);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    // try next file
                    continue;
                }

                // match url
                if (urlFromRssFile == url)
                {
                    // found a good one - create DOM and set expiry (as found on disk)
                    RssChannelDom dom = RssXmlHelper.ParseChannelXml(rssDoc);
					Logger.LogError("TryLoadFromDisk(url={Url}): dom → {Dom}", url, dom);
                    dom.SetExpiry(utcExpiryFromRssFile);
                    return dom;
                }
            }

            // not found
			Logger.LogWarning("<<< Exiting TryLoadFromDisk(url={Url})", url);
            return null;
        }

        private void TrySaveToDisk(XmlDocument doc, string url, DateTime utcExpiry)
        {
			Logger.LogError(">>> Entering TrySaveToDisk(doc={Doc},url={Url},utcExpiry={UtcExpiry})", doc, url, utcExpiry);
            if (this.directoryOnDisk == null)
            {
                return;
            }

            doc.InsertBefore(doc.CreateComment($"{utcExpiry.ToBinary()}@{url}"), doc.DocumentElement);

            string fileName = $"{GetTempFileNamePrefixFromUrl(url)}_{Guid.NewGuid().ToString().GetHashCode():x8}.rss.resources";

            try
            {
                doc.Save(Path.Combine(this.directoryOnDisk, fileName));
            }
            catch
            {
				Logger.LogError("Exception in TrySaveToDisk(doc={Doc},url={Url},utcExpiry={UtcExpiry}): {Arg3}", doc, url, utcExpiry, ex.Message);
                // can't save to disk - not a problem
            }
			Logger.LogWarning("<<< Exiting TrySaveToDisk(doc={Doc},url={Url},utcExpiry={UtcExpiry})", doc, url, utcExpiry);
        }

        private RssChannelDom GetChannelDom(string url)
        {
			Logger.LogError(">>> Entering GetChannelDom(url={Url})", url);
            RssChannelDom dom = null;

			Logger.LogError("GetChannelDom(url={Url}): dom → {Dom}", url, dom);
            lock (this.cache)
            {
                if (this.cache.TryGetValue(url, out dom))
                {
                    if (DateTime.UtcNow > dom.UtcExpiry)
                    {
                        this.cache.Remove(url);
                        dom = null;
                    }
                }
            }

            if (dom == null)
            {
                dom = this.DownloadChannelDom(url);

                lock (this.cache)
                {
                    this.cache[url] = dom;
                }
            }

			Logger.LogWarning("<<< Exiting GetChannelDom(url={Url})", url);
            return dom;
        }
    }
}
