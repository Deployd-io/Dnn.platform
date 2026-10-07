// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.AzureConnector.Services
{
    using DotNetNuke.Web.Api;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc />
    public class ServiceRouteMapper : IServiceRouteMapper
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private static readonly string[] Namespaces = ["Dnn.AzureConnector.Services",];

        /// <inheritdoc />
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			_logger.LogError(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapHttpRoute(
                "AzureConnector",
                "default",
                "{controller}/{action}",
                Namespaces);
				_logger.LogWarning("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
