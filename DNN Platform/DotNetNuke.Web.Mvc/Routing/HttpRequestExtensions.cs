// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Routing
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.UserRequest;

    internal static class HttpRequestExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private delegate bool TryMethod<T>(ITabAndModuleInfoProvider provider, HttpRequestBase request, out T output);

        public static int FindTabId(this HttpRequestBase request)
        {
			Logger.LogDebug(">>> Entering FindTabId(request={Request})", request);
			Logger.LogDebug("<<< Exiting FindTabId(request={Request})", request);
            return IterateTabAndModuleInfoProviders(request, TryFindTabId, -1);
        }

        public static ModuleInfo FindModuleInfo(this HttpRequestBase request)
        {
			Logger.LogDebug(">>> Entering FindModuleInfo(request={Request})", request);
			Logger.LogDebug("<<< Exiting FindModuleInfo(request={Request})", request);
            return IterateTabAndModuleInfoProviders<ModuleInfo>(request, TryFindModuleInfo, null);
        }

        public static int FindModuleId(this HttpRequestBase request)
        {
			Logger.LogDebug(">>> Entering FindModuleId(request={Request})", request);
			Logger.LogDebug("<<< Exiting FindModuleId(request={Request})", request);
            return IterateTabAndModuleInfoProviders(request, TryFindModuleId, -1);
        }

        public static string GetIPAddress(HttpRequestBase request)
        {
			Logger.LogDebug(">>> Entering GetIPAddress(request={Request})", request);
			Logger.LogDebug("<<< Exiting GetIPAddress(request={Request})", request);
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(request);
        }

        private static bool TryFindTabId(ITabAndModuleInfoProvider provider, HttpRequestBase request, out int output)
        {
			Logger.LogDebug(">>> Entering TryFindTabId(provider={Provider},request={Request})", provider, request);
			Logger.LogDebug("<<< Exiting TryFindTabId(provider={Provider},request={Request})", provider, request);
            return provider.TryFindTabId(request, out output);
        }

        private static bool TryFindModuleInfo(ITabAndModuleInfoProvider provider, HttpRequestBase request, out ModuleInfo output)
        {
			Logger.LogDebug(">>> Entering TryFindModuleInfo(provider={Provider},request={Request})", provider, request);
			Logger.LogDebug("<<< Exiting TryFindModuleInfo(provider={Provider},request={Request})", provider, request);
            return provider.TryFindModuleInfo(request, out output);
        }

        private static bool TryFindModuleId(ITabAndModuleInfoProvider provider, HttpRequestBase request, out int output)
        {
			Logger.LogDebug(">>> Entering TryFindModuleId(provider={Provider},request={Request})", provider, request);
			Logger.LogDebug("<<< Exiting TryFindModuleId(provider={Provider},request={Request})", provider, request);
            return provider.TryFindModuleId(request, out output);
        }

        private static T IterateTabAndModuleInfoProviders<T>(HttpRequestBase request, TryMethod<T> func, T fallback)
        {
			Logger.LogDebug(">>> Entering IterateTabAndModuleInfoProviders(request={Request},func={Func},fallback={Fallback})", request, func, fallback);
            var providers = GlobalConfiguration.Configuration.GetTabAndModuleInfoProviders();

			Logger.LogDebug("IterateTabAndModuleInfoProviders(request={Request},func={Func},fallback={Fallback}): providers → {Providers}", request, func, fallback, providers);
            foreach (var provider in providers)
            {
                T output;
                if (func(provider, request, out output))
                {
                    return output;
                }
            }

            return fallback;
        }
    }
}
