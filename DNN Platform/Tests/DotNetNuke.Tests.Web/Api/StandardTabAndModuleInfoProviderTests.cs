// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Net.Http;
    using System.Web;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.Api;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class StandardTabAndModuleInfoProviderTests
    {
        private const int ValidPortalId = 0;
        private const int ValidTabModuleId = 999;
        private const int ValidModuleId = 456;
        private const int ValidTabId = 46;

        private const string MonikerSettingName = "Moniker";
        private const string MonikerSettingValue = "TestMoniker";

        private Mock<IModuleController> _mockModuleController;
        private IModuleController _moduleController;
        private Mock<ITabController> _mockTabController;
        private Mock<ITabModulesController> _mockTabModuleController;
        private Mock<DataProvider> _mockDataProvider;

        private ITabController _tabController;
        private ITabModulesController _tabModuleController;
        private TabInfo _tabInfo;
        private ModuleInfo _moduleInfo;

        [SetUp]
        public void Setup()
        {
			eventLogger.LogDebug(">>> Entering Setup()");
            MockComponentProvider.CreateDataCacheProvider();
            this._mockDataProvider = MockComponentProvider.CreateDataProvider();
			eventLogger.LogDebug("Setup(): _mockDataProvider → {_mockDataProvider}", _mockDataProvider);
            this._mockDataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
            this._mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);

            this.RegisterMock(ModuleController.SetTestableInstance, out this._mockModuleController, out this._moduleController);
            this.RegisterMock(TabController.SetTestableInstance, out this._mockTabController, out this._tabController);
            this.RegisterMock(TabModulesController.SetTestableInstance, out this._mockTabModuleController, out this._tabModuleController);

            this._tabInfo = new TabInfo { TabID = ValidTabId };
			eventLogger.LogDebug("Setup(): _tabInfo → {_tabInfo}", _tabInfo);
			eventLogger.LogDebug("Setup(): this._tabInfo → {Arg0}", this._tabInfo);
            this._moduleInfo = new ModuleInfo
            {
				eventLogger.LogDebug("Setup(): _moduleInfo → {_moduleInfo}", _moduleInfo);
                TabModuleID = ValidTabModuleId,
                TabID = ValidTabId,
                ModuleID = ValidModuleId,
                PortalID = ValidPortalId,
            };
			eventLogger.LogDebug("Setup(): this._moduleInfo → {Arg0}", this._moduleInfo);

            this._mockTabController.Setup(x => x.GetTab(ValidTabId, ValidPortalId)).Returns(this._tabInfo);
            this._mockModuleController.Setup(x => x.GetModule(ValidModuleId, ValidTabId, false)).Returns(this._moduleInfo);
            this._mockModuleController.Setup(x => x.GetTabModule(ValidTabModuleId)).Returns(this._moduleInfo);
            this._mockTabModuleController.Setup(x => x.GetTabModuleIdsBySetting(MonikerSettingName, MonikerSettingValue)).Returns(
                new List<int> { ValidTabModuleId });
            this._mockTabModuleController.Setup(x => x.GetTabModuleSettingsByName(MonikerSettingName)).Returns(
                new Dictionary<int, string> { { ValidTabModuleId, MonikerSettingValue } });
				eventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			eventLogger.LogDebug(">>> Entering TearDown()");
            ModuleController.ClearInstance();
            TabController.ClearInstance();
			eventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        public void ValidTabAndModuleIdLoadsActiveModule()
        {
			eventLogger.LogDebug(">>> Entering ValidTabAndModuleIdLoadsActiveModule()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("ValidTabAndModuleIdLoadsActiveModule(): request → {Request}", request);
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.Headers.Add("moduleid", ValidModuleId.ToString(CultureInfo.InvariantCulture));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);
			eventLogger.LogDebug("ValidTabAndModuleIdLoadsActiveModule(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.True);
                Assert.That(returnedModuleInfo, Is.SameAs(this._moduleInfo));
            }
			eventLogger.LogDebug("<<< Exiting ValidTabAndModuleIdLoadsActiveModule()");
        }

        [Test]
        public void ExistingMonikerValueInHeaderShouldFindTheCorrectModuleInfo()
        {
			eventLogger.LogDebug(">>> Entering ExistingMonikerValueInHeaderShouldFindTheCorrectModuleInfo()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("ExistingMonikerValueInHeaderShouldFindTheCorrectModuleInfo(): request → {Request}", request);
            request.Headers.Add("X-DNN-MONIKER", MonikerSettingValue);

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);
			eventLogger.LogDebug("ExistingMonikerValueInHeaderShouldFindTheCorrectModuleInfo(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.True);
                Assert.That(returnedModuleInfo, Is.SameAs(this._moduleInfo));
            }
			eventLogger.LogDebug("<<< Exiting ExistingMonikerValueInHeaderShouldFindTheCorrectModuleInfo()");
        }

        [Test]
        public void ExistingMonikerValueInQueryStringShouldFindTheCorrectModuleInfo()
        {
			eventLogger.LogDebug(">>> Entering ExistingMonikerValueInQueryStringShouldFindTheCorrectModuleInfo()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("ExistingMonikerValueInQueryStringShouldFindTheCorrectModuleInfo(): request → {Request}", request);
            request.RequestUri = new Uri(
                "http://mysite.lvh.me/API/internalservices/controlbar/ToggleUserMode?moniker=" + HttpUtility.UrlEncode(MonikerSettingValue));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);
			eventLogger.LogDebug("ExistingMonikerValueInQueryStringShouldFindTheCorrectModuleInfo(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.True);
                Assert.That(returnedModuleInfo, Is.SameAs(this._moduleInfo));
            }
			eventLogger.LogDebug("<<< Exiting ExistingMonikerValueInQueryStringShouldFindTheCorrectModuleInfo()");
        }

        [Test]
        public void NonExistingMonikerValueShouldFailToReturnResult()
        {
			eventLogger.LogDebug(">>> Entering NonExistingMonikerValueShouldFailToReturnResult()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("NonExistingMonikerValueShouldFailToReturnResult(): request → {Request}", request);
            request.Headers.Add("X-DNN-MONIKER", "This moniker does not exist");

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);
			eventLogger.LogDebug("NonExistingMonikerValueShouldFailToReturnResult(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.False);
                Assert.That(returnedModuleInfo, Is.Null);
            }
			eventLogger.LogDebug("<<< Exiting NonExistingMonikerValueShouldFailToReturnResult()");
        }

        [Test]
        public void MissingAllHeadersShouldFailToReturnResult()
        {
			eventLogger.LogDebug(">>> Entering MissingAllHeadersShouldFailToReturnResult()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("MissingAllHeadersShouldFailToReturnResult(): request → {Request}", request);

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);
			eventLogger.LogDebug("MissingAllHeadersShouldFailToReturnResult(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.False);
                Assert.That(returnedModuleInfo, Is.Null);
            }
			eventLogger.LogDebug("<<< Exiting MissingAllHeadersShouldFailToReturnResult()");
        }

        [Test]
        public void OmittedTabIdWillNotLoadModule()
        {
			eventLogger.LogDebug(">>> Entering OmittedTabIdWillNotLoadModule()");
            // Arrange
            // no tabid
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("OmittedTabIdWillNotLoadModule(): request → {Request}", request);
            request.Headers.Add("moduleid", ValidModuleId.ToString(CultureInfo.InvariantCulture));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);
			eventLogger.LogDebug("OmittedTabIdWillNotLoadModule(): result → {Result}", result);

            // Assert
            this._mockTabController.Verify(x => x.GetTab(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
            this._mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(returnedModuleInfo, Is.Null);
                Assert.That(result, Is.False);
            }
			eventLogger.LogDebug("<<< Exiting OmittedTabIdWillNotLoadModule()");
        }

        [Test]
        public void OmittedModuleIdWillNotLoadModule()
        {
			eventLogger.LogDebug(">>> Entering OmittedModuleIdWillNotLoadModule()");
            // Arrange
            // no moduleid
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("OmittedModuleIdWillNotLoadModule(): request → {Request}", request);
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));

            // Act
            ModuleInfo returnedModuleInfo;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleInfo(request, out returnedModuleInfo);
			eventLogger.LogDebug("OmittedModuleIdWillNotLoadModule(): result → {Result}", result);

            // Assert
            this._mockModuleController.Verify(x => x.GetModule(It.IsAny<int>(), It.IsAny<int>(), false), Times.Never());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(returnedModuleInfo, Is.Null);
                Assert.That(result, Is.False);
            }
			eventLogger.LogDebug("<<< Exiting OmittedModuleIdWillNotLoadModule()");
        }

        [Test]
        public void TabIdInHeaderTakesPriority()
        {
			eventLogger.LogDebug(">>> Entering TabIdInHeaderTakesPriority()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("TabIdInHeaderTakesPriority(): request → {Request}", request);
            request.Headers.Add("tabid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", "tabid", ValidTabId + 1));

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);
			eventLogger.LogDebug("TabIdInHeaderTakesPriority(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(tabId, Is.EqualTo(ValidTabId));
                Assert.That(result, Is.True);
            }
			eventLogger.LogDebug("<<< Exiting TabIdInHeaderTakesPriority()");
        }

        [Test]
        public void ModuleIdInHeaderTakesPriority()
        {
			eventLogger.LogDebug(">>> Entering ModuleIdInHeaderTakesPriority()");
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("ModuleIdInHeaderTakesPriority(): request → {Request}", request);
            request.Headers.Add("moduleid", ValidTabId.ToString(CultureInfo.InvariantCulture));
            request.RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", "moduleid", ValidTabId + 1));

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);
			eventLogger.LogDebug("ModuleIdInHeaderTakesPriority(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(moduleId, Is.EqualTo(ValidTabId));
                Assert.That(result, Is.True);
            }
			eventLogger.LogDebug("<<< Exiting ModuleIdInHeaderTakesPriority()");
        }

        [Test]
        [TestCase("tabid")]
        [TestCase("TABID")]
        [TestCase("tAbiD")]
        public void TabIdInHeaderAllowsTabIdToBeFound(string headerName)
        {
			eventLogger.LogDebug(">>> Entering TabIdInHeaderAllowsTabIdToBeFound(headerName={HeaderName})", headerName);
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("TabIdInHeaderAllowsTabIdToBeFound(headerName={HeaderName}): request → {Request}", headerName, request);
            request.Headers.Add(headerName, ValidTabId.ToString(CultureInfo.InvariantCulture));

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);
			eventLogger.LogDebug("TabIdInHeaderAllowsTabIdToBeFound(headerName={HeaderName}): result → {Result}", headerName, result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(tabId, Is.EqualTo(ValidTabId));
                Assert.That(result, Is.True);
            }
			eventLogger.LogDebug("<<< Exiting TabIdInHeaderAllowsTabIdToBeFound(headerName={HeaderName})", headerName);
        }

        [Test]
        [TestCase("moduleid")]
        [TestCase("MODULEID")]
        [TestCase("modULeid")]
        public void ModuleIdInHeaderAllowsModuleIdToBeFound(string headerName)
        {
			eventLogger.LogDebug(">>> Entering ModuleIdInHeaderAllowsModuleIdToBeFound(headerName={HeaderName})", headerName);
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("ModuleIdInHeaderAllowsModuleIdToBeFound(headerName={HeaderName}): request → {Request}", headerName, request);
            request.Headers.Add(headerName, ValidModuleId.ToString(CultureInfo.InvariantCulture));

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);
			eventLogger.LogDebug("ModuleIdInHeaderAllowsModuleIdToBeFound(headerName={HeaderName}): result → {Result}", headerName, result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(moduleId, Is.EqualTo(ValidModuleId));
                Assert.That(result, Is.True);
            }
			eventLogger.LogDebug("<<< Exiting ModuleIdInHeaderAllowsModuleIdToBeFound(headerName={HeaderName})", headerName);
        }

        [Test]
        [TestCase("x-dnn-moniker")]
        [TestCase("X-Dnn-Moniker")]
        [TestCase("X-DNN-MONIKER")]
        public void MonikerInHeaderAllowsModuleToBeFound(string headerName)
        {
			eventLogger.LogDebug(">>> Entering MonikerInHeaderAllowsModuleToBeFound(headerName={HeaderName})", headerName);
            // Arrange
            var request = new HttpRequestMessage();
			eventLogger.LogDebug("MonikerInHeaderAllowsModuleToBeFound(headerName={HeaderName}): request → {Request}", headerName, request);
            request.Headers.Add(headerName, MonikerSettingValue);

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);
			eventLogger.LogDebug("MonikerInHeaderAllowsModuleToBeFound(headerName={HeaderName}): result → {Result}", headerName, result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.True);
                Assert.That(moduleId, Is.EqualTo(ValidModuleId));
            }
			eventLogger.LogDebug("<<< Exiting MonikerInHeaderAllowsModuleToBeFound(headerName={HeaderName})", headerName);
        }

        [Test]
        [TestCase("tabid")]
        [TestCase("TABID")]
        [TestCase("tAbiD")]
        public void TabIdInQueryStringAllowsTabIdToBeFound(string paramName)
        {
			eventLogger.LogDebug(">>> Entering TabIdInQueryStringAllowsTabIdToBeFound(paramName={ParamName})", paramName);
            // Arrange
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidTabId)),
            };

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(request, out tabId);
			eventLogger.LogDebug("TabIdInQueryStringAllowsTabIdToBeFound(paramName={ParamName}): result → {Result}", paramName, result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(tabId, Is.EqualTo(ValidTabId));
                Assert.That(result, Is.True);
            }
			eventLogger.LogDebug("<<< Exiting TabIdInQueryStringAllowsTabIdToBeFound(paramName={ParamName})", paramName);
        }

        [Test]
        [TestCase("moduleid")]
        [TestCase("MODULEID")]
        [TestCase("modULeid")]
        public void ModuleIdInQueryStringAllowsModuleIdToBeFound(string paramName)
        {
			eventLogger.LogDebug(">>> Entering ModuleIdInQueryStringAllowsModuleIdToBeFound(paramName={ParamName})", paramName);
            // Arrange
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format("http://foo.com?{0}={1}", paramName, ValidModuleId)),
            };

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(request, out moduleId);
			eventLogger.LogDebug("ModuleIdInQueryStringAllowsModuleIdToBeFound(paramName={ParamName}): result → {Result}", paramName, result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(moduleId, Is.EqualTo(ValidModuleId));
                Assert.That(result, Is.True);
            }
			eventLogger.LogDebug("<<< Exiting ModuleIdInQueryStringAllowsModuleIdToBeFound(paramName={ParamName})", paramName);
        }

        [Test]
        public void NoTabIdInRequestReturnsNoTabId()
        {
			eventLogger.LogDebug(">>> Entering NoTabIdInRequestReturnsNoTabId()");
            // Arrange

            // Act
            int tabId;
            var result = new StandardTabAndModuleInfoProvider().TryFindTabId(new HttpRequestMessage(), out tabId);
			eventLogger.LogDebug("NoTabIdInRequestReturnsNoTabId(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.False);
                Assert.That(tabId, Is.EqualTo(-1));
            }
			eventLogger.LogDebug("<<< Exiting NoTabIdInRequestReturnsNoTabId()");
        }

        [Test]
        public void NoModuleIdInRequestReturnsNoModuleId()
        {
			eventLogger.LogDebug(">>> Entering NoModuleIdInRequestReturnsNoModuleId()");
            // Arrange

            // Act
            int moduleId;
            var result = new StandardTabAndModuleInfoProvider().TryFindModuleId(new HttpRequestMessage(), out moduleId);
			eventLogger.LogDebug("NoModuleIdInRequestReturnsNoModuleId(): result → {Result}", result);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(result, Is.False);
                Assert.That(moduleId, Is.EqualTo(-1));
            }
			eventLogger.LogDebug("<<< Exiting NoModuleIdInRequestReturnsNoModuleId()");
        }

        private static IDataReader GetPortalsCallBack(string culture)
        {
			eventLogger.LogDebug(">>> Entering GetPortalsCallBack(culture={Culture})", culture);
			eventLogger.LogDebug("<<< Exiting GetPortalsCallBack(culture={Culture})", culture);
            return GetPortalCallBack(0, culture);
        }

        private static IDataReader GetPortalCallBack(int portalId, string culture)
        {
			eventLogger.LogDebug(">>> Entering GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            var table = new DataTable("Portal");

			eventLogger.LogDebug("GetPortalCallBack(portalId={PortalId},culture={Culture}): table → {Table}", portalId, culture, table);
            var cols = new[]
            {
                "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate",
                "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
                "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId",
                "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor",
                "ProcessorUserId",
                "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset",
                "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
                "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId",
                "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate",
                "CultureCode",
            };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright (c) 2018 DNN Corp.", null,
                "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website",
                "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020",
                null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0", null,
                homePage.ToString(), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11",
                "-1", "2011-08-25 07:34:29", culture);

			eventLogger.LogDebug("<<< Exiting GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            return table.CreateDataReader();
        }

        private void RegisterMock<T>(Action<T> register, out Mock<T> mock, out T instance)
            where T : class
        {
			eventLogger.LogDebug(">>> Entering RegisterMock(register={Register})", register);
            mock = new Mock<T>();
            instance = mock.Object;
			eventLogger.LogDebug("RegisterMock(register={Register}): mock → {Mock}", register, mock);
			eventLogger.LogDebug("RegisterMock(register={Register}): instance → {Instance}", register, instance);
			eventLogger.LogDebug("<<< Exiting RegisterMock(register={Register})", register);
            register(instance);
        }
    }
}
