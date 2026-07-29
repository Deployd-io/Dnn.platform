// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System.Collections.Generic;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    public class InstalledModulesController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static List<InstalledModuleInfo> GetInstalledModules()
        {
			Logger.LogDebug(">>> Entering GetInstalledModules()");
			Logger.LogDebug("<<< Exiting GetInstalledModules()");
            return CBO.FillCollection<InstalledModuleInfo>(DataProvider.Instance().GetInstalledModules());
        }
    }
}
