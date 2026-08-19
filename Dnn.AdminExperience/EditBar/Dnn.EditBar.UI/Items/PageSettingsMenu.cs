// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;
    using Dnn.EditBar.Library.Items;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Menu item to edit the current page settings.</summary>
    [Serializable]
    public class PageSettingsMenu : BaseMenuItem
    {
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="PageSettingsMenu"/> class.</summary>
        public PageSettingsMenu()
            : this(null)
        {
		eventLogger.LogDebug(">>> Entering PageSettingsMenu()");
		eventLogger.LogDebug("<<< Exiting PageSettingsMenu()");
		eventLogger.LogDebug(">>> Entering PageSettingsMenu(hostSettings={HostSettings})", hostSettings);
		eventLogger.LogDebug("PageSettingsMenu(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        }

        /// <summary>Initializes a new instance of the <see cref="PageSettingsMenu"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public PageSettingsMenu(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();
			eventLogger.LogDebug("<<< Exiting PageSettingsMenu(hostSettings={HostSettings})", hostSettings);
        }

        /// <inheritdoc />
        public override string Name => "PageSettings";

        /// <inheritdoc />
        public override string Text => "PageSettings";

        /// <inheritdoc />
        public override string Parent => Constants.LeftMenu;

        /// <inheritdoc />
        public override string Loader => "PageSettings";

        /// <inheritdoc />
        public override int Order => 15;

        /// <inheritdoc />
        public override bool Visible()
        {
			eventLogger.LogDebug(">>> Entering Visible()");
            var isInEditMode = Personalization.GetUserMode() == PortalSettings.Mode.Edit;
            var isCurrentControlPanel = this.hostSettings.ControlPanel.EndsWith("PersonaBarContainer.ascx", StringComparison.InvariantCultureIgnoreCase);
			eventLogger.LogDebug("Visible(): isCurrentControlPanel → {IsCurrentControlPanel}", isCurrentControlPanel);
            var canEditPageSettings = CanEditPageSettings();
			eventLogger.LogDebug("Visible(): canEditPageSettings → {CanEditPageSettings}", canEditPageSettings);
			eventLogger.LogDebug("Visible(): isInEditMode → {IsInEditMode}", isInEditMode);

			eventLogger.LogDebug("<<< Exiting Visible()");
            return isInEditMode && isCurrentControlPanel && canEditPageSettings;
        }

        private static bool CanEditPageSettings()
        {
			eventLogger.LogDebug(">>> Entering CanEditPageSettings()");
				eventLogger.LogDebug("<<< Exiting CanEditPageSettings()");
            return
                TabPermissionController.CanAddPage() ||
                TabPermissionController.CanAdminPage() ||
                TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() ||
                TabPermissionController.CanExportPage() ||
                TabPermissionController.CanImportPage() ||
                TabPermissionController.CanManagePage();
        }
    }
}
