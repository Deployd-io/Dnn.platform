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

    [ConsoleCommand("reload", Constants.GeneralCategory, "Prompt_Reload_Description")]

    public class Reload : IConsoleCommand
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public string LocalResourceFile => Constants.LocalResourcesFile;

        public string ResultHtml => Localization.GetString("Prompt_Reload_ResultHtml", this.LocalResourceFile);

        public string ValidationMessage
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Initialize(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
			Logger.LogDebug(">>> Entering IsValid()");
            throw new NotImplementedException();
        }

        public ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            throw new NotImplementedException();
        }
    }
}
