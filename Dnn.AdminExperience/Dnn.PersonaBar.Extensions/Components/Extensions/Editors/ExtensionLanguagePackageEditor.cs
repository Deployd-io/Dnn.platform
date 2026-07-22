// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.Editors
{
    using System;
    using System.Globalization;

    using Dnn.PersonaBar.Extensions.Components.Dto;
    using Dnn.PersonaBar.Extensions.Components.Dto.Editors;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public class ExtensionLanguagePackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JsLibraryPackageEditor));

        /// <summary>Initializes a new instance of the <see cref="ExtensionLanguagePackageEditor"/> class.</summary>
        public ExtensionLanguagePackageEditor()
        {
			Logger.LogDebug(">>> Entering ExtensionLanguagePackageEditor()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
			Logger.LogDebug("<<< Exiting ExtensionLanguagePackageEditor()");
        }

        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc />
        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
			Logger.LogDebug(">>> Entering GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            var languagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
            var languagesTab = TabController.GetTabByTabPath(portalId, "//Admin//Languages", Null.NullString);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): languagePack → {LanguagePack}", portalId, package, languagePack);
			Logger.LogDebug("GetPackageDetail(portalId={PortalId},package={Package}): languagesTab → {LanguagesTab}", portalId, package, languagesTab);

            var detail = new ExtensionLanguagePackageDetailDto(portalId, package)
            {
                Locales = Utility.GetAllLanguagesList(),
                LanguageId = languagePack.LanguageID,
                DependentPackageId = languagePack.DependentPackageID,
                EditUrlFormat = this.NavigationManager.NavigateURL(languagesTab, string.Empty, "Locale={0}"),
            };

            if (languagePack.PackageType == LanguagePackType.Extension)
            {
                // Get all the packages but only bind to combo if not a language package
                detail.Packages = Utility.GetAllPackagesListExceptLangPacks();
            }

			Logger.LogDebug("<<< Exiting GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            return detail;
        }

        /// <inheritdoc />
        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
			Logger.LogDebug(">>> Entering SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
            errorMessage = string.Empty;

			Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): errorMessage → {ErrorMessage}", packageSettings, errorMessage);
            try
            {
                var changed = false;
                var languagePack = LanguagePackController.GetLanguagePackByPackage(packageSettings.PackageId);
				Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): changed → {Changed}", packageSettings, changed);
				Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): languagePack → {LanguagePack}", packageSettings, languagePack);
                if (packageSettings.EditorActions.TryGetValue("languageId", out var value)
                    && !string.IsNullOrEmpty(value) && value != languagePack.LanguageID.ToString(CultureInfo.InvariantCulture))
                {
                    languagePack.LanguageID = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    changed = true;
                }

                if (packageSettings.EditorActions.TryGetValue("dependentPackageId", out value)
                    && !string.IsNullOrEmpty(value) && value != languagePack.DependentPackageID.ToString(CultureInfo.InvariantCulture))
                {
                    languagePack.DependentPackageID = Convert.ToInt32(value, CultureInfo.InvariantCulture);
                    changed = true;
                }

                if (changed)
                {
                    LanguagePackController.SaveLanguagePack(languagePack);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.Message;
				Logger.LogDebug("<<< Exiting SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
                return false;
            }
        }
    }
}
