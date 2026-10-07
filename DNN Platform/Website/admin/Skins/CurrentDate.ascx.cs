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
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(CurrentDate.ascx));
        public string CssClass { get; set; }

        public string DateFormat { get; set; }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
			Logger.LogError(">>> Entering OnInit(e={E})", e);
            base.OnInit(e);

            this.InitializeComponent();
			Logger.LogWarning("<<< Exiting OnInit(e={E})", e);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
			Logger.LogError(">>> Entering OnLoad(e={E})", e);
            base.OnLoad(e);
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.lblDate.CssClass = this.CssClass;
            }

            var user = UserController.Instance.GetCurrentUserInfo();
			Logger.LogError("OnLoad(e={E}): user → {User}", e, user);
            this.lblDate.Text = !string.IsNullOrEmpty(this.DateFormat) ? user.LocalTime().ToString(this.DateFormat) : user.LocalTime().ToLongDateString();
			Logger.LogWarning("<<< Exiting OnLoad(e={E})", e);
        }

        private void InitializeComponent()
        {
		Logger.LogError(">>> Entering InitializeComponent()");
        Logger.LogWarning("<<< Exiting InitializeComponent()");
        }
    }
}
