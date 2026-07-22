// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace Dnn.ContactList.Spa.Components
{
    public class SettingsService : ServiceLocator<ISettingsService, SettingsService>, ISettingsService
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        private readonly IModuleController _moduleController;
        private const string IsFormEnabledKey = "IsFormEnabledKey";

        public SettingsService()
        {
			Logger.LogDebug(">>> Entering SettingsService()");
            _moduleController = ModuleController.Instance;
			Logger.LogDebug("<<< Exiting SettingsService()");
			Logger.LogDebug("SettingsService(): _moduleController → {_moduleController}", _moduleController);
        }
        protected override Func<ISettingsService> GetFactory()
        {
			Logger.LogDebug(">>> Entering GetFactory()");
			Logger.LogDebug("<<< Exiting GetFactory()");
            return () => new SettingsService();
        }

        public bool IsFormEnabled(int moduleId, int tabId)
        {
			Logger.LogDebug(">>> Entering IsFormEnabled(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            var module = _moduleController.GetModule(moduleId, tabId, true);
            var moduleSettings = module.ModuleSettings;
			Logger.LogDebug("IsFormEnabled(moduleId={ModuleId},tabId={TabId}): module → {Module}", moduleId, tabId, module);

			Logger.LogDebug("<<< Exiting IsFormEnabled(moduleId={ModuleId},tabId={TabId})", moduleId, tabId);
            return moduleSettings[IsFormEnabledKey] != null && Boolean.Parse((string) moduleSettings[IsFormEnabledKey]);
        }

        public void SaveFormEnabled(bool isEnabled, int moduleId)
        {
			Logger.LogDebug(">>> Entering SaveFormEnabled(isEnabled={IsEnabled},moduleId={ModuleId})", isEnabled, moduleId);
            _moduleController.UpdateModuleSetting(moduleId, IsFormEnabledKey, isEnabled.ToString());
			Logger.LogDebug("<<< Exiting SaveFormEnabled(isEnabled={IsEnabled},moduleId={ModuleId})", isEnabled, moduleId);
        }
    }
}
