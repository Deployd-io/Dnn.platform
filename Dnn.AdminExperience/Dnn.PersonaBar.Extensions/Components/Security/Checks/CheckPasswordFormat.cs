// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Security.Membership;

    public class CheckPasswordFormat : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckPasswordFormat";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			eventLogger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
			eventLogger.LogDebug("Execute(): result → {Result}", result);
            {
                var format = MembershipProvider.Instance().PasswordFormat;
				eventLogger.LogDebug("Execute(): format → {Format}", format);
                if (format == PasswordFormat.Hashed)
                {
                    result.Severity = SeverityEnum.Pass;
					eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                }
                else
                {
                    result.Notes.Add("Setting:" + format.ToString());
                    result.Severity = SeverityEnum.Failure;
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
