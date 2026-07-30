// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.InternalServices.Views.Search
{
    using System.Collections.Generic;

    /// <summary>DetailedView grouped by Url (TabId).</summary>
    public class GroupedDetailView
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="GroupedDetailView"/> class.</summary>
        public GroupedDetailView()
        {
			Logger.LogDebug(">>> Entering GroupedDetailView()");
            this.Results = new List<DetailedView>();
			Logger.LogDebug("<<< Exiting GroupedDetailView()");
        }

        /// <summary>Gets or sets document's Url.</summary>
        public string DocumentUrl { get; set; }

        /// <summary>Gets or sets document's Title.</summary>
        public string Title { get; set; }

        /// <summary>Gets or sets results of the Search.</summary>
        public List<DetailedView> Results { get; set; }
    }
}
