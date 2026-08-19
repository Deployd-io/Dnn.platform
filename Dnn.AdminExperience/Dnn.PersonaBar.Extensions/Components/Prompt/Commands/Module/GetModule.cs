// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Module
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    using ModulesControllerLibrary = Dnn.PersonaBar.Library.Controllers.ModulesController;

    [ConsoleCommand("get-module", Constants.ModulesCategory, "Prompt_GetModule_Description")]
    public class GetModule : ConsoleCommandBase
    {
        [FlagParameter("id", "Prompt_GetModule_FlagId", "Integer", true)]
        private const string FlagId = "id";

        [FlagParameter("pageid", "Prompt_GetModule_FlagPageId", "Integer", true)]
        private const string FlagPageId = "pageid";

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int ModuleId { get; set; }

        private int PageId { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			eventLogger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            this.ModuleId = this.GetFlagValue(FlagId, "Module Id", -1, true, true, true);
            this.PageId = this.GetFlagValue(FlagPageId, "Page Id", -1, true, false, true);
			eventLogger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			eventLogger.LogDebug(">>> Entering Run()");
            var lst = new List<ModuleInfoModel>();
            var moduleInfo = ModulesControllerLibrary.Instance.GetModule(
                this.PortalSettings,
                this.ModuleId,
                this.PageId,
                out var message);
				eventLogger.LogDebug("Run(): lst → {Lst}", lst);
            if (moduleInfo == null && !string.IsNullOrEmpty(message.Value))
            {
                return new ConsoleErrorResultModel(message.Value);
            }

            lst.Add(ModuleInfoModel.FromDnnModuleInfo(moduleInfo));
			eventLogger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel
            {
                Data = lst,
                Records = lst.Count,
                Output = string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Prompt_GetModule_Result"), this.ModuleId, this.PageId),
            };
        }
    }
}
