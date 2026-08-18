// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Web.Caching;
    using System.Xml;

    using Dnn.Modules.ResourceManager.Components.Models;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Cache;

    /// <summary>Provides localization services.</summary>
    public class LocalizationController : ServiceLocator<ILocalizationController, LocalizationController>, ILocalizationController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LocalizationController));
        /// <inheritdoc />
        public string CultureName => Instance.CultureName;

        /// <inheritdoc />
        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture) =>
            Instance.GetLocalizedDictionary(resourceFile, culture);

        /// <inheritdoc />
        public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture, Localization localization) =>
            Instance.GetLocalizedDictionary(resourceFile, culture, localization);

        /// <inheritdoc />
        public long GetResxTimeStamp(string resourceFile, Localization localization)
            => Instance.GetResxTimeStamp(resourceFile, localization);

        /// <inheritdoc />
        protected override Func<ILocalizationController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new LocalizationControllerImplementation();
        }

        /// <summary>The localization controller implementation.</summary>
        private class LocalizationControllerImplementation : ILocalizationController
        {
            private static readonly TimeSpan FiveMinutes = TimeSpan.FromMinutes(5);
            private static readonly TimeSpan OneHour = TimeSpan.FromHours(1);

            /// <inheritdoc />
            public string CultureName
            {
                get { return Thread.CurrentThread.CurrentUICulture.Name; }
            }

            /// <inheritdoc />
            public long GetResxTimeStamp(string resourceFile, Localization localization)
            {
				Logger.LogDebug(">>> Entering GetResxTimeStamp(resourceFile={ResourceFile},localization={Localization})", resourceFile, localization);
				Logger.LogDebug("<<< Exiting GetResxTimeStamp(resourceFile={ResourceFile},localization={Localization})", resourceFile, localization);
                return this.GetLastModifiedTime(resourceFile, this.CultureName, localization).Ticks;
            }

            /// <inheritdoc />
            public Dictionary<string, string> GetLocalizedDictionary(string resourceFile, string culture, Localization localization)
            {
                Requires.NotNullOrEmpty("resourceFile", resourceFile);
                Requires.NotNullOrEmpty("culture", culture);

                var cacheKey = string.Format(CultureInfo.CurrentCulture, localization.ResxDataCacheKey, culture, resourceFile);
                if (DataCache.GetCache(cacheKey) is Dictionary<string, string> localizedDict)
                {
                    return localizedDict;
                }

                var dictionary = new Dictionary<string, string>();

                foreach (
                    var kvp in
                        GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary[kvp.Key] = kvp.Value;
                }

                DataCache.SetCache(
                    cacheKey,
                    dictionary,
                    default(DNNCacheDependency),
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

                var cacheKey = string.Format(Constants.LocalizationDataCacheKey, culture, resourceFile);
                var localizedDict = DataCache.GetCache(cacheKey) as Dictionary<string, string>;
                if (localizedDict != null)
                {
                    return localizedDict;
                }

                var dictionary = new Dictionary<string, string>();
                foreach (
                    var kvp in
                        GetLocalizationValues(resourceFile, culture).Where(kvp => !dictionary.ContainsKey(kvp.Key)))
                {
                    dictionary[kvp.Key] = kvp.Value;
                }

                DataCache.SetCache(
                    cacheKey,
                    dictionary,
                    default(DNNCacheDependency),
                    Cache.NoAbsoluteExpiration,
                    Constants.FiveMinutes,
                    CacheItemPriority.Normal,
                    null);

                return dictionary;
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
                return null;
            }

            private static void AssertHeaderValue(IEnumerable<XmlNode> headers, string key, string value)
            {
                var header =
                    headers.FirstOrDefault(
                        x => GetNameAttribute(x).Equals(key, StringComparison.InvariantCultureIgnoreCase));
                if (header != null)
                {
                    if (!header.InnerText.Equals(value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new ApplicationException(string.Format("Resource header '{0}' != '{1}'", key, value));
                    }
                }
                else
                {
				Logger.LogDebug("<<< Exiting AssertHeaderValue(headers={Headers},key={Key},value={Value})", headers, key, value);
                    throw new ApplicationException(string.Format("Resource header '{0}' is missing", key));
                }
            }

            private static IEnumerable<KeyValuePair<string, string>> GetLocalizationValues(string fullPath, string culture)
            {
                using (
                    var stream = new FileStream(
                        System.Web.HttpContext.Current.Server.MapPath(fullPath),
                        FileMode.Open,
                        FileAccess.Read))
                {
                    var document = new XmlDocument { XmlResolver = null };
                    using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { XmlResolver = null, }))
                    {
                        document.Load(xmlReader);
                    }

                    var headers = document.SelectNodes(@"/root/resheader").Cast<XmlNode>().ToArray();
                    AssertHeaderValue(headers, "resmimetype", "text/microsoft-resx");

                    foreach (var xmlNode in document.SelectNodes("/root/data").Cast<XmlNode>())
                    {
                        var name = GetNameAttribute(xmlNode);

                        const string textPostFix = ".Text";
                        if (name.EndsWith(textPostFix))
                        {
                            name = name.Substring(0, name.Length - textPostFix.Length);
                        }

                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }

                        var key = name;
                        if (key.Contains("."))
                        {
                            key = name + textPostFix;
                        }

                        var value = DotNetNuke.Services.Localization.Localization.GetString(key, fullPath, culture);

                        yield return new KeyValuePair<string, string>(name, value);
                    }
                }
            }

            private DateTime GetLastModifiedTime(string resourceFile, string culture, Localization localization)
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

                var lastModifiedDate = this.GetLastModifiedTimeInternal(resourceFile, culture);
				Logger.LogDebug("GetLastModifiedTime(resourceFile={ResourceFile},culture={Culture},localization={Localization}): lastModifiedDate → {LastModifiedDate}", resourceFile, culture, localization, lastModifiedDate);

                DataCache.SetCache(
                    cacheKey,
                    lastModifiedDate,
                    default(DNNCacheDependency),
                    Cache.NoAbsoluteExpiration,
                    OneHour,
                    CacheItemPriority.Normal,
                    null);

				Logger.LogDebug("<<< Exiting GetLastModifiedTime(resourceFile={ResourceFile},culture={Culture},localization={Localization})", resourceFile, culture, localization);
                return lastModifiedDate;
            }

            private DateTime GetLastModifiedTimeInternal(string resourceFile, string culture)
            {
				Logger.LogDebug(">>> Entering GetLastModifiedTimeInternal(resourceFile={ResourceFile},culture={Culture})", resourceFile, culture);
                var cultureSpecificFile = System.Web.HttpContext.Current.Server.MapPath(
                    resourceFile.Replace(".resx", string.Empty) + "." + culture + ".resx");
                var lastModifiedDate = DateTime.MinValue;
				Logger.LogDebug("GetLastModifiedTimeInternal(resourceFile={ResourceFile},culture={Culture}): lastModifiedDate → {LastModifiedDate}", resourceFile, culture, lastModifiedDate);

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
}
