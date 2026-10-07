// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Modules.HTMLEditorProvider;

    /// <summary>
    /// The DNNRichTextEditControl control provides a standard UI component for editing
    /// RichText.
    /// </summary>
    [ToolboxData("<{0}:DNNRichTextEditControl runat=server></{0}:DNNRichTextEditControl>")]
    public class DNNRichTextEditControl : TextEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNRichTextEditControl));
        private HtmlEditorProvider richTextEditor;
        private TextBox defaultTextEditor;

        protected Control TextEditControl
        {
            get
            {
                if (this.richTextEditor != null)
                {
                    return this.richTextEditor.HtmlEditorControl;
                }

                return this.defaultTextEditor;
            }
        }

        protected string EditorText
        {
            get
            {
                if (this.richTextEditor != null)
                {
                    return this.richTextEditor.Text;
                }

                return this.defaultTextEditor.Text;
            }

            set
            {
                if (this.richTextEditor != null)
                {
                    this.richTextEditor.Text = value;
                }
                else
                {
                    this.defaultTextEditor.Text = value;
                }
            }
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogError(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            var dataChanged = false;
            var presentValue = this.StringValue;
			Logger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);
            var postedValue = this.EditorText;
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                this.Value = postedValue;
                dataChanged = true;
            }
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.Value → {Arg2}", postDataKey, postCollection, this.Value);

			Logger.LogWarning("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return dataChanged;
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogError(">>> Entering CreateChildControls()");
            if (this.EditMode == PropertyEditorMode.Edit)
            {
                var pnlEditor = new Panel();
				Logger.LogError("CreateChildControls(): pnlEditor → {PnlEditor}", pnlEditor);
                if (string.IsNullOrEmpty(this.CssClass))
                {
                    pnlEditor.CssClass = "dnnLeft";
					Logger.LogDebug("CreateChildControls(): pnlEditor.CssClass → {Arg0}", pnlEditor.CssClass);
                }
                else
                {
                    pnlEditor.CssClass = $"{this.CssClass} dnnLeft";
                }

                this.richTextEditor = HtmlEditorProvider.Instance();
				Logger.LogDebug("CreateChildControls(): richTextEditor → {RichTextEditor}", richTextEditor);
                if (this.richTextEditor != null)
                {
                    this.richTextEditor.ControlID = this.ID + "edit";
                    this.richTextEditor.Initialize();
                    this.richTextEditor.Height = this.ControlStyle.Height;
                    this.richTextEditor.Width = this.ControlStyle.Width;
                    if (this.richTextEditor.Height.IsEmpty)
                    {
                        this.richTextEditor.Height = new Unit(250);
                    }

                    this.richTextEditor.Width = new Unit(400);
                }
                else
                {
                    this.defaultTextEditor = new TextBox
                    {
						Logger.LogDebug("CreateChildControls(): this.defaultTextEditor → {Arg0}", this.defaultTextEditor);
                        ID = this.ID + "edit",
                        Width = this.ControlStyle.Width.IsEmpty ? new Unit(300) : this.ControlStyle.Width,
                        Height = this.ControlStyle.Height.IsEmpty ? new Unit(250) : this.ControlStyle.Height,
                        TextMode = TextBoxMode.MultiLine,
                    };
                    this.defaultTextEditor.Attributes.Add("aria-label", "editor");
                }

                this.Controls.Clear();
                pnlEditor.Controls.Add(this.TextEditControl);
                this.Controls.Add(pnlEditor);
            }

            base.CreateChildControls();
			Logger.LogWarning("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void OnDataChanged(EventArgs e)
        {
			Logger.LogError(">>> Entering OnDataChanged(e={E})", e);
            var strValue = RemoveBaseTags(Convert.ToString(this.Value, CultureInfo.InvariantCulture));
			Logger.LogWarning("<<< Exiting OnDataChanged(e={E})", e);
            var strOldValue = RemoveBaseTags(Convert.ToString(this.OldValue, CultureInfo.InvariantCulture));
			Logger.LogError("OnDataChanged(e={E}): strOldValue → {StrOldValue}", e, strOldValue);
            var args = new PropertyEditorEventArgs(this.Name) { Value = this.Page.Server.HtmlEncode(strValue), OldValue = this.Page.Server.HtmlEncode(strOldValue), StringValue = this.Page.Server.HtmlEncode(RemoveBaseTags(this.StringValue)) };
			Logger.LogError("OnDataChanged(e={E}): args → {Args}", e, args);
			Logger.LogError("OnDataChanged(e={E}): strValue → {StrValue}", e, strValue);
            this.OnValueChanged(args);
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            this.EnsureChildControls();
            base.OnInit(e);
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            if (this.EditMode == PropertyEditorMode.Edit)
            {
                this.EditorText = this.Page.Server.HtmlDecode(Convert.ToString(this.Value, CultureInfo.InvariantCulture));
            }

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
            this.RenderChildren(writer);
			Logger.LogWarning("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <inheritdoc />
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderViewMode(writer={Writer})", writer);
            string propValue = this.Page.Server.HtmlDecode(Convert.ToString(this.Value, CultureInfo.InvariantCulture));
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(propValue);
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }

        private static string RemoveBaseTags(string strInput)
        {
			Logger.LogError(">>> Entering RemoveBaseTags(strInput={StrInput})", strInput);
            Logger.LogWarning("<<< Exiting RemoveBaseTags(strInput={StrInput})", strInput);
            return Globals.BaseTagRegex.Replace(strInput, " ");
        }
    }
}
