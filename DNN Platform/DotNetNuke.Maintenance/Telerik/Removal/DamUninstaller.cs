// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Removal
{
    using System;
    using System.Collections;
    using System.Globalization;
	using Microsoft.Extensions.Logging;

    /// <inheritdoc cref="IDamUninstaller" />
    internal sealed class DamUninstaller : UnInstaller, IDamUninstaller
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        /// <summary>Initializes a new instance of the <see cref="DamUninstaller"/> class.</summary>
        /// <param name="serviceProvider">An instance of <see cref="IServiceProvider"/>.</param>
        /// <param name="localizer">An instance of <see cref="ILocalizer"/>.</param>
        public DamUninstaller(IServiceProvider serviceProvider, ILocalizer localizer)
        : base(serviceProvider, localizer)
        {
		_logger.LogDebug(">>> Entering DamUninstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
		_logger.LogDebug("<<< Exiting DamUninstaller(serviceProvider={ServiceProvider},localizer={Localizer})", serviceProvider, localizer);
        }

        /// <inheritdoc />
        public void Execute()
        {
			_logger.LogDebug(">>> Entering Execute()");
            var steps = new[]
            {
                this.ReplaceModule("Digital Asset Management", "ResourceManager", this.MigrateSettings),
                this.RemoveExtension("DigitalAssetsManagement"),
            };

            var skip = false;
			_logger.LogDebug("Execute(): skip → {Skip}", skip);

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
			_logger.LogDebug("<<< Exiting Execute()");
        }

        private Hashtable MigrateSettings(Hashtable oldSettings)
        {
			_logger.LogDebug(">>> Entering MigrateSettings(oldSettings={OldSettings})", oldSettings);
            var settings = new Hashtable();
            var mode = oldSettings["Mode"];
			_logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): mode → {Mode}", oldSettings, mode);
            var newMode = 0;
			_logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): newMode → {NewMode}", oldSettings, newMode);
			_logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): settings → {Settings}", oldSettings, settings);
            if (mode != null)
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
			_logger.LogDebug("MigrateSettings(oldSettings={OldSettings}): rootFolder → {RootFolder}", oldSettings, rootFolder);
            if (rootFolder != null)
            {
                settings.Add("RM_HomeFolder", rootFolder);
            }

			_logger.LogDebug("<<< Exiting MigrateSettings(oldSettings={OldSettings})", oldSettings);
            return settings;
        }
    }
}
