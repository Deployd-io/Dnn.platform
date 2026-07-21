// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Services.Localization;

    /// <summary>The EnumEditControl control provides a standard UI component for editing enumerated properties.</summary>
    [ToolboxData("<{0}:EnumEditControl runat=server></{0}:EnumEditControl>")]
    public class EnumEditControl : EditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly Type enumType;

        /// <summary>Initializes a new instance of the <see cref="EnumEditControl"/> class.</summary>
        public EnumEditControl()
        {
		Logger.LogDebug(">>> Entering EnumEditControl()");
		Logger.LogDebug("<<< Exiting EnumEditControl()");
        }

        /// <summary>Initializes a new instance of the <see cref="EnumEditControl"/> class.</summary>
        /// <param name="type">The name of the <see cref="Type"/> of the enum.</param>
        public EnumEditControl(string type)
        {
			Logger.LogDebug(">>> Entering EnumEditControl(type={Type})", type);
            this.SystemType = type;
            this.enumType = Type.GetType(type);
			Logger.LogDebug("<<< Exiting EnumEditControl(type={Type})", type);
			Logger.LogDebug("EnumEditControl(type={Type}): this.SystemType → {Arg1}", type, this.SystemType);
			Logger.LogDebug("EnumEditControl(type={Type}): enumType → {EnumType}", type, enumType);
        }

        /// <summary>Gets or sets stringValue is the value of the control expressed as a String.</summary>
        /// <value>A string representing the Value.</value>
        protected override string StringValue
        {
            get
            {
                var retValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
                return retValue.ToString(CultureInfo.InvariantCulture);
            }

            set
            {
                int setValue = int.Parse(value, CultureInfo.InvariantCulture);
                this.Value = setValue;
            }
        }

        /// <summary>OnDataChanged runs when the PostbackData has changed.  It raises the <see cref="EditControl.ValueChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDataChanged(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnDataChanged(e={E})", e);
            int intValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
            int intOldValue = Convert.ToInt32(this.OldValue, CultureInfo.InvariantCulture);
			Logger.LogDebug("OnDataChanged(e={E}): intValue → {IntValue}", e, intValue);
			Logger.LogDebug("OnDataChanged(e={E}): intOldValue → {IntOldValue}", e, intOldValue);

            var args = new PropertyEditorEventArgs(this.Name)
            { Value = Enum.ToObject(this.enumType, intValue), OldValue = Enum.ToObject(this.enumType, intOldValue) };

            this.OnValueChanged(args);
			Logger.LogDebug("<<< Exiting OnDataChanged(e={E})", e);
        }

        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEditMode(writer={Writer})", writer);
            int propValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
            var enumValues = Enum.GetValues(this.enumType);
			Logger.LogDebug("RenderEditMode(writer={Writer}): propValue → {PropValue}", writer, propValue);
			Logger.LogDebug("RenderEditMode(writer={Writer}): enumValues → {EnumValues}", writer, enumValues);

            // Render the Select Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            for (int i = 0; i <= enumValues.Length - 1; i++)
            {
                int enumValue = (int)enumValues.GetValue(i);
                string enumName = Enum.GetName(this.enumType, enumValue);
				Logger.LogDebug("RenderEditMode(writer={Writer}): enumValue → {EnumValue}", writer, enumValue);
                enumName = Localization.GetString(enumName, this.LocalResourceFile);
				Logger.LogDebug("RenderEditMode(writer={Writer}): enumName → {EnumName}", writer, enumName);

                // Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, enumValue.ToString(CultureInfo.InvariantCulture));

                if (enumValue == propValue)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(enumName);
                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <summary>RenderViewMode renders the View (readonly) mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderViewMode(writer={Writer})", writer);
            int propValue = Convert.ToInt32(this.Value, CultureInfo.InvariantCulture);
            string enumValue = Enum.Format(this.enumType, propValue, "G");
			Logger.LogDebug("RenderViewMode(writer={Writer}): propValue → {PropValue}", writer, propValue);
			Logger.LogDebug("RenderViewMode(writer={Writer}): enumValue → {EnumValue}", writer, enumValue);

            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(enumValue);
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }
    }
}
