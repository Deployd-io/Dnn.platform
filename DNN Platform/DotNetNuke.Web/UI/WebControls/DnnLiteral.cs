// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;

    /// <summary>A literal control.</summary>
    public class DnnLiteral : Literal, ILocalizable
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private bool localize = true;

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
			Logger.LogDebug(">>> Entering LocalizeStrings()");
            if (this.Localize)
            {
                if (!string.IsNullOrEmpty(this.Text))
                {
                    this.Text = Localization.GetString(this.Text, this.LocalResourceFile);
                }
            }
			Logger.LogDebug("<<< Exiting LocalizeStrings()");
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
			Logger.LogDebug(">>> Entering Render(writer={Writer})", writer);
            this.LocalizeStrings();
            base.Render(writer);
			Logger.LogDebug("<<< Exiting Render(writer={Writer})", writer);
        }
    }
}
