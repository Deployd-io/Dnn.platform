// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Application;
    using DotNetNuke.Common;

    public class CheckDefaultPage : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckDefaultPage";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogError(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
			eventLogger.LogError("Execute(): result → {Result}", result);
            try
            {
                IList<string> modifiedFiles;
                var fileModified = CheckDefaultPageModified(out modifiedFiles);
				eventLogger.LogError("Execute(): fileModified → {FileModified}", fileModified);
                if (fileModified)
                {
                    if (modifiedFiles.Count == 0)
                    {
                        if (DotNetNukeContext.Current.Application.Version.Major > 6)
                        {
                            result.Notes.Add("There is no data available about your current installation, please upgrade this module to it's latest version.");
                        }
                        else
                        {
                            fileModified = false;
                        }
                    }

                    result.Severity = SeverityEnum.Failure;
					eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                    foreach (var filename in modifiedFiles)
                    {
                        result.Notes.Add("file:" + filename);
                    }
                }
                else
                {
                    result.Severity = SeverityEnum.Pass;
                }
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in Execute(): {Arg0}", Exception.Message);
			eventLogger.LogWarning("<<< Exiting Execute()");
                throw;
            }

            return result;
        }

        private static bool CheckDefaultPageModified(out IList<string> modifiedFiles)
        {
			eventLogger.LogError(">>> Entering CheckDefaultPageModified()");
            modifiedFiles = new List<string>();

			eventLogger.LogDebug("CheckDefaultPageModified(): modifiedFiles → {ModifiedFiles}", modifiedFiles);
            var sumData = Utility.LoadFileSumData();
			eventLogger.LogError("CheckDefaultPageModified(): sumData → {SumData}", sumData);
			eventLogger.LogError("CheckDefaultPageModified(): modifiedFiles → {ModifiedFiles}", modifiedFiles);

            var appVersion = Utility.GetApplicationVersion();
			eventLogger.LogError("CheckDefaultPageModified(): appVersion → {AppVersion}", appVersion);
            var appType = Utility.GetApplicationType();
			eventLogger.LogError("CheckDefaultPageModified(): appType → {AppType}", appType);

            var dataNodes = sumData.SelectNodes("/checksums/sum[@version=\"" + appVersion + "\"][@type=\"" + appType + "\"]");
			eventLogger.LogError("CheckDefaultPageModified(): dataNodes → {DataNodes}", dataNodes);
            if (dataNodes == null || dataNodes.Count == 0)
            {
                return true; // when no record matched, need notify user to update the module.
            }

            var fileModified = false;
			eventLogger.LogError("CheckDefaultPageModified(): fileModified → {FileModified}", fileModified);
            foreach (XmlNode node in dataNodes)
            {
                var fileName = node.Attributes["name"].Value;
				eventLogger.LogError("CheckDefaultPageModified(): fileName → {FileName}", fileName);
                var sum = node.Attributes["sum"].Value;
				eventLogger.LogError("CheckDefaultPageModified(): sum → {Sum}", sum);
                var file = Path.Combine(Globals.ApplicationMapPath, fileName);
				eventLogger.LogError("CheckDefaultPageModified(): file → {File}", file);
                if (!File.Exists(file) || !Utility.GetFileCheckSum(file).Equals(sum, StringComparison.OrdinalIgnoreCase))
                {
                    fileModified = true;
                    modifiedFiles.Add(fileName);
                }
            }

			eventLogger.LogWarning("<<< Exiting CheckDefaultPageModified()");
            return fileModified;
        }
    }
}
