// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Web;
    using System.Web.UI;

    public class CheckViewstatemac : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckViewstatemac";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogError(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            var page = HttpContext.Current.Handler as Page;
			eventLogger.LogError("Execute(): result → {Result}", result);

            if (page != null)
            {
                if (page.EnableViewStateMac == false)
                {
                    result.Severity = SeverityEnum.Failure;
					eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                }
                else
                {
                    result.Severity = SeverityEnum.Pass;
                }
            }

			eventLogger.LogWarning("<<< Exiting Execute()");
            return result;
        }
    }
}
