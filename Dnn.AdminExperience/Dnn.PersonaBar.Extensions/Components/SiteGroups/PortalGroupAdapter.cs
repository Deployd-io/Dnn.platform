// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteGroups
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    using PortalGroupInfo = Dnn.PersonaBar.SiteGroups.Models.PortalGroupInfo;
    using PortalInfo = Dnn.PersonaBar.SiteGroups.Models.PortalInfo;

    public class PortalGroupAdapter : IManagePortalGroups
    {
        private static IPortalGroupController PortalGroupController => DotNetNuke.Entities.Portals.PortalGroupController.Instance;

        /// <inheritdoc />
        public IEnumerable<PortalGroupInfo> SiteGroups()
        {
			eventLogger.LogDebug(">>> Entering SiteGroups()");
			eventLogger.LogDebug("<<< Exiting SiteGroups()");
            return PortalGroupController.GetPortalGroups().Select(g => new PortalGroupInfo
            {
                AuthenticationDomain = g.AuthenticationDomain,
                PortalGroupId = g.PortalGroupId,
                Description = g.PortalGroupDescription,
                MasterPortal = new PortalInfo
                {
                    PortalName = g.MasterPortalName,
                    PortalId = g.MasterPortalId,
                },
                PortalGroupName = g.PortalGroupName,
                Portals = PortalsOfGroup(g.PortalGroupId, g.MasterPortalId)
                        .Select(p => new PortalInfo
                        {
                            PortalId = p.PortalID,
                            PortalName = p.PortalName,
                        }),
            });
        }

        /// <inheritdoc />
        public IEnumerable<PortalInfo> AvailablePortals()
        {
			eventLogger.LogDebug(">>> Entering AvailablePortals()");
			eventLogger.LogDebug("<<< Exiting AvailablePortals()");
            return new PortalController().GetPortals()
                        .Cast<DotNetNuke.Entities.Portals.PortalInfo>()
                        .Where(x => x.PortalGroupID == Null.NullInteger)
                        .Select(p => new PortalInfo
                        {
                            PortalId = p.PortalID,
                            PortalName = p.PortalName,
                        });
        }

        /// <inheritdoc />
        public int Save(PortalGroupInfo portalGroup)
        {
			eventLogger.LogDebug(">>> Entering Save(portalGroup={PortalGroup})", portalGroup);
            if (portalGroup.PortalGroupId == -1)
            {
                return AddPortalGroup(portalGroup);
            }
            else
            {
				eventLogger.LogDebug("<<< Exiting Save(portalGroup={PortalGroup})", portalGroup);
                return UpdatePortalGroup(portalGroup);
            }
        }

        /// <inheritdoc />
        public void Delete(int portalGroupId)
        {
			eventLogger.LogDebug(">>> Entering Delete(portalGroupId={PortalGroupId})", portalGroupId);
            var group = PortalGroupController.GetPortalGroups().Single(g => g.PortalGroupId == portalGroupId);
            PortalGroupController.DeletePortalGroup(group);
			eventLogger.LogDebug("Delete(portalGroupId={PortalGroupId}): group → {Group}", portalGroupId, group);
			eventLogger.LogDebug("<<< Exiting Delete(portalGroupId={PortalGroupId})", portalGroupId);
        }

        private static IEnumerable<DotNetNuke.Entities.Portals.PortalInfo> PortalsOfGroup(int groupId, int masterPortalId)
        {
			eventLogger.LogDebug(">>> Entering PortalsOfGroup(groupId={GroupId},masterPortalId={MasterPortalId})", groupId, masterPortalId);
            return PortalGroupController
                .GetPortalsByGroup(groupId)
                .Where(x => x.PortalID != masterPortalId);
				eventLogger.LogDebug("<<< Exiting PortalsOfGroup(groupId={GroupId},masterPortalId={MasterPortalId})", groupId, masterPortalId);
        }

        private static int UpdatePortalGroup(PortalGroupInfo portalGroup)
        {
			eventLogger.LogDebug(">>> Entering UpdatePortalGroup(portalGroup={PortalGroup})", portalGroup);
            UserCopiedCallback callback = e => { };
            var @group = PortalGroupController.GetPortalGroups().Single(g => g.PortalGroupId == portalGroup.PortalGroupId);
            @group.PortalGroupName = portalGroup.PortalGroupName;
            @group.AuthenticationDomain = portalGroup.AuthenticationDomain;
            @group.PortalGroupDescription = portalGroup.Description;
            PortalGroupController.UpdatePortalGroup(@group);
            var currentPortals = PortalsOfGroup(portalGroup.PortalGroupId, portalGroup.MasterPortal.PortalId).ToList();
			eventLogger.LogDebug("UpdatePortalGroup(portalGroup={PortalGroup}): currentPortals → {CurrentPortals}", portalGroup, currentPortals);
            foreach (var portal in currentPortals)
            {
                if (portalGroup.Portals == null || portalGroup.Portals.All(p => p.PortalId != portal.PortalID))
                {
                    PortalGroupController.RemovePortalFromGroup(portal, @group, false, callback);
                }
            }

            if (portalGroup.Portals != null)
            {
                foreach (var portal in portalGroup.Portals)
                {
                    if (currentPortals.All(p => p.PortalID != portal.PortalId))
                    {
                        var p = new PortalController().GetPortal(portal.PortalId);
						eventLogger.LogDebug("UpdatePortalGroup(portalGroup={PortalGroup}): p → {P}", portalGroup, p);
                        PortalGroupController.AddPortalToGroup(p, @group, callback);
                    }
                }
            }

			eventLogger.LogDebug("<<< Exiting UpdatePortalGroup(portalGroup={PortalGroup})", portalGroup);
            return @group.PortalGroupId;
        }

        private static int AddPortalGroup(PortalGroupInfo portalGroup)
        {
			eventLogger.LogDebug(">>> Entering AddPortalGroup(portalGroup={PortalGroup})", portalGroup);
            UserCopiedCallback callback = e => { };
            var group = new DotNetNuke.Entities.Portals.PortalGroupInfo
            {
                AuthenticationDomain = portalGroup.AuthenticationDomain,
                MasterPortalId = portalGroup.MasterPortal.PortalId,
                PortalGroupDescription = portalGroup.Description,
                PortalGroupName = portalGroup.PortalGroupName,
            };
            PortalGroupController.AddPortalGroup(@group);
            if (portalGroup.Portals != null)
            {
                foreach (var portal in portalGroup.Portals)
                {
                    var p = new PortalController().GetPortal(portal.PortalId);
					eventLogger.LogDebug("AddPortalGroup(portalGroup={PortalGroup}): p → {P}", portalGroup, p);
                    PortalGroupController.AddPortalToGroup(p, @group, callback);
                }
            }

			eventLogger.LogDebug("<<< Exiting AddPortalGroup(portalGroup={PortalGroup})", portalGroup);
            return @group.PortalGroupId;
        }
    }
}
