// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.Tokens
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TokenReplaceTests
    {
        private Mock<CachingProvider> mockCache;
        private Mock<IPortalController> portalController;
        private Mock<IModuleController> moduleController;
        private Mock<IUserController> userController;
        private Mock<IHostController> mockHostController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            ComponentFactory.RegisterComponentInstance<TokenProvider>(new CoreTokenProvider());
            this.mockCache = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("SetUp(): mockCache → {MockCache}", mockCache);
            this.mockHostController = new Mock<IHostController>();
			mockEventLogger.LogDebug("SetUp(): mockHostController → {MockHostController}", mockHostController);
            this.mockHostController.As<IHostSettingsService>();
            this.portalController = new Mock<IPortalController>();
			mockEventLogger.LogDebug("SetUp(): portalController → {PortalController}", portalController);
            this.moduleController = new Mock<IModuleController>();
			mockEventLogger.LogDebug("SetUp(): moduleController → {ModuleController}", moduleController);
            this.userController = new Mock<IUserController>();
			mockEventLogger.LogDebug("SetUp(): userController → {UserController}", userController);
            PortalController.SetTestableInstance(this.portalController.Object);
            ModuleController.SetTestableInstance(this.moduleController.Object);
            UserController.SetTestableInstance(this.userController.Object);
            HostController.RegisterInstance(this.mockHostController.Object);
            this.SetupPortalSettings();
            this.SetupModuleInfo();
            this.SetupUserInfo();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.mockCache.Object);
                    services.AddSingleton((IHostSettingsService)this.mockHostController.Object);
                    services.AddSingleton(this.mockHostController.Object);
                    services.AddSingleton(this.portalController.Object);
                    services.AddSingleton(this.moduleController.Object);
                    services.AddSingleton(this.userController.Object);
                });
				mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
            PortalController.ClearInstance();
            ModuleController.ClearInstance();
            UserController.ClearInstance();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [TestCase("var myArray = [{ foo: 'bar' }]")]
        [TestCase("This is just plain text")]
        public void TextInputIsReturnedUnModified(string sourceText)
        {
			mockEventLogger.LogDebug(">>> Entering TextInputIsReturnedUnModified(sourceText={SourceText})", sourceText);
            // Arrange
            var tokenReplace = new TokenReplace(Scope.DefaultSettings, PortalSettings.Current.DefaultLanguage,
                PortalSettings.Current, UserController.Instance.GetUser(1, 1), 1);

            // Act
            var outputText = tokenReplace.ReplaceEnvironmentTokens(sourceText);
			mockEventLogger.LogDebug("TextInputIsReturnedUnModified(sourceText={SourceText}): outputText → {OutputText}", sourceText, outputText);

            // Assert
            Assert.That(sourceText, Is.EqualTo(outputText));
			mockEventLogger.LogDebug("<<< Exiting TextInputIsReturnedUnModified(sourceText={SourceText})", sourceText);
        }

        [Test]
        [TestCase("[Resx:{key:\"Email\"}]")]
        [TestCase("[Css: { path: \"~/DesktopModules/Dnn/ContactList/stylesheets/bootstrap.min.css\"}]")]
        [TestCase("[JavaScript:{ jsname: \"Knockout\" }] [JavaScript:{ path: \"~/DesktopModules/Dnn/ContactList/ClientScripts/contacts.js\"}]")]
        public void ObjectInputIsReturnedBlank(string sourceText)
        {
			mockEventLogger.LogDebug(">>> Entering ObjectInputIsReturnedBlank(sourceText={SourceText})", sourceText);
            // Arrange
            var tokenReplace = new TokenReplace(Scope.DefaultSettings, PortalSettings.Current.DefaultLanguage,
                PortalSettings.Current, UserController.Instance.GetUser(1, 1), 1);

            // Act
            var outputText = tokenReplace.ReplaceEnvironmentTokens(sourceText);
			mockEventLogger.LogDebug("ObjectInputIsReturnedBlank(sourceText={SourceText}): outputText → {OutputText}", sourceText, outputText);

            // Assert
            Assert.That(outputText.Trim(), Is.EqualTo(string.Empty));
			mockEventLogger.LogDebug("<<< Exiting ObjectInputIsReturnedBlank(sourceText={SourceText})", sourceText);
        }

        private void SetupPortalSettings()
        {
			mockEventLogger.LogDebug(">>> Entering SetupPortalSettings()");
            var portalSettings = new PortalSettings
            {
                AdministratorRoleName = Utilities.Constants.RoleName_Administrators,
                ActiveTab = new TabInfo { ModuleID = 1, TabID = 1 },
            };

            this.portalController.Setup(pc => pc.GetCurrentPortalSettings()).Returns(portalSettings);
			mockEventLogger.LogDebug("<<< Exiting SetupPortalSettings()");
        }

        private void SetupModuleInfo()
        {
			mockEventLogger.LogDebug(">>> Entering SetupModuleInfo()");
            var moduleInfo = new ModuleInfo
            {
                ModuleID = 1,
                PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
            };

            this.moduleController.Setup(mc => mc.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(moduleInfo);
				mockEventLogger.LogDebug("<<< Exiting SetupModuleInfo()");
        }

        private void SetupUserInfo()
        {
			mockEventLogger.LogDebug(">>> Entering SetupUserInfo()");
            var userInfo = new UserInfo
            {
                UserID = 1,
                Username = "admin",
                PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
            };
            this.userController.Setup(uc => uc.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns(userInfo);
			mockEventLogger.LogDebug("<<< Exiting SetupUserInfo()");
        }
    }
}
