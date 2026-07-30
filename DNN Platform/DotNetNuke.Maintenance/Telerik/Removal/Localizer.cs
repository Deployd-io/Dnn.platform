// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Globalization;

    using DotNetNuke.Services.Localization;

    /// <summary>Utility class to support simple localization.</summary>
    internal sealed class Localizer : ILocalizer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>The relative path to the resources file for localization.</summary>
        public static readonly string ResourceFile =
            "~/DesktopModules/TelerikRemoval/App_LocalResources/View.ascx.resx";

        private readonly ILocalizationProvider localizationProvider;

        /// <summary>Initializes a new instance of the <see cref="Localizer"/> class.</summary>
        /// <param name="localizationProvider">An instance of <see cref="ILocalizationProvider"/>.</param>
        public Localizer(ILocalizationProvider localizationProvider)
        {
			Logger.LogDebug(">>> Entering Localizer(localizationProvider={LocalizationProvider})", localizationProvider);
            this.localizationProvider = localizationProvider ??
				Logger.LogDebug("<<< Exiting Localizer(localizationProvider={LocalizationProvider})", localizationProvider);
				Logger.LogDebug("Localizer(localizationProvider={LocalizationProvider}): localizationProvider → {LocalizationProvider1}", localizationProvider, localizationProvider);
				Logger.LogDebug("Localizer(localizationProvider={LocalizationProvider}): this.localizationProvider → {Arg1}", localizationProvider, this.localizationProvider);
                throw new ArgumentNullException(nameof(localizationProvider));
        }

        /// <inheritdoc />
        public string Localize(string key)
        {
			Logger.LogDebug(">>> Entering Localize(key={Key})", key);
			Logger.LogDebug("<<< Exiting Localize(key={Key})", key);
            return this.localizationProvider.GetString(key, ResourceFile);
        }

        /// <inheritdoc />
        public string LocalizeFormat(string formatKey, params object[] args)
        {
			Logger.LogDebug(">>> Entering LocalizeFormat(formatKey={FormatKey},args={Args})", formatKey, args);
			Logger.LogDebug("<<< Exiting LocalizeFormat(formatKey={FormatKey},args={Args})", formatKey, args);
            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize(formatKey),
                args);
        }
    }
}
