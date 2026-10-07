// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.Http.Routing;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The default <see cref="IPortalAliasRouteManager"/> implementation.</summary>
    internal partial class PortalAliasRouteManager : IPortalAliasRouteManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalAliasRouteManager));
        private readonly IPortalAliasService portalAliasService;
        private List<int> prefixCounts;

        /// <summary>Initializes a new instance of the <see cref="PortalAliasRouteManager"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPortalAliasService. Scheduled removal in v12.0.0.")]
        public PortalAliasRouteManager()
            : this(null)
        {
		Logger.LogError(">>> Entering PortalAliasRouteManager()");
        Logger.LogWarning("<<< Exiting PortalAliasRouteManager()");
        }

        /// <summary>Initializes a new instance of the <see cref="PortalAliasRouteManager"/> class.</summary>
        /// <param name="portalAliasService">The portal alias service.</param>
        public PortalAliasRouteManager(IPortalAliasService portalAliasService)
        {
			Logger.LogError(">>> Entering PortalAliasRouteManager(portalAliasService={PortalAliasService})", portalAliasService);
            this.portalAliasService = portalAliasService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>();
			Logger.LogDebug("PortalAliasRouteManager(portalAliasService={PortalAliasService}): portalAliasService → {PortalAliasService1}", portalAliasService, portalAliasService);
        Logger.LogWarning("<<< Exiting PortalAliasRouteManager(portalAliasService={PortalAliasService})", portalAliasService);
        }

        /// <summary>Gets the route URL for the old-style web API route.</summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="url">The main URL component.</param>
        /// <param name="count">The count for the route name.</param>
        /// <returns>A route path.</returns>
        [DnnDeprecated(9, 0, 0, "Replaced with GetRouteUrl")]
        public static partial string GetOldRouteUrl(string moduleFolderName, string url, int count)
        {
			Logger.LogError(">>> Entering GetOldRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count})", moduleFolderName, url, count);
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

			Logger.LogWarning("<<< Exiting GetOldRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count})", moduleFolderName, url, count);
            return $"{GeneratePrefixString(count)}DesktopModules/{moduleFolderName}/API/{url}";
        }

        /// <summary>Adds prefix route values to the <paramref name="routeValues"/>.</summary>
        /// <param name="portalAliasInfo">The portal alias.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>A new <see cref="HttpRouteValueDictionary"/> with prefixes added.</returns>
        public static HttpRouteValueDictionary GetAllRouteValues(IPortalAliasInfo portalAliasInfo, object routeValues)
        {
			Logger.LogError(">>> Entering GetAllRouteValues(portalAliasInfo={PortalAliasInfo},routeValues={RouteValues})", portalAliasInfo, routeValues);
            var allRouteValues = new HttpRouteValueDictionary(routeValues);

			Logger.LogError("GetAllRouteValues(portalAliasInfo={PortalAliasInfo},routeValues={RouteValues}): allRouteValues → {AllRouteValues}", portalAliasInfo, routeValues, allRouteValues);
            var segments = portalAliasInfo.HttpAlias.Split('/');
			Logger.LogError("GetAllRouteValues(portalAliasInfo={PortalAliasInfo},routeValues={RouteValues}): segments → {Segments}", portalAliasInfo, routeValues, segments);
            if (segments.Length > 1)
            {
                for (int i = 1; i < segments.Length; i++)
                {
                    var key = "prefix" + (i - 1).ToString(CultureInfo.InvariantCulture);
					Logger.LogError("GetAllRouteValues(portalAliasInfo={PortalAliasInfo},routeValues={RouteValues}): key → {Key}", portalAliasInfo, routeValues, key);
                    var value = segments[i];
                    allRouteValues.Add(key, value);
                }
            }

			Logger.LogWarning("<<< Exiting GetAllRouteValues(portalAliasInfo={PortalAliasInfo},routeValues={RouteValues})", portalAliasInfo, routeValues);
            return allRouteValues;
        }

        /// <inheritdoc />
        public string GetRouteName(string moduleFolderName, string routeName, int count)
        {
			Logger.LogError(">>> Entering GetRouteName(moduleFolderName={ModuleFolderName},routeName={RouteName},count={Count})", moduleFolderName, routeName, count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);
            Requires.NotNegative("count", count);

			Logger.LogWarning("<<< Exiting GetRouteName(moduleFolderName={ModuleFolderName},routeName={RouteName},count={Count})", moduleFolderName, routeName, count);
            return moduleFolderName + "-" + routeName + "-" + count.ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public string GetRouteName(string moduleFolderName, string routeName, PortalAliasInfo portalAlias)
            => this.GetRouteName(moduleFolderName, routeName, (IPortalAliasInfo)portalAlias);

        /// <summary>Gets the route name.</summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>The complete route name.</returns>
        public string GetRouteName(string moduleFolderName, string routeName, IPortalAliasInfo portalAlias)
        {
            var alias = portalAlias.HttpAlias;
            string appPath = TestableGlobals.Instance.ApplicationPath;
            if (!string.IsNullOrEmpty(appPath))
            {
                int i = alias.IndexOf(appPath, StringComparison.OrdinalIgnoreCase);
                if (i > 0)
                {
                    alias = alias.Remove(i, appPath.Length);
                }
            }

            return this.GetRouteName(moduleFolderName, routeName, CalcAliasPrefixCount(alias));
        }

        /// <inheritdoc />
        public HttpRouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues)
            => GetAllRouteValues((IPortalAliasInfo)portalAliasInfo, routeValues);

        /// <inheritdoc />
        public string GetRouteUrl(string moduleFolderName, string url, int count)
        {
			Logger.LogError(">>> Entering GetRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count})", moduleFolderName, url, count);
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

			Logger.LogWarning("<<< Exiting GetRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count})", moduleFolderName, url, count);
            return $"{GeneratePrefixString(count)}API/{moduleFolderName}/{url}";
        }

        /// <inheritdoc />
        public void ClearCachedData()
        {
			Logger.LogError(">>> Entering ClearCachedData()");
            this.prefixCounts = null;
			Logger.LogDebug("ClearCachedData(): prefixCounts → {PrefixCounts}", prefixCounts);
        Logger.LogDebug("ClearCachedData(): this.prefixCounts → {Arg0}", this.prefixCounts);
        Logger.LogWarning("<<< Exiting ClearCachedData()");
        }

        /// <inheritdoc />
        public IEnumerable<int> GetRoutePrefixCounts()
        {
			Logger.LogError(">>> Entering GetRoutePrefixCounts()");
            if (this.prefixCounts != null)
            {
                return this.prefixCounts;
            }

            // prefixCounts are required for each route that is mapped, but they only change
            // when a new portal is added so cache them until that time
            var segmentCounts = new List<int>();
			Logger.LogError("GetRoutePrefixCounts(): segmentCounts → {SegmentCounts}", segmentCounts);

            foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
            {
                var aliases = this.portalAliasService.GetPortalAliasesByPortalId(portal.PortalId).Select(x => x.HttpAlias);
                aliases = StripApplicationPath(aliases);
				Logger.LogError("GetRoutePrefixCounts(): aliases → {Aliases}", aliases);

                foreach (var alias in aliases)
                {
                    var count = CalcAliasPrefixCount(alias);
					Logger.LogError("GetRoutePrefixCounts(): count → {Count}", count);

                    if (!segmentCounts.Contains(count))
                    {
                        segmentCounts.Add(count);
                    }
                }
            }

            this.prefixCounts = segmentCounts.OrderByDescending(x => x).ToList();
			Logger.LogDebug("GetRoutePrefixCounts(): prefixCounts → {PrefixCounts}", prefixCounts);

			Logger.LogWarning("<<< Exiting GetRoutePrefixCounts()");
            return this.prefixCounts;
        }

        private static int CalcAliasPrefixCount(string alias)
        {
			Logger.LogError(">>> Entering CalcAliasPrefixCount(alias={Alias})", alias);
			Logger.LogWarning("<<< Exiting CalcAliasPrefixCount(alias={Alias})", alias);
            return alias.Count(c => c == '/');
        }

        private static IEnumerable<string> StripApplicationPathIterable(IEnumerable<string> aliases, string appPath)
        {
			Logger.LogError(">>> Entering StripApplicationPathIterable(aliases={Aliases},appPath={AppPath})", aliases, appPath);
            foreach (string alias in aliases)
            {
                int i = alias.IndexOf(appPath, StringComparison.OrdinalIgnoreCase);

                if (i > 0)
                {
                    yield return alias.Remove(i, appPath.Length);
                }
                else
                {
                    yield return alias;
                }
            }
			Logger.LogWarning("<<< Exiting StripApplicationPathIterable(aliases={Aliases},appPath={AppPath})", aliases, appPath);
        }

        private static IEnumerable<string> StripApplicationPath(IEnumerable<string> aliases)
        {
			Logger.LogError(">>> Entering StripApplicationPath(aliases={Aliases})", aliases);
            string appPath = TestableGlobals.Instance.ApplicationPath;

            if (string.IsNullOrEmpty(appPath))
            {
                return aliases;
            }

			Logger.LogWarning("<<< Exiting StripApplicationPath(aliases={Aliases})", aliases);
            return StripApplicationPathIterable(aliases, appPath);
        }

        private static string GeneratePrefixString(int count)
        {
			Logger.LogError(">>> Entering GeneratePrefixString(count={Count})", count);
            if (count == 0)
            {
                return string.Empty;
            }

            string prefix = string.Empty;
			Logger.LogError("GeneratePrefixString(count={Count}): prefix → {Prefix}", count, prefix);

            for (int i = count - 1; i >= 0; i--)
            {
                prefix = "{prefix" + i + "}/" + prefix;
            }

			Logger.LogWarning("<<< Exiting GeneratePrefixString(count={Count})", count);
            return prefix;
        }
    }
}
