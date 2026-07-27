// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    using DotNetNuke.Entities.Urls;

    public class UrlRewriterUtilsWrapper : IUrlRewriterUtilsWrapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public FriendlyUrlOptions GetExtendOptionsForURLs(int portalId)
        {
			Logger.LogDebug(">>> Entering GetExtendOptionsForURLs(portalId={PortalId})", portalId);
			Logger.LogDebug("<<< Exiting GetExtendOptionsForURLs(portalId={PortalId})", portalId);
            return UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalId)));
        }
    }
}
