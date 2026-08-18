// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Tests
{
    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Services.Dto;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;
	using Microsoft.Extensions.Logging;

    [TestFixture]
    public class PagesControllerUnitTests
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private Mock<IBusinessControllerProvider> businessControllerProviderMock;
        private Mock<ITabController> tabControllerMock;
        private Mock<IModuleController> moduleControllerMock;
        private Mock<IPageUrlsController> pageUrlsControllerMock;
        private Mock<ITemplateController> templateControllerMock;
        private Mock<IDefaultPortalThemeController> defaultPortalThemeControllerMock;
        private Mock<ICloneModuleExecutionContext> cloneModuleExecutionContextMock;
        private Mock<IUrlRewriterUtilsWrapper> urlRewriterUtilsWrapperMock;
        private Mock<IFriendlyUrlWrapper> friendlyUrlWrapperMock;
        private Mock<IContentVerifier> contentVerifierMock;
        private PagesControllerImpl pagesController;
        private Mock<IPortalController> portalControllerMock;
        private Mock<PersonalizationController> personalizationControllerMock;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void RunBeforeEachTest()
        {
			_logger.LogDebug(">>> Entering RunBeforeEachTest()");
            this.businessControllerProviderMock = new Mock<IBusinessControllerProvider>();
            this.tabControllerMock = new Mock<ITabController>();
			_logger.LogDebug("RunBeforeEachTest(): tabControllerMock → {TabControllerMock}", tabControllerMock);
			_logger.LogDebug("RunBeforeEachTest(): businessControllerProviderMock → {BusinessControllerProviderMock}", businessControllerProviderMock);
            this.moduleControllerMock = new Mock<IModuleController>();
			_logger.LogDebug("RunBeforeEachTest(): moduleControllerMock → {ModuleControllerMock}", moduleControllerMock);
            this.pageUrlsControllerMock = new Mock<IPageUrlsController>();
			_logger.LogDebug("RunBeforeEachTest(): pageUrlsControllerMock → {PageUrlsControllerMock}", pageUrlsControllerMock);
            this.templateControllerMock = new Mock<ITemplateController>();
			_logger.LogDebug("RunBeforeEachTest(): templateControllerMock → {TemplateControllerMock}", templateControllerMock);
            this.defaultPortalThemeControllerMock = new Mock<IDefaultPortalThemeController>();
			_logger.LogDebug("RunBeforeEachTest(): defaultPortalThemeControllerMock → {DefaultPortalThemeControllerMock}", defaultPortalThemeControllerMock);
            this.cloneModuleExecutionContextMock = new Mock<ICloneModuleExecutionContext>();
			_logger.LogDebug("RunBeforeEachTest(): cloneModuleExecutionContextMock → {CloneModuleExecutionContextMock}", cloneModuleExecutionContextMock);
            this.urlRewriterUtilsWrapperMock = new Mock<IUrlRewriterUtilsWrapper>();
			_logger.LogDebug("RunBeforeEachTest(): urlRewriterUtilsWrapperMock → {UrlRewriterUtilsWrapperMock}", urlRewriterUtilsWrapperMock);
            this.friendlyUrlWrapperMock = new Mock<IFriendlyUrlWrapper>();
			_logger.LogDebug("RunBeforeEachTest(): friendlyUrlWrapperMock → {FriendlyUrlWrapperMock}", friendlyUrlWrapperMock);
            this.contentVerifierMock = new Mock<IContentVerifier>();
			_logger.LogDebug("RunBeforeEachTest(): contentVerifierMock → {ContentVerifierMock}", contentVerifierMock);
            this.portalControllerMock = new Mock<IPortalController>();
			_logger.LogDebug("RunBeforeEachTest(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
            this.personalizationControllerMock = new Mock<PersonalizationController>(Mock.Of<IHostSettings>(), Mock.Of<ICryptographyProvider>());
			_logger.LogDebug("RunBeforeEachTest(): personalizationControllerMock → {PersonalizationControllerMock}", personalizationControllerMock);
            TabController.SetTestableInstance(this.tabControllerMock.Object);
            ModuleController.SetTestableInstance(this.moduleControllerMock.Object);
            PageUrlsController.SetTestableInstance(this.pageUrlsControllerMock.Object);
            DefaultPortalThemeController.SetTestableInstance(this.defaultPortalThemeControllerMock.Object);
            CloneModuleExecutionContext.SetTestableInstance(this.cloneModuleExecutionContextMock.Object);
            PortalController.SetTestableInstance(this.portalControllerMock.Object);
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					_logger.LogDebug("RunBeforeEachTest(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.businessControllerProviderMock.Object);
                    services.AddSingleton(this.tabControllerMock.Object);
                    services.AddSingleton(this.moduleControllerMock.Object);
                    services.AddSingleton(this.pageUrlsControllerMock.Object);
                    services.AddSingleton(this.templateControllerMock.Object);
                    services.AddSingleton(this.defaultPortalThemeControllerMock.Object);
                    services.AddSingleton(this.cloneModuleExecutionContextMock.Object);
                    services.AddSingleton(this.urlRewriterUtilsWrapperMock.Object);
                    services.AddSingleton(this.friendlyUrlWrapperMock.Object);
                    services.AddSingleton(this.contentVerifierMock.Object);
                    services.AddSingleton(this.portalControllerMock.Object);
                    services.AddSingleton(this.personalizationControllerMock.Object);
                });
				_logger.LogDebug("<<< Exiting RunBeforeEachTest()");
        }

        [TearDown]
        public void TearDown()
        {
			_logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            TabController.ClearInstance();
            ModuleController.ClearInstance();
            PageUrlsController.ClearInstance();
            DefaultPortalThemeController.ClearInstance();
            CloneModuleExecutionContext.ClearInstance();
            PortalController.ClearInstance();
			_logger.LogDebug("<<< Exiting TearDown()");
        }

        [TestCase("http://www.websitename.com/home/", "/home")]
        [TestCase("/news/", "/news")]
        [TestCase("contactus/", "contactus")]
        [TestCase("blogs", "blogs")]
        public void ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(string inputUrl, string expected)
        {
			_logger.LogDebug(">>> Entering ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(inputUrl={InputUrl},expected={Expected})", inputUrl, expected);
            // Arrange
            var modified = false;
            var tab = new TabInfo();
			_logger.LogDebug("ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(inputUrl={InputUrl},expected={Expected}): tab → {Tab}", inputUrl, expected, tab);
            var portalSettings = new PortalSettings();
			_logger.LogDebug("ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(inputUrl={InputUrl},expected={Expected}): portalSettings → {PortalSettings}", inputUrl, expected, portalSettings);
            var friendlyOptions = new FriendlyUrlOptions();
			_logger.LogDebug("ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(inputUrl={InputUrl},expected={Expected}): friendlyOptions → {FriendlyOptions}", inputUrl, expected, friendlyOptions);

            this.urlRewriterUtilsWrapperMock.Setup(d => d.GetExtendOptionsForURLs(It.IsAny<int>())).Returns(friendlyOptions);
            this.friendlyUrlWrapperMock.Setup(d => d.CleanNameForUrl(It.IsAny<string>(), friendlyOptions, out modified)).Returns(expected);
            this.friendlyUrlWrapperMock.Setup(d => d.ValidateUrl(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PortalSettings>(), out modified));

            this.InitializePageController();

            PageSettings pageSettings = new PageSettings();
			_logger.LogDebug("ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(inputUrl={InputUrl},expected={Expected}): pageSettings → {PageSettings}", inputUrl, expected, pageSettings);
			_logger.LogDebug("ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(inputUrl={InputUrl},expected={Expected}): pageSettings.Url → {Arg2}", inputUrl, expected, pageSettings.Url);
            pageSettings.Url = inputUrl;

            string inValidField = string.Empty;
            string errorMessage = string.Empty;

            // Act
            bool result = this.pagesController.ValidatePageUrlSettings(portalSettings, pageSettings, tab, ref inValidField, ref errorMessage);

            // Assert
            Assert.That(result, Is.True);
            this.urlRewriterUtilsWrapperMock.VerifyAll();
            this.friendlyUrlWrapperMock.Verify(d => d.CleanNameForUrl(expected, friendlyOptions, out modified), Times.Once());
			_logger.LogDebug("<<< Exiting ValidatePageUrlSettings_CleanNameForUrl_URLArgumentShouldBeLocalPath(inputUrl={InputUrl},expected={Expected})", inputUrl, expected);
        }

        [Test]
        public void GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull()
        {
			_logger.LogDebug(">>> Entering GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull()");
            var tabId = 0;
            var portalId = 0;

            var tab = new TabInfo();
			_logger.LogDebug("GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull(): tab → {Tab}", tab);
			_logger.LogDebug("GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull(): tab.PortalID → {Arg0}", tab.PortalID);
            tab.PortalID = portalId;

            var portalSettings = new PortalSettings();
			_logger.LogDebug("GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull(): portalSettings → {PortalSettings}", portalSettings);

            // Arrange
            this.tabControllerMock.Setup(t => t.GetTab(It.IsAny<int>(), It.IsAny<int>())).Returns(tab);
            this.portalControllerMock.Setup(p => p.GetCurrentSettings()).Returns(portalSettings);
            this.portalControllerMock.Setup(p => p.GetCurrentPortalSettings()).Returns(portalSettings);
            this.portalControllerMock.Setup(p => p.GetCurrentSettings()).Returns(portalSettings);
            this.contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(It.IsAny<int>(), It.IsAny<PortalSettings>(), false)).Returns(false);

            this.InitializePageController();

            // Act
            TestDelegate pageSettingsCall = () => this.pagesController.GetPageSettings(tabId, null);
			_logger.LogDebug("GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull(): pageSettingsCall → {PageSettingsCall}", pageSettingsCall);

            // Assert
            Assert.Throws<PageNotFoundException>(pageSettingsCall);
            this.portalControllerMock.Verify(p => p.GetCurrentSettings(), Times.Once);
            this.portalControllerMock.Verify(p => p.GetCurrentPortalSettings(), Times.Once);
            this.contentVerifierMock.Verify(c => c.IsContentExistsForRequestedPortal(portalId, portalSettings, false));
			_logger.LogDebug("<<< Exiting GetPageSettings_CallGetCurrentPortalSettings_WhenSettingParameterIsNull()");
        }

        private void InitializePageController()
        {
			_logger.LogDebug(">>> Entering InitializePageController()");
            this.pagesController = new PagesControllerImpl(
                this.businessControllerProviderMock.Object,
                this.tabControllerMock.Object,
                this.moduleControllerMock.Object,
                this.pageUrlsControllerMock.Object,
                this.templateControllerMock.Object,
                this.defaultPortalThemeControllerMock.Object,
                this.cloneModuleExecutionContextMock.Object,
                this.urlRewriterUtilsWrapperMock.Object,
                this.friendlyUrlWrapperMock.Object,
                this.contentVerifierMock.Object,
                this.portalControllerMock.Object,
                this.personalizationControllerMock.Object);
				_logger.LogDebug("InitializePageController(): pagesController → {PagesController}", pagesController);
				_logger.LogDebug("<<< Exiting InitializePageController()");
        }
    }
}
