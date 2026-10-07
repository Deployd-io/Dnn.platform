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
        /// <inheritdoc />
        public string Id => "CheckUnexpectedExtensions";

        /// <inheritdoc />
        public bool LazyLoad => true;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogError(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            var invalidFolders = new List<string>();
			eventLogger.LogError("Execute(): invalidFolders → {InvalidFolders}", invalidFolders);
			eventLogger.LogError("Execute(): result → {Result}", result);
            var investigatefiles = Utility.FindUnexpectedExtensions(invalidFolders).ToList();
            if (investigatefiles.Count > 0)
            {
				eventLogger.LogError("Execute(): investigatefiles → {Investigatefiles}", investigatefiles);
				eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                result.Severity = SeverityEnum.Failure;
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
				eventLogger.LogError("Execute(): folders → {Folders}", folders);
                result.Notes.Add($"<p>The following folders are inaccessible due to permission restrictions:</p>{folders}");
            }

			eventLogger.LogWarning("<<< Exiting Execute()");
            return result;
        }
    }
}
