// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.Routing;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Gets MVC routes.</summary>
    internal partial class PortalAliasMvcRouteManager : IPortalAliasMvcRouteManager
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IPortalAliasService portalAliasService;
        private List<int> prefixCounts;

        public PortalAliasMvcRouteManager(IPortalAliasService portalAliasService)
        {
			Logger.LogDebug(">>> Entering PortalAliasMvcRouteManager(portalAliasService={PortalAliasService})", portalAliasService);
            this.portalAliasService = portalAliasService;
			Logger.LogDebug("<<< Exiting PortalAliasMvcRouteManager(portalAliasService={PortalAliasService})", portalAliasService);
			Logger.LogDebug("PortalAliasMvcRouteManager(portalAliasService={PortalAliasService}): portalAliasService → {PortalAliasService1}", portalAliasService, portalAliasService);
			Logger.LogDebug("PortalAliasMvcRouteManager(portalAliasService={PortalAliasService}): this.portalAliasService → {Arg1}", portalAliasService, this.portalAliasService);
        }

        /// <inheritdoc />
        public string GetRouteName(string moduleFolderName, string routeName, int count)
        {
			Logger.LogDebug(">>> Entering GetRouteName(moduleFolderName={ModuleFolderName},routeName={RouteName},count={Count})", moduleFolderName, routeName, count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);
            Requires.NotNegative("count", count);

			Logger.LogDebug("<<< Exiting GetRouteName(moduleFolderName={ModuleFolderName},routeName={RouteName},count={Count})", moduleFolderName, routeName, count);
            return moduleFolderName + "-" + routeName + "-" + count.ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc cref="IPortalAliasMvcRouteManager.GetRouteName(string,string,int)"/>
        [DnnDeprecated(9, 10, 0, "Use overload taking DotNetNuke.Abstractions.Portals.IPortalAliasInfo instead")]
        public partial string GetRouteName(string moduleFolderName, string routeName, PortalAliasInfo portalAlias)
        {
			Logger.LogDebug("<<< Exiting GetRouteName(moduleFolderName={ModuleFolderName},routeName={RouteName},portalAlias={PortalAlias})", moduleFolderName, routeName, portalAlias);
            return this.GetRouteName(moduleFolderName, routeName, (IPortalAliasInfo)portalAlias);
        }

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

        /// <inheritdoc cref="IPortalAliasMvcRouteManager.GetAllRouteValues"/>
        [DnnDeprecated(9, 10, 0, "Use overload taking DotNetNuke.Abstractions.Portals.IPortalAliasInfo instead")]
        public partial RouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues)
        {
			Logger.LogDebug("<<< Exiting GetAllRouteValues(portalAliasInfo={PortalAliasInfo},routeValues={RouteValues})", portalAliasInfo, routeValues);
            return this.GetAllRouteValues((IPortalAliasInfo)portalAliasInfo, routeValues);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public RouteValueDictionary GetAllRouteValues(IPortalAliasInfo portalAliasInfo, object routeValues)
        {
            var allRouteValues = new RouteValueDictionary(routeValues);

            var segments = portalAliasInfo.HttpAlias.Split('/');

            if (segments.Length <= 1)
            {
                return allRouteValues;
            }

            for (var i = 1; i < segments.Length; i++)
            {
                var key = "prefix" + (i - 1).ToString(CultureInfo.InvariantCulture);
                var value = segments[i];
                allRouteValues.Add(key, value);
            }

            return allRouteValues;
        }

        /// <inheritdoc />
        public string GetRouteUrl(string moduleFolderName, string url, int count)
        {
			Logger.LogDebug(">>> Entering GetRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count})", moduleFolderName, url, count);
            Requires.NotNegative("count", count);
            Requires.NotNullOrEmpty("moduleFolderName", moduleFolderName);

			Logger.LogDebug("<<< Exiting GetRouteUrl(moduleFolderName={ModuleFolderName},url={Url},count={Count})", moduleFolderName, url, count);
            return $"{GeneratePrefixString(count)}DesktopModules/MVC/{moduleFolderName}/{url}";
        }

        /// <inheritdoc />
        public void ClearCachedData()
        {
			Logger.LogDebug(">>> Entering ClearCachedData()");
            this.prefixCounts = null;
			Logger.LogDebug("<<< Exiting ClearCachedData()");
			Logger.LogDebug("ClearCachedData(): prefixCounts → {PrefixCounts}", prefixCounts);
			Logger.LogDebug("ClearCachedData(): this.prefixCounts → {Arg0}", this.prefixCounts);
        }

        /// <inheritdoc />
        public IEnumerable<int> GetRoutePrefixCounts()
        {
			Logger.LogDebug(">>> Entering GetRoutePrefixCounts()");
            if (this.prefixCounts != null)
            {
                return this.prefixCounts;
            }

            // prefixCounts are required for each route that is mapped, but they only change
            // when a new portal is added so cache them until that time
            var portals = PortalController.Instance.GetPortals();
            var segmentCounts1 = new List<int>();
			Logger.LogDebug("GetRoutePrefixCounts(): portals → {Portals}", portals);
			Logger.LogDebug("GetRoutePrefixCounts(): segmentCounts1 → {SegmentCounts1}", segmentCounts1);

            foreach (
                var count in
                portals.Cast<IPortalInfo>()
                    .Select(portal => this.portalAliasService.GetPortalAliasesByPortalId(portal.PortalId).Select(x => x.HttpAlias))
                    .Select(this.StripApplicationPath)
                    .SelectMany(aliases => aliases.Select(CalcAliasPrefixCount).Where(count => !segmentCounts1.Contains(count))))
            {
                segmentCounts1.Add(count);
            }

            IEnumerable<int> segmentCounts = segmentCounts1;
            this.prefixCounts = segmentCounts.OrderByDescending(x => x).ToList();
			Logger.LogDebug("GetRoutePrefixCounts(): prefixCounts → {PrefixCounts}", prefixCounts);

			Logger.LogDebug("<<< Exiting GetRoutePrefixCounts()");
            return this.prefixCounts;
        }

        private static string GeneratePrefixString(int count)
        {
			Logger.LogDebug(">>> Entering GeneratePrefixString(count={Count})", count);
            if (count == 0)
            {
                return string.Empty;
            }

            var prefix = string.Empty;

			Logger.LogDebug("GeneratePrefixString(count={Count}): prefix → {Prefix}", count, prefix);
            for (var i = count - 1; i >= 0; i--)
            {
                prefix = "{prefix" + i + "}/" + prefix;
            }

			Logger.LogDebug("<<< Exiting GeneratePrefixString(count={Count})", count);
            return prefix;
        }

        private static int CalcAliasPrefixCount(string alias)
        {
            return alias.Count(c => c == '/');
        }

        private static IEnumerable<string> StripApplicationPathIterable(IEnumerable<string> aliases, string appPath)
        {
            foreach (var alias in aliases)
            {
                var i = alias.IndexOf(appPath, StringComparison.OrdinalIgnoreCase);

                if (i > 0)
                {
                    yield return alias.Remove(i, appPath.Length);
                }
                else
                {
                    yield return alias;
                }
            }
        }

        private IEnumerable<string> StripApplicationPath(IEnumerable<string> aliases)
        {
			Logger.LogDebug(">>> Entering StripApplicationPath(aliases={Aliases})", aliases);
            var appPath = TestableGlobals.Instance.ApplicationPath;

			Logger.LogDebug("<<< Exiting StripApplicationPath(aliases={Aliases})", aliases);
            return string.IsNullOrEmpty(appPath) ? aliases : StripApplicationPathIterable(aliases, appPath);
        }
    }
}
