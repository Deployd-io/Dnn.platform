// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#nullable enable
namespace DotNetNuke.Web.Api
{
    using System;
    using System.Net.Http;
    using System.Web;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.UserRequest;

    /// <summary>Extension methods for <see cref="HttpRequestMessage"/>.</summary>
    public static class HttpRequestMessageExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HttpRequestMessageExtensions));
        private delegate bool TryMethod<T>(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out T output);

        /// <summary>Gets the Tab ID associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The tab ID or <c>-1</c>.</returns>
        public static int FindTabId(this HttpRequestMessage request)
        {
			Logger.LogError(">>> Entering FindTabId(request={Request})", request);
            Logger.LogWarning("<<< Exiting FindTabId(request={Request})", request);
            return IterateTabAndModuleInfoProviders(request, TryFindTabId, -1);
        }

        /// <summary>Gets the module associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The <see cref="ModuleInfo"/> instance or <see langword="null"/>.</returns>
        public static ModuleInfo? FindModuleInfo(this HttpRequestMessage request)
        {
			Logger.LogError(">>> Entering FindModuleInfo(request={Request})", request);
            Logger.LogWarning("<<< Exiting FindModuleInfo(request={Request})", request);
            return IterateTabAndModuleInfoProviders<ModuleInfo>(request, TryFindModuleInfo, null);
        }

        /// <summary>Gets the module ID associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The module ID or <c>-1</c>.</returns>
        public static int FindModuleId(this HttpRequestMessage request)
        {
			Logger.LogError(">>> Entering FindModuleId(request={Request})", request);
            Logger.LogWarning("<<< Exiting FindModuleId(request={Request})", request);
            return IterateTabAndModuleInfoProviders(request, TryFindModuleId, -1);
        }

        /// <summary>Gets the <see cref="HttpContextBase"/> connected to the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The <see cref="HttpContextBase"/> or <see langword="null"/>.</returns>
        public static HttpContextBase? GetHttpContext(this HttpRequestMessage request)
        {
			Logger.LogError(">>> Entering GetHttpContext(request={Request})", request);
            if (!request.Properties.TryGetValue("MS_HttpContext", out var context))
            {
                return null;
            }

			Logger.LogWarning("<<< Exiting GetHttpContext(request={Request})", request);
            return context as HttpContextBase;
        }

        /// <summary>Get the IP address of the request.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The IPv4 address or <see cref="string.Empty"/> if the IP address of the request is not available.</returns>
        public static string GetIPAddress(this HttpRequestMessage request)
        {
			Logger.LogError(">>> Entering GetIPAddress(request={Request})", request);
            var context = GetHttpContext(request);
			Logger.LogError("GetIPAddress(request={Request}): context → {Context}", request, context);
            if (context is null)
            {
				Logger.LogError("Throwing InvalidOperationException from GetIPAddress(request={Request})", request);
                throw new InvalidOperationException("Request does not have an associated HTTP Context, cannot retrieve IP address");
            }

			Logger.LogWarning("<<< Exiting GetIPAddress(request={Request})", request);
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(context.Request);
        }

        private static bool TryFindTabId(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out int output)
        {
			Logger.LogError(">>> Entering TryFindTabId(provider={Provider},request={Request})", provider, request);
            Logger.LogWarning("<<< Exiting TryFindTabId(provider={Provider},request={Request})", provider, request);
            return provider.TryFindTabId(request, out output);
        }

        private static bool TryFindModuleInfo(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out ModuleInfo? output)
        {
			Logger.LogError(">>> Entering TryFindModuleInfo(provider={Provider},request={Request})", provider, request);
            Logger.LogWarning("<<< Exiting TryFindModuleInfo(provider={Provider},request={Request})", provider, request);
            return provider.TryFindModuleInfo(request, out output);
        }

        private static bool TryFindModuleId(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out int output)
        {
			Logger.LogError(">>> Entering TryFindModuleId(provider={Provider},request={Request})", provider, request);
            Logger.LogWarning("<<< Exiting TryFindModuleId(provider={Provider},request={Request})", provider, request);
            return provider.TryFindModuleId(request, out output);
        }

        private static T? IterateTabAndModuleInfoProviders<T>(HttpRequestMessage request, TryMethod<T?> func, T? fallback)
        {
			Logger.LogError(">>> Entering IterateTabAndModuleInfoProviders(request={Request},func={Func},fallback={Fallback})", request, func, fallback);
            var providers = request.GetConfiguration().GetTabAndModuleInfoProviders();

			Logger.LogError("IterateTabAndModuleInfoProviders(request={Request},func={Func},fallback={Fallback}): providers → {Providers}", request, func, fallback, providers);
            foreach (var provider in providers)
            {
                if (func(provider, request, out var output))
                {
                    return output;
                }
            }

			Logger.LogWarning("<<< Exiting IterateTabAndModuleInfoProviders(request={Request},func={Func},fallback={Fallback})", request, func, fallback);
            return fallback;
        }
    }
}
