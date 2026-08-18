// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.ComponentModel;

using System.Collections;
using System.Collections.Generic;

using DotNetNuke.ComponentModel;
using DotNetNuke.Tests.Utilities.Fakes;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

[TestFixture]
public class ContainerWithServiceProviderFallbackTests
{
    [Test]
    public void GetComponentListSupportsInterfaces()
    {
		mockEventLogger.LogDebug(">>> Entering GetComponentListSupportsInterfaces()");
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), FakeServiceProvider.Create());
        container.RegisterComponent<IList>("payload", ComponentLifeStyleType.Singleton);
		mockEventLogger.LogDebug("GetComponentListSupportsInterfaces(): container → {Container}", container);

        var retrieved = container.GetComponentList(typeof(IList));
		mockEventLogger.LogDebug("GetComponentListSupportsInterfaces(): retrieved → {Retrieved}", retrieved);

        Assert.That(retrieved, Is.EqualTo(new List<string> { "payload" }).AsCollection);
		mockEventLogger.LogDebug("<<< Exiting GetComponentListSupportsInterfaces()");
    }

    [Test]
    public void RegisterComponentInstance_Must_Register_In_ComponentsList()
    {
		mockEventLogger.LogDebug(">>> Entering RegisterComponentInstance_Must_Register_In_ComponentsList()");
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), FakeServiceProvider.Create());
        ComponentFactory.Container = container;
		mockEventLogger.LogDebug("RegisterComponentInstance_Must_Register_In_ComponentsList(): ComponentFactory.Container → {Arg0}", ComponentFactory.Container);
		mockEventLogger.LogDebug("RegisterComponentInstance_Must_Register_In_ComponentsList(): container → {Container}", container);

        container.RegisterComponentInstance("test", typeof(IList<string>), new List<string>());

        Assert.That(ComponentFactory.GetComponents<IList<string>>().Keys, Contains.Item("test"));
		mockEventLogger.LogDebug("<<< Exiting RegisterComponentInstance_Must_Register_In_ComponentsList()");
    }

    [Test]
    public void GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
		mockEventLogger.LogDebug(">>> Entering GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		mockEventLogger.LogDebug("GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): container → {Container}", container);
		mockEventLogger.LogDebug("GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): serviceProvider → {ServiceProvider}", serviceProvider);

        container.RegisterComponent<IDictionary, Dictionary<bool, bool>>();

        Assert.That(container.GetComponent<IDictionary>(), Is.AssignableFrom<Dictionary<bool, bool>>());
		mockEventLogger.LogDebug("<<< Exiting GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
    }

    [Test]
    public void GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
		mockEventLogger.LogDebug(">>> Entering GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		mockEventLogger.LogDebug("GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): container → {Container}", container);
		mockEventLogger.LogDebug("GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): serviceProvider → {ServiceProvider}", serviceProvider);

        Assert.That(container.GetComponent<IDictionary>(), Is.AssignableFrom<Dictionary<int, int>>());
		mockEventLogger.LogDebug("<<< Exiting GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
    }

    [Test]
    public void NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
		mockEventLogger.LogDebug(">>> Entering NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		mockEventLogger.LogDebug("NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): container → {Container}", container);
		mockEventLogger.LogDebug("NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): serviceProvider → {ServiceProvider}", serviceProvider);

        Assert.That(container.GetComponent<IDictionary>("Dictionary"), Is.Null);
		mockEventLogger.LogDebug("<<< Exiting NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
    }

    [Test]
    public void NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
		mockEventLogger.LogDebug(">>> Entering NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		mockEventLogger.LogDebug("NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): container → {Container}", container);
		mockEventLogger.LogDebug("NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): serviceProvider → {ServiceProvider}", serviceProvider);

        container.RegisterComponent(typeof(IDictionary), typeof(Dictionary<bool, bool>));

        Assert.That(container.GetComponent(typeof(IDictionary)), Is.AssignableFrom<Dictionary<bool, bool>>());
		mockEventLogger.LogDebug("<<< Exiting NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
    }

    [Test]
    public void NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
		mockEventLogger.LogDebug(">>> Entering NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		mockEventLogger.LogDebug("NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): container → {Container}", container);
		mockEventLogger.LogDebug("NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): serviceProvider → {ServiceProvider}", serviceProvider);

        Assert.That(container.GetComponent(typeof(IDictionary)), Is.AssignableFrom<Dictionary<int, int>>());
		mockEventLogger.LogDebug("<<< Exiting NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
    }

    [Test]
    public void NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
		mockEventLogger.LogDebug(">>> Entering NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		mockEventLogger.LogDebug("NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): container → {Container}", container);
		mockEventLogger.LogDebug("NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): serviceProvider → {ServiceProvider}", serviceProvider);

        Assert.That(container.GetComponent("Dictionary", typeof(IDictionary)), Is.Null);
		mockEventLogger.LogDebug("<<< Exiting NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
    }
}
