// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Routing
{
    using System.Web;
    using System.Web.Routing;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;

    public abstract class ModuleRoutingProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static ModuleRoutingProvider Instance()
        {
			Logger.LogDebug(">>> Entering Instance()");
            var component = ComponentFactory.GetComponent<ModuleRoutingProvider>();

			Logger.LogDebug("Instance(): component → {Component}", component);
            if (component == null)
            {
                component = new StandardModuleRoutingProvider();
                ComponentFactory.RegisterComponentInstance<ModuleRoutingProvider>(component);
            }

			Logger.LogDebug("<<< Exiting Instance()");
            return component;
        }

        public abstract string GenerateUrl(RouteValueDictionary routeValues, ModuleInstanceContext moduleContext);

        public abstract RouteData GetRouteData(HttpContextBase httpContext, ModuleControlInfo moduleControl);
    }
}
