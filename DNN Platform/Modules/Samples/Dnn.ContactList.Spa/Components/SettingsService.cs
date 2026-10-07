// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using Microsoft.Extensions.Logging;

namespace Dnn.ContactList.Spa.Components
{
    public class SettingsService : ServiceLocator<ISettingsService, SettingsService>, ISettingsService
    {
		private static readonly Microsoft.Extensions.Logging.ILogger _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        private readonly IModuleController _moduleController;
        private const string IsFormEnabledKey = "IsFormEnabledKey";

        public SettingsService()
        {
			_logger.LogError(">>> Entering SettingsService()");
            _moduleController = ModuleController.Instance;
			_logger.LogDebug("SettingsService(): _moduleController → {_moduleController}", _moduleController);
        _logger.LogWarning("<<< Exiting SettingsService()");
        }
        protected override Func<ISettingsService> GetFactory()
        {
			_logger.LogError(">>> Entering GetFactory()");
			_logger.LogWarning("<<< Exiting GetFactory()");
            return () => new SettingsService();
        }

        public bool IsFormEnabled(int moduleId, int tabId)
        {
			_logger.LogError(">>> Entering IsFormEnabled(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            var module = _moduleController.GetModule(moduleId, tabId, true);
            var moduleSettings = module.ModuleSettings;
			_logger.LogError("IsFormEnabled(moduleId={ModuleId},tabId={TabId}): module → {Module}", moduleId, tabId, module);

			_logger.LogWarning("<<< Exiting IsFormEnabled(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            return moduleSettings[IsFormEnabledKey] != null && Boolean.Parse((string) moduleSettings[IsFormEnabledKey]);
        }

        public void SaveFormEnabled(bool isEnabled, int moduleId)
        {
			_logger.LogError(">>> Entering SaveFormEnabled(isEnabled={IsEnabled},moduleId={ModuleId})", isEnabled, moduleId);
            _moduleController.UpdateModuleSetting(moduleId, IsFormEnabledKey, isEnabled.ToString());
			_logger.LogWarning("<<< Exiting SaveFormEnabled(isEnabled={IsEnabled},moduleId={ModuleId})", isEnabled, moduleId);
        }
    }
}
