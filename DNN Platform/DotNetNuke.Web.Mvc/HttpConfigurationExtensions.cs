// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Web.Http;

    using DotNetNuke.Common;

    public static class HttpConfigurationExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HttpConfigurationExtensions));
        private const string Key = "MvcTabAndModuleInfoProvider";

        public static void AddTabAndModuleInfoProvider(this HttpConfiguration configuration, ITabAndModuleInfoProvider tabAndModuleInfoProvider)
        {
			Logger.LogDebug(">>> Entering AddTabAndModuleInfoProvider(configuration={Configuration},tabAndModuleInfoProvider={TabAndModuleInfoProvider})", configuration, tabAndModuleInfoProvider);
            Requires.NotNull("configuration", configuration);
            Requires.NotNull("tabAndModuleInfoProvider", tabAndModuleInfoProvider);

            var providers = configuration.Properties.GetOrAdd(Key, InitValue) as ConcurrentQueue<ITabAndModuleInfoProvider>;
			Logger.LogDebug("AddTabAndModuleInfoProvider(configuration={Configuration},tabAndModuleInfoProvider={TabAndModuleInfoProvider}): providers → {Providers}", configuration, tabAndModuleInfoProvider, providers);

            if (providers == null)
            {
                providers = new ConcurrentQueue<ITabAndModuleInfoProvider>();
                configuration.Properties[Key] = providers;
            }

            providers.Enqueue(tabAndModuleInfoProvider);
			Logger.LogDebug("<<< Exiting AddTabAndModuleInfoProvider(configuration={Configuration},tabAndModuleInfoProvider={TabAndModuleInfoProvider})", configuration, tabAndModuleInfoProvider);
        }

        public static IEnumerable<ITabAndModuleInfoProvider> GetTabAndModuleInfoProviders(this HttpConfiguration configuration)
        {
			Logger.LogDebug(">>> Entering GetTabAndModuleInfoProviders(configuration={Configuration})", configuration);
            Requires.NotNull("configuration", configuration);

            if (configuration.Properties.GetOrAdd(Key, InitValue) is not ConcurrentQueue<ITabAndModuleInfoProvider> providers)
            {
                // shouldn't ever happen outside of unit tests
			Logger.LogDebug("<<< Exiting GetTabAndModuleInfoProviders(configuration={Configuration})", configuration);
                return [];
            }

            return providers.ToArray();
        }

        private static object InitValue(object o)
        {
			Logger.LogDebug(">>> Entering InitValue(o={O})", o);
			Logger.LogDebug("<<< Exiting InitValue(o={O})", o);
            return new ConcurrentQueue<ITabAndModuleInfoProvider>();
        }
    }
}
