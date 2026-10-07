// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Common.Utilities;

    public enum MessagePriority
    {
        /// <summary>High priority.</summary>
        High = 0,

        /// <summary>Medium priority.</summary>
        Medium = 1,

        /// <summary>Low priority.</summary>
        Low = 2,
    }

    [Serializable]
    public class EventMessage
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventMessage));
        private NameValueCollection attributes;
        private string authorizedRoles = Null.NullString;
        private string body = Null.NullString;
        private int eventMessageID = Null.NullInteger;
        private string exceptionMessage = Null.NullString;
        private DateTime expirationDate;
        private MessagePriority priority = MessagePriority.Low;
        private string processorCommand = Null.NullString;
        private string processorType = Null.NullString;
        private string sender = Null.NullString;
        private DateTime sentDate;
        private string subscribers = Null.NullString;

        /// <summary>Initializes a new instance of the <see cref="EventMessage"/> class.</summary>
        public EventMessage()
        {
			Logger.LogError(">>> Entering EventMessage()");
            this.attributes = new NameValueCollection();
			Logger.LogDebug("EventMessage(): attributes → {Attributes}", attributes);
        Logger.LogWarning("<<< Exiting EventMessage()");
        }

        public int EventMessageID
        {
            get
            {
                return this.eventMessageID;
            }

            set
            {
                this.eventMessageID = value;
            }
        }

        public string ProcessorType
        {
            get
            {
                if (this.processorType == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.processorType;
                }
            }

            set
            {
                this.processorType = value;
            }
        }

        public string ProcessorCommand
        {
            get
            {
                if (this.processorCommand == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.processorCommand;
                }
            }

            set
            {
                this.processorCommand = value;
            }
        }

        public string Body
        {
            get
            {
                if (this.body == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.body;
                }
            }

            set
            {
                this.body = value;
            }
        }

        public string Sender
        {
            get
            {
                if (this.sender == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.sender;
                }
            }

            set
            {
                this.sender = value;
            }
        }

        public string Subscribers
        {
            get
            {
                if (this.subscribers == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.subscribers;
                }
            }

            set
            {
                this.subscribers = value;
            }
        }

        public string AuthorizedRoles
        {
            get
            {
                if (this.authorizedRoles == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.authorizedRoles;
                }
            }

            set
            {
                this.authorizedRoles = value;
            }
        }

        public MessagePriority Priority
        {
            get
            {
                return this.priority;
            }

            set
            {
                this.priority = value;
            }
        }

        public string ExceptionMessage
        {
            get
            {
                if (this.exceptionMessage == null)
                {
                    return string.Empty;
                }
                else
                {
                    return this.exceptionMessage;
                }
            }

            set
            {
                this.exceptionMessage = value;
            }
        }

        public DateTime SentDate
        {
            get
            {
                return this.sentDate.ToLocalTime();
            }

            set
            {
                this.sentDate = value.ToUniversalTime();
            }
        }

        public DateTime ExpirationDate
        {
            get
            {
                return this.expirationDate.ToLocalTime();
            }

            set
            {
                this.expirationDate = value.ToUniversalTime();
            }
        }

        public NameValueCollection Attributes
        {
            get
            {
                return this.attributes;
            }

            set
            {
                this.attributes = value;
            }
        }

        public void DeserializeAttributes(string configXml)
        {
			Logger.LogError(">>> Entering DeserializeAttributes(configXml={ConfigXml})", configXml);
            string attName = Null.NullString;
            string attValue = Null.NullString;
			Logger.LogError("DeserializeAttributes(configXml={ConfigXml}): attValue → {AttValue}", configXml, attValue);
            var settings = new XmlReaderSettings();
			Logger.LogError("DeserializeAttributes(configXml={ConfigXml}): settings → {Settings}", configXml, settings);
            settings.ConformanceLevel = ConformanceLevel.Fragment;
			Logger.LogDebug("DeserializeAttributes(configXml={ConfigXml}): settings.ConformanceLevel → {Arg1}", configXml, settings.ConformanceLevel);
            using var reader = XmlReader.Create(new StringReader(configXml));
			Logger.LogError("DeserializeAttributes(configXml={ConfigXml}): reader → {Reader}", configXml, reader);
			Logger.LogError("DeserializeAttributes(configXml={ConfigXml}): attName → {AttName}", configXml, attName);
            reader.ReadStartElement("Attributes");
            if (!reader.IsEmptyElement)
            {
                // Loop throug the Attributes
                do
                {
                    reader.ReadStartElement("Attribute");

                    // Load it from the Xml
                    reader.ReadStartElement("Name");
                    attName = reader.ReadString();
                    reader.ReadEndElement();
                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement("Value");
                        attValue = reader.ReadString();
                        reader.ReadEndElement();
                    }
                    else
                    {
                        reader.Read();
                    }

                    // Add attribute to the collection
                    this.attributes.Add(attName, attValue);
                }
                while (reader.ReadToNextSibling("Attribute"));
            }
			Logger.LogWarning("<<< Exiting DeserializeAttributes(configXml={ConfigXml})", configXml);
        }

        public string SerializeAttributes()
        {
			Logger.LogError(">>> Entering SerializeAttributes()");
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
			Logger.LogDebug("SerializeAttributes(): settings.ConformanceLevel → {Arg0}", settings.ConformanceLevel);
			Logger.LogError("SerializeAttributes(): settings → {Settings}", settings);
            settings.OmitXmlDeclaration = true;
			Logger.LogDebug("SerializeAttributes(): settings.OmitXmlDeclaration → {Arg0}", settings.OmitXmlDeclaration);

            var sb = new StringBuilder();
			Logger.LogError("SerializeAttributes(): sb → {Sb}", sb);

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartElement("Attributes");

                foreach (string key in this.Attributes.Keys)
                {
                    writer.WriteStartElement("Attribute");

                    // Write the Name element
                    writer.WriteElementString("Name", key);

                    // Write the Value element
                    if (this.Attributes[key].IndexOfAny("<'>\"&".ToCharArray()) > -1)
                    {
                        writer.WriteStartElement("Value");
                        writer.WriteCData(this.Attributes[key]);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        // Write value
                        writer.WriteElementString("Value", this.Attributes[key]);
                    }

                    // Close the Attribute node
                    writer.WriteEndElement();
                }

                // Close the Attributes node
                writer.WriteEndElement();

                // Close Writer
                writer.Close();

				Logger.LogWarning("<<< Exiting SerializeAttributes()");
                return sb.ToString();
            }
        }
    }
}
