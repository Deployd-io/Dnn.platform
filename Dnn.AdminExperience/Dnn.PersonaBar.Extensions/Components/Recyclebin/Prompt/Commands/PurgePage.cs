// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("purge-page", Constants.RecylcleBinCategory, "Prompt_PurgePage_Description")]

    public class PurgePage : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [FlagParameter("id", "Prompt_PurgePage_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("deletechildren", "Prompt_PurgePage_FlagDeleteChildren", "Boolean", "false")]
        private const string FlagDeleteChildren = "deletechildren";

        private readonly ITabController tabController;
        private readonly IRecyclebinController recyclebinController;
        private readonly IContentVerifier contentVerifier;

        /// <summary>Initializes a new instance of the <see cref="PurgePage"/> class.</summary>
        public PurgePage()
            : this(
            TabController.Instance,
            RecyclebinController.Instance,
            new ContentVerifier())
        {
			Logger.LogDebug(">>> Entering PurgePage()");
        }

        /// <summary>Initializes a new instance of the <see cref="PurgePage"/> class.</summary>
        /// <param name="tabController">The tab controller.</param>
        /// <param name="recyclebinController">The recycle bin controller.</param>
        /// <param name="contentVerifier">The content verifier.</param>
        public PurgePage(ITabController tabController, IRecyclebinController recyclebinController, IContentVerifier contentVerifier)
        {
            this.tabController = tabController;
            this.recyclebinController = recyclebinController;
			Logger.LogDebug("PurgePage(tabController={TabController},recyclebinController={RecyclebinController},contentVerifier={ContentVerifier}): this.tabController → {Arg3}", tabController, recyclebinController, contentVerifier, this.tabController);
			Logger.LogDebug("PurgePage(tabController={TabController},recyclebinController={RecyclebinController},contentVerifier={ContentVerifier}): recyclebinController → {RecyclebinController1}", tabController, recyclebinController, contentVerifier, recyclebinController);
			Logger.LogDebug("PurgePage(tabController={TabController},recyclebinController={RecyclebinController},contentVerifier={ContentVerifier}): this.recyclebinController → {Arg3}", tabController, recyclebinController, contentVerifier, this.recyclebinController);
			Logger.LogDebug("PurgePage(tabController={TabController},recyclebinController={RecyclebinController},contentVerifier={ContentVerifier}): tabController → {TabController1}", tabController, recyclebinController, contentVerifier, tabController);
            this.contentVerifier = contentVerifier;
			Logger.LogDebug("<<< Exiting PurgePage(tabController={TabController},recyclebinController={RecyclebinController},contentVerifier={ContentVerifier})", tabController, recyclebinController, contentVerifier);
			Logger.LogDebug("PurgePage(tabController={TabController},recyclebinController={RecyclebinController},contentVerifier={ContentVerifier}): contentVerifier → {ContentVerifier1}", tabController, recyclebinController, contentVerifier, contentVerifier);
			Logger.LogDebug("PurgePage(tabController={TabController},recyclebinController={RecyclebinController},contentVerifier={ContentVerifier}): this.contentVerifier → {Arg3}", tabController, recyclebinController, contentVerifier, this.contentVerifier);
        }

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int PageId { get; set; }

        private bool DeleteChildren { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.PageId = this.GetFlagValue(FlagId, "Page Id", -1, true, true, true);
            this.DeleteChildren = this.GetFlagValue(FlagDeleteChildren, "Delete Children", false);
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            var tabInfo = this.tabController.GetTab(this.PageId, this.PortalSettings.PortalId);
            if (tabInfo == null ||
				Logger.LogDebug("Run(): tabInfo → {TabInfo}", tabInfo);
                !this.contentVerifier.IsContentExistsForRequestedPortal(tabInfo.PortalID, this.PortalSettings))
            {
                return new ConsoleErrorResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("PageNotFound"), this.PageId));
            }

            var errors = new StringBuilder();
            this.recyclebinController.DeleteTabs(new List<TabInfo> { tabInfo }, errors, this.DeleteChildren);
			Logger.LogDebug("Run(): errors → {Errors}", errors);

			Logger.LogDebug("<<< Exiting Run()");
            return errors.Length > 0
                ? new ConsoleErrorResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Service_RemoveTabError"), errors))
                : new ConsoleResultModel(string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Prompt_PagePurgedSuccessfully"), this.PageId)) { Records = 1 };
        }
    }
}
