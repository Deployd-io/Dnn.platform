// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// The DNNLocaleEditControl control provides a standard UI component for selecting
    /// a Locale.
    /// </summary>
    [ToolboxData("<{0}:DNNLocaleEditControl runat=server></{0}:DNNLocaleEditControl>")]
    public class DNNLocaleEditControl : TextEditControl, IPostBackEventHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private string displayMode = "Native";
        private LanguagesListType listType = LanguagesListType.Enabled;

        protected LanguagesListType ListType => this.listType;

        protected string DisplayMode => this.displayMode;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        protected PortalSettings PortalSettings => PortalSettings.Current;

        /// <inheritdoc />
        public void RaisePostBackEvent(string eventArgument)
        {
			Logger.LogDebug(">>> Entering RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
            this.displayMode = eventArgument;
			Logger.LogDebug("<<< Exiting RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
			Logger.LogDebug("RaisePostBackEvent(eventArgument={EventArgument}): this.displayMode → {Arg1}", eventArgument, this.displayMode);
        }

        /// <summary>OnAttributesChanged runs when the CustomAttributes property has changed.</summary>
        protected override void OnAttributesChanged()
        {
			Logger.LogDebug(">>> Entering OnAttributesChanged()");
            // Get the List settings out of the "Attributes"
            if (this.CustomAttributes != null)
            {
                foreach (Attribute attribute in this.CustomAttributes)
                {
                    var listAtt = attribute as LanguagesListTypeAttribute;
                    if (listAtt != null)
                    {
                        this.listType = listAtt.ListType;
						Logger.LogDebug("OnAttributesChanged(): listType → {ListType}", listType);
						Logger.LogDebug("OnAttributesChanged(): this.listType → {Arg0}", this.listType);
                        break;
                    }
                }
            }
			Logger.LogDebug("<<< Exiting OnAttributesChanged()");
        }

        /// <summary>RenderViewMode renders the View (readonly) mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderViewMode(writer={Writer})", writer);
            Locale locale = LocaleController.Instance.GetLocale(this.StringValue);

			Logger.LogDebug("RenderViewMode(writer={Writer}): locale → {Locale}", writer, locale);
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            if (locale != null)
            {
                writer.Write(locale.Text);
            }

            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }

        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEditMode(writer={Writer})", writer);
            // Render div
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnLeft");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            var cultures = new List<CultureInfo>();
            switch (this.ListType)
			Logger.LogDebug("RenderEditMode(writer={Writer}): cultures → {Cultures}", writer, cultures);
            {
                case LanguagesListType.All:
                    var culturesArray = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                    Array.Sort(culturesArray, new CultureInfoComparer(this.DisplayMode));
					Logger.LogDebug("RenderEditMode(writer={Writer}): culturesArray → {CulturesArray}", writer, culturesArray);
                    cultures = culturesArray.ToList();
                    break;
                case LanguagesListType.Supported:
                    cultures = LocaleController.Instance.GetLocales(Null.NullInteger).Values
                        .Select(c => CultureInfo.GetCultureInfo(c.Code))
                        .ToList();
                    break;
                case LanguagesListType.Enabled:
                    cultures = LocaleController.Instance.GetLocales(this.PortalSettings.PortalId).Values
                        .Select(c => CultureInfo.GetCultureInfo(c.Code))
                        .ToList();
                    break;
            }

            var promptValue = this.StringValue == Null.NullString && cultures.Count > 1 && !this.Required;

            // Render the Select Tag
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            if (promptValue)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onchange, "onLocaleChanged(this)");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Select);

            // Render None selected option
            // Add the Value Attribute
            writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);

            if (this.StringValue == Null.NullString)
            {
                // Add the Selected Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write(Localization.GetString("Not_Specified", Localization.SharedResourceFile));
            writer.RenderEndTag();

            foreach (var culture in cultures)
            {
                this.RenderOption(writer, culture);
            }

            // Close Select Tag
            writer.RenderEndTag();

            if (promptValue)
            {
                writer.WriteBreak();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormMessage dnnFormError");
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                writer.Write(Localization.GetString("LanguageNotSelected", Localization.SharedResourceFile));
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
                writer.RenderBeginTag(HtmlTextWriterTag.Script);
				Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
                writer.Write(@"
function onLocaleChanged(element){
    var $this = $(element);
    var $error = $this.next().next();
    var value = $this.val();
    value ? $error.hide() : $error.show();
};
");
                writer.RenderEndTag();
            }

            // Render break
            writer.Write("<br />");

            // Render Span
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "dnnFormRadioButtons");
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            // Render Button Row
            this.RenderModeButtons(writer);

            // close span
            writer.RenderEndTag();

            // close div
            writer.RenderEndTag();
        }

        private bool IsSelected(string locale)
        {
            return locale == this.StringValue;
        }

        private void RenderModeButtons(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderModeButtons(writer={Writer})", writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute("aria-label", "Mode");
            if (this.DisplayMode == "English")
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, this.Page.ClientScript.GetPostBackEventReference(this, "English"));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
            writer.Write(Localization.GetString("EnglishName", Localization.GlobalResourceFile));

            ////writer.Write("<br />");
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "radio");
            writer.AddAttribute("aria-label", "Mode");
            if (this.DisplayMode == "Native")
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, this.Page.ClientScript.GetPostBackEventReference(this, "Native"));
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();

            writer.Write(Localization.GetString("NativeName", Localization.GlobalResourceFile));
			Logger.LogDebug("<<< Exiting RenderModeButtons(writer={Writer})", writer);
        }

        private void RenderOption(HtmlTextWriter writer, CultureInfo culture)
        {
			Logger.LogDebug(">>> Entering RenderOption(writer={Writer},culture={Culture})", writer, culture);
            string localeName;

            if (this.DisplayMode == "Native")
            {
                localeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.NativeName);
				Logger.LogDebug("RenderOption(writer={Writer},culture={Culture}): localeName → {LocaleName}", writer, culture, localeName);
            }
            else
            {
                localeName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(culture.EnglishName);
            }

            // Add the Value Attribute
            writer.AddAttribute(HtmlTextWriterAttribute.Value, culture.Name);

            if (this.IsSelected(culture.Name))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
            }

            // Render Option Tag
            writer.RenderBeginTag(HtmlTextWriterTag.Option);
            writer.Write(localeName);
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderOption(writer={Writer},culture={Culture})", writer, culture);
        }
    }
}
