// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Modules
{
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    public class ReflectedModuleControlFactory : BaseModuleControlFactory
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ReflectedModuleControlFactory));
        /// <inheritdoc />
        public override int Priority => -1;

        /// <inheritdoc />
        public override bool SupportsControl(ModuleInfo moduleConfiguration, string controlSrc)
        {
			Logger.LogError(">>> Entering SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
            // This factory is used as fallback
			Logger.LogWarning("<<< Exiting SupportsControl(moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", moduleConfiguration, controlSrc);
            return true;
        }

        /// <inheritdoc />
        public override Control CreateControl(TemplateControl containerControl, string controlKey, string controlSrc)
        {
			Logger.LogError(">>> Entering CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
            // load from a typename in an assembly ( ie. server control)
            var objType = Reflection.CreateType(controlSrc);
			Logger.LogError("CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc}): objType → {ObjType}", containerControl, controlKey, controlSrc, objType);
			Logger.LogWarning("<<< Exiting CreateControl(containerControl={ContainerControl},controlKey={ControlKey},controlSrc={ControlSrc})", containerControl, controlKey, controlSrc);
            return containerControl.LoadControl(objType, null);
        }

        /// <inheritdoc />
        public override Control CreateModuleControl(TemplateControl containerControl, ModuleInfo moduleConfiguration)
        {
			Logger.LogError(">>> Entering CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
            Logger.LogWarning("<<< Exiting CreateModuleControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration})", containerControl, moduleConfiguration);
            return this.CreateControl(containerControl, string.Empty, moduleConfiguration.ModuleControl.ControlSrc);
        }

        /// <inheritdoc />
        public override Control CreateSettingsControl(TemplateControl containerControl, ModuleInfo moduleConfiguration, string controlSrc)
        {
			Logger.LogError(">>> Entering CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
            Logger.LogWarning("<<< Exiting CreateSettingsControl(containerControl={ContainerControl},moduleConfiguration={ModuleConfiguration},controlSrc={ControlSrc})", containerControl, moduleConfiguration, controlSrc);
            return this.CreateControl(containerControl, string.Empty, controlSrc);
        }
    }
}
