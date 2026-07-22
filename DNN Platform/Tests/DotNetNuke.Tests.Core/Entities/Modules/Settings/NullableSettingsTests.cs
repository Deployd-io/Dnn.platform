// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Modules.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using NUnit.Framework;

    [TestFixture]
    public class NullableSettingsTests : BaseSettingsTests
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public static readonly TestCaseData<string, int?, DateTime?, TimeSpan?>[] NullableCases =
        [
            new(null, null, null, null),
            new(string.Empty, -1, new DateTime(2026, 5, 4, 3, 2, 1, DateTimeKind.Utc), TimeSpan.FromMilliseconds(3215648)),
            new("lorem ipsum", 456, new DateTime(2025, 12, 24, 1, 2, 3, DateTimeKind.Local), DateTime.Today - new DateTime(2025, 12, 24, 1, 2, 3, DateTimeKind.Local))
        ];

        protected override void SetupServiceProvider(IServiceCollection services)
        {
			Logger.LogDebug(">>> Entering SetupServiceProvider(services={Services})", services);
            services.AddSingleton<ISerializationManager, SerializationManager>();
			Logger.LogDebug("<<< Exiting SetupServiceProvider(services={Services})", services);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ar-JO")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_ar_JO(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_ar_JO(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_ar_JO(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ca-ES")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_ca_ES(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_ca_ES(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_ca_ES(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("zh-CN")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_zh_CN(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_zh_CN(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_zh_CN(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("en-US")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_en_US(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_en_US(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_en_US(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("fr-FR")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_fr_FR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_fr_FR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_fr_FR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("he-IL")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_he_IL(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_he_IL(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_he_IL(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ru-RU")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_ru_RU(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_ru_RU(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_ru_RU(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("tr-TR")]
        public void SaveSettings_CallsUpdateSetting_WithRightParameters_tr_TR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters_tr_TR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters_tr_TR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        public void SaveSettings_UpdatesCache()
        {
			Logger.LogDebug(">>> Entering SaveSettings_UpdatesCache()");
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyNullableSettings();

			Logger.LogDebug("SaveSettings_UpdatesCache(): settings → {Settings}", settings);
            this.MockModuleSettings(moduleInfo, new Hashtable());
            this.MockTabModuleSettings(moduleInfo, new Hashtable());
            this.MockCache.Setup(c => c.Insert(CacheKey(moduleInfo), settings));
            var settingsRepository = new MyNullableSettingsRepository();

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

            this.MockCache.Setup(c => c.GetItem("DNN_" + CacheKey(moduleInfo))).Returns(new MyNullableSettings());
            var settingsRepository = new MyNullableSettingsRepository();

			Logger.LogDebug("GetSettings_CallsGetCachedObject(): settingsRepository → {SettingsRepository}", settingsRepository);
            // Act
            settingsRepository.GetSettings(moduleInfo);
			Logger.LogInformation("GetSettings_CallsGetCachedObject(): find query executed in {Arg0} ms", __logStart.ElapsedMilliseconds);

            // Assert
            this.MockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting GetSettings_CallsGetCachedObject()");
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ar-JO")]
        public void GetSettings_GetsValues_FromCorrectSettings_ar_JO(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_ar_JO(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_ar_JO(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ca-ES")]
        public void GetSettings_GetsValues_FromCorrectSettings_ca_ES(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_ca_ES(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_ca_ES(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("zh-CN")]
        public void GetSettings_GetsValues_FromCorrectSettings_zh_CN(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_zh_CN(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_zh_CN(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("en-US")]
        public void GetSettings_GetsValues_FromCorrectSettings_en_US(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_en_US(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_en_US(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("fr-FR")]
        public void GetSettings_GetsValues_FromCorrectSettings_fr_FR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_fr_FR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_fr_FR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("he-IL")]
        public void GetSettings_GetsValues_FromCorrectSettings_he_IL(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_he_IL(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_he_IL(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("ru-RU")]
        public void GetSettings_GetsValues_FromCorrectSettings_ru_RU(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_ru_RU(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_ru_RU(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        [Test]
        [TestCaseSource(nameof(NullableCases))]
        [SetCulture("tr-TR")]
        public void GetSettings_GetsValues_FromCorrectSettings_tr_TR(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings_tr_TR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
            this.GetSettings_GetsValues_FromCorrectSettings(stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings_tr_TR(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        private void SaveSettings_CallsUpdateSetting_WithRightParameters(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var moduleInfo = GetModuleInfo;
            var settings = new MyNullableSettings
            {
                StringProperty = stringValue,
                IntegerProperty = integerValue,
                DateTimeProperty = datetimeValue,
                TimeSpanProperty = timeSpanValue,
            };

            this.MockModuleSettings(moduleInfo, new Hashtable());
            this.MockTabModuleSettings(moduleInfo, new Hashtable());
            var expectedStringValue = stringValue ?? string.Empty;
            this.MockModuleController.Setup(pc => pc.UpdateModuleSetting(ModuleId, "StringProperty", expectedStringValue));
            var integerString = integerValue?.ToString() ?? string.Empty;
            this.MockPortalController.Setup(pc => pc.UpdatePortalSetting(PortalId, "IntegerProperty", integerString, true, Null.NullString, false));
			Logger.LogDebug("SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): integerString → {IntegerString}", stringValue, integerValue, datetimeValue, timeSpanValue, integerString);
            var dateTimeString = datetimeValue?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
            this.MockModuleController.Setup(mc => mc.UpdateModuleSetting(ModuleId, "DateTimeProperty", dateTimeString));
			Logger.LogDebug("SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): dateTimeString → {DateTimeString}", stringValue, integerValue, datetimeValue, timeSpanValue, dateTimeString);
            var timeSpanString = timeSpanValue?.ToString("c", CultureInfo.InvariantCulture) ?? string.Empty;
            this.MockModuleController.Setup(mc => mc.UpdateTabModuleSetting(TabModuleId, "TimeSpanProperty", timeSpanString));
			Logger.LogDebug("SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): timeSpanString → {TimeSpanString}", stringValue, integerValue, datetimeValue, timeSpanValue, timeSpanString);

            var settingsRepository = new MyNullableSettingsRepository();

			Logger.LogDebug("SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): settingsRepository → {SettingsRepository}", stringValue, integerValue, datetimeValue, timeSpanValue, settingsRepository);
            // Act
            settingsRepository.SaveSettings(moduleInfo, settings);
			Logger.LogInformation("SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue,integerValue,datetimeValue,timeSpanValue)={StringValue},{IntegerValue},{DatetimeValue},{TimeSpanValue}: save query executed in {Arg4} ms", stringValue, integerValue, datetimeValue, timeSpanValue, __logStart.ElapsedMilliseconds);

            // Assert
            this.MockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
			Logger.LogInformation("SaveSettings_CallsUpdateSetting_WithRightParameters(stringValue,integerValue,datetimeValue,timeSpanValue)={StringValue},{IntegerValue},{DatetimeValue},{TimeSpanValue}: database query executed in {Arg4} ms", stringValue, integerValue, datetimeValue, timeSpanValue, __logStart.ElapsedMilliseconds);
        }

        private void GetSettings_GetsValues_FromCorrectSettings(string stringValue, int? integerValue, DateTime? datetimeValue, TimeSpan? timeSpanValue)
        {
			Logger.LogDebug(">>> Entering GetSettings_GetsValues_FromCorrectSettings(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // Arrange
            var expectedStringValue = stringValue ?? string.Empty;
            var moduleInfo = GetModuleInfo;
            var portalSettings = new Dictionary<string, string> { ["IntegerProperty"] = integerValue?.ToString() ?? string.Empty, };
            var moduleSettings = new Hashtable { ["DateTimeProperty"] = datetimeValue?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty, ["StringProperty"] = expectedStringValue, };
			Logger.LogDebug("GetSettings_GetsValues_FromCorrectSettings(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): portalSettings → {PortalSettings}", stringValue, integerValue, datetimeValue, timeSpanValue, portalSettings);
			Logger.LogDebug("GetSettings_GetsValues_FromCorrectSettings(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): moduleSettings → {ModuleSettings}", stringValue, integerValue, datetimeValue, timeSpanValue, moduleSettings);
            var tabModuleSettings = new Hashtable { ["TimeSpanProperty"] = timeSpanValue?.ToString("c", CultureInfo.InvariantCulture) ?? string.Empty, };

			Logger.LogDebug("GetSettings_GetsValues_FromCorrectSettings(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): tabModuleSettings → {TabModuleSettings}", stringValue, integerValue, datetimeValue, timeSpanValue, tabModuleSettings);
            this.MockPortalSettings(moduleInfo, portalSettings);
            this.MockModuleSettings(moduleInfo, moduleSettings);
            this.MockTabModuleSettings(moduleInfo, tabModuleSettings);

            var settingsRepository = new MyNullableSettingsRepository();

			Logger.LogDebug("GetSettings_GetsValues_FromCorrectSettings(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): settingsRepository → {SettingsRepository}", stringValue, integerValue, datetimeValue, timeSpanValue, settingsRepository);
            // Act
            var settings = settingsRepository.GetSettings(moduleInfo);

			Logger.LogDebug("GetSettings_GetsValues_FromCorrectSettings(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue}): settings → {Settings}", stringValue, integerValue, datetimeValue, timeSpanValue, settings);
			Logger.LogInformation("GetSettings_GetsValues_FromCorrectSettings(stringValue,integerValue,datetimeValue,timeSpanValue)={StringValue},{IntegerValue},{DatetimeValue},{TimeSpanValue}: find query executed in {Arg4} ms", stringValue, integerValue, datetimeValue, timeSpanValue, __logStart.ElapsedMilliseconds);
            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(settings.StringProperty, Is.EqualTo(expectedStringValue), "The retrieved string property value is not equal to the stored one");
                Assert.That(settings.IntegerProperty, Is.EqualTo(integerValue), "The retrieved integer property value is not equal to the stored one");
                Assert.That(settings.DateTimeProperty, Is.EqualTo(datetimeValue), "The retrieved datetime property value is not equal to the stored one");
                Assert.That(settings.TimeSpanProperty, Is.EqualTo(timeSpanValue), "The retrieved timespan property value is not equal to the stored one");
            }
            this.MockRepository.VerifyAll();
			Logger.LogDebug("<<< Exiting GetSettings_GetsValues_FromCorrectSettings(stringValue={StringValue},integerValue={IntegerValue},datetimeValue={DatetimeValue},timeSpanValue={TimeSpanValue})", stringValue, integerValue, datetimeValue, timeSpanValue);
        }

        public class MyNullableSettings
        {
            [ModuleSetting]
            public string StringProperty { get; set; } = "Default Value";

            [PortalSetting]
            public int? IntegerProperty { get; set; } = 500;

            [ModuleSetting]
            public DateTime? DateTimeProperty { get; set; } = DateTime.MaxValue;

            [TabModuleSetting]
            public TimeSpan? TimeSpanProperty { get; set; } = TimeSpan.FromHours(12);
        }

        public class MyNullableSettingsRepository : SettingsRepository<MyNullableSettings>
        {
        }
    }
}
