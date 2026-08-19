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

    [ConsoleCommand("clear-history", Constants.GeneralCategory, "Prompt_ClearHistory_Description")]

    public class ClearHistory : IConsoleCommand
    {
        /// <inheritdoc />
        public string LocalResourceFile => Constants.LocalResourcesFile;

        /// <inheritdoc />
        public string ResultHtml => Localization.GetString("Prompt_ClearHistory_ResultHtml", this.LocalResourceFile);

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
			eventLogger.LogDebug(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
			eventLogger.LogWarning("Throwing NotImplementedException from Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
			eventLogger.LogDebug("<<< Exiting Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsValid()
        {
			eventLogger.LogDebug(">>> Entering IsValid()");
			eventLogger.LogWarning("Throwing NotImplementedException from IsValid()");
			eventLogger.LogDebug("<<< Exiting IsValid()");
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ConsoleResultModel Run()
        {
			eventLogger.LogDebug(">>> Entering Run()");
			eventLogger.LogWarning("Throwing NotImplementedException from Run()");
			eventLogger.LogDebug("<<< Exiting Run()");
            throw new NotImplementedException();
        }
    }
}
