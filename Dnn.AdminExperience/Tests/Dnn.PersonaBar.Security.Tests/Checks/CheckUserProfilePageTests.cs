﻿namespace Dnn.PersonaBar.Security.Tests.Checks
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using Dnn.PersonaBar.Library.Dto;
    using Dnn.PersonaBar.Pages.Components;
    using Dnn.PersonaBar.Pages.Services.Dto;
    using Dnn.PersonaBar.Security.Components;
    using Dnn.PersonaBar.Security.Components.Checks;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CheckUserProfilePageTests
    {
        private const string CustomUserProfilePageTabPath = "//MyCustomUserProfilePage";
        private const int PortalId = 1;

        [Test]
        [TestCase("portalController")]
        [TestCase("tabController")]
        [TestCase("pagesController")]
        public void Constructor_WhenArgumentIsNull_ThrowsArgumentNullException(string nullParamName)
        {
			permissionProviderMock.LogDebug(">>> Entering Constructor_WhenArgumentIsNull_ThrowsArgumentNullException(nullParamName={NullParamName})", nullParamName);
            // arrange
            var argumentNullException = default(ArgumentNullException);
            var unexpectedException = default(Exception);
			permissionProviderMock.LogDebug("Constructor_WhenArgumentIsNull_ThrowsArgumentNullException(nullParamName={NullParamName}): argumentNullException → {ArgumentNullException}", nullParamName, argumentNullException);
			permissionProviderMock.LogDebug("Constructor_WhenArgumentIsNull_ThrowsArgumentNullException(nullParamName={NullParamName}): unexpectedException → {UnexpectedException}", nullParamName, unexpectedException);

            // act
            try
            {
                new CheckUserProfilePage(
                    nullParamName == "portalController" ? null : new Mock<IPortalController>().Object,
                    nullParamName == "tabController" ? null : new Mock<ITabController>().Object,
                    nullParamName == "pagesController" ? null : new Mock<IPagesController>().Object);
            }
            catch (ArgumentNullException ex)
            {
				permissionProviderMock.LogError("Exception in Constructor_WhenArgumentIsNull_ThrowsArgumentNullException(nullParamName={NullParamName}): {Arg1}", nullParamName, ex.Message);
                argumentNullException = ex;
            }
            catch (Exception ex)
            {
                unexpectedException = ex;
            }

            using (Assert.EnterMultipleScope())
            {
                // assert
                Assert.That(unexpectedException, Is.Null);
                Assert.That(argumentNullException, Is.Not.Null);
            }
            Assert.That(argumentNullException.ParamName, Is.EqualTo(nullParamName));
			permissionProviderMock.LogDebug("<<< Exiting Constructor_WhenArgumentIsNull_ThrowsArgumentNullException(nullParamName={NullParamName})", nullParamName);
        }

        [Test]
        public void Execute_WhenNotFound_ReturnsPass()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenNotFound_ReturnsPass()");
            // arrange
            var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
            var tabControllerMock = SetupTabControllerMock();
			permissionProviderMock.LogDebug("Execute_WhenNotFound_ReturnsPass(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenNotFound_ReturnsPass(): tabControllerMock → {TabControllerMock}", tabControllerMock);
            var pagesControllerMock = new Mock<IPagesController>();

			permissionProviderMock.LogDebug("Execute_WhenNotFound_ReturnsPass(): pagesControllerMock → {PagesControllerMock}", pagesControllerMock);
            var sut = new CheckUserProfilePage(
                portalControllerMock.Object,
                tabControllerMock.Object,
                pagesControllerMock.Object);

            // act
            var result = sut.Execute();

			permissionProviderMock.LogDebug("Execute_WhenNotFound_ReturnsPass(): result → {Result}", result);
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenNotFound_ReturnsPass()");
        }

        [Test]
        public void Execute_WhenDeleted_ReturnsPass()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenDeleted_ReturnsPass()");
            // arrange
            var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);

			permissionProviderMock.LogDebug("Execute_WhenDeleted_ReturnsPass(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
            var tabControllerMock = SetupTabControllerMock(profilePage: new TabInfo { IsDeleted = true });

			permissionProviderMock.LogDebug("Execute_WhenDeleted_ReturnsPass(): tabControllerMock → {TabControllerMock}", tabControllerMock);
            var pagesControllerMock = new Mock<IPagesController>();

			permissionProviderMock.LogDebug("Execute_WhenDeleted_ReturnsPass(): pagesControllerMock → {PagesControllerMock}", pagesControllerMock);
            var sut = new CheckUserProfilePage(
                portalControllerMock.Object,
                tabControllerMock.Object,
                pagesControllerMock.Object);

            // act
            var result = sut.Execute();

			permissionProviderMock.LogDebug("Execute_WhenDeleted_ReturnsPass(): result → {Result}", result);
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenDeleted_ReturnsPass()");
        }

        [Test]
        public void Execute_WhenPublic_ReturnsWarning()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenPublic_ReturnsWarning()");
            // arrange
            RegisterTestablePermissionProvider();

            var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
            var activityFeedPage = BuildActivityFeedTabInfo();
			permissionProviderMock.LogDebug("Execute_WhenPublic_ReturnsWarning(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenPublic_ReturnsWarning(): activityFeedPage → {ActivityFeedPage}", activityFeedPage);
            var tabControllerMock = SetupTabControllerMock(activityFeedPage);
            var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: true);
			permissionProviderMock.LogDebug("Execute_WhenPublic_ReturnsWarning(): tabControllerMock → {TabControllerMock}", tabControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenPublic_ReturnsWarning(): pagesControllerMock → {PagesControllerMock}", pagesControllerMock);

            var sut = new CheckUserProfilePage(
                portalControllerMock.Object,
                tabControllerMock.Object,
                pagesControllerMock.Object);

            // act
            var result = sut.Execute();

			permissionProviderMock.LogDebug("Execute_WhenPublic_ReturnsWarning(): result → {Result}", result);
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Warning));
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenPublic_ReturnsWarning()");
        }

        [Test]
        public void Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass()");
            // arrange
            RegisterTestablePermissionProvider();

            var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
            var customProfilePage = BuildCustomProfileTabInfo();
			permissionProviderMock.LogDebug("Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass(): customProfilePage → {CustomProfilePage}", customProfilePage);
            var tabControllerMock = SetupTabControllerMock(customProfilePage);
            var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: false);
			permissionProviderMock.LogDebug("Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass(): tabControllerMock → {TabControllerMock}", tabControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass(): pagesControllerMock → {PagesControllerMock}", pagesControllerMock);

            var sut = new CheckUserProfilePage(
                portalControllerMock.Object,
                tabControllerMock.Object,
                pagesControllerMock.Object);

            // act
            var result = sut.Execute();

			permissionProviderMock.LogDebug("Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass(): result → {Result}", result);
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass()");
        }

        [Test]
        public void Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass()");
            // arrange
            RegisterTestablePermissionProvider();

            var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
            var activityFeedPage = BuildActivityFeedTabInfo();
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass(): activityFeedPage → {ActivityFeedPage}", activityFeedPage);
            var tabControllerMock = SetupTabControllerMock(activityFeedPage, myProfilePage: null);
            var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: false);
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass(): tabControllerMock → {TabControllerMock}", tabControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass(): pagesControllerMock → {PagesControllerMock}", pagesControllerMock);

            var sut = new CheckUserProfilePage(
                portalControllerMock.Object,
                tabControllerMock.Object,
                pagesControllerMock.Object);

            // act
            var result = sut.Execute();

			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass(): result → {Result}", result);
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass()");
        }

        [Test]
        public void Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass()
        {
			permissionProviderMock.LogDebug(">>> Entering Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass()");
            // arrange
            RegisterTestablePermissionProvider();

            var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
            var activityFeedPage = BuildActivityFeedTabInfo();
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass(): portalControllerMock → {PortalControllerMock}", portalControllerMock);
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass(): activityFeedPage → {ActivityFeedPage}", activityFeedPage);
            var myProfilePage = BuildMyProfileTabInfo(deleted: true);
            var tabControllerMock = SetupTabControllerMock(activityFeedPage, myProfilePage);
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass(): myProfilePage → {MyProfilePage}", myProfilePage);
			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass(): tabControllerMock → {TabControllerMock}", tabControllerMock);
            var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: false);

			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass(): pagesControllerMock → {PagesControllerMock}", pagesControllerMock);
            var sut = new CheckUserProfilePage(
                portalControllerMock.Object,
                tabControllerMock.Object,
                pagesControllerMock.Object);

            // act
            var result = sut.Execute();

			permissionProviderMock.LogDebug("Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass(): result → {Result}", result);
            // assert
            Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
			permissionProviderMock.LogDebug("<<< Exiting Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass()");
        }

        [Test]
        [TestCase(true, SeverityEnum.Warning)]
        [TestCase(false, SeverityEnum.Pass)]
        public void Execute_WhenNotPublicAndActivityFeedAndMyProfileValid_ReturnsCorrectSeverity(
            bool myProfilePageIsPublic, SeverityEnum expectedSeverity)
        {
            // arrange
            RegisterTestablePermissionProvider();

            var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
            var activityFeedPage = BuildActivityFeedTabInfo();
            var myProfilePage = BuildMyProfileTabInfo();
            var tabControllerMock = SetupTabControllerMock(activityFeedPage, myProfilePage);
            var pagesControllerMock = SetupPagesControllerMock(false, myProfilePageIsPublic);

            var sut = new CheckUserProfilePage(
                portalControllerMock.Object,
                tabControllerMock.Object,
                pagesControllerMock.Object);

            // act
            var result = sut.Execute();

            // assert
            Assert.That(result.Severity, Is.EqualTo(expectedSeverity));
        }

        private static TabInfo BuildActivityFeedTabInfo(bool deleted = false) => new TabInfo
        {
			permissionProviderMock.LogDebug(">>> Entering BuildActivityFeedTabInfo(deleted={Deleted})", deleted);
            TabID = 1,
            IsDeleted = deleted,
            TabPath = CheckUserProfilePage.ActivityFeedTabPath,
            PortalID = PortalId,
        };
			permissionProviderMock.LogDebug("<<< Exiting BuildActivityFeedTabInfo(deleted={Deleted})", deleted);

        private static TabInfo BuildCustomProfileTabInfo(bool deleted = false) => new TabInfo
        {
			permissionProviderMock.LogDebug(">>> Entering BuildCustomProfileTabInfo(deleted={Deleted})", deleted);
            TabID = 1,
            IsDeleted = deleted,
            TabPath = CustomUserProfilePageTabPath,
            PortalID = PortalId,
        };
			permissionProviderMock.LogDebug("<<< Exiting BuildCustomProfileTabInfo(deleted={Deleted})", deleted);

        private static TabInfo BuildMyProfileTabInfo(bool deleted = false) => new TabInfo
        {
			permissionProviderMock.LogDebug(">>> Entering BuildMyProfileTabInfo(deleted={Deleted})", deleted);
            TabID = 2,
            IsDeleted = deleted,
            TabPath = CheckUserProfilePage.MyProfileTabPath,
            PortalID = PortalId,
        };
			permissionProviderMock.LogDebug("<<< Exiting BuildMyProfileTabInfo(deleted={Deleted})", deleted);

        private static void RegisterTestablePermissionProvider()
        {
			permissionProviderMock.LogDebug(">>> Entering RegisterTestablePermissionProvider()");
            var permissionProviderMock = new Mock<PermissionProvider>(Mock.Of<IEventLogger>(), Mock.Of<IPermissionDefinitionService>(), Mock.Of<IHostSettings>());

			permissionProviderMock.LogDebug("RegisterTestablePermissionProvider(): permissionProviderMock → {PermissionProviderMock}", permissionProviderMock);
            permissionProviderMock.Setup(x => x.ImplicitRolesForPages(It.IsAny<int>()))
                .Returns(new List<RoleInfo>());

            ComponentFactory.RegisterComponentInstance<PermissionProvider>(permissionProviderMock.Object);
			permissionProviderMock.LogDebug("<<< Exiting RegisterTestablePermissionProvider()");
        }

        private static Mock<IPagesController> SetupPagesControllerMock(
            bool userProfilePageIsPublic, bool myProfilePageIsPublic = false)
        {
            var mock = new Mock<IPagesController>();

            mock.Setup(x => x.GetPermissionsData(It.IsAny<int>()))
                .Returns((int pageId) =>
                {
                    switch (pageId)
                    {
                        case 1: return BuildPermissionsData(userProfilePageIsPublic);
                        case 2: return BuildPermissionsData(myProfilePageIsPublic);
                        default: throw new ArgumentOutOfRangeException(nameof(pageId));
                    }
                });

            return mock;
        }

        private static PagePermissions BuildPermissionsData(bool allUsersCanView)
        {
			permissionProviderMock.LogDebug(">>> Entering BuildPermissionsData(allUsersCanView={AllUsersCanView})", allUsersCanView);
            var permissionsData = new PagePermissions(Mock.Of<IPermissionDefinitionService>(), false);
            if (allUsersCanView)
			permissionProviderMock.LogDebug("BuildPermissionsData(allUsersCanView={AllUsersCanView}): permissionsData → {PermissionsData}", allUsersCanView, permissionsData);
            {
                permissionsData.RolePermissions =
                [
                    new RolePermission
                    {
                        RoleId = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture),
                        Permissions = [new Permission { PermissionName = CheckUserProfilePage.ViewTab, AllowAccess = true, },],
                    },
                ];
            }

			permissionProviderMock.LogDebug("<<< Exiting BuildPermissionsData(allUsersCanView={AllUsersCanView})", allUsersCanView);
            return permissionsData;
        }

        private static Mock<IPortalSettings> SetupPortalSettingsMock(int portalId, int userTabId)
        {
			permissionProviderMock.LogDebug(">>> Entering SetupPortalSettingsMock(portalId={PortalId},userTabId={UserTabId})", portalId, userTabId);
            var mock = new Mock<IPortalSettings>();
            mock.SetupGet(x => x.PortalId).Returns(portalId);
			permissionProviderMock.LogDebug("SetupPortalSettingsMock(portalId={PortalId},userTabId={UserTabId}): mock → {Mock}", portalId, userTabId, mock);
            mock.SetupGet(x => x.UserTabId).Returns(userTabId);
			permissionProviderMock.LogDebug("<<< Exiting SetupPortalSettingsMock(portalId={PortalId},userTabId={UserTabId})", portalId, userTabId);
            return mock;
        }

        private static Mock<IPortalController> SetupPortalControllerMock(int portalId, int userTabId)
        {
			permissionProviderMock.LogDebug(">>> Entering SetupPortalControllerMock(portalId={PortalId},userTabId={UserTabId})", portalId, userTabId);
            var settingsMock = SetupPortalSettingsMock(portalId, userTabId);

			permissionProviderMock.LogDebug("SetupPortalControllerMock(portalId={PortalId},userTabId={UserTabId}): settingsMock → {SettingsMock}", portalId, userTabId, settingsMock);
            var settingsDictionary = new Dictionary<string, string>
            {
                {
                    nameof(settingsMock.Object.ContentLocalizationEnabled),
                    settingsMock.Object.ContentLocalizationEnabled.ToString()
                },
            };

            var portalSettings = new PortalSettings
            {
                PortalId = settingsMock.Object.PortalId,
            };

            var mock = new Mock<IPortalController>();

			permissionProviderMock.LogDebug("SetupPortalControllerMock(portalId={PortalId},userTabId={UserTabId}): mock → {Mock}", portalId, userTabId, mock);
            mock.Setup(x => x.GetCurrentSettings()).Returns(settingsMock.Object);
            mock.Setup(x => x.GetPortalSettings(It.IsAny<int>())).Returns(settingsDictionary);
            mock.Setup(x => x.GetCurrentPortalSettings()).Returns(portalSettings);

            PortalController.SetTestableInstance(mock.Object);

			permissionProviderMock.LogDebug("<<< Exiting SetupPortalControllerMock(portalId={PortalId},userTabId={UserTabId})", portalId, userTabId);
            return mock;
        }

        private static Mock<ITabController> SetupTabControllerMock(
            TabInfo profilePage = null, TabInfo myProfilePage = null)
        {
            var mock = new Mock<ITabController>();

            mock.Setup(x => x.GetTab(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(profilePage);

            mock.Setup(x => x.GetTabsByPortal(It.IsAny<int>()))
                .Returns((int portalId) =>
                {
                    var tabCollection = new TabCollection();

                    if (myProfilePage != null)
                    {
                        tabCollection.Add(myProfilePage);
                    }

                    return tabCollection;
                });

            return mock;
        }
    }
}
