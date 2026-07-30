// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.DatabaseServer
{
    using System;
    using System.Data;

    using DotNetNuke.Data;
    using DotNetNuke.Services.Localization;

    public class DataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private static readonly DataProvider Provider = DataProvider.Instance();
        private static string moduleQualifier = "PersonaBar_";

        public static IDataReader GetDbInfo()
        {
			Logger.LogDebug(">>> Entering GetDbInfo()");
			Logger.LogDebug("<<< Exiting GetDbInfo()");
            return Provider.ExecuteReader(GetFullyQualifiedName("GetDbInfo"));
        }

        public static IDataReader GetDbFileInfo()
        {
			Logger.LogDebug(">>> Entering GetDbFileInfo()");
			Logger.LogDebug("<<< Exiting GetDbFileInfo()");
            return Provider.ExecuteReader(GetFullyQualifiedName("GetDbFileInfo"));
        }

        public static IDataReader GetDbBackups()
        {
			Logger.LogDebug(">>> Entering GetDbBackups()");
			Logger.LogDebug("<<< Exiting GetDbBackups()");
            return Provider.ExecuteReader(GetFullyQualifiedName("GetDbBackups"));
        }

        private static string GetFullyQualifiedName(string name)
        {
			Logger.LogDebug(">>> Entering GetFullyQualifiedName(name={Name})", name);
			Logger.LogDebug("<<< Exiting GetFullyQualifiedName(name={Name})", name);
            return string.Concat(moduleQualifier, name);
        }
    }
}
