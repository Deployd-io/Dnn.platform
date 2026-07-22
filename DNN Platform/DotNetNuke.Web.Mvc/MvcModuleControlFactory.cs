// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    public class MvcModuleControlFactory : BaseModuleControlFactory
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override int Priority => 100;

        /// <inheritdoc />
        public override bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc)
        {
			Logger.LogDebug(">>> Entering SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
			Logger.LogDebug("<<< Exiting SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
            return controlSrc.EndsWith(".mvc", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
			Logger.LogDebug(">>> Entering CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
			Logger.LogDebug("<<< Exiting CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
            return new MvcHostControl(controlKey);
        }

        /// <inheritdoc />
        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
			Logger.LogDebug(">>> Entering CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
			Logger.LogDebug("<<< Exiting CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
			Logger.LogDebug("<<< Exiting CreateModuleControl(moduleConfiguration={ModuleConfiguration})", moduleConfiguration);
            return new MvcHostControl();
        }

        /// <inheritdoc />
        public override ModuleControlBase CreateModuleControl(ModuleInfo moduleConfiguration)
        {
            ModuleControlBase moduleControl = base.CreateModuleControl(moduleConfiguration);

            var segments = moduleConfiguration.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');

            moduleControl.LocalResourceFile = $"~/DesktopModules/MVC/{moduleConfiguration.DesktopModule.FolderName}/{Localization.LocalResourceDirectory}/{segments[0]}.resx";

            return moduleControl;
        }

        /// <inheritdoc />
        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
			Logger.LogDebug(">>> Entering CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
			Logger.LogDebug("<<< Exiting CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
            return new MvcSettingsControl();
        }
    }
}
