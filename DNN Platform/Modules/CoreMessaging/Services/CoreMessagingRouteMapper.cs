// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    using DotNetNuke.Web.Api;

    /// <summary>Maps the api routes for this module.</summary>
    public sealed class CoreMessagingRouteMapper : IServiceRouteMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CoreMessagingRouteMapper));
        /// <inheritdoc />
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			Logger.LogDebug(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapHttpRoute("CoreMessaging", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.CoreMessaging.Services" });
			Logger.LogDebug("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
