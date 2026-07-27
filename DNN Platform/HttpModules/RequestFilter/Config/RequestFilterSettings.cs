// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules.RequestFilter
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Settings related to request filters.</summary>
    [Serializable]
    [XmlRoot("RewriterConfig")]
    public class RequestFilterSettings
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string RequestFilterConfig = "RequestFilter.Config";
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="RequestFilterSettings"/> class.</summary>
        [Obsolete(
            "Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public RequestFilterSettings()
            : this(null)
        {
		Logger.LogDebug(">>> Entering RequestFilterSettings()");
		Logger.LogDebug("<<< Exiting RequestFilterSettings()");
		Logger.LogDebug(">>> Entering RequestFilterSettings(hostSettings={HostSettings})", hostSettings);
		Logger.LogDebug("<<< Exiting RequestFilterSettings(hostSettings={HostSettings})", hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="RequestFilterSettings"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public RequestFilterSettings(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("RequestFilterSettings(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        /// <summary>Gets a value indicating whether request filters are enabled.</summary>
        public bool Enabled => this.hostSettings.EnableRequestFilters;

        /// <summary>Gets or sets the collection of rules.</summary>
        public List<RequestFilterRule> Rules { get; set; } = new();

        /// <summary>Get the current settings from the XML config file.</summary>
        /// <returns>A <see cref="RequestFilterSettings"/> instance.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public static RequestFilterSettings GetSettings()
            => GetSettings(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>());
			Logger.LogDebug(">>> Entering GetSettings()");

        /// <summary>Get the current settings from the XML config file.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <returns>A <see cref="RequestFilterSettings"/> instance.</returns>
        public static RequestFilterSettings GetSettings(IHostSettings hostSettings, IApplicationStatusInfo appStatus)
        {
            var settings = (RequestFilterSettings)DataCache.GetCache(RequestFilterConfig);
            if (settings == null)
			Logger.LogDebug("GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): settings → {Settings}", hostSettings, appStatus, settings);
            {
                settings = new RequestFilterSettings(hostSettings);
                string filePath = Config.GetPathToFile(appStatus, Config.ConfigFileType.DotNetNuke);

                // Create a FileStream for the Config file
                XPathDocument doc;
                using (var fileReader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var xmlReader = XmlReader.Create(fileReader))
                {
                    doc = new XPathDocument(xmlReader);
					Logger.LogDebug("GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): doc → {Doc}", hostSettings, appStatus, doc);
                }

                XPathNodeIterator ruleList = doc.CreateNavigator().Select("/configuration/blockrequests/rule");
                while (ruleList.MoveNext())
				Logger.LogDebug("GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): ruleList → {RuleList}", hostSettings, appStatus, ruleList);
                {
                    try
                    {
                        string serverVar = ruleList.Current.GetAttribute("servervar", string.Empty);
                        string values = ruleList.Current.GetAttribute("values", string.Empty);
                        var ac = (RequestFilterRuleType)Enum.Parse(typeof(RequestFilterRuleType), ruleList.Current.GetAttribute("action", string.Empty));
                        var op = (RequestFilterOperatorType)Enum.Parse(typeof(RequestFilterOperatorType), ruleList.Current.GetAttribute("operator", string.Empty));
						Logger.LogDebug("GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): ac → {Ac}", hostSettings, appStatus, ac);
						Logger.LogDebug("GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): op → {Op}", hostSettings, appStatus, op);
                        string location = ruleList.Current.GetAttribute("location", string.Empty);
                        var rule = new RequestFilterRule(serverVar, values, op, ac, location);
						Logger.LogDebug("GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): location → {Location}", hostSettings, appStatus, location);
						Logger.LogDebug("GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): rule → {Rule}", hostSettings, appStatus, rule);
                        settings.Rules.Add(rule);
                    }
                    catch (Exception ex)
                    {
						Logger.LogError("Exception in GetSettings(): {Arg0}", ex.Message);
						Logger.LogError("Exception in GetSettings(hostSettings={HostSettings},appStatus={AppStatus}): {Arg2}", hostSettings, appStatus, ex.Message);
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(new RequestFilterException($"Unable to read RequestFilter Rule: {ruleList.Current.OuterXml}:", ex));
                    }
                }

                if (File.Exists(filePath))
                {
                    // Set back into Cache
                    DataCache.SetCache(RequestFilterConfig, settings, new DNNCacheDependency(filePath));
                }
            }

			Logger.LogDebug("<<< Exiting GetSettings()");
            return settings;
        }

        /// <summary>Saves the rules into the <c>DotNetNuke.config</c> file.</summary>
        /// <param name="rules">The rules.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public static void Save(List<RequestFilterRule> rules)
            => Save(Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), rules);
			Logger.LogDebug(">>> Entering Save(rules={Rules})", rules);

        /// <summary>Saves the rules into the <c>DotNetNuke.config</c> file.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="rules">The rules.</param>
        public static void Save(IApplicationStatusInfo appStatus, List<RequestFilterRule> rules)
        {
            string filePath = Config.GetPathToFile(appStatus, Config.ConfigFileType.DotNetNuke);
			Logger.LogDebug("<<< Exiting Save(rules={Rules})", rules);
            if (!File.Exists(filePath))
            {
                string defaultConfigFile = appStatus.ApplicationMapPath + Globals.glbConfigFolder + Globals.glbDotNetNukeConfig;
                if (File.Exists(defaultConfigFile))
                {
                    File.Copy(defaultConfigFile, filePath, true);
                }
            }

            var doc = new XmlDocument { XmlResolver = null };
            using (var xmlReader = XmlReader.Create(filePath, new XmlReaderSettings { XmlResolver = null, }))
            {
                doc.Load(xmlReader);
            }

            XmlNode ruleRoot = doc.SelectSingleNode("/configuration/blockrequests");
            ruleRoot.RemoveAll();
			Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): ruleRoot → {RuleRoot}", appStatus, rules, ruleRoot);
            foreach (RequestFilterRule rule in rules)
            {
                XmlElement xmlRule = doc.CreateElement("rule");
                XmlAttribute var = doc.CreateAttribute("servervar");
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): xmlRule → {XmlRule}", appStatus, rules, xmlRule);
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): var → {Var}", appStatus, rules, var);
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): var.Value → {Arg2}", appStatus, rules, var.Value);
                var.Value = rule.ServerVariable;
                xmlRule.Attributes.Append(var);
                XmlAttribute val = doc.CreateAttribute("values");
                val.Value = rule.RawValue;
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): val → {Val}", appStatus, rules, val);
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): val.Value → {Arg2}", appStatus, rules, val.Value);
                xmlRule.Attributes.Append(val);
                XmlAttribute op = doc.CreateAttribute("operator");
                op.Value = rule.Operator.ToString();
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): op → {Op}", appStatus, rules, op);
                xmlRule.Attributes.Append(op);
                XmlAttribute action = doc.CreateAttribute("action");
                action.Value = rule.Action.ToString();
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): action → {Action}", appStatus, rules, action);
                xmlRule.Attributes.Append(action);
                XmlAttribute location = doc.CreateAttribute("location");
                location.Value = rule.Location;
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): location → {Location}", appStatus, rules, location);
				Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): location.Value → {Arg2}", appStatus, rules, location.Value);
                xmlRule.Attributes.Append(location);
                ruleRoot.AppendChild(xmlRule);
            }

            var settings = new XmlWriterSettings();
            settings.Indent = true;
			Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): settings → {Settings}", appStatus, rules, settings);
			Logger.LogDebug("Save(appStatus={AppStatus},rules={Rules}): settings.Indent → {Arg2}", appStatus, rules, settings.Indent);
            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                doc.WriteContentTo(writer);
            }
        }
    }
}
