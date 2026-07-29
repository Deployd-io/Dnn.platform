// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Components
{
    using DotNetNuke.Services.Localization;

    /// <summary>Localization helper for the control bar.</summary>
    public class LocalizationHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string ResourceFile = "admin/ControlPanel/App_LocalResources/ControlBar";

        /// <summary>Gets the localized string corresponding to the <paramref name="key"/>, for the control bar.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized Text.</returns>
        public static string GetControlBarString(string key)
        {
			Logger.LogDebug(">>> Entering GetControlBarString(key={Key})", key);
			Logger.LogDebug("<<< Exiting GetControlBarString(key={Key})", key);
            return Localization.GetString(key, ResourceFile);
        }
    }
}
