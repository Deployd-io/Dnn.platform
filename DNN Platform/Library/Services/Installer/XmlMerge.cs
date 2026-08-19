// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Upgrade;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>The XmlMerge class is a utility class for XmlSplicing config files.</summary>
    public class XmlMerge
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(XmlMerge));
        private IDictionary<string, XmlDocument> pendingDocuments;

        /// <summary>Initializes a new instance of the <see cref="XmlMerge"/> class.</summary>
        /// <param name="sourceFileName">The path to the XML merge file.</param>
        /// <param name="version">The version for which the merge is being done.</param>
        /// <param name="sender">The name of the component responsible for the merge.</param>
        public XmlMerge(string sourceFileName, string version, string sender)
        {
			Logger.LogDebug(">>> Entering XmlMerge(sourceFileName={SourceFileName},version={Version},sender={Sender})", sourceFileName, version, sender);
            this.Version = version;
            this.Sender = sender;
			Logger.LogDebug("XmlMerge(sourceFileName={SourceFileName},version={Version},sender={Sender}): this.Version → {Arg3}", sourceFileName, version, sender, this.Version);
			Logger.LogDebug("XmlMerge(sourceDoc={SourceDoc},version={Version},sender={Sender}): this.Sender → {Arg3}", sourceDoc, version, sender, this.Sender);
			Logger.LogDebug("XmlMerge(sourceFileName={SourceFileName},version={Version},sender={Sender}): this.Sender → {Arg3}", sourceFileName, version, sender, this.Sender);
            this.SourceConfig = new XmlDocument { XmlResolver = null };
            using var configReader = XmlReader.Create(sourceFileName, new XmlReaderSettings { XmlResolver = null, });
			Logger.LogDebug("XmlMerge(sourceFileName={SourceFileName},version={Version},sender={Sender}): configReader → {ConfigReader}", sourceFileName, version, sender, configReader);
            this.SourceConfig.Load(configReader);
			Logger.LogDebug("<<< Exiting XmlMerge(sourceFileName={SourceFileName},version={Version},sender={Sender})", sourceFileName, version, sender);
        }

        /// <summary>Initializes a new instance of the <see cref="XmlMerge"/> class.</summary>
        /// <param name="sourceStream">A stream with the source config.</param>
        /// <param name="version">The version for which the merge is being done.</param>
		Logger.LogDebug("<<< Exiting XmlMerge(sourceFileName={SourceFileName},version={Version},sender={Sender})", sourceFileName, version, sender);
        /// <param name="sender">The name of the component responsible for the merge.</param>
		Logger.LogDebug("<<< Exiting XmlMerge(sourceStream={SourceStream},version={Version},sender={Sender})", sourceStream, version, sender);
        public XmlMerge(Stream sourceStream, string version, string sender)
        {
			Logger.LogDebug(">>> Entering XmlMerge(sourceStream={SourceStream},version={Version},sender={Sender})", sourceStream, version, sender);
            this.Version = version;
            this.Sender = sender;
			Logger.LogDebug("XmlMerge(sourceStream={SourceStream},version={Version},sender={Sender}): this.Version → {Arg3}", sourceStream, version, sender, this.Version);
			Logger.LogDebug("XmlMerge(sourceStream={SourceStream},version={Version},sender={Sender}): configReader → {ConfigReader}", sourceStream, version, sender, configReader);
			Logger.LogDebug("XmlMerge(sourceStream={SourceStream},version={Version},sender={Sender}): this.Sender → {Arg3}", sourceStream, version, sender, this.Sender);
            this.SourceConfig = new XmlDocument { XmlResolver = null };
            using var configReader = XmlReader.Create(sourceStream, new XmlReaderSettings { XmlResolver = null, });
            this.SourceConfig.Load(configReader);
        }

        /// <summary>Initializes a new instance of the <see cref="XmlMerge"/> class.</summary>
        /// <param name="sourceReader">A reader with the source config.</param>
        /// <param name="version">The version for which the merge is being done.</param>
        /// <param name="sender">The name of the component responsible for the merge.</param>
        public XmlMerge(TextReader sourceReader, string version, string sender)
        {
			Logger.LogDebug(">>> Entering XmlMerge(sourceReader={SourceReader},version={Version},sender={Sender})", sourceReader, version, sender);
            this.Version = version;
            this.Sender = sender;
			Logger.LogDebug("XmlMerge(sourceReader={SourceReader},version={Version},sender={Sender}): this.Version → {Arg3}", sourceReader, version, sender, this.Version);
			Logger.LogDebug("XmlMerge(sourceReader={SourceReader},version={Version},sender={Sender}): configReader → {ConfigReader}", sourceReader, version, sender, configReader);
			Logger.LogDebug("XmlMerge(sourceReader={SourceReader},version={Version},sender={Sender}): this.Sender → {Arg3}", sourceReader, version, sender, this.Sender);
            this.SourceConfig = new XmlDocument { XmlResolver = null };
            using var configReader = XmlReader.Create(sourceReader, new XmlReaderSettings { XmlResolver = null, });
            this.SourceConfig.Load(configReader);
			Logger.LogDebug("<<< Exiting XmlMerge(sourceReader={SourceReader},version={Version},sender={Sender})", sourceReader, version, sender);
        }

        /// <summary>Initializes a new instance of the <see cref="XmlMerge"/> class.</summary>
        /// <param name="sourceDoc">A document with the source config.</param>
        /// <param name="version">The version for which the merge is being done.</param>
        /// <param name="sender">The name of the component responsible for the merge.</param>
        public XmlMerge(XmlDocument sourceDoc, string version, string sender)
        {
			Logger.LogDebug(">>> Entering XmlMerge(sourceDoc={SourceDoc},version={Version},sender={Sender})", sourceDoc, version, sender);
            this.Version = version;
            this.Sender = sender;
			Logger.LogDebug("XmlMerge(sourceDoc={SourceDoc},version={Version},sender={Sender}): this.Version → {Arg3}", sourceDoc, version, sender, this.Version);
            this.SourceConfig = sourceDoc;
        }

        public IDictionary<string, XmlDocument> PendingDocuments
        {
            get
            {
                if (this.pendingDocuments == null)
                {
                    this.pendingDocuments = new Dictionary<string, XmlDocument>();
                }

                return this.pendingDocuments;
            }
        }

        /// <summary>Gets the Source for the Config file.</summary>
        /// <value>An XmlDocument.</value>
        public XmlDocument SourceConfig { get; private set; }

        /// <summary>Gets the Sender (source) of the changes to be merged.</summary>
        /// <value>A String.</value>
        public string Sender { get; private set; }

        /// <summary>Gets or sets the Target Config file.</summary>
        /// <value>An XmlDocument.</value>
        public XmlDocument TargetConfig { get; set; }

        /// <summary>Gets the File Name of the Target Config file.</summary>
        /// <value>A String.</value>
        public string TargetFileName { get; private set; }

        /// <summary>Gets the Version of the changes to be merged.</summary>
        /// <value>A String.</value>
        public string Version { get; private set; }

        /// <summary>Gets a value indicating whether the last update performed by this instance resulted in any changes.</summary>
        /// <value><see langword="true"/> if there were changes, <see langword="false"/> if no changes were made to the target document.</value>
        public bool ConfigUpdateChangedNodes { get; private set; }

        /// <summary>
        /// The UpdateConfig method processes the source file and updates the Target
        /// Config Xml Document.
        /// </summary>
        /// <param name="target">An Xml Document represent the Target Xml File.</param>
        public void UpdateConfig(XmlDocument target)
        {
			Logger.LogDebug(">>> Entering UpdateConfig(target={Target})", target);
            var changedAnyNodes = false;
            this.TargetConfig = target;
			Logger.LogDebug("UpdateConfig(target={Target}): this.TargetConfig → {Arg1}", target, this.TargetConfig);
			Logger.LogDebug("UpdateConfig(target={Target}): changedAnyNodes → {ChangedAnyNodes}", target, changedAnyNodes);
            if (this.TargetConfig != null)
            {
                changedAnyNodes = this.ProcessNodes(this.SourceConfig.SelectNodes("/configuration/nodes/node"), false);
            }

            this.ConfigUpdateChangedNodes = changedAnyNodes;
			Logger.LogDebug("UpdateConfig(target={Target},fileName={FileName}): this.ConfigUpdateChangedNodes → {Arg2}", target, fileName, this.ConfigUpdateChangedNodes);
			Logger.LogDebug("UpdateConfig(target={Target}): this.ConfigUpdateChangedNodes → {Arg1}", target, this.ConfigUpdateChangedNodes);
			Logger.LogDebug("<<< Exiting UpdateConfig(target={Target})", target);
        }

        /// <summary>
        /// The UpdateConfig method processes the source file and updates the Target
        /// Config file.
        /// </summary>
        /// <param name="target">An Xml Document represent the Target Xml File.</param>
        /// <param name="fileName">The fileName for the Target Xml File - relative to the webroot.</param>
        public void UpdateConfig(XmlDocument target, string fileName)
        {
			Logger.LogDebug(">>> Entering UpdateConfig(target={Target},fileName={FileName})", target, fileName);
            var changedAnyNodes = false;
            this.TargetFileName = fileName;
			Logger.LogDebug("UpdateConfig(target={Target},fileName={FileName}): this.TargetFileName → {Arg2}", target, fileName, this.TargetFileName);
			Logger.LogDebug("UpdateConfig(target={Target},fileName={FileName}): changedAnyNodes → {ChangedAnyNodes}", target, fileName, changedAnyNodes);
            this.TargetConfig = target;
            if (this.TargetConfig != null)
            {
                changedAnyNodes = this.ProcessNodes(this.SourceConfig.SelectNodes("/configuration/nodes/node"), true);
            }

            this.ConfigUpdateChangedNodes = changedAnyNodes;
			Logger.LogDebug("<<< Exiting UpdateConfig(target={Target},fileName={FileName})", target, fileName);
        }

        /// <summary>
        /// The UpdateConfigs method processes the source file and updates the various config
        /// files.
        /// </summary>
        public void UpdateConfigs()
        {
			Logger.LogDebug(">>> Entering UpdateConfigs()");
			Logger.LogDebug("UpdateConfigs(autoSave={AutoSave}): this.ConfigUpdateChangedNodes → {Arg1}", autoSave, this.ConfigUpdateChangedNodes);
            this.UpdateConfigs(true);
			Logger.LogDebug("<<< Exiting UpdateConfigs()");
			Logger.LogDebug("UpdateConfigs(autoSave={AutoSave}): this.TargetFileName → {Arg1}", autoSave, this.TargetFileName);
        }

        public void UpdateConfigs(bool autoSave)
        {
            var changedAnyNodes = false;
            var nodes = this.SourceConfig.SelectNodes("/configuration/nodes");
			Logger.LogDebug("UpdateConfigs(autoSave={AutoSave}): nodes → {Nodes}", autoSave, nodes);
			Logger.LogDebug("UpdateConfigs(autoSave={AutoSave}): changedAnyNodes → {ChangedAnyNodes}", autoSave, changedAnyNodes);
            if (nodes != null)
            {
                foreach (XmlNode configNode in nodes)
                {
                    Debug.Assert(configNode.Attributes != null, "configNode.Attributes != null");

                    // Attempt to load TargetFile property from configFile Atribute
                    this.TargetFileName = configNode.Attributes["configfile"].Value;
                    string targetProductName = string.Empty;
					Logger.LogDebug("UpdateConfigs(autoSave={AutoSave}): targetProductName → {TargetProductName}", autoSave, targetProductName);
                    if (configNode.Attributes["productName"] != null)
                    {
                        targetProductName = configNode.Attributes["productName"].Value;
                    }

                    bool isAppliedToProduct;

                    if (!File.Exists(Globals.ApplicationMapPath + @"\" + this.TargetFileName))
                    {
                        DnnInstallLogger.InstallLogInfo($"Target File {this.TargetFileName} doesn't exist, ignore the merge process");
                        return;
                    }

                    this.TargetConfig = Config.Load(this.TargetFileName);
                    if (string.IsNullOrEmpty(targetProductName) || targetProductName == "All")
                    {
                        isAppliedToProduct = true;
                    }
                    else
                    {
						Logger.LogDebug("UpdateConfigs(autoSave={AutoSave}): isAppliedToProduct → {IsAppliedToProduct}", autoSave, isAppliedToProduct);
                        isAppliedToProduct = DotNetNukeContext.Current.Application.ApplyToProduct(targetProductName);
                    }

                    // The nodes definition is not correct so skip changes
                    if (this.TargetConfig != null && isAppliedToProduct)
                    {
                        var changedNodes = this.ProcessNodes(configNode.SelectNodes("node"), autoSave);
						Logger.LogDebug("UpdateConfigs(autoSave={AutoSave}): changedNodes → {ChangedNodes}", autoSave, changedNodes);
                        changedAnyNodes = changedAnyNodes || changedNodes;
                        if (!autoSave && changedNodes)
                        {
                            this.PendingDocuments.Add(this.TargetFileName, this.TargetConfig);
                        }
                    }
                }
            }

            this.ConfigUpdateChangedNodes = changedAnyNodes;
        }

        public void SavePendingConfigs()
        {
			Logger.LogDebug(">>> Entering SavePendingConfigs()");
            foreach (var key in this.PendingDocuments.Keys)
            {
                Config.Save(this.PendingDocuments[key], key);
            }
			Logger.LogDebug("<<< Exiting SavePendingConfigs()");
        }

        private static XmlNode FindNode(XmlNode root, string rootNodePath, XmlNamespaceManager nsmgr)
        {
			Logger.LogDebug(">>> Entering FindNode(root={Root},rootNodePath={RootNodePath},nsmgr={Nsmgr})", root, rootNodePath, nsmgr);
            rootNodePath = AdjustRootNodePathRelativeToLocationElements(root, rootNodePath);
			Logger.LogDebug("FindNode(root={Root},rootNodePath={RootNodePath},nsmgr={Nsmgr}): rootNodePath → {RootNodePath1}", root, rootNodePath, nsmgr, rootNodePath);
			Logger.LogDebug("<<< Exiting FindNode(root={Root},rootNodePath={RootNodePath},nsmgr={Nsmgr})", root, rootNodePath, nsmgr);
			Logger.LogDebug("<<< Exiting FindNode(root={Root},rootNodePath={RootNodePath})", root, rootNodePath);
            return root.SelectSingleNode(rootNodePath, nsmgr);
        }

        private static XmlNode FindNode(XmlNode root, string rootNodePath)
        {
            rootNodePath = AdjustRootNodePathRelativeToLocationElements(root, rootNodePath);
			Logger.LogDebug("FindNode(root={Root},rootNodePath={RootNodePath}): rootNodePath → {RootNodePath1}", root, rootNodePath, rootNodePath);
            return root.SelectSingleNode(rootNodePath);
        }

        private static string AdjustRootNodePathRelativeToLocationElements(XmlNode root, string rootNodePath)
        {
			Logger.LogDebug(">>> Entering AdjustRootNodePathRelativeToLocationElements(root={Root},rootNodePath={RootNodePath})", root, rootNodePath);
            if (root.Name != "location")
            {
                return rootNodePath;
            }

            var index = rootNodePath.IndexOf("configuration", StringComparison.OrdinalIgnoreCase);
            var adjustedPath = rootNodePath.Substring(index + "configuration".Length);
			Logger.LogDebug("AdjustRootNodePathRelativeToLocationElements(root={Root},rootNodePath={RootNodePath}): adjustedPath → {AdjustedPath}", root, rootNodePath, adjustedPath);
            adjustedPath = adjustedPath.TrimStart('/');
            if (string.IsNullOrEmpty(adjustedPath))
            {
                adjustedPath = ".";
            }

			Logger.LogDebug("<<< Exiting AdjustRootNodePathRelativeToLocationElements(root={Root},rootNodePath={RootNodePath})", root, rootNodePath);
            return adjustedPath;
        }

        private static bool RemoveAttribute(XmlNode rootNode, XmlNode actionNode)
        {
			Logger.LogDebug(">>> Entering RemoveAttribute(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");
            Debug.Assert(rootNode.Attributes != null, "rootNode.Attributes != null");

            var changedNode = false;
			Logger.LogDebug("RemoveAttribute(rootNode={RootNode},actionNode={ActionNode}): changedNode → {ChangedNode}", rootNode, actionNode, changedNode);
            if (actionNode.Attributes["name"] != null)
            {
                string attributeName = actionNode.Attributes["name"].Value;
                if (!string.IsNullOrEmpty(attributeName))
                {
                    if (rootNode.Attributes[attributeName] != null)
                    {
                        rootNode.Attributes.Remove(rootNode.Attributes[attributeName]);
                        DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveAttribute:attributeName=" + attributeName.ToString());
                        changedNode = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting RemoveAttribute(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            return changedNode;
        }

        private static bool RemoveNode(XmlNode node)
        {
			Logger.LogDebug(">>> Entering RemoveNode(node={Node})", node);
            var changedNode = false;
            if (node != null)
            {
                // Get Parent
                XmlNode parentNode = node.ParentNode;

                // Remove current Node
                if (parentNode != null)
                {
                    parentNode.RemoveChild(node);
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "RemoveNode:" + node.InnerXml.ToString());
                    changedNode = true;
                }
            }

			Logger.LogDebug("<<< Exiting RemoveNode(node={Node})", node);
            return changedNode;
        }

        private static string GetNodeContentWithoutComment(XmlNode node)
        {
			Logger.LogDebug(">>> Entering GetNodeContentWithoutComment(node={Node})", node);
            var cloneNode = node.Clone();
            RemoveCommentNodes(cloneNode);
			Logger.LogDebug("GetNodeContentWithoutComment(node={Node}): cloneNode → {CloneNode}", node, cloneNode);

			Logger.LogDebug("<<< Exiting GetNodeContentWithoutComment(node={Node})", node);
            return cloneNode.OuterXml;
        }

        private static void RemoveCommentNodes(XmlNode node)
        {
			Logger.LogDebug(">>> Entering RemoveCommentNodes(node={Node})", node);
            var commentNodes = new List<XmlNode>();
            foreach (XmlNode childNode in node.ChildNodes)
            {
			Logger.LogDebug("RemoveCommentNodes(node={Node}): commentNodes → {CommentNodes}", node, commentNodes);
                if (childNode.NodeType == XmlNodeType.Comment)
                {
                    commentNodes.Add(childNode);
                }
                else if (childNode.HasChildNodes)
                {
                    RemoveCommentNodes(childNode);
                }
            }

            if (commentNodes.Count > 0)
            {
                commentNodes.ForEach(n => { node.RemoveChild(n); });
            }
			Logger.LogDebug("<<< Exiting RemoveCommentNodes(node={Node})", node);
        }

        private bool AddNode(XmlNode rootNode, XmlNode actionNode)
        {
			Logger.LogDebug(">>> Entering AddNode(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            var changedNode = false;
            foreach (XmlNode child in actionNode.ChildNodes)
            {
			Logger.LogDebug("AddNode(rootNode={RootNode},actionNode={ActionNode}): changedNode → {ChangedNode}", rootNode, actionNode, changedNode);
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    rootNode.AppendChild(this.TargetConfig.ImportNode(child, true));
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "AddNode:" + child.InnerXml.ToString());
                    changedNode = true;
                }
            }

			Logger.LogDebug("<<< Exiting AddNode(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            return changedNode;
        }

        private bool PrependNode(XmlNode rootNode, XmlNode actionNode)
        {
			Logger.LogDebug(">>> Entering PrependNode(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            var changedNode = false;
            foreach (XmlNode child in actionNode.ChildNodes)
            {
			Logger.LogDebug("PrependNode(rootNode={RootNode},actionNode={ActionNode}): changedNode → {ChangedNode}", rootNode, actionNode, changedNode);
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    rootNode.PrependChild(this.TargetConfig.ImportNode(child, true));
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "PrependNode:" + child.InnerXml.ToString());
                    changedNode = true;
                }
            }

			Logger.LogDebug("<<< Exiting PrependNode(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            return changedNode;
        }

        private bool InsertNode(XmlNode childRootNode, XmlNode actionNode, NodeInsertType mode)
        {
			Logger.LogDebug(">>> Entering InsertNode(childRootNode={ChildRootNode},actionNode={ActionNode},mode={Mode})", childRootNode, actionNode, mode);
            XmlNode rootNode = childRootNode.ParentNode;
            Debug.Assert(rootNode != null, "Root Node was null");

            var changedNode = false;
			Logger.LogDebug("InsertNode(childRootNode={ChildRootNode},actionNode={ActionNode},mode={Mode}): changedNode → {ChangedNode}", childRootNode, actionNode, mode, changedNode);
            foreach (XmlNode child in actionNode.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element || child.NodeType == XmlNodeType.Comment)
                {
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "InsertNode:" + child.InnerXml.ToString());
                    switch (mode)
                    {
                        case NodeInsertType.Before:
                            rootNode.InsertBefore(this.TargetConfig.ImportNode(child, true), childRootNode);
                            changedNode = true;
                            break;
                        case NodeInsertType.After:
                            rootNode.InsertAfter(this.TargetConfig.ImportNode(child, true), childRootNode);
                            changedNode = true;
                            break;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting InsertNode(childRootNode={ChildRootNode},actionNode={ActionNode},mode={Mode})", childRootNode, actionNode, mode);
            return changedNode;
        }

        private bool ProcessNode(XmlNode node, XmlNode targetRoot)
        {
			Logger.LogDebug(">>> Entering ProcessNode(node={Node},targetRoot={TargetRoot})", node, targetRoot);
            Debug.Assert(node.Attributes != null, "node.Attributes != null");

            XmlNode rootNode = this.FindMatchingNode(targetRoot, node, "path");
			Logger.LogDebug("ProcessNode(node={Node},targetRoot={TargetRoot}): rootNode → {RootNode}", node, targetRoot, rootNode);

            string nodeAction = node.Attributes["action"].Value.ToLowerInvariant();

            if (rootNode == null)
            {
                return false; // not every TargetRoot will contain every target node
            }

            switch (nodeAction)
            {
                case "add":
                    return this.AddNode(rootNode, node);
                case "prepend":
                    return this.PrependNode(rootNode, node);
                case "insertbefore":
                    return this.InsertNode(rootNode, node, NodeInsertType.Before);
                case "insertafter":
                    return this.InsertNode(rootNode, node, NodeInsertType.After);
                case "remove":
                    return RemoveNode(rootNode);
                case "removeattribute":
                    return RemoveAttribute(rootNode, node);
                case "update":
                    return this.UpdateNode(rootNode, node);
                case "updateattribute":
                    return this.UpdateAttribute(rootNode, node);
                default:
					Logger.LogDebug("<<< Exiting ProcessNode(node={Node},targetRoot={TargetRoot})", node, targetRoot);
                    return false;
            }
        }

        private bool ProcessNodes(XmlNodeList nodes, bool saveConfig)
        {
			Logger.LogDebug(">>> Entering ProcessNodes(nodes={Nodes},saveConfig={SaveConfig})", nodes, saveConfig);
            var changedNodes = false;

			Logger.LogDebug("ProcessNodes(nodes={Nodes},saveConfig={SaveConfig}): changedNodes → {ChangedNodes}", nodes, saveConfig, changedNodes);
            // The nodes definition is not correct so skip changes
            if (this.TargetConfig != null)
            {
                // in web.config it is possible to add <location> nodes that contain nodes that would
                // otherwise be in the root <configuration> node, therefore some files can have multiple roots
                // making it tricky to decide where to apply the xml merge operations
                var targetRoots = this.GetTargetRoots().ToList();
				Logger.LogDebug("ProcessNodes(nodes={Nodes},saveConfig={SaveConfig}): targetRoots → {TargetRoots}", nodes, saveConfig, targetRoots);

                if (targetRoots.Count == 1)
                {
                    var root = targetRoots[0];
                    foreach (XmlNode node in nodes)
                    {
                        changedNodes = this.ProcessNode(node, root) || changedNodes;
                    }
                }
                else
                {
                    foreach (XmlNode node in nodes)
                    {
                        var hits = this.FindMatchingNodes(node, targetRoots, "path").ToList();
						Logger.LogDebug("ProcessNodes(nodes={Nodes},saveConfig={SaveConfig}): hits → {Hits}", nodes, saveConfig, hits);

                        if (hits.Count == 0)
                        {
                            continue;
                        }

                        if (hits.Count == 1)
                        {
                            changedNodes = this.ProcessNode(node, hits[0]) || changedNodes;
                        }
                        else if (hits.Count < targetRoots.Count)
                        {
                            changedNodes = this.ProcessNode(node, hits[0]) || changedNodes;
                        }
                        else
                        {
                            // hit on all roots
                            XmlNode hit = this.FindMatchingNodes(node, hits, "targetpath").FirstOrDefault();
							Logger.LogDebug("ProcessNodes(nodes={Nodes},saveConfig={SaveConfig}): hit → {Hit}", nodes, saveConfig, hit);
                            if (hit != null)
                            {
                                changedNodes = this.ProcessNode(node, hit) || changedNodes;
                            }
                            else
                            {
                                // all paths match at root level but no targetpaths match below that so default to the initial root
                                changedNodes = this.ProcessNode(node, hits[0]) || changedNodes;
                            }
                        }
                    }
                }

                if (saveConfig && changedNodes)
                {
                    Config.Save(this.TargetConfig, this.TargetFileName);
                }
            }

			Logger.LogDebug("<<< Exiting ProcessNodes(nodes={Nodes},saveConfig={SaveConfig})", nodes, saveConfig);
            return changedNodes;
        }

        private IEnumerable<XmlNode> FindMatchingNodes(XmlNode mergeNode, IEnumerable<XmlNode> rootNodes, string pathAttributeName)
        {
			Logger.LogDebug(">>> Entering FindMatchingNodes(mergeNode={MergeNode},rootNodes={RootNodes},pathAttributeName={PathAttributeName})", mergeNode, rootNodes, pathAttributeName);
            foreach (var targetRoot in rootNodes)
            {
                var rootNode = this.FindMatchingNode(targetRoot, mergeNode, pathAttributeName);
				Logger.LogDebug("FindMatchingNodes(mergeNode={MergeNode},rootNodes={RootNodes},pathAttributeName={PathAttributeName}): rootNode → {RootNode}", mergeNode, rootNodes, pathAttributeName, rootNode);

                if (rootNode != null)
                {
                    yield return targetRoot;
                }
            }
			Logger.LogDebug("<<< Exiting FindMatchingNodes(mergeNode={MergeNode},rootNodes={RootNodes},pathAttributeName={PathAttributeName})", mergeNode, rootNodes, pathAttributeName);
        }

        private XmlNode FindMatchingNode(XmlNode rootNode, XmlNode mergeNode, string pathAttributeName)
        {
			Logger.LogDebug(">>> Entering FindMatchingNode(rootNode={RootNode},mergeNode={MergeNode},pathAttributeName={PathAttributeName})", rootNode, mergeNode, pathAttributeName);
            Debug.Assert(mergeNode.Attributes != null, "Attributes collection was null");

            XmlNode matchingNode = null;
			Logger.LogDebug("FindMatchingNode(rootNode={RootNode},mergeNode={MergeNode},pathAttributeName={PathAttributeName}): matchingNode → {MatchingNode}", rootNode, mergeNode, pathAttributeName, matchingNode);
            if (mergeNode.Attributes[pathAttributeName] != null)
            {
                string rootNodePath = mergeNode.Attributes[pathAttributeName].Value;
                if (mergeNode.Attributes["nameSpace"] == null)
                {
                    matchingNode = FindNode(rootNode, rootNodePath);
                }
                else
                {
                    // Use Namespace Manager
                    string xmlNameSpace = mergeNode.Attributes["nameSpace"].Value;
                    string xmlNameSpacePrefix = mergeNode.Attributes["nameSpacePrefix"].Value;
                    var nsmgr = new XmlNamespaceManager(this.TargetConfig.NameTable);
					Logger.LogDebug("FindMatchingNode(rootNode={RootNode},mergeNode={MergeNode},pathAttributeName={PathAttributeName}): nsmgr → {Nsmgr}", rootNode, mergeNode, pathAttributeName, nsmgr);
                    nsmgr.AddNamespace(xmlNameSpacePrefix, xmlNameSpace);
                    matchingNode = FindNode(rootNode, rootNodePath, nsmgr);
                }
            }

			Logger.LogDebug("<<< Exiting FindMatchingNode(rootNode={RootNode},mergeNode={MergeNode},pathAttributeName={PathAttributeName})", rootNode, mergeNode, pathAttributeName);
            return matchingNode;
        }

        private IEnumerable<XmlNode> GetTargetRoots()
        {
			Logger.LogDebug(">>> Entering GetTargetRoots()");
            yield return this.TargetConfig.DocumentElement;

            var locations = this.TargetConfig.SelectNodes("/configuration/location");
			Logger.LogDebug("GetTargetRoots(): locations → {Locations}", locations);
            if (locations != null)
            {
                foreach (XmlNode node in locations)
                {
                    yield return node;
                }
            }
			Logger.LogDebug("<<< Exiting GetTargetRoots()");
        }

        private bool UpdateAttribute(XmlNode rootNode, XmlNode actionNode)
        {
			Logger.LogDebug(">>> Entering UpdateAttribute(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");
            Debug.Assert(rootNode.Attributes != null, "rootNode.Attributes != null");

            var changedNode = false;
			Logger.LogDebug("UpdateAttribute(rootNode={RootNode},actionNode={ActionNode}): changedNode → {ChangedNode}", rootNode, actionNode, changedNode);
            if (actionNode.Attributes["name"] != null && actionNode.Attributes["value"] != null)
            {
                string attributeName = actionNode.Attributes["name"].Value;
                string attributeValue = actionNode.Attributes["value"].Value;
                if (!string.IsNullOrEmpty(attributeName))
                {
                    DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateAttribute:attributeName=" + attributeName.ToString());
                    if (rootNode.Attributes[attributeName] == null)
                    {
                        rootNode.Attributes.Append(this.TargetConfig.CreateAttribute(attributeName));
                        changedNode = true;
                    }

                    var oldAttributeValue = rootNode.Attributes[attributeName].Value;
                    rootNode.Attributes[attributeName].Value = attributeValue;
                    if (!string.Equals(oldAttributeValue, attributeValue, StringComparison.Ordinal))
                    {
                        changedNode = true;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting UpdateAttribute(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            return changedNode;
        }

        private bool UpdateNode(XmlNode rootNode, XmlNode actionNode)
        {
			Logger.LogDebug(">>> Entering UpdateNode(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            Debug.Assert(actionNode.Attributes != null, "actionNode.Attributes != null");

            var changedNode = false;
			Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): changedNode → {ChangedNode}", rootNode, actionNode, changedNode);
            string keyAttribute = string.Empty;
			Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): keyAttribute → {KeyAttribute}", rootNode, actionNode, keyAttribute);
            if (actionNode.Attributes["key"] != null)
            {
                keyAttribute = actionNode.Attributes["key"].Value;
                DnnInstallLogger.InstallLogInfo(Localization.GetString("LogStart", Localization.GlobalResourceFile) + "UpdateNode: keyAttribute=" + keyAttribute.ToString());
            }

            foreach (XmlNode child in actionNode.ChildNodes)
            {
                Debug.Assert(child.Attributes != null, "child.Attributes != null");

                if (child.NodeType == XmlNodeType.Element)
                {
                    XmlNode targetNode = null;
					Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): targetNode → {TargetNode}", rootNode, actionNode, targetNode);
                    if (!string.IsNullOrEmpty(keyAttribute))
                    {
                        if (child.Attributes[keyAttribute] != null)
                        {
                            var matchingChildren = rootNode.SelectNodes(child.LocalName);
							Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): matchingChildren → {MatchingChildren}", rootNode, actionNode, matchingChildren);
                            if (matchingChildren is not null)
                            {
                                var keyValue = child.Attributes[keyAttribute].Value;
                                foreach (XmlNode node in matchingChildren)
                                {
                                    if (node.Attributes?[keyAttribute]?.Value == keyValue)
                                    {
                                        targetNode = node;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        targetNode = this.FindMatchingNode(rootNode, actionNode, "targetpath");
                    }

                    if (targetNode == null)
                    {
                        // Since there is no collision we can just add the node
                        rootNode.AppendChild(this.TargetConfig.ImportNode(child, true));
                        changedNode = true;
                        continue;
                    }

                    // There is a collision so we need to determine what to do.
                    string collisionAction = actionNode.Attributes["collision"].Value;
                    switch (collisionAction.ToLowerInvariant())
                    {
                        case "overwrite":
                            var oldContent = rootNode.InnerXml;
                            rootNode.ReplaceChild(this.TargetConfig.ImportNode(child, true), targetNode);
                            var newContent = rootNode.InnerXml;
                            changedNode = !string.Equals(oldContent, newContent, StringComparison.Ordinal);
                            break;
                        case "save":
                            var newChild = this.TargetConfig.ImportNode(child, true);
							Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): newChild → {NewChild}", rootNode, actionNode, newChild);
                            var targetXElement = XElement.Load(targetNode.CreateNavigator().ReadSubtree());
							Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): targetXElement → {TargetXElement}", rootNode, actionNode, targetXElement);
                            var newXElement = XElement.Load(newChild.CreateNavigator().ReadSubtree());
							Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): newXElement → {NewXElement}", rootNode, actionNode, newXElement);

                            if (XElement.DeepEquals(targetXElement, newXElement))
                            {
                                break;
                            }

                            string commentHeaderText = string.Format(
                                CultureInfo.InvariantCulture,
                                Localization.GetString("XMLMERGE_Upgrade", Localization.SharedResourceFile),
                                Environment.NewLine,
                                this.Sender,
                                this.Version,
                                DateTime.Now);
                            XmlComment commentHeader = this.TargetConfig.CreateComment(commentHeaderText);
							Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): commentHeader → {CommentHeader}", rootNode, actionNode, commentHeader);
                            var targetNodeContent = GetNodeContentWithoutComment(targetNode);
							Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): targetNodeContent → {TargetNodeContent}", rootNode, actionNode, targetNodeContent);
                            XmlComment commentNode = this.TargetConfig.CreateComment(targetNodeContent);
							Logger.LogDebug("UpdateNode(rootNode={RootNode},actionNode={ActionNode}): commentNode → {CommentNode}", rootNode, actionNode, commentNode);
                            rootNode.ReplaceChild(newChild, targetNode);
                            rootNode.InsertBefore(commentHeader, newChild);
                            rootNode.InsertBefore(commentNode, newChild);
                            changedNode = true;
                            break;
                        case "ignore":
                            break;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting UpdateNode(rootNode={RootNode},actionNode={ActionNode})", rootNode, actionNode);
            return changedNode;
        }
    }
}
