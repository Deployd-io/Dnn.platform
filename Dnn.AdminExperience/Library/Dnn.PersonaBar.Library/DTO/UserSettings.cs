// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Dto
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Persona Bar Settings For User.</summary>
    [DataContract]
    public partial class UserSettings : Dictionary<string, object>, IXmlSerializable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [IgnoreDataMember]
        public bool ExpandPersonaBar
        {
            get
            {
                return Convert.ToBoolean(this["expandPersonaBar"], CultureInfo.InvariantCulture);
            }

            set
            {
                this["expandPersonaBar"] = value;
            }
        }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
			Logger.LogDebug(">>> Entering GetSchema()");
			Logger.LogDebug("<<< Exiting GetSchema()");
            return null;
        }

        /// <inheritdoc />
        public void ReadXml(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadXml(reader={Reader})", reader);
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
            {
                return;
            }

            switch (reader.Name)
            {
                case "data":
                    this.ReadSettings(reader);
                    break;
                default:
#pragma warning disable CS0618 // Type or member is obsolete
                    this.ReadLegacySettings(reader);
#pragma warning restore CS0618 // Type or member is obsolete
                    break;
            }
			Logger.LogDebug("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        /// <inheritdoc />
        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            writer.WriteStartElement("data");
            writer.WriteCData(Json.Serialize(this));
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }

        private void ReadSettings(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadSettings(reader={Reader})", reader);
            reader.ReadStartElement("data");

            var settingsData = reader.ReadContentAsString();
            var settings = Json.Deserialize<IDictionary<string, object>>(settingsData);
			Logger.LogDebug("ReadSettings(reader={Reader}): settingsData → {SettingsData}", reader, settingsData);
			Logger.LogDebug("ReadSettings(reader={Reader}): settings → {Settings}", reader, settings);
            foreach (var key in settings.Keys)
            {
                this[key] = settings[key];
            }

            reader.ReadEndElement();
			Logger.LogDebug("<<< Exiting ReadSettings(reader={Reader})", reader);
        }

        [DnnDeprecated(9, 2, 1, "The method add for backward compatible, should remove this in future release.")]
        private partial void ReadLegacySettings(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadLegacySettings(reader={Reader})", reader);
            while (!reader.EOF && reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                var settingName = reader.Name;
                reader.ReadStartElement();

                var settingValue = string.Empty;
                if (reader.NodeType == XmlNodeType.Text)
				Logger.LogDebug("ReadLegacySettings(reader={Reader}): settingValue → {SettingValue}", reader, settingValue);
                {
                    settingValue = reader.ReadContentAsString();
                }

                if (!string.IsNullOrEmpty(settingValue))
                {
                    switch (settingName)
                    {
                        case "ExpandPersonaBar":
                            this["expandPersonaBar"] = Convert.ToBoolean(settingValue, CultureInfo.InvariantCulture);
                            break;
                        case "ActiveIdentifier":
                            this["activeIdentifier"] = settingValue;
                            break;
                        case "ActivePath":
                            this["activePath"] = settingValue;
                            break;
                        case "ExpandTasksPane":
                            this["expandTasksPane"] = Convert.ToBoolean(settingValue, CultureInfo.InvariantCulture);
                            break;
                        case "ComparativeTerm":
                            this["comparativeTerm"] = settingValue;
                            break;
                        case "EndDate":
                            this["endDate"] = Convert.ToDateTime(settingValue, CultureInfo.InvariantCulture);
                            break;
                        case "Legends":
                            this["legends"] = settingValue.Split(',');
                            break;
                        case "Period":
                            this["period"] = settingValue;
                            break;
                        case "StartDate":
                            this["startDate"] = Convert.ToDateTime(settingValue, CultureInfo.InvariantCulture);
                            break;
                    }
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    reader.ReadEndElement();
                }
            }
			Logger.LogDebug("<<< Exiting ReadLegacySettings(reader={Reader})", reader);
        }
    }
}
