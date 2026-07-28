// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System.Globalization;
    using System.Xml;

    public class InstalledModuleInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public int DesktopModuleId { get; set; }

        public int Instances { get; set; }

        public string FriendlyName { get; set; }

        public string ModuleName { get; set; }

        public string Version { get; set; }

        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            // Write start of main elements
            writer.WriteStartElement("module");

            writer.WriteElementString("moduleName", this.ModuleName);
            writer.WriteElementString("friendlyName", this.FriendlyName);
            writer.WriteElementString("version", this.Version);
            writer.WriteElementString("instances", this.Instances.ToString(CultureInfo.InvariantCulture));

            // Write end of Host Info
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }
    }
}
