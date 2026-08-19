// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExchangeOnlineAuthProvider.Services;

using DotNetNuke.Web.Api;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
public class ServiceRouteMapper : IServiceRouteMapper
{
	private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    /// <inheritdoc />
    public void RegisterRoutes(IMapRoute routeManager)
    {
		_logger.LogDebug(">>> Entering RegisterRoutes(routeManager={RouteManager})", routeManager);
        routeManager.MapHttpRoute(
            "ExchangeOnlineAuth",
            "default",
            "{controller}/{action}",
            new[] { "Dnn.ExchangeOnlineAuthProvider.Services" });
			_logger.LogDebug("<<< Exiting RegisterRoutes(routeManager={RouteManager})", routeManager);
    }
}
