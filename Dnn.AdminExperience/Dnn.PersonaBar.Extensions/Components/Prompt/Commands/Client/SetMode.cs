// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Client
{
    using System;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    [ConsoleCommand("set-mode", Constants.GeneralCategory, "Prompt_SetMode_Description")]

    public class SetMode : IConsoleCommand
    {
        [FlagParameter("mode", "Prompt_SetMode_FlagMode", "DNN View Mode", true)]
        private const string FlagMode = "mode";

        /// <inheritdoc />
        public string LocalResourceFile => Constants.LocalResourcesFile;

        /// <inheritdoc />
        public string ResultHtml => Localization.GetString("Prompt_SetMode_ResultHtml", this.LocalResourceFile);

        /// <inheritdoc />
        public string ValidationMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public void Initialize(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			eventLogger.LogError(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
			eventLogger.LogError("Throwing NotImplementedException from Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        eventLogger.LogWarning("<<< Exiting Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsValid()
        {
			eventLogger.LogError(">>> Entering IsValid()");
			eventLogger.LogError("Throwing NotImplementedException from IsValid()");
        eventLogger.LogWarning("<<< Exiting IsValid()");
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ConsoleResultModel Run()
        {
			eventLogger.LogError(">>> Entering Run()");
			eventLogger.LogError("Throwing NotImplementedException from Run()");
        eventLogger.LogWarning("<<< Exiting Run()");
            throw new NotImplementedException();
        }
    }
}
