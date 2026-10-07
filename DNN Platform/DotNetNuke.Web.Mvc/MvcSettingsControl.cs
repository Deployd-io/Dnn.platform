// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;

    public class MvcSettingsControl : MvcHostControl, ISettingsControl
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MvcSettingsControl));
        public MvcSettingsControl()
            : base("Settings")
        {
			Logger.LogError(">>> Entering MvcSettingsControl()");
            this.ExecuteModuleImmediately = false;
        }

        /// <inheritdoc />
        public void LoadSettings()
        {
			Logger.LogError(">>> Entering LoadSettings()");
            this.ExecuteModule();
			Logger.LogWarning("<<< Exiting LoadSettings()");
        }

        /// <inheritdoc />
        public void UpdateSettings()
        {
			Logger.LogError(">>> Entering UpdateSettings()");
            this.ExecuteModule();

            ModuleController.Instance.UpdateModule(this.ModuleContext.Configuration);
			Logger.LogWarning("<<< Exiting UpdateSettings()");
        }
    }
}
