// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    /// <summary>An image control.</summary>
    public class DnnImage : Image
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Gets or sets the icon key.</summary>
        public string IconKey { get; set; }

        /// <summary>Gets or sets the icon size.</summary>
        public string IconSize { get; set; }

        /// <summary>Gets or sets the icon style.</summary>
        public string IconStyle { get; set; }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
			Logger.LogDebug(">>> Entering OnPreRender(e={E})", e);
            base.OnPreRender(e);
            if (string.IsNullOrEmpty(this.ImageUrl))
            {
                this.ImageUrl = Entities.Icons.IconController.IconURL(this.IconKey, this.IconSize, this.IconStyle);
            }
			Logger.LogDebug("<<< Exiting OnPreRender(e={E})", e);
        }
    }
}
