// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.InternalServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TabVersionControllerTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const int UserId = 1;
        private const int TabId = 99;
        private static readonly DateTime ServerCreateOnDate = new DateTime(2018, 08, 15, 12, 0, 0, DateTimeKind.Unspecified);

        private Mock<ICBO> mockCBO;
        private Mock<IUserController> mockUserController;
        private Mock<IHostController> mockHostController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			Logger.LogDebug(">>> Entering Setup()");
            MockComponentProvider.ResetContainer();

            this.mockCBO = new Mock<ICBO>();
			Logger.LogDebug("Setup(): mockCBO → {MockCBO}", mockCBO);
            this.mockUserController = new Mock<IUserController>();
			Logger.LogDebug("Setup(): mockUserController → {MockUserController}", mockUserController);
            this.mockHostController = new Mock<IHostController>();
			Logger.LogDebug("Setup(): mockHostController → {MockHostController}", mockHostController);
            this.mockHostController.As<IHostSettingsService>();

            this.SetupCBO();
            this.SetupHostController();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
				Logger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
				Logger.LogDebug("Setup(): services → {Services}", services);
                    services.AddSingleton(this.mockCBO.Object);
                    services.AddSingleton(this.mockUserController.Object);
                    services.AddSingleton(this.mockHostController.Object);
                    services.AddSingleton((IHostSettingsService)this.mockHostController.Object);
                });
				Logger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			Logger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), nameof(TestCaseFactory.TestCases))]
        public void GetTabVersions_Verify_User_Preferred_TimeZone(string userPreferredTimeZone, DateTime expectedDateTime)
        {
			Logger.LogDebug(">>> Entering GetTabVersions_Verify_User_Preferred_TimeZone(userPreferredTimeZone={UserPreferredTimeZone},expectedDateTime={ExpectedDateTime})", userPreferredTimeZone, expectedDateTime);
            // Arrange
            this.SetupUserController(userPreferredTimeZone);

            // Act
            var tabVersionController = new TabVersionControllerTestable();
            var tabVersions = tabVersionController.GetTabVersions(TabId);
			Logger.LogDebug("GetTabVersions_Verify_User_Preferred_TimeZone(userPreferredTimeZone={UserPreferredTimeZone},expectedDateTime={ExpectedDateTime}): tabVersionController → {TabVersionController}", userPreferredTimeZone, expectedDateTime, tabVersionController);
			Logger.LogDebug("GetTabVersions_Verify_User_Preferred_TimeZone(userPreferredTimeZone={UserPreferredTimeZone},expectedDateTime={ExpectedDateTime}): tabVersions → {TabVersions}", userPreferredTimeZone, expectedDateTime, tabVersions);
            var tabVersion = tabVersions.First();
            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogDebug("GetTabVersions_Verify_User_Preferred_TimeZone(userPreferredTimeZone={UserPreferredTimeZone},expectedDateTime={ExpectedDateTime}): tabVersion → {TabVersion}", userPreferredTimeZone, expectedDateTime, tabVersion);
			Logger.LogDebug("GetTabVersions_Verify_User_Preferred_TimeZone(userPreferredTimeZone={UserPreferredTimeZone},expectedDateTime={ExpectedDateTime}): user → {User}", userPreferredTimeZone, expectedDateTime, user);
            var userTimeZone = user.Profile.PreferredTimeZone;

            var localizedDate = TimeZoneInfo.ConvertTime(tabVersion.CreatedOnDate, DateUtilsTestable.GetDatabaseDateTimeOffset(), userTimeZone);

			Logger.LogDebug("GetTabVersions_Verify_User_Preferred_TimeZone(userPreferredTimeZone={UserPreferredTimeZone},expectedDateTime={ExpectedDateTime}): localizedDate → {LocalizedDate}", userPreferredTimeZone, expectedDateTime, localizedDate);
            // Assert
            Assert.That(expectedDateTime, Is.EqualTo(localizedDate));
			Logger.LogDebug("<<< Exiting GetTabVersions_Verify_User_Preferred_TimeZone(userPreferredTimeZone={UserPreferredTimeZone},expectedDateTime={ExpectedDateTime})", userPreferredTimeZone, expectedDateTime);
        }

        private void SetupCBO()
        {
			Logger.LogDebug(">>> Entering SetupCBO()");
            this.mockCBO.Setup(c => c.GetCachedObject<List<TabVersion>>(It.IsAny<CacheItemArgs>(), It.IsAny<CacheItemExpiredCallback>(), It.IsAny<bool>()))
                .Returns(this.GetMockedTabVersions);
            CBO.SetTestableInstance(this.mockCBO.Object);
			Logger.LogDebug("<<< Exiting SetupCBO()");
        }

        private void SetupUserController(string timeZoneId)
        {
            this.mockUserController.Setup(userController => userController.GetCurrentUserInfo()).Returns(this.GetMockedUser(timeZoneId));
            UserController.SetTestableInstance(this.mockUserController.Object);
        }

        private UserInfo GetMockedUser(string timeZoneId)
        {
            var profile = new UserProfile()
            {
                PreferredTimeZone = this.GetMockedUserTimeZone(timeZoneId),
            };

            profile.ProfileProperties.Add(new Entities.Profile.ProfilePropertyDefinition(99)
            {
                CreatedByUserID = UserId,
                PropertyDefinitionId = 20,
                PropertyCategory = "Preferences",
                PropertyName = "PreferredTimeZone",
                PropertyValue = this.GetMockedUserTimeZone(timeZoneId).Id,
            });
            var user = new UserInfo()
            {
                Profile = profile,
                UserID = UserId,
                PortalID = 99,
            };

            return user;
        }

        private TimeZoneInfo GetMockedUserTimeZone(string timeZoneId)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        private TabVersion GetMockedTabVersion()
        {
			Logger.LogDebug(">>> Entering GetMockedTabVersion()");
            var tabVersion = new TabVersion
            {
                IsPublished = true,
                TabId = TabId,
                TabVersionId = 1,
                Version = 1,
                CreatedByUserID = UserId,
            };
            tabVersion.GetType().BaseType.GetProperty("CreatedOnDate").SetValue(tabVersion, ServerCreateOnDate, null);

			Logger.LogDebug("<<< Exiting GetMockedTabVersion()");
            return tabVersion;
        }

        private List<TabVersion> GetMockedTabVersions()
        {
			Logger.LogDebug(">>> Entering GetMockedTabVersions()");
			Logger.LogDebug("<<< Exiting GetMockedTabVersions()");
            return new List<TabVersion>()
            {
                this.GetMockedTabVersion(),
            };
        }

        private void SetupHostController()
        {
			Logger.LogDebug(">>> Entering SetupHostController()");
            this.mockHostController.Setup(c => c.GetString(It.IsRegex("PerformanceSetting"))).Returns(nameof(Globals.PerformanceSettings.LightCaching));
			Logger.LogDebug("<<< Exiting SetupHostController()");
        }

        private class TabVersionControllerTestable : TabVersionController
        {
        }

        private class DateUtilsTestable : DateUtils
        {
            public static new TimeZoneInfo GetDatabaseDateTimeOffset()
            {
				Logger.LogDebug(">>> Entering GetDatabaseDateTimeOffset()");
                var timeZoneId = "UTC";
				Logger.LogDebug("GetDatabaseDateTimeOffset(): timeZoneId → {TimeZoneId}", timeZoneId);
				Logger.LogDebug("<<< Exiting GetDatabaseDateTimeOffset()");
                return TimeZoneInfo.CreateCustomTimeZone(timeZoneId, new TimeSpan(0, 0, 0), timeZoneId, timeZoneId);
            }
        }

        private class TestCaseFactory
        {
            public static IEnumerable TestCases()
            {
				Logger.LogDebug(">>> Entering TestCases()");
                yield return new TestCaseData("Central European Standard Time", new DateTime(2018, 08, 15, 14, 0, 0));
                yield return new TestCaseData("Russian Standard Time", new DateTime(2018, 08, 15, 15, 0, 0));
                yield return new TestCaseData("SE Asia Standard Time", new DateTime(2018, 08, 15, 19, 0, 0));
				Logger.LogDebug("<<< Exiting TestCases()");
            }
        }

        // Assuming 12:00 Aug 15, 2018 server local time
    }
}
