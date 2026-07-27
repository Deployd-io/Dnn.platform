// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Caching;
    using System.Xml;
    using System.Xml.XPath;

    using Dnn.PersonaBar.Library.Controllers;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Cache;

    internal sealed class LocalizationController : ServiceLocator<ILocalizationController, LocalizationController>, ILocalizationController
    {
        public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

        /// <inheritdoc />
        public string CultureName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name; }
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture)
        {
            Requires.NotNullOrEmpty("resourceFile", resourceFile);
            Requires.NotNullOrEmpty("culture", culture);

            var dictionary = new Dictionary<string, string>();
            foreach (var kvp in GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
            {
                dictionary[kvp.Key] = kvp.Value;
            }

            return dictionary;
        }

        /// <inheritdoc />
        protected override Func<ILocalizationController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new LocalizationController();
        }

        private static string GetNameAttribute(XmlNode node)
        {
			Logger.LogDebug(">>> Entering GetNameAttribute(node={Node})", node);
            if (node.Attributes != null)
            {
                var attribute = node.Attributes.GetNamedItem("name");
                if (attribute != null)
				Logger.LogDebug("GetNameAttribute(node={Node}): attribute → {Attribute}", node, attribute);
                {
                    return attribute.Value;
                }
            }

			Logger.LogDebug("<<< Exiting GetNameAttribute(node={Node})", node);
            return null;
        }

        private static string GetNameAttribute(XPathNavigator navigator)
        {
			Logger.LogDebug("<<< Exiting GetNameAttribute(navigator={Navigator})", navigator);
            return navigator.GetAttribute("name", string.Empty);
        }

        private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
        {
            var header = headers.FirstOrDefault(x => GetNameAttribute(x).Equals(key, StringComparison.OrdinalIgnoreCase));
            if (header != null)
            {
                if (!header.InnerText.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new LocalizationException($"Resource header '{key}' != '{value}'");
                }
            }
            else
            {
                throw new LocalizationException($"Resource header '{key}' is missing");
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath, string culture)
        {
            var document = new XmlDocument();
            using (var stream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(fullPath), FileMode.Open, FileAccess.Read))
            using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { XmlResolver = null, }))
            {
                document.Load(xmlReader);
            }

            // ReSharper disable once AssignNullToNotNullAttribute
            var headers = document.SelectNodes(@"/root/resheader").Cast<XmlNode>().ToArray();

            AssertHeaderValue(headers, "resmimetype", "text/microsoft-resx");

            // ReSharper disable once AssignNullToNotNullAttribute
            foreach (XPathNavigator navigator in document.CreateNavigator().Select("/root/data"))
            {
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                if (navigator.NodeType == XPathNodeType.Comment)
                {
                    continue;
                }

                var name = GetNameAttribute(navigator);

                const string textPostFix = ".Text";
                if (name.EndsWith(textPostFix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(0, name.Length - textPostFix.Length);
                }

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                var valueNode = navigator.SelectSingleNode("value");
                if (valueNode != null)
                {
                    yield return new KeyValuePair<string, string>(name, valueNode.Value);
                }
            }
        }
    }
}
