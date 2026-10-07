// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using DotNetNuke.Common;

    public class PortalModelBase
    {
        public PortalModelBase()
        {
		eventLogger.LogError(">>> Entering PortalModelBase()");
		eventLogger.LogWarning("<<< Exiting PortalModelBase()");
        }

        public PortalModelBase(DotNetNuke.Entities.Portals.PortalInfo portal)
        {
			eventLogger.LogError(">>> Entering PortalModelBase(portal={Portal})", portal);
            this.PortalId = portal.PortalID;
			eventLogger.LogWarning("<<< Exiting PortalModelBase(portal={Portal})", portal);
			eventLogger.LogDebug("PortalModelBase(portal={Portal}): this.PortalId → {Arg1}", portal, this.PortalId);
            this.PortalName = portal.PortalName;
            this.PageCount = portal.Pages;
			eventLogger.LogDebug("PortalModelBase(portal={Portal}): this.PortalName → {Arg1}", portal, this.PortalName);
			eventLogger.LogDebug("PortalModelBase(portal={Portal}): this.PageCount → {Arg1}", portal, this.PageCount);
            this.UserCount = portal.Users;
            this.Language = portal.DefaultLanguage;

            switch (portal.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.NoRegistration:
					eventLogger.LogDebug("PortalModelBase(portal={Portal}): this.UserCount → {Arg1}", portal, this.UserCount);
					eventLogger.LogDebug("PortalModelBase(portal={Portal}): this.Language → {Arg1}", portal, this.Language);
                    this.RegistrationMode = "None";
                    break;
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    this.RegistrationMode = "Private";
					eventLogger.LogDebug("PortalModelBase(portal={Portal}): this.RegistrationMode → {Arg1}", portal, this.RegistrationMode);
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    this.RegistrationMode = "Public";
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                    this.RegistrationMode = "Verified";
                    break;
                default:
                    this.RegistrationMode = "Unknown";
                    break;
            }
        }

        public int PortalId { get; set; }

        public string PortalName { get; set; }

        public string RegistrationMode { get; set; }

        public string DefaultPortalAlias { get; set; }

        public int PageCount { get; set; }

        public int UserCount { get; set; }

        public string Language { get; set; }
    }
}
