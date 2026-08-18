// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Caching;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Tests.Utilities.Fakes;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

[TestFixture]
public class TabCollectionTests
{
    private FakeServiceProvider serviceProvider;

    [SetUp]
    public void SetUp()
    {
		mockEventLogger.LogDebug(">>> Entering SetUp()");
        ComponentFactory.Container = new SimpleContainer();
        MockComponentProvider.CreateDataCacheProvider();
        this.serviceProvider = FakeServiceProvider.Setup(services =>
        {
			mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
            services.AddSingleton(Mock.Of<IPortalController>());
        });
		mockEventLogger.LogDebug("<<< Exiting SetUp()");
    }

    [TearDown]
    public void TearDown()
    {
		mockEventLogger.LogDebug(">>> Entering TearDown()");
        this.serviceProvider.Dispose();
		mockEventLogger.LogDebug("<<< Exiting TearDown()");
    }

    [Test]
    public void DNN_13659_WithTabName_NullTabName()
    {
		mockEventLogger.LogDebug(">>> Entering DNN_13659_WithTabName_NullTabName()");
        // Setup
        var tabCollection = new TabCollection { new TabInfo { PortalID = 1, TabID = 1, }, };

        // Act
        var tab = tabCollection.WithTabName("TestName");
		mockEventLogger.LogDebug("DNN_13659_WithTabName_NullTabName(): tab → {Tab}", tab);

        Assert.That(tab, Is.Null);
		mockEventLogger.LogDebug("<<< Exiting DNN_13659_WithTabName_NullTabName()");
    }

    [Test]
    public void WithTabName_Match()
    {
		mockEventLogger.LogDebug(">>> Entering WithTabName_Match()");
        // Setup
        var tabCollection = new TabCollection
        {
            new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1, },
            new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2, },
            new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3, },
        };

        // Act
        var tab = tabCollection.WithTabName("TestName2");
		mockEventLogger.LogDebug("WithTabName_Match(): tab → {Tab}", tab);

        // Assert
        Assert.That(tab.TabName, Is.EqualTo("TestName2"));
		mockEventLogger.LogDebug("<<< Exiting WithTabName_Match()");
    }

    [Test]
    public void WithTabName_NoMatch()
    {
		mockEventLogger.LogDebug(">>> Entering WithTabName_NoMatch()");
        // Setup
        var tabCollection = new TabCollection
        {
            new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1, },
            new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2, },
            new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3, },
        };

        // Act
        var tab = tabCollection.WithTabName("NO_MATCH");
		mockEventLogger.LogDebug("WithTabName_NoMatch(): tab → {Tab}", tab);

        // Assert
        Assert.That(tab, Is.Null);
		mockEventLogger.LogDebug("<<< Exiting WithTabName_NoMatch()");
    }

    [Test]
    public void WithTabName_Empty()
    {
		mockEventLogger.LogDebug(">>> Entering WithTabName_Empty()");
        // Setup
        var tabCollection = new TabCollection
        {
            new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1, },
            new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2, },
            new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3, },
        };

        // Act
        var tab = tabCollection.WithTabName(string.Empty);
		mockEventLogger.LogDebug("WithTabName_Empty(): tab → {Tab}", tab);

        // Assert
        Assert.That(tab, Is.Null);
		mockEventLogger.LogDebug("<<< Exiting WithTabName_Empty()");
    }
}
