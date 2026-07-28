// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Helpers
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using Moq;

    public class SetupCachingProviderHelper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static void SetupCachingProvider(Mock<CachingProvider> mockCachingProvider)
        {
			Logger.LogDebug(">>> Entering SetupCachingProvider(mockCachingProvider={MockCachingProvider})", mockCachingProvider);
            mockCachingProvider.Setup(c => c.GetItem(It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    if (key.Contains("Portal-1_"))
                    {
                        var portals = new List<PortalInfo> { new PortalInfo() { PortalID = 0 } };

						Logger.LogDebug("SetupCachingProvider(mockCachingProvider={MockCachingProvider}): portals → {Portals}", mockCachingProvider, portals);
                        return portals;
                    }

                    return key.Contains("PortalGroups") ? new List<PortalGroupInfo>() : null;
                });
				Logger.LogDebug("<<< Exiting SetupCachingProvider(mockCachingProvider={MockCachingProvider})", mockCachingProvider);
        }
    }
}
