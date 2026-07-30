// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.UI.WebControls;

    /// <summary>A file edit control.</summary>
    public class DnnFileEditControl : IntegerEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private DnnFilePickerUploader fileControl;

        // private DnnFilePicker _fileControl;

        /// <summary>Gets or sets the current file extension filter.</summary>
        public string FileFilter { get; set; }

        /// <summary>Gets or sets the current file path.</summary>
        public string FilePath { get; set; }

        /// <remarks>
        ///   In this case because the <see cref = "fileControl" /> is a contained control, we do not need
        ///   to process the PostBackData (it has been handled by the File Control).  We just use
        ///   this method as the Framework calls it for us.
        /// </remarks>
        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
			Logger.LogDebug(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            bool dataChanged = false;
            string presentValue = this.StringValue;
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);

            // string postedValue = postCollection[string.Format("{0}FileControl$dnnFileUploadFileId", postDataKey)];
            string postedValue = this.fileControl.FileID.ToString(CultureInfo.InvariantCulture);
            if (!presentValue.Equals(postedValue, StringComparison.Ordinal))
            {
                this.Value = postedValue;
				Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): this.Value → {Arg2}", postDataKey, postCollection, this.Value);
                dataChanged = true;
            }

			Logger.LogDebug("<<< Exiting LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            return dataChanged;
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogDebug(">>> Entering CreateChildControls()");
            // First clear the controls collection
            this.Controls.Clear();

            var userControl = this.Page.LoadControl("~/controls/filepickeruploader.ascx");
            this.fileControl = userControl as DnnFilePickerUploader;
			Logger.LogDebug("CreateChildControls(): userControl → {UserControl}", userControl);
			Logger.LogDebug("CreateChildControls(): fileControl → {FileControl}", fileControl);
			Logger.LogDebug("CreateChildControls(): this.fileControl → {Arg0}", this.fileControl);

            if (this.fileControl != null)
            {
                this.fileControl.ID = $"{this.ID}FileControl";
                this.fileControl.FileFilter = this.FileFilter;
                this.fileControl.FilePath = this.FilePath;
                this.fileControl.FileID = this.IntegerValue;
                this.fileControl.UsePersonalFolder = true;
                this.fileControl.User = this.User;
                this.fileControl.Required = true;

                // Add table to Control
                this.Controls.Add(this.fileControl);
            }

            base.CreateChildControls();
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            this.EnsureChildControls();
            base.OnInit(e);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            this.fileControl.FileID = this.IntegerValue;

            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void RenderEditMode(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEditMode(writer={Writer})", writer);
            this.RenderChildren(writer);
			Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }
    }
}
