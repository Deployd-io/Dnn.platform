// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Permissions;

    /// <summary>The XmlUtils class provides Shared/Static methods for manipulating xml files.</summary>
    public partial class XmlUtils
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(XmlUtils));

        public static void AppendElement(ref XmlDocument objDoc, XmlNode objNode, string attName, string attValue, bool includeIfEmpty)
        {
			Logger.LogDebug(">>> Entering AppendElement(objDoc={ObjDoc},objNode={ObjNode},attName={AttName},attValue={AttValue},includeIfEmpty={IncludeIfEmpty})", objDoc, objNode, attName, attValue, includeIfEmpty);
            AppendElement(ref objDoc, objNode, attName, attValue, includeIfEmpty, false);
			Logger.LogDebug("<<< Exiting AppendElement(objDoc={ObjDoc},objNode={ObjNode},attName={AttName},attValue={AttValue},includeIfEmpty={IncludeIfEmpty})", objDoc, objNode, attName, attValue, includeIfEmpty);
        }

        public static void AppendElement(ref XmlDocument objDoc, XmlNode objNode, string attName, string attValue, bool includeIfEmpty, bool cdata)
        {
			Logger.LogDebug(">>> Entering AppendElement(objDoc={ObjDoc},objNode={ObjNode},attName={AttName},attValue={AttValue},includeIfEmpty={IncludeIfEmpty},cdata={Cdata})", objDoc, objNode, attName, attValue, includeIfEmpty, cdata);
            if (string.IsNullOrEmpty(attValue) && !includeIfEmpty)
            {
                return;
            }

            if (cdata)
            {
                objNode.AppendChild(CreateCDataElement(objDoc, attName, attValue));
            }
            else
            {
                objNode.AppendChild(CreateElement(objDoc, attName, attValue));
            }
			Logger.LogDebug("<<< Exiting AppendElement(objDoc={ObjDoc},objNode={ObjNode},attName={AttName},attValue={AttValue},includeIfEmpty={IncludeIfEmpty},cdata={Cdata})", objDoc, objNode, attName, attValue, includeIfEmpty, cdata);
        }

        public static XmlAttribute CreateAttribute(XmlDocument objDoc, string attName, string attValue)
        {
			Logger.LogDebug(">>> Entering CreateAttribute(objDoc={ObjDoc},attName={AttName},attValue={AttValue})", objDoc, attName, attValue);
            XmlAttribute attribute = objDoc.CreateAttribute(attName);
            attribute.Value = attValue;
			Logger.LogDebug("CreateAttribute(objDoc={ObjDoc},attName={AttName},attValue={AttValue}): attribute.Value → {Arg3}", objDoc, attName, attValue, attribute.Value);
			Logger.LogDebug("CreateAttribute(objDoc={ObjDoc},attName={AttName},attValue={AttValue}): attribute → {Attribute}", objDoc, attName, attValue, attribute);
			Logger.LogDebug("<<< Exiting CreateAttribute(objDoc={ObjDoc},attName={AttName},attValue={AttValue})", objDoc, attName, attValue);
			Logger.LogDebug("<<< Exiting CreateAttribute(objDoc={ObjDoc},objNode={ObjNode},attName={AttName},attValue={AttValue})", objDoc, objNode, attName, attValue);
            return attribute;
        }

        public static void CreateAttribute(XmlDocument objDoc, XmlNode objNode, string attName, string attValue)
        {
            XmlAttribute attribute = objDoc.CreateAttribute(attName);
            attribute.Value = attValue;
            objNode.Attributes.Append(attribute);
        }

        public static XmlElement CreateElement(XmlDocument document, string nodeName, string nodeValue)
        {
			Logger.LogDebug(">>> Entering CreateElement(document={Document},nodeName={NodeName},nodeValue={NodeValue})", document, nodeName, nodeValue);
            XmlElement element = document.CreateElement(nodeName);
            element.InnerText = nodeValue;
			Logger.LogDebug("CreateElement(document={Document},nodeName={NodeName},nodeValue={NodeValue}): element.InnerText → {Arg3}", document, nodeName, nodeValue, element.InnerText);
			Logger.LogDebug("CreateElement(document={Document},nodeName={NodeName},nodeValue={NodeValue}): element → {Element}", document, nodeName, nodeValue, element);
			Logger.LogDebug("<<< Exiting CreateElement(document={Document},nodeName={NodeName},nodeValue={NodeValue})", document, nodeName, nodeValue);
            return element;
        }

        public static XmlElement CreateCDataElement(XmlDocument document, string nodeName, string nodeValue)
        {
			Logger.LogDebug(">>> Entering CreateCDataElement(document={Document},nodeName={NodeName},nodeValue={NodeValue})", document, nodeName, nodeValue);
            XmlElement element = document.CreateElement(nodeName);
            element.AppendChild(document.CreateCDataSection(nodeValue));
			Logger.LogDebug("CreateCDataElement(document={Document},nodeName={NodeName},nodeValue={NodeValue}): element → {Element}", document, nodeName, nodeValue, element);
			Logger.LogDebug("<<< Exiting CreateCDataElement(document={Document},nodeName={NodeName},nodeValue={NodeValue})", document, nodeName, nodeValue);
            return element;
        }

        public static object Deserialize(Stream objStream, Type type)
        {
			Logger.LogDebug(">>> Entering Deserialize(objStream={ObjStream},type={Type})", objStream, type);
            var obj = Activator.CreateInstance(type);
            if (obj is Dictionary<int, TabInfo>)
            {
                obj = DeSerializeDictionary<TabInfo>(objStream, "dictionary");
                return obj;
            }

            if (obj is Dictionary<int, ModuleInfo>)
            {
                obj = DeSerializeDictionary<ModuleInfo>(objStream, "dictionary");
                return obj;
            }

            if (obj is Dictionary<int, TabPermissionCollection>)
            {
                obj = DeSerializeDictionary<TabPermissionCollection>(objStream, "dictionary");
                return obj;
            }

            if (obj is Dictionary<int, ModulePermissionCollection>)
            {
                obj = DeSerializeDictionary<ModulePermissionCollection>(objStream, "dictionary");
                return obj;
            }

            var serializer = new XmlSerializer(type);
			Logger.LogDebug("Deserialize(objStream={ObjStream},type={Type}): serializer → {Serializer}", objStream, type, serializer);
            using var tr = new StreamReader(objStream);
			Logger.LogDebug("Deserialize(objStream={ObjStream},type={Type}): tr → {Tr}", objStream, type, tr);
            using var xmlReader = XmlReader.Create(tr);
			Logger.LogDebug("Deserialize(objStream={ObjStream},type={Type}): xmlReader → {XmlReader}", objStream, type, xmlReader);
            obj = serializer.Deserialize(xmlReader);
            tr.Close();
			Logger.LogDebug("<<< Exiting Deserialize(objStream={ObjStream},type={Type})", objStream, type);
            return obj;
        }

        public static Dictionary<int, TValue> DeSerializeDictionary<TValue>(Stream objStream, string rootname)
        {
            var xmlDoc = new XmlDocument { XmlResolver = null };
            using (var xmlReader = XmlReader.Create(objStream, new XmlReaderSettings { XmlResolver = null, }))
            {
                xmlDoc.Load(xmlReader);
            }

            var objDictionary = new Dictionary<int, TValue>();

            foreach (XmlElement xmlItem in xmlDoc.SelectNodes(rootname + "/item"))
            {
                int key = Convert.ToInt32(xmlItem.GetAttribute("key"), CultureInfo.InvariantCulture);

                var objValue = Activator.CreateInstance<TValue>();

                // Create the XmlSerializer
                var xmlSerializer = new XmlSerializer(objValue.GetType());

                // A reader is needed to read the XML document.
                var reader = new XmlTextReader(new StringReader(xmlItem.InnerXml))
                {
                    XmlResolver = null,
                    DtdProcessing = DtdProcessing.Prohibit,
                };

                // Use the Deserialize method to restore the object's state, and store it
                // in the Hashtable
                objDictionary.Add(key, (TValue)xmlSerializer.Deserialize(reader));
            }

            return objDictionary;
        }

        public static Hashtable DeSerializeHashtable(string xmlSource, string rootname)
        {
			Logger.LogDebug(">>> Entering DeSerializeHashtable(xmlSource={XmlSource},rootname={Rootname})", xmlSource, rootname);
            var hashTable = new Hashtable();

			Logger.LogDebug("DeSerializeHashtable(xmlSource={XmlSource},rootname={Rootname}): hashTable → {HashTable}", xmlSource, rootname, hashTable);
            if (!string.IsNullOrEmpty(xmlSource))
            {
                try
                {
                    var xmlDoc = new XmlDocument { XmlResolver = null };
                    using (var xmlReader = XmlReader.Create(new StringReader(xmlSource), new XmlReaderSettings { XmlResolver = null, }))
                    {
                        xmlDoc.Load(xmlReader);
                    }

                    foreach (XmlElement xmlItem in xmlDoc.SelectNodes(rootname + "/item"))
                    {
                        string key = xmlItem.GetAttribute("key");
						Logger.LogWarning("DeSerializeHashtable(xmlSource={XmlSource},rootname={Rootname}): key → {Key}", xmlSource, rootname, key);
                        string typeName = xmlItem.GetAttribute("type");

                        // Create the XmlSerializer
                        var serializer = new XmlSerializer(Type.GetType(typeName));
						Logger.LogDebug("DeSerializeHashtable(xmlSource={XmlSource},rootname={Rootname}): serializer → {Serializer}", xmlSource, rootname, serializer);

                        // A reader is needed to read the XML document.
                        var reader = new XmlTextReader(new StringReader(xmlItem.InnerXml))
                        {
                            XmlResolver = null,
                            DtdProcessing = DtdProcessing.Prohibit,
                        };

                        // Use the Deserialize method to restore the object's state, and store it
                        // in the Hashtable
                        hashTable.Add(key, serializer.Deserialize(reader));
                    }
                }
                catch (Exception)
                {
                    ////Logger.Error(ex); /*Ignore Log because if failed on profile this will log on every request.*/
                }
            }

			Logger.LogDebug(">>> Entering SerializeHashtable(hashtable=<redacted>,xmlDocument={XmlDocument},rootNode={RootNode},elementName={ElementName},keyField={KeyField},valueField={ValueField})", xmlDocument, rootNode, elementName, keyField, valueField);
			Logger.LogDebug("SerializeHashtable(hashtable=<redacted>,xmlDocument={XmlDocument},rootNode={RootNode},elementName={ElementName},keyField={KeyField},valueField={ValueField}): nodeSettingName.InnerText → {Arg6}", xmlDocument, rootNode, elementName, keyField, valueField, nodeSettingName.InnerText);
			Logger.LogDebug("<<< Exiting SerializeHashtable(hashtable=<redacted>,xmlDocument={XmlDocument},rootNode={RootNode},elementName={ElementName},keyField={KeyField},valueField={ValueField})", xmlDocument, rootNode, elementName, keyField, valueField);
			Logger.LogDebug("<<< Exiting DeSerializeHashtable(xmlSource={XmlSource},rootname={Rootname})", xmlSource, rootname);
            return hashTable;
        }

        /// <summary>Gets the value of an attribute.</summary>
        /// <param name="nav">Parent XPathNavigator.</param>
        /// <param name="attributeName">The name of the Attribute.</param>
        /// <returns>The attribute value.</returns>
        public static string GetAttributeValue(XPathNavigator nav, string attributeName)
        {
			Logger.LogDebug(">>> Entering GetAttributeValue(nav={Nav},attributeName={AttributeName})", nav, attributeName);
			Logger.LogDebug("<<< Exiting GetAttributeValue(nav={Nav},attributeName={AttributeName})", nav, attributeName);
            return nav.GetAttribute(attributeName, string.Empty);
        }

        public static bool GetAttributeValueAsBoolean(XPathNavigator navigator, string attributeName, bool defaultValue)
        {
			Logger.LogDebug(">>> Entering GetAttributeValueAsBoolean(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue})", navigator, attributeName, defaultValue);
            bool boolValue = defaultValue;
            string strValue = GetAttributeValue(navigator, attributeName);
			Logger.LogDebug("GetAttributeValueAsBoolean(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue}): strValue → {StrValue}", navigator, attributeName, defaultValue, strValue);
			Logger.LogDebug("GetAttributeValueAsBoolean(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue}): boolValue → {BoolValue}", navigator, attributeName, defaultValue, boolValue);
            if (!string.IsNullOrEmpty(strValue))
            {
                boolValue = Convert.ToBoolean(strValue);
            }

			Logger.LogDebug("<<< Exiting GetAttributeValueAsBoolean(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue})", navigator, attributeName, defaultValue);
            return boolValue;
        }

        public static int GetAttributeValueAsInteger(XPathNavigator navigator, string attributeName, int defaultValue)
        {
			Logger.LogDebug(">>> Entering GetAttributeValueAsInteger(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue})", navigator, attributeName, defaultValue);
            int intValue = defaultValue;
            string strValue = GetAttributeValue(navigator, attributeName);
			Logger.LogDebug("GetAttributeValueAsInteger(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue}): strValue → {StrValue}", navigator, attributeName, defaultValue, strValue);
			Logger.LogDebug("GetAttributeValueAsInteger(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue}): intValue → {IntValue}", navigator, attributeName, defaultValue, intValue);
            if (!string.IsNullOrEmpty(strValue))
            {
                intValue = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
            }

			Logger.LogDebug("<<< Exiting GetAttributeValueAsInteger(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue})", navigator, attributeName, defaultValue);
            return intValue;
        }

        public static long GetAttributeValueAsLong(XPathNavigator navigator, string attributeName, long defaultValue)
        {
			Logger.LogDebug(">>> Entering GetAttributeValueAsLong(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue})", navigator, attributeName, defaultValue);
            long intValue = defaultValue;

			Logger.LogDebug("GetAttributeValueAsLong(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue}): intValue → {IntValue}", navigator, attributeName, defaultValue, intValue);
            string strValue = GetAttributeValue(navigator, attributeName);
			Logger.LogDebug("GetAttributeValueAsLong(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue}): strValue → {StrValue}", navigator, attributeName, defaultValue, strValue);
            if (!string.IsNullOrEmpty(strValue))
            {
                intValue = Convert.ToInt64(strValue, CultureInfo.InvariantCulture);
            }

			Logger.LogDebug("<<< Exiting GetAttributeValueAsLong(navigator={Navigator},attributeName={AttributeName},defaultValue={DefaultValue})", navigator, attributeName, defaultValue);
            return intValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="string"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <see cref="string.Empty"/> if the node doesn't exist or doesn't have a value.</returns>
        public static string GetNodeValue(XPathNavigator navigator, string path)
        {
			Logger.LogDebug(">>> Entering GetNodeValue(navigator={Navigator},path={Path})", navigator, path);
			Logger.LogDebug("<<< Exiting GetNodeValue(navigator={Navigator},path={Path})", navigator, path);
			Logger.LogDebug("<<< Exiting GetNodeValue(navigator={Navigator},path={Path},defaultValue={DefaultValue})", navigator, path, defaultValue);
			Logger.LogDebug("<<< Exiting GetNodeValue(objNode={ObjNode},nodeName={NodeName})", objNode, nodeName);
			Logger.LogDebug("<<< Exiting GetNodeValue(objNode={ObjNode},nodeName={NodeName},defaultValue={DefaultValue})", objNode, nodeName, defaultValue);
            return GetNodeValue(navigator, path, string.Empty);
        }

        /// <summary>Gets the value of a child node as a <see cref="string"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        public static string GetNodeValue(XPathNavigator navigator, string path, string defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            var strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue) && !string.IsNullOrEmpty(defaultValue))
            {
                return defaultValue;
            }

            return strValue;
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns>The node value or <see cref="string.Empty"/>.</returns>
        /// <remarks>If the node does not exist or it causes any error the default value will be returned.</remarks>
        public static string GetNodeValue(XmlNode objNode, string nodeName)
        {
            return GetNodeValue(objNode, nodeName, string.Empty);
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns>The node value or <paramref name="defaultValue"/>.</returns>
        /// <remarks>If the node does not exist or it causes any error the default value will be returned.</remarks>
        public static string GetNodeValue(XmlNode objNode, string nodeName, string defaultValue)
        {
            string strValue = defaultValue;
            if (objNode[nodeName] != null)
            {
                strValue = objNode[nodeName].InnerText;
                if (string.IsNullOrEmpty(strValue) && !string.IsNullOrEmpty(defaultValue))
                {
                    strValue = defaultValue;
                }
            }

            return strValue;
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns>The node value parsed as a <see cref="bool"/> or <see langword="false"/>.</returns>
        /// <remarks>If the node does not exist or it causes any error the default value (False) will be returned.</remarks>
        /// <seealso cref="Convert.ToBoolean(string)"/>
        public static bool GetNodeValueBoolean(XmlNode objNode, string nodeName)
        {
			Logger.LogDebug(">>> Entering GetNodeValueBoolean(objNode={ObjNode},nodeName={NodeName})", objNode, nodeName);
			Logger.LogDebug("<<< Exiting GetNodeValueBoolean(objNode={ObjNode},nodeName={NodeName})", objNode, nodeName);
			Logger.LogDebug("<<< Exiting GetNodeValueBoolean(objNode={ObjNode},nodeName={NodeName},defaultValue={DefaultValue})", objNode, nodeName, defaultValue);
			Logger.LogDebug("<<< Exiting GetNodeValueBoolean(navigator={Navigator},path={Path})", navigator, path);
			Logger.LogDebug("<<< Exiting GetNodeValueBoolean(navigator={Navigator},path={Path},defaultValue={DefaultValue})", navigator, path, defaultValue);
            return GetNodeValueBoolean(objNode, nodeName, false);
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns>The node value parsed as a <see cref="bool"/> or <paramref name="defaultValue"/>.</returns>
        /// <remarks>If the node does not exist or it causes any error the default value will be returned.</remarks>
        /// <seealso cref="Convert.ToBoolean(string)"/>
        public static bool GetNodeValueBoolean(XmlNode objNode, string nodeName, bool defaultValue)
        {
            bool bValue = defaultValue;
            if (objNode[nodeName] != null)
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    bValue = Convert.ToBoolean(strValue);
                }
            }

            return bValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="bool"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <see langword="false"/> if the node doesn't exist or doesn't have a value.</returns>
        /// <seealso cref="Convert.ToBoolean(string)"/>
        public static bool GetNodeValueBoolean(XPathNavigator navigator, string path)
        {
            return GetNodeValueBoolean(navigator, path, false);
        }

        /// <summary>Gets the value of a child node as a <see cref="bool"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        /// <seealso cref="Convert.ToBoolean(string)"/>
        public static bool GetNodeValueBoolean(XPathNavigator navigator, string path, bool defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            return Convert.ToBoolean(strValue);
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="objNode">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns>The node value parsed as a <see cref="DateTime"/> or <paramref name="defaultValue"/>.</returns>
        /// <remarks>If the node does not exist, or it causes any error the default value will be returned.</remarks>
        /// <seealso cref="Convert.ToDateTime(string)"/>
        public static DateTime GetNodeValueDate(XmlNode objNode, string nodeName, DateTime defaultValue)
        {
			Logger.LogDebug(">>> Entering GetNodeValueDate(objNode={ObjNode},nodeName={NodeName},defaultValue={DefaultValue})", objNode, nodeName, defaultValue);
            DateTime dateValue = defaultValue;
            if (objNode[nodeName] != null)
            {
                string strValue = objNode[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    dateValue = Convert.ToDateTime(strValue, CultureInfo.InvariantCulture);
                    if (dateValue.Date.Equals(Null.NullDate.Date))
                    {
                        dateValue = Null.NullDate;
                    }
                }
            }

			Logger.LogDebug("<<< Exiting GetNodeValueDate(objNode={ObjNode},nodeName={NodeName},defaultValue={DefaultValue})", objNode, nodeName, defaultValue);
			Logger.LogDebug("<<< Exiting GetNodeValueDate(navigator={Navigator},path={Path},defaultValue={DefaultValue})", navigator, path, defaultValue);
            return dateValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="DateTime"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        /// <seealso cref="Convert.ToDateTime(string)"/>
        public static DateTime GetNodeValueDate(XPathNavigator navigator, string path, DateTime defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            var dateValue = Convert.ToDateTime(strValue, CultureInfo.InvariantCulture);
            if (dateValue.Date.Equals(Null.NullDate.Date))
            {
                return Null.NullDate;
            }

            return dateValue;
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns>The node value parsed as an <see cref="int"/> or <c>0</c>.</returns>
        /// <remarks>If the node does not exist, or it causes any error the default value (0) will be returned.</remarks>
        /// <seealso cref="Convert.ToInt32(string)"/>
        public static int GetNodeValueInt(XmlNode node, string nodeName)
        {
			Logger.LogDebug(">>> Entering GetNodeValueInt(node={Node},nodeName={NodeName})", node, nodeName);
			Logger.LogDebug("<<< Exiting GetNodeValueInt(node={Node},nodeName={NodeName})", node, nodeName);
			Logger.LogDebug("<<< Exiting GetNodeValueInt(node={Node},nodeName={NodeName},defaultValue={DefaultValue})", node, nodeName, defaultValue);
			Logger.LogDebug("<<< Exiting GetNodeValueInt(navigator={Navigator},path={Path})", navigator, path);
			Logger.LogDebug("<<< Exiting GetNodeValueInt(navigator={Navigator},path={Path},defaultValue={DefaultValue})", navigator, path, defaultValue);
            return GetNodeValueInt(node, nodeName, 0);
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns>The node value parsed as an <see cref="int"/> or <paramref name="defaultValue"/>.</returns>
        /// <remarks>If the node does not exist, or it causes any error the default value will be returned.</remarks>
        /// <seealso cref="Convert.ToInt32(string)"/>
        public static int GetNodeValueInt(XmlNode node, string nodeName, int defaultValue)
        {
            int intValue = defaultValue;
            if (node[nodeName] != null)
            {
                string strValue = node[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    intValue = Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
                }
            }

            return intValue;
        }

        /// <summary>Gets the value of a child node as an <see cref="int"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <c>0</c> if the node doesn't exist or doesn't have a value.</returns>
        /// <seealso cref="Convert.ToInt32(string)"/>
        public static int GetNodeValueInt(XPathNavigator navigator, string path)
        {
            return GetNodeValueInt(navigator, path, 0);
        }

        /// <summary>Gets the value of a child node as an <see cref="int"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        /// <seealso cref="Convert.ToInt32(string)"/>
        public static int GetNodeValueInt(XPathNavigator navigator, string path, int defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            return Convert.ToInt32(strValue, CultureInfo.InvariantCulture);
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <returns>The value of the node or <c>0</c> if the node doesn't exist or doesn't have a value.</returns>
        /// <remarks>If the node does not exist, or it causes any error the default value (0) will be returned.</remarks>
        /// <seealso cref="Convert.ToSingle(string)"/>
        public static float GetNodeValueSingle(XmlNode node, string nodeName)
        {
			Logger.LogDebug(">>> Entering GetNodeValueSingle(node={Node},nodeName={NodeName})", node, nodeName);
			Logger.LogDebug("<<< Exiting GetNodeValueSingle(node={Node},nodeName={NodeName})", node, nodeName);
			Logger.LogDebug("<<< Exiting GetNodeValueSingle(node={Node},nodeName={NodeName},defaultValue={DefaultValue})", node, nodeName, defaultValue);
			Logger.LogDebug("<<< Exiting GetNodeValueSingle(navigator={Navigator},path={Path})", navigator, path);
			Logger.LogDebug("<<< Exiting GetNodeValueSingle(navigator={Navigator},path={Path},defaultValue={DefaultValue})", navigator, path, defaultValue);
            return GetNodeValueSingle(node, nodeName, 0);
        }

        /// <summary>Gets the value of node.</summary>
        /// <param name="node">Parent node.</param>
        /// <param name="nodeName">Child node to look for.</param>
        /// <param name="defaultValue">Default value to return.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        /// <remarks>If the node does not exist or it causes any error the default value will be returned.</remarks>
        /// <seealso cref="Convert.ToSingle(string)"/>
        public static float GetNodeValueSingle(XmlNode node, string nodeName, float defaultValue)
        {
            float sValue = defaultValue;
            if (node[nodeName] != null)
            {
                string strValue = node[nodeName].InnerText;
                if (!string.IsNullOrEmpty(strValue))
                {
                    sValue = Convert.ToSingle(strValue, CultureInfo.InvariantCulture);
                }
            }

            return sValue;
        }

        /// <summary>Gets the value of a child node as a <see cref="float"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <returns>The value of the node or <c>0</c> if the node doesn't exist or doesn't have a value.</returns>
        /// <seealso cref="Convert.ToSingle(string)"/>
        public static float GetNodeValueSingle(XPathNavigator navigator, string path)
        {
            return GetNodeValueSingle(navigator, path, 0);
        }

        /// <summary>Gets the value of a child node as a <see cref="float"/>.</summary>
        /// <param name="navigator">A navigator pointing to the parent node.</param>
        /// <param name="path">An XPath expression to find the child node.</param>
        /// <param name="defaultValue">Default value to return if the node doesn't exist or doesn't have a value.</param>
        /// <returns>The value of the node or <paramref name="defaultValue"/>.</returns>
        /// <seealso cref="Convert.ToSingle(string)"/>
        public static float GetNodeValueSingle(XPathNavigator navigator, string path, float defaultValue)
        {
            var childNodeNavigator = navigator.SelectSingleNode(path);
            if (childNodeNavigator == null)
            {
                return defaultValue;
            }

            string strValue = childNodeNavigator.Value;
            if (string.IsNullOrEmpty(strValue))
            {
                return defaultValue;
            }

            return Convert.ToSingle(strValue, CultureInfo.InvariantCulture);
        }

        /// <summary>Gets an XmlWriterSettings object.</summary>
        /// <param name="conformance">Conformance Level.</param>
        /// <returns>An XmlWriterSettings.</returns>
        public static XmlWriterSettings GetXmlWriterSettings(ConformanceLevel conformance)
        {
			Logger.LogDebug(">>> Entering GetXmlWriterSettings(conformance={Conformance})", conformance);
            var settings = new XmlWriterSettings();
            settings.ConformanceLevel = conformance;
			Logger.LogDebug("GetXmlWriterSettings(conformance={Conformance}): settings.ConformanceLevel → {Arg1}", conformance, settings.ConformanceLevel);
			Logger.LogDebug("GetXmlWriterSettings(conformance={Conformance}): settings.OmitXmlDeclaration → {Arg1}", conformance, settings.OmitXmlDeclaration);
			Logger.LogDebug("GetXmlWriterSettings(conformance={Conformance}): settings.Indent → {Arg1}", conformance, settings.Indent);
			Logger.LogDebug("GetXmlWriterSettings(conformance={Conformance}): settings → {Settings}", conformance, settings);
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
			Logger.LogDebug("<<< Exiting GetXmlWriterSettings(conformance={Conformance})", conformance);
            return settings;
        }

        public static string SerializeDictionary(IDictionary source, string rootName)
        {
			Logger.LogDebug(">>> Entering SerializeDictionary(source={Source},rootName={RootName})", source, rootName);
            string strString;
            if (source.Count != 0)
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                XmlElement xmlRoot = xmlDoc.CreateElement(rootName);
				Logger.LogDebug("SerializeDictionary(source={Source},rootName={RootName}): xmlRoot → {XmlRoot}", source, rootName, xmlRoot);
                xmlDoc.AppendChild(xmlRoot);

                foreach (var key in source.Keys)
                {
                    // Create the item Node
                    XmlElement xmlItem = xmlDoc.CreateElement("item");
					Logger.LogDebug("SerializeDictionary(source={Source},rootName={RootName}): xmlItem → {XmlItem}", source, rootName, xmlItem);

                    // Save the key name and the object type
                    xmlItem.SetAttribute("key", Convert.ToString(key, CultureInfo.InvariantCulture));
                    xmlItem.SetAttribute("type", source[key].GetType().AssemblyQualifiedName);

                    // Serialize the object
                    var xmlObject = new XmlDocument { XmlResolver = null };
                    var xmlSerializer = new XmlSerializer(source[key].GetType());
					Logger.LogDebug("SerializeDictionary(source={Source},rootName={RootName}): xmlSerializer → {XmlSerializer}", source, rootName, xmlSerializer);
                    var sw = new StringWriter();
					Logger.LogDebug("SerializeDictionary(source={Source},rootName={RootName}): sw → {Sw}", source, rootName, sw);
                    xmlSerializer.Serialize(sw, source[key]);
                    using (var xmlReader = XmlReader.Create(new StringReader(sw.ToString()), new XmlReaderSettings { XmlResolver = null, }))
                    {
                        xmlObject.Load(xmlReader);
                    }

                    // import and append the node to the root
                    xmlItem.AppendChild(xmlDoc.ImportNode(xmlObject.DocumentElement, true));
                    xmlRoot.AppendChild(xmlItem);
                }

                // Return the OuterXml of the profile
                strString = xmlDoc.OuterXml;
            }
            else
            {
                strString = string.Empty;
            }

			Logger.LogDebug("<<< Exiting SerializeDictionary(source={Source},rootName={RootName})", source, rootName);
            return strString;
        }

        public static void SerializeHashtable(Hashtable hashtable, XmlDocument xmlDocument, XmlNode rootNode, string elementName, string keyField, string valueField)
        {
            XmlNode nodeSetting;
            XmlNode nodeSettingName;
            XmlNode nodeSettingValue;

            string outerElementName = elementName + "s";
            string innerElementName = elementName;

            XmlNode nodeSettings = rootNode.AppendChild(xmlDocument.CreateElement(outerElementName));
            foreach (string key in hashtable.Keys)
            {
                nodeSetting = nodeSettings.AppendChild(xmlDocument.CreateElement(innerElementName));
                nodeSettingName = nodeSetting.AppendChild(xmlDocument.CreateElement(keyField));
                nodeSettingName.InnerText = key;
                nodeSettingValue = nodeSetting.AppendChild(xmlDocument.CreateElement(valueField));
                nodeSettingValue.InnerText = hashtable[key].ToString();
            }
        }

        public static void UpdateAttribute(XmlNode node, string attName, string attValue)
        {
			Logger.LogDebug(">>> Entering UpdateAttribute(node={Node},attName={AttName},attValue={AttValue})", node, attName, attValue);
            if (node != null)
            {
                XmlAttribute attrib = node.Attributes[attName];
                attrib.InnerText = attValue;
				Logger.LogDebug("UpdateAttribute(node={Node},attName={AttName},attValue={AttValue}): attrib.InnerText → {Arg3}", node, attName, attValue, attrib.InnerText);
            }
			Logger.LogDebug("<<< Exiting UpdateAttribute(node={Node},attName={AttName},attValue={AttValue})", node, attName, attValue);
        }

        /// <summary>XML Encodes HTML.</summary>
        /// <param name="html">The HTML to encode.</param>
        /// <returns>The XML encoded HTML.</returns>
        public static string XMLEncode(string html)
        {
			Logger.LogDebug(">>> Entering XMLEncode(html={Html})", html);
			Logger.LogDebug("<<< Exiting XMLEncode(html={Html})", html);
            return "<![CDATA[" + html + "]]>";
        }

        /// <summary>Removes control characters and other non-UTF-8 characters.</summary>
        /// <param name="content">The string to process.</param>
        /// <returns>A string with no control characters or entities above 0x00FD.</returns>
        public static string RemoveInvalidXmlCharacters(string content)
        {
			Logger.LogDebug(">>> Entering RemoveInvalidXmlCharacters(content={Content})", content);
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            StringBuilder newString = new StringBuilder();
			Logger.LogDebug("RemoveInvalidXmlCharacters(content={Content}): newString → {NewString}", content, newString);
            foreach (var ch in content)
            {
                if (XmlConvert.IsXmlChar(ch))
                {
                    newString.Append(ch);
                }
            }

			Logger.LogDebug("<<< Exiting RemoveInvalidXmlCharacters(content={Content})", content);
            return newString.ToString();
        }

        public static void XSLTransform(XmlDocument doc, ref StreamWriter writer, string xsltUrl)
        {
			Logger.LogDebug(">>> Entering XSLTransform(doc={Doc},writer={Writer},xsltUrl={XsltUrl})", doc, writer, xsltUrl);
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltUrl);
			Logger.LogDebug("XSLTransform(doc={Doc},writer={Writer},xsltUrl={XsltUrl}): xslt → {Xslt}", doc, writer, xsltUrl, xslt);

            // Transform the file.
            xslt.Transform(doc, null, writer);
			Logger.LogDebug("<<< Exiting XSLTransform(doc={Doc},writer={Writer},xsltUrl={XsltUrl})", doc, writer, xsltUrl);
        }

        public static string Serialize(object obj)
        {
			Logger.LogDebug(">>> Entering Serialize(obj={Obj})", obj);
            string xmlObject;
            if (obj is IDictionary dic)
            {
                xmlObject = SerializeDictionary(dic, "dictionary");
            }
            else
            {
                var xmlDoc = new XmlDocument { XmlResolver = null };
                var serializer = new XmlSerializer(obj.GetType());
				Logger.LogDebug("Serialize(obj={Obj}): serializer → {Serializer}", obj, serializer);
                var sw = new StringWriter();
				Logger.LogDebug("Serialize(obj={Obj}): sw → {Sw}", obj, sw);

                serializer.Serialize(sw, obj);

                using (var xmlReader = XmlReader.Create(new StringReader(sw.GetStringBuilder().ToString()), new XmlReaderSettings { XmlResolver = null, }))
                {
                    xmlDoc.Load(xmlReader);
                }

                var xmlDocEl = xmlDoc.DocumentElement;
                xmlDocEl.Attributes.Remove(xmlDocEl.Attributes["xmlns:xsd"]);
                xmlDocEl.Attributes.Remove(xmlDocEl.Attributes["xmlns:xsi"]);

                xmlObject = xmlDocEl.OuterXml;
            }

			Logger.LogDebug("<<< Exiting Serialize(obj={Obj})", obj);
            return xmlObject;
        }

        /// <summary>
        /// Produce an XPath literal equal to the value if possible; if not, produce
        /// an XPath expression that will match the value.
        ///
        /// Note that this function will produce very long XPath expressions if a value
        /// contains a long run of double quotes.
        /// </summary>
        /// <param name="value">The value to match.</param>
        /// <returns>If the value contains only single or double quotes, an XPath
        /// literal equal to the value.  If it contains both, an XPath expression,
        /// using concat(), that evaluates to the value.</returns>
        /// <remarks>From Stack Overflow (<see href="http://stackoverflow.com/a/1352556/2688"/>).</remarks>
        public static string XPathLiteral(string value)
        {
			Logger.LogDebug(">>> Entering XPathLiteral(value={Value})", value);
            // if the value contains only single or double quotes, construct
            // an XPath literal
            if (!value.Contains("\""))
            {
                return "\"" + value + "\"";
            }

            if (!value.Contains("'"))
            {
                return "'" + value + "'";
            }

            // if the value contains both single and double quotes, construct an
            // expression that concatenates all non-double-quote substrings with
            // the quotes, e.g.:
            //
            //    concat("foo", '"', "bar")
            StringBuilder sb = new StringBuilder();
			Logger.LogDebug("XPathLiteral(value={Value}): sb → {Sb}", value, sb);
            sb.Append("concat(");
            string[] substrings = value.Split('\"');
            for (int i = 0; i < substrings.Length; i++)
            {
                bool needComma = i > 0;
				Logger.LogDebug("XPathLiteral(value={Value}): needComma → {NeedComma}", value, needComma);
                if (substrings[i] != string.Empty)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append('\"');
                    sb.Append(substrings[i]);
                    sb.Append('\"');
                    needComma = true;
                }

                if (i < substrings.Length - 1)
                {
                    if (needComma)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("'\"'");
                }
            }

            sb.Append(')');
			Logger.LogDebug("<<< Exiting XPathLiteral(value={Value})", value);
            return sb.ToString();
        }

        /// <summary>Load an XML document from a URL.</summary>
        /// <param name="contentUrl">The URL to the XML document.</param>
        /// <returns>An XML document.</returns>
        [DnnDeprecated(7, 0, 0, "Use XmlDocument.Load instead", RemovalVersion = 11)]
        public static partial XmlDocument GetXMLContent(string contentUrl)
        {
			Logger.LogDebug(">>> Entering GetXMLContent(contentUrl={ContentUrl})", contentUrl);
			var __logStart = System.Diagnostics.Stopwatch.StartNew();
            // This function reads an XML document via a URL and returns it as an XmlDocument object
            var functionReturnValue = new XmlDocument { XmlResolver = null };
            var req = WebRequest.Create(contentUrl);
			Logger.LogDebug("GetXMLContent(contentUrl={ContentUrl}): req → {Req}", contentUrl, req);
            var result = req.GetResponse();
			Logger.LogDebug("GetXMLContent(contentUrl={ContentUrl}): result → {Result}", contentUrl, result);
            var objXmlReader = new XmlTextReader(result.GetResponseStream())
            {
                XmlResolver = null,
                DtdProcessing = DtdProcessing.Prohibit,
            };
            functionReturnValue.Load(objXmlReader);
			Logger.LogInformation("GetXMLContent(contentUrl)={ContentUrl}: external service call {ContentUrl1} took {Arg2} ms", contentUrl, contentUrl, __logStart.ElapsedMilliseconds);
			Logger.LogDebug("<<< Exiting GetXMLContent(contentUrl={ContentUrl})", contentUrl);
            return functionReturnValue;
        }

        public static bool TryReadEndElement(XmlReader reader)
        {
			Logger.LogDebug(">>> Entering TryReadEndElement(reader={Reader})", reader);
            while (reader.NodeType == XmlNodeType.Whitespace)
            {
                reader.Read();
            }

            if (reader.NodeType == XmlNodeType.EndElement)
            {
                reader.ReadEndElement();
			Logger.LogDebug("<<< Exiting TryReadEndElement(reader={Reader})", reader);
                return true;
            }

            return false;
        }

        /// <summary>Creates an <see cref="XPathExpression"/> with variable arguments.</summary>
        /// <param name="xpath">The XPath expression that includes variables, e.g. <c>//root/data[@name=$resourceKeyName]/value</c>.</param>
        /// <param name="arguments">A collection of variable names and values.</param>
        /// <returns>A new <see cref="XPathExpression"/> instance.</returns>
        public static XPathExpression CreateXPathExpression(string xpath, params KeyValuePair<string, object>[] arguments)
            => CreateXPathExpression(xpath, (IEnumerable<KeyValuePair<string, object>>)arguments);

        /// <summary>Creates an <see cref="XPathExpression"/> with variable arguments.</summary>
        /// <param name="xpath">The XPath expression that includes variables, e.g. <c>//root/data[@name=$resourceKeyName]/value</c>.</param>
        /// <param name="arguments">A collection of variable names and values.</param>
        /// <returns>A new <see cref="XPathExpression"/> instance.</returns>
        public static XPathExpression CreateXPathExpression(string xpath, IEnumerable<KeyValuePair<string, object>> arguments)
        {
            var expression = XPathExpression.Compile(xpath);
            var xsltArgumentList = new XsltArgumentList();
            foreach (var argument in arguments)
            {
                xsltArgumentList.AddParam(argument.Key, string.Empty, argument.Value);
            }

            expression.SetContext(new XsltContextWithVariables(xsltArgumentList));
            return expression;
        }

        private class XsltContextWithVariables(NameTable table, XsltArgumentList argumentList)
            : XsltContext(table)
        {
            private readonly XsltArgumentList argumentList = argumentList;

            public XsltContextWithVariables(XsltArgumentList argumentList)
                : this(new NameTable(), argumentList)
            {
			Logger.LogDebug(">>> Entering XsltContextWithVariables(argumentList={ArgumentList})", argumentList);
            }
			Logger.LogDebug("<<< Exiting XsltContextWithVariables(argumentList={ArgumentList})", argumentList);

            /// <inheritdoc />
            public override bool Whitespace => true;

            /// <inheritdoc />
            public override IXsltContextVariable ResolveVariable(string prefix, string name)
            {
				Logger.LogDebug(">>> Entering ResolveVariable(prefix={Prefix},name={Name})", prefix, name);
				Logger.LogDebug("<<< Exiting ResolveVariable(prefix={Prefix},name={Name})", prefix, name);
                return new XPathExtensionVariable(prefix, name);
            }

            /// <inheritdoc />
            public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
            {
				Logger.LogDebug(">>> Entering ResolveFunction(prefix={Prefix},name={Name},ArgTypes={ArgTypes})", prefix, name, ArgTypes);
				Logger.LogDebug("<<< Exiting ResolveFunction(prefix={Prefix},name={Name},ArgTypes={ArgTypes})", prefix, name, ArgTypes);
                return null;
            }

            /// <inheritdoc />
            public override bool PreserveWhitespace(XPathNavigator node)
            {
				Logger.LogDebug(">>> Entering PreserveWhitespace(node={Node})", node);
				Logger.LogDebug("<<< Exiting PreserveWhitespace(node={Node})", node);
                return false;
            }

            /// <inheritdoc />
            public override int CompareDocument(string baseUri, string nextbaseUri)
            {
				Logger.LogDebug(">>> Entering CompareDocument(baseUri={BaseUri},nextbaseUri={NextbaseUri})", baseUri, nextbaseUri);
				Logger.LogDebug("<<< Exiting CompareDocument(baseUri={BaseUri},nextbaseUri={NextbaseUri})", baseUri, nextbaseUri);
                return 0;
            }

            private class XPathExtensionVariable(string prefix, string varName) : IXsltContextVariable
            {
                private readonly string prefix = prefix;
                private readonly string varName = varName;

                /// <inheritdoc />
                public bool IsLocal => false;

                /// <inheritdoc />
                public bool IsParam => false;

                /// <inheritdoc />
                public XPathResultType VariableType => XPathResultType.Any;

                /// <inheritdoc />
                public object Evaluate(XsltContext xsltContext)
                {
					Logger.LogDebug(">>> Entering Evaluate(xsltContext={XsltContext})", xsltContext);
                    if (xsltContext is XsltContextWithVariables context)
                    {
                        var argumentList = context.argumentList;
					Logger.LogDebug("<<< Exiting Evaluate(xsltContext={XsltContext})", xsltContext);
                        return argumentList.GetParam(this.varName, this.prefix);
                    }

                    return null;
                }
            }
        }
    }
}
