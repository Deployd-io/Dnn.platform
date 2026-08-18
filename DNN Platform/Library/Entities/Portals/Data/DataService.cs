// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals.Data
{
    using System.Data;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;

    public class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataService));
        private readonly DataProvider provider = DataProvider.Instance();

        /// <inheritdoc />
        public int AddPortalGroup(PortalGroupInfo portalGroup, int createdByUserId)
        {
			Logger.LogDebug(">>> Entering AddPortalGroup(portalGroup={PortalGroup},createdByUserId={CreatedByUserId})", portalGroup, createdByUserId);
			Logger.LogDebug("<<< Exiting AddPortalGroup(portalGroup={PortalGroup},createdByUserId={CreatedByUserId})", portalGroup, createdByUserId);
            return this.provider.ExecuteScalar<int>(
                "AddPortalGroup",
                portalGroup.PortalGroupName,
                portalGroup.PortalGroupDescription,
                portalGroup.MasterPortalId,
                portalGroup.AuthenticationDomain,
                createdByUserId);
        }

        /// <inheritdoc />
        public void DeletePortalGroup(PortalGroupInfo portalGroup)
        {
			Logger.LogDebug(">>> Entering DeletePortalGroup(portalGroup={PortalGroup})", portalGroup);
            this.provider.ExecuteNonQuery("DeletePortalGroup", portalGroup.PortalGroupId);
			Logger.LogDebug("<<< Exiting DeletePortalGroup(portalGroup={PortalGroup})", portalGroup);
        }

        /// <inheritdoc />
        public IDataReader GetPortalGroups()
        {
			Logger.LogDebug(">>> Entering GetPortalGroups()");
			Logger.LogDebug("<<< Exiting GetPortalGroups()");
            return this.provider.ExecuteReader("GetPortalGroups");
        }

        /// <inheritdoc />
        public void UpdatePortalGroup(PortalGroupInfo portalGroup, int lastModifiedByUserId)
        {
			Logger.LogDebug(">>> Entering UpdatePortalGroup(portalGroup={PortalGroup},lastModifiedByUserId={LastModifiedByUserId})", portalGroup, lastModifiedByUserId);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            this.provider.ExecuteNonQuery(
                "UpdatePortalGroup",
                portalGroup.PortalGroupId,
                portalGroup.PortalGroupName,
                portalGroup.PortalGroupDescription,
                portalGroup.AuthenticationDomain,
                lastModifiedByUserId);
				Logger.LogInformation("UpdatePortalGroup(portalGroup,lastModifiedByUserId)={PortalGroup},{LastModifiedByUserId}: database query executed in {Arg2} ms", portalGroup, lastModifiedByUserId, __logStart.ElapsedMilliseconds);
				Logger.LogDebug("<<< Exiting UpdatePortalGroup(portalGroup={PortalGroup},lastModifiedByUserId={LastModifiedByUserId})", portalGroup, lastModifiedByUserId);
        }

        /// <inheritdoc />
        public IDataReader GetSharedModulesWithPortal(PortalInfo portal)
        {
			Logger.LogDebug(">>> Entering GetSharedModulesWithPortal(portal={Portal})", portal);
			Logger.LogDebug("<<< Exiting GetSharedModulesWithPortal(portal={Portal})", portal);
            return this.provider.ExecuteReader("GetSharedModulesWithPortal", portal.PortalID);
        }

        /// <inheritdoc />
        public IDataReader GetSharedModulesByPortal(PortalInfo portal)
        {
			Logger.LogDebug(">>> Entering GetSharedModulesByPortal(portal={Portal})", portal);
			Logger.LogDebug("<<< Exiting GetSharedModulesByPortal(portal={Portal})", portal);
            return this.provider.ExecuteReader("GetSharedModulesByPortal", portal.PortalID);
        }
    }
}
