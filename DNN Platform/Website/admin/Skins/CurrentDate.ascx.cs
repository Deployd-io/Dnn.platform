// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Entities.Users;

    /// <summary>A skin/theme object which displays the current date.</summary>
    public partial class CurrentDate : SkinObjectBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public string CssClass { get; set; }

        public string DateFormat { get; set; }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.InitializeComponent();
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.lblDate.CssClass = this.CssClass;
            }

            var user = UserController.Instance.GetCurrentUserInfo();
            this.lblDate.Text = !string.IsNullOrEmpty(this.DateFormat) ? user.LocalTime().ToString(this.DateFormat) : user.LocalTime().ToLongDateString();
			Logger.LogDebug("<<< Exiting OnLoad(e={E})", e);
			Logger.LogDebug("OnLoad(e={E}): user → {User}", e, user);
        }

        private void InitializeComponent()
        {
		Logger.LogDebug(">>> Entering InitializeComponent()");
		Logger.LogDebug("<<< Exiting InitializeComponent()");
        }
    }
}
