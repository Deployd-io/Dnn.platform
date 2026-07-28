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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckTracing";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);

			Logger.LogDebug("Execute(): result → {Result}", result);
            result.Severity = EnableTrace() ? SeverityEnum.Failure : SeverityEnum.Pass;

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }

        private static bool EnableTrace()
        {
			Logger.LogDebug(">>> Entering EnableTrace()");
			Logger.LogDebug("<<< Exiting EnableTrace()");
            return PageLevelTraceEnabled() || AppLevelTraceEnabled();
        }

        private static bool PageLevelTraceEnabled()
        {
			Logger.LogDebug(">>> Entering PageLevelTraceEnabled()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var defaultPagePath = Path.Combine(Globals.ApplicationMapPath, "Default.aspx");
                using var reader = new StreamReader(File.OpenRead(defaultPagePath));
				Logger.LogDebug("PageLevelTraceEnabled(): defaultPagePath → {DefaultPagePath}", defaultPagePath);
				Logger.LogDebug("PageLevelTraceEnabled(): reader → {Reader}", reader);
                var pageDefine = reader.ReadLine();
                if (!string.IsNullOrEmpty(pageDefine))
				Logger.LogDebug("PageLevelTraceEnabled(): pageDefine → {PageDefine}", pageDefine);
                {
                    return pageDefine.ToLowerInvariant().Contains("trace=\"true\"");
                }

                return false;
            }
            catch (Exception)
            {
				Logger.LogError("Exception in PageLevelTraceEnabled(): {Arg0}", ex.Message);
				Logger.LogInformation("PageLevelTraceEnabled(): external service call took {Arg0} ms", __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting PageLevelTraceEnabled()");
                return false;
            }
        }

        private static bool AppLevelTraceEnabled()
        {
			Logger.LogDebug(">>> Entering AppLevelTraceEnabled()");
            const string outputCacheSettingsKey = "system.web/trace";
            if (WebConfigurationManager.GetSection(outputCacheSettingsKey) is TraceSection section)
            {
                return section.Enabled;
            }

			Logger.LogDebug("<<< Exiting AppLevelTraceEnabled()");
            return false;
        }
    }
}
