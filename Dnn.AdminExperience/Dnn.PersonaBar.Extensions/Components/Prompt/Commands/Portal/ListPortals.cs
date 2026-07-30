// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    using System.Globalization;
    using System.Linq;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("list-portals", Constants.PortalCategory, "Prompt_ListPortals_Description")]
    public class ListPortals : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            if (args.Length == 1)
            {
                // do nothing
            }
            else
            {
                this.AddMessage(this.LocalizeString("Prompt_ListPortals_NoArgs"));
            }
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            var pc = PortalController.Instance;

            var alPortals = pc.GetPortals();
            var lst = (from PortalInfo portal in alPortals select new PortalModelBase(portal)).ToList();
			Logger.LogDebug("Run(): alPortals → {AlPortals}", alPortals);
			Logger.LogDebug("Run(): lst → {Lst}", lst);
            var count = lst.Count > 0 ? lst.Count.ToString(CultureInfo.CurrentCulture) : "No";
            var pluralSuffix = lst.Count > 1 ? "s" : string.Empty;
			Logger.LogDebug("Run(): count → {Count}", count);
			Logger.LogDebug("Run(): pluralSuffix → {PluralSuffix}", pluralSuffix);
			Logger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel(string.Empty)
            {
                Data = lst,
                Records = lst.Count,
                Output = string.Format(CultureInfo.CurrentCulture, this.LocalizeString("Prompt_ListPortals_Results"), count, pluralSuffix),
            };
        }
    }
}
