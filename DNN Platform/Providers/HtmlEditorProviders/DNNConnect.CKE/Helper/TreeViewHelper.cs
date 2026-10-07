// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;

    /// <summary>Tree view helper.</summary>
    /// <typeparam name="TIdentifier">Type of the ID.</typeparam>
    internal class TreeViewHelper<TIdentifier>
    {
        private readonly Dictionary<TIdentifier, TreeNodeWithParentId<TIdentifier>> nodes = new Dictionary<TIdentifier, TreeNodeWithParentId<TIdentifier>>();

        /// <summary>
        /// To iterate passed in data collection(treeViewData), with the passed in TreeNodesCollection(treeViewNodes) using Dictionary collection
        /// to get performance for large number of data for treeview control.
        /// </summary>
        /// <typeparam name="TData">Any object type to use for data to be used in TreeView.</typeparam>
        /// <param name="treeViewData">Data collection based on T.</param>
        /// <param name="treeViewNodes">Nodes need to refresh using Dictionary data structure.</param>
        /// <param name="getNodeId">Method to return Text used for NodeId from T based Object.</param>
        /// <param name="getParentId">Method to return Text used for ParentId from T based Object.</param>
        /// <param name="getNodeText">Method to return Text used for TreeNode from T based Object.</param>
        /// <param name="getNodeValue">Method to return test used for node value.</param>
        /// <param name="getNodeImageURL">Method to return Image used for TreeNode from T based Object.</param>
        /// <param name="parentIdCheck">Validate the parentId for T based object if parent should not be -1.</param>
        /// <param name="allLeaves"> if passed would be added as leaves to each node.</param>
        internal void LoadNodes<TData>(IEnumerable<TData> treeViewData, TreeNodeCollection treeViewNodes, Func<TData, TIdentifier> getNodeId, Func<TData, TIdentifier> getParentId, Func<TData, string> getNodeText, Func<TData, string> getNodeValue, Func<TData, string> getNodeImageURL, Func<TIdentifier, bool> parentIdCheck, Dictionary<TIdentifier, HashSet<TreeNode>> allLeaves = null)
        {
			eventLogger.LogError(">>> Entering LoadNodes(treeViewData={TreeViewData},treeViewNodes={TreeViewNodes},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL},parentIdCheck={ParentIdCheck},allLeaves={AllLeaves})", treeViewData, treeViewNodes, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, parentIdCheck, allLeaves);
            this.FillNodes(treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL);
            this.RefreshTreeViewNodes(treeViewNodes, parentIdCheck, allLeaves);
			eventLogger.LogWarning("<<< Exiting LoadNodes(treeViewData={TreeViewData},treeViewNodes={TreeViewNodes},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL},parentIdCheck={ParentIdCheck},allLeaves={AllLeaves})", treeViewData, treeViewNodes, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, parentIdCheck, allLeaves);
        }

        private void RefreshTreeViewNodes(TreeNodeCollection treeViewNodes, Func<TIdentifier, bool> parentIdCheck, Dictionary<TIdentifier, HashSet<TreeNode>> allLeaves = null)
        {
			eventLogger.LogError(">>> Entering RefreshTreeViewNodes(treeViewNodes={TreeViewNodes},parentIdCheck={ParentIdCheck},allLeaves={AllLeaves})", treeViewNodes, parentIdCheck, allLeaves);
            treeViewNodes.Clear();
            foreach (var id in this.nodes.Keys)
            {
                var data = this.GetTreeNodeWithParentId(id);
				eventLogger.LogError("RefreshTreeViewNodes(treeViewNodes={TreeViewNodes},parentIdCheck={ParentIdCheck},allLeaves={AllLeaves}): data → {Data}", treeViewNodes, parentIdCheck, allLeaves, data);
                var node = data.NodeData;
                var parentId = data.ParentId;
                if (parentIdCheck(parentId))
                {
                    var parentNodeWithId = this.GetTreeNodeWithParentId(parentId);
					eventLogger.LogError("RefreshTreeViewNodes(treeViewNodes={TreeViewNodes},parentIdCheck={ParentIdCheck},allLeaves={AllLeaves}): parentNodeWithId → {ParentNodeWithId}", treeViewNodes, parentIdCheck, allLeaves, parentNodeWithId);
                    if (parentNodeWithId == null)
                    {
                        // if node has an invalid parent (e.g. parent page is deleted), it can't be displayed in the tree
                        continue;
                    }

                    var parentNode = parentNodeWithId.NodeData;
                    parentNode.ChildNodes.Add(node);
                }
                else
                {
                    treeViewNodes.Add(node);
                }

                if (allLeaves != null && allLeaves.ContainsKey(id))
                {
                    foreach (var item in allLeaves[id])
                    {
                        node.ChildNodes.Add(item);
                    }
                }
            }
			eventLogger.LogWarning("<<< Exiting RefreshTreeViewNodes(treeViewNodes={TreeViewNodes},parentIdCheck={ParentIdCheck},allLeaves={AllLeaves})", treeViewNodes, parentIdCheck, allLeaves);
        }

        private void FillNodes<T>(
            IEnumerable<T> treeViewData,
            Func<T, TIdentifier> getNodeId,
            Func<T, TIdentifier> getParentId,
            Func<T, string> getNodeText,
            Func<T, string> getNodeValue,
            Func<T, string> getNodeImageURL)
        {
			eventLogger.LogError(">>> Entering FillNodes(treeViewData={TreeViewData},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL})", treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL);
            this.nodes.Clear();
            foreach (var item in treeViewData)
            {
                var idTab = getNodeId(item);
				eventLogger.LogError("FillNodes(treeViewData={TreeViewData},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL}): idTab → {IdTab}", treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, idTab);
                var displayName = getNodeText(item);
				eventLogger.LogError("FillNodes(treeViewData={TreeViewData},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL}): displayName → {DisplayName}", treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, displayName);
                var imageUrl = getNodeImageURL(item);
				eventLogger.LogError("FillNodes(treeViewData={TreeViewData},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL}): imageUrl → {ImageUrl}", treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, imageUrl);
                var nodeValue = getNodeValue(item);
				eventLogger.LogError("FillNodes(treeViewData={TreeViewData},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL}): nodeValue → {NodeValue}", treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, nodeValue);

                var nodeData = new TreeNodeWithParentId<TIdentifier>
                {
                    ParentId = getParentId(item),
                    NodeData = new TreeNode
                    {
                        Text = displayName,
                        Value = nodeValue,
                        ImageUrl = imageUrl,
                    },
                };
                this.nodes.Add(idTab, nodeData);
            }
			eventLogger.LogWarning("<<< Exiting FillNodes(treeViewData={TreeViewData},getNodeId={GetNodeId},getParentId={GetParentId},getNodeText={GetNodeText},getNodeValue={GetNodeValue},getNodeImageURL={GetNodeImageURL})", treeViewData, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL);
        }

        private TreeNodeWithParentId<TIdentifier> GetTreeNodeWithParentId(TIdentifier key)
        {
			eventLogger.LogError(">>> Entering GetTreeNodeWithParentId(key={Key})", key);
            eventLogger.LogWarning("<<< Exiting GetTreeNodeWithParentId(key={Key})", key);
            return this.nodes.TryGetValue(key, out var node) ? node : null;
        }

        private class TreeNodeWithParentId<TParentId>
        {
            public TParentId ParentId { get; set; }

            public TreeNode NodeData { get; set; }
        }
    }
}
