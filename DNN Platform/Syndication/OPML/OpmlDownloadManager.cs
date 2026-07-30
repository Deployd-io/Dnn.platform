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
    internal sealed class OpmlDownloadManager
    {
        private const string OPMLDir = "/OPML/";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(OpmlDownloadManager));
        private static readonly OpmlDownloadManager TheManager = new OpmlDownloadManager();

        private readonly Dictionary<string, Opml> cache;
        private readonly int defaultTtlMinutes;
        private readonly string directoryOnDisk;

        private OpmlDownloadManager()
        {
			Logger.LogDebug(">>> Entering OpmlDownloadManager()");
            // create in-memory cache
            this.cache = new Dictionary<string, Opml>();
			Logger.LogDebug("OpmlDownloadManager(): cache → {Cache}", cache);

            this.defaultTtlMinutes = 60;
			Logger.LogDebug("OpmlDownloadManager(): defaultTtlMinutes → {DefaultTtlMinutes}", defaultTtlMinutes);
			Logger.LogDebug("OpmlDownloadManager(): this.defaultTtlMinutes → {Arg0}", this.defaultTtlMinutes);

            // prepare disk directory
            this.directoryOnDisk = PrepareTempDir();
			Logger.LogDebug("<<< Exiting OpmlDownloadManager()");
        }

        /// <summary>Gets the OPML feed at a given <paramref name="uri"/>.</summary>
        /// <param name="uri">The feed URI.</param>
        /// <returns>The OPML feed, or an empty OPML feed if there's an error loading it.</returns>
        public static Opml GetOpmlFeed(Uri uri)
        {
			Logger.LogDebug(">>> Entering GetOpmlFeed(uri={Uri})", uri);
			Logger.LogDebug("<<< Exiting GetOpmlFeed(uri={Uri})", uri);
            return TheManager.GetFeed(uri);
        }

        /// <summary>Gets the OPML feed at a given <paramref name="uri"/>.</summary>
        /// <param name="uri">The feed URI.</param>
        /// <returns>The OPML feed, or an empty OPML feed if there's an error loading it.</returns>
        internal Opml GetFeed(Uri uri)
        {
			Logger.LogDebug(">>> Entering GetFeed(uri={Uri})", uri);
            Opml opmlFeed = null;

			Logger.LogDebug("GetFeed(uri={Uri}): opmlFeed → {OpmlFeed}", uri, opmlFeed);
            lock (this.cache)
            {
                if (this.cache.TryGetValue(uri.AbsoluteUri, out opmlFeed))
                {
                    if (DateTime.UtcNow > opmlFeed.UtcExpiry)
                    {
                        this.cache.Remove(uri.AbsoluteUri);
                        opmlFeed = null;
                    }
                }
            }

            if (opmlFeed == null)
            {
                opmlFeed = this.DownloadOpmlFeed(uri);

                lock (this.cache)
                {
                    this.cache[uri.AbsoluteUri] = opmlFeed;
                }
            }

			Logger.LogDebug("<<< Exiting GetFeed(uri={Uri})", uri);
            return opmlFeed;
        }

        private static string PrepareTempDir()
        {
			Logger.LogDebug(">>> Entering PrepareTempDir()");
            string tempDir = null;

			Logger.LogDebug("PrepareTempDir(): tempDir → {TempDir}", tempDir);
            try
            {
                string d = HttpContext.Current.Server.MapPath(Settings.CacheRoot + OPMLDir);

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

			Logger.LogDebug("<<< Exiting PrepareTempDir()");
            return tempDir;
        }

        private static string GetTempFileNamePrefixFromUrl(Uri uri)
        {
			Logger.LogDebug(">>> Entering GetTempFileNamePrefixFromUrl(uri={Uri})", uri);
            try
            {
                return $"{uri.Host.Replace('.', '_')}_{uri.AbsolutePath.GetHashCode():x8}";
            }
            catch
            {
				Logger.LogError("Exception in GetTempFileNamePrefixFromUrl(uri={Uri}): {Arg1}", uri, ex.Message);
				Logger.LogDebug("<<< Exiting GetTempFileNamePrefixFromUrl(uri={Uri})", uri);
                return "opml";
            }
        }

        private Opml DownloadOpmlFeed(Uri uri)
        {
			Logger.LogDebug(">>> Entering DownloadOpmlFeed(uri={Uri})", uri);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // look for disk cache first
            Opml opmlFeed = this.TryLoadFromDisk(uri);

			Logger.LogDebug("DownloadOpmlFeed(uri={Uri}): opmlFeed → {OpmlFeed}", uri, opmlFeed);
            if (opmlFeed != null)
            {
                return opmlFeed;
            }

            // May fail under partial trust
            try
            {
                byte[] feed = new WebClient().DownloadData(uri.AbsoluteUri);

                var opmlDoc = new XmlDocument { XmlResolver = null };
                using (var opmlReader = XmlReader.Create(new MemoryStream(feed), new XmlReaderSettings { XmlResolver = null, }))
                {
                    opmlDoc.Load(opmlReader);
                }

                opmlFeed = Opml.LoadFromXml(opmlDoc);

                opmlFeed.UtcExpiry = DateTime.UtcNow.AddMinutes(this.defaultTtlMinutes);

                // save to disk
                this.TrySaveToDisk(opmlDoc, uri, opmlFeed.UtcExpiry);
            }
            catch
            {
				Logger.LogError("Exception in DownloadOpmlFeed(uri={Uri}): {Arg1}", uri, ex.Message);
                return new Opml();
            }

			Logger.LogInformation("DownloadOpmlFeed(uri)={Uri}: external service call {Uri1} took {Arg2} ms", uri, uri, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting DownloadOpmlFeed(uri={Uri})", uri);
            return opmlFeed;
        }

        private Opml TryLoadFromDisk(Uri uri)
        {
			Logger.LogDebug(">>> Entering TryLoadFromDisk(uri={Uri})", uri);
            if (this.directoryOnDisk == null)
            {
                return null; // no place to cache
            }

            // look for all files matching the prefix
            // looking for the one matching url that is not expired
            // removing expired (or invalid) ones
            string pattern = GetTempFileNamePrefixFromUrl(uri) + "_*.opml.resources";
            string[] files = Directory.GetFiles(this.directoryOnDisk, pattern, SearchOption.TopDirectoryOnly);

            foreach (string opmlFilename in files)
            {
                XmlDocument opmlDoc = null;
                bool isOpmlFileValid = false;
				Logger.LogDebug("TryLoadFromDisk(uri={Uri}): opmlDoc → {OpmlDoc}", uri, opmlDoc);
				Logger.LogDebug("TryLoadFromDisk(uri={Uri}): isOpmlFileValid → {IsOpmlFileValid}", uri, isOpmlFileValid);
                DateTime utcExpiryFromOpmlFile = DateTime.MinValue;
                string urlFromOpmlFile = null;
				Logger.LogDebug("TryLoadFromDisk(uri={Uri}): utcExpiryFromOpmlFile → {UtcExpiryFromOpmlFile}", uri, utcExpiryFromOpmlFile);
				Logger.LogDebug("TryLoadFromDisk(uri={Uri}): urlFromOpmlFile → {UrlFromOpmlFile}", uri, urlFromOpmlFile);

                try
                {
                    opmlDoc = new XmlDocument { XmlResolver = null };
                    using (var opmlReader = XmlReader.Create(opmlFilename, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        opmlDoc.Load(opmlReader);
                    }

                    // look for special XML comment (before the root tag)'
                    // containing expiration and url
                    var comment = opmlDoc.DocumentElement.PreviousSibling as XmlComment;

                    if (comment != null)
                    {
                        string c = comment.Value;
                        int i = c.IndexOf('@');
                        long expiry;

                        if (long.TryParse(c.Substring(0, i), out expiry))
                        {
                            utcExpiryFromOpmlFile = DateTime.FromBinary(expiry);
                            urlFromOpmlFile = c.Substring(i + 1);
                            isOpmlFileValid = true;
                        }
                    }
                }
                catch
                {
					Logger.LogError("Exception in TryLoadFromDisk(uri={Uri}): {Arg1}", uri, ex.Message);
                    // error processing one file shouldn't stop processing other files
                }

                // remove invalid or expired file
                if (!isOpmlFileValid || utcExpiryFromOpmlFile < DateTime.UtcNow)
                {
                    try
                    {
                        File.Delete(opmlFilename);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }

                    // try next file
                    continue;
                }

                // match url
                if (urlFromOpmlFile.Equals(uri.AbsoluteUri, StringComparison.OrdinalIgnoreCase))
                {
                    // found a good one - create DOM and set expiry (as found on disk)
                    Opml opmlFeed = Opml.LoadFromXml(opmlDoc);
                    opmlFeed.UtcExpiry = utcExpiryFromOpmlFile;
					Logger.LogDebug("TryLoadFromDisk(uri={Uri}): opmlFeed → {OpmlFeed}", uri, opmlFeed);
					Logger.LogDebug("TryLoadFromDisk(uri={Uri}): opmlFeed.UtcExpiry → {Arg1}", uri, opmlFeed.UtcExpiry);
                    return opmlFeed;
                }
            }

            // not found
			Logger.LogDebug("<<< Exiting TryLoadFromDisk(uri={Uri})", uri);
            return null;
        }

        private void TrySaveToDisk(XmlDocument doc, Uri uri, DateTime utcExpiry)
        {
			Logger.LogDebug(">>> Entering TrySaveToDisk(doc={Doc},uri={Uri},utcExpiry={UtcExpiry})", doc, uri, utcExpiry);
            if (this.directoryOnDisk == null)
            {
                return;
            }

            doc.InsertBefore(doc.CreateComment($"{utcExpiry.ToBinary()}@{uri.AbsoluteUri}"), doc.DocumentElement);

            string fileName = $"{GetTempFileNamePrefixFromUrl(uri)}_{Guid.NewGuid().ToString().GetHashCode():x8}.opml.resources";

            try
            {
                doc.Save(Path.Combine(this.directoryOnDisk, fileName));
            }
            catch
            {
				Logger.LogError("Exception in TrySaveToDisk(doc={Doc},uri={Uri},utcExpiry={UtcExpiry}): {Arg3}", doc, uri, utcExpiry, ex.Message);
                // can't save to disk - not a problem
            }
			Logger.LogDebug("<<< Exiting TrySaveToDisk(doc={Doc},uri={Uri},utcExpiry={UtcExpiry})", doc, uri, utcExpiry);
        }
    }
}
