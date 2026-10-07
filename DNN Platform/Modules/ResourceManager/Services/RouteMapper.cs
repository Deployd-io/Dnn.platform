// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services
{
    using DotNetNuke.Web.Api;

    /// <inheritdoc />
    public class RouteMapper : IServiceRouteMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RouteMapper));
        /// <inheritdoc />
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			Logger.LogError(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapHttpRoute("ResourceManager", "default", "{controller}/{action}", new[] { "Dnn.Modules.ResourceManager.Services" });
			Logger.LogWarning("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
