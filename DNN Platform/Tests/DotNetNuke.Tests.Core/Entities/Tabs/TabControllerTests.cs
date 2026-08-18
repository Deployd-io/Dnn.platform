// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Tabs
{
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Mocks;
    using NUnit.Framework;

    [TestFixture]
    public class TabControllerTests
    {
        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            MockComponentProvider.CreateDataProvider();
			mockEventLogger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            MockComponentProvider.ResetContainer();
            TabController.ClearInstance();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [TestCase("Lpt1")]
        [TestCase("Lpt2")]
        [TestCase("Lpt3")]
        [TestCase("Lpt4")]
        [TestCase("Lpt5")]
        [TestCase("Lpt6")]
        [TestCase("Lpt7")]
        [TestCase("Lpt8")]
        [TestCase("Lpt9")]
        [TestCase("Com1")]
        [TestCase("Com2")]
        [TestCase("Com3")]
        [TestCase("Com4")]
        [TestCase("Com5")]
        [TestCase("Com6")]
        [TestCase("Com7")]
        [TestCase("Com8")]
        [TestCase("Com9")]
        [TestCase("Aux")]
        [TestCase("Con")]
        [TestCase("Nul")]
        [TestCase("SiteMap")]
        [TestCase("Linkclick")]
        [TestCase("KeepAlive")]
        [TestCase("Default")]
        [TestCase("ErrorPage")]
        [TestCase("Login")]
        [TestCase("Register")]
        public void IsValidadTab_Returns_False_For_Forbidden_PageNames(string tabName)
        {
			mockEventLogger.LogDebug(">>> Entering IsValidadTab_Returns_False_For_Forbidden_PageNames(tabName={TabName})", tabName);
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);
			mockEventLogger.LogDebug("IsValidadTab_Returns_False_For_Forbidden_PageNames(tabName={TabName}): isValid → {IsValid}", tabName, isValid);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False, "A forbidden tab name is allowed");
                Assert.That(invalidType, Is.EqualTo("InvalidTabName"), "The invalidType is not the expected one");
            }
			mockEventLogger.LogDebug("<<< Exiting IsValidadTab_Returns_False_For_Forbidden_PageNames(tabName={TabName})", tabName);
        }

        [Test]
        public void IsValidadTab_Returns_False_For_Empty_PageNames()
        {
			mockEventLogger.LogDebug(">>> Entering IsValidadTab_Returns_False_For_Empty_PageNames()");
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(string.Empty, out invalidType);
			mockEventLogger.LogDebug("IsValidadTab_Returns_False_For_Empty_PageNames(): isValid → {IsValid}", isValid);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False, "An empty tab name is allowed");
                Assert.That(invalidType, Is.EqualTo("EmptyTabName"), "The invalidType is not the expected one");
            }
			mockEventLogger.LogDebug("<<< Exiting IsValidadTab_Returns_False_For_Empty_PageNames()");
        }

        [Test]
        [TestCase("test")]
        [TestCase("mypage")]
        [TestCase("products")]
        public void IsValidadTab_Returns_True_For_Regular_PageNames(string tabName)
        {
			mockEventLogger.LogDebug(">>> Entering IsValidadTab_Returns_True_For_Regular_PageNames(tabName={TabName})", tabName);
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);
			mockEventLogger.LogDebug("IsValidadTab_Returns_True_For_Regular_PageNames(tabName={TabName}): isValid → {IsValid}", tabName, isValid);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.True, "A regular tab name is not allowed");
                Assert.That(invalidType, Is.Empty, "The invalidType is not the expected one");
            }
			mockEventLogger.LogDebug("<<< Exiting IsValidadTab_Returns_True_For_Regular_PageNames(tabName={TabName})", tabName);
        }
    }
}
