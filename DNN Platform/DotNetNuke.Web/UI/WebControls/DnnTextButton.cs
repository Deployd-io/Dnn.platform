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

    /// <summary>A text button control.</summary>
    public class DnnTextButton : LinkButton, ILocalizable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnTextButton));
        private bool localize = true;

        /// <summary>Gets or sets a confirmation message to display when the button is clicked.</summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ConfirmMessage
        {
            get
            {
                return this.ViewState["ConfirmMessage"] == null ? string.Empty : (string)this.ViewState["ConfirmMessage"];
            }

            set
            {
                this.ViewState["ConfirmMessage"] = value;
            }
        }

        /// <inheritdoc />
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public override string CssClass
        {
            get
            {
                return this.ViewState["CssClass"] == null ? string.Empty : (string)this.ViewState["CssClass"];
            }

            set
            {
                this.ViewState["CssClass"] = value;
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

        /// <summary>Gets or sets the text.</summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public new string Text
        {
            get
            {
                return this.ViewState["Text"] == null ? string.Empty : (string)this.ViewState["Text"];
            }

            set
            {
                this.ViewState["Text"] = value;
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
            if (!this.Enabled && !string.IsNullOrEmpty(this.DisabledCssClass))
            {
                this.CssClass = this.DisabledCssClass;
				Logger.LogDebug("Render(writer={Writer}): this.CssClass → {Arg1}", writer, this.CssClass);
            }

            writer.AddAttribute("class", this.CssClass.Trim());
            base.Render(writer);
			Logger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
