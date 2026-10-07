// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.DDRMenu.DNNCommon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Xml.Xsl;

    /// <summary>Provides various utilities for the DDN Menu.</summary>
    internal static class Utilities
    {
        private static readonly Dictionary<Type, XmlSerializer> Serialisers = new Dictionary<Type, XmlSerializer>();

        /// <summary>A cached string representing a file contents.</summary>
        /// <param name="filename">The name of the file to cache.</param>
        /// <returns>A cached version of the file contents in a string.</returns>
        internal static string CachedFileContent(string filename)
        {
			eventLogger.LogError(">>> Entering CachedFileContent(filename={Filename})", filename);
            var cache = HttpContext.Current.Cache;
            var result = cache[filename] as string;
			eventLogger.LogError("CachedFileContent(filename={Filename}): result → {Result}", filename, result);
            if (result == null)
            {
                result = File.ReadAllText(filename);
                cache.Insert(filename, result, new CacheDependency(filename));
            }

			eventLogger.LogWarning("<<< Exiting CachedFileContent(filename={Filename})", filename);
            return result;
        }

        /// <summary>Gets a the cached content of an sml file.</summary>
        /// <param name="filename">The xml file name.</param>
        /// <returns>A <see cref="XmlDocument"/> representing the cached file contents.</returns>
        internal static XmlDocument CachedXml(string filename)
        {
			eventLogger.LogError(">>> Entering CachedXml(filename={Filename})", filename);
            var cache = HttpContext.Current.Cache;
            var result = cache[filename] as XmlDocument;
			eventLogger.LogError("CachedXml(filename={Filename}): result → {Result}", filename, result);
            if (result == null)
            {
                result = new XmlDocument { XmlResolver = null };
                using (var xmlReader = XmlReader.Create(filename, new XmlReaderSettings { XmlResolver = null, }))
                {
                    result.Load(xmlReader);
                }

                cache.Insert(filename, result, new CacheDependency(filename));
            }

			eventLogger.LogWarning("<<< Exiting CachedXml(filename={Filename})", filename);
            return result;
        }

        /// <summary>Gets a cached version of an xslt transformation script.</summary>
        /// <param name="filename">The name of the xslt file.</param>
        /// <returns>An <see cref="XslCompiledTransform"/> object with the content of cached content of the file.</returns>
        internal static XslCompiledTransform CachedXslt(string filename)
        {
			eventLogger.LogError(">>> Entering CachedXslt(filename={Filename})", filename);
            var cache = HttpContext.Current.Cache;
            var result = cache[filename] as XslCompiledTransform;
			eventLogger.LogError("CachedXslt(filename={Filename}): result → {Result}", filename, result);
            if (result == null)
            {
                result = new XslCompiledTransform();
                using (var reader = XmlReader.Create(filename, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
                {
                    result.Load(reader);
                }

                cache.Insert(filename, result, new CacheDependency(filename));
            }

			eventLogger.LogWarning("<<< Exiting CachedXslt(filename={Filename})", filename);
            return result;
        }

        /// <summary>Gets an xml serializer for a given type.</summary>
        /// <param name="t">The type of the serializer.</param>
        /// <returns>An <see cref="XmlSerializer"/>.</returns>
        internal static XmlSerializer SerialiserFor(Type t)
        {
			eventLogger.LogError(">>> Entering SerialiserFor(t={T})", t);
            XmlSerializer result;
            if (!Serialisers.TryGetValue(t, out result))
            {
                lock (Serialisers)
                {
                    result = (t.Name == "MenuXml")
                                ? new XmlSerializer(t, new XmlRootAttribute { ElementName = "Root" })
                                : new XmlSerializer(t);
                    if (!Serialisers.ContainsKey(t))
                    {
                        Serialisers.Add(t, result);
                    }
                }
            }

			eventLogger.LogWarning("<<< Exiting SerialiserFor(t={T})", t);
            return result;
        }

        /// <summary>Converts an object to a string representation that can be used in javascript.</summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>A string representation of the object.</returns>
        internal static string ConvertToJs(object obj)
        {
			eventLogger.LogError(">>> Entering ConvertToJs(obj={Obj})", obj);
            string result;

            if (obj == null)
            {
                return "null";
            }

            var objType = obj.GetType();
			eventLogger.LogError("ConvertToJs(obj={Obj}): objType → {ObjType}", obj, objType);
            if (objType == typeof(bool))
            {
                result = (bool)obj ? "true" : "false";
				eventLogger.LogError("ConvertToJs(obj={Obj}): result → {Result}", obj, result);
            }
            else if (objType == typeof(int) || objType == typeof(decimal) || objType == typeof(double))
            {
                result = obj.ToString();
            }
            else
            {
                result = $"'{HttpUtility.JavaScriptStringEncode(obj.ToString())}'";
            }

			eventLogger.LogWarning("<<< Exiting ConvertToJs(obj={Obj})", obj);
            return result;
        }
    }
}
