// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A text box control.</summary>
    public class DnnFormTextBoxItem : DnnFormItemBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private TextBox textBox;

        /// <summary>Initializes a new instance of the <see cref="DnnFormTextBoxItem"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFormTextBoxItem()
            : this(null, null)
        {
		Logger.LogDebug(">>> Entering DnnFormTextBoxItem()");
        }
		Logger.LogDebug("<<< Exiting DnnFormTextBoxItem()");

        /// <summary>Initializes a new instance of the <see cref="DnnFormTextBoxItem"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnFormTextBoxItem(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
            : base(appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(), eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>())
        {
        }

        /// <summary>Gets or sets the autocomplete type.</summary>
        public AutoCompleteType AutoCompleteType { get; set; }

        /// <summary>Gets or sets the max length.</summary>
        public int MaxLength { get; set; }

        /// <summary>Gets or sets the columns (when <see cref="TextMode"/> is <see cref="TextBoxMode.MultiLine"/>).</summary>
        public int Columns { get; set; }

        /// <summary>Gets or sets the rows (when <see cref="TextMode"/> is <see cref="TextBoxMode.MultiLine"/>).</summary>
        public int Rows { get; set; }

        /// <summary>Gets or sets the CSS class for the text box.</summary>
        public string TextBoxCssClass
        {
            get
            {
                return this.ViewState.GetValue("TextBoxCssClass", string.Empty);
            }

            set
            {
                this.ViewState.SetValue("TextBoxCssClass", value, string.Empty);
            }
        }

        /// <summary>Gets or sets the input mode.</summary>
        public TextBoxMode TextMode { get; set; }

        /// <summary>Gets or sets a value indicating whether to clear the field's value after post back when text mode set to password mode.</summary>
        public bool ClearContentInPasswordMode { get; set; }

        /// <inheritdoc />
        protected override WebControl CreateControlInternal(Control container)
        {
			Logger.LogDebug(">>> Entering CreateControlInternal(container={Container})", container);
            this.textBox = new TextBox { ID = this.ID + "_TextBox" };

			Logger.LogDebug("CreateControlInternal(container={Container}): this.textBox → {Arg1}", container, this.textBox);
			Logger.LogDebug("CreateControlInternal(container={Container}): textBox → {TextBox}", container, textBox);
            this.textBox.Rows = this.Rows;
            this.textBox.Columns = this.Columns;
            this.textBox.TextMode = this.TextMode;
            this.textBox.CssClass = this.TextBoxCssClass;
            this.textBox.AutoCompleteType = this.AutoCompleteType;
            this.textBox.TextChanged += this.TextChanged;
            this.textBox.Attributes.Add("aria-label", this.DataField);

            // Load from ControlState
            this.textBox.Text = Convert.ToString(this.Value, CultureInfo.InvariantCulture);
            if (this.TextMode == TextBoxMode.Password)
            {
                this.textBox.Attributes.Add("autocomplete", "off");
            }

            if (this.MaxLength > 0)
            {
                this.textBox.MaxLength = this.MaxLength;
            }

            container.Controls.Add(this.textBox);

			Logger.LogDebug("<<< Exiting CreateControlInternal(container={Container})", container);
            return this.textBox;
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);

            if (this.TextMode == TextBoxMode.Password && !this.ClearContentInPasswordMode)
            {
                this.textBox.Attributes.Add("value", Convert.ToString(this.Value, CultureInfo.InvariantCulture));
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }

        private void TextChanged(object sender, EventArgs e)
        {
			Logger.LogDebug(">>> Entering TextChanged(sender={Sender},e={E})", sender, e);
            this.UpdateDataSource(this.Value, this.textBox.Text, this.DataField);
			Logger.LogDebug("<<< Exiting TextChanged(sender={Sender},e={E})", sender, e);
        }
    }
}
