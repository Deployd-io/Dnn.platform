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
			eventLogger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
			eventLogger.LogDebug("Execute(): result → {Result}", result);
            {
                IList<string> modifiedFiles;
                var fileModified = CheckDefaultPageModified(out modifiedFiles);
				eventLogger.LogDebug("Execute(): fileModified → {FileModified}", fileModified);
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
			eventLogger.LogDebug("<<< Exiting Execute()");
                throw;
            }

            return result;
        }

        private static bool CheckDefaultPageModified(out IList<string> modifiedFiles)
        {
			eventLogger.LogDebug(">>> Entering CheckDefaultPageModified()");
            modifiedFiles = new List<string>();

			eventLogger.LogDebug("CheckDefaultPageModified(): modifiedFiles → {ModifiedFiles}", modifiedFiles);
            var sumData = Utility.LoadFileSumData();
			eventLogger.LogDebug("CheckDefaultPageModified(): sumData → {SumData}", sumData);

            var appVersion = Utility.GetApplicationVersion();
			eventLogger.LogDebug("CheckDefaultPageModified(): appVersion → {AppVersion}", appVersion);
            var appType = Utility.GetApplicationType();
			eventLogger.LogDebug("CheckDefaultPageModified(): appType → {AppType}", appType);

            var dataNodes = sumData.SelectNodes("/checksums/sum[@version=\"" + appVersion + "\"][@type=\"" + appType + "\"]");
			eventLogger.LogDebug("CheckDefaultPageModified(): dataNodes → {DataNodes}", dataNodes);
            if (dataNodes == null || dataNodes.Count == 0)
            {
                return true; // when no record matched, need notify user to update the module.
            }

            var fileModified = false;
			eventLogger.LogDebug("CheckDefaultPageModified(): fileModified → {FileModified}", fileModified);
            foreach (XmlNode node in dataNodes)
            {
                var fileName = node.Attributes["name"].Value;
				eventLogger.LogDebug("CheckDefaultPageModified(): fileName → {FileName}", fileName);
                var sum = node.Attributes["sum"].Value;
				eventLogger.LogDebug("CheckDefaultPageModified(): sum → {Sum}", sum);
                var file = Path.Combine(Globals.ApplicationMapPath, fileName);
				eventLogger.LogDebug("CheckDefaultPageModified(): file → {File}", file);
                if (!File.Exists(file) || !Utility.GetFileCheckSum(file).Equals(sum, StringComparison.OrdinalIgnoreCase))
                {
                    fileModified = true;
                    modifiedFiles.Add(fileName);
                }
            }

			eventLogger.LogDebug("<<< Exiting CheckDefaultPageModified()");
            return fileModified;
        }
    }
}
