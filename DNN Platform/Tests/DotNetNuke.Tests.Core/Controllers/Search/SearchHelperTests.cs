// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary> Testing various aspects of SearchController.</summary>
    [TestFixture]
    public class SearchHelperTests
    {
        private const int OtherSearchTypeId = 2;
        private const string TermDNN = "DNN";
        private const string TermDotNetNuke = "DotnetNuke";
        private const string TermLaptop = "Laptop";
        private const string TermNotebook = "Notebook";
        private const string TermJump = "Jump";
        private const string TermLeap = "Leap";
        private const string TermHop = "Hop";
        private const int PortalId0 = 0;
        private const string CultureEnUs = "en-US";
        private Mock<DataProvider> dataProvider;
        private Mock<CachingProvider> cachingProvider;
        private SearchHelperImpl searchHelper;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
			mockEventLogger.LogDebug(">>> Entering SetUp()");
            ComponentFactory.Container = new SimpleContainer();
            this.cachingProvider = MockComponentProvider.CreateDataCacheProvider();
			mockEventLogger.LogDebug("SetUp(): cachingProvider → {CachingProvider}", cachingProvider);
            this.dataProvider = MockComponentProvider.CreateDataProvider();
			mockEventLogger.LogDebug("SetUp(): dataProvider → {DataProvider}", dataProvider);
            this.SetupDataProvider();
            this.searchHelper = new SearchHelperImpl(Mock.Of<IHostSettings>(), Mock.Of<IHostSettingsService>(), Mock.Of<IPortalController>(), Mock.Of<IApplicationStatusInfo>());
			mockEventLogger.LogDebug("SetUp(): searchHelper → {SearchHelper}", searchHelper);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("SetUp(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton(this.cachingProvider.Object);
                    services.AddSingleton(this.dataProvider.Object);
                    services.AddSingleton(this.searchHelper);
                });

            DataCache.ClearCache();
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
        public void SearchHelper_GetSynonyms_Two_Terms_Returns_Correct_Results()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_GetSynonyms_Two_Terms_Returns_Correct_Results()");
            // Arrange

            // Act
            var synonyms = this.searchHelper.GetSynonyms(PortalId0, CultureEnUs, TermDNN).ToArray();
			mockEventLogger.LogDebug("SearchHelper_GetSynonyms_Two_Terms_Returns_Correct_Results(): synonyms → {Synonyms}", synonyms);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(synonyms.Count(), Is.EqualTo(1));
                Assert.That(synonyms[0], Is.EqualTo(TermDotNetNuke.ToLowerInvariant()));
            }
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_GetSynonyms_Two_Terms_Returns_Correct_Results()");
        }

        [Test]
        public void SearchHelper_GetSynonyms_Three_Terms_Returns_Correct_Results()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_GetSynonyms_Three_Terms_Returns_Correct_Results()");
            // Arrange

            // Act
            var synonyms = this.searchHelper.GetSynonyms(PortalId0, CultureEnUs, TermHop).ToArray();
			mockEventLogger.LogDebug("SearchHelper_GetSynonyms_Three_Terms_Returns_Correct_Results(): synonyms → {Synonyms}", synonyms);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(synonyms.Count(), Is.EqualTo(2));
                Assert.That(synonyms[0], Is.EqualTo(TermJump.ToLowerInvariant()));
                Assert.That(synonyms[1], Is.EqualTo(TermLeap.ToLowerInvariant()));
            }
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_GetSynonyms_Three_Terms_Returns_Correct_Results()");
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardButExact_1()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_NoWildCardButExact_1()");
            // Arrange
            const string inPhrase = "\"brown fox\"";
            const string expected = inPhrase;

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, false);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_NoWildCardButExact_1(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_NoWildCardButExact_1()");
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardButExact_2()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_NoWildCardButExact_2()");
            // Arrange
            const string inPhrase = "\"brown fox\" -\"(Lazy) dog\" jumps";
            const string expected = inPhrase;

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, false);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_NoWildCardButExact_2(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_NoWildCardButExact_2()");
        }

        [Test]
        public void SearchHelper_Rephrase_NoWildCardNoExact()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_NoWildCardNoExact()");
            // Arrange
            const string inPhrase = "(brown) OR (fox AND dog) +jumbs";
            const string expected = inPhrase;

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, false);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_NoWildCardNoExact(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_NoWildCardNoExact()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_0()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_0()");
            // Arrange
            const string inPhrase = "fox dog";
            const string expected = "(fox OR fox*) (dog OR dog*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_0(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_0()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_1()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_1()");
            // Arrange
            const string inPhrase = "(lazy-dog)";
            const string expected = "(lazy-dog OR lazy-dog*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_1(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_1()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_2()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_2()");
            // Arrange
            const string inPhrase = "fox (dog)";
            const string expected = "(fox OR fox*) (dog OR dog*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_2(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_2()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_3()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_3()");
            // Arrange
            const string inPhrase = "(dog) fox";
            const string expected = "(dog OR dog*) (fox OR fox*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_3(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_3()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_4()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_4()");
            // Arrange
            const string inPhrase = "brown fox (lazy) dog";
            const string expected = "(brown OR brown*) (fox OR fox*) (lazy OR lazy*) (dog OR dog*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_4(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_4()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_5()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_5()");
            // Arrange
            const string inPhrase = "(brown fox) lazy dog";
            const string expected = "((brown OR brown*) (fox OR fox*)) (lazy OR lazy*) (dog OR dog*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_5(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_5()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_6()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_6()");
            // Arrange
            const string inPhrase = "brown fox (lazy dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) (dog OR dog*))";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_6(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_6()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_7()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_7()");
            // Arrange
            const string inPhrase = "brown fox (lazy AND dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) AND (dog OR dog*))";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_7(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_7()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardNoExact_8()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardNoExact_8()");
            // Arrange
            const string inPhrase = "brown fox (lazy and dog)";
            const string expected = "(brown OR brown*) (fox OR fox*) ((lazy OR lazy*) (and OR and*) (dog OR dog*))";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardNoExact_8(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardNoExact_8()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_1()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardWithExact_1()");
            // Arrange
            const string inPhrase = "\"brown fox\"";
            const string expected = inPhrase;

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardWithExact_1(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardWithExact_1()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_2()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardWithExact_2()");
            // Arrange
            const string inPhrase = "\"brown fox\" dog";
            const string expected = "\"brown fox\" (dog OR dog*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardWithExact_2(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardWithExact_2()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithExact_3()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardWithExact_3()");
            // Arrange
            const string inPhrase = "The +\"brown fox\" -\"Lazy dog\" jumps";
            const string expected = "(The OR The*) +\"brown fox\" -\"Lazy dog\" (jumps OR jumps*)";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardWithExact_3(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardWithExact_3()");
        }

        [Test]
        public void SearchHelper_Rephrase_WildCardWithTilde_4()
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_WildCardWithTilde_4()");
            // Arrange
            const string inPhrase = "dog jump~2";
            const string expected = "(dog OR dog*) jump~2";

            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_WildCardWithTilde_4(): analyzed → {Analyzed}", analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_WildCardWithTilde_4()");
        }

        [Test]

        // Arrange
        [TestCase("Cäu", "(Cau OR Cau*)")]
        [TestCase("Cäutätörül", "(Cautatorul OR Cautatorul*)")]
        [TestCase("Ãbcdef", "(Abcdef OR Abcdef*)")]
        public void SearchHelper_Rephrase_AccentedCharsReplaced_Replaced(string inPhrase, string expected)
        {
			mockEventLogger.LogDebug(">>> Entering SearchHelper_Rephrase_AccentedCharsReplaced_Replaced(inPhrase={InPhrase},expected={Expected})", inPhrase, expected);
            // Act
            var analyzed = this.searchHelper.RephraseSearchText(inPhrase, true);
			mockEventLogger.LogDebug("SearchHelper_Rephrase_AccentedCharsReplaced_Replaced(inPhrase={InPhrase},expected={Expected}): analyzed → {Analyzed}", inPhrase, expected, analyzed);

            // Assert
            Assert.That(analyzed, Is.EqualTo(expected));
			mockEventLogger.LogDebug("<<< Exiting SearchHelper_Rephrase_AccentedCharsReplaced_Replaced(inPhrase={InPhrase},expected={Expected})", inPhrase, expected);
        }

        private void SetupDataProvider()
        {
			mockEventLogger.LogDebug(">>> Entering SetupDataProvider()");
            // Standard DataProvider Path for Logging
            this.dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);

            this.dataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(this.GetPortalsCallBack);

            this.dataProvider.Setup(d => d.GetPortalSettings(It.IsAny<int>(), It.IsAny<string>())).Returns<int, string>(this.GetPortalSettingsCallBack);

            var dataTable = new DataTable("SynonymsGroups");
			mockEventLogger.LogDebug("SetupDataProvider(): dataTable → {DataTable}", dataTable);
            var pkId = dataTable.Columns.Add("SynonymsGroupID", typeof(int));
			mockEventLogger.LogDebug("SetupDataProvider(): pkId → {PkId}", pkId);
            dataTable.Columns.Add("SynonymsTags", typeof(string));
            dataTable.Columns.Add("CreatedByUserID", typeof(int));
            dataTable.Columns.Add("CreatedOnDate", typeof(DateTime));
            dataTable.Columns.Add("LastModifiedByUserID", typeof(int));
            dataTable.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            dataTable.Columns.Add("PortalID", typeof(int));

            dataTable.PrimaryKey = new[] { pkId };
			mockEventLogger.LogDebug("SetupDataProvider(): dataTable.PrimaryKey → {Arg0}", dataTable.PrimaryKey);

            // Create some test data
            var now = DateTime.Now;
            dataTable.Rows.Add(1, string.Join(",", new[] { TermDNN, TermDotNetNuke }), 1, now, 1, now, 0);
            dataTable.Rows.Add(2, string.Join(",", new[] { TermLaptop, TermNotebook }), 1, now, 1, now, 0);
            dataTable.Rows.Add(3, string.Join(",", new[] { TermJump, TermLeap, TermHop }), 1, now, 1, now, 0);

            this.dataProvider.Setup(x => x.GetAllSynonymsGroups(0, It.IsAny<string>())).Returns(dataTable.CreateDataReader());
			mockEventLogger.LogDebug("<<< Exiting SetupDataProvider()");
        }

        private IDataReader GetPortalSettingsCallBack(int portalId, string culture)
        {
			mockEventLogger.LogDebug(">>> Entering GetPortalSettingsCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            var table = new DataTable("PortalSettings");

			mockEventLogger.LogDebug("GetPortalSettingsCallBack(portalId={PortalId},culture={Culture}): table → {Table}", portalId, culture, table);
            var cols = new string[]
                           {
                            "SettingName", "SettingValue", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode",
                           };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            table.Rows.Add("SearchAdminInitialization", "true", "-1", DateTime.Now, "-1", DateTime.Now, CultureEnUs);

			mockEventLogger.LogDebug("<<< Exiting GetPortalSettingsCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            return table.CreateDataReader();
        }

        private IDataReader GetPortalsCallBack(string culture)
        {
			mockEventLogger.LogDebug(">>> Entering GetPortalsCallBack(culture={Culture})", culture);
			mockEventLogger.LogDebug("<<< Exiting GetPortalsCallBack(culture={Culture})", culture);
            return this.GetPortalCallBack(PortalId0, CultureEnUs);
        }

        private IDataReader GetPortalCallBack(int portalId, string culture)
        {
			mockEventLogger.LogDebug(">>> Entering GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            DataTable table = new DataTable("Portal");

			mockEventLogger.LogDebug("GetPortalCallBack(portalId={PortalId},culture={Culture}): table → {Table}", portalId, culture, table);
            var cols = new string[]
                           {
                               "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate", "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
                               "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId", "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor", "ProcessorUserId",
                               "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset", "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
                               "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId", "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate", "CultureCode",
                           };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            var homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright 2011 by DotNetNuke Corporation", null, "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website", "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020", null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0", null, homePage.ToString(), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11", "-1", "2011-08-25 07:34:29", culture);

			mockEventLogger.LogDebug("<<< Exiting GetPortalCallBack(portalId={PortalId},culture={Culture})", portalId, culture);
            return table.CreateDataReader();
        }
    }
}
