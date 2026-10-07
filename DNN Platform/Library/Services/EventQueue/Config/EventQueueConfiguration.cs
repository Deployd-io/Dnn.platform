// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue.Config
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Cache;

    [Serializable]
    internal class EventQueueConfiguration
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventQueueConfiguration));
        /// <summary>Initializes a new instance of the <see cref="EventQueueConfiguration"/> class.</summary>
        internal EventQueueConfiguration()
        {
			Logger.LogError(">>> Entering EventQueueConfiguration()");
            this.PublishedEvents = new Dictionary<string, PublishedEvent>();
            this.EventQueueSubscribers = new Dictionary<string, SubscriberInfo>();
        }

        internal Dictionary<string, SubscriberInfo> EventQueueSubscribers { get; set; }

        internal Dictionary<string, PublishedEvent> PublishedEvents { get; set; }

        public static void RegisterEventSubscription(EventQueueConfiguration config, string eventname, SubscriberInfo subscriber)
        {
			Logger.LogError(">>> Entering RegisterEventSubscription(config={Config},eventname={Eventname},subscriber={Subscriber})", config, eventname, subscriber);
            var e = new PublishedEvent();
            e.EventName = eventname;
			Logger.LogDebug("RegisterEventSubscription(config={Config},eventname={Eventname},subscriber={Subscriber}): e.EventName → {Arg3}", config, eventname, subscriber, e.EventName);
			Logger.LogError("RegisterEventSubscription(config={Config},eventname={Eventname},subscriber={Subscriber}): e → {E}", config, eventname, subscriber, e);
            e.Subscribers = subscriber.ID;
			Logger.LogDebug("RegisterEventSubscription(config={Config},eventname={Eventname},subscriber={Subscriber}): e.Subscribers → {Arg3}", config, eventname, subscriber, e.Subscribers);
            config.PublishedEvents.Add(e.EventName, e);
            if (!config.EventQueueSubscribers.ContainsKey(subscriber.ID))
            {
                config.EventQueueSubscribers.Add(subscriber.ID, subscriber);
            }
			Logger.LogWarning("<<< Exiting RegisterEventSubscription(config={Config},eventname={Eventname},subscriber={Subscriber})", config, eventname, subscriber);
        }

        internal static EventQueueConfiguration GetConfig()
        {
			Logger.LogError(">>> Entering GetConfig()");
            var config = (EventQueueConfiguration)DataCache.GetCache("EventQueueConfig");
			Logger.LogError("GetConfig(): config → {Config}", config);
            if (config == null)
            {
                string filePath = Globals.HostMapPath + "EventQueue\\EventQueue.config";
                if (File.Exists(filePath))
                {
                    config = new EventQueueConfiguration();

                    // Deserialize into EventQueueConfiguration
                    config.Deserialize(FileSystemUtils.ReadFile(filePath));

                    // Set back into Cache
                    DataCache.SetCache("EventQueueConfig", config, new DNNCacheDependency(filePath));
                }
                else
                {
                    // make a default config file
                    config = new EventQueueConfiguration();
                    config.PublishedEvents = new Dictionary<string, PublishedEvent>();
                    config.EventQueueSubscribers = new Dictionary<string, SubscriberInfo>();
                    var subscriber = new SubscriberInfo("DNN Core");
					Logger.LogError("GetConfig(): subscriber → {Subscriber}", subscriber);
                    RegisterEventSubscription(config, "Application_Start", subscriber);
                    RegisterEventSubscription(config, "Application_Start_FirstRequest", subscriber);
                    SaveConfig(config, filePath);
                }
            }

			Logger.LogWarning("<<< Exiting GetConfig()");
            return config;
        }

        internal static void SaveConfig(EventQueueConfiguration config, string filePath)
        {
			Logger.LogError(">>> Entering SaveConfig(config={Config},filePath={FilePath})", config, filePath);
            StreamWriter oStream = File.CreateText(filePath);
            oStream.WriteLine(config.Serialize());
			Logger.LogError("SaveConfig(config={Config},filePath={FilePath}): oStream → {OStream}", config, filePath, oStream);
            oStream.Close();

            // Set back into Cache
            DataCache.SetCache("EventQueueConfig", config, new DNNCacheDependency(filePath));
			Logger.LogWarning("<<< Exiting SaveConfig(config={Config},filePath={FilePath})", config, filePath);
        }

        private void Deserialize(string configXml)
        {
			Logger.LogError(">>> Entering Deserialize(configXml={ConfigXml})", configXml);
            if (!string.IsNullOrEmpty(configXml))
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                using (var configReader = XmlReader.Create(new StringReader(configXml), new XmlReaderSettings { XmlResolver = null, }))
                {
                    xmlDoc.Load(configReader);
                }

                foreach (XmlElement xmlItem in xmlDoc.SelectNodes("/EventQueueConfig/PublishedEvents/Event"))
                {
                    var oPublishedEvent = new PublishedEvent();
					Logger.LogError("Deserialize(configXml={ConfigXml}): oPublishedEvent → {OPublishedEvent}", configXml, oPublishedEvent);
                    oPublishedEvent.EventName = xmlItem.SelectSingleNode("EventName").InnerText;
                    oPublishedEvent.Subscribers = xmlItem.SelectSingleNode("Subscribers").InnerText;
                    this.PublishedEvents.Add(oPublishedEvent.EventName, oPublishedEvent);
                }

                foreach (XmlElement xmlItem in xmlDoc.SelectNodes("/EventQueueConfig/EventQueueSubscribers/Subscriber"))
                {
                    var oSubscriberInfo = new SubscriberInfo();
					Logger.LogError("Deserialize(configXml={ConfigXml}): oSubscriberInfo → {OSubscriberInfo}", configXml, oSubscriberInfo);
                    oSubscriberInfo.ID = xmlItem.SelectSingleNode("ID").InnerText;
                    oSubscriberInfo.Name = xmlItem.SelectSingleNode("Name").InnerText;
                    oSubscriberInfo.Address = xmlItem.SelectSingleNode("Address").InnerText;
                    oSubscriberInfo.Description = xmlItem.SelectSingleNode("Description").InnerText;
                    oSubscriberInfo.PrivateKey = xmlItem.SelectSingleNode("PrivateKey").InnerText;
                    this.EventQueueSubscribers.Add(oSubscriberInfo.ID, oSubscriberInfo);
                }
            }
			Logger.LogWarning("<<< Exiting Deserialize(configXml={ConfigXml})", configXml);
        }

        private string Serialize()
        {
			Logger.LogError(">>> Entering Serialize()");
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
			Logger.LogDebug("Serialize(): settings.ConformanceLevel → {Arg0}", settings.ConformanceLevel);
			Logger.LogDebug("Serialize(): settings.CloseOutput → {Arg0}", settings.CloseOutput);
			Logger.LogDebug("Serialize(): settings.OmitXmlDeclaration → {Arg0}", settings.OmitXmlDeclaration);
			Logger.LogError("Serialize(): settings → {Settings}", settings);
            settings.Indent = true;
			Logger.LogDebug("Serialize(): settings.Indent → {Arg0}", settings.Indent);
            settings.CloseOutput = true;
            settings.OmitXmlDeclaration = false;

            var sb = new StringBuilder();
			Logger.LogError("Serialize(): sb → {Sb}", sb);

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("EventQueueConfig");

                writer.WriteStartElement("PublishedEvents");
                foreach (string key in this.PublishedEvents.Keys)
                {
                    writer.WriteStartElement("Event");

                    writer.WriteElementString("EventName", this.PublishedEvents[key].EventName);
                    writer.WriteElementString("Subscribers", this.PublishedEvents[key].Subscribers);

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.WriteStartElement("EventQueueSubscribers");
                foreach (string key in this.EventQueueSubscribers.Keys)
                {
                    writer.WriteStartElement("Subscriber");

                    writer.WriteElementString("ID", this.EventQueueSubscribers[key].ID);
                    writer.WriteElementString("Name", this.EventQueueSubscribers[key].Name);
                    writer.WriteElementString("Address", this.EventQueueSubscribers[key].Address);
                    writer.WriteElementString("Description", this.EventQueueSubscribers[key].Description);
                    writer.WriteElementString("PrivateKey", this.EventQueueSubscribers[key].PrivateKey);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                // Close EventQueueConfig
                writer.WriteEndElement();

                writer.Close();
				Logger.LogWarning("<<< Exiting Serialize()");
                return sb.ToString();
            }
        }
    }
}
