// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Globalization;

    using DotNetNuke.Services.Localization;
	using Microsoft.Extensions.Logging;

    /// <summary>Utility class to support simple localization.</summary>
    internal sealed class Localizer : ILocalizer
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>The relative path to the resources file for localization.</summary>
        public static readonly string ResourceFile =
            "~/DesktopModules/TelerikRemoval/App_LocalResources/View.ascx.resx";

        private readonly ILocalizationProvider localizationProvider;

        /// <summary>Initializes a new instance of the <see cref="Localizer"/> class.</summary>
        /// <param name="localizationProvider">An instance of <see cref="ILocalizationProvider"/>.</param>
        public Localizer(ILocalizationProvider localizationProvider)
        {
			_logger.LogDebug(">>> Entering Localizer(localizationProvider={LocalizationProvider})", localizationProvider);
            this.localizationProvider = localizationProvider ??
				_logger.LogDebug("<<< Exiting Localizer(localizationProvider={LocalizationProvider})", localizationProvider);
				_logger.LogDebug("Localizer(localizationProvider={LocalizationProvider}): localizationProvider → {LocalizationProvider1}", localizationProvider, localizationProvider);
				_logger.LogDebug("Localizer(localizationProvider={LocalizationProvider}): this.localizationProvider → {Arg1}", localizationProvider, this.localizationProvider);
				_logger.LogWarning("Throwing ArgumentNullException from Localizer(localizationProvider={LocalizationProvider})", localizationProvider);
		_logger.LogWarning("Throwing ArgumentNullException from Localizer(localizationProvider={LocalizationProvider})", localizationProvider);
                throw new ArgumentNullException(nameof(localizationProvider));
        }

        /// <inheritdoc />
        public string Localize(string key)
        {
			_logger.LogDebug(">>> Entering Localize(key={Key})", key);
			_logger.LogDebug("<<< Exiting Localize(key={Key})", key);
            return this.localizationProvider.GetString(key, ResourceFile);
        }

        /// <inheritdoc />
        public string LocalizeFormat(string formatKey, params object[] args)
        {
			_logger.LogDebug(">>> Entering LocalizeFormat(formatKey={FormatKey},args={Args})", formatKey, args);
			_logger.LogDebug("<<< Exiting LocalizeFormat(formatKey={FormatKey},args={Args})", formatKey, args);
            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize(formatKey),
                args);
        }
    }
}
