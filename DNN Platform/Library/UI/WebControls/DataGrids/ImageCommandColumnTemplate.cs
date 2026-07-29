// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.Utilities;

    /// <summary>The ImageCommandColumnTemplate provides a Template for the ImageCommandColumn.</summary>
    public class ImageCommandColumnTemplate : ITemplate
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="ImageCommandColumnTemplate"/> class.</summary>
        public ImageCommandColumnTemplate()
            : this(ListItemType.Item)
        {
		Logger.LogDebug(">>> Entering ImageCommandColumnTemplate()");
		Logger.LogDebug("<<< Exiting ImageCommandColumnTemplate()");
		Logger.LogDebug(">>> Entering ImageCommandColumnTemplate(itemType={ItemType})", itemType);
		Logger.LogDebug("<<< Exiting ImageCommandColumnTemplate(itemType={ItemType})", itemType);
        }

        /// <summary>Initializes a new instance of the <see cref="ImageCommandColumnTemplate"/> class.</summary>
        /// <param name="itemType">The list item type.</param>
        public ImageCommandColumnTemplate(ListItemType itemType)
        {
            this.ItemType = itemType;
			Logger.LogDebug("ImageCommandColumnTemplate(itemType={ItemType}): this.ItemType → {Arg1}", itemType, this.ItemType);
        }

        /// <summary>Gets or sets the CommandName for the Column.</summary>
        /// <value>A String.</value>
        public string CommandName { get; set; }

        /// <summary>Gets or sets a value indicating whether the Column is in design mode.</summary>
        /// <value>A Boolean.</value>
        public bool DesignMode { get; set; }

        /// <summary>Gets or sets the CommandName for the Column.</summary>
        /// <value>A String.</value>
        public ImageCommandColumnEditMode EditMode { get; set; } = ImageCommandColumnEditMode.Command;

        /// <summary>Gets or sets the URL of the Image.</summary>
        /// <value>A String.</value>
        public string ImageURL { get; set; }

        /// <summary>Gets or sets the type of Template to Create.</summary>
        /// <value>A String.</value>
        public ListItemType ItemType { get; set; } = ListItemType.Item;

        /// <summary>Gets or sets the Key Field that provides a Unique key to the data Item.</summary>
        /// <value>A String.</value>
        public string KeyField { get; set; }

        /// <summary>Gets or sets the URL of the Link (unless DataBinding through KeyField).</summary>
        /// <value>A String.</value>
        public string NavigateURL { get; set; }

        /// <summary>Gets or sets the URL Formatting string.</summary>
        /// <value>A String.</value>
        public string NavigateURLFormatString { get; set; }

        /// <summary>Gets or sets javascript text to attach to the OnClick Event.</summary>
        /// <value>A String.</value>
        public string OnClickJS { get; set; }

        /// <summary>Gets or sets a value indicating whether an Image is displayed.</summary>
        /// <remarks>Defaults to True.</remarks>
        /// <value>A Boolean.</value>
        public bool ShowImage { get; set; } = true;

        /// <summary>Gets or sets the Text (for Header/Footer Templates).</summary>
        /// <value>A String.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the buttons are visible (this is overridden if
        /// the VisibleField is set).
        /// </summary>
        /// <value>A Boolean.</value>
        public bool Visible { get; set; } = true;

        /// <summary>Gets or sets an flag that indicates whether the buttons are visible.</summary>
        /// <value>A Boolean.</value>
        public string VisibleField { get; set; }

        /// <summary>InstantiateIn instantiates the template (implementation of ITemplate).</summary>
        /// <param name="container">The parent container (DataGridItem).</param>
        public void InstantiateIn(Control container)
        {
			Logger.LogDebug(">>> Entering InstantiateIn(container={Container})", container);
            switch (this.ItemType)
            {
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                case ListItemType.SelectedItem:
                case ListItemType.EditItem:
                    if (this.EditMode == ImageCommandColumnEditMode.URL)
                    {
                        var hypLink = new HyperLink();
                        hypLink.ToolTip = this.Text;
						Logger.LogDebug("InstantiateIn(container={Container}): hypLink → {HypLink}", container, hypLink);
						Logger.LogDebug("InstantiateIn(container={Container}): hypLink.ToolTip → {Arg1}", container, hypLink.ToolTip);
                        if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                        {
                            var img = new Image();
                            if (this.DesignMode)
							Logger.LogDebug("InstantiateIn(container={Container}): img → {Img}", container, img);
                            {
                                img.ImageUrl = this.ImageURL.Replace("~/", "../../");
                            }
                            else
                            {
                                img.ImageUrl = this.ImageURL;
								Logger.LogDebug("InstantiateIn(container={Container}): img.ImageUrl → {Arg1}", container, img.ImageUrl);
                            }

                            hypLink.Controls.Add(img);
                            img.ToolTip = this.Text;
							Logger.LogDebug("InstantiateIn(container={Container}): img.ToolTip → {Arg1}", container, img.ToolTip);
                        }
                        else
                        {
                            hypLink.Text = this.Text;
							Logger.LogDebug("InstantiateIn(container={Container}): hypLink.Text → {Arg1}", container, hypLink.Text);
                        }

                        hypLink.DataBinding += this.Item_DataBinding;
                        container.Controls.Add(hypLink);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                        {
                            var colIcon = new ImageButton();
                            if (this.DesignMode)
							Logger.LogDebug("InstantiateIn(container={Container}): colIcon → {ColIcon}", container, colIcon);
                            {
                                colIcon.ImageUrl = this.ImageURL.Replace("~/", "../../");
                            }
                            else
                            {
                                colIcon.ImageUrl = this.ImageURL;
								Logger.LogDebug("InstantiateIn(container={Container}): colIcon.ImageUrl → {Arg1}", container, colIcon.ImageUrl);
                            }

                            colIcon.ToolTip = this.Text;
							Logger.LogDebug("InstantiateIn(container={Container}): colIcon.ToolTip → {Arg1}", container, colIcon.ToolTip);
                            if (!string.IsNullOrEmpty(this.OnClickJS))
                            {
                                ClientAPI.AddButtonConfirm(colIcon, this.OnClickJS);
                            }

                            colIcon.CommandName = this.CommandName;
							Logger.LogDebug("InstantiateIn(container={Container}): colIcon.CommandName → {Arg1}", container, colIcon.CommandName);
                            colIcon.DataBinding += this.Item_DataBinding;
                            container.Controls.Add(colIcon);
                        }

                        if (!string.IsNullOrEmpty(this.Text) && !this.ShowImage)
                        {
                            var colLink = new LinkButton();
                            colLink.ToolTip = this.Text;
							Logger.LogDebug("InstantiateIn(container={Container}): colLink → {ColLink}", container, colLink);
							Logger.LogDebug("InstantiateIn(container={Container}): colLink.ToolTip → {Arg1}", container, colLink.ToolTip);
                            if (!string.IsNullOrEmpty(this.OnClickJS))
                            {
                                ClientAPI.AddButtonConfirm(colLink, this.OnClickJS);
                            }

                            colLink.CommandName = this.CommandName;
							Logger.LogDebug("InstantiateIn(container={Container}): colLink.CommandName → {Arg1}", container, colLink.CommandName);
							Logger.LogDebug("InstantiateIn(container={Container}): colLink.Text → {Arg1}", container, colLink.Text);
                            colLink.Text = this.Text;
                            colLink.DataBinding += this.Item_DataBinding;
                            container.Controls.Add(colLink);
                        }
                    }

                    break;
                case ListItemType.Footer:
                case ListItemType.Header:
                    container.Controls.Add(new LiteralControl(this.Text));
                    break;
            }
			Logger.LogDebug("<<< Exiting InstantiateIn(container={Container})", container);
        }

        /// <summary>Gets whether theButton is visible.</summary>
        /// <param name="container">The parent container (DataGridItem).</param>
        private bool GetIsVisible(DataGridItem container)
        {
			Logger.LogDebug(">>> Entering GetIsVisible(container={Container})", container);
            bool isVisible;
            if (!string.IsNullOrEmpty(this.VisibleField))
            {
                isVisible = Convert.ToBoolean(DataBinder.Eval(container.DataItem, this.VisibleField), CultureInfo.InvariantCulture);
				Logger.LogDebug("GetIsVisible(container={Container}): isVisible → {IsVisible}", container, isVisible);
            }
            else
            {
                isVisible = this.Visible;
            }

			Logger.LogDebug("<<< Exiting GetIsVisible(container={Container})", container);
            return isVisible;
        }

        /// <summary>Gets the value of the key.</summary>
        /// <param name="container">The parent container (DataGridItem).</param>
        private int GetValue(DataGridItem container)
        {
			Logger.LogDebug(">>> Entering GetValue(container={Container})", container);
            int keyValue = Null.NullInteger;
            if (!string.IsNullOrEmpty(this.KeyField))
			Logger.LogDebug("GetValue(container={Container}): keyValue → {KeyValue}", container, keyValue);
            {
                keyValue = Convert.ToInt32(DataBinder.Eval(container.DataItem, this.KeyField), CultureInfo.InvariantCulture);
            }

			Logger.LogDebug("<<< Exiting GetValue(container={Container})", container);
            return keyValue;
        }

        /// <summary>Item_DataBinding runs when an Item of type ListItemType.Item is being data-bound.</summary>
        /// <param name="sender"> The object that triggers the event.</param>
        /// <param name="e">An EventArgs object.</param>
        private void Item_DataBinding(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering Item_DataBinding(sender={Sender},e={E})", sender, e);
            DataGridItem container;
            int keyValue = Null.NullInteger;
            if (this.EditMode == ImageCommandColumnEditMode.URL)
			Logger.LogDebug("Item_DataBinding(sender={Sender},e={E}): keyValue → {KeyValue}", sender, e, keyValue);
            {
                var hypLink = (HyperLink)sender;
                container = (DataGridItem)hypLink.NamingContainer;
				Logger.LogDebug("Item_DataBinding(sender={Sender},e={E}): hypLink → {HypLink}", sender, e, hypLink);
				Logger.LogDebug("Item_DataBinding(sender={Sender},e={E}): container → {Container}", sender, e, container);
                keyValue = this.GetValue(container);
                if (!string.IsNullOrEmpty(this.NavigateURLFormatString))
                {
                    hypLink.NavigateUrl = string.Format(CultureInfo.InvariantCulture, this.NavigateURLFormatString, keyValue);
                }
                else
                {
                    hypLink.NavigateUrl = keyValue.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                // Bind Image Button
                if (!string.IsNullOrEmpty(this.ImageURL) && this.ShowImage)
                {
                    var colIcon = (ImageButton)sender;
                    container = (DataGridItem)colIcon.NamingContainer;
					Logger.LogDebug("Item_DataBinding(sender={Sender},e={E}): colIcon → {ColIcon}", sender, e, colIcon);
                    keyValue = this.GetValue(container);
                    colIcon.CommandArgument = keyValue.ToString(CultureInfo.InvariantCulture);
                    colIcon.Visible = this.GetIsVisible(container);
                }

                if (!string.IsNullOrEmpty(this.Text) && !this.ShowImage)
                {
                    // Bind Link Button
                    var colLink = (LinkButton)sender;
                    container = (DataGridItem)colLink.NamingContainer;
					Logger.LogDebug("Item_DataBinding(sender={Sender},e={E}): colLink → {ColLink}", sender, e, colLink);
                    keyValue = this.GetValue(container);
                    colLink.CommandArgument = keyValue.ToString(CultureInfo.InvariantCulture);
                    colLink.Visible = this.GetIsVisible(container);
                }
            }
			Logger.LogDebug("<<< Exiting Item_DataBinding(sender={Sender},e={E})", sender, e);
        }
    }
}
