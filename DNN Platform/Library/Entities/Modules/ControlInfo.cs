// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Data;
    using System.Xml;

    using DotNetNuke.Common.Utilities;

    /// <summary>ControlInfo provides a base class for Module Controls and SkinControls.</summary>
    [Serializable]
    public abstract class ControlInfo : BaseEntityInfo
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ControlInfo));
        /// <summary>Initializes a new instance of the <see cref="ControlInfo"/> class.</summary>
        protected ControlInfo()
        {
			Logger.LogError(">>> Entering ControlInfo()");
            this.SupportsPartialRendering = Null.NullBoolean;
        }

        /// <summary>Gets or sets the Control Key.</summary>
        /// <returns>A String.</returns>
        public string ControlKey { get; set; }

        /// <summary>Gets or sets the Control Source.</summary>
        /// <returns>A String.</returns>
        public string ControlSrc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the control support the AJAX
        /// Update Panel.
        /// </summary>
        /// <returns>A Boolean.</returns>
        public bool SupportsPartialRendering { get; set; }

        /// <summary>Fills a ControlInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        protected override void FillInternal(IDataReader dr)
        {
			Logger.LogError(">>> Entering FillInternal(dr={Dr})", dr);
            // Call EntityBaseInfo's implementation
            base.FillInternal(dr);
			Logger.LogWarning("<<< Exiting FillInternal(dr={Dr})", dr);
            this.ControlKey = Null.SetNullString(dr["ControlKey"]);
            this.ControlSrc = Null.SetNullString(dr["ControlSrc"]);
            this.SupportsPartialRendering = Null.SetNullBoolean(dr["SupportsPartialRendering"]);
        }

        protected void ReadXmlInternal(XmlReader reader)
        {
			Logger.LogError(">>> Entering ReadXmlInternal(reader={Reader})", reader);
            switch (reader.Name)
            {
                case "controlKey":
                    this.ControlKey = reader.ReadElementContentAsString();
                    break;
                case "controlSrc":
                    this.ControlSrc = reader.ReadElementContentAsString();
                    break;
                case "supportsPartialRendering":
                    string elementvalue = reader.ReadElementContentAsString();
                    if (!string.IsNullOrEmpty(elementvalue))
                    {
                        this.SupportsPartialRendering = bool.Parse(elementvalue);
                    }

                    break;
            }
			Logger.LogWarning("<<< Exiting ReadXmlInternal(reader={Reader})", reader);
        }

        protected void WriteXmlInternal(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteXmlInternal(writer={Writer})", writer);
            // write out properties
            writer.WriteElementString("controlKey", this.ControlKey);
            writer.WriteElementString("controlSrc", this.ControlSrc);
            writer.WriteElementString("supportsPartialRendering", this.SupportsPartialRendering.ToString());
			Logger.LogWarning("<<< Exiting WriteXmlInternal(writer={Writer})", writer);
        }
    }
}
