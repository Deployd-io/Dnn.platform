// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Client
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    [ConsoleCommand("exit", Constants.GeneralCategory, "Prompt_Exit_Description")]
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
    public class Exit : IConsoleCommand
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public string LocalResourceFile => Constants.LocalResourcesFile;

        /// <inheritdoc />
        public string ResultHtml => Localization.GetString("Prompt_Exit_ResultHtml", this.LocalResourceFile);

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
			Logger.LogDebug(">>> Entering Initialize(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsValid()
        {
			Logger.LogDebug(">>> Entering IsValid()");
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            throw new NotImplementedException();
        }
    }
}
