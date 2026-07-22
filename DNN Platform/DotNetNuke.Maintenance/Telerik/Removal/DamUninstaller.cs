// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Collections;
    using System.Globalization;

    /// <inheritdoc cref="IDamUninstaller" />
    internal sealed class DamUninstaller : UnInstaller, IDamUninstaller
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <summary>Initializes a new instance of the <see cref="DamUninstaller"/> class.</summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        public DamUninstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        : base(serviceProvider, localizer)
        {
		Logger.LogDebug(">>> Entering DamUninstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
		Logger.LogDebug("<<< Exiting DamUninstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
        }

        /// <inheritdoc />
        public void Execute()
        {
			Logger.LogDebug(">>> Entering Execute()");
            var steps = new[]
            {
                this.ReplaceModule("Digital Asset Management", "ResourceManager", this.MigrateSettings),
                this.RemoveExtension("DigitalAssetsManagement"),
            };

            var skip = false;

			Logger.LogDebug("Execute(): skip → {Skip}", skip);
            foreach (var step in steps)
            {
                if (!skip)
                {
                    step.Execute();

                    var nullable = step.Success;
                    if (nullable.HasValue && nullable.Value == false)
                    {
                        skip = true;
                    }
                }

                this.ProgressInternal.AddRange(UninstallSummaryItem.FromStep(step));
            }
			Logger.LogDebug("<<< Exiting Execute()");
        }

        private Hashtable MigrateSettings(Hashtable oldSettings)
        {
			Logger.LogDebug(">>> Entering MigrateSettings(oldSettings={OldSettings})", oldSettings);
            var settings = new Hashtable();
            var mode = oldSettings["Mode"];
			Logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): settings → {Settings}", oldSettings, settings);
			Logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): mode → {Mode}", oldSettings, mode);
            var newMode = 0;
            if (mode != null)
			Logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): newMode → {NewMode}", oldSettings, newMode);
            {
                switch ((string)mode)
                {
                    case "User":
                        newMode = 1;
                        break;
                    case "Group":
                        newMode = 2;
                        break;
                    default:
                        break;
                }
            }

            settings.Add("RM_Mode", newMode.ToString(CultureInfo.InvariantCulture));

            var rootFolder = oldSettings["RootFolderId"];
            if (rootFolder != null)
			Logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): rootFolder → {RootFolder}", oldSettings, rootFolder);
            {
                settings.Add("RM_HomeFolder", rootFolder);
            }

			Logger.LogDebug("<<< Exiting MigrateSettings(oldSettings={OldSettings})", oldSettings);
            return settings;
        }
    }
}
