// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Web.Mvc.Routing;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Mvc
{
    public class RouteConfig : IMvcRouteMapper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			_logger.LogDebug(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapRoute("ContactList", "ContactList", "{controller}/{action}", new[]
            {"Dnn.ContactList.Mvc.Controllers"});
			_logger.LogDebug("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
