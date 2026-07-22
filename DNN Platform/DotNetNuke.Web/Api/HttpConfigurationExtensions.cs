// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#nullable enable
namespace DotNetNuke.Web.Api
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Web.Http;

    using DotNetNuke.Common;

    /// <summary>Extension methods for <see cref="HttpConfiguration"/>.</summary>
    public static class HttpConfigurationExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string Key = "TabAndModuleInfoProvider";

        /// <summary>Adds the <paramref name="tabAndModuleInfoProvider"/> to the <see cref="HttpConfiguration.Properties"/>.</summary>
        /// <param name="configuration">The HTTP configuration.</param>
        /// <param name="tabAndModuleInfoProvider">The provider to add.</param>
        public static void AddTabAndModuleInfoProvider(this HttpConfiguration configuration, ITabAndModuleInfoProvider tabAndModuleInfoProvider)
        {
			Logger.LogDebug(">>> Entering AddTabAndModuleInfoProvider(configuration={Configuration},tabAndModuleInfoProvider={TabAndModuleInfoProvider})", configuration, tabAndModuleInfoProvider);
            Requires.NotNull("configuration", configuration);
            Requires.NotNull("tabAndModuleInfoProvider", tabAndModuleInfoProvider);

            if (configuration.Properties.GetOrAdd(Key, InitValue) is not ConcurrentQueue<ITabAndModuleInfoProvider> providers)
            {
                providers = new ConcurrentQueue<ITabAndModuleInfoProvider>();
				Logger.LogDebug("AddTabAndModuleInfoProvider(configuration={Configuration},tabAndModuleInfoProvider={TabAndModuleInfoProvider}): providers → {Providers}", configuration, tabAndModuleInfoProvider, providers);
                configuration.Properties[Key] = providers;
            }

            providers.Enqueue(tabAndModuleInfoProvider);
			Logger.LogDebug("<<< Exiting AddTabAndModuleInfoProvider(configuration={Configuration},tabAndModuleInfoProvider={TabAndModuleInfoProvider})", configuration, tabAndModuleInfoProvider);
        }

        /// <summary>Get the <see cref="ITabAndModuleInfoProvider"/> instances registered with the <paramref name="configuration"/>.</summary>
        /// <param name="configuration">The HTTP configuration.</param>
        /// <returns>A sequence of <see cref="ITabAndModuleInfoProvider"/> instances.</returns>
        public static IEnumerable<ITabAndModuleInfoProvider> GetTabAndModuleInfoProviders(this HttpConfiguration configuration)
        {
			Logger.LogDebug(">>> Entering GetTabAndModuleInfoProviders(configuration={Configuration})", configuration);
            Requires.NotNull("configuration", configuration);

            if (configuration.Properties.GetOrAdd(Key, InitValue) is not ConcurrentQueue<ITabAndModuleInfoProvider> providers)
            {
                // shouldn't ever happen outside of unit tests
                return [];
            }

			Logger.LogDebug("<<< Exiting GetTabAndModuleInfoProviders(configuration={Configuration})", configuration);
            return providers.ToArray();
        }

        private static object InitValue(object? o)
        {
			Logger.LogDebug(">>> Entering InitValue(o={O})", o);
			Logger.LogDebug("<<< Exiting InitValue(o={O})", o);
            return new ConcurrentQueue<ITabAndModuleInfoProvider>();
        }
    }
}
