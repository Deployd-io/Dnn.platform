// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls.Config
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;

    [Serializable]
    [XmlRoot("RewriterConfig")]
    public class RewriterConfiguration
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RewriterConfiguration));
        private static readonly object ThreadLocker = new object();
        private RewriterRuleCollection rules;

        public RewriterRuleCollection Rules
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

        public static RewriterConfiguration GetConfig()
        {
			Logger.LogDebug(">>> Entering GetConfig()");
            var config = new RewriterConfiguration { Rules = new RewriterRuleCollection() };
            string filePath = string.Empty;
			Logger.LogDebug("GetConfig(): config → {Config}", config);
			Logger.LogDebug("GetConfig(): filePath → {FilePath}", filePath);
            try
            {
                config = (RewriterConfiguration)DataCache.GetCache("RewriterConfig");
                if (config == null)
                {
                    lock (ThreadLocker)
                    {
                        config = (RewriterConfiguration)DataCache.GetCache("RewriterConfig");
                        if (config == null)
                        {
                            filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.SiteUrls);

                            // Create a FileStream for the Config file
                            XPathDocument doc;
                            using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (var xmlReader = XmlReader.Create(fileReader))
                            {
                                doc = new XPathDocument(xmlReader);
								Logger.LogDebug("GetConfig(): doc → {Doc}", doc);
                            }

                            config = new RewriterConfiguration { Rules = new RewriterRuleCollection() };
                            foreach (XPathNavigator nav in doc.CreateNavigator().Select("RewriterConfig/Rules/RewriterRule"))
                            {
                                var rule = new RewriterRule
                                {
                                    LookFor = nav.SelectSingleNode("LookFor").Value,
                                    SendTo = nav.SelectSingleNode("SendTo").Value,
                                };
                                config.Rules.Add(rule);
                            }

                            if (File.Exists(filePath))
                            {
                                // Set back into Cache
                                DataCache.SetCache("RewriterConfig", config, new DNNCacheDependency(filePath));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // log it
                var log = new LogInfo { LogTypeKey = nameof(EventLogType.HOST_ALERT) };
                log.AddProperty("UrlRewriter.RewriterConfiguration", "GetConfig Failed");
				Logger.LogDebug("GetConfig(): log → {Log}", log);
                log.AddProperty("FilePath", filePath);
                log.AddProperty("ExceptionMessage", ex.Message);
                LogController.Instance.AddLog(log);
                Logger.Error(log);
            }

			Logger.LogDebug("<<< Exiting GetConfig()");
            return config;
        }

        public static void SaveConfig(RewriterRuleCollection rules)
        {
			Logger.LogDebug(">>> Entering SaveConfig(rules={Rules})", rules);
            if (rules != null)
            {
                var config = new RewriterConfiguration { Rules = rules };

				Logger.LogDebug("SaveConfig(rules={Rules}): config → {Config}", rules, config);
                // Create a new Xml Serializer
                var ser = new XmlSerializer(typeof(RewriterConfiguration));

				Logger.LogDebug("SaveConfig(rules={Rules}): ser → {Ser}", rules, ser);
                // Create a FileStream for the Config file
                var filePath = Common.Utilities.Config.GetPathToFile(Common.Utilities.Config.ConfigFileType.SiteUrls);
                if (File.Exists(filePath))
				Logger.LogDebug("SaveConfig(rules={Rules}): filePath → {FilePath}", rules, filePath);
                {
                    // make sure file is not read-only
                    File.SetAttributes(filePath, FileAttributes.Normal);
                }

                using (var fileWriter = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
                using (var writer = new StreamWriter(fileWriter))
                {
                    // Serialize the RewriterConfiguration
                    ser.Serialize(writer, config);

                    // Close the Writers
                    writer.Close();
                    fileWriter.Close();
                }

                // Set Cache
                DataCache.SetCache("RewriterConfig", config, new DNNCacheDependency(filePath));
            }
			Logger.LogDebug("<<< Exiting SaveConfig(rules={Rules})", rules);
        }
    }
}
