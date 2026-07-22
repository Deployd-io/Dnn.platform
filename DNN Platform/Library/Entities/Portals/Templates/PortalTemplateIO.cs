// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Internal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;

    /// <inheritdoc cref="IPortalTemplateIO" />
    [DnnDeprecated(9, 11, 1, "No replacement")]
    public partial class PortalTemplateIO : ServiceLocator<IPortalTemplateIO, PortalTemplateIO>, IPortalTemplateIO
    {
        /// <inheritdoc />
        public IEnumerable<string> EnumerateTemplates()
        {
			Logger.LogDebug(">>> Entering EnumerateTemplates()");
            string path = Globals.HostMapPath;
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.template").Where(x => Path.GetFileNameWithoutExtension(x) != "admin");
            }

			Logger.LogDebug("<<< Exiting EnumerateTemplates()");
            return [];
        }

        /// <inheritdoc />
        public IEnumerable<string> EnumerateLanguageFiles()
        {
			Logger.LogDebug(">>> Entering EnumerateLanguageFiles()");
            string path = Globals.HostMapPath;
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.template.??-??.resx");
            }

			Logger.LogDebug("<<< Exiting EnumerateLanguageFiles()");
            return [];
        }

        /// <inheritdoc />
        public string GetResourceFilePath(string templateFilePath)
        {
			Logger.LogDebug(">>> Entering GetResourceFilePath(templateFilePath={TemplateFilePath})", templateFilePath);
			Logger.LogDebug("<<< Exiting GetResourceFilePath(templateFilePath={TemplateFilePath})", templateFilePath);
            return CheckFilePath(templateFilePath + ".resources");
        }

        /// <inheritdoc />
        public string GetLanguageFilePath(string templateFilePath, string cultureCode)
        {
			Logger.LogDebug(">>> Entering GetLanguageFilePath(templateFilePath={TemplateFilePath},cultureCode={CultureCode})", templateFilePath, cultureCode);
			Logger.LogDebug("<<< Exiting GetLanguageFilePath(templateFilePath={TemplateFilePath},cultureCode={CultureCode})", templateFilePath, cultureCode);
            return CheckFilePath($"{templateFilePath}.{cultureCode}.resx");
        }

        /// <inheritdoc />
        public TextReader OpenTextReader(string filePath)
        {
			Logger.LogDebug(">>> Entering OpenTextReader(filePath={FilePath})", filePath);
            StreamReader reader = null;

            var retryable = new RetryableAction(
                () => reader = new StreamReader(File.Open(filePath, FileMode.Open)),
                filePath,
                10,
                TimeSpan.FromMilliseconds(50),
                2);

            retryable.TryIt();
			Logger.LogDebug("<<< Exiting OpenTextReader(filePath={FilePath})", filePath);
            return reader;
        }

        /// <inheritdoc />
        public (string, List<string>) GetTemplateLanguages(string templateFilePath)
        {
            var defaultLanguage = string.Empty;
            var locales = new List<string>();
            var templateXml = new XmlDocument { XmlResolver = null };
            using (var templateReader = XmlReader.Create(templateFilePath, new XmlReaderSettings { XmlResolver = null, }))
            {
                templateXml.Load(templateReader);
            }

            var node = templateXml.SelectSingleNode("//settings/defaultlanguage");
            if (node != null)
            {
				private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
                defaultLanguage = node.InnerText;
            }

            node = templateXml.SelectSingleNode("//locales");
            if (node != null)
            {
                foreach (XmlNode lnode in node.SelectNodes("//locale"))
                {
                    locales.Add(lnode.InnerText);
                }
            }

            return (defaultLanguage, locales);
        }

        /// <inheritdoc />
        protected override Func<IPortalTemplateIO> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new PortalTemplateIO();
        }

        private static string CheckFilePath(string path)
        {
			Logger.LogDebug(">>> Entering CheckFilePath(path={Path})", path);
			Logger.LogDebug("<<< Exiting CheckFilePath(path={Path})", path);
            return File.Exists(path) ? path : string.Empty;
        }
    }
}
