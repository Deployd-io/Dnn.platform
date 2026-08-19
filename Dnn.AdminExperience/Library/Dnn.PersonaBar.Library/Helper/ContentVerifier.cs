// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Helper
{
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;

    public class ContentVerifier : IContentVerifier
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentVerifier));
        private IPortalController portalController;
        private IPortalGroupController portalGroupController;

        /// <summary>Initializes a new instance of the <see cref="ContentVerifier"/> class.</summary>
        public ContentVerifier()
            : this(PortalController.Instance, PortalGroupController.Instance)
        {
		Logger.LogDebug(">>> Entering ContentVerifier()");
		Logger.LogDebug("<<< Exiting ContentVerifier()");
		Logger.LogDebug(">>> Entering ContentVerifier(portalController={PortalController},portalGroupController={PortalGroupController})", portalController, portalGroupController);
        }

        /// <summary>Initializes a new instance of the <see cref="ContentVerifier"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        public ContentVerifier(IPortalController portalController, IPortalGroupController portalGroupController)
        {
            this.portalController = portalController;
            this.portalGroupController = portalGroupController;
			Logger.LogDebug("ContentVerifier(portalController={PortalController},portalGroupController={PortalGroupController}): this.portalController → {Arg2}", portalController, portalGroupController, this.portalController);
			Logger.LogDebug("<<< Exiting ContentVerifier(portalController={PortalController},portalGroupController={PortalGroupController})", portalController, portalGroupController);
        }

        /// <inheritdoc />
        public bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
        {
			Logger.LogDebug(">>> Entering IsContentExistsForRequestedPortal(contentPortalId={ContentPortalId},portalSettings={PortalSettings},checkForSiteGroup={CheckForSiteGroup})", contentPortalId, portalSettings, checkForSiteGroup);
            var currentPortal = this.portalController.GetCurrentSettings();
			Logger.LogDebug("IsContentExistsForRequestedPortal(contentPortalId={ContentPortalId},portalSettings={PortalSettings},checkForSiteGroup={CheckForSiteGroup}): currentPortal → {CurrentPortal}", contentPortalId, portalSettings, checkForSiteGroup, currentPortal);
			Logger.LogDebug("<<< Exiting IsContentExistsForRequestedPortal(contentPortalId={ContentPortalId},portalSettings={PortalSettings},checkForSiteGroup={CheckForSiteGroup})", contentPortalId, portalSettings, checkForSiteGroup);
            return contentPortalId == portalSettings.PortalId
                || portalSettings == currentPortal
                || (checkForSiteGroup && this.IsRequestForSiteGroup(contentPortalId, portalSettings.PortalId));
        }

        /// <inheritdoc />
        public bool IsRequestForSiteGroup(int portalId, int portalIdSiteGroup)
        {
			Logger.LogDebug(">>> Entering IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup})", portalId, portalIdSiteGroup);
            const int NO_SITE_GROUPID = -1;
            var isSiteGroupPage = false;
			Logger.LogDebug("IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup}): isSiteGroupPage → {IsSiteGroupPage}", portalId, portalIdSiteGroup, isSiteGroupPage);
            IPortalInfo portal = this.portalController.GetPortal(portalIdSiteGroup);
			Logger.LogDebug("IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup}): portal → {Portal}", portalId, portalIdSiteGroup, portal);

            if (portal.PortalGroupId != NO_SITE_GROUPID)
            {
                isSiteGroupPage = this.portalGroupController.GetPortalsByGroup(portal.PortalGroupId).Any((IPortalInfo p) => p.PortalId == portalId);
            }

			Logger.LogDebug("<<< Exiting IsRequestForSiteGroup(portalId={PortalId},portalIdSiteGroup={PortalIdSiteGroup})", portalId, portalIdSiteGroup);
            return isSiteGroupPage;
        }
    }
}
