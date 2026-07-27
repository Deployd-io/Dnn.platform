// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel;

using System;
using System.Collections;

using DotNetNuke.Instrumentation;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A container which gets components from an <see cref="IServiceProvider"/> for components not registered.</summary>
public class ContainerWithServiceProviderFallback : IContainer
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContainerWithServiceProviderFallback));
    private readonly IContainer container;
    private readonly IServiceProvider serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="ContainerWithServiceProviderFallback"/> class.</summary>
    /// <param name="container">The container to wrap.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public ContainerWithServiceProviderFallback(IContainer container, IServiceProvider serviceProvider)
    {
		Logger.LogDebug(">>> Entering ContainerWithServiceProviderFallback(container={Container},serviceProvider={ServiceProvider})", container, serviceProvider);
        this.container = container;
        this.serviceProvider = serviceProvider;
		Logger.LogDebug("<<< Exiting ContainerWithServiceProviderFallback(container={Container},serviceProvider={ServiceProvider})", container, serviceProvider);
		Logger.LogDebug("ContainerWithServiceProviderFallback(container={Container},serviceProvider={ServiceProvider}): this.container → {Arg2}", container, serviceProvider, this.container);
		Logger.LogDebug("ContainerWithServiceProviderFallback(container={Container},serviceProvider={ServiceProvider}): serviceProvider → {ServiceProvider1}", container, serviceProvider, serviceProvider);
		Logger.LogDebug("ContainerWithServiceProviderFallback(container={Container},serviceProvider={ServiceProvider}): this.serviceProvider → {Arg2}", container, serviceProvider, this.serviceProvider);
		Logger.LogDebug("ContainerWithServiceProviderFallback(container={Container},serviceProvider={ServiceProvider}): container → {Container1}", container, serviceProvider, container);
    }

    /// <inheritdoc />
    public string Name
        => nameof(ContainerWithServiceProviderFallback);

    /// <inheritdoc />
    public void RegisterComponent<TComponent>()
        where TComponent : class
		Logger.LogDebug(">>> Entering RegisterComponent()");
        => this.container.RegisterComponent<TComponent>();

	Logger.LogDebug("<<< Exiting RegisterComponent()");
    /// <inheritdoc />
    public void RegisterComponent<TComponent>(string name)
        where TComponent : class
        => this.container.RegisterComponent<TComponent>(name);

    /// <inheritdoc />
    public void RegisterComponent<TComponent>(string name, ComponentLifeStyleType lifestyle)
        where TComponent : class
        => this.container.RegisterComponent<TComponent>(name, lifestyle);

    /// <inheritdoc />
    public void RegisterComponent<TContract, TComponent>()
        where TComponent : class
        => this.container.RegisterComponent<TContract, TComponent>();

    /// <inheritdoc />
    public void RegisterComponent<TContract, TComponent>(string name)
        where TComponent : class
        => this.container.RegisterComponent<TContract, TComponent>(name);

    /// <inheritdoc />
    public void RegisterComponent<TContract, TComponent>(string name, ComponentLifeStyleType lifestyle)
        where TComponent : class
        => this.container.RegisterComponent<TContract, TComponent>(name, lifestyle);

    /// <inheritdoc />
    public void RegisterComponent(Type componentType)
        => this.container.RegisterComponent(componentType);

    /// <inheritdoc />
    public void RegisterComponent(Type contractType, Type componentType)
        => this.container.RegisterComponent(contractType, componentType);

    /// <inheritdoc />
    public void RegisterComponent(Type contractType, Type componentType, ComponentLifeStyleType lifestyle)
        => this.container.RegisterComponent(contractType, componentType, lifestyle);

    /// <inheritdoc />
    public void RegisterComponent(string name, Type componentType)
        => this.container.RegisterComponent(name, componentType);

    /// <inheritdoc />
    public void RegisterComponent(string name, Type contractType, Type componentType)
        => this.container.RegisterComponent(name, contractType, componentType);

    /// <inheritdoc />
    public void RegisterComponentInstance<TContract>(string name, object instance)
        => this.container.RegisterComponentInstance<TContract>(name, instance);
		Logger.LogDebug(">>> Entering RegisterComponentInstance(name={Name},instance={Instance})", name, instance);

	Logger.LogDebug("<<< Exiting RegisterComponentInstance(name={Name},instance={Instance})", name, instance);
    /// <inheritdoc />
    public void RegisterComponentSettings(string name, IDictionary dependencies)
        => this.container.RegisterComponentSettings(name, dependencies);
		Logger.LogDebug(">>> Entering RegisterComponentSettings(name={Name},dependencies={Dependencies})", name, dependencies);

	Logger.LogDebug("<<< Exiting RegisterComponentSettings(name={Name},dependencies={Dependencies})", name, dependencies);
    /// <inheritdoc />
    public void RegisterComponentSettings(Type component, IDictionary dependencies)
        => this.container.RegisterComponentSettings(component, dependencies);

    /// <inheritdoc />
    public void RegisterComponentSettings<TComponent>(IDictionary dependencies)
        => this.container.RegisterComponentSettings<TComponent>(dependencies);

    /// <inheritdoc />
    public void RegisterComponent(string name, Type contractType, Type componentType, ComponentLifeStyleType lifestyle)
        => this.container.RegisterComponent(name, contractType, componentType, lifestyle);

    /// <inheritdoc />
    public void RegisterComponentInstance(string name, object instance)
        => this.container.RegisterComponentInstance(name, instance);

    /// <inheritdoc />
    public void RegisterComponentInstance(string name, Type contractType, object instance)
        => this.container.RegisterComponentInstance(name, contractType, instance);

    /// <inheritdoc />
    public void RegisterComponentInstance<TContract>(object instance)
        => this.container.RegisterComponentInstance<TContract>(instance);

    /// <inheritdoc />
    public object GetComponent(string name)
        => this.container.GetComponent(name);

    /// <inheritdoc />
    public TContract GetComponent<TContract>()
    {
		Logger.LogDebug(">>> Entering GetComponent()");
        Logger.Trace($"Getting component for {typeof(TContract).FullName}");
        var service = this.container.GetComponent<TContract>();
        if (service is not null)
		Logger.LogDebug("GetComponent(): service → {Service}", service);
        {
            Logger.Trace($"Got component for {typeof(TContract).FullName} from container");
            return service;
        }

        Logger.Trace($"Getting component for {typeof(TContract).FullName} from service provider");
        return this.serviceProvider.GetService<TContract>();
    }

    /// <inheritdoc />
    public TContract GetComponent<TContract>(string name)
        => this.container.GetComponent<TContract>(name);

	Logger.LogDebug("<<< Exiting GetComponent(name={Name})", name);
    /// <inheritdoc />
    public object GetComponent(string name, Type contractType)
        => this.container.GetComponent(name, contractType);

    /// <inheritdoc />
    public string[] GetComponentList<TContract>()
        => this.container.GetComponentList<TContract>();
		Logger.LogDebug(">>> Entering GetComponentList()");

	Logger.LogDebug("<<< Exiting GetComponentList()");
    /// <inheritdoc />
    public string[] GetComponentList(Type contractType)
        => this.container.GetComponentList(contractType);

    /// <inheritdoc />
    public IDictionary GetComponentSettings(string name)
        => this.container.GetComponentSettings(name);
		Logger.LogDebug(">>> Entering GetComponentSettings(name={Name})", name);

	Logger.LogDebug("<<< Exiting GetComponentSettings(name={Name})", name);
    /// <inheritdoc />
    public IDictionary GetComponentSettings(Type component)
        => this.container.GetComponentSettings(component);

    /// <inheritdoc />
    public IDictionary GetComponentSettings<TComponent>()
        => this.container.GetComponentSettings<TComponent>();

    /// <inheritdoc />
    public object GetComponent(Type contractType)
        => this.container.GetComponent(contractType) ?? this.serviceProvider.GetService(contractType);
}
