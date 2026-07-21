// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System.Security.Permissions;
    using System.Web;
    using System.Web.UI;

    [AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal)]
    public class NavDataSource : HierarchicalDataSourceControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        // Return a strongly typed view for the current data source control.
        private NavDataSourceView view;

        /// <inheritdoc />
        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
			Logger.LogDebug(">>> Entering GetHierarchicalView(viewPath={ViewPath})", viewPath);
            this.view = new NavDataSourceView(viewPath);
			Logger.LogDebug("GetHierarchicalView(viewPath={ViewPath}): view → {View}", viewPath, view);
			Logger.LogDebug("<<< Exiting GetHierarchicalView(viewPath={ViewPath})", viewPath);
            return this.view;
        }
    }
}
