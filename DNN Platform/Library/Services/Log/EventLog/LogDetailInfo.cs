// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Abstractions.Logging;

    /// <inheritdoc />
    [Serializable]
    public class LogDetailInfo : ILogDetailInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LogDetailInfo));
        /// <summary>Initializes a new instance of the <see cref="LogDetailInfo"/> class.</summary>
        public LogDetailInfo()
            : this(string.Empty, string.Empty)
        {
		Logger.LogDebug(">>> Entering LogDetailInfo()");
		Logger.LogDebug("<<< Exiting LogDetailInfo()");
		Logger.LogDebug(">>> Entering LogDetailInfo(name={Name},value={Value})", name, value);
        }

        /// <summary>Initializes a new instance of the <see cref="LogDetailInfo"/> class.</summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public LogDetailInfo(string name, string value)
        {
            this.PropertyName = name;
            this.PropertyValue = value;
			Logger.LogDebug("LogDetailInfo(name={Name},value={Value}): this.PropertyName → {Arg2}", name, value, this.PropertyName);
			Logger.LogDebug("<<< Exiting LogDetailInfo(name={Name},value={Value})", name, value);
        }

        /// <inheritdoc />
        public string PropertyName { get; set; }

        /// <inheritdoc />
        public string PropertyValue { get; set; }

        public void ReadXml(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadXml(reader={Reader})", reader);
            reader.ReadStartElement("PropertyName");
            this.PropertyName = reader.ReadString();
            reader.ReadEndElement();
            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement("PropertyValue");
                this.PropertyValue = reader.ReadString();
                reader.ReadEndElement();
            }
            else
            {
                reader.Read();
            }
			Logger.LogDebug("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        /// <inheritdoc />
        public override string ToString()
        {
			Logger.LogDebug(">>> Entering ToString()");
            var sb = new StringBuilder();
            sb.Append("<p><strong>");
			Logger.LogDebug("ToString(): sb → {Sb}", sb);
            sb.Append(this.PropertyName);
            sb.Append("</strong>: ");
            sb.Append(this.PropertyValue);
            sb.Append("</p>");
			Logger.LogDebug("<<< Exiting ToString()");
            return sb.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            writer.WriteStartElement("LogProperty");
            writer.WriteElementString("PropertyName", this.PropertyName);
            writer.WriteElementString("PropertyValue", this.PropertyValue);
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }
    }
}
