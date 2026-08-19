// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Entities.Host;

    public class CheckAllowableFileExtensions : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckAllowableFileExtensions";

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
                if (Host.AllowedExtensionWhitelist.IsAllowedExtension("asp")
                        || Host.AllowedExtensionWhitelist.IsAllowedExtension("aspx")
                        || Host.AllowedExtensionWhitelist.IsAllowedExtension("php"))
                {
                    result.Severity = SeverityEnum.Failure;
					eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                    result.Notes.Add("Extensions: " + Host.AllowedExtensionWhitelist.ToDisplayString());
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
