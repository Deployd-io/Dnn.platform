// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;

    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    /// <summary>A time zone edit control.</summary>
    public class DnnTimeZoneEditControl : TextEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private DnnTimeZoneComboBox timeZones;

        /// <summary>Initializes a new instance of the <see cref="DnnTimeZoneEditControl"/> class.</summary>
        public DnnTimeZoneEditControl()
        {
		Logger.LogDebug(">>> Entering DnnTimeZoneEditControl()");
		Logger.LogDebug("<<< Exiting DnnTimeZoneEditControl()");
        }

        /// <inheritdoc />
        public override string EditControlClientId
        {
            get
            {
                this.EnsureChildControls();
                return this.timeZones.ClientID;
            }
        }

        /// <inheritdoc />
        public override bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
        {
			Logger.LogDebug(">>> Entering LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection})", postDataKey, postCollection);
            bool dataChanged = false;
            string presentValue = this.StringValue;
			Logger.LogDebug("LoadPostData(postDataKey={PostDataKey},postCollection={PostCollection}): dataChanged → {DataChanged}", postDataKey, postCollection, dataChanged);
            string postedValue = this.timeZones.SelectedValue;
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
            this.timeZones = new DnnTimeZoneComboBox();
            this.timeZones.ViewStateMode = ViewStateMode.Disabled;
			Logger.LogDebug("CreateChildControls(): timeZones → {TimeZones}", timeZones);

            this.Controls.Clear();
            this.Controls.Add(this.timeZones);

            base.CreateChildControls();
			Logger.LogDebug("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void OnDataChanged(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnDataChanged(e={E})", e);
            var args = new PropertyEditorEventArgs(this.Name);
            args.Value = TimeZoneInfo.FindSystemTimeZoneById(this.StringValue);
			Logger.LogDebug("OnDataChanged(e={E}): args → {Args}", e, args);
			Logger.LogDebug("OnDataChanged(e={E}): args.OldValue → {Arg1}", e, args.OldValue);
			Logger.LogDebug("OnDataChanged(e={E}): args.StringValue → {Arg1}", e, args.StringValue);
            args.OldValue = this.OldStringValue;
            args.StringValue = this.StringValue;
            this.OnValueChanged(args);
			Logger.LogDebug("<<< Exiting OnDataChanged(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnInit(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            this.EnsureChildControls();
            base.OnInit(e);
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnPreRender(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            this.timeZones.DataBind(this.StringValue);

            if ((this.Page != null) && this.EditMode == PropertyEditorMode.Edit)
            {
                this.Page.RegisterRequiresPostBack(this);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void RenderEditMode(System.Web.UI.HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderEditMode(writer={Writer})", writer);
            this.RenderChildren(writer);
			Logger.LogDebug("<<< Exiting RenderEditMode(writer={Writer})", writer);
        }

        /// <inheritdoc />
        protected override void RenderViewMode(System.Web.UI.HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering RenderViewMode(writer={Writer})", writer);
            string propValue = this.Page.Server.HtmlDecode(Convert.ToString(this.Value, CultureInfo.InvariantCulture));
            this.ControlStyle.AddAttributesToRender(writer);
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
            writer.Write(propValue);
            writer.RenderEndTag();
			Logger.LogDebug("<<< Exiting RenderViewMode(writer={Writer})", writer);
        }
    }
}
