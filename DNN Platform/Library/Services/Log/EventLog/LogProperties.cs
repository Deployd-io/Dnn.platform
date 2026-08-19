// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;

    /// <inheritdoc cref="ILogProperties" />
    public class LogProperties : ArrayList, ILogProperties, IList<ILogDetailInfo>
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LogProperties));
        /// <inheritdoc />
        public string Summary
        {
            get
            {
                string summary = HtmlUtils.Clean(this.ToString(), true);
                if (summary.Length > 75)
                {
                    summary = summary.Substring(0, 75);
                }

                return summary;
            }
        }

        /// <inheritdoc />
        ILogDetailInfo IList<ILogDetailInfo>.this[int index]
        {
            get => (ILogDetailInfo)this[index];
            set => this[index] = value;
        }

        /// <inheritdoc />
        public void Add(ILogDetailInfo item) =>
            base.Add(item);

        /// <inheritdoc />
        public bool Contains(ILogDetailInfo item) =>
            base.Contains(item);

        /// <inheritdoc />
        public void CopyTo(ILogDetailInfo[] array, int arrayIndex) =>
            base.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public int IndexOf(ILogDetailInfo item)
            => base.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, ILogDetailInfo item)
            => base.Insert(index, item);

        /// <inheritdoc />
        IEnumerator<ILogDetailInfo> IEnumerable<ILogDetailInfo>.GetEnumerator() =>
            this.ToArray().Cast<ILogDetailInfo>().GetEnumerator();

        /// <inheritdoc />
        public bool Remove(ILogDetailInfo item)
        {
			Logger.LogDebug(">>> Entering Remove(item={Item})", item);
            var index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
			Logger.LogDebug("<<< Exiting Remove(item={Item})", item);
                return true;
            }

            return false;
        }

        public void Deserialize(string content)
        {
			Logger.LogDebug(">>> Entering Deserialize(content={Content})", content);
            using var reader = XmlReader.Create(new StringReader(content));
            reader.ReadStartElement("LogProperties");
			Logger.LogDebug("Deserialize(content={Content}): reader → {Reader}", content, reader);
            if (reader.ReadState != ReadState.EndOfFile && reader.NodeType != XmlNodeType.None && !string.IsNullOrEmpty(reader.LocalName))
            {
                this.ReadXml(reader);
            }

            reader.Close();
			Logger.LogDebug("<<< Exiting Deserialize(content={Content})", content);
        }

        public void ReadXml(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadXml(reader={Reader})", reader);
            do
            {
                reader.ReadStartElement("LogProperty");

                // Create new LogDetailInfo object
                var logDetail = new LogDetailInfo();
				Logger.LogDebug("ReadXml(reader={Reader}): logDetail → {LogDetail}", reader, logDetail);

                // Load it from the Xml
                logDetail.ReadXml(reader);

                // Add to the collection
                this.Add(logDetail);
            }
            while (reader.ReadToNextSibling("LogProperty"));
			Logger.LogDebug("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        public string Serialize()
        {
			Logger.LogDebug(">>> Entering Serialize()");
            var settings = new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment, OmitXmlDeclaration = true, };
            var sb = new StringBuilder();
			Logger.LogDebug("Serialize(): sb → {Sb}", sb);

            using var writer = XmlWriter.Create(sb, settings);
			Logger.LogDebug("Serialize(): writer → {Writer}", writer);
            this.WriteXml(writer);
            writer.Close();
			Logger.LogDebug("<<< Exiting Serialize()");
            return sb.ToString();
        }

        /// <inheritdoc />
        public override string ToString()
        {
			Logger.LogDebug(">>> Entering ToString()");
            var sb = new StringBuilder();
            foreach (LogDetailInfo logDetail in this)
            {
			Logger.LogDebug("ToString(): sb → {Sb}", sb);
                sb.Append(logDetail.ToString());
            }

			Logger.LogDebug("<<< Exiting ToString()");
            return sb.ToString();
        }

        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            writer.WriteStartElement("LogProperties");
            foreach (LogDetailInfo logDetail in this)
            {
                logDetail.WriteXml(writer);
            }

            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }
    }
}
