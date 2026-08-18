// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Common
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities.Fakes;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class NavigationManagerTests
    {
        private const int TabID = 100;
        private const int PortalID = 7;
        private const string DefaultURLPattern = "/Default.aspx?tabid={0}";
        private const string DefaultSuperTabPattern = "&portalid={0}";
        private const string ControlKeyPattern = "&ctl={0}";
        private const string LanguagePattern = "&language={0}";
        private INavigationManager navigationManager;
        private FakeServiceProvider serviceProvider;

        [OneTimeSetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            var portalControllerMock = PortalControllerMock();
            this.navigationManager = new NavigationManager(portalControllerMock, Mock.Of<IHostSettings>());
			mockEventLogger.LogDebug("Setup(): navigationManager → {NavigationManager}", navigationManager);
			mockEventLogger.LogDebug("Setup(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
            PortalController.SetTestableInstance(portalControllerMock);
            TabController.SetTestableInstance(TabControllerMock());
            LocaleController.SetTestableInstance(LocaleControllerMock());

            IPortalController PortalControllerMock()
            {
                var mockPortalController = new Mock<IPortalController>();
				mockEventLogger.LogDebug("Setup(): mockPortalController → {MockPortalController}", mockPortalController);
                mockPortalController
                    .Setup(x => x.GetCurrentPortalSettings())
                    .Returns(PortalSettingsMock());
                mockPortalController
                    .Setup(x => x.GetCurrentSettings())
                    .Returns(PortalSettingsMock());

                return mockPortalController.Object;

                PortalSettings PortalSettingsMock()
                {
                    var portalSettings = new PortalSettings
                    {
                        PortalId = PortalID,
                        ActiveTab = new TabInfo
                        {
                            TabID = TabID,
                        },
                    };

                    return portalSettings;
                }
            }

            ITabController TabControllerMock()
            {
                var mockTabController = new Mock<ITabController>();
				mockEventLogger.LogDebug("Setup(): mockTabController → {MockTabController}", mockTabController);
                mockTabController
                    .Setup(x => x.GetTabsByPortal(Null.NullInteger))
                    .Returns(default(TabCollection));
                mockTabController
                    .Setup(x => x.GetTab(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(new TabInfo
                    {
                        CultureCode = "en-US",
                    });

                return mockTabController.Object;
            }

            ILocaleController LocaleControllerMock()
            {
                var mockLocaleController = new Mock<ILocaleController>();
				mockEventLogger.LogDebug("Setup(): mockLocaleController → {MockLocaleController}", mockLocaleController);
                mockLocaleController
                    .Setup(x => x.GetLocales(It.IsAny<int>()))
                    .Returns(new Dictionary<string, Locale>
                    {
                        { "en-US", new Locale() },
                        { "TEST", new Locale() },
                    });

                return mockLocaleController.Object;
            }

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(PortalController.Instance);
                    services.AddSingleton(TabController.Instance);
                    services.AddSingleton(LocaleController.Instance);
                });
				mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            this.navigationManager = null;
			mockEventLogger.LogDebug("TearDown(): navigationManager → {NavigationManager}", navigationManager);
			mockEventLogger.LogDebug("TearDown(): this.navigationManager → {Arg0}", this.navigationManager);
            TabController.ClearInstance();
            LocaleController.ClearInstance();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void NavigateUrlTest()
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrlTest()");
            var expected = string.Format(DefaultURLPattern, TabID);
            var actual = this.navigationManager.NavigateURL();
			mockEventLogger.LogDebug("NavigateUrlTest(): actual → {Actual}", actual);
			mockEventLogger.LogDebug("NavigateUrlTest(): expected → {Expected}", expected);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrlTest()");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_CustomTabID(int tabId)
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_CustomTabID(tabId={TabId})", tabId);
            var expected = string.Format(DefaultURLPattern, tabId);
            var actual = this.navigationManager.NavigateURL(tabId);
			mockEventLogger.LogDebug("NavigateUrl_CustomTabID(tabId={TabId}): actual → {Actual}", tabId, actual);
			mockEventLogger.LogDebug("NavigateUrl_CustomTabID(tabId={TabId}): expected → {Expected}", tabId, expected);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_CustomTabID(tabId={TabId})", tabId);
        }

        [Test]
        public void NavigateUrl_CustomTab_NotSuperTab()
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_CustomTab_NotSuperTab()");
            var customTabId = 55;
            var expected = string.Format(DefaultURLPattern, customTabId);
			mockEventLogger.LogDebug("NavigateUrl_CustomTab_NotSuperTab(): expected → {Expected}", expected);
            var actual = this.navigationManager.NavigateURL(customTabId, false);
			mockEventLogger.LogDebug("NavigateUrl_CustomTab_NotSuperTab(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_CustomTab_NotSuperTab()");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_CustomTab_IsSuperTab(int tabId)
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_CustomTab_IsSuperTab(tabId={TabId})", tabId);
            var expected = string.Format(DefaultURLPattern, tabId) + string.Format(DefaultSuperTabPattern, PortalID);
            var actual = this.navigationManager.NavigateURL(tabId, true);
			mockEventLogger.LogDebug("NavigateUrl_CustomTab_IsSuperTab(tabId={TabId}): actual → {Actual}", tabId, actual);
			mockEventLogger.LogDebug("NavigateUrl_CustomTab_IsSuperTab(tabId={TabId}): expected → {Expected}", tabId, expected);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_CustomTab_IsSuperTab(tabId={TabId})", tabId);
        }

        [Test]
        [Ignore("TODO - We can't properly test this until we migrate Globals.AccessDeniedURL to an interface in the abstraction project")]
        public void NavigateUrl_ControlKey_AccessDenied()
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_ControlKey_AccessDenied()");
            // The dependencies go very deep and make it very
            // difficult to properly test just the NavigationManager logic.
            var actual = this.navigationManager.NavigateURL("Access Denied");
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_AccessDenied(): actual → {Actual}", actual);
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_ControlKey_AccessDenied()");
        }

        [Test]
        public void NavigateUrl_ControlKey()
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_ControlKey()");
            var controlKey = "My-Control-Key";
            var expected = string.Format(DefaultURLPattern, TabID) + string.Format(ControlKeyPattern, controlKey);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey(): expected → {Expected}", expected);
            var actual = this.navigationManager.NavigateURL(controlKey);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey(): actual → {Actual}", actual);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey(): controlKey → {ControlKey}", controlKey);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_ControlKey()");
        }

        [Test]
        public void NavigateUrl_ControlKey_EmptyAdditionalParameter()
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_ControlKey_EmptyAdditionalParameter()");
            var controlKey = "My-Control-Key";
            var expected = string.Format(DefaultURLPattern, TabID) + string.Format(ControlKeyPattern, controlKey);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_EmptyAdditionalParameter(): expected → {Expected}", expected);
            var actual = this.navigationManager.NavigateURL(controlKey, []);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_EmptyAdditionalParameter(): actual → {Actual}", actual);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_EmptyAdditionalParameter(): controlKey → {ControlKey}", controlKey);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_ControlKey_EmptyAdditionalParameter()");
        }

        [Test]
        public void NavigateUrl_ControlKey_SingleAdditionalParameter()
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_ControlKey_SingleAdditionalParameter()");
            var controlKey = "My-Control-Key";
            var parameters = new string[] { "My-Parameter" };
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_SingleAdditionalParameter(): parameters → {Parameters}", parameters);
            var expected = string.Format(DefaultURLPattern, TabID) +
                string.Format(ControlKeyPattern, controlKey) +
                $"&{parameters[0]}";
				mockEventLogger.LogDebug("NavigateUrl_ControlKey_SingleAdditionalParameter(): controlKey → {ControlKey}", controlKey);
            var actual = this.navigationManager.NavigateURL(controlKey, parameters);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_SingleAdditionalParameter(): actual → {Actual}", actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_ControlKey_SingleAdditionalParameter()");
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        public void NavigateUrl_ControlKey_MultipleAdditionalParameter(int count)
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_ControlKey_MultipleAdditionalParameter(count={Count})", count);
            string[] parameters = new string[count];
            for (int index = 0; index < count; index++)
            {
                parameters[index] = $"My-Parameter{index}";
            }

            var controlKey = "My-Control-Key";
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_MultipleAdditionalParameter(count={Count}): controlKey → {ControlKey}", count, controlKey);
            var expected = string.Format(DefaultURLPattern, TabID) +
                string.Format(ControlKeyPattern, controlKey) +
                parameters.Select(s => $"&{s}").Aggregate((x, y) => $"{x}{y}");
            var actual = this.navigationManager.NavigateURL(controlKey, parameters);
			mockEventLogger.LogDebug("NavigateUrl_ControlKey_MultipleAdditionalParameter(count={Count}): actual → {Actual}", count, actual);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_ControlKey_MultipleAdditionalParameter(count={Count})", count);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_TabID_ControlKey(int tabId)
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_TabID_ControlKey(tabId={TabId})", tabId);
            var controlKey = "My-Control-Key";
            var expected = string.Format(DefaultURLPattern, tabId) + string.Format(ControlKeyPattern, controlKey);
			mockEventLogger.LogDebug("NavigateUrl_TabID_ControlKey(tabId={TabId}): expected → {Expected}", tabId, expected);
            var actual = this.navigationManager.NavigateURL(tabId, controlKey);
			mockEventLogger.LogDebug("NavigateUrl_TabID_ControlKey(tabId={TabId}): actual → {Actual}", tabId, actual);
			mockEventLogger.LogDebug("NavigateUrl_TabID_ControlKey(tabId={TabId}): controlKey → {ControlKey}", tabId, controlKey);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_TabID_ControlKey(tabId={TabId})", tabId);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_TabID_EmptyControlKey(int tabId)
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_TabID_EmptyControlKey(tabId={TabId})", tabId);
            var expected = string.Format(DefaultURLPattern, tabId);
            var actual = this.navigationManager.NavigateURL(tabId, string.Empty);
			mockEventLogger.LogDebug("NavigateUrl_TabID_EmptyControlKey(tabId={TabId}): actual → {Actual}", tabId, actual);
			mockEventLogger.LogDebug("NavigateUrl_TabID_EmptyControlKey(tabId={TabId}): expected → {Expected}", tabId, expected);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_TabID_EmptyControlKey(tabId={TabId})", tabId);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_TabID_NullControlKey(int tabId)
        {
			mockEventLogger.LogDebug(">>> Entering NavigateUrl_TabID_NullControlKey(tabId={TabId})", tabId);
            var expected = string.Format(DefaultURLPattern, tabId);
            var actual = this.navigationManager.NavigateURL(tabId, string.Empty);
			mockEventLogger.LogDebug("NavigateUrl_TabID_NullControlKey(tabId={TabId}): actual → {Actual}", tabId, actual);
			mockEventLogger.LogDebug("NavigateUrl_TabID_NullControlKey(tabId={TabId}): expected → {Expected}", tabId, expected);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_TabID_NullControlKey(tabId={TabId})", tabId);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabID_ControlKey_Parameter(int count, string controlKey)
        {
            string[] parameters = new string[count];
            for (int index = 0; index < count; index++)
            {
                parameters[index] = $"My-Parameter{index}";
            }

            var customTabId = 51;
            var expected = string.Format(DefaultURLPattern, customTabId) +
                string.Format(ControlKeyPattern, controlKey);

            if (parameters.Length > 0)
            {
                expected += parameters.Select(s => $"&{s}").Aggregate((x, y) => $"{x}{y}");
            }

            var actual = this.navigationManager.NavigateURL(customTabId, controlKey, parameters);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_TabID_ControlKey_Parameter(count={Count},controlKey={ControlKey})", count, controlKey);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabID_ControlKey_NullParameter(int tabId, string controlKey)
        {
            var expected = string.Format(DefaultURLPattern, tabId) +
                string.Format(ControlKeyPattern, controlKey);

            var actual = this.navigationManager.NavigateURL(tabId, controlKey, null);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_TabID_ControlKey_NullParameter(tabId={TabId},controlKey={ControlKey})", tabId, controlKey);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabId_NullSettings_ControlKey(int tabId, string controlKey)
        {
            var expected = string.Format(DefaultURLPattern, tabId) +
                string.Format(ControlKeyPattern, controlKey);

            var actual = this.navigationManager.NavigateURL(tabId, default(IPortalSettings), controlKey, null);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_TabId_NullSettings_ControlKey(tabId={TabId},controlKey={ControlKey})", tabId, controlKey);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabId_Settings_ControlKey(int tabId, string controlKey)
        {
            var mockSettings = new Mock<IPortalSettings>();
            mockSettings
                .Setup(x => x.ContentLocalizationEnabled)
                .Returns(true);

            var expected = string.Format(DefaultURLPattern, tabId) +
                string.Format(ControlKeyPattern, controlKey) +
                string.Format(LanguagePattern, "en-US");

            var actual = this.navigationManager.NavigateURL(tabId, mockSettings.Object, controlKey, null);

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting NavigateUrl_TabId_Settings_ControlKey(tabId={TabId},controlKey={ControlKey})", tabId, controlKey);
        }
    }
}
