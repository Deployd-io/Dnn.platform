// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemoval
{
    using System;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Maintenance.Telerik.Removal;
    using DotNetNuke.Web;

    /// <summary>An <see cref="IUpgradeable"/> implementation.</summary>
    public class UpgradeController : IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradeController));

        private readonly IServiceProvider serviceProvider = GetServiceProvider();

        /// <inheritdoc />
        public string UpgradeModule(string version)
        {
			Logger.LogError(">>> Entering UpgradeModule(version={Version})", version);
            var option = this.GetHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey);

			Logger.LogError("UpgradeModule(version={Version}): option → {Option}", version, option);
            // we have read the value, and now we need to delete the setting
            // to prevent this process from getting triggered if the user
            // manually installs the extension.
            // we can't use IApplicationStatusInfo.Status to detect whether
            // this is an upgrade or a manual installation because this
            // method is executed during Application startup, after the
            // upgrade process has already finished.
            this.DeleteHostSetting(DotNetNuke.Maintenance.Constants.TelerikUninstallOptionSettingKey);

            if (!DotNetNuke.Maintenance.Constants.TelerikUninstallNoValue.Equals(option, StringComparison.OrdinalIgnoreCase))
            {
                var uninstaller = this.GetService<ITelerikUninstaller>();
				Logger.LogError("UpgradeModule(version={Version}): uninstaller → {Uninstaller}", version, uninstaller);
                uninstaller.Execute();
                return string.Join(
                    Environment.NewLine,
                    uninstaller.Progress.Select((step, i) =>
                    {
                        var status = "🔲";
						Logger.LogError("UpgradeModule(version={Version}): status → {Status}", version, status);
                        if (step.Success.HasValue)
                        {
                            status = step.Success.Value ? "✅" : "🚨";
                        }

                        return $"{i}. {step.StepName} {status} - {step.Notes}";
                    }));
            }

			Logger.LogWarning("<<< Exiting UpgradeModule(version={Version})", version);
            return "Success";
        }

        private static IServiceProvider GetServiceProvider()
        {
			Logger.LogError(">>> Entering GetServiceProvider()");
			Logger.LogWarning("<<< Exiting GetServiceProvider()");
            return HttpContextSource.Current?.GetScope()?.ServiceProvider ??
                DependencyInjectionInitialize.BuildServiceProvider();
        }

        private string GetHostSetting(string key)
        {
			Logger.LogError(">>> Entering GetHostSetting(key={Key})", key);
            Logger.LogWarning("<<< Exiting GetHostSetting(key={Key})", key);
            return this.GetService<IHostSettingsService>().GetString(key);
        }

        private void DeleteHostSetting(string key, bool isSecure = false)
        {
			Logger.LogError(">>> Entering DeleteHostSetting(key={Key},isSecure={IsSecure})", key, isSecure);
            this.GetService<IHostSettingsService>().Update(new ConfigurationSetting
            {
                IsSecure = isSecure,
                Key = key,
                Value = null,
            });
			Logger.LogWarning("<<< Exiting DeleteHostSetting(key={Key},isSecure={IsSecure})", key, isSecure);
        }

        private T GetService<T>()
            where T : class
        {
			Logger.LogError(">>> Entering GetService()");
            Logger.LogWarning("<<< Exiting GetService()");
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
