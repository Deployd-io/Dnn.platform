// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System.Xml;

    /// <summary>The XmlExtensions class allows you to write more efficient code to manage Xml documents.</summary>
    public static class XmlExtensions
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Adds an element to the specified XmlNode.</summary>
        /// <param name="node">The node to add the element to.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="elementValue">The element value.</param>
        /// <returns>
        /// The added element.
        /// </returns>
        public static XmlNode AddElement(this XmlNode node, string elementName, string elementValue)
        {
			Logger.LogDebug(">>> Entering AddElement(node={Node},elementName={ElementName},elementValue={ElementValue})", node, elementName, elementValue);
			Logger.LogDebug("<<< Exiting AddElement(node={Node},elementName={ElementName},elementValue={ElementValue})", node, elementName, elementValue);
            return node.AddElement(elementName, elementValue, false);
        }

        /// <summary>Adds an element to the specified XmlNode.</summary>
        /// <param name="node">The node to add the element to.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <param name="elementValue">The element value.</param>
        /// <param name="useCData">if set to <see langword="true"/> use a CData encapsulation.</param>
        /// <returns>
        /// The added element.
        /// </returns>
        public static XmlNode AddElement(this XmlNode node, string elementName, string elementValue, bool useCData)
        {
            var newElement = node.OwnerDocument.CreateElement(elementName);
            if (useCData)
            {
                var cData = node.OwnerDocument.CreateCDataSection(elementValue);
                newElement.AppendChild(cData);
            }
            else
            {
                newElement.InnerText = elementValue;
            }

            node.AppendChild(newElement);
			Logger.LogDebug("<<< Exiting AddElement(node={Node},elementName={ElementName},elementValue={ElementValue},useCData={UseCData})", node, elementName, elementValue, useCData);
            return newElement;
        }

        /// <summary>Adds an element to the specified XmlNode using the specified namespace.</summary>
        /// <param name="node">The node to add the element to.</param>
        /// <param name="elementName">Name of the element (without the abbreviated prefix).</param>
        /// <param name="elementValue">The element value.</param>
        /// <param name="useCData">if set to <see langword="true"/> use a CData encapsulation.</param>
        /// <param name="namespaceUri">The namespace URI.</param>
        /// <param name="namespaceAbbr">The namespace abbreviation.</param>
        /// <returns>
        /// The added node.
        /// </returns>
        public static XmlNode AddElement(this XmlNode node, string elementName, string elementValue, bool useCData, string namespaceUri, string namespaceAbbr)
        {
            var newElement = node.OwnerDocument.CreateElement(namespaceAbbr + ":" + elementName, namespaceUri);
            if (useCData)
            {
                var cData = node.OwnerDocument.CreateCDataSection(elementValue);
                newElement.AppendChild(cData);
            }
            else
            {
                newElement.InnerText = elementValue;
            }

            node.AppendChild(newElement);
			Logger.LogDebug("<<< Exiting AddElement(node={Node},elementName={ElementName},elementValue={ElementValue},useCData={UseCData},namespaceUri={NamespaceUri},namespaceAbbr={NamespaceAbbr})", node, elementName, elementValue, useCData, namespaceUri, namespaceAbbr);
            return newElement;
        }

        /// <summary>Adds an attribute to the specified node.</summary>
        /// <param name="node">The node to add the attribute to.</param>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <returns>The node the attribute was added to.</returns>
        public static XmlNode AddAttribute(this XmlNode node, string attributeName, string attributeValue)
        {
			Logger.LogDebug(">>> Entering AddAttribute(node={Node},attributeName={AttributeName},attributeValue={AttributeValue})", node, attributeName, attributeValue);
            XmlAttribute newAttribute = node.OwnerDocument.CreateAttribute(attributeName);
            newAttribute.InnerText = attributeValue;
			Logger.LogDebug("AddAttribute(node={Node},attributeName={AttributeName},attributeValue={AttributeValue}): newAttribute → {NewAttribute}", node, attributeName, attributeValue, newAttribute);
			Logger.LogDebug("AddAttribute(node={Node},attributeName={AttributeName},attributeValue={AttributeValue}): newAttribute.InnerText → {Arg3}", node, attributeName, attributeValue, newAttribute.InnerText);
            node.Attributes.Append(newAttribute);
			Logger.LogDebug("<<< Exiting AddAttribute(node={Node},attributeName={AttributeName},attributeValue={AttributeValue})", node, attributeName, attributeValue);
            return node;
        }

        /// <summary>Adds an attribute to an XmlNode using the specified namespace.</summary>
        /// <param name="node">The node to add the attribtue to.</param>
        /// <param name="attributeName">Name of the attribute without the namespace abbreviation prefix.</param>
        /// <param name="attributeValue">The attribute value.</param>
        /// <param name="namespaceUri">The namespace URI.</param>
        /// <param name="namespaceAbbr">The namespace abbreviation.</param>
        /// <returns>The node the attribute was added to.</returns>
        public static XmlNode AddAttribute(this XmlNode node, string attributeName, string attributeValue, string namespaceUri, string namespaceAbbr)
        {
            XmlAttribute newAttribute = node.OwnerDocument.CreateAttribute(namespaceAbbr + ":" + attributeName, namespaceUri);
            newAttribute.InnerText = attributeValue;
            node.Attributes.Append(newAttribute);
			Logger.LogDebug("<<< Exiting AddAttribute(node={Node},attributeName={AttributeName},attributeValue={AttributeValue},namespaceUri={NamespaceUri},namespaceAbbr={NamespaceAbbr})", node, attributeName, attributeValue, namespaceUri, namespaceAbbr);
            return node;
        }

        /// <summary>
        /// Remove the child node with the provided name.
        /// </summary>
        /// <param name="node">The parent node.</param>
        /// <param name="childNodeName">The child node name.</param>
        public static void RemoveChildNode(this XmlNode node, string childNodeName)
        {
			Logger.LogDebug(">>> Entering RemoveChildNode(node={Node},childNodeName={ChildNodeName})", node, childNodeName);
            if (node != null)
            {
                var childNode = node.SelectSingleNode(childNodeName);
                if (childNode != null)
				Logger.LogDebug("RemoveChildNode(node={Node},childNodeName={ChildNodeName}): childNode → {ChildNode}", node, childNodeName, childNode);
                {
                    node.RemoveChild(childNode);
                }
            }
			Logger.LogDebug("<<< Exiting RemoveChildNode(node={Node},childNodeName={ChildNodeName})", node, childNodeName);
        }
    }
}
