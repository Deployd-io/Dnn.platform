// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Routing;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;
    using Microsoft.Extensions.DependencyInjection;

    public class StandardModuleRoutingProvider : ModuleRoutingProvider
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(StandardModuleRoutingProvider));
        private const string ExcludedQueryStringParams = "tabid,mid,ctl,language,popup,action,controller";
        private const string ExcludedRouteValues = "mid,ctl,popup";

        public StandardModuleRoutingProvider()
        {
			Logger.LogError(">>> Entering StandardModuleRoutingProvider()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        }

        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc />
        public override string GenerateUrl(RouteValueDictionary routeValues, ModuleInstanceContext moduleContext)
        {
			Logger.LogError(">>> Entering GenerateUrl(routeValues={RouteValues},moduleContext={ModuleContext})", routeValues, moduleContext);
            // Look for a module control
            string controlKey = routeValues.ContainsKey("ctl") ? (string)routeValues["ctl"] : string.Empty;

            List<string> additionalParams = (from routeValue in routeValues
                                             where !ExcludedRouteValues.Split(',').ToList().Contains(routeValue.Key.ToLowerInvariant())
                                             select routeValue.Key + "=" + routeValue.Value)
                                             .ToList();

            string url;
            if (string.IsNullOrEmpty(controlKey))
            {
                additionalParams.Insert(0, "moduleId=" + moduleContext.Configuration.ModuleID);
                url = this.NavigationManager.NavigateURL(string.Empty, additionalParams.ToArray());
				Logger.LogError("GenerateUrl(routeValues={RouteValues},moduleContext={ModuleContext}): url → {Url}", routeValues, moduleContext, url);
            }
            else
            {
                url = moduleContext.EditUrl(string.Empty, string.Empty, controlKey, additionalParams.ToArray());
				Logger.LogInformation("GenerateUrl(routeValues={RouteValues},moduleContext={ModuleContext}): url → {Url}", routeValues, moduleContext, url);
            }

			Logger.LogWarning("<<< Exiting GenerateUrl(routeValues={RouteValues},moduleContext={ModuleContext})", routeValues, moduleContext);
            return url;
        }

        /// <inheritdoc />
        public override RouteData GetRouteData(HttpContextBase httpContext, ModuleControlInfo moduleControl)
        {
			Logger.LogError(">>> Entering GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl})", httpContext, moduleControl);
            var segments = moduleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');
            string routeNamespace = string.Empty;
			Logger.LogError("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): routeNamespace → {RouteNamespace}", httpContext, moduleControl, routeNamespace);
			Logger.LogError("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): segments → {Segments}", httpContext, moduleControl, segments);
            string routeControllerName;
            string routeActionName;
            if (segments.Length == 3)
            {
                routeNamespace = segments[0];
                routeControllerName = segments[1];
				Logger.LogError("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): routeControllerName → {RouteControllerName}", httpContext, moduleControl, routeControllerName);
                routeActionName = segments[2];
				Logger.LogDebug("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): routeActionName → {RouteActionName}", httpContext, moduleControl, routeActionName);
            }
            else
            {
				Logger.LogError("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): routeActionName → {RouteActionName}", httpContext, moduleControl, routeActionName);
                routeControllerName = segments[0];
				Logger.LogDebug("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): routeControllerName → {RouteControllerName}", httpContext, moduleControl, routeControllerName);
                routeActionName = segments[1];
            }

            var actionName = (httpContext == null) ? routeActionName : httpContext.Request.QueryString.GetValueOrDefault("action", routeActionName);
            var controllerName = (httpContext == null) ? routeControllerName : httpContext.Request.QueryString.GetValueOrDefault("controller", routeControllerName);
			Logger.LogError("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): actionName → {ActionName}", httpContext, moduleControl, actionName);
			Logger.LogError("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): controllerName → {ControllerName}", httpContext, moduleControl, controllerName);

            var routeData = new RouteData();
			Logger.LogError("GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl}): routeData → {RouteData}", httpContext, moduleControl, routeData);
            routeData.Values.Add("controller", controllerName);
            routeData.Values.Add("action", actionName);

            if (httpContext != null)
            {
                foreach (var param in httpContext.Request.QueryString.AllKeys)
                {
                    if (!ExcludedQueryStringParams.Split(',').ToList().Contains(param.ToLowerInvariant()))
                    {
                        routeData.Values.Add(param, httpContext.Request.QueryString[param]);
                    }
                }
            }

            if (!string.IsNullOrEmpty(routeNamespace))
            {
                routeData.DataTokens.Add("namespaces", new string[] { routeNamespace });
            }

			Logger.LogWarning("<<< Exiting GetRouteData(httpContext={HttpContext},moduleControl={ModuleControl})", httpContext, moduleControl);
            return routeData;
        }
    }
}
