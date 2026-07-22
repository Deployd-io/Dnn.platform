// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using DotNetNuke.Common;

    public class PortalModelBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public PortalModelBase()
        {
		Logger.LogDebug(">>> Entering PortalModelBase()");
		Logger.LogDebug("<<< Exiting PortalModelBase()");
        }

        public PortalModelBase(DotNetNuke.Entities.Portals.PortalInfo portal)
        {
			Logger.LogDebug(">>> Entering PortalModelBase(portal={Portal})", portal);
            this.PortalId = portal.PortalID;
            this.PortalName = portal.PortalName;
			Logger.LogDebug("PortalModelBase(portal={Portal}): this.PortalId → {Arg1}", portal, this.PortalId);
			Logger.LogDebug("PortalModelBase(portal={Portal}): this.PortalName → {Arg1}", portal, this.PortalName);
            this.PageCount = portal.Pages;
            this.UserCount = portal.Users;
            this.Language = portal.DefaultLanguage;
			Logger.LogDebug("PortalModelBase(portal={Portal}): this.PageCount → {Arg1}", portal, this.PageCount);
			Logger.LogDebug("PortalModelBase(portal={Portal}): this.UserCount → {Arg1}", portal, this.UserCount);
			Logger.LogDebug("PortalModelBase(portal={Portal}): this.Language → {Arg1}", portal, this.Language);

            switch (portal.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.NoRegistration:
                    this.RegistrationMode = "None";
                    break;
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    this.RegistrationMode = "Private";
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    this.RegistrationMode = "Public";
					Logger.LogDebug("PortalModelBase(portal={Portal}): this.RegistrationMode → {Arg1}", portal, this.RegistrationMode);
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
					Logger.LogDebug("<<< Exiting PortalModelBase()");
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
