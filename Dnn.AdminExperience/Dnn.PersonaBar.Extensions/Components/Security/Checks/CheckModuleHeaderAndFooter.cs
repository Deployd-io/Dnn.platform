// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Data;

    public class CheckModuleHeaderAndFooter : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckModuleHeaderAndFooter";

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
				var __logStart = System.Diagnostics.Stopwatch.StartNew();
                var dr = DataProvider.Instance().ExecuteReader("SecurityAnalyzer_GetModulesHasHeaderFooter");
                result.Severity = SeverityEnum.Pass;
				eventLogger.LogError("Execute(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
                while (dr.Read())
                {
					eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
					eventLogger.LogError("Execute(): dr → {Dr}", dr);
                    result.Severity = SeverityEnum.Warning;
                    var note = string.Format(CultureInfo.CurrentCulture, "<b>TabId:</b> {0}, Module Id: {1}", dr["TabId"], dr["ModuleId"]);
                    var headerValue = dr["Header"].ToString();
					eventLogger.LogError("Execute(): note → {Note}", note);
					eventLogger.LogError("Execute(): headerValue → {HeaderValue}", headerValue);
                    var footerValue = dr["Footer"].ToString();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
						eventLogger.LogError("Execute(): footerValue → {FooterValue}", footerValue);
                        note += $"<br />Header: {HttpUtility.HtmlEncode(headerValue)}";
                    }

                    if (!string.IsNullOrEmpty(footerValue))
                    {
                        note += $"<br />Footer: {HttpUtility.HtmlEncode(footerValue)}";
                    }

                    note += "< br />";

                    result.Notes.Add(note);
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
    }
}
