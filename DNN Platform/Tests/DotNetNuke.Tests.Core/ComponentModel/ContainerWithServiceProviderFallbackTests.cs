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
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    [Test]
    public void GetComponentListSupportsInterfaces()
    {
		Logger.LogDebug(">>> Entering GetComponentListSupportsInterfaces()");
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), FakeServiceProvider.Create());
        container.RegisterComponent<IList>("payload", ComponentLifeStyleType.Singleton);
		Logger.LogDebug("GetComponentListSupportsInterfaces(): container → {Container}", container);

        var retrieved = container.GetComponentList(typeof(IList));

		Logger.LogDebug("GetComponentListSupportsInterfaces(): retrieved → {Retrieved}", retrieved);
        Assert.That(retrieved, Is.EqualTo(new List<string> { "payload" }).AsCollection);
		Logger.LogDebug("<<< Exiting GetComponentListSupportsInterfaces()");
    }

    [Test]
    public void RegisterComponentInstance_Must_Register_In_ComponentsList()
    {
		Logger.LogDebug(">>> Entering RegisterComponentInstance_Must_Register_In_ComponentsList()");
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), FakeServiceProvider.Create());
        ComponentFactory.Container = container;
		Logger.LogDebug("RegisterComponentInstance_Must_Register_In_ComponentsList(): container → {Container}", container);
		Logger.LogDebug("RegisterComponentInstance_Must_Register_In_ComponentsList(): ComponentFactory.Container → {Arg0}", ComponentFactory.Container);

        container.RegisterComponentInstance("test", typeof(IList<string>), new List<string>());

        Assert.That(ComponentFactory.GetComponents<IList<string>>().Keys, Contains.Item("test"));
		Logger.LogDebug("<<< Exiting RegisterComponentInstance_Must_Register_In_ComponentsList()");
    }

    [Test]
    public void GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
		Logger.LogDebug(">>> Entering GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		Logger.LogDebug("GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): serviceProvider → {ServiceProvider}", serviceProvider);
		Logger.LogDebug("GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): container → {Container}", container);

        container.RegisterComponent<IDictionary, Dictionary<bool, bool>>();

        Assert.That(container.GetComponent<IDictionary>(), Is.AssignableFrom<Dictionary<bool, bool>>());
		Logger.LogDebug("<<< Exiting GenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
    }

    [Test]
    public void GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
		Logger.LogDebug(">>> Entering GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		Logger.LogDebug("GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): serviceProvider → {ServiceProvider}", serviceProvider);
		Logger.LogDebug("GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): container → {Container}", container);

        Assert.That(container.GetComponent<IDictionary>(), Is.AssignableFrom<Dictionary<int, int>>());
		Logger.LogDebug("<<< Exiting GenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
    }

    [Test]
    public void NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
		Logger.LogDebug(">>> Entering NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		Logger.LogDebug("NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): serviceProvider → {ServiceProvider}", serviceProvider);
		Logger.LogDebug("NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): container → {Container}", container);

        Assert.That(container.GetComponent<IDictionary>("Dictionary"), Is.Null);
		Logger.LogDebug("<<< Exiting NamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
    }

    [Test]
    public void NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()
    {
		Logger.LogDebug(">>> Entering NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		Logger.LogDebug("NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): serviceProvider → {ServiceProvider}", serviceProvider);
		Logger.LogDebug("NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer(): container → {Container}", container);

        container.RegisterComponent(typeof(IDictionary), typeof(Dictionary<bool, bool>));

        Assert.That(container.GetComponent(typeof(IDictionary)), Is.AssignableFrom<Dictionary<bool, bool>>());
		Logger.LogDebug("<<< Exiting NonGenericGetComponent_WhenRegisteredTypeInContainerAndInServiceProvider_GetsTypeFromContainer()");
    }

    [Test]
    public void NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()
    {
		Logger.LogDebug(">>> Entering NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		Logger.LogDebug("NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): serviceProvider → {ServiceProvider}", serviceProvider);
		Logger.LogDebug("NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider(): container → {Container}", container);

        Assert.That(container.GetComponent(typeof(IDictionary)), Is.AssignableFrom<Dictionary<int, int>>());
		Logger.LogDebug("<<< Exiting NonGenericGetComponent_WhenOnlyRegisteredInServiceProvider_GetsTypeFromServiceProvider()");
    }

    [Test]
    public void NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()
    {
		Logger.LogDebug(">>> Entering NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
        var serviceProvider = FakeServiceProvider.Setup(services => services.AddTransient<IDictionary, Dictionary<int, int>>());
        var container = new ContainerWithServiceProviderFallback(new SimpleContainer(), serviceProvider);
		Logger.LogDebug("NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): serviceProvider → {ServiceProvider}", serviceProvider);
		Logger.LogDebug("NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull(): container → {Container}", container);

        Assert.That(container.GetComponent("Dictionary", typeof(IDictionary)), Is.Null);
		Logger.LogDebug("<<< Exiting NonNamedGenericGetComponent_WhenOnlyRegisteredInServiceProvider_ReturnsNull()");
    }
}
