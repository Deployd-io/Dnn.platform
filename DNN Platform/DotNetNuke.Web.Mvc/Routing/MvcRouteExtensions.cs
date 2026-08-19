// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Routing;

    public static class MvcRouteExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MvcRouteExtensions));
        private const string NamespaceKey = "namespaces";
        private const string NameKey = "name";

        /// <summary>Get the name of the route.</summary>
        /// <param name="route">The route.</param>
        /// <returns>Route name.</returns>
        public static string GetName(this Route route)
        {
			Logger.LogDebug(">>> Entering GetName(route={Route})", route);
			Logger.LogDebug("<<< Exiting GetName(route={Route})", route);
            return (string)route.DataTokens[NameKey];
        }

        internal static void SetNameSpaces(this Route route, string[] namespaces)
        {
			Logger.LogDebug(">>> Entering SetNameSpaces(route={Route},namespaces={Namespaces})", route, namespaces);
            route.DataTokens[NamespaceKey] = namespaces;
			Logger.LogDebug("<<< Exiting SetNameSpaces(route={Route},namespaces={Namespaces})", route, namespaces);
        }

        /// <summary>Get Namespaces that are searched for controllers for this route.</summary>
        /// <param name="route">The route.</param>
        /// <returns>Namespaces.</returns>
        internal static string[] GetNameSpaces(this Route route)
        {
			Logger.LogDebug(">>> Entering GetNameSpaces(route={Route})", route);
			Logger.LogDebug("<<< Exiting GetNameSpaces(route={Route})", route);
            return (string[])route.DataTokens[NamespaceKey];
        }

        internal static void SetName(this Route route, string name)
        {
			Logger.LogDebug(">>> Entering SetName(route={Route},name={Name})", route, name);
            route.DataTokens[NameKey] = name;
			Logger.LogDebug("<<< Exiting SetName(route={Route},name={Name})", route, name);
        }
    }
}
