// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Services.Exceptions;

    /// <inheritdoc />
    [Serializable]
    public partial class LogInfo : ILogInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LogInfo));
        /// <summary>Initializes a new instance of the <see cref="LogInfo"/> class.</summary>
        public LogInfo()
        {
			Logger.LogError(">>> Entering LogInfo()");
            ((ILogInfo)this).LogGuid = Guid.NewGuid().ToString();
            this.BypassBuffering = false;
			Logger.LogDebug("LogInfo(): this.BypassBuffering → {Arg0}", this.BypassBuffering);
            ((ILogInfo)this).LogProperties = new LogProperties();
            ((ILogInfo)this).LogPortalId = -1;
			Logger.LogWarning("<<< Exiting LogInfo()");
            this.LogPortalName = string.Empty;
			Logger.LogDebug("LogInfo(): this.LogPortalName → {Arg0}", this.LogPortalName);
            ((ILogInfo)this).LogUserId = -1;
            ((ILogInfo)this).LogEventId = -1;
            this.LogUserName = string.Empty;
			Logger.LogDebug("LogInfo(): this.LogUserName → {Arg0}", this.LogUserName);
            this.Exception = new ExceptionInfo();
        }

        /// <summary>Initializes a new instance of the <see cref="LogInfo"/> class.</summary>
        /// <param name="content">XML serialized log info.</param>
        public LogInfo(string content)
            : this()
        {
            this.Deserialize(content);
        }

        /// <inheritdoc />
        string ILogInfo.LogGuid { get; set; }

        /// <inheritdoc />
        string ILogInfo.LogFileId { get; set; }

        /// <inheritdoc />
        public string LogTypeKey { get; set; }

        /// <inheritdoc />
        int ILogInfo.LogUserId { get; set; }

        /// <inheritdoc />
        int ILogInfo.LogEventId { get; set; }

        /// <inheritdoc />
        public string LogUserName { get; set; }

        /// <inheritdoc />
        int ILogInfo.LogPortalId { get; set; }

        /// <inheritdoc />
        public string LogPortalName { get; set; }

        /// <inheritdoc />
        public DateTime LogCreateDate { get; set; }

        /// <inheritdoc />
        public long LogCreateDateNum { get; set; }

        public LogProperties LogProperties
        {
            get => (LogProperties)((ILogInfo)this).LogProperties;
            set => ((ILogInfo)this).LogProperties = value;
        }

        /// <inheritdoc />
        ILogProperties ILogInfo.LogProperties { get; set; }

        /// <inheritdoc />
        public bool BypassBuffering { get; set; }

        /// <inheritdoc />
        public string LogServerName { get; set; }

        /// <inheritdoc />
        string ILogInfo.LogConfigId { get; set; }

        public ExceptionInfo Exception
        {
            get => (ExceptionInfo)((ILogInfo)this).Exception;
            set => ((ILogInfo)this).Exception = value;
        }

        /// <inheritdoc />
        IExceptionInfo ILogInfo.Exception { get; set; }

        public static bool IsSystemType(string propName)
        {
			Logger.LogError(">>> Entering IsSystemType(propName={PropName})", propName);
            switch (propName)
            {
                case "LogGUID":
                case "LogFileID":
                case "LogTypeKey":
                case "LogCreateDate":
                case "LogCreateDateNum":
                case "LogPortalID":
                case "LogPortalName":
                case "LogUserID":
                case "LogUserName":
                case "BypassBuffering":
                case "LogServerName":
			Logger.LogWarning("<<< Exiting IsSystemType(propName={PropName})", propName);
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void AddProperty(string propertyName, string propertyValue)
        {
			Logger.LogError(">>> Entering AddProperty(propertyName={PropertyName},propertyValue={PropertyValue})", propertyName, propertyValue);
            try
            {
                if (propertyValue == null)
                {
                    propertyValue = string.Empty;
					Logger.LogError("AddProperty(propertyName={PropertyName},propertyValue={PropertyValue}): propertyValue → {PropertyValue1}", propertyName, propertyValue, propertyValue);
                }

                if (propertyName.Length > 50)
                {
                    propertyName = propertyName.Substring(0, 50);
					Logger.LogError("AddProperty(propertyName={PropertyName},propertyValue={PropertyValue}): propertyName → {PropertyName1}", propertyName, propertyValue, propertyName);
                }

                if (propertyValue.Length > 500)
                {
                    propertyValue = "(TRUNCATED TO 500 CHARS): " + propertyValue.Substring(0, 500);
                }

                var objLogDetailInfo = new LogDetailInfo();
				Logger.LogError("AddProperty(propertyName={PropertyName},propertyValue={PropertyValue}): objLogDetailInfo → {ObjLogDetailInfo}", propertyName, propertyValue, objLogDetailInfo);
				Logger.LogDebug("AddProperty(propertyName={PropertyName},propertyValue={PropertyValue}): objLogDetailInfo.PropertyValue → {Arg2}", propertyName, propertyValue, objLogDetailInfo.PropertyValue);
                objLogDetailInfo.PropertyName = propertyName;
				Logger.LogDebug("AddProperty(propertyName={PropertyName},propertyValue={PropertyValue}): objLogDetailInfo.PropertyName → {Arg2}", propertyName, propertyValue, objLogDetailInfo.PropertyName);
                objLogDetailInfo.PropertyValue = propertyValue;
                this.LogProperties.Add(objLogDetailInfo);
            }
            catch (Exception exc)
            {
				Logger.LogError("Exception in AddProperty(propertyName={PropertyName},propertyValue={PropertyValue}): {Arg2}", propertyName, propertyValue, exc.Message);
                Exceptions.LogException(exc);
            }
			Logger.LogWarning("<<< Exiting AddProperty(propertyName={PropertyName},propertyValue={PropertyValue})", propertyName, propertyValue);
        }

        public void Deserialize(string content)
        {
			Logger.LogError(">>> Entering Deserialize(content={Content})", content);
            using XmlReader reader = XmlReader.Create(new StringReader(content));
			Logger.LogError("Deserialize(content={Content}): reader → {Reader}", content, reader);
            if (reader.Read())
            {
                this.ReadXml(reader);
            }

            reader.Close();
			Logger.LogWarning("<<< Exiting Deserialize(content={Content})", content);
        }

        public void ReadXml(XmlReader reader)
        {
			Logger.LogError(">>> Entering ReadXml(reader={Reader})", reader);
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    switch (reader.Name)
                    {
                        case "LogGUID":
                            ((ILogInfo)this).LogGuid = reader.ReadContentAsString();
                            break;
                        case "LogFileID":
                            ((ILogInfo)this).LogFileId = reader.ReadContentAsString();
                            break;
                        case "LogTypeKey":
                            this.LogTypeKey = reader.ReadContentAsString();
                            break;
                        case "LogUserID":
                            ((ILogInfo)this).LogUserId = reader.ReadContentAsInt();
                            break;
                        case "LogEventID":
                            ((ILogInfo)this).LogEventId = reader.ReadContentAsInt();
                            break;
                        case "LogUserName":
                            this.LogUserName = reader.ReadContentAsString();
                            break;
                        case "LogPortalID":
                            ((ILogInfo)this).LogPortalId = reader.ReadContentAsInt();
                            break;
                        case "LogPortalName":
                            this.LogPortalName = reader.ReadContentAsString();
                            break;
                        case "LogCreateDate":
                            this.LogCreateDate = DateTime.Parse(reader.ReadContentAsString(), CultureInfo.InvariantCulture);
                            break;
                        case "LogCreateDateNum":
                            this.LogCreateDateNum = reader.ReadContentAsLong();
                            break;
                        case "BypassBuffering":
                            this.BypassBuffering = bool.Parse(reader.ReadContentAsString());
                            break;
                        case "LogServerName":
                            this.LogServerName = reader.ReadContentAsString();
                            break;
                        case "LogConfigID":
                            ((ILogInfo)this).LogConfigId = reader.ReadContentAsString();
                            break;
                    }
                }
            }

            // Check for LogProperties child node
            reader.Read();
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "LogProperties")
            {
                reader.ReadStartElement("LogProperties");
                if (reader.ReadState != ReadState.EndOfFile && reader.NodeType != XmlNodeType.None && !string.IsNullOrEmpty(reader.LocalName))
                {
                    this.LogProperties.ReadXml(reader);
                }
            }

            // Check for Exception child node
            if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Exception")
            {
                this.Exception.ReadXml(reader);
            }
			Logger.LogWarning("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        public string Serialize()
        {
			Logger.LogError(">>> Entering Serialize()");
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
			Logger.LogDebug("Serialize(): settings.ConformanceLevel → {Arg0}", settings.ConformanceLevel);
			Logger.LogError("Serialize(): settings → {Settings}", settings);
            settings.OmitXmlDeclaration = true;
			Logger.LogDebug("Serialize(): settings.OmitXmlDeclaration → {Arg0}", settings.OmitXmlDeclaration);
            var sb = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
				Logger.LogError("Serialize(): sb → {Sb}", sb);
                this.WriteXml(writer);
                writer.Close();
				Logger.LogWarning("<<< Exiting Serialize()");
                return sb.ToString();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
			Logger.LogError(">>> Entering ToString()");
            var str = new StringBuilder();
            str.Append("<p><strong>LogGUID:</strong>" + ((ILogInfo)this).LogGuid + "</p>");
			Logger.LogError("ToString(): str → {Str}", str);
            str.Append("<p><strong>LogType:</strong>" + this.LogTypeKey + "</p>");
            str.Append("<p><strong>UserID:</strong>" + ((ILogInfo)this).LogUserId + "</p>");
            str.Append("<p><strong>EventID:</strong>" + ((ILogInfo)this).LogEventId + "</p>");
            str.Append("<p><strong>Username:</strong>" + this.LogUserName + "</p>");
            str.Append("<p><strong>PortalID:</strong>" + ((ILogInfo)this).LogPortalId + "</p>");
            str.Append("<p><strong>PortalName:</strong>" + this.LogPortalName + "</p>");
            str.Append("<p><strong>CreateDate:</strong>" + this.LogCreateDate + "</p>");
            str.Append("<p><strong>ServerName:</strong>" + this.LogServerName + "</p>");
            str.Append(this.LogProperties.ToString());
            if (!string.IsNullOrEmpty(this.Exception.ExceptionHash))
            {
                str.Append(this.Exception.ToString());
            }

			Logger.LogWarning("<<< Exiting ToString()");
            return str.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteXml(writer={Writer})", writer);
            writer.WriteStartElement("log");
            writer.WriteAttributeString("LogGUID", ((ILogInfo)this).LogGuid);
            writer.WriteAttributeString("LogFileID", ((ILogInfo)this).LogFileId);
            writer.WriteAttributeString("LogTypeKey", this.LogTypeKey);
            writer.WriteAttributeString("LogUserID", ((ILogInfo)this).LogUserId.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("LogEventID", ((ILogInfo)this).LogEventId.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("LogUserName", this.LogUserName);
            writer.WriteAttributeString("LogPortalID", ((ILogInfo)this).LogPortalId.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("LogPortalName", this.LogPortalName);
            writer.WriteAttributeString("LogCreateDate", this.LogCreateDate.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("LogCreateDateNum", this.LogCreateDateNum.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("BypassBuffering", this.BypassBuffering.ToString());
            writer.WriteAttributeString("LogServerName", this.LogServerName);
            writer.WriteAttributeString("LogConfigID", ((ILogInfo)this).LogConfigId);
            this.LogProperties.WriteXml(writer);
            if (!string.IsNullOrEmpty(this.Exception.ExceptionHash))
            {
                this.Exception.WriteXml(writer);
            }

            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting WriteXml(writer={Writer})", writer);
        }
    }
}
