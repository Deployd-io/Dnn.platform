// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System.Drawing;
    using System.Web.UI.WebControls;

    public static class SizeExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SizeExtensions));
        public static Orientation Orientation(this Size size)
        {
			Logger.LogDebug(">>> Entering Orientation(size={Size})", size);
			Logger.LogDebug("<<< Exiting Orientation(size={Size})", size);
            return size.Width > size.Height
                       ? System.Web.UI.WebControls.Orientation.Horizontal
                       : System.Web.UI.WebControls.Orientation.Vertical;
        }
    }
}
