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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SetUp]
        public void SetUp()
        {
			Logger.LogDebug(">>> Entering SetUp()");
            MockComponentProvider.CreateDataProvider();
			Logger.LogDebug("<<< Exiting SetUp()");
        }

        [TearDown]
        public void TearDown()
        {
			Logger.LogDebug(">>> Entering TearDown()");
            MockComponentProvider.ResetContainer();
            TabController.ClearInstance();
			Logger.LogDebug("<<< Exiting TearDown()");
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
			Logger.LogDebug(">>> Entering IsValidadTab_Returns_False_For_Forbidden_PageNames(tabName={TabName})", tabName);
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);

			Logger.LogDebug("IsValidadTab_Returns_False_For_Forbidden_PageNames(tabName={TabName}): isValid → {IsValid}", tabName, isValid);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False, "A forbidden tab name is allowed");
                Assert.That(invalidType, Is.EqualTo("InvalidTabName"), "The invalidType is not the expected one");
            }
			Logger.LogDebug("<<< Exiting IsValidadTab_Returns_False_For_Forbidden_PageNames(tabName={TabName})", tabName);
        }

        [Test]
        public void IsValidadTab_Returns_False_For_Empty_PageNames()
        {
			Logger.LogDebug(">>> Entering IsValidadTab_Returns_False_For_Empty_PageNames()");
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(string.Empty, out invalidType);

			Logger.LogDebug("IsValidadTab_Returns_False_For_Empty_PageNames(): isValid → {IsValid}", isValid);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.False, "An empty tab name is allowed");
                Assert.That(invalidType, Is.EqualTo("EmptyTabName"), "The invalidType is not the expected one");
            }
			Logger.LogDebug("<<< Exiting IsValidadTab_Returns_False_For_Empty_PageNames()");
        }

        [Test]
        [TestCase("test")]
        [TestCase("mypage")]
        [TestCase("products")]
        public void IsValidadTab_Returns_True_For_Regular_PageNames(string tabName)
        {
			Logger.LogDebug(">>> Entering IsValidadTab_Returns_True_For_Regular_PageNames(tabName={TabName})", tabName);
            // Arrange
            string invalidType;

            // Act
            var isValid = TabController.IsValidTabName(tabName, out invalidType);

			Logger.LogDebug("IsValidadTab_Returns_True_For_Regular_PageNames(tabName={TabName}): isValid → {IsValid}", tabName, isValid);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(isValid, Is.True, "A regular tab name is not allowed");
                Assert.That(invalidType, Is.Empty, "The invalidType is not the expected one");
            }
			Logger.LogDebug("<<< Exiting IsValidadTab_Returns_True_For_Regular_PageNames(tabName={TabName})", tabName);
        }
    }
}
