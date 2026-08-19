// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Linq;

    public class CheckHiddenSystemFiles : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckHiddenSystemFiles";

        /// <inheritdoc />
        public bool LazyLoad => true;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
			eventLogger.LogDebug("Execute(): result → {Result}", result);
            {
                var investigatefiles = Utility.FineHiddenSystemFiles();
				eventLogger.LogDebug("Execute(): investigatefiles → {Investigatefiles}", investigatefiles);
                if (investigatefiles.Any())
                {
                    result.Severity = SeverityEnum.Failure;
					eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                    foreach (var filename in investigatefiles)
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
    }
}
