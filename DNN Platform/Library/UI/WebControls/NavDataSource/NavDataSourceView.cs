// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.Web.UI;

    /// <summary>The NavDataSourceView class encapsulates the capabilities of the NavDataSource data source control.</summary>
    public class NavDataSourceView : HierarchicalDataSourceView
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NavDataSourceView));
        private readonly string sKey;

        /// <summary>Initializes a new instance of the <see cref="NavDataSourceView"/> class.</summary>
        /// <param name="viewPath">The view path by which to filter nav nodes.</param>
        public NavDataSourceView(string viewPath)
        {
			Logger.LogError(">>> Entering NavDataSourceView(viewPath={ViewPath})", viewPath);
            if (string.IsNullOrEmpty(viewPath))
            {
                this.sKey = string.Empty;
            }
            else if (viewPath.IndexOf(@"\", StringComparison.Ordinal) > -1)
            {
                this.sKey = viewPath.Substring(viewPath.LastIndexOf(@"\", StringComparison.Ordinal) + 1);
            }
            else
            {
                this.sKey = viewPath;
            }
        }

        public string Namespace { get; set; } = "MyNS";

        /// <summary>
        /// Starting with the rootNode, recursively build a list of
        /// PageInfo nodes, create PageHierarchyData
        /// objects, add them all to the PageHierarchicalEnumerable,
        /// and return the list.
        /// </summary>
        /// <returns>A collection of navigation nodes.</returns>
        public override IHierarchicalEnumerable Select()
        {
			Logger.LogError(">>> Entering Select()");
            var objPages = new NavDataPageHierarchicalEnumerable();
            DNNNodeCollection objNodes;
			Logger.LogError("Select(): objPages → {ObjPages}", objPages);
            objNodes = Navigation.GetNavigationNodes(this.Namespace);
			Logger.LogDebug("Select(): objNodes → {ObjNodes}", objNodes);
            if (!string.IsNullOrEmpty(this.sKey))
            {
                objNodes = objNodes.FindNodeByKey(this.sKey).DNNNodes;
				Logger.LogError("Select(): objNodes → {ObjNodes}", objNodes);
            }

            foreach (DNNNode objNode in objNodes)
            {
                objPages.Add(new NavDataPageHierarchyData(objNode));
            }

			Logger.LogWarning("<<< Exiting Select()");
            return objPages;
        }
    }
}
