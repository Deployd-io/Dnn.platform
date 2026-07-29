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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckDefaultPage";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
			Logger.LogDebug("Execute(): result → {Result}", result);
            {
                IList<string> modifiedFiles;
                var fileModified = CheckDefaultPageModified(out modifiedFiles);
                if (fileModified)
				Logger.LogDebug("Execute(): fileModified → {FileModified}", fileModified);
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
					Logger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
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
				Logger.LogError("Exception in Execute(): {Arg0}", ex.Message);
                throw;
            }

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }

        private static bool CheckDefaultPageModified(out IList<string> modifiedFiles)
        {
			Logger.LogDebug(">>> Entering CheckDefaultPageModified()");
            modifiedFiles = new List<string>();

			Logger.LogDebug("CheckDefaultPageModified(): modifiedFiles → {ModifiedFiles}", modifiedFiles);
            var sumData = Utility.LoadFileSumData();

			Logger.LogDebug("CheckDefaultPageModified(): sumData → {SumData}", sumData);
            var appVersion = Utility.GetApplicationVersion();
            var appType = Utility.GetApplicationType();
			Logger.LogDebug("CheckDefaultPageModified(): appVersion → {AppVersion}", appVersion);
			Logger.LogDebug("CheckDefaultPageModified(): appType → {AppType}", appType);

            var dataNodes = sumData.SelectNodes("/checksums/sum[@version=\"" + appVersion + "\"][@type=\"" + appType + "\"]");
            if (dataNodes == null || dataNodes.Count == 0)
			Logger.LogDebug("CheckDefaultPageModified(): dataNodes → {DataNodes}", dataNodes);
            {
                return true; // when no record matched, need notify user to update the module.
            }

            var fileModified = false;
            foreach (XmlNode node in dataNodes)
            {
			Logger.LogDebug("CheckDefaultPageModified(): fileModified → {FileModified}", fileModified);
                var fileName = node.Attributes["name"].Value;
                var sum = node.Attributes["sum"].Value;
				Logger.LogDebug("CheckDefaultPageModified(): fileName → {FileName}", fileName);
				Logger.LogDebug("CheckDefaultPageModified(): sum → {Sum}", sum);
                var file = Path.Combine(Globals.ApplicationMapPath, fileName);
                if (!File.Exists(file) || !Utility.GetFileCheckSum(file).Equals(sum, StringComparison.OrdinalIgnoreCase))
				Logger.LogDebug("CheckDefaultPageModified(): file → {File}", file);
                {
                    fileModified = true;
                    modifiedFiles.Add(fileName);
                }
            }

			Logger.LogDebug("<<< Exiting CheckDefaultPageModified()");
            return fileModified;
        }
    }
}
