// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class SitemapBuilder
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int SitemapMaxUrls = 50000;

        private const string SitemapVersion = "0.9";
        private static readonly object Lock = new object();

        private static List<SitemapProvider> providers;

        private readonly PortalSettings portalSettings;
        private string cacheFileName;
        private string cacheIndexFileNameFormat;

        /// <summary>Initializes a new instance of the <see cref="SitemapBuilder"/> class.</summary>
        /// <param name="ps">Current PortalSettings for the portal being processed.</param>
        public SitemapBuilder(PortalSettings ps)
        {
			Logger.LogDebug(">>> Entering SitemapBuilder(ps={Ps})", ps);
            this.portalSettings = ps;

			Logger.LogDebug("SitemapBuilder(ps={Ps}): this.portalSettings → {Arg1}", ps, this.portalSettings);
			Logger.LogDebug("SitemapBuilder(ps={Ps}): portalSettings → {PortalSettings}", ps, portalSettings);
            LoadProviders();
			Logger.LogDebug("<<< Exiting SitemapBuilder(ps={Ps})", ps);
        }

        public string CacheFileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.cacheFileName))
                {
                    var currentCulture = this.portalSettings.CultureCode?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(currentCulture))
                    {
                        currentCulture = Localization.GetPageLocale((IPortalSettings)this.portalSettings).Name.ToLowerInvariant();
                    }

                    this.cacheFileName = $"sitemap.{currentCulture}.xml";
                }

                return this.cacheFileName;
            }
        }

        public string CacheIndexFileNameFormat
        {
            get
            {
                if (string.IsNullOrEmpty(this.cacheIndexFileNameFormat))
                {
                    var currentCulture = Localization.GetPageLocale((IPortalSettings)this.portalSettings).Name.ToLowerInvariant();
                    this.cacheIndexFileNameFormat = $"sitemap_{{0}}.{currentCulture}.xml";
                }

                return this.cacheIndexFileNameFormat;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public List<SitemapProvider> Providers => providers;

        /// <summary>Builds the complete portal sitemap.</summary>
        /// <param name="output">The writer to which the sitemap is to be written.</param>
        public void BuildSiteMap(TextWriter output)
        {
			Logger.LogDebug(">>> Entering BuildSiteMap(output={Output})", output);
            int cacheDays = int.Parse(PortalController.GetPortalSetting("SitemapCacheDays", this.portalSettings.PortalId, "1"), CultureInfo.InvariantCulture);
            bool cached = cacheDays > 0;
			Logger.LogDebug("BuildSiteMap(output={Output}): cacheDays → {CacheDays}", output, cacheDays);
			Logger.LogDebug("BuildSiteMap(output={Output}): cached → {Cached}", output, cached);

            if (cached && this.CacheIsValid())
            {
                this.WriteSitemapFileToOutput(this.CacheFileName, output);
                return;
            }

            var allUrls = new List<SitemapUrl>();

			Logger.LogDebug("BuildSiteMap(output={Output}): allUrls → {AllUrls}", output, allUrls);
            // excluded urls by priority
            float excludePriority = 0;
            excludePriority = float.Parse(PortalController.GetPortalSetting("SitemapExcludePriority", this.portalSettings.PortalId, "0"), NumberFormatInfo.InvariantInfo);
			Logger.LogDebug("BuildSiteMap(output={Output}): excludePriority → {ExcludePriority}", output, excludePriority);

            // get all urls
            bool isProviderEnabled = false;
            bool isProviderPriorityOverrided = false;
			Logger.LogDebug("BuildSiteMap(output={Output}): isProviderEnabled → {IsProviderEnabled}", output, isProviderEnabled);
			Logger.LogDebug("BuildSiteMap(output={Output}): isProviderPriorityOverrided → {IsProviderPriorityOverrided}", output, isProviderPriorityOverrided);
            float providerPriorityValue = 0;

			Logger.LogDebug("BuildSiteMap(output={Output}): providerPriorityValue → {ProviderPriorityValue}", output, providerPriorityValue);
            foreach (SitemapProvider provider in this.Providers)
            {
                isProviderEnabled = bool.Parse(PortalController.GetPortalSetting(provider.Name + "Enabled", this.portalSettings.PortalId, "True"));

                if (isProviderEnabled)
                {
                    // check if we should override the priorities
                    isProviderPriorityOverrided = bool.Parse(PortalController.GetPortalSetting(provider.Name + "Override", this.portalSettings.PortalId, "False"));

                    // stored as an integer (pr * 100) to prevent from translating errors with the decimal point
                    providerPriorityValue = float.Parse(PortalController.GetPortalSetting(provider.Name + "Value", this.portalSettings.PortalId, "50"), CultureInfo.InvariantCulture) / 100;

                    // Get all urls from provider
                    List<SitemapUrl> urls = [];
                    try
					Logger.LogDebug("BuildSiteMap(output={Output}): urls → {Urls}", output, urls);
                    {
                        urls = provider.GetUrls(this.portalSettings.PortalId, this.portalSettings, SitemapVersion);
                    }
                    catch (Exception ex)
                    {
						Logger.LogError("Exception in BuildSiteMap(output={Output}): {Arg1}", output, ex.Message);
                        var exceptionMessage = Localization.GetExceptionMessage(
                            "SitemapProviderError",
                            "URL sitemap provider '{0}' failed with error: {1}",
                            provider.Name,
                            ex.Message);
                        Services.Exceptions.Exceptions.LogException(new SitemapException(exceptionMessage, ex));
                    }

                    foreach (SitemapUrl url in urls)
                    {
                        if (isProviderPriorityOverrided)
                        {
                            url.Priority = providerPriorityValue;
							Logger.LogDebug("BuildSiteMap(output={Output}): url.Priority → {Arg1}", output, url.Priority);
                        }

                        if (url.Priority > 0 && url.Priority >= excludePriority)
                        {
                            // #RS# a valid sitemap needs priorities larger then 0, otherwise the sitemap will be rejected by google as invalid
                            allUrls.Add(url);
                        }
                    }
                }
            }

            if (allUrls.Count > SitemapMaxUrls)
            {
                // create a sitemap index file

                // enabled cache if it's not already
                if (!cached)
                {
                    cached = true;
                    PortalController.UpdatePortalSetting(this.portalSettings.PortalId, "SitemapCacheDays", "1");
                }

                // create all the files
                int index = 0;
                int numFiles = (allUrls.Count / SitemapMaxUrls) + 1;
                int elementsInFile = allUrls.Count / numFiles;
				Logger.LogDebug("BuildSiteMap(output={Output}): numFiles → {NumFiles}", output, numFiles);

                for (index = 1; index <= numFiles; index++)
                {
                    int lowerIndex = elementsInFile * (index - 1);
                    int elements = 0;
					Logger.LogDebug("BuildSiteMap(output={Output}): lowerIndex → {LowerIndex}", output, lowerIndex);
					Logger.LogDebug("BuildSiteMap(output={Output}): elements → {Elements}", output, elements);
                    if (index == numFiles)
                    {
                        // last file
                        elements = allUrls.Count - (elementsInFile * (numFiles - 1));
                    }
                    else
                    {
                        elements = elementsInFile;
                    }

                    this.WriteSitemap(cached, output, index, allUrls.GetRange(lowerIndex, elements));
                }

                // create the sitemap index
                this.WriteSitemapIndex(output, index - 1);
            }
            else
            {
                // create a regular sitemap file
                this.WriteSitemap(cached, output, 0, allUrls);
            }

            if (cached)
            {
                this.WriteSitemapFileToOutput(this.CacheFileName, output);
            }
			Logger.LogDebug("<<< Exiting BuildSiteMap(output={Output})", output);
        }

        /// <summary>  Returns the sitemap file that is part of a sitemapindex.</summary>
        /// <param name="index">Index of the sitemap to return.</param>
        /// <param name="output">The output stream.</param>
        /// <remarks>
        ///   The file should already exist since when using sitemapindexes the files are all cached to disk.
        /// </remarks>
        public void GetSitemapIndexFile(string index, TextWriter output)
        {
            if (!int.TryParse(index, NumberStyles.None, CultureInfo.InvariantCulture, out var theIndex))
            {
                return;
            }

            var currentCulture = Localization.GetPageLocale((IPortalSettings)this.portalSettings).Name.ToLowerInvariant();
            this.WriteSitemapFileToOutput($"sitemap_{theIndex}.{currentCulture}.xml", output);
			Logger.LogDebug("<<< Exiting GetSitemapIndexFile(index={Index},output={Output})", index, output);
        }

        private static void LoadProviders()
        {
			Logger.LogDebug(">>> Entering LoadProviders()");
            // Avoid claiming lock if providers are already loaded
            if (providers == null)
            {
                lock (Lock)
                {
                    providers = [];
					Logger.LogDebug("LoadProviders(): providers → {Providers}", providers);

                    foreach (var comp in ComponentFactory.GetComponents<SitemapProvider>())
                    {
                        comp.Value.Name = comp.Key;
                        comp.Value.Description = comp.Value.Description;
                        providers.Add(comp.Value);
                    }

                    ////ProvidersHelper.InstantiateProviders(section.Providers, _providers, GetType(SiteMapProvider))
                }
            }
			Logger.LogDebug("<<< Exiting LoadProviders()");
        }

        /// <summary>  Adds a new url to the sitemap.</summary>
        /// <param name="sitemapUrl">The url to be included in the sitemap.</param>
        /// <param name="writer">The writer.</param>
        private static void AddUrl(SitemapUrl sitemapUrl, XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering AddUrl(sitemapUrl={SitemapUrl},writer={Writer})", sitemapUrl, writer);
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", sitemapUrl.Url);
            writer.WriteElementString("lastmod", sitemapUrl.LastModified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            writer.WriteElementString("changefreq", sitemapUrl.ChangeFrequency.ToString().ToLowerInvariant());
            writer.WriteElementString("priority", sitemapUrl.Priority.ToString("F01", CultureInfo.InvariantCulture));

            ////if (sitemapUrl.AlternateUrls != null)
            ////{
            ////   foreach (AlternateUrl alternate in sitemapUrl.AlternateUrls)
            ////   {
            ////       writer.WriteStartElement("link", "http://www.w3.org/1999/xhtml");
            ////       writer.WriteAttributeString("rel", "alternate");
            ////       writer.WriteAttributeString("hreflang", alternate.Language);
            ////       writer.WriteAttributeString("href", alternate.Url);
            ////       writer.WriteEndElement();
            ////   }
            ////}
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting AddUrl(sitemapUrl={SitemapUrl},writer={Writer})", sitemapUrl, writer);
        }

        private static bool IsChildPortal(PortalSettings ps, HttpContext context)
        {
			Logger.LogDebug(">>> Entering IsChildPortal(ps={Ps},context={Context})", ps, context);
            bool isChild = false;
            string portalName = null;
			Logger.LogDebug("IsChildPortal(ps={Ps},context={Context}): isChild → {IsChild}", ps, context, isChild);
			Logger.LogDebug("IsChildPortal(ps={Ps},context={Context}): portalName → {PortalName}", ps, context, portalName);
            var arr = PortalAliasController.Instance.GetPortalAliasesByPortalId(ps.PortalId).ToList();
            string serverPath = Globals.GetAbsoluteServerPath(context.Request);
			Logger.LogDebug("IsChildPortal(ps={Ps},context={Context}): arr → {Arr}", ps, context, arr);

            if (arr.Count > 0)
            {
                IPortalAliasInfo portalAlias = arr[0];
                portalName = Globals.GetPortalDomainName(ps.PortalAlias.HTTPAlias, null, true);
                if (portalAlias.HttpAlias.IndexOf("/", StringComparison.Ordinal) > -1)
                {
                    portalName = PortalController.GetPortalFolder(portalAlias.HttpAlias);
                }

                if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                {
                    isChild = true;
                }
            }

			Logger.LogDebug("<<< Exiting IsChildPortal(ps={Ps},context={Context})", ps, context);
            return isChild;
        }

        /// <summary>  Generates a sitemap file.</summary>
        /// <param name="cached">Whether the generated file should be cached or not.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="index">For sitemapindex files the number of the file being generated, 0 otherwise.</param>
        /// <param name="allUrls">The list of urls to be included in the file.</param>
        /// <remarks>
        ///   If the output should be cached it will generate a file under the portal directory (portals\[portalid]\sitemaps\) with
        ///   the result of the generation. If the file is part of a sitemap, <paramref name = "index">index</paramref> will be appended to the
        ///   filename cached on disk ("sitemap_1.xml").
        /// </remarks>
        private void WriteSitemap(bool cached, TextWriter output, int index, List<SitemapUrl> allUrls)
        {
            // sitemap Output: can be a file is cache is enabled
            TextWriter sitemapOutput = output;
            try
            {
                if (cached)
                {
                    if (!Directory.Exists(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap"))
                    {
                        Directory.CreateDirectory(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap");
                    }

                    var cachedFile = (index > 0) ? string.Format(CultureInfo.InvariantCulture, this.CacheIndexFileNameFormat, index) : this.CacheFileName;
                    sitemapOutput = new StreamWriter(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + cachedFile, false, Encoding.UTF8);
                }

                // Initialize writer
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                settings.OmitXmlDeclaration = false;

                using (var writer = XmlWriter.Create(sitemapOutput, settings))
                {
                    // build header
                    writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/" + SitemapVersion);
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
                    const string schemaLocation = "http://www.sitemaps.org/schemas/sitemap/" + SitemapVersion;
                    writer.WriteAttributeString("xsi", "schemaLocation", null, $"{schemaLocation} {schemaLocation}/sitemap.xsd");

                    // write urls to output
                    foreach (SitemapUrl url in allUrls)
                    {
                        AddUrl(url, writer);
                    }

                    writer.WriteEndElement();
                    writer.Close();
                }

                if (cached)
                {
                    sitemapOutput.Flush();
                    sitemapOutput.Close();
                }
            }
            finally
            {
                sitemapOutput?.Dispose();
            }
        }

        /// <summary>Generates a sitemap index file.</summary>
        /// <param name="output">The output stream.</param>
        /// <param name="totalFiles">Number of files that are included in the sitemap index.</param>
        private void WriteSitemapIndex(TextWriter output, int totalFiles)
        {
			Logger.LogDebug(">>> Entering WriteSitemapIndex(output={Output},totalFiles={TotalFiles})", output, totalFiles);
            using var sitemapOutput = new StreamWriter($@"{this.portalSettings.HomeSystemDirectoryMapPath}Sitemap\{this.CacheFileName}", false, Encoding.UTF8);

			Logger.LogDebug("WriteSitemapIndex(output={Output},totalFiles={TotalFiles}): sitemapOutput → {SitemapOutput}", output, totalFiles, sitemapOutput);
            // Initialize writer
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false,
            };

            using (var writer = XmlWriter.Create(sitemapOutput, settings))
            {
                // build header
                writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/" + SitemapVersion);

                // write urls to output
                for (int index = 1; index <= totalFiles; index++)
                {
                    string url = null;

                    url = "~/Sitemap.aspx?i=" + index.ToString(CultureInfo.InvariantCulture);
                    if (IsChildPortal(this.portalSettings, HttpContext.Current))
                    {
                        url += "&portalid=" + this.portalSettings.PortalId.ToString(CultureInfo.InvariantCulture);
                    }

                    writer.WriteStartElement("sitemap");
                    writer.WriteElementString("loc", Globals.AddHTTP(HttpContext.Current.Request.Url.Host + Globals.ResolveUrl(url)));
                    writer.WriteElementString("lastmod", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.Close();
            }

            sitemapOutput.Flush();
            sitemapOutput.Close();
			Logger.LogDebug("<<< Exiting WriteSitemapIndex(output={Output},totalFiles={TotalFiles})", output, totalFiles);
        }

        /// <summary>  Is sitemap is cached, verifies is the cached file exists and is still valid.</summary>
        /// <returns>True is the cached file exists and is still valid, false otherwise.</returns>
        private bool CacheIsValid()
        {
			Logger.LogDebug(">>> Entering CacheIsValid()");
            int cacheDays = int.Parse(PortalController.GetPortalSetting("SitemapCacheDays", this.portalSettings.PortalId, "1"), CultureInfo.InvariantCulture);
            var isValid = File.Exists(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + this.CacheFileName);
			Logger.LogDebug("CacheIsValid(): cacheDays → {CacheDays}", cacheDays);
			Logger.LogDebug("CacheIsValid(): isValid → {IsValid}", isValid);

            if (!isValid)
            {
                return isValid;
            }

            DateTime lastmod = File.GetLastWriteTime(this.portalSettings.HomeSystemDirectoryMapPath + "/Sitemap/" + this.CacheFileName);
            if (lastmod.AddDays(cacheDays) < DateTime.Now)
			Logger.LogDebug("CacheIsValid(): lastmod → {Lastmod}", lastmod);
            {
                isValid = false;
            }

			Logger.LogDebug("<<< Exiting CacheIsValid()");
            return isValid;
        }

        /// <summary>  When the sitemap is cached, reads the sitemap file and writes to the output stream.</summary>
        /// <param name="file">File name.</param>
        /// <param name="output">The output stream.</param>
        private void WriteSitemapFileToOutput(string file, TextWriter output)
        {
			Logger.LogDebug(">>> Entering WriteSitemapFileToOutput(file={File},output={Output})", file, output);
            if (!File.Exists(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + file))
            {
                return;
            }

            // write the cached file to output
            using var reader = new StreamReader(this.portalSettings.HomeSystemDirectoryMapPath + "/Sitemap/" + file, Encoding.UTF8);
            output.Write(reader.ReadToEnd());
			Logger.LogDebug("WriteSitemapFileToOutput(file={File},output={Output}): reader → {Reader}", file, output, reader);

            reader.Close();
        }
    }
}
