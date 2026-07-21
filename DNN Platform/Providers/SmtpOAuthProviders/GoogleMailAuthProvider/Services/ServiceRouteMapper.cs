// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Services;

using DotNetNuke.Web.Api;

/// <inheritdoc />
public class ServiceRouteMapper : IServiceRouteMapper
{
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    /// <inheritdoc />
    public void RegisterRoutes(IMapRoute routeManager)
    {
		Logger.LogDebug(">>> Entering RegisterRoutes(routeManager={RouteManager})", routeManager);
        routeManager.MapHttpRoute(
            "GoogleMailAuth",
            "default",
            "{controller}/{action}",
            new[] { "Dnn.GoogleMailAuthProvider.Services" });
			Logger.LogDebug("<<< Exiting RegisterRoutes(routeManager={RouteManager})", routeManager);
    }
}
