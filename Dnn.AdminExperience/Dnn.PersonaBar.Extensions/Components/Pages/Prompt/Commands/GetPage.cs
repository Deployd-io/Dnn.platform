// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Helper;
    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Security;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("get-page", Constants.PagesCategory, "Prompt_GetPage_Description")]
    public class GetPage : ConsoleCommandBase
    {
        [FlagParameter("name", "Prompt_GetPage_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("id", "Prompt_GetPage_FlagId", "Integer")]
        private const string FlagId = "id";

        [FlagParameter("parentid", "Prompt_GetPage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        private readonly ITabController tabController;
        private readonly ISecurityService securityService;
        private readonly IContentVerifier contentVerifier;

        public GetPage()
            : this(TabController.Instance, SecurityService.Instance, new ContentVerifier())
        {
		eventLogger.LogDebug(">>> Entering GetPage()");
        }

		eventLogger.LogDebug("<<< Exiting GetPage()");
        public GetPage(
            ITabController tabController,
            ISecurityService securityService,
            IContentVerifier contentVerifier)
        {
            this.tabController = tabController;
            this.securityService = securityService;
			eventLogger.LogDebug("GetPage(tabController={TabController},securityService={SecurityService},contentVerifier={ContentVerifier}): this.tabController → {Arg3}", tabController, securityService, contentVerifier, this.tabController);
			eventLogger.LogDebug("GetPage(tabController={TabController},securityService={SecurityService},contentVerifier={ContentVerifier}): securityService → {SecurityService1}", tabController, securityService, contentVerifier, securityService);
            this.contentVerifier = contentVerifier;
        }

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourceFile;

        private int PageId { get; set; } = -1;

        private string PageName { get; set; }

        private int ParentId { get; set; } = -1;

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			eventLogger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.PageId = this.GetFlagValue(FlagId, "Page Id", -1, false, true);
            this.PageName = this.GetFlagValue(FlagName, "Page Name", string.Empty);
            this.ParentId = this.GetFlagValue(FlagParentId, "Parent Id", -1);
            if (this.PageId == -1 && string.IsNullOrEmpty(this.PageName))
            {
                this.AddMessage(this.LocalizeString("Prompt_ParameterRequired"));
            }
			eventLogger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			eventLogger.LogDebug(">>> Entering Run()");
            var lst = new List<PageModel>();
            var tab = this.PageId != -1
                ? this.tabController.GetTab(this.PageId, this.PortalId)
                : (this.ParentId > 0
                    ? this.tabController.GetTabByName(this.PageName, this.PortalId, this.ParentId)
                    : this.tabController.GetTabByName(this.PageName, this.PortalId));
					eventLogger.LogDebug("Run(): lst → {Lst}", lst);

            if (tab != null)
            {
                if (!this.securityService.CanManagePage(this.PageId))
                {
                    return new ConsoleErrorResultModel(this.LocalizeString("MethodPermissionDenied"));
                }

                if (this.contentVerifier.IsContentExistsForRequestedPortal(tab.PortalID, this.PortalSettings))
                {
                    lst.Add(new PageModel(tab));
                    return new ConsoleResultModel { Data = lst, Records = lst.Count, Output = this.LocalizeString("Prompt_PageFound") };
                }
            }

			eventLogger.LogDebug("<<< Exiting Run()");
            return new ConsoleErrorResultModel(this.LocalizeString("Prompt_PageNotFound"));
        }
    }
}
