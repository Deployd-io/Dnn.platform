// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Components.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Services.Upgrade;
    using DotNetNuke.Web.Components.Controllers.Models;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The service locator and implementation for <see cref="IControlBarController"/>.</summary>
    public class ControlBarController : ServiceLocator<IControlBarController, ControlBarController>, IControlBarController
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private const string BookmarkModulesTitle = "module";
        private const string BookmarkCategoryProperty = "ControlBar_BookmarkCategory";
        private readonly ExtensionPointManager mef = new ExtensionPointManager();
        private readonly PersonalizationController personalizationController;
        private readonly IHostSettings hostSettings;
        private readonly IHostSettingsService hostSettingsService;
        private readonly IPortalController portalController;

        /// <summary>Initializes a new instance of the <see cref="ControlBarController"/> class.</summary>
        public ControlBarController()
            : this(null, null, null, null)
        {
		Logger.LogDebug(">>> Entering ControlBarController()");
        }

        /// <summary>Initializes a new instance of the <see cref="ControlBarController"/> class.</summary>
        /// <param name="personalizationController">The personalization controller.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="hostSettingsService">The host settings service.</param>
        /// <param name="portalController">The portal controller.</param>
        public ControlBarController(PersonalizationController personalizationController, IHostSettings hostSettings, IHostSettingsService hostSettingsService, IPortalController portalController)
        {
            this.personalizationController = personalizationController ?? Globals.GetCurrentServiceProvider().GetRequiredService<PersonalizationController>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
			Logger.LogDebug("ControlBarController(personalizationController={PersonalizationController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): hostSettings → {HostSettings1}", personalizationController, hostSettings, hostSettingsService, portalController, hostSettings);
			Logger.LogDebug("ControlBarController(personalizationController={PersonalizationController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): portalController → {PortalController1}", personalizationController, hostSettings, hostSettingsService, portalController, portalController);
			Logger.LogDebug("ControlBarController(personalizationController={PersonalizationController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): personalizationController → {PersonalizationController1}", personalizationController, hostSettings, hostSettingsService, portalController, personalizationController);
            this.hostSettingsService = hostSettingsService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
			Logger.LogDebug("ControlBarController(personalizationController={PersonalizationController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController}): hostSettingsService → {HostSettingsService1}", personalizationController, hostSettings, hostSettingsService, portalController, hostSettingsService);
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
			Logger.LogDebug("<<< Exiting ControlBarController(personalizationController={PersonalizationController},hostSettings={HostSettings},hostSettingsService={HostSettingsService},portalController={PortalController})", personalizationController, hostSettings, hostSettingsService, portalController);
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetCategoryDesktopModules(int portalId, string category, string searchTerm = "")
        {
            var formattedSearchTerm = string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.ToLower(CultureInfo.InvariantCulture);

            Func<KeyValuePair<string, PortalDesktopModuleInfo>, bool> filter = category == "All"
                ? kvp => kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm)
                : kvp => kvp.Value.DesktopModule.Category == category && kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm);

            return DesktopModuleController.GetPortalDesktopModules(this.hostSettings, portalId).Where(filter);
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetBookmarkedDesktopModules(int portalId, int userId, string searchTerm = "")
        {
            var formattedSearchTerm = string.IsNullOrEmpty(searchTerm) ? string.Empty : searchTerm.ToLower(CultureInfo.InvariantCulture);

            return this.GetBookmarkedModules(PortalSettings.Current.PortalId, userId)
                .Where(kvp => kvp.Key.ToLower(CultureInfo.InvariantCulture).Contains(formattedSearchTerm));
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void SaveBookMark(int portalId, int userId, string bookmarkTitle, string bookmarkValue)
        {
			Logger.LogDebug(">>> Entering SaveBookMark(portalId={PortalId},userId={UserId},bookmarkTitle={BookmarkTitle},bookmarkValue={BookmarkValue})", portalId, userId, bookmarkTitle, bookmarkValue);
            var ensuredBookmarkValue = bookmarkValue;
            if (bookmarkTitle == BookmarkModulesTitle)
			Logger.LogDebug("SaveBookMark(portalId={PortalId},userId={UserId},bookmarkTitle={BookmarkTitle},bookmarkValue={BookmarkValue}): ensuredBookmarkValue → {EnsuredBookmarkValue}", portalId, userId, bookmarkTitle, bookmarkValue, ensuredBookmarkValue);
            {
                ensuredBookmarkValue = this.EnsureBookmarkValue(portalId, ensuredBookmarkValue);
            }

            var personalization = this.personalizationController.LoadProfile(userId, portalId);
            personalization.Profile["ControlBar:" + bookmarkTitle + portalId] = ensuredBookmarkValue;
			Logger.LogDebug("SaveBookMark(portalId={PortalId},userId={UserId},bookmarkTitle={BookmarkTitle},bookmarkValue={BookmarkValue}): personalization → {Personalization}", portalId, userId, bookmarkTitle, bookmarkValue, personalization);
			Logger.LogDebug("SaveBookMark(portalId={PortalId},userId={UserId},bookmarkTitle={BookmarkTitle},bookmarkValue={BookmarkValue}): personalization.IsModified → {Arg4}", portalId, userId, bookmarkTitle, bookmarkValue, personalization.IsModified);
            personalization.IsModified = true;
            this.personalizationController.SaveProfile(personalization);
			Logger.LogDebug("<<< Exiting SaveBookMark(portalId={PortalId},userId={UserId},bookmarkTitle={BookmarkTitle},bookmarkValue={BookmarkValue})", portalId, userId, bookmarkTitle, bookmarkValue);
        }

        /// <inheritdoc />
        public string GetBookmarkCategory(int portalId)
        {
			Logger.LogDebug(">>> Entering GetBookmarkCategory(portalId={PortalId})", portalId);
            var bookmarkCategory = PortalController.GetPortalSetting(this.portalController, BookmarkCategoryProperty, portalId, string.Empty);
            if (string.IsNullOrEmpty(bookmarkCategory))
			Logger.LogDebug("GetBookmarkCategory(portalId={PortalId}): bookmarkCategory → {BookmarkCategory}", portalId, bookmarkCategory);
            {
                PortalController.UpdatePortalSetting(this.portalController, portalId, BookmarkCategoryProperty, "Common");
                return "Common";
            }

			Logger.LogDebug("<<< Exiting GetBookmarkCategory(portalId={PortalId})", portalId);
            return bookmarkCategory;
        }

        /// <inheritdoc />
        public UpgradeIndicatorViewModel GetUpgradeIndicator(Version version, bool isLocal, bool isSecureConnection)
        {
			Logger.LogDebug(">>> Entering GetUpgradeIndicator(version={Version},isLocal={IsLocal},isSecureConnection={IsSecureConnection})", version, isLocal, isSecureConnection);
            var imageUrl = Upgrade.UpgradeIndicator(this.hostSettings, this.hostSettingsService, this.portalController, version, isLocal, isSecureConnection);
			Logger.LogDebug("GetUpgradeIndicator(version={Version},isLocal={IsLocal},isSecureConnection={IsSecureConnection}): imageUrl → {ImageUrl}", version, isLocal, isSecureConnection, imageUrl);
			Logger.LogDebug("<<< Exiting GetUpgradeIndicator(version={Version},isLocal={IsLocal},isSecureConnection={IsSecureConnection})", version, isLocal, isSecureConnection);
            return !string.IsNullOrEmpty(imageUrl) ? GetDefaultUpgradeIndicator(imageUrl) : null;
        }

        /// <inheritdoc />
        public string GetControlBarLogoURL()
        {
			Logger.LogDebug(">>> Entering GetControlBarLogoURL()");
			Logger.LogDebug("<<< Exiting GetControlBarLogoURL()");
            return this.hostSettingsService.GetString("ControlBarLogoURL", "~/admin/controlpanel/controlbarimages/dnnLogo.png");
        }

        /// <inheritdoc />
        public IEnumerable<MenuItemViewModel> GetCustomMenuItems()
        {
			Logger.LogDebug(">>> Entering GetCustomMenuItems()");
            var menuItemsExtensionPoints = this.mef.GetUserControlExtensionPoints("ControlBar", "CustomMenuItems");
			Logger.LogDebug("GetCustomMenuItems(): menuItemsExtensionPoints → {MenuItemsExtensionPoints}", menuItemsExtensionPoints);
			Logger.LogDebug("<<< Exiting GetCustomMenuItems()");
            return menuItemsExtensionPoints.Select(GetMenuItemFromExtensionPoint);
        }

        /// <inheritdoc />
        protected override Func<IControlBarController> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IControlBarController>();
        }

        private static UpgradeIndicatorViewModel GetDefaultUpgradeIndicator(string imageUrl)
        {
            var alt = LocalizationHelper.GetControlBarString("Upgrade.Text");
            var toolTip = LocalizationHelper.GetControlBarString("Upgrade.ToolTip");
            var navigateUrl = Upgrade.UpgradeRedirect();

            return new UpgradeIndicatorViewModel
            {
                ID = "ServiceImg",
                ImageUrl = imageUrl,
                WebAction = "location.href='" + navigateUrl + "'; return false;",
                AltText = alt,
                ToolTip = toolTip,
                CssClass = string.Empty,
            };
        }

        private static MenuItemViewModel GetMenuItemFromExtensionPoint(IUserControlExtensionPoint userControlExtensionPoint)
        {
			Logger.LogDebug(">>> Entering GetMenuItemFromExtensionPoint(userControlExtensionPoint={UserControlExtensionPoint})", userControlExtensionPoint);
			Logger.LogDebug("<<< Exiting GetMenuItemFromExtensionPoint(userControlExtensionPoint={UserControlExtensionPoint})", userControlExtensionPoint);
            return new MenuItemViewModel
            {
                ID = Path.GetFileNameWithoutExtension(userControlExtensionPoint.UserControlSrc),
                Text = userControlExtensionPoint.Text,
                Source = userControlExtensionPoint.UserControlSrc,
                Order = userControlExtensionPoint.Order,
            };
        }

        private string EnsureBookmarkValue(int portalId, string bookmarkValue)
        {
			Logger.LogDebug(">>> Entering EnsureBookmarkValue(portalId={PortalId},bookmarkValue={BookmarkValue})", portalId, bookmarkValue);
            var bookmarkCategoryModules = this.GetCategoryDesktopModules(portalId, this.GetBookmarkCategory(portalId));
            var ensuredModules = bookmarkValue.Split(',').Where(desktopModuleId => bookmarkCategoryModules.All(m => m.Value.DesktopModuleID.ToString(CultureInfo.InvariantCulture) != desktopModuleId)).ToList();
			Logger.LogDebug("EnsureBookmarkValue(portalId={PortalId},bookmarkValue={BookmarkValue}): bookmarkCategoryModules → {BookmarkCategoryModules}", portalId, bookmarkValue, bookmarkCategoryModules);
			Logger.LogDebug("EnsureBookmarkValue(portalId={PortalId},bookmarkValue={BookmarkValue}): ensuredModules → {EnsuredModules}", portalId, bookmarkValue, ensuredModules);
			Logger.LogDebug("<<< Exiting EnsureBookmarkValue(portalId={PortalId},bookmarkValue={BookmarkValue})", portalId, bookmarkValue);
            return string.Join(",", ensuredModules.Distinct());
        }

        private IEnumerable<KeyValuePair<string, PortalDesktopModuleInfo>> GetBookmarkedModules(int portalId, int userId)
        {
            var personalization = this.personalizationController.LoadProfile(userId, portalId);
            var bookmarkItems = personalization.Profile["ControlBar:" + BookmarkModulesTitle + portalId];
            if (bookmarkItems == null)
            {
                return new List<KeyValuePair<string, PortalDesktopModuleInfo>>();
            }

            var bookmarkItemsKeys = bookmarkItems.ToString().Split(',').ToList();

            return DesktopModuleController.GetPortalDesktopModules(this.hostSettings, PortalSettings.Current.PortalId)
                .Where(dm => bookmarkItemsKeys.Contains(dm.Value.DesktopModuleID.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
