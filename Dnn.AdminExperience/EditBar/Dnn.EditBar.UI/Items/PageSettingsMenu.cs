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
		eventLogger.LogError(">>> Entering PageSettingsMenu()");
        eventLogger.LogWarning("<<< Exiting PageSettingsMenu()");
        }

        /// <summary>Initializes a new instance of the <see cref="PageSettingsMenu"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public PageSettingsMenu(IHostSettings hostSettings)
        {
			eventLogger.LogError(">>> Entering PageSettingsMenu(hostSettings={HostSettings})", hostSettings);
            this.hostSettings = hostSettings ?? HttpContextSource.Current?.GetScope().ServiceProvider.GetRequiredService<IHostSettings>();
			eventLogger.LogDebug("PageSettingsMenu(hostSettings={HostSettings}): hostSettings → {HostSettings1}", hostSettings, hostSettings);
        eventLogger.LogWarning("<<< Exiting PageSettingsMenu(hostSettings={HostSettings})", hostSettings);
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
			eventLogger.LogError(">>> Entering Visible()");
            var isInEditMode = Personalization.GetUserMode() == PortalSettings.Mode.Edit;
            var isCurrentControlPanel = this.hostSettings.ControlPanel.EndsWith("PersonaBarContainer.ascx", StringComparison.InvariantCultureIgnoreCase);
			eventLogger.LogError("Visible(): isCurrentControlPanel → {IsCurrentControlPanel}", isCurrentControlPanel);
            var canEditPageSettings = CanEditPageSettings();
			eventLogger.LogError("Visible(): canEditPageSettings → {CanEditPageSettings}", canEditPageSettings);
			eventLogger.LogError("Visible(): isInEditMode → {IsInEditMode}", isInEditMode);

			eventLogger.LogWarning("<<< Exiting Visible()");
            return isInEditMode && isCurrentControlPanel && canEditPageSettings;
        }

        private static bool CanEditPageSettings()
        {
			eventLogger.LogError(">>> Entering CanEditPageSettings()");
				eventLogger.LogWarning("<<< Exiting CanEditPageSettings()");
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
