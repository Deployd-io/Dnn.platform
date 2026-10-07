// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Localization;

    public class DualListBox : WebControl, IPostBackEventHandler, IPostBackDataHandler
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DualListBox));
        private readonly Style availableListBoxStyle = new Style();
        private readonly Style buttonStyle = new Style();
        private readonly TableStyle containerStyle = new TableStyle();
        private readonly Style headerStyle = new Style();
        private readonly Style selectedListBoxStyle = new Style();
        private List<string> addValues;
        private List<string> removeValues;

        /// <summary>Initializes a new instance of the <see cref="DualListBox"/> class.</summary>
        public DualListBox()
        {
			Logger.LogError(">>> Entering DualListBox()");
            this.ShowAddButton = true;
			Logger.LogDebug("DualListBox(): this.ShowAddButton → {Arg0}", this.ShowAddButton);
            this.ShowAddAllButton = true;
            this.ShowRemoveButton = true;
			Logger.LogDebug("DualListBox(): this.ShowAddAllButton → {Arg0}", this.ShowAddAllButton);
        Logger.LogWarning("<<< Exiting DualListBox()");
            this.ShowRemoveAllButton = true;
        }

        public event DualListBoxEventHandler AddButtonClick;

        public event EventHandler AddAllButtonClick;

        public event DualListBoxEventHandler RemoveButtonClick;

        public event EventHandler RemoveAllButtonClick;

        /// <summary>Gets the value of the Available List Box Style.</summary>
        /// <value>A Style object.</value>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Available List Box.")]
        public Style AvailableListBoxStyle
        {
            get
            {
                return this.availableListBoxStyle;
            }
        }

        /// <summary>Gets the value of the Button Style.</summary>
        /// <value>A Style object.</value>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Button.")]
        public Style ButtonStyle
        {
            get
            {
                return this.buttonStyle;
            }
        }

        /// <summary>Gets the value of the Container Style.</summary>
        /// <value>A Style object.</value>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Container.")]
        public TableStyle ContainerStyle
        {
            get
            {
                return this.containerStyle;
            }
        }

        /// <summary>Gets the value of the Header Style.</summary>
        /// <value>A Style object.</value>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Header.")]
        public Style HeaderStyle
        {
            get
            {
                return this.headerStyle;
            }
        }

        /// <summary>Gets the value of the Selected List Box Style.</summary>
        /// <value>A Style object.</value>
        [Browsable(true)]
        [Category("Styles")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [Description("Set the Style for the Selected List Box.")]
        public Style SelectedListBoxStyle
        {
            get
            {
                return this.selectedListBoxStyle;
            }
        }

        public string AddAllImageURL { get; set; }

        public string AddAllKey { get; set; }

        public string AddAllText { get; set; }

        public string AddImageURL { get; set; }

        public string AddKey { get; set; }

        public string AddText { get; set; }

        public object AvailableDataSource { get; set; }

        public string DataTextField { private get; set; }

        public string DataValueField { private get; set; }

        public string LocalResourceFile { get; set; }

        public string RemoveAllImageURL { get; set; }

        public string RemoveAllKey { get; set; }

        public string RemoveAllText { get; set; }

        public string RemoveImageURL { get; set; }

        public string RemoveKey { get; set; }

        public string RemoveText { get; set; }

        public object SelectedDataSource { get; set; }

        public bool ShowAddButton { get; set; }

        public bool ShowAddAllButton { get; set; }

        public bool ShowRemoveButton { get; set; }

        public bool ShowRemoveAllButton { get; set; }

        public bool CausesValidation { get; set; }

        public string ValidationGroup { get; set; }

        /// <inheritdoc />
        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        /// <inheritdoc />
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogError(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            bool retValue = Null.NullBoolean;
            string addItems = postCollection[postDataKey + "_Available"];
			Logger.LogError("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): retValue → {RetValue}", postDataKey, postCollection, retValue);
            if (!string.IsNullOrEmpty(addItems))
            {
                this.addValues = new List<string>();
				Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): addValues → {AddValues}", postDataKey, postCollection, addValues);
                foreach (string addItem in addItems.Split(','))
                {
                    this.addValues.Add(addItem);
                }

                retValue = true;
            }

            string removeItems = postCollection[postDataKey + "_Selected"];
            if (!string.IsNullOrEmpty(removeItems))
            {
                this.removeValues = new List<string>();
				Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): removeValues → {RemoveValues}", postDataKey, postCollection, removeValues);
                foreach (string removeItem in removeItems.Split(','))
                {
                    this.removeValues.Add(removeItem);
                }

                retValue = true;
            }

			Logger.LogWarning("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return retValue;
        }

        /// <inheritdoc />
        public void RaisePostDataChangedEvent()
        {
		Logger.LogError(">>> Entering RaisePostDataChangedEvent()");
        Logger.LogWarning("<<< Exiting RaisePostDataChangedEvent()");
        }

        /// <inheritdoc />
        public void RaisePostBackEvent(string eventArgument)
        {
			Logger.LogError(">>> Entering RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
            switch (eventArgument)
            {
                case "Add":
                    this.OnAddButtonClick(new DualListBoxEventArgs(this.addValues));
                    break;
                case "AddAll":
                    this.OnAddAllButtonClick(new EventArgs());
                    break;
                case "Remove":
                    this.OnRemoveButtonClick(new DualListBoxEventArgs(this.removeValues));
                    break;
                case "RemoveAll":
                    this.OnRemoveAllButtonClick(new EventArgs());
                    break;
            }
			Logger.LogWarning("<<< Exiting RaisePostBackEvent(eventArgument={EventArgument})", eventArgument);
        }

        protected virtual PostBackOptions GetPostBackOptions(string argument)
        {
			Logger.LogError(">>> Entering GetPostBackOptions(argument={Argument})", argument);
            var postBackOptions = new PostBackOptions(this, argument) { RequiresJavaScriptProtocol = true };

			Logger.LogError("GetPostBackOptions(argument={Argument}): postBackOptions → {PostBackOptions}", argument, postBackOptions);
            if (this.CausesValidation && this.Page.GetValidators(this.ValidationGroup).Count > 0)
            {
                postBackOptions.PerformValidation = true;
				Logger.LogDebug("GetPostBackOptions(argument={Argument}): postBackOptions.PerformValidation → {Arg1}", argument, postBackOptions.PerformValidation);
                postBackOptions.ValidationGroup = this.ValidationGroup;
				Logger.LogDebug("GetPostBackOptions(argument={Argument}): postBackOptions.ValidationGroup → {Arg1}", argument, postBackOptions.ValidationGroup);
            }

			Logger.LogWarning("<<< Exiting GetPostBackOptions(argument={Argument})", argument);
            return postBackOptions;
        }

        protected void OnAddButtonClick(DualListBoxEventArgs e)
        {
			Logger.LogError(">>> Entering OnAddButtonClick(e={E})", e);
            if (this.AddButtonClick != null)
            {
                this.AddButtonClick(this, e);
            }
			Logger.LogWarning("<<< Exiting OnAddButtonClick(e={E})", e);
        }

        protected void OnAddAllButtonClick(EventArgs e)
        {
			Logger.LogError(">>> Entering OnAddAllButtonClick(e={E})", e);
            if (this.AddAllButtonClick != null)
            {
                this.AddAllButtonClick(this, e);
            }
			Logger.LogWarning("<<< Exiting OnAddAllButtonClick(e={E})", e);
        }

        protected void OnRemoveButtonClick(DualListBoxEventArgs e)
        {
			Logger.LogError(">>> Entering OnRemoveButtonClick(e={E})", e);
            if (this.RemoveButtonClick != null)
            {
                this.RemoveButtonClick(this, e);
            }
			Logger.LogWarning("<<< Exiting OnRemoveButtonClick(e={E})", e);
        }

        protected void OnRemoveAllButtonClick(EventArgs e)
        {
			Logger.LogError(">>> Entering OnRemoveAllButtonClick(e={E})", e);
            if (this.RemoveAllButtonClick != null)
            {
                this.RemoveAllButtonClick(this, e);
            }
			Logger.LogWarning("<<< Exiting OnRemoveAllButtonClick(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void RenderContents(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderContents(writer={Writer})", writer);
            // render table
            if (this.ContainerStyle != null)
            {
                this.ContainerStyle.AddAttributesToRender(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            // Render Header Row
            this.RenderHeader(writer);

            // Render ListBox row
            this.RenderListBoxes(writer);

            // Render end of table
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderContents(writer={Writer})", writer);
        }

        private NameValueCollection GetList(string listType, object dataSource)
        {
			Logger.LogError(">>> Entering GetList(listType={ListType},dataSource={DataSource})", listType, dataSource);
            var dataList = dataSource as IEnumerable;
            var list = new NameValueCollection();
			Logger.LogError("GetList(listType={ListType},dataSource={DataSource}): dataList → {DataList}", listType, dataSource, dataList);
            if (dataList == null)
            {
				Logger.LogError("GetList(listType={ListType},dataSource={DataSource}): list → {List}", listType, dataSource, list);
				Logger.LogError("Throwing ArgumentException from GetList(listType={ListType},dataSource={DataSource})", listType, dataSource);
                throw new ArgumentException("The " + listType + "DataSource must implement the IEnumerable Interface");
            }
            else
            {
                foreach (object item in dataList)
                {
                    const BindingFlags bindings = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
                    PropertyInfo objTextProperty = item.GetType().GetProperty(this.DataTextField, bindings);
					Logger.LogError("GetList(listType={ListType},dataSource={DataSource}): objTextProperty → {ObjTextProperty}", listType, dataSource, objTextProperty);
                    PropertyInfo objValueProperty = item.GetType().GetProperty(this.DataValueField, bindings);
					Logger.LogError("GetList(listType={ListType},dataSource={DataSource}): objValueProperty → {ObjValueProperty}", listType, dataSource, objValueProperty);
                    string objValue = Convert.ToString(objValueProperty.GetValue(item, null), CultureInfo.InvariantCulture);
                    string objText = Convert.ToString(objTextProperty.GetValue(item, null), CultureInfo.InvariantCulture);

                    list.Add(objText, objValue);
                }
            }

			Logger.LogWarning("<<< Exiting GetList(listType={ListType},dataSource={DataSource})", listType, dataSource);
            return list;
        }

        private void RenderButton(string buttonType, HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderButton(buttonType={ButtonType},writer={Writer})", buttonType, writer);
            string buttonText = Null.NullString;
            string imageUrl = Null.NullString;
			Logger.LogError("RenderButton(buttonType={ButtonType},writer={Writer}): imageUrl → {ImageUrl}", buttonType, writer, imageUrl);

            // Begin Button Row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // Begin Button Cell
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            switch (buttonType)
            {
                case "Add":
                    buttonText = string.IsNullOrEmpty(this.AddKey)
                                    ? this.AddText
                                    : Localization.GetString(this.AddKey, this.LocalResourceFile);
                    imageUrl = this.AddImageURL;
                    break;
                case "AddAll":
                    buttonText = string.IsNullOrEmpty(this.AddAllKey)
                                    ? this.AddAllText
                                    : Localization.GetString(this.AddAllKey, this.LocalResourceFile);
                    imageUrl = this.AddAllImageURL;
                    break;
                case "Remove":
                    buttonText = string.IsNullOrEmpty(this.RemoveKey)
                                    ? this.RemoveText
                                    : Localization.GetString(this.RemoveKey, this.LocalResourceFile);
                    imageUrl = this.RemoveImageURL;
                    break;
                case "RemoveAll":
                    buttonText = string.IsNullOrEmpty(this.RemoveAllKey)
                                    ? this.RemoveAllText
                                    : Localization.GetString(this.RemoveAllKey, this.LocalResourceFile);
                    imageUrl = this.RemoveAllImageURL;
                    break;
            }

            // Render Hyperlink
            writer.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ClientScript.GetPostBackEventReference(this.GetPostBackOptions(buttonType)));
            writer.AddAttribute(HtmlTextWriterAttribute.Title, buttonText);
            writer.RenderBeginTag(HtmlTextWriterTag.A);

            // Render Image
            if (!string.IsNullOrEmpty(imageUrl))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Src, this.ResolveClientUrl(imageUrl));
                writer.AddAttribute(HtmlTextWriterAttribute.Title, buttonText);
                writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
                writer.RenderEndTag();
            }
            else
            {
                writer.Write(buttonText);
            }

            // End of Hyperlink
            writer.RenderEndTag();

            // End of Button Cell
            writer.RenderEndTag();

            // Render end of Button Row
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderButton(buttonType={ButtonType},writer={Writer})", buttonType, writer);
        }

        private void RenderButtons(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderButtons(writer={Writer})", writer);
            // render table
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            if (this.ShowAddButton)
            {
                this.RenderButton("Add", writer);
            }

            if (this.ShowAddAllButton)
            {
                this.RenderButton("AddAll", writer);
            }

            // Begin Button Row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            // Begin Button Cell
            writer.RenderBeginTag(HtmlTextWriterTag.Td);

            writer.Write("&nbsp;");

            // End of Button Cell
            writer.RenderEndTag();

            // Render end of Button Row
            writer.RenderEndTag();

            if (this.ShowRemoveButton)
            {
                this.RenderButton("Remove", writer);
            }

            if (this.ShowRemoveAllButton)
            {
                this.RenderButton("RemoveAll", writer);
            }

            // Render end of table
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderButtons(writer={Writer})", writer);
        }

        private void RenderListBox(string listType, object dataSource, Style style, HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderListBox(listType={ListType},dataSource={DataSource},style={Style},writer={Writer})", listType, dataSource, style, writer);
            if (dataSource != null)
            {
                NameValueCollection list = this.GetList(listType, dataSource);
				Logger.LogError("RenderListBox(listType={ListType},dataSource={DataSource},style={Style},writer={Writer}): list → {List}", listType, dataSource, style, writer, list);
                if (list != null)
                {
                    if (style != null)
                    {
                        style.AddAttributesToRender(writer);
                    }

                    // Render ListBox
                    writer.AddAttribute(HtmlTextWriterAttribute.Multiple, "multiple");
                    writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID + "_" + listType);
                    writer.RenderBeginTag(HtmlTextWriterTag.Select);
                    for (int i = 0; i <= list.Count - 1; i++)
                    {
                        // Render option tags for each item
                        writer.AddAttribute(HtmlTextWriterAttribute.Value, list.Get(i));
                        writer.RenderBeginTag(HtmlTextWriterTag.Option);
                        writer.Write(list.GetKey(i));
                        writer.RenderEndTag();
                    }

                    // Render ListBox end
                    writer.RenderEndTag();
                }
            }
			Logger.LogWarning("<<< Exiting RenderListBox(listType={ListType},dataSource={DataSource},style={Style},writer={Writer})", listType, dataSource, style, writer);
        }

        private void RenderHeader(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderHeader(writer={Writer})", writer);
            // render Header row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (this.HeaderStyle != null)
            {
                this.HeaderStyle.AddAttributesToRender(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(Localization.GetString(this.ID + "_Available", this.LocalResourceFile));
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.RenderEndTag();
            if (this.HeaderStyle != null)
            {
                this.HeaderStyle.AddAttributesToRender(writer);
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(Localization.GetString(this.ID + "_Selected", this.LocalResourceFile));
            writer.RenderEndTag();

            // Render end of Header Row
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderHeader(writer={Writer})", writer);
        }

        private void RenderListBoxes(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering RenderListBoxes(writer={Writer})", writer);
            // render List Boxes row
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderListBox("Available", this.AvailableDataSource, this.AvailableListBoxStyle, writer);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderButtons(writer);
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            this.RenderListBox("Selected", this.SelectedDataSource, this.SelectedListBoxStyle, writer);
            writer.RenderEndTag();

            // Render end of List Boxes Row
            writer.RenderEndTag();
			Logger.LogWarning("<<< Exiting RenderListBoxes(writer={Writer})", writer);
        }
    }
}
