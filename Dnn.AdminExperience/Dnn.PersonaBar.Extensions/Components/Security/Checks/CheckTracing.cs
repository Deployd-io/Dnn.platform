// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;

    using DotNetNuke.Common;

    public class CheckTracing : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckTracing";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);

			eventLogger.LogDebug("Execute(): result → {Result}", result);
            result.Severity = EnableTrace() ? SeverityEnum.Failure : SeverityEnum.Pass;

			eventLogger.LogDebug("<<< Exiting Execute()");
            return result;
        }

        private static bool EnableTrace()
        {
			eventLogger.LogDebug(">>> Entering EnableTrace()");
			eventLogger.LogDebug("<<< Exiting EnableTrace()");
            return PageLevelTraceEnabled() || AppLevelTraceEnabled();
        }

        private static bool PageLevelTraceEnabled()
        {
			eventLogger.LogDebug(">>> Entering PageLevelTraceEnabled()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var defaultPagePath = Path.Combine(Globals.ApplicationMapPath, "Default.aspx");
				eventLogger.LogDebug("PageLevelTraceEnabled(): defaultPagePath → {DefaultPagePath}", defaultPagePath);
                using var reader = new StreamReader(File.OpenRead(defaultPagePath));
				eventLogger.LogDebug("PageLevelTraceEnabled(): reader → {Reader}", reader);
                var pageDefine = reader.ReadLine();
				eventLogger.LogDebug("PageLevelTraceEnabled(): pageDefine → {PageDefine}", pageDefine);
                if (!string.IsNullOrEmpty(pageDefine))
                {
                    return pageDefine.ToLowerInvariant().Contains("trace=\"true\"");
                }

                return false;
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in PageLevelTraceEnabled(): {Arg0}", Exception.Message);
				eventLogger.LogInformation("PageLevelTraceEnabled(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
				eventLogger.LogDebug("<<< Exiting PageLevelTraceEnabled()");
                return false;
            }
        }

        private static bool AppLevelTraceEnabled()
        {
			eventLogger.LogDebug(">>> Entering AppLevelTraceEnabled()");
            const string outputCacheSettingsKey = "system.web/trace";
            if (WebConfigurationManager.GetSection(outputCacheSettingsKey) is TraceSection section)
            {
			eventLogger.LogDebug("<<< Exiting AppLevelTraceEnabled()");
                return section.Enabled;
            }

            return false;
        }
    }
}
