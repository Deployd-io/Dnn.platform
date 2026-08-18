// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using DotNetNuke.Common;

    /// <summary>An image edit control.</summary>
    public class DnnImageEditControl : DnnFileEditControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DnnImageEditControl));
        /// <summary>Initializes a new instance of the <see cref="DnnImageEditControl"/> class.</summary>
        public DnnImageEditControl()
        {
			Logger.LogDebug(">>> Entering DnnImageEditControl()");
            this.FileFilter = Globals.ImageFileTypes;
			Logger.LogDebug("DnnImageEditControl(): this.FileFilter → {Arg0}", this.FileFilter);
			Logger.LogDebug("<<< Exiting DnnImageEditControl()");
        }
    }
}
