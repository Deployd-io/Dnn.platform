// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;

    /// <summary>The VersionEditControl control provides a standard UI component for editing <see cref="Version"/> properties.</summary>
    [ToolboxData("<{0}:VersionEditControl runat=server></{0}:VersionEditControl>")]
    public class VersionEditControl : EditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(VersionEditControl));
        protected Version Version
        {
            get
            {
                return this.Value as Version;
            }
        }

        /// <summary>Gets or sets stringValue is the value of the control expressed as a String.</summary>
        /// <value>A string representing the Value.</value>
        protected override string StringValue
        {
            get
            {
                return this.Value.ToString();
            }

            set
            {
                this.Value = new Version(value);
            }
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogDebug(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            string majorVersion = postCollection[postDataKey + "_Major"];
            string minorVersion = postCollection[postDataKey + "_Minor"];
            string buildVersion = postCollection[postDataKey + "_Build"];
            bool dataChanged = false;
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);
            Version presentValue = this.Version;
            var postedValue = new Version(majorVersion + "." + minorVersion + "." + buildVersion);
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): postedValue → {PostedValue}", postDataKey, postCollection, postedValue);
            if (!postedValue.Equals(presentValue))
            {
                this.Value = postedValue;
                dataChanged = true;
            }
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.Value → {Arg2}", postDataKey, postCollection, this.Value);

			Logger.LogDebug("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return dataChanged;
        }

        protected void RenderDropDownList(HtmlTextWriter writer, string type, int val)
        {
			Logger.LogDebug(">>> Entering RenderDropDownList(writer={Writer},type={Type},val={Val})", writer, type, val);
            // Render the Select Tag
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_" + type);
            writer.AddStyleAttribute("width", "60px");
            writer.RenderBeginTag(HtmlTextWriterTag.Select);
            for (int i = 0; i <= 99; i++)
            {
                // Add the Value Attribute
                writer.AddAttribute(HtmlTextWriterAttribute.Value, i.ToString(CultureInfo.InvariantCulture));
                if (val == i)
                {
                    // Add the Selected Attribute
                    writer.AddAttribute(HtmlTextWriterAttribute.Selected, "selected");
                }

                // Render Option Tag
                writer.RenderBeginTag(HtmlTextWriterTag.Option);
                writer.Write(i.ToString("00", CultureInfo.CurrentCulture));
                writer.RenderEndTag();
            }

            // Close Select Tag
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderDropDownList(writer={Writer},type={Type},val={Val})", writer, type, val);
        }

        /// <summary>OnDataChanged runs when the PostbackData has changed.  It raises the <see cref="EditControl.ValueChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDataChanged(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnDataChanged(e={E})", e);
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.Value;
			Logger.LogDebug("OnDataChanged(e={E}): args.Value → {Arg1}", e, args.Value);
			Logger.LogDebug("OnDataChanged(e={E}): args.OldValue → {Arg1}", e, args.OldValue);
			Logger.LogDebug("OnDataChanged(e={E}): args.StringValue → {Arg1}", e, args.StringValue);
			Logger.LogDebug("OnDataChanged(e={E}): args → {Args}", e, args);
            args.OldValue = this.OldValue;
            args.StringValue = this.StringValue;

            this.OnValueChanged(args);
			Logger.LogDebug("<<< Exiting OnDataChanged(e={E})", e);
        }

        /// <summary>OnPreRender runs just before the control is rendered.  It forces a postback to the Control.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (this.Page != null && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEditMode(writer={Writer})", writer);
            // Render a containing span Tag
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);

            // Render Major
            this.RenderDropDownList(writer, "Major", this.Version.Major);

            writer.Write("&nbsp;");

            // Render Minor
            this.RenderDropDownList(writer, "Minor", this.Version.Minor);

            writer.Write("&nbsp;");

            // Render Build
            this.RenderDropDownList(writer, "Build", this.Version.Build);

            // Close Select Tag
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <summary>RenderViewMode renders the View (readonly) mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderViewMode(writer={Writer})", writer);
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            if (this.Version != null)
            {
                writer.Write(this.Version.ToString(3));
            }

            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }
    }
}
