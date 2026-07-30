// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>DesktopModuleInfo provides the Entity Layer for Desktop Modules.</summary>
    [Serializable]
    public class DesktopModuleInfo : ContentItem, IXmlSerializable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private Dictionary<string, ModuleDefinitionInfo> moduleDefinitions;
        private PageInfo pageInfo;

        /// <summary>Initializes a new instance of the <see cref="DesktopModuleInfo"/> class.</summary>
        public DesktopModuleInfo()
        {
			Logger.LogDebug(">>> Entering DesktopModuleInfo()");
            this.IsPremium = Null.NullBoolean;
            this.IsAdmin = Null.NullBoolean;
			Logger.LogDebug("DesktopModuleInfo(): this.IsPremium → {Arg0}", this.IsPremium);
			Logger.LogDebug("DesktopModuleInfo(): this.IsAdmin → {Arg0}", this.IsAdmin);
            this.CodeSubDirectory = Null.NullString;
            this.PackageID = Null.NullInteger;
            this.DesktopModuleID = Null.NullInteger;
			Logger.LogDebug("DesktopModuleInfo(): this.CodeSubDirectory → {Arg0}", this.CodeSubDirectory);
			Logger.LogDebug("DesktopModuleInfo(): this.PackageID → {Arg0}", this.PackageID);
			Logger.LogDebug("DesktopModuleInfo(): this.DesktopModuleID → {Arg0}", this.DesktopModuleID);
            this.SupportedFeatures = Null.NullInteger;
            this.Shareable = ModuleSharing.Unknown;
			Logger.LogDebug("<<< Exiting DesktopModuleInfo()");
			Logger.LogDebug("DesktopModuleInfo(): this.SupportedFeatures → {Arg0}", this.SupportedFeatures);
			Logger.LogDebug("DesktopModuleInfo(): this.Shareable → {Arg0}", this.Shareable);
        }

        /// <summary>Gets the Module Definitions for this Desktop Module.</summary>
        public Dictionary<string, ModuleDefinitionInfo> ModuleDefinitions
        {
            get
            {
                if (this.moduleDefinitions == null)
                {
                    if (this.DesktopModuleID > Null.NullInteger)
                    {
                        this.moduleDefinitions = ModuleDefinitionController.GetModuleDefinitionsByDesktopModuleID(this.DesktopModuleID);
                    }
                    else
                    {
                        this.moduleDefinitions = new Dictionary<string, ModuleDefinitionInfo>();
                    }
                }

                return this.moduleDefinitions;
            }
        }

        /// <summary>Gets or sets the ID of the Desktop Module.</summary>
        public int DesktopModuleID { get; set; }

        /// <summary>Gets or sets the ID of the Package for this Desktop Module.</summary>
        public int PackageID { get; set; }

        /// <summary>Gets or sets whether this has an associated Admin page.</summary>
        public string AdminPage { get; set; }

        /// <summary>Gets or sets the BusinessControllerClass of the Desktop Module.</summary>
        public string BusinessControllerClass { get; set; }

        public string Category
        {
            get
            {
                Term term = (from Term t in this.Terms select t).FirstOrDefault();
                return (term != null) ? term.Name : string.Empty;
            }

            set
            {
                this.Terms.Clear();
                ITermController termController = Util.GetTermController();
                var term = (from Term t in termController.GetTermsByVocabulary("Module_Categories")
                            where t.Name == value
                            select t)
                            .FirstOrDefault();
                if (term != null)
                {
                    this.Terms.Add(term);
                }
            }
        }

        /// <summary>Gets or sets the AppCode Folder Name of the Desktop Module.</summary>
        public string CodeSubDirectory { get; set; }

        /// <summary>
        /// Gets or sets a Regular Expression that matches the versions of the core
        /// that this module is compatible with.
        /// </summary>
        public string CompatibleVersions { get; set; }

        /// <summary>Gets or sets a list of Dependencies for the module.</summary>
        public string Dependencies { get; set; }

        /// <summary>Gets or sets the  Description of the Desktop Module.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the Folder Name of the Desktop Module.</summary>
        public string FolderName { get; set; }

        /// <summary>Gets or sets the Friendly Name of the Desktop Module.</summary>
        public string FriendlyName { get; set; }

        /// <summary>Gets or sets whether this has an associated hostpage.</summary>
        public string HostPage { get; set; }

        /// <summary>Gets or sets a value indicating whether the Module is an Admin Module.</summary>
        public bool IsAdmin { get; set; }

        /// <summary>Gets or sets a value indicating whether the Module is Portable.</summary>
        public bool IsPortable
        {
            get
            {
                return this.GetFeature(DesktopModuleSupportedFeature.IsPortable);
            }

            set
            {
                this.UpdateFeature(DesktopModuleSupportedFeature.IsPortable, value);
            }
        }

        /// <summary>Gets or sets a value indicating whether the Module is a Premium Module.</summary>
        public bool IsPremium { get; set; }

        /// <summary>Gets or sets a value indicating whether the Module is Searchable.</summary>
        public bool IsSearchable
        {
            get
            {
                return this.GetFeature(DesktopModuleSupportedFeature.IsSearchable);
            }

            set
            {
                this.UpdateFeature(DesktopModuleSupportedFeature.IsSearchable, value);
            }
        }

        /// <summary>Gets or sets a value indicating whether the Module is Upgradable.</summary>
        public bool IsUpgradeable
        {
            get
            {
                return this.GetFeature(DesktopModuleSupportedFeature.IsUpgradeable);
            }

            set
            {
                this.UpdateFeature(DesktopModuleSupportedFeature.IsUpgradeable, value);
            }
        }

        /// <summary>Gets or sets is the module allowed to be shared across sites?.</summary>
        public ModuleSharing Shareable
        {
            get;
            set;
        }

        /// <summary>Gets or sets the Name of the Desktop Module.</summary>
        public string ModuleName { get; set; }

        /// <summary>Gets or sets a list of Permissions for the module.</summary>
        public string Permissions { get; set; }

        /// <summary>Gets or sets the Supported Features of the Module.</summary>
        public int SupportedFeatures { get; set; }

        /// <summary>Gets or sets the Version of the Desktop Module.</summary>
        public string Version { get; set; }

        public PageInfo Page
        {
            get
            {
                if (this.pageInfo == null && this.PackageID > Null.NullInteger)
                {
                    var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this.PackageID);
                    if (package != null && !string.IsNullOrEmpty(package.Manifest))
                    {
                        var xmlDocument = new XmlDocument { XmlResolver = null };
                        using (var manifestReader = XmlReader.Create(new StringReader(package.Manifest), new XmlReaderSettings { XmlResolver = null, }))
                        {
                            xmlDocument.Load(manifestReader);
                        }

                        var pageNode = xmlDocument.SelectSingleNode("//package//components//component[@type=\"Module\"]//page");
                        if (pageNode != null)
                        {
                            this.pageInfo = CBO.DeserializeObject<PageInfo>(new StringReader(pageNode.OuterXml));
                        }
                    }
                }

                return this.pageInfo;
            }

            set
            {
                this.pageInfo = value;
            }
        }

        /// <summary>Fills a DesktopModuleInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public override void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.DesktopModuleID = Null.SetNullInteger(dr["DesktopModuleID"]);
            this.PackageID = Null.SetNullInteger(dr["PackageID"]);
            this.ModuleName = Null.SetNullString(dr["ModuleName"]);
            this.FriendlyName = Null.SetNullString(dr["FriendlyName"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.FolderName = Null.SetNullString(dr["FolderName"]);
            this.Version = Null.SetNullString(dr["Version"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.IsPremium = Null.SetNullBoolean(dr["IsPremium"]);
            this.IsAdmin = Null.SetNullBoolean(dr["IsAdmin"]);
            this.BusinessControllerClass = Null.SetNullString(dr["BusinessControllerClass"]);
            this.SupportedFeatures = Null.SetNullInteger(dr["SupportedFeatures"]);
            this.CompatibleVersions = Null.SetNullString(dr["CompatibleVersions"]);
            this.Dependencies = Null.SetNullString(dr["Dependencies"]);
            this.Permissions = Null.SetNullString(dr["Permissions"]);
            this.Shareable = (ModuleSharing)Null.SetNullInteger(dr["Shareable"]);
            this.AdminPage = Null.SetNullString(dr["AdminPage"]);
            this.HostPage = Null.SetNullString(dr["HostPage"]);

            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
        }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
			Logger.LogDebug("<<< Exiting GetSchema()");
            return null;
        }

        /// <summary>Reads a DesktopModuleInfo from an XmlReader.</summary>
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

                if (reader.NodeType == XmlNodeType.Element && reader.Name == "moduleDefinitions" && !reader.IsEmptyElement)
                {
                    this.ReadModuleDefinitions(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "supportedFeatures" && !reader.IsEmptyElement)
                {
                    this.ReadSupportedFeatures(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "shareable" && !reader.IsEmptyElement)
                {
                    this.ReadModuleSharing(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "page" && !reader.IsEmptyElement)
                {
                    this.ReadPageInfo(reader);
                }
                else
                {
                    switch (reader.Name)
                    {
                        case "desktopModule":
                            break;
                        case "moduleName":
                            this.ModuleName = reader.ReadElementContentAsString();
                            break;
                        case "friendlyName":
                            this.FriendlyName = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            this.Description = reader.ReadElementContentAsString();
                            break;
                        case "foldername":
                            this.FolderName = reader.ReadElementContentAsString();
                            break;
                        case "businessControllerClass":
                            this.BusinessControllerClass = reader.ReadElementContentAsString();
                            break;
                        case "codeSubDirectory":
                            this.CodeSubDirectory = reader.ReadElementContentAsString();
                            break;
                        case "page":
                            this.ReadPageInfo(reader);

                            if (this.Page.HasAdminPage())
                            {
                                this.AdminPage = this.Page.Name;
								Logger.LogDebug("ReadXml(reader={Reader}): this.AdminPage → {Arg1}", reader, this.AdminPage);
                            }

                            if (this.Page.HasHostPage())
                            {
                                this.HostPage = this.Page.Name;
								Logger.LogDebug("ReadXml(reader={Reader}): this.HostPage → {Arg1}", reader, this.HostPage);
                            }

                            break;
                        case "isAdmin":
                            if (bool.TryParse(reader.ReadElementContentAsString(), out var isAdmin))
                            {
                                this.IsAdmin = isAdmin;
								Logger.LogDebug("ReadXml(reader={Reader}): this.IsAdmin → {Arg1}", reader, this.IsAdmin);
                            }

                            break;
                        case "isPremium":
                            if (bool.TryParse(reader.ReadElementContentAsString(), out var isPremium))
                            {
                                this.IsPremium = isPremium;
								Logger.LogDebug("ReadXml(reader={Reader}): this.IsPremium → {Arg1}", reader, this.IsPremium);
                            }

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

        /// <summary>Writes a DesktopModuleInfo to an XmlWriter.</summary>
        /// <param name="writer">The XmlWriter to use.</param>
        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            // Write start of main elemenst
            writer.WriteStartElement("desktopModule");

            // write out properties
            writer.WriteElementString("moduleName", this.ModuleName);
            writer.WriteElementString("foldername", this.FolderName);
            writer.WriteElementString("businessControllerClass", this.BusinessControllerClass);
            if (!string.IsNullOrEmpty(this.CodeSubDirectory))
            {
                writer.WriteElementString("codeSubDirectory", this.CodeSubDirectory);
            }

            // Write out Supported Features
            writer.WriteStartElement("supportedFeatures");
            if (this.IsPortable)
            {
                writer.WriteStartElement("supportedFeature");
                writer.WriteAttributeString("type", "Portable");
                writer.WriteEndElement();
            }

            if (this.IsSearchable)
            {
                writer.WriteStartElement("supportedFeature");
                writer.WriteAttributeString("type", "Searchable");
                writer.WriteEndElement();
            }

            if (this.IsUpgradeable)
            {
                writer.WriteStartElement("supportedFeature");
                writer.WriteAttributeString("type", "Upgradeable");
                writer.WriteEndElement();
            }

            // Write end of Supported Features
            writer.WriteEndElement();

            // Write admin/host page info.
            if (this.Page != null)
            {
                this.Page.WriteXml(writer);
            }

            // Module sharing
            if (this.Shareable != ModuleSharing.Unknown)
            {
                writer.WriteStartElement("shareable");
                switch (this.Shareable)
                {
                    case ModuleSharing.Supported:
                        writer.WriteString("Supported");
                        break;
                    case ModuleSharing.Unsupported:
                        writer.WriteString("Unsupported");
                        break;
                }

                writer.WriteEndElement();
            }

            // Write start of Module Definitions
            writer.WriteStartElement("moduleDefinitions");

            // Iterate through definitions
            foreach (ModuleDefinitionInfo definition in this.ModuleDefinitions.Values)
            {
                definition.WriteXml(writer);
            }

            // Write end of Module Definitions
            writer.WriteEndElement();

            // Write end of main element
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }

        /// <summary>Clears a Feature from the Features.</summary>
        /// <param name="feature">The feature to Clear.</param>
        private void ClearFeature(DesktopModuleSupportedFeature feature)
        {
			Logger.LogDebug(">>> Entering ClearFeature(feature={Feature})", feature);
            // And with the 1's complement of Feature to Clear the Feature flag
            this.SupportedFeatures = this.SupportedFeatures & ~((int)feature);
			Logger.LogDebug("<<< Exiting ClearFeature(feature={Feature})", feature);
        }

        /// <summary>Gets a Feature from the Features.</summary>
        /// <param name="feature">The feature to Get.</param>
        private bool GetFeature(DesktopModuleSupportedFeature feature)
        {
			Logger.LogDebug(">>> Entering GetFeature(feature={Feature})", feature);
			Logger.LogDebug("<<< Exiting GetFeature(feature={Feature})", feature);
            return this.SupportedFeatures > Null.NullInteger && (this.SupportedFeatures & (int)feature) == (int)feature;
        }

        /// <summary>Sets a Feature in the Features.</summary>
        /// <param name="feature">The feature to Set.</param>
        private void SetFeature(DesktopModuleSupportedFeature feature)
        {
			Logger.LogDebug(">>> Entering SetFeature(feature={Feature})", feature);
            this.SupportedFeatures |= (int)feature;
			Logger.LogDebug("<<< Exiting SetFeature(feature={Feature})", feature);
        }

        /// <summary>Updates a Feature in the Features.</summary>
        /// <param name="feature">The feature to Set.</param>
        /// <param name="isSet">A Boolean indicating whether to set or clear the feature.</param>
        private void UpdateFeature(DesktopModuleSupportedFeature feature, bool isSet)
        {
			Logger.LogDebug(">>> Entering UpdateFeature(feature={Feature},isSet={IsSet})", feature, isSet);
            if (isSet)
            {
                this.SetFeature(feature);
            }
            else
            {
                this.ClearFeature(feature);
            }
			Logger.LogDebug("<<< Exiting UpdateFeature(feature={Feature},isSet={IsSet})", feature, isSet);
        }

        /// <summary>Reads a Supported Features from an XmlReader.</summary>
        /// <param name="reader">The XmlReader to use.</param>
        private void ReadSupportedFeatures(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadSupportedFeatures(reader={Reader})", reader);
            this.SupportedFeatures = 0;
            reader.ReadStartElement("supportedFeatures");
			Logger.LogDebug("ReadSupportedFeatures(reader={Reader}): this.SupportedFeatures → {Arg1}", reader, this.SupportedFeatures);
            do
            {
                if (reader.HasAttributes)
                {
                    reader.MoveToFirstAttribute();
                    switch (reader.ReadContentAsString())
                    {
                        case "Portable":
                            this.IsPortable = true;
							Logger.LogDebug("ReadSupportedFeatures(reader={Reader}): this.IsPortable → {Arg1}", reader, this.IsPortable);
                            break;
                        case "Searchable":
                            this.IsSearchable = true;
							Logger.LogDebug("ReadSupportedFeatures(reader={Reader}): this.IsSearchable → {Arg1}", reader, this.IsSearchable);
                            break;
                        case "Upgradeable":
                            this.IsUpgradeable = true;
							Logger.LogDebug("ReadSupportedFeatures(reader={Reader}): this.IsUpgradeable → {Arg1}", reader, this.IsUpgradeable);
                            break;
                    }
                }
            }
            while (reader.ReadToNextSibling("supportedFeature"));
			Logger.LogDebug("<<< Exiting ReadSupportedFeatures(reader={Reader})", reader);
        }

        private void ReadModuleSharing(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadModuleSharing(reader={Reader})", reader);
            var sharing = reader.ReadElementString("shareable");

			Logger.LogDebug("ReadModuleSharing(reader={Reader}): sharing → {Sharing}", reader, sharing);
            if (string.IsNullOrEmpty(sharing))
            {
                this.Shareable = ModuleSharing.Unknown;
				Logger.LogDebug("ReadModuleSharing(reader={Reader}): this.Shareable → {Arg1}", reader, this.Shareable);
            }
            else
            {
                switch (sharing.ToLowerInvariant())
                {
                    case "supported":
                        this.Shareable = ModuleSharing.Supported;
                        break;
                    case "unsupported":
                        this.Shareable = ModuleSharing.Unsupported;
                        break;
                    default:
                    case "unknown":
                        this.Shareable = ModuleSharing.Unknown;
                        break;
                }
            }
			Logger.LogDebug("<<< Exiting ReadModuleSharing(reader={Reader})", reader);
        }

        /// <summary>Reads a Module Definitions from an XmlReader.</summary>
        /// <param name="reader">The XmlReader to use.</param>
        private void ReadModuleDefinitions(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadModuleDefinitions(reader={Reader})", reader);
            reader.ReadStartElement("moduleDefinitions");
            do
            {
                reader.ReadStartElement("moduleDefinition");

                // Create new ModuleDefinition object
                var moduleDefinition = new ModuleDefinitionInfo();

				Logger.LogDebug("ReadModuleDefinitions(reader={Reader}): moduleDefinition → {ModuleDefinition}", reader, moduleDefinition);
                // Load it from the Xml
                moduleDefinition.ReadXml(reader);

                // Add to the collection
                this.ModuleDefinitions.Add(moduleDefinition.FriendlyName, moduleDefinition);
            }
            while (reader.ReadToNextSibling("moduleDefinition"));
			Logger.LogDebug("<<< Exiting ReadModuleDefinitions(reader={Reader})", reader);
        }

        private void ReadPageInfo(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering ReadPageInfo(reader={Reader})", reader);
            this.Page = new PageInfo();

            // Load it from the Xml
            this.Page.ReadXml(reader.ReadSubtree());

            if (this.Page.HasAdminPage())
            {
                this.AdminPage = this.Page.Name;
				Logger.LogDebug("ReadPageInfo(reader={Reader}): this.AdminPage → {Arg1}", reader, this.AdminPage);
            }

            if (this.Page.HasHostPage())
            {
                this.HostPage = this.Page.Name;
				Logger.LogDebug("ReadPageInfo(reader={Reader}): this.HostPage → {Arg1}", reader, this.HostPage);
            }
			Logger.LogDebug("<<< Exiting ReadPageInfo(reader={Reader})", reader);
        }

        [Serializable]
        public class PageInfo : IXmlSerializable
        {
            [XmlAttribute("type")]
            public string Type { get; set; }

            [XmlAttribute("common")]
            public bool IsCommon { get; set; }

            [XmlElement("name")]
            public string Name { get; set; }

            [XmlElement("icon")]
            public string Icon { get; set; }

            [XmlElement("largeIcon")]
            public string LargeIcon { get; set; }

            [XmlElement("description")]
            public string Description { get; set; }

            public bool HasAdminPage()
            {
				Logger.LogDebug(">>> Entering HasAdminPage()");
				Logger.LogDebug("<<< Exiting HasAdminPage()");
                return this.Type.IndexOf("admin", StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger;
            }

            public bool HasHostPage()
            {
				Logger.LogDebug(">>> Entering HasHostPage()");
				Logger.LogDebug("<<< Exiting HasHostPage()");
                return this.Type.IndexOf("host", StringComparison.InvariantCultureIgnoreCase) > Null.NullInteger;
            }

            /// <inheritdoc />
            public XmlSchema GetSchema()
            {
                return null;
            }

            /// <inheritdoc />
            public void ReadXml(XmlReader reader)
            {
                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (reader.Name == "page")
                        {
                            break;
                        }

                        reader.Read();
                        continue;
                    }

                    if (reader.NodeType == XmlNodeType.Whitespace)
                    {
                        reader.Read();
                        continue;
                    }

                    switch (reader.Name)
                    {
                        case "page":
                            this.Type = reader.GetAttribute("type");
                            var commonValue = reader.GetAttribute("common");
                            if (!string.IsNullOrEmpty(commonValue))
                            {
                                this.IsCommon = commonValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                            }

                            reader.Read();
                            break;
                        case "name":
                            this.Name = reader.ReadElementContentAsString();
                            break;
                        case "icon":
                            this.Icon = reader.ReadElementContentAsString();
                            break;
                        case "largeIcon":
                            this.LargeIcon = reader.ReadElementContentAsString();
                            break;
                        case "description":
                            this.Description = reader.ReadElementContentAsString();
                            break;
                        default:
                            reader.Read();
                            break;
                    }
                }
            }

            /// <inheritdoc />
            public void WriteXml(XmlWriter writer)
            {
                // Write start of main elements
                writer.WriteStartElement("page");
                writer.WriteAttributeString("type", this.Type);
                writer.WriteAttributeString("common", this.IsCommon.ToString().ToLowerInvariant());

                // write out properties
                writer.WriteElementString("name", this.Name);
                writer.WriteElementString("icon", this.Icon);
                writer.WriteElementString("largeIcon", this.LargeIcon);
                writer.WriteElementString("description", this.Description);

                // Write end of main element
                writer.WriteEndElement();
            }
        }
    }
}
