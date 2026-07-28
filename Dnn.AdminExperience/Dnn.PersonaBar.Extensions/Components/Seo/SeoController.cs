// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Seo.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Sitemap;

    public class SeoController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly PortalSettings portalSettings;

        /// <summary>Initializes a new instance of the <see cref="SeoController"/> class.</summary>
        /// <param name="applicationStatusInfo">Application status info.</param>
        public SeoController(IApplicationStatusInfo applicationStatusInfo)
        {
			Logger.LogDebug(">>> Entering SeoController(applicationStatusInfo={ApplicationStatusInfo})", applicationStatusInfo);
            this.applicationStatusInfo = applicationStatusInfo;
            this.portalSettings = PortalSettings.Current;
			Logger.LogDebug("<<< Exiting SeoController(applicationStatusInfo={ApplicationStatusInfo})", applicationStatusInfo);
			Logger.LogDebug("SeoController(applicationStatusInfo={ApplicationStatusInfo}): this.applicationStatusInfo → {Arg1}", applicationStatusInfo, this.applicationStatusInfo);
			Logger.LogDebug("SeoController(applicationStatusInfo={ApplicationStatusInfo}): portalSettings → {PortalSettings}", applicationStatusInfo, portalSettings);
			Logger.LogDebug("SeoController(applicationStatusInfo={ApplicationStatusInfo}): this.portalSettings → {Arg1}", applicationStatusInfo, this.portalSettings);
			Logger.LogDebug("SeoController(applicationStatusInfo={ApplicationStatusInfo}): applicationStatusInfo → {ApplicationStatusInfo1}", applicationStatusInfo, applicationStatusInfo);
        }

        public IEnumerable<SitemapProvider> GetSitemapProviders()
        {
			Logger.LogDebug(">>> Entering GetSitemapProviders()");
            var builder = new SitemapBuilder(this.portalSettings);
			Logger.LogDebug("GetSitemapProviders(): builder → {Builder}", builder);
			Logger.LogDebug("<<< Exiting GetSitemapProviders()");
            return builder.Providers;
        }

        public void ResetCache()
        {
			Logger.LogDebug(">>> Entering ResetCache()");
            var cacheFolder = new DirectoryInfo(this.portalSettings.HomeSystemDirectoryMapPath + "sitemap\\");

			Logger.LogDebug("ResetCache(): cacheFolder → {CacheFolder}", cacheFolder);
            if (cacheFolder.Exists)
            {
                foreach (var file in cacheFolder.GetFiles("sitemap*.xml"))
                {
                    file.Delete();
                }
            }
			Logger.LogDebug("<<< Exiting ResetCache()");
        }

        public string GetSearchEngineSubmissionUrl(string searchEngine)
        {
			Logger.LogDebug(">>> Entering GetSearchEngineSubmissionUrl(searchEngine={SearchEngine})", searchEngine);
            var url = string.Empty;

			Logger.LogDebug("GetSearchEngineSubmissionUrl(searchEngine={SearchEngine}): url → {Url}", searchEngine, url);
            switch (searchEngine.ToUpperInvariant().Trim())
            {
                case "GOOGLE":
                    url += "http://www.google.com/addurl?q=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(HttpContext.Current.Request)));
                    url += "&dq=";
                    if (!string.IsNullOrEmpty(this.portalSettings.PortalName))
                    {
                        url += Globals.HTTPPOSTEncode(this.portalSettings.PortalName);
                    }

                    if (!string.IsNullOrEmpty(this.portalSettings.Description))
                    {
                        url += Globals.HTTPPOSTEncode(this.portalSettings.Description);
                    }

                    if (!string.IsNullOrEmpty(this.portalSettings.KeyWords))
                    {
                        url += Globals.HTTPPOSTEncode(this.portalSettings.KeyWords);
                    }

                    url += "&submit=Add+URL";
                    break;
                case "YAHOO!":
                    url = "http://siteexplorer.search.yahoo.com/submit";
                    break;
                case "BING":
                    url = "http://www.bing.com/webmaster";
                    break;
            }

			Logger.LogDebug("<<< Exiting GetSearchEngineSubmissionUrl(searchEngine={SearchEngine})", searchEngine);
            return url;
        }

        public void CreateVerification(string verification)
        {
			Logger.LogDebug(">>> Entering CreateVerification(verification={Verification})", verification);
            if (string.IsNullOrEmpty(verification) || !verification.EndsWith(".html", StringComparison.Ordinal))
            {
                return;
            }

            var verificationPath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, Path.GetFileName(verification));
            if (File.Exists(verificationPath))
			Logger.LogDebug("CreateVerification(verification={Verification}): verificationPath → {VerificationPath}", verification, verificationPath);
            {
                return;
            }

            var portalAlias = !string.IsNullOrEmpty(this.portalSettings.DefaultPortalAlias)
                ? this.portalSettings.DefaultPortalAlias
                : ((IPortalAliasInfo)this.portalSettings.PortalAlias).HttpAlias;

            // write SiteMap verification file
            var objStream = File.CreateText(verificationPath);
            objStream.WriteLine("Google SiteMap Verification File");
			Logger.LogDebug("CreateVerification(verification={Verification}): objStream → {ObjStream}", verification, objStream);
            objStream.WriteLine(" - " + Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx");
            objStream.WriteLine(" - " + UserController.Instance.GetCurrentUserInfo().DisplayName);
            objStream.Close();
			Logger.LogDebug("<<< Exiting CreateVerification(verification={Verification})", verification);
        }
    }
}
