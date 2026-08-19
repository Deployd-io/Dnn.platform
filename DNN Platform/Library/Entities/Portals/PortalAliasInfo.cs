// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Data;
    using System.Globalization;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Urls;

    using NewBrowserType = DotNetNuke.Abstractions.Urls.BrowserTypes;

    /// <inheritdoc />
    [Serializable]
    public class PortalAliasInfo : BaseEntityInfo, IHydratable, IXmlSerializable, IPortalAliasInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalAliasInfo));
        /// <summary>Initializes a new instance of the <see cref="PortalAliasInfo"/> class.</summary>
        public PortalAliasInfo()
        {
		Logger.LogDebug(">>> Entering PortalAliasInfo()");
		Logger.LogDebug("<<< Exiting PortalAliasInfo()");
        }

        /// <summary>Initializes a new instance of the <see cref="PortalAliasInfo"/> class.</summary>
        /// <param name="alias">The alias to clone.</param>
        public PortalAliasInfo(PortalAliasInfo alias)
            : this((IPortalAliasInfo)alias)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalAliasInfo"/> class.</summary>
        /// <param name="alias">The alias to clone.</param>
        public PortalAliasInfo(IPortalAliasInfo alias)
        {
			Logger.LogDebug(">>> Entering PortalAliasInfo(alias={Alias})", alias);
            this.ThisAsInterface.HttpAlias = alias.HttpAlias;
            this.ThisAsInterface.PortalAliasId = alias.PortalAliasId;
            this.ThisAsInterface.PortalId = alias.PortalId;
            this.IsPrimary = alias.IsPrimary;
			Logger.LogDebug("PortalAliasInfo(alias={Alias}): this.IsPrimary → {Arg1}", alias, this.IsPrimary);
            this.Redirect = alias.Redirect;
            this.ThisAsInterface.BrowserType = alias.BrowserType;
			Logger.LogDebug("PortalAliasInfo(alias={Alias}): this.Redirect → {Arg1}", alias, this.Redirect);
            this.CultureCode = alias.CultureCode;
            this.Skin = alias.Skin;
			Logger.LogDebug("<<< Exiting PortalAliasInfo(alias={Alias})", alias);
        }

        /// <inheritdoc />
        string IPortalAliasInfo.HttpAlias { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.HttpAlias instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public string HTTPAlias
        {
            get => this.ThisAsInterface.HttpAlias;
            set => this.ThisAsInterface.HttpAlias = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc />
        int IPortalAliasInfo.PortalAliasId { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.PortalAliasId instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int PortalAliasID
        {
            get => this.ThisAsInterface.PortalAliasId;
            set => this.ThisAsInterface.PortalAliasId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc />
        int IPortalAliasInfo.PortalId { get; set; }

        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.PortalId instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int PortalID
        {
            get => this.ThisAsInterface.PortalId;
            set => this.ThisAsInterface.PortalId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        /// <inheritdoc />
        public bool IsPrimary { get; set; }

        /// <inheritdoc />
        public bool Redirect { get; set; }

        /// <summary>Gets or sets the Browser Type.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.7.2. Use DotNetNuke.Abstractions.Portals.IPortalAliasInfo.BrowserType instead. Scheduled removal in v11.0.0.")]
        public BrowserTypes BrowserType
        {
            get => this.ThisAsInterface.BrowserType.ToDeprecatedBrowserTypes();
            set => this.ThisAsInterface.BrowserType = value.ToAbstractionsBrowserTypes();
        }

        /// <inheritdoc />
        NewBrowserType IPortalAliasInfo.BrowserType { get; set; }

        /// <inheritdoc />
        public string CultureCode { get; set; }

        /// <inheritdoc />
        public string Skin { get; set; }

        /// <inheritdoc />
        public int KeyID
        {
            get { return this.ThisAsInterface.PortalAliasId; }
            set { this.ThisAsInterface.PortalAliasId = value; }
        }

        private IPortalAliasInfo ThisAsInterface => this;

        /// <inheritdoc />
        public void Fill(IDataReader dr)
        {
			Logger.LogDebug(">>> Entering Fill(dr={Dr})", dr);
            this.FillInternal(dr);

            this.ThisAsInterface.PortalAliasId = Null.SetNullInteger(dr["PortalAliasID"]);
            this.ThisAsInterface.PortalId = Null.SetNullInteger(dr["PortalID"]);
            this.ThisAsInterface.HttpAlias = Null.SetNullString(dr["HTTPAlias"]);
            this.IsPrimary = Null.SetNullBoolean(dr["IsPrimary"]);
            var browserType = Null.SetNullString(dr["BrowserType"]);
			Logger.LogDebug("Fill(dr={Dr}): browserType → {BrowserType}", dr, browserType);
            this.BrowserType = string.IsNullOrEmpty(browserType) || browserType.Equals("normal", StringComparison.OrdinalIgnoreCase)
                              ? BrowserTypes.Normal
                              : BrowserTypes.Mobile;
            this.CultureCode = Null.SetNullString(dr["CultureCode"]);
            this.Skin = Null.SetNullString(dr["Skin"]);
			Logger.LogDebug("<<< Exiting Fill(dr={Dr})", dr);
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

                switch (reader.Name)
                {
                    case "portalAlias":
                        break;
                    case "portalID":
                        this.ThisAsInterface.PortalId = reader.ReadElementContentAsInt();
                        break;
                    case "portalAliasID":
                        this.ThisAsInterface.PortalAliasId = reader.ReadElementContentAsInt();
                        break;
                    case "HTTPAlias":
                        this.ThisAsInterface.HttpAlias = reader.ReadElementContentAsString();
                        break;
                    case "skin":
                        this.Skin = reader.ReadElementContentAsString();
                        break;
                    case "cultureCode":
                        this.CultureCode = reader.ReadElementContentAsString();
                        break;
                    case "browserType":
                        string type = reader.ReadElementContentAsString();
                        this.BrowserType = type.Equals("mobile", StringComparison.OrdinalIgnoreCase) ? BrowserTypes.Mobile : BrowserTypes.Normal;
                        break;
                    case "primary":
                        this.IsPrimary = reader.ReadElementContentAsBoolean();
                        break;
                    default:
                        if (reader.NodeType == XmlNodeType.Element && !string.IsNullOrEmpty(reader.Name))
                        {
                            reader.ReadElementContentAsString();
                        }

                        break;
                }
            }
			Logger.LogDebug("<<< Exiting ReadXml(reader={Reader})", reader);
        }

        /// <inheritdoc />
        public void WriteXml(XmlWriter writer)
        {
			Logger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            // Write start of main elements
            writer.WriteStartElement("portalAlias");

            // write out properties
            writer.WriteElementString("portalID", this.ThisAsInterface.PortalId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("portalAliasID", this.ThisAsInterface.PortalAliasId.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("HTTPAlias", this.ThisAsInterface.HttpAlias);
            writer.WriteElementString("skin", this.Skin);
            writer.WriteElementString("cultureCode", this.CultureCode);
            writer.WriteElementString("browserType", this.BrowserType.ToString().ToLowerInvariant());
            writer.WriteElementString("primary", this.IsPrimary.ToString().ToLowerInvariant());

            // Write end of main element
            writer.WriteEndElement();
			Logger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }
    }
}
