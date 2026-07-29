// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Linq;

    public class CheckHiddenSystemFiles : IAuditCheck
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckHiddenSystemFiles";

        /// <inheritdoc />
        public bool LazyLoad => true;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
			Logger.LogDebug("Execute(): result → {Result}", result);
            {
                var investigatefiles = Utility.FineHiddenSystemFiles();
                if (investigatefiles.Any())
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
            }
            catch (Exception)
            {
				Logger.LogError("Exception in Execute(): {Arg0}", ex.Message);
                throw;
            }

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }
    }
}
