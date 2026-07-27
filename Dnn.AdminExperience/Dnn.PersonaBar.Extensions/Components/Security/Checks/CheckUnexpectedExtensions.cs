// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CheckUnexpectedExtensions : IAuditCheck
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckUnexpectedExtensions";

        /// <inheritdoc />
        public bool LazyLoad => true;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            var invalidFolders = new List<string>();
			Logger.LogDebug("Execute(): result → {Result}", result);
			Logger.LogDebug("Execute(): invalidFolders → {InvalidFolders}", invalidFolders);
            var investigatefiles = Utility.FindUnexpectedExtensions(invalidFolders).ToList();
            if (investigatefiles.Count > 0)
			Logger.LogDebug("Execute(): investigatefiles → {Investigatefiles}", investigatefiles);
            {
                result.Severity = SeverityEnum.Failure;
				Logger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                foreach (var filename in investigatefiles)
                {
                    result.Notes.Add("file:" + filename);
                }
            }
            else
            {
                result.Severity = SeverityEnum.Pass;
            }

            if (invalidFolders.Count > 0)
            {
                var folders = string.Join(string.Empty, invalidFolders.Select(f => $"<p>{f}</p>").ToArray());
                result.Notes.Add($"<p>The following folders are inaccessible due to permission restrictions:</p>{folders}");
				Logger.LogDebug("Execute(): folders → {Folders}", folders);
            }

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }
    }
}
