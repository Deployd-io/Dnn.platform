// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Common
{
    using System.Linq;

    using DotNetNuke.Entities.Portals;

    internal class PortalHelper
    {
        internal static bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
        {
			eventLogger.LogDebug(">>> Entering IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup})", portalId, portalIdSiteGroup);
            const int NO_SITE_GROUPID = -1;
            var isSiteGroupPage = false;
			eventLogger.LogDebug("IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup}): isSiteGroupPage → {IsSiteGroupPage}", portalId, portalIdSiteGroup, isSiteGroupPage);
            var portal = PortalController.Instance.GetPortal(portalIdSiteGroup);
			eventLogger.LogDebug("IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup}): portal → {Portal}", portalId, portalIdSiteGroup, portal);

            if (portal.PortalGroupID != NO_SITE_GROUPID)
            {
                isSiteGroupPage = PortalGroupController.Instance.GetPortalsByGroup(portal.PortalGroupID).Any(p => p.PortalID == portalId);
            }

			eventLogger.LogDebug("<<< Exiting IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup})", portalId, portalIdSiteGroup);
            return isSiteGroupPage;
        }
    }
}
