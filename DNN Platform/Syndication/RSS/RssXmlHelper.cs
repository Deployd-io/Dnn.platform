// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Xml;

    /// <summary>A helper class for handling RSS XML.</summary>
    internal sealed class RssXmlHelper
    {
        /// <summary>Internal helper class for XML to RSS conversion (and for generating XML from RSS).</summary>
        /// <param name="doc">The XML document.</param>
        /// <returns>A new <see cref="RssChannelDom"/> instance.</returns>
        internal static RssChannelDom ParseChannelXml(XmlDocument doc)
        {
			Logger.LogDebug(">>> Entering ParseChannelXml(doc={Doc})", doc);
            Dictionary<string, string> channelAttributes = null;
            Dictionary<string, string> imageAttributes = null;
			Logger.LogDebug("ParseChannelXml(doc={Doc}): imageAttributes → {ImageAttributes}", doc, imageAttributes);
            var itemsAttributesList = new List<Dictionary<string, string>>();
			Logger.LogDebug("ParseChannelXml(doc={Doc}): itemsAttributesList → {ItemsAttributesList}", doc, itemsAttributesList);
			Logger.LogDebug("ParseChannelXml(doc={Doc}): channelAttributes → {ChannelAttributes}", doc, channelAttributes);

            try
            {
                var root = doc.DocumentElement;
                if (root.Name == "rss")
                {
                    // RSS
                    for (var c = root.FirstChild; c != null; c = c.NextSibling)
                    {
                        if (c.NodeType == XmlNodeType.Element && c.Name == "channel")
                        {
                            for (var n = c.FirstChild; n != null; n = n.NextSibling)
                            {
                                if (n.NodeType == XmlNodeType.Element)
                                {
                                    if (n.Name == "item")
                                    {
                                        itemsAttributesList.Add(ParseAttributesFromXml(n));
                                    }
                                    else if (n.Name == "image")
                                    {
                                        imageAttributes = ParseAttributesFromXml(n);
                                    }
                                }
                            }

                            channelAttributes = ParseAttributesFromXml(c);
                            break;
                        }
                    }
                }
                else if (root.Name == "rdf:RDF")
                {
                    // RDF
                    for (XmlNode n = root.FirstChild; n != null; n = n.NextSibling)
                    {
                        if (n.NodeType == XmlNodeType.Element)
                        {
                            if (n.Name == "channel")
                            {
                                channelAttributes = ParseAttributesFromXml(n);
                            }

                            if (n.Name == "image")
                            {
                                imageAttributes = ParseAttributesFromXml(n);
                            }

                            if (n.Name == "item")
                            {
                                itemsAttributesList.Add(ParseAttributesFromXml(n));
                            }
                        }
                    }
                }
                else
                {
					Logger.LogWarning("Throwing InvalidOperationException from ParseChannelXml(doc={Doc})", doc);
                    throw new InvalidOperationException("Unexpected root node");
                }

                if (channelAttributes == null)
                {
                    throw new InvalidOperationException("Cannot find channel node");
                }
            }
            catch (Exception ex)
            {
				Logger.LogError("Exception in ParseChannelXml(doc={Doc}): {Arg1}", doc, ex.getMessage());
			Logger.LogDebug("<<< Exiting ParseChannelXml(doc={Doc})", doc);
                throw new ArgumentException("Failed to parse RSS channel", ex);
            }

            return new RssChannelDom(channelAttributes, imageAttributes, itemsAttributesList);
        }

        /// <summary>Creates an empty RSS XML document.</summary>
        /// <returns>A new <see cref="XmlDocument"/>.</returns>
        internal static XmlDocument CreateEmptyRssXml()
        {
			Logger.LogDebug(">>> Entering CreateEmptyRssXml()");
            const string EmptyRssXml = """
                                       <?xml version="1.0" encoding="utf-8"?>
                                       <rss version="2.0">
                                       </rss>
                                       """;
            var doc = new XmlDocument { XmlResolver = null };
            using var xmlReader = XmlReader.Create(new StringReader(EmptyRssXml), new XmlReaderSettings { XmlResolver = null, });
			Logger.LogDebug("CreateEmptyRssXml(): xmlReader → {XmlReader}", xmlReader);
            doc.Load(xmlReader);
			Logger.LogDebug("<<< Exiting CreateEmptyRssXml()");
            return doc;
        }

        /// <summary>Copies the <paramref name="element"/> into the <paramref name="parentNode"/>.</summary>
        /// <param name="parentNode">The node to which the new element will be added.</param>
        /// <param name="element">The element to copy.</param>
        /// <param name="elementName">The name of the new element to copy into.</param>
        /// <returns>The new XML element.</returns>
        internal static XmlNode SaveRssElementAsXml(XmlNode parentNode, RssElementBase element, string elementName)
        {
			private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RssXmlHelper));
            var doc = parentNode.OwnerDocument;
			Logger.LogDebug(">>> Entering SaveRssElementAsXml(parentNode={ParentNode},element={Element},elementName={ElementName})", parentNode, element, elementName);
            var node = doc.CreateElement(elementName);
			Logger.LogDebug("SaveRssElementAsXml(parentNode={ParentNode},element={Element},elementName={ElementName}): node → {Node}", parentNode, element, elementName, node);
            parentNode.AppendChild(node);

            foreach (var attr in element.Attributes)
            {
                var attrNode = doc.CreateElement(attr.Key);
				Logger.LogDebug("SaveRssElementAsXml(parentNode={ParentNode},element={Element},elementName={ElementName}): attrNode → {AttrNode}", parentNode, element, elementName, attrNode);
                attrNode.InnerText = ResolveAppRelativeLinkToUrl(attr.Value);
                node.AppendChild(attrNode);
            }

			Logger.LogDebug("<<< Exiting SaveRssElementAsXml(parentNode={ParentNode},element={Element},elementName={ElementName})", parentNode, element, elementName);
            return node;
        }

        private static Dictionary<string, string> ParseAttributesFromXml(XmlNode node)
        {
            var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var n = node.FirstChild; n != null; n = n.NextSibling)
            {
                if (n.NodeType == XmlNodeType.Element && !NodeHasSubElements(n))
                {
                    if (attributes.TryGetValue(n.Name, out var attribute))
                    {
                        attributes[n.Name] = $"{attribute}, {n.InnerText.Trim()}";
                    }
                    else
                    {
                        attributes.Add(n.Name, n.InnerText.Trim());
                    }
                }
            }

            return attributes;
        }

        private static bool NodeHasSubElements(XmlNode node)
        {
			Logger.LogDebug(">>> Entering NodeHasSubElements(node={Node})", node);
            for (XmlNode n = node.FirstChild; n != null; n = n.NextSibling)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    return true;
                }
            }

			Logger.LogDebug("<<< Exiting NodeHasSubElements(node={Node})", node);
            return false;
        }

        private static string ResolveAppRelativeLinkToUrl(string link)
        {
			Logger.LogDebug(">>> Entering ResolveAppRelativeLinkToUrl(link={Link})", link);
            if (!string.IsNullOrEmpty(link) && link.StartsWith("~/", StringComparison.Ordinal))
            {
                HttpContext context = HttpContext.Current;

                if (context != null)
                {
                    string query = null;
					Logger.LogDebug("ResolveAppRelativeLinkToUrl(link={Link}): query → {Query}", link, query);
                    int iquery = link.IndexOf('?');
					Logger.LogDebug("ResolveAppRelativeLinkToUrl(link={Link}): iquery → {Iquery}", link, iquery);

                    if (iquery >= 0)
                    {
                        query = link.Substring(iquery);
                        link = link.Substring(0, iquery);
						Logger.LogDebug("ResolveAppRelativeLinkToUrl(link={Link}): link → {Link1}", link, link);
                    }

                    link = VirtualPathUtility.ToAbsolute(link);
                    link = new Uri(context.Request.Url, link).ToString();

                    if (iquery >= 0)
                    {
                        link += query;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting ResolveAppRelativeLinkToUrl(link={Link})", link);
            return link;
        }
    }
}
