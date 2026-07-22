// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    using System;

    using DotNetNuke.Web.Api;

    public class JournalRouteMapper : IServiceRouteMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
			Logger.LogDebug(">>> Entering RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
            mapRouteManager.MapHttpRoute("Journal", "default", "{controller}/{action}", new[] { "DotNetNuke.Modules.Journal" });
			Logger.LogDebug("<<< Exiting RegisterRoutes(mapRouteManager={MapRouteManager})", mapRouteManager);
        }
    }
}
