// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Web.UI;

    /// <summary>
    /// The TrueFalseEditControl control provides a standard UI component for editing
    /// true/false (boolean) properties.
    /// </summary>
    [ToolboxData("<{0}:CheckEditControl runat=server></{0}:CheckEditControl>")]
    public class CheckEditControl : TrueFalseEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CheckEditControl));
        /// <summary>Initializes a new instance of the <see cref="CheckEditControl"/> class.</summary>
        public CheckEditControl()
        {
			Logger.LogError(">>> Entering CheckEditControl()");
            this.SystemType = "System.Boolean";
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogError(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            string postedValue = postCollection[postDataKey];
            bool boolValue = false;
			Logger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): boolValue → {BoolValue}", postDataKey, postCollection, boolValue);
            if (!(postedValue == null || postedValue == string.Empty))
            {
                boolValue = true;
            }

            if (!this.BooleanValue.Equals(boolValue))
            {
                this.Value = boolValue;
				Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.Value → {Arg2}", postDataKey, postCollection, this.Value);
                return true;
            }

			Logger.LogWarning("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return false;
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            if (this.Page != null && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderEditMode(writer={Writer})", writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            if (this.BooleanValue)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
                writer.AddAttribute(HtmlTextWriterAttribute.Value, "1");
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Value, string.Empty);
            }

            writer.AddAttribute("onclick", "this.value = this.checked ? '1' : '';");
            writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <inheritdoc />
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderViewMode(writer={Writer})", writer);
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");
            if (this.BooleanValue)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Checked, "checked");
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
            writer.RenderBeginTag(HtmlTextWriterTag.Input);
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }
    }
}
