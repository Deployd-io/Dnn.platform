// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Web.Compilation;

    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;

    public class ProviderInstaller : IComponentInstaller
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProviderInstaller));
        private readonly ComponentLifeStyleType componentLifeStyle;
        private readonly Type providerInterface;
        private readonly string providerType;
        private Type defaultProvider;

        /// <summary>Initializes a new instance of the <see cref="ProviderInstaller"/> class.</summary>
        /// <param name="providerType">The type name of the provider implementation.</param>
        /// <param name="providerInterface">The <see cref="Type"/> of the provider interface.</param>
        public ProviderInstaller(string providerType, Type providerInterface)
        {
			Logger.LogDebug(">>> Entering ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface})", providerType, providerInterface);
            this.componentLifeStyle = ComponentLifeStyleType.Singleton;
            this.providerType = providerType;
			Logger.LogDebug("ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface}): this.componentLifeStyle → {Arg2}", providerType, providerInterface, this.componentLifeStyle);
			Logger.LogDebug("ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface}): this.providerType → {Arg2}", providerType, providerInterface, this.providerType);
			Logger.LogDebug("ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},lifeStyle={LifeStyle}): this.providerType → {Arg3}", providerType, providerInterface, lifeStyle, this.providerType);
            this.providerInterface = providerInterface;
			Logger.LogDebug("<<< Exiting ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},defaultProvider={DefaultProvider})", providerType, providerInterface, defaultProvider);
        }

        /// <summary>Initializes a new instance of the <see cref="ProviderInstaller"/> class.</summary>
        /// <param name="providerType">The type name of the provider implementation.</param>
        /// <param name="providerInterface">The <see cref="Type"/> of the provider interface.</param>
        /// <param name="defaultProvider">The default <see cref="Type"/> for the implementation.</param>
        public ProviderInstaller(string providerType, Type providerInterface, Type defaultProvider)
        {
			Logger.LogDebug(">>> Entering ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},defaultProvider={DefaultProvider})", providerType, providerInterface, defaultProvider);
            this.componentLifeStyle = ComponentLifeStyleType.Singleton;
            this.providerType = providerType;
			Logger.LogDebug("ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},defaultProvider={DefaultProvider}): this.componentLifeStyle → {Arg3}", providerType, providerInterface, defaultProvider, this.componentLifeStyle);
			Logger.LogDebug("ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},defaultProvider={DefaultProvider}): this.providerType → {Arg3}", providerType, providerInterface, defaultProvider, this.providerType);
            this.providerInterface = providerInterface;
            this.defaultProvider = defaultProvider;
			Logger.LogDebug("<<< Exiting ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface})", providerType, providerInterface);
        }

        /// <summary>Initializes a new instance of the <see cref="ProviderInstaller"/> class.</summary>
        /// <param name="providerType">The type name of the provider implementation.</param>
        /// <param name="providerInterface">The <see cref="Type"/> of the provider interface.</param>
        /// <param name="lifeStyle">The <see cref="ComponentLifeStyleType"/> of the instance.</param>
        public ProviderInstaller(string providerType, Type providerInterface, ComponentLifeStyleType lifeStyle)
        {
			Logger.LogDebug(">>> Entering ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},lifeStyle={LifeStyle})", providerType, providerInterface, lifeStyle);
            this.componentLifeStyle = lifeStyle;
			Logger.LogDebug("ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},lifeStyle={LifeStyle}): this.componentLifeStyle → {Arg3}", providerType, providerInterface, lifeStyle, this.componentLifeStyle);
            this.providerType = providerType;
            this.providerInterface = providerInterface;
			Logger.LogDebug("<<< Exiting ProviderInstaller(providerType={ProviderType},providerInterface={ProviderInterface},lifeStyle={LifeStyle})", providerType, providerInterface, lifeStyle);
        }

        /// <inheritdoc />
        public void InstallComponents(IContainer container)
        {
			Logger.LogDebug(">>> Entering InstallComponents(container={Container})", container);
            ProviderConfiguration config = ProviderConfiguration.GetProviderConfiguration(this.providerType);

			Logger.LogDebug("InstallComponents(container={Container}): config → {Config}", container, config);
            // Register the default provider first (so it is the first component registered for its service interface
            if (config != null)
            {
                this.InstallProvider(container, (Provider)config.Providers[config.DefaultProvider]);

                // Register the others
                foreach (Provider provider in config.Providers.Values)
                {
                    // Skip the default because it was registered above
                    if (!config.DefaultProvider.Equals(provider.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        this.InstallProvider(container, provider);
                    }
                }
            }
			Logger.LogDebug("<<< Exiting InstallComponents(container={Container})", container);
        }

        private void InstallProvider(IContainer container, Provider provider)
        {
			Logger.LogDebug(">>> Entering InstallProvider(container={Container},provider={Provider})", container, provider);
            if (provider != null)
            {
                Type type = null;

                // Get the provider type
                try
                {
                    type = BuildManager.GetType(provider.Type, false, true);
                }
                catch (TypeLoadException)
                {
					Logger.LogError("Exception in InstallProvider(container={Container},provider={Provider}): {Arg2}", container, provider, TypeLoadException.getMessage());
                    if (this.defaultProvider != null)
                    {
                        type = this.defaultProvider;
                    }
                }

                if (type == null)
                {
                    Logger.Error(new ConfigurationErrorsException($"Could not load provider {provider.Type}"));
                }
                else
                {
                    // Register the component
                    container.RegisterComponent(provider.Name, this.providerInterface, type, this.componentLifeStyle);

                    // Load the settings into a dictionary
                    var settingsDict = new Dictionary<string, string> { { "providerName", provider.Name } };
                    foreach (string key in provider.Attributes.Keys)
                    {
                        settingsDict.Add(key, provider.Attributes.Get(key));
                    }

                    // Register the settings as dependencies
                    container.RegisterComponentSettings(type.FullName, settingsDict);
                }
            }
			Logger.LogDebug("<<< Exiting InstallProvider(container={Container},provider={Provider})", container, provider);
        }
    }
}
