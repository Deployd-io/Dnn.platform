// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;

    using DotNetNuke.Entities.Portals;

    public class CheckSiteRegistration : IAuditCheck
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string Id => "CheckSiteRegistration";

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
                var portalController = new PortalController();
                result.Severity = SeverityEnum.Pass;
				Logger.LogDebug("Execute(): portalController → {PortalController}", portalController);
				Logger.LogDebug("Execute(): result.Severity → {Arg0}", result.Severity);
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
				Logger.LogError("Exception in Execute(): {Arg0}", ex.Message);
                throw;
            }

			Logger.LogDebug("<<< Exiting Execute()");
            return result;
        }
    }
}
