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
        public static void SetupCachingProvider(Mock<CachingProvider> mockCachingProvider)
        {
			mockEventLogger.LogDebug(">>> Entering SetupCachingProvider(mockCachingProvider={MockCachingProvider})", mockCachingProvider);
            mockCachingProvider.Setup(c => c.GetItem(It.IsAny<string>()))
                .Returns<string>(key =>
                {
                    if (key.Contains("Portal-1_"))
                    {
                        var portals = new List<PortalInfo> { new PortalInfo() { PortalID = 0 } };
						mockEventLogger.LogDebug("SetupCachingProvider(mockCachingProvider={MockCachingProvider}): portals → {Portals}", mockCachingProvider, portals);

                        return portals;
                    }

				mockEventLogger.LogDebug("<<< Exiting SetupCachingProvider(mockCachingProvider={MockCachingProvider})", mockCachingProvider);
                    return key.Contains("PortalGroups") ? new List<PortalGroupInfo>() : null;
                });
        }
    }
}
