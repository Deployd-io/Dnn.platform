// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Entities.Portals;

    public class PortalAliasHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static PortalAliasInfo GetPrimaryPortalAlias(int portalId = 0)
        {
			Logger.LogDebug(">>> Entering GetPrimaryPortalAlias(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetPrimaryPortalAlias(portalId={PortalId})", portalId);
            return DatabaseHelper.ExecuteStoredProcedure<PortalAliasInfo>("GetPortalAliases").First(p => p.PortalID == portalId && p.IsPrimary);
        }

        public static IEnumerable<PortalAliasInfo> GetPortalAliaes(int portalId = 0)
        {
			Logger.LogDebug(">>> Entering GetPortalAliaes(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetPortalAliaes(portalId={PortalId})", portalId);
            return DatabaseHelper.ExecuteStoredProcedure<PortalAliasInfo>("GetPortalAliases").Where(p => p.PortalID == portalId);
        }

        public static IEnumerable<PortalAliasInfo> GetAllPortalAliaes()
        {
			Logger.LogDebug(">>> Entering GetAllPortalAliaes()");
			Logger.LogDebug("<<< Exiting GetAllPortalAliaes()");
            return DatabaseHelper.ExecuteStoredProcedure<PortalAliasInfo>("GetPortalAliases");
        }

        public static void AddPortalAlias(PortalAliasInfo portal)
        {
			Logger.LogDebug(">>> Entering AddPortalAlias(portal={Portal})", portal);
            DatabaseHelper.ExecuteStoredProcedure(
                "AddPortalAlias",
                portal.PortalID,
                portal.HTTPAlias,
                portal.CultureCode,
                portal.Skin,
                portal.BrowserType,
                portal.IsPrimary,
                -1);
				Logger.LogDebug("<<< Exiting AddPortalAlias(portal={Portal})", portal);
        }
    }
}
