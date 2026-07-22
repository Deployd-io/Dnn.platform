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
	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
    private FakeServiceProvider serviceProvider;

    [SetUp]
    public void SetUp()
    {
		Logger.LogDebug(">>> Entering SetUp()");
        ComponentFactory.Container = new SimpleContainer();
        MockComponentProvider.CreateDataCacheProvider();
        this.serviceProvider = FakeServiceProvider.Setup(services =>
        {
		Logger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
            services.AddSingleton(Mock.Of<IPortalController>());
        });
		Logger.LogDebug("<<< Exiting SetUp()");
    }

    [TearDown]
    public void TearDown()
    {
		Logger.LogDebug(">>> Entering TearDown()");
        this.serviceProvider.Dispose();
		Logger.LogDebug("<<< Exiting TearDown()");
    }

    [Test]
    public void DNN_13659_WithTabName_NullTabName()
    {
		Logger.LogDebug(">>> Entering DNN_13659_WithTabName_NullTabName()");
        // Setup
        var tabCollection = new TabCollection { new TabInfo { PortalID = 1, TabID = 1, }, };

        // Act
        var tab = tabCollection.WithTabName("TestName");

		Logger.LogDebug("DNN_13659_WithTabName_NullTabName(): tab → {Tab}", tab);
        Assert.That(tab, Is.Null);
		Logger.LogDebug("<<< Exiting DNN_13659_WithTabName_NullTabName()");
    }

    [Test]
    public void WithTabName_Match()
    {
		Logger.LogDebug(">>> Entering WithTabName_Match()");
        // Setup
        var tabCollection = new TabCollection
        {
            new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1, },
            new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2, },
            new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3, },
        };

        // Act
        var tab = tabCollection.WithTabName("TestName2");

		Logger.LogDebug("WithTabName_Match(): tab → {Tab}", tab);
        // Assert
        Assert.That(tab.TabName, Is.EqualTo("TestName2"));
		Logger.LogDebug("<<< Exiting WithTabName_Match()");
    }

    [Test]
    public void WithTabName_NoMatch()
    {
		Logger.LogDebug(">>> Entering WithTabName_NoMatch()");
        // Setup
        var tabCollection = new TabCollection
        {
            new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1, },
            new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2, },
            new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3, },
        };

        // Act
        var tab = tabCollection.WithTabName("NO_MATCH");

		Logger.LogDebug("WithTabName_NoMatch(): tab → {Tab}", tab);
        // Assert
        Assert.That(tab, Is.Null);
		Logger.LogDebug("<<< Exiting WithTabName_NoMatch()");
    }

    [Test]
    public void WithTabName_Empty()
    {
		Logger.LogDebug(">>> Entering WithTabName_Empty()");
        // Setup
        var tabCollection = new TabCollection
        {
            new TabInfo { TabName = "TestName1", PortalID = 1, TabID = 1, },
            new TabInfo { TabName = "TestName2", PortalID = 1, TabID = 2, },
            new TabInfo { TabName = "TestName3", PortalID = 1, TabID = 3, },
        };

        // Act
        var tab = tabCollection.WithTabName(string.Empty);

		Logger.LogDebug("WithTabName_Empty(): tab → {Tab}", tab);
        // Assert
        Assert.That(tab, Is.Null);
		Logger.LogDebug("<<< Exiting WithTabName_Empty()");
    }
}
