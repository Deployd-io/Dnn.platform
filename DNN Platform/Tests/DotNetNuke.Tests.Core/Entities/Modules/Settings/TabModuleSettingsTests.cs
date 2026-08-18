// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections;
    using System.Globalization;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    [TestFixture]
    public class TabModuleSettingsTests : BaseSettingsTests
    {
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
			mockEventLogger.LogDebug(">>> Entering Setup()");
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
					mockEventLogger.LogDebug("Setup(): serviceProvider → {ServiceProvider}", serviceProvider);
                    services.AddSingleton<ISerializationManager, SerializationManager>();
                });
				mockEventLogger.LogDebug("<<< Exiting Setup()");
        }

        [TearDown]
        public void TearDown()
        {
			mockEventLogger.LogDebug(">>> Entering TearDown()");
            this.serviceProvider.Dispose();
			mockEventLogger.LogDebug("<<< Exiting TearDown()");
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_UpdatesCache()");
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyTabModuleSettings();
			mockEventLogger.LogDebug("SaveSettings_UpdatesCache(): settings → {Settings}", settings);

            this.MockTabModuleSettings(moduleInfo, new Hashtable());
            this.MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyTabModuleSettingsRepository();
			mockEventLogger.LogDebug("SaveSettings_UpdatesCache(): settingsRepository → {SettingsRepository}", settingsRepository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			mockEventLogger.LogInformation("SaveSettings_UpdatesCache(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            settingsRepository.SaveSettings(moduleInfo, settings);

            // Assert
            this.MockRepository.VerifyAll();
			mockEventLogger.LogInformation("SaveSettings_UpdatesCache(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_UpdatesCache()");
        }

        [Test]
        public void GetSettings_CallsGetCachedObject()
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_CallsGetCachedObject()");
            // Arrange
            var moduleInfo = GetModuleInfo;

            this.MockTabModuleSettings(moduleInfo, new Hashtable());
            this.MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyTabModuleSettings());
            var settingsRepository = new MyTabModuleSettingsRepository();
			mockEventLogger.LogDebug("GetSettings_CallsGetCachedObject(): settingsRepository → {SettingsRepository}", settingsRepository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			mockEventLogger.LogInformation("GetSettings_CallsGetCachedObject(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
            settingsRepository.GetSettings(moduleInfo);

            // Assert
            this.MockRepository.VerifyAll();
			mockEventLogger.LogDebug("<<< Exiting GetSettings_CallsGetCachedObject()");
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void GetSettings_GetsValuesFrom_ModuleSettingsCollection_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        private void SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyTabModuleSettings
            {
                StringProperty = stringValue,
                IntegerProperty = integerValue,
                DoubleProperty = doubleValue,
                BooleanProperty = booleanValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
                EnumProperty = enumValue,
                ComplexProperty = complexValue,
            };

            this.MockTabModuleSettings(moduleInfo, new Hashtable());
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "StringProperty", stringValue));
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "IntegerProperty", integerValue.ToString()));
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture)));
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString()));
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("o", CultureInfo.InvariantCulture)));
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c", CultureInfo.InvariantCulture)));
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "EnumProperty", enumValue.ToString()));
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, SettingNamePrefix + "ComplexProperty", $"{complexValue.X} | {complexValue.Y}"));

            var settingsRepository = new MyTabModuleSettingsRepository();
			mockEventLogger.LogDebug("SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue}): settingsRepository → {SettingsRepository}", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, settingsRepository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
			mockEventLogger.LogInformation("SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue,integerValue,doubleValue,booleanValue,datetimeValue,timeSpanValue,enumValue,complexValue)={StringValue},{IntegerValue},{DoubleValue},{BooleanValue},{DatetimeValue},{TimeSpanValue},{EnumValue},{ComplexValue}: save query executed in {Arg8} ms", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, __logStart.ElapsedMilliseconds);
            settingsRepository.SaveSettings(moduleInfo, settings);

            // Assert
            this.MockRepository.VerifyAll();
			mockEventLogger.LogInformation("SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue,integerValue,doubleValue,booleanValue,datetimeValue,timeSpanValue,enumValue,complexValue)={StringValue},{IntegerValue},{DoubleValue},{BooleanValue},{DatetimeValue},{TimeSpanValue},{EnumValue},{ComplexValue}: database query executed in {Arg8} ms", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, __logStart.ElapsedMilliseconds);
			mockEventLogger.LogDebug("<<< Exiting SaveSettings_CallsUpdateTabModuleSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        private void GetSettings_GetsValuesFrom_ModuleSettingsCollection(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			mockEventLogger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            // Arrange
            var moduleInfo = GetModuleInfo;
            var tabModuleSettings = new Hashtable
                                    {
                                        { SettingNamePrefix + "StringProperty", stringValue },
                                        { SettingNamePrefix + "IntegerProperty", integerValue.ToString() },
                                        { SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture) },
                                        { SettingNamePrefix + "BooleanProperty", booleanValue.ToString() },
                                        { SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("o", CultureInfo.InvariantCulture) },
                                        { SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c", CultureInfo.InvariantCulture) },
                                        { SettingNamePrefix + "EnumProperty", enumValue.ToString() },
                                        { SettingNamePrefix + "ComplexProperty", $"{complexValue.X} | {complexValue.Y}" },
                                    };

            this.MockTabModuleSettings(moduleInfo, tabModuleSettings);

            var settingsRepository = new MyTabModuleSettingsRepository();
			mockEventLogger.LogDebug("GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue}): settingsRepository → {SettingsRepository}", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, settingsRepository);

            // Act
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            var settings = settingsRepository.GetSettings(moduleInfo);

			mockEventLogger.LogDebug("GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue}): settings → {Settings}", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, settings);
			mockEventLogger.LogInformation("GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue,integerValue,doubleValue,booleanValue,datetimeValue,timeSpanValue,enumValue,complexValue)={StringValue},{IntegerValue},{DoubleValue},{BooleanValue},{DatetimeValue},{TimeSpanValue},{EnumValue},{ComplexValue}: find query executed in {Arg8} ms", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, __logStart.ElapsedMilliseconds);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(settings.StringProperty, Is.EqualTo(stringValue), "The retrieved string property value is not equal to the stored one");
                Assert.That(settings.IntegerProperty, Is.EqualTo(integerValue), "The retrieved integer property value is not equal to the stored one");
                Assert.That(settings.DoubleProperty, Is.EqualTo(doubleValue), "The retrieved double property value is not equal to the stored one");
                Assert.That(settings.BooleanProperty, Is.EqualTo(booleanValue), "The retrieved boolean property value is not equal to the stored one");
                Assert.That(settings.DateTimeProperty, Is.EqualTo(datetimeValue), "The retrieved datetime property value is not equal to the stored one");
                Assert.That(settings.TimeSpanProperty, Is.EqualTo(timeSpanValue), "The retrieved timespan property value is not equal to the stored one");
                Assert.That(settings.EnumProperty, Is.EqualTo(enumValue), "The retrieved enum property value is not equal to the stored one");
                Assert.That(settings.ComplexProperty, Is.EqualTo(complexValue), "The retrieved complex property value is not equal to the stored one");
            }
            this.MockRepository.VerifyAll();
			mockEventLogger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_ModuleSettingsCollection(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        public class MyTabModuleSettings
        {
            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public string StringProperty { get; set; } = string.Empty;

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public int IntegerProperty { get; set; }

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public double DoubleProperty { get; set; }

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public bool BooleanProperty { get; set; }

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public DateTime DateTimeProperty { get; set; } = DateTime.Now;

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public TimeSpan TimeSpanProperty { get; set; } = TimeSpan.Zero;

            [TabModuleSetting(Prefix = SettingNamePrefix)]
            public TestingEnum EnumProperty { get; set; } = TestingEnum.Value1;

            [TabModuleSetting(Prefix = SettingNamePrefix, Serializer = "DotNetNuke.Tests.Core.Entities.Modules.Settings.ComplexTypeSerializer,DotNetNuke.Tests.Core")]
            public ComplexType ComplexProperty { get; set; } = new ComplexType(20, 25);
        }

        public class MyTabModuleSettingsRepository : SettingsRepository<MyTabModuleSettings>
        {
        }
    }
}
