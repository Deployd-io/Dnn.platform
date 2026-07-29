// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Tabs
{
    using System;

    public class TabException : Exception
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="TabException"/> class.</summary>
        /// <param name="tabId">The ID of the tab.</param>
        /// <param name="message">The message that describes the error.</param>
        public TabException(int tabId, string message)
            : base(message)
        {
			Logger.LogDebug(">>> Entering TabException(tabId={TabId},message={Message})", tabId, message);
            this.TabId = tabId;
			Logger.LogDebug("<<< Exiting TabException(tabId={TabId},message={Message})", tabId, message);
			Logger.LogDebug("TabException(tabId={TabId},message={Message}): this.TabId → {Arg2}", tabId, message, this.TabId);
        }

        public int TabId { get; private set; }
    }
}
