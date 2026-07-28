// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System.Collections;
    using System.Web.UI.WebControls;

    public class ListItemComparer : IComparer
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public int Compare(object x, object y)
        {
			Logger.LogDebug(">>> Entering Compare(x={X},y={Y})", x, y);
            var a = (ListItem)x;
            var b = (ListItem)y;
			Logger.LogDebug("Compare(x={X},y={Y}): a → {A}", x, y, a);
			Logger.LogDebug("Compare(x={X},y={Y}): b → {B}", x, y, b);
            var c = new CaseInsensitiveComparer();
			Logger.LogDebug("Compare(x={X},y={Y}): c → {C}", x, y, c);
			Logger.LogDebug("<<< Exiting Compare(x={X},y={Y})", x, y);
            return c.Compare(a.Text, b.Text);
        }
    }
}
