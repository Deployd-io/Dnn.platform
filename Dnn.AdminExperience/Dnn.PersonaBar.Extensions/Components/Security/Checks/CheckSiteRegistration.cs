// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Entities.Portals;

    public class CheckSiteRegistration : IAuditCheck
    {
        /// <inheritdoc />
        public string Id => "CheckSiteRegistration";

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
                var portalController = new PortalController();
				eventLogger.LogDebug("Execute(): portalController → {PortalController}", portalController);
				eventLogger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
                result.Severity = SeverityEnum.Pass;
                foreach (PortalInfo portal in portalController.GetPortals())
                {
                    // check for public registration
                    if (portal.UserRegistration == 2)
                    {
                        result.Severity = SeverityEnum.Warning;
                        result.Notes.Add("Portal:" + portal.PortalName);
                    }
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
