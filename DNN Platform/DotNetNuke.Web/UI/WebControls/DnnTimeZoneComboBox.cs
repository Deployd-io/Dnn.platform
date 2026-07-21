// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    /// <summary>A time zone combobox control.</summary>
    public class DnnTimeZoneComboBox : DropDownList
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        protected override void OnInit(System.EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnInit(e={E})", e);
            // Utilities.ApplySkin(this);
            base.OnInit(e);

            this.DataTextField = "DisplayName";
			Logger.LogDebug("OnInit(e={E}): this.DataTextField → {Arg1}", e, this.DataTextField);
            this.DataValueField = "Id";

			Logger.LogDebug("OnInit(e={E}): this.DataValueField → {Arg1}", e, this.DataValueField);
            this.DataSource = TimeZoneInfo.GetSystemTimeZones();
            this.DataBind();
			Logger.LogDebug("<<< Exiting OnInit(e={E})", e);
        }
    }
}
