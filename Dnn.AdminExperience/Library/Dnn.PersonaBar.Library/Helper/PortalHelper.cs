// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Helper
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>A helper for portals.</summary>
    public partial class PortalHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalHelper));
        private static readonly ContentVerifier ContentVerifier = new ContentVerifier();

        /// <inheritdoc cref="IContentVerifier.IsContentExistsForRequestedPortal"/>
#pragma warning disable CS1066
        [DnnDeprecated(9, 2, 1, "Use IContentVerifier.IsContentExistsForRequestedPortal")]
        public static partial bool IsContentExistsForRequestedPortal(int contentPortalId, PortalSettings portalSettings, bool checkForSiteGroup = false)
        {
			Logger.LogDebug(">>> Entering IsContentExistsForRequestedPortal(contentPortalId={ContentPortalId},portalSettings={PortalSettings},checkForSiteGroup={CheckForSiteGroup})", contentPortalId, portalSettings, checkForSiteGroup);
			Logger.LogDebug("<<< Exiting IsContentExistsForRequestedPortal(contentPortalId={ContentPortalId},portalSettings={PortalSettings},checkForSiteGroup={CheckForSiteGroup})", contentPortalId, portalSettings, checkForSiteGroup);
            return ContentVerifier.IsContentExistsForRequestedPortal(contentPortalId, portalSettings, checkForSiteGroup);
        }
#pragma warning restore CS1066
    }
}
