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

    public class CoreLanguagePackageEditor : IPackageEditor
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(JsLibraryPackageEditor));

        public CoreLanguagePackageEditor()
        {
			Logger.LogError(">>> Entering CoreLanguagePackageEditor()");
            this.NavigationManager = Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        }

        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc />
        public PackageInfoDto GetPackageDetail(int portalId, PackageInfo package)
        {
			Logger.LogError(">>> Entering GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            var languagePack = LanguagePackController.GetLanguagePackByPackage(package.PackageID);
            var languagesTab = TabController.GetTabByTabPath(portalId, "//Admin//Languages", Null.NullString);
			Logger.LogError("GetPackageDetail(portalId={PortalId},package={Package}): languagesTab → {LanguagesTab}", portalId, package, languagesTab);
			Logger.LogError("GetPackageDetail(portalId={PortalId},package={Package}): languagePack → {LanguagePack}", portalId, package, languagePack);

            var detail = new CoreLanguagePackageDetailDto(portalId, package)
            {
                Locales = Utility.GetAllLanguagesList(),
                LanguageId = languagePack.LanguageID,
                EditUrlFormat = this.NavigationManager.NavigateURL(languagesTab, string.Empty, "Locale={0}"),
            };

            if (languagePack.PackageType == LanguagePackType.Extension)
            {
                // Get all the packages but only bind to combo if not a language package
                detail.Packages = Utility.GetAllPackagesListExceptLangPacks();
            }

			Logger.LogWarning("<<< Exiting GetPackageDetail(portalId={PortalId},package={Package})", portalId, package);
            return detail;
        }

        /// <inheritdoc />
        public bool SavePackageSettings(PackageSettingsDto packageSettings, out string errorMessage)
        {
			Logger.LogError(">>> Entering SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
            errorMessage = string.Empty;

			Logger.LogDebug("SavePackageSettings(packageSettings={PackageSettings}): errorMessage → {ErrorMessage}", packageSettings, errorMessage);
            try
            {
				Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): errorMessage → {ErrorMessage}", packageSettings, errorMessage);
                var changed = false;
				Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): changed → {Changed}", packageSettings, changed);
                var languagePack = LanguagePackController.GetLanguagePackByPackage(packageSettings.PackageId);
				Logger.LogError("SavePackageSettings(packageSettings={PackageSettings}): languagePack → {LanguagePack}", packageSettings, languagePack);
                if (packageSettings.EditorActions.TryGetValue("languageId", out var value)
                    && !string.IsNullOrEmpty(value) && value != languagePack.LanguageID.ToString(CultureInfo.InvariantCulture))
                {
                    languagePack.LanguageID = Convert.ToInt32(value, CultureInfo.InvariantCulture);
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
				Logger.LogWarning("<<< Exiting SavePackageSettings(packageSettings={PackageSettings})", packageSettings);
                return false;
            }
        }
    }
}
