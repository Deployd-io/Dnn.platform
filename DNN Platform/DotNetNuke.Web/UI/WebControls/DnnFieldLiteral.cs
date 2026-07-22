// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    /// <summary>A literal control.</summary>
    public class DnnFieldLiteral : DnnLiteral
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override void LocalizeStrings()
        {
			Logger.LogDebug(">>> Entering LocalizeStrings()");
            base.LocalizeStrings();
            this.Text = this.Text + Utilities.GetLocalizedString("FieldSuffix.Text");
			Logger.LogDebug("<<< Exiting LocalizeStrings()");
        }
    }
}
