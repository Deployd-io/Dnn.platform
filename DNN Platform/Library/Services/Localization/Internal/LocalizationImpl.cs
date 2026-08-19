// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;

    internal class LocalizationImpl : ILocalization
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalizationImpl));
        /// <inheritdoc />
        public string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback)
        {
			Logger.LogDebug(">>> Entering BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback})", cultureCodes, fallback);
            if (cultureCodes == null)
            {
				Logger.LogWarning("Throwing ArgumentException from BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback})", cultureCodes, fallback);
                throw new ArgumentException("cultureCodes cannot be null");
            }

            if (fallback == null)
            {
                throw new ArgumentException("fallback cannot be null");
            }

            var values = cultureCodes.ToList();
			Logger.LogDebug("BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback}): values → {Values}", cultureCodes, fallback, values);

            foreach (string langHeader in HttpContextSource.Current.Request.UserLanguages ?? [])
            {
                string lang = langHeader;
				Logger.LogDebug("BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback}): lang → {Lang}", cultureCodes, fallback, lang);

                // strip any ;q=xx
                lang = lang.Split(';')[0];

                // check for exact match e.g. de-DE == de-DE
                if (lang.Contains('-'))
                {
                    var match = values.FirstOrDefault(x => x == lang);
					Logger.LogDebug("BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback}): match → {Match}", cultureCodes, fallback, match);
                    if (match != null)
                    {
                        return match;
                    }
                }

                // only keep the initial language value
                if (lang.Length > 1)
                {
                    lang = lang.Substring(0, 2);

                    // check for language match e.g. en-GB == en-US because en == en
                    var match = values.FirstOrDefault(x => x.StartsWith(lang, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        return match;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes},fallback={Fallback})", cultureCodes, fallback);
			Logger.LogDebug("<<< Exiting BestCultureCodeBasedOnBrowserLanguages(cultureCodes={CultureCodes})", cultureCodes);
            return fallback;
        }

        /// <inheritdoc />
        public string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes)
        {
            return this.BestCultureCodeBasedOnBrowserLanguages(cultureCodes, Localization.SystemLocale);
        }

        /// <inheritdoc />
        public CultureInfo GetPageLocale(PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering GetPageLocale(portalSettings={PortalSettings})", portalSettings);
			Logger.LogDebug("<<< Exiting GetPageLocale(portalSettings={PortalSettings})", portalSettings);
            return Localization.GetPageLocale((IPortalSettings)portalSettings);
        }

        /// <inheritdoc />
        public void SetThreadCultures(CultureInfo cultureInfo, PortalSettings portalSettings)
        {
			Logger.LogDebug(">>> Entering SetThreadCultures(cultureInfo={CultureInfo},portalSettings={PortalSettings})", cultureInfo, portalSettings);
            Localization.SetThreadCultures(cultureInfo, (IPortalSettings)portalSettings);
			Logger.LogDebug("<<< Exiting SetThreadCultures(cultureInfo={CultureInfo},portalSettings={PortalSettings})", cultureInfo, portalSettings);
        }
    }
}
