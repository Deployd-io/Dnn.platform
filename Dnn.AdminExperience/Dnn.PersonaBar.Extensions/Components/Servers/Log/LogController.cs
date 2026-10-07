// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Components.Log
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Services.FileSystem;

    public class LogController
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public List<LogFile> GetLogFilesList()
        {
			eventLogger.LogError(">>> Entering GetLogFilesList()");
            var logPath = Globals.ApplicationMapPath + @"\portals\_default\logs";
			eventLogger.LogError("GetLogFilesList(): logPath → {LogPath}", logPath);
			eventLogger.LogWarning("<<< Exiting GetLogFilesList()");
            return GetLogList(logPath, "*.resources");
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public List<LogFile> GetUpgradeLogList()
        {
			eventLogger.LogError(">>> Entering GetUpgradeLogList()");
            var objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration("data");
            var strProviderPath = DataProvider.Instance().GetProviderPath();
			eventLogger.LogError("GetUpgradeLogList(): strProviderPath → {StrProviderPath}", strProviderPath);
			eventLogger.LogError("GetUpgradeLogList(): objProviderConfiguration → {ObjProviderConfiguration}", objProviderConfiguration);
			eventLogger.LogWarning("<<< Exiting GetUpgradeLogList()");
            return GetLogList(strProviderPath, "*.log.resources");
        }

        private static List<LogFile> GetLogList(string logPath, string searchPattern)
        {
			eventLogger.LogError(">>> Entering GetLogList(logPath={LogPath},searchPattern={SearchPattern})", logPath, searchPattern);
            var fileSizeFormatProvider = new FileSizeFormatProvider();
			eventLogger.LogError("GetLogList(logPath={LogPath},searchPattern={SearchPattern}): fileSizeFormatProvider → {FileSizeFormatProvider}", logPath, searchPattern, fileSizeFormatProvider);
			eventLogger.LogWarning("<<< Exiting GetLogList(logPath={LogPath},searchPattern={SearchPattern})", logPath, searchPattern);
            return (
                from file in new DirectoryInfo(logPath).EnumerateFiles(searchPattern)
                select new LogFile
                {
                    Name = file.Name,
                    LastWriteTimeUtc = file.LastWriteTimeUtc,
                    Size = string.Format(fileSizeFormatProvider, "{0:fs}", file.Length),
                })
                .ToList();
        }
    }
}
