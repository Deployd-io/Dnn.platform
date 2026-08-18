// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Icons;

    /// <summary>
    /// The ImageCommandColumn control provides an Image Command (or Hyperlink) column
    /// for a Data Grid.
    /// </summary>
    public class ImageCommandColumn : TemplateColumn
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ImageCommandColumn));
        private ImageCommandColumnEditMode editMode = ImageCommandColumnEditMode.Command;
        private bool showImage = true;
        private string imageURL = string.Empty;

        /// <summary>Gets or sets the CommandName for the Column.</summary>
        /// <value>A String.</value>
        public string CommandName { get; set; }

        /// <summary>Gets or sets editMode for the Column.</summary>
        /// <value>A String.</value>
        public ImageCommandColumnEditMode EditMode
        {
            get { return this.editMode; }
            set { this.editMode = value; }
        }

        /// <summary>Gets or sets the URL of the Image.</summary>
        /// <value>A String.</value>
        public string ImageURL
        {
            get
            {
                if (!string.IsNullOrEmpty(this.imageURL))
                {
                    return this.imageURL;
                }

                return IconController.IconURL(this.IconKey, this.IconSize, this.IconStyle);
            }

            set
            {
                this.imageURL = value;
            }
        }

        /// <summary>Gets or sets the Icon Key to obtain ImageURL.</summary>
        /// <value>A String.</value>
        public string IconKey { get; set; }

        /// <summary>Gets or sets the Icon Siz to obtain ImageURL.</summary>
        /// <value>A String.</value>
        public string IconSize { get; set; }

        /// <summary>Gets or sets the Icon Style to obtain ImageURL.</summary>
        /// <value>A String.</value>
        public string IconStyle { get; set; }

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
// ReSharper disable InconsistentNaming
        public string OnClickJS { get; set; }

        // ReSharper restore InconsistentNaming

        /// <summary>Gets or sets a value indicating whether an Image is displayed.</summary>
        /// <remarks>Defaults to True.</remarks>
        /// <value>A Boolean.</value>
        public bool ShowImage
        {
            get { return this.showImage; }
            set { this.showImage = value; }
        }

        /// <summary>Gets or sets the Text (for Header/Footer Templates).</summary>
        /// <value>A String.</value>
        public string Text { get; set; }

        /// <summary>Gets or sets an flag that indicates whether the buttons are visible.</summary>
        /// <value>A Boolean.</value>
        public string VisibleField { get; set; }

        /// <summary>Initialises the Column.</summary>
        public override void Initialize()
        {
			Logger.LogDebug(">>> Entering Initialize()");
            this.ItemTemplate = this.CreateTemplate(ListItemType.Item);
            this.EditItemTemplate = this.CreateTemplate(ListItemType.EditItem);
            this.HeaderTemplate = this.CreateTemplate(ListItemType.Header);

            if (HttpContext.Current == null)
            {
                this.HeaderStyle.Font.Names = new[] { "Tahoma, Verdana, Arial" };
                this.HeaderStyle.Font.Size = new FontUnit("10pt");
                this.HeaderStyle.Font.Bold = true;
            }

            this.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
			Logger.LogDebug("<<< Exiting Initialize()");
        }

        /// <summary>Creates a ImageCommandColumnTemplate.</summary>
        /// <returns>A ImageCommandColumnTemplate.</returns>
        private ImageCommandColumnTemplate CreateTemplate(ListItemType type)
        {
			Logger.LogDebug(">>> Entering CreateTemplate(type={Type})", type);
            bool isDesignMode = HttpContext.Current == null;
            var template = new ImageCommandColumnTemplate(type);
			Logger.LogDebug("CreateTemplate(type={Type}): template → {Template}", type, template);
            if (type != ListItemType.Header)
            {
                template.ImageURL = this.ImageURL;
				Logger.LogDebug("CreateTemplate(type={Type}): template.ImageURL → {Arg1}", type, template.ImageURL);
                if (!isDesignMode)
                {
                    template.CommandName = this.CommandName;
					Logger.LogDebug("CreateTemplate(type={Type}): template.CommandName → {Arg1}", type, template.CommandName);
					Logger.LogDebug("CreateTemplate(type={Type}): template.VisibleField → {Arg1}", type, template.VisibleField);
					Logger.LogDebug("CreateTemplate(type={Type}): template.KeyField → {Arg1}", type, template.KeyField);
                    template.VisibleField = this.VisibleField;
                    template.KeyField = this.KeyField;
                }
            }

            template.EditMode = this.EditMode;
			Logger.LogDebug("CreateTemplate(type={Type}): template.EditMode → {Arg1}", type, template.EditMode);
			Logger.LogDebug("CreateTemplate(type={Type}): template.NavigateURL → {Arg1}", type, template.NavigateURL);
			Logger.LogDebug("CreateTemplate(type={Type}): template.NavigateURLFormatString → {Arg1}", type, template.NavigateURLFormatString);
			Logger.LogDebug("CreateTemplate(type={Type}): template.OnClickJS → {Arg1}", type, template.OnClickJS);
			Logger.LogDebug("CreateTemplate(type={Type}): template.ShowImage → {Arg1}", type, template.ShowImage);
			Logger.LogDebug("CreateTemplate(type={Type}): template.Visible → {Arg1}", type, template.Visible);
            template.NavigateURL = this.NavigateURL;
            template.NavigateURLFormatString = this.NavigateURLFormatString;
            template.OnClickJS = this.OnClickJS;
            template.ShowImage = this.ShowImage;
            template.Visible = this.Visible;

            template.Text = type == ListItemType.Header ? this.HeaderText : this.Text;

            // Set Design Mode to True
            template.DesignMode = isDesignMode;
			Logger.LogDebug("CreateTemplate(type={Type}): template.DesignMode → {Arg1}", type, template.DesignMode);

			Logger.LogDebug("<<< Exiting CreateTemplate(type={Type})", type);
            return template;
        }
    }
}
