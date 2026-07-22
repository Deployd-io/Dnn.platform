// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Commands.Portal
{
    using System.Collections.Generic;
    using System.Globalization;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Prompt.Components.Models;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    [ConsoleCommand("get-portal", Constants.PortalCategory, "Prompt_GetPortal_Description")]
    public class GetPortal : ConsoleCommandBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        [FlagParameter("id", "Prompt_GetPortal_FlagId", "Integer")]
        private const string FlagId = "id";

        /// <inheritdoc />
        public override string LocalResourceFile => Constants.LocalResourcesFile;

        private int PortalIdFlagValue { get; set; }

        /// <inheritdoc />
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
			Logger.LogDebug(">>> Entering Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
            // default usage: return current portal if nothing else specified
            if (args.Length == 1)
            {
                this.PortalIdFlagValue = this.PortalId;
				Logger.LogDebug("Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId}): this.PortalIdFlagValue → {Arg4}", args, portalSettings, userInfo, activeTabId, this.PortalIdFlagValue);
            }
            else
            {
                // allow hosts to get info on other portals
                if (this.User.IsSuperUser && args.Length >= 2)
                {
                    this.PortalIdFlagValue = this.GetFlagValue(FlagId, "Portal Id", this.PortalId, true, true);
                }
                else
                {
                    // admins cannot access info on other portals.
                    this.AddMessage(this.LocalizeString("Prompt_GetPortal_NoArgs"));
                }
            }
			Logger.LogDebug("<<< Exiting Init(args={Args},portalSettings={PortalSettings},userInfo={UserInfo},activeTabId={ActiveTabId})", args, portalSettings, userInfo, activeTabId);
        }

        /// <inheritdoc />
        public override ConsoleResultModel Run()
        {
			Logger.LogDebug(">>> Entering Run()");
            var pc = new PortalController();
            var lst = new List<PortalModel>();
			Logger.LogDebug("Run(): pc → {Pc}", pc);
			Logger.LogDebug("Run(): lst → {Lst}", lst);

            var portal = pc.GetPortal((int)this.PortalIdFlagValue);
            if (portal == null)
			Logger.LogDebug("Run(): portal → {Portal}", portal);
            {
                return new ConsoleErrorResultModel(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString("Prompt_GetPortal_NotFound"),
                        this.PortalIdFlagValue));
            }

            lst.Add(new PortalModel(portal));
			Logger.LogDebug("<<< Exiting Run()");
            return new ConsoleResultModel(string.Empty)
            {
                Data = lst,
                Records = lst.Count,
                Output = string.Format(
                    CultureInfo.CurrentCulture,
                    this.LocalizeString("Prompt_GetPortal_Found"),
                    this.PortalIdFlagValue),
            };
        }
    }
}
