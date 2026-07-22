// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    [TestFixture]
    public class PortalSettingsTests : BaseSettingsTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        protected override void SetupServiceProvider(IServiceCollection services)
        {
			Logger.LogDebug(">>> Entering SetupServiceProvider(services={Services})", services);
            services.AddSingleton<ISerializationManager, SerializationManager>();
			Logger.LogDebug("<<< Exiting SetupServiceProvider(services={Services})", services);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void SaveSettings_CallsUpdatePortalSetting_WithRightParameters_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdatePortalSetting_WithRightParameters_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.SaveSettings_CallsUpdatePortalSetting_WithRightParameters(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdatePortalSetting_WithRightParameters_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
			Logger.LogDebug(">>> Entering SaveSettings_UpdatesCache()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyPortalSettings();

			Logger.LogDebug("SaveSettings_UpdatesCache(): settings → {Settings}", settings);
            this.MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyPortalSettingsRepository();

			Logger.LogDebug("SaveSettings_UpdatesCache(): settingsRepository → {SettingsRepository}", settingsRepository);
            // Act
            settingsRepository.SaveSettings(moduleInfo, settings);
			Logger.LogInformation("SaveSettings_UpdatesCache(): save query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            this.MockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting SaveSettings_UpdatesCache()");
			Logger.LogInformation("SaveSettings_UpdatesCache(): database query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);
        }

        [Test]
        public void GetSettings_CallsGetCachedObject()
        {
			Logger.LogDebug(">>> Entering GetSettings_CallsGetCachedObject()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var moduleInfo = GetModuleInfo;

            this.MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyPortalSettings());
            var settingsRepository = new MyPortalSettingsRepository();

			Logger.LogDebug("GetSettings_CallsGetCachedObject(): settingsRepository → {SettingsRepository}", settingsRepository);
            // Act
            settingsRepository.GetSettings(moduleInfo);
			Logger.LogInformation("GetSettings_CallsGetCachedObject(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            this.MockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting GetSettings_CallsGetCachedObject()");
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ar-JO")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ar_JO(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_ar_JO(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ca-ES")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ca_ES(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_ca_ES(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("zh-CN")]
        public void GetSettings_GetsValuesFrom_PortalSettings_zh_CN(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_zh_CN(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("en-US")]
        public void GetSettings_GetsValuesFrom_PortalSettings_en_US(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_en_US(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("fr-FR")]
        public void GetSettings_GetsValuesFrom_PortalSettings_fr_FR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_fr_FR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("he-IL")]
        public void GetSettings_GetsValuesFrom_PortalSettings_he_IL(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_he_IL(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("ru-RU")]
        public void GetSettings_GetsValuesFrom_PortalSettings_ru_RU(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_ru_RU(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        [Test]
        [TestCaseSource(nameof(SettingsCases))]
        [SetCulture("tr-TR")]
        public void GetSettings_GetsValuesFrom_PortalSettings_tr_TR(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
            this.GetSettings_GetsValuesFrom_PortalSettings(stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings_tr_TR(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        private void SaveSettings_CallsUpdatePortalSetting_WithRightParameters(string stringValue, int integerValue, double doubleValue,
            bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyPortalSettings
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

            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "StringProperty", stringValue, true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "IntegerProperty", integerValue.ToString(), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "DoubleProperty", doubleValue.ToString(CultureInfo.InvariantCulture), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "BooleanProperty", booleanValue.ToString(), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "DateTimeProperty", datetimeValue.ToString("o", CultureInfo.InvariantCulture), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "TimeSpanProperty", timeSpanValue.ToString("c", CultureInfo.InvariantCulture), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "EnumProperty", enumValue.ToString(), true, Null.NullString, false));
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, SettingNamePrefix + "ComplexProperty", $"{complexValue.X} | {complexValue.Y}", true, Null.NullString, false));

            var settingsRepository = new MyPortalSettingsRepository();

            // Act
            settingsRepository.SaveSettings(moduleInfo, settings);

            // Assert
            this.MockRepository.VerifyAll();
        }

        private void GetSettings_GetsValuesFrom_PortalSettings(string stringValue, int integerValue, double doubleValue, bool booleanValue, DateTime datetimeValue, TimeSpan timeSpanValue, TestingEnum enumValue, ComplexType complexValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValuesFrom_PortalSettings(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var moduleInfo = GetModuleInfo;
            var portalSettings = new Dictionary<string, string>
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

            this.MockPortalSettings(moduleInfo, portalSettings);

            var settingsRepository = new MyPortalSettingsRepository();

			Logger.LogDebug("GetSettings_GetsValuesFrom_PortalSettings(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue}): settingsRepository → {SettingsRepository}", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, settingsRepository);
            // Act
            var settings = settingsRepository.GetSettings(moduleInfo);

			Logger.LogDebug("GetSettings_GetsValuesFrom_PortalSettings(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue}): settings → {Settings}", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, settings);
			Logger.LogInformation("GetSettings_GetsValuesFrom_PortalSettings(stringValue,integerValue,doubleValue,booleanValue,datetimeValue,timeSpanValue,enumValue,complexValue)={StringValue},{IntegerValue},{DoubleValue},{BooleanValue},{DatetimeValue},{TimeSpanValue},{EnumValue},{ComplexValue}: find query executed in {Arg8} ms", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue, __logStart.ElapsedMilliseconds);
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
			Logger.LogDebug("<<< Exiting GetSettings_GetsValuesFrom_PortalSettings(stringValue={StringValue},integerValue={IntegerValue},doubleValue={DoubleValue},booleanValue={BooleanValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue},enumValue={EnumValue},complexValue={ComplexValue})", stringValue, integerValue, doubleValue, booleanValue, datetimeValue, timeSpanValue, enumValue, complexValue);
        }

        public class MyPortalSettings
        {
            [PortalSetting(Prefix = SettingNamePrefix)]
            public string StringProperty { get; set; } = string.Empty;

            [PortalSetting(Prefix = SettingNamePrefix)]
            public int IntegerProperty { get; set; }

            [PortalSetting(Prefix = SettingNamePrefix)]
            public double DoubleProperty { get; set; }

            [PortalSetting(Prefix = SettingNamePrefix)]
            public bool BooleanProperty { get; set; }

            [PortalSetting(Prefix = SettingNamePrefix)]
            public DateTime DateTimeProperty { get; set; } = DateTime.Now;

            [PortalSetting(Prefix = SettingNamePrefix)]
            public TimeSpan TimeSpanProperty { get; set; } = TimeSpan.Zero;

            [PortalSetting(Prefix = SettingNamePrefix)]
            public TestingEnum EnumProperty { get; set; } = TestingEnum.Value1;

            [PortalSetting(Prefix = SettingNamePrefix, Serializer = "DotNetNuke.Tests.Core.Entities.Modules.Settings.ComplexTypeSerializer,DotNetNuke.Tests.Core")]
            public ComplexType ComplexProperty { get; set; } = new ComplexType(20, 25);
        }

        public class MyPortalSettingsRepository : SettingsRepository<MyPortalSettings>
        {
        }
    }
}
