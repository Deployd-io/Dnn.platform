// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Caching;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Extension methods for <see langword="ViewEngineCollection"/>.</summary>
    public static partial class ViewEngineCollectionExt
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ViewEngineCollectionExt));
        /// <summary>Finds the specified view by using the specified controller context.</summary>
        /// <param name="viewEngineCollection">The view engine collection.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="masterName">The name of the master.</param>
        /// <param name="useCache"><see langword="true"/> to specify that the view engine returns the cached view, if a cached view exists; otherwise, <see langword="false"/>.</param>
        /// <returns>The page view.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial ViewEngineResult FindView(this ViewEngineCollection viewEngineCollection, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
            => viewEngineCollection.FindView(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), controllerContext, viewName, masterName, useCache);

        /// <summary>Finds the specified view by using the specified controller context.</summary>
        /// <param name="viewEngineCollection">The view engine collection.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="viewName">The name of the view.</param>
        /// <param name="masterName">The name of the master.</param>
        /// <param name="useCache"><see langword="true"/> to specify that the view engine returns the cached view, if a cached view exists; otherwise, <see langword="false"/>.</param>
        /// <returns>The page view.</returns>
        public static ViewEngineResult FindView(this ViewEngineCollection viewEngineCollection, IHostSettings hostSettings, ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
			Logger.LogError(">>> Entering FindView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache})", viewEngineCollection, controllerContext, viewName, masterName, useCache);
            try
            {
                var cacheKey = CreateCacheKey(controllerContext, "View", viewName, masterName, (controllerContext.Controller as IDnnController)?.ModuleContext.PortalId ?? 0);
				Logger.LogError("FindView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache}): cacheKey → {CacheKey}", viewEngineCollection, controllerContext, viewName, masterName, useCache, cacheKey);
                var parameters = new object[]
                {
					Logger.LogError("FindView(viewEngineCollection={ViewEngineCollection},hostSettings={HostSettings},controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache}): cacheKey → {CacheKey}", viewEngineCollection, hostSettings, controllerContext, viewName, masterName, useCache, cacheKey);
                    new Func<IViewEngine, ViewEngineResult>(e => e.FindView(controllerContext, viewName, masterName, false)),
                    false,
                };
                var cacheArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default, "Find", viewEngineCollection, parameters);
				Logger.LogError("FindView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache}): cacheArg → {CacheArg}", viewEngineCollection, controllerContext, viewName, masterName, useCache, cacheArg);
				Logger.LogError("FindView(viewEngineCollection={ViewEngineCollection},hostSettings={HostSettings},controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache}): cacheArg → {CacheArg}", viewEngineCollection, hostSettings, controllerContext, viewName, masterName, useCache, cacheArg);

                return useCache ? CBO.GetCachedObject<ViewEngineResult>(hostSettings, cacheArg, CallFind) : CallFind(cacheArg);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in FindView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache}): {Arg5}", viewEngineCollection, controllerContext, viewName, masterName, useCache, Exception.Message);
				Logger.LogWarning("<<< Exiting FindView(viewEngineCollection={ViewEngineCollection},hostSettings={HostSettings},controllerContext={ControllerContext},viewName={ViewName},masterName={MasterName},useCache={UseCache})", viewEngineCollection, hostSettings, controllerContext, viewName, masterName, useCache);
                return viewEngineCollection.FindView(controllerContext, viewName, masterName);
            }
        }

        /// <summary>Finds the specified partial view by using the specified controller context.</summary>
        /// <param name="viewEngineCollection">The view engine collection.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="partialViewName">The name of the partial view.</param>
        /// <param name="useCache"><see langword="true"/> to specify that the view engine returns the cached view, if a cached view exists; otherwise, <see langword="true"/>.</param>
        /// <returns>The partial view.</returns>
        [DnnDeprecated(10, 2, 2, "Please use overload taking IHostSettings")]
        public static partial ViewEngineResult FindPartialView(this ViewEngineCollection viewEngineCollection, ControllerContext controllerContext, string partialViewName, bool useCache)
            => viewEngineCollection.FindPartialView(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), controllerContext, partialViewName, useCache);

        public static ViewEngineResult FindPartialView(this ViewEngineCollection viewEngineCollection, IHostSettings hostSettings, ControllerContext controllerContext, string partialViewName, bool useCache)
        {
			Logger.LogError(">>> Entering FindPartialView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache})", viewEngineCollection, controllerContext, partialViewName, useCache);
            try
            {
                var cacheKey = CreateCacheKey(controllerContext, "Partial", partialViewName, string.Empty, (controllerContext.Controller as IDnnController)?.ModuleContext.PortalId ?? 0);
				Logger.LogError("FindPartialView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache}): cacheKey → {CacheKey}", viewEngineCollection, controllerContext, partialViewName, useCache, cacheKey);
                var parameters = new object[]
                {
					Logger.LogError("FindPartialView(viewEngineCollection={ViewEngineCollection},hostSettings={HostSettings},controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache}): cacheKey → {CacheKey}", viewEngineCollection, hostSettings, controllerContext, partialViewName, useCache, cacheKey);
                    new Func<IViewEngine, ViewEngineResult>(e => e.FindPartialView(controllerContext, partialViewName, false)),
                    false,
                };
                var cacheArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default, "Find", viewEngineCollection, parameters);
				Logger.LogError("FindPartialView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache}): cacheArg → {CacheArg}", viewEngineCollection, controllerContext, partialViewName, useCache, cacheArg);
				Logger.LogError("FindPartialView(viewEngineCollection={ViewEngineCollection},hostSettings={HostSettings},controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache}): cacheArg → {CacheArg}", viewEngineCollection, hostSettings, controllerContext, partialViewName, useCache, cacheArg);

                return useCache ? CBO.GetCachedObject<ViewEngineResult>(hostSettings, cacheArg, CallFind) : CallFind(cacheArg);
            }
            catch (Exception)
            {
				Logger.LogError("Exception in FindPartialView(viewEngineCollection={ViewEngineCollection},controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache}): {Arg4}", viewEngineCollection, controllerContext, partialViewName, useCache, Exception.Message);
				Logger.LogWarning("<<< Exiting FindPartialView(viewEngineCollection={ViewEngineCollection},hostSettings={HostSettings},controllerContext={ControllerContext},partialViewName={PartialViewName},useCache={UseCache})", viewEngineCollection, hostSettings, controllerContext, partialViewName, useCache);
                return viewEngineCollection.FindPartialView(controllerContext, partialViewName);
            }
        }

        private static ViewEngineResult CallFind(CacheItemArgs cacheItem)
        {
			Logger.LogError(">>> Entering CallFind(cacheItem={CacheItem})", cacheItem);
            var factoryType = Reflection.CreateType("System.Web.Mvc.ViewEngineCollection");
            var name = cacheItem.Params[0] as string;
			Logger.LogError("CallFind(cacheItem={CacheItem}): name → {Name}", cacheItem, name);
			Logger.LogError("CallFind(cacheItem={CacheItem}): factoryType → {FactoryType}", cacheItem, factoryType);
            var target = cacheItem.Params[1];
            var parameters = cacheItem.Params[2] as object[];
            var result = factoryType.InvokeMember(name, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, target, parameters, CultureInfo.InvariantCulture);
			Logger.LogError("CallFind(cacheItem={CacheItem}): result → {Result}", cacheItem, result);
			Logger.LogWarning("<<< Exiting CallFind(cacheItem={CacheItem})", cacheItem);
            return result as ViewEngineResult;
        }

        private static string CreateCacheKey(ControllerContext controllerContext, string section, string name, string areaName, int portalId)
        {
			Logger.LogError(">>> Entering CreateCacheKey(controllerContext={ControllerContext},section={Section},name={Name},areaName={AreaName},portalId={PortalId})", controllerContext, section, name, areaName, portalId);
            var __logStart = System.Diagnostics.Stopwatch.StartNew();
            return string.Format(
                CultureInfo.InvariantCulture,
                ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}",
                ((string[])controllerContext.RouteData.DataTokens["namespaces"]).FirstOrDefault(),
                section,
                name,
                controllerContext.RouteData.Values["controller"],
                areaName,
                portalId);
        }
    }
}
