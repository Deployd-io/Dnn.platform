// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;

    /// <summary>A text link control.</summary>
    public class DnnTextLink : WebControl, ILocalizable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnTextLink));
        private bool localize = true;
        private HyperLink textHyperlinkControl;

        /// <summary>Initializes a new instance of the <see cref="DnnTextLink"/> class.</summary>
        public DnnTextLink()
            : base("span")
        {
			Logger.LogError(">>> Entering DnnTextLink()");
            this.CssClass = "dnnTextLink";
			Logger.LogDebug("DnnTextLink(): this.CssClass → {Arg0}", this.CssClass);
        Logger.LogWarning("<<< Exiting DnnTextLink()");
            this.DisabledCssClass = "dnnTextLink disabled";
        }

        /// <summary>Gets or sets the text.</summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                return this.TextHyperlinkControl.Text;
            }

            set
            {
                this.TextHyperlinkControl.Text = value;
            }
        }

        /// <inheritdoc />
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public override string ToolTip
        {
            get
            {
                return this.TextHyperlinkControl.ToolTip;
            }

            set
            {
                this.TextHyperlinkControl.ToolTip = value;
            }
        }

        /// <summary>Gets or sets the URL.</summary>
        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Localizable(true)]
        public string NavigateUrl
        {
            get
            {
                return this.TextHyperlinkControl.NavigateUrl;
            }

            set
            {
                this.TextHyperlinkControl.NavigateUrl = value;
            }
        }

        /// <inheritdoc cref="HyperLink.Target" />
        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Target
        {
            get
            {
                return this.TextHyperlinkControl.Target;
            }

            set
            {
                this.TextHyperlinkControl.Target = value;
            }
        }

        /// <summary>Gets or sets the CSS class to apply when the control is disabled.</summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public new string DisabledCssClass
        {
            get
            {
                return this.ViewState["DisabledCssClass"] == null ? string.Empty : (string)this.ViewState["DisabledCssClass"];
            }

            set
            {
                this.ViewState["DisabledCssClass"] = value;
            }
        }

        /// <inheritdoc />
        public bool Localize
        {
            get
            {
                return this.localize;
            }

            set
            {
                this.localize = value;
            }
        }

        /// <inheritdoc />
        public string LocalResourceFile { get; set; }

        private HyperLink TextHyperlinkControl
        {
            get
            {
                if (this.textHyperlinkControl == null)
                {
                    this.textHyperlinkControl = new HyperLink();
                }

                return this.textHyperlinkControl;
            }
        }

        /// <inheritdoc />
        public virtual void LocalizeStrings()
        {
			Logger.LogError(">>> Entering LocalizeStrings()");
            if (this.Localize)
            {
                if (!string.IsNullOrEmpty(this.ToolTip))
                {
                    this.ToolTip = Localization.GetString(this.ToolTip, this.LocalResourceFile);
                }

                if (!string.IsNullOrEmpty(this.Text))
                {
                    this.Text = Localization.GetString(this.Text, this.LocalResourceFile);

                    if (string.IsNullOrEmpty(this.ToolTip))
                    {
                        this.ToolTip = Localization.GetString($"{this.Text}.ToolTip", this.LocalResourceFile);
                    }

                    if (string.IsNullOrEmpty(this.ToolTip))
                    {
                        this.ToolTip = this.Text;
                    }
					Logger.LogDebug("LocalizeStrings(): this.ToolTip → {Arg0}", this.ToolTip);
                }
            }
			Logger.LogWarning("<<< Exiting LocalizeStrings()");
        }

        /// <inheritdoc />
        protected override void CreateChildControls()
        {
			Logger.LogError(">>> Entering CreateChildControls()");
            this.Controls.Clear();
            this.Controls.Add(this.TextHyperlinkControl);
			Logger.LogWarning("<<< Exiting CreateChildControls()");
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogError(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
			Logger.LogWarning("<<< Exiting OnPreRender(e={E})", e);
            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogError(">>> Entering Render(writer={Writer})", writer);
            this.LocalizeStrings();

            if (!this.Enabled)
            {
                if (!string.IsNullOrEmpty(this.DisabledCssClass))
                {
                    this.CssClass = this.DisabledCssClass;
					Logger.LogDebug("Render(writer={Writer}): this.CssClass → {Arg1}", writer, this.CssClass);
                }

                this.NavigateUrl = "javascript:void(0);";
            }

            this.RenderBeginTag(writer);
            this.RenderChildren(writer);
            this.RenderEndTag(writer);
			Logger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
