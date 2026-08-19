// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;

    /// <summary>
    /// The SkinsEditControl control provides a standard UI component for editing
    /// skins.
    /// </summary>
    [ToolboxData("<{0}:SkinsEditControl runat=server></{0}:SkinsEditControl>")]
    public class SkinsEditControl : EditControl, IPostBackEventHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SkinsEditControl));
        private string addedItem = Null.NullString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkinsEditControl"/> class.
        /// Constructs a SkinsEditControl.
        /// </summary>
        public SkinsEditControl()
        {
		Logger.LogDebug(">>> Entering SkinsEditControl()");
		Logger.LogDebug("<<< Exiting SkinsEditControl()");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkinsEditControl"/> class.
        /// Constructs a SkinsEditControl.
        /// </summary>
        /// <param name="type">The type of the property.</param>
        public SkinsEditControl(string type)
        {
			Logger.LogDebug(">>> Entering SkinsEditControl(type={Type})", type);
            this.SystemType = type;
			Logger.LogDebug("<<< Exiting SkinsEditControl(type={Type})", type);
        }

        /// <summary>Gets oldStringValue returns the String representation of the OldValue.</summary>
        /// <value>A String representing the OldValue.</value>
        protected string OldStringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (this.OldDictionaryValue != null)
                {
                    foreach (string skin in this.OldDictionaryValue.Values)
                    {
                        strValue += skin + ",";
                    }
                }

                return strValue;
            }
        }

        /// <summary>Gets or sets dictionaryValue returns the Dictionary(Of Integer, String) representation of the Value.</summary>
        /// <value>A Dictionary(Of Integer, String) representing the Value.</value>
        protected Dictionary<int, string> DictionaryValue
        {
            get
            {
                return this.Value as Dictionary<int, string>;
            }

            set
            {
                this.Value = value;
            }
        }

        /// <summary>Gets or sets oldDictionaryValue returns the Dictionary(Of Integer, String) representation of the OldValue.</summary>
        /// <value>A Dictionary(Of Integer, String) representing the OldValue.</value>
        protected Dictionary<int, string> OldDictionaryValue
        {
            get
            {
                return this.OldValue as Dictionary<int, string>;
            }

            set
            {
                this.OldValue = value;
            }
        }

        /// <summary>Gets or sets stringValue is the value of the control expressed as a String.</summary>
        /// <value>A string representing the Value.</value>
        protected override string StringValue
        {
            get
            {
                string strValue = Null.NullString;
                if (this.DictionaryValue != null)
                {
                    foreach (string skin in this.DictionaryValue.Values)
                    {
                        strValue += skin + ",";
                    }
                }

                return strValue;
            }

            set
            {
                this.Value = value;
            }
        }

        protected string AddedItem
        {
            get
            {
                return this.addedItem;
            }

            set
            {
                this.addedItem = value;
            }
        }

        /// <inheritdoc />
        public void RaisePostBackEvent(string eventArgument)
        {
			Logger.LogDebug(">>> Entering RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
            PropertyEditorEventArgs args;
            switch (eventArgument.Substring(0, 3))
            {
                case "Del":
                    args = new PropertyEditorEventArgs(this.Name);
                    args.Value = this.DictionaryValue;
					Logger.LogDebug("RaisePostBackEvent(eventArgument={EventArgument}): args → {Args}", eventArgument, args);
					Logger.LogDebug("RaisePostBackEvent(eventArgument={EventArgument}): args.Value → {Arg1}", eventArgument, args.Value);
					Logger.LogDebug("RaisePostBackEvent(eventArgument={EventArgument}): args.OldValue → {Arg1}", eventArgument, args.OldValue);
					Logger.LogDebug("RaisePostBackEvent(eventArgument={EventArgument}): args.Changed → {Arg1}", eventArgument, args.Changed);
                    args.OldValue = this.OldDictionaryValue;
                    args.Key = int.Parse(eventArgument.Substring(7), CultureInfo.InvariantCulture);
                    args.Changed = true;
                    this.OnItemDeleted(args);
                    break;
                case "Add":
                    args = new PropertyEditorEventArgs(this.Name);
                    args.Value = this.AddedItem;
                    args.StringValue = this.AddedItem;
                    args.Changed = true;
					Logger.LogDebug("RaisePostBackEvent(eventArgument={EventArgument}): args.StringValue → {Arg1}", eventArgument, args.StringValue);
                    this.OnItemAdded(args);
                    break;
            }
			Logger.LogDebug("<<< Exiting RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogDebug(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            bool dataChanged = false;
            string postedValue;
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);
            var newDictionaryValue = new Dictionary<int, string>();
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): newDictionaryValue → {NewDictionaryValue}", postDataKey, postCollection, newDictionaryValue);
            foreach (KeyValuePair<int, string> kvp in this.DictionaryValue)
            {
                postedValue = postCollection[this.UniqueID + "_skin" + kvp.Key];
                if (kvp.Value.Equals(postedValue, StringComparison.Ordinal))
				Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): postedValue → {PostedValue}", postDataKey, postCollection, postedValue);
                {
                    newDictionaryValue[kvp.Key] = kvp.Value;
                }
                else
                {
                    newDictionaryValue[kvp.Key] = postedValue;
                    dataChanged = true;
                }
            }

            postedValue = postCollection[this.UniqueID + "_skinnew"];
            if (!string.IsNullOrEmpty(postedValue))
            {
                this.AddedItem = postedValue;
				Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.AddedItem → {Arg2}", postDataKey, postCollection, this.AddedItem);
            }

            this.DictionaryValue = newDictionaryValue;
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.DictionaryValue → {Arg2}", postDataKey, postCollection, this.DictionaryValue);
			Logger.LogDebug("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return dataChanged;
        }

        /// <summary>OnDataChanged runs when the PostbackData has changed.  It raises the <see cref="EditControl.ValueChanged"/> Event.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnDataChanged(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnDataChanged(e={E})", e);
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = this.DictionaryValue;
			Logger.LogDebug("OnDataChanged(e={E}): args.Value → {Arg1}", e, args.Value);
			Logger.LogDebug("OnDataChanged(e={E}): args.OldValue → {Arg1}", e, args.OldValue);
			Logger.LogDebug("OnDataChanged(e={E}): args.StringValue → {Arg1}", e, args.StringValue);
			Logger.LogDebug("OnDataChanged(e={E}): args.Changed → {Arg1}", e, args.Changed);
			Logger.LogDebug("OnDataChanged(e={E}): args → {Args}", e, args);
            args.OldValue = this.OldDictionaryValue;
            args.StringValue = string.Empty;
            args.Changed = true;
            this.OnValueChanged(args);
			Logger.LogDebug("<<< Exiting OnDataChanged(e={E})", e);
        }

        /// <summary>OnPreRender runs just before the control is due to be rendered.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            // Register control for PostBack
            this.Page.RegisterRequiresPostBack(this);
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <summary>RenderEditMode renders the Edit mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
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

            if (this.DictionaryValue != null)
            {
                foreach (KeyValuePair<int, string> kvp in this.DictionaryValue)
                {
                    // Render Hyperlink
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackClientHyperlink(this, "Delete_" + kvp.Key, false));
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "javascript:return confirm('" + ClientAPI.GetSafeJSString(Localization.GetString("DeleteItem")) + "');");
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("cmdDelete", this.LocalResourceFile));
                    writer.RenderBeginTag(HtmlTextWriterTag.A);

                    // Render Image
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Delete"));
                    writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);

                    // Render end of Image
                    writer.RenderEndTag();

                    // Render end of Hyperlink
                    writer.RenderEndTag();

                    this.ControlStyle.AddAttributesToRender(writer);
                    writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                    writer.AddAttribute(HtmlTextWriterAttribute.Value, kvp.Value);
                    if (length > Null.NullInteger)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString(CultureInfo.InvariantCulture));
                    }

                    writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_skin" + kvp.Key);
                    writer.RenderBeginTag(HtmlTextWriterTag.Input);
                    writer.RenderEndTag();

                    writer.WriteBreak();
                }

                writer.WriteBreak();

                // Create Add Row
                // Render Hyperlink
                writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackClientHyperlink(this, "Add", false));
                writer.AddAttribute(HtmlTextWriterAttribute.Title, Localization.GetString("cmdAdd", this.LocalResourceFile));
                writer.RenderBeginTag(HtmlTextWriterTag.A);

                // Render Image
                writer.AddAttribute(HtmlTextWriterAttribute.Src, IconController.IconURL("Add"));
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);

                // Render end of Image
                writer.RenderEndTag();

                // Render end of Hyperlink
                writer.RenderEndTag();

                this.ControlStyle.AddAttributesToRender(writer);
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
                writer.AddAttribute(HtmlTextWriterAttribute.Value, Null.NullString);
                if (length > Null.NullInteger)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Maxlength, length.ToString(CultureInfo.InvariantCulture));
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_skinnew");
                writer.RenderBeginTag(HtmlTextWriterTag.Input);
                writer.RenderEndTag();
                writer.WriteBreak();
            }
			Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <summary>RenderViewMode renders the View (readonly) mode of the control.</summary>
        /// <param name="writer">A HtmlTextWriter.</param>
        protected override void RenderViewMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderViewMode(writer={Writer})", writer);
            if (this.DictionaryValue != null)
            {
                foreach (KeyValuePair<int, string> kvp in this.DictionaryValue)
                {
                    this.ControlStyle.AddAttributesToRender(writer);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(kvp.Value);
                    writer.RenderEndTag();
                    writer.WriteBreak();
                }
            }
			Logger.LogDebug("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }
    }
}
