// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;

    /// <summary>A label control.</summary>
    public class DnnLabel : Label, ILocalizable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnLabel));
        private bool localize = true;

        /// <summary>Initializes a new instance of the <see cref="DnnLabel"/> class.</summary>
        public DnnLabel()
        {
			Logger.LogError(">>> Entering DnnLabel()");
            this.CssClass = "dnnFormLabel";
        }

        /// <inheritdoc />
        public bool Localize
        {
            get
            {
                return !this.DesignMode && this.localize;
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
                    var unLocalized = this.Text;

                    this.Text = Localization.GetString(unLocalized, this.LocalResourceFile);

                    if (string.IsNullOrEmpty(this.ToolTip))
                    {
                        this.ToolTip = Localization.GetString(unLocalized + ".ToolTip", this.LocalResourceFile);
                    }
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
            base.Render(writer);
			Logger.LogWarning("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
