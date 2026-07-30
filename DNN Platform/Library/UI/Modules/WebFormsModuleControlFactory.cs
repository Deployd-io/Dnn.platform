// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;

    public class WebFormsModuleControlFactory : BaseModuleControlFactory
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Opml));
        /// <inheritdoc />
        public override int Priority => 100;

        /// <inheritdoc />
        public override bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc)
        {
			Logger.LogDebug(">>> Entering SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
			Logger.LogDebug("<<< Exiting SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
            return controlSrc.EndsWith(".ascx", System.StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
			Logger.LogDebug(">>> Entering CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
			Logger.LogDebug("<<< Exiting CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
            return ControlUtilities.LoadControl<Control>(containerControl, controlSrc);
        }

        /// <inheritdoc />
        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
			Logger.LogDebug(">>> Entering CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
			Logger.LogDebug("<<< Exiting CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
            return this.CreateControl(containerControl, string.Empty, moduleConfiguration.ModuleControl.ControlSrc);
        }

        /// <inheritdoc />
        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
			Logger.LogDebug(">>> Entering CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
			Logger.LogDebug("<<< Exiting CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
            return this.CreateControl(containerControl, string.Empty, controlSrc);
        }
    }
}
