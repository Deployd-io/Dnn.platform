// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;

    /// <summary>The TextColumnTemplate provides a Template for the TextColumn.</summary>
    public class TextColumnTemplate : ITemplate
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TextColumnTemplate));
        private ListItemType mItemType = ListItemType.Item;

        /// <summary>Initializes a new instance of the <see cref="TextColumnTemplate"/> class.</summary>
        public TextColumnTemplate()
            : this(ListItemType.Item)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TextColumnTemplate"/> class.</summary>
        /// <param name="itemType">The list item type.</param>
        public TextColumnTemplate(ListItemType itemType)
        {
			Logger.LogError(">>> Entering TextColumnTemplate(itemType={ItemType})", itemType);
            this.ItemType = itemType;
        }

        /// <summary>Gets or sets the Data Field is the field that binds to the Text Column.</summary>
        /// <value>A String.</value>
        public string DataField { get; set; }

        /// <summary>Gets or sets a value indicating whether the Column is in design mode.</summary>
        /// <value>A Boolean.</value>
        public bool DesignMode { get; set; }

        /// <summary>Gets or sets the type of Template to Create.</summary>
        /// <value>A String.</value>
        public ListItemType ItemType
        {
            get
            {
                return this.mItemType;
            }

            set
            {
                this.mItemType = value;
            }
        }

        /// <summary>Gets or sets the Text (for Header/Footer Templates).</summary>
        /// <value>A String.</value>
        public string Text { get; set; }

        /// <summary>Gets or sets the Width of the Column.</summary>
        /// <value>A Unit.</value>
        public Unit Width { get; set; }

        /// <summary>InstantiateIn instantiates the template (implementation of ITemplate).</summary>
        /// <param name="container">The parent container (DataGridItem).</param>
        public void InstantiateIn(Control container)
        {
			Logger.LogError(">>> Entering InstantiateIn(container={Container})", container);
            switch (this.ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    var lblText = new Label();
					Logger.LogError("InstantiateIn(container={Container}): lblText → {LblText}", container, lblText);
                    lblText.Width = this.Width;
					Logger.LogDebug("InstantiateIn(container={Container}): lblText.Width → {Arg1}", container, lblText.Width);
                    lblText.DataBinding += this.Item_DataBinding;
                    container.Controls.Add(lblText);
                    break;
                case ListItemType.EditItem:
                    var txtText = new TextBox();
					Logger.LogError("InstantiateIn(container={Container}): txtText → {TxtText}", container, txtText);
                    txtText.Width = this.Width;
					Logger.LogDebug("InstantiateIn(container={Container}): txtText.Width → {Arg1}", container, txtText.Width);
                    txtText.DataBinding += this.Item_DataBinding;
                    container.Controls.Add(txtText);
                    break;
                case ListItemType.Footer:
                case ListItemType.Header:
                    container.Controls.Add(new LiteralControl(this.Text));
                    break;
            }
			Logger.LogWarning("<<< Exiting InstantiateIn(container={Container})", container);
        }

        /// <summary>Gets the value of the Data Field.</summary>
        /// <param name="container">The parent container (DataGridItem).</param>
        private string GetValue(DataGridItem container)
        {
			Logger.LogError(">>> Entering GetValue(container={Container})", container);
            string itemValue = Null.NullString;
			Logger.LogError("GetValue(container={Container}): itemValue → {ItemValue}", container, itemValue);
            if (!string.IsNullOrEmpty(this.DataField))
            {
                if (this.DesignMode)
                {
                    itemValue = "DataBound to " + this.DataField;
                }
                else
                {
                    if (container.DataItem != null)
                    {
                        object evaluation = DataBinder.Eval(container.DataItem, this.DataField);
						Logger.LogError("GetValue(container={Container}): evaluation → {Evaluation}", container, evaluation);
                        if (evaluation != null)
                        {
                            itemValue = evaluation.ToString();
                        }
                    }
                }
            }

			Logger.LogWarning("<<< Exiting GetValue(container={Container})", container);
            return itemValue;
        }

        /// <summary>Item_DataBinding runs when an Item of type ListItemType.Item is being data-bound.</summary>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An EventArgs object.</param>
        private void Item_DataBinding(object sender, EventArgs e)
        {
			Logger.LogError(">>> Entering Item_DataBinding(sender={Sender},e={E})", sender, e);
            DataGridItem container;
            int keyValue = Null.NullInteger;
            switch (this.ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                    var lblText = (Label)sender;
					Logger.LogError("Item_DataBinding(sender={Sender},e={E}): lblText → {LblText}", sender, e, lblText);
                    container = (DataGridItem)lblText.NamingContainer;
					Logger.LogError("Item_DataBinding(sender={Sender},e={E}): container → {Container}", sender, e, container);
					Logger.LogDebug("Item_DataBinding(sender={Sender},e={E}): container → {Container}", sender, e, container);
                    lblText.Text = this.GetValue(container);
                    break;
                case ListItemType.EditItem:
                    var txtText = (TextBox)sender;
					Logger.LogError("Item_DataBinding(sender={Sender},e={E}): txtText → {TxtText}", sender, e, txtText);
                    container = (DataGridItem)txtText.NamingContainer;
                    txtText.Text = this.GetValue(container);
                    break;
            }
			Logger.LogWarning("<<< Exiting Item_DataBinding(sender={Sender},e={E})", sender, e);
        }
    }
}
