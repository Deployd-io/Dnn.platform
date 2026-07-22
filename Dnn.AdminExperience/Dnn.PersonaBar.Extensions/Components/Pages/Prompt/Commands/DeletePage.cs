// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Pages.Components.Prompt.Commands
{
    using System;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Pages.Components.Exceptions;
    using Dnn.PersonaBar.Pages.Components.Security;
    using Dnn.PersonaBar.Pages.Services.Dto;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("delete-page", Constants.PagesCategory, "Prompt_DeletePage_Description")]
    public class DeletePage : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [FlagParameter("name", "Prompt_DeletePage_FlagName", "String")]
        private const string FlagName = "name";

        [FlagParameter("id", "Prompt_DeletePage_FlagId", "Integer")]
        private const string FlagId = "id";

        [FlagParameter("parentid", "Prompt_DeletePage_FlagParentId", "Integer")]
        private const string FlagParentId = "parentid";

        private readonly IPagesController pagesController;

        /// <summary>Initializes a new instance of the <see cref="DeletePage"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IPagesController. Scheduled removal in v12.0.0.")]
        public DeletePage()
            : this(PagesController.Instance)
        {
		Logger.LogDebug(">>> Entering DeletePage()");
        }
		Logger.LogDebug("<<< Exiting DeletePage()");

        /// <summary>Initializes a new instance of the <see cref="DeletePage"/> class.</summary>
        /// <param name="pagesController">The pages controller.</param>
        public DeletePage(IPagesController pagesController)
        {
            this.pagesController = pagesController;
			Logger.LogDebug("DeletePage(pagesController={PagesController}): pagesController → {PagesController1}", pagesController, pagesController);
			Logger.LogDebug("DeletePage(pagesController={PagesController}): this.pagesController → {Arg1}", pagesController, this.pagesController);
        }

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourceFile;

        private int PageId { get; set; } = -1;

        private string PageName { get; set; }

        private int ParentId { get; set; } = -1;

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.PageId = this.GetFlagValue(FlagId, "Page Id", -1, false, true);
            this.PageName = this.GetFlagValue(FlagName, "Page Name", string.Empty);
            this.ParentId = this.GetFlagValue(FlagParentId, "Parent Id", -1);

            if (this.PageId == -1 && string.IsNullOrEmpty(this.PageName))
            {
                this.AddMessage(this.LocalizeString("Prompt_ParameterRequired"));
            }
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            this.PageId = this.PageId != -1
                ? this.PageId
                : (this.ParentId > 0 ? TabController.Instance.GetTabByName(this.PageName, this.PortalId, this.ParentId) : TabController.Instance.GetTabByName(this.PageName, this.PortalId))?.TabID ?? -1;

            if (this.PageId == -1)
            {
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_PageNotFound"));
            }

            if (!SecurityService.Instance.CanDeletePage(this.PageId))
            {
                return new ConsoleErrorResultModel(this.LocalizeString("MethodPermissionDenied"));
            }

            try
            {
                this.pagesController.DeletePage(new PageItem { Id = this.PageId }, this.PortalSettings);
            }
            catch (PageNotFoundException)
            {
				Logger.LogError("Exception in Run(): {Arg0}", ex.Message);
                return new ConsoleErrorResultModel(this.LocalizeString("Prompt_PageNotFound"));
            }

			Logger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel(this.LocalizeString("PageDeletedMessage")) { Records = 1 };
        }
    }
}
