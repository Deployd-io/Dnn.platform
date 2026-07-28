// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The DualListBoxEventArgs class is a cusom EventArgs class for
    /// handling Event Args in the DualListBox.
    /// </summary>
    public class DualListBoxEventArgs : EventArgs
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>
        /// Initializes a new instance of the <see cref="DualListBoxEventArgs"/> class.
        /// Constructs a new DualListBoxEventArgs.
        /// </summary>
        /// <param name="items">The items.</param>
        public DualListBoxEventArgs(List<string> items)
        {
			Logger.LogDebug(">>> Entering DualListBoxEventArgs(items={Items})", items);
            this.Items = items;
			Logger.LogDebug("<<< Exiting DualListBoxEventArgs(items={Items})", items);
			Logger.LogDebug("DualListBoxEventArgs(items={Items}): this.Items → {Arg1}", items, this.Items);
        }

        /// <summary>Gets or sets the Items.</summary>
        /// <value>A List(Of String).</value>
        public List<string> Items { get; set; }
    }
}
