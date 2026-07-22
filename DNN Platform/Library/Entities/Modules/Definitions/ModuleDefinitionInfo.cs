// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules.Definitions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security.Permissions;

    /// <summary>ModuleDefinitionInfo provides the Entity Layer for Module Definitions.</summary>
    [Serializable]
    public class ModuleDefinitionInfo : IXmlSerializable, IHydratable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Dictionary<string, ModuleControlInfo> moduleControls;
        private string definitionName;

        /// <summary>Initializes a new instance of the <see cref="ModuleDefinitionInfo"/> class.</summary>
        public ModuleDefinitionInfo()
        {
			Logger.LogDebug(">>> Entering ModuleDefinitionInfo()");
            this.Permissions = new Dictionary<string, PermissionInfo>();
            this.DesktopModuleID = Null.NullInteger;
			Logger.LogDebug("ModuleDefinitionInfo(): this.DesktopModuleID → {Arg0}", this.DesktopModuleID);
            this.ModuleDefID = Null.NullInteger;
			Logger.LogDebug("<<< Exiting ModuleDefinitionInfo()");
			Logger.LogDebug("ModuleDefinitionInfo(): this.ModuleDefID → {Arg0}", this.ModuleDefID);
        }

        /// <summary>Gets the Dictionary of ModuleControls that are part of this definition.</summary>
        public Dictionary<string, ModuleControlInfo> ModuleControls
        {
            get
            {
                if (this.moduleControls == null)
                {
                    this.LoadControls();
                }

                return this.moduleControls;
            }
        }

        /// <summary>Gets or sets the Module Definition ID.</summary>
        public int ModuleDefID { get; set; }

        /// <summary>Gets or sets the Default Cache Time.</summary>
        public int DefaultCacheTime { get; set; }

        /// <summary>Gets or sets the associated Desktop Module ID.</summary>
        public int DesktopModuleID { get; set; }

        /// <summary>Gets or sets the Friendly Name.</summary>
        public string FriendlyName { get; set; }

        /// <summary>Gets or sets the DefinitionName.</summary>
        public string DefinitionName
        {
            get
            {
                if (string.IsNullOrEmpty(this.definitionName))
                {
                    return this.FriendlyName;
                }

                return this.definitionName;
            }

            set
            {
                this.definitionName = value;
            }
        }

        /// <summary>Gets the Dictionary of Permissions that are part of this definition.</summary>
        public Dictionary<string, PermissionInfo> Permissions { get; private set; }

        /// <inheritdoc />
        public int KeyID
        {
            get
            {
                return this.ModuleDefID;
            }

            set
            {
                this.ModuleDefID = value;
            }
        }

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            this.DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
            this.DefaultCacheTime = Null.SetNullInteger(dr["DefaultCacheTime"]);
            this.FriendlyName = Null.SetNullString(dr["FriendlyName"]);
            if (dr.GetSchemaTable().Select("ColumnName = 'DefinitionName'").Length > 0)
            {
                this.DefinitionName = Null.SetNullString(dr["DefinitionName"]);
            }
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
			Logger.LogDebug(">>> Entering GetSchema()");
			Logger.LogDebug("<<< Exiting GetSchema()");
            return null;
        }

        /// <summary>Reads a ModuleDefinitionInfo from an XmlReader.</summary>
        /// <param name="reader">The XmlReader to use.</param>
        public void ReadXml(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadXml(reader={Reader})", reader);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    break;
                }

                if (reader.NodeType == XmlNodeType.Whitespace)
                {
                    continue;
                }

                if (reader is { NodeType: XmlNodeType.Element, Name: "moduleControls", })
                {
                    this.ReadModuleControls(reader);
                }
                else
                {
                    switch (reader.Name)
                    {
                        case "moduleDefinition":
                            break;
                        case "friendlyName":
                            this.FriendlyName = reader.ReadElementContentAsString();
                            break;
                        case "defaultCacheTime":
                            string elementValue = reader.ReadElementContentAsString();
                            if (!string.IsNullOrEmpty(elementValue))
                            {
                                this.DefaultCacheTime = int.Parse(elementValue, CultureInfo.InvariantCulture);
                            }

                            break;
                        case "permissions": // Ignore permissions node
                            reader.Skip();
                            break;
                        case "definitionName":
                            this.DefinitionName = reader.ReadElementContentAsString();
                            break;
                        default:
                            if (reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
                            {
                                reader.ReadElementContentAsString();
                            }

                            break;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        /// <summary>Writes a ModuleDefinitionInfo to an XmlWriter.</summary>
        /// <param name="writer">The XmlWriter to use.</param>
        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            // Write start of main elements
            writer.WriteStartElement("moduleDefinition");

            // write out properties
            writer.WriteElementString("friendlyName", this.FriendlyName);
            writer.WriteElementString("definitionName", this.DefinitionName);
            writer.WriteElementString("defaultCacheTime", this.DefaultCacheTime.ToString(CultureInfo.InvariantCulture));

            // Write start of Module Controls
            writer.WriteStartElement("moduleControls");

            // Iterate through controls
            foreach (ModuleControlInfo control in this.ModuleControls.Values)
            {
                control.WriteXml(writer);
            }

            // Write end of Module Controls
            writer.WriteEndElement();

            // Write end of main element
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }

        public void LoadControls()
        {
			Logger.LogDebug(">>> Entering LoadControls()");
            this.moduleControls = this.ModuleDefID > Null.NullInteger ? ModuleControlController.GetModuleControlsByModuleDefinitionID(this.ModuleDefID) : new Dictionary<string, ModuleControlInfo>();
			Logger.LogDebug("<<< Exiting LoadControls()");
			Logger.LogDebug("LoadControls(): moduleControls → {ModuleControls}", moduleControls);
        }

        /// <summary>Reads the ModuleControls from an XmlReader.</summary>
        /// <param name="reader">The XmlReader to use.</param>
        private void ReadModuleControls(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadModuleControls(reader={Reader})", reader);
            reader.ReadStartElement("moduleControls");
            do
            {
                reader.ReadStartElement("moduleControl");
                var moduleControl = new ModuleControlInfo();
                moduleControl.ReadXml(reader);
				Logger.LogDebug("ReadModuleControls(reader={Reader}): moduleControl → {ModuleControl}", reader, moduleControl);
                this.ModuleControls.Add(moduleControl.ControlKey, moduleControl);
            }
            while (reader.ReadToNextSibling("moduleControl"));
			Logger.LogDebug("<<< Exiting ReadModuleControls(reader={Reader})", reader);
        }
    }
}
