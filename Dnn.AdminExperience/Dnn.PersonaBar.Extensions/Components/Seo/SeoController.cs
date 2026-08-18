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
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly PortalSettings portalSettings;

        /// <summary>Initializes a new instance of the <see cref="SeoController"/> class.</summary>
        /// <param name="applicationStatusInfo">Application status info.</param>
        public SeoController(IApplicationStatusInfo applicationStatusInfo)
        {
			eventLogger.LogDebug(">>> Entering SeoController(applicationStatusInfo={ApplicationStatusInfo})", applicationStatusInfo);
            this.applicationStatusInfo = applicationStatusInfo;
            this.portalSettings = PortalSettings.Current;
			eventLogger.LogDebug("SeoController(applicationStatusInfo={ApplicationStatusInfo}): this.applicationStatusInfo → {Arg1}", applicationStatusInfo, this.applicationStatusInfo);
			eventLogger.LogDebug("SeoController(applicationStatusInfo={ApplicationStatusInfo}): portalSettings → {PortalSettings}", applicationStatusInfo, portalSettings);
			eventLogger.LogDebug("<<< Exiting SeoController(applicationStatusInfo={ApplicationStatusInfo})", applicationStatusInfo);
        }

        public IEnumerable<SitemapProvider> GetSitemapProviders()
        {
			eventLogger.LogDebug(">>> Entering GetSitemapProviders()");
            var builder = new SitemapBuilder(this.portalSettings);
			eventLogger.LogDebug("GetSitemapProviders(): builder → {Builder}", builder);
			eventLogger.LogDebug("<<< Exiting GetSitemapProviders()");
            return builder.Providers;
        }

        public void ResetCache()
        {
			eventLogger.LogDebug(">>> Entering ResetCache()");
            var cacheFolder = new DirectoryInfo(this.portalSettings.HomeSystemDirectoryMapPath + "sitemap\\");

			eventLogger.LogDebug("ResetCache(): cacheFolder → {CacheFolder}", cacheFolder);
            if (cacheFolder.Exists)
            {
                foreach (var file in cacheFolder.GetFiles("sitemap*.xml"))
                {
                    file.Delete();
                }
            }
			eventLogger.LogDebug("<<< Exiting ResetCache()");
        }

        public string GetSearchEngineSubmissionUrl(string searchEngine)
        {
			eventLogger.LogDebug(">>> Entering GetSearchEngineSubmissionUrl(searchEngine={SearchEngine})", searchEngine);
            var url = string.Empty;

			eventLogger.LogDebug("GetSearchEngineSubmissionUrl(searchEngine={SearchEngine}): url → {Url}", searchEngine, url);
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

			eventLogger.LogDebug("<<< Exiting GetSearchEngineSubmissionUrl(searchEngine={SearchEngine})", searchEngine);
            return url;
        }

        public void CreateVerification(string verification)
        {
			eventLogger.LogDebug(">>> Entering CreateVerification(verification={Verification})", verification);
            if (string.IsNullOrEmpty(verification) || !verification.EndsWith(".html", StringComparison.Ordinal))
            {
                return;
            }

            var verificationPath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, Path.GetFileName(verification));
			eventLogger.LogDebug("CreateVerification(verification={Verification}): verificationPath → {VerificationPath}", verification, verificationPath);
            if (File.Exists(verificationPath))
            {
                return;
            }

            var portalAlias = !string.IsNullOrEmpty(this.portalSettings.DefaultPortalAlias)
                ? this.portalSettings.DefaultPortalAlias
                : ((IPortalAliasInfo)this.portalSettings.PortalAlias).HttpAlias;

            // write SiteMap verification file
            var objStream = File.CreateText(verificationPath);
			eventLogger.LogDebug("CreateVerification(verification={Verification}): objStream → {ObjStream}", verification, objStream);
            objStream.WriteLine("Google SiteMap Verification File");
            objStream.WriteLine(" - " + Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx");
            objStream.WriteLine(" - " + UserController.Instance.GetCurrentUserInfo().DisplayName);
            objStream.Close();
			eventLogger.LogDebug("<<< Exiting CreateVerification(verification={Verification})", verification);
        }
    }
}
