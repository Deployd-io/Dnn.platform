// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    using System.IO;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Localization;

    public abstract class BaseModuleControlFactory : IModuleControlFactory
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public abstract int Priority { get; }

        /// <inheritdoc />
        public abstract bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc);

        /// <inheritdoc />
        public abstract Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc);

        /// <inheritdoc />
        public abstract Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration);

        /// <inheritdoc />
        public virtual ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration)
        {
			Logger.LogDebug(">>> Entering CreateModuleControl(moduleConfiguration={ModuleConfiguration})", moduleConfiguration);
            var moduleControl = new ModuleControlBase();
            moduleControl.ModuleContext.Configuration = moduleConfiguration;
			Logger.LogDebug("CreateModuleControl(moduleConfiguration={ModuleConfiguration}): moduleControl → {ModuleControl}", moduleConfiguration, moduleControl);

            moduleControl.LocalResourceFile = moduleConfiguration.ModuleControl.ControlSrc.Replace(Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc), string.Empty) +
                                       Localization.LocalResourceDirectory + "/" +
                                       Path.GetFileName(moduleConfiguration.ModuleControl.ControlSrc);

			Logger.LogDebug("<<< Exiting CreateModuleControl(moduleConfiguration={ModuleConfiguration})", moduleConfiguration);
            return moduleControl;
        }

        /// <inheritdoc />
        public abstract Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc);
    }
}
