// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Web.Api;

    /// <summary>The <see cref="IServiceRouteMapper"/> for the Persona Bar API.</summary>
    public class ServiceRouteMapper : IServiceRouteMapper
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void RegisterRoutes(IMapRoute routeManager)
        {
			Logger.LogDebug(">>> Entering RegisterRoutes(routeManager={RouteManager})", routeManager);
            // get all persona bar services from persona bar modules.
            var services = FindPersonaBarServices();

			Logger.LogDebug("RegisterRoutes(routeManager={RouteManager}): services → {Services}", routeManager, services);
            if (services.Count > 0)
            {
                routeManager.MapHttpRoute("PersonaBar", "default", "{controller}/{action}", services.ToArray());
            }
			Logger.LogDebug("<<< Exiting RegisterRoutes(routeManager={RouteManager})", routeManager);
        }

        private static IEnumerable<Type> GetAllApiControllers()
        {
			Logger.LogDebug(">>> Entering GetAllApiControllers()");
            var typeLocator = new TypeLocator();
			Logger.LogDebug("GetAllApiControllers(): typeLocator → {TypeLocator}", typeLocator);
			Logger.LogDebug("<<< Exiting GetAllApiControllers()");
            return typeLocator.GetAllMatchingTypes(
                t => t is { IsClass: true, IsAbstract: false, IsVisible: true, } &&
                     typeof(PersonaBarApiController).IsAssignableFrom(t));
        }

        private static List<string> FindPersonaBarServices()
        {
			Logger.LogDebug(">>> Entering FindPersonaBarServices()");
            var controllerTypes = GetAllApiControllers();
            var namespaces = new List<string>();
			Logger.LogDebug("FindPersonaBarServices(): controllerTypes → {ControllerTypes}", controllerTypes);
			Logger.LogDebug("FindPersonaBarServices(): namespaces → {Namespaces}", namespaces);
            foreach (var type in controllerTypes)
            {
                var scopeAttr = (MenuPermissionAttribute)type.GetCustomAttributes(false).Cast<Attribute>().FirstOrDefault(a => a is MenuPermissionAttribute);
                if (scopeAttr != null)
				Logger.LogDebug("FindPersonaBarServices(): scopeAttr → {ScopeAttr}", scopeAttr);
                {
                    if (!namespaces.Contains(type.Namespace))
                    {
                        namespaces.Add(type.Namespace);
                    }
                }
            }

			Logger.LogDebug("<<< Exiting FindPersonaBarServices()");
            return namespaces;
        }
    }
}
