// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.Search.Internals
{
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Search.Internals;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Lucene.Net.Analysis.Cz;
    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    public class LuceneControllerImplTests
    {
        [Test]
        public void GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly()
        {
			mockEventLogger.LogDebug(">>> Entering GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly()");
            FakeServiceProvider serviceProvider = null;
            try
            {
                // Arrange
                const string HostSettingsTableName = "HostSettings";
                const string SettingNameColumnName = "SettingName";
                const string SettingValueColumnName = "SettingValue";
                const string SettingIsSecureColumnName = "SettingIsSecure";
                const string CustomAnalyzerCacheKeyName = "Search_CustomAnalyzer";
                const string CzechAnalyzerTypeName = "Lucene.Net.Analysis.Cz.CzechAnalyzer, Lucene.Net.Contrib.Analyzers";
                var mockData = MockComponentProvider.CreateDataProvider();
				mockEventLogger.LogDebug("GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly(): mockData → {MockData}", mockData);
                var hostSettings = new DataTable(HostSettingsTableName);
				mockEventLogger.LogDebug("GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly(): hostSettings → {HostSettings}", hostSettings);
                var nameCol = hostSettings.Columns.Add(SettingNameColumnName);
				mockEventLogger.LogDebug("GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly(): nameCol → {NameCol}", nameCol);
                hostSettings.Columns.Add(SettingValueColumnName);
                hostSettings.Columns.Add(SettingIsSecureColumnName);
                hostSettings.PrimaryKey = [nameCol];
				mockEventLogger.LogDebug("GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly(): hostSettings.PrimaryKey → {Arg0}", hostSettings.PrimaryKey);
                hostSettings.Rows.Add(CustomAnalyzerCacheKeyName, CzechAnalyzerTypeName, true);
                mockData.Setup(c => c.GetHostSettings()).Returns(hostSettings.CreateDataReader());
                var mockedApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
				mockEventLogger.LogDebug("GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly(): mockedApplicationStatusInfo → {MockedApplicationStatusInfo}", mockedApplicationStatusInfo);
                mockedApplicationStatusInfo.Setup(s => s.Status).Returns(UpgradeStatus.Install);
                mockedApplicationStatusInfo.Setup(s => s.ApplicationMapPath).Returns(string.Empty);

                serviceProvider = FakeServiceProvider.Setup(
                    services =>
                    {
                        services.AddSingleton(mockData.Object);
                        services.AddSingleton(mockedApplicationStatusInfo.Object);
                    });

                MockComponentProvider.CreateDataCacheProvider();
                DataCache.ClearCache();
                var luceneController = new LuceneControllerImpl(new FakeHostController(new Dictionary<string, IConfigurationSetting>(0)), mockedApplicationStatusInfo.Object);
				mockEventLogger.LogDebug("GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly(): luceneController → {LuceneController}", luceneController);

                // Act
                var analyzer = luceneController.GetCustomAnalyzer();
				mockEventLogger.LogDebug("GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly(): analyzer → {Analyzer}", analyzer);

                // Assert
                Assert.That(analyzer, Is.InstanceOf<CzechAnalyzer>());
            }
            finally
            {
                serviceProvider?.Dispose();
            }
			mockEventLogger.LogDebug("<<< Exiting GetCustomAnalyzer_WithTheProvidedAnalyzer_ReturnsTheAnalyzerCorrectly()");
        }
    }
}
