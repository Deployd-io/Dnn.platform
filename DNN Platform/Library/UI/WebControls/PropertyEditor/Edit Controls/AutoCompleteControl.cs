// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// The AutoCompleteControl is the same as a TextEditControl but it looks up similar values
    /// in the profile of other users in the same portal and offers those in a dropdown under the text
    /// box where the user is entering the value. So if you'd use this as the input control
    /// for "City" it will find all entered cities and look up values as the user types the city in
    /// the textbox. Selection is not enforced and if a user enters a new city it is added to the list.
    /// </summary>
    [ToolboxData("<{0}:TextEditControl runat=server></{0}:TextEditControl>")]
    internal class AutoCompleteControl : TextEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AutoCompleteControl));
        /// <summary>Initializes a new instance of the <see cref="AutoCompleteControl"/> class.</summary>
        public AutoCompleteControl()
        {
			Logger.LogDebug(">>> Entering AutoCompleteControl()");
            this.Init += this.AutoCompleteControl_Init;
            this.Load += this.AutoCompleteControl_Load;
			Logger.LogDebug("<<< Exiting AutoCompleteControl()");
        }

        /// <inheritdoc />
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEditMode(writer={Writer})", writer);
            int length = Null.NullInteger;
            if (this.CustomAttributes != null)
			Logger.LogDebug("RenderEditMode(writer={Writer}): length → {Length}", writer, length);
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    if (attribute is MaxLengthAttribute)
                    {
                        var lengthAtt = (MaxLengthAttribute)attribute;
						Logger.LogDebug("RenderEditMode(writer={Writer}): lengthAtt → {LengthAtt}", writer, lengthAtt);
                        length = lengthAtt.Length;
                        break;
                    }
                }
            }

            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Value, this.StringValue);
            if (length > Null.NullInteger)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString(CultureInfo.InvariantCulture));
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.AddAttribute("data-name", this.Name);
            writer.AddAttribute("data-pid", Entities.Portals.PortalSettings.Current.PortalId.ToString(CultureInfo.InvariantCulture));
            writer.AddAttribute("data-editor", "AutoCompleteControl");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        private void AutoCompleteControl_Init(object sender, System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering AutoCompleteControl_Init(sender={Sender},e={E})", sender, e);
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/components/ProfileAutoComplete/dnn.ProfileAutoComplete.js");
            ClientResourceManager.RegisterFeatureStylesheet(this.Page, "~/Resources/Shared/components/ProfileAutoComplete/dnn.AutoComplete.css");
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
			Logger.LogDebug("<<< Exiting AutoCompleteControl_Init(sender={Sender},e={E})", sender, e);
        }

        private void AutoCompleteControl_Load(object sender, System.EventArgs e)
        {
		Logger.LogDebug(">>> Entering AutoCompleteControl_Load(sender={Sender},e={E})", sender, e);
		Logger.LogDebug("<<< Exiting AutoCompleteControl_Load(sender={Sender},e={E})", sender, e);
        }
    }
}
