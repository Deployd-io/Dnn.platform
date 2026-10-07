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
			eventLogger.LogError(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);

			eventLogger.LogError("Execute(): result → {Result}", result);
            result.Severity = EnableTrace() ? SeverityEnum.Failure : SeverityEnum.Pass;

			eventLogger.LogWarning("<<< Exiting Execute()");
            return result;
        }

        private static bool EnableTrace()
        {
			eventLogger.LogError(">>> Entering EnableTrace()");
            eventLogger.LogWarning("<<< Exiting EnableTrace()");
            return PageLevelTraceEnabled() || AppLevelTraceEnabled();
        }

        private static bool PageLevelTraceEnabled()
        {
			eventLogger.LogError(">>> Entering PageLevelTraceEnabled()");
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var defaultPagePath = Path.Combine(Globals.ApplicationMapPath, "Default.aspx");
				eventLogger.LogError("PageLevelTraceEnabled(): defaultPagePath → {DefaultPagePath}", defaultPagePath);
                using var reader = new StreamReader(File.OpenRead(defaultPagePath));
				eventLogger.LogError("PageLevelTraceEnabled(): reader → {Reader}", reader);
                var pageDefine = reader.ReadLine();
				eventLogger.LogError("PageLevelTraceEnabled(): pageDefine → {PageDefine}", pageDefine);
                if (!string.IsNullOrEmpty(pageDefine))
                {
                    return pageDefine.ToLowerInvariant().Contains("trace=\"true\"");
                }

                return false;
            }
            catch (Exception)
            {
				eventLogger.LogError("Exception in PageLevelTraceEnabled(): {Arg0}", Exception.Message);
				eventLogger.LogError("PageLevelTraceEnabled(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
				eventLogger.LogWarning("<<< Exiting PageLevelTraceEnabled()");
                return false;
            }
        }

        private static bool AppLevelTraceEnabled()
        {
			eventLogger.LogError(">>> Entering AppLevelTraceEnabled()");
            const string outputCacheSettingsKey = "system.web/trace";
            if (WebConfigurationManager.GetSection(outputCacheSettingsKey) is TraceSection section)
            {
                return section.Enabled;
            }

			eventLogger.LogWarning("<<< Exiting AppLevelTraceEnabled()");
            return false;
        }
    }
}
