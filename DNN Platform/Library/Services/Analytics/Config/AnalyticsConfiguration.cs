// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Analytics.Config
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;

    [Serializable]
    [XmlRoot("AnalyticsConfig")]
    public class AnalyticsConfiguration
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AnalyticsConfiguration));
        private AnalyticsRuleCollection rules;
        private AnalyticsSettingCollection settings;

        public AnalyticsSettingCollection Settings
        {
            get
            {
                return this.settings;
            }

            set
            {
                this.settings = value;
            }
        }

        public AnalyticsRuleCollection Rules
        {
            get
            {
                return this.rules;
            }

            set
            {
                this.rules = value;
            }
        }

        public static AnalyticsConfiguration GetConfig(string analyticsEngineName)
        {
			Logger.LogDebug(">>> Entering GetConfig(analyticsEngineName={AnalyticsEngineName})", analyticsEngineName);
            string cacheKey = analyticsEngineName + "." + PortalSettings.Current.PortalId;

            var config = new AnalyticsConfiguration();
			Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): config → {Config}", analyticsEngineName, config);
            config.Rules = new AnalyticsRuleCollection();
            config.Settings = new AnalyticsSettingCollection();

            string filePath = string.Empty;
			Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): filePath → {FilePath}", analyticsEngineName, filePath);
            try
            {
                config = (AnalyticsConfiguration)DataCache.GetCache(cacheKey);
                if (config == null)
                {
                    filePath = PortalSettings.Current.HomeDirectoryMapPath + @"\" + analyticsEngineName + ".config";

                    if (!File.Exists(filePath))
                    {
                        return null;
                    }

                    // Create a FileStream for the Config file
                    XPathDocument doc;
                    using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var xmlReader = XmlReader.Create(fileReader))
                    {
                        doc = new XPathDocument(xmlReader);
                    }
					Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): doc → {Doc}", analyticsEngineName, doc);

                    config = new AnalyticsConfiguration();
                    config.Rules = new AnalyticsRuleCollection();
                    config.Settings = new AnalyticsSettingCollection();

                    var allSettings = new Hashtable();
					Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): allSettings → {AllSettings}", analyticsEngineName, allSettings);
                    foreach (XPathNavigator nav in doc.CreateNavigator().Select("AnalyticsConfig/Settings/AnalyticsSetting"))
                    {
                        var setting = new AnalyticsSetting();
						Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): setting → {Setting}", analyticsEngineName, setting);
                        setting.SettingName = nav.SelectSingleNode("SettingName").Value;
                        setting.SettingValue = nav.SelectSingleNode("SettingValue")?.Value;
                        config.Settings.Add(setting);
                    }

                    foreach (XPathNavigator nav in doc.CreateNavigator().Select("AnalyticsConfig/Rules/AnalyticsRule"))
                    {
                        var rule = new AnalyticsRule();
						Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): rule → {Rule}", analyticsEngineName, rule);
                        rule.RoleId = Convert.ToInt32(nav.SelectSingleNode("RoleId").Value, CultureInfo.InvariantCulture);
                        rule.TabId = Convert.ToInt32(nav.SelectSingleNode("TabId").Value, CultureInfo.InvariantCulture);
                        rule.Label = nav.SelectSingleNode("Label").Value;
                        var valueNode = nav.SelectSingleNode("Value");
						Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): valueNode → {ValueNode}", analyticsEngineName, valueNode);
                        if (valueNode != null)
                        {
                            rule.Value = valueNode.Value;
							Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): rule.Value → {Arg1}", analyticsEngineName, rule.Value);
                        }

                        config.Rules.Add(rule);
                    }

                    if (File.Exists(filePath))
                    {
                        // Set back into Cache
                        DataCache.SetCache(cacheKey, config, new DNNCacheDependency(filePath));
                    }
                }
            }
            catch (Exception ex)
            {
                // log it
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.ADMIN_ALERT) };
				Logger.LogDebug("GetConfig(analyticsEngineName={AnalyticsEngineName}): log → {Log}", analyticsEngineName, log);
                log.AddProperty("Analytics.AnalyticsConfiguration", "GetConfig Failed");
                log.AddProperty("FilePath", filePath);
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(ex);
            }

			Logger.LogDebug("<<< Exiting GetConfig(analyticsEngineName={AnalyticsEngineName})", analyticsEngineName);
            return config;
        }

        public static void SaveConfig(string analyticsEngineName, AnalyticsConfiguration config)
        {
			Logger.LogDebug(">>> Entering SaveConfig(analyticsEngineName={AnalyticsEngineName},config={Config})", analyticsEngineName, config);
            string cacheKey = analyticsEngineName + "." + PortalSettings.Current.PortalId;
            if (config.Settings != null)
            {
                // Create a new Xml Serializer
                var ser = new XmlSerializer(typeof(AnalyticsConfiguration));
				Logger.LogDebug("SaveConfig(analyticsEngineName={AnalyticsEngineName},config={Config}): ser → {Ser}", analyticsEngineName, config, ser);
                string filePath = string.Empty;
				Logger.LogDebug("SaveConfig(analyticsEngineName={AnalyticsEngineName},config={Config}): filePath → {FilePath}", analyticsEngineName, config, filePath);

                // Create a FileStream for the Config file
                filePath = PortalSettings.Current.HomeDirectoryMapPath + @"\" + analyticsEngineName + ".config";
                if (File.Exists(filePath))
                {
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                using (var fileWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var writer = new StreamWriter(fileWriter))
                {
                    // Serialize the AnalyticsConfiguration
                    ser.Serialize(writer, config);

                    // Close the Writers
                    writer.Close();
                    fileWriter.Close();
                }

                DataCache.SetCache(cacheKey, config, new DNNCacheDependency(filePath));
            }
			Logger.LogDebug("<<< Exiting SaveConfig(analyticsEngineName={AnalyticsEngineName},config={Config})", analyticsEngineName, config);
        }
    }
}
