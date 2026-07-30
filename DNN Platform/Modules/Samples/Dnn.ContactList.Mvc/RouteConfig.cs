// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Web.Mvc.Routing;

namespace Dnn.ContactList.Mvc
{
    public class RouteConfig : IMvcRouteMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			Logger.LogDebug(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapRoute("ContactList", "ContactList", "{controller}/{action}", new[]
            {"Dnn.ContactList.Mvc.Controllers"});
			Logger.LogDebug("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
