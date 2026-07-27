// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.SearchInput
{
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;

    public class SearchInputController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public ArrayList GetSearchResultModules(int portalID)
        {
			Logger.LogDebug(">>> Entering GetSearchResultModules(portalID={PortalID})", portalID);
			Logger.LogDebug("<<< Exiting GetSearchResultModules(portalID={PortalID})", portalID);
            return CBO.FillCollection(DataProvider.Instance().GetSearchResultModules(portalID), typeof(SearchResultsModuleInfo));
        }
    }
}
