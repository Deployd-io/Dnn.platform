// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Templates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;

    public class PortalTemplateInfo : IPortalTemplateInfo
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));
        private string resourceFilePath;

        /// <summary>Initializes a new instance of the <see cref="PortalTemplateInfo"/> class.</summary>
        /// <param name="templateFilePath">The path to the template file.</param>
        /// <param name="cultureCode">The culture code.</param>
        public PortalTemplateInfo(string templateFilePath, string cultureCode)
        {
			Logger.LogDebug(">>> Entering PortalTemplateInfo(templateFilePath={TemplateFilePath},cultureCode={CultureCode})", templateFilePath, cultureCode);
            this.TemplateFilePath = templateFilePath;

			Logger.LogDebug("PortalTemplateInfo(templateFilePath={TemplateFilePath},cultureCode={CultureCode}): this.TemplateFilePath → {Arg2}", templateFilePath, cultureCode, this.TemplateFilePath);
            this.InitLocalizationFields(cultureCode);
            this.InitNameAndDescription();
			Logger.LogDebug("<<< Exiting PortalTemplateInfo(templateFilePath={TemplateFilePath},cultureCode={CultureCode})", templateFilePath, cultureCode);
        }

        public string ResourceFilePath
        {
            get
            {
                if (this.resourceFilePath == null)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    this.resourceFilePath = PortalTemplateIO.Instance.GetResourceFilePath(this.TemplateFilePath);
#pragma warning restore CS0618 // Type or member is obsolete
                }

                return this.resourceFilePath;
            }
        }

        public string Name { get; private set; }

        public string CultureCode { get; private set; }

        public string TemplateFilePath { get; private set; }

        public string LanguageFilePath { get; private set; }

        public string Description { get; private set; }

        private static string ReadLanguageFileValue(XDocument xmlDoc, string name)
        {
            return (from f in xmlDoc.Descendants("data")
                    where (string)f.Attribute("name") == name
                    select (string)f.Element("value")).SingleOrDefault();
        }

        private void InitNameAndDescription()
        {
			Logger.LogDebug(">>> Entering InitNameAndDescription()");
            if (!string.IsNullOrEmpty(this.LanguageFilePath))
            {
                this.LoadNameAndDescriptionFromLanguageFile();
            }

            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = Path.GetFileNameWithoutExtension(this.TemplateFilePath);
            }

            if (string.IsNullOrEmpty(this.Description))
            {
                this.LoadDescriptionFromTemplateFile();
            }
			Logger.LogDebug("<<< Exiting InitNameAndDescription()");
        }

        private void LoadDescriptionFromTemplateFile()
        {
			Logger.LogDebug(">>> Entering LoadDescriptionFromTemplateFile()");
            try
            {
                XDocument xmlDoc;
                using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.TemplateFilePath))
                {
                    xmlDoc = XDocument.Load(reader);
                }

                this.Description = xmlDoc.Elements("portal").Elements("description").SingleOrDefault().Value;
            }
            catch (Exception e)
            {
                Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
            }
			Logger.LogDebug("<<< Exiting LoadDescriptionFromTemplateFile()");
        }

        private void LoadNameAndDescriptionFromLanguageFile()
        {
			Logger.LogDebug(">>> Entering LoadNameAndDescriptionFromLanguageFile()");
            try
            {
                using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.LanguageFilePath))
                {
                    var xmlDoc = XDocument.Load(reader);
					Logger.LogDebug("LoadNameAndDescriptionFromLanguageFile(): xmlDoc → {XmlDoc}", xmlDoc);

                    this.Name = ReadLanguageFileValue(xmlDoc, "LocalizedTemplateName.Text");
                    this.Description = ReadLanguageFileValue(xmlDoc, "PortalDescription.Text");
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
            }
			Logger.LogDebug("<<< Exiting LoadNameAndDescriptionFromLanguageFile()");
        }

        private void InitLocalizationFields(string cultureCode)
        {
			Logger.LogDebug(">>> Entering InitLocalizationFields(cultureCode={CultureCode})", cultureCode);
            this.LanguageFilePath = PortalTemplateIO.Instance.GetLanguageFilePath(this.TemplateFilePath, cultureCode);
            if (string.IsNullOrEmpty(this.LanguageFilePath))
            {
                var locales = new List<string>();
				Logger.LogDebug("InitLocalizationFields(cultureCode={CultureCode}): locales → {Locales}", cultureCode, locales);
                (cultureCode, locales) = PortalTemplateIO.Instance.GetTemplateLanguages(this.TemplateFilePath);
                if (string.IsNullOrEmpty(cultureCode))
                {
                    var portalSettings = PortalSettings.Current;
                    cultureCode = portalSettings != null ? PortalController.GetPortalDefaultLanguage(portalSettings.PortalId) : Localization.SystemLocale;
                }
            }

            this.CultureCode = cultureCode;
			Logger.LogDebug("InitLocalizationFields(cultureCode={CultureCode}): this.CultureCode → {Arg1}", cultureCode, this.CultureCode);
			Logger.LogDebug("<<< Exiting InitLocalizationFields(cultureCode={CultureCode})", cultureCode);
        }
    }
}
