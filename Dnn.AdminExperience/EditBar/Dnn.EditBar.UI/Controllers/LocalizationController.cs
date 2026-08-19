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
			eventLogger.LogDebug(">>> Entering GetFactory()");
			eventLogger.LogDebug("<<< Exiting GetFactory()");
            return () => new LocalizationController();
        }

        private static string GetNameAttribute(XmlNode node)
        {
			eventLogger.LogDebug(">>> Entering GetNameAttribute(node={Node})", node);
            if (node.Attributes != null)
            {
                var attribute = node.Attributes.GetNamedItem("name");
				eventLogger.LogDebug("GetNameAttribute(node={Node}): attribute → {Attribute}", node, attribute);
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

			eventLogger.LogDebug("<<< Exiting GetNameAttribute(node={Node})", node);
			eventLogger.LogDebug("<<< Exiting GetNameAttribute(navigator={Navigator})", navigator);
            return null;
        }

        private static string GetNameAttribute(XPathNavigator navigator)
        {
            return navigator.GetAttribute("name", string.Empty);
        }

        private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
        {
			eventLogger.LogDebug(">>> Entering AssertHeaderValue(headers={Headers},key={Key},value={Value})", headers, key, value);
            var header = headers.FirstOrDefault(x => GetNameAttribute(x).Equals(key, StringComparison.OrdinalIgnoreCase));
            if (header != null)
            {
                if (!header.InnerText.Equals(value, StringComparison.OrdinalIgnoreCase))
                {
					eventLogger.LogWarning("Throwing LocalizationException from AssertHeaderValue(headers={Headers},key={Key},value={Value})", headers, key, value);
                    throw new LocalizationException($"Resource header '{key}' != '{value}'");
                }
            }
            else
            {
			eventLogger.LogDebug("<<< Exiting AssertHeaderValue(headers={Headers},key={Key},value={Value})", headers, key, value);
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
