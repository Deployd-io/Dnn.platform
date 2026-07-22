// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Security.Membership;

    public class CheckPasswordFormat : IAuditCheck
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckPasswordFormat";

        /// <inheritdoc />
        public bool LazyLoad => false;

        /// <inheritdoc />
        public CheckResult Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var result = new CheckResult(SeverityEnum.Unverified, this.Id);
            try
			Logger.LogDebug("Execute(): result → {Result}", result);
            {
                var format = MembershipProvider.Instance().PasswordFormat;
                if (format == PasswordFormat.Hashed)
				Logger.LogDebug("Execute(): format → {Format}", format);
                {
                    result.Severity = SeverityEnum.Pass;
					Logger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                }
                else
                {
                    result.Notes.Add("Setting:" + format.ToString());
                    result.Severity = SeverityEnum.Failure;
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
