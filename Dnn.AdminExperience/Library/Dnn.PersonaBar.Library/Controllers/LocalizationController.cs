// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Caching;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Cache;

    public class LocalizationController : ServiceLocator<ILocalizationController, LocalizationController>, ILocalizationController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalizationController));
        public static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

        /// <inheritdoc />
        public string CultureName
        {
            get { return Thread.CurrentThread.CurrentUICulture.Name; }
        }

        /// <inheritdoc />
        public long GetResxTimeStamp(string resourceFile, Dto.Localization localization)
        {
			Logger.LogDebug(">>> Entering GetResxTimeStamp(resourceFile={ResourceFile},localization={Localization})", resourceFile, localization);
			Logger.LogDebug("<<< Exiting GetResxTimeStamp(resourceFile={ResourceFile},localization={Localization})", resourceFile, localization);
            return GetLastModifiedTime(resourceFile, this.CultureName, localization).Ticks;
        }

        /// <inheritdoc />
        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture, Dto.Localization localization)
        {
            Requires.NotNullOrEmpty("resourceFile", resourceFile);
            Requires.NotNullOrEmpty("culture", culture);

            var cacheKey = string.Format(CultureInfo.CurrentCulture, localization.ResxDataCacheKey, culture, resourceFile);
            if (DataCache.GetCache(cacheKey) is Dictionary<string, string> localizedDict)
            {
                return localizedDict;
            }

            var dictionary = new Dictionary<string, string>();

            foreach (var kvp in GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
            {
                dictionary[kvp.Key] = kvp.Value;
            }

            DataCache.SetCache(
                cacheKey,
                dictionary,
                (DNNCacheDependency)null,
                Cache.NoAbsoluteExpiration,
                FiveMinutes,
                CacheItemPriority.Normal,
                null);

            return dictionary;
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
				Logger.LogDebug("GetNameAttribute(node={Node}): attribute → {Attribute}", node, attribute);
                if (attribute != null)
                {
                    return attribute.Value;
                }
            }

			Logger.LogDebug("<<< Exiting GetNameAttribute(node={Node})", node);
			Logger.LogDebug("<<< Exiting GetNameAttribute(navigator={Navigator})", navigator);
            return null;
        }

        private static string GetNameAttribute(XPathNavigator navigator)
        {
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
			Logger.LogDebug("<<< Exiting AssertHeaderValue(headers={Headers},key={Key},value={Value})", headers, key, value);
                throw new LocalizationException($"Resource header '{key}' is missing");
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath, string culture)
        {
            var document = new XmlDocument { XmlResolver = null };
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
                if (name.EndsWith(textPostFix, StringComparison.Ordinal))
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

        private static DateTime GetLastModifiedTime(string resourceFile, string culture, Dto.Localization localization)
        {
			Logger.LogDebug(">>> Entering GetLastModifiedTime(resourceFile={ResourceFile},culture={Culture},localization={Localization})", resourceFile, culture, localization);
            Requires.NotNullOrEmpty("culture", culture);

            var cacheKey = string.Format(CultureInfo.CurrentCulture, localization.ResxModifiedDateCacheKey, culture);
			Logger.LogDebug("GetLastModifiedTime(resourceFile={ResourceFile},culture={Culture},localization={Localization}): cacheKey → {CacheKey}", resourceFile, culture, localization, cacheKey);
            var cachedData = DataCache.GetCache(cacheKey);
			Logger.LogDebug("GetLastModifiedTime(resourceFile={ResourceFile},culture={Culture},localization={Localization}): cachedData → {CachedData}", resourceFile, culture, localization, cachedData);
            if (cachedData is DateTime)
            {
                return (DateTime)DataCache.GetCache(cacheKey);
            }

            var lastModifiedDate = GetLastModifiedTimeInternal(resourceFile, culture);
			Logger.LogDebug("GetLastModifiedTime(resourceFile={ResourceFile},culture={Culture},localization={Localization}): lastModifiedDate → {LastModifiedDate}", resourceFile, culture, localization, lastModifiedDate);

            DataCache.SetCache(
                cacheKey,
                lastModifiedDate,
                (DNNCacheDependency)null,
                Cache.NoAbsoluteExpiration,
                OneHour,
                CacheItemPriority.Normal,
                null);

			Logger.LogDebug("<<< Exiting GetLastModifiedTime(resourceFile={ResourceFile},culture={Culture},localization={Localization})", resourceFile, culture, localization);
            return lastModifiedDate;
        }

        private static DateTime GetLastModifiedTimeInternal(string resourceFile, string culture)
        {
			Logger.LogDebug(">>> Entering GetLastModifiedTimeInternal(resourceFile={ResourceFile},culture={Culture})", resourceFile, culture);
            var cultureSpecificFile = System.Web.HttpContext.Current.Server.MapPath($"{resourceFile.Replace(".resx", string.Empty)}.{culture}.resx");
            var lastModifiedDate = DateTime.MinValue;
			Logger.LogDebug("GetLastModifiedTimeInternal(resourceFile={ResourceFile},culture={Culture}): lastModifiedDate → {LastModifiedDate}", resourceFile, culture, lastModifiedDate);
			Logger.LogDebug("GetLastModifiedTimeInternal(resourceFile={ResourceFile},culture={Culture}): cultureSpecificFile → {CultureSpecificFile}", resourceFile, culture, cultureSpecificFile);

            if (File.Exists(cultureSpecificFile))
            {
                lastModifiedDate = File.GetLastWriteTime(cultureSpecificFile);
            }
            else
            {
                var cultureNeutralFile = System.Web.HttpContext.Current.Server.MapPath(resourceFile);
				Logger.LogDebug("GetLastModifiedTimeInternal(resourceFile={ResourceFile},culture={Culture}): cultureNeutralFile → {CultureNeutralFile}", resourceFile, culture, cultureNeutralFile);
                if (File.Exists(cultureNeutralFile))
                {
                    lastModifiedDate = File.GetLastWriteTime(cultureNeutralFile);
                }
            }

			Logger.LogDebug("<<< Exiting GetLastModifiedTimeInternal(resourceFile={ResourceFile},culture={Culture})", resourceFile, culture);
            return lastModifiedDate;
        }
    }
}
