// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.UI.WebControls;
    using DotNetNuke.Web.DDRMenu.DNNCommon;

    /// <summary>Represents a single menu node.</summary>
    [Serializable]
    [XmlRoot("root", Namespace = "")]
    public class MenuNode : IXmlSerializable
    {
        private List<MenuNode> children;

        /// <summary>Initializes a new instance of the <see cref="MenuNode"/> class.</summary>
        public MenuNode()
        {
		eventLogger.LogDebug(">>> Entering MenuNode()");
		eventLogger.LogDebug("<<< Exiting MenuNode()");
        }

        /// <summary>Initializes a new instance of the <see cref="MenuNode"/> class from existing nodes.</summary>
        /// <param name="dnnNodes">The collection of Dnn nodes.</param>
        public MenuNode(DNNNodeCollection dnnNodes)
        {
			eventLogger.LogDebug(">>> Entering MenuNode(dnnNodes={DnnNodes})", dnnNodes);
            this.Children = ConvertDNNNodeCollection(dnnNodes, this);
			eventLogger.LogDebug("<<< Exiting MenuNode(dnnNodes={DnnNodes})", dnnNodes);
        }

        /// <summary>Initializes a new instance of the <see cref="MenuNode"/> class from existing nodes.</summary>
        /// <param name="nodes">A list of <see cref="MenuNode"/>.</param>
        public MenuNode(List<MenuNode> nodes)
        {
			eventLogger.LogDebug(">>> Entering MenuNode(nodes={Nodes})", nodes);
            this.Children = nodes;
            this.Children.ForEach(c => c.Parent = this);
			eventLogger.LogDebug("MenuNode(nodes={Nodes}): this.Children → {Arg1}", nodes, this.Children);
			eventLogger.LogDebug("<<< Exiting MenuNode(nodes={Nodes})", nodes);
        }

        /// <summary>Initializes a new instance of the <see cref="MenuNode"/> class from an existing node and a parent.</summary>
        /// <param name="dnnNode">The <see cref="DNNNode"/> to initialize from.</param>
        /// <param name="parent">The parent <see cref="MenuNode"/>.</param>
        public MenuNode(DNNNode dnnNode, MenuNode parent)
        {
            this.TabId = Convert.ToInt32(dnnNode.ID);
            this.Text = dnnNode.Text;
            this.Url = (dnnNode.ClickAction == eClickAction.PostBack)
                    ? "postback:" + dnnNode.ID
                    : string.IsNullOrEmpty(dnnNode.JSFunction) ? dnnNode.NavigateURL : "javascript:" + dnnNode.JSFunction;
					eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Text → {Arg2}", dnnNode, parent, this.Text);
            this.Enabled = dnnNode.Enabled;
            this.Selected = dnnNode.Selected;
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Enabled → {Arg2}", dnnNode, parent, this.Enabled);
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Selected → {Arg2}", dnnNode, parent, this.Selected);
            this.Breadcrumb = dnnNode.BreadCrumb;
            this.Separator = dnnNode.IsBreak;
            this.Icon = dnnNode.Image;
            this.Target = dnnNode.Target;
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Breadcrumb → {Arg2}", dnnNode, parent, this.Breadcrumb);
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Separator → {Arg2}", dnnNode, parent, this.Separator);
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Icon → {Arg2}", dnnNode, parent, this.Icon);
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Target → {Arg2}", dnnNode, parent, this.Target);
            this.Title = null;
            this.Keywords = null;
            this.Description = null;
            this.Parent = parent;
            this.CommandName = dnnNode.get_CustomAttribute("CommandName");
            this.CommandArgument = dnnNode.get_CustomAttribute("CommandArgument");

            DNNAbstract.DNNNodeToMenuNode(dnnNode, this);
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Title → {Arg2}", dnnNode, parent, this.Title);
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Keywords → {Arg2}", dnnNode, parent, this.Keywords);
			eventLogger.LogDebug("MenuNode(dnnNode={DnnNode},parent={Parent}): this.Description → {Arg2}", dnnNode, parent, this.Description);

            if ((dnnNode.DNNNodes != null) && (dnnNode.DNNNodes.Count > 0))
            {
                this.Children = ConvertDNNNodeCollection(dnnNode.DNNNodes, this);
            }
        }

        /// <summary>Gets a value indicating whether this node is the first of it's level.</summary>
        public bool First
        {
            get { return (this.Parent == null) || (this.Parent.Children[0] == this); }
        }

        /// <summary>Gets a value indicating whether this node is the last of it's level.</summary>
        public bool Last
        {
            get { return (this.Parent == null) || (this.Parent.Children[this.Parent.Children.Count - 1] == this); }
        }

        /// <summary>Gets a value indicating how deep this node is in the nodes hierarchy.</summary>
        public int Depth
        {
            get
            {
                var result = -1;
                var current = this;
                while (current.Parent != null)
                {
                    result++;
                    current = current.Parent;
                }

                return result;
            }
        }

        /// <summary>Gets or sets the id of the tab (page) for this node.</summary>
        public int TabId { get; set; }

        /// <summary>Gets or sets the text of the node.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets the title of this node.</summary>
        public string Title { get; set; }

        /// <summary>Gets or sets the url of this node.</summary>
        public string Url { get; set; }

        /// <summary>Gets or sets a value indicating whether this node is enabled.</summary>
        public bool Enabled { get; set; }

        /// <summary>Gets or sets a value indicating whether this node is selected.</summary>
        public bool Selected { get; set; }

        /// <summary>Gets or sets a value indicating whether this node is included in the breadcrumb.</summary>
        public bool Breadcrumb { get; set; }

        /// <summary>Gets or sets a value indicating whether this node is a separator.</summary>
        public bool Separator { get; set; }

        /// <summary>Gets or sets the icon for this node.</summary>
        public string Icon { get; set; }

        /// <summary>Gets or sets the large image for this node.</summary>
        public string LargeImage { get; set; }

        /// <summary>Gets or sets the command name for this node.</summary>
        public string CommandName { get; set; }

        /// <summary>Gets or sets the command argument for this node.</summary>
        public string CommandArgument { get; set; }

        /// <summary>Gets or sets the html target for this node.</summary>
        public string Target { get; set; }

        /// <summary>Gets or sets the keywords for this node.</summary>
        public string Keywords { get; set; }

        /// <summary>Gets or sets the description for this node.</summary>
        public string Description { get; set; }

        /// <summary>Gets or sets the childrens of this node.</summary>
        public List<MenuNode> Children
        {
            get { return this.children ?? (this.children = new List<MenuNode>()); }
            set { this.children = value; }
        }

        /// <summary>Gets or sets the parent node for this node.</summary>
        public MenuNode Parent { get; set; }

        /// <summary>Converts a <see cref="DNNNodeCollection"/> into a <see cref="List{MenuNode}"/>.</summary>
        /// <param name="dnnNodes">The DNNNodeCollection to convert.</param>
        /// <param name="parent">The parent node in which to place the nodes.</param>
        /// <returns><see cref="List{MenuNode}"/>.</returns>
        public static List<MenuNode> ConvertDNNNodeCollection(DNNNodeCollection dnnNodes, MenuNode parent)
        {
			eventLogger.LogDebug(">>> Entering ConvertDNNNodeCollection(dnnNodes={DnnNodes},parent={Parent})", dnnNodes, parent);
            var result = new List<MenuNode>();
            foreach (DNNNode node in dnnNodes)
            {
			eventLogger.LogDebug("ConvertDNNNodeCollection(dnnNodes={DnnNodes},parent={Parent}): result → {Result}", dnnNodes, parent, result);
                result.Add(new MenuNode(node, parent));
            }

			eventLogger.LogDebug("<<< Exiting ConvertDNNNodeCollection(dnnNodes={DnnNodes},parent={Parent})", dnnNodes, parent);
            return result;
        }

        /// <summary>Finds a node by a provided tab (page) id.</summary>
        /// <param name="tabId">The id of the tab (page).</param>
        /// <returns>The found  <see cref="MenuNode"/> or null.</returns>
        public MenuNode FindById(int tabId)
        {
			eventLogger.LogDebug(">>> Entering FindById(tabId={TabId})", tabId);
            if (tabId == this.TabId)
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var result = child.FindById(tabId);
				eventLogger.LogDebug("FindById(tabId={TabId}): result → {Result}", tabId, result);
                if (result != null)
                {
                    return result;
                }
            }

			eventLogger.LogDebug("<<< Exiting FindById(tabId={TabId})", tabId);
            return null;
        }

        /// <summary>Finds a node by the tab (page) name.</summary>
        /// <param name="tabName">The name of the tab (page).</param>
        /// <returns>A <see cref="MenuNode"/> or null.</returns>
        public MenuNode FindByName(string tabName)
        {
			eventLogger.LogDebug(">>> Entering FindByName(tabName={TabName})", tabName);
            if (tabName.Equals(this.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var result = child.FindByName(tabName);
				eventLogger.LogDebug("FindByName(tabName={TabName}): result → {Result}", tabName, result);
                if (result != null)
                {
                    return result;
                }
            }

			eventLogger.LogDebug("<<< Exiting FindByName(tabName={TabName})", tabName);
            return null;
        }

        /// <summary>Flattens the hierarchy of the children nodes.</summary>
        /// <param name="root">The root menu node.</param>
        /// <returns>A new flattened list of <see cref="MenuNode"/>.</returns>
        public List<MenuNode> FlattenChildren(MenuNode root)
        {
			eventLogger.LogDebug(">>> Entering FlattenChildren(root={Root})", root);
            var flattened = new List<MenuNode>();
            if (root.TabId != 0)
			eventLogger.LogDebug("FlattenChildren(root={Root}): flattened → {Flattened}", root, flattened);
            {
                flattened.Add(root);
            }

            var children = root.Children;

            if (children != null)
            {
                foreach (var child in children)
                {
                    flattened.AddRange(this.FlattenChildren(child));
                }
            }

			eventLogger.LogDebug("<<< Exiting FlattenChildren(root={Root})", root);
            return flattened;
        }

        /// <summary>Finds a menu node either by it's name or it's id.</summary>
        /// <param name="tabNameOrId">A string representing either the tab (page) name or id.</param>
        /// <returns>The found <see cref="MenuNode"/> or null.</returns>
        public MenuNode FindByNameOrId(string tabNameOrId)
        {
			eventLogger.LogDebug(">>> Entering FindByNameOrId(tabNameOrId={TabNameOrId})", tabNameOrId);
            if (tabNameOrId.Equals(this.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return this;
            }

            if (tabNameOrId == this.TabId.ToString())
            {
                return this;
            }

            foreach (var child in this.Children)
            {
                var result = child.FindByNameOrId(tabNameOrId);
				eventLogger.LogDebug("FindByNameOrId(tabNameOrId={TabNameOrId}): result → {Result}", tabNameOrId, result);
                if (result != null)
                {
                    return result;
                }
            }

			eventLogger.LogDebug("<<< Exiting FindByNameOrId(tabNameOrId={TabNameOrId})", tabNameOrId);
            return null;
        }

        /// <summary>
        /// Finds all menu nodes in the hierarchy with matching name or id.
        /// </summary>
        /// <param name="tabNameOrId">A string representing either the tab (page) name or id.</param>
        /// <returns>
        /// A <see cref="IEnumerable{MenuNode}"/> instance containing all menu nodes found.
        /// </returns>
        public IEnumerable<MenuNode> FindAllByNameOrId(string tabNameOrId)
        {
			eventLogger.LogDebug(">>> Entering FindAllByNameOrId(tabNameOrId={TabNameOrId})", tabNameOrId);
            foreach (var found in this.Children.SelectMany(child => child.FindAllByNameOrId(tabNameOrId)))
            {
                yield return found;
            }

            if (tabNameOrId.Equals(this.Text, StringComparison.InvariantCultureIgnoreCase)
                || tabNameOrId == this.TabId.ToString())
            {
                yield return this;
            }
			eventLogger.LogDebug("<<< Exiting FindAllByNameOrId(tabNameOrId={TabNameOrId})", tabNameOrId);
        }

        /// <summary>Checks if this node has childrens.</summary>
        /// <returns>A value indicating whether this node has childrens.</returns>
        public bool HasChildren()
        {
			eventLogger.LogDebug(">>> Entering HasChildren()");
			eventLogger.LogDebug("<<< Exiting HasChildren()");
            return this.Children.Count > 0;
        }

        /// <inheritdoc />
        public XmlSchema GetSchema()
        {
			eventLogger.LogDebug(">>> Entering GetSchema()");
			eventLogger.LogDebug("<<< Exiting GetSchema()");
            return null;
        }

        /// <inheritdoc />
        public void ReadXml(XmlReader reader)
        {
			eventLogger.LogDebug(">>> Entering ReadXml(reader={Reader})", reader);
            var empty = reader.IsEmptyElement;

            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "id":
                            this.TabId = Convert.ToInt32(reader.Value);
                            break;
                        case "text":
                            this.Text = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.Text → {Arg1}", reader, this.Text);
                            break;
                        case "title":
                            this.Title = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.Title → {Arg1}", reader, this.Title);
                            break;
                        case "url":
                            this.Url = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.Url → {Arg1}", reader, this.Url);
                            break;
                        case "enabled":
                            this.Enabled = reader.Value == "1";
                            break;
                        case "selected":
                            this.Selected = reader.Value == "1";
                            break;
                        case "breadcrumb":
                            this.Breadcrumb = reader.Value == "1";
                            break;
                        case "separator":
                            this.Separator = reader.Value == "1";
                            break;
                        case "icon":
                            this.Icon = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.Icon → {Arg1}", reader, this.Icon);
                            break;
                        case "largeimage":
                            this.LargeImage = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.LargeImage → {Arg1}", reader, this.LargeImage);
                            break;
                        case "commandname":
                            this.CommandName = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.CommandName → {Arg1}", reader, this.CommandName);
                            break;
                        case "commandargument":
                            this.CommandArgument = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.CommandArgument → {Arg1}", reader, this.CommandArgument);
                            break;
                        case "target":
                            this.Target = reader.Value;
							eventLogger.LogDebug("ReadXml(reader={Reader}): this.Target → {Arg1}", reader, this.Target);
                            break;

                            // default:
                            //    throw new XmlException(String.Format("Unexpected attribute '{0}'", reader.Name));
                    }
                }
                while (reader.MoveToNextAttribute());
            }

            if (empty)
            {
                return;
            }

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name.ToLowerInvariant())
                        {
                            case "node":
                                var child = new MenuNode { Parent = this };
                                child.ReadXml(reader);
                                this.Children.Add(child);
                                break;
                            case "keywords":
                                this.Keywords = reader.ReadElementContentAsString().Trim();
                                break;
                            case "description":
                                this.Description = reader.ReadElementContentAsString().Trim();
                                break;
                            case "root":
                                break;
                            default:
								eventLogger.LogWarning("Throwing XmlException from ReadXml(reader={Reader})", reader);
                                throw new XmlException(string.Format("Unexpected element '{0}'", reader.Name));
                        }

                        break;
                    case XmlNodeType.EndElement:
                        reader.ReadEndElement();
						eventLogger.LogDebug("<<< Exiting ReadXml(reader={Reader})", reader);
                        return;
                }
            }
        }

        /// <inheritdoc />
        public void WriteXml(XmlWriter writer)
        {
			eventLogger.LogDebug(">>> Entering WriteXml(writer={Writer})", writer);
            if (this.Parent != null)
            {
                writer.WriteStartElement("node");

                AddXmlAttribute(writer, "id", this.TabId);
                AddXmlAttribute(writer, "text", this.Text);
                AddXmlAttribute(writer, "title", this.Title);
                AddXmlAttribute(writer, "url", this.Url);
                AddXmlAttribute(writer, "enabled", this.Enabled);
                AddXmlAttribute(writer, "selected", this.Selected);
                AddXmlAttribute(writer, "breadcrumb", this.Breadcrumb);
                AddXmlAttribute(writer, "separator", this.Separator);
                AddXmlAttribute(writer, "target", this.Target);
                AddXmlAttribute(writer, "icon", this.Icon);
                AddXmlAttribute(writer, "largeimage", this.LargeImage);
                AddXmlAttribute(writer, "commandname", this.CommandName);
                AddXmlAttribute(writer, "commandargument", this.CommandArgument);
                AddXmlAttribute(writer, "first", this.First);
                AddXmlAttribute(writer, "last", this.Last);
                AddXmlAttribute(writer, "only", this.First && this.Last);
                AddXmlAttribute(writer, "depth", this.Depth);
                AddXmlElement(writer, "keywords", this.Keywords);
                AddXmlElement(writer, "description", this.Description);
            }

            this.Children.ForEach(c => c.WriteXml(writer));

            if (this.Parent != null)
            {
                writer.WriteEndElement();
            }
			eventLogger.LogDebug("<<< Exiting WriteXml(writer={Writer})", writer);
        }

        /// <summary>Removes all the provided nodes.</summary>
        /// <param name="filteredNodes">The list of nodes to remove.</param>
        internal void RemoveAll(List<MenuNode> filteredNodes)
        {
			eventLogger.LogDebug(">>> Entering RemoveAll(filteredNodes={FilteredNodes})", filteredNodes);
            this.Children.RemoveAll(filteredNodes.Contains);
            foreach (var child in this.Children)
            {
                child.RemoveAll(filteredNodes);
            }
			eventLogger.LogDebug("<<< Exiting RemoveAll(filteredNodes={FilteredNodes})", filteredNodes);
        }

        /// <summary>Applies context to the node for paths and urls.</summary>
        /// <param name="defaultImagePath">The default location for images.</param>
        internal void ApplyContext(string defaultImagePath)
        {
			eventLogger.LogDebug(">>> Entering ApplyContext(defaultImagePath={DefaultImagePath})", defaultImagePath);
            this.Icon = this.ApplyContextToImagePath(this.Icon, defaultImagePath);
            this.LargeImage = this.ApplyContextToImagePath(this.LargeImage, defaultImagePath);

            if (this.Url != null && this.Url.StartsWith("postback:"))
            {
                var postbackControl = DNNContext.Current.HostControl;
                this.Url = postbackControl.Page.ClientScript.GetPostBackClientHyperlink(postbackControl, this.Url.Substring(9));
            }

            this.Children.ForEach(c => c.ApplyContext(defaultImagePath));
			eventLogger.LogDebug("<<< Exiting ApplyContext(defaultImagePath={DefaultImagePath})", defaultImagePath);
        }

        private static void AddXmlAttribute(XmlWriter writer, string name, string value)
        {
			eventLogger.LogDebug(">>> Entering AddXmlAttribute(writer={Writer},name={Name},value={Value})", writer, name, value);
            if (!string.IsNullOrEmpty(value))
            {
                writer.WriteAttributeString(name, value);
            }
			eventLogger.LogDebug("<<< Exiting AddXmlAttribute(writer={Writer},name={Name},value={Value})", writer, name, value);
        }

        private static void AddXmlAttribute(XmlWriter writer, string name, int value)
        {
            writer.WriteAttributeString(name, value.ToString());
        }

        private static void AddXmlAttribute(XmlWriter writer, string name, bool value)
        {
            writer.WriteAttributeString(name, value ? "1" : "0");
        }

        private static void AddXmlElement(XmlWriter writer, string name, string value)
        {
			eventLogger.LogDebug(">>> Entering AddXmlElement(writer={Writer},name={Name},value={Value})", writer, name, value);
            if (!string.IsNullOrEmpty(value))
            {
                writer.WriteElementString(name, value);
            }
			eventLogger.LogDebug("<<< Exiting AddXmlElement(writer={Writer},name={Name},value={Value})", writer, name, value);
        }

        private string ApplyContextToImagePath(string imagePath, string defaultImagePath)
        {
			eventLogger.LogDebug(">>> Entering ApplyContextToImagePath(imagePath={ImagePath},defaultImagePath={DefaultImagePath})", imagePath, defaultImagePath);
            var result = imagePath;
            if (!string.IsNullOrEmpty(result))
			eventLogger.LogDebug("ApplyContextToImagePath(imagePath={ImagePath},defaultImagePath={DefaultImagePath}): result → {Result}", imagePath, defaultImagePath, result);
            {
                if (result.StartsWith("~", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = Globals.ResolveUrl(result);
                }
                else if (!(result.Contains("://") || result.StartsWith("/", StringComparison.Ordinal)))
                {
                    result = defaultImagePath + result;
                }
            }

			eventLogger.LogDebug("<<< Exiting ApplyContextToImagePath(imagePath={ImagePath},defaultImagePath={DefaultImagePath})", imagePath, defaultImagePath);
            return result;
        }
    }
}
