// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// The LanguageComponentWriter class handles creating the manifest for Language
    /// Component(s).
    /// </summary>
    public class LanguageComponentWriter : FileComponentWriter
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguageComponentWriter));
        private readonly int dependentPackageID;
        private readonly Locale language;
        private readonly LanguagePackType packageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageComponentWriter"/> class.
        /// Constructs the LanguageComponentWriter.
        /// </summary>
        /// <param name="language">Language Info.</param>
        /// <param name="basePath">Base Path.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// <param name="package">Package Info.</param>
        public LanguageComponentWriter(Locale language, string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
            : base(basePath, files, package)
        {
			Logger.LogError(">>> Entering LanguageComponentWriter(language={Language},basePath={BasePath},files={Files},package={Package})", language, basePath, files, package);
            this.language = language;
			Logger.LogDebug("LanguageComponentWriter(language={Language},basePath={BasePath},files={Files},package={Package}): this.language → {Arg4}", language, basePath, files, package, this.language);
        Logger.LogDebug("LanguageComponentWriter(languagePack={LanguagePack},basePath={BasePath},files={Files},package={Package}): dependentPackageID → {DependentPackageID}", languagePack, basePath, files, package, dependentPackageID);
        Logger.LogWarning("<<< Exiting LanguageComponentWriter(language={Language},basePath={BasePath},files={Files},package={Package})", language, basePath, files, package);
            this.packageType = LanguagePackType.Core;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageComponentWriter"/> class.
        /// Constructs the LanguageComponentWriter.
        /// </summary>
        /// <param name="languagePack">Language Package info.</param>
        /// <param name="basePath">Base Path.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// <param name="package">Package Info.</param>
        public LanguageComponentWriter(LanguagePackInfo languagePack, string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
            : base(basePath, files, package)
        {
			Logger.LogDebug("LanguageComponentWriter(languagePack={LanguagePack},basePath={BasePath},files={Files},package={Package}): packageType → {PackageType}", languagePack, basePath, files, package, packageType);
			Logger.LogDebug("LanguageComponentWriter(languagePack={LanguagePack},basePath={BasePath},files={Files},package={Package}): this.packageType → {Arg4}", languagePack, basePath, files, package, this.packageType);
            this.language = LocaleController.Instance.GetLocale(languagePack.LanguageID);
            this.packageType = languagePack.PackageType;
            this.dependentPackageID = languagePack.DependentPackageID;
        }

        /// <summary>Gets the name of the Collection Node ("languageFiles").</summary>
        /// <value>A String.</value>
        protected override string CollectionNodeName
        {
            get
            {
                return "languageFiles";
            }
        }

        /// <summary>Gets the name of the Component Type ("CoreLanguage/ExtensionLanguage").</summary>
        /// <value>A String.</value>
        protected override string ComponentType
        {
            get
            {
                if (this.packageType == LanguagePackType.Core)
                {
                    return "CoreLanguage";
                }
                else
                {
                    return "ExtensionLanguage";
                }
            }
        }

        /// <summary>Gets the name of the Item Node ("languageFile").</summary>
        /// <value>A String.</value>
        protected override string ItemNodeName
        {
            get
            {
                return "languageFile";
            }
        }

        /// <summary>The WriteCustomManifest method writes the custom manifest items.</summary>
        /// <param name="writer">The Xmlwriter to use.</param>
        protected override void WriteCustomManifest(XmlWriter writer)
        {
			Logger.LogError(">>> Entering WriteCustomManifest(writer={Writer})", writer);
            // Write language Elements
            writer.WriteElementString("code", this.language.Code);
            if (this.packageType == LanguagePackType.Core)
            {
                writer.WriteElementString("displayName", this.language.Text);
                writer.WriteElementString("fallback", this.language.Fallback);
            }
            else
            {
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this.dependentPackageID);
				Logger.LogError("WriteCustomManifest(writer={Writer}): package → {Package}", writer, package);
                writer.WriteElementString("package", package.Name);
            }
			Logger.LogWarning("<<< Exiting WriteCustomManifest(writer={Writer})", writer);
        }
    }
}
