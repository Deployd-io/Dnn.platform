// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Web.Api;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Spa.Services
{
    /// <summary>
    /// The ServiceRouteMapper tells the DNN Web API Framework what routes this module uses
    /// </summary>
    public class ServiceRouteMapper : IServiceRouteMapper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>
        /// RegisterRoutes is used to register the module's routes
        /// </summary>
        /// <param name="mapRouteManager"></param>
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			_logger.LogError(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapHttpRoute("Dnn/ContactList", "default", "{controller}/{action}", new[] { "Dnn.ContactList.Spa.Services" });
			_logger.LogWarning("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
