// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Abstractions.Logging;

    /// <inheritdoc />
    [Serializable]
    public class ExceptionInfo : IExceptionInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExceptionInfo));
        /// <summary>Initializes a new instance of the <see cref="ExceptionInfo"/> class.</summary>
        public ExceptionInfo()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ExceptionInfo"/> class.</summary>
        /// <param name="e">The exception.</param>
        public ExceptionInfo(Exception e)
        {
			Logger.LogError(">>> Entering ExceptionInfo(e={E})", e);
            this.Message = e.Message;
			Logger.LogDebug("ExceptionInfo(e={E}): this.Message → {Arg1}", e, this.Message);
            this.StackTrace = e.StackTrace;
            this.Source = e.Source;
            if (e.InnerException != null)
            {
				Logger.LogDebug("ExceptionInfo(e={E}): this.StackTrace → {Arg1}", e, this.StackTrace);
				Logger.LogDebug("ExceptionInfo(e={E}): this.Source → {Arg1}", e, this.Source);
                this.InnerMessage = e.InnerException.Message;
                this.InnerStackTrace = e.InnerException.StackTrace;
            }
			Logger.LogDebug("ExceptionInfo(e={E}): this.InnerMessage → {Arg1}", e, this.InnerMessage);

            this.ExceptionHash = e.Hash();
        }

        /// <inheritdoc />
        public string AssemblyVersion { get; set; }

        /// <inheritdoc />
        public int PortalId { get; set; }

        /// <inheritdoc />
        public int UserId { get; set; }

        /// <inheritdoc />
        public int TabId { get; set; }

        /// <inheritdoc />
        public string RawUrl { get; set; }

        /// <inheritdoc />
        public string Referrer { get; set; }

        /// <inheritdoc />
        public string UserAgent { get; set; }

        /// <inheritdoc />
        public string ExceptionHash { get; set; }

        /// <inheritdoc />
        public string Message { get; set; }

        /// <inheritdoc />
        public string StackTrace { get; set; }

        /// <inheritdoc />
        public string InnerMessage { get; set; }

        /// <inheritdoc />
        public string InnerStackTrace { get; set; }

        /// <inheritdoc />
        public string Source { get; set; }

        /// <inheritdoc />
        public string FileName { get; set; }

        /// <inheritdoc />
        public int FileLineNumber { get; set; }

        /// <inheritdoc />
        public int FileColumnNumber { get; set; }

        /// <inheritdoc />
        public string Method { get; set; }

        public void Deserialize(string content)
        {
			Logger.LogError(">>> Entering Deserialize(content={Content})", content);
            using (XmlReader reader = XmlReader.Create(new StringReader(content)))
            {
                if (reader.Read())
                {
                    this.ReadXml(reader);
                }

                reader.Close();
            }
			Logger.LogWarning("<<< Exiting Deserialize(content={Content})", content);
        }

        public void ReadXml(XmlReader reader)
        {
			Logger.LogError(">>> Entering ReadXml(reader={Reader})", reader);
            do
            {
                switch (reader.Name)
                {
                    case "AssemblyVersion":
                        this.AssemblyVersion = reader.ReadContentAsString();
                        break;
                    case "PortalId":
                        this.PortalId = reader.ReadContentAsInt();
                        break;
                    case "UserId":
                        this.UserId = reader.ReadContentAsInt();
                        break;
                    case "TabId":
                        this.TabId = reader.ReadContentAsInt();
                        break;
                    case "RawUrl":
                        this.RawUrl = reader.ReadContentAsString();
                        break;
                    case "Referrer":
                        this.Referrer = reader.ReadContentAsString();
                        break;
                    case "UserAgent":
                        this.UserAgent = reader.ReadContentAsString();
                        break;
                    case "ExceptionHash":
                        this.ExceptionHash = reader.ReadContentAsString();
                        break;
                    case "Message":
                        this.Message = reader.ReadContentAsString();
                        break;
                    case "StackTrace":
                        this.StackTrace = reader.ReadContentAsString();
                        break;
                    case "InnerMessage":
                        this.InnerMessage = reader.ReadContentAsString();
                        break;
                    case "InnerStackTrace":
                        this.InnerStackTrace = reader.ReadContentAsString();
                        break;
                    case "Source":
                        this.Source = reader.ReadContentAsString();
                        break;
                    case "FileName":
                        this.FileName = reader.ReadContentAsString();
                        break;
                    case "FileLineNumber":
                        this.FileLineNumber = reader.ReadContentAsInt();
                        break;
                    case "FileColumnNumber":
                        this.FileColumnNumber = reader.ReadContentAsInt();
                        break;
                    case "Method":
                        this.Method = reader.ReadContentAsString();
                        break;
                }

                reader.ReadEndElement();
                reader.Read();
            }
            while (reader.ReadState != ReadState.EndOfFile && reader.NodeType != XmlNodeType.None && !string.IsNullOrEmpty(reader.LocalName));
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
            str.Append("<p><strong>AssemblyVersion:</strong>" + WebUtility.HtmlEncode(this.AssemblyVersion) + "</p>");
			Logger.LogError("ToString(): str → {Str}", str);
            str.Append("<p><strong>PortalId:</strong>" + this.PortalId + "</p>");
            str.Append("<p><strong>UserId:</strong>" + this.UserId + "</p>");
            str.Append("<p><strong>TabId:</strong>" + this.TabId + "</p>");
            str.Append("<p><strong>RawUrl:</strong>" + WebUtility.HtmlEncode(this.RawUrl) + "</p>");
            str.Append("<p><strong>Referrer:</strong>" + WebUtility.HtmlEncode(this.Referrer) + "</p>");
            str.Append("<p><strong>UserAgent:</strong>" + WebUtility.HtmlEncode(this.UserAgent) + "</p>");
            str.Append("<p><strong>ExceptionHash:</strong>" + WebUtility.HtmlEncode(this.ExceptionHash) + "</p>");
            str.Append("<p><strong>Message:</strong>" + WebUtility.HtmlEncode(this.Message) + "</p>");
            str.Append("<p><strong>StackTrace:</strong><pre>" + WebUtility.HtmlEncode(this.StackTrace)?.Replace(") at ", ")<br/>at ") + "</pre></p>");
            str.Append("<p><strong>InnerMessage:</strong>" + WebUtility.HtmlEncode(this.InnerMessage) + "</p>");
            str.Append("<p><strong>InnerStackTrace:</strong><pre>" + WebUtility.HtmlEncode(this.InnerStackTrace)?.Replace(") at ", ")<br/>at ") + "</pre></p>");
            str.Append("<p><strong>Source:</strong>" + WebUtility.HtmlEncode(this.Source) + "</p>");
            str.Append("<p><strong>FileName:</strong>" + WebUtility.HtmlEncode(this.FileName) + "</p>");
            str.Append("<p><strong>FileLineNumber:</strong>" + this.FileLineNumber + "</p>");
            str.Append("<p><strong>FileColumnNumber:</strong>" + this.FileColumnNumber + "</p>");
            str.Append("<p><strong>Method:</strong>" + WebUtility.HtmlEncode(this.Method) + "</p>");
			Logger.LogWarning("<<< Exiting ToString()");
            return str.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteXml(writer={Writer})", writer);
            writer.WriteStartElement("Exception");
            writer.WriteElementString("AssemblyVersion", this.AssemblyVersion);
            writer.WriteElementString("PortalId", this.PortalId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("UserId", this.UserId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("TabId", this.TabId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("RawUrl", this.RawUrl);
            writer.WriteElementString("Referrer", this.Referrer);
            writer.WriteElementString("UserAgent", this.UserAgent);
            writer.WriteElementString("ExceptionHash", this.ExceptionHash);
            writer.WriteElementString("Message", this.Message);
            writer.WriteElementString("StackTrace", this.StackTrace);
            writer.WriteElementString("InnerMessage", this.InnerMessage);
            writer.WriteElementString("InnerStackTrace", this.InnerStackTrace);
            writer.WriteElementString("Source", this.Source);
            writer.WriteElementString("FileName", this.FileName);
            writer.WriteElementString("FileLineNumber", this.FileLineNumber.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("FileColumnNumber", this.FileColumnNumber.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Method", this.Method);
            writer.WriteEndElement();
			Logger.LogWarning("<<< Exiting WriteXml(writer={Writer})", writer);
        }
    }
}
