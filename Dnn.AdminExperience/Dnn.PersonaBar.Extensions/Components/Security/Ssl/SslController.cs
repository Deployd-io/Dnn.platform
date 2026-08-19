// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Security.Ssl
{
    using System;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    public class SslController : ServiceLocator<ISSlController, SslController>, ISSlController
    {
        /// <inheritdoc />
        public PortalStats GetPortalStats(int portalId)
        {
			eventLogger.LogDebug(">>> Entering GetPortalStats(portalId={PortalId})", portalId);
            using (IDataContext context = DataContext.Instance())
            {
				eventLogger.LogDebug("<<< Exiting GetPortalStats(portalId={PortalId})", portalId);
                return context.ExecuteQuery<PortalStats>(System.Data.CommandType.StoredProcedure, "{databaseOwner}{objectQualifier}PersonaBar_GetPortalStats", portalId).FirstOrDefault();
            }
        }

        protected override Func<ISSlController> GetFactory()
        {
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => new SslController();
        }
    }
}
