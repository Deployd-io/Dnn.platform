// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Components.Checks
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Pages.Components;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    /// <summary>
    /// A security check for permissions on the User Profile page defined in
    /// Site Settings > Site Behavior > Default Pages > User Profile Page,
    /// according to the following criteria:
    /// Case 1: the selected user profile page is "Activity Feed"
    /// * If the Activity Feed page cannot be found, or is deleted, it returns PASS.
    /// * Otherwise, if the Activity Feed page is public, it returns ALERT.
    /// * Otherwise, if My Profile page cannot be found or is deleted, it returns PASS.
    /// * Otherwise, if My Profile page is public, it returns ALERT.
    /// * Otherwise, it returns PASS.
    /// Case 2: the selected user profile page is not "Activity Feed"
    /// * If the selected user profile page cannot be found, or is deleted, it returns PASS.
    /// * Otherwise, if the selected user profile page is public, it returns ALERT.
    /// * Otherwise, it returns PASS.
    /// </summary>
    public class CheckUserProfilePage : BaseCheck
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>The default "Activity Feed" tab path.</summary>
        public const string ActivityFeedTabPath = "//ActivityFeed";

        /// <summary>The default "My Profile" tab path.</summary>
        public const string MyProfileTabPath = "//ActivityFeed//MyProfile";

        /// <summary>Name of the "View Tab" permission.</summary>
        public const string ViewTab = "View Tab";

        private readonly ITabController tabController;
        private readonly IPagesController pagesController;
        private readonly Lazy<IPortalSettings> portalSettings;

        /// <summary>Initializes a new instance of the <see cref="CheckUserProfilePage"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IPagesController. Scheduled removal in v12.0.0.")]
        public CheckUserProfilePage()
            : this(PortalController.Instance, TabController.Instance, PagesController.Instance)
        {
		Logger.LogDebug(">>> Entering CheckUserProfilePage()");
        }
		Logger.LogDebug("<<< Exiting CheckUserProfilePage()");

        /// <summary>Initializes a new instance of the <see cref="CheckUserProfilePage"/> class.</summary>
        /// <param name="pagesController">An instance of the <see cref="IPagesController"/> interface.</param>
        public CheckUserProfilePage(IPagesController pagesController)
            : this(PortalController.Instance, TabController.Instance, pagesController)
        {
		Logger.LogDebug(">>> Entering CheckUserProfilePage(pagesController={PagesController})", pagesController);
		Logger.LogDebug("<<< Exiting CheckUserProfilePage(pagesController={PagesController})", pagesController);
        }

        /// <summary>Initializes a new instance of the <see cref="CheckUserProfilePage"/> class.</summary>
        /// <param name="portalController">An instance of the <see cref="IPortalController"/> interface.</param>
        /// <param name="tabController">An instance of the <see cref="ITabController"/> interface.</param>
        /// <param name="pagesController">An instance of the <see cref="IPagesController"/> interface.</param>
        internal CheckUserProfilePage(
            IPortalController portalController,
            ITabController tabController,
            IPagesController pagesController)
        {
            this.tabController = tabController ?? throw new ArgumentNullException(nameof(tabController));
            this.pagesController = pagesController ?? throw new ArgumentNullException(nameof(pagesController));

            if (portalController == null)
            {
                throw new ArgumentNullException(nameof(portalController));
            }

            this.portalSettings = new Lazy<IPortalSettings>(portalController.GetCurrentSettings);
        }

        private int PortalId => this.portalSettings.Value.PortalId;

        private int UserTabId => this.portalSettings.Value.UserTabId;

        /// <inheritdoc />
        protected override CheckResult ExecuteInternal()
        {
			Logger.LogDebug(">>> Entering ExecuteInternal()");
			Logger.LogDebug("<<< Exiting ExecuteInternal()");
            return this.UserProfilePageIsPublic() ? this.Public() : this.NotPublic();
        }

        private bool UserProfilePageIsPublic()
        {
			Logger.LogDebug(">>> Entering UserProfilePageIsPublic()");
            var userProfilePage = this.tabController.GetTab(this.UserTabId, this.PortalId);

			Logger.LogDebug("UserProfilePageIsPublic(): userProfilePage → {UserProfilePage}", userProfilePage);
            if (userProfilePage == null || userProfilePage.IsDeleted)
            {
                return false; // invalid UserTabId in portal settings somehow
            }

            if (this.PageIsPublic(userProfilePage))
            {
                return true;
            }

            var userProfilePageIsActivityFeed = string.Equals(
                ActivityFeedTabPath,
                userProfilePage.TabPath,
                StringComparison.OrdinalIgnoreCase);

            if (!userProfilePageIsActivityFeed)
            {
                return false; // neither public nor activity feed, so no need to check "My Profile" page.
            }

            var myProfilePage = this.FindTabByPath(MyProfileTabPath, this.PortalId);

			Logger.LogDebug("UserProfilePageIsPublic(): myProfilePage → {MyProfilePage}", myProfilePage);
            if (myProfilePage == null)
            {
                return false; // "My Profile" page not found, so not public.
            }

			Logger.LogDebug(">>> Entering PageIsPublic(tabInfo={TabInfo})", tabInfo);
			Logger.LogDebug("<<< Exiting PageIsPublic(tabInfo={TabInfo})", tabInfo);
            return this.PageIsPublic(myProfilePage);
        }

        private CheckResult Public()
        {
			Logger.LogDebug(">>> Entering Public()");
            return new CheckResult(SeverityEnum.Warning, this.Id);
        }

        private CheckResult NotPublic()
        {
			Logger.LogDebug(">>> Entering NotPublic()");
			Logger.LogDebug("<<< Exiting NotPublic()");
            return new CheckResult(SeverityEnum.Pass, this.Id);
        }

        private TabInfo FindTabByPath(string tabPath, int portalId)
        {
			Logger.LogDebug(">>> Entering FindTabByPath(tabPath={TabPath},portalId={PortalId})", tabPath, portalId);
			Logger.LogDebug("<<< Exiting FindTabByPath(tabPath={TabPath},portalId={PortalId})", tabPath, portalId);
            return this.tabController.GetTabsByPortal(portalId)
                .Select(kvp => kvp.Value)
                .Where(tab => !tab.IsDeleted)
                .Where(tab => string.Equals(tab.TabPath, tabPath, StringComparison.OrdinalIgnoreCase))
                .SingleOrDefault();
        }

        private bool PageIsPublic(TabInfo tabInfo)
        {
            var allUsersRoleId = int.Parse(Globals.glbRoleAllUsers, CultureInfo.InvariantCulture);

            var permissions = this.pagesController.GetPermissionsData(tabInfo.TabID);

			Logger.LogDebug("<<< Exiting Public()");
            return permissions.RolePermissions
                .Any(rp => rp.RoleId == allUsersRoleId &&
                    rp.Permissions.Any(p => p.PermissionName == ViewTab && p.AllowAccess));
        }
    }
}
