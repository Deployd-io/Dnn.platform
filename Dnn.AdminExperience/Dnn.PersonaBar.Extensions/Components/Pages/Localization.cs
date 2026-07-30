// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using System.IO;

    public class Localization
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static string LocalResourcesFile => Path.Combine(Library.Constants.PersonaBarRelativePath, "Modules/Dnn.Pages/App_LocalResources/Pages.resx");

        public static string GetString(string key)
        {
			Logger.LogDebug(">>> Entering GetString(key={Key})", key);
			Logger.LogDebug("<<< Exiting GetString(key={Key})", key);
            return DotNetNuke.Services.Localization.Localization.GetString(key, LocalResourcesFile);
        }
    }
}
