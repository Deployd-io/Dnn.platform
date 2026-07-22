// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Entities.Portals;

    public class PortalModel : PortalModelBase
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        public PortalModel()
        {
		Logger.LogDebug(">>> Entering PortalModel()");
		Logger.LogDebug("<<< Exiting PortalModel()");
        }

        public PortalModel(PortalInfo portal)
            : base(portal)
        {
            // get portal settings for specified portal
            var ps = new PortalSettings(portal);
            this.CdfVersion = ps.CdfVersion;
			Logger.LogDebug("PortalModel(portal={Portal}): ps → {Ps}", portal, ps);
			Logger.LogDebug("PortalModel(portal={Portal}): this.CdfVersion → {Arg1}", portal, this.CdfVersion);
            this.SiteTheme = Utilities.FormatSkinName(ps.DefaultPortalSkin);
            this.AdminTheme = Utilities.FormatSkinName(ps.DefaultAdminSkin);
            this.Container = Utilities.FormatContainerName(ps.DefaultPortalContainer);
            this.AdminContainer = Utilities.FormatContainerName(ps.DefaultAdminContainer);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public string __PageCount => "list-pages";

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]

        // ReSharper disable once InconsistentNaming
        public string __UserCount => "list-users";

        public int CdfVersion { get; set; }

        public string SiteTheme { get; set; }

        public string AdminTheme { get; set; }

        public string Container { get; set; }

        public string AdminContainer { get; set; }
    }
}
